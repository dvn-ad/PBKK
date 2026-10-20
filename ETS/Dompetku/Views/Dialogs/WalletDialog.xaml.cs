using System;
using System.Windows;
using System.Windows.Controls;
using Dompetku.Data;
using Dompetku.Models;
using Dompetku.Services;

namespace Dompetku.Views.Dialogs
{
    public partial class WalletDialog : Window
    {
        private readonly WalletRepository _walletRepo = new();
        private readonly Wallet? _existingWallet;

        public bool IsSaved { get; private set; }

        public WalletDialog(Wallet? existing = null)
        {
            InitializeComponent();
            _existingWallet = existing;

            if (_existingWallet != null)
            {
                TxtTitle.Text = "Edit Dompet";
                TxtName.Text = _existingWallet.WalletName;
                TxtBalance.Text = _existingWallet.CurrentBalance.ToString("N0");
                TxtBalance.IsEnabled = false; // Saldo berjalan diubah via transaksi/transfer
                LblBalance.Text = "Saldo Saat Ini (Rp - Read Only)";

                foreach (ComboBoxItem item in CmbType.Items)
                {
                    if (item.Tag?.ToString() == _existingWallet.WalletType)
                    {
                        item.IsSelected = true;
                        break;
                    }
                }

                foreach (ComboBoxItem item in CmbColor.Items)
                {
                    if (item.Tag?.ToString() == _existingWallet.ColorCode)
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
            }
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            var user = AuthService.CurrentUser;
            if (user == null) return;

            string name = TxtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Nama dompet harus diisi.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return;
            }

            string type = ((ComboBoxItem)CmbType.SelectedItem).Tag?.ToString() ?? "Cash";
            string color = ((ComboBoxItem)CmbColor.SelectedItem).Tag?.ToString() ?? "#2563EB";

            try
            {
                if (_existingWallet == null)
                {
                    var textBal = TxtBalance.Text.Trim().Replace("Rp", "").Replace(".", "").Replace(",", ".").Trim();
                    decimal.TryParse(textBal, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal bal);

                    var newWallet = new Wallet
                    {
                        UserId = user.UserId,
                        WalletName = name,
                        WalletType = type,
                        CurrentBalance = bal,
                        ColorCode = color
                    };
                    _walletRepo.Add(newWallet);
                }
                else
                {
                    _existingWallet.WalletName = name;
                    _existingWallet.WalletType = type;
                    _existingWallet.ColorCode = color;
                    _walletRepo.Update(_existingWallet);
                }

                IsSaved = true;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menyimpan dompet: {ex.Message}", "Kesalahan Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
