using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Studio.Interfaces;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class ExplorerViewModelTests
    {
        ExplorerViewModel _target;

        Guid _localhostServerEnvironmentId;
        Mock<IShellViewModel> _shellViewModelMock;
        Mock<IServer> _localhostServerMock;
        Mock<IWindowsGroupPermission> _windowsGroupPermissionMock;
        Mock<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator> _eventAggregatorMock;

        [TestInitialize]
        public void TestInitialize()
        { 
            _shellViewModelMock = new Mock<IShellViewModel>();
            _localhostServerEnvironmentId = Guid.NewGuid();
            _localhostServerMock = new Mock<IServer>();
            _localhostServerMock.Setup(it => it.EnvironmentID).Returns(_localhostServerEnvironmentId);
            var mockEnvironmentConnection = SetupMockConnection();
            _localhostServerMock.SetupGet(it => it.Connection).Returns(mockEnvironmentConnection.Object);
            _windowsGroupPermissionMock = new Mock<IWindowsGroupPermission>();
            _localhostServerMock.Setup(it => it.Permissions).Returns(new List<IWindowsGroupPermission>()
            {
                _windowsGroupPermissionMock.Object
            });
            _localhostServerMock.SetupGet(it => it.DisplayName).Returns("localhostServerResourceName");
            _shellViewModelMock.SetupGet(it => it.LocalhostServer).Returns(_localhostServerMock.Object);
            _eventAggregatorMock = new Mock<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator>();

            var connectControlSingleton = new Mock<Dev2.ConnectionHelpers.IConnectControlSingleton>();
            CustomContainer.Register(connectControlSingleton.Object);
            var environmentRepository = new Mock<IServerRepository>();
            CustomContainer.Register(environmentRepository.Object);
            var explorerTooltips = new Mock<IExplorerTooltips>();
            CustomContainer.Register(new Mock<IExplorerTooltips>().Object);
            _target = new ExplorerViewModel(_shellViewModelMock.Object, _eventAggregatorMock.Object,true);
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
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestRefreshCommand()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            environmentViewModelMock.SetupGet(it => it.IsConnected).Returns(true);
            environmentViewModelMock.SetupGet(it => it.ResourceName).Returns("localhostServerResourceName");

            var environmentId = _target.ConnectControlViewModel.SelectedConnection.EnvironmentID;

            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.DisplayName).Returns("localhostServerResourceName");
            mockServer.Setup(server => server.EnvironmentID).Returns(environmentId);
            environmentViewModelMock.SetupGet(it => it.Server).Returns(mockServer.Object);

            var child = new Mock<IExplorerItemViewModel>();
            child.SetupGet(it => it.AllowResourceCheck).Returns(true);
            child.SetupGet(it => it.IsVisible).Returns(true);
            environmentViewModelMock.Setup(a => a.Children).Returns(new ObservableCollection<IExplorerItemViewModel>() { child.Object });

            _target.Environments.Add(environmentViewModelMock.Object);
            _target.SearchText = "someText";
            _target.Environments.Remove(_target.Environments.First(it => it is EnvironmentViewModel));
            //act
            _target.RefreshCommand.Execute(null);
            Assert.IsTrue(_target.RefreshCommand.CanExecute(null));

            //assert
            environmentViewModelMock.VerifyGet(it => it.IsConnected);
            environmentViewModelMock.Verify( it => it.LoadAsync(It.IsAny<bool>(), It.IsAny<bool>()));
            environmentViewModelMock.Verify(it => it.Filter("someText"));
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_ValidateEnvironmentContainsDoesNotAdd()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            environmentViewModelMock.SetupGet(it => it.IsConnected).Returns(true);
            environmentViewModelMock.SetupGet(it => it.ResourceName).Returns("localhostServerResourceName");

            var environmentId = _target.ConnectControlViewModel.SelectedConnection.EnvironmentID;

            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.DisplayName).Returns("localhostServerResourceName");
            mockServer.Setup(server => server.EnvironmentID).Returns(environmentId);
            environmentViewModelMock.SetupGet(it => it.Server).Returns(mockServer.Object);

            var items = new ObservableCollection<IEnvironmentViewModel>();
            items.Add(environmentViewModelMock.Object);
            items.Add(environmentViewModelMock.Object);

            _target.Environments = items;
            //act

            //assert
            Assert.AreEqual(1, _target.Environments.Count);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestClearSearchTextCommand()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            environmentViewModelMock.SetupGet(it => it.IsConnected).Returns(true);

            _target.Environments.Add(environmentViewModelMock.Object);
            _target.SearchText = "someText";

            //act
            _target.ClearSearchTextCommand.Execute(null);
            Assert.IsTrue(_target.ClearSearchTextCommand.CanExecute(null));

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(_target.SearchText));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestCreateFolderCommand()
        {
            //arrange
            var selectedItemMock = new Mock<IExplorerTreeItem>();
            var selectedItemCommandMock = new Mock<ICommand>();
            selectedItemCommandMock.Setup(it => it.CanExecute(It.IsAny<object>())).Returns(true);
            selectedItemMock.SetupGet(it => it.CreateFolderCommand).Returns(selectedItemCommandMock.Object);
            _target.SelectedItem = selectedItemMock.Object;

            //act
            _target.CreateFolderCommand.Execute(null);
            Assert.IsTrue(_target.CreateFolderCommand.CanExecute(null));

            //assert
            selectedItemCommandMock.Verify(it=>it.CanExecute(null));
            selectedItemCommandMock.Verify(it=>it.Execute(null));
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestIsFromActivityDrop()
        {
            //arrange
            var isIsFromActivityDropChanged = false;
            _target.PropertyChanged += (s, e) =>
            {
                isIsFromActivityDropChanged = isIsFromActivityDropChanged || e.PropertyName == "IsFromActivityDrop";
            };

            //act
            _target.IsFromActivityDrop = !_target.IsFromActivityDrop;

            //assert
            Assert.IsTrue(_target.IsFromActivityDrop);
            Assert.IsTrue(isIsFromActivityDropChanged);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestIsRefreshing()
        {
            //arrange
            var isIsRefreshingChanged = false;
            _target.PropertyChanged += (s, e) =>
            {
                isIsRefreshingChanged = isIsRefreshingChanged || e.PropertyName == "IsRefreshing";
            };

            //act
            _target.IsRefreshing = !_target.IsRefreshing;

            //assert
            Assert.IsTrue(_target.IsRefreshing);
            Assert.IsTrue(isIsRefreshingChanged);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestAllowDrag()
        {
            //arrange
            var isAllowDragChanged = false;
            _target.PropertyChanged += (s, e) =>
            {
                isAllowDragChanged = isAllowDragChanged || e.PropertyName == "AllowDrag";
            };
            _target.AllowDrag = false;

            //act
            _target.AllowDrag = !_target.AllowDrag;

            //assert
            Assert.IsTrue(_target.AllowDrag);
            Assert.IsTrue(isAllowDragChanged);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestSelectedItemExplorerViewModel_()
        {
            //arrange
            var isSelectedItem = false;
            var isSelectedItemChangedEventRaised = false;
            var selectedItemMock = new Mock<IExplorerTreeItem>();
            _target.PropertyChanged += (s, e) =>
            {
                isSelectedItem = isSelectedItem || e.PropertyName == "SelectedItem";
            };
            _target.SelectedItemChanged += (s, e) =>
            {
                isSelectedItemChangedEventRaised = true;
            };
            _target.SelectedItem = null;

            //act
            _target.SelectedItem = selectedItemMock.Object;
            _target.SelectedItem = selectedItemMock.Object;

            //assert
            Assert.AreSame(_target.SelectedItem, selectedItemMock.Object);
            Assert.IsTrue(isSelectedItemChangedEventRaised);
            Assert.IsTrue(isSelectedItem);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestShowConnectControl()
        {
            //arrange
            _target.ShowConnectControl = false;

            //act
            _target.ShowConnectControl = !_target.ShowConnectControl;

            //assert
            Assert.IsTrue(_target.ShowConnectControl);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestSelectedDataItems()
        {
            //arrange
            var isSelectedItems = false;
            var selectedItemMock = new Mock<IExplorerTreeItem>();
            var selectedDataItems = new object[] {selectedItemMock.Object};
            _target.SelectedDataItems = new object[] {};
            _target.PropertyChanged += (s, e) =>
            {
                isSelectedItems = isSelectedItems || e.PropertyName == "SelectedDataItems";
            };

            //act
            _target.SelectedDataItems = selectedDataItems;
            var value = _target.SelectedDataItems;

            //assert
            Assert.AreSame(selectedDataItems, value);
            Assert.IsTrue(isSelectedItems);
            Assert.AreSame(selectedItemMock.Object, _target.SelectedItem);
            Assert.IsTrue(_target.ShowConnectControl);
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestEnvironments()
        {
            //arrange
            var isEnvironmentsChanged = false;
            _target.PropertyChanged += (s, e) =>
            {
                isEnvironmentsChanged = isEnvironmentsChanged || e.PropertyName == "Environments";
            };
            var list = new ObservableCollection<IEnvironmentViewModel>();

            //act
            _target.Environments = list;
            var actual = _target.Environments;
            //assert
            Assert.AreEqual(1, actual.Count);
            Assert.IsTrue(isEnvironmentsChanged);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestSelectedEnvironment()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();

            //act
            _target.SelectedEnvironment = environmentViewModelMock.Object;

            //assert
            Assert.AreSame(environmentViewModelMock.Object, _target.SelectedEnvironment);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestSelectedServer()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            var serverMock = new Mock<IServer>();
            environmentViewModelMock.SetupGet(it => it.Server).Returns(serverMock.Object);
            _target.SelectedEnvironment = environmentViewModelMock.Object;

            //act
            var actual = _target.SelectedServer;

            //assert
            Assert.AreSame(serverMock.Object, actual);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestSearchTextNotChanged()
        {
            //arrange
            var isSearchText = false;
            _target.PropertyChanged += (s, e) =>
            {
                isSearchText = isSearchText || e.PropertyName == "SearchText";
            };

            //act
            _target.SearchText = _target.SearchText;

            //assert
            Assert.IsFalse(isSearchText);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestSearchTextChanged()
        {
            //arrange
            var isSearchText = false;
            var isEnvironments = false;
            _target.PropertyChanged += (s, e) =>
            {
                isSearchText = isSearchText || e.PropertyName == "SearchText";
                isEnvironments = isEnvironments || e.PropertyName == "Environments";
            };
            var list = new ObservableCollection<IEnvironmentViewModel>();
            _target.Environments = list;

            //act
            _target.SearchText = _target.SearchText+"someTxt";

            //assert
            Assert.IsTrue(isSearchText);
            Assert.IsTrue(isEnvironments);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestSearchToolTip()
        {
            //arrange

            //act
            var actual = _target.SearchToolTip;

            //assert
            Assert.IsTrue(!string.IsNullOrEmpty(actual));
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestExplorerClearSearchTooltip()
        {
            //arrange

            //act
            var actual = _target.ExplorerClearSearchTooltip;

            //assert
            Assert.IsTrue(!string.IsNullOrEmpty(actual));
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestRefreshToolTip()
        {
            //arrange

            //act
            var actual = _target.RefreshToolTip;

            //assert
            Assert.IsTrue(!string.IsNullOrEmpty(actual));
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestIsDeploy()
        {
            //arrange
            _target.IsDeploy = false;

            //act
            _target.IsDeploy = !_target.IsDeploy;

            //assert
            Assert.IsTrue(_target.IsDeploy);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExplorerViewModel_TestConstructorArgumentNull()
        {
            new ExplorerViewModel(null, _eventAggregatorMock.Object,true);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public async Task ExplorerViewModel_TestServerConnected()
        {
            //arrange
            var studioUpdateManagerMock = new Mock<IStudioUpdateManager>();
            var connectionMock = new Mock<IServer>();
            connectionMock.Setup(it => it.ConnectAsync()).Returns(Task.FromResult(true));
            connectionMock.SetupGet(it => it.UpdateRepository).Returns(studioUpdateManagerMock.Object);
            var explorerItemMock = new Mock<IExplorerItem>();
            connectionMock.Setup(it=>it.LoadExplorer(It.IsAny<bool>())).Returns(Task.FromResult(explorerItemMock.Object));
            var isEnvironments = false;
            _target.PropertyChanged += (s, e) =>
            {
                isEnvironments = isEnvironments || e.PropertyName == "Environments";
            };

            //act
            await _target.ConnectControlViewModel.TryConnectAsync(connectionMock.Object);

            //assert   
            Assert.IsTrue(isEnvironments);
            Assert.IsTrue(_target.Environments.Any());
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public async Task ExplorerViewModel_TestServerDisconnected()
        {
            //arrange
            var studioUpdateManagerMock = new Mock<IStudioUpdateManager>();

            var serverConnectionEnvironmentId = Guid.NewGuid();

            var serverConnectionMock = new Mock<IServer>();
            serverConnectionMock.Setup(it => it.ConnectAsync()).Returns(Task.FromResult(true));
            serverConnectionMock.SetupGet(it => it.UpdateRepository).Returns(studioUpdateManagerMock.Object);
            serverConnectionMock.SetupGet(it => it.EnvironmentID).Returns(serverConnectionEnvironmentId);
            serverConnectionMock.SetupGet(server => server.EnvironmentID).Returns(serverConnectionEnvironmentId);
            serverConnectionMock.SetupGet(it => it.DisplayName).Returns("someName");
            serverConnectionMock.SetupGet(it => it.DisplayName).Returns("someName (Connected)");

            var explorerItemMock = new Mock<IExplorerItem>();
            serverConnectionMock.Setup(it => it.LoadExplorer(It.IsAny<bool>())).Returns(Task.FromResult(explorerItemMock.Object));

            var child = new Mock<IExplorerItemViewModel>();
            child.SetupGet(it => it.AllowResourceCheck).Returns(true);
            child.SetupGet(it => it.IsVisible).Returns(true);
            var mockEnvironment = new Mock<IEnvironmentViewModel>();
            mockEnvironment.Setup(env => env.Server).Returns(serverConnectionMock.Object);
            mockEnvironment.Setup(env => env.Children).Returns(new ObservableCollection<IExplorerItemViewModel>() { child.Object });
            mockEnvironment.Setup(env => env.AsList()).Returns(new ObservableCollection<IExplorerItemViewModel>() { child.Object });

            _target.Environments.Add(mockEnvironment.Object);
            var isEnvironments = false;
            _target.PropertyChanged += (s, e) =>
            {
                isEnvironments = isEnvironments || e.PropertyName == "Environments";
            };

            //act
            await _target.ConnectControlViewModel.TryConnectAsync(serverConnectionMock.Object);

            //assert   
            Assert.IsTrue(isEnvironments);
            Assert.IsTrue(_target.Environments.Any());
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestServerReConnected()
        {
            //arrange
            var studioUpdateManagerMock = new Mock<IStudioUpdateManager>();
            var connectionMock = new Mock<IServer>();
            connectionMock.Setup(it => it.ConnectAsync()).Returns(Task.FromResult(true));
            connectionMock.SetupGet(it => it.UpdateRepository).Returns(studioUpdateManagerMock.Object);
            var explorerItemMock = new Mock<IExplorerItem>();
            connectionMock.Setup(it => it.LoadExplorer(It.IsAny<bool>())).Returns(Task.FromResult(explorerItemMock.Object));
            var isEnvironments = false;
            _target.PropertyChanged += (s, e) =>
            {
                isEnvironments = isEnvironments || e.PropertyName == "Environments";
            };
            var netwworkStateChangedEventArgs = new Mock<INetworkStateChangedEventArgs>();
            netwworkStateChangedEventArgs.SetupGet(it => it.State).Returns(ConnectionNetworkState.Connected);
            var server1Mock = new Mock<IServer>();
            server1Mock.SetupGet(it => it.EnvironmentID).Returns(Guid.Empty);
            _target.ConnectControlViewModel.Servers.Add(server1Mock.Object);
            _target.ConnectControlViewModel.LoadServers();
            _target.IsLoading = false;

            //act
            server1Mock.Raise(it=>it.NetworkStateChanged+=null,netwworkStateChangedEventArgs.Object,server1Mock.Object);

            //assert   
            Assert.IsFalse(isEnvironments);
            Assert.IsFalse(_target.IsLoading);
        }
      
        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public async Task ExplorerViewModel_TestRefreshEnvironment()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            var serverMock = new Mock<IServer>();
            var envId = Guid.NewGuid();
            serverMock.SetupGet(it => it.EnvironmentID).Returns(envId);
            environmentViewModelMock.SetupGet(it => it.Server).Returns(serverMock.Object);
            environmentViewModelMock.SetupGet(it => it.IsConnected).Returns(true);
            
            var child = new Mock<IExplorerItemViewModel>();
            child.SetupGet(it => it.AllowResourceCheck).Returns(true);
            child.SetupGet(it => it.IsVisible).Returns(true);
            environmentViewModelMock.Setup(a => a.Children).Returns(new ObservableCollection<IExplorerItemViewModel>() { child.Object });
            
            _target.Environments = new ObservableCollection<IEnvironmentViewModel>() {environmentViewModelMock.Object};
            _target.SearchText = "someText";

            //act
            await _target.RefreshEnvironment(envId);

            //assert
            environmentViewModelMock.VerifyGet(it => it.IsConnected);
            environmentViewModelMock.Verify(it => it.LoadAsync(It.IsAny<bool>(), It.IsAny<bool>()));
            environmentViewModelMock.Verify(it => it.Filter("someText"));
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public async Task ExplorerViewModel_TestRefreshEnvironmentSetsPermissions()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            var serverMock = new Mock<IServer>();
            var envId = Guid.NewGuid();
            serverMock.SetupGet(it => it.EnvironmentID).Returns(envId);
            environmentViewModelMock.SetupGet(it => it.Server).Returns(serverMock.Object);
            environmentViewModelMock.SetupGet(it => it.IsConnected).Returns(true);

            var child = new Mock<IExplorerItemViewModel>();
            child.SetupGet(it => it.AllowResourceCheck).Returns(true);
            child.SetupGet(it => it.IsVisible).Returns(true);
            environmentViewModelMock.Setup(a => a.Children).Returns(new ObservableCollection<IExplorerItemViewModel>() { child.Object });

            _target.Environments = new ObservableCollection<IEnvironmentViewModel>() { environmentViewModelMock.Object };
            _target.SearchText = "someText";

            //act
            await _target.RefreshEnvironment(envId);

            //assert
            environmentViewModelMock.VerifyGet(it => it.IsConnected);
            environmentViewModelMock.Verify(it => it.LoadAsync(It.IsAny<bool>(), It.IsAny<bool>()));
            environmentViewModelMock.Verify(it => it.Filter("someText"));
            environmentViewModelMock.Verify(it => it.SetPropertiesForDialogFromPermissions(It.IsAny<IWindowsGroupPermission>()));
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public async Task ExplorerViewModel_TestRefreshSelectedEnvironment()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            var serverMock = new Mock<IServer>();
            var envId = Guid.NewGuid();
            serverMock.SetupGet(it => it.EnvironmentID).Returns(envId);
            environmentViewModelMock.SetupGet(it => it.Server).Returns(serverMock.Object);
            environmentViewModelMock.SetupGet(it => it.IsConnected).Returns(true);

            var child = new Mock<IExplorerItemViewModel>();
            child.SetupGet(it => it.AllowResourceCheck).Returns(true);
            child.SetupGet(it => it.IsVisible).Returns(true);
            environmentViewModelMock.Setup(a => a.Children).Returns(new ObservableCollection<IExplorerItemViewModel>() { child.Object });

            _target.Environments = new ObservableCollection<IEnvironmentViewModel>() { environmentViewModelMock.Object };
            _target.SelectedEnvironment = environmentViewModelMock.Object;

            //act
            await _target.RefreshSelectedEnvironmentAsync();

            //assert
            environmentViewModelMock.VerifyGet(it => it.IsConnected);
            environmentViewModelMock.Verify(it => it.LoadAsync(It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestFilter()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            _target.Environments = new ObservableCollection<IEnvironmentViewModel>() { environmentViewModelMock.Object };
            var isEnvironments = false;
            _target.PropertyChanged += (s, e) =>
            {
                isEnvironments = isEnvironments || e.PropertyName == "Environments";
            };

            //act
            _target.Filter("someText");

            //assert
            Assert.IsTrue(isEnvironments);
            environmentViewModelMock.Verify(it => it.Filter("someText"));
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_RemoveItemChildRemoveItem()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            var serverMock = new Mock<IServer>();
            environmentViewModelMock.SetupGet(it => it.Server).Returns(serverMock.Object);
            environmentViewModelMock.SetupGet(it => it.Children)
                .Returns(new ObservableCollection<IExplorerItemViewModel>());
            var explorerItemViewModelMock = new Mock<IExplorerItemViewModel>();
            explorerItemViewModelMock.SetupGet(it => it.Server).Returns(serverMock.Object);
            _target.Environments = new ObservableCollection<IEnvironmentViewModel>() { environmentViewModelMock.Object };
            var isEnvironments = false;
            _target.PropertyChanged += (s, e) =>
            {
                isEnvironments = isEnvironments || e.PropertyName == "Environments";
            };

            //act
            _target.RemoveItem(explorerItemViewModelMock.Object);

            //assert
            Assert.IsTrue(isEnvironments);
            environmentViewModelMock.Verify(it => it.RemoveItem(explorerItemViewModelMock.Object));
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_RemoveItemChildRemoveChild()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            var serverMock = new Mock<IServer>();
            var mockEnvironmentConnection = SetupMockConnection();
            serverMock.SetupGet(it => it.Connection).Returns(mockEnvironmentConnection.Object);
            environmentViewModelMock.SetupGet(it => it.Server).Returns(serverMock.Object);
            var explorerItemViewModelMock = new Mock<IExplorerItemViewModel>();
            explorerItemViewModelMock.SetupGet(it => it.Server).Returns(serverMock.Object);
            environmentViewModelMock.SetupGet(it => it.Children)
                .Returns(new ObservableCollection<IExplorerItemViewModel>() { explorerItemViewModelMock.Object});
            _target.Environments = new ObservableCollection<IEnvironmentViewModel>() { environmentViewModelMock.Object };
            var isEnvironments = false;
            _target.PropertyChanged += (s, e) =>
            {
                isEnvironments = isEnvironments || e.PropertyName == "Environments";
            };

            //act
            _target.RemoveItem(explorerItemViewModelMock.Object);

            //assert
            Assert.IsTrue(isEnvironments);
            environmentViewModelMock.Verify(it => it.RemoveChild(explorerItemViewModelMock.Object));
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestUpdateHelpDescriptor()
        {
            //arrange
            var helpWindowViewModelMock = new Mock<IHelpWindowViewModel>();
            var mainViewModelMock = new Mock<IShellViewModel>();
            mainViewModelMock.SetupGet(it => it.HelpViewModel).Returns(helpWindowViewModelMock.Object);
            CustomContainer.Register(mainViewModelMock.Object);

            //act
            _target.UpdateHelpDescriptor("someHelpText");

            //assert
            helpWindowViewModelMock.Verify(it => it.UpdateHelpText("someHelpText"));
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestSelectItemGuid()
        {
            //arrange
            var itemId = Guid.NewGuid();
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            Action<IExplorerItemViewModel> propAction = null;
            var explorerItemMock = new Mock<IExplorerItemViewModel>();
            var explorerItemMockSelectAction = new Mock<IExplorerItemViewModel>();
            environmentViewModelMock.Setup(
                it => it.SelectItem(It.IsAny<Guid>(), It.IsAny<Action<IExplorerItemViewModel>>()))
                .Callback<Guid, Action<IExplorerItemViewModel>>((id, act) => act?.Invoke(explorerItemMock.Object));
            environmentViewModelMock.SetupSet(it => it.SelectAction = It.IsAny<Action<IExplorerItemViewModel>>())
                .Callback<Action<IExplorerItemViewModel>>(a => propAction = a);
             _target.Environments = new ObservableCollection<IEnvironmentViewModel>() { environmentViewModelMock.Object };
            
            //act
            _target.SelectItem(itemId);

            //assert
            Assert.AreSame(explorerItemMock.Object, _target.SelectedItem);
            Assert.IsNotNull(propAction);
            propAction(explorerItemMockSelectAction.Object);
            Assert.AreSame(explorerItemMockSelectAction.Object, _target.SelectedItem);
            environmentViewModelMock.Verify(it => it.SelectItem(itemId, It.IsAny<Action<IExplorerItemViewModel>>()));
            environmentViewModelMock.VerifySet(it => it.SelectAction = It.IsAny<Action<IExplorerItemViewModel>>());
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestSelectItemString()
        {
            //arrange
            var itemPath = "somePath";
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            Action<IExplorerItemViewModel> propAction = null;
            var explorerItemMock = new Mock<IExplorerItemViewModel>();
            var explorerItemMockSelectAction = new Mock<IExplorerItemViewModel>();
            environmentViewModelMock.Setup(
                it => it.SelectItem(It.IsAny<string>(), It.IsAny<Action<IExplorerItemViewModel>>()))
                .Callback<string, Action<IExplorerItemViewModel>>((id, act) => act?.Invoke(explorerItemMock.Object));
            environmentViewModelMock.SetupSet(it => it.SelectAction = It.IsAny<Action<IExplorerItemViewModel>>())
                .Callback<Action<IExplorerItemViewModel>>(a => propAction = a);
            _target.Environments = new ObservableCollection<IEnvironmentViewModel>() { environmentViewModelMock.Object };

            //act
            _target.SelectItem(itemPath);

            //assert
            Assert.AreSame(explorerItemMock.Object, _target.SelectedItem);
            Assert.IsNotNull(propAction);
            propAction(explorerItemMockSelectAction.Object);
            Assert.AreSame(explorerItemMockSelectAction.Object, _target.SelectedItem);
            environmentViewModelMock.Verify(it => it.SelectItem(itemPath, It.IsAny<Action<IExplorerItemViewModel>>()));
            environmentViewModelMock.VerifySet(it => it.SelectAction = It.IsAny<Action<IExplorerItemViewModel>>());
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestDispose()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            _target.Environments = new ObservableCollection<IEnvironmentViewModel>() { environmentViewModelMock.Object };

            //act
            _target.Dispose();

            //assert
            environmentViewModelMock.Verify(it => it.Dispose());
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestFindItems()
        {
            //act
            var value = _target.FindItems();

            //assert
            Assert.IsNull(value);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExplorerViewModel))]
        public void ExplorerViewModel_TestSelectAction()
        {
            //act
            _target.Environments.First().SelectAction(null);
        }
    }
}