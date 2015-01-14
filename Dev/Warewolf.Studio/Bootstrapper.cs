using System.Windows;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using Warewolf.Studio.Core;
using Warewolf.Studio.ViewModels.DummyModels;
using Warewolf.Studio.Views;

namespace Warewolf.Studio
{
    class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            var window = (Window)Shell;

            var regionManager = Container.Resolve<IRegionManager>();
            var explorerRegion = regionManager.Regions[RegionNames.Explorer];
            var explorerView = new ExplorerView();
            explorerView.DataContext = new DummyExplorerViewModel();
            explorerRegion.Add(explorerView, "Explorer");
            explorerRegion.Activate(explorerView);
            window.Show();

        }

    }
}
