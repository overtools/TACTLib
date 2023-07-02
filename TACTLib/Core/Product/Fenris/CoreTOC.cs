using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Fenris;

public class CoreTOC {
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0xC)]
    public struct TOCEntry {
        public SnoHandle Sno;
        public int NameOffset;
    }

    public int[] GroupCounts { get; }
    public int[] GroupOffsets { get; }
    public int[] GroupCounts2 { get; }
    public uint PrimaryId { get; }
    public Dictionary<SnoHandle, string> Files { get; } = new();

    public CoreTOC(Stream? stream, EncryptedSnos? encrypted) {
        using var _ = new PerfCounter("CoreTOC::cctor`Stream`EncryptedSnos");
        if (stream == null) {
            throw new ArgumentNullException(nameof(stream));
        }

        Span<byte> stackBuffer = stackalloc byte[0x4];
        if (stream.Read(stackBuffer) != 4) {
            throw new InvalidDataException();
        }

        var count = BinaryPrimitives.ReadUInt32LittleEndian(stackBuffer);
        GroupCounts = new int[count];
        GroupOffsets = new int[count];
        GroupCounts2 = new int[count];

        var buffer = MemoryMarshal.AsBytes(GroupCounts.AsSpan());
        var offset = 0;
        while (offset < buffer.Length) {
            var read = stream.Read(buffer[offset..]);
            if (read == 0) {
                break;
            }

            offset += read;
        }

        buffer = MemoryMarshal.AsBytes(GroupOffsets.AsSpan());
        offset = 0;
        while (offset < buffer.Length) {
            var read = stream.Read(buffer[offset..]);
            if (read == 0) {
                break;
            }

            offset += read;
        }

        buffer = MemoryMarshal.AsBytes(GroupCounts2.AsSpan());
        offset = 0;
        while (offset < buffer.Length) {
            var read = stream.Read(buffer[offset..]);
            if (read == 0) {
                break;
            }

            offset += read;
        }

        if (stream.Read(stackBuffer) != 4) {
            throw new InvalidDataException();
        }

        PrimaryId = BinaryPrimitives.ReadUInt32LittleEndian(stackBuffer);

        var baseOffset = stream.Position;

        stackBuffer = stackalloc byte[0xC];
        Span<byte> stringBuffer = stackalloc byte[0xFF];
        for (var i = 0; i < count; ++i) {
            // I'm assuming that GroupCounts2 is the count but with replaced files included.
            Debug.Assert(GroupCounts[i] == GroupCounts2[i], "GroupCounts[i] == GroupCounts2[i]");
            stream.Position = baseOffset + GroupOffsets[i];

            var stringOffset = baseOffset + GroupOffsets[i] + 0xC * GroupCounts[i];
            for (var j = 0; j < GroupCounts[i]; ++j) {
                if (stream.Read(stackBuffer) != 0xC) {
                    throw new InvalidDataException();
                }
                var entry = MemoryMarshal.Read<TOCEntry>(stackBuffer);

                if (encrypted?.Lookup.ContainsKey(entry.Sno) == true) {
                    // todo: implement EncryptedNameDict, encrypted names are replaced with spaces and stored in that file.
                    continue;
                }

                var tmp = stream.Position;
                stream.Position = stringOffset + entry.NameOffset;

                if (stream.Read(stringBuffer) > 0) {
                    Files[entry.Sno] = Encoding.ASCII.GetString(stringBuffer[..stringBuffer.IndexOf((byte) 0)]);
                }

                stream.Position = tmp;
            }
        }
    }
}
