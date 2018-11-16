using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Tank {
    public class BinaryPackageData {  // "bundle"
        public readonly HeaderData Header;
        public readonly Entry4[] Entries;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HeaderData {
            public int EntryCount;
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 2, Size = 9)]
        public struct Entry1 {
            public ulong GUID;
            public byte Offset;  // 1
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2, Size = 10)]
        public struct Entry2 {
            public ulong GUID;
            public ushort Offset;  // 2
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 2, Size = 12)]
        public struct Entry4 {
            public ulong GUID;
            public uint Offset;  // 4
        }

        public BinaryPackageData(Stream stream) {
            using (BinaryReader reader = new BinaryReader(stream)) {
                Header = reader.Read<HeaderData>();
                
                // note: sizeof(header) = 9
                stream.Position = 9;
                
                // offset size is adaptive depending on bundle size
                // todo: maybe don't use Select then convert. bit of a hack
                if (stream.Length <= byte.MaxValue)
                    Entries = reader.ReadArray<Entry1>(Header.EntryCount).Select(x => new Entry4 {
                        GUID = x.GUID,
                        Offset = x.Offset
                    }).ToArray();
                else if (stream.Length <= ushort.MaxValue) {
                    Entries = reader.ReadArray<Entry2>(Header.EntryCount).Select(x => new Entry4 {
                        GUID = x.GUID,
                        Offset = x.Offset
                    }).ToArray();
                } else {
                    Entries = reader.ReadArray<Entry4>(Header.EntryCount);
                }
            }
        } 
    }
}
