using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Studio.Interfaces;
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
            _shellViewModelMock.Setup(model => model.ExplorerViewModel).Returns(new Mock<IExplorerViewModel>().Object);
            _shellViewModelMock.Setup(model => model.ExplorerViewModel.ConnectControlViewModel).Returns(new Mock<IConnectControlViewModel>().Object);
            _serverMock = new Mock<IServer>();
            _studioUpdateManagerMock = new Mock<IStudioUpdateManager>();
            _explorerItemMock = new Mock<IExplorerItem>();
            _explorerItemMock.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItem>());
            _serverMock.Setup(it => it.LoadExplorer(false)).ReturnsAsync(_explorerItemMock.Object);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_studioUpdateManagerMock.Object);
            _serverMock.SetupGet(it => it.DisplayName).Returns("someResName");
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(connection => connection.IsConnected).Returns(true);
            _serverMock.Setup(server => server.Connection).Returns(mockEnvironmentConnection.Object);
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

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DeployTests_GivenIsSet_ShouldFireOnPropertyChanged()
        {
            //---------------Set up test pack-------------------
            var wasCalled = false;
            _target.IsLoading = false;
            _target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "DeployTests")
                    wasCalled = true;


            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            _target.DeployTests = true;
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
        }
        #endregion Test properties
    }
}