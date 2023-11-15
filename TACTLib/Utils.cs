using System;
using System.Globalization;

namespace TACTLib {
    public static class Utils {
        public static byte[] StringToByteArray(string str) {
            str = str.Replace(" ", string.Empty);

            byte[] res = new byte[str.Length / 2];
            for (var i = 0; i < res.Length; ++i) res[i] = byte.Parse(str.AsSpan(i * 2, 2), NumberStyles.HexNumber);

            return res;
        }
    }
}