using System;
using System.Threading.Tasks;
using System.Windows;
using Inventory.Core.Models;
using Inventory.Core.Interfaces;

namespace TechInventory.Views
{
    public partial class SelectDeviceWindow : Window
    {
        public int SelectedDeviceId { get; private set; }
        public string SelectedDeviceName { get; private set; }

        private readonly IDeviceRepository _deviceRepo;

        public SelectDeviceWindow(IDeviceRepository deviceRepo)
        {
            InitializeComponent();
            _deviceRepo = deviceRepo;
            Loaded += async (s, e) => await LoadDevices();
        }

        private async Task LoadDevices()
        {
            var all = await _deviceRepo.GetAllAsync();
            DeviceComboBox.ItemsSource = all;
            if (all.Count > 0)
            {
                DeviceComboBox.SelectedIndex = 0;
                var first = all[0];
                SelectedDeviceId = first.DeviceID;
                SelectedDeviceName = first.Name;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceComboBox.SelectedItem is Device device)
            {
                SelectedDeviceId = device.DeviceID;
                SelectedDeviceName = device.Name;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Выберите устройство", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}