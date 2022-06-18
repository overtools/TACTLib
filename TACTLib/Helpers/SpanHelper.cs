using System;
using System.Runtime.InteropServices;

namespace TACTLib.Helpers
{
    public static class SpanHelper
    {
        public static Span<byte> Advance(ref Span<byte> input, int numBytes)
        {
            var result = input.Slice(0, numBytes);
            input = input.Slice(numBytes);

            return result;
        }

        public static unsafe ref T ReadStruct<T>(ref Span<byte> input) where T : unmanaged
        {
            ref var result = ref MemoryMarshal.AsRef<T>(input);
            input = input.Slice(sizeof(T));
            return ref result;
        }

        public static byte ReadByte(ref Span<byte> input)
        {
            var result = input[0];
            input = input.Slice(1);
            return result;
        }
    }
}