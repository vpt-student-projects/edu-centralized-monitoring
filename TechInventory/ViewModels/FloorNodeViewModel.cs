namespace TechInventory.ViewModels
{
    public class FloorNodeViewModel : TreeNodeViewModel
    {
        public int FloorNumber { get; set; }

        public FloorNodeViewModel(string name, int floor)
        {
            Name = name;
            FloorNumber = floor;
        }
    }
}