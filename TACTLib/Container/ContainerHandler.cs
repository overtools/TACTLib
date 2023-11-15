using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using TACTLib.Client;
using TACTLib.Core;
using TACTLib.Core.Key;
using TACTLib.Helpers;

namespace TACTLib.Container {
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ContainerHandler : IContainerHandler {
        /// <summary>Number of index files</summary>
        public const int CASC_INDEX_COUNT = 0x10;

        /// <summary>
        /// Container directory. Where the data, config, indices etc subdirectories are located.
        /// </summary>
        public readonly string ContainerDirectory;
        
        public const string DataDirectory = "data";
        public const string ConfigDirectory = "config";
        public const string CDNIndicesDirectory = "indices";
        public const string PatchDirectory = "patch";

        private readonly ClientHandler _client;
        private readonly Dictionary<int, SafeFileHandle> m_dataFiles;
        private readonly EKeyEntry[][] IndexEntryBuckets = new EKeyEntry[CASC_INDEX_COUNT][];

        private bool m_seenCorruptHeader;
        //private static unsafe ReadOnlySpan<byte> ZEROED_HEADER => new byte[sizeof(DataHeader)];
        public static bool ALLOW_CORRUPT_HEADER = true;

        public ContainerHandler(ClientHandler client) {
            _client = client;
            if (client.BasePath == null) throw new Exception("no 'BasePath' specified");
            ContainerDirectory = Path.Combine(client.BasePath, GetContainerDirectory(client.Product));

            for (int i = 0; i < CASC_INDEX_COUNT; i++) {
                IndexEntryBuckets[i] = Array.Empty<EKeyEntry>();
            }
            LoadIndexFiles();

            m_dataFiles = new Dictionary<int, SafeFileHandle>();
            foreach (var (i, path) in GetDataFilePaths()) {
                //Logger.Debug("CASC", $"Opening data file {i} at {path}");
                m_dataFiles.Add(i, File.OpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.RandomAccess));
            }
        }

