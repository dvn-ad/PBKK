-- =======================================================
-- Database: DompetkuDB
-- Skrip DDL Pembuatan Database dan Tabel Sistem DOMPETKU
-- =======================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'DompetkuDB')
BEGIN
    CREATE DATABASE DompetkuDB;
END
GO

USE DompetkuDB;
GO

-- 1. Tabel Users
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        user_id INT IDENTITY(1,1) PRIMARY KEY,
        username NVARCHAR(50) NOT NULL UNIQUE,
        password_hash NVARCHAR(255) NOT NULL,
        full_name NVARCHAR(100) NOT NULL,
        created_at DATETIME DEFAULT GETDATE()
    );
END
GO

-- 2. Tabel Wallets (Dompet / Akun Finansial)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Wallets')
BEGIN
    CREATE TABLE Wallets (
        wallet_id INT IDENTITY(1,1) PRIMARY KEY,
        user_id INT NOT NULL,
        wallet_name NVARCHAR(50) NOT NULL,
        wallet_type NVARCHAR(30) NOT NULL, -- 'Cash', 'Bank', 'E-Wallet', 'Other'
        current_balance DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        color_code NVARCHAR(7) DEFAULT '#2563EB',
        created_at DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_Wallets_Users FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
    );
END
GO

-- 3. Tabel Categories (Kategori Pemasukan & Pengeluaran)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
BEGIN
    CREATE TABLE Categories (
        category_id INT IDENTITY(1,1) PRIMARY KEY,
        user_id INT NOT NULL,
        category_name NVARCHAR(50) NOT NULL,
        type NVARCHAR(10) NOT NULL, -- 'Income' atau 'Expense'
        icon_name NVARCHAR(50) NULL,
        color_code NVARCHAR(7) DEFAULT '#64748B',
        CONSTRAINT FK_Categories_Users FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
    );
END
GO

-- 4. Tabel Transactions (Transaksi Mutasi)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Transactions')
BEGIN
    CREATE TABLE Transactions (
        transaction_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        user_id INT NOT NULL,
        wallet_id INT NOT NULL,
        category_id INT NOT NULL,
        transaction_type NVARCHAR(10) NOT NULL, -- 'Income' atau 'Expense'
        amount DECIMAL(18,2) NOT NULL CHECK (amount > 0),
        transaction_date DATETIME NOT NULL DEFAULT GETDATE(),
        description NVARCHAR(255) NULL,
        created_at DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_Transactions_Users FOREIGN KEY (user_id) REFERENCES Users(user_id),
        CONSTRAINT FK_Transactions_Wallets FOREIGN KEY (wallet_id) REFERENCES Wallets(wallet_id),
        CONSTRAINT FK_Transactions_Categories FOREIGN KEY (category_id) REFERENCES Categories(category_id)
    );
END
GO

-- 5. Tabel Transfers (Mutasi Antar Dompet)
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
        CONSTRAINT FK_Transfers_ToWallet FOREIGN KEY (to_wallet_id) REFERENCES Wallets(wallet_id),
        CONSTRAINT CHK_Different_Wallets CHECK (from_wallet_id <> to_wallet_id)
    );
END
GO

-- 6. Tabel Budgets (Pagu Anggaran Bulanan)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Budgets')
BEGIN
    CREATE TABLE Budgets (
        budget_id INT IDENTITY(1,1) PRIMARY KEY,
        user_id INT NOT NULL,
        category_id INT NOT NULL,
        limit_amount DECIMAL(18,2) NOT NULL CHECK (limit_amount > 0),
        period_month INT NOT NULL CHECK (period_month BETWEEN 1 AND 12),
        period_year INT NOT NULL CHECK (period_year >= 2020),
        created_at DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_Budgets_Users FOREIGN KEY (user_id) REFERENCES Users(user_id),
        CONSTRAINT FK_Budgets_Categories FOREIGN KEY (category_id) REFERENCES Categories(category_id),
        CONSTRAINT UQ_User_Category_Period UNIQUE (user_id, category_id, period_month, period_year)
    );
END
GO
