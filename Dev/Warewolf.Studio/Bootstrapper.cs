using System.Windows;
using Dev2.Common.Interfaces.Studio;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Toolbox;
using Infragistics.Themes;
using Infragistics.Windows.DockManager;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter;
using Warewolf.Studio.Core.View_Interfaces;
using Warewolf.Studio.Themes.Luna;
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

        #region Overrides of UnityBootstrapper

        /// <summary>
        /// Configures the <see cref="T:Microsoft.Practices.Unity.IUnityContainer"/>. May be overwritten in a derived class to add specific
        ///             type mappings required by the application.
        /// </summary>
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            ThemeManager.ApplicationTheme = new LunaTheme();
            Container.RegisterInstance<IExplorerViewModel>(new DummyExplorerViewModel());
            Container.RegisterInstance<IToolboxViewModel>(new DummyToolboxViewModel());
            Container.RegisterInstance<IMenuViewModel>(new DummyMenuViewModel());

            Container.RegisterInstance<IExplorerView>(new ExplorerView());
            Container.RegisterInstance<IToolboxView>(new ToolboxView());
            Container.RegisterInstance<IMenuView>(new MenuView());
        }

        #endregion

        protected override void InitializeShell()
        {
            base.InitializeShell();
            var window = (Window)Shell;
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
