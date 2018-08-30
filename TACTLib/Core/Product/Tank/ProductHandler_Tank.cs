using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TACTLib.Client;
using TACTLib.Container;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Tank {
    // ReSharper disable once InconsistentNaming
    public class ProductHandler_Tank : IProductHandler {
        private readonly ClientHandler _client;
        
        public readonly Manifest[] Manifests;
        public readonly RootFile[] RootFiles;
        
        public const string RegionDev = "RDEV";
        public const string SpeechManifestName = "speech";
        public const string FieldOrderCheck = @"#FILEID|MD5|CHUNK_ID|PRIORITY|MPRIORITY|FILENAME|INSTALLPATH";

        public ConcurrentDictionary<ulong, Asset> Assets;
        
        public ProductHandler_Tank(ClientHandler client, Stream stream) {
            _client = client;
            using (BinaryReader reader = new BinaryReader(stream)) {
                string str = Encoding.ASCII.GetString(reader.ReadBytes((int) stream.Length));

                string[] array = str.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                if (FieldOrderCheck != array[0])
                    throw new InvalidDataException($"ProductHandler_Tank: Root file field list mismatch ({array[0]})");

                RootFiles = new RootFile[array.Length - 1];
                for (int i = 1; i < array.Length; i++) {
                    RootFiles[i - 1] = new RootFile(array[i].Split('|'));
                }
            }

            Dictionary<string, ManifestRecord> manifestFiles = new Dictionary<string, ManifestRecord>();
            foreach (RootFile rootFile in RootFiles) {
                string extension = Path.GetExtension(rootFile.FileName);
                if (extension != ".cmf" && extension != ".apm") continue;

                string manifestName = Path.GetFileNameWithoutExtension(rootFile.FileName);
                if (manifestName == null) throw new InvalidDataException();

                if (!manifestFiles.TryGetValue(manifestName, out ManifestRecord rec)) {
                    rec = new ManifestRecord(manifestName);
                    manifestFiles[manifestName] = rec;
                }
                
                if (extension == ".cmf") {
                    rec.CMFKey = rootFile.MD5;
                } else if (extension == ".apm") {
                    rec.APMKey = rootFile.MD5;
                }
            }
            
            Manifests = new Manifest[2];
            int totalAssetCount = 0;
            foreach (KeyValuePair<string,ManifestRecord> manifestRecord in manifestFiles) {
                if (!client.EncodingHandler.TryGetEncodingEntry(manifestRecord.Value.CMFKey, out _)) continue;
                if (!client.EncodingHandler.TryGetEncodingEntry(manifestRecord.Value.APMKey, out _)) continue;
                
                if (!manifestRecord.Key.Contains(RegionDev)) continue; // is a CN (china?) CMF. todo: support this

                if (manifestRecord.Key.Contains(SpeechManifestName)) {
                    if (manifestRecord.Value.Locale != client.CreateArgs.Tank.SpokenLanguage) continue;
                } else {
                    if (manifestRecord.Value.Locale != client.CreateArgs.Tank.TextLanguage) continue;
                }

                var manifest = LoadManifest(client, manifestRecord.Value);
                if (manifestRecord.Key.Contains(SpeechManifestName)) {
                    Manifests[1] = manifest;
                } else {
                    Manifests[0] = manifest;
                }
                totalAssetCount += manifest.ContentManifest.Header.DataCount;
            }

            Assets = new ConcurrentDictionary<ulong, Asset>(Environment.ProcessorCount + 2, totalAssetCount);
            Logger.Info("CASC", "Mapping assets...");
            using (var _ = new PerfCounter("ProductHandler_Tank: Map assets"))
            for (int i = 0; i < Manifests.Length; i++) {
                Manifest manifest = Manifests[i];

                int i1 = i;
                Parallel.For(0, manifest.PackageManifest.Header.PackageCount, new ParallelOptions {
                    MaxDegreeOfParallelism = 4
                }, j =>
                {
                    var assets = manifest.PackageManifest.Records[j];

                    for (int k = 0; k < assets.Length; k++) {
                        var asset = assets[k];
                        if (Assets.ContainsKey(asset.GUID)) continue;
                        Assets[asset.GUID] = new Asset((byte)i1, j, k);
                    }
                });
            }
        }

        public struct Asset {
            public byte ManifestIdx;
            public int PackageIdx;
            public int RecordIdx;

            public Asset(byte manifestIdx, int packageIdx, int recordIdx) {
                ManifestIdx = manifestIdx;
                PackageIdx = packageIdx;
                RecordIdx = recordIdx;
            }
        }

        private static Manifest LoadManifest(ClientHandler client, ManifestRecord record) {
            Manifest manifest = new Manifest();
            using (Stream cmfStream = client.OpenCKey(record.CMFKey)) {
                try {
                    manifest.ContentManifest = new ContentManifestFile(client, cmfStream, $"{record.Name}.cmf");
                } catch (CryptographicException) {
                    Logger.Error("CASC", "Fatal - CMF decryption failed. Please update TACTLib.");
                    if (Debugger.IsAttached) {
                        Debugger.Break();
                    }
                }
            }

            using (Stream apmStream = client.OpenCKey(record.APMKey)) {
                manifest.PackageManifest = new ApplicationPackageManifest(client, apmStream, manifest.ContentManifest, record.Name);
            }
            return manifest;
        }

        public class ManifestRecord {
            public string Name;
            public string Locale;
            
            public CKey CMFKey;
            public CKey APMKey;

            public ManifestRecord(string manifestName) {
                Name = manifestName;
                Locale = GetManifestLocale(manifestName);
            }
        }

        public class Manifest {
            public ContentManifestFile ContentManifest;
            public ApplicationPackageManifest PackageManifest;
        }

        public class RootFile {
            public string FileID;
            public CKey MD5;
            public byte ChunkID;
            public byte Priority;
            public byte MPriority;
            public string FileName;
            public string InstallPath;

            public RootFile(string[] data) {
                FileID = data[0];
                MD5 = CKey.FromString(data[1]);
                ChunkID = byte.Parse(data[2]);
                Priority = byte.Parse(data[3]);
                MPriority = byte.Parse(data[4]);
                FileName = data[5];
                InstallPath = data[6];
            }
        }

        private static string GetManifestLocale(string name) {
            string tag = name.Split('_').Reverse().Single(v => v[0] == 'L' && v.Length == 5);

            return tag?.Substring(1);
        }
        
        //private readonly Dictionary<CKey, byte[]> _bundleCache = new Dictionary<CKey, byte[]>(CASCKeyComparer.Instance);
        
        public Stream OpenFile(ulong guid) {
            if (!Assets.TryGetValue(guid, out Asset asset)) throw new FileNotFoundException($"{guid:X8}");
            var manifest = Manifests[asset.ManifestIdx];
            var package = manifest.PackageManifest.Packages[asset.PackageIdx];
            var record = manifest.PackageManifest.Records[asset.PackageIdx][asset.RecordIdx];
            if ((record.Flags & ContentFlags.Bundle) != 0) {
                if (!manifest.ContentManifest.TryGet(record.GUID, out var data)) {
                    throw new FileNotFoundException();
                }

                using (Stream bundleStream = manifest.ContentManifest.OpenFile(_client, package.BundleGUID)) {
                    MemoryStream stream = new MemoryStream((int)data.Size);
                    bundleStream.Position = record.BundleOffset;
                    Utils.CopyBytes(bundleStream, stream, (int)data.Size);
                    stream.Position = 0;
                    return stream;
                }
                //using (Stream bundle = _client.OpenCKey())
                // if (!_bundleCache.ContainsKey(record.LoadHash)) {
                //     using (Stream bundleStream = CASC.OpenFile(enc.Key)) {
                //         byte[] buf = new byte[bundleStream.Length];
                //         bundleStream.Read(buf, 0, (int)bundleStream.Length);
                //         _bundleCache[record.LoadHash] = buf;
                //     }
                // }
                // MemoryStream stream = new MemoryStream((int)record.Size);
                // stream.Write(_bundleCache[record.LoadHash], (int)record.Offset, (int)record.Size);
                // stream.Position = 0;
                // return stream;
            }

            return Manifests[asset.ManifestIdx].ContentManifest.OpenFile(_client, record.GUID);
        }

        //public void WipeBundleCache() {
        //    _bundleCache.Clear();
        //}
    }
}