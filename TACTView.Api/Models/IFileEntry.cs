using JetBrains.Annotations;
using TACTLib;
using TACTLib.Container;

namespace TACTView.Api.Models {
    [PublicAPI]
    public interface IFileEntry : IDirectoryEntry {
        long Size { get; init; }
        Locale Locale { get; init; }
        CKey Key { get; init; }
    }
}
