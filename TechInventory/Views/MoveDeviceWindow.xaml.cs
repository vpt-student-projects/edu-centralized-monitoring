using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Inventory.Core.Models;
using TechInventory.Helpers;

namespace TechInventory.Views
{
    public partial class MoveDeviceWindow : Window
    {
        public ObservableCollection<Room> Rooms { get; } = new();
        public Device CurrentDevice { get; }
        public int SelectedRoomId { get; set; }
        public int? NewPosition { get; set; }
        public string Reason { get; set; } = "";

        private readonly Inventory.Core.Interfaces.IMovementRepository _movementRepo;
        private readonly Inventory.Core.Interfaces.IDeviceRepository _deviceRepo;
        private readonly Inventory.Core.Interfaces.IRoomRepository _roomRepo;

        public MoveDeviceWindow(Device device,
            Inventory.Core.Interfaces.IMovementRepository movementRepo,
            Inventory.Core.Interfaces.IDeviceRepository deviceRepo,
            Inventory.Core.Interfaces.IRoomRepository roomRepo)
        {
            InitializeComponent();
            CurrentDevice = device;
            _movementRepo = movementRepo;
            _deviceRepo = deviceRepo;
            _roomRepo = roomRepo;

            DataContext = this;
            Loaded += async (s, e) => await LoadRooms();
        }

        private async Task LoadRooms()
        {
            var allRooms = await _roomRepo.GetAllAsync();
            Rooms.Clear();
            foreach (var r in allRooms.OrderBy(r => r.Name))
                Rooms.Add(r);
            RoomComboBox.ItemsSource = Rooms;
            if (Rooms.Any())
                SelectedRoomId = Rooms.First().RoomID;

            var currentRoom = await _roomRepo.GetByIdAsync(CurrentDevice.CurrentRoomID);
            CurrentRoomText.Text = $"Текущий кабинет: {currentRoom?.Name ?? CurrentDevice.CurrentRoomID.ToString()}";
        }

        private async void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRoomId == CurrentDevice.CurrentRoomID)
            {
                MessageBox.Show("Устройство уже находится в этом кабинете", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? position = null;
            if (!string.IsNullOrWhiteSpace(PositionTextBox.Text))
            {
                if (int.TryParse(PositionTextBox.Text, out int pos))
                    position = pos;
                else
                {
                    MessageBox.Show("Некорректная позиция", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                var movement = new MovementHistory
                {
                    DeviceID = CurrentDevice.DeviceID,
                    OldRoomID = CurrentDevice.CurrentRoomID,
                    NewRoomID = SelectedRoomId,
                    Reason = string.IsNullOrWhiteSpace(Reason) ? null : Reason,
                    MoveDate = DateTime.UtcNow
                };
                await _movementRepo.AddMovementAsync(movement);

                CurrentDevice.CurrentRoomID = SelectedRoomId;
                CurrentDevice.PositionInRoom = position;
                await _deviceRepo.UpdateAsync(CurrentDevice);

                DialogResult = true;
                Close();
                Logger.Log($"Устройство {CurrentDevice.Name} перемещено из {CurrentDevice.CurrentRoomID} в {SelectedRoomId}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перемещения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}