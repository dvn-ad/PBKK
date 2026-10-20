using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Dompetku.Models;

namespace Dompetku.Data
{
    public class ReportRepository
    {
        public (decimal income, decimal expense) GetMonthlyTotals(int userId, int month, int year)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                    ISNULL(SUM(CASE WHEN transaction_type = 'Income' THEN amount ELSE 0 END), 0) AS total_income,
                    ISNULL(SUM(CASE WHEN transaction_type = 'Expense' THEN amount ELSE 0 END), 0) AS total_expense
                FROM Transactions
                WHERE user_id = @userId AND MONTH(transaction_date) = @month AND YEAR(transaction_date) = @year";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@month", month);
            cmd.Parameters.AddWithValue("@year", year);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (reader.GetDecimal(0), reader.GetDecimal(1));
            }
            return (0m, 0m);
        }

        public List<CategoryExpenseSummary> GetExpenseByCategory(int userId, int month, int year)
        {
            var list = new List<CategoryExpenseSummary>();
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT c.category_name, c.color_code, SUM(t.amount) AS total_amount
                FROM Transactions t
                INNER JOIN Categories c ON t.category_id = c.category_id
                WHERE t.user_id = @userId 
                  AND t.transaction_type = 'Expense'
                  AND MONTH(t.transaction_date) = @month 
                  AND YEAR(t.transaction_date) = @year
                GROUP BY c.category_name, c.color_code
                ORDER BY total_amount DESC";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@month", month);
            cmd.Parameters.AddWithValue("@year", year);

            decimal grandTotal = 0;
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var item = new CategoryExpenseSummary
                    {
                        CategoryName = reader.GetString("category_name"),
                        ColorCode = reader.IsDBNull(reader.GetOrdinal("color_code")) ? "#2563EB" : reader.GetString("color_code"),
                        TotalAmount = reader.GetDecimal("total_amount")
                    };
                    grandTotal += item.TotalAmount;
                    list.Add(item);
                }
            }

            if (grandTotal > 0)
            {
                foreach (var item in list)
                {
                    item.Percentage = (double)(item.TotalAmount / grandTotal) * 100.0;
                }
            }

            return list;
        }

        public List<MonthlyTrendSummary> GetLast6MonthsCashflow(int userId)
        {
            var list = new List<MonthlyTrendSummary>();
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            var now = DateTime.Now;
            for (int i = 5; i >= 0; i--)
            {
                var dt = now.AddMonths(-i);
                int m = dt.Month;
                int y = dt.Year;
                string label = dt.ToString("MMM yyyy");

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT 
                        ISNULL(SUM(CASE WHEN transaction_type = 'Income' THEN amount ELSE 0 END), 0) AS total_income,
                        ISNULL(SUM(CASE WHEN transaction_type = 'Expense' THEN amount ELSE 0 END), 0) AS total_expense
                    FROM Transactions
                    WHERE user_id = @userId AND MONTH(transaction_date) = @month AND YEAR(transaction_date) = @year";
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@month", m);
                cmd.Parameters.AddWithValue("@year", y);

                using var reader = cmd.ExecuteReader();
                decimal inc = 0m, exp = 0m;
                if (reader.Read())
                {
                    inc = reader.GetDecimal(0);
                    exp = reader.GetDecimal(1);
                }

                list.Add(new MonthlyTrendSummary
                {
                    MonthLabel = label,
                    TotalIncome = inc,
                    TotalExpense = exp
                });
            }

            return list;
        }
    }
}
