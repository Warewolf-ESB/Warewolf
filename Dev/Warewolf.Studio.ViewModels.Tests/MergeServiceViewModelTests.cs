using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
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

            _target = new MergeServiceViewModel(_shellViewModelMock.Object, _eventAggregatorMock.Object, "Selected Service", _mergeView.Object, _selectedEnvironment.Object);
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
