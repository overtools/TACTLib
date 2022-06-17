using System;
using System.Runtime.InteropServices;
using TACTLib.Helpers;
using static TACTLib.Utils;

namespace TACTLib.Container {
    /// <summary>
    /// Encoding Key
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct EKey {
        // ReSharper disable once InconsistentNaming
        /// <summary>Encoding Key size, in bytes</summary>
        public const int CASC_EKEY_SIZE = 9;
        
        /// <summary>Key value</summary>
        public fixed byte Value[CASC_EKEY_SIZE];

        /// <summary>
        /// Convert to a hex string
        /// </summary>
        /// <returns>Hex stirng</returns>
        public string ToHexString() {
            fixed (byte* b = Value)
                return PtrToSpan(b, CASC_EKEY_SIZE).ToHexString();
        }
        
        /// <summary>
        /// Create from a hex string
        /// </summary>
        /// <param name="string">Source stirng</param>
        /// <returns>Created EKey</returns>
        public static EKey FromString(string @string) {
            return FromByteArray(StringToByteArray(@string));
        }
        
        /// <summary>
        /// Create <see cref="EKey"/> from a byte array
        /// </summary>
        /// <param name="array">Source array</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Array length != <see cref="CASC_EKEY_SIZE"/></exception>
        public static EKey FromByteArray(byte[] array) {
            if (array.Length < CASC_EKEY_SIZE)
                throw new ArgumentException($"array size < {CASC_EKEY_SIZE}");

            return MemoryMarshal.Read<EKey>(array);
        }
    }
}