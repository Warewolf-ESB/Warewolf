using Caliburn.Micro;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Dev2.Studio.Core.Interfaces;
using System.Collections.Generic;
using Moq;
using System.Collections.ObjectModel;
using Unlimited.Framework;
using Dev2.Studio.Core;
using Dev2.Composition;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Xml.Linq;
using Dev2.Core.Tests.Utils;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Core.Factories;

namespace Dev2.Core.Tests
{
    
    
    /// <summary>
    ///This is a test class for DataListItemViewModelTest and is intended
    ///to contain all DataListItemViewModelTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DataListItemViewModelTest:IHandle<DataListItemSelectedMessage> {
        
        #region Locals
                
        private DataListViewModelTestHelper _helper;        
        IDataListItemModel _dataListItemModel;
        int count = 0;

        private static ImportServiceContext _importServiceContext;

        #endregion
       
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
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class

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
        public void MyTestInitialize() {
            ImportService.CurrentContext = _importServiceContext;
            _helper = new DataListViewModelTestHelper();           
            _dataListItemModel = DataListItemModelFactory.CreateDataListModel("testItem");

            //PrepopulateDataListItemViewModel();
        }


        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //

        //private void PrepopulateDataListItemViewModel() {
        //    _dataListItemModel.Description = "testDescription";
        //    _dataListItemModel.Name = "testName";

        //    _dataListItemModel.Children.Add(new DataListItemModel(_mockDataListViewModel.Object) { Parent = _dataListItemModel, Name = "Country", Description = "Name of Country", IsSelected = true });
        //    IDataListItemModel carRecordset = new DataListItemModel(_mockDataListViewModel.Object) { Parent = _dataListItemModel, Name = "Car", Description = "A recordset of information about a car" };
        //    carRecordset.Children.Add(new DataListItemModel(_mockDataListViewModel.Object) { Parent = carRecordset, Name = "Make", Description = "Make of vehicle" });
        //    carRecordset.Children.Add(new DataListItemModel(_mockDataListViewModel.Object) { Parent = carRecordset, Name = "Model", Description = "Model of vehicle" });
        //    _dataListItemModel.Children.Add(carRecordset);
        //}
        #endregion

        #region Parent Property Tests

        /// <summary>
        ///A test for DataListItemViewModel Parent Property Setter
        ///</summary>
        [TestMethod()]
        public void CreationofItemAsChild_ExpectedCorrectReferenceToParent() {
            IDataListItemModel childSet = DataListItemModelFactory.CreateDataListModel("TestChild", "", _dataListItemModel);

            Assert.AreEqual("testItem", childSet.Parent.Name);
        }

        #endregion Parent Property Tests

        #region AddChildren Tests

        /// <summary>
        ///A test for Adding DataListItems to the DataListViewModel
        ///</summary>
        [TestMethod()]
        public void AddChild_ExpectedChildCreationOnDataListItem() {
            IDataListItemModel dataListItemToAdd = DataListItemModelFactory.CreateDataListModel("testName");
            int countBeforeAdd = _dataListItemModel.Children.Count;
            _dataListItemModel.Children.Add(dataListItemToAdd);
            Assert.IsTrue(_dataListItemModel.Children.Count > countBeforeAdd && _dataListItemModel.Children.Count < countBeforeAdd + 2);
        }


        // Should this be checking for errors?
        /// <summary>
        /// A test for adding invalid children
        /// </summary>
        [TestMethod]
        public void AddChild_InvalidChildrenCollection_Expected_ChildrenContainErrors() {   
            IDataListItemModel child = DataListItemModelFactory.CreateDataListModel("test!@#");
            
            _dataListItemModel.Children.Add(child);

            Assert.IsTrue(_dataListItemModel.Children.Count == 1 && _dataListItemModel.Children[0].HasError);
        }

        #endregion AddChildren Tests

        #region RemoveChild Tests

