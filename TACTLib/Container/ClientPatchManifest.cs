using System.Runtime.InteropServices;

namespace TACTLib.Container {
    public class ClientPatchManifest {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct Header {
            public short Signature;
            public byte Version;
            public byte CKeySize;
            public byte OKeySize;
            public byte PatchKeySize;
            public byte BlockSize;
            public fixed byte BlockCount[2];  // uint16-BE
            public byte Flags;
            public CKey CKey;
            public EKey EKey;
            public fixed byte DecodedSize[4];  // uint32-BE
            public fixed byte EncodedSize[4];  // uint32-BE
            public byte ESpecLength;
            // char encoding_format[ESpecLength];
        }

        public unsafe struct BlockHeader {
            public CKey LastFileCKey;
            public CKey BlockMD5; // maybe shouldn't be CKey
            public fixed byte BlockOffset[4];  // uint32-BE
        }

        // at BlockOffset
        // count unspecified: read until the next file num_patches would be 0 or block would exceed max block size
        public unsafe struct BlockFile {
            public byte NumPatches;
            public CKey TargetCKey;
            public fixed byte DecodedSize[5]; // uint40-BE
        }

        public unsafe struct BlockFilePatch {
            public EKey SourceEKey;
            public fixed byte DecodedSize[5]; // uint40-BE
            public EKey PatchEKey;  // todo: check size  (patch_ekey[header.patch_key_size])
            public fixed byte PatchSize[4];  // uint32-BE
            public byte Unknown;  // some sort of patch index number. first entry seems to always be 1
        }
    }
}