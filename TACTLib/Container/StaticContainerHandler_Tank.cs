using TACTLib.Client;

namespace TACTLib.Container;

[StaticContainerHandler(TACTProduct.Overwatch)]
public class StaticContainerHandler_Tank : StaticContainerHandler {
    public StaticContainerHandler_Tank(ClientHandler client) : base(client) { }

    protected override string GetFileName(ulong chunk, ulong archive, int index = 0) => $"data.{chunk:D3}.{archive:D3}";
    protected override string ContainerDirectory => "data";
}
