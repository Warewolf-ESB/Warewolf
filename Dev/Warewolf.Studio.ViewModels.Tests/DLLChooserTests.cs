using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class DLLChooserTests
    {
        #region Fields

        private Mock<IDllListingModel> _modelMock;
        private Mock<IManagePluginSourceModel> _updateManagerMock;
        private List<string> _changedProperties;
        private DLLChooser _target;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _modelMock = new Mock<IDllListingModel>();
            _updateManagerMock = new Mock<IManagePluginSourceModel>();

            _modelMock.Setup(model => model.Name).Returns("dllName");
            _modelMock.Setup(model => model.FullName).Returns("dllFullName");

            _changedProperties = new List<string>();
            _target = new DLLChooser(_updateManagerMock.Object);
            _target.PropertyChanged += (sender, args) => { _changedProperties.Add(args.PropertyName); };
        }

        #endregion Test initialize

        #region Test commands

        [TestMethod]
        public void TestCancelCommandCanExecute()
        {
            //act
            var result = _target.CancelCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestCancelCommandExecute()
        {
            //act
            _target.CancelCommand.Execute(null);

            //assert
            Assert.IsNull(_target.SelectedDll);
        }

        [TestMethod]
        public void TestSelectCommandCanExecute()
        {
            //act
            var result = _target.SelectCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestSaveCommandExecute()
        {
            //act
            _target.SelectedDll = _modelMock.Object;
            _target.SelectCommand.Execute(null);

            //assert
            Assert.AreEqual(_modelMock.Object, _target.SelectedDll);
        }

        #endregion Test commands

        #region Test properties

        [TestMethod]
        public void TestSelectedDll()
        {
            //act
            _target.SelectedDll = _modelMock.Object;

            //assert
            Assert.AreEqual(_modelMock.Object, _target.SelectedDll);
        }

        [TestMethod]
        public void TestIsLoading()
        {
            //act
            _target.IsLoading = true;

            //assert
            Assert.IsTrue(_target.IsLoading);
        }

        [TestMethod]
        public void TestSearchTerm()
        {
            //act
            var expectedValue = "textFilter";
            _target.SearchTerm = expectedValue;

            //assert
            Assert.AreEqual(expectedValue, _target.SearchTerm);
        }

        #endregion Test properties
    }
}
