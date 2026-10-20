namespace Dompetku.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Type { get; set; } = "Expense"; // 'Income' atau 'Expense'
        public string? IconName { get; set; } = "Tag";
        public string ColorCode { get; set; } = "#64748B";

        public bool IsExpense => Type == "Expense";
        public bool IsIncome => Type == "Income";
    }
}
