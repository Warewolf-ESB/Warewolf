using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Windows.Input;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces.Studio.ViewModels
{
    public interface IExplorerItemViewModel : IExplorerTreeItem
    {
        string ResourceName { get; set; } 
        ICollection<IExplorerItemViewModel> Children { get; set; }
        bool Checked { get; set; }
        Guid ResourceId { get; set; }
        ResourceType ResourceType { get; set; }
        ICommand OpenCommand { get; set; }

        bool IsRenaming{ get; set; }
        bool IsNotRenaming { get;  }
        ICommand ItemSelectedCommand { get; set; }
        ICommand LostFocus { get; set; }
        bool IsVisible { get; set; }
        bool AllowEditing { get; set; }
        bool CanExecute { get; set; }
        bool CanEdit { get; set; }
        bool CanView { get; set; }
        bool IsVersion { get; set; }
        bool AreVersionsVisible { get; set; }
        string VersionNumber { get; set; }
        string VersionHeader { get; set; }
        void Filter(string filter);
        bool Move(IExplorerItemViewModel destination);
        bool CanDrop { get; set; }
        bool CanDrag { get; set; }
        ICommand OpenVersionCommand { get; set; }
        IExplorerItemViewModel Parent { get; set; }
        string Inputs { get; set; }
        string Outputs { get; set; }
        string ExecuteToolTip { get; }
        string EditToolTip { get; }

        void AddSibling(IExplorerItemViewModel sibling);
        void AddChild(IExplorerItemViewModel child);
        void RemoveChild(IExplorerItemViewModel child);
    }

    public enum ExplorerEventContext
    {
        Selected
    }
}