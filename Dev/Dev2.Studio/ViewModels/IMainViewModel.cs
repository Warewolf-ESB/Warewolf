using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using System.Windows.Input;
using Dev2.Workspaces;

namespace Dev2.Studio.ViewModels
{
    public interface IMainViewModel
    {
        bool CanSave { get; }
        bool CanDebug { get; }

        IFrameworkSecurityContext SecurityContext { get; }

        ICommand SaveCommand { get; }
        ICommand DebugCommand { get; }
        ICommand DeployCommand { get; }
        ICommand ExitCommand { get; }
        ICommand EditCommand { get; }
        ICommand ViewInBrowserCommand { get; }
        ICommand RunCommand { get; }
        RelayCommand<string> NewResourceCommand { get; }

        void AddStartTabs();

        void AddMissingAndFindUnusedVariableForActiveWorkflow();
    }
}
