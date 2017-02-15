using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Input;
using Dev2.Common.Interfaces.Security;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace Dev2.Common.Interfaces
{
    public interface IExplorerItemViewModel : IExplorerTreeItem
    {
        bool Checked { get; set; }
        bool IsRenaming{ get; set; }
        bool IsVisible { get; set; }
        bool CanExecute { get; set; }
        bool CanEdit { get; set; }
        bool CanView { get; set; }
        bool CanShowDependencies { get; set; }
        bool IsVersion { get; set; }
        bool CanViewSwagger { get; set; }
        bool CanDuplicate { get; set; }
        bool CanCreateTest { get; set; }

        string VersionNumber { get; set; }
        string VersionHeader { get; set; }
        string ExecuteToolTip { get; }
        string EditToolTip { get; }
        string ActivityName { get; }


        ICommand ViewSwaggerCommand { get; set; }
        ICommand OpenCommand { get; set; }
        ICommand OpenVersionCommand { get; set; }
        ICommand DeleteVersionCommand { get; set; }
        ICommand ShowDependenciesCommand { get; set; }
        ICommand LostFocus { get; set; }
        bool CanMove { get; }
        ICommand DuplicateCommand { get; set; }
        ICommand CreateTestCommand { get; set; }
        bool CanDebugInputs { get; set; }
        bool CanDebugStudio { get; set; }
        bool CanDebugBrowser { get; set; }
        bool CanCreateSchedule { get; set; }
        bool CanViewRunAllTests { get; set; }
        bool CanContribute { get; set; }
        ObservableCollection<IExplorerItemViewModel> UnfilteredChildren { get; set; }

        IEnumerable<IExplorerItemViewModel> AsList();

        Task<bool> Move(IExplorerTreeItem destination);

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void AddSibling(IExplorerItemViewModel sibling);
        void CreateNewFolder();
        void Apply(Action<IExplorerItemViewModel> action);
        void Filter(string filter);
        void Filter(Func<IExplorerItemViewModel, bool> filter);
        void ShowErrorMessage(string errorMessage, string header);

        void ShowDependencies();

        void SetPermissions(Permissions explorerItemPermissions, bool isDeploy = false);
    }
}