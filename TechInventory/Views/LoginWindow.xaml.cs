using System;
using System.Windows;
using Inventory.Core;

namespace TechInventory.Views
{
    public partial class LoginWindow : Window
    {
        private AppServices? _services;

        public LoginWindow()
        {
            InitializeComponent();
            Loaded += async (s, e) => await InitializeServicesAsync();
        }

        private async Task InitializeServicesAsync()
        {
            try
            {
                string dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "inventory.db");
                string connectionString = $"Data Source={dbPath};Version=3;";

                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();
                    await conn.CloseAsync();
                }

                _services = new AppServices(connectionString);
                ErrorTextBlock.Text = "";
            }
            catch (Exception ex)
            {
                ErrorTextBlock.Text = $"Ошибка открытия БД: {ex.Message}\nТип ошибки: {ex.GetType()}\n{ex.StackTrace}";
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ErrorTextBlock.Text = "Введите логин и пароль";
                return;
            }

            if (_services == null)
            {
                ErrorTextBlock.Text = "Нет подключения к базе данных";
                return;
            }

            try
            {
                var user = await _services.UserRepository.GetByLoginAsync(login, password);
                if (user != null)
                {
                    var mainWindow = new MainWindow(_services, user);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    ErrorTextBlock.Text = "Неверный логин или пароль";
                }
            }
            catch (Exception ex)
            {
                ErrorTextBlock.Text = $"Ошибка входа: {ex.Message}";
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (_services == null)
            {
                ErrorTextBlock.Text = "Нет подключения к БД";
                return;
            }
            var regWindow = new RegisterWindow(_services.UserRepository);
            regWindow.Owner = this;
            regWindow.ShowDialog();
        }
    }
}