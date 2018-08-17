using System.Runtime.InteropServices;
using TACTLib.Helpers;
using static TACTLib.Helpers.Utils;

namespace TACTLib.Container {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct EKey {
        // ReSharper disable once InconsistentNaming
        public const int CASC_EKEY_SIZE = 9;
        
        public fixed byte Value[CASC_EKEY_SIZE];

        public string ToHexString() {
            fixed (byte* b = Value)
            return PtrToByteArray(b, CASC_EKEY_SIZE).ToHexString();
        }
        
        public static EKey FromString(string @string) {
            return FromByteArray(StringToByteArray(@string));
        }
        
        public static EKey FromByteArray(byte[] array) {
            //if (array.Length != CASC_EKEY_SIZE)
            //    throw new ArgumentException($"array size != {CASC_EKEY_SIZE}");
            // todo: array can be to long but it's kinda ok because we need to truncate the last bytes anyway

            fixed (byte* ptr = array) {
                return *(EKey*) ptr;
            }
        }
    }
}