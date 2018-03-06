using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Studio.Interfaces;
using Dev2.ViewModels.Search;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(connection => connection.IsConnected).Returns(true);
            _serverMock.Setup(server => server.Connection).Returns(mockEnvironmentConnection.Object);
            _shellViewModelMock.SetupGet(it => it.LocalhostServer).Returns(_serverMock.Object);
            _eventAggregatorMock = new Mock<IEventAggregator>();

            _target = new SearchViewModel(_shellViewModelMock.Object, _eventAggregatorMock.Object);
        }

        #endregion Test initialize

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void SearchViewModel_IsAllSelected_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------

            _target.SearchValue.SearchOptions.IsAllSelected = true;

            var _wasCalled = false;
            _target.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsAllSelected")
                {
                    _wasCalled = true;
                }
            };

            Assert.IsTrue(_target.SearchValue.SearchOptions.IsAllSelected);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void SearchViewModel_IsWorkflowNameSelected_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------

            _target.SearchValue.SearchOptions.IsWorkflowNameSelected = true;

            var _wasCalled = false;
            _target.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsWorkflowNameSelected")
                {
                    _wasCalled = true;
                }
            };

            Assert.IsTrue(_target.SearchValue.SearchOptions.IsWorkflowNameSelected);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void SearchViewModel_IsToolTitleSelected_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------

            _target.SearchValue.SearchOptions.IsToolTitleSelected = true;

            var _wasCalled = false;
            _target.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsToolTitleSelected")
                {
                    _wasCalled = true;
                }
            };

            Assert.IsTrue(_target.SearchValue.SearchOptions.IsToolTitleSelected);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void SearchViewModel_IsScalarNameSelected_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------

            _target.SearchValue.SearchOptions.IsScalarNameSelected = true;

            var _wasCalled = false;
            _target.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsScalarNameSelected")
                {
                    _wasCalled = true;
                }
            };

            Assert.IsTrue(_target.SearchValue.SearchOptions.IsScalarNameSelected);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void SearchViewModel_IsObjectNameSelected_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------

            _target.SearchValue.SearchOptions.IsObjectNameSelected = true;

            var _wasCalled = false;
            _target.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsObjectNameSelected")
                {
                    _wasCalled = true;
                }
            };

            Assert.IsTrue(_target.SearchValue.SearchOptions.IsObjectNameSelected);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void SearchViewModel_IsRecSetNameSelected_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------

            _target.SearchValue.SearchOptions.IsRecSetNameSelected = true;

            var _wasCalled = false;
            _target.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsRecSetNameSelected")
                {
                    _wasCalled = true;
                }
            };

            Assert.IsTrue(_target.SearchValue.SearchOptions.IsRecSetNameSelected);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void SearchViewModel_IsInputVariableSelected_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------

            _target.SearchValue.SearchOptions.IsInputVariableSelected = true;

            var _wasCalled = false;
            _target.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsInputVariableSelected")
                {
                    _wasCalled = true;
                }
            };

            Assert.IsTrue(_target.SearchValue.SearchOptions.IsInputVariableSelected);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void SearchViewModel_IsOutputVariableSelected_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------

            _target.SearchValue.SearchOptions.IsOutputVariableSelected = true;

            var _wasCalled = false;
            _target.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsOutputVariableSelected")
                {
                    _wasCalled = true;
                }
            };

            Assert.IsTrue(_target.SearchValue.SearchOptions.IsOutputVariableSelected);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void SearchViewModel_IsTestNameSelected_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------

            _target.SearchValue.SearchOptions.IsTestNameSelected = true;

            var _wasCalled = false;
            _target.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsTestNameSelected")
                {
                    _wasCalled = true;
                }
            };

            Assert.IsTrue(_target.SearchValue.SearchOptions.IsTestNameSelected);
            Assert.IsTrue(_wasCalled);
        }

    }
}
