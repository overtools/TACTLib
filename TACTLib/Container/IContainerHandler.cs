using System;

namespace TACTLib.Container {
    public interface IContainerHandler {
        ArraySegment<byte>? OpenEKey(FullEKey ekey, int eSize);
        bool CheckResidency(FullEKey ekey);
    }
}
