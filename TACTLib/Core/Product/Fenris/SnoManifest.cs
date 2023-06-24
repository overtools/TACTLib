using System;
using System.Buffers.Binary;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Fenris;

public class SnoManifest {
    public uint[] SnoIds { get; }
    public SnoChild[] ChildIds { get; }

    public SnoManifest(Stream? stream) { // used to assist lookups, i think.
        using var _ = new PerfCounter("SnoManifest::cctor`Stream");
        if (stream == null) {
            SnoIds = Array.Empty<uint>();
            ChildIds = Array.Empty<SnoChild>();
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
            SnoIds = new uint[count];
            var buffer = MemoryMarshal.AsBytes(SnoIds.AsSpan());
            var offset = 0;
            while (offset < buffer.Length) {
                var read = stream.Read(buffer[offset..]);
                if (read == 0) {
                    break;
                }

                offset += read;
            }
        } else {
            SnoIds = Array.Empty<uint>();
        }

        if (stream.Read(header[..4]) != 4) {
            throw new DataException();
        }
        count = BinaryPrimitives.ReadUInt32LittleEndian(header);
        if (count > 0) {
            ChildIds = new SnoChild[count];
            var buffer = MemoryMarshal.AsBytes(ChildIds.AsSpan());
            var offset = 0;
            while (offset < buffer.Length) {
                var read = stream.Read(buffer[offset..]);
                if (read == 0) {
                    break;
                }

                offset += read;
            }
        } else {
            ChildIds = Array.Empty<SnoChild>();
        }
    }
}
