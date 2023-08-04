using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TACTLib.Core.Product.Fenris;

public class EncryptedNameDict {
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
    public struct EncryptedSno {
        public SnoHandle Sno;
        public ulong KeyId;
    }
    public Dictionary<SnoHandle, string> Files { get; } = new();

    public EncryptedNameDict(Stream stream) {
        Span<uint> header = stackalloc uint[2];
        if (stream.Read(MemoryMarshal.AsBytes(header)) != 8) {
            throw new DataException();
        }

        if (header[0] != 0xABCD4567) {
            throw new InvalidDataException("Not an EncryptedNameDict");
        }

        var present = new SnoHandle[header[1]].AsSpan();
        var buffer = MemoryMarshal.AsBytes(present);
        var offset = 0;
        while (offset < buffer.Length) {
            var read = stream.Read(buffer[offset..]);
            if (read == 0) {
                break;
            }

            offset += read;
        }

        Span<byte> stringBuffer = stackalloc byte[0xFF];
        for (var i = 0; i < header[1]; ++i) {
            var tmp = stream.Position;

            if (stream.Read(stringBuffer) > 0) {
                Files[present[i]] = Encoding.ASCII.GetString(stringBuffer[..stringBuffer.IndexOf((byte) 0)]);
                stream.Position = tmp + stringBuffer.IndexOf((byte) 0) + 1;
            }
        }
    }
}
