using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Dompetku.Models;

namespace Dompetku.Data
{
    public class BudgetRepository
    {
        public List<Budget> GetBudgetsWithSpent(int userId, int month, int year)
        {
            var list = new List<Budget>();
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT b.budget_id, b.user_id, b.category_id, b.limit_amount, 
                       b.period_month, b.period_year, b.created_at,
                       c.category_name, c.color_code AS category_color,
                       ISNULL((
                           SELECT SUM(t.amount) 
                           FROM Transactions t 
                           WHERE t.user_id = b.user_id 
                             AND t.category_id = b.category_id 
                             AND t.transaction_type = 'Expense'
                             AND MONTH(t.transaction_date) = b.period_month 
                             AND YEAR(t.transaction_date) = b.period_year
                       ), 0) AS spent_amount
                FROM Budgets b
                INNER JOIN Categories c ON b.category_id = c.category_id
                WHERE b.user_id = @userId AND b.period_month = @month AND b.period_year = @year
                ORDER BY b.limit_amount DESC";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@month", month);
            cmd.Parameters.AddWithValue("@year", year);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Budget
                {
                    BudgetId = reader.GetInt32("budget_id"),
                    UserId = reader.GetInt32("user_id"),
                    CategoryId = reader.GetInt32("category_id"),
                    LimitAmount = reader.GetDecimal("limit_amount"),
                    PeriodMonth = reader.GetInt32("period_month"),
                    PeriodYear = reader.GetInt32("period_year"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    CategoryName = reader.GetString("category_name"),
                    CategoryColor = reader.IsDBNull(reader.GetOrdinal("category_color")) ? "#64748B" : reader.GetString("category_color"),
                    SpentAmount = reader.GetDecimal("spent_amount")
                });
            }

            return list;
        }

        public void SaveOrUpdate(Budget budget)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                IF EXISTS (SELECT 1 FROM Budgets WHERE user_id = @userId AND category_id = @catId AND period_month = @month AND period_year = @year)
                BEGIN
                    UPDATE Budgets 
                    SET limit_amount = @limit
                    WHERE user_id = @userId AND category_id = @catId AND period_month = @month AND period_year = @year
                END
                ELSE
                BEGIN
                    INSERT INTO Budgets (user_id, category_id, limit_amount, period_month, period_year)
                    VALUES (@userId, @catId, @limit, @month, @year)
                END";
            cmd.Parameters.AddWithValue("@userId", budget.UserId);
            cmd.Parameters.AddWithValue("@catId", budget.CategoryId);
            cmd.Parameters.AddWithValue("@limit", budget.LimitAmount);
            cmd.Parameters.AddWithValue("@month", budget.PeriodMonth);
            cmd.Parameters.AddWithValue("@year", budget.PeriodYear);

            cmd.ExecuteNonQuery();
        }

        public void Delete(int budgetId, int userId)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Budgets WHERE budget_id = @id AND user_id = @userId";
            cmd.Parameters.AddWithValue("@id", budgetId);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.ExecuteNonQuery();
        }
    }
}
