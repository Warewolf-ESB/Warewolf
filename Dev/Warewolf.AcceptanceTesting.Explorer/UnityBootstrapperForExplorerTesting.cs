using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Util;
using Infragistics.Themes;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Warewolf.AcceptanceTesting.Core;
using Warewolf.Studio.Core.View_Interfaces;
using Warewolf.Studio.Themes.Luna;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;

namespace Warewolf.AcceptanceTesting.Explorer
{
    internal class UnityBootstrapperForExplorerTesting : UnityBootstrapperForTesting
    {
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            AppSettings.LocalHost = "http://myserver:3124/";
            // ReSharper disable ObjectCreationAsStatement
            new Application();
            // ReSharper restore ObjectCreationAsStatement
            ThemeManager.ApplicationTheme = new LunaTheme();

            Container.RegisterType<IServer, ServerForTesting>(new InjectionConstructor());
            Container.RegisterInstance<IShellViewModel>(new ShellViewModel(Container,
                Container.Resolve<IRegionManager>(), Container.Resolve<IEventAggregator>()));
            Container.RegisterInstance<IExplorerViewModel>(new ExplorerViewModel(Container.Resolve<IShellViewModel>(),
                Container.Resolve<IEventAggregator>()));

            var explorerView = new ExplorerView();
            explorerView.DataContext = Container.Resolve<IExplorerViewModel>();
            Container.RegisterInstance<IExplorerView>(explorerView);
        }
    }
}