
using System;
using System.Buffers.Binary;

namespace TACTLib.Helpers {
    public struct UInt32BE
    {
        private uint m_data;

        public uint ToInt()
        {
            if (BitConverter.IsLittleEndian) return BinaryPrimitives.ReverseEndianness(m_data);
            return m_data;
        }
    }
}