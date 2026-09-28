using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TACTLib.Config {

    public class BuildConfig : Config {
        public FileRecord? Root;
        public FileRecord? Install;
        public FileRecord? Patch;
        public FileRecord? Download;
        public FileRecord? Encoding;
        public SizeRecord? EncodingSize;
        public VFSRecord? VFSRoot;
        public List<VFSRecord> VFS;

        public string GetBuildName() => (Values.TryGetValue("build-name", out var buildName) ? buildName.FirstOrDefault() : null) ?? "Unknown";

        public BuildConfig(Stream? stream) : base(stream) {
            TryGetFileRecord("root", out Root);
            TryGetFileRecord("install", out Install);
            TryGetFileRecord("patch", out Patch);
            TryGetFileRecord("download", out Download);
            TryGetFileRecord("encoding", out Encoding);
            TryGetSizeRecord("encoding-size", out EncodingSize);
            BuildVFS();
        }

        private void BuildVFS() {
            VFS = [];

            if (!TryGetVFSRecord("vfs-root", out VFSRoot)) {
                return;
            }

            VFS.Add(VFSRoot);
            var index = 1;
            while (TryGetVFSRecord("vfs-" + index++, out var vfs)) {
                VFS.Add(vfs);
            }
        }

        private bool TryGetFileRecord(string key, [MaybeNullWhen(false)] out FileRecord record) {
            if (!Values.TryGetValue(key, out var list)) {
                record = null;
                return false;
            }

            record = GetFileRecord(list);
            return true;
        }

        private bool TryGetSizeRecord(string key, [MaybeNullWhen(false)] out SizeRecord record) {
            if (!Values.TryGetValue(key, out var list)) {
                record = null;
                return false;
            }

            record = GetSizeRecord(list);
            return true;
        }

        private bool TryGetVFSRecord(string key, [MaybeNullWhen(false)] out VFSRecord record) {
            if (!TryGetFileRecord(key, out var file)) {
                record = null;
                return false;
            }

            record = new VFSRecord {
                File = file,
                Size = TryGetSizeRecord(key + "-size", out var size) ? size : default,
                Spec = Values.TryGetValue(key + "-espec", out var list) ? string.Join(" ", list) : default,
            };
            return true;
        }

        private static FileRecord GetFileRecord(List<string> vals) => new() {
            ContentKey = vals.Count > 0 ? CKey.FromString(vals[0]) : default,
            EncodingKey = vals.Count > 1 ? CKey.FromString(vals[1]) : default,
        };

        private static SizeRecord GetSizeRecord(List<string> vals) => new() {
            ContentSize = vals.Count > 0 ? int.Parse(vals[0], NumberStyles.Integer, CultureInfo.InvariantCulture) : default,
            EncodedSize = vals.Count > 1 ? int.Parse(vals[1], NumberStyles.Integer, CultureInfo.InvariantCulture) : default,
        };

        public record FileRecord {
            public CKey ContentKey { get; init; }
            public FullEKey EncodingKey { get; init; }
        }

        public record SizeRecord {
            public int ContentSize { get; init; }
            public int EncodedSize { get; init; }
        }

        public record VFSRecord {
            public required FileRecord File { get; init; }
            public SizeRecord? Size { get; init; }
            public string? Spec { get; init; }
        }
    }
}
