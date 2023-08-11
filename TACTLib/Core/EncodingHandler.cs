using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TACTLib.Client;
using TACTLib.Core.Key;
using TACTLib.Helpers;

namespace TACTLib.Core {
    public class EncodingHandler {
        private readonly CKey[] CKeyEKeyHeaderKeys;
        private readonly CKeyEKeyEntry[][] CKeyEKeyPages;

        private readonly FullEKey[] EKeyESpecHeaderKeys;
        private readonly EKeyESpecEntry[][] EKeyESpecPages;

        public EncodingHandler(ClientHandler client) : this(client,
            client.ConfigHandler.BuildConfig.Encoding.EncodingKey, client.ConfigHandler.BuildConfig.EncodingSize!.EncodedSize)
        {
        }

        public EncodingHandler(ClientHandler client, FullKey eKey, int eSize) {
            using var stream = client.OpenEKey(eKey, eSize)!;
            using var reader = new BinaryReader(stream);
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

            var CKeyEKeyHeaders = reader.ReadArray<PageHeader>((int)cKeyEKeyPageCount);
            CKeyEKeyHeaderKeys = CKeyEKeyHeaders.Select(x => x.FirstKey).ToArray();
            CKeyEKeyPages = new CKeyEKeyEntry[CKeyEKeyHeaders.Length][];
            for (var i = 0; i < CKeyEKeyHeaders.Length; i++) {
                var page = new byte[cKeyEKeyPageSize * 1024];
                stream.DefinitelyRead(page);

                int structSize;
                unsafe {
                    structSize = sizeof(CKeyEKeyEntry);
                }
                var newPage = new CKeyEKeyEntry[page.Length / structSize];
                // for quick searching, we have to stop the page from being dynamic size
                // only 1 ekey allowed

                var arrOffset = 0;
                var insertIndex = 0;
                while (true) {
                    var foundEntrySpan = page.AsSpan(arrOffset);
                    if (foundEntrySpan.Length < structSize) break;
                    var foundEntry = MemoryMarshal.Read<CKeyEKeyEntry>(foundEntrySpan);
                    if (foundEntry.EKeyCount == 0) break; // end
                    arrOffset += structSize;
                    arrOffset += (foundEntry.EKeyCount - 1) * 16;

                    newPage[insertIndex++] = foundEntry;
                }

                // BinarySearch breaks on the empty elements.. does that also happen for EKeyESpec?
                CKeyEKeyPages[i] = newPage.Take(insertIndex).ToArray();
            }

            var EKeyESpecHeaders = reader.ReadArray<PageHeader>((int)eKeyEspecPageCount);
            EKeyESpecHeaderKeys = EKeyESpecHeaders.Select(x => x.FirstKey).ToArray();
            EKeyESpecPages = new EKeyESpecEntry[EKeyESpecHeaders.Length][];
            for (var i = 0; i < EKeyESpecHeaders.Length; i++) {
                var page = new byte[eKeyESpecPageSize * 1024];
                stream.DefinitelyRead(page);
                
                var entries = MemoryMarshal.Cast<byte, EKeyESpecEntry>(page);
                for (var j = 0; j < entries.Length; j++)
                {
                    if (entries[j].ESpecIndex.ToInt() != uint.MaxValue) continue;
                    // FFFFF.. = terminator
                    entries = entries.Slice(0, j);
                    break;
                }
                EKeyESpecPages[i] = entries.ToArray();
            }
        }

        public bool TryGetEncodingEntry(CKey cKey, out CKeyEKeyEntry entry) {
            var searchResult = Array.BinarySearch(CKeyEKeyHeaderKeys, cKey, FullKeyOrderComparer.Instance);

            int pageIndex;
            if (searchResult > 0) {
                pageIndex = searchResult;
            } else {
                var firstElementLarger = ~searchResult;
                pageIndex = firstElementLarger - 1;
            }

            var speculativeEntry = new CKeyEKeyEntry {
                CKey = cKey
            };
            var entries = CKeyEKeyPages[pageIndex];
            var foundIndex = Array.BinarySearch(entries, speculativeEntry, FullKeyOrderComparer.Instance);

            if (foundIndex >= 0) {
                entry = entries[foundIndex];
                return true;
            }

            entry = new CKeyEKeyEntry();
            Console.Out.WriteLine($"cant get ekey for {cKey.ToHexString()}. a o");
            return false;
        }

        public int GetEncodedSize(FullEKey ekey) {
            var searchResult = Array.BinarySearch(EKeyESpecHeaderKeys, ekey, FullKeyOrderComparer.Instance);

            int pageIndex;
            if (searchResult >= 0) {
                pageIndex = searchResult;
            } else {
                var firstElementLarger = ~searchResult;
                pageIndex = firstElementLarger - 1;
            }

            var speculativeEntry = new EKeyESpecEntry {
                EKey = ekey
            };
            var entries = EKeyESpecPages[pageIndex];
            var foundIndex = Array.BinarySearch(entries, speculativeEntry, FullKeyOrderComparer.Instance);

            if (foundIndex >= 0) {
                return (int)entries[foundIndex].FileSize.ToInt();
            }

            Console.Out.WriteLine($"cant get size for {ekey.ToHexString()}. a o");
            return 0;
        }

        public IEnumerable<CKey> GetCKeys() {
            foreach (var page in CKeyEKeyPages) {
                foreach (var entry in page) {
                    yield return entry.CKey;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header {
            public ushort Signature; // Encoding signature, "EN"
            public byte Version; // Version number
            public byte CKeySize; // Number of bytes in a CKey
            public byte EKeySize; // Number of bytes in a EKey

            public UInt16BE m_ckeyEKeyPageSize;  // in kilo bytes. e.g. 4 in here → 4096 byte pages (default)
            public UInt16BE m_eKeyESpecPageSize; // same

            public UInt32BE m_cKeyEKeyPageCount;
            public UInt32BE m_eKeyEspecPageCount;

            public byte m_unknown;

            public UInt32BE m_especBlockSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct PageHeader {
            public FullKey FirstKey; // First key in the page
            public MD5Key PageHash; // MD5 of the page
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct EKeyESpecEntry {
            public FullEKey EKey;
            public UInt32BE ESpecIndex;
            public UInt40BE FileSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CKeyEKeyEntry {
            public ushort EKeyCount; // number of EKeys
            public UInt32BE ContentSize; // decoded size
            public FullKey CKey; // decoded key
            public FullEKey EKey; // encoded key

            /// <summary>Get content size</summary>
            /// <returns>Content size</returns>
            public int GetSize() {
                return (int)ContentSize.ToInt();
            }
        }
    }
}