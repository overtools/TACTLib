using System.Collections.Generic;
using System.IO;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Fenris;

// Both for convenience, and also as a priority order.
public enum SnoManifestRole {
	Base,
	Core,
	Speech,
	Text,
}

public class SnoManifest {
	public SnoManifest(Stream? stream) {
		// used to assist lookups, I think.
		using var _ = new PerfCounter("SnoManifest::cctor`Stream");
		if (stream == null) {
			SnoIds = [];
			ChildIds = [];
			SnoIds2 = [];
			ChildIds2 = [];
			return;
		}

		var magic = stream.Read<uint>();
		if (magic != 0xEAF1FE90) {
			throw new InvalidDataException("Not a SNO Manifest file");
		}

		var count = stream.Read<int>();
		var snoIds = stream.ReadArray<uint>(count);
		SnoIds = [.. snoIds];

		count = stream.Read<int>();
		var childIds = stream.ReadArray<SnoChild>(count);
		ChildIds = [.. childIds];

		if (stream.Position == stream.Length) {
			SnoIds2 = [];
			ChildIds2 = [];
			return;
		}

		count = stream.Read<int>();
		var childIds2 = stream.ReadArray<SnoChild>(count);
		ChildIds2 = [.. childIds2];

		count = stream.Read<int>();
		var snoIds2 = stream.ReadArray<uint>(count);
		SnoIds2 = [.. snoIds2];
	}

	public Locale Locale { get; set; }
	public SnoManifestRole Role { get; set; }
	public HashSet<uint> SnoIds { get; }
	public HashSet<uint> SnoIds2 { get; }
	public HashSet<SnoChild> ChildIds { get; }
	public HashSet<SnoChild> ChildIds2 { get; }

	public bool ContainsChild(uint id, uint subId) =>
		ChildIds.Contains(new SnoChild {
			SnoId = id,
			SubId = subId,
		}) || ChildIds2.Contains(new SnoChild {
			SnoId = id,
			SubId = subId,
		});

	public bool Contains(uint id) => SnoIds.Contains(id) || SnoIds2.Contains(id);
}
