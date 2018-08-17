﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using TACTLib.Client;
using TACTLib.Container;
using TACTLib.Helpers;
using static TACTLib.Helpers.Utils;

namespace TACTLib.Core {
    public class EncodingHandler {
        /// <summary>Encoding table</summary>
        private readonly Dictionary<CKey, CKeyEntry> _encodingEntries;
        
        public EncodingHandler(ClientHandler client) {
            _encodingEntries = new Dictionary<CKey, CKeyEntry>(CASCKeyComparer.Instance);
            
            using (Stream stream = client.OpenEKey(client.ConfigHandler.BuildConfig.Encoding.EncodingKey)) {
                if (stream == null) return;
                using (BinaryReader reader = new BinaryReader(stream)) {
                    Header header = reader.Read<Header>();

                    if (header.Signature != 0x4E45 || header.CKeySize != 16 || header.EKeySize != 16 ||
                        header.Version != 1) {
                        throw new InvalidDataException($"EncodingHandler: encoding header invalid (magic: {header.Signature:X2})");
                    }
                    
                    ushort cKeyPageSize = reader.ReadUInt16BE();  // in kilo bytes. e.g. 4 in here → 4096 byte pages (default)
                    ushort especPageSize = reader.ReadUInt16BE(); // same
                    
                    int cKeyPageCount = reader.ReadInt32BE();
                    int especPageCount = reader.ReadInt32BE();

                    byte unknown = reader.ReadByte();
                    Debug.Assert(unknown == 0); // asserted by agent

                    int especBlockSize = reader.ReadInt32BE();

                    stream.Position += especBlockSize;

                    PageHeader[] pageHeaders = reader.ReadArray<PageHeader>(cKeyPageCount);
                    for (int i = 0; i < cKeyPageCount; i++) {
                        long pageEnd = stream.Position + cKeyPageSize * 1024;

                        while (stream.Position <= pageEnd) {
                            CKeyEntry entry = reader.Read<CKeyEntry>();
                        
                            stream.Position +=  (CKey.CASC_CKEY_SIZE - EKey.CASC_EKEY_SIZE) + (entry.EKeyCount - 1) * header.EKeySize;
                            // 16-9 because we are truncating the eKey

                            _encodingEntries[entry.CKey] = entry;
                        }
                        
                        reader.BaseStream.Position = pageEnd; // just checking
                    }
                }
            }
        }

        public bool TryGetEncodingEntry(CKey cKey, out CKeyEntry entry) {
            return _encodingEntries.TryGetValue(cKey, out entry);
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
            public EKey EKey;

            /// <summary>Get content size</summary>
            /// <returns>Content size</returns>
            public int GetSize() {
                fixed (byte* b = ContentSize)
                    return Int32FromPtrBE(b);
            }
        }
    }
}