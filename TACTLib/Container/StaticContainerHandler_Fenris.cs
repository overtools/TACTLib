using System.Globalization;
using TACTLib.Client;

namespace TACTLib.Container;

[StaticContainerHandler(TACTProduct.Diablo4)]
public class StaticContainerHandler_Fenris : StaticContainerHandler {
    public StaticContainerHandler_Fenris(ClientHandler client) : base(client) { }

    protected override string GetFileName(ulong chunk, ulong archive, int index = 0) => index switch {
        1 => $"{chunk:D3}/{archive >> 16}-{GetSnoType(archive & 0xffff)}.dat",
        2 => $"{chunk:D3}/{archive}-child.dat",
        3 => $"{chunk:D3}/0x{archive:X04}-payload.dat",
        _ => $"{chunk:D3}/0x{archive:X04}-meta.dat",
    };

    private static string GetSnoType(ulong type) => type switch {
        0 => "payload",
        _ => (type - 1).ToString("D4", CultureInfo.InvariantCulture),
    };

    protected override string ContainerDirectory => "Data";
}
