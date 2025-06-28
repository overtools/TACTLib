using System;
using System.Runtime.InteropServices;

namespace TACTLib.Helpers
{
    public static class SpanHelper
    {
        public static Span<T> Advance<T>(scoped ref Span<T> input, int numEntries)
        {
            var result = input.Slice(0, numEntries);
            input = input.Slice(numEntries);

            return result;
        }
        
        public static ReadOnlySpan<T> Advance<T>(scoped ref ReadOnlySpan<T> input, int numEntries)
        {
            var result = input.Slice(0, numEntries);
            input = input.Slice(numEntries);

            return result;
        }

        public static unsafe ref T ReadStruct<T>(scoped ref Span<byte> input) where T : unmanaged
        {
            ref var result = ref MemoryMarshal.AsRef<T>(input);
            input = input.Slice(sizeof(T));
            return ref result;
        }
        
        public static unsafe T ReadStruct<T>(scoped ref ReadOnlySpan<byte> input) where T : unmanaged
        {
            var result = MemoryMarshal.Read<T>(input);
            input = input.Slice(sizeof(T));
            return result;
        }
        
        public static unsafe ReadOnlySpan<T> ReadArray<T>(scoped ref ReadOnlySpan<byte> input, int count) where T : unmanaged
        {
            var resultBytes = Advance(ref input, sizeof(T) * count);
            return MemoryMarshal.Cast<byte, T>(resultBytes);
        }

        public static byte ReadByte(scoped ref Span<byte> input)
        {
            var result = input[0];
            input = input.Slice(1);
            return result;
        }
    }
}