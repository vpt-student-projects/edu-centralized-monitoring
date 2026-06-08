namespace TechInventory.ViewModels
{
    public class DeviceTileViewModel : ViewModelBase
    {
        public int DeviceID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string StatusName { get; set; } = "Неизвестно";
        public bool HasOpenTickets { get; set; }
        public string AssignedTo { get; set; } = "";
        public string Icon { get; set; } = "";
        public string TooltipText => $"{Name} — {StatusName}";

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}