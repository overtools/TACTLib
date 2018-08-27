using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using TACTLib.Container;
using TACTLib.Helpers;
using static TACTLib.Utils;

namespace TACTLib.Core {
    public class VFSManifestReader {
        // ReSharper disable once InconsistentNaming
        public const uint TVFS_FOLDER_SIZE_MASK = 0x7FFFFFFF;
        // ReSharper disable once InconsistentNaming
        public const uint TVFS_FOLDER_NODE = 0x80000000;     // Highest bit is set if a file node is a folder
        
        [Flags]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum ManifestFlags {
            /// <summary>
            /// Include CKey in content file record
            /// </summary>
            INCLUDE_CKEY = 0x0001,

            /// <summary>
            /// Write support. Include a table of encoding specifiers. This is required for writing files to the underlying storage. This bit is implied by the patch-support bit
            /// </summary>
            WRITE_SUPPORT = 0x0002,
            
            /// <summary>
            /// Patch support. Include patch records in the content file records.
            /// </summary>
            PATCH_SUPPORT = 0x0004,

            /// <summary>
            /// Lowercase manifest. All paths in the path table have been converted to ASCII lowercase (i.e. [A-Z] converted to [a-z])
            /// </summary>
            LOWERCASE_MANIFEST = 0x0008
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [Flags]
        public enum PathEntryFlags {
            /// <summary>
            /// There is path separator before the name
            /// </summary>
            PATH_SEPARATOR_PRE = 0x0001,
            
            /// <summary>
            /// There is path separator after the name
            /// </summary>
            PATH_SEPARATOR_POST = 0x0002,
            
            /// <summary>
            /// The NodeValue in path table entry is valid
            /// </summary>
            NODE_VALUE = 0x0004
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct ManifestHeader {
            /// <summary>
            /// Magic identifier. "TVFS"/0x53465654
            /// </summary>
            public int Magic;
            
            /// <summary>
            /// Version. Always 1
            /// </summary>
            public byte Version;
            
            /// <summary>
            /// Header size. Always >= 0x26
            /// </summary>
            public byte HeaderSize;
            
            /// <summary>
            /// Encoding Key size. Always 9
            /// </summary>
            public byte EKeySize;
            
            /// <summary>
            /// Patch Key size. Always 9
            /// </summary>
            public byte PKeySize;

            public fixed byte Flags[4];

            /// <summary>
            /// Offset of the path table
            /// </summary>
            public fixed byte PathTableOffset[4];
            
            /// <summary>
            /// Size of the path table
            /// </summary>
            public fixed byte PathTableSize[4];
            
            /// <summary>
            /// Offset of the VFS table
            /// </summary>
            public fixed byte VfsTableOffset[4];
            
            /// <summary>
            /// Size of the VFS table
            /// </summary>
            public fixed byte VfsTableSize[4];
            
            /// <summary>
            /// Offset of the container file table
            /// </summary>
            public fixed byte CftTableOffset[4];
            
            /// <summary>
            /// Size of the container file table
            /// </summary>
            public fixed byte CftTableSize[4];
            
            /// <summary>
            /// The maximum depth of the path prefix tree stored in the path table
            /// </summary>
            public fixed byte MaxDepth[2];

            //todo: only when write
            ///// <summary>
            ///// The offset of the encoding specifier table. Only if the write-support bit is set in the header flag
            ///// </summary>
            //public fixed byte EstTableOffset[4];
            //
            ///// <summary>
            ///// The size of the encoding specifier table. Only if the write-support bit is set in the header flag
            ///// </summary>
            //public fixed byte EstTableSize[4];
            
            public ManifestFlags GetFlags() {
                fixed (byte* b = Flags)
                    return (ManifestFlags) Int32FromPtrBE(b);
            }
        }

        public class Manifest {
            public readonly ManifestHeader Header;
            
            public readonly ManifestFlags Flags;
            public readonly int PathTableOffset;
            public readonly int PathTableSize;
            public readonly int VfsTableOffset;
            public readonly int VfsTableSize;
            public readonly int CftTableOffset;
            public readonly int CftTableSize;
            public readonly ushort MaxDepth;
            
            public readonly int CftOffsSize;
            //public readonly int EstOffsSize;

            public readonly List<VFSFile> Files;

            public unsafe Manifest(ManifestHeader header) {
                Header = header;

                Flags = header.GetFlags();
                PathTableOffset = Int32FromPtrBE(header.PathTableOffset);
                PathTableSize = Int32FromPtrBE(header.PathTableSize);
                VfsTableOffset = Int32FromPtrBE(header.VfsTableOffset);
                VfsTableSize = Int32FromPtrBE(header.VfsTableSize);
                CftTableOffset = Int32FromPtrBE(header.CftTableOffset);
                CftTableSize = Int32FromPtrBE(header.CftTableSize);
                MaxDepth = (ushort)Int16FromPtrBE(header.MaxDepth);
                CftOffsSize = GetOffsetFieldSize(CftTableSize);
                //EstOffsSize = GetOffsetFieldSize(header.EstTableSize);
                
                Files = new List<VFSFile>();
            }
        
            // Returns size of "container file table offset" fiels in the VFS.
            // - If the container file table is larger than 0xffffff bytes, it's 4 bytes
            // - If the container file table is larger than 0xffff bytes, it's 3 bytes
            // - If the container file table is larger than 0xff bytes, it's 2 bytes
            // - If the container file table is smaller than 0xff bytes, it's 1 byte
            private static int GetOffsetFieldSize(int tableSize) {
                if(tableSize > 0xffffff)
                    return 4;
                if(tableSize > 0xffff)
                    return 3;
                if(tableSize > 0xff)
                    return 2;
                return 1;
            }
        }

        public struct PathEntry {
            public string Name;
            public PathEntryFlags NodeFlags;
            public int NodeValue; // Node value
        }
        
        private static PathEntry ReadPathEntry(BinaryReader reader, long pathTableEnd) {
            PathEntry pathEntry = new PathEntry();
            
            byte bBefore = (byte)reader.PeekChar();
            if (reader.BaseStream.Position < pathTableEnd && bBefore == 0) {
                pathEntry.NodeFlags |= PathEntryFlags.PATH_SEPARATOR_PRE;
                reader.BaseStream.Position++;
            }

            if (reader.BaseStream.Position < pathTableEnd && bBefore != 0xFF) {
                byte length = reader.ReadByte();
                pathEntry.Name = Encoding.UTF8.GetString(reader.ReadBytes(length));
            }

            byte bAfter = (byte) reader.PeekChar();
            if (reader.BaseStream.Position < pathTableEnd && bAfter == 0) {
                pathEntry.NodeFlags |= PathEntryFlags.PATH_SEPARATOR_POST;
                reader.BaseStream.Position++;
            }

            if (reader.BaseStream.Position < pathTableEnd) {
                byte check = (byte)reader.PeekChar();
                if (check == 0xFF) { // Check for node value
                    reader.BaseStream.Position += 1;
                    
                    pathEntry.NodeValue = reader.ReadInt32BE();
                    pathEntry.NodeFlags |= PathEntryFlags.NODE_VALUE;
                } else {  // Non-0xFF after the name means path separator after
                    pathEntry.NodeFlags |= PathEntryFlags.PATH_SEPARATOR_POST;
                    Debug.Assert(reader.PeekChar() != 0);
                }
            }

            return pathEntry;
        }

        public static Manifest Read(BinaryReader reader) {
            ManifestHeader header = reader.Read<ManifestHeader>();
            Manifest manifest = new Manifest(header);

            ParseDirectoryData(manifest, reader);
            
            return manifest;
        }
        
        private static void ParseDirectoryData(Manifest manifest, BinaryReader reader) {
            long rootDirPtr = manifest.PathTableOffset;
            long rootDirEnd = rootDirPtr + manifest.PathTableSize;
            if (manifest.PathTableOffset + 1 + sizeof(int) < rootDirEnd) {
                // The structure of the root directory
                // -----------------------------------
                // 1byte   0xFF
                // 4bytes  NodeValue (BigEndian). The most significant bit is set
                //          - Lower 31 bits contain length of the directory data, including NodeValue

                byte b = reader.ReadByte();
                if (b == 0xFF) {
                    int nodeValue = reader.ReadInt32BE();
                    
                    rootDirEnd = rootDirPtr + 1 + (nodeValue & TVFS_FOLDER_SIZE_MASK);
                    rootDirPtr = rootDirPtr + 1 + sizeof(int);

                    if (rootDirEnd > manifest.PathTableOffset + manifest.PathTableSize) {
                        throw new Exception();
                    }
                }
            }
            
            ParsePathFileTable(manifest, reader, rootDirPtr, rootDirEnd, "");
        }

        private static void ParsePathFileTable(Manifest manifest, BinaryReader reader, long pathTablePtr, long pathTableEnd, string pathBuffer) {
            reader.BaseStream.Position = pathTablePtr;

            string pathBufferBak = pathBuffer;

            while (reader.BaseStream.Position < pathTableEnd) {
                var entry = ReadPathEntry(reader, pathTableEnd);

                pathBuffer = AppendNodeToPath(entry, pathBuffer);
                
                // Folder component
                if ((entry.NodeFlags & PathEntryFlags.NODE_VALUE) != 0) {
                    // If the TVFS_FOLDER_NODE is set, then the path node is a directory,
                    // with its data immediately following the path node. Lower 31 bits of NodeValue
                    // contain the length of the directory (including the NodeValue!)
                    if ((entry.NodeValue & TVFS_FOLDER_NODE) != 0) {
                        long directoryEnd = reader.BaseStream.Position + (entry.NodeValue & TVFS_FOLDER_SIZE_MASK) - sizeof(int);
                        
                        // Check the available data
                        Debug.Assert((entry.NodeValue & TVFS_FOLDER_SIZE_MASK) >= sizeof(int));
                        
                        // Recursively call the folder parser on the same file
                        ParsePathFileTable(manifest, reader, reader.BaseStream.Position, directoryEnd, pathBuffer);

                        // skip directory data
                        reader.BaseStream.Position = directoryEnd;
                    } else {
                        // Capture the VFS and Container Table Entry in order to get the file EKey
                        long before = reader.BaseStream.Position;
                        var file = ReadVfsSpanEntries(manifest, reader, out int spanSize, entry.NodeValue);
                        reader.BaseStream.Position = before;

                        file.Name = pathBuffer;
                        
                        manifest.Files.Add(file);
                    }

                    pathBuffer = pathBufferBak;
                }
            }
        }

        private static VFSFile ReadVfsSpanEntries(Manifest manifest, BinaryReader reader, out int spanSize, int vfsOffset) {
            // Get the number of span entries
            //if(!(pbVfsFileTable <= pbVfsFileEntry && pbVfsFileEntry <= pbVfsFileEnd))
            //    return ERROR_INVALID_PARAMETER;
            long vfsFileTable = manifest.VfsTableOffset;
            long vfsFileEntry = vfsFileTable + vfsOffset;
            //long vfsFileEnd = vfsFileTable + manifest.VfsTableSize;

            reader.BaseStream.Position = vfsFileEntry;

            int spanCount = reader.ReadByte(); 
            // 1 - 224 = valid file, 225-254 = other file, 255 = deleted file
            // We will ignore all files with unsupported span count
            if (spanCount == 0 || spanCount > 224) {
                throw new Exception();
            }
            
            // todo: spanCount *can* be > 1 on viper
            // So far we've only saw entries with 1 span.
            // Need to test files with multiple spans. Ignore such files for now.
            //Debug.Assert(spanCount == 1);

            // Structure of the span entry:
            // (4bytes): Offset into the referenced file (big endian)
            // (4bytes): Size of the span (big endian)
            // (?bytes): Offset into Container File Table. Length depends on container file table size

            int fileOffset = reader.ReadInt32BE();
            spanSize = reader.ReadInt32BE();
            
            int cftOffset = 0;
            if (manifest.CftOffsSize == 1) {
                cftOffset = reader.ReadByte();
            } else if (manifest.CftOffsSize == 2) {
                cftOffset = reader.ReadInt16BE();
            } else if (manifest.CftOffsSize == 3) {
                throw new NotImplementedException();
                //cftOffset = reader.ReadInt24();
            } else if (manifest.CftOffsSize == 4) {
                cftOffset = reader.ReadInt32BE();
            }
            
            int cftFileTable = manifest.CftTableOffset;
            int cftFileEntry = cftFileTable + cftOffset;
            //int cftFileEnd = cftFileTable + manifest.CftTableSize;

            reader.BaseStream.Position = cftFileEntry;
            var eKey = reader.Read<EKey>();
            VFSFile file = new VFSFile {
                Offset = fileOffset,
                Name = null,
                EKey = eKey
            };
            return file;
        }
        
        private static string AppendNodeToPath(PathEntry entry, string path) {
            if ((entry.NodeFlags & PathEntryFlags.PATH_SEPARATOR_PRE) != 0)
                path += "/";

            // Append the name fragment, if any
            if (entry.Name != null)
                path += entry.Name;

            // Append the postfix separator, if needed
            if ((entry.NodeFlags & PathEntryFlags.PATH_SEPARATOR_POST) != 0)
                path += "/";
            return path;
        }
    }
}