using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Inventory.Core;
using Inventory.Core.Models;
using TechInventory.Helpers;
using TechInventory.Views;

namespace TechInventory.ViewModels
{
    public class DeviceCardViewModel : ViewModelBase
    {
        private readonly AppServices _services;
        private readonly int _deviceId;
        private Device? _device;
        private string _statusName = "";
        private string _typeName = "";
        private string _assignedUserName = "нет";
        private bool _isLoading;
        public bool IsAdmin { get; }
        public ObservableCollection<MovementViewModel> Movements { get; } = new();
        public ObservableCollection<TicketViewModel> Tickets { get; } = new();

        public ObservableCollection<User> AvailableUsers { get; } = new();
        private int? _selectedUserId;
        public int? SelectedUserId
        {
            get => _selectedUserId;
            set => SetProperty(ref _selectedUserId, value);
        }

        public bool CanAssign => MainWindow.CurrentUser?.Role == "Admin";
        public ICommand AssignUserCommand { get; }

        public Device? Device
        {
            get => _device;
            set => SetProperty(ref _device, value);
        }

        public string StatusName
        {
            get => _statusName;
            set => SetProperty(ref _statusName, value);
        }

        public string TypeName
        {
            get => _typeName;
            set => SetProperty(ref _typeName, value);
        }

        public string AssignedUserName
        {
            get => _assignedUserName;
            set => SetProperty(ref _assignedUserName, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand MoveDeviceCommand { get; }
        public ICommand CreateTicketCommand { get; }

        public DeviceCardViewModel(AppServices services, int deviceId)
        {
            _services = services;
            _deviceId = deviceId;
            IsAdmin = MainWindow.CurrentUser?.Role == "Admin";
            MoveDeviceCommand = new RelayCommand(async _ => await MoveDevice(), _ => Device != null);
            CreateTicketCommand = new RelayCommand(async _ => await CreateTicket(), _ => Device != null);
            AssignUserCommand = new RelayCommand(async _ => await AssignUser(), _ => CanAssign && SelectedUserId.HasValue);
        }

        public async Task LoadAsync()
        {
            await LoadDeviceDataAsync();
        }

        private async Task LoadDeviceDataAsync()
        {
            IsLoading = true;
            try
            {
                Device = await _services.DeviceService.GetDeviceByIdAsync(_deviceId);
                if (Device == null)
                {
                    ShowError("Устройство не найдено");
                    return;
                }

                var status = await SafeGetDictionaryValue(Device.StatusID);
                StatusName = status ?? "?";

                var type = await SafeGetDictionaryValue(Device.TypeID);
                TypeName = type ?? "?";

                if (Device.AssignedToUserID.HasValue)
                {
                    try
                    {
                        var user = await _services.UserRepository.GetByIdAsync(Device.AssignedToUserID.Value);
                        AssignedUserName = user?.FullName ?? user?.Login ?? "нет";
                    }
                    catch { AssignedUserName = "нет"; }
                }
                else
                {
                    AssignedUserName = "нет";
                }

                if (CanAssign)
                {
                    var allUsers = await _services.UserRepository.GetAllAsync();
                    AvailableUsers.Clear();
                    foreach (var u in allUsers.Where(u => u.Role == "Teacher" || u.Role == "Admin"))
                        AvailableUsers.Add(u);
                    SelectedUserId = Device.AssignedToUserID;
                }
                try
                {
                    var movements = await _services.MovementRepository.GetByDeviceAsync(_deviceId);
                    Movements.Clear();
                    if (movements != null)
                    {
                        foreach (var m in movements)
                        {
                            string fromRoom = m.OldRoomID.ToString();
                            string toRoom = m.NewRoomID.ToString();
                            try
                            {
                                var oldRoom = await _services.RoomRepository.GetByIdAsync(m.OldRoomID);
                                if (oldRoom != null) fromRoom = oldRoom.Name;
                            }
                            catch { }
                            try
                            {
                                var newRoom = await _services.RoomRepository.GetByIdAsync(m.NewRoomID);
                                if (newRoom != null) toRoom = newRoom.Name;
                            }
                            catch { }

                            Movements.Add(new MovementViewModel
                            {
                                MoveDate = m.MoveDate,
                                FromRoom = fromRoom,
                                ToRoom = toRoom
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка загрузки перемещений: {ex.Message}");
                }

                try
                {
                    var tickets = await _services.TicketRepository.GetByDeviceAsync(_deviceId);
                    Tickets.Clear();
                    if (tickets != null)
                    {
                        foreach (var t in tickets)
                        {
                            string ticketStatus = t.StatusID switch
                            {
                                1 => "Новая",
                                2 => "В работе",
                                3 => "Завершена",
                                _ => "?"
                            };

                            string roomName = "–";
                            if (t.RoomID.HasValue)
                            {
                                try
                                {
                                    var room = await _services.RoomRepository.GetByIdAsync(t.RoomID.Value);
                                    roomName = room?.Name ?? "–";
                                }
                                catch { }
                            }

                            Tickets.Add(new TicketViewModel
                            {
                                TicketID = t.TicketID,
                                Description = t.Description ?? "",
                                Status = ticketStatus,
                                Priority = t.Priority switch { 1 => "Высокий", 2 => "Средний", _ => "Низкий" },
                                RoomName = roomName,
                                CreatedAt = t.CreatedAt.ToString("g")
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка загрузки заявок: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Общая ошибка: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task<string?> SafeGetDictionaryValue(int dictionaryId)
        {
            try
            {
                var item = await _services.DictionaryRepository.GetByIdAsync(dictionaryId);
                return item?.Value;
            }
            catch { return "?"; }
        }

        private void ShowError(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error));
        }

        private async Task MoveDevice()
        {
            if (Device == null) return;

            var moveWindow = new MoveDeviceWindow(
                Device,
                _services.MovementRepository,
                _services.DeviceRepository,
                _services.RoomRepository);

            moveWindow.Owner = Application.Current.Windows.OfType<DeviceCardWindow>().FirstOrDefault();
            if (moveWindow.ShowDialog() == true)
            {
                await LoadDeviceDataAsync();
            }
        }

        private async Task CreateTicket()
        {
            if (Device == null) return;
            var ticketWindow = new CreateTicketWindow(_services.TicketService, Device.DeviceID, Device.Name);
            ticketWindow.Owner = Application.Current.Windows.OfType<DeviceCardWindow>().FirstOrDefault();
            if (ticketWindow.ShowDialog() == true)
            {
                await LoadDeviceDataAsync();
            }
        }

        private async Task AssignUser()
        {
            if (Device == null || !SelectedUserId.HasValue) return;
            Device.AssignedToUserID = SelectedUserId.Value;
            await _services.DeviceRepository.UpdateAsync(Device);

            var user = AvailableUsers.FirstOrDefault(u => u.UserID == SelectedUserId.Value);
            AssignedUserName = user?.FullName ?? user?.Login ?? "нет";
        }
    }
    public class TicketViewModel : ViewModelBase
    {
        public int TicketID { get; set; }
        public string Description { get; set; } = "";
        public string Status { get; set; } = "";
        public string Priority { get; set; } = "";
        public string RoomName { get; set; } = "";
        public string CreatedAt { get; set; } = "";
    }

    public class MovementViewModel : ViewModelBase
    {
        public DateTime MoveDate { get; set; }
        public string FromRoom { get; set; } = "";
        public string ToRoom { get; set; } = "";
    }
}