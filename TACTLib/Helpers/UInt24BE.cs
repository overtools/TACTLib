namespace TACTLib.Helpers
{
    public unsafe struct UInt24BE
    {
        private fixed byte m_data[3];

        public int ToInt()
        {
            return m_data[2] | (m_data[1] << 8) | (m_data[0] << 16);
        }
    }
}