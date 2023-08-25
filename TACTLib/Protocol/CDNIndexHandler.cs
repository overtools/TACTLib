using System;
using System.Collections.Generic;
using System.IO;
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

    public class CDNIndexHandler
    {
        private readonly ClientHandler m_client;
        private readonly Dictionary<FullKey, IndexEntry> CDNIndexData = new Dictionary<FullKey, IndexEntry>(CASCKeyComparer.Instance);

        //private byte[][] ArchiveGroupPages = Array.Empty<byte[]>();
        //private FullKey[] ArchiveGroupPageLastEKeys = Array.Empty<FullKey>();
        // todo: paging

        private const int ARCHIVE_ID_GROUP = -1;
        
        private CDNIndexHandler(ClientHandler client)
        {
            m_client = client;

            var archiveGroupHash = client.ConfigHandler.CDNConfig.Values["archive-group"][0];
            if (m_client.ContainerHandler != null && OpenIndexFile(archiveGroupHash, ARCHIVE_ID_GROUP)) {
                // no need to load individual indices
                return;
            }
            
            for (var i = 0; i < client.ConfigHandler.CDNConfig.Archives.Count; i++)
            {
                string archive = client.ConfigHandler.CDNConfig.Archives[i];

                if (m_client.ContainerHandler != null)
                    OpenOrDownloadIndexFile(archive, i);
                else
                    DownloadIndexFile(archive, i);
            }
        }

        public static CDNIndexHandler Initialize(ClientHandler clientHandler)
        {
            var handler = new CDNIndexHandler(clientHandler);
            return handler;
        }
        
        private static FixedFooter ReadFooter(BinaryReader br) {
            var footerHashSize = 16;

            while (true) {
                if (footerHashSize < 8) throw new Exception("unable to determine footer hash size");

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
            pageSize = footer.m_blockSizeKB * 1024;
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

            br.BaseStream.Position = 0;
            var page = new byte[pageSize];
            for (int pageIndex = 0; pageIndex < pageCount; pageIndex++) {
                br.DefinitelyRead(page);

                var pageSpan = page.AsSpan();
                while (pageSpan.Length >= 16) {
                    var key = SpanHelper.ReadStruct<FullKey>(ref pageSpan);
                    if (key.CompareTo(default) == 0) break;

                    uint size;
                    if (footer.m_sizeBytes == 4) size = SpanHelper.ReadStruct<UInt32BE>(ref pageSpan).ToInt();
                    else throw new Exception($"unhanled `size` size: {footer.m_sizeBytes}");

                    ushort entryArchiveIndex = (ushort)archiveIndex;
                    if (archiveIndex == ARCHIVE_ID_GROUP) {
                        entryArchiveIndex = SpanHelper.ReadStruct<UInt16BE>(ref pageSpan).ToInt();
                    }
                    
                    uint offset;
                    if (footer.m_offsetBytes == 4) offset = SpanHelper.ReadStruct<UInt32BE>(ref pageSpan).ToInt();
                    else throw new Exception($"unhanled `offset` size: {footer.m_offsetBytes}");
                    
                    var entry = new IndexEntry
                    {
                        Index = entryArchiveIndex,
                        Size = size,
                        Offset = offset
                    };
                    CDNIndexData[key] = entry;
                }
            }

            br.BaseStream.Position += pageCount * footer.m_keyBytes;
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
                var cdn = (CDNClient) m_client.NetHandle!;
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
            var dir = ((ContainerHandler)m_client.ContainerHandler!).ContainerDirectory;
            var path = Path.Combine(dir, ContainerHandler.CDNIndicesDirectory, archive + ".index");
            return path;
        }

        private bool OpenIndexFile(string archive, int archiveIndex) {
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

        public bool TryGetIndexEntry(FullKey ckey, out IndexEntry indexEntry) {
            return CDNIndexData.TryGetValue(ckey, out indexEntry);
            
            //indexEntry = default;
            //return false;
        }

        public byte[]? OpenDataFile(IndexEntry entry)
        {
            var archive = m_client.ConfigHandler.CDNConfig.Archives[entry.Index];

            var cdn = (CDNClient) m_client.NetHandle!;
            var stream = cdn.FetchCDN("data", archive, ((int)entry.Offset, (int)entry.Offset + (int)entry.Size - 1));
            return stream;
        }

        private struct FixedFooter {
            public byte m_version;
            public byte m_unk0x11;
            public byte m_unk0x12;
            public byte m_blockSizeKB;
            public byte m_offsetBytes;
            public byte m_sizeBytes;
            public byte m_keyBytes;
            public byte m_checksumSize;
            public uint m_numElements;

            public int DynamicSize => SIZE + m_checksumSize * 2;

            public static unsafe int SIZE => sizeof(FixedFooter);
        }
    }
}