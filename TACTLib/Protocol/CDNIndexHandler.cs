using System;
using System.Collections.Generic;
using System.IO;
using TACTLib.Client;
using TACTLib.Container;
using TACTLib.Helpers;

namespace TACTLib.Protocol
{
    public struct IndexEntry
    {
        public int Index;
        public int Offset;
        public int Size;
    }

    public class CDNIndexHandler
    {
        public Dictionary<CKey, IndexEntry> CDNIndexData = new Dictionary<CKey, IndexEntry>(CASCKeyComparer.Instance);
        public int Count => CDNIndexData.Count; // todo: why does this exist lol

        public readonly ClientHandler m_client;

        private CDNIndexHandler(ClientHandler client)
        {
            m_client = client;
        }

        public static CDNIndexHandler Initialize(ClientHandler clientHandler)
        {
            var handler = new CDNIndexHandler(clientHandler);

            for (int i = 0; i < clientHandler.ConfigHandler.CDNConfig.Archives.Count; i++)
            {
                string archive = clientHandler.ConfigHandler.CDNConfig.Archives[i];

                if (handler.m_client.ContainerHandler != null)
                    handler.DownloadIndexFile(archive, i);
                else
                    handler.OpenOrDownloadIndexFile(archive, i);
            }
            return handler;
        }

        private void ParseIndex(Stream stream, int i)
        {
            const int pageLength = 4 << 10;
            const int MD5_HASH_SIZE = CKey.CASC_CKEY_SIZE;
            
            using (var br = new BinaryReader(stream))
            {
                stream.Seek(-12, SeekOrigin.End);
                int count = br.ReadInt32();
                stream.Seek(0, SeekOrigin.Begin);

                var cbIndexFile = stream.Length - 0x14;
                var nPageCount = cbIndexFile / (pageLength + MD5_HASH_SIZE);
                var length = nPageCount * pageLength;
                
                var zeroKey = new CKey();
                var zeroHash = CASCKeyComparer.Instance.GetHashCode(zeroKey);

                long pageStart = 0;
                while (stream.Position < length)
                {
                    CKey key = br.Read<CKey>();
                    if (CASCKeyComparer.Instance.GetHashCode(key) == zeroHash)
                    {
                        stream.Position = pageStart + pageLength;
                        pageStart = stream.Position;
                        continue;
                    }
                    IndexEntry entry = new IndexEntry()
                    {
                        Index = i,
                        Size = br.ReadInt32BE(),
                        Offset = br.ReadInt32BE()
                    };
                    CDNIndexData[key] = entry;
                }
            }
        }

        private void DownloadIndexFile(string archive, int i)
        {
            try
            {
                var cdn = (CDNClient) m_client.NetHandle;
                using var stream = cdn.FetchCDN("data", archive, null, ".index");
                ParseIndex(stream, i);
            }
            catch (Exception exc)
            {
                throw new Exception($"DownloadFile failed: {archive} - {exc}");
            }
        }

        private void OpenOrDownloadIndexFile(string archive, int i)
        {
            try
            {
                var dir = m_client.ContainerHandler.ContainerDirectory;
                var path = Path.Combine(dir, ContainerHandler.CDNIndicesDirectory, archive + ".index");
                if (File.Exists(path))
                {
                    using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    ParseIndex(fs, i);
                }
                else
                {
                    // todo: save to disk. issue is that we don't know when to remove
                    DownloadIndexFile(archive, i);
                }
            }
            catch (Exception exc)
            {
                throw new Exception($"OpenFile failed: {archive} - {exc}");
            }
        }

        public Stream OpenDataFile(IndexEntry entry)
        {
            var archive = m_client.ConfigHandler.CDNConfig.Archives[entry.Index];

            var cdn = (CDNClient) m_client.NetHandle;
            var stream = cdn.FetchCDN("data", archive, (entry.Offset, entry.Offset + entry.Size - 1));
            return stream;
        }
    }
}