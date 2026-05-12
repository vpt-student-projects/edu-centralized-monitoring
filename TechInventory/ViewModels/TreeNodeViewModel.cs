using System.Collections.ObjectModel;

namespace TechInventory.ViewModels
{
    public class TreeNodeViewModel : ViewModelBase
    {
        private bool _isExpanded;
        public string Name { get; set; } = string.Empty;

        public ObservableCollection<TreeNodeViewModel> Children { get; set; } = new();

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        // Для корневых узлов (корпуса) полезно сразу раскрыть
        public TreeNodeViewModel()
        {
            IsExpanded = true;
        }
    }
}