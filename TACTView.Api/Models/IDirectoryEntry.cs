using System.Collections.ObjectModel;

namespace TACTView.Api.Models {
    public interface IDirectoryEntry {
        public string Name { get; init; }
        public IDirectoryEntry? Parent { get; init; }
        public ObservableCollection<IDirectoryEntry> Children { get; init; }
        public object? CustomData { get; init; }
    }
}
