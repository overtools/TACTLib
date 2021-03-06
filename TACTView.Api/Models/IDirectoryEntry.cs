using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace TACTView.Api.Models {
    [PublicAPI]
    public interface IDirectoryEntry {
        public string Name { get; init; }
        public IDirectoryEntry? Parent { get; init; }
        public ObservableCollection<IDirectoryEntry> Children { get; init; }
        public object? CustomData { get; init; }
    }
}
