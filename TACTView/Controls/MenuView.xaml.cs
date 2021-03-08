using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using TACTLib;
using TACTLib.Client;
using TACTView.Models;
using TACTView.ViewModels;

namespace TACTView.Controls {
    public partial class MenuView {
        public static readonly DependencyProperty TACTProperty = DependencyProperty.Register("TACT", typeof(TACTViewModel), typeof(MenuView), new PropertyMetadata(default(TACTViewModel)));

        public MenuView() {
            InitializeComponent();
        }

        public TACTViewModel TACT {
            get => (TACTViewModel) GetValue(TACTProperty);
            set => SetValue(TACTProperty, value);
        }


        private void OpenCASC(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void OpenNGDP(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void Exit(object sender, RoutedEventArgs e) {
            Application.Current.MainWindow?.Close();
            Environment.Exit(0);
        }

        private void OpenRecent(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }

        private void OpenFlavor(object sender, RoutedEventArgs e) {
            var item = sender as MenuItem;
            var info = item?.DataContext as FlavoredCASCEntry;
            if (info == null) return;

            var config = new ClientCreateArgs {
                Online = true,
                OnlineProduct = info.DetectedProduct,
                Flavor = string.IsNullOrEmpty(info.Flavor) ? "" : info.Flavor.Substring(1, info.Flavor.Length - 2),
                SubDirectory = info.Flavor,
                Product = info.Code
            };

            Load(info.Directory, config);
        }

        private void OpenInstall(object sender, RoutedEventArgs e) {
            var item = sender as MenuItem;
            var info = item?.DataContext as CASCEntry;
            if (info == null || string.IsNullOrEmpty(info.Directory) || info.Flavors.Count > 0) return;

            var config = new ClientCreateArgs {
                Online = true,
                OnlineProduct = info.DetectedProduct,
                Flavor = "",
                SubDirectory = "",
                Product = info.Code
            };

            Load(info.Directory, config);
        }

        private void Load(string path, ClientCreateArgs config) {
            IsEnabled = false;
            TACT.Progress.Report(-1, 1, 0, $"Loading {config.OnlineProduct:G}");
            var tact = TACT;
            new Thread(() => {
                tact.ClientHandler = new ClientHandler(path, config);
                Dispatcher.Invoke(InvokePropertyConnector);
            }).Start();
        }

        private void InvokePropertyConnector() {
            if (TACT.ClientHandler == null) return;
            if (!TACT.Registry.HasProductConnector(TACT.ClientHandler.Product)) return;
            var services = TACT.BuildServiceCollection();
            var tact = TACT;
            var root = new DirectoryEntry(ProductHelpers.HumanReadableProduct(TACT.ClientHandler.Product));
            new Thread(() => {
                tact.Registry.GetProductConnector(tact.ClientHandler.Product, services)?.GetEntries(root);
                Dispatcher.Invoke(() => {
                    TACT.UpdateTree(root);
                    IsEnabled = true;
                });
            }).Start();
        }
    }
}
