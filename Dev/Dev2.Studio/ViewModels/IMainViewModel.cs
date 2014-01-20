using System.Windows.Input;
using Dev2.Security;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels
{
    public interface IMainViewModel
    {

        ICommand DeployCommand { get; }
        ICommand ExitCommand { get; }
        AuthorizeCommand<string> NewResourceCommand { get; }
        IEnvironmentModel ActiveEnvironment { get; set; }
        SimpleBaseViewModel DeployResource { get; set; }
    }
}
