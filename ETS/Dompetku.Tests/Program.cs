using System;
using Dompetku.Data;
using Dompetku.Models;
using Dompetku.Services;

namespace Dompetku.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("  DOMPETKU AUTOMATED TEST RUNNER");
            Console.WriteLine("========================================");

            // 1. Database Initialization
            Console.Write("[1/7] Initializing Database... ");
            DatabaseInitializer.Initialize();
            Console.WriteLine("OK");

            // 2. Authentication Test
            Console.Write("[2/7] Testing AuthService Login... ");
            bool loginOk = AuthService.Login("admin", "admin123", out string err);
            if (!loginOk || AuthService.CurrentUser == null)
            {
                Console.WriteLine($"FAILED: {err}");
                return;
            }
            Console.WriteLine($"OK (User: {AuthService.CurrentUser.FullName})");

            var user = AuthService.CurrentUser;

            // 3. Wallets Repository
            Console.Write("[3/7] Testing WalletRepository... ");
            var walletRepo = new WalletRepository();
            var wallets = walletRepo.GetAll(user.UserId);
            if (wallets.Count == 0)
            {
                Console.WriteLine("FAILED: No wallets found");
                return;
            }
            decimal totalBal = walletRepo.GetTotalBalance(user.UserId);
            Console.WriteLine($"OK ({wallets.Count} Wallets, Total: Rp {totalBal:N0})");

            // 4. Categories Repository
            Console.Write("[4/7] Testing CategoryRepository... ");
            var catRepo = new CategoryRepository();
            var cats = catRepo.GetAll(user.UserId);
            if (cats.Count == 0)
            {
                Console.WriteLine("FAILED: No categories found");
                return;
            }
            Console.WriteLine($"OK ({cats.Count} Categories)");

            // 5. Transactions Repository (Add, Balance Check, Delete Revert Check)
            Console.Write("[5/7] Testing TransactionRepository & ACID balance... ");
            var txRepo = new TransactionRepository();
            var testWallet = wallets[0];
            decimal balBefore = walletRepo.GetById(testWallet.WalletId, user.UserId)!.CurrentBalance;

            var testTx = new Transaction
            {
                UserId = user.UserId,
                WalletId = testWallet.WalletId,
                CategoryId = cats[0].CategoryId,
                TransactionType = "Income",
                Amount = 50000m,
                TransactionDate = DateTime.Now,
                Description = "Unit test transaction income"
            };

            txRepo.Add(testTx);
            decimal balAfterAdd = walletRepo.GetById(testWallet.WalletId, user.UserId)!.CurrentBalance;
            if (balAfterAdd != balBefore + 50000m)
            {
                Console.WriteLine($"FAILED: Expected balance {balBefore + 50000m}, got {balAfterAdd}");
                return;
            }

            // Revert by deleting
            txRepo.Delete(testTx.TransactionId, user.UserId);
            decimal balAfterDelete = walletRepo.GetById(testWallet.WalletId, user.UserId)!.CurrentBalance;
            if (balAfterDelete != balBefore)
            {
                Console.WriteLine($"FAILED: Reverting balance expected {balBefore}, got {balAfterDelete}");
                return;
            }
            Console.WriteLine("OK (ACID balance verified)");

            // 6. Budgets & Report Repository
            Console.Write("[6/7] Testing Budgets & Reports... ");
            var budgetRepo = new BudgetRepository();
            var budgets = budgetRepo.GetBudgetsWithSpent(user.UserId, DateTime.Now.Month, DateTime.Now.Year);

            var reportRepo = new ReportRepository();
            var (inc, exp) = reportRepo.GetMonthlyTotals(user.UserId, DateTime.Now.Month, DateTime.Now.Year);
            var catExpenses = reportRepo.GetExpenseByCategory(user.UserId, DateTime.Now.Month, DateTime.Now.Year);
            var trends = reportRepo.GetLast6MonthsCashflow(user.UserId);
            Console.WriteLine($"OK ({budgets.Count} Budgets, {catExpenses.Count} Expense Cats, {trends.Count} Trend Months)");

            // 7. CSV Export Service
            Console.Write("[7/7] Testing CsvExportService... ");
            var txList = txRepo.GetRecent(user.UserId, 5);
            var tempCsv = Path.Combine(Path.GetTempPath(), "test_export.csv");
            CsvExportService.ExportTransactionsToCsv(txList, tempCsv);
            if (!File.Exists(tempCsv) || new FileInfo(tempCsv).Length == 0)
            {
                Console.WriteLine("FAILED: CSV file empty");
                return;
            }
            File.Delete(tempCsv);
            Console.WriteLine("OK");

            Console.WriteLine("\n========================================");
            Console.WriteLine("  ALL TESTS PASSED SUCCESSFULLY! (100%)");
            Console.WriteLine("========================================");
        }
    }
}
