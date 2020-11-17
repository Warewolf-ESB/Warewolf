using System;
using System.Collections.Generic;
using System.Windows.Media;

using Dev2.Common.Interfaces.Help;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace Warewolf.Studio.ViewModels.Help.Tests
{
    [TestClass]
    public class HelpWindowViewModelTests
    {
        #region Fields

        Mock<IHelpDescriptorViewModel> _defaultViewModelMock;
        Mock<IHelpWindowModel> _modelMock;

        List<string> _changedProperties;

        HelpWindowViewModel _target;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _defaultViewModelMock = new Mock<IHelpDescriptorViewModel>();
            _modelMock = new Mock<IHelpWindowModel>();
            _target = new HelpWindowViewModel(_defaultViewModelMock.Object, _modelMock.Object);
            _changedProperties = new List<string>();
            _target.PropertyChanged += _target_PropertyChanged;
        }

        #endregion Test initialize

        #region Test construction

        [TestMethod]
        [Timeout(500)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestHelpWindowViewModelNullDefaultViewModel()
        {
            new HelpWindowViewModel(null, _modelMock.Object);
        }

        [TestMethod]
        [Timeout(500)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestHelpWindowViewModelNullModel()
        {
            new HelpWindowViewModel(_defaultViewModelMock.Object, null);
        }

        #endregion Test construction

        #region Test properties

        [TestMethod]
        [Timeout(100)]
        public void TestHelpText()
        {
            //arrange
            var expectedValue = "Some description";
            _defaultViewModelMock.SetupGet(it => it.Description).Returns(expectedValue);

            //act
            var value = _target.HelpText;

            //assert
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestHelpName()
        {
            //arrange
            var expectedValue = "Some help name";
            _defaultViewModelMock.SetupGet(it => it.Name).Returns(expectedValue);

            //act
            var value = _target.HelpName;

            //assert
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestHelpImage()
        {
            //arrange
            var expectedValue = new DrawingImage();
            _defaultViewModelMock.SetupGet(it => it.Icon).Returns(expectedValue);

            //act
            var value = _target.HelpImage;

            //assert
            Assert.AreSame(expectedValue, value);
        }

        [TestMethod]
        [Timeout(1000)]
        public void TestHelpWindowViewModel()
        {
            //act
            var value = _target.HelpModel;

            //assert
            Assert.AreSame(_modelMock.Object, value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestCurrentHelpText()
        {
            //arrange
            var expectedValueMock = new Mock<IHelpDescriptorViewModel>();
            _changedProperties.Clear();

            //act
            _target.CurrentHelpText = expectedValueMock.Object;
            var value = _target.CurrentHelpText;

            //assert
            Assert.IsTrue(_changedProperties.Contains("HelpName"));
            Assert.IsTrue(_changedProperties.Contains("HelpText"));
            Assert.IsTrue(_changedProperties.Contains("HelpImage"));
            Assert.AreSame(expectedValueMock.Object, value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestWebPageVisible()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.WebPageVisible = expectedValue;
            var value = _target.WebPageVisible;

            //assert
            Assert.IsTrue(_changedProperties.Contains("WebPageVisible"));
            Assert.AreEqual(expectedValue, value);
        }

        #endregion Test properties

        #region Test methods

        [TestMethod]
        [Timeout(500)]
        public void TestUpdateHelpTextEmpty()
        {
            //act
            _target.UpdateHelpText(string.Empty);

            //assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(_target.HelpText));
        }

        [TestMethod]
        [Timeout(500)]
        public void TestUpdateHelpTextNonEmpty()
        {
            //act
            _target.UpdateHelpText("someText");

            //assert
            Assert.IsFalse(string.IsNullOrWhiteSpace(_target.HelpText));
            Assert.IsTrue(_target.HelpText.Contains("someText"));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestOnHelpTextReceived()
        {
            //arrange
            var helpDescriptorMock = new Mock<IHelpDescriptor>();
            var expectedName = "someName";
            helpDescriptorMock.SetupGet(it => it.Name).Returns(expectedName);

            //act
            _modelMock.Raise(it => it.OnHelpTextReceived += null, _target, helpDescriptorMock.Object);

            //assert
            Assert.IsInstanceOfType(_target.CurrentHelpText, typeof(HelpDescriptorViewModel));
            Assert.AreEqual(expectedName, _target.CurrentHelpText.Name);
        }

        [TestMethod]
        [Timeout(100)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestOnHelpTextReceivedException()
        {
            //act
            _modelMock.Raise(it => it.OnHelpTextReceived += null, _target, null);

            //assert
            Assert.AreSame(_defaultViewModelMock.Object, _target.CurrentHelpText);
        }

        [TestMethod]
        [Timeout(250)]
        public void HelpWindowViewModel_TestDispose()
        {
            //arrange
            var helpDescriptorMock = new Mock<IHelpDescriptor>();
            var expectedName = "someName";
            helpDescriptorMock.SetupGet(it => it.Name).Returns(expectedName);

            //act
            _target.Dispose();

            //assert
            _modelMock.Raise(it => it.OnHelpTextReceived += null, _target, helpDescriptorMock.Object);
            Assert.AreSame(_defaultViewModelMock.Object, _target.CurrentHelpText);
        }

        #endregion Test methods

        #region Private helper methods

        void _target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _changedProperties.Add(e.PropertyName);
        }

        #endregion Private helper methods
    }
}