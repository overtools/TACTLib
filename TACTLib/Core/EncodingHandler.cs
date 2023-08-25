﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TACTLib.Client;
using TACTLib.Core.Key;
using TACTLib.Helpers;

namespace TACTLib.Core {
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class EncodingHandler {
        private readonly CKey[] CKeyEKeyHeaderKeys;
        private readonly CKey[][] CKeyEKeyPagesA;
        private readonly FullEKey[][][] CKeyEKeyPagesB;

        private readonly FullEKey[] EKeyESpecHeaderKeys;
        private readonly EKeyESpecEntry[][] EKeyESpecPages;

        public EncodingHandler(ClientHandler client) : this(client,
            client.ConfigHandler.BuildConfig.Encoding.EncodingKey, client.ConfigHandler.BuildConfig.EncodingSize!.EncodedSize)
        {
        }

        public EncodingHandler(ClientHandler client, FullEKey eKey, int eSize) {
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

            var cKeyEKeyHeaders = reader.ReadArray<PageHeader>((int)cKeyEKeyPageCount);
            CKeyEKeyHeaderKeys = cKeyEKeyHeaders.Select(x => x.FirstKey).ToArray();
            CKeyEKeyPagesA = new CKey[cKeyEKeyHeaders.Length][];
            CKeyEKeyPagesB = new FullEKey[cKeyEKeyHeaders.Length][][];
            
            for (var pageIdx = 0; pageIdx < cKeyEKeyHeaders.Length; pageIdx++) {
                var page = new byte[cKeyEKeyPageSize * 1024];
                stream.DefinitelyRead(page);

                int structSize;
                unsafe {
                    structSize = sizeof(CKeyEKeyEntry);
                }

                var pageEntryCount = page.Length / structSize; // (approx)
                CKeyEKeyPagesA[pageIdx] = new FullKey[pageEntryCount];
                CKeyEKeyPagesB[pageIdx] = new FullEKey[pageEntryCount][];

                var arrOffset = 0;
                var entryIdx = 0;
                while (true) {
                    var foundEntrySpan = page.AsSpan(arrOffset);
                    if (foundEntrySpan.Length < structSize) break;
                    var foundEntry = MemoryMarshal.Read<CKeyEKeyEntry>(foundEntrySpan);
                    if (foundEntry.EKeyCount == 0) break; // end
                    arrOffset += structSize;
                    
                    CKeyEKeyPagesA[pageIdx][entryIdx] = foundEntry.CKey;

                    var ekeyArray = new FullEKey[foundEntry.EKeyCount];
                    CKeyEKeyPagesB[pageIdx][entryIdx] = ekeyArray;

                    for (int ekeyIdx = 0; ekeyIdx < foundEntry.EKeyCount; ekeyIdx++) {
                        if (ekeyIdx == 0) ekeyArray[ekeyIdx] = foundEntry.EKey;
                        else ekeyArray[ekeyIdx] = MemoryMarshal.Read<FullEKey>(page.AsSpan(arrOffset + 16 * (ekeyIdx - 1), 16));
                    }
                    
                    arrOffset += (foundEntry.EKeyCount - 1) * 16;
                    entryIdx++;
                }
                
                CKeyEKeyPagesA[pageIdx] = CKeyEKeyPagesA[pageIdx].Take(entryIdx).ToArray();
            }

            var eKeyESpecHeaders = reader.ReadArray<PageHeader>((int)eKeyEspecPageCount);
            EKeyESpecHeaderKeys = eKeyESpecHeaders.Select(x => x.FirstKey).ToArray();
            EKeyESpecPages = new EKeyESpecEntry[eKeyESpecHeaders.Length][];
            for (var pageIdx = 0; pageIdx < eKeyESpecHeaders.Length; pageIdx++) {
                var page = new byte[eKeyESpecPageSize * 1024];
                stream.DefinitelyRead(page);
                
                var entries = MemoryMarshal.Cast<byte, EKeyESpecEntry>(page);
                for (var entryIdx = 0; entryIdx < entries.Length; entryIdx++)
                {
                    if (entries[entryIdx].ESpecIndex.ToInt() != uint.MaxValue) continue;
                    // FFFFF.. = terminator
                    entries = entries.Slice(0, entryIdx);
                    break;
                }
                EKeyESpecPages[pageIdx] = entries.ToArray();
            }
        }

        public bool TryGetEncodingEntry(CKey cKey, out ReadOnlySpan<FullEKey> entry) {
            var searchResult = Array.BinarySearch(CKeyEKeyHeaderKeys, cKey);

            int pageIndex;
            if (searchResult >= 0) {
                pageIndex = searchResult;
            } else {
                var firstElementLarger = ~searchResult;
                pageIndex = firstElementLarger - 1;
                if (pageIndex < 0) goto NOT_FOUND;
            }
            
            var entries = CKeyEKeyPagesA[pageIndex];
            var foundIndex = Array.BinarySearch(entries, cKey);

            if (foundIndex >= 0) {
                entry = CKeyEKeyPagesB[pageIndex][foundIndex];
                return true;
            }

            NOT_FOUND:
            entry = Array.Empty<FullEKey>();
            Logger.Debug(nameof(EncodingHandler), $"Unable to get EKey for {cKey.ToHexString()} (This is okay, can be due to bundle encryption)");
            //Console.Out.WriteLine($"cant get ekey for {cKey.ToHexString()}. a o");
            return false;
        }

        public int GetEncodedSize(FullEKey ekey) {
            var searchResult = Array.BinarySearch(EKeyESpecHeaderKeys, ekey);

            int pageIndex;
            if (searchResult >= 0) {
                pageIndex = searchResult;
            } else {
                var firstElementLarger = ~searchResult;
                pageIndex = firstElementLarger - 1;
                if (pageIndex < 0) goto NOT_FOUND;
            }

            var speculativeEntry = new EKeyESpecEntry {
                EKey = ekey
            };
            var entries = EKeyESpecPages[pageIndex];
            var foundIndex = Array.BinarySearch(entries, speculativeEntry);

            if (foundIndex >= 0) {
                return (int)entries[foundIndex].FileSize.ToInt();
            }

            NOT_FOUND:
            Logger.Warn(nameof(EncodingHandler), $"Unable to get ESize for {ekey.ToHexString()}. This should not happen (and will crash static containers)");
            //Console.Out.WriteLine($"cant get size for {ekey.ToHexString()}. a o");
            return 0;
        }

        public IEnumerable<CKey> GetCKeys() {
            foreach (var page in CKeyEKeyPagesA) {
                foreach (var entry in page) {
                    yield return entry;
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
        public struct EKeyESpecEntry : IComparable<EKeyESpecEntry> {
            public FullEKey EKey;
            public UInt32BE ESpecIndex;
            public UInt40BE FileSize;

            public int CompareTo(EKeyESpecEntry other) {
                return EKey.CompareTo(other.EKey);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CKeyEKeyEntry : IComparable<CKeyEKeyEntry> {
            public ushort EKeyCount; // number of EKeys
            public UInt32BE ContentSize; // decoded size
            public FullKey CKey; // decoded key
            public FullEKey EKey; // encoded key

            /// <summary>Get content size</summary>
            /// <returns>Content size</returns>
            public int GetSize() {
                return (int)ContentSize.ToInt();
            }

            public int CompareTo(CKeyEKeyEntry other) {
                return CKey.CompareTo(other.CKey);
            }
        }
    }
}