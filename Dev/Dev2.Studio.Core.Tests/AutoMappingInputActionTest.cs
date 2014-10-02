
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Studio.Core.Actions;
using Dev2.Studio.ViewModels.DataList.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Dev2.Studio.Core.Interfaces;
using Moq;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Core.Tests
{
    /// <summary>
    ///This is a test class for AutoMappingInputActionTest and is intended
    ///to contain all AutoMappingInputActionTest Unit Tests
    ///</summary>
    [TestClass()] 
    public class AutoMappingInputActionTest {

        AutoMappingInputAction _autoMappingInputAction;
        Mock<IDataMappingViewModel> _mockDataMappingViewModel;
        Mock<IWebActivity> _mockWebActivity;
        Mock<IInputOutputViewModel> _mockInputOutputViewModel;

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
        }

        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();

            _mockDataMappingViewModel = Dev2MockFactory.SetupIDataMappingViewModel();
            _mockWebActivity = Dev2MockFactory.SetupWebActivityMock();
            _mockInputOutputViewModel = Dev2MockFactory.SetupIInputOutputViewModel("UnitTestDataListItem", "TestValue", "", false, "", false, "InputOutputTestDisplayName", "");
        }
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        #endregion

        #region CTOR Tests

        /// <summary>
        ///A test for AutoMappingOutputAction Constructor
        ///</summary>
        [TestMethod()]
        public void AutoMappingInputActionConstructor_Expected_AutoMappingActionCreatedThatCanBeExecuted() {
         
            _autoMappingInputAction = new AutoMappingInputAction(_mockDataMappingViewModel.Object, _mockWebActivity.Object);
            Assert.IsTrue(_autoMappingInputAction.CanExecute());
        }

        #endregion CTOR Tests

        #region LoadInputMapping Tests

        /// <summary>
        ///A test for IsInitialLoadOutputAutoMapping
        ///</summary>
        [TestMethod()]
        public void IsInitialLoadInputAutoMapping_Expected_ReturnedItemThatWasPassedInForInputMapping() {        
            _autoMappingInputAction = new AutoMappingInputAction(_mockDataMappingViewModel.Object, _mockWebActivity.Object);

            IInputOutputViewModel expected = _mockInputOutputViewModel.Object;
            IInputOutputViewModel actual;
            actual = _autoMappingInputAction.LoadInputAutoMapping(_mockInputOutputViewModel.Object);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for empty Item Value for LoadInputMapping method
        ///</summary>
        [TestMethod()]
        public void LoadInputAutoMappingInputOutputViewModelScalarValue_Expected_ReturnedItemThatWasPassedInForInputMapping() {
            _autoMappingInputAction = new AutoMappingInputAction(_mockDataMappingViewModel.Object, _mockWebActivity.Object);

            IInputOutputViewModel expected = _mockInputOutputViewModel.Object;
            IInputOutputViewModel actual;
            _mockInputOutputViewModel.Setup(c => c.Value).Returns(string.Empty);

            Mock<IDataListViewModel> mockDataListViewModel = Dev2MockFactory.SetupDataListViewModel();
            mockDataListViewModel.Setup(dlVM => dlVM.DataList).Returns(ReturnDefaultDataListViewModel(mockDataListViewModel.Object, false, "InputOutputTestDisplayName"));
            mockDataListViewModel.Setup(dlVM => dlVM.ScalarCollection).Returns(ReturnDefaultDataListViewModel(mockDataListViewModel.Object, false, "InputOutputTestDisplayName"));
            mockDataListViewModel.Setup(dlVM => dlVM.RecsetCollection).Returns(new OptomizedObservableCollection<IDataListItemModel>());

            DataListSingleton.SetDataList(mockDataListViewModel.Object);

            actual = _autoMappingInputAction.LoadInputAutoMapping(_mockInputOutputViewModel.Object);
            _mockInputOutputViewModel.VerifySet(input => input.Value = It.IsAny<string>(), Times.Exactly(1), "Input View Model failed to set to DataList Representation value");
        }

        /// <summary>
        /// Test to ensure RecordSets are considered in mapping
        /// </summary>
        [TestMethod]
        public void LoadInputAutoMapping_InputOutViewModelRecordSetValue_Expected_ItemValuePopulatedWithRecordSetName() {
            _autoMappingInputAction = new AutoMappingInputAction(_mockDataMappingViewModel.Object, _mockWebActivity.Object);

            IInputOutputViewModel expected = _mockInputOutputViewModel.Object;
            IInputOutputViewModel actual;
            _mockInputOutputViewModel.Setup(c => c.Value).Returns(string.Empty);

            Mock<IDataListViewModel> mockDataListViewModel = Dev2MockFactory.SetupDataListViewModel();
            mockDataListViewModel.Setup(dlVM => dlVM.DataList).Returns(ReturnDefaultDataListViewModel(mockDataListViewModel.Object, true, "InputOutputTestDisplayName"));
            mockDataListViewModel.Setup(dlVM => dlVM.ScalarCollection).Returns(ReturnDefaultDataListViewModel(mockDataListViewModel.Object, true, "InputOutputTestDisplayName"));
            mockDataListViewModel.Setup(dlVM => dlVM.RecsetCollection).Returns(new OptomizedObservableCollection<IDataListItemModel>());

            DataListSingleton.SetDataList(mockDataListViewModel.Object);

            _mockInputOutputViewModel.SetupGet(c => c.RecordSetName).Returns("InputOutputTestDisplayName");


            actual = _autoMappingInputAction.LoadInputAutoMapping(_mockInputOutputViewModel.Object);
            _mockInputOutputViewModel.VerifySet(input => input.Value = It.IsAny<string>(), Times.Exactly(1), "Input View Model failed to set to DataList Representation value");
        }

        /// <summary>
        ///A test for NotInitialLoadOutputAutoMapping
        ///</summary>
        [TestMethod()]
        public void NotInitialLoadInputAutoMappingTest() {          
            _autoMappingInputAction = new AutoMappingInputAction(_mockDataMappingViewModel.Object, _mockWebActivity.Object);

            IInputOutputViewModel expected = _mockInputOutputViewModel.Object;
            IInputOutputViewModel actual;
            actual = _autoMappingInputAction.LoadInputAutoMapping(_mockInputOutputViewModel.Object);
            Assert.AreEqual(expected, actual);
        }

        #region Negative Test Cases

        [TestMethod]
        public void LoadInputAutoMappingInputOutputViewModelNullScalar_Expected_InputOutputViewModelValueNotSet() {
            _autoMappingInputAction = new AutoMappingInputAction(_mockDataMappingViewModel.Object, _mockWebActivity.Object);
            IInputOutputViewModel actual;
            Mock<IDataListViewModel> dataListVM = Dev2MockFactory.SetupDataListViewModel();

            dataListVM.Setup(c => c.DataList).Returns(ReturnDefaultDataListViewModel(dataListVM.Object, false));
            actual = _autoMappingInputAction.LoadInputAutoMapping(_mockInputOutputViewModel.Object);
            _mockInputOutputViewModel.VerifySet(dmVM => dmVM.Value = It.IsAny<string>(), Times.Never());
        }

        [TestMethod]
        public void LoadInputAutoMappingInputOutputViewModelNullRecordSet_Expected_InputOutputViewModelValueNotSet() {
            _autoMappingInputAction = new AutoMappingInputAction(_mockDataMappingViewModel.Object, _mockWebActivity.Object);
            IInputOutputViewModel actual;
            Mock<IDataListViewModel> dataListVM = Dev2MockFactory.SetupDataListViewModel();

            dataListVM.Setup(c => c.DataList).Returns(ReturnDefaultDataListViewModel(dataListVM.Object, false));
            actual = _autoMappingInputAction.LoadInputAutoMapping(_mockInputOutputViewModel.Object);
            _mockInputOutputViewModel.VerifySet(dmVM => dmVM.Value = It.IsAny<string>(), Times.Never());
        }


        #endregion Negative Test Cases

        #endregion LoadInputMapping Tests

        #region Test Methods

        private OptomizedObservableCollection<IDataListItemModel> ReturnDefaultDataListViewModel(IDataListViewModel dLVM, bool isRecordSet, string dataListItemName = null) {
            OptomizedObservableCollection<IDataListItemModel> CollectionObservableCollection = new OptomizedObservableCollection<IDataListItemModel>();
            Mock<IDataListItemModel> dataListItemViewModel = CreateDataListItemViewModel(dataListItemName, dLVM);
            if (isRecordSet) {

                    dataListItemViewModel.Setup(c => c.IsRecordset).Returns(true);
                    dataListItemViewModel.Setup(c => c.DisplayName).Returns("UnitTestRepresentationValue");
                    dataListItemViewModel.Setup(c => c.Children).Returns(CollectionObservableCollection);
                   
            }
            else {
                dataListItemViewModel.SetupGet(d => d.DisplayName).Returns(dataListItemName);
                

            }
            CollectionObservableCollection.Add(dataListItemViewModel.Object);

            return CollectionObservableCollection;
        }

        public Mock<IDataListItemModel> CreateDataListItemViewModel(string name, IDataListViewModel dLVM) {
            Mock<IDataListItemModel> dataListItemViewModel = Dev2MockFactory.SetupDataListItemViewModel();
            dataListItemViewModel.Setup(dLIVM => dLIVM.DisplayName).Returns(name);
            dataListItemViewModel.Setup(dLIVM => dLIVM.Name).Returns(name);
            return dataListItemViewModel;
        }

        #endregion Test Methods
    }
}
