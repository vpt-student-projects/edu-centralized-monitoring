using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Inventory.Core.Interfaces;
using Inventory.Core.Models;

namespace TechInventory.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly IUserRepository _userRepo;

        public RegisterWindow(IUserRepository userRepo)
        {
            InitializeComponent();
            _userRepo = userRepo;
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;
            string fullName = FullNameBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Логин и пароль обязательны", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var allUsers = await _userRepo.GetAllAsync();
            if (allUsers.Any(u => u.Login == login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newUser = new User
            {
                Login = login,
                PasswordHash = password,
                FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName,
                Role = "Teacher"
            };

            await _userRepo.AddAsync(newUser);
            MessageBox.Show("Регистрация успешна! Теперь можете войти.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}