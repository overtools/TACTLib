using System.Windows;
using TACTLib;
using TACTView.Api.Models;
using TACTView.ViewModels;

namespace TACTView {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public MainWindow() {
            Logger.RegisterBasic();
            InitializeComponent();
        }

        private void UpdateSelectedItem(object sender, RoutedPropertyChangedEventArgs<object> e) {
            ((TACTViewModel) DataContext).SetDirectory((IDirectoryEntry) e.NewValue);
        }
    }
}
