using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TACTLib.Helpers;
using static TACTLib.Utils;

namespace TACTLib.Container {
    /// <summary>
    /// Content Key
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct CKey {
        // ReSharper disable once InconsistentNaming
        /// <summary>Content Key size, in bytes</summary>
        public const int CASC_CKEY_SIZE = 16;

        /// <summary>Key value</summary>
        public fixed byte Value[CASC_CKEY_SIZE];

        /// <summary>
        /// Convert to a hex string
        /// </summary>
        /// <returns>Hex stirng</returns>
        public readonly string ToHexString() {
            fixed (byte* b = Value)
                return PtrToSpan(b, CASC_CKEY_SIZE).ToHexString();
        }

        /// <summary>
        /// Create from a hex string
        /// </summary>
        /// <param name="string">Source stirng</param>
        /// <returns>Created CKey</returns>
        public static CKey FromString(string @string) {
            return FromByteArray(StringToByteArray(@string));
        }

        /// <summary>
        /// Create <see cref="CKey"/> from a byte array
        /// </summary>
        /// <param name="array">Source array</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Array length != <see cref="CASC_CKEY_SIZE"/></exception>
        public static CKey FromByteArray(byte[] array) {
            if (array.Length < CASC_CKEY_SIZE)
                throw new ArgumentException($"array size < {CASC_CKEY_SIZE}");

            return MemoryMarshal.Read<CKey>(array);
        }

        public EKey AsEKey() {
            return Unsafe.As<CKey, EKey>(ref this);
        }
    }
}