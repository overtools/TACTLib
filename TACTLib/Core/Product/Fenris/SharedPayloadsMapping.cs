using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
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
        
        Span<byte> header = stackalloc byte[8];
        if (stream.Read(header) != 8) {
            throw new DataException();
        }

        if (BinaryPrimitives.ReadUInt32LittleEndian(header) != 0xABBA0003) {
            throw new InvalidDataException("Not an CoreTOCSharedPayloadsMapping.dat file");
        }

        var count = BinaryPrimitives.ReadUInt32LittleEndian(header[4..]);
        if (count > 0) {
            var entries = new uint[count * 2];
            var buffer = MemoryMarshal.AsBytes(entries.AsSpan());
            var offset = 0;
            while (offset < buffer.Length) {
                var read = stream.Read(buffer[offset..]);
                if (read == 0) {
                    break;
                }

                offset += read;
            }

            for (var i = 0; i < count; ++i) {
                SharedPayloads[entries[i << 1]] = entries[(i << 1) + 1];
            }
        }
        
        if (stream.Read(header[..4]) != 4) {
            throw new DataException();
        }

        count = BinaryPrimitives.ReadUInt32LittleEndian(header);
        if (count > 0) {
            var entries = new SnoChild[count * 2];
            var buffer = MemoryMarshal.AsBytes(entries.AsSpan());
            var offset = 0;
            while (offset < buffer.Length) {
                var read = stream.Read(buffer[offset..]);
                if (read == 0) {
                    break;
                }

                offset += read;
            }

            for (var i = 0; i < count; ++i) {
                SharedChildren[entries[i << 1]] = entries[(i << 1) + 1];
            }
        }
    }
}
