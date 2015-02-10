using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Util;
using Infragistics.Themes;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using Warewolf.AcceptanceTesting.Core;
using Warewolf.Core;
using Warewolf.Studio.Core.View_Interfaces;
using Warewolf.Studio.Models.Help;
using Warewolf.Studio.Themes.Luna;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.ViewModels.Help;
using Warewolf.Studio.Views;

namespace Warewolf.AcceptanceTesting.Help
{
    internal class UnityBootstrapperForHelpTesting : UnityBootstrapper
    {

        protected override DependencyObject CreateShell()
        {
            return new DependencyObject();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
           
            Container.RegisterInstance<IHelpWindowModel>(new HelpModel(Container.Resolve<IEventAggregator>()));
            Container.RegisterInstance<IHelpWindowViewModel>(new HelpWindowViewModel(new HelpDescriptorViewModel(new HelpDescriptor("", "<body>This is the default help</body>", null)), Container.Resolve<IHelpWindowModel>()));

            var helpView = new HelpView();
            helpView.DataContext = Container.Resolve<IHelpWindowViewModel>();
            Container.RegisterInstance<IHelpView>(helpView);

        }

    }
}