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
using Dev2.Interfaces;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ExplorerViewModelTests
    {
        #region Fields

        private ExplorerViewModel _target;

        private Guid _localhostServerEnvironmentId;
        private Mock<IShellViewModel> _shellViewModelMock;
        private Mock<IServer> _localhostServerMock;
        private Mock<IWindowsGroupPermission> _windowsGroupPermissionMock;
        private Mock<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator> _eventAggregatorMock;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        { 
            _shellViewModelMock = new Mock<IShellViewModel>();
            _localhostServerEnvironmentId = Guid.NewGuid();
            _localhostServerMock = new Mock<IServer>();
            _localhostServerMock.Setup(it => it.EnvironmentID).Returns(_localhostServerEnvironmentId);
            _windowsGroupPermissionMock = new Mock<IWindowsGroupPermission>();
            _localhostServerMock.Setup(it => it.Permissions).Returns(new List<IWindowsGroupPermission>()
            {
                _windowsGroupPermissionMock.Object
            });
            _localhostServerMock.Setup(it => it.GetServerConnections()).Returns(new List<IServer>());
            _localhostServerMock.SetupGet(it => it.ResourceName).Returns("localhostServerResourceName");
            _shellViewModelMock.SetupGet(it => it.LocalhostServer).Returns(_localhostServerMock.Object);
            _eventAggregatorMock = new Mock<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator>();
            _target = new ExplorerViewModel(_shellViewModelMock.Object, _eventAggregatorMock.Object,true);
        }

        #endregion Test initialize

        #region Test commands

        [TestMethod]
        public void TestRefreshCommand()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            environmentViewModelMock.SetupGet(it => it.IsConnected).Returns(true);
            environmentViewModelMock.SetupGet(it => it.ResourceName).Returns("localhostServerResourceName");

            var environmentId = _target.ConnectControlViewModel.SelectedConnection.EnvironmentID;

            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.ResourceName).Returns("localhostServerResourceName");
            mockServer.Setup(server => server.EnvironmentID).Returns(environmentId);
            environmentViewModelMock.SetupGet(it => it.Server).Returns(mockServer.Object);
            
            _target.Environments.Add(environmentViewModelMock.Object);
            _target.SearchText = "someText";
            _target.Environments.Remove(_target.Environments.First(it => it is EnvironmentViewModel));
            //act
            _target.RefreshCommand.Execute(null);
            Assert.IsTrue(_target.RefreshCommand.CanExecute(null));

            //assert
            environmentViewModelMock.VerifyGet(it => it.IsConnected);
            environmentViewModelMock.Verify( it => it.Load(It.IsAny<bool>(), It.IsAny<bool>()));
            environmentViewModelMock.Verify(it => it.Filter("someText"));
        }

        [TestMethod]
        public void TestClearSearchTextCommand()
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
        public void TestCreateFolderCommand()
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

        #endregion Test commands

        #region Test properties

        [TestMethod]
        public void TestIsFromActivityDrop()
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
        public void TestIsRefreshing()
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
        public void TestAllowDrag()
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
        public void TestSelectedItem()
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
        public void TestShowConnectControl()
        {
            //arrange
            _target.ShowConnectControl = false;

            //act
            _target.ShowConnectControl = !_target.ShowConnectControl;

            //assert
            Assert.IsTrue(_target.ShowConnectControl);
        }

        [TestMethod]
        public void TestSelectedDataItems()
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
        public void TestEnvironments()
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
            Assert.AreSame(list, actual);
            Assert.IsTrue(isEnvironmentsChanged);
        }

        [TestMethod]
        public void TestSelectedEnvironment()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();

            //act
            _target.SelectedEnvironment = environmentViewModelMock.Object;

            //assert
            Assert.AreSame(environmentViewModelMock.Object, _target.SelectedEnvironment);
        }

        [TestMethod]
        public void TestSelectedServer()
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
        public void TestSearchTextNotChanged()
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
        public void TestSearchTextChanged()
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
        public void TestSearchToolTip()
        {
            //arrange

            //act
            var actual = _target.SearchToolTip;

            //assert
            Assert.IsTrue(!string.IsNullOrEmpty(actual));
        }

        [TestMethod]
        public void TestExplorerClearSearchTooltip()
        {
            //arrange

            //act
            var actual = _target.ExplorerClearSearchTooltip;

            //assert
            Assert.IsTrue(!string.IsNullOrEmpty(actual));
        }

        [TestMethod]
        public void TestRefreshToolTip()
        {
            //arrange

            //act
            var actual = _target.RefreshToolTip;

            //assert
            Assert.IsTrue(!string.IsNullOrEmpty(actual));
        }

        [TestMethod]
        public void TestIsDeploy()
        {
            //arrange
            _target.IsDeploy = false;

            //act
            _target.IsDeploy = !_target.IsDeploy;

            //assert
            Assert.IsTrue(_target.IsDeploy);
        }

        #endregion Test properties

        #region Test methods

        [TestMethod]
        public void TestServerDisconnect()
        {
            //arrange
            var isEnvironmentChanged = false;
            _target.PropertyChanged += (s, e) =>
            {
                isEnvironmentChanged = isEnvironmentChanged || e.PropertyName == "Environments";
            };
            var childMock = new Mock<IExplorerItemViewModel>();
            childMock.SetupGet(it => it.IsVisible).Returns(true);
            _target.Environments.First().AddChild(childMock.Object);
            _localhostServerMock.SetupGet(it => it.IsConnected).Returns(true);
            _localhostServerMock.SetupGet(it => it.HasLoaded).Returns(true);

            //act
            _target.ConnectControlViewModel.ToggleConnectionStateCommand.Execute(null);

            //assert
            childMock.VerifySet(it => it.IsVisible = false);
            Assert.IsTrue(isEnvironmentChanged);
            Assert.IsFalse(_target.Environments.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructorArgumentNull()
        {
            new ExplorerViewModel(null, _eventAggregatorMock.Object,true);
        }

        [TestMethod]
        public async Task TestServerConnected()
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
            await _target.ConnectControlViewModel.Connect(connectionMock.Object);

            //assert   
            Assert.IsTrue(isEnvironments);
            Assert.IsTrue(_target.Environments.Any());
        }

        [TestMethod]
        public void TestServerReConnected()
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
            server1Mock.SetupGet(it => it.ResourceID).Returns(Guid.Empty);
            _target.ConnectControlViewModel.Servers.Add(server1Mock.Object);
            _target.ConnectControlViewModel.LoadServers();
            _target.IsLoading = false;

            //act
            server1Mock.Raise(it=>it.NetworkStateChanged+=null,netwworkStateChangedEventArgs.Object,server1Mock.Object);

            //assert   
            Assert.IsFalse(isEnvironments);
        }
      
        [TestMethod]
        public void TestRefreshEnvironment()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            var serverMock = new Mock<IServer>();
            var envId = Guid.NewGuid();
            serverMock.SetupGet(it => it.EnvironmentID).Returns(envId);
            environmentViewModelMock.SetupGet(it => it.Server).Returns(serverMock.Object);
            environmentViewModelMock.SetupGet(it => it.IsConnected).Returns(true);
            _target.Environments = new ObservableCollection<IEnvironmentViewModel>() {environmentViewModelMock.Object};
            _target.SearchText = "someText";

            //act
            _target.RefreshEnvironment(envId);

            //assert
            environmentViewModelMock.VerifyGet(it => it.IsConnected);
            environmentViewModelMock.Verify(it => it.Load(It.IsAny<bool>(), It.IsAny<bool>()));
            environmentViewModelMock.Verify(it => it.Filter("someText"));
        }

        [TestMethod]
        public async Task TestRefreshSelectedEnvironment()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            var serverMock = new Mock<IServer>();
            var envId = Guid.NewGuid();
            serverMock.SetupGet(it => it.EnvironmentID).Returns(envId);
            environmentViewModelMock.SetupGet(it => it.Server).Returns(serverMock.Object);
            environmentViewModelMock.SetupGet(it => it.IsConnected).Returns(true);
            _target.Environments = new ObservableCollection<IEnvironmentViewModel>() { environmentViewModelMock.Object };
            _target.SelectedEnvironment = environmentViewModelMock.Object;

            //act
            await _target.RefreshSelectedEnvironment();

            //assert
            environmentViewModelMock.VerifyGet(it => it.IsConnected);
            environmentViewModelMock.Verify(it => it.Load(It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [TestMethod]
        public void TestFilter()
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
        public void RemoveItemChildRemoveItem()
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
        public void RemoveItemChildRemoveChild()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            var serverMock = new Mock<IServer>();
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
        public void TestUpdateHelpDescriptor()
        {
            //arrange
            var helpWindowViewModelMock = new Mock<IHelpWindowViewModel>();
            var mainViewModelMock = new Mock<IMainViewModel>();
            mainViewModelMock.SetupGet(it => it.HelpViewModel).Returns(helpWindowViewModelMock.Object);
            CustomContainer.Register(mainViewModelMock.Object);

            //act
            _target.UpdateHelpDescriptor("someHelpText");

            //assert
            helpWindowViewModelMock.Verify(it => it.UpdateHelpText("someHelpText"));
        }

        [TestMethod]
        public void TestSelectItemGuid()
        {
            //arrange
            var itemId = Guid.NewGuid();
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            Action<IExplorerItemViewModel> propAction = null;
            var explorerItemMock = new Mock<IExplorerItemViewModel>();
            var explorerItemMockSelectAction = new Mock<IExplorerItemViewModel>();
            environmentViewModelMock.Setup(
                it => it.SelectItem(It.IsAny<Guid>(), It.IsAny<Action<IExplorerItemViewModel>>()))
                .Callback<Guid, Action<IExplorerItemViewModel>>((id, act) => act(explorerItemMock.Object));
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
        public void TestSelectItemString()
        {
            //arrange
            var itemPath = "somePath";
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            Action<IExplorerItemViewModel> propAction = null;
            var explorerItemMock = new Mock<IExplorerItemViewModel>();
            var explorerItemMockSelectAction = new Mock<IExplorerItemViewModel>();
            environmentViewModelMock.Setup(
                it => it.SelectItem(It.IsAny<string>(), It.IsAny<Action<IExplorerItemViewModel>>()))
                .Callback<string, Action<IExplorerItemViewModel>>((id, act) => act(explorerItemMock.Object));
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
        public void TestDispose()
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
        public void TestFindItems()
        {
            //act
            var value = _target.FindItems();

            //assert
            Assert.IsNull(value);
        }

        [TestMethod]
        public void TestSelectAction()
        {
            //act
            _target.Environments.First().SelectAction(null);
        }

        #endregion Test methods
    }
}