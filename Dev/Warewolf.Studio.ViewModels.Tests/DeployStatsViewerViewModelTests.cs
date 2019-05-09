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
using Dev2;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.Core;
using Dev2.ConnectionHelpers;
using Dev2.Studio.Interfaces.Deploy;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class DeployStatsViewerViewModelTests
    {
        [TestMethod, Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void DeployStatsViewerViewModel_Given_NewItem_OnSourceAndDestination_CheckDestinationPersmisions_ShouldBeTrue()
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
            var mockEnvironmentConnection = SetupMockConnection();

            var localhost = new Mock<IServer>();
            localhost.Setup(a => a.DisplayName).Returns("Localhost");
            localhost.SetupGet(server => server.CanDeployTo).Returns(true);
            localhost.SetupGet(server => server.IsConnected).Returns(true);
            localhost.SetupGet(it => it.Connection).Returns(mockEnvironmentConnection.Object);

            var otherServer = new Mock<IServer>();
            otherServer.Setup(server => server.IsConnected).Returns(true);
            otherServer.Setup(a => a.DisplayName).Returns("OtherServer");
            otherServer.SetupGet(server => server.CanDeployFrom).Returns(true);
            otherServer.SetupGet(it => it.Connection).Returns(mockEnvironmentConnection.Object);

            shellViewModel.Setup(x => x.LocalhostServer).Returns(localhost.Object);

            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            CustomContainer.Register(connectControlSingleton.Object);
            var environmentRepository = new Mock<IServerRepository>();
            CustomContainer.Register(environmentRepository.Object);

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
            stat.TryCalculate(sourceExplorerItem);
            //------------Assert Results-------------------------
            Assert.IsTrue(sourceExplorerItem.First().CanDeploy);
            Assert.AreEqual(stat.NewResources, 1);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void DeployStatsViewerViewModel_Given_TheSameServer_CheckDestinationPersmisions_ShouldBeTrue()
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

            var destinationViewModel = SetDestinationExplorerItemViewModels(Guid.Empty, localhost, shellViewModel, localhost);

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
    }
}
