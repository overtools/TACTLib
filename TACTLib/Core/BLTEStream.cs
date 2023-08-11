using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using TACTLib.Client;
using TACTLib.Exceptions;
using TACTLib.Helpers;

namespace TACTLib.Core {
    /// <summary>BLTE block</summary>
    public struct DataBlock {
        public int m_encodedSize;
        public int m_decodedSize;
        public CKey m_md5;
    }

    /// <summary>BLTE encoded stream</summary>
    public class BLTEStream : Stream {
        public const uint Magic = 0x45544C42;
        private const byte EncryptionSalsa20 = (byte)'S';
        private const byte EncryptionArc4 = (byte)'A';
        
        private readonly ClientHandler m_client;

        private readonly byte[] m_inputData;
        private readonly MemoryStream m_outputStream;
        private int m_readOffset;
        
        private readonly DataBlock[] m_dataBlocks;
        private int m_blockIndex;

        //public readonly HashSet<string> Keys = new HashSet<string>();

        public readonly int EncodedSize;
        public readonly int DecodedSize;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => DecodedSize;

        public override long Position {
            get => m_outputStream.Position;
            set {
                while (value > m_outputStream!.Length)
                    if (!ProcessNextBlock())
                        break;

                m_outputStream.Position = value;
            }
        }

        public BLTEStream(ClientHandler client, byte[]? src, int offset) {
            if (src == null) throw new ArgumentNullException(nameof(src));

            m_inputData = src;
            m_client = client;

            // todo: remove reader and ms
            var ms = new MemoryStream(m_inputData);
            ms.Position = offset;
            var _reader = new BinaryReader(ms);
            
            var size = src.Length;
            if (size < 8)
                throw new BLTEDecoderException(Dump(), "not enough data: {0}", 8);

            var magic = _reader.ReadInt32();
            if (magic != Magic) {
                throw new BLTEDecoderException(Dump(), $"frame header mismatch (bad BLTE file) {magic:X}");
            }

            var headerSize = _reader.ReadInt32BE();

            /*if (CASCConfig.ValidateData)
            {
                long oldPos = _reader.BaseStream.Position;

                _reader.BaseStream.Position = 0;

                byte[] newHash = _md5.ComputeHash(_reader.ReadBytes(headerSize > 0 ? headerSize : size));

                if (!md5.EqualsTo(newHash))
                    throw new BLTEDecoderException("data corrupted");

                _reader.BaseStream.Position = oldPos;
            }*/

            int numBlocks;

            if (headerSize > 0) {
                if (size < 12)
                    throw new BLTEDecoderException(Dump(), "not enough data: {0}", 12);

                Span<byte> fcbytes = stackalloc byte[4]; // todo: too lazy to change....
                _reader.DefinitelyRead(fcbytes);

                numBlocks = (fcbytes[1] << 16) | (fcbytes[2] << 8) | (fcbytes[3] << 0);

                if (fcbytes[0] != 0x0F || numBlocks == 0)
                    throw new BLTEDecoderException(Dump(), "bad table format 0x{0:x2}, numBlocks {1}", fcbytes[0], numBlocks);

                var frameHeaderSize = 24 * numBlocks + 12;

                if (headerSize != frameHeaderSize)
                    throw new BLTEDecoderException(Dump(), "header size mismatch");

                if (size < frameHeaderSize)
                    throw new BLTEDecoderException(Dump(), "not enough data: {0}", frameHeaderSize);
            } else {
                numBlocks = 1;
            }

            // todo: if header: reverse endian inplace
            // if not header: create 1 fake block
            m_dataBlocks = new DataBlock[numBlocks];

            for (var i = 0; i < numBlocks; i++)
            {
                ref var block = ref m_dataBlocks[i];
                
                if (headerSize != 0)
                {
                    block.m_encodedSize = _reader.ReadInt32BE();
                    block.m_decodedSize = _reader.ReadInt32BE();
                    block.m_md5 = _reader.Read<CKey>();
                } else
                {
                    block.m_encodedSize = size - 8;
                    block.m_decodedSize = size - 8 - 1;
                    block.m_md5 = default;
                }
            }

            foreach(var dataBlock in m_dataBlocks)
            {
                EncodedSize += dataBlock.m_encodedSize;
                DecodedSize += dataBlock.m_decodedSize;
            }

            m_outputStream = new MemoryStream(DecodedSize);

            m_readOffset = (int)_reader.BaseStream.Position;
            ProcessNextBlock();

            //for (int i = 0; i < _dataBlocks.Length; i++)
            //{
            //    ProcessNextBlock();
            //}
        }
        
        public byte[] Dump()
        {
            // todo: repair
            throw new NotImplementedException();
        }

