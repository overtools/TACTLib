using System;
using TACTLib.Container;

namespace TACTLib.Exceptions {
    /// <summary>Thrown when Container Handler catastrophically fails</summary>
    [Serializable]
    public class CASCException : Exception {
        public ContainerHandler.IndexEntry Index;
        
        public CASCException(ContainerHandler.IndexEntry index, string message) : base(message) {
            Index = index;
        }
        
        public CASCException(ContainerHandler.IndexEntry index, string message, Exception inner) : base(message, inner) {
            Index = index;
        }
    }
}
