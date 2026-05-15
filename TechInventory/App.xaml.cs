using System.Windows;
using TechInventory.Helpers;

namespace TechInventory
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += (s, ex) =>
            {
                Logger.Log("Необработанное исключение: " + ex.Exception.ToString());
                MessageBox.Show($"Произошла ошибка: {ex.Exception.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                ex.Handled = true;
            };

            var loginWindow = new Views.LoginWindow();
            loginWindow.Show();
        }
    }
}