using System;

namespace TACTLib {
    public static class Utils {
        public static unsafe Span<byte> PtrToSpan(byte* ptr, int len) {
            return new Span<byte>(ptr, len);
        }

        // ReSharper disable once InconsistentNaming
        public static unsafe int Int32FromPtrBE(byte* ptr, int start=0) {
            return ptr[3+start] | (ptr[2+start] << 8) | (ptr[1+start] << 16) | (ptr[0+start] << 24);
        }
        
        // ReSharper disable once InconsistentNaming
        public static unsafe short Int16FromPtrBE(byte* ptr, int start=0) {
            return (short)(ptr[start + 1] | (ptr[start] << 8));
        }
        
        public static byte[] StringToByteArray(string str) {
            str = str.Replace(" ", string.Empty);

            byte[] res = new byte[str.Length / 2];
            for (int i = 0; i < res.Length; ++i) res[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);

            return res;
        }
    }
}