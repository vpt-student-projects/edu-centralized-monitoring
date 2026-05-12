namespace TechInventory.ViewModels
{
    public class BuildingNodeViewModel : TreeNodeViewModel
    {
        public string BuildingName { get; set; }

        public BuildingNodeViewModel(string name, string buildingName)
        {
            Name = name;
            BuildingName = buildingName;
        }
    }
}