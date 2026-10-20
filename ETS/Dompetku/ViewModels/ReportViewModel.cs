using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Dompetku.Data;
using Dompetku.Models;
using Dompetku.Services;

namespace Dompetku.ViewModels
{
    public class ReportViewModel : BaseViewModel
    {
        private readonly ReportRepository _reportRepo = new();

        public ObservableCollection<CategoryExpenseSummary> CategoryExpenses { get; } = new();
        public ObservableCollection<MonthlyTrendSummary> MonthlyTrends { get; } = new();

        private int _selectedMonth = DateTime.Now.Month;
        public int SelectedMonth
        {
            get => _selectedMonth;
            set { if (SetProperty(ref _selectedMonth, value)) LoadReport(); }
        }

        private int _selectedYear = DateTime.Now.Year;
        public int SelectedYear
        {
            get => _selectedYear;
            set { if (SetProperty(ref _selectedYear, value)) LoadReport(); }
        }

        private decimal _totalIncome;
        public decimal TotalIncome
        {
            get => _totalIncome;
            set => SetProperty(ref _totalIncome, value);
        }

        private decimal _totalExpense;
        public decimal TotalExpense
        {
            get => _totalExpense;
            set => SetProperty(ref _totalExpense, value);
        }

        private decimal _netCashFlow;
        public decimal NetCashFlow
        {
            get => _netCashFlow;
            set => SetProperty(ref _netCashFlow, value);
        }

        private double _savingsRate;
        public double SavingsRate
        {
            get => _savingsRate;
            set => SetProperty(ref _savingsRate, value);
        }

        public ICommand RefreshCommand { get; }

        public ReportViewModel()
        {
            RefreshCommand = new RelayCommand(LoadReport);
            LoadReport();
        }

        public void LoadReport()
        {
            var user = AuthService.CurrentUser;
            if (user == null) return;

            // Monthly Totals
            var (inc, exp) = _reportRepo.GetMonthlyTotals(user.UserId, SelectedMonth, SelectedYear);
            TotalIncome = inc;
            TotalExpense = exp;
            NetCashFlow = inc - exp;
            SavingsRate = inc > 0 ? (double)((inc - exp) / inc) * 100.0 : 0.0;

            // Category breakdown
            CategoryExpenses.Clear();
            foreach (var item in _reportRepo.GetExpenseByCategory(user.UserId, SelectedMonth, SelectedYear))
            {
                CategoryExpenses.Add(item);
            }

            // Trend 6 bulan
            MonthlyTrends.Clear();
            foreach (var trend in _reportRepo.GetLast6MonthsCashflow(user.UserId))
            {
                MonthlyTrends.Add(trend);
            }
        }
    }
}
