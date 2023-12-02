using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace TACTLib.Helpers {
    [StructLayout(LayoutKind.Sequential)]
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