// https://raw.githubusercontent.com/WoW-Tools/CascLib/master/CascLib/RootHandlers/MNDXRootHandler.cs

namespace TACTLib.Core.Product.MNDX
{
    internal struct NAME_FRAG
    {
        public int ItemIndex;   // Back index to various tables
        public int NextIndex;   // The following item index
        public int FragOffs;    // Higher 24 bits are 0xFFFFFF00 --> A single matching character
                                // Otherwise --> Offset to the name fragment table
    }
}
