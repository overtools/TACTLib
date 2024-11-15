using System;
using TACTLib.Client;

namespace TACTLib.Protocol
{
    public interface ICDNClient
    {
        void SetClientHandler(ClientHandler handler);
        
        public byte[]? FetchData(string key, Range? range=null, string? suffix=null)
        {
            return Fetch("data", key.ToLowerInvariant(), range, suffix);
        }
        
        public byte[]? FetchLooseData(CKey key)
        {
            return FetchData(key.ToHexString());
        }
        
        public byte[]? FetchIndexFile(string archiveKey)
        {
            return FetchData(archiveKey, null, ".index");
        }
        
        public byte[]? FetchIndexEntry(string archiveKey, IndexEntry indexEntry)
        {
            return FetchData(archiveKey, new Range((int)indexEntry.Offset, (int)indexEntry.Offset + (int)indexEntry.Size - 1));
        }

        public byte[]? FetchConfig(string key)
        {
            return Fetch("config", key.ToLowerInvariant());
        }
        
        byte[]? Fetch(string type, string key, Range? range=null, string? suffix=null);
    }
}