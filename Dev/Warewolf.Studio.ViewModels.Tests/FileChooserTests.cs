using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class FileChooserTests
    {
        #region Fields

        List<string> _attachments;
        Mock<IFileChooserModel> _modelMock;
        Mock<IFileListing> _fileListingItemMock;
        bool _closeActionExecuted;
        string _fileListingItemName;

        List<string> _changedProperties;
        FileChooser _target;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _modelMock = new Mock<IFileChooserModel>();
            _fileListingItemName = "someFileListing";
            _fileListingItemMock = new Mock<IFileListing>();
            _fileListingItemMock.SetupGet(it => it.Name).Returns(_fileListingItemName);
            _modelMock.Setup(it => it.FetchDrives()).Returns(new List<IFileListing>() { _fileListingItemMock.Object });
            _attachments = new List<string>() { _fileListingItemName };
            _closeActionExecuted = false;

            _changedProperties = new List<string>();
            _target = new FileChooser(_attachments, _modelMock.Object, () => _closeActionExecuted = true, true);
            _target.PropertyChanged += (sender, args) => { _changedProperties.Add(args.PropertyName); };
        }

        #endregion Test initialize

        #region Test commands

        [TestMethod]
        [Timeout(250)]
        public void TestCancelFileChooserCommandCanExecute()
        {
            //act
            var result = _target.CancelCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Timeout(250)]
        public void FileChooser_TestCancelCommandExecute()
        {
            //act
            _target.CancelCommand.Execute(null);

            //assert
            Assert.AreEqual(MessageBoxResult.Cancel, _target.Result);
            Assert.IsTrue(_closeActionExecuted);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSaveCommandCanExecute()
        {
            //act
            var result = _target.SaveCommand.CanExecute(null);

            //assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Timeout(1000)]
        public void TestSaveCommandExecute()
        {
            //act
            _target.SaveCommand.Execute(null);

            //assert
            Assert.AreEqual(MessageBoxResult.OK, _target.Result);
            Assert.IsTrue(_closeActionExecuted);
        }

        #endregion Test commands

        #region Test properties

        [TestMethod]
        [Timeout(100)]
        public void TestAttachments()
        {
            //arrange
            var expectedValue = new List<string>();

            //act
            _target.Attachments = expectedValue;
            var value = _target.Attachments;

            //assert
            Assert.AreSame(expectedValue, value);
        }


        [TestMethod]
        [Timeout(100)]
        public void TestAllowMultipleSelection()
        {
            //act
            _target.AllowMultipleSelection = true;

            //assert
            Assert.IsTrue(_target.AllowMultipleSelection);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSelectedDriveName()
        {
            //act
            _target.SelectedDriveName = _fileListingItemName;

            //assert
            Assert.AreEqual(_fileListingItemName, _target.SelectedDriveName);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestDrives()
        {
            //arrange
            var expectedValue = new List<FileListingModel>();

            //act
            _target.Drives = expectedValue;
            var value = _target.Drives;

            //assert
            Assert.AreSame(expectedValue, value);
        }

        [TestMethod]
        [Timeout(1000)]
        public void TestDriveName()
        {
            //arrange
            var fileListingItemFullName1 = "d1";
            var fileListingItemMock1 = new Mock<IFileListingModel>();
            fileListingItemMock1.SetupGet(it => it.FullName).Returns(fileListingItemFullName1);
            var fileListingItemFullName2 = "d2";
            var fileListingItemMock2 = new Mock<IFileListingModel>();
            fileListingItemMock2.SetupGet(it => it.FullName).Returns(fileListingItemFullName2);
            var expectedValue = fileListingItemFullName1 + ";" + fileListingItemFullName2;
            _target.Drives = new List<FileListingModel>()
                                 {
                                     new FileListingModel(
                                         _modelMock.Object,
                                         fileListingItemMock1.Object,
                                         () => { }) { IsChecked = true },
                                     new FileListingModel(
                                         _modelMock.Object,
                                         fileListingItemMock2.Object,
                                         () => { }) { IsChecked = true }
                                 };
            _changedProperties.Clear();

            //act
            _target.DriveName = "someValue";
            var value = _target.DriveName;

            //assert
            Assert.AreEqual(expectedValue, value);
            Assert.IsTrue(_changedProperties.Contains("DriveName"));
        }

        [TestMethod]
        [Timeout(250)]
        public void TestDrivesChanged()
        {
            //arrange
            _changedProperties.Clear();

            //act
            _target.Drives.First().IsChecked = true;

            //assert
            Assert.IsTrue(_changedProperties.Contains("DriveName"));
        }

        #endregion Test properties

        #region Test methods

        [TestMethod]
        [Timeout(250)]
        public void FileChooser_TestExpand()
        {
            //assert
            Assert.IsTrue(_target.Drives.First().IsChecked);
        }

        [TestMethod]
        [Timeout(250)]
        public void TestGetAttachments()
        {
            //arrange
            var fileListingItemFullName1 = "d1";
            var fileListingItemMock1 = new Mock<IFileListingModel>();
            fileListingItemMock1.SetupGet(it => it.FullName).Returns(fileListingItemFullName1);
            var fileListingItemFullName2 = "d2";
            var fileListingItemMock2 = new Mock<IFileListingModel>();
            fileListingItemMock2.SetupGet(it => it.FullName).Returns(fileListingItemFullName2);
            _target.Drives = new List<FileListingModel>()
                                 {
                                     new FileListingModel(
                                         _modelMock.Object,
                                         fileListingItemMock1.Object,
                                         () => { }) { IsChecked = true },
                                     new FileListingModel(
                                         _modelMock.Object,
                                         fileListingItemMock2.Object,
                                         () => { }) { IsChecked = true }
                                 };
            _changedProperties.Clear();

            //act
            var result = _target.GetAttachments();

            //assert
            Assert.IsTrue(result.Contains(fileListingItemFullName1));
            Assert.IsTrue(result.Contains(fileListingItemFullName2));
        }

        [TestMethod]
        [Timeout(100)]
        public void TestSelectAttachment()
        {
            //arrange
            var fileListingName = "C:\\";
            var fileChildListingName = "someName2";
            var fileGrandChildListingName = "someName2";
            var name = fileListingName + fileChildListingName + "\\" + fileGrandChildListingName;
            var fileListingMock = new Mock<IFileListingModel>();
            fileListingMock.SetupGet(it => it.Name).Returns(fileListingName);
            var fileChildListingMock = new Mock<IFileListingModel>();
            fileChildListingMock.SetupGet(it => it.Name).Returns(fileChildListingName);
            fileListingMock.SetupGet(it => it.Children)
                .Returns(new ObservableCollection<IFileListingModel>() { fileChildListingMock.Object });
            var fileGrandChildListingMock = new Mock<IFileListingModel>();
            fileGrandChildListingMock.SetupGet(it => it.Name).Returns(fileGrandChildListingName);
            fileChildListingMock.SetupGet(it => it.Children)
                .Returns(new ObservableCollection<IFileListingModel>() { fileGrandChildListingMock.Object });
            var model = new List<IFileListingModel>() { fileListingMock.Object };

            //act
            _target.SelectAttachment(name, model);

            //assert
            fileListingMock.VerifySet(it => it.IsExpanded = true);
            fileChildListingMock.VerifySet(it => it.IsExpanded = true);
            fileGrandChildListingMock.VerifySet(it => it.IsChecked = true);
        }

        [TestMethod]
        [Timeout(100)]
        public void TestCancel()
        {
            //act
            _target.Cancel();

            //assert
            Assert.AreEqual(MessageBoxResult.Cancel, _target.Result);
            Assert.IsTrue(_closeActionExecuted);
        }

        [TestMethod]
        [Timeout(250)]
        public void TestFileChooserSave()
        {
            //act
            _target.Save();

            //assert
            Assert.AreEqual(MessageBoxResult.OK, _target.Result);
            Assert.IsTrue(_closeActionExecuted);
        }

        #endregion Test methods
    }
}
