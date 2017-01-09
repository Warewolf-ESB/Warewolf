using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public interface IExplorerTreeItem : IDisposable,IEqualityComparer<IExplorerTreeItem>
    {
        string ResourceType { get; set; }
        string ResourcePath { get; set; }
        string ResourceName { get; set; }
        Guid ResourceId { get; set; }
        int ChildrenCount { get; }

        bool IsExpanderVisible { get; set; }
        bool CanDrop { get; set; }
        bool CanDrag { get; set; }
        bool CanCreateSource { get; set; }
        bool CanRename { get; set; }
        bool CanDelete { get; set; }
        bool CanCreateFolder { get; set; }
        bool CanDeploy { get; set; }
        bool CanShowVersions { get; set; }
        bool CanRollback { get; }
        bool IsExpanded { get; set; }
        bool ForcedRefresh { get; set; }
        bool IsSelected { get; set; }
        bool CanShowServerVersion { get; set; }
        bool AllowResourceCheck { get; set; }
        bool CanCreateWorkflowService { get; set; }
        bool AreVersionsVisible { get; set; }
        bool ShowContextMenu { get; set; }
        bool? IsFolderChecked { get; set; }
        bool? IsResourceChecked { get; set; }
        bool IsResourceCheckedEnabled { get; set; }
        string DeployResourceCheckboxTooltip { get; set; }
        bool? IsResourceUnchecked { get; set; }
        bool IsSource { get; set; }
        bool IsService { get; set; }
        bool IsFolder { get; set; }
        bool IsServer { get; set; }
        bool IsResourceVersion { get; set; }
        bool CanViewApisJson { get; set; }

        ICommand ViewApisJsonCommand { get; set; }
        ICommand DeployCommand { get; set; }
        ICommand RenameCommand { get; set; }
        ICommand CreateFolderCommand { get; set; }
        ICommand DeleteCommand { get; set; }
        ICommand ShowVersionHistory { get; set; }
        ICommand RollbackCommand { get; set; }
        ICommand Expand { get; set; }

        IServer Server { get; set; }
        IExplorerTreeItem Parent { get; set; }
        IShellViewModel ShellViewModel { get; }

        ObservableCollection<IExplorerItemViewModel> Children { get; set; }
        Action<IExplorerItemViewModel> SelectAction { get; set; }
        bool IsSaveDialog { get; set; }

        void AddChild(IExplorerItemViewModel child);
        void RemoveChild(IExplorerItemViewModel child);
        void SelectItem(Guid id, Action<IExplorerItemViewModel> foundAction);
        void UpdateChildrenCount();

        
    }
}