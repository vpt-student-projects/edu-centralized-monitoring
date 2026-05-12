using System.Windows;
using Inventory.Core;
using TechInventory.ViewModels;

namespace TechInventory.Views
{
    public partial class TicketsWindow : Window
    {
        public TicketsWindow(AppServices services)
        {
            InitializeComponent();
            DataContext = new TicketListViewModel(services);
        }
    }
}