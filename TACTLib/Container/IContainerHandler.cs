using System;

namespace TACTLib.Container {
    public interface IContainerHandler {
        ArraySegment<byte>? OpenEKey(FullEKey ekey, int eSize);
        ArraySegment<byte>? OpenEKey(FullEKey ekey);
        bool CheckResidency(FullEKey ekey);
    }
}
