using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TACTLib.Client;
using TACTLib.Client.HandlerArgs;
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
        
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct Package {
            public long OffsetRecords;
            public long OffsetUnknown1;
            public long OffsetUnknown2; // streamed?
            public long OffsetUnknown3;
            public long OffsetUnknown4;
            public long OffsetUnknown5;
            
            public uint RecordCount;
            public uint SiblingCount;
            
            public uint Unknown1;
            public uint Unknown2;
            public uint Unknown3;
            public uint Unknown4;
            
            public uint BundleCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]  // size = 12
        public struct PackageRecord {
            public ulong GUID;
            public short Unknown1;
            public byte Unknown2;
            public RecordFlags Flags;
        }

        [Flags]
        public enum RecordFlags : byte {
            None = 0,
            Bundle = 0x40
        }

        public APMHeader Header;
        public Entry[] Entries;
        public PackageEntry[] PackageEntries;
        public Package[] Packages;
        public PackageRecord[][] Records;

        public ulong[][] PackageBundles;
        
        public ApplicationPackageManifest(ClientHandler client, ProductHandler_Tank tankHandler, Stream stream, string name) {
            ClientCreateArgs_Tank args = client.CreateArgs.HandlerArgs as ClientCreateArgs_Tank ?? new ClientCreateArgs_Tank();

            var cmf = tankHandler.MainContentManifest;
            
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
                PackageBundles = new ulong[Header.PackageCount][];

                int c = 0;
                using (PerfCounter _ = new PerfCounter("APM:LoadPackages"))
                Parallel.For(0, Header.PackageCount, new ParallelOptions {
                    MaxDegreeOfParallelism = 4
                }, i => {
                    c++;
                    if (c % 1000 == 0) {
                        if (!Console.IsOutputRedirected) {
                            Console.Out.Write($"Loading packages: {Math.Floor(c / (float)Header.PackageCount * 10000) / 100:F0}% ({c}/{Header.PackageCount})\r");
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
            PackageEntry entry = PackageEntries[i];
            using (Stream packageStream = cmf.OpenFile(client, entry.PackageGUID))
            using (BinaryReader packageReader = new BinaryReader(packageStream)) {
                var package = packageReader.Read<Package>();
                Packages[i] = package;

                if (package.RecordCount == 0) {
                    Records[i] = new PackageRecord[0];
                    return;
                }
                
                if (package.BundleCount > 0) {
                    packageStream.Position = package.OffsetUnknown4;
                    PackageBundles[i] = packageReader.ReadArray<ulong>((int)package.BundleCount);
                }

                packageStream.Position = package.OffsetRecords;
                using (GZipStream decompressedStream = new GZipStream(packageStream, CompressionMode.Decompress))
                using (BinaryReader decompressedReader = new BinaryReader(decompressedStream)) {
                    Records[i] = decompressedReader.ReadArray<PackageRecord>((int) package.RecordCount);
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