using System;
using System.Runtime.InteropServices;
using TACTLib.Helpers;
using static TACTLib.Utils;

namespace TACTLib.Core.Key {
    /// <summary>
    /// Encoding Key
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct TruncatedKey {
        // ReSharper disable once InconsistentNaming
        /// <summary>Encoding Key size, in bytes</summary>
        public const int CASC_TRUNCATED_KEY_SIZE = 9;

        /// <summary>Key value</summary>
        public fixed byte Value[CASC_TRUNCATED_KEY_SIZE];

        /// <summary>
        /// Convert to a hex string
        /// </summary>
        /// <returns>Hex stirng</returns>
        public readonly string ToHexString() {
            fixed (byte* b = Value)
                return PtrToSpan(b, CASC_TRUNCATED_KEY_SIZE).ToHexString();
        }

        /// <summary>
        /// Create from a hex string
        /// </summary>
        /// <param name="string">Source stirng</param>
        /// <returns>Created EKey</returns>
        public static TruncatedKey FromString(string @string) {
            return FromByteArray(StringToByteArray(@string));
        }

        /// <summary>
        /// Create <see cref="TruncatedKey"/> from a byte array
        /// </summary>
        /// <param name="array">Source array</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Array length != <see cref="CASC_TRUNCATED_KEY_SIZE"/></exception>
        public static TruncatedKey FromByteArray(byte[] array) {
            if (array.Length < CASC_TRUNCATED_KEY_SIZE)
                throw new ArgumentException($"array size < {CASC_TRUNCATED_KEY_SIZE}");

            return MemoryMarshal.Read<TruncatedKey>(array);
        }
    }
}