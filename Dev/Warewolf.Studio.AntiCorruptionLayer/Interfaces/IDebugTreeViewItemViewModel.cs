using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Warewolf.Studio.AntiCorruptionLayer
{
    public interface IDebugTreeViewItemViewModel : INotifyPropertyChanged
    {
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
        int Depth { get; }
        ObservableCollection<IDebugTreeViewItemViewModel> Children { get; }
        IDebugTreeViewItemViewModel Parent { get; set; }
        bool? HasError { get; set; }

        void VerifyErrorState();
    }
}