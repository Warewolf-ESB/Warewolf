using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Unity;
using Warewolf.AcceptanceTesting.Core;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;

namespace Warewolf.AcceptanceTesting.Explorer
{
    internal class UnityBootstrapperForExplorerTesting : UnityBootstrapperForTesting
    {
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            var explorerView = new ExplorerView();
            explorerView.DataContext = new ExplorerViewModel(Container.Resolve<IShellViewModel>(),
                Container.Resolve<IEventAggregator>());
            Container.RegisterInstance<IExplorerView>(explorerView);
        }
    }
}