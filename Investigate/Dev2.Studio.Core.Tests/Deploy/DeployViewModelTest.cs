using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.Deploy;
using Dev2.Studio.ViewModels.Deploy;
using Dev2.Studio.ViewModels.Explorer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class DeployViewModelTest
    {
        #region Class Members

        private static ImportServiceContext _okayContext;
        private static ImportServiceContext _cancelContext;
        private static Mock<IEventAggregator> _eventAggregator;
        private static Mock<IWindowManager> _windowManager;

        #endregion Class Members

        #region Initialization

        [ClassInitialize()]
        public static void MyTestClassInitialize(TestContext testContext)
        {
            _eventAggregator = new Mock<IEventAggregator>();
            _windowManager = new Mock<IWindowManager>();
            _okayContext = CompositionInitializer.DeployViewModelOkayTest(_eventAggregator, _windowManager);
            _cancelContext = CompositionInitializer.DeployViewModelCancelTest();
        }

        #endregion Initialization

        #region Connect

        [TestMethod]
        public void DeployViewModelConnectWindowManagerShoConnectDialogAdded()
        {
            ImportService.CurrentContext = _okayContext;

            _windowManager.Setup(wm => wm.ShowDialog(It.IsAny<ConnectViewModel>(), null, null)).Verifiable();
            var servers = new List<IServer> { null, null };

            var serverProvider = new Mock<IServerProvider>();
            serverProvider.Setup(s => s.Load()).Returns(servers);

            var repo = CreateEnvironmentRepositoryMock();   

            var deployViewModel = new DeployViewModel(serverProvider.Object, repo.Object);
            deployViewModel.ConnectCommand.Execute(null);

            _windowManager.Verify(e => e.ShowDialog(It.IsAny<ConnectViewModel>(), null, null), Times.Once());
        }

        [TestMethod]
        public void DeployViewModelConnectWithCancelDialogResultExpectedServerNotAdded()
        {
            ImportService.CurrentContext = _cancelContext;

            var servers = new List<IServer> { null, null };

            var serverProvider = new Mock<IServerProvider>();
            serverProvider.Setup(s => s.Load()).Returns(servers);

            var repo = CreateEnvironmentRepositoryMock();

            var deployViewModel = new DeployViewModel(serverProvider.Object, repo.Object);
            deployViewModel.ConnectCommand.Execute(null);

            var actual = deployViewModel.Servers.Count;

            Assert.AreEqual(servers.Count, actual);
        }

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

            var deployViewModel = new DeployViewModel(serverProvider.Object, repo);

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

            var resourceRepo1 = new ResourceRepository(e1.Object);
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

            var resourceRepo2 = new ResourceRepository(e2.Object);
            e2.Setup(e => e.ResourceRepository).Returns(resourceRepo2);

            var serverProvider = new Mock<IServerProvider>();
            serverProvider.Setup(s => s.Load()).Returns(new List<IServer> { s1, s2 });

            var repo = new TestEnvironmentRespository(source.Object, e1.Object, e2.Object);

            var statsCalc = new Mock<IDeployStatsCalculator>();
            statsCalc.Setup(s => s.SelectForDeployPredicate(It.IsAny<ITreeNode>())).Returns(true);

            var deployViewModel = new DeployViewModel(serverProvider.Object, repo, statsCalc.Object);

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
            var envID = SetupVMForMessages(out server, out vm);

            var sourceCtx = vm.SourceContext;

            var msg = new AddServerToDeployMessage(server, sourceCtx);
            vm.Handle(msg);
            Assert.IsTrue(vm.SelectedSourceServer.ID.Equals(envID.ToString()));
        }


        [TestMethod]
        public void HandleAddServerToDeployMessageWithDestinationContextExpectSelectedAsDestination()
        {
            ServerDTO server;
            DeployViewModel vm;
            var envID = SetupVMForMessages(out server, out vm);

            var destCtx = vm.DestinationContext;

            var msg = new AddServerToDeployMessage(server, destCtx);
            vm.Handle(msg);
            Assert.IsTrue(vm.SelectedDestinationServer.ID.Equals(envID.ToString()));

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

        private static Guid SetupVMForMessages(out ServerDTO server, out DeployViewModel vm)
        {
            ImportService.CurrentContext = _okayContext;
            var env = EnviromentRepositoryTest.CreateMockEnvironment();
            var envID = env.Object.ID;
            server = new ServerDTO(env.Object);

            var serverProvider = new Mock<IServerProvider>();
            serverProvider.Setup(s => s.Load()).Returns(new List<IServer> { server });
            var repo = CreateEnvironmentRepositoryMock();

            vm = new DeployViewModel(serverProvider.Object, repo.Object);
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


    }
}
