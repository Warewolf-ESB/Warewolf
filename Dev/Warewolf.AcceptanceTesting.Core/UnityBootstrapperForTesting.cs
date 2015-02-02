using System.Windows;
using Microsoft.Practices.Prism.UnityExtensions;

namespace Warewolf.AcceptanceTesting.Core
{
    public abstract class UnityBootstrapperForTesting : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return new DependencyObject();
        }

    }
}