using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
    [TestClass]
    public class DeployDestinationViewModelTests
    {
        #region Fields

        private DeployDestinationViewModel _target;

        private Mock<IShellViewModel> _shellViewModelMock;
        private Mock<IServer> _serverMock;
        private Mock<IEventAggregator> _eventAggregatorMock;
        private Mock<IStudioUpdateManager> _studioUpdateManagerMock;
        private Mock<IExplorerItem> _explorerItemMock;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _shellViewModelMock = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();
            _studioUpdateManagerMock = new Mock<IStudioUpdateManager>();
            _explorerItemMock = new Mock<IExplorerItem>();
            _explorerItemMock.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItem>());
            _serverMock.Setup(it => it.LoadExplorer(false)).ReturnsAsync(_explorerItemMock.Object);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_studioUpdateManagerMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("someResName");
            _shellViewModelMock.SetupGet(it => it.LocalhostServer).Returns(_serverMock.Object);
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _target = new DeployDestinationViewModel(_shellViewModelMock.Object, _eventAggregatorMock.Object);
        }

        #endregion Test initialize

        #region Test properties

        [TestMethod]
        public void TestMinSupportedVersion()
        {
            //arrange
            var selectedEnvironmentMock = new Mock<IEnvironmentViewModel>();
            var serverMock = new Mock<IServer>();
            var version = "4.0.3";
            serverMock.Setup(it => it.GetMinSupportedVersion()).Returns(version);
            selectedEnvironmentMock.SetupGet(it => it.Server).Returns(serverMock.Object);
            _target.SelectedEnvironment = selectedEnvironmentMock.Object;

            //act
            var actual = _target.MinSupportedVersion;

            //assert
            Assert.AreEqual(Version.Parse(version), actual);
        }

        [TestMethod]
        public void TestServerVersion()
        {
            //arrange
            var selectedEnvironmentMock = new Mock<IEnvironmentViewModel>();
            var serverMock = new Mock<IServer>();
            var version = "4.0.3";
            serverMock.Setup(it => it.GetServerVersion()).Returns(version);
            selectedEnvironmentMock.SetupGet(it => it.Server).Returns(serverMock.Object);
            _target.SelectedEnvironment = selectedEnvironmentMock.Object;

            //act
            var actual = _target.ServerVersion;

            //assert
            Assert.AreEqual(Version.Parse(version), actual);
        }

        [TestMethod]
        public void TestIsLoading()
        {
            //arrange
            var isIsLoadingChanged = false;
            _target.IsLoading = false;
            _target.PropertyChanged += (s, e) =>
            {
                isIsLoadingChanged = isIsLoadingChanged || e.PropertyName == "IsLoading";
            };

            //act
            _target.IsLoading = !_target.IsLoading;

            //assert
            Assert.IsTrue(_target.IsLoading);
            Assert.IsTrue(isIsLoadingChanged);
        }
        
        #endregion Test properties

        #region Test methods

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
            Assert.AreSame(_target.SelectedEnvironment, _target.Environments.FirstOrDefault());
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
            var explorerMock = new Mock<IExplorerItem>();
            serverMock.Setup(it => it.LoadExplorer(false)).ReturnsAsync(explorerMock.Object);
            var isSelectedItemChangedRaised = false;
            _target.ServerStateChanged += (sender, server) =>
            {
                isSelectedItemChangedRaised = ReferenceEquals(_target, sender) &&
                                              ReferenceEquals(server, serverMock.Object);
            };
            var statsAreaMock = new Mock<IDeployStatsViewerViewModel>();
            _target.StatsArea = statsAreaMock.Object;

            //act
            await _target.ConnectControlViewModel.Connect(serverMock.Object);

            //assert
            Assert.IsTrue(isSelectedItemChangedRaised);
            statsAreaMock.Verify(it => it.ReCalculate());
            Assert.IsTrue(isEnvironmentChanged);
            Assert.AreEqual(2, _target.Environments.Count);
            Assert.AreSame(_target.SelectedEnvironment,
                _target.Environments.FirstOrDefault(it => it.Server.EnvironmentID == envId));
        }

        [TestMethod]
        public void TestSelectedEnvironmentChanged()
        {
            //arrange
            var environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            var serverMock = new Mock<IServer>();
            var serverId = Guid.NewGuid();
            serverMock.SetupGet(it => it.EnvironmentID).Returns(serverId);
            serverMock.SetupGet(it => it.ResourceName).Returns("someName");
            environmentViewModelMock.SetupGet(it => it.IsVisible).Returns(true);
            environmentViewModelMock.SetupGet(it => it.Server).Returns(serverMock.Object);
            var env = _target.Environments.First();
            var explorerItemViewModelMock = new Mock<IExplorerItemViewModel>();
            explorerItemViewModelMock.SetupGet(it => it.IsVisible).Returns(true);
            env.AddChild(explorerItemViewModelMock.Object);
            var statsAreaMock = new Mock<IDeployStatsViewerViewModel>();
            _target.StatsArea = statsAreaMock.Object;
            _target.Environments = _target.Environments.Union(new[] { environmentViewModelMock.Object }).ToList();

            //act
            _target.ConnectControlViewModel.SelectedConnection = serverMock.Object;

            //assert
            Assert.AreSame(environmentViewModelMock.Object, _target.SelectedEnvironment);
            statsAreaMock.Verify(it=>it.ReCalculate());
        }

        #endregion Test methods

    }
}