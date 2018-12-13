using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Tank {
    public class Bundle {  // note to self: uses "BINARY PACAKGE DATA" handler, but so do movies, so that's not the type
        public readonly HeaderData Header;
        public readonly Entry4[] Entries;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HeaderData {
            public int EntryCount;
            public int Flags;
            public byte OffsetSize;
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

        public Bundle(Stream stream) {
            using (BinaryReader reader = new BinaryReader(stream)) {
                Header = reader.Read<HeaderData>();
                
                // todo: maybe don't use Select then convert. bit of a hack
                if (Header.OffsetSize == 1)
                    Entries = reader.ReadArray<Entry1>(Header.EntryCount).Select(x => new Entry4 {
                        GUID = x.GUID,
                        Offset = x.Offset
                    }).ToArray();
                else if (Header.OffsetSize == 2) {
                    Entries = reader.ReadArray<Entry2>(Header.EntryCount).Select(x => new Entry4 {
                        GUID = x.GUID,
                        Offset = x.Offset
                    }).ToArray();
                } else if (Header.OffsetSize == 4) {
                    Entries = reader.ReadArray<Entry4>(Header.EntryCount);
                } else {
                    throw new Exception("unknown bundle offset size. contact the devs");
                    // this should never happen
                }
            }
        } 
    }
}
