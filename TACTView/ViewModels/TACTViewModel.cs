using System.Collections.ObjectModel;
using TACTView.Api.Models;

namespace TACTView.ViewModels {
    internal class TACTViewModel {
        public ObservableCollection<IDirectoryEntry> RootDirectory { get; } = new();
        public ProgressViewModel Progress { get; } = new();
        public ModuleViewModel Modules { get; } = ModuleViewModel.Instance;
        public RegistryViewModel Registry { get; } = RegistryViewModel.Instance;
        public MenuViewModel Menu { get; } = new();
    }
}
