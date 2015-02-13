using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces.Studio.ViewModels
{
    public interface IExplorerTreeItem
    {
        ResourceType ResourceType { get; set; }
        string ResourceName { get; set; } 
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
        bool CanShowVersions { get; set; }
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
        ICollection<IExplorerItemViewModel> Children { get; set; }
        IExplorerTreeItem Parent { get; set; }
        void AddChild(IExplorerItemViewModel child);
        void RemoveChild(IExplorerItemViewModel child);

    }
}