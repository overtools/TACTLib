using System;

namespace TACTLib.Exceptions {
    /// <summary>Thrown when the BLTE reader is missing a key</summary>
    [Serializable]
    public class BLTEKeyException : Exception {
        public ulong MissingKey;

        public BLTEKeyException(ulong key) : base($"unknown key {key:X16}") {
            MissingKey = key;
        }
    }
}
