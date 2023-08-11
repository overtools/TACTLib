using System;
using System.Collections.Generic;
using System.IO;
using TACTLib.Container;

namespace TACTLib.Config {
    public class BuildConfig : Config {
        public FileRecord Root;
        public FileRecord? Install;
        public FileRecord? Patch;
        public FileRecord? Download;
        public FileRecord Encoding;
        public SizeRecord? m_encodingSize;
        public FileRecord? VFSRoot;
        
        public BuildConfig(Stream? stream) : base(stream) {
            GetFileRecord("root", out var root);
            GetFileRecord("install", out Install);
            GetFileRecord("patch", out Patch);
            GetFileRecord("download", out Download);
            GetFileRecord("encoding", out var encoding);
            GetSizeRecord("encoding-size", out m_encodingSize);
            GetFileRecord("vfs-root", out VFSRoot);

            if (root == null) throw new NullReferenceException(nameof(root));
            Root = root;
            
            if (encoding == null) throw new NullReferenceException(nameof(encoding));
            Encoding = encoding;
        }

        private void GetFileRecord(string key, out FileRecord? @out) {
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
        
        private void GetSizeRecord(string key, out SizeRecord? @out) {
            if (!Values.TryGetValue(key, out var list)) {
                @out = null;
                return;
            }
            @out = new SizeRecord {
                m_contentSize = int.Parse(list[0]),
                m_encodedSize = int.Parse(list[1])
            };
        }

        private FileRecord GetFileRecord(IReadOnlyList<string> vals) {
            FileRecord record = new FileRecord();

            if (vals.Count > 0) {
                record.ContentKey = CKey.FromString(vals[0]);
            }

            if (vals.Count > 1) {
                record.EncodingKey = CKey.FromString(vals[1]);
            }
            
            return record;
        }

        public class FileRecord {
            public CKey ContentKey;
            public CKey EncodingKey;
            
            //public int DecodedSize;
            //public int EncodedSize;
        }

        public class SizeRecord {
            public int m_contentSize;
            public int m_encodedSize;
        }
    }
}