        private void HandleDataBlock(ArraySegment<byte> data, int index)
        {
            var bodyData = data.Slice(1);
            switch ((char)data[0]) {
                case 'E': // E (encrypted)
                    var decrypted = Decrypt(bodyData, index);
                    HandleDataBlock(decrypted, index);
                    break;
                case 'F': // F (frame, recursive)
                    throw new BLTEDecoderException(Dump(), "DecoderFrame not implemented");
                case 'N': // N (not compressed)
                    m_outputStream.Write(bodyData);
                    break;
                case 'Z': // Z (zlib compressed)
                    Decompress(bodyData, m_outputStream);
                    break;
                default:
                    throw new BLTEDecoderException(Dump(), "unknown BLTE block type {0} (0x{1:X2})!", (char) data[0], data[0]);
            }
        }

        private ArraySegment<byte> Decrypt(ArraySegment<byte> data, int index)
        {
            var dataOffset = 0;
            
            var keyNameSize = data[dataOffset++];
            if (keyNameSize == 0 || keyNameSize != 8)
                throw new BLTEDecoderException(Dump(), "keyNameSize == 0 || keyNameSize != 8");

            var keyNameData = data.Slice(dataOffset, keyNameSize);
            var keyName = BinaryPrimitives.ReadUInt64LittleEndian(keyNameData);
            dataOffset += keyNameSize;
            
            var key = m_client.ConfigHandler.Keyring.GetKey(keyName);
            if (key == null)
                throw new BLTEKeyException(keyName);
            //Keys.Add(keyName.ToString("X16"));

            var ivPartSize = data[dataOffset++];
            if (ivPartSize != 4 && ivPartSize != 8)
                throw new BLTEDecoderException(Dump(), $"ivPartSize should be 4 or 8. got {ivPartSize}");
            var ivPart = data.Slice(dataOffset, ivPartSize);
            dataOffset += ivPartSize;

            if (data.Count < ivPartSize + keyNameSize + 3)
                throw new BLTEDecoderException(Dump(), "data.Length < IVSize + keyNameSize + 3");

            var encType = data[dataOffset++];
            if (encType != EncryptionSalsa20 && encType != EncryptionArc4) // 'S' or 'A'
                throw new BLTEDecoderException(Dump(), "encType != ENCRYPTION_SALSA20 && encType != ENCRYPTION_ARC4");

            // expand to 8 bytes
            Span<byte> iv = stackalloc byte[8];
            ivPart.AsSpan().CopyTo(iv);
            
            // do some magic (knowledge passed down through generations)
            for (int shift = 0, i = 0; i < sizeof(int); shift += 8, i++) iv[i] ^= (byte) ((index >> shift) & 0xFF);

            if (encType == EncryptionSalsa20)
            {
                var bodyData = data.Slice(dataOffset);
                var bodySpan = bodyData.AsSpan();

                var decryptor = new Salsa20(key, iv);
                decryptor.Transform(bodySpan, bodySpan);
                
                return bodyData;
            } else
            {
                throw new BLTEDecoderException(Dump(), $"encType {encType} not implemented");
            }
        }
        
        private static void Decompress(ArraySegment<byte> data, Stream outputStream)
        {
            // skip first 3 bytes (zlib)
            
            using (var memoryStream = new MemoryStream(data.Array!, data.Offset, data.Count))
            using (var zlibStream = new ZLibStream(memoryStream, CompressionMode.Decompress)) {
                NoAllocCopyTo(zlibStream, outputStream);
            }
        }

        public static void NoAllocCopyTo(Stream dis, Stream destination, int bufferSize=81920)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try {
                int count;
                while ((count = dis.Read(buffer, 0, buffer.Length)) != 0)
                    destination.Write(buffer, 0, count);
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        
        public override void Flush()
        {
            m_outputStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (m_outputStream!.Position + count > m_outputStream.Length && m_blockIndex < m_dataBlocks.Length) {
                if (!ProcessNextBlock())
                    return 0;

                return Read(buffer, offset, count);
            }

            return m_outputStream.Read(buffer, offset, count);
        }

        public bool ProcessNextBlock()
        {
            if (m_blockIndex == m_dataBlocks.Length)
                return false;

            var oldPos = m_outputStream.Position;
            m_outputStream.Position = m_outputStream.Length;

            var block = m_dataBlocks[m_blockIndex];

            var blockData = new ArraySegment<byte>(m_inputData, m_readOffset, block.m_encodedSize);
            m_readOffset += block.m_encodedSize;

            //            if (!block.Hash.IsZeroed() && CASCConfig.ValidateData)
            //            {
            //                byte[] blockHash = _md5.ComputeHash(block.Data);
            //
            //                if (!block.Hash.EqualsTo(blockHash))
            //                    throw new BLTEDecoderException("MD5 mismatch");
            //            }

            HandleDataBlock(blockData, m_blockIndex);
            m_blockIndex++;

            m_outputStream.Position = oldPos;

            return true;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
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

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }

        protected override void Dispose(bool disposing)
        {
            m_outputStream.Dispose();
        }
    }
}
