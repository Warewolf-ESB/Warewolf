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
        ICommand RenameCommand { get; set; }
        
        bool IsRenaming{ get; set; }
        bool IsNotRenaming { get;  }
        ICommand ItemSelectedCommand { get; set; }
        bool IsVisible { get; set; }
        bool AllowEditing { get; set; }
        IServer Server { get; }
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