using System;
using System.Collections.Generic;
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
        
        var magic = stream.Read<uint>();
        if (magic != 0xABBA0003) {
            throw new InvalidDataException("Not an CoreTOCReplacedSNOs.dat file");
        }

        var count = stream.Read<int>();
        Entries = stream.ReadArray<ReplacedSno>(count);
        Lookup = Entries.ToDictionary(x => x.Sno.Id, x => x.ReplacementSno.Id);
    }
}
