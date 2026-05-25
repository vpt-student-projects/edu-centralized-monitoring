using System;
using System.IO;
using System.Windows;
using Inventory.Core;
using TechInventory.Helpers;

namespace TechInventory
{
    public partial class App : Application
    {
        public static AppServices? Services { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "inventory.db");
            string connectionString = $"Data Source={dbPath};Version=3;";
            Services = new AppServices(connectionString);
            bool connected = await Services.TestConnectionAsync();
            if (!connected)
            {
                MessageBox.Show("Не удалось подключиться к базе данных.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            string sessionFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.dat");
            if (File.Exists(sessionFile))
            {
                string userIdStr = File.ReadAllText(sessionFile);
                if (int.TryParse(userIdStr, out int userId))
                {
                    var user = await Services.UserRepository.GetByIdAsync(userId);
                    if (user != null)
                    {
                        var mainWindow = new Views.MainWindow(Services, user);
                        mainWindow.Show();
                        return;
                    }
                }
                File.Delete(sessionFile);
            }

            var loginWindow = new Views.LoginWindow();
            loginWindow.Show();
        }
    }
}