using System;

namespace TACTLib {
    public static class Utils {
        public static unsafe Span<byte> PtrToSpan(byte* ptr, int len) {
            return new Span<byte>(ptr, len);
        }

        // ReSharper disable once InconsistentNaming
        public static unsafe int Int32FromPtrBE(byte* ptr) {
            return ptr[3] | (ptr[2] << 8) | (ptr[1] << 16) | (ptr[0] << 24);
        }
        
        // ReSharper disable once InconsistentNaming
        public static unsafe short Int16FromPtrBE(byte* ptr) {
            return (short)(ptr[1] | (ptr[0] << 8));
        }
        
        public static byte[] StringToByteArray(string str) {
            str = str.Replace(" ", string.Empty);

            byte[] res = new byte[str.Length / 2];
            for (int i = 0; i < res.Length; ++i) res[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);

            return res;
        }
    }
}