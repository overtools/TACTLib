using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using TACTLib.Client;
using TACTLib.Exceptions;
using TACTLib.Helpers;

namespace TACTLib.Core
{
    public static class BLTEDecoder
    {
        private const byte EncryptionSalsa20 = (byte)'S';
        private const byte EncryptionArc4 = (byte)'A';

        [StructLayout(LayoutKind.Sequential)]
        private struct Header
        {
            public uint m_magic;
            public int m_frameHeaderSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FrameHeader
        {
            public byte m_format;
            public UInt24BE m_blockCount;
        }

        public static unsafe byte[] Decode(ClientHandler client, Span<byte> src) {
            if (src == null) throw new ArgumentNullException(nameof(src));

            var span = src;

            ref var header = ref SpanHelper.ReadStruct<Header>(ref span);
            if (BitConverter.IsLittleEndian) header.m_frameHeaderSize = BinaryPrimitives.ReverseEndianness(header.m_frameHeaderSize);

            if (header.m_magic != BLTEStream.Magic) throw new BLTEDecoderException(null, $"frame header mismatch (bad BLTE file) {header.m_magic:X}");
            
            scoped ReadOnlySpan<DataBlock> blocks;
            if (header.m_frameHeaderSize > 0)
            {
                var frameHeaderSpan = SpanHelper.Advance(ref span, header.m_frameHeaderSize - 8);
                ref var frameHeader = ref SpanHelper.ReadStruct<FrameHeader>(ref frameHeaderSpan);

                var numBlocks = frameHeader.m_blockCount.ToInt();
                if (frameHeader.m_format != 0x0F || numBlocks == 0)
                    throw new BLTEDecoderException(null, "bad table format 0x{0:x2}, numBlocks {1}", frameHeader.m_format, numBlocks);

                // ReSharper disable once InconsistentNaming
                var blocksRW = MemoryMarshal.Cast<byte, DataBlock>(frameHeaderSpan);
                if (BitConverter.IsLittleEndian)
                {
                    for (var i = 0; i < numBlocks; i++)
                    {
                        blocksRW[i].m_encodedSize = BinaryPrimitives.ReverseEndianness(blocksRW[i].m_encodedSize);
                        blocksRW[i].m_decodedSize = BinaryPrimitives.ReverseEndianness(blocksRW[i].m_decodedSize);
                    }
                }
                blocks = blocksRW;
            } else
            {
                blocks = stackalloc DataBlock[1]
                {
                    new DataBlock
                    {
                        m_encodedSize = src.Length - 8,
                        m_decodedSize = src.Length - 8 - 1,
                        m_md5 = default,
                    }
                };
            }

            var decodedSize = 0;
            foreach (var block in blocks)
            {
                decodedSize += block.m_decodedSize;
            }
            var output = new byte[decodedSize];
            var outputSlice = output.AsSpan();

            for (var i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                var encodedBlockData = SpanHelper.Advance(ref span, block.m_encodedSize);
                var blockOutput = SpanHelper.Advance(ref outputSlice, block.m_decodedSize);
                HandleDataBlock(client, encodedBlockData, blockOutput, i);
            }
            return output;
        }

        private static void HandleDataBlock(ClientHandler client, Span<byte> inputData, Span<byte> outputData, int blockIndex)
        {
            switch ((char)SpanHelper.ReadByte(ref inputData)) {
                case 'E': // E (encrypted)
                    var decrypted = Decrypt(client, inputData, blockIndex);
                    HandleDataBlock(client, decrypted, outputData, blockIndex);
                    break;
                case 'N': // N (not compressed)
                    if (outputData.Length != inputData.Length) throw new InvalidDataException();
                    inputData.CopyTo(outputData);
                    break;
                case 'Z': // Z (zlib compressed)
                    Decompress(inputData, outputData);
                    break;
                case 'F': // F (frame, recursive)
                    throw new BLTEDecoderException(null, "DecoderFrame not implemented");
                default:
                    throw new BLTEDecoderException(null, "unknown BLTE block type {0} (0x{1:X2})!", (char) inputData[0], inputData[0]);
            }
        }

        private static Span<byte> Decrypt(ClientHandler client, Span<byte> data, int blockIndex)
        {
            var keyNameSize = SpanHelper.ReadByte(ref data);
            if (keyNameSize == 0 || keyNameSize != 8)
                throw new BLTEDecoderException(null, "keyNameSize == 0 || keyNameSize != 8");

            var keyNameData = SpanHelper.Advance(ref data, keyNameSize);
            var keyName = BinaryPrimitives.ReadUInt64LittleEndian(keyNameData);

            var key = client.ConfigHandler.Keyring.GetKey(keyName);
            if (key == null)
                throw new BLTEKeyException(keyName);

            var ivPartSize = SpanHelper.ReadByte(ref data);
            if (ivPartSize != 4 && ivPartSize != 8)
                throw new BLTEDecoderException(null, $"ivPartSize should be 4 or 8. got {ivPartSize}");
            var ivPart = SpanHelper.Advance(ref data, ivPartSize);

            var encType = SpanHelper.ReadByte(ref data);
            if (encType != EncryptionSalsa20 && encType != EncryptionArc4) // 'S' or 'A'
                throw new BLTEDecoderException(null, "encType != ENCRYPTION_SALSA20 && encType != ENCRYPTION_ARC4");

            // expand to 8 bytes
            Span<byte> iv = stackalloc byte[8];
            ivPart.CopyTo(iv);

            // augment low dword of iv using blockIndex
            MemoryMarshal.AsRef<uint>(iv) ^= (uint)blockIndex;
            // alternative impl
            //for (int shift = 0, i = 0; i < sizeof(int); shift += 8, i++) iv2[i] ^= (byte) ((blockIndex >> shift) & 0xFF);

            if (encType == EncryptionSalsa20)
            {
                var decryptor = new Salsa20(key, iv);
                decryptor.Transform(data, data);
                return data;
            } else
            {
                throw new BLTEDecoderException(null, $"encType {encType} not implemented");
            }
        }

        private static unsafe void Decompress(ReadOnlySpan<byte> input, Span<byte> output)
        {
            fixed (byte* pBuffer = input) {
                using var unmanagedInputStream = new UnmanagedMemoryStream(pBuffer, input.Length);
                using var deflateStream = new ZLibStream(unmanagedInputStream, CompressionMode.Decompress);
                deflateStream.DefinitelyRead(output);
            }
        }
    }
}