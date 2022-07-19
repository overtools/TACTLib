using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using TACTLib.Client;
using TACTLib.Container;
using TACTLib.Helpers;
using static TACTLib.Utils;

namespace TACTLib.Core {
    public class EncodingHandler {
        /// <summary>Encoding table</summary>
        public readonly Dictionary<CKey, CKeyEntry> Entries;

        public unsafe EncodingHandler(ClientHandler client) {
            Entries = new Dictionary<CKey, CKeyEntry>(CASCKeyComparer.Instance);

            var key = client.ConfigHandler.BuildConfig.Encoding.EncodingKey;

            using (var stream = client.CreateArgs.VersionSource > ClientCreateArgs.InstallMode.Local ? client.OpenCKey(key)! : client.OpenEKey(key.AsEKey())!)
            using (var reader = new BinaryReader(stream)) {
                var header = reader.Read<Header>();

                if (header.Signature != 0x4E45 || header.CKeySize != 16 || header.EKeySize != 16 ||
                    header.Version != 1) {
                    throw new InvalidDataException($"EncodingHandler: encoding header invalid (magic: {header.Signature:X4}, csize: {header.CKeySize}, esize: {header.EKeySize})");
                }

                var cKeyPageSize = reader.ReadUInt16BE();  // in kilo bytes. e.g. 4 in here → 4096 byte pages (default)
                var especPageSize = reader.ReadUInt16BE(); // same

                var cKeyPageCount = reader.ReadInt32BE();
                var especPageCount = reader.ReadInt32BE();

                var unknown = reader.ReadByte();
                Debug.Assert(unknown == 0); // asserted by agent

                var especBlockSize = reader.ReadInt32BE();

                //string[] strings = Encoding.ASCII.GetString(reader.ReadBytes(especBlockSize)).Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                stream.Position += especBlockSize;

                //PageHeader[] pageHeaders = reader.ReadArray<PageHeader>(cKeyPageCount);
                stream.Position += cKeyPageCount * sizeof(PageHeader);
                for (var i = 0; i < cKeyPageCount; i++) {
                    var pageEnd = stream.Position + cKeyPageSize * 1024;

                    while (stream.Position <= pageEnd) {
                        var entry = reader.Read<CKeyEntry>();
                        if (entry.EKeyCount == 0) break;

                        stream.Position += (entry.EKeyCount - 1) * header.EKeySize;

                        if (Entries.ContainsKey(entry.CKey)) continue;

                        Entries[entry.CKey] = entry;
                    }

                    stream.Position = pageEnd; // just checking
                }
            }
        }

        public bool TryGetEncodingEntry(CKey cKey, out CKeyEntry entry) {
            return Entries.TryGetValue(cKey, out entry);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header {
            /// <summary>Encoding signature, "EN"</summary>
            public short Signature;

            /// <summary>Version number</summary>
            public byte Version;

            /// <summary>Number of bytes in a CKey</summary>
            public byte CKeySize;

            /// <summary>Number of bytes in a EKey</summary>
            public byte EKeySize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct PageHeader {
            /// <summary>First key in the page</summary>
            public CKey FirstKey;

            /// <summary>MD5 of the page</summary>
            public CKey PageHash;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct CKeyEntry {
            /// <summary>Number of EKeys</summary>
            public ushort EKeyCount;

            /// <summary>Content file size (big endian)</summary>
            public fixed byte ContentSize[4];

            /// <summary>Content Key. MD5 of the file content.</summary>
            public CKey CKey;  // Content key. This is MD5 of the file content

            /// <summary>Encoding Key. This is (trimmed) MD5 hash of the file header, containing MD5 hashes of all the logical blocks of the file</summary>
            public CKey EKey; // :kyaah:

        }
    }
}