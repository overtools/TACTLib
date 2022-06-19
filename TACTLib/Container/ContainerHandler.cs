using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using TACTLib.Client;
using TACTLib.Helpers;
using static TACTLib.Utils;

namespace TACTLib.Container {
    public class ContainerHandler {
        // ReSharper disable once InconsistentNaming
        /// <summary>Number of index files</summary>
        public const int CASC_INDEX_COUNT = 0x10;

        /// <summary>
        /// Container directory. Where the data, config, indices etc subdirectories are located.
        /// </summary>
        public readonly string ContainerDirectory;

        /// <summary>Data directory name</summary>
        public const string DataDirectory = "data";
        
        /// <summary>Config directory name</summary>
        public const string ConfigDirectory = "config";
        
        /// <summary>Indices directory name</summary>
        public const string CDNIndicesDirectory = "indices";
        
        /// <summary>Patch directory name</summary>
        public const string PatchDirectory = "patch";

        /// <summary>Local index map</summary>
        public readonly Dictionary<EKey, IndexEntry> IndexEntries;

        private readonly ClientHandler _client;

        private readonly Dictionary<int, SafeFileHandle> m_dataFiles;

        public ContainerHandler(ClientHandler client) {
            _client = client;
            if (client.BasePath == null) throw new Exception("no 'BasePath' specified");
            ContainerDirectory = Path.Combine(client.BasePath, GetContainerDirectory(client.Product));

            IndexEntries = new Dictionary<EKey, IndexEntry>(CASCKeyComparer.Instance);
            LoadIndexFiles();

            m_dataFiles = new Dictionary<int, SafeFileHandle>();
            
            for (var i = 0; i < 50; i++)
            {
                var path = GetDataFilePath(i);
                if (!File.Exists(path)) continue;

                m_dataFiles.Add(i, File.OpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.Read));
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
                header.EKeyBytes != EKey.CASC_EKEY_SIZE) {
                throw new InvalidDataException("invalid index header");
            }

            var eKey1Block = reader.Read<BlockSizeAndHash>();
            var entryCount = eKey1Block.BlockSize / sizeof(EKeyEntry);

            EKeyEntry[] entries = reader.ReadArray<EKeyEntry>(entryCount);
            Dictionary<int, long> dataFileSizes = new Dictionary<int, long>();
            for (var i = 0; i < entryCount; i++) {
                var entry = entries[i];
                if (IndexEntries.ContainsKey(entry.EKey)) {
                    continue;
                }

                var indexEntry = new IndexEntry(entry); 

                if (!dataFileSizes.TryGetValue(indexEntry.Index, out var dataFileSize)) {
                    var path = GetDataFilePath(indexEntry.Index);
                    if (!File.Exists(path)) {
                        continue;
                    }
                    dataFileSize = new FileInfo(path).Length;
                    dataFileSizes[indexEntry.Index] = dataFileSize;
                }

                if (indexEntry.Offset >= dataFileSize) {
                    continue;
                }
                    
                IndexEntries[entry.EKey] = indexEntry;
            }
        }

        /// <summary>
        /// Open an encoded file from Encoding Key
        /// </summary>
        /// <param name="key">The Encoding Key</param>
        /// <returns>Loaded file</returns>
        internal ArraySegment<byte>? OpenEKey(EKey key) {
            if (!IndexEntries.TryGetValue(key, out var indexEntry)) {
                Debugger.Log(0, "ContainerHandler", $"Missing local index {key.ToHexString()}\n");
                return null;
            }
            return OpenIndexEntry(indexEntry);
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
            
            ref var fileHeader = ref MemoryMarshal.AsRef<DataHeader>(buffer);
            
            if (fileHeader.m_size != indexEntry.EncodedSize) {
                throw new InvalidDataException($"fileHeader.m_size != indexEntry.EncodedSize. {fileHeader.m_size} != {indexEntry.EncodedSize}");
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

        private string GetDataFilePath(int index) {
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

            if (product == TACTProduct.Warcraft3 || product == TACTProduct.WorldOfWarcraft || product == TACTProduct.Diablo2 ||
                product == TACTProduct.Diablo3 || product == TACTProduct.BlackOps4 || product == TACTProduct.ModernWarfare)
                return "Data";

            if (product == TACTProduct.Overwatch)
                return Path.Combine("data", "casc");
            
            throw new NotImplementedException($"Product \"{product}\" is not supported.");
        }

        [StructLayout(LayoutKind.Sequential, Size = 30)]
        public struct DataHeader
        {
            public CKey m_md5;
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
            public byte ArchiveFileHeaderBytes;    // Number of bits for the file offset (rest is archive index)
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
        public unsafe struct EKeyEntry {
            /// <summary>Encoding Key</summary>
            public EKey EKey;                   // The first 9 bytes of the encoded key
            
            /// <summary>Index of data file and offset within (big endian)</summary>
            public fixed byte FileOffsetBE[5];
            
            /// <summary>Size of the encoded file</summary>
            public uint EncodedSize;
        }
        
        public struct IndexEntry {
            /// <summary>Data file index</summary>
            public int Index;
            
            /// <summary>Offset to data, in bytes</summary>
            public int Offset;
            
            public uint EncodedSize;

            public unsafe IndexEntry(EKeyEntry entry) {
                var indexHigh = entry.FileOffsetBE[0];
                var indexLow = Int32FromPtrBE(entry.FileOffsetBE + 1);
                Index = indexHigh << 2 | (byte) ((indexLow & 0xC0000000) >> 30);
                Offset = indexLow & 0x3FFFFFFF;

                EncodedSize = entry.EncodedSize;
            }
        }
    }
}