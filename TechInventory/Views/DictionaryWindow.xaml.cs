using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Inventory.Core.Interfaces;
using Inventory.Core.Models;
using TechInventory.ViewModels;

namespace TechInventory.Views
{
    public partial class DictionaryWindow : Window
    {
        private readonly IDictionaryRepository _dictRepo;
        public ObservableCollection<DictionaryItem> Items { get; } = new();

        public DictionaryWindow(IDictionaryRepository dictRepo)
        {
            InitializeComponent();
            _dictRepo = dictRepo;
            DataContext = this;
            CategoryCombo.SelectedIndex = 0;
            Loaded += async (s, e) => await LoadItems();
        }

        private async Task LoadItems()
        {
            string? category = CategoryCombo.SelectedValue as string;
            var all = await _dictRepo.GetAllAsync();
            Items.Clear();
            foreach (var item in all.Where(i => i.Category == category))
                Items.Add(item);
        }

        private async void AddItem_Click(object sender, RoutedEventArgs e)
        {
            string? category = CategoryCombo.SelectedValue as string;
            if (string.IsNullOrEmpty(category)) return;

            string value = Microsoft.VisualBasic.Interaction.InputBox("Введите значение:", "Новый элемент", "");
            if (string.IsNullOrWhiteSpace(value)) return;

            await _dictRepo.AddAsync(new DictionaryItem { Category = category, Value = value });
            await LoadItems();
        }

        private async void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListView.SelectedItem is DictionaryItem selected)
            {
                string value = Microsoft.VisualBasic.Interaction.InputBox("Измените значение:", "Редактирование", selected.Value);
                if (string.IsNullOrWhiteSpace(value)) return;
                selected.Value = value;
                await _dictRepo.UpdateAsync(selected);
                await LoadItems();
            }
        }

        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsListView.SelectedItem is DictionaryItem selected)
            {
                var res = MessageBox.Show($"Удалить элемент '{selected.Value}'?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes)
                {
                    await _dictRepo.DeleteAsync(selected.ID);
                    await LoadItems();
                }
            }
        }
    }
}