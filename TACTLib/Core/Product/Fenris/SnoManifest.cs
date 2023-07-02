using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
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

        Span<byte> header = stackalloc byte[8];
        if (stream.Read(header) != 8) {
            throw new DataException();
        }

        if (BinaryPrimitives.ReadUInt32LittleEndian(header) != 0xEAF1FE90) {
            throw new InvalidDataException("Not a SNO Manifest file");
        }

        var count = BinaryPrimitives.ReadUInt32LittleEndian(header[4..]);
        if (count > 0) {
            var snoIds = new uint[count];
            var buffer = MemoryMarshal.AsBytes(snoIds.AsSpan());
            var offset = 0;
            while (offset < buffer.Length) {
                var read = stream.Read(buffer[offset..]);
                if (read == 0) {
                    break;
                }

                offset += read;
            }

            SnoIds = new HashSet<uint>(snoIds);
        } else {
            SnoIds = new HashSet<uint>();
        }

        if (stream.Read(header[..4]) != 4) {
            throw new DataException();
        }
        count = BinaryPrimitives.ReadUInt32LittleEndian(header);
        if (count > 0) {
            var childIds = new SnoChild[count];
            var buffer = MemoryMarshal.AsBytes(childIds.AsSpan());
            var offset = 0;
            while (offset < buffer.Length) {
                var read = stream.Read(buffer[offset..]);
                if (read == 0) {
                    break;
                }

                offset += read;
            }
            ChildIds = new HashSet<SnoChild>(childIds);
        } else {
            ChildIds = new HashSet<SnoChild>();
        }
    }

    public bool ContainsChild(uint id, uint subId) =>
        ChildIds.Contains(new SnoChild() {
            SnoId = id,
            SubId = subId,
        });

    public bool Contains(uint id) => SnoIds.Contains(id);
}
