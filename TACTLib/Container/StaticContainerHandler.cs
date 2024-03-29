using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using CommunityToolkit.HighPerformance.Helpers;
using TACTLib.Client;
using TACTLib.Helpers;

namespace TACTLib.Container {
    public class StaticContainerHandler : IContainerHandler {
        private readonly ClientHandler m_client;
        private readonly string m_containerDirectory;

        private readonly byte m_keyLayoutIndexBits;
        private readonly KeyLayout[] m_keyLayouts;

        private struct KeyLayout {
            public byte m_chunkBits;
            public byte m_archiveBits;
            public byte m_offsetBits;
        }
        
        public StaticContainerHandler(ClientHandler client) {
            m_client = client;
            if (client.BasePath == null) throw new Exception("no 'BasePath' specified");
            m_containerDirectory = Path.Combine(client.BasePath, GetContainerDirectory(client.Product));

            m_keyLayoutIndexBits = byte.Parse(client.ConfigHandler.BuildConfig.Values["key-layout-index-bits"][0]);
            m_keyLayouts = new KeyLayout[Math.Max((int)Math.Pow(2, m_keyLayoutIndexBits), 1)];
            
            foreach (var keyLayoutPair in client.ConfigHandler.BuildConfig.Values
                    .Where(static x => x.Key.StartsWith("key-layout-") && x.Key != "key-layout-index-bits")) {

                var layoutIndex = byte.Parse(keyLayoutPair.Key.AsSpan("key-layout-".Length));

                m_keyLayouts[layoutIndex] = new KeyLayout {
                    m_chunkBits = byte.Parse(keyLayoutPair.Value[0]),
                    m_archiveBits = byte.Parse(keyLayoutPair.Value[1]),
                    m_offsetBits = byte.Parse(keyLayoutPair.Value[2])
                };
            }
        }

        private void ExtractStorageLocation(FullEKey ekey, out ulong chunk, out ulong archive, out ulong offset) {
            //var chunk = 0ul;
            //var archive = 0ul;
            //var offset = 0ul;
            //var alignment = 0ul;

            var ekeySpan = (ReadOnlySpan<byte>)ekey;
            var ekeyHiUl = BinaryPrimitives.ReadUInt64BigEndian(ekeySpan.Slice(8));

            //Console.Out.WriteLine($"{ekeyHiUl}");
            var keyLayoutBitCount = m_keyLayoutIndexBits;
            var keyLayoutIndexBitOffset = 56-keyLayoutBitCount;
            var keyLayoutIndex = BitHelper.ExtractRange(ekeyHiUl, (byte)keyLayoutIndexBitOffset, keyLayoutBitCount);
            var keyLayout = m_keyLayouts[keyLayoutIndex];
            
            var chunkBitCount = keyLayout.m_chunkBits;
            var chunkBitOffset = keyLayoutIndexBitOffset-chunkBitCount;
            chunk = BitHelper.ExtractRange(ekeyHiUl, (byte)chunkBitOffset, chunkBitCount);

            var archiveBitCount = keyLayout.m_archiveBits;
            var archiveBitOffset = chunkBitOffset-archiveBitCount;
            archive = BitHelper.ExtractRange(ekeyHiUl, (byte)archiveBitOffset, archiveBitCount);

            var offsetBitCount = keyLayout.m_offsetBits;
            var offsetBitOffset = archiveBitOffset-offsetBitCount;
            offset = BitHelper.ExtractRange(ekeyHiUl, (byte)offsetBitOffset, offsetBitCount);
        }

        private static string GetFileName(ulong chunk, ulong archive) {
            return $"data.{chunk:D3}.{archive:D3}";
        }

        private string GetFilePath(ulong chunk, ulong archive) {
            return Path.Combine(m_containerDirectory, GetFileName(chunk, archive));
        }

        public ArraySegment<byte>? OpenEKey(FullEKey ekey, int eSize) {
            ExtractStorageLocation(ekey, out var chunk, out var archive, out var offset);
            
            using var stream = File.OpenRead(GetFilePath(chunk, archive));
            stream.Position = (long)offset;
            var data = new byte[eSize];
            stream.DefinitelyRead(data);

            return data;
        }

        public bool CheckResidency(FullEKey ekey) {
            ExtractStorageLocation(ekey, out var chunk, out var archive, out _);
            return File.Exists(GetFilePath(chunk, archive));
        }

        private static string GetContainerDirectory(TACTProduct product) {
            if (product == TACTProduct.Overwatch)
                return Path.Combine("data");

            throw new NotImplementedException($"Product \"{product}\" is not supported.");
        }
    }
}