using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Search;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Search;
using Dev2.Studio.Interfaces;
using Dev2.ViewModels.Search;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Warewolf.Studio.ViewModels.Tests.Search
{
    [TestClass]
    [DoNotParallelize]//CustomContainer.Register
    public class SearchViewModelTests
    {
        private SearchViewModel _target;

        private Mock<IShellViewModel> _shellViewModelMock;
        private Mock<IServer> _serverMock;
        private Mock<IEventAggregator> _eventAggregatorMock;
        private Mock<IStudioUpdateManager> _studioUpdateManagerMock;
        private Mock<IExplorerItem> _explorerItemMock;
        private Mock<IEnvironmentConnection> _environmentConnectonMock;
        private Mock<IPopupController> _popupControllerMock;

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
            _serverMock.Setup(it => it.GetServerVersion()).Returns("1.1.3");
            _serverMock.Setup(it => it.GetMinSupportedVersion()).Returns("1.1.2");
            _environmentConnectonMock = SetupMockConnection();
            _environmentConnectonMock.Setup(connection => connection.IsConnected).Returns(true);
            _serverMock.Setup(server => server.Connection).Returns(_environmentConnectonMock.Object);
            _shellViewModelMock.SetupGet(it => it.LocalhostServer).Returns(_serverMock.Object);
            _popupControllerMock = new Mock<IPopupController>();
            _shellViewModelMock.SetupGet(o => o.PopupProvider).Returns(_popupControllerMock.Object);
            _eventAggregatorMock = new Mock<IEventAggregator>();

            var connectControlSingleton = new Mock<Dev2.ConnectionHelpers.IConnectControlSingleton>();
            CustomContainer.Register(connectControlSingleton.Object);
            var environmentRepository = new Mock<IServerRepository>();
            CustomContainer.Register(environmentRepository.Object);
            var explorerTooltips = new Mock<IExplorerTooltips>();
            CustomContainer.Register(new Mock<IExplorerTooltips>().Object);
            CustomContainer.Register(new Mock<IPopupController>().Object);
            _target = new SearchViewModel(_shellViewModelMock.Object, _eventAggregatorMock.Object);
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
		[Timeout(1000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SearchViewModel_Constructor")]
        public void SearchViewModel_Constructor_ExpectedValues()
        {
            //------------Setup for test--------------------------

            Assert.IsNotNull(_target);
            Assert.IsNotNull(_target.ConnectControlViewModel);
            Assert.IsNotNull(_target.SelectedEnvironment);
            Assert.IsFalse(_target.IsSearching);
            Assert.AreEqual("Search", _target.DisplayName);
        }

        [TestMethod]
        [Owner("Devaji Chotaliya")]
        [TestCategory("SearchViewModel_UpdateServerCompareChanged")]
        public void SearchViewModel_WhenSelectedEnvironmentIsAlreadyConnected_SelectedEnvironmentChanged_ReturnSelectedEnvironment()
        {
            //--------------Arrange------------------------------
            const string environmentDisplayName = "someResName";

            //--------------Act----------------------------------
            _target.UpdateServerCompareChanged(new Mock<IConnectControlViewModel>().Object, Guid.Empty);

            //--------------Assert-------------------------------
            Assert.IsNotNull(_target.SelectedEnvironment);
            Assert.AreEqual(environmentDisplayName, _target.SelectedEnvironment.DisplayName);
            Assert.AreEqual(1, _target.Environments.Count);
            Assert.IsTrue(_target.CanShowResults);
            Assert.IsFalse(_target.IsSearching);
            Assert.AreEqual(string.Empty, _target.Search.SearchInput);
            Assert.AreEqual(0, _target.SearchResults.Count);
        }

        [TestMethod]
        [Owner("Devaji Chotaliya")]
        [TestCategory("SearchViewModel_UpdateServerCompareChanged")]
        public void SearchViewModel_WhenSelectedEnvironmentIsNotConnected_SelectedEnvironmentChanged_ReturnSelectedEnvironment()
        {
            //--------------Arrange------------------------------
            const string environmentDisplayName = "someResName2";
            Guid environmentId = Guid.NewGuid();

            _serverMock = new Mock<IServer>();
            var studioUpdateManagerMock = new Mock<IStudioUpdateManager>();
            _serverMock.Setup(server => server.LoadExplorer(false)).ReturnsAsync(_explorerItemMock.Object);
            _serverMock.SetupGet(server => server.UpdateRepository).Returns(_studioUpdateManagerMock.Object);
            _serverMock.SetupGet(server => server.DisplayName).Returns(environmentDisplayName);
            _serverMock.Setup(it => it.GetServerVersion()).Returns("1.1.2");
            _environmentConnectonMock = SetupMockConnection();
            _environmentConnectonMock.Setup(connection => connection.IsConnected).Returns(true);
            _serverMock.Setup(server => server.Connection).Returns(_environmentConnectonMock.Object);
            _serverMock.Setup(server => server.EnvironmentID).Returns(environmentId);

            var _connectControlViewModel = new Mock<IConnectControlViewModel>();
            _connectControlViewModel.Setup(connectControl => connectControl.SelectedConnection).Returns(_serverMock.Object);

            //--------------Act----------------------------------
            _target.UpdateServerCompareChanged(_connectControlViewModel.Object, environmentId);

            //--------------Assert-------------------------------
            Assert.IsNotNull(_target.SelectedEnvironment);
            Assert.AreEqual(environmentDisplayName, _target.SelectedEnvironment.DisplayName);
            Assert.AreEqual(2, _target.Environments.Count);
            Assert.IsTrue(_target.CanShowResults);
            Assert.IsFalse(_target.IsSearching);
            Assert.AreEqual(string.Empty, _target.Search.SearchInput);
            Assert.AreEqual(0, _target.SearchResults.Count);
        }


        [TestMethod]
        [Owner("Devaji Chotaliya")]
        [TestCategory("SearchViewModel_UpdateServerCompareChanged")]
        public void SearchViewModel_WhenSelectedEnvironmentIsNotConnected_SelectedEnvironmentChanged_ShowServerVersionConflictPopup_UserSelectedOK_ReturnSearchResultTrue()
        {
            //--------------Arrange------------------------------
            const string environmentDisplayName = "someResName2";
            Guid environmentId = Guid.NewGuid();

            _serverMock = new Mock<IServer>();
            var studioUpdateManagerMock = new Mock<IStudioUpdateManager>();
            _serverMock.Setup(server => server.LoadExplorer(false)).ReturnsAsync(_explorerItemMock.Object);
            _serverMock.SetupGet(server => server.UpdateRepository).Returns(_studioUpdateManagerMock.Object);
            _serverMock.SetupGet(server => server.DisplayName).Returns(environmentDisplayName);
            _serverMock.Setup(it => it.GetServerVersion()).Returns("1.1.0");
            _environmentConnectonMock = SetupMockConnection();
            _environmentConnectonMock.Setup(connection => connection.IsConnected).Returns(true);
            _serverMock.Setup(server => server.Connection).Returns(_environmentConnectonMock.Object);
            _serverMock.Setup(server => server.EnvironmentID).Returns(environmentId);
            _popupControllerMock.Setup(popup => popup.ShowSearchServerVersionConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.OK);

            var _connectControlViewModel = new Mock<IConnectControlViewModel>();
            _connectControlViewModel.Setup(connectControl => connectControl.SelectedConnection).Returns(_serverMock.Object);

            //--------------Act----------------------------------
            _target.UpdateServerCompareChanged(_connectControlViewModel.Object, environmentId);

            //--------------Assert-------------------------------
            Assert.IsNotNull(_target.SelectedEnvironment);
            Assert.AreEqual(environmentDisplayName, _target.SelectedEnvironment.DisplayName);
            Assert.AreEqual(2, _target.Environments.Count);
            Assert.IsTrue(_target.CanShowResults);
            Assert.IsFalse(_target.IsSearching);
            Assert.AreEqual(string.Empty, _target.Search.SearchInput);
            Assert.AreEqual(0, _target.SearchResults.Count);
        }

        [TestMethod]
        [Owner("Devaji Chotaliya")]
        [TestCategory("SearchViewModel_UpdateServerCompareChanged")]
        public void SearchViewModel_WhenSelectedEnvironmentIsNotConnected_SelectedEnvironmentChanged_ShowServerVersionConflictPopup_UserSelectedCancel_ReturnSearchResultFalse()
        {
            //--------------Arrange------------------------------
            const string environmentDisplayName = "someResName2";
            Guid environmentId = Guid.NewGuid();

            _serverMock = new Mock<IServer>();
            var studioUpdateManagerMock = new Mock<IStudioUpdateManager>();
            _serverMock.Setup(server => server.LoadExplorer(false)).ReturnsAsync(_explorerItemMock.Object);
            _serverMock.SetupGet(server => server.UpdateRepository).Returns(_studioUpdateManagerMock.Object);
            _serverMock.SetupGet(server => server.DisplayName).Returns(environmentDisplayName);
            _serverMock.Setup(it => it.GetServerVersion()).Returns("1.1.0");
            _environmentConnectonMock = SetupMockConnection();
            _environmentConnectonMock.Setup(connection => connection.IsConnected).Returns(true);
            _serverMock.Setup(server => server.Connection).Returns(_environmentConnectonMock.Object);
            _serverMock.Setup(server => server.EnvironmentID).Returns(environmentId);
            _popupControllerMock.Setup(popup => popup.ShowSearchServerVersionConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.Cancel);

            var _connectControlViewModel = new Mock<IConnectControlViewModel>();
            _connectControlViewModel.Setup(connectControl => connectControl.SelectedConnection).Returns(_serverMock.Object);

            //--------------Act----------------------------------
            _target.UpdateServerCompareChanged(_connectControlViewModel.Object, environmentId);

            //--------------Assert-------------------------------
            Assert.IsNotNull(_target.SelectedEnvironment);
            Assert.AreEqual(environmentDisplayName, _target.SelectedEnvironment.DisplayName);
            Assert.AreEqual(2, _target.Environments.Count);
            Assert.IsFalse(_target.CanShowResults);
            Assert.IsFalse(_target.IsSearching);
            Assert.AreEqual(string.Empty, _target.Search.SearchInput);
            Assert.AreEqual(0, _target.SearchResults.Count);
        }

        [TestMethod]
        [Owner("Devaji Chotaliya")]
        [TestCategory("SearchViewModel_SearchWarewolf")]
        public void SearchViewModel_SearchWarewolf_GivenSearchInputEmpty_ReturnSearchResult()
        {
            //--------------Arrange------------------------------
            _target.Search.SearchInput = string.Empty;

            //--------------Act----------------------------------
            _target.SearchWarewolf();
            //--------------Assert-------------------------------
            Assert.AreEqual(0, _target.SearchResults.Count);
            Assert.IsFalse(_target.IsSearching);
        }

        [TestMethod]
        [Owner("Devaji Chotaliya")]
        [TestCategory("SearchViewModel_SearchWarewolf")]
        public void SearchViewModel_SearchWarewolf_GivenSearchInputIsNotEmpty_ReturnSearchResult()
        {
            //--------------Arrange------------------------------
            _target.Search.SearchInput = "Error";

            var searchResults = new List<ISearchResult>()
            {
                new SearchResult(Guid.NewGuid(), "Error","Error", SearchItemType.ToolTitle,string.Empty),
                new SearchResult(Guid.NewGuid(), "Control Flow - Decision","Control Flow - Decision", SearchItemType.ToolTitle,string.Empty),
                new SearchResult(Guid.NewGuid(), "RunBackup","RunBackup", SearchItemType.ToolTitle,string.Empty),
                new SearchResult(Guid.NewGuid(), "Error","Error", SearchItemType.SourceName,string.Empty),
            };

            var _resourceRepositoryMock = new Mock<IResourceRepository>();
            _resourceRepositoryMock.Setup(a => a.Filter(It.IsAny<ISearch>())).Returns(searchResults);

            _serverMock.Setup(server => server.ResourceRepository).Returns(_resourceRepositoryMock.Object);

            //--------------Act----------------------------------
            _target.SearchWarewolf();
            //--------------Assert-------------------------------
            Assert.AreEqual(4, _target.SearchResults.Count);
            Assert.IsFalse(_target.IsSearching);
        }
    }
}
