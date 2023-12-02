using System;
using System.Collections.Generic;
using System.IO;

namespace TACTLib.Config {
    public class BuildConfig : Config {
        public FileRecord Root;
        public FileRecord? Install;
        public FileRecord? Patch;
        public FileRecord? Download;
        public FileRecord Encoding;
        public SizeRecord? EncodingSize;
        public FileRecord? VFSRoot;
        
        public BuildConfig(Stream? stream) : base(stream) {
            GetFileRecord("root", out var root);
            GetFileRecord("install", out Install);
            GetFileRecord("patch", out Patch);
            GetFileRecord("download", out Download);
            GetFileRecord("encoding", out var encoding);
            GetSizeRecord("encoding-size", out EncodingSize);
            GetFileRecord("vfs-root", out VFSRoot);

            if (root == null) throw new NullReferenceException(nameof(root));
            Root = root;
            
            if (encoding == null) throw new NullReferenceException(nameof(encoding));
            Encoding = encoding;
        }

        private void GetFileRecord(string key, out FileRecord? @out) {
            if (!Values.TryGetValue(key, out var list)) {
                @out = null;
                return;
            }
            @out = GetFileRecord(list);
        }
        
        private void GetSizeRecord(string key, out SizeRecord? @out) {
            if (!Values.TryGetValue(key, out var list)) {
                @out = null;
                return;
            }
            @out = new SizeRecord {
                ContentSize = int.Parse(list[0]),
                EncodedSize = int.Parse(list[1])
            };
        }

        private static FileRecord GetFileRecord(IReadOnlyList<string> vals) {
            FileRecord record = new FileRecord();

            if (vals.Count > 0) {
                record.ContentKey = CKey.FromString(vals[0]);
            }

            if (vals.Count > 1) {
                record.EncodingKey = FullEKey.FromString(vals[1]);
            }
            
            return record;
        }

        public class FileRecord {
            public CKey ContentKey;
            public FullEKey EncodingKey;
        }

        public class SizeRecord {
            public int ContentSize;
            public int EncodedSize;
        }
    }
}