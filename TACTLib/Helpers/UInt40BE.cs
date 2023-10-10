using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace TACTLib.Helpers {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UInt40BE
    {
        private byte m_high;
        private uint m_low;
        //private fixed byte val[5];

        public readonly ulong ToInt() {
            //return val[4] | (ulong)val[3] << 8 | (ulong)val[2] << 16 | (ulong)val[1] << 24 | (ulong)val[0] << 32;
            if (BitConverter.IsLittleEndian) {
                return (ulong) m_high << 32 | BinaryPrimitives.ReverseEndianness(m_low);
            }
            throw new NotImplementedException();
        }
    }
}