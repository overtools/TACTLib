using System.Runtime.InteropServices;

namespace TACTLib.Core.Product.Fenris;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x8)]
public struct SnoHandle {
    public uint Group; // 0xFFFFFFFF if none
    public uint Id; // 0 if none
}