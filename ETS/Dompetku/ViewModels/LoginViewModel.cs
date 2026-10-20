using System;
using System.Windows.Input;
using Dompetku.Services;

namespace Dompetku.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private bool _isRegisterMode;
        public bool IsRegisterMode
        {
            get => _isRegisterMode;
            set
            {
                if (SetProperty(ref _isRegisterMode, value))
                {
                    ErrorMessage = string.Empty;
                    OnPropertyChanged(nameof(HeaderTitle));
                    OnPropertyChanged(nameof(SubmitButtonText));
                    OnPropertyChanged(nameof(ToggleModePrompt));
                    OnPropertyChanged(nameof(ToggleModeActionText));
                }
            }
        }

        private string _username = "admin";
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password = "admin123";
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _fullName = string.Empty;
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string HeaderTitle => IsRegisterMode ? "Buat Akun Baru" : "Masuk ke Dompetku";
        public string SubmitButtonText => IsRegisterMode ? "Daftar Sekarang" : "Masuk";
        public string ToggleModePrompt => IsRegisterMode ? "Sudah memiliki akun?" : "Belum memiliki akun?";
        public string ToggleModeActionText => IsRegisterMode ? "Masuk di sini" : "Daftar";

        public ICommand SubmitCommand { get; }
        public ICommand ToggleModeCommand { get; }

        public event Action? LoginSuccessful;

        public LoginViewModel()
        {
            SubmitCommand = new RelayCommand(ExecuteSubmit);
            ToggleModeCommand = new RelayCommand(ToggleMode);
        }

        private void ToggleMode()
        {
            IsRegisterMode = !IsRegisterMode;
            if (IsRegisterMode)
            {
                Username = string.Empty;
                Password = string.Empty;
                FullName = string.Empty;
            }
            else
            {
                Username = string.Empty;
                Password = string.Empty;
            }
        }

        private void ExecuteSubmit()
        {
            ErrorMessage = string.Empty;

            if (IsRegisterMode)
            {
                if (string.IsNullOrWhiteSpace(FullName))
                {
                    ErrorMessage = "Nama lengkap harus diisi.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(Username) || Username.Trim().Length < 3)
                {
                    ErrorMessage = "Username minimal 3 karakter.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
                {
                    ErrorMessage = "Password minimal 6 karakter.";
                    return;
                }

                if (AuthService.Register(Username, Password, FullName, out string err))
                {
                    LoginSuccessful?.Invoke();
                }
                else
                {
                    ErrorMessage = err;
                }
            }
            else
            {
                if (AuthService.Login(Username, Password, out string err))
                {
                    LoginSuccessful?.Invoke();
                }
                else
                {
                    ErrorMessage = err;
                }
            }
        }
    }
}
