using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.HighPerformance.Buffers;
using TACTLib.Client;
using TACTLib.Container;
using TACTLib.Exceptions;
using TACTLib.Helpers;

namespace TACTLib.Core {
    /// <summary>BLTE block</summary>
    internal class DataBlock {
        public int CompSize;
        public int DecompSize;
        public CKey Hash;
    }

    /// <summary>BLTE encoded stream</summary>
    public class BLTEStream : Stream {
        public static Salsa20 s_salsa20 = new Salsa20();
        
        public const int Magic = 0x45544C42;
        private const byte EncryptionSalsa20 = 0x53;
        private const byte EncryptionArc4 = 0x41;

        private Stream m_inputStream;
        private BinaryReader m_inputReader;
        private DataBlock[] m_blocks;
        private int m_blockCount;

        private MemoryOwner<byte> m_decodedMemory;
        private MemoryStream m_decodedStream;
        private long m_length;

        public HashSet<string> Keys { get; set; } = new HashSet<string>();

        private readonly ClientHandler _client;
        
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => m_length;
        
        public override long Position {
            get => m_decodedStream.Position;
            set {
                while (value > m_decodedStream.Length)
                    if (!ProcessNextBlock())
                        break;
                m_decodedStream.Position = value;
            }
        }

        public BLTEStream(ClientHandler client, Stream src) {
            _client = client;
            m_inputStream = src;
            m_inputReader = new BinaryReader(src);

            Init();
        }
        
        public byte[] Dump() {
            byte[] data = new byte[m_inputStream.Length];
            long tmp = m_inputStream.Position;
            m_inputStream.Position = 0;
            m_inputStream.Read(data, 0, data.Length);
            m_inputStream.Position = tmp;
            return data;
        }

        private void Init() {
            int size = (int) m_inputReader.BaseStream.Length;

            if (size < 8)
                throw new BLTEDecoderException(Dump(), "not enough data: {0}", 8);

            int magic = m_inputReader.ReadInt32();

            if (magic != Magic) {
                throw new BLTEDecoderException(Dump(), "frame header mismatch (bad BLTE file)");
            }

            int headerSize = m_inputReader.ReadInt32BE();

            /*if (CASCConfig.ValidateData)
            {
                long oldPos = _reader.BaseStream.Position;

                _reader.BaseStream.Position = 0;

                byte[] newHash = _md5.ComputeHash(_reader.ReadBytes(headerSize > 0 ? headerSize : size));

                if (!md5.EqualsTo(newHash))
                    throw new BLTEDecoderException("data corrupted");

                _reader.BaseStream.Position = oldPos;
            }*/

            int numBlocks = 1;

            if (headerSize > 0) {
                if (size < 12)
                    throw new BLTEDecoderException(Dump(), "not enough data: {0}", 12);

                byte[] fcbytes = m_inputReader.ReadBytes(4);

                numBlocks = (fcbytes[1] << 16) | (fcbytes[2] << 8) | (fcbytes[3] << 0);

                if (fcbytes[0] != 0x0F || numBlocks == 0)
                    throw new BLTEDecoderException(Dump(), "bad table format 0x{0:x2}, numBlocks {1}", fcbytes[0], numBlocks);

                int frameHeaderSize = 24 * numBlocks + 12;

                if (headerSize != frameHeaderSize)
                    throw new BLTEDecoderException(Dump(), "header size mismatch");

                if (size < frameHeaderSize)
                    throw new BLTEDecoderException(Dump(), "not enough data: {0}", frameHeaderSize);
            }

            m_blocks = new DataBlock[numBlocks];

            for (int i = 0; i < numBlocks; i++) {
                var block = new DataBlock();

                if (headerSize != 0) {
                    block.CompSize = m_inputReader.ReadInt32BE();
                    block.DecompSize = m_inputReader.ReadInt32BE();
                    block.Hash = m_inputReader.Read<CKey>();
                } else {
                    block.CompSize = size - 8;
                    block.DecompSize = size - 8 - 1;
                    block.Hash = default;
                }

                m_blocks[i] = block;
            }

            var allBlocksSize = m_blocks.Sum(b => b.DecompSize);
            m_decodedMemory = MemoryOwner<byte>.Allocate(allBlocksSize);
            if (!MemoryMarshal.TryGetArray(m_decodedMemory.Memory, out ArraySegment<byte> segment)) {
                throw new Exception("Unable to get pooled buffer for BLTE decode");
            }
            m_decodedStream = new MemoryStream(segment.Array, segment.Offset, segment.Count);
            m_decodedStream.SetLength(0);

            ProcessNextBlock();

            m_length = headerSize == 0 ? m_decodedStream.Length : m_decodedStream.Capacity;

            //for (int i = 0; i < _dataBlocks.Length; i++)
            //{
            //    ProcessNextBlock();
            //}
        }

