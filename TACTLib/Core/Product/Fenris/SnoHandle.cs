using System.Runtime.InteropServices;

namespace TACTLib.Core.Product.Fenris;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x8)]
public record struct SnoHandle {
	public static SnoHandle Invalid { get; } = new() { Group = uint.MaxValue };

	public uint Group { get; set; } // 0xFFFFFFFF if none
	public uint Id { get; set; } // 0 if none
	public bool IsInvalid => Group == uint.MaxValue && Id == 0;
}
