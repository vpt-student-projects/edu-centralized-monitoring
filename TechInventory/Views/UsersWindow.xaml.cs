using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Inventory.Core.Interfaces;
using TechInventory.ViewModels;

namespace TechInventory.Views
{
    public partial class UsersWindow : Window
    {
        private readonly IUserRepository _userRepo;
        public ObservableCollection<UserListItem> Users { get; } = new();

        public UsersWindow(IUserRepository userRepo)
        {
            InitializeComponent();
            _userRepo = userRepo;
            DataContext = this;
            Loaded += async (s, e) => await LoadUsers();
        }

        private async Task LoadUsers()
        {
            var all = await _userRepo.GetAllAsync();
            Users.Clear();
            foreach (var u in all)
            {
                Users.Add(new UserListItem
                {
                    UserID = u.UserID,
                    Login = u.Login,
                    FullName = u.FullName ?? "–",
                    Role = u.Role
                });
            }
        }

        private async void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new UserEditWindow();
            dlg.Owner = this;
            if (dlg.ShowDialog() == true)
            {
                await _userRepo.AddAsync(dlg.User);
                await LoadUsers();
            }
        }

        private async void EditUser_Click(object sender, RoutedEventArgs e)
        {
            var selected = UsersListView.SelectedItem as UserListItem;
            if (selected == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var fullUser = await _userRepo.GetByIdAsync(selected.UserID);
            if (fullUser == null) return;

            var dlg = new UserEditWindow(fullUser);
            dlg.Owner = this;
            if (dlg.ShowDialog() == true)
            {
                await _userRepo.UpdateAsync(fullUser);
                await LoadUsers();
            }
        }

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var selected = UsersListView.SelectedItem as UserListItem;
            if (selected == null)
            {
                MessageBox.Show("Выберите пользователя для удаления", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Удалить пользователя '{selected.Login}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                await _userRepo.DeleteAsync(selected.UserID);
                await LoadUsers();
            }
        }
    }

    public class UserListItem : ViewModelBase
    {
        public int UserID { get; set; }
        public string Login { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "";
    }
}