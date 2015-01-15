using System.Windows;
using Infragistics.Windows.DockManager;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using Warewolf.Studio.Core;
using Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter;
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
            var explorerView = new ExplorerView { DataContext = new DummyExplorerViewModel() };
            explorerRegion.Add(explorerView, RegionNames.Explorer);
            explorerRegion.Activate(explorerView);

            var toolboxRegion = regionManager.Regions[RegionNames.Toolbox];
            var toolBoxView = new ToolboxView { DataContext = new DummyToolboxViewModel() };
            toolboxRegion.Add(toolBoxView, RegionNames.Toolbox);
            toolboxRegion.Activate(toolBoxView); 
            
            var menuRegion = regionManager.Regions[RegionNames.Menu];
            var menuView = new MenuView();
            menuRegion.Add(menuView, RegionNames.Menu);
            menuRegion.Activate(menuView);

            window.Show();

        }

        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            RegionAdapterMappings mappings = base.ConfigureRegionAdapterMappings();
            mappings.RegisterMapping(typeof(TabGroupPane), Container.Resolve<TabGroupPaneRegionAdapter>());
            return mappings;
        }

    }
}
