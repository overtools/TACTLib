using System;

namespace TACTLib.Exceptions {
    /// <summary>Thrown when we don't support specific build version</summary>
    [Serializable]
    public class UnsupportedBuildVersionException : Exception {
        public UnsupportedBuildVersionException(string message) : base(message) { }
        public UnsupportedBuildVersionException(string message, Exception inner) : base(message, inner) { }
    }
}