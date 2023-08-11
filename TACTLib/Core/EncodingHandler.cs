using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TACTLib.Client;
using TACTLib.Container;
using TACTLib.Helpers;

namespace TACTLib.Core {
    public class EncodingHandler {
        /// <summary>Encoding table</summary>
        public readonly Dictionary<CKey, CKeyEKeyEntry> Entries;
        
        private readonly CKey[] EKeyESpecHeaderKeys;
        private byte[][] EKeyESpecPages;

        public EncodingHandler(ClientHandler client) : this(client,
            client.ConfigHandler.BuildConfig.Encoding.EncodingKey, client.ConfigHandler.BuildConfig.m_encodingSize!.m_encodedSize)
        {
        }

        public unsafe EncodingHandler(ClientHandler client, CKey key, int eSize) {
            Entries = new Dictionary<CKey, CKeyEKeyEntry>(CASCKeyComparer.Instance);

            using (var stream = client.CreateArgs.VersionSource > ClientCreateArgs.InstallMode.Local ? client.OpenCKey(key)! : client.OpenEKey(key, eSize)!)
            using (var reader = new BinaryReader(stream)) {
                /*using (var outFile = File.OpenWrite("steam_encoding.bin")) {
                    stream.CopyTo(outFile);
                }
                stream.Position = 0;
                
                File.WriteAllBytes("steam_encoding_encoded.bin", client.ContainerHandler!.OpenEKey(key, eSize)!.Value.ToArray());*/
                
                var header = reader.Read<Header>();

                if (header.Signature != 0x4E45 || header.CKeySize != 16 || header.EKeySize != 16 ||
                    header.Version != 1) {
                    throw new InvalidDataException($"EncodingHandler: encoding header invalid (magic: {header.Signature:X4}, csize: {header.CKeySize}, esize: {header.EKeySize})");
                }

                var cKeyEKeyPageSize = header.m_ckeyEKeyPageSize.ToInt();  
                var eKeyESpecPageSize = header.m_eKeyESpecPageSize.ToInt(); 

                var cKeyEKeyPageCount = header.m_cKeyEKeyPageCount.ToInt();
                var eKeyEspecPageCount = header.m_eKeyEspecPageCount.ToInt();
                
                Debug.Assert(header.m_unknown == 0); // asserted by agent

                //string[] strings = Encoding.ASCII.GetString(reader.ReadBytes(especBlockSize)).Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                stream.Position += header.m_especBlockSize.ToInt();

                //PageHeader[] pageHeaders = reader.ReadArray<PageHeader>(cKeyPageCount);
                stream.Position += cKeyEKeyPageCount * sizeof(PageHeader);
                for (var i = 0; i < cKeyEKeyPageCount; i++) {
                    var pageEnd = stream.Position + cKeyEKeyPageSize * 1024;

                    while (stream.Position <= pageEnd) {
                        var entry = reader.Read<CKeyEKeyEntry>();
                        if (entry.EKeyCount == 0) break;

                        stream.Position += (entry.EKeyCount - 1) * header.EKeySize;

                        if (Entries.ContainsKey(entry.CKey)) {
                            //Console.Out.WriteLine($"double or nothing {entry.CKey.ToHexString()}");
                            continue;
                        }

                        Entries[entry.CKey] = entry;
                    }

                    stream.Position = pageEnd; // just checking
                }
                
                var EKeyESpecHeaders = reader.ReadArray<PageHeader>((int)eKeyEspecPageCount);
                EKeyESpecHeaderKeys = EKeyESpecHeaders.Select(x => x.FirstKey).ToArray();
                EKeyESpecPages = new byte[eKeyEspecPageCount][];
                for (var i = 0; i < eKeyEspecPageCount; i++) {
                    var page = new byte[eKeyESpecPageSize * 1024];
                    stream.DefinitelyRead(page);
                    EKeyESpecPages[i] = page;
                }
            }
        }

        public bool TryGetEncodingEntry(CKey cKey, out CKeyEKeyEntry entry) {
            return Entries.TryGetValue(cKey, out entry);
        }
        
        public int GetEncodedSize(CKey ekey) {
            var searchResult = Array.BinarySearch(EKeyESpecHeaderKeys, ekey, CKeyOrderComparer.Instance);

            int pageIndex;
            if (searchResult > 0) {
                pageIndex = searchResult;
            } else {
                var firstElementLarger = ~searchResult;
                pageIndex = firstElementLarger - 1;
            }
            
            var entries = MemoryMarshal.Cast<byte, EKeyESpecEntry>(EKeyESpecPages[pageIndex]);
            // todo: is another binary search worth..?
            for (int j = 0; j < entries.Length; j++) {
                var comparison = CKeyOrderComparer.CKeyCompare(ekey, entries[j].EKey);
                if (comparison == 0) return (int)entries[j].FileSize.ToInt();
                if (comparison > 0) break;
            }
            
            Console.Out.WriteLine($"cant get size for {ekey.ToHexString()}. a o");
            return 0;
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

            public UInt16BE m_ckeyEKeyPageSize;  // in kilo bytes. e.g. 4 in here → 4096 byte pages (default)
            public UInt16BE m_eKeyESpecPageSize; // same

            public UInt32BE m_cKeyEKeyPageCount;
            public UInt32BE m_eKeyEspecPageCount;

            public byte m_unknown;

            public UInt32BE m_especBlockSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct PageHeader {
            /// <summary>First key in the page</summary>
            public CKey FirstKey;

            /// <summary>MD5 of the page</summary>
            public CKey PageHash;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct EKeyESpecEntry {
            public CKey EKey;
            public UInt32BE ESpecIndex;
            public UInt40BE FileSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CKeyEKeyEntry {
            /// <summary>Number of EKeys</summary>
            public ushort EKeyCount;

            /// <summary>Content file size (big endian)</summary>
            public UInt32BE ContentSize;

            /// <summary>Content Key. MD5 of the file content.</summary>
            public CKey CKey;  // Content key. This is MD5 of the file content

            /// <summary>Encoding Key. This is (trimmed) MD5 hash of the file header, containing MD5 hashes of all the logical blocks of the file</summary>
            public CKey EKey; // :kyaah:

            /// <summary>Get content size</summary>
            /// <returns>Content size</returns>
            public int GetSize() {
                return (int)ContentSize.ToInt();
            }
        }
    }
}