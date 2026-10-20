using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Dompetku.Data;
using Dompetku.Models;
using Dompetku.Services;

namespace Dompetku.ViewModels
{
    public class BudgetViewModel : BaseViewModel
    {
        private readonly BudgetRepository _budgetRepo = new();

        public ObservableCollection<Budget> Budgets { get; } = new();

        private int _selectedMonth = DateTime.Now.Month;
        public int SelectedMonth
        {
            get => _selectedMonth;
            set { if (SetProperty(ref _selectedMonth, value)) LoadBudgets(); }
        }

        private int _selectedYear = DateTime.Now.Year;
        public int SelectedYear
        {
            get => _selectedYear;
            set { if (SetProperty(ref _selectedYear, value)) LoadBudgets(); }
        }

        private decimal _totalBudgetLimit;
        public decimal TotalBudgetLimit
        {
            get => _totalBudgetLimit;
            set => SetProperty(ref _totalBudgetLimit, value);
        }

        private decimal _totalBudgetSpent;
        public decimal TotalBudgetSpent
        {
            get => _totalBudgetSpent;
            set => SetProperty(ref _totalBudgetSpent, value);
        }

        public ICommand DeleteBudgetCommand { get; }
        public ICommand RefreshCommand { get; }

        public event Action? RequestRefreshAll;

        public BudgetViewModel()
        {
            DeleteBudgetCommand = new RelayCommand(DeleteBudget);
            RefreshCommand = new RelayCommand(LoadBudgets);

            LoadBudgets();
        }

        public void LoadBudgets()
        {
            var user = AuthService.CurrentUser;
            if (user == null) return;

            Budgets.Clear();
            decimal totalLim = 0;
            decimal totalSp = 0;

            foreach (var b in _budgetRepo.GetBudgetsWithSpent(user.UserId, SelectedMonth, SelectedYear))
            {
                Budgets.Add(b);
                totalLim += b.LimitAmount;
                totalSp += b.SpentAmount;
            }

            TotalBudgetLimit = totalLim;
            TotalBudgetSpent = totalSp;
        }

        private void DeleteBudget(object? param)
        {
            if (param is Budget b)
            {
                var user = AuthService.CurrentUser;
                if (user == null) return;

                var res = MessageBox.Show($"Hapus anggaran untuk kategori '{b.CategoryName}'?", "Konfirmasi Hapus", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        _budgetRepo.Delete(b.BudgetId, user.UserId);
                        LoadBudgets();
                        RequestRefreshAll?.Invoke();
                        MessageBox.Show("Anggaran berhasil dihapus.", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Gagal menghapus anggaran: {ex.Message}", "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
