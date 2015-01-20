using System;
using System.Collections.Generic;
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
        bool IsVisible { get; set; }
    }

    public interface INewItemMessage
    {
        IExplorerItemViewModel Parent{get;}

        ResourceType Type { get;  }

    }

    public interface IDeployItemMessage
    {
        IExplorerItemViewModel Item { get; set; }

        IExplorerItemModel SourceServer { get; set; }

    }
}