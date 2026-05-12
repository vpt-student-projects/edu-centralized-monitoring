using System;
using System.Windows;
using System.Windows.Controls;
using Inventory.Core.Models;

namespace TechInventory.Views
{
    public partial class UserEditWindow : Window
    {
        public User User { get; private set; }
        private readonly bool _isNewUser;

        public UserEditWindow(User? user = null)
        {
            InitializeComponent();
            if (user != null)
            {
                _isNewUser = false;
                User = user;
                LoginBox.Text = user.Login;
                FullNameBox.Text = user.FullName ?? "";
                foreach (ComboBoxItem item in RoleCombo.Items)
                {
                    if ((string)item.Tag == user.Role)
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
            }
            else
            {
                _isNewUser = true;
                User = new User();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;
            string fullName = FullNameBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Логин обязателен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_isNewUser && string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пароль обязателен для нового пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            User.Login = login;
            User.FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName;
            User.Role = ((ComboBoxItem)RoleCombo.SelectedItem).Tag.ToString();

            if (!string.IsNullOrWhiteSpace(password))
                User.PasswordHash = password; // без хеширования для MVP

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}