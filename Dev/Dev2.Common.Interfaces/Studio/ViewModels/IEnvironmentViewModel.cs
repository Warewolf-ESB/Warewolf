using System.Collections.Generic;
using System.Windows.Input;

namespace Dev2.Common.Interfaces.Studio.ViewModels
{
    public interface IEnvironmentViewModel : IExplorerTreeItem
    {
        ICollection<IExplorerItemViewModel> Children { get; set; }
        string DisplayName { get; set; }
        bool IsConnected { get; }
        bool IsLoaded { get; }
        void Connect();
        bool IsConnecting { get; }
        void Load();
        void Filter(string filter);
        ICollection<IExplorerItemViewModel> AsList();
        void SetItemCheckedState(System.Guid id, bool state);
        void RemoveItem(IExplorerItemViewModel vm);
        ICommand RefreshCommand { get; set; }
        bool IsServerIconVisible { get; set; }
        bool IsServerUnavailableIconVisible { get; set; }
        ICommand ShowServerVersionCommand { get; set; }
    }
}