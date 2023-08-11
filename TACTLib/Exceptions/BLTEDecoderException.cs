using System;

namespace TACTLib.Exceptions {
    /// <summary>Thrown when the BLTE reader encounters invalid data</summary>
    [Serializable]
    public class BLTEDecoderException : Exception {
        public readonly byte[]? BLTEData;

        public BLTEDecoderException(byte[]? data, string message) : base(message) {
            BLTEData = data;
        }

        public BLTEDecoderException(byte[]? data, string fmt, params object[] args) : this(data, string.Format(fmt, args)) { }

        public byte[]? GetBLTEData() => BLTEData;
    }
}
