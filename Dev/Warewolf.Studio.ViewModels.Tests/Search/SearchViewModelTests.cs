using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Studio.Interfaces;
using Dev2.ViewModels.Search;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.ObjectModel;

namespace Warewolf.Studio.ViewModels.Tests.Search
{
    [TestClass]
    public class SearchViewModelTests
    {
        #region Fields

        private SearchViewModel _target;

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
            var mockEnvironmentConnection = SetupMockConnection();
            mockEnvironmentConnection.Setup(connection => connection.IsConnected).Returns(true);
            _serverMock.Setup(server => server.Connection).Returns(mockEnvironmentConnection.Object);
            _shellViewModelMock.SetupGet(it => it.LocalhostServer).Returns(_serverMock.Object);
            _eventAggregatorMock = new Mock<IEventAggregator>();

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

        #endregion Test initialize

        [TestMethod,Timeout(60000)]
        [Owner("Pieter Terblanche")]
        public void SearchViewModel_Constructor_ExpectedValues()
        {
            //------------Setup for test--------------------------

            Assert.IsNotNull(_target);
            Assert.IsNotNull(_target.ConnectControlViewModel);
            Assert.IsNotNull(_target.SelectedEnvironment);
            Assert.IsFalse(_target.IsSearching);
            Assert.AreEqual("Search", _target.DisplayName);
        }

    }
}
