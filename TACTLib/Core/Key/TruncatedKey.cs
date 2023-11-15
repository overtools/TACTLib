using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using TACTLib.Helpers;
using static TACTLib.Utils;

namespace TACTLib.Core.Key {
    /// <summary>
    /// Encoding Key
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [InlineArray(CASC_TRUNCATED_KEY_SIZE)]
    public struct TruncatedKey : IComparable<TruncatedKey>  {
        // ReSharper disable once InconsistentNaming
        /// <summary>Encoding Key size, in bytes</summary>
        public const int CASC_TRUNCATED_KEY_SIZE = 9;

        private byte _first;

        /// <summary>
        /// Convert to a hex string
        /// </summary>
        /// <returns>Hex stirng</returns>
        public readonly string ToHexString() {
            return Extensions.ToHexString(this);
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
        public static TruncatedKey FromByteArray(ReadOnlySpan<byte> array) {
            if (array.Length < CASC_TRUNCATED_KEY_SIZE)
                throw new ArgumentException($"array size < {CASC_TRUNCATED_KEY_SIZE}");

            return MemoryMarshal.Read<TruncatedKey>(array);
        }
        
        public int CompareTo(TruncatedKey other) {
            return TruncatedKeyCompare(this, other);
        }

        public static int TruncatedKeyCompare(TruncatedKey left, TruncatedKey right)
        {
            var leftSpan = (ReadOnlySpan<byte>)left;
            var rightSpan = (ReadOnlySpan<byte>)right;

            var leftU0 = BinaryPrimitives.ReadUInt64BigEndian(leftSpan);
            var rightU0 = BinaryPrimitives.ReadUInt64BigEndian(rightSpan);

            var compareA = leftU0.CompareTo(rightU0);
            if (compareA != 0) return compareA;

            var leftU1 = MemoryMarshal.Read<byte>(leftSpan.Slice(8));
            var rightU1 = MemoryMarshal.Read<byte>(rightSpan.Slice(8));
            return leftU1.CompareTo(rightU1);
        }
    }
}