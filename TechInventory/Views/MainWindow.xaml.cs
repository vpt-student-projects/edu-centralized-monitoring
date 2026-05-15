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

            if (currentUser.Role != "Admin")
            {
                UsersButton.Visibility = Visibility.Collapsed;
                RoomsButton.Visibility = Visibility.Collapsed;
                CreateTicketFromMainButton.Visibility = Visibility.Collapsed;
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
                if (DataContext is MainViewModel vm && vm.OpenDeviceCommand.CanExecute(deviceTile))
                    vm.OpenDeviceCommand.Execute(deviceTile);
            }
        }

        private void TicketsButton_Click(object sender, RoutedEventArgs e)
        {
            var ticketsWindow = new TicketsWindow(_services);
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
    }
}