using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Inventory.Core.Interfaces;
using Inventory.Core.Models;

namespace TechInventory.Views
{
    public partial class DeviceEditWindow : Window
    {
        private readonly IRoomRepository _roomRepo;
        private readonly IDictionaryRepository _dictRepo;
        private readonly IDeviceRepository _deviceRepo;
        private readonly Device? _existingDevice;

        // Для создания нового устройства
        public DeviceEditWindow(IRoomRepository roomRepo, IDictionaryRepository dictRepo, IDeviceRepository deviceRepo)
        {
            InitializeComponent();
            _roomRepo = roomRepo;
            _dictRepo = dictRepo;
            _deviceRepo = deviceRepo;
            _existingDevice = null;
            Loaded += async (s, e) => await LoadCombos();
        }

        // Для редактирования существующего устройства
        public DeviceEditWindow(IRoomRepository roomRepo, IDictionaryRepository dictRepo, IDeviceRepository deviceRepo, Device device)
            : this(roomRepo, dictRepo, deviceRepo)
        {
            _existingDevice = device;
            Title = "Изменить устройство";
            NameBox.Text = device.Name;
            SpecsBox.Text = device.Specs ?? "";
        }

        private async Task LoadCombos()
        {
            var types = await _dictRepo.GetByCategoryAsync("DeviceType");
            TypeCombo.ItemsSource = types;
            TypeCombo.SelectedIndex = types.Any() ? 0 : -1;

            var rooms = await _roomRepo.GetAllAsync();
            RoomCombo.ItemsSource = rooms;
            RoomCombo.SelectedIndex = rooms.Any() ? 0 : -1;

            if (_existingDevice != null)
            {
                TypeCombo.SelectedValue = _existingDevice.TypeID;
                RoomCombo.SelectedValue = _existingDevice.CurrentRoomID;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Введите название устройства", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (TypeCombo.SelectedValue == null || RoomCombo.SelectedValue == null)
            {
                MessageBox.Show("Выберите тип и кабинет", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_existingDevice == null)
                {
                    // Создание нового
                    var newDevice = new Device
                    {
                        Name = NameBox.Text.Trim(),
                        TypeID = (int)TypeCombo.SelectedValue,
                        CurrentRoomID = (int)RoomCombo.SelectedValue,
                        Specs = string.IsNullOrWhiteSpace(SpecsBox.Text) ? null : SpecsBox.Text.Trim(),
                        StatusID = 3   // «Работает»
                    };
                    await _deviceRepo.AddAsync(newDevice);
                }
                else
                {
                    // Редактирование существующего
                    _existingDevice.Name = NameBox.Text.Trim();
                    _existingDevice.TypeID = (int)TypeCombo.SelectedValue;
                    _existingDevice.CurrentRoomID = (int)RoomCombo.SelectedValue;
                    _existingDevice.Specs = string.IsNullOrWhiteSpace(SpecsBox.Text) ? null : SpecsBox.Text.Trim();
                    await _deviceRepo.UpdateAsync(_existingDevice);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения устройства: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}