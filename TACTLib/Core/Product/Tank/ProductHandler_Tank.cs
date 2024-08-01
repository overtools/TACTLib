using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TACTLib.Client;
using TACTLib.Client.HandlerArgs;
using TACTLib.Core.Product.CommonV2;

namespace TACTLib.Core.Product.Tank {
    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable once InconsistentNaming
    [ProductHandler(TACTProduct.Overwatch)]
    public partial class ProductHandler_Tank : IProductHandler {
        private readonly ClientHandler m_client;

        public readonly RootFile[] m_rootFiles;

        public readonly AssetPackageManifest? m_packageManifest;
        public readonly ContentManifestFile m_rootContentManifest = null!;
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
        public readonly ConcurrentDictionary<ulong, Asset> m_assets = null!;
        private readonly bool m_usingResourceGraph;

        public const uint VERSION_148_PTR = 68309;
        public const uint VERSION_152_PTR = 72317;

        public const int PACKAGE_IDX_FAKE_TEXT_CMF = -1;
        public const int PACKAGE_IDX_FAKE_SPEECH_CMF = -2;
        public const int PACKAGE_IDX_FAKE_ROOT_CMF = -3;
        
        public const string REGION_DEV = "RDEV";
        public const string REGION_CN = "RCN";
        public const string SPEECH_MANIFEST_NAME = "speech";
        public const string TEXT_MANIFEST_NAME = "text";
        
        public static string? GetManifestLocale(string name) {
            var tag = name.Split('_').Reverse().SingleOrDefault(v => v[0] == 'L' && v.Length >= 4);
            return tag?.Substring(1);
        }

        private const string OutdatedTACTLibErrorMessage =
            "Fatal - Manifest decryption failed. Please update TACTLib. This can happen due the game receiving a new patch that is not supported by this version of TACTLib.";

