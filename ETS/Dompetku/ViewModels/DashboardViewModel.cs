using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Dompetku.Data;
using Dompetku.Models;
using Dompetku.Services;

namespace Dompetku.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly WalletRepository _walletRepo = new();
        private readonly TransactionRepository _txRepo = new();
        private readonly BudgetRepository _budgetRepo = new();
        private readonly ReportRepository _reportRepo = new();

        private decimal _totalBalance;
        public decimal TotalBalance
        {
            get => _totalBalance;
            set => SetProperty(ref _totalBalance, value);
        }

        private decimal _incomeThisMonth;
        public decimal IncomeThisMonth
        {
            get => _incomeThisMonth;
            set => SetProperty(ref _incomeThisMonth, value);
        }

        private decimal _expenseThisMonth;
        public decimal ExpenseThisMonth
        {
            get => _expenseThisMonth;
            set => SetProperty(ref _expenseThisMonth, value);
        }

        private decimal _netCashFlow;
        public decimal NetCashFlow
        {
            get => _netCashFlow;
            set => SetProperty(ref _netCashFlow, value);
        }

        private string _greetingText = "Selamat Datang!";
        public string GreetingText
        {
            get => _greetingText;
            set => SetProperty(ref _greetingText, value);
        }

        public ObservableCollection<Wallet> Wallets { get; } = new();
        public ObservableCollection<Transaction> RecentTransactions { get; } = new();
        public ObservableCollection<Budget> ActiveBudgets { get; } = new();

        public ICommand RefreshCommand { get; }

        public DashboardViewModel()
        {
            RefreshCommand = new RelayCommand(LoadData);
            LoadData();
        }

        public void LoadData()
        {
            var user = AuthService.CurrentUser;
            if (user == null) return;

            var hour = DateTime.Now.Hour;
            var greeting = hour switch
            {
                < 11 => "Selamat Pagi",
                < 15 => "Selamat Siang",
                < 18 => "Selamat Sore",
                _ => "Selamat Malam"
            };
            GreetingText = $"{greeting}, {user.FullName}!";

            var now = DateTime.Now;

            // 1. Total Balance
            TotalBalance = _walletRepo.GetTotalBalance(user.UserId);

            // 2. Monthly Income & Expense
            var (inc, exp) = _reportRepo.GetMonthlyTotals(user.UserId, now.Month, now.Year);
            IncomeThisMonth = inc;
            ExpenseThisMonth = exp;
            NetCashFlow = inc - exp;

            // 3. Wallets
            Wallets.Clear();
            foreach (var w in _walletRepo.GetAll(user.UserId))
            {
                Wallets.Add(w);
            }

            // 4. Recent Transactions
            RecentTransactions.Clear();
            foreach (var t in _txRepo.GetRecent(user.UserId, 5))
            {
                RecentTransactions.Add(t);
            }

            // 5. Active Budgets
            ActiveBudgets.Clear();
            foreach (var b in _budgetRepo.GetBudgetsWithSpent(user.UserId, now.Month, now.Year))
            {
                ActiveBudgets.Add(b);
            }
        }
    }
}
