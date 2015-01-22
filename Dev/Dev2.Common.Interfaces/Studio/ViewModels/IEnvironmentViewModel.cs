using System.Collections.Generic;
using System.Windows.Input;

namespace Dev2.Common.Interfaces.Studio.ViewModels
{
    public interface IExplorerTreeItem
    {
        ICommand NewCommand { get; set; }
        ICommand DeployCommand { get; set; }
        bool CanCreateDbService { get; set; }
        bool CanCreateDbSource { get; set; }
        bool CanCreateWebService { get; set; }
        bool CanCreateWebSource { get; set; }
        bool CanCreatePluginService { get; set; }
        bool CanCreatePluginSource { get; set; }
        bool CanRename { get; set; }
        bool CanDelete { get; set; }
        bool CanDeploy { get; set; }
        
    }
    public interface IEnvironmentViewModel : IExplorerTreeItem
    {
        IServer Server { get; set; }
        ICollection<IExplorerItemViewModel> ExplorerItemViewModels { get; set; }
        string DisplayName { get; set; }
        bool IsConnected { get; }
        bool IsLoaded { get; }
        void Connect();
        void Load();
        void Filter(string filter);
        ICollection<IExplorerItemViewModel> AsList();
        void SetItemCheckedState(System.Guid id, bool state);
        void RemoveItem(IExplorerItemViewModel vm);
    }
}