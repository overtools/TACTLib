using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TACTLib.Client;
using TACTLib.Exceptions;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Tank {
    public class ResourceGraph {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct TRGHeader6 { // version 5 and 6
            public uint m_0; // 0
            public uint m_buildVersion; // 4
            public uint m_8; // 8
            public uint m_12; // 12
            public uint m_16; // 16
            public uint m_20; // 20
            public int m_packageCount; // 24
            public int m_packageBlockSize; // 28
            public int m_skinCount; // 32
            public int m_skinBlockSize; // 36
            public uint m_40; // 40
            public uint m_44; // 44
            public int m_graphBlockSize; // 48
            public uint m_footerMagic; // 52

            public TRGHeader Upgrade() => new TRGHeader {
                m_0 = m_0,
                m_buildVersion = m_buildVersion,
                m_8 = m_8,
                m_12 = m_12,
                m_16 = m_16,
                m_20 = m_20,
                m_packageCount = m_packageCount,
                m_packageBlockSize = m_packageBlockSize,
                m_skinCount = m_skinCount,
                m_skinBlockSize = m_skinBlockSize,
                m_typeBundleIndexCount = 0,
                m_typeBundleIndexBlockSize = 0,
                m_48 = m_40,
                m_52 = m_44,
                m_graphBlockSize = m_graphBlockSize,
                m_footerMagic = m_footerMagic
            };
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct TRGHeader7 { // version 7, 8, 9, & 10
            public uint m_0; // 0
            public uint m_buildVersion; // 4
            public uint m_8; // 8
            public uint m_12; // 12
            public uint m_16; // 16
            public uint m_20; // 20
            public int m_packageCount; // 24
            public int m_packageBlockSize; // 28
            public int m_skinCount; // 32
            public int m_skinBlockSize; // 36
            public int m_typeBundleIndexCount; // 40
            public int m_typeBundleIndexBlockSize; // 44
            public uint m_48; // 48
            public uint m_52; // 52
            public int m_graphBlockSize; // 56
            public uint m_footerMagic; // 60

            public TRGHeader Upgrade() => new TRGHeader {
                m_0 = m_0,
                m_buildVersion = m_buildVersion,
                m_8 = m_8,
                m_12 = m_12,
                m_16 = m_16,
                m_20 = m_20,
                m_packageCount = m_packageCount,
                m_packageBlockSize = m_packageBlockSize,
                m_skinCount = m_skinCount,
                m_skinBlockSize = m_skinBlockSize,
                m_typeBundleIndexCount = m_typeBundleIndexCount,
                m_typeBundleIndexBlockSize = m_typeBundleIndexBlockSize,
                m_48 = m_48,
                m_52 = m_52,
                m_graphBlockSize = m_graphBlockSize,
                m_footerMagic = m_footerMagic
            };
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct TRGHeader { // version 11
            public uint m_0; // 0
            public uint m_buildVersion; // 4
            public uint m_8; // 8
            public uint m_12; // 12
            public uint m_16; // 16
            public uint m_20; // 20
            public int m_packageCount; // 24
            public int m_packageBlockSize; // 28
            public int m_skinCount; // 32
            public int m_skinBlockSize; // 36
            public int m_typeBundleIndexCount; // 40
            public int m_typeBundleIndexBlockSize; // 44
            public uint m_48; // 48
            public uint m_52; // 52
            public uint m_56; // 56 - new
            public uint m_60; // 60 - new
            public int m_graphBlockSize; // 64
            public uint m_footerMagic; // 68

            public const uint UNENCRYPTED_MAGIC = 0x747267;
            public const uint ENCRYPTED_MAGIC = 0x677274;

            public uint GetNonEncryptedMagic() {
                if (!IsEncrypted()) return m_footerMagic;
                return BinaryPrimitives.ReverseEndianness(m_footerMagic);
            }

            public byte GetVersion() {
                var shift = 24;
                if (Is217(this)) shift = 25; // todo: why did this happen
                
                var magic = GetNonEncryptedMagic();
                var version = magic >> shift;
                
                // if we see version lower than 11 with s17 shift, build check is wrong
                Debug.Assert(shift < 25 || version >= 11);
                return (byte)version;
            }

            public bool IsEncrypted() {
                return (m_footerMagic >> 8) == ENCRYPTED_MAGIC;
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 29, Pack = 1)]
        public struct Package5 { // before 9
            public ulong m_assetGUID;
            public ulong m_resourceKeyID;
            public uint m_16;
            public uint m_20;
            public uint m_24;
            public byte m_28;

            public Package Upgrade() => new Package() {
                m_assetGUID = m_assetGUID,
                m_resourceKeyID = m_resourceKeyID,
                m_16 = m_16,
                m_20 = m_20,
                m_24 = m_24,
                m_28 = m_28
            };
        }

        [StructLayout(LayoutKind.Sequential, Size = 30, Pack = 1)]
        public struct Package { // 9+
            public ulong m_assetGUID;
            public ulong m_resourceKeyID;
            public uint m_16;
            public uint m_20;
            public uint m_24;
            public byte m_28;
            public byte m_29;
        }

        public struct SkinHeader {
            public long m_assetPtr;
            public ulong m_skinGUID;
            public uint m_16;
            public uint m_20;
            public uint m_24;
            public uint m_28;
            public uint m_32;
            public ushort m_assetCount;
            public ushort m_38;
        }

        public record Skin(SkinHeader m_header, SkinAsset8B[] m_assets);

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct Zach {
            [FieldOffset(0)] public ulong m_assetGUID;
            [FieldOffset(0)] public uint m_ref;

            [FieldOffset(8)] public byte m_8;
            [FieldOffset(9)] public byte m_9;
        }

        public struct SkinAsset5 {
            public ulong m_assetGUID;
            public uint m_wtf1;
            public uint m_wtf2;

            public SkinAsset8B Upgrade() => new SkinAsset8B {
                m_srcAsset = m_assetGUID,
                m_wtf1 = m_wtf1,
                m_wtf2 = m_wtf2
            };
        }

        public struct SkinAsset6 {
            public ulong m_srcAsset;
            public ulong m_destAsset;
            public uint m_wtf1;
            public uint m_wtf2;

            public SkinAsset8B Upgrade() => new SkinAsset8B {
                m_srcAsset = m_srcAsset,
                m_destAsset = m_destAsset,
                m_wtf1 = m_wtf1,
                m_wtf2 = m_wtf2
            };
        }

        public struct SkinAsset8B {
            public ulong m_srcAsset;
            public ulong m_destAsset;
            public ulong m_addedWithoutBumpingVersionNumberWhy;
            public uint m_wtf1;
            public uint m_wtf2;
        }

        public string m_name;
        public TRGHeader m_header;
        public Dictionary<ulong, Package> m_packages;
        public Dictionary<ulong, Skin> m_skins;
        public byte[] m_graphBlock;
        public byte[]? m_typeBundleIndexBlock;

        public static bool IsPre152(TRGHeader header) {
            return header.m_buildVersion < ProductHandler_Tank.VERSION_152_PTR || header.m_buildVersion == 72604; // 72604 = 1.51 on proc2
        }

        public static bool IsPre212(TRGHeader header) {
            return header.m_buildVersion < 128702; // 128702 = 2.12 on pro
        }
        
        public static bool Is217(TRGHeader header) {
            return header.m_buildVersion >= 139475 && // 139475 = 2.17 on pro
                   header.m_buildVersion < 141395; // 141395 = 2.18 on pro
        }

        public ResourceGraph(ClientHandler client, Stream stream, string name) {
            m_name = name;
            using (BinaryReader reader = new BinaryReader(stream)) {
                m_header = reader.Read<TRGHeader>();
                if (IsPre152(m_header)) {
                    stream.Position = 0;
                    m_header = reader.Read<TRGHeader6>().Upgrade();
                } else if (IsPre212(m_header)) {
                    stream.Position = 0;
                    m_header = reader.Read<TRGHeader7>().Upgrade();
                }

                var version = m_header.GetVersion();
                if (version is < 5 or > 12) {
                    throw new UnsupportedBuildVersionException($"unable to parse TRG. invalid version {version}, expected 5, 6, 7, 8, 9, 10, 11 or 12");
                }

                // version 7: type bundle index added
                // version 10: added extra entries to skin assets.. for trg runtime overrides (instead of on the skin asset)
                // version 11: 2 new header fields, unknown
                // s17: no version change but 1 bit was stolen from magic (shift for version number changed)...
                // version 12(s18): shift change undone, graph changed to raise size limit

                var isEnc = m_header.IsEncrypted();

                if (!isEnc) {
                    ParseBlocks(reader, name);
                } else {
                    using (var decryptedReader = ManifestCryptoHandler.GetDecryptedReader(name, "TRG", m_header, m_header.m_buildVersion, client.Product, stream))
                        ParseBlocks(decryptedReader, name);
                }
            }

            if (m_packages == null) throw new NullReferenceException(nameof(m_packages));
            if (m_skins == null) throw new NullReferenceException(nameof(m_skins));
            if (m_graphBlock == null) throw new NullReferenceException(nameof(m_graphBlock));
        }

        private void ParseBlocks(BinaryReader reader, string name) {
            var version = m_header.GetVersion();

            byte[] packageBlock = reader.ReadBytes(m_header.m_packageBlockSize);
            byte[] skinBlock = reader.ReadBytes(m_header.m_skinBlockSize);
            m_graphBlock = reader.ReadBytes(m_header.m_graphBlockSize);
            if (version >= 7) {
                m_typeBundleIndexBlock = reader.ReadBytes(m_header.m_typeBundleIndexBlockSize);
                //File.WriteAllBytes(name + "_typeBundleIndex", m_typeBundleIndexBlock);
            }

            // todo: don't waste time and memory by loading into byte arrays

            //File.WriteAllBytes(name + "_package", packageBlockTest);
            //File.WriteAllBytes(name + "_graph", graphBlockTest);

            Package[] packages;
            using (var packageStream = new MemoryStream(packageBlock))
            using (var packageReader = new BinaryReader(packageStream)) {
                if (version >= 9) {
                    packages = packageReader.ReadArray<Package>(m_header.m_packageCount);
                } else {
                    packages = packageReader.ReadArray<Package5>(m_header.m_packageCount).Select(x => x.Upgrade()).ToArray();
                }
            }

            m_packages = new Dictionary<ulong, Package>();
            foreach (var package in packages) {
                m_packages[package.m_assetGUID] = package;
            }

            var ver8ButWithExtraDataWhoDidThis = version == 8 && m_header.m_buildVersion >= 105760;
            // ow2: added random crap without bumping version. thanks blizz

            SkinHeader[] skins = new SkinHeader[m_header.m_skinCount];
            m_skins = new Dictionary<ulong, Skin>();
            using (var skinStream = new MemoryStream(skinBlock))
            using (var skinReader = new BinaryReader(skinStream)) {
                for (var i = 0; i < m_header.m_skinCount; i++) {
                    var skinStart = skinStream.Position;

                    var skinHeader = skinReader.Read<SkinHeader>();
                    skins[i] = skinHeader;

                    if (skinHeader.m_assetPtr == 0) continue;
                    skinStream.Position = skinStart + skinHeader.m_assetPtr;

                    SkinAsset8B[] assets;
                    if (version > 8 || ver8ButWithExtraDataWhoDidThis) {
                        assets = skinReader.ReadArray<SkinAsset8B>(skinHeader.m_assetCount);
                    } else if (version >= 6) {
                        assets = skinReader.ReadArray<SkinAsset6>(skinHeader.m_assetCount).Select(x => x.Upgrade()).ToArray();
                    } else {
                        assets = skinReader.ReadArray<SkinAsset5>(skinHeader.m_assetCount).Select(x => x.Upgrade()).ToArray();
                    }

                    m_skins[skinHeader.m_skinGUID] = new Skin(skinHeader, assets);
                }
            }
        }
    }
}