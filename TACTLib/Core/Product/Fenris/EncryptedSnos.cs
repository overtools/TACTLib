using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Fenris;

public class EncryptedSnos {
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
    public record struct EncryptedSno {
        public SnoHandle Sno;
        public ulong KeyId;
    }

    public EncryptedSno[] Entries { get; }
    public Dictionary<SnoHandle, ulong> Lookup = [];

    public EncryptedSnos(Stream? stream) {
        using var _ = new PerfCounter("EncryptedSnos::cctor`Stream");
        if (stream == null) {
            Entries = [];
            return;
        }

        var magic = stream.Read<uint>();
        if (magic != 0x4CBF334D) {
            throw new InvalidDataException("Not an EncryptedSNOs.dat file");
        }

        var count = stream.Read<int>();
        Entries = stream.ReadArray<EncryptedSno>(count);
        Lookup = Entries.ToDictionary(x => x.Sno, x => x.KeyId);
    }
}