        public ProductHandler_Tank(ClientHandler client, Stream stream) {
            m_client = client;

            var clientArgs = client.CreateArgs.HandlerArgs as ClientCreateArgs_Tank ?? new ClientCreateArgs_Tank();

            using (var reader = new StreamReader(stream)) {
                m_rootFiles = RootFile.ParseList(reader).ToArray();
            }

            if (!clientArgs.LoadManifest) return;

            var totalAssetCount = 0;
            
            foreach (var rootFile in m_rootFiles.Reverse()) {  // cmf first, then apm
                var extension = Path.GetExtension(rootFile.FileName!);
                if (extension != ".cmf" && extension != ".apm" && extension != ".trg") {
                    // not a manifest
                    continue;
                }

                var manifestName = Path.GetFileNameWithoutExtension(rootFile.FileName!); // for matching
                var manifestFileName = Path.GetFileName(rootFile.FileName!); // for crypto

                if (!manifestName.Contains(clientArgs.ManifestRegion ?? REGION_DEV)) continue; // is a CN (china) CMF
                var manifestLocale = GetManifestLocale(manifestName);
                
                switch (extension) {
                    case ".cmf": {
                        var isSpeech = manifestName.Contains(SPEECH_MANIFEST_NAME);
                        var isText = manifestName.Contains(TEXT_MANIFEST_NAME);

                        if (isSpeech) {
                            if (manifestLocale != client.CreateArgs.SpeechLanguage) continue;
                        } else if (manifestLocale != null) {
                            // text or old root/text combo
                            if (manifestLocale != client.CreateArgs.TextLanguage) continue;
                        }
                        
                        ContentManifestFile cmf;
                        try {
                            using var cmfStream = client.OpenCKey(rootFile.MD5)!;
                            //using (Stream file = File.OpenWrite($"{manifestName}.cmf")) {
                            //    cmfStream.CopyTo(file);
                            //}
                            cmf = new ContentManifestFile(client, cmfStream, manifestFileName);
                        } catch (CryptographicException) {
                            Logger.Error("CASC", OutdatedTACTLibErrorMessage);
                            if (Debugger.IsAttached) {
                                Debugger.Break();
                            }
                            throw;
                        }
                        if (isSpeech) {
                            m_speechContentManifest = cmf;
                        } else if (isText) {
                            m_textContentManifest = cmf;
                        } else {
                            m_rootContentManifest = cmf;
                        }
                        totalAssetCount += cmf.m_header.m_dataCount;
                        break;
                    }
                    case ".apm": {
                        if (manifestLocale != client.CreateArgs.TextLanguage) break; // not relevant
                        
                        using var apmStream = client.OpenCKey(rootFile.MD5)!;
                        m_packageManifest = new AssetPackageManifest(client, this, apmStream, manifestName);
                        break;
                    }
                    case ".trg":
                        try {
                            using var trgStream = client.OpenCKey(rootFile.MD5)!;
                            //using (Stream file = File.OpenWrite($"{manifestName}.trg")) {
                            //    trgStream.CopyTo(file);
                            //}
                            m_resourceGraph = new ResourceGraph(client, trgStream, manifestFileName);
                        } catch (CryptographicException) {
                            Logger.Error("CASC", OutdatedTACTLibErrorMessage);
                            if (Debugger.IsAttached) {
                                Debugger.Break();
                            }
                            throw;
                        }

                        break;
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

            if (m_usingResourceGraph && clientArgs.LoadBundlesForLookup) {
                m_hackedLookedUpBundles = new HashSet<ulong>();
                m_hackedBundleLookup = new Dictionary<ulong, ulong>();
                DoBundleLookupHack();
            }
        }
        
        public ContentManifestFile.HashData GetHashData(ulong guid) {
            if (!TryGetHashData(guid, out var hashData)) {
                throw new FileNotFoundException($"{guid:X16}");
            }
            return hashData;
        }
        
        public bool TryGetHashData(ulong guid, out ContentManifestFile.HashData hashData) {
            if (m_textContentManifest != null && m_textContentManifest.TryGet(guid, out hashData)) return true;
            if (m_speechContentManifest != null && m_speechContentManifest.TryGet(guid, out hashData)) return true;
            return m_rootContentManifest.TryGet(guid, out hashData);
        }

        public ContentManifestFile GetContentManifestForAsset(ulong guid) {
            if (m_textContentManifest != null && m_textContentManifest.Exists(guid)) return m_textContentManifest;
            if (m_speechContentManifest != null && m_speechContentManifest.Exists(guid)) return m_speechContentManifest;
            return m_rootContentManifest;
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

        public Stream? OpenFile(ContentManifestFile.HashData hashData) {
            return m_client.OpenCKey(hashData.ContentKey);
        }

        /// <summary>
        /// Opens file by GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public Stream? OpenFile(ulong guid) {
            if (m_assets == null!) {
                // oops, we haven't finished setup yet
                // loading bundled things at this point would be a bad idea
                var hashData = GetHashData(guid);
                return OpenFile(hashData)!;
            }
            
            if (!m_assets.TryGetValue(guid, out var asset)) throw new FileNotFoundException($"{guid:X16}");

            if (m_hackedBundleLookup != null && m_hackedBundleLookup.TryGetValue(guid, out var bundleGUID)) {
                return OpenFileFromBundle(bundleGUID, guid);
            }
            
            var normalStream = OpenPackageFile(asset);
            // could be null here, if in encrypted bundle that the hack can't read
            return normalStream;
        }
        
        /// <summary>
        /// Open an <see cref="Asset"/>
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        private Stream? OpenPackageFile(Asset asset) {
            UnpackPackageAsset(asset, out var record);

            if (!record.m_flags.HasFlag(AssetPackageManifest.RecordFlags.Bundle)) {
                var hashData = GetHashData(record.m_GUID);
                return OpenFile(hashData);
            }
            
            var bundles = m_packageManifest!.m_packageBundles[asset.m_packageIdx];
            foreach (var bundleGuid in bundles) {
                var foundStream = OpenFileFromBundle(bundleGuid, record.m_GUID);
                if (foundStream != null) {
                    return foundStream;
                }
            }
            throw new Exception("bundle not found. :tim:");
        }
        
        /// <summary>
        /// Unpacks asset indices to real data
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="record"></param>
        public void UnpackPackageAsset(Asset asset, out AssetPackageManifest.PackageRecord record) {
            if (asset.m_packageIdx >= 0) {
                record = m_packageManifest!.m_packageRecords[asset.m_packageIdx][asset.m_recordIdx];
                return;
            }
            
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

            record = new AssetPackageManifest.PackageRecord {
                m_GUID = contentManifest.m_hashList[asset.m_recordIdx].GUID
            };
        }
    }
}
