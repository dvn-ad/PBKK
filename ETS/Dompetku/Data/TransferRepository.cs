using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Dompetku.Models;

namespace Dompetku.Data
{
    public class TransferRepository
    {
        public List<Transfer> GetAll(int userId)
        {
            var list = new List<Transfer>();
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT t.transfer_id, t.user_id, t.from_wallet_id, t.to_wallet_id, 
                       t.amount, t.admin_fee, t.transfer_date, t.notes, t.created_at,
                       w1.wallet_name AS from_wallet_name,
                       w2.wallet_name AS to_wallet_name
                FROM Transfers t
                INNER JOIN Wallets w1 ON t.from_wallet_id = w1.wallet_id
                INNER JOIN Wallets w2 ON t.to_wallet_id = w2.wallet_id
                WHERE t.user_id = @userId
                ORDER BY t.transfer_date DESC, t.transfer_id DESC";
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Transfer
                {
                    TransferId = reader.GetInt64("transfer_id"),
                    UserId = reader.GetInt32("user_id"),
                    FromWalletId = reader.GetInt32("from_wallet_id"),
                    ToWalletId = reader.GetInt32("to_wallet_id"),
                    Amount = reader.GetDecimal("amount"),
                    AdminFee = reader.GetDecimal("admin_fee"),
                    TransferDate = reader.GetDateTime("transfer_date"),
                    Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? string.Empty : reader.GetString("notes"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    FromWalletName = reader.GetString("from_wallet_name"),
                    ToWalletName = reader.GetString("to_wallet_name")
                });
            }
            return list;
        }

        public void Add(Transfer transfer)
        {
            if (transfer.FromWalletId == transfer.ToWalletId)
            {
                throw new ArgumentException("Dompet asal dan tujuan tidak boleh sama.");
            }

            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                // 1. Check if source wallet has sufficient balance
                using (var cmdCheck = conn.CreateCommand())
                {
                    cmdCheck.Transaction = tx;
                    cmdCheck.CommandText = "SELECT current_balance FROM Wallets WHERE wallet_id = @id AND user_id = @userId";
                    cmdCheck.Parameters.AddWithValue("@id", transfer.FromWalletId);
                    cmdCheck.Parameters.AddWithValue("@userId", transfer.UserId);
                    var bal = (decimal)(cmdCheck.ExecuteScalar() ?? 0m);
                    if (bal < (transfer.Amount + transfer.AdminFee))
                    {
                        throw new InvalidOperationException($"Saldo dompet asal tidak mencukupi (Tersedia: Rp {bal:N0}, Dibutuhkan: Rp {transfer.Amount + transfer.AdminFee:N0}).");
                    }
                }

                // 2. Insert Transfer record
                using (var cmdIns = conn.CreateCommand())
                {
                    cmdIns.Transaction = tx;
                    cmdIns.CommandText = @"
                        INSERT INTO Transfers (user_id, from_wallet_id, to_wallet_id, amount, admin_fee, transfer_date, notes)
                        OUTPUT INSERTED.transfer_id
                        VALUES (@userId, @fromId, @toId, @amount, @fee, @date, @notes)";
                    cmdIns.Parameters.AddWithValue("@userId", transfer.UserId);
                    cmdIns.Parameters.AddWithValue("@fromId", transfer.FromWalletId);
                    cmdIns.Parameters.AddWithValue("@toId", transfer.ToWalletId);
                    cmdIns.Parameters.AddWithValue("@amount", transfer.Amount);
                    cmdIns.Parameters.AddWithValue("@fee", transfer.AdminFee);
                    cmdIns.Parameters.AddWithValue("@date", transfer.TransferDate);
                    cmdIns.Parameters.AddWithValue("@notes", (object?)transfer.Notes ?? DBNull.Value);
                    transfer.TransferId = (long)cmdIns.ExecuteScalar();
                }

                // 3. Deduct from source wallet
                using (var cmdDeduct = conn.CreateCommand())
                {
                    cmdDeduct.Transaction = tx;
                    cmdDeduct.CommandText = "UPDATE Wallets SET current_balance = current_balance - @total WHERE wallet_id = @id";
                    cmdDeduct.Parameters.AddWithValue("@total", transfer.Amount + transfer.AdminFee);
                    cmdDeduct.Parameters.AddWithValue("@id", transfer.FromWalletId);
                    cmdDeduct.ExecuteNonQuery();
                }

                // 4. Add to destination wallet
                using (var cmdAdd = conn.CreateCommand())
                {
                    cmdAdd.Transaction = tx;
                    cmdAdd.CommandText = "UPDATE Wallets SET current_balance = current_balance + @amount WHERE wallet_id = @id";
                    cmdAdd.Parameters.AddWithValue("@amount", transfer.Amount);
                    cmdAdd.Parameters.AddWithValue("@id", transfer.ToWalletId);
                    cmdAdd.ExecuteNonQuery();
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
