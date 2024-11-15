using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using CommunityToolkit.HighPerformance;

namespace TACTLib.Core
{
    public struct Salsa20
    {
        private UIntArray m_state;
        
        [InlineArray(STATE_COUNT)]
        private struct UIntArray
        {
            private uint m_first;
        }
        
        public const int BLOCK_SIZE = 64;
        public const int IV_SIZE = 8;
        public const int ROUNDS = 20;
        public const int STATE_COUNT = BLOCK_SIZE/sizeof(uint); // 16
        
        public static ReadOnlySpan<byte> Sigma => "expand 32-byte k"u8;
        public static ReadOnlySpan<byte> Tau => "expand 16-byte k"u8;
        
        public Salsa20(ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, ulong startOffset=0) {
            if (key.Length != 16 && key.Length != 32) throw new CryptographicException("Invalid key size; it must be 128 or 256 bits.");
            if (iv.Length != IV_SIZE) throw new CryptographicException("Invalid IV size; it must be 8 bytes.");
            
            m_state[1] = ExtractU32(key, 0);
            m_state[2] = ExtractU32(key, 4);
            m_state[3] = ExtractU32(key, 8);
            m_state[4] = ExtractU32(key, 12);

            var constants = key.Length == 32 ? Sigma : Tau;
            var keyIndex = key.Length - 16;

            m_state[11] = ExtractU32(key, keyIndex + 0);
            m_state[12] = ExtractU32(key, keyIndex + 4);
            m_state[13] = ExtractU32(key, keyIndex + 8);
            m_state[14] = ExtractU32(key, keyIndex + 12);
            m_state[0] = ExtractU32(constants, 0);
            m_state[5] = ExtractU32(constants, 4);
            m_state[10] = ExtractU32(constants, 8);
            m_state[15] = ExtractU32(constants, 12);

            m_state[6] = ExtractU32(iv, 0);
            m_state[7] = ExtractU32(iv, 4);

            var (q, r) = Math.DivRem(startOffset/BLOCK_SIZE, uint.MaxValue);
            m_state[8] = (uint)r;
            m_state[9] = (uint)q;
        }
        
        public void TransformBlocks(ReadOnlySpan<byte> input, Span<byte> output) {
            while (input.Length > 0) {
                TransformBlock(input, output);

                var blockSize = Math.Min(BLOCK_SIZE, input.Length);
                input = input.Slice(blockSize);
                output = output.Slice(blockSize);
            }
        }
        
        public void TransformBlock(ReadOnlySpan<byte> input, Span<byte> output) {
            var hashOutput = new UIntArray();
            var hashOutputBytes = ((ReadOnlySpan<uint>)hashOutput).AsBytes();
            
            Hash(ref hashOutput, in m_state);
            m_state[8]++;
            if (m_state[8] == 0) m_state[9]++;

            // todo: TensorPrimitives?
            // or.. full vectorized algorithm? https://github.com/HMBSbige/CryptoBase/blob/master/src/CryptoBase/SymmetricCryptos/StreamCryptos/Salsa20Utils.cs
            var blockSize = Math.Min(BLOCK_SIZE, input.Length);
            for (var i = 0; i < blockSize; i++)
                output[i] = (byte) (input[i] ^ hashOutputBytes[i]);
        }

        private static void Hash(ref UIntArray output, ref readonly UIntArray existingState) {
            var state = existingState;

            for (var round = ROUNDS; round > 0; round -= 2) {
                // todo: combine back into quarter-rounds? depends on inlining
                state[4]  ^= Rotate(state[0] + state[12],  7);
                state[8]  ^= Rotate(state[4]  + state[0],  9);
                state[12] ^= Rotate(state[8]  + state[4],  13);
                state[0]  ^= Rotate(state[12] + state[8],  18);
                state[9]  ^= Rotate(state[5]  + state[1],  7);
                state[13] ^= Rotate(state[9]  + state[5],  9);
                state[1]  ^= Rotate(state[13] + state[9],  13);
                state[5]  ^= Rotate(state[1]  + state[13], 18);
                state[14] ^= Rotate(state[10] + state[6],  7);
                state[2]  ^= Rotate(state[14] + state[10], 9);
                state[6]  ^= Rotate(state[2]  + state[14], 13);
                state[10] ^= Rotate(state[6]  + state[2],  18);
                state[3]  ^= Rotate(state[15] + state[11], 7);
                state[7]  ^= Rotate(state[3]  + state[15], 9);
                state[11] ^= Rotate(state[7]  + state[3],  13);
                state[15] ^= Rotate(state[11] + state[7],  18);
                state[1]  ^= Rotate(state[0]  + state[3],  7);
                state[2]  ^= Rotate(state[1]  + state[0],  9);
                state[3]  ^= Rotate(state[2]  + state[1],  13);
                state[0]  ^= Rotate(state[3]  + state[2],  18);
                state[6]  ^= Rotate(state[5]  + state[4],  7);
                state[7]  ^= Rotate(state[6]  + state[5],  9);
                state[4]  ^= Rotate(state[7]  + state[6],  13);
                state[5]  ^= Rotate(state[4]  + state[7],  18);
                state[11] ^= Rotate(state[10] + state[9],  7);
                state[8]  ^= Rotate(state[11] + state[10], 9);
                state[9]  ^= Rotate(state[8]  + state[11], 13);
                state[10] ^= Rotate(state[9]  + state[8],  18);
                state[12] ^= Rotate(state[15] + state[14], 7);
                state[13] ^= Rotate(state[12] + state[15], 9);
                state[14] ^= Rotate(state[13] + state[12], 13);
                state[15] ^= Rotate(state[14] + state[13], 18);
            }
            
            for (var index = 0; index < STATE_COUNT; index++)
                output[index] = state[index] + existingState[index];
        }
        
        private static uint Rotate(uint v, int c) {
            return BitOperations.RotateLeft(v, c);
        }
        
        private static uint ExtractU32(ReadOnlySpan<byte> input, int inputOffset) {
            return BitConverter.ToUInt32(input.Slice(inputOffset, 4));
        }
    }
}