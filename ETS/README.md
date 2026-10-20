# DOMPETKU – Sistem Informasi Keuangan Pribadi

Aplikasi Desktop Windows modern untuk manajemen keuangan pribadi berbasis **WPF (C#, XAML, .NET 9)**, **ADO.NET**, dan **Microsoft SQL Server**.

---

## 🛠️ Tech Stack & Arsitektur
- **Platform:** Windows Desktop (WPF - Windows Presentation Foundation)
- **Framework & Bahasa:** .NET 9.0, C# 13, XAML
- **Data Access:** Native ADO.NET (`Microsoft.Data.SqlClient`) dengan parameterized queries & `SqlTransaction`
- **Database Engine:** Microsoft SQL Server (LocalDB / Express / Developer)
- **Design Pattern:** Layered MVVM (Model - View - ViewModel - Repository)

---

## 🚀 Fitur Utama
1. **Autentikasi & Akun Pengguna:**
   - Registrasi & Login akun lokal.
   - Keamanan hashing password menggunakan algoritma **SHA-256**.
   - Otomatis membuatkan dompet awal & 11 kategori standar saat registrasi akun baru.
2. **Manajemen Multi-Dompet / Rekening:**
   - Mendukung tipe Kas Tunai, Rekening Bank, Dompet Digital (E-Wallet), dan Lainnya.
   - Pengecekan saldo real-time.
   - Validasi integritas relasional sebelum penghapusan dompet.
3. **Transfer Saldo Antar Dompet:**
   - Perpindahan saldo antar-rekening/dompet secara atomik (`SqlTransaction`).
   - Mendukung biaya admin transaksi.
   - Validasi saldo mencukupi sebelum transfer.
4. **Pencatatan Pemasukan & Pengeluaran (Ledger):**
   - Transaksi Pemasukan (+) dan Pengeluaran (-) dengan nominal, kategori, dompet, tanggal, dan catatan.
   - Saldo dompet otomatis tersinkronisasi saat transaksi dibuat atau dihapus.
   - Filter komprehensif: rentang tanggal, dompet, kategori, jenis transaksi, dan pencarian teks catatan.
   - Tombol **Ekspor CSV** untuk ekspor data mutasi.
5. **Kontrol Pagu Anggaran (Budgeting Alert):**
   - Batas anggaran bulanan per kategori.
   - Perhitungan persentase realisasi anggaran real-time.
   - Indikator status visual:
     - 🟢 **Aman** (< 80%)
     - 🟠 **Perhatian** (80% - 100%)
     - 🔴 **Overbudget** (> 100%)
6. **Laporan & Dashboard Analitik:**
   - Ringkasan 4 metrik utama: Total Saldo, Pemasukan Bulan Berjalan, Pengeluaran Bulan Berjalan, dan Arus Kas Bersih (Net Cash Flow).
   - Visualisasi persentase distribusi pengeluaran per kategori.
   - Tabel tren arus kas 6 bulan terakhir.
7. **Kelola Kategori Fleksibel:**
   - Tambah, ubah, dan hapus pos kategori pemasukan & pengeluaran disertai kode warna.

---

## 🔑 Kredensial Demo Awal
Aplikasi telah dilengkapi demo data yang siap langsung digunakan:
- **Username:** `admin`
- **Password:** `admin123`

*(Anda juga dapat mendaftarkan akun baru langsung dari menu registrasi di halaman Login)*

---

## 🗄️ Konfigurasi Database SQL Server
File konfigurasi koneksi berada di:
[`Dompetku/App.config`](file:///c:/Users/pin/CODES/PBKK/ETS/Dompetku/App.config)
```xml
<connectionStrings>
  <add name="DompetkuConnection" 
       connectionString="Server=.\SQLEXPRESS;Database=DompetkuDB;Integrated Security=True;TrustServerCertificate=True;" 
       providerName="Microsoft.Data.SqlClient" />
</connectionStrings>
```
> **Catatan:** Saat aplikasi pertama kali dijalankan, `DatabaseInitializer` akan secara otomatis memeriksa dan membuat database `DompetkuDB` serta tabel-tabelnya jika belum ada di SQL Server lokal Anda. Skrip DDL dan Seeding manual juga tersedia di folder [`Database/`](file:///c:/Users/pin/CODES/PBKK/ETS/Database).

---

## 💻 Cara Menjalankan Aplikasi
1. Buka PowerShell atau Terminal pada direktori `c:\Users\pin\CODES\PBKK\ETS\Dompetku`.
2. Jalankan perintah:
   ```powershell
   dotnet run
   ```
3. Atau buka file solusi [`Dompetku.sln`](file:///c:/Users/pin/CODES/PBKK/ETS/Dompetku.sln) menggunakan Visual Studio 2022 / JetBrains Rider / VS Code, lalu tekan **F5 (Start Debugging)**.
