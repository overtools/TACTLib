using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Fenris;

public class ReplacedSnos {
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
    public struct ReplacedSno {
        public SnoHandle Sno;
        public ulong Hash; // ??
        public SnoHandle ReplacementSno;
    }
    
    public ReplacedSno[] Entries { get; }
    public Dictionary<uint, uint> Lookup = new();

    public ReplacedSnos(Stream? stream) {
        using var _ = new PerfCounter("ReplacedSnos::cctor`Stream");
        if (stream == null) {
            Entries = Array.Empty<ReplacedSno>();
            return;
        }
        
        Span<byte> header = stackalloc byte[8];
        if (stream.Read(header) != 8) {
            throw new DataException();
        }

        if (BinaryPrimitives.ReadUInt32LittleEndian(header) != 0xABBA0003) {
            throw new InvalidDataException("Not an CoreTOCReplacedSNOs.dat file");
        }

        var count = BinaryPrimitives.ReadUInt32LittleEndian(header[4..]);
        if (count == 0) {
            Entries = Array.Empty<ReplacedSno>();
        }

        Entries = new ReplacedSno[count];
        var buffer = MemoryMarshal.AsBytes(Entries.AsSpan());
        var offset = 0;
        while (offset < buffer.Length) {
            var read = stream.Read(buffer[offset..]);
            if (read == 0) {
                break;
            }

            offset += read;
        }

        Lookup = Entries.ToDictionary(x => x.Sno.Id, x => x.ReplacementSno.Id);
    }
}
