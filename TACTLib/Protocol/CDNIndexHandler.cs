using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using Microsoft.Win32.SafeHandles;
using TACTLib.Client;
using TACTLib.Container;
using TACTLib.Core.Key;
using TACTLib.Helpers;

namespace TACTLib.Protocol
{
    public struct IndexEntry
    {
        public ushort Index;
        public uint Offset;
        public uint Size;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CDNIndexHandler
    {
        private readonly ClientHandler Client;
        private readonly Dictionary<FullKey, IndexEntry> CDNIndexData = new Dictionary<FullKey, IndexEntry>(CASCKeyComparer.Instance);

        private FixedFooter ArchiveGroupFooter;
        private SafeFileHandle ArchiveGroupFileHandle = new SafeFileHandle();
        private FullKey[] ArchiveGroupPageLastEKeys = Array.Empty<FullKey>();
        
        private byte[][] LooseFilesPages = Array.Empty<byte[]>();
        private FullKey[] LooseFilesLastEKeys = Array.Empty<FullKey>();

        private const int ARCHIVE_ID_GROUP = -1;
        private const int ARCHIVE_ID_LOOSE = -2;

        public static CDNIndexHandler Initialize(ClientHandler clientHandler)
        {
            var handler = new CDNIndexHandler(clientHandler);
            return handler;
        }
        
        private CDNIndexHandler(ClientHandler client)
        {
            Client = client;
            
            // load loose files index so we dont have to hit the cdn just to get 404'd
            OpenOrDownloadIndexFile(client.ConfigHandler.CDNConfig.Values["file-index"][0], ARCHIVE_ID_LOOSE);

            var archiveGroupHash = client.ConfigHandler.CDNConfig.Values["archive-group"][0];
            if (LoadGroupIndexFile(archiveGroupHash)) {
                // new paging impl
                return;
            }
            
            // using shared impl...
            //if (m_client.ContainerHandler != null && OpenIndexFile(archiveGroupHash, ARCHIVE_ID_GROUP)) {
            //    // no need to load individual indices
            //    return;
            //}
            
            for (var index = 0; index < client.ConfigHandler.CDNConfig.Archives.Count; index++)
            {
                string archive = client.ConfigHandler.CDNConfig.Archives[index];
                OpenOrDownloadIndexFile(archive, index);
            }
        }

        private bool LoadGroupIndexFile(string hash) {
            if (Client.ContainerHandler == null) return false; // file only exists locally
            
            var path = GetArchiveIndexPath(hash);
            if (!File.Exists(path)) return false;

            ArchiveGroupFileHandle = File.OpenHandle(path);
            var length = (int)RandomAccess.GetLength(ArchiveGroupFileHandle);

            var maxFooterSize = FixedFooter.SIZE + FixedFooter.MAX_CHECKSUM_SIZE; // (we only care about the end checksum, not start)
            var maxFooterBuffer = new byte[maxFooterSize];
            RandomAccess.Read(ArchiveGroupFileHandle, maxFooterBuffer, length-maxFooterSize);
            
            using (var maxFooterReader = new BinaryReader(new MemoryStream(maxFooterBuffer))) {
                ArchiveGroupFooter = ReadFooter(maxFooterReader);
            }

            GetTableParameters(ArchiveGroupFooter, length, out var pageSize, out var pageCount);
            
            if (ArchiveGroupFooter.m_sizeBytes != 4) throw new Exception($"invalid `size` size for group: {ArchiveGroupFooter.m_sizeBytes} (expected 4)");
            if (ArchiveGroupFooter.m_offsetBytes != 6) throw new Exception($"invalid `offset` size for group: {ArchiveGroupFooter.m_offsetBytes} (expected 6)");
            
            var lastEKeyArrayOffset = pageCount * pageSize;
            ArchiveGroupPageLastEKeys = new FullEKey[pageCount];
            RandomAccess.Read(ArchiveGroupFileHandle, ArchiveGroupPageLastEKeys.AsSpan().AsBytes(), lastEKeyArrayOffset);

            return true;
        }
        
        private static FixedFooter ReadFooter(BinaryReader br) {
            var footerHashSize = FixedFooter.MAX_CHECKSUM_SIZE;

            while (true) {
                if (footerHashSize < FixedFooter.MIN_CHECKSUM_SIZE) throw new Exception("unable to determine footer hash size");

                br.BaseStream.Position = br.BaseStream.Length - footerHashSize - FixedFooter.SIZE;
                var footer = br.Read<FixedFooter>();
                if (footer.m_version != 1) goto NEXT;
                if (footer.m_unk0x11 != 0) goto NEXT;
                if (footer.m_unk0x12 != 0) goto NEXT;
                if (footer.m_checksumSize != footerHashSize) goto NEXT;
                // todo: more validation.. whar is hash
                
                //Console.Out.WriteLine($"he's {footerHashSize} ?");
                return footer;
                    
                NEXT:
                footerHashSize--;
            }
        }

        private static void GetTableParameters(FixedFooter footer, int length, out int pageSize, out int pageCount) {
            pageSize = footer.PageSizeBytes;
            var totalSizeForPage = pageSize + footer.m_keyBytes + footer.m_checksumSize; // every page will have lastEKey + hash
            pageCount = (length - footer.DynamicSize) / totalSizeForPage;
            
            if (footer.m_keyBytes != 16) throw new Exception($"invalid key size: {footer.m_keyBytes}");

            var calculatedSize = 0;
            calculatedSize += pageCount * pageSize;
            calculatedSize += pageCount * footer.m_keyBytes;
            calculatedSize += pageCount * footer.m_checksumSize;
            calculatedSize += footer.DynamicSize;

            if (calculatedSize != length) {
                throw new Exception("index file size mismatch");
            }
        }
        
        private void ParseIndex(Stream stream, int archiveIndex)
        {
            using var br = new BinaryReader(stream);
            var footer = ReadFooter(br);
            
            GetTableParameters(footer, (int)br.BaseStream.Length, out var pageSize, out var pageCount);
            if (archiveIndex == ARCHIVE_ID_GROUP) footer.m_offsetBytes -= 2; // archive index is part of offset
            if (archiveIndex == ARCHIVE_ID_LOOSE) {
                LooseFilesPages = new byte[pageCount][];
            }

            br.BaseStream.Position = 0;
            var page = new byte[pageSize];
            for (int pageIndex = 0; pageIndex < pageCount; pageIndex++) {
                br.DefinitelyRead(page);

                if (archiveIndex == ARCHIVE_ID_LOOSE) {
                    LooseFilesPages[pageIndex] = page.ToArray(); // dont store same array multiple times
                    continue;
                }

                var pageSpan = page.AsSpan();
                while (pageSpan.Length >= 16) {
                    var key = SpanHelper.ReadStruct<FullKey>(ref pageSpan);
                    if (key.CompareTo(default) == 0) break;

                    uint size;
                    if (footer.m_sizeBytes == 4) size = SpanHelper.ReadStruct<UInt32BE>(ref pageSpan).ToInt();
                    else throw new Exception($"unhandled `size` size: {footer.m_sizeBytes}");

                    ushort entryArchiveIndex = (ushort)archiveIndex;
                    if (archiveIndex == ARCHIVE_ID_GROUP) {
                        entryArchiveIndex = SpanHelper.ReadStruct<UInt16BE>(ref pageSpan).ToInt();
                    }
                    
                    uint offset;
                    if (footer.m_offsetBytes == 4) offset = SpanHelper.ReadStruct<UInt32BE>(ref pageSpan).ToInt();
                    else throw new Exception($"unhandled `offset` size: {footer.m_offsetBytes}");
                    
                    var entry = new IndexEntry
                    {
                        Index = entryArchiveIndex,
                        Size = size,
                        Offset = offset
                    };
                    CDNIndexData[key] = entry;
                }
            }

            if (archiveIndex == ARCHIVE_ID_LOOSE) {
                LooseFilesLastEKeys = br.ReadArray<FullEKey>(pageCount);
            } else {
                br.BaseStream.Position += pageCount * footer.m_keyBytes;
            }
            br.BaseStream.Position += pageCount * footer.m_checksumSize;
            br.BaseStream.Position += footer.DynamicSize;
            if (br.BaseStream.Position != br.BaseStream.Length) {
                throw new Exception($"didnt wrong length data read from index. pos: {br.BaseStream.Position}. len: {br.BaseStream.Length}");
            }
            
            //var lastEKeys = br.ReadArray<FullEKey>(pageCount);
            //var test = lastEKeys.Select(x => x.ToHexString()).ToArray();
            //Console.Out.WriteLine(test);
        }

        private void DownloadIndexFile(string archive, int i)
        {
            try
            {
                var cdn = Client.CDNClient!;
                var indexData = cdn.FetchCDN("data", archive, null, ".index");
                if (indexData == null) throw new Exception($"failed to fetch archive index data for {archive} (index {i})");
                using var indexDataStream = new MemoryStream(indexData);
                ParseIndex(indexDataStream, i);
            }
            catch (Exception exc)
            {
                throw new Exception($"DownloadFile failed: {archive} - {exc}");
            }
        }

        private string GetArchiveIndexPath(string archive) {
            var dir = ((ContainerHandler)Client.ContainerHandler!).ContainerDirectory;
            var path = Path.Combine(dir, ContainerHandler.CDNIndicesDirectory, archive + ".index");
            return path;
        }

        private bool OpenIndexFile(string archive, int archiveIndex) {
            if (Client.ContainerHandler == null) return false;
            var path = GetArchiveIndexPath(archive);
            if (!File.Exists(path)) return false;
            
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            ParseIndex(fs, archiveIndex);
            return true;
        }

        private void OpenOrDownloadIndexFile(string archive, int archiveIndex)
        {
            try {
                var localResult = OpenIndexFile(archive, archiveIndex);
                if (localResult) return;
                
                DownloadIndexFile(archive, archiveIndex);
            }
            catch (Exception exc)
            {
                throw new Exception($"OpenOrDownloadIndexFile failed: {archive} - {exc}");
            }
        }

        private static bool SearchLastKeys(FullEKey key, FullEKey[] lastKeysArray, out int pageIndex) {
            var binarySearchResult = Array.BinarySearch(lastKeysArray, key);
            if (binarySearchResult >= 0) {
                pageIndex = binarySearchResult;
            } else {
                var firstElementLarger = ~binarySearchResult;
                pageIndex = firstElementLarger;
            }
            if (pageIndex >= lastKeysArray.Length) return false;
            return true;
        }

        private bool TryGetIndexEntryFromGroup(FullEKey eKey, out IndexEntry indexEntry) {
            //if (ArchiveGroupFileHandle.IsInvalid) {
            //    indexEntry = default;
            //    return false;
            //}

            if (!SearchLastKeys(eKey, ArchiveGroupPageLastEKeys, out var pageIndex)) {
                goto NOT_FOUND;
            }
            
            Span<byte> pageData = stackalloc byte[ArchiveGroupFooter.PageSizeBytes];
            var pageOffset = ArchiveGroupFooter.PageSizeBytes * pageIndex;
            RandomAccess.Read(ArchiveGroupFileHandle, pageData, pageOffset);

            ReadOnlySpan<ArchiveGroupEntry> pageEntries = MemoryMarshal.Cast<byte, ArchiveGroupEntry>(pageData);
            var speculativeEntry = new ArchiveGroupEntry {
                m_eKey = eKey
            };
            var finalSearchResult = pageEntries.BinarySearch(speculativeEntry);
            if (finalSearchResult >= 0) {
                var foundEntry = pageEntries[finalSearchResult];
                indexEntry = new IndexEntry {
                    Index = foundEntry.m_archiveIndex.ToInt(),
                    Offset = foundEntry.m_offset.ToInt(),
                    Size = foundEntry.m_size.ToInt()
                };
                return true;
            }
            
            //Console.Out.WriteLine($"not found oopsie {pageIndex}: {finalSearchResult}");

            NOT_FOUND:
            indexEntry = default;
            return false;
        }

        public bool TryGetIndexEntry(FullEKey eKey, out IndexEntry indexEntry) {
            if (TryGetIndexEntryFromGroup(eKey, out indexEntry)) {
                return true;
            }
            
            return CDNIndexData.TryGetValue(eKey, out indexEntry);
        }

        public bool IsLooseFile(FullKey key) {
            if (!SearchLastKeys(key, LooseFilesLastEKeys, out var pageIndex)) {
                return false;
            }
            
            ReadOnlySpan<LooseFileEntry> pageEntries = MemoryMarshal.Cast<byte, LooseFileEntry>(LooseFilesPages[pageIndex]);
            var speculativeEntry = new LooseFileEntry {
                m_eKey = key
            };
            var finalSearchResult = pageEntries.BinarySearch(speculativeEntry);
            return finalSearchResult >= 0;
        }

        public byte[]? OpenIndexEntry(IndexEntry entry)
        {
            var archive = Client.ConfigHandler.CDNConfig.Archives[entry.Index];

            var cdn = Client.CDNClient!;
            var stream = cdn.FetchCDN("data", archive, ((int)entry.Offset, (int)entry.Offset + (int)entry.Size - 1));
            return stream;
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct LooseFileEntry : IComparable<LooseFileEntry> {
            public FullEKey m_eKey;
            public UInt32BE m_size;

            public int CompareTo(LooseFileEntry other) {
                return m_eKey.CompareTo(other.m_eKey);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ArchiveGroupEntry : IComparable<ArchiveGroupEntry> {
            public FullEKey m_eKey;
            public UInt32BE m_size;
            public UInt16BE m_archiveIndex;
            public UInt32BE m_offset;

            public int CompareTo(ArchiveGroupEntry other) {
                return m_eKey.CompareTo(other.m_eKey);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct FixedFooter {
            public byte m_version;
            public byte m_unk0x11;
            public byte m_unk0x12;
            public byte m_pageSizeKB;
            public byte m_offsetBytes;
            public byte m_sizeBytes;
            public byte m_keyBytes;
            public byte m_checksumSize;
            public uint m_numElements;

            public int PageSizeBytes => m_pageSizeKB * 1024;
            public int DynamicSize => SIZE + m_checksumSize * 2;

            public static unsafe int SIZE => sizeof(FixedFooter);
            public const byte MIN_CHECKSUM_SIZE = 8;
            public const byte MAX_CHECKSUM_SIZE = 16;
        }
    }
}