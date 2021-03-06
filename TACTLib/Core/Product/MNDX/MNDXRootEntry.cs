using TACTLib.Container;

// https://raw.githubusercontent.com/WoW-Tools/CascLib/master/CascLib/RootHandlers/MNDXRootHandler.cs

namespace TACTLib.Core.Product.MNDX
{
    public class MNDXRootEntry
    {
        public CKey Key;            // Encoding key for the file
        public int Flags;           // High 8 bits: Flags, low 24 bits: package index
        public int FileSize;        // Uncompressed file size, in bytes
        internal MNDXRootEntry Next;
    }
}
