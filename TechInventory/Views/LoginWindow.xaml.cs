using System;
using System.IO;
using System.Windows;
using Inventory.Core;
using TechInventory.Helpers;

namespace TechInventory.Views
{
    public partial class LoginWindow : Window
    {
        private AppServices? _services;

        public LoginWindow()
        {
            InitializeComponent();
            _services = App.Services; 
            Loaded += (s, e) => ErrorTextBlock.Text = "";
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
                string passwordHash = PasswordHasher.Hash(password);
                var user = await _services.UserRepository.GetByLoginAsync(login, passwordHash);
                if (user != null)
                {
                    string sessionFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.dat");
                    File.WriteAllText(sessionFile, user.UserID.ToString());

                    var mainWindow = new MainWindow(_services, user);
                    mainWindow.Show();
                    this.Close();

                    Logger.Log($"Пользователь {user.Login} вошёл");
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