using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Help;

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
        ICommand Expand { get; set; }
        IExplorerItemViewModel Parent { get; set; }
        void AddSibling(IExplorerItemViewModel sibling);
        void AddChild(IExplorerItemViewModel child);
        void RemoveChild(IExplorerItemViewModel child);
    }

    public interface IVersionInfoViewModel: IExplorerTreeItem
    {
        string VersionName { get; set; }
        Guid ResourceId { get; set; }
        string Version { get; set; }
        DateTime VersionDate { get; set; }
        bool CanRollBack { get; set; }
        ICommand OpenCommand { get; set; }

        bool IsVisible { get; set; }
   
        string VersionHeader { get; set; }
        string Reason { get; set; }
    }
    public interface IExplorerHelpDescriptorBuilder
    {
        IHelpDescriptor Build(IExplorerItemViewModel model,ExplorerEventContext ctx );
    }

    public enum ExplorerEventContext
    {
        Selected
    }

    public interface INewItemMessage
    {
        IExplorerItemViewModel Parent{get;}

        ResourceType Type { get;  }

    }

    public interface IDeployItemMessage
    {
        IExplorerItemViewModel Item { get;  }

        IExplorerItemViewModel SourceServer { get; }

    }
}