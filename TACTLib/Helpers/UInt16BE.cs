using System;
using System.Buffers.Binary;

namespace TACTLib.Helpers {
    public struct UInt16BE
    {
        private ushort m_data;

        public readonly ushort ToInt()
        {
            if (BitConverter.IsLittleEndian) return BinaryPrimitives.ReverseEndianness(m_data);
            return m_data;
        }
    }
}