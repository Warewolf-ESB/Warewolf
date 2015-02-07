using System.Windows.Input;

namespace Dev2.Common.Interfaces.Studio.ViewModels
{
    public interface IExplorerTreeItem
    {
        bool IsExpanderVisible { get; set; }
        ICommand NewCommand { get; set; }
        ICommand DeployCommand { get; set; }
        bool CanCreateDbService { get; set; }
        bool CanCreateDbSource { get; set; }
        bool CanCreateServerSource { get; set; }
        bool CanCreateWebService { get; set; }
        bool CanCreateWebSource { get; set; }
        bool CanCreatePluginService { get; set; }
        bool CanCreatePluginSource { get; set; }
        bool CanRename { get; set; }
        bool CanDelete { get; set; }
        bool CanCreateFolder { get; set; }
        bool CanDeploy { get; set; }
        bool CanShowVersions { get; }
        bool CanRollback { get;  }
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
        ICommand RenameCommand { get; set; }
        ICommand CreateFolderCommand { get; set; }
        ICommand DeleteCommand { get; set; }
        ICommand ShowVersionHistory { get; set; }
        ICommand RollbackCommand { get; set; }
        IServer Server { get; set; }
        ICommand Expand { get; set; }
    }
}