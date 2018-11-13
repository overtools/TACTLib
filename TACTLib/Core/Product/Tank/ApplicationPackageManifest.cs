using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using LZ4;
using TACTLib.Client;
using TACTLib.Client.HandlerArgs;
using TACTLib.Container;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Tank {
    public class ApplicationPackageManifest {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct APMHeader {
            public ulong Build;
            public uint Unknown1;
            public uint Unknown2;
            public uint Unknown3;
            public uint Unknown4;
            public int PackageCount;
            public uint Unknown5;
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
            public uint Unknown1;
            public uint Unknown2;
            public uint Unknown3;
            public uint Unknown4;
        }
        
        public enum PackageCompressionMethod : uint {
            Uncompressed = 0,
            Gzip = 1
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct Package {
            public long OffsetRecords;
            public long OffsetUnknown1;
            public long OffsetUnknown2; // streamed?
            public long OffsetUnknown3;
            public long OffsetUnknown4;
            
            public uint Unknown1;
            public uint Unknown2;
            public uint Unknown3;
            
            public uint RecordCount;
            public uint Unknown2Count;
            
            public uint CompressedSize;
            
            public uint Unknown1Count;
            public uint Unknown3Count;
            public uint Unknown4Count;
            
            public ulong BundleGUID;
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

            public CachePackageRecord(PackageRecord record, ContentManifestFile cmf) {
                Flags = record.Flags;
                BundleOffset = record.BundleOffset;
                CMFIndex = cmf.IndexMap[record.GUID];
            }
        }

        public APMHeader Header;
        public Entry[] Entries;
        public PackageEntry[] PackageEntries;
        public Package[] Packages;
        public PackageRecord[][] Records;
        
        public ApplicationPackageManifest(ClientHandler client, Stream stream, ContentManifestFile cmf, string name) {
            ClientCreateArgs_Tank args = client.CreateArgs.HandlerArgs as ClientCreateArgs_Tank ?? new ClientCreateArgs_Tank();
            
            using (BinaryReader reader = new BinaryReader(stream)) {
                Header = reader.Read<APMHeader>();

                if(Header.Build >= 12923648 || Header.Build < 52320) {
                    throw new NotSupportedException("Overwatch 1.29 or earlier is not supported");
                }
                
                Entries = reader.ReadArray<Entry>(Header.EntryCount);
                PackageEntries = reader.ReadArray<PackageEntry>(Header.PackageCount);

                if (!VerifyEntries(cmf)) {
                    Logger.Debug("APM", "Entry hash invalid. IV may be wrong");
                }
                
                Packages = new Package[Header.PackageCount];
                Records = new PackageRecord[Header.PackageCount][];
                
                if (args.CacheAPM) {
                    string path = GetCachePath(name);
                    if (File.Exists(path)) {
                        try {

                            if (LoadCache(cmf, path)) {
                                return;
                            }

                            // else: regen
                        } catch (Exception) {
                            Logger.Error("APM", $"Failed to load APM Cache for {name}");
                            File.Delete(path);
                        }
                    }
                }

                for (int i = 0; i < Header.PackageCount; i++) {
                    LoadPackage(i, client, cmf);
                }

                //int c = 0;
                //using (PerfCounter _ = new PerfCounter("APM:LoadPackages"))
                //Parallel.For(0, Header.PackageCount, new ParallelOptions {
                //    MaxDegreeOfParallelism = 4
                //}, i => {
                //    c++;
                //    if (c % 1000 == 0) {
                //        if (!Console.IsOutputRedirected) {
                //            Console.Out.Write($"Loading packages: {Math.Floor(c / (float)Header.PackageCount * 10000) / 100:F0}% ({c}/{Header.PackageCount})\r");
                //        }
                //    }
                //    
                //    LoadPackage(i, client, cmf);
                //});
                
                if (!Console.IsOutputRedirected) {
                    Console.Write(new string(' ', Console.WindowWidth-1)+"\r");
                }

                if (args.CacheAPM) {
                    Logger.Debug("APM", $"Saving cache for {name}");
                    using (PerfCounter _ = new PerfCounter("APM:SaveCache"))
                    SaveCache(cmf, name);
                }
            }
        }

        private void SaveCache(ContentManifestFile cmf, string name) {
            using (Stream file = File.OpenWrite(GetCachePath(name)))
            using (LZ4Stream lz4Stream = new LZ4Stream(file, LZ4StreamMode.Compress, LZ4StreamFlags.HighCompression))
            using (BinaryWriter writer = new BinaryWriter(lz4Stream)) {
                file.SetLength(0);
                writer.Write(123u);
                writer.WriteStructArray(Packages);
                
                for (int i = 0; i < Header.PackageCount; ++i) {
                    var package = Packages[i];
                    var records = Records[i];
                    CachePackageRecord[] cacheRecords = new CachePackageRecord[package.RecordCount];
                    for (int j = 0; j < package.RecordCount; j++) {
                        cacheRecords[j] = new CachePackageRecord(records[j], cmf);
                    }
                    writer.WriteStructArray(cacheRecords);
                }
            }
        }

        private string GetCachePath(string name) {
            string programDir = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
            string apmDir = Path.Combine(programDir, "CASCCache", "APM");

            if (!Directory.Exists(apmDir)) {
                Directory.CreateDirectory(apmDir);
            }
            
            return Path.Combine(apmDir, $"{Header.Build}_{Path.GetFileNameWithoutExtension(name)}.apmcached");
        }
        
        private bool LoadCache(ContentManifestFile cmf, string path) {
            using (Stream file = File.OpenRead(path))
            using (LZ4Stream lz4Stream = new LZ4Stream(file, LZ4StreamMode.Decompress))
            using (BinaryReader reader = new BinaryReader(lz4Stream)) {
                if(reader.ReadUInt32() != 123) {
                    throw new InvalidDataException("invalid magic");
                }

                Packages = reader.ReadArray<Package>(Header.PackageCount);

                for (int i = 0; i < Header.PackageCount; i++) {
                    var package = Packages[i];

                    CachePackageRecord[] cacheRecords = reader.ReadArray<CachePackageRecord>((int)package.RecordCount);
                    var records = new PackageRecord[package.RecordCount];
                    Records[i] = records;

                    for (int j = 0; j < package.RecordCount; j++) {
                        var cacheRecord = cacheRecords[j];
                        records[j] = new PackageRecord {
                            BundleOffset = cacheRecord.BundleOffset,
                            Flags = cacheRecord.Flags,
                            GUID = cmf.HashList[cacheRecord.CMFIndex].GUID
                        };
                    }
                }
            }

            return true;
        }

        private void LoadPackage(int i, ClientHandler client, ContentManifestFile cmf) {
            PackageEntry entry = PackageEntries[i];
            using (Stream packageStream = cmf.OpenFile(client, entry.PackageGUID))
            using (BinaryReader packageReader = new BinaryReader(packageStream)) {
                Packages[i] = packageReader.Read<Package>();
                var pkg = Packages[i];

                if (Packages[i].RecordCount == 0) {
                    Records[i] = new PackageRecord[0];
                    return;
                }
                
                packageStream.Position = Packages[i].OffsetRecords;
                using (GZipStream decompressedStream = new GZipStream(packageStream, CompressionMode.Decompress))
                using (BinaryReader decompressedReader = new BinaryReader(decompressedStream)) {
                    Records[i] = decompressedReader.ReadArray<PackageRecord>((int) Packages[i].RecordCount);
                }
            }
        }

        private bool VerifyEntries(ContentManifestFile cmf) {
            for (int i = 0; i < Header.EntryCount; i++) {
                Entry a = Entries[i];
                Entry b = cmf.Entries[i];

                if (a.HashA != b.HashA) {
                    return false;
                }
                // todo: HashB is always 0 in APM. what does this mean?
            }

            return true;
        }
    }
}