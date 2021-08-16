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
using TACTLib.Exceptions;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Tank {
    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable once InconsistentNaming
    [ProductHandler(TACTProduct.Overwatch)]
    public class ProductHandler_Tank : IProductHandler {
        private readonly ClientHandler m_client;

        public readonly RootFile[] m_rootFiles;

        public readonly ApplicationPackageManifest? m_packageManifest;
        public readonly ContentManifestFile m_rootContentManifest;
        public readonly ContentManifestFile? m_textContentManifest;
        public readonly ContentManifestFile? m_speechContentManifest;
        public readonly ResourceGraph? m_resourceGraph;
        
        public struct Asset {
            public int m_packageIdx;
            public int m_recordIdx;

            public Asset(int packageIdx, int recordIdx) {
                m_packageIdx = packageIdx;
                m_recordIdx = recordIdx;
            }
        }
        public ConcurrentDictionary<ulong, Asset> m_assets;
        
        private readonly Dictionary<ulong, ulong>? m_hackedBundleLookup;
        private readonly HashSet<ulong>? m_hackedLookedUpBundles;
        private readonly bool m_usingResourceGraph;

        private class BundleCache {
            public Dictionary<ulong, uint> m_offsets;
            public Memory<byte> m_buffer;
        }
        private readonly Dictionary<ulong, BundleCache> m_bundleCache = new Dictionary<ulong, BundleCache>();

        public const uint VERSION_148_PTR = 68309;
        public const uint VERSION_152_PTR = 72317;

        public const int PACKAGE_IDX_FAKE_TEXT_CMF = -1;
        public const int PACKAGE_IDX_FAKE_SPEECH_CMF = -2;
        public const int PACKAGE_IDX_FAKE_ROOT_CMF = -3;
        
        public const string REGION_DEV = "RDEV";
        public const string REGION_CN = "RCN";
        public const string SPEECH_MANIFEST_NAME = "speech";
        public const string TEXT_MANIFEST_NAME = "text";
        public const string ROOT_FILE_FIELD_ORDER_CHECK = @"#FILEID|MD5|CHUNK_ID|PRIORITY|MPRIORITY|FILENAME|INSTALLPATH";

        private const string OutdatedTACTLibErrorMessage =
            "Fatal - Manifest decryption failed. Please update TACTLib. This can happen due the game receiving a new patch that is not supported by this version of TACTLib.";

        public ProductHandler_Tank(ClientHandler client, Stream stream) {
            m_client = client;

            var clientArgs = client.CreateArgs.HandlerArgs as ClientCreateArgs_Tank ?? new ClientCreateArgs_Tank();

            using (BinaryReader reader = new BinaryReader(stream)) {
                string str = Encoding.ASCII.GetString(reader.ReadBytes((int) stream.Length));

                string[] array = str.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                if (ROOT_FILE_FIELD_ORDER_CHECK != array[0]) {
                    throw new InvalidDataException($"ProductHandler_Tank: Root file field list mismatch ({array[0]})");
                }

                m_rootFiles = new RootFile[array.Length - 1];
                for (var i = 1; i < array.Length; i++) {
                    m_rootFiles[i - 1] = new RootFile(array[i].Split('|'));
                }
            }

            if (!clientArgs.LoadManifest) return;

            var totalAssetCount = 0;
            
            foreach (RootFile rootFile in m_rootFiles.Reverse()) {  // cmf first, then apm
                string extension = Path.GetExtension(rootFile.FileName!);
                if (extension != ".cmf" && extension != ".apm" && extension != ".trg") continue;

                var manifestName = Path.GetFileNameWithoutExtension(rootFile.FileName!);
                var manifestFileName = Path.GetFileName(rootFile.FileName!);

                if (!manifestName.Contains(clientArgs.ManifestRegion ?? REGION_DEV)) continue; // is a CN (china) CMF. todo: support this
                var locale = GetManifestLocale(manifestName);
                
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (extension == ".cmf") {
                    var speech = manifestName.Contains(SPEECH_MANIFEST_NAME);
                    var text = manifestName.Contains(TEXT_MANIFEST_NAME);

                    if (speech) {
                        if (locale != client.CreateArgs.SpeechLanguage) continue;
                    } else if (locale != null) {
                        // text or old root/text combo
                        if (locale != client.CreateArgs.TextLanguage) continue;
                    }
                        
                    ContentManifestFile cmf;
                    //using (Stream file = File.OpenWrite($"{manifestName}.cmf")) {
                    //    cmfStream.CopyTo(file);
                    //}
                    try {
                        using (Stream cmfStream = client.OpenCKey(rootFile.MD5)!)
                            cmf = new ContentManifestFile(client, cmfStream, manifestFileName);
                    } catch (CryptographicException) {
                        Logger.Error("CASC", OutdatedTACTLibErrorMessage);
                        if (Debugger.IsAttached) {
                            Debugger.Break();
                        }
                        throw;
                    }
                    if (speech) {
                        m_speechContentManifest = cmf;
                    } else if (text) {
                        m_textContentManifest = cmf;
                    } else {
                        m_rootContentManifest = cmf;
                    }
                    totalAssetCount += cmf.m_header.m_dataCount;
                } else if (extension == ".apm") {
                    if (locale != client.CreateArgs.TextLanguage) continue;
                    using (Stream apmStream = client.OpenCKey(rootFile.MD5)!)
                        m_packageManifest = new ApplicationPackageManifest(client, this, apmStream, manifestName);
                } else if (extension == ".trg") {
                    try {
                        using (Stream trgStream = client.OpenCKey(rootFile.MD5)!) {
                            //using (Stream file = File.OpenWrite($"{manifestName}.trg")) {
                            //    trgStream.CopyTo(file);
                            //}
                            
                            m_resourceGraph = new ResourceGraph(client, trgStream, manifestFileName);
                        }
                    } catch (CryptographicException) {
                        Logger.Error("CASC", OutdatedTACTLibErrorMessage);
                        if (Debugger.IsAttached) {
                            Debugger.Break();
                        }
                        throw;
                    }
                }
            }

            if (m_rootContentManifest == null) throw new NullReferenceException(nameof(m_rootContentManifest));

            m_usingResourceGraph = m_rootContentManifest.m_header.m_buildVersion >= VERSION_148_PTR;
            m_assets = new ConcurrentDictionary<ulong, Asset>(Environment.ProcessorCount + 2, totalAssetCount);

            if (!m_usingResourceGraph) {
                if (m_packageManifest == null) throw new NullReferenceException(nameof(m_packageManifest));

                for (var i = 0; i < m_packageManifest.m_header.m_packageCount; i++) {
                    var records = m_packageManifest.m_packageRecords[i];

                    for (var j = 0; j < records.Length; j++) {
                        var record = records[j];
                        //Console.Out.WriteLine($"{record.GUID:X8} {record.Unknown1} {record.Unknown2} {Manifests[0].ContentManifest.Exists(record.GUID)} {Manifests[1].ContentManifest.Exists(record.GUID)}");

                        if (m_assets.ContainsKey(record.m_GUID)) continue;
                        m_assets[record.m_GUID] = new Asset(i, j);
                    }
                }
            }
            
            RegisterCMFAssets(m_rootContentManifest, PACKAGE_IDX_FAKE_ROOT_CMF);
            RegisterCMFAssets(m_textContentManifest, PACKAGE_IDX_FAKE_TEXT_CMF);
            RegisterCMFAssets(m_speechContentManifest, PACKAGE_IDX_FAKE_SPEECH_CMF);

            if (m_usingResourceGraph) {
                m_hackedLookedUpBundles = new HashSet<ulong>();
                m_hackedBundleLookup = new Dictionary<ulong, ulong>();
                DoBundleLookupHack();
            }
        }

        public void DoBundleLookupHack() {
            if (!m_usingResourceGraph) return;
            
            foreach (var asset in m_assets) {
                if ((asset.Key & 0xFFF000000000000ul) != 0x0D90000000000000) continue; // bundles only
                
                if (m_hackedLookedUpBundles!.Contains(asset.Key)) continue;
            
                var cmf = GetContentManifestForAsset(asset.Key);
                if (!cmf.Exists(asset.Key)) {
                    Logger.Debug("TRG", $"bundle {asset.Key:X16} is goned???");
                    continue;
                }

                Bundle bundle;
                try {
                    bundle = OpenBundle(asset.Key);
                } catch (BLTEKeyException e) {
                    Logger.Debug("TRG", $"can't load bundle {asset.Key:X16} because key {e.MissingKey:X8} is missing from the keyring.");
                    continue;
                }

                if (bundle.Entries == null) {
                    // Logger.Debug("TRG", $"can't load bundle {asset.Key:X16} everything is fucked.");
                    throw new TankException("Bundle is fragmented: run, repair, or reinstall the game");
                }
                
                foreach (var valuePair in bundle.Entries) {
                    m_hackedBundleLookup![valuePair.GUID] = asset.Key;
                }
                m_hackedLookedUpBundles.Add(asset.Key);
            }
        }

        private void RegisterCMFAssets(ContentManifestFile? contentManifestFile, int fakePackageIdx) {
            if (contentManifestFile == null) return;
            Parallel.For(0, contentManifestFile.m_hashList.Length, new ParallelOptions {
                MaxDegreeOfParallelism = 4
            }, j => {
                var cmfAsset = contentManifestFile.m_hashList[j];
                if (m_assets.ContainsKey(cmfAsset.GUID)) return;
                m_assets[cmfAsset.GUID] = new Asset(fakePackageIdx, j);
            }); 
        }

        public static string? GetManifestLocale(string name) {
            var tag = name.Split('_').Reverse().SingleOrDefault(v => v[0] == 'L' && v.Length >= 4);
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
            if (!m_assets.TryGetValue(guid, out var asset)) throw new FileNotFoundException($"{guid:X8}");

            if (m_usingResourceGraph) {
                if (m_hackedBundleLookup!.TryGetValue(guid, out var bundleGUID)) {
                    var foundStream = OpenFileFromBundle(bundleGUID, guid);
                    return GuidStream.Create(foundStream, guid);
                }
            }

            var normalStream = OpenFile(asset);
            // could be null here, if in encrypted bundle that the hack can't read
            return GuidStream.Create(normalStream, guid);
        }
        
        /// <summary>
        /// Open an <see cref="Asset"/>
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public Stream? OpenFile(Asset asset) {
            UnpackAsset(asset, out _, out var record);

            if (!record.m_flags.HasFlag(ApplicationPackageManifest.RecordFlags.Bundle)) {
                var cmf = GetContentManifestForAsset(record.m_GUID);
                return cmf.OpenFile(m_client, record.m_GUID);
            }
            
            ulong[] bundles = m_packageManifest!.m_packageBundles[asset.m_packageIdx];

            foreach (var bundleGuid in bundles) {
                var foundStream = OpenFileFromBundle(bundleGuid, record.m_GUID);
                if (foundStream != null) {
                    return foundStream;
                }
            }
            throw new Exception("bundle not found. :tim:");
        }

        private BundleCache GetBundleCache(ulong bundleGuid) {
            lock (m_bundleCache) {
                if (m_bundleCache.TryGetValue(bundleGuid, out var cache)) return cache;
                var cmf = GetContentManifestForAsset(bundleGuid);

                Bundle bundle;
                Memory<byte> buf;
                using (Stream bundleStream = cmf.OpenFile(m_client, bundleGuid)!) {
                    buf = new byte[(int) bundleStream.Length];
                    bundleStream.Read(buf);
                    bundleStream.Position = 0;

                    //using (Stream outStr = File.OpenWrite($"{bundleGuid:X16}.bndl")) {
                    //    bundleStream.CopyTo(outStr);
                    //}
                
                    bundle = new Bundle(bundleStream, m_usingResourceGraph);
                }
                var offsetMap = bundle.Entries.ToDictionary(x => x.GUID, x => x.Offset);
                    
                cache = new BundleCache {
                    m_buffer = buf,
                    m_offsets = offsetMap
                };
                m_bundleCache[bundleGuid] = cache;
                return cache;
            }
        }
        
        private Bundle OpenBundle(ulong bundleGuid) {
            var cmf = GetContentManifestForAsset(bundleGuid);
            using (var bundleStream = cmf.OpenFile(m_client, bundleGuid))
                return new Bundle(bundleStream, m_usingResourceGraph);
        }

        private Stream? OpenFileFromBundle(ulong bundleGuid, ulong guid) {
            var cmf = GetContentManifestForAsset(bundleGuid);
            var cache = GetBundleCache(bundleGuid);

            if (!cache.m_offsets.TryGetValue(guid, out var offset)) return null;
            if (!cmf.TryGet(guid, out var data)) {
                throw new FileNotFoundException();
            }
            var slice = cache.m_buffer.Slice((int)offset, (int)data.Size);
            return new MemoryStream(slice.ToArray());
        }

        public ContentManifestFile GetContentManifestForAsset(ulong guid) {
            if (m_textContentManifest != null && m_textContentManifest.Exists(guid)) return m_textContentManifest;
            if (m_speechContentManifest != null && m_speechContentManifest.Exists(guid)) return m_speechContentManifest;
            return m_rootContentManifest;
        }
        
        /// <summary>
        /// Unpacks asset indices to real data
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="package"></param>
        /// <param name="record"></param>
        public void UnpackAsset(Asset asset, out ApplicationPackageManifest.PackageHeader package, out ApplicationPackageManifest.PackageRecord record) {
            if (asset.m_packageIdx < 0) {
                package = new ApplicationPackageManifest.PackageHeader();
                ContentManifestFile? contentManifest;
                
                if (asset.m_packageIdx == PACKAGE_IDX_FAKE_ROOT_CMF) {
                    contentManifest = m_rootContentManifest;
                } else if (asset.m_packageIdx == PACKAGE_IDX_FAKE_TEXT_CMF) {
                    contentManifest = m_textContentManifest!;
                } else if (asset.m_packageIdx == PACKAGE_IDX_FAKE_SPEECH_CMF) {
                    contentManifest = m_speechContentManifest!;
                } else {
                    throw new Exception("wat");
                }
                record = new ApplicationPackageManifest.PackageRecord {
                    m_GUID = contentManifest.m_hashList[asset.m_recordIdx].GUID
                };
            } else {
                package = m_packageManifest!.m_packages[asset.m_packageIdx];
                record = m_packageManifest.m_packageRecords[asset.m_packageIdx][asset.m_recordIdx];
            }
        }

        /// <summary>
        /// Clears bundle cache
        /// </summary>
        public void WipeBundleCache() {
            lock (m_bundleCache) {
                m_bundleCache.Clear();
            }
        }
    }

    public class GuidStream : Stream {
        public Stream BaseStream { get; }
        public ulong GUID { get; }

        public GuidStream(Stream baseStream, ulong guid) {
            BaseStream = baseStream;
            GUID = guid;
        }

        public override void Flush() {
            BaseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return BaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            BaseStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return BaseStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            BaseStream.Write(buffer, offset, count);
        }

        public override bool CanRead => BaseStream.CanRead;

        public override bool CanSeek => BaseStream.CanSeek;

        public override bool CanWrite => BaseStream.CanWrite;

        public override long Length => BaseStream.Length;

        public override long Position {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        public static GuidStream Create(Stream? baseStream, ulong guid) {
            if (baseStream == null) throw new ArgumentNullException(); // enable datatool stu error handling
            return new GuidStream(baseStream, guid);
        }
    }
}
