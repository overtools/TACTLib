using System.Runtime.InteropServices;

namespace TACTLib.Core.Product.Fenris;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x8)]
public record struct SnoChild {
    public uint SnoId;
    public uint SubId;
}
