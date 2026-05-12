using System.Windows;
using Inventory.Core;
using TechInventory.ViewModels;

namespace TechInventory.Views
{
    public partial class DeviceCardWindow : Window
    {
        private readonly DeviceCardViewModel _viewModel;

        public DeviceCardWindow(AppServices services, int deviceId)
        {
            InitializeComponent();
            _viewModel = new DeviceCardViewModel(services, deviceId);
            DataContext = _viewModel;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Только теперь окно полностью готово к обновлению UI
            await _viewModel.LoadAsync();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}