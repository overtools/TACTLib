using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace TACTLib.Core
{
    public unsafe struct Salsa20
    {
        private fixed uint m_stateBuffer[STATE_COUNT];
        
        private const int STATE_COUNT = 16;
        private const int BLOCK_SIZE = 64;
        private const int ROUNDS = 20;
        
        private static ReadOnlySpan<byte> Sigma => "expand 32-byte k"u8;
        private static ReadOnlySpan<byte> Tau => "expand 16-byte k"u8;

        private Span<uint> m_state => MemoryMarshal.CreateSpan(ref m_stateBuffer[0], STATE_COUNT);
        
        public Salsa20(ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv) {
            if (key.Length != 16 && key.Length != 32) throw new CryptographicException("Invalid key size; it must be 128 or 256 bits.");
            if (iv.Length != 8) throw new CryptographicException("Invalid IV size; it must be 8 bytes.");
            
            m_state[1] = ToUInt32(key, 0);
            m_state[2] = ToUInt32(key, 4);
            m_state[3] = ToUInt32(key, 8);
            m_state[4] = ToUInt32(key, 12);

            var constants = key.Length == 32 ? Sigma : Tau;
            var keyIndex = key.Length - 16;

            m_state[11] = ToUInt32(key, keyIndex + 0);
            m_state[12] = ToUInt32(key, keyIndex + 4);
            m_state[13] = ToUInt32(key, keyIndex + 8);
            m_state[14] = ToUInt32(key, keyIndex + 12);
            m_state[0] = ToUInt32(constants, 0);
            m_state[5] = ToUInt32(constants, 4);
            m_state[10] = ToUInt32(constants, 8);
            m_state[15] = ToUInt32(constants, 12);

            m_state[6] = ToUInt32(iv, 0);
            m_state[7] = ToUInt32(iv, 4);
            m_state[8] = 0;
            m_state[9] = 0;
        }
        
        public void Transform(ReadOnlySpan<byte> input, Span<byte> output) {
            Span<byte> hashOutput = stackalloc byte[64];
            
            while (input.Length > 0) {
                Hash(hashOutput, m_state);
                m_state[8] = AddOne(m_state[8]);
                if (m_state[8] == 0) m_state[9] = AddOne(m_state[9]);

                var blockSize = Math.Min(BLOCK_SIZE, input.Length);
                for (var i = 0; i < blockSize; i++)
                    output[i] = (byte) (input[i] ^ hashOutput[i]);

                input = input.Slice(blockSize);
                output = output.Slice(blockSize);
            }
        }

        private static void Hash(Span<byte> output, ReadOnlySpan<uint> existingState) {
            Debug.Assert(existingState.Length == 16); // should be the existing state buffer
            Span<uint> state = stackalloc uint[existingState.Length];
            existingState.CopyTo(state);

            for (var round = ROUNDS; round > 0; round -= 2) {
                state[4] ^= Rotate(Add(state[0], state[12]), 7);
                state[8] ^= Rotate(Add(state[4], state[0]), 9);
                state[12] ^= Rotate(Add(state[8], state[4]), 13);
                state[0] ^= Rotate(Add(state[12], state[8]), 18);
                state[9] ^= Rotate(Add(state[5], state[1]), 7);
                state[13] ^= Rotate(Add(state[9], state[5]), 9);
                state[1] ^= Rotate(Add(state[13], state[9]), 13);
                state[5] ^= Rotate(Add(state[1], state[13]), 18);
                state[14] ^= Rotate(Add(state[10], state[6]), 7);
                state[2] ^= Rotate(Add(state[14], state[10]), 9);
                state[6] ^= Rotate(Add(state[2], state[14]), 13);
                state[10] ^= Rotate(Add(state[6], state[2]), 18);
                state[3] ^= Rotate(Add(state[15], state[11]), 7);
                state[7] ^= Rotate(Add(state[3], state[15]), 9);
                state[11] ^= Rotate(Add(state[7], state[3]), 13);
                state[15] ^= Rotate(Add(state[11], state[7]), 18);
                state[1] ^= Rotate(Add(state[0], state[3]), 7);
                state[2] ^= Rotate(Add(state[1], state[0]), 9);
                state[3] ^= Rotate(Add(state[2], state[1]), 13);
                state[0] ^= Rotate(Add(state[3], state[2]), 18);
                state[6] ^= Rotate(Add(state[5], state[4]), 7);
                state[7] ^= Rotate(Add(state[6], state[5]), 9);
                state[4] ^= Rotate(Add(state[7], state[6]), 13);
                state[5] ^= Rotate(Add(state[4], state[7]), 18);
                state[11] ^= Rotate(Add(state[10], state[9]), 7);
                state[8] ^= Rotate(Add(state[11], state[10]), 9);
                state[9] ^= Rotate(Add(state[8], state[11]), 13);
                state[10] ^= Rotate(Add(state[9], state[8]), 18);
                state[12] ^= Rotate(Add(state[15], state[14]), 7);
                state[13] ^= Rotate(Add(state[12], state[15]), 9);
                state[14] ^= Rotate(Add(state[13], state[12]), 13);
                state[15] ^= Rotate(Add(state[14], state[13]), 18);
            }
            
            for (var index = 0; index < 16; index++)
                ToBytes(Add(state[index], existingState[index]), output, 4 * index);
        }
        
        private static uint Rotate(uint v, int c) {
            return BitOperations.RotateLeft(v, c);
        }

        private static uint Add(uint v, uint w) {
            return unchecked(v + w);
        }
        
        private static uint AddOne(uint v) {
            return unchecked(v + 1);
        }
        
        private static void ToBytes(uint input, Span<byte> output, int outputOffset) {
            MemoryMarshal.Write(output.Slice(outputOffset, 4), ref input);
        }
        
        private static uint ToUInt32(ReadOnlySpan<byte> input, int inputOffset) {
            return BitConverter.ToUInt32(input.Slice(inputOffset, 4));
        }
    }
}