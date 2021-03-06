using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace TACTView.Api.Models {
    [PublicAPI]
    public interface IDirectoryEntry {
        string Name { get; init; }
        IDirectoryEntry? Parent { get; init; }
        ObservableCollection<IDirectoryEntry> Children { get; init; }
        object? CustomData { get; init; }
    }
}
