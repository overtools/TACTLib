using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace TACTLib.Helpers {
    [StructLayout(LayoutKind.Sequential)]
    public struct UInt32BE
    {
        private uint m_data;

        public readonly uint ToInt()
        {
            if (BitConverter.IsLittleEndian) return BinaryPrimitives.ReverseEndianness(m_data);
            return m_data;
        }
    }
}