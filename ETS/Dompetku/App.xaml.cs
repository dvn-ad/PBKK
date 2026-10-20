using System.Windows;
using Dompetku.Data;
using Dompetku.Views;

namespace Dompetku
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ensure database and schema are ready
            DatabaseInitializer.Initialize();

            // Show Login Window
            var loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}
