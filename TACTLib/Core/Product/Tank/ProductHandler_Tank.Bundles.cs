using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TACTLib.Exceptions;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Tank {
    public partial class ProductHandler_Tank {
        private class BundleCache {
            public Dictionary<ulong, uint> m_offsets;
            public Memory<byte> m_buffer;
        }
        private readonly Dictionary<ulong, BundleCache> m_bundleCache = new Dictionary<ulong, BundleCache>();
        
        private readonly Dictionary<ulong, ulong>? m_hackedBundleLookup;
        private readonly HashSet<ulong>? m_hackedLookedUpBundles;

        private void DoBundleLookupHack() {
            if (!m_usingResourceGraph) return;
            
            foreach (var asset in m_assets) {
                if ((asset.Key & 0xFFF000000000000ul) != 0x0D90000000000000) continue; // bundles only
                if (m_hackedLookedUpBundles!.Contains(asset.Key)) continue; // already done

                if (!TryGetHashData(asset.Key, out _)) {
                    Logger.Debug("TRG", $"bundle {asset.Key:X16} doesn't exist???");
                    continue;
                }

                Bundle bundle;
                try {
                    bundle = OpenBundle(asset.Key);
                } catch (BLTEKeyException e) {
                    Logger.Debug("TRG", $"can't load bundle {asset.Key:X16} because key {e.MissingKey:X8} is missing from the keyring.");
                    continue;
                }

                if (bundle.Header.OffsetSize == 0) {
                    throw new InvalidDataException($"failed to load bundle {asset.Key:X16}");
                }
                
                foreach (var valuePair in bundle.Entries) {
                    m_hackedBundleLookup![valuePair.GUID] = asset.Key;
                }
                m_hackedLookedUpBundles.Add(asset.Key);
            }
        }

        private BundleCache GetBundleCache(ulong bundleGuid) {
            lock (m_bundleCache) {
                if (m_bundleCache.TryGetValue(bundleGuid, out var cache)) return cache;

                Bundle bundle;
                byte[] buf;
                using (var bundleStream = OpenFile(bundleGuid)) {
                    buf = new byte[(int) bundleStream.Length];
                    bundleStream.DefinitelyRead(buf);
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
            using (var bundleStream = OpenFile(bundleGuid))
                return new Bundle(bundleStream, m_usingResourceGraph);
        }

        private Stream? OpenFileFromBundle(ulong bundleGuid, ulong guid) {
            var cache = GetBundleCache(bundleGuid);
            if (!cache.m_offsets.TryGetValue(guid, out var offset)) return null;
            
            var hashData = GetHashData(guid);
            var slice = cache.m_buffer.Slice((int)offset, (int)hashData.Size);
            return new MemoryStream(slice.ToArray());
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
}
