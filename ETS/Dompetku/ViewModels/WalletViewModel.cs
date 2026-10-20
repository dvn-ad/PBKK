using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Dompetku.Data;
using Dompetku.Models;
using Dompetku.Services;

namespace Dompetku.ViewModels
{
    public class WalletViewModel : BaseViewModel
    {
        private readonly WalletRepository _walletRepo = new();
        private readonly TransferRepository _transferRepo = new();

        public ObservableCollection<Wallet> Wallets { get; } = new();
        public ObservableCollection<Transfer> Transfers { get; } = new();

        private decimal _totalBalance;
        public decimal TotalBalance
        {
            get => _totalBalance;
            set => SetProperty(ref _totalBalance, value);
        }

        public ICommand DeleteWalletCommand { get; }
        public ICommand RefreshCommand { get; }

        public event Action? RequestRefreshAll;

        public WalletViewModel()
        {
            DeleteWalletCommand = new RelayCommand(DeleteWallet);
            RefreshCommand = new RelayCommand(LoadData);

            LoadData();
        }

        public void LoadData()
        {
            var user = AuthService.CurrentUser;
            if (user == null) return;

            Wallets.Clear();
            foreach (var w in _walletRepo.GetAll(user.UserId))
            {
                Wallets.Add(w);
            }

            TotalBalance = _walletRepo.GetTotalBalance(user.UserId);

            Transfers.Clear();
            foreach (var tr in _transferRepo.GetAll(user.UserId))
            {
                Transfers.Add(tr);
            }
        }

        private void DeleteWallet(object? param)
        {
            if (param is Wallet w)
            {
                var user = AuthService.CurrentUser;
                if (user == null) return;

                if (!_walletRepo.CanDelete(w.WalletId, user.UserId, out string reason))
                {
                    MessageBox.Show(reason, "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var res = MessageBox.Show($"Apakah Anda yakin ingin menghapus dompet '{w.WalletName}'?", "Konfirmasi Hapus", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        _walletRepo.Delete(w.WalletId, user.UserId);
                        LoadData();
                        RequestRefreshAll?.Invoke();
                        MessageBox.Show("Dompet berhasil dihapus.", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Gagal menghapus dompet: {ex.Message}", "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
