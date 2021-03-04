using System.Collections.ObjectModel;
using TACTView.Api.Models;

namespace TACTView.ViewModels {
    public class TACTViewModel {
        public ObservableCollection<IDirectoryEntry> RootDirectory { get; } = new();
    }
}
