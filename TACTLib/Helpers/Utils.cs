using System;
using System.Runtime.InteropServices;

namespace TACTLib.Helpers {
    public static class Utils {
        public static unsafe byte[] PtrToByteArray(byte* ptr, int len) {
            byte[] buf = new byte[len];
            Marshal.Copy((IntPtr)ptr, buf, 0, len);
            return buf;
        }

        // ReSharper disable once InconsistentNaming
        public static unsafe int Int32FromPtrBE(byte* ptr) {
            return Int32FromByteArrayBE(PtrToByteArray(ptr, 4));
        }
        
        // ReSharper disable once InconsistentNaming
        public static unsafe int Int16FromPtrBE(byte* ptr) {
            return Int16FromByteArrayBE(PtrToByteArray(ptr, 2));
        }

        // ReSharper disable once InconsistentNaming
        public static int Int32FromByteArrayBE(byte[] val, int start=0) {
            return val[3+start] | (val[2+start] << 8) | (val[1+start] << 16) | (val[0+start] << 24);
        }
        
        // ReSharper disable once InconsistentNaming
        public static short Int16FromByteArrayBE(byte[] val, int start=0) {
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