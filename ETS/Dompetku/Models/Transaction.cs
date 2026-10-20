using System;

namespace Dompetku.Models
{
    public class Transaction
    {
        public long TransactionId { get; set; }
        public int UserId { get; set; }
        public int WalletId { get; set; }
        public int CategoryId { get; set; }
        public string TransactionType { get; set; } = "Expense"; // 'Income' atau 'Expense'
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string? Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Joined view properties
        public string WalletName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = "#64748B";

        public bool IsExpense => TransactionType == "Expense";
        public bool IsIncome => TransactionType == "Income";

        public string DisplayAmountSign => IsExpense ? $"- Rp {Amount:N0}" : $"+ Rp {Amount:N0}";
        public string DisplayFormattedAmount => $"Rp {Amount:N0}";
    }
}