        [TestMethod]
        public void RemoveChild_ExpectRootDataListItemToHaveOneChild() {
            IDataListItemModel dataListItemToAdd = DataListItemModelFactory.CreateDataListModel("testDataListItem");
            _dataListItemModel.Children.Add(dataListItemToAdd);

            int countBefore = _dataListItemModel.Children.Count;
            _dataListItemModel.Children.Remove(dataListItemToAdd);

            Assert.AreNotEqual(_dataListItemModel.Children.Count, countBefore);
        }

        #endregion RemoveChild Tests

        #region Properties Tests

        /// <summary>
        ///A test for DisplayName
        ///</summary>
        [TestMethod]
        public void GetDisplayName_ExpectedReturnDataListItemDisplayName() {
           _dataListItemModel.Children.Add(DataListItemModelFactory.CreateDataListModel("testChild","",_dataListItemModel));
           string expected = "testChild";

            string actual = _dataListItemModel.Children[0].DisplayName;            
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsRecordSet
        ///</summary>
        [TestMethod()]
        public void IsNotRecordSet_ExpectedDataItemNotContainChildren() {
            bool expected = false;
            bool actual = _dataListItemModel.IsRecordset;
            Assert.AreEqual(expected, actual);
        }       

        /// <summary>
        ///A test for Name
        ///</summary>
        [TestMethod()]
        public void DataItemNameUpdate_ExpectedDataItemNameUpdated() {
            string expected = "testItem";
            string actual = _dataListItemModel.Name;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests if an invalidly formed DataListItem is sent to the ActivityComparer
        ///</summary>
        [TestMethod]
        public void MalformedDataListItemToDsfActivityComparer() {
            string mediatorKey = string.Empty;
            //mediatorKey = Mediator.RegisterToReceiveMessage(MediatorMessages.DataListItemSelected, input => MediatorRecieveTestMethod());
            _dataListItemModel.HasError = true;
            _dataListItemModel.IsSelected = true;
            Assert.AreEqual(0, count);
            //Mediator.DeRegisterAllActionsForMessage(MediatorMessages.DataListItemSelected);
            
        }

        #endregion Properties Tests   

        #region VerifyName Tests

        /// <summary>
        ///Verifying the name has no special chars
        ///</summary>
        [TestMethod()]
        public void VerifyNameHasNoSpecialChars() {
            _dataListItemModel.DisplayName = "test@";
            Assert.IsTrue(_dataListItemModel.HasError);
        }

        #endregion VerifyName Tests

        //#region CheckForDuplicates Tests

        //// Sashen - 17-10-2012 - This test case fails because when adding children, a parent is not set on the child
        ///// <summary>
        /////Checking if there are dublicates in the Data List
        /////</summary>
        //[TestMethod()]
        //public void CheckForDuplicates() {

        //    _dataListItemViewModel.AddChild(_helper.CreateDataListItemViewModel("Country", _mockDataListViewModel.Object));
        //    _dataListItemViewModel.AddChild(_helper.CreateDataListItemViewModel("Country", _mockDataListViewModel.Object));
        //    var lastItem = _dataListItemViewModel.Children[_dataListItemViewModel.Children.Count - 1];
        //    _dataListItemViewModel.CheckForDuplicate(lastItem);
        //    Assert.IsTrue(lastItem.ErrorMessage == "You cannot enter duplicate names in the Data List");
        //}

        //#endregion CheckForDuplicates Tests

        //#region VerifyDataList Structure Tests

        ///// <summary>
        ///// Test to ensure logic behind VerifyDataListStructure method
        ///// </summary>
        //[TestMethod]
        //public void VerifyDataListStructureDataListContainingChildren_Expected_DataListViewModelAddRowMethodCall() {
        //    _mockDataListViewModel.Setup(dl => dl.AddRowIfAllRowsHaveData()).Verifiable();
            
        //    _dataListItemModel.VerifyDataListStructure();
        //    _mockDataListViewModel.Verify(dl => dl.AddRowIfAllRowsHaveData(), Times.Once());
        //}

        ///// <summary>
        ///// Test that the DataListViewModel does not add empty rows if the datalist is empty
        ///// </summary>
        //[TestMethod]
        //public void VerifyDataListStructureDataListEmptyDataList_Expected_DataListViewModelAddRowMethodCall() {
        //    _mockDataListViewModel.Setup(dl => dl.AddRowIfAllRowsHaveData()).Verifiable();
        //    _mockDataListViewModel.Setup(dl => dl.DataList).Returns(new OptomizedObservableCollection<IDataListItemModel>());

        //    IDataListItemModel dlVM = _helper.CreateDataListItemViewModel("test", _mockDataListViewModel.Object);
        //    _dataListItemModel.IsRoot = true;
            
        //    _dataListItemModel.VerifyDataListStructure();

        //    _mockDataListViewModel.Verify(dl => dl.AddRowIfAllRowsHaveData(), Times.Never());
        //}

        //#endregion VerifyDataList Structure Tests

        //#region MatchesSearchFilter Tests
        
        ///// <summary>
        ///// Test to ensure that partial matches are made by the MatchesSearchFilter Method
        ///// </summary>
        //[TestMethod]
        //public void MatchesSearchFilter_SearchStringMatchesItemInDataList_Expected_ReturnTrue() {
        //    IDataListItemModel dataListItemViewModel = _helper.CreateDataListItemViewModel("UnitTestDataListItem", _mockDataListViewModel.Object);
        //    bool actual = dataListItemViewModel.MatchesSearchFilter("UnitTest");
        //    Assert.IsTrue(actual);
        //}

        ///// <summary>
        ///// Test to ensure that non-related items are not returned as a Match by MatchesSearchFilter Method
        ///// </summary>
        //[TestMethod]
        //public void MatchesSearchFilter_SearchStringDoesNotMatchDataListItem_Expected_ReturnFalse() {
            
        //    int numberOfDataListItems = 10;
        //    IDataListItemModel dataListItemViewModel = _helper.CreateDataListItemViewModel(string.Empty, _mockDataListViewModel.Object);
        //    IList<IDataListItemModel> dataListItemViewModelList = _helper.CreateDataListItemViewModel("UniteTestDataListItem", numberOfDataListItems, _mockDataListViewModel.Object);
        //    dataListItemViewModel.AddChildren(dataListItemViewModelList);
        //    dataListItemViewModel.IsRoot = true;

        //    bool actual = dataListItemViewModel.MatchesSearchFilter("NoneExistant");
        //    Assert.IsFalse(actual);

        //}

        ///// <summary>
        ///// Test to ensure that empty dataListViewModel does not return true when it is empty
        ///// </summary>
        //[TestMethod]
        //public void MatchesSearchFilter_EmptyDataListItem_Expected_ReturnsFalse() {
        //    IDataListItemModel dataListItemViewModel = _helper.CreateDataListItemViewModel(string.Empty, _mockDataListViewModel.Object);
        //    dataListItemViewModel.IsRoot = true;

        //    bool actual = dataListItemViewModel.MatchesSearchFilter("TestText");
        //    Assert.IsFalse(actual);
        //}

        //#endregion MatchesSearchFilter Tests        

        #region Static Method Tests

        // Sashen - 17-10-2012 Both the static methods "ProcessChildren" and "CreateDataListXmlElement" are tested by the ToDataListXML Method

        #endregion Static Method Tests

        #region Internal Test Methods

        public int MediatorRecieveTestMethod() {
            count++;
            return count;
        }

        #endregion Internal Test Methods

        #region Implementation of IHandle<DataListItemSelectedMessage>

        public void Handle(DataListItemSelectedMessage message)
        {
            MediatorRecieveTestMethod();
        }

        #endregion
    }
}
