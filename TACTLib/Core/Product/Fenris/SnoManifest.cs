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

    public Locale Locale { get; set; }
    public SnoManifestRole Role { get; set; }
    public HashSet<uint> SnoIds { get; }
    public HashSet<SnoChild> ChildIds { get; }

    public SnoManifest(Stream? stream) { // used to assist lookups, i think.
        using var _ = new PerfCounter("SnoManifest::cctor`Stream");
        if (stream == null) {
            SnoIds = new HashSet<uint>();
            ChildIds = new HashSet<SnoChild>();
            return;
        }

        var magic = stream.Read<uint>();
        if (magic != 0xEAF1FE90) {
            throw new InvalidDataException("Not a SNO Manifest file");
        }

        var count = stream.Read<int>();
        var snoIds = stream.ReadArray<uint>(count);
        SnoIds = new HashSet<uint>(snoIds);

        count = stream.Read<int>();
        var childIds = stream.ReadArray<SnoChild>(count);
        ChildIds = new HashSet<SnoChild>(childIds);
    }

    public bool ContainsChild(uint id, uint subId) =>
        ChildIds.Contains(new SnoChild() {
            SnoId = id,
            SubId = subId,
        });

    public bool Contains(uint id) => SnoIds.Contains(id);
}
