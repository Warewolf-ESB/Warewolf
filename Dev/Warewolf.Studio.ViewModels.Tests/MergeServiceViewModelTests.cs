using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.Environments;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class MergeServiceViewModelTests
    {
        private MergeServiceViewModel _target;
        private Mock<IEnvironmentViewModel> _selectedEnvironment;
        private Mock<IShellViewModel> _shellViewModelMock;
        private Mock<IServer> _serverMock;
        private Mock<IEventAggregator> _eventAggregatorMock;
        private Mock<IStudioUpdateManager> _studioUpdateManagerMock;
        private Mock<IExplorerItem> _explorerItemMock;
        private Mock<IMergeView> _mergeView;

        [TestInitialize]
        public void TestInitialize()
        {
            _selectedEnvironment = new Mock<IEnvironmentViewModel>();
            _selectedEnvironment.Setup(p => p.DisplayName).Returns("someResName");
            _shellViewModelMock = new Mock<IShellViewModel>();
            var mockExplorerViewModel = new Mock<IExplorerViewModel>();
            _shellViewModelMock.Setup(model => model.ExplorerViewModel).Returns(mockExplorerViewModel.Object);
            _shellViewModelMock.Setup(model => model.ExplorerViewModel.ConnectControlViewModel).Returns(new Mock<IConnectControlViewModel>().Object);
            _serverMock = new Mock<IServer>();
            _serverMock.Setup(server => server.GetServerVersion()).Returns("1.1.2");
            _studioUpdateManagerMock = new Mock<IStudioUpdateManager>();
            _explorerItemMock = new Mock<IExplorerItem>();
            _explorerItemMock.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItem>());
            _serverMock.Setup(it => it.LoadExplorer(false)).ReturnsAsync(_explorerItemMock.Object);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_studioUpdateManagerMock.Object);
            _serverMock.SetupGet(it => it.DisplayName).Returns("someResName");
            _shellViewModelMock.SetupGet(it => it.LocalhostServer).Returns(_serverMock.Object);
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _mergeView = new Mock<IMergeView>();

            var serverProvider = new Mock<IEnvironmentModelProvider>();
            var environmentModels = new List<IServer>
            {
                new TestServer(new Mock<Caliburn.Micro.IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(true).Object, new Mock<IResourceRepository>().Object, false),
                new TestServer(new Mock<Caliburn.Micro.IEventAggregator>().Object, Guid.NewGuid(), CreateConnection(false).Object, new Mock<IResourceRepository>().Object, false)
            };
            var environmentRepository = new Mock<IServerRepository>();
            environmentRepository.Setup(e => e.All()).Returns(environmentModels);
            serverProvider.Setup(s => s.Load()).Returns(environmentModels);
            serverProvider.Setup(s => s.ReloadServers()).Returns(environmentModels);
            IConnectControlSingleton connectControlSingleton = new ConnectControlSingleton(serverProvider.Object, environmentRepository.Object);
            //------------Execute Test---------------------------
            CustomContainer.Register(environmentRepository.Object);
            CustomContainer.Register(connectControlSingleton);

            var resourceId = Guid.NewGuid();

            var explorerItemViewModelMock = new Mock<IExplorerItemViewModel>();
            explorerItemViewModelMock.SetupGet(it => it.IsVisible).Returns(true);
            explorerItemViewModelMock.SetupGet(it => it.ResourceType).Returns("WorkflowService");
            explorerItemViewModelMock.SetupGet(it => it.ResourceName).Returns("Selected Service");
            explorerItemViewModelMock.SetupGet(it => it.ResourceId).Returns(resourceId);


            _target = new MergeServiceViewModel(_shellViewModelMock.Object, _eventAggregatorMock.Object, explorerItemViewModelMock.Object, _mergeView.Object, _selectedEnvironment.Object);
        }

        static Mock<IEnvironmentConnection> CreateConnection(bool isConnected)
        {
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.ServerEvents).Returns(new Mock<IEventPublisher>().Object);
            conn.Setup(connection => connection.WebServerUri).Returns(new Uri("http://localhost:3142"));
            conn.Setup(connection => connection.AppServerUri).Returns(new Uri("http://localhost:3142/dsf"));
            conn.Setup(c => c.IsConnected).Returns(isConnected);
            conn.Setup(connection => connection.DisplayName).Returns("localhost");
            return conn;
        }

        [TestMethod]
        public void TestEnvironments()
        {
            //arrange
            //act
            var env = _target.Environments;

            //assert
            Assert.IsNotNull(env);
        }

        [Ignore("Implement: CanExecute, CanEdit, CanView, ShowContextMenu, AllowResourceCheck, CanDrop, CanDrag")]
        [TestMethod]
        public void TestConstructorExpectedProperties()
        {
            Assert.IsNotNull(_target.MergeConnectControlViewModel);
            Assert.IsFalse(_target.MergeConnectControlViewModel.CanEditServer);
            Assert.IsFalse(_target.MergeConnectControlViewModel.CanCreateServer);
        }

        [TestMethod]
        public void TestSelectedEnvironmentChanged()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            var serverMock = new Mock<IServer>();
            var serverId = Guid.NewGuid();
            serverMock.SetupGet(it => it.EnvironmentID).Returns(serverId);
            serverMock.SetupGet(it => it.DisplayName).Returns("newServerName");
            environmentViewModelMock.SetupGet(it => it.IsVisible).Returns(true);
            environmentViewModelMock.SetupGet(it => it.Server).Returns(serverMock.Object);
            var env = _target.Environments.First();
            var explorerItemViewModelMock = new Mock<IExplorerItemViewModel>();
            explorerItemViewModelMock.SetupGet(it => it.IsVisible).Returns(true);
            explorerItemViewModelMock.SetupGet(it => it.ResourceType).Returns("Dev2Server");
            explorerItemViewModelMock.SetupGet(it => it.ResourceName).Returns("newServerName");
            explorerItemViewModelMock.SetupGet(it => it.ResourceId).Returns(serverId);
            explorerItemViewModelMock.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItemViewModel>());
            env.AddChild(explorerItemViewModelMock.Object);
            env.ResourceId = serverId;

            var explorerItemViewModels = new ObservableCollection<IExplorerItemViewModel> { explorerItemViewModelMock.Object };
            environmentViewModelMock.Setup(e => e.Children).Returns(explorerItemViewModels);
            environmentViewModelMock.Setup(e => e.AsList()).Returns(explorerItemViewModels);

            var environmentViewModels = _target.Environments.Union(new[] { environmentViewModelMock.Object }).ToList();
            _target.Environments = new ObservableCollection<IEnvironmentViewModel>(environmentViewModels);

            _shellViewModelMock.Setup(model => model.ExplorerViewModel.Environments).Returns(_target.Environments);

            //act
            _target.MergeConnectControlViewModel.SelectedConnection = serverMock.Object;

            //assert

            explorerItemViewModelMock.VerifySet(it => it.CanExecute = false);
            explorerItemViewModelMock.VerifySet(it => it.CanEdit = false);
            explorerItemViewModelMock.VerifySet(it => it.CanView = false);
            explorerItemViewModelMock.VerifySet(it => it.ShowContextMenu = true);
            explorerItemViewModelMock.VerifySet(it => it.AllowResourceCheck = true);
            explorerItemViewModelMock.VerifySet(it => it.CanDrop = false);
            explorerItemViewModelMock.VerifySet(it => it.CanDrag = false);
        }

        [TestMethod]
        public async Task TestOtherServerСonnect()
        {
            //arrange
            var isEnvironmentChanged = false;
            _target.PropertyChanged += (s, e) =>
            {
                isEnvironmentChanged = isEnvironmentChanged || e.PropertyName == "Environments";
            };
            var serverMock = new Mock<IServer>();
            var envId = Guid.NewGuid();
            serverMock.SetupGet(it => it.EnvironmentID).Returns(envId);
            serverMock.Setup(it => it.ConnectAsync()).ReturnsAsync(true);

            //act
            await _target.MergeConnectControlViewModel.Connect(serverMock.Object);

            //assert
            Assert.IsTrue(isEnvironmentChanged);
            Assert.AreEqual(1, _target.Environments.Count);
        }

    }
}
