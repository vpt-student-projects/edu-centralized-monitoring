using System;
using System.Windows;
using Inventory.Core.Models;

namespace TechInventory.Views
{
    public partial class RoomEditWindow : Window
    {
        public Room Room { get; private set; }
        private readonly bool _isNew;

        public RoomEditWindow(Room? room = null)
        {
            InitializeComponent();
            if (room != null)
            {
                _isNew = false;
                Room = room;
                NameBox.Text = room.Name;
                FloorBox.Text = room.Floor.ToString();
                BuildingBox.Text = room.Building;
                DescriptionBox.Text = room.Description ?? "";
            }
            else
            {
                _isNew = true;
                Room = new Room();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Название обязательно", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(FloorBox.Text, out int floor))
            {
                MessageBox.Show("Этаж должен быть числом", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Room.Name = NameBox.Text.Trim();
            Room.Floor = floor;
            Room.Building = BuildingBox.Text.Trim();
            Room.Description = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? null : DescriptionBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}