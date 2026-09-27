using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using TACTLib.Client;

namespace TACTLib.Core.VFS {
    public class VFSFileTree {
        private readonly ClientHandler _client;

        private readonly Dictionary<string, VFSFile> _files;
        private readonly List<VFSManifestReader.Manifest> _manifests;

        public IEnumerable<string> Files => _files.Keys;

        public VFSFileTree(ClientHandler client) {
            _client = client;
            _files = [];
            _manifests = [];
        }

        public bool Load(Stream? stream) {
            if (!IsVFSFile(stream)) {
                return false;
            }

            using var reader = new BinaryReader(stream, Encoding.ASCII);

            var manifest = VFSManifestReader.Read(reader);
            _manifests.Add(manifest);

            _files.EnsureCapacity(_files.Count + manifest.Files.Count);
            foreach (var file in manifest.Files) {
                if (file.Name != null) {
                    _files.Add(file.Name, file);
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if a stream is a VFS file
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static bool IsVFSFile([NotNullWhen(true)] Stream? stream) {
            if (stream == null || stream.Length - stream.Position < Unsafe.SizeOf<VFSManifestReader.ManifestHeader>()) {
                return false;
            }

            var magic = 0u;
            stream.ReadExactly(MemoryMarshal.AsBytes(new Span<uint>(ref magic)));
            stream.Position -= sizeof(uint);
            return magic == 0x53465654;
        }

        /// <summary>
        /// Open file by path
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">where esize?</exception>
        public Stream? Open(string file) {
            if (!_files.TryGetValue(file, out var vfsFile)) {
                return null;
            }

            if (vfsFile.CKey is {} cKey && _client.OpenCKey(cKey) is {} stream) {
                return stream;
            }

            if (vfsFile is { CSize: 0, ESize: 0 }) {
                if (_client.IsStaticContainer || _client.EncodingHandler == null) {
                    throw new NotImplementedException("where esize?");
                }

                vfsFile.ESize = _client.EncodingHandler.GetEncodedSize(vfsFile.EKey);
            }

            return _client.OpenEKey(vfsFile.EKey, vfsFile.ESize, vfsFile.ESpec);
        }
    }
}
