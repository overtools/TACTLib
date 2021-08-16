using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TACTLib.Client;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Tank {
    public class ApplicationPackageManifest {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct APMHeader {
            public ulong m_buildVersion;
            public uint m_unknown1;
            public uint m_unknown2;
            public uint m_unknown3;
            public uint m_unknown4;
            public int m_packageCount;
            public uint m_unknown5;
            public int m_entryCount;
            public uint m_checksum; // ?
        }
            
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct Entry {
            public uint m_index;
            public ulong m_hashA;
            public ulong m_hashB;
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct PackageEntry {
            public ulong m_packageGUID; // 077 file
            public ulong m_keyID;
            public uint m_unknown1;
            public uint m_unknown2;
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct PackageHeader {
            public long m_offsetRecords;  // 0
            public long m_offset8; // 8
            public long m_offset16; // 16
            public long m_offset24; // 24
            public long m_offsetBundles;  // 32
            public long m_offset40; // 40
            
            public uint m_recordCount;  // 48
            public uint m_count52; // 52
            public uint m_count56;  // 56
            public uint m_count60;  // 60
            public uint m_count64;  // 64
            public uint m_count68; // 68
            public uint m_bundleCount;  // 72
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]  // size = 12
        public struct PackageRecord {
            [FieldOffset(0)] public ulong m_GUID;
            [FieldOffset(8)] public uint m_flagsReal;
            
            [FieldOffset(8)] public short m_unknown1;
            [FieldOffset(10)] public byte m_unknown2;
            [FieldOffset(11)] public RecordFlags m_flags;
        }

        [Flags]
        public enum RecordFlags : byte {
            None = 0,
            Bundle = 0x40
        }

        public APMHeader m_header;
        public Entry[] m_entries;
        public PackageEntry[] m_packageEntries;
        public PackageHeader[] m_packages;
        public PackageRecord[][] m_packageRecords;
        public ulong[][] m_packageBundles;
        
        public ApplicationPackageManifest(ClientHandler client, ProductHandler_Tank tankHandler, Stream stream, string name) {
            var cmf = tankHandler.m_rootContentManifest;
            
            using (BinaryReader reader = new BinaryReader(stream)) {
                m_header = reader.Read<APMHeader>();

                if(m_header.m_buildVersion >= 12923648 || m_header.m_buildVersion < 52320) {
                    throw new NotSupportedException("Overwatch 1.29 or earlier is not supported");
                }
                
                m_entries = reader.ReadArray<Entry>(m_header.m_entryCount);
                m_packageEntries = reader.ReadArray<PackageEntry>(m_header.m_packageCount);

                if (!VerifyEntries(cmf)) {
                    Logger.Debug("APM", "Entry hash invalid. IV may be wrong");
                }
                
                m_packages = new PackageHeader[m_header.m_packageCount];
                m_packageRecords = new PackageRecord[m_header.m_packageCount][];
                m_packageBundles = new ulong[m_header.m_packageCount][];

                var c = 0;
                using (PerfCounter _ = new PerfCounter("APM:LoadPackages"))
                    Parallel.For(0, m_header.m_packageCount, new ParallelOptions {
                        MaxDegreeOfParallelism = 4
                    }, i => {
                        c++;
                        if (c % 1000 == 0) {
                            if (!Console.IsOutputRedirected) {
                                Console.Out.Write($"Loading packages: {Math.Floor(c / (float)m_header.m_packageCount * 10000) / 100:F0}% ({c}/{m_header.m_packageCount})\r");
                            }
                        }
                    
                        LoadPackage(i, client, cmf);
                    });
                
                if (!Console.IsOutputRedirected) {
                    Console.Write(new string(' ', Console.WindowWidth-1)+"\r");
                }
            }
        }

        private void LoadPackage(int i, ClientHandler client, ContentManifestFile cmf) {
            var entry = m_packageEntries[i];
            using (Stream packageStream = cmf.OpenFile(client, entry.m_packageGUID)!)
            using (BinaryReader packageReader = new BinaryReader(packageStream)) {
                var package = packageReader.Read<PackageHeader>();
                m_packages[i] = package;

                if (package.m_recordCount == 0) {
                    m_packageRecords[i] = new PackageRecord[0];
                    return;
                }
                
                if (package.m_bundleCount > 0) {
                    packageStream.Position = package.m_offsetBundles;
                    m_packageBundles[i] = packageReader.ReadArray<ulong>((int)package.m_bundleCount);
                }

                packageStream.Position = package.m_offsetRecords;
                using (GZipStream decompressedStream = new GZipStream(packageStream, CompressionMode.Decompress))
                using (BinaryReader decompressedReader = new BinaryReader(decompressedStream)) {
                    m_packageRecords[i] = decompressedReader.ReadArray<PackageRecord>((int) package.m_recordCount);
                }
            }
        }

        private bool VerifyEntries(ContentManifestFile cmf) {
            for (var i = 0; i < m_header.m_entryCount; i++) {
                var a = m_entries[i];
                var b = cmf.m_entries[i];

                if (a.m_hashA != b.m_hashA) {
                    return false;
                }
                // todo: HashB is always 0 in APM. what does this mean?
            }

            return true;
        }
    }
}