using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Fenris;

public class ReplacedSnos {
	public Dictionary<uint, uint> Lookup = [];

	public ReplacedSnos(Stream? stream) {
		using var _ = new PerfCounter("ReplacedSnos::cctor`Stream");
		if (stream == null) {
			Entries = [];
			return;
		}

		var magic = stream.Read<uint>();
		if (magic != 0xABBA0003) {
			throw new InvalidDataException("Not an CoreTOCReplacedSNOs.dat file");
		}

		var count = stream.Read<int>();
		Entries = stream.ReadArray<ReplacedSno>(count);
		Lookup = Entries.ToDictionary(x => x.Sno.Id, x => x.ReplacementSno.Id);
	}

	public ReplacedSno[] Entries { get; }

	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
	public record struct ReplacedSno {
		public SnoHandle Sno { get; set; }
		public ulong Hash { get; set; } // ??
		public SnoHandle ReplacementSno { get; set; }
	}
}
