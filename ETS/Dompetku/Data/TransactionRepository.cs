using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Dompetku.Models;

namespace Dompetku.Data
{
    public class TransactionRepository
    {
        public List<Transaction> GetFiltered(int userId, DateTime? startDate, DateTime? endDate, int? walletId, int? categoryId, string? type, string? search)
        {
            var list = new List<Transaction>();
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            var query = @"
                SELECT t.transaction_id, t.user_id, t.wallet_id, t.category_id, 
                       t.transaction_type, t.amount, t.transaction_date, t.description, t.created_at,
                       w.wallet_name, c.category_name, c.color_code AS category_color
                FROM Transactions t
                INNER JOIN Wallets w ON t.wallet_id = w.wallet_id
                INNER JOIN Categories c ON t.category_id = c.category_id
                WHERE t.user_id = @userId";

            using var cmd = conn.CreateCommand();
            cmd.Parameters.AddWithValue("@userId", userId);

            if (startDate.HasValue)
            {
                query += " AND t.transaction_date >= @startDate";
                cmd.Parameters.AddWithValue("@startDate", startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                query += " AND t.transaction_date <= @endDate";
                cmd.Parameters.AddWithValue("@endDate", endDate.Value.Date.AddDays(1).AddTicks(-1));
            }
            if (walletId.HasValue && walletId.Value > 0)
            {
                query += " AND t.wallet_id = @walletId";
                cmd.Parameters.AddWithValue("@walletId", walletId.Value);
            }
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query += " AND t.category_id = @categoryId";
                cmd.Parameters.AddWithValue("@categoryId", categoryId.Value);
            }
            if (!string.IsNullOrEmpty(type) && type != "All")
            {
                query += " AND t.transaction_type = @type";
                cmd.Parameters.AddWithValue("@type", type);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND (t.description LIKE @search OR c.category_name LIKE @search OR w.wallet_name LIKE @search)";
                cmd.Parameters.AddWithValue("@search", $"%{search.Trim()}%");
            }

            query += " ORDER BY t.transaction_date DESC, t.transaction_id DESC";
            cmd.CommandText = query;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Transaction
                {
                    TransactionId = reader.GetInt64("transaction_id"),
                    UserId = reader.GetInt32("user_id"),
                    WalletId = reader.GetInt32("wallet_id"),
                    CategoryId = reader.GetInt32("category_id"),
                    TransactionType = reader.GetString("transaction_type"),
                    Amount = reader.GetDecimal("amount"),
                    TransactionDate = reader.GetDateTime("transaction_date"),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? string.Empty : reader.GetString("description"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    WalletName = reader.GetString("wallet_name"),
                    CategoryName = reader.GetString("category_name"),
                    CategoryColor = reader.IsDBNull(reader.GetOrdinal("category_color")) ? "#64748B" : reader.GetString("category_color")
                });
            }

            return list;
        }

        public List<Transaction> GetRecent(int userId, int count = 5)
        {
            var list = new List<Transaction>();
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
                SELECT TOP ({count}) t.transaction_id, t.user_id, t.wallet_id, t.category_id, 
                       t.transaction_type, t.amount, t.transaction_date, t.description, t.created_at,
                       w.wallet_name, c.category_name, c.color_code AS category_color
                FROM Transactions t
                INNER JOIN Wallets w ON t.wallet_id = w.wallet_id
                INNER JOIN Categories c ON t.category_id = c.category_id
                WHERE t.user_id = @userId
                ORDER BY t.transaction_date DESC, t.transaction_id DESC";
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Transaction
                {
                    TransactionId = reader.GetInt64("transaction_id"),
                    UserId = reader.GetInt32("user_id"),
                    WalletId = reader.GetInt32("wallet_id"),
                    CategoryId = reader.GetInt32("category_id"),
                    TransactionType = reader.GetString("transaction_type"),
                    Amount = reader.GetDecimal("amount"),
                    TransactionDate = reader.GetDateTime("transaction_date"),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? string.Empty : reader.GetString("description"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    WalletName = reader.GetString("wallet_name"),
                    CategoryName = reader.GetString("category_name"),
                    CategoryColor = reader.IsDBNull(reader.GetOrdinal("category_color")) ? "#64748B" : reader.GetString("category_color")
                });
            }

