using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using CommunityToolkit.HighPerformance.Helpers;
using TACTLib.Client;
using TACTLib.Helpers;

namespace TACTLib.Container {
    public abstract class StaticContainerHandler : IContainerHandler {
        protected readonly ClientHandler m_client;
        protected readonly string m_basePath;

        protected readonly byte m_keyLayoutIndexBits;
        protected readonly KeyLayout[] m_keyLayouts;

        protected struct KeyLayout {
            public byte m_chunkBits;
            public byte m_archiveBits;
            public byte m_offsetBits;
            public uint m_offsetMultiplier;
        }

        protected StaticContainerHandler(ClientHandler client) {
            m_client = client;
            m_basePath = client.BasePath ?? throw new Exception("no 'BasePath' specified");

            m_keyLayoutIndexBits = byte.Parse(client.ConfigHandler.BuildConfig.Values["key-layout-index-bits"][0]);
            m_keyLayouts = new KeyLayout[Math.Max((int)Math.Pow(2, m_keyLayoutIndexBits), 1)];

            foreach (var keyLayoutPair in client.ConfigHandler.BuildConfig.Values
                    .Where(static x => x.Key.StartsWith("key-layout-") && x.Key != "key-layout-index-bits")) {

                var layoutIndex = byte.Parse(keyLayoutPair.Key.AsSpan("key-layout-".Length));

                m_keyLayouts[layoutIndex] = new KeyLayout {
                    m_chunkBits = byte.Parse(keyLayoutPair.Value[0]),
                    m_archiveBits = byte.Parse(keyLayoutPair.Value[1]),
                    m_offsetBits = byte.Parse(keyLayoutPair.Value[2]),
                    m_offsetMultiplier = Math.Max(1, keyLayoutPair.Value.Count > 3 ? uint.Parse(keyLayoutPair.Value[3]) : 1),
                };
            }
        }

        public void ExtractStorageLocation(FullEKey ekey, out ulong chunk, out ulong archive, out ulong offset, out int keyLayoutIndex) {
            //var chunk = 0ul;
            //var archive = 0ul;
            //var offset = 0ul;
            //var alignment = 0ul;

            var ekeySpan = (ReadOnlySpan<byte>)ekey;
            var ekeyHiUl = BinaryPrimitives.ReadUInt64BigEndian(ekeySpan.Slice(8));

            //Console.Out.WriteLine($"{ekeyHiUl}");
            var keyLayoutBitCount = m_keyLayoutIndexBits;
            var keyLayoutIndexBitOffset = 56-keyLayoutBitCount;
            keyLayoutIndex = (int) BitHelper.ExtractRange(ekeyHiUl, (byte)keyLayoutIndexBitOffset, keyLayoutBitCount);
            var keyLayout = m_keyLayouts[keyLayoutIndex];

            var chunkBitCount = keyLayout.m_chunkBits;
            var chunkBitOffset = keyLayoutIndexBitOffset-chunkBitCount;
            chunk = BitHelper.ExtractRange(ekeyHiUl, (byte)chunkBitOffset, chunkBitCount);

            var archiveBitCount = keyLayout.m_archiveBits;
            var archiveBitOffset = chunkBitOffset-archiveBitCount;
            archive = BitHelper.ExtractRange(ekeyHiUl, (byte)archiveBitOffset, archiveBitCount);

            var offsetBitCount = keyLayout.m_offsetBits;
            var offsetBitOffset = archiveBitOffset-offsetBitCount;
            offset = BitHelper.ExtractRange(ekeyHiUl, (byte) offsetBitOffset, offsetBitCount) * keyLayout.m_offsetMultiplier;
        }

        protected abstract string ContainerDirectory { get; }
        protected abstract string GetFileName(ulong chunk, ulong archive, int index = 0);
        protected virtual string GetFilePath(ulong chunk, ulong archive, int index = 0) => Path.Join(m_basePath, ContainerDirectory, GetFileName(chunk, archive, index));

        public string GetFilePath(FullEKey ekey) {
            ExtractStorageLocation(ekey, out var chunk, out var archive, out var offset, out var index);
            return GetFilePath(chunk, archive, index);
        }

        public ArraySegment<byte>? OpenEKey(FullEKey ekey, int eSize) {
            ExtractStorageLocation(ekey, out var chunk, out var archive, out var offset, out var index);

            using var stream = File.OpenRead(GetFilePath(chunk, archive, index));
            stream.Position = (long)offset;
            var data = GC.AllocateUninitializedArray<byte>(eSize);
            stream.DefinitelyRead(data);

            return data;
        }

        public bool CheckResidency(FullEKey ekey) {
            ExtractStorageLocation(ekey, out var chunk, out var archive, out _, out var index);
            return File.Exists(GetFilePath(chunk, archive, index));
        }
    }
}
