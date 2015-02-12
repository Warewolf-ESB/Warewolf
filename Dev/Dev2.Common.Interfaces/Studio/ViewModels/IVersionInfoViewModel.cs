using System;
using System.Windows.Input;

namespace Dev2.Common.Interfaces.Studio.ViewModels
{
    public interface IVersionInfoViewModel: IExplorerTreeItem
    {
        string VersionName { get; set; }
        Guid ResourceId { get; set; }
        string Version { get; set; }
        DateTime VersionDate { get; set; }
        bool CanRollBack { get; set; }
        ICommand OpenCommand { get; set; }
        bool IsVisible { get; set; }   
        string VersionHeader { get;  }
        string Reason { get; set; }
    }
}