            return list;
        }

        public void Add(Transaction transaction)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                // 1. Insert Transaction
                using var cmdTx = conn.CreateCommand();
                cmdTx.Transaction = tx;
                cmdTx.CommandText = @"
                    INSERT INTO Transactions (user_id, wallet_id, category_id, transaction_type, amount, transaction_date, description)
                    OUTPUT INSERTED.transaction_id
                    VALUES (@userId, @walletId, @categoryId, @type, @amount, @date, @desc)";
                cmdTx.Parameters.AddWithValue("@userId", transaction.UserId);
                cmdTx.Parameters.AddWithValue("@walletId", transaction.WalletId);
                cmdTx.Parameters.AddWithValue("@categoryId", transaction.CategoryId);
                cmdTx.Parameters.AddWithValue("@type", transaction.TransactionType);
                cmdTx.Parameters.AddWithValue("@amount", transaction.Amount);
                cmdTx.Parameters.AddWithValue("@date", transaction.TransactionDate);
                cmdTx.Parameters.AddWithValue("@desc", (object?)transaction.Description ?? DBNull.Value);

                transaction.TransactionId = (long)cmdTx.ExecuteScalar();

                // 2. Adjust Wallet Balance
                using var cmdBal = conn.CreateCommand();
                cmdBal.Transaction = tx;
                if (transaction.TransactionType == "Income")
                {
                    cmdBal.CommandText = "UPDATE Wallets SET current_balance = current_balance + @amount WHERE wallet_id = @walletId";
                }
                else
                {
                    cmdBal.CommandText = "UPDATE Wallets SET current_balance = current_balance - @amount WHERE wallet_id = @walletId";
                }
                cmdBal.Parameters.AddWithValue("@amount", transaction.Amount);
                cmdBal.Parameters.AddWithValue("@walletId", transaction.WalletId);
                cmdBal.ExecuteNonQuery();

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void Delete(long transactionId, int userId)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                // 1. Get existing transaction details
                int walletId = 0;
                string type = "";
                decimal amount = 0;

                using (var cmdGet = conn.CreateCommand())
                {
                    cmdGet.Transaction = tx;
                    cmdGet.CommandText = "SELECT wallet_id, transaction_type, amount FROM Transactions WHERE transaction_id = @id AND user_id = @userId";
                    cmdGet.Parameters.AddWithValue("@id", transactionId);
                    cmdGet.Parameters.AddWithValue("@userId", userId);
                    using var reader = cmdGet.ExecuteReader();
                    if (reader.Read())
                    {
                        walletId = reader.GetInt32(0);
                        type = reader.GetString(1);
                        amount = reader.GetDecimal(2);
                    }
                    else
                    {
                        throw new InvalidOperationException("Transaksi tidak ditemukan.");
                    }
                }

                // 2. Delete transaction
                using (var cmdDel = conn.CreateCommand())
                {
                    cmdDel.Transaction = tx;
                    cmdDel.CommandText = "DELETE FROM Transactions WHERE transaction_id = @id AND user_id = @userId";
                    cmdDel.Parameters.AddWithValue("@id", transactionId);
                    cmdDel.Parameters.AddWithValue("@userId", userId);
                    cmdDel.ExecuteNonQuery();
                }

                // 3. Revert Wallet Balance
                using (var cmdBal = conn.CreateCommand())
                {
                    cmdBal.Transaction = tx;
                    if (type == "Income")
                    {
                        cmdBal.CommandText = "UPDATE Wallets SET current_balance = current_balance - @amount WHERE wallet_id = @walletId";
                    }
                    else
                    {
                        cmdBal.CommandText = "UPDATE Wallets SET current_balance = current_balance + @amount WHERE wallet_id = @walletId";
                    }
                    cmdBal.Parameters.AddWithValue("@amount", amount);
                    cmdBal.Parameters.AddWithValue("@walletId", walletId);
                    cmdBal.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
