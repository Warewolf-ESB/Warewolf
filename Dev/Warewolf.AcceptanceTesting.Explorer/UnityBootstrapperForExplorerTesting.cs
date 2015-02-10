using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.Prism.PubSubEvents;
using Warewolf.AcceptanceTesting.Core;
using Warewolf.Studio.Core.View_Interfaces;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;

namespace Warewolf.AcceptanceTesting.Explorer
{
    internal class UnityBootstrapperForExplorerTesting : UnityBootstrapperForTesting
    {
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            
            Container.RegisterInstance<IExplorerViewModel>(new ExplorerViewModel(Container.Resolve<IShellViewModel>(),
                Container.Resolve<IEventAggregator>()));

            var explorerView = new ExplorerView();
            explorerView.DataContext = Container.Resolve<IExplorerViewModel>();
            Container.RegisterInstance<IExplorerView>(explorerView);
        }
    }
}