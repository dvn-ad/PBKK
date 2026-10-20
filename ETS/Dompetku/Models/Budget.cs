using System;

namespace Dompetku.Models
{
    public class Budget
    {
        public int BudgetId { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public decimal LimitAmount { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Joined view properties
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = "#64748B";
        public decimal SpentAmount { get; set; }

        public decimal RemainingAmount => LimitAmount - SpentAmount;
        public double PercentageUsed => LimitAmount > 0 ? (double)(SpentAmount / LimitAmount) * 100.0 : 0.0;

        public string FormattedLimit => $"Rp {LimitAmount:N0}";
        public string FormattedSpent => $"Rp {SpentAmount:N0}";
        public string FormattedRemaining => RemainingAmount >= 0 ? $"Sisa: Rp {RemainingAmount:N0}" : $"Lebih: Rp {Math.Abs(RemainingAmount):N0}";

        public string StatusText
        {
            get
            {
                if (PercentageUsed >= 100.0) return "Overbudget";
                if (PercentageUsed >= 80.0) return "Perhatian (>= 80%)";
                return "Aman";
            }
        }

        public string StatusColor
        {
            get
            {
                if (PercentageUsed >= 100.0) return "#DC2626"; // Red
                if (PercentageUsed >= 80.0) return "#F59E0B";  // Orange
                return "#16A34A";                             // Green
            }
        }
    }
}
