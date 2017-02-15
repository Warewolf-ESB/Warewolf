

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Explorer;
using Dev2.Services.Security;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.Core;

// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class DeployViewModelTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_Ctor"), ExpectedException(typeof(ArgumentNullException))]
        public void DeployViewModel_Ctor_NullParamsFirst_ExprecErrors()
        {
            //------------Setup for test--------------------------
            var deployViewModel = new SingleExplorerDeployViewModel(null, new Mock<IDeploySourceExplorerViewModel>().Object, new List<IExplorerTreeItem>(), new Mock<IDeployStatsViewerViewModel>().Object, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_Ctor"), ExpectedException(typeof(ArgumentNullException))]
        public void DeployViewModel_Ctor_NullParamsSecond_ExprecErrors()
        {
            //------------Setup for test--------------------------
            var deployViewModel = new SingleExplorerDeployViewModel(new Mock<IDeployDestinationExplorerViewModel>().Object, null, new List<IExplorerTreeItem>(), new Mock<IDeployStatsViewerViewModel>().Object, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_Ctor"), ExpectedException(typeof(ArgumentNullException))]
        public void DeployViewModel_Ctor_NullParamsThird_ExprecErrors()
        {
            //------------Setup for test--------------------------
            var deployViewModel = new SingleExplorerDeployViewModel(new Mock<IDeployDestinationExplorerViewModel>().Object, new Mock<IDeploySourceExplorerViewModel>().Object, null, new Mock<IDeployStatsViewerViewModel>().Object, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_Ctor"), ExpectedException(typeof(ArgumentNullException))]
        public void DeployViewModel_Ctor_NullParamsFourth_ExprecErrors()
        {
            //------------Setup for test--------------------------
            var deployViewModel = new SingleExplorerDeployViewModel(new Mock<IDeployDestinationExplorerViewModel>().Object, new Mock<IDeploySourceExplorerViewModel>().Object, new List<IExplorerTreeItem>(), null, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

    
    
        private Mock<IConnectControlViewModel> _sourceConnectControl;
        private Mock<IConnectControlViewModel> _destConnectControl;
        private Mock<IDeployDestinationExplorerViewModel> _deployDestinationExplorerViewModel;
        private Mock<IDeploySourceExplorerViewModel> _deploySourceExplorerViewModel;

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeploySourceExplorerViewModel_Ctor"), ExpectedException(typeof(ArgumentNullException))]
        public void DeploySourceExplorerViewModel_Ctor_Nulls_ExpectErrors()
        {
            //------------Setup for test--------------------------
            var deploySourceExplorerViewModel = new DeploySourceExplorerViewModel(null, new Mock<IEventAggregator>().Object, new Mock<IDeployStatsViewerViewModel>().Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeploySourceExplorerViewModel_Ctor_valid")]
        public void DeploySourceExplorerViewModel_Ctor_Nulls_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            server.Setup(a => a.ResourceName).Returns("LocalHost");
            var shell = new Mock<IShellViewModel>();
            CustomContainer.Register<IShellViewModel>(shell.Object);
            Task<IExplorerItem> tsk = new Task<IExplorerItem>(() => new ServerExplorerItem());
            server.Setup(a => a.LoadExplorer(false)).Returns(tsk);
            server.Setup(a => a.GetServerConnections()).Returns(new List<IServer>());
            shell.Setup(a => a.LocalhostServer).Returns(server.Object);
            var deploySourceExplorerViewModel = new DeploySourceExplorerViewModel(shell.Object, new Mock<IEventAggregator>().Object, new Mock<IDeployStatsViewerViewModel>().Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeploySourceExplorerViewModel_Ctor_valid")]
        public void DeploySourceExplorerViewModel_Updates_AllItemsToHaveNoContextMenuFunctions()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            server.Setup(a => a.ResourceName).Returns("LocalHost");
            var shell = new Mock<IShellViewModel>();
            CustomContainer.Register<IShellViewModel>(shell.Object);
            Task<IExplorerItem> tsk = new Task<IExplorerItem>(() => new ServerExplorerItem());
            server.Setup(a => a.LoadExplorer(false)).Returns(tsk);
            server.Setup(a => a.GetServerConnections()).Returns(new List<IServer>());
            shell.Setup(a => a.LocalhostServer).Returns(server.Object);
            var deploySourceExplorerViewModel = new DeploySourceExplorerViewModel(shell.Object, new Mock<IEventAggregator>().Object, new Mock<IDeployStatsViewerViewModel>().Object);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DeployViewModel_Version_Mistmatch()
        {
            SetupDeployViewModelMock();
            var deployViewModel = new Mock<IDeployViewModel>();//SingleExplorerDeployViewModel(_deployDestinationExplorerViewModel.Object, _deploySourceExplorerViewModel.Object, _explorerTreeItems.Object, _deployStatsViewerViewModel.Object, _shellViewModel.Object, _popupController.Object);
            var sourceVersion = new Version("0.1.5050.0001");
            var destVersion = new Version("0.0.6087.8873");
            _deploySourceExplorerViewModel.SetupGet(model => model.ServerVersion).Returns(sourceVersion);
            _deployDestinationExplorerViewModel.SetupGet(model => model.ServerVersion).Returns(destVersion);
            deployViewModel.Setup(model => model.DeployCommand.Execute(null));
            deployViewModel.Object.DeployCommand.Execute(null);
            deployViewModel.Verify(model => model.DeployCommand.Execute(null), Times.AtLeast(1));
        }
        
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Given_SameItem_OnSourceAndDestination_DeployStatsViewerViewModel_CheckDestinationPersmisions_ShouldBeFalse()
        {
            //------------Setup for test--------------------------
            var shellViewModel = new Mock<IShellViewModel>();
            var eventAggregator = new Mock<IEventAggregator>();
            var destPermissions = new List<IWindowsGroupPermission>();
            destPermissions.Add(new WindowsGroupPermission
            {
                Administrator = true
            });
            var localhost = new Mock<IServer>();
            localhost.Setup(a => a.ResourceName).Returns("Localhost");
            localhost.SetupGet(server => server.Permissions).Returns(destPermissions);
            localhost.SetupGet(server => server.CanDeployTo).Returns(true);
            shellViewModel.Setup(x => x.LocalhostServer).Returns(localhost.Object);
            shellViewModel.Setup(x => x.ActiveServer).Returns(new Mock<IServer>().Object);
            var deployDestinationViewModel = new DeployDestinationViewModel(shellViewModel.Object, eventAggregator.Object);
            
            IList<IExplorerTreeItem> items = new List<IExplorerTreeItem>();            
            var parentMock = new Mock<IExplorerTreeItem>();
            var itemViewModel = new ExplorerItemViewModel(localhost.Object, parentMock.Object, null, shellViewModel.Object, null);
            var explorerItemViewModel = new ExplorerItemNodeViewModel(localhost.Object, itemViewModel, null);
            var destinationViewModel = new AsyncObservableCollection<IExplorerItemViewModel>();
            destinationViewModel.Add(explorerItemViewModel);
            parentMock.SetupGet(it => it.Children).Returns(destinationViewModel);
            var sourcePermissions = new List<IWindowsGroupPermission>();
            var otherServer = new Mock<IServer>();
            otherServer.SetupGet(server => server.Permissions).Returns(sourcePermissions);
            otherServer.SetupGet(server => server.CanDeployFrom).Returns(true);
            parentMock.SetupGet(item => item.Server).Returns(otherServer.Object);
            items.Add(parentMock.Object);
            deployDestinationViewModel.Environments.First().Children = destinationViewModel;
            deployDestinationViewModel.SelectedEnvironment = deployDestinationViewModel.Environments.First();
            
            var stat = new DeployStatsViewerViewModel(items, deployDestinationViewModel);
            Assert.IsTrue(deployDestinationViewModel.SelectedEnvironment.AsList().Count > 0);
            //------------Execute Test---------------------------
            Assert.IsNotNull(stat);
            stat.Calculate(items);
            //------------Assert Results-------------------------
            Assert.IsFalse(items.First().CanDeploy);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Given_NewItem_OnSourceAndDestination_DeployStatsViewerViewModel_CheckDestinationPersmisions_ShouldBeTrue()
        {
            //------------Setup for test--------------------------
            var shellViewModel = new Mock<IShellViewModel>();
            var eventAggregator = new Mock<IEventAggregator>();
                        
            var localhost = new Mock<IServer>();
            localhost.Setup(a => a.ResourceName).Returns("Localhost");
            localhost.SetupGet(server => server.CanDeployTo).Returns(true);
            localhost.SetupGet(server => server.IsConnected).Returns(true);

            var otherServer = new Mock<IServer>();
            otherServer.Setup(server => server.IsConnected).Returns(true);
            otherServer.Setup(a => a.ResourceName).Returns("OtherServer");
            otherServer.SetupGet(server => server.CanDeployFrom).Returns(true);

            shellViewModel.Setup(x => x.LocalhostServer).Returns(localhost.Object);

            var deployDestinationViewModel = new DeployDestinationViewModel(shellViewModel.Object, eventAggregator.Object);
            
            var sourceItemViewModel = new ExplorerItemViewModel(localhost.Object, null, null, shellViewModel.Object, null);

            var sourceViewModel = new AsyncObservableCollection<IExplorerItemViewModel>();
            var sourceExplorerItemViewModel = new ExplorerItemNodeViewModel(localhost.Object, sourceItemViewModel, null);
            sourceViewModel.Add(sourceExplorerItemViewModel);

            var destinationViewModel = SetDestinationExplorerItemViewModels(Guid.NewGuid(), otherServer, shellViewModel, localhost);
            
            IList<IExplorerTreeItem> sourceExplorerItem = new List<IExplorerTreeItem>();

            sourceExplorerItem.Add(sourceExplorerItemViewModel);
            
            deployDestinationViewModel.Environments.First().Children = destinationViewModel;
            deployDestinationViewModel.SelectedEnvironment = deployDestinationViewModel.Environments.First();
            deployDestinationViewModel.SelectedEnvironment.Connect();
            sourceExplorerItem.First().CanDeploy = true;
            sourceExplorerItem.First().IsResourceChecked = true;

            var stat = new DeployStatsViewerViewModel(sourceExplorerItem, deployDestinationViewModel);
            Assert.IsTrue(deployDestinationViewModel.SelectedEnvironment.AsList().Count > 0);
            //------------Execute Test---------------------------
            Assert.IsNotNull(stat);
            stat.Calculate(sourceExplorerItem);
            //------------Assert Results-------------------------
            Assert.IsTrue(sourceExplorerItem.First().CanDeploy);
            Assert.AreEqual(stat.NewResources, 1);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Given_TheSameServer_CheckDestinationPersmisions_ShouldBeTrue()
        {

            //------------Setup for test--------------------------
            var shellViewModel = new Mock<IShellViewModel>();
            var eventAggregator = new Mock<IEventAggregator>();

            var localhost = new Mock<IServer>();
            localhost.Setup(a => a.ResourceName).Returns("Localhost");
            localhost.SetupGet(server => server.CanDeployTo).Returns(true);
            localhost.SetupGet(server => server.CanDeployFrom).Returns(true);
          
            shellViewModel.Setup(x => x.LocalhostServer).Returns(localhost.Object);

            var deployDestinationViewModel = new DeployDestinationViewModel(shellViewModel.Object, eventAggregator.Object);

            var sourceItemViewModel = new ExplorerItemViewModel(localhost.Object, null, null, shellViewModel.Object, null);

            var sourceViewModel = new AsyncObservableCollection<IExplorerItemViewModel>();
            var sourceExplorerItemViewModel = new ExplorerItemNodeViewModel(localhost.Object, sourceItemViewModel, null);
            sourceViewModel.Add(sourceExplorerItemViewModel);

            var destinationViewModel = SetDestinationExplorerItemViewModels(Guid.Empty,localhost, shellViewModel, localhost);

            IList<IExplorerTreeItem> sourceExplorerItem = new List<IExplorerTreeItem>();

            sourceExplorerItem.Add(sourceExplorerItemViewModel);

            deployDestinationViewModel.Environments.First().Children = destinationViewModel;
            deployDestinationViewModel.SelectedEnvironment = deployDestinationViewModel.Environments.First();

            var stat = new DeployStatsViewerViewModel(sourceExplorerItem, deployDestinationViewModel);
            Assert.IsTrue(deployDestinationViewModel.SelectedEnvironment.AsList().Count > 0);
            //------------Execute Test---------------------------
            Assert.IsNotNull(stat);
            stat.Calculate(sourceExplorerItem);
            //------------Assert Results-------------------------
            Assert.IsTrue(sourceExplorerItem.First().CanDeploy);
        }

        private static AsyncObservableCollection<IExplorerItemViewModel> SetDestinationExplorerItemViewModels(Guid resourceId, Mock<IServer> otherServer, Mock<IShellViewModel> shellViewModel, Mock<IServer> localhost)
        {
            var destExplorerItemMock = new Mock<IExplorerTreeItem>();
            var destItemViewModel = new ExplorerItemViewModel(otherServer.Object, destExplorerItemMock.Object, null, shellViewModel.Object, null);
            var destExplorerItemViewModel = new ExplorerItemNodeViewModel(localhost.Object, destItemViewModel, null);
            var destinationViewModel = new AsyncObservableCollection<IExplorerItemViewModel>();
            destExplorerItemViewModel.ResourceId = resourceId;
            destinationViewModel.Add(destExplorerItemViewModel);
            return destinationViewModel;
        }

        #region Setup

        private void SetupDeployViewModelMock()
        {
            _sourceConnectControl = new Mock<IConnectControlViewModel>();
            _destConnectControl = new Mock<IConnectControlViewModel>();
            _deployDestinationExplorerViewModel = new Mock<IDeployDestinationExplorerViewModel>();
            _deployDestinationExplorerViewModel.SetupGet(model => model.ConnectControlViewModel).Returns(_destConnectControl.Object);
            _deploySourceExplorerViewModel = new Mock<IDeploySourceExplorerViewModel>();
            _deploySourceExplorerViewModel.SetupGet(model => model.ConnectControlViewModel).Returns(_sourceConnectControl.Object);
            var conflict = new Mock<IList<Conflict>>();
        }

        #endregion

    }
}
