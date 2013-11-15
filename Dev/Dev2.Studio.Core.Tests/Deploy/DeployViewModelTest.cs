using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.Utils;
using Dev2.Messages;
using Dev2.Providers.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Studio.Deploy;
using Dev2.Studio.Enums;
using Dev2.Studio.TO;
using Dev2.Studio.ViewModels.Deploy;
using Dev2.Studio.ViewModels.Navigation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming

namespace Dev2.Core.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    [Ignore]//Ashley: 15-11-2013 background nullref exception in navigationviewmodel load resources async during unit test run in environment (round 1)
    public class DeployViewModelTest
    {
        #region Class Members

        private static ImportServiceContext _okayContext;
        private static ImportServiceContext _cancelContext;
        private static Mock<IWindowManager> _windowManager;

        #endregion Class Members

        #region Initialization

        [ClassInitialize()]
        public static void MyTestClassInitialize(TestContext testContext)
        {
            _windowManager = new Mock<IWindowManager>();
            _okayContext = CompositionInitializer.DeployViewModelOkayTest(_windowManager);
            _cancelContext = CompositionInitializer.DeployViewModelCancelTest();
        }

        #endregion Initialization

        #region Connect

        [TestMethod]
        public void DeployViewModelConnectWithServerExpectedDoesNotDisconnectOtherServers()
        {
            // BUG 9276 : TWR : 2013.04.19
            ImportService.CurrentContext = _okayContext;

            var source = EnviromentRepositoryTest.CreateMockEnvironment();
            var sourceConn = Mock.Get(source.Object.Connection);
            sourceConn.Setup(c => c.Disconnect()).Verifiable();

            var e1 = EnviromentRepositoryTest.CreateMockEnvironment();
            var c1 = Mock.Get(e1.Object.Connection);
            c1.Setup(c => c.Disconnect()).Verifiable();
            var s1 = new ServerDTO(e1.Object);

            var e2 = EnviromentRepositoryTest.CreateMockEnvironment();
            var c2 = Mock.Get(e2.Object.Connection);
            c2.Setup(c => c.Disconnect()).Verifiable();
            var s2 = new ServerDTO(e2.Object);

            var serverProvider = new Mock<IServerProvider>();
            serverProvider.Setup(s => s.Load()).Returns(new List<IServer> { s1, s2 });

            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);

            var deployViewModel = new DeployViewModel(serverProvider.Object, repo, new Mock<IEventAggregator>().Object);

            // EnvironmentModel.IEquatable fails on Mock proxies - so clear before doing test!!
            deployViewModel.Source.Environments.Clear();
            deployViewModel.Target.Environments.Clear();


            Assert.IsTrue(source.Object.IsConnected);
            Assert.IsTrue(s1.Environment.IsConnected);
            Assert.IsTrue(s2.Environment.IsConnected);

            deployViewModel.SelectedSourceServer = s1;
            sourceConn.Verify(c => c.Disconnect(), Times.Never());
            c1.Verify(c => c.Disconnect(), Times.Never());
            c2.Verify(c => c.Disconnect(), Times.Never());

            deployViewModel.SelectedDestinationServer = s2;
            sourceConn.Verify(c => c.Disconnect(), Times.Never());
            c1.Verify(c => c.Disconnect(), Times.Never());
            c2.Verify(c => c.Disconnect(), Times.Never());

            Assert.IsTrue(source.Object.IsConnected);
            Assert.IsTrue(s1.Environment.IsConnected);
            Assert.IsTrue(s2.Environment.IsConnected);
        }

        #endregion

        #region Deploy

        [TestMethod]
        public void DeployViewModelDeployWithServerExpectedDoesNotDisconnectOtherServers()
        {
            // BUG 9276 : TWR : 2013.04.19
            ImportService.CurrentContext = _okayContext;

            var source = EnviromentRepositoryTest.CreateMockEnvironment();
            var sourceConn = Mock.Get(source.Object.Connection);
            sourceConn.Setup(c => c.Disconnect()).Verifiable();

            var e1 = EnviromentRepositoryTest.CreateMockEnvironment();
            var s1 = new ServerDTO(e1.Object);
            var c1 = Mock.Get(e1.Object.Connection);
            c1.Setup(c => c.Disconnect()).Verifiable();

            var resourceRepo1 = new ResourceRepository(e1.Object, new Mock<IWizardEngine>().Object, new Mock<IFrameworkSecurityContext>().Object);
            e1.Setup(e => e.ResourceRepository).Returns(resourceRepo1);
            var r1 = new Mock<IContextualResourceModel>();
            r1.Setup(r => r.Category).Returns("test");
            r1.Setup(r => r.ResourceName).Returns("testResource");
            r1.Setup(r => r.Environment).Returns(e1.Object);
            resourceRepo1.Add(r1.Object);

            var e2 = EnviromentRepositoryTest.CreateMockEnvironment();
            var s2 = new ServerDTO(e2.Object);
            var c2 = Mock.Get(e2.Object.Connection);
            c2.Setup(c => c.Disconnect()).Verifiable();

            var resourceRepo2 = new ResourceRepository(e2.Object, new Mock<IWizardEngine>().Object, new Mock<IFrameworkSecurityContext>().Object);
            e2.Setup(e => e.ResourceRepository).Returns(resourceRepo2);

            var serverProvider = new Mock<IServerProvider>();
            serverProvider.Setup(s => s.Load()).Returns(new List<IServer> { s1, s2 });

            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);

            var statsCalc = new Mock<IDeployStatsCalculator>();
            statsCalc.Setup(s => s.SelectForDeployPredicate(It.IsAny<ITreeNode>())).Returns(true);

            var deployViewModel = new DeployViewModel(serverProvider.Object, repo, new Mock<IEventAggregator>().Object, statsCalc.Object);

            deployViewModel.SelectedSourceServer = s1;
            deployViewModel.SelectedDestinationServer = s2;

            Assert.IsTrue(source.Object.IsConnected);
            Assert.IsTrue(s1.Environment.IsConnected);
            Assert.IsTrue(s2.Environment.IsConnected);

            deployViewModel.DeployCommand.Execute(null);

            sourceConn.Verify(c => c.Disconnect(), Times.Never());
            c1.Verify(c => c.Disconnect(), Times.Never());
            c2.Verify(c => c.Disconnect(), Times.Never());
        }

        #endregion

        #region AddServerToDeployMessage

        [TestMethod]
        public void HandleAddServerToDeployMessageWithSourceContextExpectSelectedAsSource()
        {
            ServerDTO server;
            DeployViewModel vm;
            IEnvironmentModel publishedEnvironmentModel = null;
            bool publishedIsSource = false;
            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<UpdateSelectedServer>())).Callback<Object>(updateSelectedServer =>
            {
                publishedEnvironmentModel = ((UpdateSelectedServer)updateSelectedServer).EnvironmentModel;
                publishedIsSource = ((UpdateSelectedServer)updateSelectedServer).IsSourceServer;
            });
            var envID = SetupVMForMessages(out server, out vm, mockEventAggregator);

            var sourceCtx = vm.SourceContext;

            var msg = new AddServerToDeployMessage(server, sourceCtx);
            vm.Handle(msg);
            Assert.IsTrue(vm.SelectedSourceServer.ID.Equals(envID.ToString()));
            Assert.IsTrue(publishedIsSource);
            Assert.AreEqual(server.Environment, publishedEnvironmentModel);
        }


        [TestMethod]
        public void HandleAddServerToDeployMessageWithDestinationContextExpectSelectedAsDestination()
        {
            ServerDTO server;
            DeployViewModel vm;
            IEnvironmentModel publishedEnvironmentModel = null;
            bool publishedIsSource = true;
            var mockEventAggregator = new Mock<IEventAggregator>();
            mockEventAggregator.Setup(aggregator => aggregator.Publish(It.IsAny<UpdateSelectedServer>())).Callback<Object>(updateSelectedServer =>
            {
                publishedEnvironmentModel = ((UpdateSelectedServer)updateSelectedServer).EnvironmentModel;
                publishedIsSource = ((UpdateSelectedServer)updateSelectedServer).IsSourceServer;
            });
            var envID = SetupVMForMessages(out server, out vm,mockEventAggregator);

            var destCtx = vm.DestinationContext;

            var msg = new AddServerToDeployMessage(server, destCtx);
            vm.Handle(msg);
            Assert.IsTrue(vm.SelectedDestinationServer.ID.Equals(envID.ToString()));
            Assert.IsFalse(publishedIsSource);
            Assert.AreEqual(server.Environment,publishedEnvironmentModel);
        }

        [TestMethod]
        public void HandleAddServerToDeployMessageWithIsSourceTrueExpectSelectedAsSource()
        {
            ServerDTO server;
            DeployViewModel vm;
            var envID = SetupVMForMessages(out server, out vm);

            var msg = new AddServerToDeployMessage(server, true, false);
            vm.Handle(msg);
            Assert.IsTrue(vm.SelectedSourceServer.ID.Equals(envID.ToString()));
        }

        [TestMethod]
        public void HandleAddServerToDeployMessageWithIsDestinationTrueExpectSelectedAsDestination()
        {
            ServerDTO server;
            DeployViewModel vm;
            var envID = SetupVMForMessages(out server, out vm);

            var msg = new AddServerToDeployMessage(server, false, true);
            vm.Handle(msg);
            Assert.IsTrue(vm.SelectedDestinationServer.ID.Equals(envID.ToString()));
        }

        [TestMethod]
        public void IsInstanceOfIHandleEnvironmentDeleted()
        {
            ServerDTO server;
            DeployViewModel vm;
            SetupVMForMessages(out server, out vm);
            Assert.IsInstanceOfType(vm, typeof(IHandle<EnvironmentDeletedMessage>));
        }

        [TestMethod]
        public void EnvironmentDeletedCallsREmoveEnvironmentFromBothSourceAndDestinationNavigationViewModels()
        {
            //Setup
            ServerDTO server;
            DeployViewModel vm;
            SetupVMForMessages(out server, out vm);
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

        #region CreateEnvironmentRepositoryMock

        private static Guid SetupVMForMessages(out ServerDTO server, out DeployViewModel vm,Mock<IEventAggregator> mockEventAggregator = null)
        {
            ImportService.CurrentContext = _okayContext;
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            var envID = env.Object.ID;
            server = new ServerDTO(env.Object);

            var serverProvider = new Mock<IServerProvider>();
            serverProvider.Setup(s => s.Load()).Returns(new List<IServer> { server });
            var repo = CreateEnvironmentRepositoryMock();
            if(mockEventAggregator == null)
            {
                mockEventAggregator = new Mock<IEventAggregator>();
            }
            vm = new DeployViewModel(serverProvider.Object, repo.Object, mockEventAggregator.Object);
            return envID;
        }

        static Mock<IEnvironmentRepository> CreateEnvironmentRepositoryMock()
        {
            var repo = new Mock<IEnvironmentRepository>();
            repo.Setup(l => l.Load()).Verifiable();

            var model = new Mock<IEnvironmentModel>();
            repo.Setup(l => l.Save(model.Object)).Verifiable();

            IList<IEnvironmentModel> models = new List<IEnvironmentModel>();
            repo.Setup(l => l.All()).Returns(models);


            return repo;
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
            ImportService.AddExportedValueToContainer<IFrameworkSecurityContext>(new MockSecurityProvider(""));

            //New Mocks
            var mockedServerRepo = new Mock<IEnvironmentRepository>();
            var server = new Mock<IEnvironmentModel>();
            var secondServer = new Mock<IEnvironmentModel>();
            var provider = new Mock<IServerProvider>();
            var resourceNode = new Mock<IContextualResourceModel>();

            //Setup Servers
            server.Setup(svr => svr.IsConnected).Returns(true);
            server.Setup(svr => svr.Connection).Returns(DebugOutputViewModelTest.CreateMockConnection(new Random(), new string[0]).Object);
            secondServer.Setup(svr => svr.IsConnected).Returns(true);
            secondServer.Setup(svr => svr.Connection).Returns(DebugOutputViewModelTest.CreateMockConnection(new Random(), new string[0]).Object);
            mockedServerRepo.Setup(svr => svr.Fetch(It.IsAny<IServer>())).Returns(server.Object);
            provider.Setup(prov => prov.Load()).Returns(new List<IServer>() { new ServerDTO(server.Object), new ServerDTO(secondServer.Object) });

            //Setup Navigation Tree
            var eventAggregator = new Mock<IEventAggregator>().Object;
            var mockedSource = new NavigationViewModel(eventAggregator, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, It.IsAny<Guid>(), mockedServerRepo.Object, false, enDsfActivityType.All);
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

            var deployViewModel = new DeployViewModel(provider.Object, mockedServerRepo.Object, new Mock<IEventAggregator>().Object)
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
        // ReSharper disable InconsistentNaming
        public void DeployViewModel_UnitTest_CanDeployToDisconnectedServer_ReturnsFalse()
        // ReSharper restore InconsistentNaming
        {
            Mock<IEnvironmentModel> destEnv;
            Mock<IServer> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);

            deployViewModel.SelectedDestinationServer = destServer.Object;

            destEnv.Setup(e => e.IsConnected).Returns(true);
            Assert.IsTrue(deployViewModel.CanDeploy, "DeployViewModel CanDeploy is false when server is connected.");

            destEnv.Setup(e => e.IsConnected).Returns(false);
            Assert.IsFalse(deployViewModel.CanDeploy, "DeployViewModel CanDeploy is true when server is disconnected.");
        }
        
        [TestMethod]
        [TestCategory("DeployViewModel_CanDeploy")]
        [Owner("Hagashen Naidu")]
        public void DeployViewModel_CanDeployWithSameSourceAndDestinationServer_ReturnsFalse()
        {
            Mock<IEnvironmentModel> destEnv;
            Mock<IServer> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            destServer.Setup(server => server.AppAddress).Returns("http://localhost");
            deployViewModel.SelectedDestinationServer = destServer.Object;
            deployViewModel.SelectedSourceServer = destServer.Object;

            destEnv.Setup(e => e.IsConnected).Returns(true);
            Assert.IsFalse(deployViewModel.CanDeploy);
        }

        static DeployViewModel SetupDeployViewModel(out Mock<IEnvironmentModel> destEnv, out Mock<IServer> destServer)
        {
            ImportService.CurrentContext = _okayContext;

            destEnv = new Mock<IEnvironmentModel>();

            destServer = new Mock<IServer>();
            destServer.Setup(s => s.Environment).Returns(destEnv.Object);

            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(r => r.Fetch(It.IsAny<IServer>())).Returns(destEnv.Object);

            var servers = new List<IServer> { destServer.Object };
            var serverProvider = new Mock<IServerProvider>();
            serverProvider.Setup(s => s.Load()).Returns(servers);

            var deployItemCount = 1;
            var statsCalc = new Mock<IDeployStatsCalculator>();
            statsCalc.Setup(c => c.CalculateStats(It.IsAny<IEnumerable<ITreeNode>>(), It.IsAny<Dictionary<string, Func<ITreeNode, bool>>>(), It.IsAny<ObservableCollection<DeployStatsTO>>(), out deployItemCount));

            var deployViewModel = new DeployViewModel(serverProvider.Object, envRepo.Object, new Mock<IEventAggregator>().Object, statsCalc.Object);
            return deployViewModel;
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeployViewModel_ServersAreNotTheSame")]
        public void DeployViewModel_ServersAreNotTheSame_SourceServerIsNull_True()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> destEnv;
            Mock<IServer> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            deployViewModel.SelectedSourceServer = null;
            deployViewModel.SelectedDestinationServer = new Mock<IServer>().Object;
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
            Mock<IServer> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            deployViewModel.SelectedSourceServer = new Mock<IServer>().Object;
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
            Mock<IServer> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            var mockSourceServer = new Mock<IServer>();
            mockSourceServer.Setup(server => server.AppAddress).Returns("http://localhost");
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            deployViewModel.SelectedDestinationServer = new Mock<IServer>().Object;
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
            Mock<IServer> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            var mockSourceServer = new Mock<IServer>();
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            var mockDestinationServer = new Mock<IServer>();
            mockDestinationServer.Setup(server => server.AppAddress).Returns("http://remote");
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
            Mock<IServer> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            var mockSourceServer = new Mock<IServer>();
            mockSourceServer.Setup(server => server.AppAddress).Returns("http://localhost");
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            var mockDestinationServer = new Mock<IServer>();
            mockDestinationServer.Setup(server => server.AppAddress).Returns("http://remote");
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
            Mock<IServer> destServer;
            var deployViewModel = SetupDeployViewModel(out destEnv, out destServer);
            var mockSourceServer = new Mock<IServer>();
            mockSourceServer.Setup(server => server.AppAddress).Returns("http://localhost");
            deployViewModel.SelectedSourceServer = mockSourceServer.Object;
            var mockDestinationServer = new Mock<IServer>();
            mockDestinationServer.Setup(server => server.AppAddress).Returns("http://localhost");
            deployViewModel.SelectedDestinationServer = mockDestinationServer.Object;
            //------------Execute Test---------------------------
            var serversAreNotTheSame = deployViewModel.ServersAreNotTheSame;
            //------------Assert Results-------------------------
            Assert.IsFalse(serversAreNotTheSame);
        }

    }
}
