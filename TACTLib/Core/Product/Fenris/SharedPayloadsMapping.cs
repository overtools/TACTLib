using System.Collections.Generic;
using System.IO;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Fenris;

public class SharedPayloadsMapping {
    public Dictionary<uint, uint> SharedPayloads { get; set; } = new();
    public Dictionary<SnoChild, SnoChild> SharedChildren { get; set; } = new();

    public SharedPayloadsMapping(Stream? stream) {
        using var _ = new PerfCounter("SharedPayloadsMapping::cctor`Stream");
        if (stream == null) {
            return;
        }

        var magic = stream.Read<uint>();
        if (magic != 0xABBA0003) {
            throw new InvalidDataException("Not an CoreTOCSharedPayloadsMapping.dat file");
        }

        var count = stream.Read<uint>();
        {
            var entries = stream.ReadArray<uint>((int)count * 2);

            for (var i = 0; i < count; ++i) {
                SharedPayloads[entries[i << 1]] = entries[(i << 1) + 1];
            }
        }

        count = stream.Read<uint>();
        {
            var entries = stream.ReadArray<SnoChild>((int)count * 2);

            for (var i = 0; i < count; ++i) {
                SharedChildren[entries[i << 1]] = entries[(i << 1) + 1];
            }
        }
    }
}
