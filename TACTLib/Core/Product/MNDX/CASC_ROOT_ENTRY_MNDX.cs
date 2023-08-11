

// https://raw.githubusercontent.com/WoW-Tools/CascLib/master/CascLib/RootHandlers/MNDXRootHandler.cs

namespace TACTLib.Core.Product.MNDX
{
    internal class CASC_ROOT_ENTRY_MNDX
    {
        public CKey MD5;            // Encoding key for the file
        public int Flags;           // High 8 bits: Flags, low 24 bits: package index
        public int FileSize;        // Uncompressed file size, in bytes
        public CASC_ROOT_ENTRY_MNDX? Next;
    }
}
