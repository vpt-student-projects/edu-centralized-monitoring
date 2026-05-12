namespace TechInventory.ViewModels
{
    public class RoomNodeViewModel : TreeNodeViewModel
    {
        public int RoomID { get; set; }
        public string RoomName { get; set; } = string.Empty;

        public RoomNodeViewModel(string name, int roomId)
        {
            Name = name;
            RoomID = roomId;
            RoomName = name;
        }
    }
}