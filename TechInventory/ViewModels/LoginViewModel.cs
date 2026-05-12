using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Inventory.Core;
using TechInventory.Helpers;
using TechInventory.Views;

namespace TechInventory.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _login = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoggingIn;
        private AppServices? _services;

        public string Login
        {
            get => _login;
            set { SetProperty(ref _login, value); ErrorMessage = string.Empty; }
        }

        public string Password
        {
            get => _password;
            set { SetProperty(ref _password, value); ErrorMessage = string.Empty; }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoggingIn
        {
            get => _isLoggingIn;
            set
            {
                if (SetProperty(ref _isLoggingIn, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(async _ => await ExecuteLogin(), _ => !IsLoggingIn);
            _ = InitializeServicesAsync();
        }

        private async Task InitializeServicesAsync()
        {
            ErrorMessage = "Проверка подключения к базе данных...";
            try
            {
                string dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "inventory.db");
                string connectionString = $"Data Source={dbPath};Version=3;";
                _services = await AppServices.InitializeAsync(connectionString);
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка БД: {ex.Message}";
            }
        }

        private async Task ExecuteLogin()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Введите логин и пароль";
                return;
            }

            if (_services == null)
            {
                ErrorMessage = "Нет подключения к базе данных";
                return;
            }

            IsLoggingIn = true;
            ErrorMessage = string.Empty;

            try
            {
                var user = await _services.UserRepository.GetByLoginAsync(Login, Password);
                if (user != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var mainWindow = new MainWindow(_services, user);
                        mainWindow.Show();
                        var loginWindow = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
                        loginWindow?.Close();
                    });
                }
                else
                {
                    ErrorMessage = "Неверный логин или пароль";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка входа: {ex.Message}";
            }
            finally
            {
                IsLoggingIn = false;
            }
        }
    }
}