using System;
using System.IO;

namespace TACTLib {
    public static class Utils {
        public static unsafe Span<byte> PtrToSpan(byte* ptr, int len) {
            return new Span<byte>(ptr, len);
        }

        // ReSharper disable once InconsistentNaming
        public static unsafe int Int32FromPtrBE(byte* ptr) {
            return Int32FromSpanBE(PtrToSpan(ptr, 4));
        }
        
        // ReSharper disable once InconsistentNaming
        public static unsafe int Int16FromPtrBE(byte* ptr) {
            return Int16FromSpanBE(PtrToSpan(ptr, 2));
        }

        // ReSharper disable once InconsistentNaming
        public static int Int32FromSpanBE(Span<byte> val, int start=0) {
            return val[3+start] | (val[2+start] << 8) | (val[1+start] << 16) | (val[0+start] << 24);
        }
        
        // ReSharper disable once InconsistentNaming
        public static short Int16FromSpanBE(Span<byte> val, int start=0) {
            return (short)(val[start + 1] | (val[start] << 8));
        }
        
        public static byte[] StringToByteArray(string str) {
            str = str.Replace(" ", string.Empty);

            byte[] res = new byte[str.Length / 2];
            for (int i = 0; i < res.Length; ++i) res[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);

            return res;
        }
    }
}