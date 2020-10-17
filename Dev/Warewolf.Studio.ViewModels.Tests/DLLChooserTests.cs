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

        Mock<IDllListingModel> _modelMock;
        Mock<IManagePluginSourceModel> _updateManagerMock;
        List<string> _changedProperties;
        DLLChooser _target;

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
        [Timeout(100)]
        public void TestCancelCommandCanExecute()
        {
            //act
            var result = _target.CancelCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestCancelCommandExecute()
        {
            //act
            _target.CancelCommand.Execute(null);

            //assert
            Assert.IsNull(_target.SelectedDll);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSelectCommandCanExecute()
        {
            //act
            var result = _target.SelectCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Timeout(100)]
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
        [Timeout(100)]
        public void TestSelectedDll()
        {
            //act
            _target.SelectedDll = _modelMock.Object;

            //assert
            Assert.AreEqual(_modelMock.Object, _target.SelectedDll);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsLoading()
        {
            //act
            _target.IsLoading = true;

            //assert
            Assert.IsTrue(_target.IsLoading);
        }

        [TestMethod]
        [Timeout(100)]
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
