using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

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
        public IReadOnlyList<FileRecord>? VFSManifests;
        public IReadOnlyList<SizeRecord>? VFSManifestsSize;

        public string GetBuildName() => Values["build-name"][0];

        public BuildConfig(Stream? stream) : base(stream) {
            GetRecord("root", out Root);
            TryGetRecord("install", out Install);
            TryGetRecord("patch", out Patch);
            TryGetRecord("download", out Download);
            GetRecord("encoding", out Encoding);
            TryGetRecord("encoding-size", out EncodingSize);

            TryGetRecord("vfs-root", out VFSRoot);
            TryGetRecord("vfs-root-size", out VFSRootSize);
            TryGetRecords("vfs-{0}", 1, out VFSManifests);
            TryGetRecords("vfs-{0}-size", 1, out VFSManifestsSize);
        }

        private void GetRecord<T>(string key, out T @out) where T : IBuildConfigRecord<T> {
            if (!TryGetRecord(key, out @out!)) {
                throw new NullReferenceException($"Failed to find \"{key}\" in the build config.");
            }
        }

        private bool TryGetRecord<T>(string key, [NotNullWhen(true)] out T? @out) where T : IBuildConfigRecord<T> {
            if (!Values.TryGetValue(key, out var vals)) {
                @out = default;
                return false;
            }

            @out = T.Decode(vals);
            return true;
        }

        private void GetRecords<T>(string key, int baseIter, out IReadOnlyList<T> @out) where T : IBuildConfigRecord<T> {
            if (!TryGetRecords(key, baseIter, out @out!)) {
                throw new NullReferenceException($"Failed to find any \"{key}\" build config records.");
            }
        }

        private bool TryGetRecords<T>(string key, int baseIter, [NotNullWhen(true)] out IReadOnlyList<T>? @out) where T : IBuildConfigRecord<T> {
            Debug.Assert(key.Contains("{0}"));

            if (!Values.TryGetValue(string.Format(key, baseIter++), out var baseVals)) {
                @out = null;
                return false;
            }

            var values = new List<T> { T.Decode(baseVals) };
            @out = values;
            while (true) {
                if (!Values.TryGetValue(string.Format(key, baseIter++), out var vals)) {
                    break;
                }

                values.Add(T.Decode(vals));
            }

            return true;
        }

        private interface IBuildConfigRecord<out T> where T : IBuildConfigRecord<T> {
            abstract static T Decode(List<string> vals);
        }

        public class FileRecord : IBuildConfigRecord<FileRecord> {
            public CKey ContentKey;
            public FullEKey EncodingKey;

            public static FileRecord Decode(List<string> vals) {
                FileRecord record = new FileRecord();

                if (vals.Count > 0) {
                    record.ContentKey = CKey.FromString(vals[0]);
                }

                if (vals.Count > 1) {
                    record.EncodingKey = FullEKey.FromString(vals[1]);
                }

                return record;
            }
        }

        public class SizeRecord : IBuildConfigRecord<SizeRecord> {
            public int ContentSize;
            public int EncodedSize;

            public static SizeRecord Decode(List<string> vals) {
                return new SizeRecord {
                    ContentSize = int.Parse(vals[0]),
                    EncodedSize = int.Parse(vals[1])
                };
            }
        }
    }
}