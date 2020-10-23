using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Dev2.Studio.Interfaces;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class SingleEnvironmentExplorerViewModelTests
    {
        #region Fields

        SingleEnvironmentExplorerViewModel _target;

        Mock<IEnvironmentViewModel> _environmentViewModelMock;
        Mock<IExplorerItemViewModel> _explorerItemViewModelMock;
        Guid _selectedId;
        bool _filterByType;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _selectedId = Guid.NewGuid();
            _filterByType = true;
            _explorerItemViewModelMock = new Mock<IExplorerItemViewModel>();
            _explorerItemViewModelMock.SetupGet(it => it.ResourceName).Returns("someResName");
            _explorerItemViewModelMock.SetupGet(it => it.ResourceType).Returns("Folder");
            _environmentViewModelMock = new Mock<IEnvironmentViewModel>();
            _environmentViewModelMock.Setup(it => it.Filter(It.IsAny<Func<IExplorerItemViewModel, bool>>()))
                .Callback<Func<IExplorerItemViewModel, bool>>(arg => arg(_explorerItemViewModelMock.Object));
            _target = new SingleEnvironmentExplorerViewModel(_environmentViewModelMock.Object, _selectedId, _filterByType);
        }

        #endregion Test initialize

        #region Test properties

        [TestMethod]
        [Timeout(100)]
        public void TestSearchTextUnchanged()
        {
            //arrange
            var isSearchTextChanged = false;
            _target.PropertyChanged += (s, e) =>
            {
                isSearchTextChanged = isSearchTextChanged || e.PropertyName == "SearchText";
            };

            //act
            _target.SearchText = _target.SearchText;

            //assert
            _environmentViewModelMock.Verify(it => it.Filter(It.IsAny<Func<IExplorerItemViewModel, bool>>()));
            Assert.IsTrue(string.IsNullOrEmpty(_target.SearchText));
            Assert.IsFalse(isSearchTextChanged);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSearchText()
        {
            //arrange
            var isSearchTextChanged = false;
            _target.SearchText = "";
            _target.PropertyChanged += (s, e) =>
            {
                isSearchTextChanged = isSearchTextChanged || e.PropertyName == "SearchText";
            };
            var searchTextValue = _explorerItemViewModelMock.Object.ResourceName;

            //act
            _target.SearchText = searchTextValue;

            //assert
            _environmentViewModelMock.Verify(it => it.Filter(It.IsAny<Func<IExplorerItemViewModel, bool>>()));
            Assert.AreEqual(searchTextValue, _target.SearchText);
            Assert.IsTrue(isSearchTextChanged);
        }

        #endregion Test properties

        #region Test commands

        [TestMethod]
        [Timeout(100)]
        public void TestRefreshCommand()
        {
            //arrange
            _environmentViewModelMock.SetupGet(it => it.IsConnected).Returns(true);
            _target.SearchText = _explorerItemViewModelMock.Object.ResourceName;

            //act
            _target.RefreshCommand.Execute(null);

            //assert
            _environmentViewModelMock.Verify(it => it.LoadDialogAsync(_selectedId));
            _environmentViewModelMock.Verify(it => it.Filter(It.IsAny<Func<IExplorerItemViewModel, bool>>()));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestRefreshCommandFilterByTypeFalse()
        {
            //arrange
            _target = new SingleEnvironmentExplorerViewModel(_environmentViewModelMock.Object, _selectedId, false);
            _environmentViewModelMock.SetupGet(it => it.IsConnected).Returns(true);
            _target.SearchText = _explorerItemViewModelMock.Object.ResourceName;

            //act
            _target.RefreshCommand.Execute(null);

            //assert
            _environmentViewModelMock.Verify(it => it.LoadDialogAsync(_selectedId));
            _environmentViewModelMock.Verify(it => it.Filter(It.IsAny<Func<IExplorerItemViewModel, bool>>()));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestRefreshCommandEmptySearch()
        {
            //arrange
            _environmentViewModelMock.SetupGet(it => it.IsConnected).Returns(true);
            _target.SearchText = "";

            //act
            _target.RefreshCommand.Execute(null);

            //assert
            _environmentViewModelMock.Verify(it => it.LoadDialogAsync(_selectedId));
            _environmentViewModelMock.Verify(it => it.Filter(It.IsAny<Func<IExplorerItemViewModel, bool>>()));
        }

        #endregion Test commands
    }
}