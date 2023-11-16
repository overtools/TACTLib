using System;

namespace TACTLib {
    public static class Utils {
        public static byte[] StringToByteArray(ReadOnlySpan<char> str) {
            // todo: span
            return Convert.FromHexString(str);
        }
    }
}