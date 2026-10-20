using System.Windows;
using System.Windows.Controls;
using Dompetku.ViewModels;
using Dompetku.Views.Dialogs;

namespace Dompetku.Views.UserControls
{
    public partial class TransactionsView : UserControl
    {
        public TransactionsView()
        {
            InitializeComponent();
        }

        private void OnAddTransactionClick(object sender, RoutedEventArgs e)
        {
            var dialog = new TransactionDialog
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && dialog.IsSaved)
            {
                if (DataContext is TransactionViewModel vm)
                {
                    vm.LoadFilterLookups();
                    vm.ApplyFilter();
                }

                // If parent window has MainViewModel, notify refresh
                if (Window.GetWindow(this)?.DataContext is MainViewModel mainVm)
                {
                    mainVm.RefreshAllData();
                }
            }
        }
    }
}
