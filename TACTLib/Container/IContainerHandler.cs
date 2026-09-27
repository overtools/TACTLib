using System;

namespace TACTLib.Container {
    public interface IContainerHandler {
        ArraySegment<byte>? OpenEKey(FullEKey ekey, int eSize, string? meta = null);
        bool CheckResidency(FullEKey ekey, string? meta = null);
    }
}
