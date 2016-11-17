using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IDebugTreeViewItemViewModel : INotifyPropertyChanged
    {
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
        int Depth { get; }
        ObservableCollection<IDebugTreeViewItemViewModel> Children { get; }
        IDebugTreeViewItemViewModel Parent { get; set; }
        bool? HasError { get; set; }
        bool? HasNoError { get; set; }
        bool? MockSelected { get; set; }
        string ActivityTypeName { get; set; }
        bool IsTestView { get; set; }

        void VerifyErrorState();
    }
}