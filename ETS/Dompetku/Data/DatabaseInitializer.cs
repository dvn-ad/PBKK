using System;
using Microsoft.Data.SqlClient;

namespace Dompetku.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            try
            {
                // 1. Ensure Database exists using master connection
                var masterConnStr = "Server=.\\SQLEXPRESS;Database=master;Integrated Security=True;TrustServerCertificate=True;";
                using (var masterConn = new SqlConnection(masterConnStr))
                {
                    masterConn.Open();
                    using var cmd = masterConn.CreateCommand();
                    cmd.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'DompetkuDB')
                        BEGIN
                            CREATE DATABASE DompetkuDB;
                        END";
                    cmd.ExecuteNonQuery();
                }

                // 2. Ensure Tables exist
                using (var conn = DatabaseConfig.GetConnection())
                {
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
                        BEGIN
                            CREATE TABLE Users (
                                user_id INT IDENTITY(1,1) PRIMARY KEY,
                                username NVARCHAR(50) NOT NULL UNIQUE,
                                password_hash NVARCHAR(255) NOT NULL,
                                full_name NVARCHAR(100) NOT NULL,
                                created_at DATETIME DEFAULT GETDATE()
                            );
                        END;

                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Wallets')
                        BEGIN
                            CREATE TABLE Wallets (
                                wallet_id INT IDENTITY(1,1) PRIMARY KEY,
                                user_id INT NOT NULL,
                                wallet_name NVARCHAR(50) NOT NULL,
                                wallet_type NVARCHAR(30) NOT NULL,
                                current_balance DECIMAL(18,2) NOT NULL DEFAULT 0.00,
                                color_code NVARCHAR(7) DEFAULT '#2563EB',
                                created_at DATETIME DEFAULT GETDATE(),
                                CONSTRAINT FK_Wallets_Users FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
                            );
                        END;

                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
                        BEGIN
                            CREATE TABLE Categories (
                                category_id INT IDENTITY(1,1) PRIMARY KEY,
                                user_id INT NOT NULL,
                                category_name NVARCHAR(50) NOT NULL,
                                type NVARCHAR(10) NOT NULL,
                                icon_name NVARCHAR(50) NULL,
                                color_code NVARCHAR(7) DEFAULT '#64748B',
                                CONSTRAINT FK_Categories_Users FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
                            );
                        END;

                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Transactions')
                        BEGIN
                            CREATE TABLE Transactions (
                                transaction_id BIGINT IDENTITY(1,1) PRIMARY KEY,
                                user_id INT NOT NULL,
                                wallet_id INT NOT NULL,
                                category_id INT NOT NULL,
                                transaction_type NVARCHAR(10) NOT NULL,
                                amount DECIMAL(18,2) NOT NULL CHECK (amount > 0),
                                transaction_date DATETIME NOT NULL DEFAULT GETDATE(),
                                description NVARCHAR(255) NULL,
                                created_at DATETIME DEFAULT GETDATE(),
                                CONSTRAINT FK_Transactions_Users FOREIGN KEY (user_id) REFERENCES Users(user_id),
                                CONSTRAINT FK_Transactions_Wallets FOREIGN KEY (wallet_id) REFERENCES Wallets(wallet_id),
                                CONSTRAINT FK_Transactions_Categories FOREIGN KEY (category_id) REFERENCES Categories(category_id)
                            );
                        END;

                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Transfers')
                        BEGIN
                            CREATE TABLE Transfers (
                                transfer_id BIGINT IDENTITY(1,1) PRIMARY KEY,
                                user_id INT NOT NULL,
                                from_wallet_id INT NOT NULL,
                                to_wallet_id INT NOT NULL,
                                amount DECIMAL(18,2) NOT NULL CHECK (amount > 0),
                                admin_fee DECIMAL(18,2) NOT NULL DEFAULT 0.00 CHECK (admin_fee >= 0),
                                transfer_date DATETIME NOT NULL DEFAULT GETDATE(),
                                notes NVARCHAR(255) NULL,
                                created_at DATETIME DEFAULT GETDATE(),
                                CONSTRAINT FK_Transfers_Users FOREIGN KEY (user_id) REFERENCES Users(user_id),
                                CONSTRAINT FK_Transfers_FromWallet FOREIGN KEY (from_wallet_id) REFERENCES Wallets(wallet_id),
                                CONSTRAINT FK_Transfers_ToWallet FOREIGN KEY (to_wallet_id) REFERENCES Wallets(wallet_id)
                            );
                        END;

                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Budgets')
                        BEGIN
                            CREATE TABLE Budgets (
                                budget_id INT IDENTITY(1,1) PRIMARY KEY,
                                user_id INT NOT NULL,
                                category_id INT NOT NULL,
                                limit_amount DECIMAL(18,2) NOT NULL CHECK (limit_amount > 0),
                                period_month INT NOT NULL,
                                period_year INT NOT NULL,
                                created_at DATETIME DEFAULT GETDATE(),
                                CONSTRAINT FK_Budgets_Users FOREIGN KEY (user_id) REFERENCES Users(user_id),
                                CONSTRAINT FK_Budgets_Categories FOREIGN KEY (category_id) REFERENCES Categories(category_id),
                                CONSTRAINT UQ_User_Category_Period UNIQUE (user_id, category_id, period_month, period_year)
                            );
                        END;
                    ";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DB Initialization Error: {ex.Message}");
            }
        }
    }
}
