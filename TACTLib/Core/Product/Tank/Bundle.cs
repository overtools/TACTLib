using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Tank {
    public class Bundle {
        public readonly HeaderData148 Header;
        public readonly Entry4[] Entries;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HeaderData {
            public int EntryCount;
            public int Flags;
            public byte OffsetSize;

            public HeaderData148 To148() => new HeaderData148 {
                EntryCount = EntryCount,
                OffsetSize = OffsetSize
            };
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HeaderData148 {
            public int EntryCount;
            public byte OffsetSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 9)]
        public struct Entry1 {
            public ulong GUID;
            public byte Offset; // 1
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 10)]
        public struct Entry2 {
            public ulong GUID;
            public ushort Offset; // 2
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 12)]
        public struct Entry4 {
            public ulong GUID;
            public uint Offset; // 4
        }

        public Bundle(Stream? stream, bool is148) {
            if (stream == null) {
                Entries = Array.Empty<Entry4>();
                return;
            }

            using (BinaryReader reader = new BinaryReader(stream)) {
                if (is148) {
                    Header = reader.Read<HeaderData148>();
                } else {
                    Header = reader.Read<HeaderData>().To148();
                }

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
                    throw new Exception($"unknown bundle offset size {Header.OffsetSize}. contact the devs");
                    // this should never happen
                }
            }
        }
    }
}