        private void LoadIndexFiles() {
            for (var i = 0; i < CASC_INDEX_COUNT; i++) {
                var files = Directory.EnumerateFiles(Path.Combine(ContainerDirectory, DataDirectory), $"{i:X2}*.idx" + _client.CreateArgs.ExtraFileEnding, new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive }).ToList();
                if (files.Count == 0) continue;

                string? selectedFile = null;
                var selectedVersion = 0;
                foreach (var file in files) {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var sub = fileName.AsSpan(2);
                    var version = int.Parse(sub, NumberStyles.HexNumber);

                    if (version > selectedVersion) {
                        selectedFile = file;
                        selectedVersion = version;
                    }
                }

                if (selectedFile == null) throw new InvalidDataException($"unable to find index {i:X2}, impossible");
                LoadIndexFile(selectedFile, i);
            }
        }

        /// <summary>
        /// Load an index file
        /// </summary>
        /// <param name="file">File path</param>
        /// <param name="bucketIndex">Index</param>
        /// <exception cref="InvalidDataException">Index file is invalid</exception>
        private unsafe void LoadIndexFile(string file, int bucketIndex)
        {
            using Stream stream = File.OpenRead(file);
            using BinaryReader reader = new BinaryReader(stream);

            var header = reader.Read<IndexHeaderV2>();
            if (header.IndexVersion != 0x07 ||
                header.BucketIndex != bucketIndex ||
                header.ExtraBytes != 0x00 ||
                header.SpanSizeBytes != 0x04 ||
                header.SpanOffsBytes != 0x05 ||
                header.EKeyBytes != EKey.CASC_TRUNCATED_KEY_SIZE) {
                throw new InvalidDataException("invalid index header");
            }

            if (header.FileOffsetBits != 30)
                throw new InvalidDataException($"invalid index header: header.FileOffsetBits ({header.FileOffsetBits}) != 30");

            var eKey1Block = reader.Read<BlockSizeAndHash>();
            var entryCount = eKey1Block.BlockSize / sizeof(EKeyEntry);

            EKeyEntry[] entries = reader.ReadArray<EKeyEntry>(entryCount);
            IndexEntryBuckets[header.BucketIndex] = entries;
        }

        private static byte EKeyToIndexNumber(TruncatedKey k) {
            // https://wowdev.wiki/CASC#.IDX_Journals
            var i = (byte)(k[0] ^ k[1] ^ k[2] ^ k[3] ^ k[4] ^ k[5] ^ k[6] ^ k[7] ^ k[8]);
            return (byte)((i & 0xf) ^ (i >> 4));
        }

        private bool TryFindIndexEntry(TruncatedKey key, out EKeyEntry entry) {
            var bucketNumber = EKeyToIndexNumber(key);
            var indexEntriesInBucket = IndexEntryBuckets[bucketNumber];

            var speculativeEntry = new EKeyEntry {
                EKey = key
            };
            var entryIndex = Array.BinarySearch(indexEntriesInBucket, speculativeEntry);
            if (entryIndex < 0 || entryIndex >= indexEntriesInBucket.Length) {
                entry = default;
                return false;
            }

            entry = indexEntriesInBucket[entryIndex];
            if (!CASCKeyComparer.Instance.Equals(entry.EKey, key)) {
                throw new Exception("key failed Equals check"); // todo: temp
            }
            return true; 
        }

        public IEnumerable<KeyValuePair<EKey, IndexEntry>> GetIndexEntries() {
            foreach (var bucket in IndexEntryBuckets) {
                foreach (var entry in bucket) {
                    yield return new KeyValuePair<TruncatedKey, IndexEntry>(entry.EKey, new IndexEntry(entry));
                }
            }
        }

        /// <summary>
        /// Open an encoded file from Encoding Key
        /// </summary>
        /// <param name="key">The Encoding Key</param>
        /// <returns>Loaded file</returns>
        private ArraySegment<byte>? OpenEKey(TruncatedKey key) {
            if (!TryFindIndexEntry(key, out var entry)) {
                return null;
            }
            var convertedEntry = new IndexEntry(entry);
            return OpenIndexEntry(convertedEntry);
        }

        public ArraySegment<byte>? OpenEKey(FullEKey ekey, int eSize) {
            return OpenEKey(ekey.AsTruncated());
        }

        public bool CheckResidency(FullEKey ekey) {
            return TryFindIndexEntry(ekey.AsTruncated(), out _);
        }

        private IEnumerable<(int Index, string Path)> GetDataFilePaths() {
            foreach (var path in Directory.EnumerateFiles(Path.Combine(ContainerDirectory, DataDirectory), $"data.*{_client.CreateArgs.ExtraFileEnding}")) {
                var number = path[^(3 + (_client.CreateArgs.ExtraFileEnding?.Length ?? 0))..];
                if(int.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out var index)) {
                    yield return (index, path);
                }
            }
        }

        /// <summary>
        /// Open an index entry and get data
        /// </summary>
        /// <param name="indexEntry">Source index entry</param>
        /// <returns>Encoded stream</returns>
        private unsafe ArraySegment<byte> OpenIndexEntry(IndexEntry indexEntry)
        {
            var dataHandle = m_dataFiles[indexEntry.Index];

            var size = indexEntry.EncodedSize;
            var buffer = new byte[size];
            var bytesRead = RandomAccess.Read(dataHandle, buffer, indexEntry.Offset);
            if (bytesRead != buffer.Length)
            {
                throw new EndOfStreamException($"bytesRead != buffer.Length. {bytesRead} != {buffer.Length}");
            }

            var initialMagic = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            if (initialMagic == BLTEStream.Magic) {
                return buffer;
            }

            ref var fileHeader = ref MemoryMarshal.AsRef<DataHeader>(buffer);
            if (fileHeader.m_size != indexEntry.EncodedSize) {
                // header struct: https://github.com/ladislav-zezula/CascLib/blob/22b558e710d730edaa7b1610349081fce7fb0f7a/src/CascStructs.h#L244
                
                // header check: https://github.com/ladislav-zezula/CascLib/blob/22b558e710d730edaa7b1610349081fce7fb0f7a/src/CascReadFile.cpp#L177
                // also validating size matches
                // BUT
                // apparently the data can just blte with no DataHeader?
                // lets collect a fourCC to try debug the crashes here
                
                if (!BinaryPrimitives.TryReadUInt32LittleEndian(buffer.AsSpan(sizeof(DataHeader)), out var fourCC)) {
                    fourCC = 0xDEADBEEFu;
                }
                
                // todo: i was checking for just zeroed here.. but some headers are invalid data
                // from aini:
                // Size Wrong Other Samples[0]:
                //   EKey: 09ea93678a82a9c94c
                //   CKey: 07994465b4e6cd87335f502ac2ca0650
                //   Header: aad8ae3d2cffa11a6d7ba9fe5be0b4d9965f89c8cbc2fe2af4d998afc6af
                //   FourCC: 45544C42
                //   Expected Size: 30581
                //   Offset: 0x1FDEA87E
                
                // && headerSpan.SequenceEqual(ZEROED_HEADER)
                var headerSpan = buffer.AsSpan(0, sizeof(DataHeader));
                if (ALLOW_CORRUPT_HEADER && fourCC == BLTEStream.Magic) {
                    // aight
                } else {
                    throw new InvalidDataException($"fileHeader.m_size != indexEntry.EncodedSize. {fileHeader.m_size} != {indexEntry.EncodedSize}. fourCC: {fourCC:X8}");
                }
            }

            var segment = new ArraySegment<byte>(buffer);
            var dataSegment = segment.Slice(sizeof(DataHeader));

            // todo: eh
            //Span<byte> md5Buffer = stackalloc byte[16];
            //MD5.HashData(buffer, md5Buffer);
            //if (!MemoryExtensions.SequenceEqual(fileHeader.m_md5.CreateSpan(), md5Buffer))
            //{
            //    throw new Exception();
            //}

            return dataSegment;
        }

        public unsafe bool OpenIndexEntryForDebug(IndexEntry indexEntry, out DataHeader header, out uint fourCC) {
            
            header = default;
            fourCC = 0xDEADBEEFu;

            var sizeToRead = sizeof(DataHeader);
            if (indexEntry.EncodedSize < sizeToRead) return false;

            if (indexEntry.EncodedSize >= sizeToRead + 4) {
                // give me fourCC
                sizeToRead += 4;
            }
            
            var dataHandle = m_dataFiles[indexEntry.Index];
            var buffer = new byte[sizeToRead];
            var bytesRead = RandomAccess.Read(dataHandle, buffer, indexEntry.Offset);
            if (bytesRead != buffer.Length)
            {
                throw new EndOfStreamException($"bytesRead != buffer.Length. {bytesRead} != {buffer.Length}");
            }

            header = MemoryMarshal.Read<DataHeader>(buffer);
            if (BinaryPrimitives.TryReadUInt32LittleEndian(buffer.AsSpan(sizeof(DataHeader)), out var fourCC2)) {
                fourCC = fourCC2;
            }
            return true;
        }

        public IEnumerable<int> GetDataFileIndices() => m_dataFiles.Keys;

        public string GetDataFilePath(int index) {
            return Path.Combine(ContainerDirectory, DataDirectory, $"data.{index:D3}") + _client.CreateArgs.ExtraFileEnding;
        }

        /// <summary>
        /// Get container directory from product type
        /// </summary>
        /// <param name="product">Target product</param>
        /// <returns>Container directory</returns>
        /// <exception cref="NotImplementedException">Product is unsupported</exception>
        public static string GetContainerDirectory(TACTProduct product) {
            if (product == TACTProduct.HeroesOfTheStorm)
                return "HeroesData";

            if (product == TACTProduct.StarCraft2)
                return "SC2Data";

            if (product == TACTProduct.Hearthstone)
                return "Hearthstone_Data";

            if (product == TACTProduct.Warcraft3 || product == TACTProduct.WorldOfWarcraft || 
                product == TACTProduct.Diablo2 || product == TACTProduct.Diablo3 || product == TACTProduct.Diablo4 || 
                product == TACTProduct.BlackOps4 || product == TACTProduct.ModernWarfare)
                return "Data";

            if (product == TACTProduct.Overwatch)
                return Path.Combine("data", "casc");

            throw new NotImplementedException($"Product \"{product}\" is not supported.");
        }

        [StructLayout(LayoutKind.Sequential, Size = 30)]
        public struct DataHeader
        {
            public MD5Key m_md5;
            public uint m_size;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct IndexHeaderV2 {
            public BlockSizeAndHash BlockHeader;
            public ushort IndexVersion;
            public byte BucketIndex;
            public byte ExtraBytes;
            public byte SpanSizeBytes;             // Size of field with file size
            public byte SpanOffsBytes;             // Size of field with file offset
            public byte EKeyBytes;                 // Size of the file key (bytes)
            public byte FileOffsetBits;            // Number of bits for the file offset (rest is archive index)
            public ulong ArchiveTotalSizeMaximum;  // The maximum size of a casc installation; 0x4000000000, or 256GiB.
            public fixed byte Padding[8];          // Always here
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct BlockSizeAndHash {
            /// <summary>
            /// Block size, in bytes
            /// </summary>
            public int BlockSize;

            /// <summary>
            /// hashlittle2 on the following BlockSize bytes of the file with an initial value of 0 for pb and pc.
            /// </summary>
            public int BlockHash;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct EKeyEntry : IComparable<EKeyEntry> {
            /// <summary>Encoding Key</summary>
            public EKey EKey;                   // The first 9 bytes of the encoded key

            /// <summary>Index of data file and offset within (big endian)</summary>
            public UInt40BE FileOffsetBE;

            /// <summary>Size of the encoded file</summary>
            public uint EncodedSize;

            public int CompareTo(EKeyEntry other) {
                return EKey.CompareTo(other.EKey);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IndexEntry {
            /// <summary>Data file index</summary>
            public ushort Index;

            /// <summary>Offset to data, in bytes</summary>
            public uint Offset;

            public uint EncodedSize;

            public IndexEntry(EKeyEntry entry) {
                var indexInt = entry.FileOffsetBE.ToInt();

                const int fileOffsetBitCount = 30; // technically can vary via header but hardcoded same as everything else

                Index = (ushort)(indexInt >> fileOffsetBitCount);
                Offset = (uint)(indexInt & ((1 << fileOffsetBitCount) - 1));

                EncodedSize = entry.EncodedSize;
            }
        }
    }
}
