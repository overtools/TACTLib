using System;

namespace TACTLib.Container {
    public interface IContainerHandler {
        ArraySegment<byte>? OpenEKey(CKey ekey, int eSize);
    }
}