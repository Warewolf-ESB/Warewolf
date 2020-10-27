using System.Collections.Generic;
using System.Windows;

using FontAwesome.WPF;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class MessageBoxViewModelTests
    {
        #region Fields

        string _message;
        string _title;
        MessageBoxButton _buttons;
        FontAwesomeIcon _icon;
        bool _isDependenciesButtonVisible;
        bool _isError;
        bool _isInfo;
        bool _isQuestion;
        List<string> _duplicates;
        bool _isDeleteAnywayButtonVisible;
        bool _applyToAll;

        List<string> _changedProperties;
        MessageBoxViewModel _target;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _message = "someMessage";
            _title = "someTitle";
            _buttons = MessageBoxButton.YesNoCancel;
            _icon = FontAwesomeIcon.Navicon;
            _isDependenciesButtonVisible = true;
            _isError = false;
            _isInfo = true;
            _isQuestion = false;
            _duplicates = new List<string>();
            _isDeleteAnywayButtonVisible = false;
            _applyToAll = false;
            _changedProperties = new List<string>();
            _target = new MessageBoxViewModel(
                _message,
                _title,
                _buttons,
                _icon,
                _isDependenciesButtonVisible,
                _isError,
                _isInfo,
                _isQuestion,
                _duplicates,
                _isDeleteAnywayButtonVisible,
                _applyToAll);
            _target.PropertyChanged += (sender, args) => { _changedProperties.Add(args.PropertyName); };
        }

        #endregion Test initialize

        #region Test properties

        [TestMethod]
        [Timeout(100)]
        public void TestTitle()
        {
            //arrange
            var expectedValue = "someTitle";
            _changedProperties.Clear();

            //act
            _target.Title = expectedValue;
            var value = _target.Title;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("Title"));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsError()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.IsError = expectedValue;
            var value = _target.IsError;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("IsError"));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsInfo()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.IsInfo = expectedValue;
            var value = _target.IsInfo;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("IsInfo"));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsQuestion()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.IsQuestion = expectedValue;
            var value = _target.IsQuestion;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("IsQuestion"));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsDuplicatesVisible()
        {
            //arrange
            var expectedValue = true;
            _changedProperties.Clear();

            //act
            _target.IsDuplicatesVisible = expectedValue;
            var value = _target.IsDuplicatesVisible;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("IsDuplicatesVisible"));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestUrlsFound()
        {
            //arrange
            var duplicateList = new List<string> { "test1", "test2" };
            
            _changedProperties.Clear();

            //act
            _target.UrlsFound = duplicateList;
            var value = _target.UrlsFound;

            //asert
            Assert.AreEqual(duplicateList, value);
            Assert.IsTrue(_changedProperties.Contains("UrlsFound"));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestButtons()
        {
            //arrange
            var expectedValue = MessageBoxButton.YesNo;
            _changedProperties.Clear();

            //act
            _target.Buttons = expectedValue;
            var value = _target.Buttons;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("IsNoButtonVisible"));
            Assert.IsTrue(_changedProperties.Contains("IsYesButtonVisible"));
            Assert.IsTrue(_changedProperties.Contains("IsCancelButtonVisible"));
            Assert.IsTrue(_changedProperties.Contains("IsOkButtonVisible"));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestMessage()
        {
            //arrange
            var expectedValue = "someMessage";
            _changedProperties.Clear();

            //act
            _target.Message = expectedValue;
            var value = _target.Message;

            //asert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("Message"));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsDependenciesButtonVisible()
        {
            //arrange
            var expectedValue = true;

            //act
            _target.IsDependenciesButtonVisible = expectedValue;
            var value = _target.IsDependenciesButtonVisible;

            //asert
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsDeleteAnywayButtonVisible()
        {
            //arrange
            var expectedValue = true;

            //act
            _target.IsDeleteAnywayButtonVisible = expectedValue;
            var value = _target.IsDeleteAnywayButtonVisible;

            //asert
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsApplyToAllCheckboxVisible()
        {
            //arrange
            var expectedValue = true;

            //act
            _target.ApplyToAll = expectedValue;
            var value = _target.ApplyToAll;

            //asert
            Assert.AreEqual(expectedValue, value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsNoButtonVisibleYesNo()
        {
            //arrange
            _target.Buttons = MessageBoxButton.YesNo;

            //act
            var value = _target.IsNoButtonVisible;

            //asert
            Assert.IsTrue(value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsNoButtonVisibleYesNoCancel()
        {
            //arrange
            _target.Buttons = MessageBoxButton.YesNoCancel;

            //act
            var value = _target.IsNoButtonVisible;

            //asert
            Assert.IsTrue(value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsNoButtonVisibleOKCancel()
        {
            //arrange
            _target.Buttons = MessageBoxButton.OKCancel;

            //act
            var value = _target.IsNoButtonVisible;

            //asert
            Assert.IsFalse(value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsYesButtonVisibleYesNo()
        {
            //arrange
            _target.Buttons = MessageBoxButton.YesNo;

            //act
            var value = _target.IsYesButtonVisible;

            //asert
            Assert.IsTrue(value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsYesButtonVisibleYesNoCancel()
        {
            //arrange
            _target.Buttons = MessageBoxButton.YesNoCancel;

            //act
            var value = _target.IsYesButtonVisible;

            //asert
            Assert.IsTrue(value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsYesButtonVisibleOKCancel()
        {
            //arrange
            _target.Buttons = MessageBoxButton.OKCancel;

            //act
            var value = _target.IsYesButtonVisible;

            //asert
            Assert.IsFalse(value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsCancelButtonVisibleYesNo()
        {
            //arrange
            _target.Buttons = MessageBoxButton.YesNo;

            //act
            var value = _target.IsCancelButtonVisible;

            //asert
            Assert.IsFalse(value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsCancelButtonVisibleYesNoCancel()
        {
            //arrange
            _target.Buttons = MessageBoxButton.YesNoCancel;

            //act
            var value = _target.IsCancelButtonVisible;

            //asert
            Assert.IsTrue(value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsCancelButtonVisibleOKCancel()
        {
            //arrange
            _target.Buttons = MessageBoxButton.OKCancel;

            //act
            var value = _target.IsCancelButtonVisible;

            //asert
            Assert.IsTrue(value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsOkButtonVisibleYesNo()
        {
            //arrange
            _target.Buttons = MessageBoxButton.YesNo;

            //act
            var value = _target.IsOkButtonVisible;

            //asert
            Assert.IsFalse(value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsOkButtonVisibleOK()
        {
            //arrange
            _target.Buttons = MessageBoxButton.OK;

            //act
            var value = _target.IsOkButtonVisible;

            //asert
            Assert.IsTrue(value);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestIsOkButtonVisibleOKCancel()
        {
            //arrange
            _target.Buttons = MessageBoxButton.OKCancel;

            //act
            var value = _target.IsOkButtonVisible;

            //asert
            Assert.IsTrue(value);
        }

        #endregion Test properties

        #region Test properties

        [TestMethod]
        [Timeout(100)]
        public void TestYes()
        {
            //act
            _target.Yes();

            //assert
            Assert.AreEqual(MessageBoxResult.Yes, _target.Result);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestNo()
        {
            //act
            _target.No();

            //assert
            Assert.AreEqual(MessageBoxResult.No, _target.Result);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestCancel()
        {
            //act
            _target.Cancel();

            //assert
            Assert.AreEqual(MessageBoxResult.Cancel, _target.Result);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestOk()
        {
            //act
            _target.Ok();

            //assert
            Assert.AreEqual(MessageBoxResult.OK, _target.Result);
        }

        #endregion Test properties
    }
}