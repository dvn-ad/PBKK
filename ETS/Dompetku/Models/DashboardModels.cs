using System.Collections.Generic;

namespace Dompetku.Models
{
    public class DashboardSummary
    {
        public decimal TotalBalance { get; set; }
        public decimal IncomeThisMonth { get; set; }
        public decimal ExpenseThisMonth { get; set; }
        public decimal NetCashFlow => IncomeThisMonth - ExpenseThisMonth;

        public string FormattedTotalBalance => $"Rp {TotalBalance:N0}";
        public string FormattedIncomeThisMonth => $"Rp {IncomeThisMonth:N0}";
        public string FormattedExpenseThisMonth => $"Rp {ExpenseThisMonth:N0}";
        public string FormattedNetCashFlow => $"Rp {NetCashFlow:N0}";

        public List<Wallet> Wallets { get; set; } = new();
        public List<Transaction> RecentTransactions { get; set; } = new();
        public List<Budget> ActiveBudgets { get; set; } = new();
    }

    public class CategoryExpenseSummary
    {
        public string CategoryName { get; set; } = string.Empty;
        public string ColorCode { get; set; } = "#2563EB";
        public decimal TotalAmount { get; set; }
        public double Percentage { get; set; }
        public string FormattedAmount => $"Rp {TotalAmount:N0}";
        public string FormattedPercentage => $"{Percentage:F1}%";
    }

    public class MonthlyTrendSummary
    {
        public string MonthLabel { get; set; } = string.Empty;
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public string FormattedIncome => $"Rp {TotalIncome:N0}";
        public string FormattedExpense => $"Rp {TotalExpense:N0}";
    }
}
