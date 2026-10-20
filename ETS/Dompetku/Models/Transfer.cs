using System;

namespace Dompetku.Models
{
    public class Transfer
    {
        public long TransferId { get; set; }
        public int UserId { get; set; }
        public int FromWalletId { get; set; }
        public int ToWalletId { get; set; }
        public decimal Amount { get; set; }
        public decimal AdminFee { get; set; }
        public DateTime TransferDate { get; set; } = DateTime.Now;
        public string? Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Joined view properties
        public string FromWalletName { get; set; } = string.Empty;
        public string ToWalletName { get; set; } = string.Empty;

        public decimal TotalDeducted => Amount + AdminFee;
        public string FormattedAmount => $"Rp {Amount:N0}";
        public string FormattedTotal => $"Rp {TotalDeducted:N0}";
    }
}
