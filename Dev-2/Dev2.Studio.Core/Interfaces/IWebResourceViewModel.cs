using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Dev2.Studio.Core.Interfaces {
    public interface IWebResourceViewModel {
        string Name { get; set; }
        bool IsFolder { get; set; }
        string Uri { get; set; }
        string Base64Data { get; set; }
        bool IsRoot { get; set; }
        IWebResourceViewModel Parent { get; }
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
        ObservableCollection<IWebResourceViewModel> Children { get; }
        ICommand CopyCommand { get; }
        void AddChild(IWebResourceViewModel d);
        void SetParent(IWebResourceViewModel parent);
    }
}