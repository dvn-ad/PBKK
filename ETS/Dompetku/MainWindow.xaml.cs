using System.Windows;
using Dompetku.ViewModels;
using Dompetku.Views;
using Dompetku.Views.Dialogs;

namespace Dompetku
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            _viewModel.RequestLogout += OnRequestLogout;
            _viewModel.RequestOpenQuickAddTransaction += OnOpenQuickAddTransaction;
            _viewModel.RequestOpenQuickTransfer += OnOpenQuickTransfer;
        }

        private void OnRequestLogout()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void OnOpenQuickAddTransaction()
        {
            var dialog = new TransactionDialog
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true && dialog.IsSaved)
            {
                _viewModel.RefreshAllData();
            }
        }

        private void OnOpenQuickTransfer()
        {
            var dialog = new TransferDialog
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true && dialog.IsSaved)
            {
                _viewModel.RefreshAllData();
            }
        }
    }
}
