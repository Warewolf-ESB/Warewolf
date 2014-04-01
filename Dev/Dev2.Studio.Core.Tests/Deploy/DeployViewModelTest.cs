using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using Caliburn.Micro;
using Dev2.AppResources.Enums;
using Dev2.Composition;
using Dev2.Core.Tests.Deploy;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.Utils;
using Dev2.Providers.Events;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.Deploy;
using Dev2.Studio.TO;
using Dev2.Studio.ViewModels.Deploy;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.ViewModels.Deploy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Dev2.Core.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DeployViewModelTest : DeployViewModelTestBase
    {
        #region Connect

        [TestMethod]
        public void DeployViewModelConnectWithServerExpectedDoesNotDisconnectOtherServers()
        {
            ImportService.CurrentContext = OkayContext;

            var source = EnviromentRepositoryTest.CreateMockEnvironment();
            var sourceConn = Mock.Get(source.Object.Connection);
            sourceConn.Setup(c => c.Disconnect()).Verifiable();

            var e1 = EnviromentRepositoryTest.CreateMockEnvironment();
            var c1 = Mock.Get(e1.Object.Connection);
            c1.Setup(c => c.Disconnect()).Verifiable();
            var s1 = e1.Object;

            var e2 = EnviromentRepositoryTest.CreateMockEnvironment();
            var c2 = Mock.Get(e2.Object.Connection);
            c2.Setup(c => c.Disconnect()).Verifiable();
            var s2 = e2.Object;

            var serverProvider = new Mock<IEnvironmentModelProvider>();
            serverProvider.Setup(s => s.Load()).Returns(new List<IEnvironmentModel> { s1, s2 });

            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);

            var deployViewModel = new DeployViewModel(AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, serverProvider.Object, repo, new Mock<IEventAggregator>().Object);

            // EnvironmentModel.IEquatable fails on Mock proxies - so clear before doing test!!
            deployViewModel.Source.Environments.Clear();
            deployViewModel.Target.Environments.Clear();


            Assert.IsTrue(source.Object.IsConnected);
            Assert.IsTrue(s1.IsConnected);
            Assert.IsTrue(s2.IsConnected);

            deployViewModel.SelectedSourceServer = s1;
            sourceConn.Verify(c => c.Disconnect(), Times.Never());
            c1.Verify(c => c.Disconnect(), Times.Never());
            c2.Verify(c => c.Disconnect(), Times.Never());

            deployViewModel.SelectedDestinationServer = s2;
            sourceConn.Verify(c => c.Disconnect(), Times.Never());
            c1.Verify(c => c.Disconnect(), Times.Never());
            c2.Verify(c => c.Disconnect(), Times.Never());

            Assert.IsTrue(source.Object.IsConnected);
            Assert.IsTrue(s1.IsConnected);
            Assert.IsTrue(s2.IsConnected);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DeployViewModel_SelectDestinationServer")]
        public void DeployViewModel_SelectDestinationServer_SelectDestinationServer_CalculateStatsHitOnce()
        {
            //------------Setup for test--------------------------
            ImportService.CurrentContext = OkayContext;

            var source = EnviromentRepositoryTest.CreateMockEnvironment();
            var sourceConn = Mock.Get(source.Object.Connection);
            sourceConn.Setup(c => c.Disconnect()).Verifiable();

            var e1 = EnviromentRepositoryTest.CreateMockEnvironment();
            var c1 = Mock.Get(e1.Object.Connection);
            c1.Setup(c => c.Disconnect()).Verifiable();
            var s1 = e1.Object;

            var e2 = EnviromentRepositoryTest.CreateMockEnvironment();
            var c2 = Mock.Get(e2.Object.Connection);
            c2.Setup(c => c.Disconnect()).Verifiable();
            var s2 = e2.Object;

            var serverProvider = new Mock<IEnvironmentModelProvider>();
            serverProvider.Setup(s => s.Load()).Returns(new List<IEnvironmentModel> { s1, s2 });

            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);

            Mock<IDeployStatsCalculator> mockDeployStatsCalculator = new Mock<IDeployStatsCalculator>();
            int calcStats;
            mockDeployStatsCalculator.Setup(c => c.CalculateStats(It.IsAny<IEnumerable<ITreeNode>>(), It.IsAny<Dictionary<string, Func<ITreeNode, bool>>>(), It.IsAny<ObservableCollection<DeployStatsTO>>(), out calcStats)).Verifiable();

            var deployViewModel = new DeployViewModel(AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, serverProvider.Object, repo, new Mock<IEventAggregator>().Object, mockDeployStatsCalculator.Object);

            // EnvironmentModel.IEquatable fails on Mock proxies - so clear before doing test!!
            deployViewModel.Source.Environments.Clear();
            deployViewModel.Target.Environments.Clear();

            //------------Execute Test---------------------------

            deployViewModel.SelectedSourceServer = s1;

            deployViewModel.SelectedDestinationServer = s2;

            //------------Assert Results-------------------------

            mockDeployStatsCalculator.Verify(c => c.CalculateStats(It.IsAny<IEnumerable<ITreeNode>>(), It.IsAny<Dictionary<string, Func<ITreeNode, bool>>>(), It.IsAny<ObservableCollection<DeployStatsTO>>(), out calcStats), Times.Exactly(2));
        }

        #endregion

        #region Deploy

        [TestMethod]
        public void DeployViewModelDeployWithServerExpectedDoesNotDisconnectOtherServers()
        {
            ImportService.CurrentContext = OkayContext;

            var source = EnviromentRepositoryTest.CreateMockEnvironment();
            var sourceConn = Mock.Get(source.Object.Connection);
            sourceConn.Setup(c => c.Disconnect()).Verifiable();

            var e1 = EnviromentRepositoryTest.CreateMockEnvironment();
            var s1 = e1.Object;
            var c1 = Mock.Get(e1.Object.Connection);
            c1.Setup(c => c.Disconnect()).Verifiable();

            var resourceRepo1 = new ResourceRepository(e1.Object);
            e1.Setup(e => e.ResourceRepository).Returns(resourceRepo1);
            var r1 = new Mock<IContextualResourceModel>();
            r1.Setup(r => r.Category).Returns("test");
            r1.Setup(r => r.ResourceName).Returns("testResource");
            r1.Setup(r => r.Environment).Returns(e1.Object);
            resourceRepo1.Add(r1.Object);

            var e2 = EnviromentRepositoryTest.CreateMockEnvironment();
            var s2 = e2.Object;
            var c2 = Mock.Get(e2.Object.Connection);
            c2.Setup(c => c.Disconnect()).Verifiable();

            var resourceRepo2 = new ResourceRepository(e2.Object);
            e2.Setup(e => e.ResourceRepository).Returns(resourceRepo2);

            var serverProvider = new Mock<IEnvironmentModelProvider>();
            serverProvider.Setup(s => s.Load()).Returns(new List<IEnvironmentModel> { s1, s2 });

            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);

            var statsCalc = new Mock<IDeployStatsCalculator>();
            statsCalc.Setup(s => s.SelectForDeployPredicate(It.IsAny<ITreeNode>())).Returns(true);

            var deployViewModel = new DeployViewModel(AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, serverProvider.Object, repo, new Mock<IEventAggregator>().Object, statsCalc.Object) { SelectedSourceServer = s1, SelectedDestinationServer = s2 };

            Assert.IsTrue(source.Object.IsConnected);
            Assert.IsTrue(s1.IsConnected);
            Assert.IsTrue(s2.IsConnected);

            deployViewModel.DeployCommand.Execute(null);

            sourceConn.Verify(c => c.Disconnect(), Times.Never());
            c1.Verify(c => c.Disconnect(), Times.Never());
            c2.Verify(c => c.Disconnect(), Times.Never());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DeployViewModelTest")]
        public void DeployViewModelTest_DeployCommand_AConflictWasFound_DialogIsShown()
        {
            DeployViewModel deployViewModel;

            var deployStatsCalculator = SetupDeployViewModel(out deployViewModel);

            var isOverwriteMessageDisplayed = false;

            deployViewModel.ShowDialog = o =>
                {
                    var viewModel = (DeployDialogViewModel)o;
                    viewModel.DialogResult = ViewModelDialogResults.Cancel;
                    isOverwriteMessageDisplayed = true;
                };

            SetupResources(deployStatsCalculator, true);
            deployViewModel.DeployCommand.Execute(null);

            Assert.IsTrue(isOverwriteMessageDisplayed);
            Assert.IsFalse(deployViewModel.DeploySuccessfull);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DeployViewModelTest")]
        public void DeployViewModelTest_DeployCommand_AConflictWasFoundAndUserOptsToOverwrite_DeploysSuccessfully()
        {
            DeployViewModel deployViewModel;

            var deployStatsCalculator = SetupDeployViewModel(out deployViewModel);

            var isOverwriteMessageDisplayed = false;

            deployViewModel.ShowDialog = o =>
            {
                var viewModel = (DeployDialogViewModel)o;
                viewModel.DialogResult = ViewModelDialogResults.Okay;
                isOverwriteMessageDisplayed = true;
            };

            deployViewModel.HasNoResourcesToDeploy = (o, i) => false;

            SetupResources(deployStatsCalculator, true);

            var mockEnv = EnviromentRepositoryTest.CreateMockEnvironment();
            var resourceRepository = new Mock<IResourceRepository>();
            mockEnv.Setup(m => m.ResourceRepository).Returns(resourceRepository.Object);
            resourceRepository.Setup(m => m.DeleteResource(It.IsAny<IResourceModel>()));
            deployViewModel.Target.Environments[0] = mockEnv.Object;

            deployViewModel.DeployCommand.Execute(null);

            Assert.IsTrue(isOverwriteMessageDisplayed);
            Assert.IsTrue(deployViewModel.DeploySuccessfull);
            resourceRepository.Verify(m => m.DeleteResource(It.IsAny<IResourceModel>()));
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DeployViewModelTest")]
        public void DeployViewModelTest_DeployCommand_AConflictWasNotFound_DialogIsNotShown()
        {
            DeployViewModel deployViewModel;

            var deployStatsCalculator = SetupDeployViewModel(out deployViewModel);

            var isOverwriteMessageDisplayed = false;

            deployViewModel.ShowDialog = o =>
            {
                var viewModel = (DeployDialogViewModel)o;
                viewModel.DialogResult = ViewModelDialogResults.Cancel;
                isOverwriteMessageDisplayed = true;
            };

            SetupResources(deployStatsCalculator, false);
            deployViewModel.DeployCommand.Execute(null);

            Assert.IsFalse(isOverwriteMessageDisplayed);
            Assert.IsFalse(deployViewModel.DeploySuccessfull);
        }
        #endregion

        #region AddServerToDeployMessage

        [TestMethod]
        public void HandleAddServerToDeployMessageWithSourceContextExpectSelectedAsSource()
        {
            IEnvironmentModel server;
            DeployViewModel vm;
            var mockEventAggregator = new Mock<IEventAggregator>();

            var envID = SetupVmForMessages(out server, out vm, mockEventAggregator);

            var msg = new AddServerToDeployMessage(server, ConnectControlInstanceType.DeploySource);
            vm.Handle(msg);
            Assert.IsTrue(vm.SelectedSourceServer.ID == envID);
        }


        [TestMethod]
        public void HandleAddServerToDeployMessageWithDestinationContextExpectSelectedAsDestination()
        {
            IEnvironmentModel server;
            DeployViewModel vm;
            var mockEventAggregator = new Mock<IEventAggregator>();

            var envID = SetupVmForMessages(out server, out vm, mockEventAggregator);

            var msg = new AddServerToDeployMessage(server, ConnectControlInstanceType.DeployTarget);
            vm.Handle(msg);
            Assert.IsTrue(vm.SelectedDestinationServer.ID == envID);
        }

        [TestMethod]
        public void HandleAddServerToDeployMessageWithIsSourceTrueExpectSelectedAsSource()
        {
            IEnvironmentModel server;
            DeployViewModel vm;
            var envID = SetupVmForMessages(out server, out vm);

            var msg = new AddServerToDeployMessage(server, ConnectControlInstanceType.DeploySource);
            vm.Handle(msg);
            Assert.IsTrue(vm.SelectedSourceServer.ID == envID);
        }

        [TestMethod]
        public void HandleAddServerToDeployMessageWithIsDestinationTrueExpectSelectedAsDestination()
        {
            IEnvironmentModel server;
            DeployViewModel vm;
            var envID = SetupVmForMessages(out server, out vm);

            var msg = new AddServerToDeployMessage(server, ConnectControlInstanceType.DeployTarget);
            vm.Handle(msg);
            Assert.IsTrue(vm.SelectedDestinationServer.ID == envID);
        }

        [TestMethod]
        public void IsInstanceOfIHandleEnvironmentDeleted()
        {
            IEnvironmentModel server;
            DeployViewModel vm;
            SetupVmForMessages(out server, out vm);
            Assert.IsInstanceOfType(vm, typeof(IHandle<EnvironmentDeletedMessage>));
        }

        [TestMethod]
        public void EnvironmentDeletedCallsREmoveEnvironmentFromBothSourceAndDestinationNavigationViewModels()
        {
            //Setup
            IEnvironmentModel server;
            DeployViewModel vm;
            SetupVmForMessages(out server, out vm);
            var mockEnv = EnviromentRepositoryTest.CreateMockEnvironment();
            vm.Target.AddEnvironment(mockEnv.Object);
            vm.Source.AddEnvironment(mockEnv.Object);
            Assert.AreEqual(1, vm.Target.Environments.Count);
            Assert.AreEqual(1, vm.Source.Environments.Count);

            //Test
            var msg = new EnvironmentDeletedMessage(mockEnv.Object);
            vm.Handle(msg);

            //Assert
            Assert.AreEqual(0, vm.Target.Environments.Count);
            Assert.AreEqual(0, vm.Source.Environments.Count);
        }
        #endregion

        #region SelectItemInDeployMessage

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("DeployViewModel_SelectItemInDeploy")]
        public void DeployViewModel_SelectItemInDeploy_TwoServers_ItemAndServerSelected()
        {
            //MEFF
            var importServiceContext = new ImportServiceContext();
            ImportService.CurrentContext = importServiceContext;
            ImportService.Initialize(new List<ComposablePartCatalog>());

            //New Mocks
            var mockedServerRepo = new Mock<IEnvironmentRepository>();
            var server = new Mock<IEnvironmentModel>();
            var secondServer = new Mock<IEnvironmentModel>();
            var provider = new Mock<IEnvironmentModelProvider>();
            var resourceNode = new Mock<IContextualResourceModel>();

            //Setup Servers
            server.Setup(svr => svr.IsConnected).Returns(true);
            server.Setup(svr => svr.Connection).Returns(DebugOutputViewModelTest.CreateMockConnection(new Random(), new string[0]).Object);
            secondServer.Setup(svr => svr.IsConnected).Returns(true);
            secondServer.Setup(svr => svr.Connection).Returns(DebugOutputViewModelTest.CreateMockConnection(new Random(), new string[0]).Object);
            mockedServerRepo.Setup(svr => svr.Fetch(It.IsAny<IEnvironmentModel>())).Returns(server.Object);
            provider.Setup(prov => prov.Load()).Returns(new List<IEnvironmentModel> { server.Object, secondServer.Object });

            //Setup Navigation Tree
            var eventAggregator = new Mock<IEventAggregator>().Object;
            var mockedSource = new NavigationViewModel(eventAggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, It.IsAny<Guid>(), mockedServerRepo.Object);
            var treeParent = new CategoryTreeViewModel(eventAggregator, null, "Test Category", ResourceType.WorkflowService)
            {
                IsExpanded = false
            };
            const string expectedResourceName = "Test Resource";
            resourceNode.Setup(res => res.ResourceName).Returns(expectedResourceName);
            resourceNode.Setup(res => res.Environment.Connection.ServerEvents).Returns(new Mock<IEventPublisher>().Object);

            var resourceTreeNode = new ResourceTreeViewModel(eventAggregator, treeParent, resourceNode.Object);

            //Setup Server Resources
            server.Setup(svr => svr.LoadResources()).Callback(() => mockedSource.Root.Add(treeParent));

            var deployViewModel = new DeployViewModel(AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, provider.Object, mockedServerRepo.Object, new Mock<IEventAggregator>().Object)
            {
                Source = mockedSource
            };

            var initialResource = new Mock<IContextualResourceModel>();
            initialResource.Setup(res => res.Environment).Returns(server.Object);
            initialResource.Setup(res => res.ResourceName).Returns(expectedResourceName);

            //------------Execute Test--------------------------- 
            deployViewModel.Handle(new SelectItemInDeployMessage(initialResource.Object.ResourceName, initialResource.Object.Environment));

            // Assert item visible and selected
            Assert.IsTrue(resourceTreeNode.IsChecked.GetValueOrDefault(), "Deployed item not selected in deploy");
            Assert.IsTrue(treeParent.IsExpanded, "Item not visible in deploy view");
        }

        #endregion

        [TestMethod]
        [Description("DeployViewModel CanDeploy must be false if server is disconnected.")]
        [TestCategory("DeployViewModel_CanDeploy")]
        [Owner("Trevor Williams-Ros")]
        public void DeployViewModel_UnitTest_CanDeployToDisconnectedServer_ReturnsFalse()
        {
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockSourceServer.Setup(server => server.IsConnected).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployFrom).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployTo).Returns(true);

            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);

            deployViewModel.SelectedDestinationServer = destServer.Object;
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            deployViewModel.HasItemsToDeploy = (sourceDeployItemCount, destinationDeployItemCount) => true;

            destEnv.Setup(e => e.IsAuthorizedDeployTo).Returns(true);

            destEnv.Setup(e => e.IsConnected).Returns(true);
            Assert.IsTrue(deployViewModel.CanDeploy, "DeployViewModel CanDeploy is false when server is connected.");

            destEnv.Setup(e => e.IsConnected).Returns(false);
            //Changed to true because it was showing the error when not connected which isnt right
            Assert.IsTrue(deployViewModel.CanDeploy, "DeployViewModel CanDeploy is false when server is disconnected.");
        }

        [TestMethod]
        [TestCategory("DeployViewModel_SelectedSourceServerDisconnected")]
        [Owner("Trevor Williams-Ros")]
        public void DeployViewModel_SourceServerDisconnected_SourceServerHasDroppedTrue()
        {
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockSourceServer.Setup(server => server.IsConnected).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployFrom).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployTo).Returns(true);

            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);

            deployViewModel.SelectedDestinationServer = destServer.Object;
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            deployViewModel.HasItemsToDeploy = (sourceDeployItemCount, destinationDeployItemCount) => true;
            destEnv.Setup(e => e.IsAuthorizedDeployTo).Returns(true);

            Assert.IsFalse(deployViewModel.SourceServerHasDropped);

            mockSourceServer.Setup(server => server.IsConnected).Returns(false);
            var connectedEventArgs = new ConnectedEventArgs { IsConnected = false };
            mockSourceServer.Raise(model => model.IsConnectedChanged += null, connectedEventArgs);

            Assert.IsTrue(deployViewModel.SourceServerHasDropped);
            Assert.IsTrue(deployViewModel.ShowServerDisconnectedMessage);
            Assert.AreEqual("Source server has disconnected.", deployViewModel.ServerDisconnectedMessage);
        }

        [TestMethod]
        [TestCategory("DeployViewModel_SelectedSourceServerDisconnectedMustClearConflicts")]
        [Owner("Leon Rajindrapersadh")]
        public void DeployViewModel_SourceServerDisconnected_ClearsConflictingNodes()
        {

            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockSourceServer.Setup(server => server.IsConnected).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployFrom).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployTo).Returns(true);

            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;


            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);

            deployViewModel.SelectedDestinationServer = destServer.Object;

            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            var target = new PartialNaviationViewModel(deployViewModel.Target);
            deployViewModel.Target = target;
            deployViewModel.HasItemsToDeploy = (sourceDeployItemCount, destinationDeployItemCount) => true;
            destEnv.Setup(e => e.IsAuthorizedDeployTo).Returns(true);

            Assert.IsFalse(deployViewModel.SourceServerHasDropped);

            mockSourceServer.Setup(server => server.IsConnected).Returns(false);
            var connectedEventArgs = new ConnectedEventArgs { IsConnected = false };
            deployViewModel.SourceEnvironmentConnectedChanged(this, connectedEventArgs);

            Assert.IsTrue(target.ClearCalled);


        }

        [TestMethod]
        [TestCategory("DeployViewModel_SelectedSourceServerDisconnected")]
        [Owner("Trevor Williams-Ros")]
        public void DeployViewModel_DestinationServerDisconnected_DestinationServerHasDroppedTrue()
        {
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockSourceServer.Setup(server => server.IsConnected).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployFrom).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployTo).Returns(true);

            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);

            deployViewModel.SelectedDestinationServer = destServer.Object;
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            deployViewModel.HasItemsToDeploy = (sourceDeployItemCount, destinationDeployItemCount) => true;
            destEnv.Setup(e => e.IsAuthorizedDeployTo).Returns(true);

            Assert.IsFalse(deployViewModel.DestinationServerHasDropped);
            destEnv.Setup(server => server.IsConnected).Returns(false);
            var connectedEventArgs = new ConnectedEventArgs { IsConnected = false };
            destEnv.Raise(model => model.IsConnectedChanged += null, connectedEventArgs);

            Assert.IsTrue(deployViewModel.DestinationServerHasDropped);
            Assert.IsTrue(deployViewModel.ShowServerDisconnectedMessage);
            Assert.AreEqual("Destination server has disconnected.", deployViewModel.ServerDisconnectedMessage);

        }

        [TestMethod]
        [TestCategory("DeployViewModel_SelectedSourceServerDisconnected")]
        [Owner("Trevor Williams-Ros")]
        public void DeployViewModel_SourceAndDestinationServerDisconnected_BothPropertiesTrue()
        {
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockSourceServer.Setup(server => server.IsConnected).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployFrom).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployTo).Returns(true);

            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);

            deployViewModel.SelectedDestinationServer = destServer.Object;
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            deployViewModel.HasItemsToDeploy = (sourceDeployItemCount, destinationDeployItemCount) => true;
            destEnv.Setup(e => e.IsAuthorizedDeployTo).Returns(true);
            destEnv.Setup(server => server.IsConnected).Returns(true);
            Assert.IsFalse(deployViewModel.DestinationServerHasDropped);

            var connectedEventArgs = new ConnectedEventArgs { IsConnected = false };
            destEnv.Setup(server => server.IsConnected).Returns(false);
            mockSourceServer.Setup(server => server.IsConnected).Returns(false);
            destEnv.Raise(model => model.IsConnectedChanged += null, connectedEventArgs);
            mockSourceServer.Raise(model => model.IsConnectedChanged += null, connectedEventArgs);

            Assert.IsTrue(deployViewModel.DestinationServerHasDropped);
            Assert.IsTrue(deployViewModel.SourceServerHasDropped);
            Assert.IsTrue(deployViewModel.ShowServerDisconnectedMessage);
            Assert.AreEqual("Source and Destination servers have disconnected.", deployViewModel.ServerDisconnectedMessage);

        }

        [TestMethod]
        [TestCategory("DeployViewModel_CanDeploy")]
        [Owner("Hagashen Naidu")]
        public void DeployViewModel_CanDeployWithSameSourceAndDestinationServer_ReturnsFalse()
        {
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            destServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            deployViewModel.SelectedDestinationServer = destServer.Object;
            deployViewModel.SelectedSourceServer = destServer.Object;

            destEnv.Setup(e => e.IsConnected).Returns(true);
            Assert.IsFalse(deployViewModel.CanDeploy);
        }

        [TestMethod]
        [TestCategory("DeployViewModel_CanDeploy")]
        [Owner("Trevor Williams-Ros")]
        public void DeployViewModel_CanDeploy_IsAuthorizedToDeployToFrom_Correct()
        {
            Verify_CanDeploy_IsAuthorized(expectedCanDeploy: false, isAuthorizedDeployFrom: false, isAuthorizedDeployTo: false);
            Verify_CanDeploy_IsAuthorized(expectedCanDeploy: false, isAuthorizedDeployFrom: false, isAuthorizedDeployTo: true);
            Verify_CanDeploy_IsAuthorized(expectedCanDeploy: false, isAuthorizedDeployFrom: true, isAuthorizedDeployTo: false);
            Verify_CanDeploy_IsAuthorized(expectedCanDeploy: true, isAuthorizedDeployFrom: true, isAuthorizedDeployTo: true);
        }

        void Verify_CanDeploy_IsAuthorized(bool expectedCanDeploy, bool isAuthorizedDeployFrom, bool isAuthorizedDeployTo)
        {
            var sourceConnection = new Mock<IEnvironmentConnection>();
            sourceConnection.Setup(c => c.AppServerUri).Returns(new Uri("http://localhost"));

            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(e => e.Connection).Returns(sourceConnection.Object);
            mockSourceServer.Setup(server => server.IsConnected).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployFrom).Returns(isAuthorizedDeployFrom);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployTo).Returns(true);

            var remoteConnection = new Mock<IEnvironmentConnection>();
            remoteConnection.Setup(c => c.AppServerUri).Returns(new Uri("http://remote"));

            var mockDestinationServer = new Mock<IEnvironmentModel>();
            mockDestinationServer.Setup(e => e.Connection).Returns(remoteConnection.Object);
            mockDestinationServer.Setup(server => server.IsConnected).Returns(true);
            mockDestinationServer.Setup(server => server.IsAuthorizedDeployFrom).Returns(true);
            mockDestinationServer.Setup(server => server.IsAuthorizedDeployTo).Returns(isAuthorizedDeployTo);

            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            deployViewModel.SelectedDestinationServer = mockDestinationServer.Object;
            deployViewModel.HasItemsToDeploy = (sourceDeployItemCount, destinationDeployItemCount) => true;

            Assert.AreEqual(expectedCanDeploy, deployViewModel.CanDeploy);
        }


        [TestMethod]
        [TestCategory("DeployViewModel_CanSelectAllDependencies")]
        [Owner("Trevor Williams-Ros")]
        public void DeployViewModel_CanSelectAllDependencies_IsAuthorizedToDeployFrom_Correct()
        {
            ImportService.CurrentContext = OkayContext;
            Verify_CanSelectAllDependencies_IsAuthorized(expectedCanSelect: false, isAuthorizedDeployFrom: false);
            Verify_CanSelectAllDependencies_IsAuthorized(expectedCanSelect: true, isAuthorizedDeployFrom: true);
        }

        void Verify_CanSelectAllDependencies_IsAuthorized(bool expectedCanSelect, bool isAuthorizedDeployFrom)
        {
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            mockSourceServer.Setup(server => server.IsConnected).Returns(true);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployFrom).Returns(isAuthorizedDeployFrom);
            mockSourceServer.Setup(server => server.IsAuthorizedDeployTo).Returns(true);

            var mockDestinationServer = new Mock<IEnvironmentModel>();
            mockDestinationServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://remote"));
            mockDestinationServer.Setup(server => server.IsConnected).Returns(true);
            mockDestinationServer.Setup(server => server.IsAuthorizedDeployFrom).Returns(true);
            mockDestinationServer.Setup(server => server.IsAuthorizedDeployTo).Returns(true);

            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            deployViewModel.SelectedDestinationServer = mockDestinationServer.Object;

            Assert.AreEqual(expectedCanSelect, deployViewModel.CanSelectAllDependencies);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_ServersAreNotTheSame")]
        public void DeployViewModel_ServersAreNotTheSame_SourceServerIsNull_True()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            deployViewModel.SelectedSourceServer = null;
            deployViewModel.SelectedDestinationServer = new Mock<IEnvironmentModel>().Object;
            //------------Execute Test---------------------------
            var serversAreNotTheSame = deployViewModel.ServersAreNotTheSame;
            //------------Assert Results-------------------------
            Assert.IsTrue(serversAreNotTheSame);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_ServersAreNotTheSame")]
        public void DeployViewModel_ServersAreNotTheSame_DestinationServerIsNull_True()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            deployViewModel.SelectedSourceServer = new Mock<IEnvironmentModel>().Object;
            deployViewModel.SelectedDestinationServer = null;
            //------------Execute Test---------------------------
            var serversAreNotTheSame = deployViewModel.ServersAreNotTheSame;
            //------------Assert Results-------------------------
            Assert.IsTrue(serversAreNotTheSame);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_ServersAreNotTheSame")]
        public void DeployViewModel_ServersAreNotTheSame_DestinationServerNullAppAddress_True()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            var mockDestinationServer = new Mock<IEnvironmentModel>();
            mockDestinationServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://different"));

            deployViewModel.SelectedDestinationServer = mockDestinationServer.Object;
            //------------Execute Test---------------------------
            var serversAreNotTheSame = deployViewModel.ServersAreNotTheSame;
            //------------Assert Results-------------------------
            Assert.IsTrue(serversAreNotTheSame);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_ServersAreNotTheSame")]
        public void DeployViewModel_ServersAreNotTheSame_SourceServerNullAppAddress_True()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            var mockDestinationServer = new Mock<IEnvironmentModel>();
            mockDestinationServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://remote"));
            deployViewModel.SelectedDestinationServer = mockDestinationServer.Object;
            //------------Execute Test---------------------------
            var serversAreNotTheSame = deployViewModel.ServersAreNotTheSame;
            //------------Assert Results-------------------------
            Assert.IsTrue(serversAreNotTheSame);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_ServersAreNotTheSame")]
        public void DeployViewModel_ServersAreNotTheSame_SourceDestinationServerNotSameAppAddress_True()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            var mockDestinationServer = new Mock<IEnvironmentModel>();
            mockDestinationServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://remote"));
            deployViewModel.SelectedDestinationServer = mockDestinationServer.Object;
            //------------Execute Test---------------------------
            var serversAreNotTheSame = deployViewModel.ServersAreNotTheSame;
            //------------Assert Results-------------------------
            Assert.IsTrue(serversAreNotTheSame);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_ServersAreNotTheSame")]
        public void DeployViewModel_ServersAreNotTheSame_SourceDestinationServerSameAppAddress_False()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> destEnv;
            Mock<IEnvironmentModel> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            var mockSourceServer = new Mock<IEnvironmentModel>();
            mockSourceServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            var mockDestinationServer = new Mock<IEnvironmentModel>();
            mockDestinationServer.Setup(server => server.Connection.AppServerUri).Returns(new Uri("http://localhost"));
            deployViewModel.SelectedDestinationServer = mockDestinationServer.Object;
            //------------Execute Test---------------------------
            var serversAreNotTheSame = deployViewModel.ServersAreNotTheSame;
            //------------Assert Results-------------------------
            Assert.IsFalse(serversAreNotTheSame);
        }

    }

    public class PartialNaviationViewModel : NavigationViewModel
    {
        public bool ClearCalled { get; set; }

        public PartialNaviationViewModel(NavigationViewModel model)
            : base(model.EventAggregator, model.AsyncWorker, model.Context, model.EnvironmentRepository, model.IsFromActivityDrop, model.DsfActivityType)
        {

        }

        public override void ClearConflictingNodesNodes()
        {
            ClearCalled = true;
        }
    }
}
