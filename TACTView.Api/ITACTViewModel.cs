using System.ComponentModel;
using JetBrains.Annotations;
using TACTLib.Core.Product;
using TACTView.Api.Models;

namespace TACTView.Api {
    [PublicAPI]
    public interface ITACTViewModel : INotifyPropertyChanged {
        public IProductHandler? ProductHandler { get; }
        public IDirectoryEntry? CurrentDirectory { get; }
    }
}
