using System;
using System.Windows;
using System.Windows.Controls;
using Inventory.Core.Interfaces;
using TechInventory.Helpers;

namespace TechInventory.Views
{
    public partial class CreateTicketWindow : Window
    {
        private readonly ITicketService _ticketService;
        private readonly int _deviceId;
        private readonly string _deviceName;

        public int CreatedTicketId { get; private set; }

        public CreateTicketWindow(ITicketService ticketService, int deviceId, string deviceName)
        {
            InitializeComponent();
            _ticketService = ticketService;
            _deviceId = deviceId;
            _deviceName = deviceName;

            DeviceNameText.Text = deviceName;
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Введите описание заявки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int priority = 2;
            if (PriorityCombo.SelectedItem is ComboBoxItem item && item.Tag != null)
                priority = int.Parse(item.Tag.ToString());

            try
            {
                var ticket = await _ticketService.CreateTicketAsync(_deviceId, DescriptionTextBox.Text.Trim(), priority);
                CreatedTicketId = ticket.TicketID;
                DialogResult = true;
                Close();
                Logger.Log($"Заявка #{ticket.TicketID} создана для устройства {_deviceName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}