namespace TechInventory.ViewModels
{
    public class TicketListItem : ViewModelBase
    {
        public int TicketID { get; set; }
        public string DeviceName { get; set; } = "";
        public string RoomName { get; set; } = "–";
        public string Description { get; set; } = "";
        public string Priority { get; set; } = "";
        public string Status { get; set; } = "";
        public string CreatedAt { get; set; } = "";
        public int? RoomID { get; set; }
        public int DeviceID { get; set; }
    }
}