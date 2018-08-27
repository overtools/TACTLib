using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TACTLib.Client;
using TACTLib.Container;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Tank {
    public class ApplicationPackageManifest {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct APMHeader {
            public ulong Build;
            public ulong Unknown1;
            public uint Unknown2;
            public int PackageCount;
            public uint Unknown3;
            public int EntryCount;
            public uint Checksum;
        }
            
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct Entry {
            public uint Index;
            public ulong HashA;
            public ulong HashB;
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct PackageEntry {
            public ulong PackageGUID; // 077 file
            public ulong Unknown1;
            public uint Unknown2;
            public uint Unknown3;
            public uint Unknown4;
                
            public uint Unknown5;
        }
        
        public enum PackageCompressionMethod : uint {
            Uncompressed = 0,
            Gzip = 1
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct Package {
            public long OffsetRecords;
            public long OffsetSiblings;
            public long OffsetUnknown;
            public long OffsetSiblings2;
            public ulong Unknown1;
            public uint Unknown2;
            public uint RecordCount;
            public PackageCompressionMethod CompressionMethod;
            public ulong SizeRecords;
            public uint SiblingCount;
            public uint Checksum;
            public uint Unknown3;
            public ulong BundleGUID; // 09C file
            public ulong Unknown4;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]  // size = 16
        public struct PackageRecord {
            public ulong GUID;
            public ContentFlags Flags;
            public uint BundleOffset;
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct CachePackageRecord {
            public int CMFIndex;
            public ContentFlags Flags;
            public uint BundleOffset;
        }

        public APMHeader Header;
        public Entry[] Entries;
        public PackageEntry[] PackageEntries;
        public Package[] Packages;
        public PackageRecord[][] Records;
        public ulong[][] PackageSiblings;
        
        public ApplicationPackageManifest(ClientHandler client, Stream stream, ContentManifestFile cmf) {
            using (BinaryReader reader = new BinaryReader(stream)) {
                Header = reader.Read<APMHeader>();
                
                Entries = reader.ReadArray<Entry>(Header.EntryCount);
                PackageEntries = reader.ReadArray<PackageEntry>(Header.PackageCount);
                VerifyEntries(cmf);
                
                // todo load cache here
                if (false) {
                    return;
                }
                
                Packages = new Package[Header.PackageCount];
                Records = new PackageRecord[Header.PackageCount][];
                
                //using (PerfCounter _ = new PerfCounter("APM:LoadPackage(packageCount)"))
                //for (int i = 0; i < Header.PackageCount; i++) {
                //    LoadPackage(i, client, cmf);
                //}

                using (PerfCounter _ = new PerfCounter("APM:LoadPackage(packageCount)"))
                Parallel.For(0, Header.PackageCount, new ParallelOptions {
                    MaxDegreeOfParallelism = 4
                }, i => {
                    LoadPackage(i, client, cmf);
                });

                
                // todo save cache here
                if (false) {
                    return;
                }
            }
        }

        private void LoadPackage(int i, ClientHandler client, ContentManifestFile cmf) {
            PackageEntry entry = PackageEntries[i];
            if (!cmf.TryGet(entry.PackageGUID, out var packageCMF)) return; // lol?
            if (!client.EncodingHandler.TryGetEncodingEntry(packageCMF.ContentKey, out var packageEncoding)) return;
                    
            using (Stream packageStream = client.OpenEKey(packageEncoding.EKey))
            using (BinaryReader packageReader = new BinaryReader(packageStream)) {
                Packages[i] = packageReader.Read<Package>();
                        
                packageStream.Position = Packages[i].OffsetRecords;
                using (GZipStream decompressedStream = new GZipStream(packageStream, CompressionMode.Decompress))
                using (BinaryReader decompressedReader = new BinaryReader(decompressedStream)) {
                    Records[i] = decompressedReader.ReadArray<PackageRecord>((int)Packages[i].RecordCount);
                }
            }
        }

        private void VerifyEntries(ContentManifestFile cmf) {
            for (int i = 0; i < Header.EntryCount; i++) {
                Entry a = Entries[i];
                Entry b = cmf.Entries[i];

                if (a.HashA != b.HashA) {
                    throw new InvalidDataException();
                }
                // todo: HashB is always 0 in APM. what does this mean?
            }
        }
    }
}