using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Fenris;

public class CoreTOC {
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0xC)]
    public record struct TOCEntry {
        public SnoHandle Sno;
        public int NameOffset;
    }

    public int[] GroupCounts { get; }
    public int[] GroupOffsets { get; }
    public int[] GroupCounts2 { get; }
    public uint[] GroupFormats { get; }
    public uint PrimaryId { get; }
    public Dictionary<SnoHandle, string> Files { get; } = [];

    public CoreTOC(Stream? stream, EncryptedSnos encrypted) {
        using var _ = new PerfCounter("CoreTOC::cctor`Stream`EncryptedSnos");
        if (stream == null) {
            throw new ArgumentNullException(nameof(stream));
        }

        var magic = stream.Read<uint>();
        var isNew = magic == 0xBCDE6611;
        if (!isNew) {
            stream.Position = 0;
        }

        var count = stream.Read<int>();
        GroupCounts = stream.ReadArray<int>(count);
        GroupOffsets = stream.ReadArray<int>(count);
        GroupCounts2 = stream.ReadArray<int>(count);
        GroupFormats = isNew ? stream.ReadArray<uint>(count) : [];

        PrimaryId = stream.Read<uint>();

        var baseOffset = stream.Position;

        Span<byte> stringBuffer = stackalloc byte[0x1FF];
        for (var i = 0; i < count; ++i) {
            // I'm assuming that GroupCounts2 is the count but with replaced files included.
            Debug.Assert(GroupCounts[i] == GroupCounts2[i]);
            stream.Position = baseOffset + GroupOffsets[i];

            var stringOffset = baseOffset + GroupOffsets[i] + 0xC * GroupCounts[i];
            for (var j = 0; j < GroupCounts[i]; ++j) {
                var entry = stream.Read<TOCEntry>();

                if (encrypted.Lookup.ContainsKey(entry.Sno)) {
                    continue;
                }

                var tmp = stream.Position;
                stream.Position = stringOffset + entry.NameOffset;

                if (stream.Read(stringBuffer) > 0) { // todo: verify usage of stream.Read here
                    Files[entry.Sno] = Encoding.ASCII.GetString(stringBuffer[..stringBuffer.IndexOf((byte) 0)]);
                }

                stream.Position = tmp;
            }
        }
    }
}
