using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Dompetku.Data;
using Dompetku.Models;
using Dompetku.Services;

namespace Dompetku.ViewModels
{
    public class TransactionViewModel : BaseViewModel
    {
        private readonly TransactionRepository _txRepo = new();
        private readonly WalletRepository _walletRepo = new();
        private readonly CategoryRepository _categoryRepo = new();

        public ObservableCollection<Transaction> Transactions { get; } = new();
        public ObservableCollection<Wallet> WalletsFilter { get; } = new();
        public ObservableCollection<Category> CategoriesFilter { get; } = new();

        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set { if (SetProperty(ref _startDate, value)) ApplyFilter(); }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set { if (SetProperty(ref _endDate, value)) ApplyFilter(); }
        }

        private Wallet? _selectedWalletFilter;
        public Wallet? SelectedWalletFilter
        {
            get => _selectedWalletFilter;
            set { if (SetProperty(ref _selectedWalletFilter, value)) ApplyFilter(); }
        }

        private Category? _selectedCategoryFilter;
        public Category? SelectedCategoryFilter
        {
            get => _selectedCategoryFilter;
            set { if (SetProperty(ref _selectedCategoryFilter, value)) ApplyFilter(); }
        }

        private string _selectedTypeFilter = "All";
        public string SelectedTypeFilter
        {
            get => _selectedTypeFilter;
            set { if (SetProperty(ref _selectedTypeFilter, value)) ApplyFilter(); }
        }

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set { if (SetProperty(ref _searchQuery, value)) ApplyFilter(); }
        }

        private decimal _totalFilteredIncome;
        public decimal TotalFilteredIncome
        {
            get => _totalFilteredIncome;
            set => SetProperty(ref _totalFilteredIncome, value);
        }

        private decimal _totalFilteredExpense;
        public decimal TotalFilteredExpense
        {
            get => _totalFilteredExpense;
            set => SetProperty(ref _totalFilteredExpense, value);
        }

        public ICommand ResetFilterCommand { get; }
        public ICommand ExportCsvCommand { get; }
        public ICommand DeleteTransactionCommand { get; }

        public event Action? RequestRefreshAll;

        public TransactionViewModel()
        {
            ResetFilterCommand = new RelayCommand(ResetFilter);
            ExportCsvCommand = new RelayCommand(ExportCsv);
            DeleteTransactionCommand = new RelayCommand(DeleteTransaction);

            var now = DateTime.Now;
            _startDate = new DateTime(now.Year, now.Month, 1);
            _endDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

            LoadFilterLookups();
            ApplyFilter();
        }

        public void LoadFilterLookups()
        {
            var user = AuthService.CurrentUser;
            if (user == null) return;

            WalletsFilter.Clear();
            WalletsFilter.Add(new Wallet { WalletId = 0, WalletName = "-- Semua Dompet --" });
            foreach (var w in _walletRepo.GetAll(user.UserId))
            {
                WalletsFilter.Add(w);
            }
            _selectedWalletFilter = WalletsFilter[0];
            OnPropertyChanged(nameof(SelectedWalletFilter));

            CategoriesFilter.Clear();
            CategoriesFilter.Add(new Category { CategoryId = 0, CategoryName = "-- Semua Kategori --" });
            foreach (var c in _categoryRepo.GetAll(user.UserId))
            {
                CategoriesFilter.Add(c);
            }
            _selectedCategoryFilter = CategoriesFilter[0];
            OnPropertyChanged(nameof(SelectedCategoryFilter));
        }

        public void ApplyFilter()
        {
            var user = AuthService.CurrentUser;
            if (user == null) return;

            int? walletId = SelectedWalletFilter?.WalletId > 0 ? SelectedWalletFilter.WalletId : null;
            int? categoryId = SelectedCategoryFilter?.CategoryId > 0 ? SelectedCategoryFilter.CategoryId : null;

            var list = _txRepo.GetFiltered(user.UserId, StartDate, EndDate, walletId, categoryId, SelectedTypeFilter, SearchQuery);

            Transactions.Clear();
            decimal inc = 0, exp = 0;
            foreach (var t in list)
            {
                Transactions.Add(t);
                if (t.IsIncome) inc += t.Amount;
                else exp += t.Amount;
            }

            TotalFilteredIncome = inc;
            TotalFilteredExpense = exp;
        }

        private void ResetFilter()
        {
            var now = DateTime.Now;
            _startDate = new DateTime(now.Year, now.Month, 1);
            _endDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
            _selectedTypeFilter = "All";
            _searchQuery = string.Empty;
            _selectedWalletFilter = WalletsFilter.FirstOrDefault();
            _selectedCategoryFilter = CategoriesFilter.FirstOrDefault();

            OnPropertyChanged(nameof(StartDate));
            OnPropertyChanged(nameof(EndDate));
            OnPropertyChanged(nameof(SelectedTypeFilter));
            OnPropertyChanged(nameof(SearchQuery));
            OnPropertyChanged(nameof(SelectedWalletFilter));
            OnPropertyChanged(nameof(SelectedCategoryFilter));

            ApplyFilter();
        }

        private void DeleteTransaction(object? param)
        {
            if (param is Transaction tx)
            {
                var result = MessageBox.Show(
                    $"Apakah Anda yakin ingin menghapus transaksi {tx.CategoryName} sebesar Rp {tx.Amount:N0}?\nSaldo dompet akan disesuaikan kembali secara otomatis.",
                    "Konfirmasi Hapus",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var user = AuthService.CurrentUser;
                        if (user != null)
                        {
                            _txRepo.Delete(tx.TransactionId, user.UserId);
                            ApplyFilter();
                            RequestRefreshAll?.Invoke();
                            MessageBox.Show("Transaksi berhasil dihapus.", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Gagal menghapus transaksi: {ex.Message}", "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ExportCsv()
        {
            if (Transactions.Count == 0)
            {
                MessageBox.Show("Tidak ada data transaksi yang dapat diekspor sesuai filter saat ini.", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv",
                FileName = $"Mutasi_Dompetku_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    CsvExportService.ExportTransactionsToCsv(Transactions, dialog.FileName);
                    MessageBox.Show($"Data mutasi transaksi berhasil diekspor ke:\n{dialog.FileName}", "Ekspor Berhasil", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gagal mengekspor file: {ex.Message}", "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
