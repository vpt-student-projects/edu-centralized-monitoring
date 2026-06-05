using System;
using System.Windows;
using Inventory.Core;
using TechInventory.ViewModels;

namespace TechInventory.Views
{
    public partial class TicketsWindow : Window
    {
        private readonly TicketListViewModel _vm;
        private bool _firstActivation = true;

        public TicketsWindow(AppServices services,
            Inventory.Core.Models.User? currentUser = null)
        {
            InitializeComponent();
            _vm = new TicketListViewModel(services, currentUser);
            DataContext = _vm;
        }

        protected override async void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            // Пропускаем первую активацию — данные уже грузятся в конструкторе VM
            if (_firstActivation)
            {
                _firstActivation = false;
                return;
            }
            await _vm.RefreshAsync();
        }
    }
}