using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Deploy;
using Microsoft.Practices.Prism.PubSubEvents;
using Moq;
using Dev2;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass()]
    public class DeploySourceExplorerViewModelTests
    {
        #region Fields

        DeploySourceExplorerViewModel _target;
        Mock<IEnvironmentViewModel> _selectedEnvironment;
        Mock<IShellViewModel> _shellViewModelMock;
        Mock<IServer> _serverMock;
        Mock<IEventAggregator> _eventAggregatorMock;
        Mock<IDeployStatsViewerViewModel> _deployStatsViewerViewModel;
        Mock<IStudioUpdateManager> _studioUpdateManagerMock;
        Mock<IExplorerItem> _explorerItemMock;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            var explorerTooltips = new Mock<IExplorerTooltips>();
            CustomContainer.Register(explorerTooltips.Object);
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

            var mockEnvironmentConnection = SetupMockConnection();
            _serverMock.SetupGet(it => it.Connection).Returns(mockEnvironmentConnection.Object);

            _shellViewModelMock.SetupGet(it => it.LocalhostServer).Returns(_serverMock.Object);
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _deployStatsViewerViewModel = new Mock<IDeployStatsViewerViewModel>();

            var environmentRepository = new Mock<IServerRepository>();
            var environments = new List<IServer>
                {
                    _serverMock.Object
                };
            environmentRepository.Setup(e => e.All()).Returns(environments);
            CustomContainer.Register(environmentRepository.Object);

            _target = new DeploySourceExplorerViewModel(_shellViewModelMock.Object, _eventAggregatorMock.Object, _deployStatsViewerViewModel.Object, _selectedEnvironment.Object);
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

        #endregion Test initialize

        #region Test properties

        [TestMethod]
        [Timeout(2000)]
        public void DeploySourceTestEnvironments()
        {
            //arrange
            //act
            var env = _target.Environments;

            //assert
            Assert.IsNotNull(env);
        }

        [TestMethod]
        [Timeout(2000)]
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
        [Timeout(500)]
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
        [Timeout(1000)]
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
        [Timeout(1000)]
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
            var selectedItemsValue = new List<IExplorerTreeItem>() { child1.Object, child2.Object, child3.Object };

            //act
            _target.SelectedItems = selectedItemsValue;

            //assert
            child1.VerifySet(it => it.IsSelected = true);
            child2.VerifySet(it => it.IsSelected = true);
            child3.VerifySet(it => it.IsSelected = true, Times.Never);
        }

        [TestMethod]
        [Timeout(1000)]
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
        [Timeout(250)]
        public void TestDeploySourceServerVersion()
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
        [Timeout(2000)]
        public void TestSelectedEnvironmentChanged()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            var serverMock = new Mock<IServer>();
            var serverId = Guid.NewGuid();
            serverMock.SetupGet(it => it.EnvironmentID).Returns(serverId);
            serverMock.SetupGet(it => it.DisplayName).Returns("newServerName");

            var mockEnvironmentConnection = SetupMockConnection();
            serverMock.SetupGet(it => it.Connection).Returns(mockEnvironmentConnection.Object);

            environmentViewModelMock.SetupGet(it => it.IsVisible).Returns(true);
            environmentViewModelMock.SetupGet(it => it.Server).Returns(serverMock.Object);
            var env = _target.Environments.First();
            var explorerItemViewModelMock = new Mock<IExplorerItemViewModel>();
            explorerItemViewModelMock.SetupGet(it => it.IsVisible).Returns(true);
            explorerItemViewModelMock.SetupGet(it => it.ResourceType).Returns("Dev2Server");
            explorerItemViewModelMock.SetupGet(it => it.ResourceName).Returns("newServerName");
            explorerItemViewModelMock.SetupGet(it => it.ResourceId).Returns(serverId);
            explorerItemViewModelMock.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItemViewModel>());
            environmentViewModelMock
                .Setup(it => it.AsList())
                .Returns(new List<IExplorerItemViewModel>() { explorerItemViewModelMock.Object });
            env.AddChild(explorerItemViewModelMock.Object);
            env.ResourceId = serverId;
            env.Server = serverMock.Object;
            var environmentViewModels = _target.Environments.Union(new[] { environmentViewModelMock.Object }).ToList();
            _target.Environments = new ObservableCollection<IEnvironmentViewModel>(environmentViewModels);

            _shellViewModelMock.Setup(model => model.ExplorerViewModel.Environments).Returns(_target.Environments);

            //act
            _target.ConnectControlViewModel.SelectedConnection = serverMock.Object;

            //assert

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
        [Timeout(250)]
        public async Task Deploy_TestOtherServerСonnect()
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
            await _target.ConnectControlViewModel.TryConnectAsync(serverMock.Object);

            //assert
            Assert.IsTrue(isEnvironmentChanged);
            Assert.AreEqual(1, _target.Environments.Count);
        }

        [TestMethod]
        [Timeout(500)]
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
            _deployStatsViewerViewModel.Verify(it => it.TryCalculate(It.Is<IList<IExplorerTreeItem>>(list => list.Count == 1 && list.Contains(explorerItemViewModelResourceCheckedMock.Object))));
        }

        [TestMethod]
        [Timeout(150)]
        public void TestCalculateOnNullExplorerItems()
        {
            //arrange
            var explorerItemViewModelMock = new Mock<IExplorerItemViewModel>();
            var selectedEnvironmentMock = new Mock<IEnvironmentViewModel>();
            selectedEnvironmentMock.Setup(it => it.IsConnected).Returns(true);
            selectedEnvironmentMock.Setup(it => it.AsList()).Returns(new List<IExplorerItemViewModel> { explorerItemViewModelMock.Object });

            var destination = new Mock<IDeployDestinationExplorerViewModel>();
            destination.Setup(dest => dest.ConnectControlViewModel).Returns(new Mock<IConnectControlViewModel>().Object);
            destination.Setup(dest => dest.SelectedEnvironment).Returns(selectedEnvironmentMock.Object);

            var deployStatsViewerViewModel = new DeployStatsViewerViewModel(destination.Object);
            deployStatsViewerViewModel.ReCalculate();

            //assert
            Assert.AreEqual(0, deployStatsViewerViewModel.Connectors);
            Assert.AreEqual(0, deployStatsViewerViewModel.Services);
            Assert.AreEqual(0, deployStatsViewerViewModel.Sources);
            Assert.AreEqual(0, deployStatsViewerViewModel.Unknown);
        }

        [TestMethod]
        [Timeout(500)]
        public void TestSelectActionFolder()
        {
            //arrange
            var childMock = new Mock<IExplorerItemViewModel>();
            var axMock = new Mock<IExplorerItemViewModel>();
            axMock.SetupGet(it => it.IsResourceChecked).Returns(true);
            axMock.SetupGet(it => it.ResourceType).Returns("Folder");
            axMock.Setup(it => it.Children)
                .Returns(new ObservableCollection<IExplorerItemViewModel> { childMock.Object });

            //act
            _target.Environments.First().SelectAction(axMock.Object);

            //assert

            _deployStatsViewerViewModel.Verify(
                it => it.TryCalculate(It.Is<IList<IExplorerTreeItem>>(match => !match.Any())));
        }

        [TestMethod]
        [Timeout(250)]
        public void TestSelectActionParentFolder()
        {
            //arrange
            var childMock = new Mock<IExplorerItemViewModel>();
            var axParentMock = new Mock<IExplorerItemViewModel>();
            childMock.SetupGet(it => it.IsResourceChecked).Returns(true);
            childMock.SetupGet(it => it.ResourceType).Returns("DbService");
            axParentMock.SetupGet(it => it.ResourceType).Returns("Folder");
            childMock.SetupGet(it => it.Parent).Returns(axParentMock.Object);
            axParentMock.Setup(it => it.UnfilteredChildren)
                .Returns(new ObservableCollection<IExplorerItemViewModel>() { childMock.Object });

            //act
            _target.Environments.First().SelectAction(childMock.Object);

            //assert
            axParentMock.VerifySet(it => it.IsFolderChecked = true);
            _deployStatsViewerViewModel.Verify(
                it => it.TryCalculate(It.Is<IList<IExplorerTreeItem>>(match => !match.Any())));
        }

        [TestMethod]
        [Timeout(250)]
        public void TestSelectActionParentServerSource()
        {
            //arrange
            var childMock = new Mock<IExplorerItemViewModel>();
            var axParentMock = new Mock<IEnvironmentViewModel>();
            childMock.SetupGet(it => it.IsResourceChecked).Returns(true);
            childMock.SetupGet(it => it.ResourceType).Returns("DbService");
            axParentMock.SetupGet(it => it.ResourceType).Returns("ServerSource");
            childMock.SetupGet(it => it.Parent).Returns(axParentMock.Object);
            axParentMock.Setup(it => it.UnfilteredChildren)
                .Returns(new ObservableCollection<IExplorerItemViewModel>() { childMock.Object });

            //act
            _target.Environments.First().SelectAction(childMock.Object);

            //assert
            axParentMock.VerifySet(it => it.IsFolderChecked = true);
            _deployStatsViewerViewModel.Verify(
                it => it.TryCalculate(It.Is<IList<IExplorerTreeItem>>(match => !match.Any())));
        }        
        #endregion Test methods
    }
}