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
using TACTLib.Client.HandlerArgs;
using TACTLib.Core.Product.CommonV2;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Tank {
    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable once InconsistentNaming
    [ProductHandler(TACTProduct.Overwatch)]
    public class ProductHandler_Tank : IProductHandler {
        private readonly ClientHandler _client;

        public readonly RootFile[] RootFiles;

        public readonly ApplicationPackageManifest PackageManifest;
        public readonly ContentManifestFile MainContentManifest;
        public readonly ContentManifestFile SpeechContentManifest;

        public const string RegionDev = "RDEV";
        public const string SpeechManifestName = "speech";
        private const string FieldOrderCheck = @"#FILEID|MD5|CHUNK_ID|PRIORITY|MPRIORITY|FILENAME|INSTALLPATH";

        public ConcurrentDictionary<ulong, Asset> Assets;

        public ProductHandler_Tank(ClientHandler client, Stream stream) {
            _client = client;

            var clientArgs = client.CreateArgs.HandlerArgs as ClientCreateArgs_Tank ?? new ClientCreateArgs_Tank();

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

            if (!clientArgs.LoadManifest) return;

            int totalAssetCount = 0;
            
            foreach (RootFile rootFile in RootFiles.Reverse()) {  // cmf first, then apm
                string extension = Path.GetExtension(rootFile.FileName);
                if (extension != ".cmf" && extension != ".apm") continue;
                
                string manifestName = Path.GetFileNameWithoutExtension(rootFile.FileName);
                if (manifestName == null) throw new InvalidDataException();
                
                if (!manifestName.Contains(RegionDev)) continue; // is a CN (china?) CMF. todo: support this
                var locale = GetManifestLocale(manifestName);
                
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (extension == ".cmf") {
                    bool speech = manifestName.Contains(SpeechManifestName);

                    if (speech) {
                        if (locale != client.CreateArgs.SpeechLanguage) continue;
                    } else {
                        if (locale != client.CreateArgs.TextLanguage) continue;
                    }
                    
                    ContentManifestFile cmf;
                    using (Stream cmfStream = client.OpenCKey(rootFile.MD5)) {
                        try {
                            cmf = new ContentManifestFile(client, cmfStream, $"{manifestName}.cmf");
                        } catch (CryptographicException) {
                            Logger.Error("CASC", "Fatal - CMF decryption failed. Please update TACTLib.");
                            if (Debugger.IsAttached) {
                                Debugger.Break();
                            }
                            throw;
                        }
                    }
                    if (speech) {
                        SpeechContentManifest = cmf;
                    } else {
                        MainContentManifest = cmf;
                    }
                    totalAssetCount += cmf.Header.DataCount;
                } else if (extension == ".apm") {
                    if (locale != client.CreateArgs.TextLanguage) continue;
                    using (Stream apmStream = client.OpenCKey(rootFile.MD5)) {
                        PackageManifest = new ApplicationPackageManifest(client, this, apmStream, manifestName);
                    }
                }
            }
            
            Assets = new ConcurrentDictionary<ulong, Asset>(Environment.ProcessorCount + 2, totalAssetCount);

            for (int i = 0; i < PackageManifest.Header.PackageCount; i++) {
                var records = PackageManifest.Records[i];

                for (int j = 0; j < records.Length; j++) {
                    var record = records[j];
                    //Console.Out.WriteLine($"{record.GUID:X8} {record.Unknown1} {record.Unknown2} {Manifests[0].ContentManifest.Exists(record.GUID)} {Manifests[1].ContentManifest.Exists(record.GUID)}");

                    if (Assets.ContainsKey(record.GUID)) continue;
                    Assets[record.GUID] = new Asset(i, j);
                }
            }

            foreach (ContentManifestFile contentManifestFile in new [] {MainContentManifest, SpeechContentManifest}) {
                Parallel.For(0, contentManifestFile.HashList.Length, new ParallelOptions {
                    MaxDegreeOfParallelism = 4
                }, j => {
                    var cmfAsset = contentManifestFile.HashList[j];
                    if (Assets.ContainsKey(cmfAsset.GUID)) return;
                    Assets[cmfAsset.GUID] = new Asset(-1, j);
                }); 
            }
        }

        public struct Asset {
            public int PackageIdx;
            public int RecordIdx;

            public Asset(int packageIdx, int recordIdx) {
                PackageIdx = packageIdx;
                RecordIdx = recordIdx;
            }
        }

        private static string GetManifestLocale(string name) {
            string tag = name.Split('_').Reverse().SingleOrDefault(v => v[0] == 'L' && v.Length >= 4);

            return tag?.Substring(1);
        }

        /// <inheritdoc />
        public Stream OpenFile(object key) {
            switch (key) {
                case ulong guid:
                    return OpenFile(guid);
                case long badGuid:
                    return OpenFile((ulong) badGuid);
                default:
                    throw new ArgumentException(nameof(key));
            }
        }

        /// <summary>
        /// Opens file by GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public Stream OpenFile(ulong guid) {
            if (!Assets.TryGetValue(guid, out Asset asset)) throw new FileNotFoundException($"{guid:X8}");
            return OpenFile(asset);
        }

        private readonly Dictionary<ulong, Memory<byte>> _bundleCache = new Dictionary<ulong, Memory<byte>>();
        private readonly Dictionary<ulong, Dictionary<ulong, uint>> _bundleOffsetCache = new Dictionary<ulong, Dictionary<ulong, uint>>();
        
        /// <summary>
        /// Open an <see cref="Asset"/>
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public Stream OpenFile(Asset asset) {
            UnpackAsset(asset, out var package, out var record);

            if (!record.Flags.HasFlag(ApplicationPackageManifest.RecordFlags.Bundle)) {
                var cmf = GetContentManifestForAsset(record.GUID);
                return cmf.OpenFile(_client, record.GUID);
            }
            
            ulong[] bundles = PackageManifest.PackageBundles[asset.PackageIdx];

            foreach (ulong bundleGuid in bundles) {
                var cmf = GetContentManifestForAsset(bundleGuid);
                if (!_bundleOffsetCache.TryGetValue(bundleGuid, out var offsetCache)) {
                    offsetCache = CreateOffsetCache(cmf, bundleGuid);
                    _bundleOffsetCache[bundleGuid] = offsetCache;
                }
                if (offsetCache.TryGetValue(record.GUID, out uint offset)) {
                    lock (_bundleCache) {
                        if (!cmf.TryGet(record.GUID, out var data)) {
                            throw new FileNotFoundException();
                        }
                        var slice =_bundleCache[bundleGuid].Slice((int)offset, (int)data.Size);
                        return new MemoryStream(slice.ToArray());
                    }
                }
            }
            throw new Exception("bundle not found. :tim:");
        }

        private Dictionary<ulong, uint> CreateOffsetCache(ContentManifestFile cmf, ulong bundleGuid) {
            using (Stream bundleStream = cmf.OpenFile(_client, bundleGuid)) {
                Memory<byte> buf = new byte[(int) bundleStream.Length];
                bundleStream.Read(buf);
                lock (_bundleCache) {
                    _bundleCache[bundleGuid] = buf;
                }
                bundleStream.Position = 0;
                
                Bundle bundle = new Bundle(bundleStream);
                return bundle.Entries.ToDictionary(x => x.GUID, x => x.Offset);
            }
        }

        public ContentManifestFile GetContentManifestForAsset(ulong guid) {
            if (SpeechContentManifest.Exists(guid)) {
                return SpeechContentManifest;
            }
            return MainContentManifest;
        }
        
        /// <summary>
        /// Unpacks asset indices to real data
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="package"></param>
        /// <param name="record"></param>
        public void UnpackAsset(Asset asset, out ApplicationPackageManifest.Package package, out ApplicationPackageManifest.PackageRecord record) {
            if (asset.PackageIdx == -1) {
                package = new ApplicationPackageManifest.Package();
                record = new ApplicationPackageManifest.PackageRecord {
                    GUID = MainContentManifest.HashList[asset.RecordIdx].GUID
                };
            } else {
                package = PackageManifest.Packages[asset.PackageIdx];
                record = PackageManifest.Records[asset.PackageIdx][asset.RecordIdx];
            }
        }

        /// <summary>
        /// Clears bundle cache
        /// </summary>
        public void WipeBundleCache() {
            lock (_bundleCache) {
                _bundleCache.Clear();
            }
        }
    }
}
