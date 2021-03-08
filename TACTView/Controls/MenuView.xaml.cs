using System;
using System.Windows;

namespace TACTView.Controls {
    public partial class MenuView {
        public MenuView() {
            InitializeComponent();
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
            throw new NotImplementedException();
        }

        private void OpenInstall(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }
    }
}
