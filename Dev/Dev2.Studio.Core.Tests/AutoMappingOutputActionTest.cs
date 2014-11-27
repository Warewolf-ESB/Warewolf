
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
    ///This is a test class for AutoMappingOutputActionTest and is intended
    ///to contain all AutoMappingOutputActionTest Unit Tests 
    ///</summary>
    [TestClass()]
    public class AutoMappingOutputActionTest {

        #region Local Test Variables

        Mock<IDataMappingViewModel> _mockDataMappingViewModel = Dev2MockFactory.SetupIDataMappingViewModel();
        Mock<IWebActivity> _mockWebActivity = Dev2MockFactory.SetupWebActivityMock();
        Mock<IInputOutputViewModel> _mockInputOutputViewModel = Dev2MockFactory.SetupIInputOutputViewModel("UnitTestDataListItem", "TestValue", "", false, "", false, "InputOutputTestDisplayName", "");
        AutoMappingOutputAction _autoMappingOutputAction;
        private static ImportServiceContext _importServiceContext;

        #endregion Local Test Variables


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
            _importServiceContext = CompositionInitializer.InitializeMockedMainViewModel();
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
            ImportService.CurrentContext = _importServiceContext;
        }
        
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        #region CTOR Tests

        /// <summary>
        ///A test for AutoMappingOutputAction Constructor
        ///</summary>
        [TestMethod()]
        public void AutoMappingOutputActionConstructorTest() {                      
            _autoMappingOutputAction = new AutoMappingOutputAction(_mockDataMappingViewModel.Object, _mockWebActivity.Object);
            
        }

        #endregion CTOR Tests

        #region LoadOutputMapping Tests

        /// <summary>
        ///A test for IsInitialLoadOutputAutoMapping
        ///</summary>
        [TestMethod()]
        public void IsInitialLoadOutputAutoMappingTest() {        
            _autoMappingOutputAction = new AutoMappingOutputAction(_mockDataMappingViewModel.Object, _mockWebActivity.Object);

            IInputOutputViewModel expected = _mockInputOutputViewModel.Object;
            IInputOutputViewModel actual;
            actual = _autoMappingOutputAction.LoadOutputAutoMapping(_mockInputOutputViewModel.Object);
            Assert.AreEqual(expected, actual);            
        }

        /// <summary>
        ///A test for NotInitialLoadOutputAutoMapping
        ///</summary>
        [TestMethod()]
        public void NotInitialLoadOutputAutoMappingTest() {       
            _autoMappingOutputAction = new AutoMappingOutputAction(_mockDataMappingViewModel.Object, _mockWebActivity.Object);

            IInputOutputViewModel expected = _mockInputOutputViewModel.Object;
            IInputOutputViewModel actual;
            actual = _autoMappingOutputAction.LoadOutputAutoMapping(_mockInputOutputViewModel.Object);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for empty Item Value for LoadOutputMapping method
        ///</summary>
        [TestMethod()]
        public void LoadOutputAutoMappingInputOutputViewModelScalarValue_Expected_ReturnedItemThatWasPassedInForInputMapping() {
            _autoMappingOutputAction = new AutoMappingOutputAction(_mockDataMappingViewModel.Object, _mockWebActivity.Object);

            IInputOutputViewModel expected = _mockInputOutputViewModel.Object;
            IInputOutputViewModel actual;
            _mockInputOutputViewModel.Setup(c => c.Value).Returns(string.Empty);

            Mock<IDataListViewModel> mockDataListViewModel = Dev2MockFactory.SetupDataListViewModel();
            mockDataListViewModel.Setup(dlVM => dlVM.DataList).Returns(ReturnDefaultDataListViewModel(mockDataListViewModel.Object, false, "InputOutputTestDisplayName"));           
            mockDataListViewModel.Setup(dlVM => dlVM.ScalarCollection).Returns(ReturnDefaultDataListViewModel(mockDataListViewModel.Object, false, "InputOutputTestDisplayName"));
            mockDataListViewModel.Setup(dlVM => dlVM.RecsetCollection).Returns(new OptomizedObservableCollection<IDataListItemModel>());

            DataListSingleton.SetDataList(mockDataListViewModel.Object);

            actual = _autoMappingOutputAction.LoadOutputAutoMapping(_mockInputOutputViewModel.Object);
            _mockInputOutputViewModel.VerifySet(input => input.Value = It.IsAny<string>(), Times.Exactly(1), "Input View Model failed to set to DataList Representation value");
        }

        /// <summary>
        /// Test to ensure RecordSets are considered in mapping
        /// </summary>

        [TestMethod]
        public void LoadOutputAutoMapping_InputOutViewModelRecordSetValue_Expected_ItemValuePopulatedWithRecordSetName() {
            _autoMappingOutputAction = new AutoMappingOutputAction(_mockDataMappingViewModel.Object, _mockWebActivity.Object);

            IInputOutputViewModel expected = _mockInputOutputViewModel.Object;
            IInputOutputViewModel actual;
            _mockInputOutputViewModel.Setup(c => c.Value).Returns(string.Empty);

            Mock<IDataListViewModel> mockDataListViewModel = Dev2MockFactory.SetupDataListViewModel();
            mockDataListViewModel.Setup(dlVM => dlVM.DataList).Returns(ReturnDefaultDataListViewModel(mockDataListViewModel.Object, true, "InputOutputTestDisplayName"));            
            mockDataListViewModel.Setup(dlVM => dlVM.ScalarCollection).Returns(ReturnDefaultDataListViewModel(mockDataListViewModel.Object, true, "InputOutputTestDisplayName"));
            mockDataListViewModel.Setup(dlVM => dlVM.RecsetCollection).Returns(new OptomizedObservableCollection<IDataListItemModel>());

            DataListSingleton.SetDataList(mockDataListViewModel.Object);

            _mockInputOutputViewModel.SetupGet(c => c.RecordSetName).Returns("InputOutputTestDisplayName");


            actual = _autoMappingOutputAction.LoadOutputAutoMapping(_mockInputOutputViewModel.Object);
            _mockInputOutputViewModel.VerifySet(input => input.Value = It.IsAny<string>(), Times.Exactly(1), "Input View Model failed to set to DataList Representation value");
        }

        #region Negative Test Cases

        [TestMethod]
        public void LoadOutputAutoMappingInputOutputViewModelNullScalar_Expected_InputOutputViewModelValueNotSet() {
            _autoMappingOutputAction = new AutoMappingOutputAction(_mockDataMappingViewModel.Object, _mockWebActivity.Object);
            IInputOutputViewModel actual;
            Mock<IDataListViewModel> dataListVM = Dev2MockFactory.SetupDataListViewModel();

            DataListSingleton.SetDataList(dataListVM.Object);

            dataListVM.Setup(c => c.DataList).Returns(ReturnDefaultDataListViewModel(dataListVM.Object, false));
            actual = _autoMappingOutputAction.LoadOutputAutoMapping(_mockInputOutputViewModel.Object);
            _mockInputOutputViewModel.VerifySet(dmVM => dmVM.Value = It.IsAny<string>(), Times.Never());
        }

        [TestMethod]
        public void LoadOutputputAutoMappingInputOutputViewModelNullRecordSet_Expected_InputOutputViewModelValueNotSet() {
            _autoMappingOutputAction = new AutoMappingOutputAction(_mockDataMappingViewModel.Object, _mockWebActivity.Object);
            IInputOutputViewModel actual;
            Mock<IDataListViewModel> dataListVM = Dev2MockFactory.SetupDataListViewModel();

            dataListVM.Setup(c => c.DataList).Returns(ReturnDefaultDataListViewModel(dataListVM.Object, false));

            dataListVM.Setup(c => c.DataList).Returns(ReturnDefaultDataListViewModel(dataListVM.Object, false));
            actual = _autoMappingOutputAction.LoadOutputAutoMapping(_mockInputOutputViewModel.Object);
            _mockInputOutputViewModel.VerifySet(dmVM => dmVM.Value = It.IsAny<string>(), Times.Never());
        }

        #endregion Negative Test Cases

        #endregion LoadOutputMapping Tests

        #region Internal Test Methods

        private OptomizedObservableCollection<IDataListItemModel> ReturnDefaultDataListViewModel(IDataListViewModel dLVM, bool isRecordSet, string dataListItemName = null) {
            OptomizedObservableCollection<IDataListItemModel> CollectionObservableCollection = new OptomizedObservableCollection<IDataListItemModel>();
            Mock<IDataListItemModel> dataListItemViewModel = CreateDataListItemViewModel(dataListItemName, dLVM);
            if(isRecordSet) {

                dataListItemViewModel.Setup(c => c.IsRecordset).Returns(true);
                dataListItemViewModel.Setup(c => c.DisplayName).Returns("UnitTestRepresentationValue");
                dataListItemViewModel.Setup(c => c.Children).Returns(CollectionObservableCollection);

            }
            else {
                dataListItemViewModel.SetupGet(d => d.DisplayName).Returns(dataListItemName);
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

        #endregion Internal Test Methods
    }
}
