using TACTLib.Client;

namespace TACTLib.Container;

[StaticContainerHandler(TACTProduct.Diablo2)]
public class StaticContainerHandler_Osi : StaticContainerHandler {
    public StaticContainerHandler_Osi(ClientHandler client) : base(client) { }

    protected override string GetFileName(ulong chunk, ulong archive, int index = 0) => $"{chunk:D2}-{archive:x08}.data";
    protected override string ContainerDirectory => "data";
}
