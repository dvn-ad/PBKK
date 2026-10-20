using System;
using System.Security.Cryptography;
using System.Text;
using Dompetku.Data;
using Dompetku.Models;

namespace Dompetku.Services
{
    public static class AuthService
    {
        private static readonly UserRepository _userRepo = new();

        public static User? CurrentUser { get; private set; }

        public static bool IsLoggedIn => CurrentUser != null;

        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = sha.ComputeHash(bytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        public static bool Login(string username, string password, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Username dan password tidak boleh kosong.";
                return false;
            }

            try
            {
                var hash = HashPassword(password);
                var user = _userRepo.Login(username, hash);
                if (user != null)
                {
                    CurrentUser = user;
                    return true;
                }
                errorMessage = "Username atau password salah.";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Gagal terhubung ke database: {ex.Message}";
                return false;
            }
        }

        public static bool Register(string username, string password, string fullName, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(username) || username.Trim().Length < 3)
            {
                errorMessage = "Username minimal 3 karakter.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                errorMessage = "Password minimal 6 karakter.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(fullName))
            {
                errorMessage = "Nama lengkap harus diisi.";
                return false;
            }

            try
            {
                if (_userRepo.UsernameExists(username))
                {
                    errorMessage = "Username sudah digunakan oleh akun lain.";
                    return false;
                }

                var hash = HashPassword(password);
                var newUser = _userRepo.Register(username, hash, fullName);
                CurrentUser = newUser;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Registrasi gagal: {ex.Message}";
                return false;
            }
        }

        public static void Logout()
        {
            CurrentUser = null;
        }

        public static void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }
    }
}
