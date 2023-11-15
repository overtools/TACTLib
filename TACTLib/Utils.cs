﻿using System;
using System.Buffers.Binary;
using System.Globalization;

namespace TACTLib {
    public static class Utils {
        // ReSharper disable once InconsistentNaming
        public static unsafe int Int32FromPtrBE(byte* ptr)
        {
            // todo: please delete
            return BinaryPrimitives.ReadInt32BigEndian(new ReadOnlySpan<byte>(ptr, 4));
        }
        
        // ReSharper disable once InconsistentNaming
        public static unsafe short Int16FromPtrBE(byte* ptr)
        {
            // todo: please delete
            return BinaryPrimitives.ReadInt16BigEndian(new ReadOnlySpan<byte>(ptr, 2));
        }
        
        public static byte[] StringToByteArray(string str) {
            str = str.Replace(" ", string.Empty);

            byte[] res = new byte[str.Length / 2];
            for (var i = 0; i < res.Length; ++i) res[i] = byte.Parse(str.AsSpan(i * 2, 2), NumberStyles.HexNumber);

            return res;
        }
    }
}