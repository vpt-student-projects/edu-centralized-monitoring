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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Введите название кабинета", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(FloorBox.Text.Trim(), out int floor))
            {
                MessageBox.Show("Некорректный этаж", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Room.Name = name;
            Room.Floor = floor;
            Room.Building = BuildingBox.Text.Trim();
            Room.Description = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? null : DescriptionBox.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}