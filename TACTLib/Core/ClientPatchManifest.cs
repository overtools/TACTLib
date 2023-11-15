using System.Runtime.InteropServices;
using TACTLib.Helpers;

namespace TACTLib.Core {
    public class ClientPatchManifest {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header {
            /// <summary>
            /// Patch signature, "PA"
            /// </summary>
            public short Signature;
            public byte Version;
            public byte CKeySize;
            public byte OKeySize;
            public byte PatchKeySize;
            public byte BlockSize;
            public UInt16BE BlockCount;  // uint16-BE
            public byte Flags;
            public CKey CKey;
            public EKey EKey;
            public UInt32BE DecodedSize;  // uint32-BE
            public UInt32BE EncodedSize;  // uint32-BE
            public byte ESpecLength;
            // char encoding_format[ESpecLength];
        }

        public struct BlockHeader {
            public CKey LastFileCKey;
            public CKey BlockMD5; // maybe shouldn't be CKey
            public UInt32BE BlockOffset;  // uint32-BE
        }

        // at BlockOffset
        // count unspecified: read until the next file num_patches would be 0 or block would exceed max block size
        public struct BlockFile {
            public byte NumPatches;
            public CKey TargetCKey;
            public UInt40BE DecodedSize; // uint40-BE
        }

        public struct BlockFilePatch {
            public EKey SourceEKey;
            public UInt40BE DecodedSize; // uint40-BE
            public EKey PatchEKey;  // todo: check size  (patch_ekey[header.patch_key_size])
            public UInt32BE PatchSize;  // uint32-BE
            public byte Unknown;  // some sort of patch index number. first entry seems to always be 1
        }
    }
}