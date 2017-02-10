using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.Explorer;
using Microsoft.Practices.Prism.PubSubEvents;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass()]
    public class DeploySourceExplorerViewModelTests
    {
        #region Fields

        private DeploySourceExplorerViewModel _target;

        private Mock<IShellViewModel> _shellViewModelMock;
        private Mock<IServer> _serverMock;
        private Mock<IEventAggregator> _eventAggregatorMock;
        private Mock<IDeployStatsViewerViewModel> _deployStatsViewerViewModel;
        private Mock<IStudioUpdateManager> _studioUpdateManagerMock;
        private Mock<IExplorerItem> _explorerItemMock;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _shellViewModelMock = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();
            _serverMock.Setup(server => server.GetServerVersion()).Returns("1.1.2");
            _studioUpdateManagerMock = new Mock<IStudioUpdateManager>();
            _explorerItemMock=new Mock<IExplorerItem>();
            _explorerItemMock.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItem>());
            _serverMock.Setup(it => it.LoadExplorer(false)).ReturnsAsync(_explorerItemMock.Object);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_studioUpdateManagerMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("someResName");
            _shellViewModelMock.SetupGet(it => it.LocalhostServer).Returns(_serverMock.Object);
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _deployStatsViewerViewModel = new Mock<IDeployStatsViewerViewModel>();
            _target = new DeploySourceExplorerViewModel(_shellViewModelMock.Object, _eventAggregatorMock.Object, _deployStatsViewerViewModel.Object);
        }

        #endregion Test initialize

        #region Test properties

        [TestMethod]
        public void TestEnvironments()
        {
            //arrange
            //act
            var env = _target.Environments;

            //assert
        }

        [TestMethod]
        public void TestPreselected()
        {
            //arrange
            var firstPreselectedItemMock = new Mock<IExplorerItemViewModel>();
            var firstServerMock = new Mock<IServer>();
            var firstServerId = Guid.NewGuid();
            firstServerMock.SetupGet(it => it.EnvironmentID).Returns(firstServerId);
            firstPreselectedItemMock.SetupGet(it => it.Server).Returns(firstServerMock.Object);

            var secondPreselectedItemMock = new Mock<IExplorerItemViewModel>();
            var secondServerMock = new Mock<IServer>();
            var secondServerId = Guid.NewGuid();
            secondServerMock.SetupGet(it => it.EnvironmentID).Returns(secondServerId);
            secondPreselectedItemMock.SetupGet(it => it.Server).Returns(secondServerMock.Object);

            var preselected = new[] { firstPreselectedItemMock.Object, secondPreselectedItemMock.Object };
            var selectedEnvironmentMock = new Mock<IEnvironmentViewModel>();
            var explorerItemViewModelMock = new Mock<IExplorerItemViewModel>();

            selectedEnvironmentMock
                .Setup(it => it.AsList())
                .Returns(new List<IExplorerItemViewModel>() { explorerItemViewModelMock.Object, firstPreselectedItemMock.Object, secondPreselectedItemMock.Object });
            _target.SelectedEnvironment = selectedEnvironmentMock.Object;
            //act
            _target.Preselected = preselected;
            var value = _target.Preselected;

            //assert
            Assert.IsNull(value);
            explorerItemViewModelMock.VerifySet(it => it.IsResourceChecked = false);
            firstPreselectedItemMock.VerifySet(it => it.IsResourceChecked = true);
            secondPreselectedItemMock.VerifySet(it => it.IsResourceChecked = true);
        }

        [TestMethod]
        public void TestSelectedItemsEmpty()
        {
            //arrange
            _target.SelectedEnvironment = null;

            //act
            var actual = _target.SelectedItems;

            //assert
            Assert.IsTrue(!actual.Any());
        }

        [TestMethod]
        public void TestSelectedItemsNotEmpty()
        {
            //arrange
            var selectedEnvironmentMock = new Mock<IEnvironmentViewModel>();
            var child = new Mock<IExplorerItemViewModel>();
            child.SetupGet(it => it.IsResourceChecked).Returns(true);
            var childNotChecked = new Mock<IExplorerItemViewModel>();
            childNotChecked.SetupGet(it => it.IsResourceChecked).Returns(false);
            selectedEnvironmentMock.SetupGet(it => it.UnfilteredChildren).Returns(new ObservableCollection<IExplorerItemViewModel>()
            {
                child.Object,
                childNotChecked.Object
            });
            _target.SelectedEnvironment = selectedEnvironmentMock.Object;

            //act
            var actual = _target.SelectedItems;

            //assert
            Assert.AreEqual(1, actual.Count);
            Assert.IsTrue(actual.Contains(child.Object));
        }

        [TestMethod]
        public void TestSelectedItemsSet()
        {
            //arrange
            var selectedEnvironmentMock = new Mock<IEnvironmentViewModel>();
            var child1 = new Mock<IExplorerItemViewModel>();
            child1.SetupGet(it => it.ResourceId).Returns(Guid.NewGuid());
            var child2 = new Mock<IExplorerItemViewModel>();
            child2.SetupGet(it => it.ResourceId).Returns(Guid.NewGuid());
            var child3 = new Mock<IExplorerItemViewModel>();
            child3.SetupGet(it => it.ResourceId).Returns(Guid.NewGuid());
            selectedEnvironmentMock.Setup(it => it.AsList()).Returns(new List<IExplorerItemViewModel>()
            {
                child1.Object,
                child2.Object
            });
            _target.SelectedEnvironment = selectedEnvironmentMock.Object;
            var selectedItemsValue = new List<IExplorerTreeItem>() {child1.Object, child2.Object, child3.Object};

            //act
            _target.SelectedItems = selectedItemsValue;

            //assert
            child1.VerifySet(it => it.IsSelected = true);
            child2.VerifySet(it => it.IsSelected = true);
            child3.VerifySet(it => it.IsSelected = true, Times.Never);
        }

        [TestMethod]
        public void TestSelectedItemsSetEnvironmentNull()
        {
            //arrange
            var child1 = new Mock<IExplorerItemViewModel>();
            child1.SetupGet(it => it.ResourceId).Returns(Guid.NewGuid());
            var child2 = new Mock<IExplorerItemViewModel>();
            child2.SetupGet(it => it.ResourceId).Returns(Guid.NewGuid());
            var child3 = new Mock<IExplorerItemViewModel>();
            child3.SetupGet(it => it.ResourceId).Returns(Guid.NewGuid());

            var selectedItemsValue = new List<IExplorerTreeItem>() { child1.Object, child2.Object, child3.Object };

            //act
            _target.SelectedItems = selectedItemsValue;

            //assert
            child1.VerifySet(it => it.IsSelected = true, Times.Never);
            child2.VerifySet(it => it.IsSelected = true, Times.Never);
            child3.VerifySet(it => it.IsSelected = true, Times.Never);
        }

        [TestMethod]
        public void TestServerVersion()
        {
            //arrange
            var selectedEnvironmentMock = new Mock<IEnvironmentViewModel>();
            var serverMock = new Mock<IServer>();
            var input = "1.1.2";
            serverMock.Setup(it => it.GetServerVersion()).Returns(input);
            selectedEnvironmentMock.SetupGet(it => it.Server).Returns(serverMock.Object);
            _target.SelectedEnvironment = selectedEnvironmentMock.Object;

            //act
            var value = _target.ServerVersion;

            //assert
            Assert.AreEqual(Version.Parse(input), value);
        }

        #endregion Test properties

        #region Test methods

        [TestMethod]
        public void TestSelectedEnvironmentChanged()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            var serverMock = new Mock<IServer>();
            var serverId = Guid.NewGuid();
            serverMock.SetupGet(it => it.EnvironmentID).Returns(serverId);
            environmentViewModelMock.SetupGet(it => it.IsVisible).Returns(true);
            environmentViewModelMock.SetupGet(it => it.Server).Returns(serverMock.Object);
            var env = _target.Environments.First();
            var explorerItemViewModelMock = new Mock<IExplorerItemViewModel>();
            explorerItemViewModelMock.SetupGet(it => it.IsVisible).Returns(true);
            env.AddChild(explorerItemViewModelMock.Object);
            var environmentViewModels = _target.Environments.Union(new[] { environmentViewModelMock.Object }).ToList();
            _target.Environments = new ObservableCollection<IEnvironmentViewModel>(environmentViewModels );

            //act
            _target.ConnectControlViewModel.SelectedConnection = _serverMock.Object;

            //assert
            environmentViewModelMock.VerifySet(it => it.IsVisible = false);

            explorerItemViewModelMock.VerifySet(it => it.CanExecute = false);
            explorerItemViewModelMock.VerifySet(it => it.CanEdit = false);
            explorerItemViewModelMock.VerifySet(it => it.CanView = false);
            explorerItemViewModelMock.VerifySet(it => it.ShowContextMenu = false);
            explorerItemViewModelMock.VerifySet(it => it.SelectAction = It.IsAny<Action<IExplorerItemViewModel>>());
            explorerItemViewModelMock.VerifySet(it => it.AllowResourceCheck = true);
            explorerItemViewModelMock.VerifySet(it => it.CanDrop = false);
            explorerItemViewModelMock.VerifySet(it => it.CanDrag = false);
        }

        [TestMethod]
        public void TestServerСonnect()
        {
            //arrange
            var isEnvironmentChanged = false;
            _target.PropertyChanged += (s, e) =>
            {
                isEnvironmentChanged = isEnvironmentChanged || e.PropertyName == "Environments";
            };
            _serverMock.SetupGet(it => it.IsConnected).Returns(false);
            _serverMock.SetupGet(it => it.HasLoaded).Returns(false);
            _serverMock.Setup(it => it.ConnectAsync()).ReturnsAsync(true);

            //act
            _target.ConnectControlViewModel.ToggleConnectionStateCommand.Execute(null);

            //assert
            Assert.IsTrue(isEnvironmentChanged);
            Assert.AreEqual(2, _target.Environments.Count);
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
            await _target.ConnectControlViewModel.Connect(serverMock.Object);

            //assert
            Assert.IsTrue(isEnvironmentChanged);
            Assert.AreEqual(1, _target.Environments.Count);
        }

        [TestMethod]
        public void TestServerDisconnect()
        {
            //arrange
            var isEnvironmentChanged = false;
            _target.PropertyChanged += (s, e) =>
            {
                isEnvironmentChanged = isEnvironmentChanged || e.PropertyName == "Environments";
            };
            _serverMock.SetupGet(it => it.IsConnected).Returns(true);
            _serverMock.SetupGet(it => it.HasLoaded).Returns(true);

            //act
            _target.ConnectControlViewModel.ToggleConnectionStateCommand.Execute(null);

            //assert
            Assert.IsTrue(isEnvironmentChanged);
            Assert.IsFalse(_target.Environments.Any());
        }

        [TestMethod]
        public void TestEnvironmentSelectAll()
        {
            //arrange
            var env = _target.Environments.First();
            var explorerItemViewModelMock = new Mock<IExplorerItemViewModel>();
            explorerItemViewModelMock.SetupGet(it => it.IsVisible).Returns(true);
            explorerItemViewModelMock.SetupGet(it => it.ResourceName).Returns("Resource1");
            var explorerItemViewModelResourceCheckedMock = new Mock<IExplorerItemViewModel>();
            explorerItemViewModelResourceCheckedMock.SetupGet(it => it.IsVisible).Returns(true);
            explorerItemViewModelResourceCheckedMock.SetupGet(it => it.IsResourceChecked).Returns(true);
            explorerItemViewModelResourceCheckedMock.SetupGet(it => it.ResourceName).Returns("Resource2");
            env.AddChild(explorerItemViewModelMock.Object);
            env.AddChild(explorerItemViewModelResourceCheckedMock.Object);
            //act
            env.SelectAll();

            //assert
            _deployStatsViewerViewModel.Verify(it=>it.Calculate(It.Is<IList<IExplorerTreeItem>>(list=>list.Count==1 && list.Contains(explorerItemViewModelResourceCheckedMock.Object))));
        }

        [TestMethod]
        public void TestSelectActionFolder()
        {
            //arrange
            var childMock = new Mock<IExplorerItemViewModel>();
            var axMock = new Mock<IExplorerItemViewModel>();
            axMock.SetupGet(it => it.IsResourceChecked).Returns(true);
            axMock.SetupGet(it => it.ResourceType).Returns("Folder");
            axMock.Setup(it => it.Children)
                .Returns(new ObservableCollection<IExplorerItemViewModel> {childMock.Object});

            //act
            _target.Environments.First().SelectAction(axMock.Object);

            //assert
            
            _deployStatsViewerViewModel.Verify(
                it => it.Calculate(It.Is<IList<IExplorerTreeItem>>(match => !match.Any())));
        }

        [TestMethod]
        public void TestSelectActionParentFolder()
        {
            //arrange
            var childMock = new Mock<IExplorerItemViewModel>();
            var axParentMock = new Mock<IExplorerItemViewModel>();
            var axMock = new Mock<IExplorerItemViewModel>();
            axMock.SetupGet(it => it.IsResourceChecked).Returns(true);
            axMock.SetupGet(it => it.ResourceType).Returns("DbService");
            axParentMock.SetupGet(it => it.ResourceType).Returns("Folder");
            axMock.SetupGet(it => it.Parent).Returns(axParentMock.Object);
            axMock.Setup(it => it.Children)
                .Returns(new ObservableCollection<IExplorerItemViewModel>() { childMock.Object });

            //act
            _target.Environments.First().SelectAction(axMock.Object);

            //assert
            axParentMock.VerifySet(it => it.IsFolderChecked = true);
            _deployStatsViewerViewModel.Verify(
                it => it.Calculate(It.Is<IList<IExplorerTreeItem>>(match => !match.Any())));
        }

        [TestMethod]
        public void TestSelectActionParentServerSource()
        {
            //arrange
            var childMock = new Mock<IExplorerItemViewModel>();
            var axParentMock = new Mock<IExplorerItemViewModel>();
            var axMock = new Mock<IExplorerItemViewModel>();
            axMock.SetupGet(it => it.IsResourceChecked).Returns(true);
            axMock.SetupGet(it => it.ResourceType).Returns("DbService");
            axParentMock.SetupGet(it => it.ResourceType).Returns("ServerSource");
            axMock.SetupGet(it => it.Parent).Returns(axParentMock.Object);
            axMock.Setup(it => it.Children)
                .Returns(new ObservableCollection<IExplorerItemViewModel>() { childMock.Object });

            //act
            _target.Environments.First().SelectAction(axMock.Object);

            //assert
            axParentMock.VerifySet(it => it.IsFolderChecked = true);
            _deployStatsViewerViewModel.Verify(
                it => it.Calculate(It.Is<IList<IExplorerTreeItem>>(match => !match.Any())));
        }

        #endregion Test methods
    }
}