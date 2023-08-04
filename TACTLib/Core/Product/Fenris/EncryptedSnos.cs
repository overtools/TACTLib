using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TACTLib.Core.VFS;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Fenris;

public class EncryptedSnos {
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
    public struct EncryptedSno {
        public SnoHandle Sno;
        public ulong KeyId;
    }

    public EncryptedSno[] Entries { get; }
    public Dictionary<SnoHandle, ulong> Lookup = new();
    public Dictionary<ulong, EncryptedNameDict?> NameDicts = new();

    public EncryptedSnos(Stream? stream, VFSFileTree vfs) {
        using var _ = new PerfCounter("EncryptedSnos::cctor`Stream");
        if (stream == null) {
            Entries = Array.Empty<EncryptedSno>();
            return;
        }

        Span<byte> header = stackalloc byte[8];
        if (stream.Read(header) != 8) {
            throw new DataException();
        }

        if (BinaryPrimitives.ReadUInt32LittleEndian(header) != 0x4CBF334D) {
            throw new InvalidDataException("Not an EncryptedSNOs.dat file");
        }

        var count = BinaryPrimitives.ReadUInt32LittleEndian(header[4..]);
        if (count == 0) {
            Entries = Array.Empty<EncryptedSno>();
            return;
        }

        Entries = new EncryptedSno[count];
        var buffer = MemoryMarshal.AsBytes(Entries.AsSpan());
        var offset = 0;
        while (offset < buffer.Length) {
            var read = stream.Read(buffer[offset..]);
            if (read == 0) {
                break;
            }

            offset += read;
        }

        foreach (var entry in Entries) {
            Lookup[entry.Sno] = entry.KeyId;

            if (!NameDicts.ContainsKey(entry.KeyId)) {
                EncryptedNameDict? nameDict = null;
                try {
                    using var dict = vfs.Open($"Base\\EncryptedNameDict-0x{entry.KeyId}.dat");
                    if (dict != null) {
                        nameDict = new EncryptedNameDict(dict);
                    }
                } catch {
                    // ignored
                } finally {
                    NameDicts[entry.KeyId] = nameDict;
                }
            }
        }

        Lookup = Entries.ToDictionary(x => x.Sno, x => x.KeyId);
    }
}
