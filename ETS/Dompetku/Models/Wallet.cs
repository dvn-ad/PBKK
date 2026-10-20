using System;

namespace Dompetku.Models
{
    public class Wallet
    {
        public int WalletId { get; set; }
        public int UserId { get; set; }
        public string WalletName { get; set; } = string.Empty;
        public string WalletType { get; set; } = "Cash"; // 'Cash', 'Bank', 'E-Wallet', 'Other'
        public decimal CurrentBalance { get; set; }
        public string ColorCode { get; set; } = "#2563EB";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string FormattedBalance => $"Rp {CurrentBalance:N0}";
    }
}
