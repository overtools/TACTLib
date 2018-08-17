using System;
using System.Runtime.InteropServices;
using TACTLib.Helpers;
using static TACTLib.Helpers.Utils;

namespace TACTLib.Container {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct CKey {
        // ReSharper disable once InconsistentNaming
        public const int CASC_CKEY_SIZE = 16;
        
        public fixed byte Value[CASC_CKEY_SIZE];

        public string ToHexString() {
            fixed (byte* b = Value)
            return PtrToByteArray(b, CASC_CKEY_SIZE).ToHexString();;
        }
        
        public static CKey FromString(string @string) {
            return FromByteArray(StringToByteArray(@string));
        }

        public static CKey FromByteArray(byte[] array) {
            if (array.Length != CASC_CKEY_SIZE)
                throw new ArgumentException($"array size != {CASC_CKEY_SIZE}");

            fixed (byte* ptr = array) {
                return *(CKey*) ptr;
            }
        }
    }
}