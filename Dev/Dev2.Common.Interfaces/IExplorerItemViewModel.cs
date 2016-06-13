using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public interface IExplorerItemViewModel : IExplorerTreeItem
    {
        
        bool Checked { get; set; }
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
        bool CanShowDependencies { get; set; }

        bool IsVersion { get; set; }
        string VersionNumber { get; set; }
        string VersionHeader { get; set; }
        void Filter(string filter);
        Task<bool> Move(IExplorerTreeItem destination);
        
        ICommand OpenVersionCommand { get; set; }
        ICommand DeleteVersionCommand { get; set; }
        ICommand ShowDependenciesCommand { get; set; }
        
        string Inputs { get; set; }
        string Outputs { get; set; }
        string ExecuteToolTip { get; }
        string EditToolTip { get; }
        string ActivityName { get; }
       

        void AddSibling(IExplorerItemViewModel sibling);
        void CreateNewFolder();
        void Apply(Action<IExplorerItemViewModel> action);
        IExplorerItemViewModel Find(string resourcePath);
        void Filter(Func<IExplorerItemViewModel, bool> filter);

        IEnumerable<IExplorerItemViewModel> AsList();
        void ShowErrorMessage(string errorMessage);
    }

    public enum ExplorerEventContext
    {
        Selected
    }
}