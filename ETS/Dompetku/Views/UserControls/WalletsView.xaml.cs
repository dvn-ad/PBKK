using System.Windows;
using System.Windows.Controls;
using Dompetku.Models;
using Dompetku.ViewModels;
using Dompetku.Views.Dialogs;

namespace Dompetku.Views.UserControls
{
    public partial class WalletsView : UserControl
    {
        public WalletsView()
        {
            InitializeComponent();
        }

        private void OnAddWalletClick(object sender, RoutedEventArgs e)
        {
            var dialog = new WalletDialog
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && dialog.IsSaved)
            {
                if (DataContext is WalletViewModel vm) vm.LoadData();
                if (Window.GetWindow(this)?.DataContext is MainViewModel mainVm) mainVm.RefreshAllData();
            }
        }

        private void OnEditWalletClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Wallet w)
            {
                var dialog = new WalletDialog(w)
                {
                    Owner = Window.GetWindow(this)
                };

                if (dialog.ShowDialog() == true && dialog.IsSaved)
                {
                    if (DataContext is WalletViewModel vm) vm.LoadData();
                    if (Window.GetWindow(this)?.DataContext is MainViewModel mainVm) mainVm.RefreshAllData();
                }
            }
        }

        private void OnTransferClick(object sender, RoutedEventArgs e)
        {
            var dialog = new TransferDialog
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && dialog.IsSaved)
            {
                if (DataContext is WalletViewModel vm) vm.LoadData();
                if (Window.GetWindow(this)?.DataContext is MainViewModel mainVm) mainVm.RefreshAllData();
            }
        }
    }
}
