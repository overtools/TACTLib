using System.Collections.Generic;
using TACTLib.Client;
using TACTLib.Container;

namespace TACTLib.Config {
    public class BuildConfig : Config {
        public FileRecord Root;
        public FileRecord Install;
        public FileRecord Patch;
        public FileRecord Download;
        public FileRecord Encoding;
        public FileRecord VFSRoot;
        
        public BuildConfig(ClientHandler client, string key) : base(client, key) {
            GetFileRecord("root", out Root);
            GetFileRecord("install", out Install);
            GetFileRecord("patch", out Patch);
            GetFileRecord("download", out Download);
            GetFileRecord("encoding", out Encoding);
            GetFileRecord("vfs-root", out VFSRoot);
        }

        private void GetFileRecord(string key, out FileRecord @out) {
            if (Values.ContainsKey(key)) {
                @out = GetFileRecord(Values[key]);
                
                //string sizeString = $"{key}-size";
                //if (Values.ContainsKey(sizeString)) {
                //    @out.Size = int.Parse(Values[sizeString][0]);  // todo: hmm
                //}
            } else {
                @out = null;
            }
        }

        private FileRecord GetFileRecord(IReadOnlyList<string> vals) {
            FileRecord record = new FileRecord();

            if (vals.Count > 0) {
                record.ContentKey = CKey.FromString(vals[0]);
            }

            if (vals.Count > 1) {
                record.EncodingKey = EKey.FromString(vals[1]);
            }
            return record;
        }

        public class FileRecord {
            public CKey ContentKey;
            public EKey EncodingKey;
            
            //public int DecodedSize;
            //public int EncodedSize;
        }
    }
}