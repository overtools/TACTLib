using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using CommunityToolkit.HighPerformance.Helpers;
using TACTLib.Client;
using TACTLib.Helpers;

namespace TACTLib.Container {
	[StaticContainerHandler(TACTProduct.Overwatch)]
	public class StaticContainerHandler_Tank : StaticContainerHandler {
		public StaticContainerHandler_Tank(ClientHandler client) : base(client) { }

		protected override string GetFileName(ulong chunk, ulong archive, string? meta = null, int index = 0) => $"data.{chunk:D3}.{archive:D3}";
		protected override string ContainerDirectory => "data";
	}

	[StaticContainerHandler(TACTProduct.Diablo4)]
	public class StaticContainerHandler_Fenris : StaticContainerHandler {
		public StaticContainerHandler_Fenris(ClientHandler client) : base(client) { }

		protected override string GetFileName(ulong chunk, ulong archive, string? meta = null, int index = 0) => index switch {
			1 => $"{chunk:D3}/{archive}-{meta ?? "meta"}.dat",
			_ => $"{chunk:D3}/0x{archive:X04}-{meta ?? "meta"}.dat",
		};

		protected override string ContainerDirectory => "Data";
	}

	[StaticContainerHandler(TACTProduct.Diablo2)]
	public class StaticContainerHandler_Osi : StaticContainerHandler {
		public StaticContainerHandler_Osi(ClientHandler client) : base(client) { }

		protected override string GetFileName(ulong chunk, ulong archive, string? meta = null, int index = 0) => $"{chunk:D2}-{archive:x08}.data";
		protected override string ContainerDirectory => "data";
	}

	public abstract class StaticContainerHandler : IContainerHandler {
		protected readonly ClientHandler m_client;
		protected readonly string m_basePath;

		protected readonly byte m_keyLayoutIndexBits;
		protected readonly KeyLayout[] m_keyLayouts;

		protected struct KeyLayout {
            public byte m_chunkBits;
            public byte m_archiveBits;
            public byte m_offsetBits;
            public int m_offsetMultiplier;
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
                    m_offsetMultiplier = keyLayoutPair.Value.Count > 3 ? int.Parse(keyLayoutPair.Value[3]) : 1
                };
            }
        }

		protected void ExtractStorageLocation(FullEKey ekey, out ulong chunk, out ulong archive, out ulong offset, out int keyLayoutIndex) {
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
            offset = BitHelper.ExtractRange(ekeyHiUl, (byte) offsetBitOffset, offsetBitCount) * (ulong) keyLayout.m_offsetMultiplier;
        }

		protected abstract string ContainerDirectory { get; }
		protected abstract string GetFileName(ulong chunk, ulong archive, string? meta = null, int index = 0);
		protected virtual string GetFilePath(ulong chunk, ulong archive, string? meta = null, int index = 0) => Path.Join(m_basePath, ContainerDirectory, GetFileName(chunk, archive, meta, index));

		public ArraySegment<byte>? OpenEKey(FullEKey ekey, int eSize, string? meta = null) {
            ExtractStorageLocation(ekey, out var chunk, out var archive, out var offset, out var index);

            using var stream = File.OpenRead(GetFilePath(chunk, archive, meta, index));
            stream.Position = (long)offset;
            var data = GC.AllocateUninitializedArray<byte>(eSize);
            stream.DefinitelyRead(data);

            return data;
        }

        public bool CheckResidency(FullEKey ekey, string? meta = null) {
            ExtractStorageLocation(ekey, out var chunk, out var archive, out _, out var index);
            return File.Exists(GetFilePath(chunk, archive, meta, index));
        }
    }
}
