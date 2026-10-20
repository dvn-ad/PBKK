using System.Windows;
using Dompetku.ViewModels;

namespace Dompetku.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindow()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;

            TxtPasswordBox.Password = _viewModel.Password;

            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LoginViewModel.Password))
                {
                    if (TxtPasswordBox.Password != _viewModel.Password)
                    {
                        TxtPasswordBox.Password = _viewModel.Password;
                    }
                }
            };

            _viewModel.LoginSuccessful += OnLoginSuccessful;
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = TxtPasswordBox.Password;
            }
        }

        private void OnLoginSuccessful()
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }
    }
}
