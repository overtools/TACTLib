using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TACTLib.Client;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Tank {
    public class ResourceGraph {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct TRGHeader { // version 5 and 6
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

            public const uint UNENCRYPTED_MAGIC = 0x747267;
            public const uint ENCRYPTED_MAGIC = 0x677274;
            
            public uint GetMagicNonEnc() => m_footerMagic & 0xFFFFFF00; 
            public uint GetMagicEnc() => m_footerMagic >> 8;

            public bool IsEncrypted()
            {
                return GetMagicEnc() == ENCRYPTED_MAGIC;
            }
            
            public byte GetVersion()
            {
                return IsEncrypted() ? (byte)(m_footerMagic & 0x000000FF) : (byte)((m_footerMagic & 0xFF000000) >> 24);
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 29, Pack = 1)]
        public struct Package {
            public ulong m_assetGUID;
            public ulong m_resourceKeyID;
            public uint m_16;
            public uint m_20;
            public uint m_24;
            public byte m_28;
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

        public class Skin {
            public SkinHeader m_header;
            public SkinAsset6[] m_assets;

            public Skin(SkinHeader skinHeader) {
                m_header = skinHeader;
            }
        }

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

            public SkinAsset6 Upgrade() => throw new NotImplementedException();
            //new SkinAsset6 {
            //    m_assetGUID = m_assetGUID
            //}; // please check
        }
        
        public struct SkinAsset6 {
            // todo: 
        }

        public TRGHeader m_header;
        public Dictionary<ulong, Package> m_packages;
        public Dictionary<ulong, Skin> m_skins;

        public ResourceGraph(ClientHandler client, Stream stream, string name) {
            using BinaryReader reader = new BinaryReader(stream);
            m_header = reader.Read<TRGHeader>();
                
            var version = m_header.GetVersion();
            if (version != 5) {
                throw new InvalidDataException($"unable to parse TRG. invalid version {version}, expected 5");
            }

            var isEnc = m_header.IsEncrypted();

            if (!isEnc) {
                ParseBlocks(reader, name);
            } else {
                using var decryptedReader = ManifestCryptoHandler.GetDecryptedReader(name, "TRG", m_header, m_header.m_buildVersion, client.Product, stream);
                ParseBlocks(decryptedReader, name);
            }
        }

        private void ParseBlocks(BinaryReader reader, string name) {
            var version = m_header.GetVersion();
            
            byte[] packageBlockTest = reader.ReadBytes(m_header.m_packageBlockSize);
            byte[] skinBlockTest = reader.ReadBytes(m_header.m_skinBlockSize);
            byte[] graphBlockTest = reader.ReadBytes(m_header.m_graphBlockSize);
            
            // todo: don't waste time and memory by loading into byte arrays

            //File.WriteAllBytes(name + "_package", packageBlockTest);
            //File.WriteAllBytes(name + "_graph", graphBlockTest);

            Package[] packages;
            using (var packageStream = new MemoryStream(packageBlockTest))
            using (var packageReader = new BinaryReader(packageStream)) {
                packages = packageReader.ReadArray<Package>(m_header.m_packageCount);
            }

            m_packages = new Dictionary<ulong, Package>();
            foreach (Package package in packages) {
                m_packages[package.m_assetGUID] = package;
            }

            SkinHeader[] skins = new SkinHeader[m_header.m_skinCount];
            m_skins = new Dictionary<ulong, Skin>();
            using (var skinStream = new MemoryStream(skinBlockTest))
            using (var skinReader = new BinaryReader(skinStream)) {
                for (int i = 0; i < m_header.m_skinCount; i++) {
                    long skinStart = skinStream.Position;

                    var skinHeader = skinReader.Read<SkinHeader>();
                    skins[i] = skinHeader;

                    var skin = new Skin(skinHeader);
                    m_skins[skinHeader.m_skinGUID] = skin;

                    if (skinHeader.m_assetPtr == 0) continue;
                    skinStream.Position = skinStart + skinHeader.m_assetPtr;

                    if (version == 5) {
                        skin.m_assets = skinReader.ReadArray<SkinAsset5>(skinHeader.m_assetCount).Select(x => x.Upgrade()).ToArray();
                    } else {
                        skin.m_assets = skinReader.ReadArray<SkinAsset6>(skinHeader.m_assetCount);
                    }

                    //Logger.Info("trg", $"{skin.m_skinGUID:X16}");
                    //foreach (SkinAsset asset in assets) {
                    //    Logger.Info("trg", $"    {asset.m_assetGUID:X16}");
                    //}
                }
            }

            /*//var rootPackage = packages.First(x => x.m_assetGUID == 0);
            var firstSkin = skins.First();

            using (var graphStream = new MemoryStream(graphBlockTest))
            using (var graphReader = new BinaryReader(graphStream)) {
                var st = GetZach(rootPackage.m_16, graphReader);
                //var st = GetZach(firstSkin.m_24, graphReader);

                bool doMore = ((st.m_8 >> 3) & 1) != 0;
                if (doMore) {
                    long morePtr = graphStream.Position;
                    if ((st.m_guid & 0xFFFF800000000000ul) == 0x4F10000000000000) { // (GuidToAssetRepo(0x8Fu) >> 4) & 0xFFF000000000000ul | 0x4000000000000000ul
                        morePtr += 8; // todo: what data is here
                    } else if ((st.m_9 & 1) != 0) {
                        var moreRead = graphReader.ReadUInt32(); // todo: guid array
                        morePtr += 8 * moreRead + 4;
                    }

                    while (true) {
                        graphStream.Position = morePtr;
                        var more = graphReader.ReadUInt32();
                        var moreZach = GetZach(more, graphReader);

                        if (((more >> 29) & 1) != 0) {
                            Console.WriteLine("end");
                            break;
                        }

                        morePtr += 4;
                    }
                }
            }*/
        }

        public static Zach GetZach(uint num, BinaryReader graphReader) {
            if ((num & 0x1FFFFFF) >= 0x1FFFFFF) {
                Debug.Assert(false, "trg: number too big?");
            }
            
            Zach zach;
            while (true) {
                if (((num >> 30) & 1) != 0) {
                    throw new NotImplementedException();
                } else {
                    graphReader.BaseStream.Position = num & 0x1FFFFFF;
                    zach = graphReader.Read<Zach>();
                }

                if (((zach.m_8 >> 4) & 1) == 0) break;
                num = zach.m_ref; // todo: is this right at all
            }
            return zach;
        }
    }
}
