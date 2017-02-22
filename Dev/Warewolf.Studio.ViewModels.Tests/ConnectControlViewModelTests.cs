using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Core.Tests.Environments;
using Dev2.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.AntiCorruptionLayer;
using Warewolf.Testing;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ConnectControlViewModelTests
    {
        #region Fields

        private Mock<IServer> _serverMock;

        private Mock<IEventAggregator> _eventAggregatorMock;

        private Mock<IStudioUpdateManager> _updateRepositoryMock;

        private List<string> _changedProperties;

        private Guid _serverEnvironmentId;

        private ConnectControlViewModel _target;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _serverMock = new Mock<IServer>();
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _changedProperties = new List<string>();
            _target = new ConnectControlViewModel(_serverMock.Object, _eventAggregatorMock.Object) { ShouldUpdateActiveEnvironment = true };
            _target.ShouldUpdateActiveEnvironment = true;
            _target.PropertyChanged += _target_PropertyChanged;
        }

        #endregion Test initialize

        #region Test construction

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConnectControlViewModelServerNull()
        {
            //act
            var connectControlViewModel = new ConnectControlViewModel(null, _eventAggregatorMock.Object);
            Assert.IsNotNull(connectControlViewModel);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConnectControlViewModelEventAggregatorNull()
        {
            //act
            var connectControlViewModel = new ConnectControlViewModel(_serverMock.Object, null);
            Assert.IsNotNull(connectControlViewModel);
        }

        #endregion Test construction

        #region Test commands

        [TestMethod]
        public void TestEditConnectionCommandCantbeExecuted()
        {
            //arrange
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.Empty);

            //act
            Assert.IsFalse(_target.EditConnectionCommand.CanExecute(null));
        }

        [TestMethod]
        public void TestEditConnectionCommandSelectedConnectionNull()
        {
            //arrange
            _target.SelectedConnection = null;

            //act
            _target.EditConnectionCommand.Execute(null);
        }

        [TestMethod]
        public void TestEditConnectionCommand()
        {
            //arrange
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());
            _serverMock.SetupGet(it => it.AllowEdit).Returns(true);

            //act
            Assert.IsTrue(_target.EditConnectionCommand.CanExecute(null));
            _target.EditConnectionCommand.Execute(null);
        }

        [TestMethod]
        public void TestToggleConnectionStateCommand()
        {
            //arrange
            _serverMock.Setup(it => it.GetServerVersion()).Returns("0.0.0.1");
            _serverMock.SetupGet(it => it.IsConnected).Returns(true);
            _serverMock.SetupGet(it => it.HasLoaded).Returns(true);
            var popupControllerMock = new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>();
            CustomContainer.Register(popupControllerMock.Object);
            bool disconnectedEventRaised = false;
            _target.ServerDisconnected =
                (s, e) => { disconnectedEventRaised = s == _target && e == _serverMock.Object; };

            //act
            var canExecute = _target.ToggleConnectionStateCommand.CanExecute(null);
            _target.ToggleConnectionStateCommand.Execute(null);

            //assert
            Assert.IsTrue(canExecute);
            Assert.IsTrue(disconnectedEventRaised);
            Assert.IsFalse(_target.IsConnecting);
        }

        [TestMethod]
        public void TestToggleConnectionStateCommandException()
        {
            //arrange
            _serverMock.Setup(it => it.GetServerVersion()).Throws(new Exception());

            //act
            var canExecute = _target.ToggleConnectionStateCommand.CanExecute(null);
            _target.ToggleConnectionStateCommand.Execute(null);

            //assert
            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        public void TestToggleConnectionStateCommandIsNotConnected()
        {
            //arrange
            _serverMock.Setup(it => it.GetServerVersion()).Returns("0.0.0.1");
            var envId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(envId);
            _serverMock.SetupGet(it => it.IsConnected).Returns(false);
            _serverMock.SetupGet(it => it.HasLoaded).Returns(false);
            _serverMock.Setup(it => it.ConnectAsync()).ReturnsAsync(true);
            var shellViewModelMock = new Mock<IShellViewModel>();
            CustomContainer.Register(shellViewModelMock.Object);
            var popupControllerMock = new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>();
            CustomContainer.Register(popupControllerMock.Object);
            //act
            var canExecute = _target.ToggleConnectionStateCommand.CanExecute(null);
            _target.ToggleConnectionStateCommand.Execute(null);

            //assert
            Assert.IsTrue(canExecute);
            popupControllerMock.Verify(it => it.ShowConnectServerVersionConflict("0.0.0.1", "0.0.0.6"), Times.Never());
            _serverMock.Verify(it => it.ConnectAsync());
            shellViewModelMock.Verify(it => it.SetActiveEnvironment(envId));
        }

        #endregion Test commands

        #region Test properties

        [TestMethod]
        public void TestSelectedConnectionNewServerLabel()
        {
            //arrange
            var mainViewModelMock = new Mock<IShellViewModel>();
            CustomContainer.Register(mainViewModelMock.Object);
            var newSelectedConnection = new Mock<IServer>();
            var newSelectedConnectionEnvironmentId = Guid.NewGuid();
            newSelectedConnection.SetupGet(it => it.ResourceName).Returns("New Remote Server...");
            newSelectedConnection.SetupGet(it => it.EnvironmentID).Returns(newSelectedConnectionEnvironmentId);
            _changedProperties.Clear();
            var isSelectedEnvironmentChangedRaised = false;
            _target.SelectedEnvironmentChanged += (sender, args) =>
                {
                    isSelectedEnvironmentChangedRaised = sender == _target && args == _serverEnvironmentId;
                };
            var isCanExecuteChangedRaised = false;
            _target.EditConnectionCommand.CanExecuteChanged += (sender, args) =>
                {
                    isCanExecuteChangedRaised = true;
                };

            //act
            _target.SelectedConnection = newSelectedConnection.Object;
            var value = _target.SelectedConnection;

            //assert
            Assert.AreNotSame(value, newSelectedConnection.Object);
            mainViewModelMock.Verify(it => it.SetActiveEnvironment(_serverEnvironmentId));
            mainViewModelMock.Verify(it => it.NewServerSource(It.IsAny<string>()));
            Assert.IsFalse(_target.IsConnected);
            Assert.IsFalse(_target.AllowConnection);
            Assert.IsTrue(_changedProperties.Contains("SelectedConnection"));
            Assert.IsFalse(isSelectedEnvironmentChangedRaised);
            Assert.IsTrue(isCanExecuteChangedRaised);
        }

        [TestMethod]
        public void TestSelectedConnectionNonLocalhostLabel()
        {
            //arrange
            var mainViewModelMock = new Mock<IShellViewModel>();
            CustomContainer.Register(mainViewModelMock.Object);
            var newSelectedConnection = new Mock<IServer>();
            var newSelectedConnectionEnvironmentId = Guid.NewGuid();
            newSelectedConnection.SetupGet(it => it.ResourceName).Returns("Nonlocalhost");
            newSelectedConnection.SetupGet(it => it.EnvironmentID).Returns(newSelectedConnectionEnvironmentId);
            newSelectedConnection.SetupGet(it => it.HasLoaded).Returns(true);
            newSelectedConnection.SetupGet(it => it.IsConnected).Returns(true);
            _changedProperties.Clear();
            var isSelectedEnvironmentChangedRaised = false;
            _target.SelectedEnvironmentChanged += (sender, args) =>
                {
                    isSelectedEnvironmentChangedRaised = sender == _target && args == _serverEnvironmentId;
                };
            var isCanExecuteChangedRaised = false;
            _target.EditConnectionCommand.CanExecuteChanged += (sender, args) =>
                {
                    isCanExecuteChangedRaised = true;
                };
            //act
            _target.SelectedConnection = newSelectedConnection.Object;
            var value = _target.SelectedConnection;

            //assert
            Assert.AreSame(value, newSelectedConnection.Object);
            mainViewModelMock.Verify(it => it.SetActiveEnvironment(newSelectedConnectionEnvironmentId));
            mainViewModelMock.Verify(it => it.SetActiveServer(newSelectedConnection.Object));
            Assert.IsTrue(_target.IsConnected);
            Assert.IsTrue(_target.AllowConnection);
            Assert.IsTrue(_changedProperties.Contains("SelectedConnection"));
            Assert.IsFalse(isSelectedEnvironmentChangedRaised);
            Assert.IsTrue(isCanExecuteChangedRaised);
        }

        [TestMethod]
        public void TestSelectedConnectionLocalhostLabel()
        {
            //arrange
            var mainViewModelMock = new Mock<IShellViewModel>();
            CustomContainer.Register(mainViewModelMock.Object);
            var newSelectedConnection = new Mock<IServer>();
            var newSelectedConnectionEnvironmentId = Guid.NewGuid();
            newSelectedConnection.SetupGet(it => it.ResourceName).Returns("localhost");
            newSelectedConnection.SetupGet(it => it.EnvironmentID).Returns(newSelectedConnectionEnvironmentId);
            newSelectedConnection.SetupGet(it => it.HasLoaded).Returns(true);
            newSelectedConnection.SetupGet(it => it.IsConnected).Returns(true);
            _changedProperties.Clear();
            var isSelectedEnvironmentChangedRaised = false;
            _target.SelectedEnvironmentChanged += (sender, args) =>
                {
                    isSelectedEnvironmentChangedRaised = sender == _target && args == _serverEnvironmentId;
                };
            var isCanExecuteChangedRaised = false;
            _target.EditConnectionCommand.CanExecuteChanged += (sender, args) =>
                {
                    isCanExecuteChangedRaised = true;
                };

            //act
            _target.SelectedConnection = newSelectedConnection.Object;

            //assert
            mainViewModelMock.Verify(it => it.SetActiveEnvironment(newSelectedConnectionEnvironmentId));
            mainViewModelMock.Verify(it => it.SetActiveServer(newSelectedConnection.Object));
            Assert.IsTrue(_target.IsConnected);
            Assert.IsTrue(_changedProperties.Contains("SelectedConnection"));
            Assert.IsFalse(isSelectedEnvironmentChangedRaised);
            Assert.IsTrue(isCanExecuteChangedRaised);
        }

        [TestMethod]
        public void TestIsLoading()
        {
            //arrange
            _changedProperties.Clear();
            const bool ExpectedValue = true;

            //act
            _target.IsLoading = ExpectedValue;
            var value = _target.IsLoading;

            //assert
            Assert.AreEqual(ExpectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("IsLoading"));
        }

        [TestMethod]
        public void TestToggleConnectionToolTip()
        {
            //assert
            Assert.IsTrue(!string.IsNullOrEmpty(_target.ToggleConnectionToolTip));

        }

        [TestMethod]
        public void TestEditConnectionToolTip()
        {
            //assert
            Assert.IsTrue(!string.IsNullOrEmpty(_target.EditConnectionToolTip));
        }

        [TestMethod]
        public void TestConnectionsToolTip()
        {
            //assert
            Assert.IsTrue(!string.IsNullOrEmpty(_target.ConnectionsToolTip));
        }

        #endregion Test properties

        #region Test methods

        [TestMethod]
        public void TestUpdateHelpDescriptor()
        {
            //arrange
            var mainViewModelMock = new Mock<IMainViewModel>();
            var helpViewModelMock = new Mock<IHelpWindowViewModel>();
            mainViewModelMock.SetupGet(it => it.HelpViewModel).Returns(helpViewModelMock.Object);
            CustomContainer.Register(mainViewModelMock.Object);
            const string HelpText = "someText";

            //act
            _target.UpdateHelpDescriptor(HelpText);

            //assert
            helpViewModelMock.Verify(it => it.UpdateHelpText(HelpText));
        }

        [TestMethod]
        public async Task TestConnectNullArgument()
        {
            //act
            var result = await _target.Connect(null);

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TestConnectException()
        {
            //arrange
            var serverMock = new Mock<IServer>();
            serverMock.Setup(it => it.ConnectAsync()).ThrowsAsync(new Exception());

            //act
            var result = await _target.Connect(serverMock.Object);

            //assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TestConnect()
        {
            //arrange
            var serverMock = new Mock<IServer>();
            serverMock.Setup(it => it.ConnectAsync()).ReturnsAsync(true);
            var serverEnvironmentId = Guid.NewGuid();
            serverMock.SetupGet(it => it.EnvironmentID).Returns(serverEnvironmentId);
            _changedProperties.Clear();
            var serverConnectedRaised = false;
            _target.ServerConnected = (sender, e) =>
                {
                    serverConnectedRaised = sender == _target && e == serverMock.Object;
                };
            var mainViewModelMock = new Mock<IShellViewModel>();
            CustomContainer.Register(mainViewModelMock.Object);

            //act
            var result = await _target.Connect(serverMock.Object);

            //assert
            Assert.IsTrue(result);
            Assert.IsTrue(serverConnectedRaised);
            Assert.IsTrue(_changedProperties.Contains("IsConnected"));
            serverMock.Verify(it => it.ConnectAsync());
            mainViewModelMock.Verify(it => it.SetActiveServer(serverMock.Object));
        }

        [TestMethod]
        public async Task TestConnectUnsuccessful()
        {
            //arrange
            var serverMock = new Mock<IServer>();
            serverMock.Setup(it => it.ConnectAsync()).ReturnsAsync(false);
            var serverEnvironmentId = Guid.NewGuid();
            serverMock.SetupGet(it => it.EnvironmentID).Returns(serverEnvironmentId);
            serverMock.SetupGet(it => it.DisplayName).Returns("DisplayName");
            _changedProperties.Clear();
            var serverConnectedRaised = false;
            _target.ServerConnected = (sender, e) =>
                {
                    serverConnectedRaised = sender == _target && e == serverMock.Object;
                };
            var mainViewModelMock = new Mock<IShellViewModel>();
            CustomContainer.Register(mainViewModelMock.Object);
            var popupControllerMock = new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>();
            popupControllerMock.Setup(it => it.ShowConnectionTimeoutConfirmation("DisplayName"))
                .Returns<string>(
                    dispName =>
                        {
                            serverMock.Setup(it => it.ConnectAsync()).ReturnsAsync(true);
                            return MessageBoxResult.Yes;
                        });
            CustomContainer.Register(popupControllerMock.Object);

            //act
            var result = await _target.Connect(serverMock.Object);

            //assert
            Assert.IsTrue(result);
            Assert.IsTrue(serverConnectedRaised);
            Assert.IsTrue(_changedProperties.Contains("IsConnected"));
            serverMock.Verify(it => it.ConnectAsync());
            popupControllerMock.Verify(it => it.ShowConnectionTimeoutConfirmation("DisplayName"));
            mainViewModelMock.Verify(it => it.SetActiveServer(serverMock.Object));
        }


        [TestMethod]
        public void TestLoadServers()
        {
            //arrange
            var serverConnectionMock = new Mock<IServer>();
            var serverConnectionEnvironmentId = Guid.NewGuid();
            serverConnectionMock.SetupGet(it => it.EnvironmentID).Returns(serverConnectionEnvironmentId);
            serverConnectionMock.SetupGet(server => server.ResourceID).Returns(Guid.NewGuid);

            _serverMock.Setup(server => server.FetchServer(serverConnectionEnvironmentId))
                .Returns(serverConnectionMock.Object);
            //act
            _updateRepositoryMock.Object.ServerSaved.Invoke(serverConnectionEnvironmentId);

            //assert
            Assert.IsTrue(_target.Servers.Contains(serverConnectionMock.Object));
        }

        [TestMethod]
        public void TestOnServerOnNetworkStateChanged()
        {
            //arrange
            var serverConnectionMock = new Mock<IServer>();
            var serverConnectionEnvironmentId = Guid.NewGuid();
            var serverArg = new Mock<IServer>();
            var serverDisconnectedRaised = false;
            serverArg.SetupGet(it => it.IsConnected).Returns(false);
            serverArg.SetupGet(it => it.EnvironmentID).Returns(serverConnectionEnvironmentId);
            serverConnectionMock.SetupGet(it => it.EnvironmentID).Returns(serverConnectionEnvironmentId);
            serverConnectionMock.SetupGet(server => server.ResourceID).Returns(Guid.NewGuid);
            serverConnectionMock.SetupGet(it => it.ResourceName).Returns("someName");
            serverConnectionMock.SetupGet(it => it.DisplayName).Returns("My display name(Connected)");
            _target.ServerHasDisconnected = (obj, arg) => { serverDisconnectedRaised = obj == _target && arg == serverArg.Object; };
            _serverMock.Setup(it => it.GetServerConnections()).Returns(new List<IServer>() { serverConnectionMock.Object });
            _updateRepositoryMock.SetupGet(manager => manager.ServerSaved).Returns(new Mock<Action<Guid>>().Object);
            _updateRepositoryMock.Object.ServerSaved.Invoke(serverConnectionMock.Object.ResourceID);
            var argsMock = new Mock<INetworkStateChangedEventArgs>();
            argsMock.SetupGet(it => it.State).Returns(ConnectionNetworkState.Disconnected);
            _target.SelectedConnection = serverConnectionMock.Object;
            _changedProperties.Clear();

            //act
            serverConnectionMock.Raise(it => it.NetworkStateChanged += null, argsMock.Object, serverArg.Object);

            //assert
            Assert.IsFalse(_target.Servers.Contains(serverConnectionMock.Object));
            Assert.IsFalse(_target.IsConnected);
            Assert.IsFalse(serverDisconnectedRaised);
            serverConnectionMock.VerifySet(it => it.DisplayName = "My display name", Times.Never);
        }

        [TestMethod]
        public void TestOnServerOnNetworkStateChangedConnected()
        {
            //arrange
            var serverConnectionMock = new Mock<IServer>();
            var serverArg = new Mock<IServer>();
            var serverReconnectedRaised = false;
            serverConnectionMock.SetupGet(it => it.ResourceName).Returns("someName");
            serverConnectionMock.SetupGet(server => server.ResourceID).Returns(Guid.NewGuid);
            _target.ServerReConnected = (obj, arg) => { serverReconnectedRaised = obj == _target && arg == serverArg.Object; };
            _serverMock.Setup(it => it.GetServerConnections()).Returns(new List<IServer>() { serverConnectionMock.Object });
            _updateRepositoryMock.SetupGet(manager => manager.ServerSaved).Returns(new Mock<Action<Guid>>().Object);
            _updateRepositoryMock.Object.ServerSaved.Invoke(serverConnectionMock.Object.ResourceID);
            var argsMock = new Mock<INetworkStateChangedEventArgs>();
            argsMock.SetupGet(it => it.State).Returns(ConnectionNetworkState.Connected);
            _target.SelectedConnection = serverConnectionMock.Object;
            _changedProperties.Clear();

            //act
            serverConnectionMock.Raise(it => it.NetworkStateChanged += null, argsMock.Object, serverArg.Object);

            //assert
            Assert.IsFalse(serverReconnectedRaised);
        }

        [TestMethod]
        public void TestLoadAllNewServers()
        {
            //arrange
            var serverConnectionMock = new Mock<IServer>();
            var serverConnectionEnvironmentId = Guid.NewGuid();
            serverConnectionMock.SetupGet(it => it.EnvironmentID).Returns(serverConnectionEnvironmentId);
            serverConnectionMock.SetupGet(server => server.ResourceID).Returns(Guid.NewGuid);
            _serverMock.Setup(server => server.FetchServer(serverConnectionEnvironmentId))
                .Returns(serverConnectionMock.Object);
            _updateRepositoryMock.Object.ServerSaved.Invoke(serverConnectionEnvironmentId);
            var server1Mock = new Mock<IServer>();
            server1Mock.SetupGet(it => it.EnvironmentID).Returns(serverConnectionEnvironmentId);
            server1Mock.SetupGet(it => it.DisplayName).Returns("server1MockDisplayName");
            var newEnvironmentID = Guid.NewGuid();
            var server2Mock = new Mock<IServer>();
            server2Mock.SetupGet(it => it.EnvironmentID).Returns(newEnvironmentID);

            _serverMock.Setup(it => it.GetAllServerConnections()).Returns(new List<IServer> { server1Mock.Object, server2Mock.Object });

            //act
            _target.LoadServers();

            //assert
            Assert.IsTrue(_target.Servers.Contains(server1Mock.Object));
            Assert.IsTrue(_target.Servers.Contains(server2Mock.Object));
        }

        [TestMethod]
        public void TestEditSelectedConnectionShouldSetSelectedConnection()
        {
            //arrange
            var serverConnectionMock = new Mock<IServer>();
            var serverConnectionEnvironmentId = Guid.NewGuid();
            serverConnectionMock.SetupGet(it => it.EnvironmentID).Returns(serverConnectionEnvironmentId);
            serverConnectionMock.SetupGet(server => server.ResourceID).Returns(Guid.NewGuid);
            _serverMock.Setup(it => it.GetAllServerConnections()).Returns(new List<IServer> { serverConnectionMock.Object });
            _updateRepositoryMock.SetupGet(manager => manager.ServerSaved).Returns(new Mock<Action<Guid>>().Object);
            _updateRepositoryMock.Object.ServerSaved.Invoke(serverConnectionMock.Object.ResourceID);
            var server1Mock = new Mock<IServer>();
            server1Mock.SetupGet(it => it.EnvironmentID).Returns(serverConnectionEnvironmentId);
            server1Mock.SetupGet(it => it.DisplayName).Returns("server1MockDisplayName");
            var newEnvironmentID = Guid.NewGuid();
            var server2Mock = new Mock<IServer>();
            server2Mock.SetupGet(it => it.EnvironmentID).Returns(newEnvironmentID);
            server1Mock.SetupGet(it => it.DisplayName).Returns("server2MockDisplayName");

            _serverMock.Setup(it => it.GetServerConnections())
                .Returns(new List<IServer> { server1Mock.Object, server2Mock.Object });
            _target.SelectedConnection = server2Mock.Object;
            //act
            _target.EditConnectionCommand.Execute(null);

            //assert
            Assert.IsNotNull(_target.SelectedConnection);
        }

        [TestMethod]
        public void TestCreateNewRemoteServerEnvironment()
        {
            //arrange
            var serverConnectionMock = new Mock<IServer>();
            var serverConnectionEnvironmentId = Guid.NewGuid();
            serverConnectionMock.SetupGet(it => it.EnvironmentID).Returns(serverConnectionEnvironmentId);
            serverConnectionMock.SetupGet(server1 => server1.ResourceID).Returns(Guid.NewGuid);
            _serverMock.Setup(it => it.GetServerConnections()).Returns(new List<IServer>() { serverConnectionMock.Object });

            //act
            _updateRepositoryMock.SetupGet(manager => manager.ServerSaved).Returns(new Mock<Action<Guid>>().Object);
            _updateRepositoryMock.Object.ServerSaved.Invoke(serverConnectionMock.Object.ResourceID);

            //assert
            var createdServers = _target.Servers.OfType<Server>().ToList();
            Assert.IsTrue(createdServers.Any());
            var server = createdServers.First();
            Assert.IsTrue(!string.IsNullOrEmpty(server.ResourceName));
            Assert.AreNotEqual(Guid.Empty, server.EnvironmentID);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("ConnectControlViewModel_EditServer")]
        public void ConnectControlViewModelEditServerServerIDMatchIsTrue()
        {
            var serverGuid = Guid.NewGuid();
            Uri uri = new Uri("http://bravo.com/");

            var mockShellViewModel = new Mock<IShellViewModel>();
            var mockExplorerRepository = new Mock<IExplorerRepository>();
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();

            mockEnvironmentConnection.Setup(a => a.AppServerUri).Returns(uri);
            mockEnvironmentConnection.Setup(a => a.UserName).Returns("johnny");
            mockEnvironmentConnection.Setup(a => a.Password).Returns("bravo");
            mockEnvironmentConnection.Setup(a => a.WebServerUri).Returns(uri);
            mockEnvironmentConnection.Setup(a => a.ID).Returns(serverGuid);

            mockExplorerRepository.Setup(
                repository => repository.CreateFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()));
            mockExplorerRepository.Setup(
                repository => repository.Rename(It.IsAny<IExplorerItemViewModel>(), It.IsAny<string>())).Returns(true);

            var server = new ServerForTesting(mockExplorerRepository);
            var server2 = new Server
            {
                EnvironmentID = serverGuid,
                EnvironmentConnection = mockEnvironmentConnection.Object
            };
            server.EnvironmentID = serverGuid;
            server.ResourceName = "mr_J_bravo";
            mockShellViewModel.Setup(a => a.ActiveServer).Returns(server);
            mockShellViewModel.Setup(model => model.LocalhostServer).Returns(server);

            CustomContainer.Register<IServer>(server);
            CustomContainer.Register(mockShellViewModel.Object);

            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.SetupGet(a => a.Connection).Returns(mockEnvironmentConnection.Object);
            environmentModel.SetupGet(a => a.IsConnected).Returns(true);

            var e1 = new EnvironmentModel(serverGuid, mockEnvironmentConnection.Object);
            var repo = new TestEnvironmentRespository(environmentModel.Object, e1) { ActiveEnvironment = e1 };
            var environmentRepository = new EnvironmentRepository(repo);
            Assert.IsNotNull(environmentRepository);

            bool passed = false;
            mockShellViewModel.Setup(a => a.OpenResource(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IServer>()))
                .Callback((Guid id1,Guid id2,  IServer a) =>
                {
                    passed = a.EnvironmentID == serverGuid;
                });
            //------------Setup for test--------------------------
            var connectControlViewModel = new ConnectControlViewModel(server, new EventAggregator());
            PrivateObject p = new PrivateObject(connectControlViewModel);
            p.SetField("_selectedConnection", server2);
            //------------Execute Test---------------------------
            connectControlViewModel.Edit();

            //------------Assert Results-------------------------
            Assert.IsTrue(passed);
        }

        #endregion Test methods

        #region Private helper methods

        private void _target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _changedProperties.Add(e.PropertyName);
        }

        #endregion Private helper methods
    }
}
