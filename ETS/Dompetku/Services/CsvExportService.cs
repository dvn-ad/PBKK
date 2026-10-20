using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dompetku.Models;

namespace Dompetku.Services
{
    public static class CsvExportService
    {
        public static void ExportTransactionsToCsv(IEnumerable<Transaction> transactions, string filePath)
        {
            var sb = new StringBuilder();
            // Header
            sb.AppendLine("ID;Tanggal;Tipe;Kategori;Dompet;Nominal;Catatan");

            foreach (var t in transactions)
            {
                var desc = (t.Description ?? string.Empty).Replace(";", ",").Replace("\r", " ").Replace("\n", " ");
                var cat = t.CategoryName.Replace(";", ",");
                var wal = t.WalletName.Replace(";", ",");
                sb.AppendLine($"{t.TransactionId};{t.TransactionDate:yyyy-MM-dd HH:mm};{t.TransactionType};{cat};{wal};{t.Amount:F2};{desc}");
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}
