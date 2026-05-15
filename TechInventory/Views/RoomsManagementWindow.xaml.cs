using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Inventory.Core.Models;
using Inventory.Core.Interfaces;
using System.Threading.Tasks;

namespace TechInventory.Views
{
    public partial class RoomsManagementWindow : Window
    {
        private readonly IRoomRepository _roomRepo;
        public ObservableCollection<Room> Rooms { get; } = new();

        public RoomsManagementWindow(IRoomRepository roomRepo)
        {
            InitializeComponent();
            _roomRepo = roomRepo;
            Loaded += async (s, e) => await LoadRooms();
        }

        private async Task LoadRooms()
        {
            var rooms = await _roomRepo.GetAllAsync();
            Rooms.Clear();
            foreach (var r in rooms)
                Rooms.Add(r);
            RoomsGrid.ItemsSource = Rooms;
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new RoomEditWindow();
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                await _roomRepo.AddAsync(dialog.Room);
                await LoadRooms();
            }
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            var selected = RoomsGrid.SelectedItem as Room;
            if (selected == null)
            {
                MessageBox.Show("Выберите кабинет для редактирования", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var dialog = new RoomEditWindow(selected);
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                await _roomRepo.UpdateAsync(dialog.Room);
                await LoadRooms();
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selected = RoomsGrid.SelectedItem as Room;
            if (selected == null)
            {
                MessageBox.Show("Выберите кабинет для удаления", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var result = MessageBox.Show($"Удалить кабинет '{selected.Name}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _roomRepo.DeleteAsync(selected.RoomID);
                    await LoadRooms();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}