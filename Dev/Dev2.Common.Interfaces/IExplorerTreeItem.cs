using Dev2.Common.Interfaces.Data;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public interface IExplorerTreeItem : IDisposable
    {
        string ResourceType { get; set; }
        string ResourcePath { get; set; }

        bool CanDrop { get; set; }
        bool CanDrag { get; set; }

        string ResourceName { get; set; }
        Guid ResourceId { get; set; }
        bool IsExpanderVisible { get; set; }
        ICommand NewCommand { get; set; }
        ICommand DeployCommand { get; set; }
        bool CanCreateDbSource { get; set; }
        bool CanCreateServerSource { get; set; }
        bool CanCreateWebSource { get; set; }
        bool CanCreatePluginSource { get; set; }
        bool CanCreateEmailSource { get; set; }

        // ReSharper disable once InconsistentNaming
        bool CanCreateRabbitMQSource { get; set; }

        bool CanCreateDropboxSource { get; set; }
        bool CanCreateSharePointSource { get; set; }
        bool CanRename { get; set; }
        bool CanDelete { get; set; }
        bool CanCreateFolder { get; set; }
        bool CanDeploy { get; set; }
        bool CanShowVersions { get; set; }
        bool CanRollback { get; }
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
        bool CanShowServerVersion { get; set; }
        bool AllowResourceCheck { get; set; }
        bool? IsResourceChecked { get; set; }

        ICommand RenameCommand { get; set; }
        ICommand CreateFolderCommand { get; set; }
        ICommand DeleteCommand { get; set; }
        ICommand ShowVersionHistory { get; set; }
        ICommand RollbackCommand { get; set; }
        IServer Server { get; set; }
        ICommand Expand { get; set; }
        ObservableCollection<IExplorerItemViewModel> Children { get; set; }
        IExplorerTreeItem Parent { get; set; }
        bool CanCreateWorkflowService { get; set; }
        bool AreVersionsVisible { get; set; }
        bool ShowContextMenu { get; set; }
        IShellViewModel ShellViewModel { get; }
        int ChildrenCount { get; }
        Action<IExplorerItemViewModel> SelectAction { get; set; }
        bool? IsFolderChecked { get; set; }
        bool? IsResourceUnchecked { get; set; }

        void AddChild(IExplorerItemViewModel child);

        void RemoveChild(IExplorerItemViewModel child);

        void SelectItem(Guid id, Action<IExplorerItemViewModel> foundAction);

        bool IsSource { get; set; }
        bool IsService { get; set; }
        bool IsFolder { get; set; }
        bool IsReservedService { get; set; }
        bool IsServer { get; set; }
        bool IsResourceVersion { get; set; }
    }
}