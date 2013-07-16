using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using System.Windows.Input;
using Dev2.Workspaces;

namespace Dev2.Studio.ViewModels
{
    public interface IMainViewModel
    {
        IFrameworkSecurityContext SecurityContext { get; }

        ICommand DeployCommand { get; }
        ICommand ExitCommand { get; }
        RelayCommand<string> NewResourceCommand { get; }
        IEnvironmentModel ActiveEnvironment { get; set; }

        void AddStartTabs();
    }
}
