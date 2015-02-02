using System.Windows;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;

namespace Warewolf.AcceptanceTesting.Core
{
    public abstract class UnityBootstrapperForTesting : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<ShellForTesting>();
        }

    }
}