using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Dompetku.Models;

namespace Dompetku.Data
{
    public class CategoryRepository
    {
        public List<Category> GetAll(int userId)
        {
            var list = new List<Category>();
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT category_id, user_id, category_name, type, icon_name, color_code
                FROM Categories
                WHERE user_id = @userId
                ORDER BY type DESC, category_name ASC";
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Category
                {
                    CategoryId = reader.GetInt32("category_id"),
                    UserId = reader.GetInt32("user_id"),
                    CategoryName = reader.GetString("category_name"),
                    Type = reader.GetString("type"),
                    IconName = reader.IsDBNull(reader.GetOrdinal("icon_name")) ? "Tag" : reader.GetString("icon_name"),
                    ColorCode = reader.IsDBNull(reader.GetOrdinal("color_code")) ? "#64748B" : reader.GetString("color_code")
                });
            }
            return list;
        }

        public List<Category> GetByType(int userId, string type)
        {
            var list = new List<Category>();
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT category_id, user_id, category_name, type, icon_name, color_code
                FROM Categories
                WHERE user_id = @userId AND type = @type
                ORDER BY category_name ASC";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@type", type);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Category
                {
                    CategoryId = reader.GetInt32("category_id"),
                    UserId = reader.GetInt32("user_id"),
                    CategoryName = reader.GetString("category_name"),
                    Type = reader.GetString("type"),
                    IconName = reader.IsDBNull(reader.GetOrdinal("icon_name")) ? "Tag" : reader.GetString("icon_name"),
                    ColorCode = reader.IsDBNull(reader.GetOrdinal("color_code")) ? "#64748B" : reader.GetString("color_code")
                });
            }
            return list;
        }

        public int Add(Category category)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Categories (user_id, category_name, type, icon_name, color_code)
                OUTPUT INSERTED.category_id
                VALUES (@userId, @name, @type, @icon, @color)";
            cmd.Parameters.AddWithValue("@userId", category.UserId);
            cmd.Parameters.AddWithValue("@name", category.CategoryName.Trim());
            cmd.Parameters.AddWithValue("@type", category.Type);
            cmd.Parameters.AddWithValue("@icon", (object?)category.IconName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@color", category.ColorCode ?? "#64748B");

            return (int)cmd.ExecuteScalar();
        }

        public void Update(Category category)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE Categories
                SET category_name = @name, type = @type, color_code = @color, icon_name = @icon
                WHERE category_id = @id AND user_id = @userId";
            cmd.Parameters.AddWithValue("@name", category.CategoryName.Trim());
            cmd.Parameters.AddWithValue("@type", category.Type);
            cmd.Parameters.AddWithValue("@color", category.ColorCode ?? "#64748B");
            cmd.Parameters.AddWithValue("@icon", (object?)category.IconName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", category.CategoryId);
            cmd.Parameters.AddWithValue("@userId", category.UserId);

            cmd.ExecuteNonQuery();
        }

        public bool CanDelete(int categoryId, int userId, out string reason)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            using var cmdTx = conn.CreateCommand();
            cmdTx.CommandText = "SELECT COUNT(1) FROM Transactions WHERE category_id = @id AND user_id = @userId";
            cmdTx.Parameters.AddWithValue("@id", categoryId);
            cmdTx.Parameters.AddWithValue("@userId", userId);
            int txCount = (int)cmdTx.ExecuteScalar();
            if (txCount > 0)
            {
                reason = $"Kategori ini tidak dapat dihapus karena telah digunakan pada {txCount} transaksi.";
                return false;
            }

            using var cmdBg = conn.CreateCommand();
            cmdBg.CommandText = "SELECT COUNT(1) FROM Budgets WHERE category_id = @id AND user_id = @userId";
            cmdBg.Parameters.AddWithValue("@id", categoryId);
            cmdBg.Parameters.AddWithValue("@userId", userId);
            int bgCount = (int)cmdBg.ExecuteScalar();
            if (bgCount > 0)
            {
                reason = $"Kategori ini tidak dapat dihapus karena terdaftar dalam {bgCount} anggaran aktif.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public void Delete(int categoryId, int userId)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Categories WHERE category_id = @id AND user_id = @userId";
            cmd.Parameters.AddWithValue("@id", categoryId);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.ExecuteNonQuery();
        }
    }
}
