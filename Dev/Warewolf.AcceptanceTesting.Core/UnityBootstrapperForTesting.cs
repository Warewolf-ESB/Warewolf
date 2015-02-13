using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Util;
using Infragistics.Themes;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using Moq;
using Warewolf.Studio.Themes.Luna;
using Warewolf.Studio.ViewModels;

namespace Warewolf.AcceptanceTesting.Core
{
    public abstract class UnityBootstrapperForTesting : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return new DependencyObject();
        }
        public Mock<IPopupController> PopupController { get; set; }
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            AppSettings.LocalHost = "http://myserver:3124/";
            // ReSharper disable ObjectCreationAsStatement
            new Application();
            // ReSharper restore ObjectCreationAsStatement
            ThemeManager.ApplicationTheme = new LunaTheme();
            PopupController = new Mock<IPopupController>();
            Container.RegisterType<IServer, ServerForTesting>(new InjectionConstructor());
            Container.RegisterInstance(PopupController.Object);

            Container.RegisterInstance<IServer>(new ServerForTesting());
            Container.RegisterInstance<IShellViewModel>(new ShellViewModel(Container, Container.Resolve<IRegionManager>(), Container.Resolve<IEventAggregator>()));         
        }
    }
}