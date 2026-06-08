using System.Windows;
using System.Windows.Input;
using Inventory.Core;
using Inventory.Core.Models;
using TechInventory.ViewModels;
using TechInventory.Views;

namespace TechInventory.Views
{
    public partial class MainWindow : Window
    {
        private readonly AppServices _services;
        public static User? CurrentUser { get; private set; }

        public MainWindow(AppServices services, User currentUser)
        {
            InitializeComponent();
            _services = services;
            CurrentUser = currentUser;
            DataContext = new MainViewModel(services);

            CurrentUserName.Text = string.IsNullOrWhiteSpace(currentUser.FullName)
                ? currentUser.Login
                : currentUser.FullName;
            CurrentUserRole.Text = currentUser.Role switch
            {
                "Admin" => "Администратор",
                "Teacher" => "Преподаватель",
                _ => currentUser.Role
            };

            if (currentUser.Role != "Admin")
            {
                UsersButton.Visibility = Visibility.Collapsed;
                RoomsButton.Visibility = Visibility.Collapsed;
                CreateTicketFromMainButton.Visibility = Visibility.Collapsed;
                AddDeviceButton.Visibility = Visibility.Collapsed;
                DeleteDeviceButton.Visibility = Visibility.Collapsed;
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainViewModel vm)
                vm.SelectedRoom = e.NewValue as RoomNodeViewModel;
        }

        private void DeviceTile_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is DeviceTileViewModel deviceTile)
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.SelectedTile = deviceTile;   
                    if (vm.OpenDeviceCommand.CanExecute(deviceTile))
                        vm.OpenDeviceCommand.Execute(deviceTile);
                }
            }
        }

        private void TicketsButton_Click(object sender, RoutedEventArgs e)
        {
            var ticketsWindow = new TicketsWindow(_services, MainWindow.CurrentUser);
            ticketsWindow.Owner = this;
            ticketsWindow.ShowDialog();
        }

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            var usersWindow = new UsersWindow(_services.UserRepository);
            usersWindow.Owner = this;
            usersWindow.ShowDialog();
        }

        private async void CreateTicketFromMain_Click(object sender, RoutedEventArgs e)
        {
            var selectDevice = new SelectDeviceWindow(_services.DeviceRepository);
            selectDevice.Owner = this;
            if (selectDevice.ShowDialog() == true)
            {
                var createTicket = new CreateTicketWindow(_services.TicketService,
                    selectDevice.SelectedDeviceId, selectDevice.SelectedDeviceName);
                createTicket.Owner = this;
                createTicket.ShowDialog();
            }
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            string dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "inventory.db");
            var reportWindow = new ReportWindow(dbPath);
            reportWindow.Owner = this;
            reportWindow.ShowDialog();
        }

        private async void RoomsButton_Click(object sender, RoutedEventArgs e)
        {
            var roomsWindow = new RoomsManagementWindow(_services.RoomRepository);
            roomsWindow.Owner = this;
            roomsWindow.ShowDialog();
            if (DataContext is MainViewModel vm)
                await vm.RefreshRoomsAsync();
        }

        private void DictionaryButton_Click(object sender, RoutedEventArgs e)
        {
            var dictWindow = new DictionaryWindow(_services.DictionaryRepository);
            dictWindow.Owner = this;
            dictWindow.ShowDialog();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Вы действительно хотите выйти из системы?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                string sessionFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.dat");
                if (System.IO.File.Exists(sessionFile))
                    System.IO.File.Delete(sessionFile);

                MainWindow.CurrentUser = null;
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }
        private void AddDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new DeviceEditWindow(
                _services.RoomRepository,
                _services.DictionaryRepository,
                _services.DeviceRepository);
            window.Owner = this;
            if (window.ShowDialog() == true)
            {
                if (DataContext is MainViewModel vm)
                    _ = vm.RefreshRoomsAsync();  
            }
        }

        private void DeleteDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.SelectedRoom != null && vm.SelectedTile != null)
            {
                var tile = vm.SelectedTile;
                if (MessageBox.Show($"Удалить устройство {tile.Name}? Все связанные заявки и перемещения также будут удалены.",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _services.DeviceRepository.DeleteAsync(tile.DeviceID).Wait();
                        vm.SelectedTile = null;
                        _ = vm.RefreshRoomsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Сначала выберите кабинет, затем кликните на плитку устройства.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}