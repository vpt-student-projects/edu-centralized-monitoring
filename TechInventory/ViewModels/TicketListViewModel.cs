using Inventory.Core;
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

        public ObservableCollection<TicketListItem> Tickets { get; } = new();

        public ObservableCollection<string> PriorityOptions { get; } = new() { "Все", "Высокий", "Средний", "Низкий" };
        public ObservableCollection<string> StatusOptions { get; } = new() { "Все", "Новая", "В работе", "Завершена" };

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

        private TicketListItem? _selectedTicket;
        public TicketListItem? SelectedTicket
        {
            get => _selectedTicket;
            set => SetProperty(ref _selectedTicket, value);
        }

        private List<TicketListItem> _allTickets = new();

        public ICommand RefreshCommand { get; }
        public ICommand CloseTicketCommand { get; }

        public TicketListViewModel(AppServices services)
        {
            _services = services;
            RefreshCommand = new RelayCommand(async _ => await LoadTicketsAsync());
            CloseTicketCommand = new RelayCommand(async _ => await CloseSelectedTicket(), _ => SelectedTicket != null);
            _ = LoadTicketsAsync();
        }

        private async Task LoadTicketsAsync()
        {
            try
            {
                var tickets = await _services.TicketRepository.GetAllTicketsAsync();
                _allTickets.Clear();

                foreach (var ticket in tickets)
                {
                    var item = new TicketListItem
                    {
                        TicketID = ticket.TicketID,
                        Description = ticket.Description ?? "",
                        CreatedAt = ticket.CreatedAt.ToString("g")
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка закрытия заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}