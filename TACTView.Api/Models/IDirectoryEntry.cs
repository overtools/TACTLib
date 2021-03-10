using System.Collections.ObjectModel;
using JetBrains.Annotations;
using TACTLib;
using TACTLib.Container;

namespace TACTView.Api.Models {
    [PublicAPI]
    public interface IDirectoryEntry {
        string Name { get; init; }
        long Size { get; init; }
        Locale Locale { get; init; }
        CKey Key { get; init; }
        bool IsDirectory { get; init; }
        IDirectoryEntry? Parent { get; init; }
        ObservableCollection<IDirectoryEntry> Children { get; init; }
        object? CustomData { get; init; }

        IDirectoryEntry CreateDirectory(string name);
        IDirectoryEntry CreateFile(string name, Locale locale, CKey key, long size, object? customData = null);
    }
}
