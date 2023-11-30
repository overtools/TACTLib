using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace TACTLib.Config {
    public class BuildConfig : Config {
        public FileRecord Root;
        public FileRecord? Install;
        public FileRecord? Patch;
        public FileRecord? Download;
        public FileRecord Encoding;
        public SizeRecord? EncodingSize;
        public FileRecord? VFSRoot;
        public SizeRecord? VFSRootSize;
        public List<ESpecRecord> ESpecRecords = [];
        public bool HasNoEncoding { get; }

        public BuildConfig(Stream? stream) : base(stream) {
            GetFileRecord("root", out Root!);
            GetFileRecord("install", out Install);
            GetFileRecord("patch", out Patch);
            GetFileRecord("download", out Download);
            GetFileRecord("encoding", out Encoding!);
            GetSizeRecord("encoding-size", out EncodingSize);
            GetFileRecord("vfs-root", out VFSRoot);
            GetSizeRecord("vfs-root-size", out VFSRootSize);

            var vfsIndex = 0;
            while (true) {
                GetFileRecord($"vfs-{++vfsIndex}", out var vfsRecord);
                if (vfsRecord == null) {
                    break;
                }
                GetSizeRecord($"vfs-{vfsIndex}-size", out var vfsSize);
                Values.TryGetValue($"vfs-{vfsIndex}-espec", out var vfsEspecs);

                ESpecRecords.Add(new ESpecRecord {
                    Record = vfsRecord,
                    Size = vfsSize!,
                    ESpec = (vfsEspecs?.ElementAtOrDefault(0) ?? "n").ToUpper()[0],
                });
            }

            // ReSharper disable twice ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            HasNoEncoding = Root == null || Encoding == null;
            Root ??= new FileRecord();
            Encoding ??= new FileRecord();
        }

        private void GetFileRecord(string key, out FileRecord? @out) {
            if (!Values.TryGetValue(key, out var list)) {
                @out = null;
                return;
            }
            @out = GetFileRecord(Values[key]);
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

        public bool TryGetESpecRecord(FullEKey ekey, [MaybeNullWhen(false)] out ESpecRecord record) {
            foreach (var espec in ESpecRecords) {
                if (espec.Record.EncodingKey.CompareTo(ekey) == 0) {
                    record = espec;
                    return true;
                }
            }

            record = null;
            return false;
        }

        public class FileRecord {
            public CKey ContentKey;
            public FullEKey EncodingKey;
        }

        public class SizeRecord {
            public int ContentSize;
            public int EncodedSize;
        }

        public class ESpecRecord {
            public FileRecord Record = null!;
            public SizeRecord Size = null!;
            public char ESpec;
        }
    }
}
