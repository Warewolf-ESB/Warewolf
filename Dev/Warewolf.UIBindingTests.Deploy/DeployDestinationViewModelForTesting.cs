using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.ViewModels;

namespace Warewolf.UIBindingTests.Deploy
{
    class DeployDestinationViewModelForTesting : DeployDestinationViewModel
    {
        public IList<IExplorerItemViewModel> Children { get; set; }

        public DeployDestinationViewModelForTesting(IShellViewModel shellViewModel, Microsoft.Practices.Prism.PubSubEvents.IEventAggregator aggregator)
            : base(shellViewModel, aggregator)
        {
            
        }

        #region Overrides of DeploySourceExplorerViewModel

        protected override Task<bool> LoadEnvironment(IEnvironmentViewModel localhostEnvironment, bool isDeploy = false)
        {
            localhostEnvironment.Children = new ObservableCollection<IExplorerItemViewModel>(Children ?? new List<IExplorerItemViewModel> { CreateExplorerVMS() });
            PrivateObject p = new PrivateObject(localhostEnvironment);
            p.SetField("_isConnected", true);
            localhostEnvironment.ResourceId = Guid.Empty;
            AfterLoad(localhostEnvironment.Server.EnvironmentID);
            return Task.FromResult(true);
        }

        IExplorerItemViewModel CreateExplorerVMS()
        {
            ExplorerItemViewModel ax = null;
            ax = new ExplorerItemViewModel(new Mock<IServer>().Object, null, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object)
            {
                ResourceName = "Examples",
                ResourcePath = "Examples",
                ResourceId = Guid.NewGuid(),
                
                Children = new ObservableCollection<IExplorerItemViewModel>
                {
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "Utility - Date and Times", ResourcePath = "Examples\\Utility - Date and Time" }
                    ,             new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.Parse("7CC8CA4E-8261-433F-8EF1-612DE003907C"),ResourceName = "dora", ResourcePath = "Examples\\dora" }
                    ,new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "NameIdConflict", ResourcePath = "Examples\\bob",ResourceType = "DbSource"},
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.Parse("7CC8CA4E-8261-433F-8EF1-612DE003907C"),ResourceName = "Control Flow - Sequence", ResourcePath = "Examples\\Control Flow - Sequence",ResourceType = "DbSource"},

                }, 
                CanDeploy = true
            };
            return ax;
        }

        #endregion
        public override Version MinSupportedVersion => new Version(SelectedServer.GetMinSupportedVersion());
        public override Version ServerVersion => new Version(SelectedServer.GetServerVersion());
    }
}