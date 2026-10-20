using System;
using System.Linq;
using System.Windows;
using Dompetku.Data;
using Dompetku.Models;
using Dompetku.Services;

namespace Dompetku.Views.Dialogs
{
    public partial class TransactionDialog : Window
    {
        private readonly WalletRepository _walletRepo = new();
        private readonly CategoryRepository _categoryRepo = new();
        private readonly TransactionRepository _txRepo = new();
        private bool _isInitialized;

        public bool IsSaved { get; private set; }

        public TransactionDialog()
        {
            InitializeComponent();
            _isInitialized = true;
            DpDate.SelectedDate = DateTime.Now;
            LoadLookups();
        }

        private void LoadLookups()
        {
            var user = AuthService.CurrentUser;
            if (user == null) return;

            var wallets = _walletRepo.GetAll(user.UserId);
            if (CmbWallet != null)
            {
                CmbWallet.ItemsSource = wallets;
                if (wallets.Count > 0) CmbWallet.SelectedIndex = 0;
            }

            ReloadCategories();
        }

        private void ReloadCategories()
        {
            if (!_isInitialized || CmbCategory == null || RbExpense == null) return;

            var user = AuthService.CurrentUser;
            if (user == null) return;

            string type = (RbExpense.IsChecked == true) ? "Expense" : "Income";
            var categories = _categoryRepo.GetByType(user.UserId, type);
            CmbCategory.ItemsSource = categories;
            if (categories.Count > 0) CmbCategory.SelectedIndex = 0;
        }

        private void OnTypeChanged(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            ReloadCategories();
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            var user = AuthService.CurrentUser;
            if (user == null) return;

            // Validate amount
            var textAmt = TxtAmount.Text.Trim().Replace("Rp", "").Replace(".", "").Replace(",", ".").Trim();
            if (!decimal.TryParse(textAmt, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Masukkan nominal transaksi yang valid dan lebih dari 0.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtAmount.Focus();
                return;
            }

            if (CmbWallet.SelectedValue == null)
            {
                MessageBox.Show("Silakan pilih dompet transaksi.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbCategory.SelectedValue == null)
            {
                MessageBox.Show("Silakan pilih kategori transaksi.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int walletId = (int)CmbWallet.SelectedValue;
            int categoryId = (int)CmbCategory.SelectedValue;
            string txType = (RbExpense.IsChecked == true) ? "Expense" : "Income";
            DateTime date = DpDate.SelectedDate ?? DateTime.Now;
            string desc = TxtDescription.Text.Trim();

            // Check if wallet balance is sufficient for expense
            if (txType == "Expense")
            {
                var wallet = _walletRepo.GetById(walletId, user.UserId);
                if (wallet != null && wallet.CurrentBalance < amount)
                {
                    var confirm = MessageBox.Show(
                        $"Saldo dompet '{wallet.WalletName}' hanya Rp {wallet.CurrentBalance:N0}. Pengeluaran sebesar Rp {amount:N0} akan membuat saldo menjadi negatif. Lanjutkan?",
                        "Peringatan Saldo",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (confirm != MessageBoxResult.Yes) return;
                }
            }

            try
            {
                var tx = new Transaction
                {
                    UserId = user.UserId,
                    WalletId = walletId,
                    CategoryId = categoryId,
                    TransactionType = txType,
                    Amount = amount,
                    TransactionDate = date,
                    Description = desc
                };

                _txRepo.Add(tx);
                IsSaved = true;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menyimpan transaksi: {ex.Message}", "Kesalahan Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
