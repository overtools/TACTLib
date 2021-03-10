using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TACTView.Api;
using TACTView.Api.Models;

namespace TACTView.Controls {
    // implemented as a service.
    public sealed partial class TACTFileView : INotifyPropertyChanged {
        public TACTFileView(ITACTViewModel vm) {
            ViewModel = vm;
            ViewModel.PropertyChanged += (_, @event) => {
                if (@event.PropertyName == nameof(ITACTViewModel.CurrentDirectory)) OnPropertyChanged(nameof(Files));
            };

            InitializeComponent();
        }

        private ITACTViewModel ViewModel { get; }

        public ObservableCollection<IDirectoryEntry>? Files => ViewModel.CurrentDirectory?.Children;

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
