using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using TACTLib.Client;

namespace TACTLib.Core {
    public class VFSFileTree {
        private readonly ClientHandler _client;

        private readonly Dictionary<string, VFSFile> _files;
        private readonly VFSManifestReader.Manifest _manifest;

        public readonly ReadOnlyCollection<string> Files;

        public VFSFileTree(ClientHandler client) {
            _client = client;
            using (Stream stream = client.OpenCKey(client.ConfigHandler.BuildConfig.VFSRoot.ContentKey))
            using (BinaryReader reader = new BinaryReader(stream, Encoding.Default)) {

                //using (Stream file = File.OpenWrite("vfs.hex")) {
                //    stream.CopyTo(file);
                //    stream.Position = 0;
                //}

                _manifest = VFSManifestReader.Read(reader);

                _files = new Dictionary<string, VFSFile>(_manifest.Files.Count);
                foreach (VFSFile file in _manifest.Files) {
                    _files[file.Name] = file;
                }
            }

            Files = Array.AsReadOnly(_files.Keys.ToArray());
        }

        /// <summary>
        /// Open file by path
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public Stream Open(string file) {
            if (_files.TryGetValue(file, out VFSFile vfsFile)) {
                if (vfsFile is VFSCFile cFile) {
                    return _client.OpenCKey(cFile.CKey);
                }
                return _client.OpenEKey(vfsFile.EKey);
            }
            throw new FileNotFoundException(file);
        }
    }
}
