using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using DragonLib;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TACTLib.Client;
using TACTLib.Core.Product;
using TACTView.Api;
using TACTView.Api.Models;
using TACTView.Api.Registry;
using TACTView.Controls;

namespace TACTView.ViewModels {
    public sealed class TACTViewModel : Singleton<TACTViewModel>, ITACTViewModel {
        public TACTViewModel() {
            Tabs = new ObservableCollection<TabItem> {
                new TACTFileView(this)
            };
        }

        public ObservableCollection<IDirectoryEntry> RootDirectory { get; init; } = new();
        public ProgressViewModel Progress { get; } = new();
        public ModuleViewModel Modules { get; } = ModuleViewModel.Instance;
        public RegistryViewModel Registry { get; } = RegistryViewModel.Instance;
        public MenuViewModel Menu { get; } = new();
        public ClientHandler? ClientHandler { get; set; }
        public ObservableCollection<TabItem> Tabs { get; set; }
        public IProductHandler? ProductHandler => ClientHandler?.ProductHandler;
        public IDirectoryEntry? CurrentDirectory { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public IServiceCollection BuildServiceCollection() {
            var collection = new ServiceCollection()
                             .AddSingleton<IProgressReporter>(Progress)
                             .AddSingleton<IRegistry<IFileHandler>>(Registry)
                             .AddSingleton<IRegistry<IProductConnector>>(Registry)
                             .AddSingleton<IRegistry<IPlugin>>(Registry)
                             .AddSingleton<ITACTViewModel>(this)
                             .AddSingleton(Tabs);
            if (ProductHandler != null)
                collection.AddSingleton(ProductHandler)
                          .AddSingleton(ProductHandler.GetType(), ProductHandler);
            return collection;
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateTree(IDirectoryEntry directory) {
            RootDirectory.Clear();
            RootDirectory.Add(directory);
            OnPropertyChanged(nameof(RootDirectory));
            SetDirectory(directory);
        }

        public void SetDirectory(IDirectoryEntry directory) {
            CurrentDirectory = directory;
            OnPropertyChanged(nameof(CurrentDirectory));
        }
    }
}
