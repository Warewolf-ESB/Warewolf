/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dev2;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Explorer;
using Dev2.Services.Security;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Deploy;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.Core;
using Dev2.ConnectionHelpers;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class DeployViewModelTests
    {
        [TestMethod]
        [Timeout(250)]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_Ctor")]
		[ExpectedException(typeof(ArgumentNullException))]
        public void DeployViewModel_Ctor_NullParamsFirst_ExprecErrors()
        {
            //------------Setup for test--------------------------
            var deployViewModel = new SingleExplorerDeployViewModel(null, new Mock<IDeploySourceExplorerViewModel>().Object, new List<IExplorerTreeItem>(), new Mock<IDeployStatsViewerViewModel>().Object, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Timeout(500)]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_Ctor")]
		[ExpectedException(typeof(ArgumentNullException))]
        public void DeployViewModel_Ctor_NullParamsSecond_ExprecErrors()
        {
            //------------Setup for test--------------------------
            var deployViewModel = new SingleExplorerDeployViewModel(new Mock<IDeployDestinationExplorerViewModel>().Object, null, new List<IExplorerTreeItem>(), new Mock<IDeployStatsViewerViewModel>().Object, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Timeout(100)]
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
        [Timeout(100)]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployViewModel_Ctor"), ExpectedException(typeof(ArgumentNullException))]
        public void DeployViewModel_Ctor_NullParamsFourth_ExprecErrors()
        {
            //------------Setup for test--------------------------
            var deployViewModel = new SingleExplorerDeployViewModel(new Mock<IDeployDestinationExplorerViewModel>().Object, new Mock<IDeploySourceExplorerViewModel>().Object, new List<IExplorerTreeItem>(), null, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
        }

        Mock<IConnectControlViewModel> _sourceConnectControl;
        Mock<IConnectControlViewModel> _destConnectControl;
        Mock<IDeployDestinationExplorerViewModel> _deployDestinationExplorerViewModel;
        Mock<IDeploySourceExplorerViewModel> _deploySourceExplorerViewModel;

        [TestMethod]
        [Timeout(100)]
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
        [Timeout(250)]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeploySourceExplorerViewModel_Ctor_valid")]
        public void DeploySourceExplorerViewModel_Ctor_Nulls_ExpectSuccess()
        {
            var explorerTooltips = new Mock<IExplorerTooltips>();
            CustomContainer.Register(explorerTooltips.Object);
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            server.Setup(a => a.DisplayName).Returns("LocalHost");
            var mockEnvironmentConnection = SetupMockConnection();
            server.SetupGet(it => it.Connection).Returns(mockEnvironmentConnection.Object);
            var shell = new Mock<IShellViewModel>();
            shell.Setup(model => model.ExplorerViewModel).Returns(new Mock<IExplorerViewModel>().Object);
            shell.Setup(model => model.ExplorerViewModel.ConnectControlViewModel).Returns(new Mock<IConnectControlViewModel>().Object);
            CustomContainer.Register(shell.Object);
            var tsk = new Task<IExplorerItem>(() => new ServerExplorerItem());
            server.Setup(a => a.LoadExplorer(false)).Returns(tsk);
            shell.Setup(a => a.LocalhostServer).Returns(server.Object);

            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            CustomContainer.Register(connectControlSingleton.Object);
            var environmentRepository = new Mock<IServerRepository>();
            CustomContainer.Register(environmentRepository.Object);

            var deploySourceExplorerViewModel = new DeploySourceExplorerViewModel(shell.Object, new Mock<IEventAggregator>().Object, new Mock<IDeployStatsViewerViewModel>().Object);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
        }

        private static Mock<IEnvironmentConnection> SetupMockConnection()
        {
            var uri = new Uri("http://bravo.com/");
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(a => a.AppServerUri).Returns(uri);
            mockEnvironmentConnection.Setup(a => a.AuthenticationType).Returns(Dev2.Runtime.ServiceModel.Data.AuthenticationType.Public);
            mockEnvironmentConnection.Setup(a => a.WebServerUri).Returns(uri);
            mockEnvironmentConnection.Setup(a => a.ID).Returns(Guid.Empty);
            return mockEnvironmentConnection;
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeploySourceExplorerViewModel_Ctor_valid")]
        public void DeploySourceExplorerViewModel_Updates_AllItemsToHaveNoContextMenuFunctions()
        {
            var explorerTooltips = new Mock<IExplorerTooltips>();
            CustomContainer.Register(explorerTooltips.Object);
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            server.Setup(a => a.DisplayName).Returns("LocalHost");
            var mockEnvironmentConnection = SetupMockConnection();
            server.SetupGet(it => it.Connection).Returns(mockEnvironmentConnection.Object);
            var shell = new Mock<IShellViewModel>();
            shell.Setup(model => model.ExplorerViewModel).Returns(new Mock<IExplorerViewModel>().Object);
            shell.Setup(model => model.ExplorerViewModel.ConnectControlViewModel).Returns(new Mock<IConnectControlViewModel>().Object);
            CustomContainer.Register<IShellViewModel>(shell.Object);
            var tsk = new Task<IExplorerItem>(() => new ServerExplorerItem());
            server.Setup(a => a.LoadExplorer(false)).Returns(tsk);
            shell.Setup(a => a.LocalhostServer).Returns(server.Object);

            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            CustomContainer.Register(connectControlSingleton.Object);
            var environmentRepository = new Mock<IServerRepository>();
            CustomContainer.Register(environmentRepository.Object);

            var deploySourceExplorerViewModel = new DeploySourceExplorerViewModel(shell.Object, new Mock<IEventAggregator>().Object, new Mock<IDeployStatsViewerViewModel>().Object);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Sanele Mthembu")]
        public void DeployViewModel_Version_Mistmatch()
        {
            SetupDeployViewModelMock();
            var deployViewModel = new Mock<IDeployViewModel>();
            var sourceVersion = new Version("0.1.5050.0001");
            var destVersion = new Version("0.0.6087.8873");
            _deploySourceExplorerViewModel.SetupGet(model => model.ServerVersion).Returns(sourceVersion);
            _deployDestinationExplorerViewModel.SetupGet(model => model.ServerVersion).Returns(destVersion);
            deployViewModel.Setup(model => model.DeployCommand.Execute(null));
            deployViewModel.Object.DeployCommand.Execute(null);
            deployViewModel.Verify(model => model.DeployCommand.Execute(null), Times.AtLeast(1));
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Sanele Mthembu")]
        public void Given_TheSameServer_CheckDestinationPersmisions_ShouldBeTrue()
        {
            var explorerTooltips = new Mock<IExplorerTooltips>();
            CustomContainer.Register(explorerTooltips.Object);
            //------------Setup for test--------------------------
            var shellViewModel = new Mock<IShellViewModel>();
            shellViewModel.Setup(model => model.ExplorerViewModel).Returns(new Mock<IExplorerViewModel>().Object);
            shellViewModel.Setup(model => model.ExplorerViewModel.ConnectControlViewModel).Returns(new Mock<IConnectControlViewModel>().Object);
            var envMock = new Mock<IEnvironmentViewModel>();
            shellViewModel.SetupGet(model => model.ExplorerViewModel.Environments).Returns(new Caliburn.Micro.BindableCollection<IEnvironmentViewModel>()
            {
                envMock.Object
            });
            var eventAggregator = new Mock<IEventAggregator>();

            var localhost = new Mock<IServer>();
            localhost.Setup(a => a.DisplayName).Returns("Localhost");
            localhost.SetupGet(server => server.CanDeployTo).Returns(true);
            localhost.SetupGet(server => server.CanDeployFrom).Returns(true);
            var mockEnvironmentConnection = SetupMockConnection();
            localhost.SetupGet(it => it.Connection).Returns(mockEnvironmentConnection.Object);
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
            stat.TryCalculate(sourceExplorerItem);
            //------------Assert Results-------------------------
            Assert.IsTrue(sourceExplorerItem.First().CanDeploy);
        }

        static AsyncObservableCollection<IExplorerItemViewModel> SetDestinationExplorerItemViewModels(Guid resourceId, Mock<IServer> otherServer, Mock<IShellViewModel> shellViewModel, Mock<IServer> localhost)
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

        void SetupDeployViewModelMock()
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
