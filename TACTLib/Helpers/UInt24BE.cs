using System.Runtime.CompilerServices;

namespace TACTLib.Helpers
{
    [InlineArray(3)]
    public struct UInt24BE
    {
        private byte m_first;

        public readonly int ToInt()
        {
            return this[2] | (this[1] << 8) | (this[0] << 16);
        }
    }
}