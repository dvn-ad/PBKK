using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Dompetku.Models;

namespace Dompetku.Data
{
    public class UserRepository
    {
        public bool UsernameExists(string username)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(1) FROM Users WHERE LOWER(username) = LOWER(@username)";
            cmd.Parameters.AddWithValue("@username", username.Trim());
            var count = (int)cmd.ExecuteScalar();
            return count > 0;
        }

        public User? Login(string username, string passwordHash)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT user_id, username, password_hash, full_name, created_at 
                FROM Users 
                WHERE LOWER(username) = LOWER(@username) AND password_hash = @passwordHash";
            cmd.Parameters.AddWithValue("@username", username.Trim());
            cmd.Parameters.AddWithValue("@passwordHash", passwordHash);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    UserId = reader.GetInt32("user_id"),
                    Username = reader.GetString("username"),
                    PasswordHash = reader.GetString("password_hash"),
                    FullName = reader.GetString("full_name"),
                    CreatedAt = reader.GetDateTime("created_at")
                };
            }
            return null;
        }

        public User Register(string username, string passwordHash, string fullName)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                // 1. Insert User
                using var cmdUser = conn.CreateCommand();
                cmdUser.Transaction = tx;
                cmdUser.CommandText = @"
                    INSERT INTO Users (username, password_hash, full_name)
                    OUTPUT INSERTED.user_id, INSERTED.created_at
                    VALUES (@username, @passwordHash, @fullName)";
                cmdUser.Parameters.AddWithValue("@username", username.Trim());
                cmdUser.Parameters.AddWithValue("@passwordHash", passwordHash);
                cmdUser.Parameters.AddWithValue("@fullName", fullName.Trim());

                int newUserId = 0;
                DateTime createdAt = DateTime.Now;
                using (var r = cmdUser.ExecuteReader())
                {
                    if (r.Read())
                    {
                        newUserId = r.GetInt32(0);
                        createdAt = r.GetDateTime(1);
                    }
                }

                // 2. Seed Default Categories for this user
                using var cmdCat = conn.CreateCommand();
                cmdCat.Transaction = tx;
                cmdCat.CommandText = @"
                    INSERT INTO Categories (user_id, category_name, type, icon_name, color_code) VALUES
                    (@uid, 'Gaji Pokok', 'Income', 'Briefcase', '#16A34A'),
                    (@uid, 'Bonus & Freelance', 'Income', 'Star', '#059669'),
                    (@uid, 'Investasi & Dividen', 'Income', 'TrendingUp', '#0D9488'),
                    (@uid, 'Pemasukan Lainnya', 'Income', 'PlusCircle', '#10B981'),
                    (@uid, 'Makanan & Minuman', 'Expense', 'Utensils', '#EF4444'),
                    (@uid, 'Transportasi', 'Expense', 'Car', '#F97316'),
                    (@uid, 'Tagihan & Utilitas', 'Expense', 'Zap', '#F59E0B'),
                    (@uid, 'Belanja Kebutuhan', 'Expense', 'ShoppingBag', '#8B5CF6'),
                    (@uid, 'Hiburan & Liburan', 'Expense', 'Film', '#EC4899'),
                    (@uid, 'Kesehatan', 'Expense', 'Heart', '#E11D48'),
                    (@uid, 'Pendidikan & Kursus', 'Expense', 'BookOpen', '#6366F1');";
                cmdCat.Parameters.AddWithValue("@uid", newUserId);
                cmdCat.ExecuteNonQuery();

                // 3. Seed Default Wallet
                using var cmdWallet = conn.CreateCommand();
                cmdWallet.Transaction = tx;
                cmdWallet.CommandText = @"
                    INSERT INTO Wallets (user_id, wallet_name, wallet_type, current_balance, color_code)
                    VALUES (@uid, 'Uang Tunai (Cash)', 'Cash', 0.00, '#10B981');";
                cmdWallet.Parameters.AddWithValue("@uid", newUserId);
                cmdWallet.ExecuteNonQuery();

                tx.Commit();

                return new User
                {
                    UserId = newUserId,
                    Username = username.Trim(),
                    PasswordHash = passwordHash,
                    FullName = fullName.Trim(),
                    CreatedAt = createdAt
                };
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void UpdateProfile(int userId, string fullName)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Users SET full_name = @fullName WHERE user_id = @userId";
            cmd.Parameters.AddWithValue("@fullName", fullName.Trim());
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.ExecuteNonQuery();
        }

        public void UpdatePassword(int userId, string newPasswordHash)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Users SET password_hash = @hash WHERE user_id = @userId";
            cmd.Parameters.AddWithValue("@hash", newPasswordHash);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.ExecuteNonQuery();
        }
    }
}
