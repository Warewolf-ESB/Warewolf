using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces
{
    public interface IFileListing
    {
        string Name { get; set; }
        string FullName { get; set; }
        ICollection<IFileListing> Children { get; set; }
        bool IsDirectory { get; set; }
    }

    public interface IDllListingModel : IFileListing
    {
        void Filter(string searchTerm);
        new ObservableCollection<IDllListingModel> Children { get; set; }
        bool IsExpanded { get; set; }
        bool IsExpanderVisible { get; }
        bool IsVisible { get; set; }
        bool IsSelected { get; set; }
        int TotalChildrenCount { get; set; }
        bool ProgressVisibility { get; set; }
        int ChildrenCount { get; }
        int CurrentProgress { get; set; }
        string ClsId { get; set; }
        bool Is32Bit { get; set; }
    }

    public interface IFileListingModel : IFileListing
    {
        new ObservableCollection<IFileListingModel> Children { get; set; }
        bool IsExpanded { get; set; }
        bool IsExpanderVisible { get; }
        bool IsVisible { get; set; }
        bool IsSelected { get; set; }
        int TotalChildrenCount { get; set; }
        bool ProgressVisibility { get; set; }
        int ChildrenCount { get; }
        int CurrentProgress { get; set; }
        bool IsChecked { get; set; }

        List<string> FilterSelected(List<string> acc);
    }
}