        private void HandleDataBlock(BinaryReader reader, int index) {
            var c = (char)reader.ReadByte();
            switch (c) {
                case 'E': // E (encrypted)
                {
                    reader.BaseStream.Position--;
                    using var dataBuffer = SpanOwner<byte>.Allocate((int)reader.BaseStream.Length);
                    reader.Read(dataBuffer.Span);
                    byte[] decrypted = Decrypt(dataBuffer.Span, index);
                    using var decryptedStream = new MemoryStream(decrypted);
                    using var decryptedReader = new BinaryReader(decryptedStream);
                    HandleDataBlock(decryptedReader, index);
                    break;
                }
                case 'F': // F (frame, recursive)
                {
                    throw new BLTEDecoderException(Dump(), "DecoderFrame not implemented");
                }
                case 'N': // N (not compressed)
                {
                    NoAllocCopyTo(reader.BaseStream, m_decodedStream);
                    break;
                }
                case 'Z': // Z (zlib compressed)
                {
                    Decompress(reader, m_decodedStream);
                    break;
                }
                default:
                    throw new BLTEDecoderException(Dump(), "unknown BLTE block type {0} (0x{1:X2})!", c, (byte)c);
            }
        }

        private byte[] Decrypt(ReadOnlySpan<byte> data, int index) {
            var dataSpan = data;
            
            byte keyNameSize = data[1];

            if (keyNameSize == 0 || keyNameSize != 8)
                throw new BLTEDecoderException(Dump(), "keyNameSize == 0 || keyNameSize != 8");
            
            var keyName = BitConverter.ToUInt64(dataSpan.Slice(2, keyNameSize));

            byte ivSize = data[keyNameSize + 2];

            if (ivSize != 4 || ivSize > 0x10)
                throw new BLTEDecoderException(Dump(), "IVSize != 4 || IVSize > 0x10");

            var ivPart = dataSpan.Slice(keyNameSize + 3, ivSize);

            if (data.Length < ivSize + keyNameSize + 4)
                throw new BLTEDecoderException(Dump(), "data.Length < IVSize + keyNameSize + 4");

            int dataOffset = keyNameSize + ivSize + 3;

            byte encType = data[dataOffset];

            if (encType != EncryptionSalsa20 && encType != EncryptionArc4) // 'S' or 'A'
                throw new BLTEDecoderException(Dump(), "encType != ENCRYPTION_SALSA20 && encType != ENCRYPTION_ARC4");

            dataOffset++;

            // expand to 8 bytes
            byte[] iv = new byte[8];
            ivPart.CopyTo(new Span<byte>(iv));

            // magic
            for (int shift = 0, i = 0; i < sizeof(int); shift += 8, i++) iv[i] ^= (byte) ((index >> shift) & 0xFF);

            // todo: could be null, but this shouldn't be called in that case
            byte[] key = _client.ConfigHandler.Keyring.GetKey(keyName);

            if (key == null)
                throw new BLTEKeyException(keyName);

            Keys.Add(keyName.ToString("X16"));

            if (encType == EncryptionSalsa20) {
                using var decryptor = s_salsa20.CreateDecryptor2(key, iv);
                return decryptor.TransformFinalBlock(data, dataOffset, data.Length - dataOffset);
            }

            // ARC4 ?
            throw new BLTEDecoderException(Dump(), "encType ENCRYPTION_ARC4 not implemented");
        }
        
        public static void NoAllocCopyTo(Stream dis, Stream destination, int bufferSize=81920) {
            using var bufferOwner = SpanOwner<byte>.Allocate(bufferSize);
            
            int count;
            while ((count = dis.Read(bufferOwner.Span)) != 0)
                destination.Write(bufferOwner.Span.Slice(0, count));
        }

        private static void Decompress(BinaryReader reader, Stream outStream) {
            reader.BaseStream.Position += 2; // skip first 2 bytes (zlib)
            using var dfltStream = new DeflateStream(reader.BaseStream, CompressionMode.Decompress);
            NoAllocCopyTo(dfltStream, outStream);
        }

        public override void Flush() {
            m_inputStream.Flush();
            m_decodedStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            while (m_decodedStream.Position + count > m_decodedStream.Length && m_blockCount < m_blocks.Length) {
                if (!ProcessNextBlock())
                    return 0;
            }

            return m_decodedStream.Read(buffer, offset, count);
        }

        private bool ProcessNextBlock() {
            if (m_blockCount >= m_blocks.Length)
                return false;

            long oldPos = m_decodedStream.Position;
            m_decodedStream.Position = m_decodedStream.Length;

            var block = m_blocks[m_blockCount];
            using var subStream = new SliceStream(m_inputStream, block.CompSize, true);
            using var blockReader = new BinaryReader(subStream);
            HandleDataBlock(blockReader, m_blockCount);
            m_blockCount++;

            m_decodedStream.Position = oldPos;

            return true;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            switch (origin) {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
            }

            return Position;
        }

        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException();

        protected override void Dispose(bool disposing) {
            try {
                if (!disposing) return;
                m_decodedMemory?.Dispose();
                m_inputStream?.Dispose();
                m_inputReader?.Dispose();
                m_decodedStream?.Dispose();
                m_blocks = null;
            } finally {
                m_decodedMemory = null;
                m_inputStream = null;
                m_inputReader = null;
                m_decodedStream = null;

                base.Dispose(disposing);
            }
            GC.SuppressFinalize(this);
        }
    }
}