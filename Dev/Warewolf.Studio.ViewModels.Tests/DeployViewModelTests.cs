

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


    }

    [TestClass]
    public class DeploySourceViewModelTests
    {
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
        public void DeployStatsViewerViewModel_CheckDestinationPersmisions()
        {
            //------------Setup for test--------------------------
            var shellViewModel = new Mock<IShellViewModel>();
            var eventAggregator = new Mock<IEventAggregator>();
            var destPermissions = new List<IWindowsGroupPermission>();
            destPermissions.Add(new WindowsGroupPermission
            {
                Administrator = false
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
        }
    }
}
