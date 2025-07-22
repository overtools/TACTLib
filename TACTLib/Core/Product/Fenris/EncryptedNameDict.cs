using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Fenris;

public class EncryptedNameDict {
    public Dictionary<SnoHandle, string> Files { get; } = [];

    public EncryptedNameDict(Stream stream) {
        var magic = stream.Read<uint>();
        if (magic != 0xABCD4567) {
            throw new InvalidDataException("Not an EncryptedNameDict");
        }

        var count = stream.Read<int>();
        var present = stream.ReadArray<SnoHandle>(count);

        Span<byte> stringBuffer = stackalloc byte[0xFF];
        foreach (var handle in present)
        {
            var tmp = stream.Position;

            if (stream.Read(stringBuffer) > 0) { // todo: verify usage of stream.Read here
                Files[handle] = Encoding.ASCII.GetString(stringBuffer[..stringBuffer.IndexOf((byte) 0)]);
                stream.Position = tmp + stringBuffer.IndexOf((byte) 0) + 1;
            }
        }
    }
}
