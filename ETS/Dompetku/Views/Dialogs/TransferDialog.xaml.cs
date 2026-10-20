using System;
using System.Linq;
using System.Windows;
using Dompetku.Data;
using Dompetku.Models;
using Dompetku.Services;

namespace Dompetku.Views.Dialogs
{
    public partial class TransferDialog : Window
    {
        private readonly WalletRepository _walletRepo = new();
        private readonly TransferRepository _transferRepo = new();

        public bool IsSaved { get; private set; }

        public TransferDialog()
        {
            InitializeComponent();
            DpDate.SelectedDate = DateTime.Now;
            LoadWallets();
        }

        private void LoadWallets()
        {
            var user = AuthService.CurrentUser;
            if (user == null) return;

            var wallets = _walletRepo.GetAll(user.UserId);
            CmbFromWallet.ItemsSource = wallets;
            CmbToWallet.ItemsSource = wallets.ToList();

            if (wallets.Count > 0) CmbFromWallet.SelectedIndex = 0;
            if (wallets.Count > 1) CmbToWallet.SelectedIndex = 1;
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            var user = AuthService.CurrentUser;
            if (user == null) return;

            if (CmbFromWallet.SelectedValue == null || CmbToWallet.SelectedValue == null)
            {
                MessageBox.Show("Silakan pilih dompet asal dan dompet tujuan.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int fromId = (int)CmbFromWallet.SelectedValue;
            int toId = (int)CmbToWallet.SelectedValue;

            if (fromId == toId)
            {
                MessageBox.Show("Dompet asal dan tujuan tidak boleh sama.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var textAmt = TxtAmount.Text.Trim().Replace("Rp", "").Replace(".", "").Replace(",", ".").Trim();
            if (!decimal.TryParse(textAmt, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Masukkan nominal transfer yang valid dan lebih dari 0.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtAmount.Focus();
                return;
            }

            var textFee = TxtAdminFee.Text.Trim().Replace("Rp", "").Replace(".", "").Replace(",", ".").Trim();
            decimal fee = 0;
            if (!string.IsNullOrWhiteSpace(textFee) && (!decimal.TryParse(textFee, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out fee) || fee < 0))
            {
                MessageBox.Show("Biaya admin harus berupa angka bernilai 0 atau lebih.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtAdminFee.Focus();
                return;
            }

            try
            {
                var tr = new Transfer
                {
                    UserId = user.UserId,
                    FromWalletId = fromId,
                    ToWalletId = toId,
                    Amount = amount,
                    AdminFee = fee,
                    TransferDate = DpDate.SelectedDate ?? DateTime.Now,
                    Notes = TxtNotes.Text.Trim()
                };

                _transferRepo.Add(tr);
                IsSaved = true;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Transfer gagal: {ex.Message}", "Kesalahan Transaksi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
