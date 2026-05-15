using Inventory.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TechInventory.Helpers;
using TechInventory.Views;

namespace TechInventory.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private Dictionary<int, string> _deviceTypes = new();
        private readonly AppServices _services;
        private RoomNodeViewModel? _selectedRoom;
        private bool _isLoading;
        private Dictionary<int, string> _deviceStatuses = new();
        private HashSet<int> _deviceIdsWithOpenTickets = new();
        private Dictionary<int, string> _userNames = new();   

        public ObservableCollection<TreeNodeViewModel> Buildings { get; } = new();
        public ObservableCollection<DeviceTileViewModel> Devices { get; } = new();
        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { SetProperty(ref _searchText, value); ApplyDeviceFilter(); }
        }

        private List<DeviceTileViewModel> _allDeviceTiles = new();

        private void ApplyDeviceFilter()
        {
            Devices.Clear();
            foreach (var tile in _allDeviceTiles.Where(d =>
                string.IsNullOrEmpty(SearchText) ||
                d.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                Devices.Add(tile);
            }
        }
        public RoomNodeViewModel? SelectedRoom
        {
            get => _selectedRoom;
            set
            {
                if (SetProperty(ref _selectedRoom, value))
                    _ = LoadDevicesForSelectedRoomAsync();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand OpenDeviceCommand { get; }

        public MainViewModel(AppServices services)
        {
            _services = services;
            OpenDeviceCommand = new RelayCommand(OpenDevice);
            _ = InitializeDataAsync();
        }

        private async Task InitializeDataAsync()
        {
            IsLoading = true;
            try
            {
                var statuses = await _services.DictionaryRepository.GetByCategoryAsync("DeviceStatus");
                _deviceStatuses = statuses.ToDictionary(s => s.ID, s => s.Value);
                var types = await _services.DictionaryRepository.GetByCategoryAsync("DeviceType");
                _deviceTypes = types.ToDictionary(t => t.ID, t => t.Value);
                var openTicketsDevices = await _services.DeviceRepository.GetDevicesWithOpenTicketsAsync();
                _deviceIdsWithOpenTickets = new HashSet<int>(openTicketsDevices.Select(d => d.DeviceID));
                var users = await _services.UserRepository.GetAllAsync();
                _userNames = users.ToDictionary(u => u.UserID, u => u.FullName ?? u.Login);
                var rooms = await _services.RoomRepository.GetAllAsync();
                var buildingGroups = rooms
                    .GroupBy(r => r.Building)
                    .OrderBy(g => g.Key);

                foreach (var buildingGroup in buildingGroups)
                {
                    var buildingNode = new BuildingNodeViewModel($"Корпус {buildingGroup.Key}", buildingGroup.Key);
                    var floorGroups = buildingGroup
                        .GroupBy(r => r.Floor)
                        .OrderBy(g => g.Key);

                    foreach (var floorGroup in floorGroups)
                    {
                        var floorNode = new FloorNodeViewModel($"Этаж {floorGroup.Key}", floorGroup.Key);
                        foreach (var room in floorGroup.OrderBy(r => r.Name))
                        {
                            var roomNode = new RoomNodeViewModel($"Кабинет {room.Name}", room.RoomID);
                            floorNode.Children.Add(roomNode);
                        }
                        buildingNode.Children.Add(floorNode);
                    }
                    Buildings.Add(buildingNode);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadDevicesForSelectedRoomAsync()
        {
            if (SelectedRoom == null)
            {
                _allDeviceTiles.Clear();
                Devices.Clear();
                return;
            }

            try
            {
                var devices = await _services.DeviceService.GetDevicesByRoomAsync(SelectedRoom.RoomID);
                _allDeviceTiles.Clear();
                Devices.Clear();
                foreach (var device in devices.OrderBy(d => d.PositionInRoom ?? 0))
                {
                    string statusName = _deviceStatuses.TryGetValue(device.StatusID, out var name) ? name : "?";
                    bool hasOpenTicket = _deviceIdsWithOpenTickets.Contains(device.DeviceID);

                    string assignedTo = "";
                    if (device.AssignedToUserID.HasValue)
                        _userNames.TryGetValue(device.AssignedToUserID.Value, out assignedTo);

                    string typeName = _deviceTypes.TryGetValue(device.TypeID, out var tname) ? tname : "?";
                    string icon = typeName switch
                    {
                        "Системный блок" => "🖥️",
                        "Монитор" => "🖥️",   
                        "Принтер" => "🖨️",
                        _ => "📦"
                    };
                    Devices.Add(new DeviceTileViewModel
                    {
                        DeviceID = device.DeviceID,
                        Name = device.Name,
                        StatusName = statusName,
                        HasOpenTickets = hasOpenTicket,
                        AssignedTo = assignedTo ?? "",
                        Icon = icon
                    });
                    _allDeviceTiles = Devices.ToList(); 
                    ApplyDeviceFilter(); 
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки устройств: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public async Task RefreshRoomsAsync()
        {
            var rooms = await _services.RoomRepository.GetAllAsync();
            Buildings.Clear();

            var buildingGroups = rooms
                .GroupBy(r => r.Building)
                .OrderBy(g => g.Key);

            foreach (var buildingGroup in buildingGroups)
            {
                var buildingNode = new BuildingNodeViewModel($"Корпус {buildingGroup.Key}", buildingGroup.Key);
                var floorGroups = buildingGroup
                    .GroupBy(r => r.Floor)
                    .OrderBy(g => g.Key);

                foreach (var floorGroup in floorGroups)
                {
                    var floorNode = new FloorNodeViewModel($"Этаж {floorGroup.Key}", floorGroup.Key);
                    foreach (var room in floorGroup.OrderBy(r => r.Name))
                    {
                        var roomNode = new RoomNodeViewModel($"Кабинет {room.Name}", room.RoomID);
                        floorNode.Children.Add(roomNode);
                    }
                    buildingNode.Children.Add(floorNode);
                }
                Buildings.Add(buildingNode);
            }
        }

        private void OpenDevice(object? parameter)
        {
            if (parameter is DeviceTileViewModel deviceTile)
            {
                var cardWindow = new DeviceCardWindow(_services, deviceTile.DeviceID);
                cardWindow.Owner = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                cardWindow.ShowDialog();
            }
        }
    }
}