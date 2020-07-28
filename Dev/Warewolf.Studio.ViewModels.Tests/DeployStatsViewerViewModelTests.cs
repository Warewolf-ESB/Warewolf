/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;
using Dev2.Data;
using Warewolf.Trigger.Queue;
using Warewolf.Triggers;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class DeployStatsViewerViewModelTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DeployStatsViewerViewModel))]
        public void DeployStatsViewerViewModel_OneParamCTOR_Status_IsEmptyString_ShouldBeTrue()
        {
            //-------------------------Arrange----------------------------
            var mockDeployDestinationExplorerViewModel = new Mock<IDeployDestinationExplorerViewModel>();
            var mockConnectControlViewModel = new Mock<IConnectControlViewModel>();

            mockDeployDestinationExplorerViewModel.Setup(o => o.ConnectControlViewModel).Returns(mockConnectControlViewModel.Object);
            //-------------------------Act--------------------------------
            var deployStatsViewerViewModel = new DeployStatsViewerViewModel(mockDeployDestinationExplorerViewModel.Object);
            //-------------------------Assert-----------------------------
            Assert.AreEqual("", deployStatsViewerViewModel.Status);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DeployStatsViewerViewModel))]
        public void DeployStatsViewerViewModel_TryCalculate_ShouldBeTrue()
        {
            var explorerTooltips = new Mock<IExplorerTooltips>();
            CustomContainer.Register(explorerTooltips.Object);
            //------------Setup for test--------------------------
            var shellViewModel = new Mock<IShellViewModel>();
            shellViewModel.Setup(model => model.ExplorerViewModel).Returns(new Mock<IExplorerViewModel>().Object);
            shellViewModel.Setup(model => model.ExplorerViewModel.ConnectControlViewModel).Returns(new Mock<IConnectControlViewModel>().Object);
            var envMock = new Mock<IEnvironmentViewModel>();
            shellViewModel.SetupGet(model => model.ExplorerViewModel.Environments).Returns(new Caliburn.Micro.BindableCollection<IEnvironmentViewModel>
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

            var sourceExplorerItemViewModel = new ExplorerItemNodeViewModel(localhost.Object, sourceItemViewModel, null);

            var destinationViewModel = SetDestinationExplorerItemViewModels(Guid.NewGuid(), otherServer, shellViewModel, localhost);

            IList<IExplorerTreeItem> sourceExplorerItem = new List<IExplorerTreeItem>();

            sourceExplorerItemViewModel.ResourceId = Guid.NewGuid();

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
            stat.TryCalculate(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(sourceExplorerItem.First().CanDeploy);
            Assert.AreEqual(0, stat.NewResources);

            Assert.AreEqual(0, stat.Services);
            Assert.AreEqual(0, stat.Sources);
            Assert.AreEqual(0, stat.Unknown);
            Assert.AreEqual(0, stat.Overrides);
            Assert.AreEqual(0, stat.New.Count);
            Assert.AreEqual(0, stat.Conflicts.Count);
            Assert.AreEqual(0, stat.Connectors);
            Assert.IsNull(stat.CalculateAction);
            Assert.IsNull(stat.Status);
            Assert.IsNull(stat.RenameErrors);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DeployStatsViewerViewModel))]
        public void DeployStatsViewerViewModel_ReCalculate_ShouldBeTrue()
        {
            //-------------------------Arrange----------------------------
            var mockDeployDestinationExplorerViewModel = new Mock<IDeployDestinationExplorerViewModel>();
            var mockConnectControlViewModel = new Mock<IConnectControlViewModel>();

            mockDeployDestinationExplorerViewModel.Setup(o => o.ConnectControlViewModel).Returns(mockConnectControlViewModel.Object);

            var deployStatsViewerViewModel = new DeployStatsViewerViewModel(mockDeployDestinationExplorerViewModel.Object);
            //-------------------------Act--------------------------------
            deployStatsViewerViewModel.ReCalculate();
            //-------------------------Assert-----------------------------
            Assert.AreEqual("", deployStatsViewerViewModel.Status);
        }

        [TestMethod, Timeout(60000)]
        [Owner("Sanele Mthembu")]
        [TestCategory(nameof(DeployStatsViewerViewModel))]
        public void DeployStatsViewerViewModel_Given_TheSameServer_CheckDestinationPersmisions_ShouldBeTrue()
        {
            var explorerTooltips = new Mock<IExplorerTooltips>();
            CustomContainer.Register(explorerTooltips.Object);
            //------------Setup for test--------------------------
            var shellViewModel = new Mock<IShellViewModel>();
            shellViewModel.Setup(model => model.ExplorerViewModel).Returns(new Mock<IExplorerViewModel>().Object);
            shellViewModel.Setup(model => model.ExplorerViewModel.ConnectControlViewModel).Returns(new Mock<IConnectControlViewModel>().Object);
            var envMock = new Mock<IEnvironmentViewModel>();
            shellViewModel.SetupGet(model => model.ExplorerViewModel.Environments).Returns(new Caliburn.Micro.BindableCollection<IEnvironmentViewModel>
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

            IList<IExplorerTreeItem> sourceExplorerItem = new List<IExplorerTreeItem>
            {
                sourceExplorerItemViewModel
            };

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

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DeployStatsViewerViewModel))]
        public void DeployStatsViewerViewModel_CalculateRenameErrors_HasErrors_ShouldBeTrue()
        {
            //-------------------------Arrange----------------------------
            var mockDeployDestinationExplorerViewModel = new Mock<IDeployDestinationExplorerViewModel>();
            var mockConnectControlViewModel = new Mock<IConnectControlViewModel>();
            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            var mockServer = new Mock<IServer>();
            var mockShellViewModel = new Mock<IShellViewModel>();

            mockExplorerItemViewModel.Setup(o => o.ActivityName).Returns("tesssssss");
            mockExplorerItemViewModel.Setup(o => o.ResourceType).Returns(@"not_Folder");
            mockExplorerItemViewModel.Setup(o => o.ResourcePath).Returns(@"Category\Testing");
            mockExplorerItemViewModel.Setup(o => o.ResourceId).Returns(Guid.NewGuid());
            mockExplorerItemViewModel.Setup(o => o.IsResourceChecked).Returns(true);

            mockExplorerTreeItem.Setup(a => a.ResourceId).Returns(Guid.NewGuid());
            mockExplorerTreeItem.Setup(a => a.ResourcePath).Returns(@"Category\Testing");
            mockExplorerTreeItem.Setup(a => a.IsResourceChecked).Returns(true);
            mockExplorerTreeItem.Setup(a => a.ResourceType).Returns(@"not_Folder");
            mockExplorerTreeItem.Setup(a => a.Server).Returns(new Mock<IServer>().Object);

            IList<IExplorerTreeItem> sourceExplorerItem = new List<IExplorerTreeItem>
            {
                mockExplorerTreeItem.Object
            };

            mockDeployDestinationExplorerViewModel.Setup(o => o.ConnectControlViewModel).Returns(mockConnectControlViewModel.Object);
            mockDeployDestinationExplorerViewModel.Setup(o => o.SelectedEnvironment.UnfilteredChildren).Returns(new ObservableCollection<IExplorerItemViewModel> { mockExplorerItemViewModel.Object });

            var deployStatsViewerViewModel = new DeployStatsViewerViewModel(mockDeployDestinationExplorerViewModel.Object);
            //-------------------------Act--------------------------------
            deployStatsViewerViewModel.TryCalculate(sourceExplorerItem);
            //-------------------------Assert-----------------------------
            mockServer.VerifyAll();
            mockShellViewModel.VerifyAll();
            mockExplorerTreeItem.VerifyAll();
            mockConnectControlViewModel.VerifyAll();

            Assert.AreEqual("", deployStatsViewerViewModel.Status);
            Assert.AreEqual("\nCategory\\Testing-->Category\\Testing\r\nPlease rename either the source or destination before continuing", deployStatsViewerViewModel.RenameErrors);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DeployStatsViewerViewModel))]
        public void DeployStatsViewerViewModel_CheckDestinationPermissions_ExplorerItemViewModel_IsNotNull_ShouldBeTrue()
        {
            //-------------------------Arrange----------------------------
            var mockDeployDestinationExplorerViewModel = new Mock<IDeployDestinationExplorerViewModel>();
            var mockConnectControlViewModel = new Mock<IConnectControlViewModel>();
            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            var mockServer = new Mock<IServer>();
            var mockShellViewModel = new Mock<IShellViewModel>();

            mockExplorerItemViewModel.Setup(o => o.ActivityName).Returns("tesssssss");
            mockExplorerItemViewModel.Setup(o => o.ResourceType).Returns(@"not_Folder");
            mockExplorerItemViewModel.Setup(o => o.ResourcePath).Returns(@"Category\Testing");
            mockExplorerItemViewModel.Setup(o => o.ResourceId).Returns(new Guid("00000000-0000-0000-0000-000000000000"));
            mockExplorerItemViewModel.Setup(o => o.IsResourceChecked).Returns(true);
            mockExplorerItemViewModel.Setup(o => o.Server).Returns(mockServer.Object);
            mockExplorerItemViewModel.Setup(o => o.Server.CanDeployTo).Returns(true);

            mockExplorerTreeItem.Setup(a => a.ResourceId).Returns(new Guid("00000000-0000-0000-0000-000000000000"));
            mockExplorerTreeItem.Setup(a => a.Server).Returns(mockServer.Object);
            mockExplorerTreeItem.Setup(a => a.Server.CanDeployFrom).Returns(true);
            mockExplorerTreeItem.Setup(a => a.Server.CanDeployTo).Returns(true);

            mockDeployDestinationExplorerViewModel.Setup(o => o.ConnectControlViewModel).Returns(mockConnectControlViewModel.Object);
            mockDeployDestinationExplorerViewModel.Setup(o => o.SelectedEnvironment).Returns(new Mock<IEnvironmentViewModel>().Object);
            mockDeployDestinationExplorerViewModel.Setup(o => o.SelectedEnvironment.IsConnected).Returns(true);
            mockDeployDestinationExplorerViewModel.Setup(o => o.SelectedEnvironment.AsList()).Returns(new List<IExplorerItemViewModel> { mockExplorerItemViewModel.Object });

            var deployStatsViewerViewModel = new DeployStatsViewerViewModel(new List<IExplorerTreeItem> { mockExplorerTreeItem.Object }, mockDeployDestinationExplorerViewModel.Object);
            //-------------------------Act--------------------------------
            deployStatsViewerViewModel.CheckDestinationPermissions();
            //-------------------------Assert-----------------------------
            mockServer.VerifyAll();
            mockShellViewModel.VerifyAll();
            mockConnectControlViewModel.VerifyAll();
            IExplorerTreeItem treeItem = mockExplorerTreeItem.Object;

            Assert.IsFalse(treeItem.CanDeploy);
            Assert.IsNull(deployStatsViewerViewModel.Status);
            Assert.IsNull(deployStatsViewerViewModel.RenameErrors);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DeployStatsViewerViewModel))]
        public void DeployStatsViewerViewModel_CheckDestinationPermissions_ExplorerItemViewModel_IsNull_ShouldBeTrue()
        {
            //-------------------------Arrange----------------------------
            var mockDeployDestinationExplorerViewModel = new Mock<IDeployDestinationExplorerViewModel>();
            var mockConnectControlViewModel = new Mock<IConnectControlViewModel>();
            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            var mockShellViewModel = new Mock<IShellViewModel>();

            mockExplorerItemViewModel.Setup(o => o.ActivityName).Returns("tesssssss");
            mockExplorerItemViewModel.Setup(o => o.ResourceType).Returns(@"not_Folder");
            mockExplorerItemViewModel.Setup(o => o.ResourcePath).Returns(@"Category\Testing");
            mockExplorerItemViewModel.Setup(o => o.ResourceId).Returns(new Guid("00000000-0000-0000-0000-000000000000"));
            mockExplorerItemViewModel.Setup(o => o.IsResourceChecked).Returns(true);

            mockExplorerTreeItem.Setup(a => a.ResourceId).Returns(new Guid("00000000-0000-0000-0000-000000000001"));
            mockExplorerTreeItem.Setup(a => a.CanDeploy).Returns(true);

            mockDeployDestinationExplorerViewModel.Setup(o => o.ConnectControlViewModel).Returns(mockConnectControlViewModel.Object);
            mockDeployDestinationExplorerViewModel.Setup(o => o.SelectedEnvironment).Returns(new Mock<IEnvironmentViewModel>().Object);
            mockDeployDestinationExplorerViewModel.Setup(o => o.SelectedEnvironment.IsConnected).Returns(true);
            mockDeployDestinationExplorerViewModel.Setup(o => o.SelectedEnvironment.AsList()).Returns(new List<IExplorerItemViewModel> { mockExplorerItemViewModel.Object });

            var deployStatsViewerViewModel = new DeployStatsViewerViewModel(new List<IExplorerTreeItem> { mockExplorerTreeItem.Object }, mockDeployDestinationExplorerViewModel.Object);
            //-------------------------Act--------------------------------
            deployStatsViewerViewModel.CheckDestinationPermissions();
            //-------------------------Assert-----------------------------
            mockShellViewModel.VerifyAll();
            mockConnectControlViewModel.VerifyAll();

            IExplorerTreeItem treeItem = mockExplorerTreeItem.Object;

            Assert.IsTrue(treeItem.CanDeploy);
            Assert.IsNull(deployStatsViewerViewModel.Status);
            Assert.IsNull(deployStatsViewerViewModel.RenameErrors);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DeployStatsViewerViewModel))]
        public void DeployStatsViewerViewModel_TryCalculate_DeployTests_False_Expect_None()
        {
            var mockDeployDestinationExplorerViewModel = new Mock<IDeployDestinationExplorerViewModel>();
            mockDeployDestinationExplorerViewModel.Setup(o => o.DeployTests).Returns(false);

            var guid = Guid.NewGuid();

            var serviceTestModelTo = new ServiceTestModelTO {ResourceId = guid};

            var serviceTestModelTos = new List<IServiceTestModelTO> {serviceTestModelTo};

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o => o.LoadResourceTestsForDeploy(guid)).Returns(serviceTestModelTos);
            mockResourceRepository.Setup(o => o.LoadResourceTriggersForDeploy(guid)).Returns(new List<ITriggerQueue>());

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.ResourceRepository).Returns(mockResourceRepository.Object);

            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModel.Setup(o => o.ResourceType).Returns(@"WorkflowService");
            mockExplorerItemViewModel.Setup(o => o.IsResourceChecked).Returns(true);
            mockExplorerItemViewModel.Setup(o => o.Server).Returns(mockServer.Object);

            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            mockExplorerTreeItem.Setup(o => o.ResourceId).Returns(guid);
            mockExplorerTreeItem.Setup(o => o.ResourceType).Returns(@"WorkflowService");
            mockExplorerTreeItem.Setup(o => o.IsResourceChecked).Returns(true);
            mockExplorerTreeItem.Setup(o => o.Server).Returns(mockServer.Object);

            var explorerTreeItems = new List<IExplorerTreeItem> { mockExplorerTreeItem.Object };
            var deployStatsViewerViewModel = new DeployStatsViewerViewModel(explorerTreeItems, mockDeployDestinationExplorerViewModel.Object);
            //-------------------------Act--------------------------------
            deployStatsViewerViewModel.TryCalculate(explorerTreeItems);

            Assert.AreEqual(1, deployStatsViewerViewModel.Services);
            Assert.AreEqual(0, deployStatsViewerViewModel.Tests);
            Assert.AreEqual(0, deployStatsViewerViewModel.Triggers);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DeployStatsViewerViewModel))]
        public void DeployStatsViewerViewModel_TryCalculate_DeployTests_True_Expect_Result()
        {
            var mockDeployDestinationExplorerViewModel = new Mock<IDeployDestinationExplorerViewModel>();
            mockDeployDestinationExplorerViewModel.Setup(o => o.DeployTests).Returns(true);

            var guid = Guid.NewGuid();

            var serviceTestModelTo = new ServiceTestModelTO {ResourceId = guid};

            var serviceTestModelTos = new List<IServiceTestModelTO> {serviceTestModelTo};

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o => o.LoadResourceTestsForDeploy(guid)).Returns(serviceTestModelTos);
            mockResourceRepository.Setup(o => o.LoadResourceTriggersForDeploy(guid)).Returns(new List<ITriggerQueue>());

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.ResourceRepository).Returns(mockResourceRepository.Object);

            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModel.Setup(o => o.ResourceType).Returns(@"WorkflowService");
            mockExplorerItemViewModel.Setup(o => o.IsResourceChecked).Returns(true);
            mockExplorerItemViewModel.Setup(o => o.Server).Returns(mockServer.Object);

            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            mockExplorerTreeItem.Setup(o => o.ResourceId).Returns(guid);
            mockExplorerTreeItem.Setup(o => o.ResourceType).Returns(@"WorkflowService");
            mockExplorerTreeItem.Setup(o => o.IsResourceChecked).Returns(true);
            mockExplorerTreeItem.Setup(o => o.Server).Returns(mockServer.Object);

            var explorerTreeItems = new List<IExplorerTreeItem> { mockExplorerTreeItem.Object };
            var deployStatsViewerViewModel = new DeployStatsViewerViewModel(explorerTreeItems, mockDeployDestinationExplorerViewModel.Object);
            //-------------------------Act--------------------------------
            deployStatsViewerViewModel.TryCalculate(explorerTreeItems);

            Assert.AreEqual(1, deployStatsViewerViewModel.Services);
            Assert.AreEqual(1, deployStatsViewerViewModel.Tests);
            Assert.AreEqual(0, deployStatsViewerViewModel.Triggers);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DeployStatsViewerViewModel))]
        public void DeployStatsViewerViewModel_DeployTests_UpdateTestsStatsArea_TestsOverrides()
        {
            var guid = Guid.NewGuid();
            var serviceTestModelTo = new ServiceTestModelTO {ResourceId = guid};
            var serviceTestModelTos = new List<IServiceTestModelTO> {serviceTestModelTo};

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o => o.LoadResourceTestsForDeploy(guid)).Returns(serviceTestModelTos);
            mockResourceRepository.Setup(o => o.LoadResourceTriggersForDeploy(guid)).Returns(new List<ITriggerQueue>());

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.ResourceRepository).Returns(mockResourceRepository.Object);

            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModel.Setup(o => o.ResourceId).Returns(guid);
            mockExplorerItemViewModel.Setup(o => o.ResourceType).Returns(@"WorkflowService");
            mockExplorerItemViewModel.Setup(o => o.IsResourceChecked).Returns(true);
            mockExplorerItemViewModel.Setup(o => o.Server).Returns(mockServer.Object);

            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            mockExplorerTreeItem.Setup(o => o.ResourceId).Returns(guid);
            mockExplorerTreeItem.Setup(o => o.ResourceType).Returns(@"WorkflowService");
            mockExplorerTreeItem.Setup(o => o.IsResourceChecked).Returns(true);
            mockExplorerTreeItem.Setup(o => o.Server).Returns(mockServer.Object);

            var mockExplorerItem = new Mock<IExplorerItemViewModel>();
            mockExplorerItem.Setup(o => o.ResourceId).Returns(guid);
            mockExplorerItem.Setup(o => o.ResourceType).Returns(@"WorkflowService");
            mockExplorerItem.Setup(o => o.IsResourceChecked).Returns(true);
            mockExplorerItem.Setup(o => o.Server).Returns(mockServer.Object);

            var explorerTreeItems = new List<IExplorerTreeItem> { mockExplorerTreeItem.Object };
            var explorerItems = new ObservableCollection<IExplorerItemViewModel> { mockExplorerItem.Object };

            var mockEnvironmentViewModel = new Mock<IEnvironmentViewModel>();
            mockEnvironmentViewModel.Setup(o => o.UnfilteredChildren).Returns(explorerItems);

            var mockDeployDestinationExplorerViewModel = new Mock<IDeployDestinationExplorerViewModel>();
            mockDeployDestinationExplorerViewModel.Setup(o => o.SelectedEnvironment)
                .Returns(mockEnvironmentViewModel.Object);
            mockDeployDestinationExplorerViewModel.Setup(o => o.DeployTests).Returns(true);

            var deployStatsViewerViewModel = new DeployStatsViewerViewModel(explorerTreeItems, mockDeployDestinationExplorerViewModel.Object);
            //-------------------------Act--------------------------------
            deployStatsViewerViewModel.UpdateTestsStatsArea();

            Assert.AreEqual(1, deployStatsViewerViewModel.OverridesTests);
            Assert.AreEqual(0, deployStatsViewerViewModel.NewTests);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DeployStatsViewerViewModel))]
        public void DeployStatsViewerViewModel_DeployTests_UpdateTestsStatsArea_ExpectNone()
        {
            var guid = Guid.NewGuid();
            var serviceTestModelTo = new ServiceTestModelTO {ResourceId = guid};
            var serviceTestModelTos = new List<IServiceTestModelTO> {serviceTestModelTo};

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o => o.LoadResourceTestsForDeploy(guid)).Returns(serviceTestModelTos);
            mockResourceRepository.Setup(o => o.LoadResourceTriggersForDeploy(guid)).Returns(new List<ITriggerQueue>());

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.ResourceRepository).Returns(mockResourceRepository.Object);

            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModel.Setup(o => o.ResourceId).Returns(guid);
            mockExplorerItemViewModel.Setup(o => o.ResourceType).Returns(@"WorkflowService");
            mockExplorerItemViewModel.Setup(o => o.IsResourceChecked).Returns(true);
            mockExplorerItemViewModel.Setup(o => o.Server).Returns(mockServer.Object);

            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            mockExplorerTreeItem.Setup(o => o.ResourceId).Returns(guid);
            mockExplorerTreeItem.Setup(o => o.ResourceType).Returns(@"WorkflowService");
            mockExplorerTreeItem.Setup(o => o.IsResourceChecked).Returns(true);
            mockExplorerTreeItem.Setup(o => o.Server).Returns(mockServer.Object);

            var explorerTreeItems = new List<IExplorerTreeItem> { mockExplorerTreeItem.Object };
            var mockEnvironmentViewModel = new Mock<IEnvironmentViewModel>();

            var mockDeployDestinationExplorerViewModel = new Mock<IDeployDestinationExplorerViewModel>();
            mockDeployDestinationExplorerViewModel.Setup(o => o.SelectedEnvironment)
                .Returns(mockEnvironmentViewModel.Object);
            mockDeployDestinationExplorerViewModel.Setup(o => o.DeployTests).Returns(true);

            var deployStatsViewerViewModel = new DeployStatsViewerViewModel(explorerTreeItems, mockDeployDestinationExplorerViewModel.Object);
            //-------------------------Act--------------------------------
            deployStatsViewerViewModel.UpdateTestsStatsArea();

            Assert.AreEqual(0, deployStatsViewerViewModel.OverridesTests);
            Assert.AreEqual(0, deployStatsViewerViewModel.NewTests);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DeployStatsViewerViewModel))]
        public void DeployStatsViewerViewModel_TryCalculate_DeployTriggers_Expect_None()
        {
            var mockDeployDestinationExplorerViewModel = new Mock<IDeployDestinationExplorerViewModel>();
            mockDeployDestinationExplorerViewModel.Setup(o => o.DeployTriggers).Returns(false);

            var guid = Guid.NewGuid();

            var triggerQueue = new TriggerQueue {ResourceId = guid};

            var triggerQueues = new List<ITriggerQueue> {triggerQueue};

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o => o.LoadResourceTriggersForDeploy(guid)).Returns(triggerQueues);
            mockResourceRepository.Setup(o => o.LoadResourceTestsForDeploy(guid)).Returns(new List<IServiceTestModelTO>());

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.ResourceRepository).Returns(mockResourceRepository.Object);

            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModel.Setup(o => o.ResourceType).Returns(@"WorkflowService");
            mockExplorerItemViewModel.Setup(o => o.IsResourceChecked).Returns(true);
            mockExplorerItemViewModel.Setup(o => o.Server).Returns(mockServer.Object);

            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            mockExplorerTreeItem.Setup(o => o.ResourceId).Returns(guid);
            mockExplorerTreeItem.Setup(o => o.ResourceType).Returns(@"WorkflowService");
            mockExplorerTreeItem.Setup(o => o.IsResourceChecked).Returns(true);
            mockExplorerTreeItem.Setup(o => o.Server).Returns(mockServer.Object);

            var explorerTreeItems = new List<IExplorerTreeItem> { mockExplorerTreeItem.Object };
            var deployStatsViewerViewModel = new DeployStatsViewerViewModel(explorerTreeItems, mockDeployDestinationExplorerViewModel.Object);
            //-------------------------Act--------------------------------
            deployStatsViewerViewModel.TryCalculate(explorerTreeItems);

            Assert.AreEqual(1, deployStatsViewerViewModel.Services);
            Assert.AreEqual(0, deployStatsViewerViewModel.Triggers);
            Assert.AreEqual(0, deployStatsViewerViewModel.Tests);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DeployStatsViewerViewModel))]
        public void DeployStatsViewerViewModel_TryCalculate_DeployTriggers_Expect_Result()
        {
            var mockDeployDestinationExplorerViewModel = new Mock<IDeployDestinationExplorerViewModel>();
            mockDeployDestinationExplorerViewModel.Setup(o => o.DeployTriggers).Returns(true);

            var guid = Guid.NewGuid();

            var triggerQueue = new TriggerQueue {ResourceId = guid};

            var triggerQueues = new List<ITriggerQueue> {triggerQueue};

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(o => o.LoadResourceTriggersForDeploy(guid)).Returns(triggerQueues);
            mockResourceRepository.Setup(o => o.LoadResourceTestsForDeploy(guid)).Returns(new List<IServiceTestModelTO>());

            var mockServer = new Mock<IServer>();
            mockServer.Setup(o => o.ResourceRepository).Returns(mockResourceRepository.Object);

            var mockExplorerItemViewModel = new Mock<IExplorerItemViewModel>();
            mockExplorerItemViewModel.Setup(o => o.ResourceType).Returns(@"WorkflowService");
            mockExplorerItemViewModel.Setup(o => o.IsResourceChecked).Returns(true);
            mockExplorerItemViewModel.Setup(o => o.Server).Returns(mockServer.Object);

            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            mockExplorerTreeItem.Setup(o => o.ResourceId).Returns(guid);
            mockExplorerTreeItem.Setup(o => o.ResourceType).Returns(@"WorkflowService");
            mockExplorerTreeItem.Setup(o => o.IsResourceChecked).Returns(true);
            mockExplorerTreeItem.Setup(o => o.Server).Returns(mockServer.Object);

            var explorerTreeItems = new List<IExplorerTreeItem> { mockExplorerTreeItem.Object };
            var deployStatsViewerViewModel = new DeployStatsViewerViewModel(explorerTreeItems, mockDeployDestinationExplorerViewModel.Object);
            //-------------------------Act--------------------------------
            deployStatsViewerViewModel.TryCalculate(explorerTreeItems);

            Assert.AreEqual(1, deployStatsViewerViewModel.Services);
            Assert.AreEqual(1, deployStatsViewerViewModel.Triggers);
            Assert.AreEqual(0, deployStatsViewerViewModel.Tests);
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
