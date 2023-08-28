using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using TACTLib.Helpers;
using static TACTLib.Utils;

namespace TACTLib.Core.Key {
    /// <summary>
    /// Content Key
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct FullKey : IComparable<FullKey> {
        // ReSharper disable once InconsistentNaming
        /// <summary>Content Key size, in bytes</summary>
        public const int CASC_FULL_KEY_SIZE = 16;

        /// <summary>Key value</summary>
        public fixed byte Value[CASC_FULL_KEY_SIZE];

        /// <summary>
        /// Convert to a hex string
        /// </summary>
        /// <returns>Hex stirng</returns>
        public readonly string ToHexString() {
            fixed (byte* b = Value)
                return PtrToSpan(b, CASC_FULL_KEY_SIZE).ToHexString();
        }

        /// <summary>
        /// Create from a hex string
        /// </summary>
        /// <param name="string">Source stirng</param>
        /// <returns>Created FullKey</returns>
        public static FullKey FromString(string @string) {
            return FromByteArray(StringToByteArray(@string));
        }

        /// <summary>
        /// Create <see cref="FullKey"/> from a byte array
        /// </summary>
        /// <param name="array">Source array</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Array length != <see cref="CASC_FULL_KEY_SIZE"/></exception>
        public static FullKey FromByteArray(ReadOnlySpan<byte> array) {
            if (array.Length < CASC_FULL_KEY_SIZE)
                throw new ArgumentException($"array size < {CASC_FULL_KEY_SIZE}");

            return MemoryMarshal.Read<FullKey>(array);
        }

        public EKey AsTruncated() {
            return Unsafe.As<FullKey, EKey>(ref this);
        }

        public int CompareTo(FullKey other) {
            return FullKeyCompare(this, other);
        }

        public static int FullKeyCompare(FullKey left, FullKey right)
        {
            var leftSpan = MemoryMarshal.CreateReadOnlySpan(ref left, 1).AsBytes();
            var rightSpan = MemoryMarshal.CreateReadOnlySpan(ref right, 1).AsBytes();

            var leftU0 = BinaryPrimitives.ReadUInt64BigEndian(leftSpan);
            var rightU0 = BinaryPrimitives.ReadUInt64BigEndian(rightSpan);

            var compareA = leftU0.CompareTo(rightU0);
            if (compareA != 0) return compareA;

            var leftU1 = BinaryPrimitives.ReadUInt64BigEndian(leftSpan.Slice(8));
            var rightU1 = BinaryPrimitives.ReadUInt64BigEndian(rightSpan.Slice(8));
            return leftU1.CompareTo(rightU1);
        }
    }
}