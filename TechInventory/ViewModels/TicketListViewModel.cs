using Inventory.Core;
using Inventory.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TechInventory.Helpers;

namespace TechInventory.ViewModels
{
    public class TicketListViewModel : ViewModelBase
    {
        private readonly AppServices _services;
        private readonly User? _currentUser;
        private HashSet<int> _userDeviceIds = new();
        public bool IsTeacher => _currentUser?.Role == "Teacher";
        private bool _onlyMyDevices;
        public bool OnlyMyDevices
        {
            get => _onlyMyDevices;
            set { SetProperty(ref _onlyMyDevices, value); ApplyFilters(); }
        }
        public ObservableCollection<TicketListItem> Tickets { get; } = new();
        public ObservableCollection<string> PriorityOptions { get; } = new() { "Все", "Высокий", "Средний", "Низкий" };
        public ObservableCollection<string> StatusOptions { get; } = new() { "Все", "Новая", "В работе", "Завершена" };
        public ObservableCollection<RoomFilterItem> RoomOptions { get; } = new();

        private string _selectedPriority = "Все";
        public string SelectedPriority
        {
            get => _selectedPriority;
            set { SetProperty(ref _selectedPriority, value); ApplyFilters(); }
        }

        private string _selectedStatus = "Все";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set { SetProperty(ref _selectedStatus, value); ApplyFilters(); }
        }

        private int? _selectedRoomId;
        public int? SelectedRoomId
        {
            get => _selectedRoomId;
            set { SetProperty(ref _selectedRoomId, value); ApplyFilters(); }
        }

        private TicketListItem? _selectedTicket;
        public TicketListItem? SelectedTicket
        {
            get => _selectedTicket;
            set => SetProperty(ref _selectedTicket, value);
        }

        private List<TicketListItem> _allTickets = new();

        public ICommand RefreshCommand { get; }
        public ICommand CloseTicketCommand { get; }

        public TicketListViewModel(AppServices services, User? currentUser = null)
        {
            _services = services;
            _currentUser = currentUser;
            RefreshCommand = new RelayCommand(async _ => await LoadTicketsAsync());
            CloseTicketCommand = new RelayCommand(async _ => await CloseSelectedTicket(), _ => SelectedTicket != null);
            _ = LoadTicketsAsync();
        }

        private async Task LoadTicketsAsync()
        {
            try
            {
                if (IsTeacher && _currentUser != null)
                {
                    var devices = await _services.DeviceRepository.GetAllAsync();
                    _userDeviceIds = new HashSet<int>(
                        devices
                            .Where(d => d.AssignedToUserID == _currentUser.UserID)
                            .Select(d => d.DeviceID)
                    );
                }
                else
                {
                    _userDeviceIds.Clear();
                }

                var tickets = await _services.TicketRepository.GetAllTicketsAsync();
                _allTickets.Clear();

                foreach (var ticket in tickets)
                {
                    var item = new TicketListItem
                    {
                        TicketID = ticket.TicketID,
                        DeviceID = ticket.DeviceID,
                        Description = ticket.Description ?? "",
                        CreatedAt = ticket.CreatedAt.ToString("g"),
                        RoomID = ticket.RoomID
                    };

                    item.Status = ticket.StatusID switch
                    {
                        1 => "Новая",
                        2 => "В работе",
                        3 => "Завершена",
                        _ => "?"
                    };

                    item.Priority = ticket.Priority switch { 1 => "Высокий", 2 => "Средний", 3 => "Низкий", _ => "?" };

                    try
                    {
                        var device = await _services.DeviceService.GetDeviceByIdAsync(ticket.DeviceID);
                        item.DeviceName = device?.Name ?? "?";
                    }
                    catch { item.DeviceName = "?"; }

                    if (ticket.RoomID.HasValue)
                    {
                        try
                        {
                            var room = await _services.RoomRepository.GetByIdAsync(ticket.RoomID.Value);
                            item.RoomName = room?.Name ?? "–";
                        }
                        catch { item.RoomName = "–"; }
                    }
                    else
                        item.RoomName = "–";

                    _allTickets.Add(item);
                }

                var rooms = await _services.RoomRepository.GetAllAsync();
                RoomOptions.Clear();
                RoomOptions.Add(new RoomFilterItem { RoomID = 0, DisplayName = "Все" });
                foreach (var r in rooms.OrderBy(r => r.Name))
                    RoomOptions.Add(new RoomFilterItem { RoomID = r.RoomID, DisplayName = r.Name });
                SelectedRoomId = 0;

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            Tickets.Clear();
            var filtered = _allTickets.AsEnumerable();

            if (SelectedPriority != "Все")
                filtered = filtered.Where(t => t.Priority == SelectedPriority);

            if (SelectedStatus != "Все")
                filtered = filtered.Where(t => t.Status == SelectedStatus);

            if (SelectedRoomId.HasValue && SelectedRoomId.Value > 0)
                filtered = filtered.Where(t => t.RoomID == SelectedRoomId.Value);

            if (IsTeacher && OnlyMyDevices)
                filtered = filtered.Where(t => _userDeviceIds.Contains(t.DeviceID));

            foreach (var t in filtered.OrderByDescending(t => t.TicketID))
                Tickets.Add(t);
        }

        private async Task CloseSelectedTicket()
        {
            if (SelectedTicket == null) return;
            var result = MessageBox.Show($"Закрыть заявку #{SelectedTicket.TicketID}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _services.TicketService.CloseTicketAsync(SelectedTicket.TicketID);
                await LoadTicketsAsync();
                Logger.Log($"Заявка #{SelectedTicket.TicketID} закрыта");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка закрытия заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class RoomFilterItem
    {
        public int RoomID { get; set; }
        public string DisplayName { get; set; } = "";
    }
}