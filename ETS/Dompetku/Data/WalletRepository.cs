using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Dompetku.Models;

namespace Dompetku.Data
{
    public class WalletRepository
    {
        public List<Wallet> GetAll(int userId)
        {
            var list = new List<Wallet>();
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT wallet_id, user_id, wallet_name, wallet_type, current_balance, color_code, created_at
                FROM Wallets
                WHERE user_id = @userId
                ORDER BY wallet_name ASC";
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Wallet
                {
                    WalletId = reader.GetInt32("wallet_id"),
                    UserId = reader.GetInt32("user_id"),
                    WalletName = reader.GetString("wallet_name"),
                    WalletType = reader.GetString("wallet_type"),
                    CurrentBalance = reader.GetDecimal("current_balance"),
                    ColorCode = reader.IsDBNull(reader.GetOrdinal("color_code")) ? "#2563EB" : reader.GetString("color_code"),
                    CreatedAt = reader.GetDateTime("created_at")
                });
            }
            return list;
        }

        public Wallet? GetById(int walletId, int userId)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT wallet_id, user_id, wallet_name, wallet_type, current_balance, color_code, created_at
                FROM Wallets
                WHERE wallet_id = @walletId AND user_id = @userId";
            cmd.Parameters.AddWithValue("@walletId", walletId);
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Wallet
                {
                    WalletId = reader.GetInt32("wallet_id"),
                    UserId = reader.GetInt32("user_id"),
                    WalletName = reader.GetString("wallet_name"),
                    WalletType = reader.GetString("wallet_type"),
                    CurrentBalance = reader.GetDecimal("current_balance"),
                    ColorCode = reader.IsDBNull(reader.GetOrdinal("color_code")) ? "#2563EB" : reader.GetString("color_code"),
                    CreatedAt = reader.GetDateTime("created_at")
                };
            }
            return null;
        }

        public int Add(Wallet wallet)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Wallets (user_id, wallet_name, wallet_type, current_balance, color_code)
                OUTPUT INSERTED.wallet_id
                VALUES (@userId, @name, @type, @balance, @color)";
            cmd.Parameters.AddWithValue("@userId", wallet.UserId);
            cmd.Parameters.AddWithValue("@name", wallet.WalletName.Trim());
            cmd.Parameters.AddWithValue("@type", wallet.WalletType);
            cmd.Parameters.AddWithValue("@balance", wallet.CurrentBalance);
            cmd.Parameters.AddWithValue("@color", wallet.ColorCode ?? "#2563EB");

            return (int)cmd.ExecuteScalar();
        }

        public void Update(Wallet wallet)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE Wallets 
                SET wallet_name = @name, wallet_type = @type, color_code = @color
                WHERE wallet_id = @id AND user_id = @userId";
            cmd.Parameters.AddWithValue("@name", wallet.WalletName.Trim());
            cmd.Parameters.AddWithValue("@type", wallet.WalletType);
            cmd.Parameters.AddWithValue("@color", wallet.ColorCode ?? "#2563EB");
            cmd.Parameters.AddWithValue("@id", wallet.WalletId);
            cmd.Parameters.AddWithValue("@userId", wallet.UserId);

            cmd.ExecuteNonQuery();
        }

        public bool CanDelete(int walletId, int userId, out string reason)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            // Check transactions
            using var cmdTx = conn.CreateCommand();
            cmdTx.CommandText = "SELECT COUNT(1) FROM Transactions WHERE wallet_id = @id AND user_id = @userId";
            cmdTx.Parameters.AddWithValue("@id", walletId);
            cmdTx.Parameters.AddWithValue("@userId", userId);
            int txCount = (int)cmdTx.ExecuteScalar();
            if (txCount > 0)
            {
                reason = $"Dompet tidak dapat dihapus karena memiliki {txCount} riwayat transaksi aktif.";
                return false;
            }

            // Check transfers
            using var cmdTr = conn.CreateCommand();
            cmdTr.CommandText = "SELECT COUNT(1) FROM Transfers WHERE (from_wallet_id = @id OR to_wallet_id = @id) AND user_id = @userId";
            cmdTr.Parameters.AddWithValue("@id", walletId);
            cmdTr.Parameters.AddWithValue("@userId", userId);
            int trCount = (int)cmdTr.ExecuteScalar();
            if (trCount > 0)
            {
                reason = $"Dompet tidak dapat dihapus karena memiliki {trCount} riwayat transfer saldo.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public void Delete(int walletId, int userId)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Wallets WHERE wallet_id = @id AND user_id = @userId";
            cmd.Parameters.AddWithValue("@id", walletId);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.ExecuteNonQuery();
        }

        public decimal GetTotalBalance(int userId)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT ISNULL(SUM(current_balance), 0) FROM Wallets WHERE user_id = @userId";
            cmd.Parameters.AddWithValue("@userId", userId);
            return (decimal)cmd.ExecuteScalar();
        }
    }
}
