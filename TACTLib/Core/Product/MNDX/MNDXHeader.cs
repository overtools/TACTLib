// https://raw.githubusercontent.com/WoW-Tools/CascLib/master/CascLib/RootHandlers/MNDXRootHandler.cs

namespace TACTLib.Core.Product.MNDX
{
    internal struct MNDXHeader
    {
        public int Signature;                            // 'MNDX'
        public int HeaderVersion;                        // Must be <= 2
        public int FormatVersion;
    }
}
