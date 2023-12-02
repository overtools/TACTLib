using System;
using System.Runtime.InteropServices;

namespace TACTLib.Helpers
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UInt24BE
    {
        private byte m_a;
        private byte m_b;
        private byte m_c;

        public readonly int ToInt()
        {
            if (BitConverter.IsLittleEndian)
            {
                return m_c | (m_b << 8) | (m_a << 16);
            }
            throw new NotImplementedException();
        }
    }
}