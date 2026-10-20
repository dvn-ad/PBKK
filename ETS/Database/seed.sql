-- =======================================================
-- Database: DompetkuDB
-- Skrip Seeding Data Awal (Demo Data)
-- =======================================================

USE DompetkuDB;
GO

-- 1. Tambah Demo User jika belum ada (username: admin, password: admin123)
IF NOT EXISTS (SELECT * FROM Users WHERE username = 'admin')
BEGIN
    INSERT INTO Users (username, password_hash, full_name)
    VALUES ('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Pengguna Demo');
END
GO

DECLARE @UserId INT;
SELECT @UserId = user_id FROM Users WHERE username = 'admin';

-- 2. Kategori Default
IF NOT EXISTS (SELECT * FROM Categories WHERE user_id = @UserId)
BEGIN
    -- Kategori Pemasukan
    INSERT INTO Categories (user_id, category_name, type, icon_name, color_code) VALUES
    (@UserId, 'Gaji Pokok', 'Income', 'Briefcase', '#16A34A'),
    (@UserId, 'Bonus & Freelance', 'Income', 'Star', '#059669'),
    (@UserId, 'Investasi & Dividen', 'Income', 'TrendingUp', '#0D9488'),
    (@UserId, 'Pemasukan Lainnya', 'Income', 'PlusCircle', '#10B981');

    -- Kategori Pengeluaran
    INSERT INTO Categories (user_id, category_name, type, icon_name, color_code) VALUES
    (@UserId, 'Makanan & Minuman', 'Expense', 'Utensils', '#EF4444'),
    (@UserId, 'Transportasi', 'Expense', 'Car', '#F97316'),
    (@UserId, 'Tagihan & Utilitas', 'Expense', 'Zap', '#F59E0B'),
    (@UserId, 'Belanja Kebutuhan', 'Expense', 'ShoppingBag', '#8B5CF6'),
    (@UserId, 'Hiburan & Liburan', 'Expense', 'Film', '#EC4899'),
    (@UserId, 'Kesehatan', 'Expense', 'Heart', '#E11D48'),
    (@UserId, 'Pendidikan & Kursus', 'Expense', 'BookOpen', '#6366F1');
END
GO

DECLARE @UserId INT;
SELECT @UserId = user_id FROM Users WHERE username = 'admin';

-- 3. Dompet Default
IF NOT EXISTS (SELECT * FROM Wallets WHERE user_id = @UserId)
BEGIN
    INSERT INTO Wallets (user_id, wallet_name, wallet_type, current_balance, color_code) VALUES
    (@UserId, 'Uang Tunai', 'Cash', 750000.00, '#10B981'),
    (@UserId, 'Rekening Bank BCA', 'Bank', 5200000.00, '#2563EB'),
    (@UserId, 'GoPay / OVO', 'E-Wallet', 350000.00, '#8B5CF6');
END
GO

DECLARE @UserId INT;
SELECT @UserId = user_id FROM Users WHERE username = 'admin';

-- 4. Sample Transaksi
IF NOT EXISTS (SELECT * FROM Transactions WHERE user_id = @UserId)
BEGIN
    DECLARE @W_Cash INT, @W_Bca INT, @W_Gopay INT;
    SELECT @W_Cash = wallet_id FROM Wallets WHERE user_id = @UserId AND wallet_type = 'Cash';
    SELECT @W_Bca = wallet_id FROM Wallets WHERE user_id = @UserId AND wallet_type = 'Bank';
    SELECT @W_Gopay = wallet_id FROM Wallets WHERE user_id = @UserId AND wallet_type = 'E-Wallet';

    DECLARE @C_Gaji INT, @C_Makan INT, @C_Transport INT, @C_Tagihan INT, @C_Belanja INT;
    SELECT @C_Gaji = category_id FROM Categories WHERE user_id = @UserId AND category_name = 'Gaji Pokok';
    SELECT @C_Makan = category_id FROM Categories WHERE user_id = @UserId AND category_name = 'Makanan & Minuman';
    SELECT @C_Transport = category_id FROM Categories WHERE user_id = @UserId AND category_name = 'Transportasi';
    SELECT @C_Tagihan = category_id FROM Categories WHERE user_id = @UserId AND category_name = 'Tagihan & Utilitas';
    SELECT @C_Belanja = category_id FROM Categories WHERE user_id = @UserId AND category_name = 'Belanja Kebutuhan';

    -- Pemasukan
    INSERT INTO Transactions (user_id, wallet_id, category_id, transaction_type, amount, transaction_date, description)
    VALUES (@UserId, @W_Bca, @C_Gaji, 'Income', 6000000.00, DATEADD(DAY, -15, GETDATE()), 'Gaji Bulanan');

    -- Pengeluaran
    INSERT INTO Transactions (user_id, wallet_id, category_id, transaction_type, amount, transaction_date, description)
    VALUES 
    (@UserId, @W_Cash, @C_Makan, 'Expense', 50000.00, DATEADD(DAY, -3, GETDATE()), 'Makan siang bersama tim'),
    (@UserId, @W_Gopay, @C_Transport, 'Expense', 35000.00, DATEADD(DAY, -2, GETDATE()), 'Ojek online ke kampus/kantor'),
    (@UserId, @W_Bca, @C_Tagihan, 'Expense', 450000.00, DATEADD(DAY, -5, GETDATE()), 'Bayar listrik & internet WiFi'),
    (@UserId, @W_Bca, @C_Belanja, 'Expense', 265000.00, DATEADD(DAY, -1, GETDATE()), 'Supermarket mingguan');
END
GO

DECLARE @UserId INT;
SELECT @UserId = user_id FROM Users WHERE username = 'admin';

-- 5. Sample Budget Bulan Ini
IF NOT EXISTS (SELECT * FROM Budgets WHERE user_id = @UserId)
BEGIN
    DECLARE @C_Makan INT, @C_Transport INT, @C_Belanja INT;
    SELECT @C_Makan = category_id FROM Categories WHERE user_id = @UserId AND category_name = 'Makanan & Minuman';
    SELECT @C_Transport = category_id FROM Categories WHERE user_id = @UserId AND category_name = 'Transportasi';
    SELECT @C_Belanja = category_id FROM Categories WHERE user_id = @UserId AND category_name = 'Belanja Kebutuhan';

    DECLARE @CurMonth INT = MONTH(GETDATE());
    DECLARE @CurYear INT = YEAR(GETDATE());

    INSERT INTO Budgets (user_id, category_id, limit_amount, period_month, period_year)
    VALUES 
    (@UserId, @C_Makan, 1500000.00, @CurMonth, @CurYear),
    (@UserId, @C_Transport, 600000.00, @CurMonth, @CurYear),
    (@UserId, @C_Belanja, 1000000.00, @CurMonth, @CurYear);
END
GO
