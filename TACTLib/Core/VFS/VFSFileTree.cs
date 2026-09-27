using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using TACTLib.Client;

namespace TACTLib.Core.VFS {
    public class VFSFileTree {
        private readonly ClientHandler _client;

        private readonly Dictionary<string, VFSFile> _files;
        private readonly VFSManifestReader.Manifest _manifest;

        public readonly ReadOnlyCollection<string> Files;

        public VFSFileTree(ClientHandler client, Stream stream) {
            _client = client;
            using BinaryReader reader = new BinaryReader(stream, Encoding.ASCII);
            //using (Stream file = File.OpenWrite("vfs.hex")) {
            //    stream.CopyTo(file);
            //    stream.Position = 0;
            //}

            _manifest = VFSManifestReader.Read(reader);

            _files = new Dictionary<string, VFSFile>(_manifest.Files.Count);
            foreach (VFSFile file in _manifest.Files) {
                if (file.Name != null) {
                    _files[file.Name] = file;
                }
            }

            Files = Array.AsReadOnly(_files.Keys.ToArray());
        }

        /// <summary>
        /// Checks if a stream is a VFS file
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static bool IsVFSFile(Stream stream) {
            if (stream.Length - stream.Position < Unsafe.SizeOf<VFSManifestReader.ManifestHeader>()) {
                return false;
            }

            var magic = 0u;
            stream.ReadExactly(MemoryMarshal.AsBytes(new Span<uint>(ref magic)));
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

            return _client.OpenEKey(vfsFile.EKey, vfsFile.CSize == 0 ?vfsFile.ESize  : vfsFile.CSize, vfsFile.ESpec);
        }
    }
}
