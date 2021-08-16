using System;

namespace TACTLib.Exceptions {
    /// <summary>Thrown when Container Handler catastrophically fails</summary>
    [Serializable]
    public class TankException : Exception {
        public TankException(string message) : base(message) { }
        public TankException(string message, Exception inner) : base(message, inner) { }
    }
}
