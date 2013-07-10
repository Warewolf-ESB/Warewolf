using System.Linq;
using Dev2.Composition;
using Dev2.Core.Tests.Utils;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Studio;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class DataListViewModelTests
    {
        #region Locals

        private DataListViewModel _dataListViewModel;
        private Mock<IContextualResourceModel> _mockResourceModel;

        //private Mock<IMediatorRepo> _mockMediatorRepo;
        private DataListViewModelTestHelper _helper;

        #endregion

        #region Initialization


        [TestInitialize]
        public void Initialize()
        {
            ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();
            _helper = new DataListViewModelTestHelper();

            //_mockMediatorRepo = new Mock<IMediatorRepo>();
            _mockResourceModel = Dev2MockFactory.SetupResourceModelMock();

            //_mockMediatorRepo.Setup(c => c.addKey(It.IsAny<Int32>(), It.IsAny<MediatorMessages>(), It.IsAny<String>()));
            //_mockMediatorRepo.Setup(c => c.deregisterAllItemMessages(It.IsAny<Int32>()));
            _dataListViewModel = new DataListViewModel();
            _dataListViewModel.InitializeDataListViewModel(_mockResourceModel.Object);
            //Mock<IMainViewModel> _mockMainViewModel = Dev2MockFactory.SetupMainViewModel();
            OptomizedObservableCollection<IDataListItemModel> _scallarCollection = new OptomizedObservableCollection<IDataListItemModel>();
            OptomizedObservableCollection<IDataListItemModel> _recsetCollection = new OptomizedObservableCollection<IDataListItemModel>();
            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();

            IDataListItemModel carRecordset = DataListItemModelFactory.CreateDataListModel("Car", "A recordset of information about a car", enDev2ColumnArgumentDirection.Both);
            carRecordset.Children.Add(DataListItemModelFactory.CreateDataListModel("Make", "Make of vehicle", carRecordset));
            carRecordset.Children.Add(DataListItemModelFactory.CreateDataListModel("Model", "Model of vehicle", carRecordset));

            _dataListViewModel.RecsetCollection.Add(carRecordset);
            _dataListViewModel.ScalarCollection.Add(DataListItemModelFactory.CreateDataListModel("Country", "name of Country", enDev2ColumnArgumentDirection.Both));

            DataListSingleton.SetDataList(_dataListViewModel);
        }


        private void MockSetup()
        {
        }

        private void PrepoluateDataListViewModel()
        {
        }

        #endregion Initialize


        // It would be very useful to have a sort of test Designer to generate XAML, it's apparently         

        #region Add Missing Tests

        [TestMethod]
        public void AddMissingDataListItems_AddScalars_ExpectedAddDataListItems()
        {
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();

            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Field).Returns("Province");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(true);
            parts.Add(part.Object);


            _dataListViewModel.AddMissingDataListItems(parts, false);
            Assert.IsFalse(_dataListViewModel.DataList[_dataListViewModel.DataList.Count - 3].IsRecordset);
        }

        [TestMethod]
        public void AddMissingDataListItems_AddRecordSet_ExpectedNewRecordSetCreatedonRootNode()
        {
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("Province");
            part.Setup(c => c.DisplayValue).Returns("[[Province]]");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.Field).Returns("");
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts, false);
            Assert.IsTrue(_dataListViewModel.RecsetCollection.Count == 3);
        }
        
        [TestMethod]
        public void AddMissingDataListItems_AddRecordSetWhenDataListContainsScalarWithSameName()
        {
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("Province");
            part.Setup(c => c.DisplayValue).Returns("[[Province]]");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.Field).Returns("");
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts, false);
            _dataListViewModel.AddMissingDataListItems(parts);

            Assert.IsTrue(_dataListViewModel.DataList.Count == 5 && !_dataListViewModel.DataList[3].HasError);
        }

        [TestMethod]
        public void AddMissingScalarItemWhereItemsAlreadyExistsInDataListExpectedNoItemsAdded()
        {
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();

            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Field).Returns("Province");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(true);
            parts.Add(part.Object);         

            _dataListViewModel.AddMissingDataListItems(parts, false);
            //Second add trying to add the same items to the data list again
            _dataListViewModel.AddMissingDataListItems(parts, false);
            Assert.IsFalse(_dataListViewModel.DataList[_dataListViewModel.DataList.Count - 3].IsRecordset);
            Assert.IsTrue(_dataListViewModel.ScalarCollection[0].DisplayName == "Province");
            Assert.IsTrue(_dataListViewModel.ScalarCollection[1].DisplayName == "Country");
            Assert.IsTrue(_dataListViewModel.ScalarCollection[2].DisplayName == string.Empty);
            Assert.IsTrue(_dataListViewModel.RecsetCollection[0].DisplayName == "Car()");           
        }

        [TestMethod]
        public void AddMissingRecordsetItemWhereItemsAlreadyExistsInDataListExpectedNoItemsAdded()
        {
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();

            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("Province");
            part.Setup(c => c.DisplayValue).Returns("[[Province]]");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.Field).Returns("");
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts, false);
            //Second add trying to add the same items to the data list again
            _dataListViewModel.AddMissingDataListItems(parts, false);
            Assert.IsTrue(_dataListViewModel.RecsetCollection.Count == 3);            
            Assert.IsTrue(_dataListViewModel.ScalarCollection[0].DisplayName == "Country");
            Assert.IsTrue(_dataListViewModel.ScalarCollection[1].DisplayName == string.Empty);
            Assert.IsTrue(_dataListViewModel.RecsetCollection[0].DisplayName == "Province()"); 
            Assert.IsTrue(_dataListViewModel.RecsetCollection[1].DisplayName == "Car()");
            
        }

        [TestMethod]
        public void AddMissingRecordsetChildItemWhereItemsAlreadyExistsInDataListExpectedNoItemsAdded()
        {
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();

            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("Province");
            part.Setup(c => c.DisplayValue).Returns("[[Province]]");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.Field).Returns("field1");
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts, false);
            //Second add trying to add the same items to the data list again            
            _dataListViewModel.AddMissingDataListItems(parts, false);
            Assert.IsTrue(_dataListViewModel.RecsetCollection[0].Children.Count == 2);
            Assert.IsTrue(_dataListViewModel.RecsetCollection[0].Children[0].DisplayName == "Province().field1");
        }

        #endregion Add Missing Tests

        #region RemoveUnused Tests

        [TestMethod]
        public void RemoveUnusedDataListItems_RemoveScalars_ExpectedItemRemovedFromDataList()
        {
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Field).Returns("testing");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(true);
            parts.Add(part.Object);

            // Mock Setup            

            Mock<IMainViewModel> mockMainViewModel = Dev2MockFactory.MainViewModel;
            //Juries 8810 TODO
            //mockMainViewModel.Setup(c => c.ActiveDataList).Returns(_dataListViewModel);
            _dataListViewModel.AddMissingDataListItems(parts, false);
            int beforeCount = _dataListViewModel.DataList.Count;
            parts.Add(part.Object);
            _dataListViewModel.SetUnusedDataListItems(parts);
            _dataListViewModel.RemoveUnusedDataListItems();
            int afterCount = _dataListViewModel.DataList.Count;
            Assert.IsTrue(beforeCount > afterCount);
        }       
        
        [TestMethod]
        public void SetUnusedDataListItemsWhenTwoScalarsSameNameExpectedBothMarkedAsUnused()
        {
            //---------------------------Setup----------------------------------------------------------
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part1 = new Mock<IDataListVerifyPart>();
            part1.Setup(c => c.Field).Returns("testing");
            part1.Setup(c => c.Description).Returns("A state in a republic");
            part1.Setup(c => c.IsScalar).Returns(true);
            var part2 = new Mock<IDataListVerifyPart>();
            part2.Setup(c => c.Field).Returns("testing");
            part2.Setup(c => c.Description).Returns("Duplicate testing");
            part2.Setup(c => c.IsScalar).Returns(true);
            parts.Add(part1.Object);
            parts.Add(part2.Object);
            var dataListItemModels = _dataListViewModel.CreateDataListItems(parts, true);
            _dataListViewModel.ScalarCollection.AddRange(dataListItemModels);
            //-------------------------Execute Test ------------------------------------------
            _dataListViewModel.SetUnusedDataListItems(parts);
            //-------------------------Assert Resule------------------------------------------
            int actual = _dataListViewModel.DataList.Count(model => !model.IsUsed && !model.IsRecordset && !string.IsNullOrEmpty(model.Name));
            Assert.AreEqual(2,actual);
        } 
        
        [TestMethod]
        public void SetUnusedDataListItemsWhenTwoRecsetsSameNameExpectedBothMarkedAsUnused()
        {
            //---------------------------Setup----------------------------------------------------------
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part1 = new Mock<IDataListVerifyPart>();
            part1.Setup(c => c.Recordset).Returns("testing");
            part1.Setup(c => c.DisplayValue).Returns("[[testing]]");
            part1.Setup(c => c.Description).Returns("A state in a republic");
            part1.Setup(c => c.IsScalar).Returns(false);
            var part2 = new Mock<IDataListVerifyPart>();
            part2.Setup(c => c.Recordset).Returns("testing");
            part2.Setup(c => c.DisplayValue).Returns("[[testing]]");
            part2.Setup(c => c.Description).Returns("Duplicate testing");
            part2.Setup(c => c.IsScalar).Returns(false);
            parts.Add(part1.Object);
            parts.Add(part2.Object);

            IDataListItemModel mod = new DataListItemModel("testing");
            mod.Children.Add(new DataListItemModel("f1",parent:mod));
            IDataListItemModel mod2 = new DataListItemModel("testing");
            mod2.Children.Add(new DataListItemModel("f2",parent:mod2));
            
            _dataListViewModel.RecsetCollection.Add(mod);
            _dataListViewModel.RecsetCollection.Add(mod2);
           
            //-------------------------Execute Test ------------------------------------------
            _dataListViewModel.SetUnusedDataListItems(parts);
            //-------------------------Assert Resule------------------------------------------
            int actual = _dataListViewModel.DataList.Count(model => !model.IsUsed && model.IsRecordset);
            Assert.AreEqual(2,actual);
        }


        [TestMethod]
        public void RemoveUnusedDataListItems_RemoveMalformedScalar_ExpectedItemNotRemovedFromDataList()
        {
            //TO DO: Implement Logic for the Add Malformed Scalar test method
        }

        [TestMethod]
        public void RemoveUnusedDataListItems_RemoveMalformedRecordSet_ExpectedRecordSetRemove()
        {
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("Province");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(false);
            parts.Add(part.Object);
           
            _dataListViewModel.AddMissingDataListItems(parts, false);
            int beforeCount = _dataListViewModel.DataList.Count;
            parts.Add(part.Object);
            _dataListViewModel.SetUnusedDataListItems(parts);
            _dataListViewModel.RemoveUnusedDataListItems();
            int afterCount = _dataListViewModel.DataList.Count;
            Assert.IsTrue(beforeCount > afterCount);
        }

        #endregion RemoveUnused Tests

        #region RemoveRowIfEmpty Tests

        [TestMethod()]
        public void RemoveRowIfEmpty_ExpectedCountofDataListItemsReduceByOne()
        {
            _dataListViewModel.AddBlankRow(new DataListItemModel("Test"));
            int beforeCount = _dataListViewModel.ScalarCollection.Count;
            _dataListViewModel.ScalarCollection[0].Description = string.Empty;
            _dataListViewModel.ScalarCollection[0].DisplayName = string.Empty;
            _dataListViewModel.RemoveBlankRows(_dataListViewModel.ScalarCollection[0]);
            int afterCount = _dataListViewModel.ScalarCollection.Count;

            Assert.IsTrue(beforeCount > afterCount);
        }

        #endregion RemoveRowIfEmpty Tests

        #region AddRowIfAllCellsHaveData Tests

        /// <summary>
        ///Testing that there is always a blank row in the data list
        ///</summary>
        [TestMethod]
        public void AddRowIfAllCellsHaveData_AllDataListRowsContainingData_Expected_RowAdded()
        {
            int beforeCount = _dataListViewModel.DataList.Count;
            _dataListViewModel.AddBlankRow(_dataListViewModel.ScalarCollection[0]);            
            int afterCount = _dataListViewModel.DataList.Count;
            Assert.IsTrue(afterCount > beforeCount);
        }

        /// <summary>
        /// Tests that no rows are added to the datalistItem collection if there is already a blank row
        /// </summary>
        [TestMethod]
        public void AddRowIfAllCellsHaveData_BlankRowAlreadyExists_Expected_NoRowsAdded()
        {
            
            _dataListViewModel.AddBlankRow(_dataListViewModel.ScalarCollection[0]);
            int beforeCount = _dataListViewModel.DataList.Count;
            _dataListViewModel.AddBlankRow(_dataListViewModel.ScalarCollection[0]);
            int afterCount = _dataListViewModel.DataList.Count;

            Assert.AreEqual(beforeCount, afterCount);
        }

        #endregion AddRowIfAllCellsHaveData Tests

        #region CreateDataListItems Tests

        /// <summary>
        /// Test to ensure that the DataListViewModel can sucessfully created DataListItemViewModels from DataListVerifyParts using Scalar Notation
        /// </summary>
        [TestMethod]
        public void CreateDataListItem_ScalarDataListParts_Expected_DataListItemCreatedAndAddedToDataListViewModel()
        {
            int numberOfPartsToCreate = 10;
            IList<IDataListVerifyPart> parts = CreateListScalarDataListVerifyParts("testPart", numberOfPartsToCreate);
            IList<IDataListItemModel> dataListItems = _dataListViewModel.CreateDataListItems(parts, false);

            Assert.AreEqual(numberOfPartsToCreate, dataListItems.Count);
        }

        /// <summary>
        /// Test to ensure that the DataListViewModel is able to create DataListItemViewModel from DataListVerifyParts using RecordSet notation
        /// </summary>
        [TestMethod]
        public void CreateDataListItem_RecordSetDataListParts_Expected_DataListItemCreateAndAddedToDataList()
        {
            int numberOfPartsToCreate = 10;
            IList<IDataListVerifyPart> parts = CreateRecordSetPartWithMultipleFields("TestRecordSet", numberOfPartsToCreate, "testField");
            IList<IDataListItemModel> dataListItems = _dataListViewModel.CreateDataListItems(parts, true);

            Assert.AreEqual(10, dataListItems.Count);
        }


        #endregion CreateDataListItems Tests

        #region AddRecordsetNamesIfMissing Tests

        [TestMethod]
        public void AddRecordSetNamesIfMissing_DataListContainingRecordSet_Expected_Positive()
        {                     
            _dataListViewModel.AddRecordsetNamesIfMissing();

            Assert.IsTrue(_dataListViewModel.RecsetCollection.Count == 1 && _dataListViewModel.RecsetCollection[0].Children[0].DisplayName == "Car().Make");
        }     

        #endregion AddRecordsetNamesIfMissing Tests

        #region Add Tests


        #endregion Add Tests

        #region AddRecordSet Tests


        #endregion AddRecordSet Tests

        #region WriteDataToResourceModel Tests

        [TestMethod]
        public void WriteDataListToResourceModel_ScalarAnsrecset_Expected_Positive()
        {
            string result = _dataListViewModel.WriteToResourceModel();

            string expectedResult = @"<DataList><Country Description=""name of Country"" IsEditable=""True"" ColumnIODirection=""Both"" /><Car Description=""A recordset of information about a car"" IsEditable=""True"" ColumnIODirection=""Both"" ><Make Description=""Make of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" /><Model Description=""Model of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" /></Car></DataList>";

            Assert.AreEqual(expectedResult, result);
        }

        #endregion WriteDataToResourceModel Tests

        #region Internal Test Methods

        private IDataListItemModel CreateDataListItemViewModel(string name, IDataListViewModel parent)
        {
            return _helper.CreateDataListItemViewModel(name, parent);
        }

        private IDataListItemModel CreateRecordSetDataListItem(string name, int numberOfRecordsToCreate, string recordSetPrefix, IDataListViewModel parent)
        {
            return _helper.CreateRecordSetDataListItem(name, numberOfRecordsToCreate, recordSetPrefix, parent);
        }

        private IDataListViewModel CreateDataListViewModel(IResourceModel resourceModel, IDataListItemModel dataListItems)
        {
            return _helper.CreateDataListViewModel(resourceModel, dataListItems);
        }

        private IList<IDataListVerifyPart> CreateListScalarDataListVerifyParts(string prefix, int numberOfPartsToCreate)
        {
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            if (numberOfPartsToCreate.Equals(0))
            {
                parts.Add(IntellisenseFactory.CreateDataListValidationScalarPart(prefix));
            }
            else
            {
                for (int i = 1; i <= numberOfPartsToCreate; i++)
                {
                    parts.Add(IntellisenseFactory.CreateDataListValidationScalarPart(string.Format("{0}{1}", prefix, i.ToString())));
                }
            }
            return parts;
        }

        internal IList<IDataListVerifyPart> CreateRecordSetPartWithMultipleFields(string recordSetName, int numberofField, string fieldPrefix)
        {
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            for (int i = 1; i <= numberofField; i++)
            {
                parts.Add(IntellisenseFactory.CreateDataListValidationRecordsetPart(recordSetName, string.Format("{0}{1}", fieldPrefix, i.ToString())));
            }
            return parts;
        }


        internal IDataListVerifyPart CreateRecordSetPart(string recordSetName, string fieldName)
        {
            IDataListVerifyPart part = IntellisenseFactory.CreateDataListValidationRecordsetPart(recordSetName, fieldName);

            return part;
        }


        //private IList<IDataListVerifyPart> CreateDataListVerifyParts(string[] partsToCreate) {

        //}

        private void sortInitialization()
        {
            _dataListViewModel.ScalarCollection.Add(DataListItemModelFactory.CreateDataListModel("zzz"));
            _dataListViewModel.ScalarCollection.Add(DataListItemModelFactory.CreateDataListModel("ttt"));
            _dataListViewModel.ScalarCollection.Add(DataListItemModelFactory.CreateDataListModel("aaa"));
            _dataListViewModel.RecsetCollection.Add(DataListItemModelFactory.CreateDataListModel("zzz"));
            _dataListViewModel.RecsetCollection.Add(DataListItemModelFactory.CreateDataListModel("ttt"));
            _dataListViewModel.RecsetCollection.Add(DataListItemModelFactory.CreateDataListModel("aaa"));
        }

        private void sortCleanup()
        {
            _dataListViewModel.ScalarCollection.Clear();
            _dataListViewModel.RecsetCollection.Clear();

            IDataListItemModel carRecordset = DataListItemModelFactory.CreateDataListModel("Car", "A recordset of information about a car", enDev2ColumnArgumentDirection.Both);
            carRecordset.Children.Add(DataListItemModelFactory.CreateDataListModel("Make", "Make of vehicle", carRecordset));
            carRecordset.Children.Add(DataListItemModelFactory.CreateDataListModel("Model", "Model of vehicle", carRecordset));

            _dataListViewModel.RecsetCollection.Add(carRecordset);
            _dataListViewModel.ScalarCollection.Add(DataListItemModelFactory.CreateDataListModel("Country", "name of Country", enDev2ColumnArgumentDirection.Both));
        }

        #endregion Internal Test Methods

        #region Sort

        [TestMethod]
        public void SortOnceExpectedSortsAscendingOrder()
        {
            sortInitialization();

            //Execute
            _dataListViewModel.SortCommand.Execute(null);

            //Scalar List Asserts
            Assert.AreEqual("aaa", _dataListViewModel.ScalarCollection[0].DisplayName, "Sort datalist left scalar list unsorted");
            Assert.AreEqual("Country", _dataListViewModel.ScalarCollection[1].DisplayName, "Sort datalist left scalar list unsorted");
            Assert.AreEqual("ttt", _dataListViewModel.ScalarCollection[2].DisplayName, "Sort datalist left scalar list unsorted");
            Assert.AreEqual("zzz", _dataListViewModel.ScalarCollection[3].DisplayName, "Sort datalist left scalar list unsorted");
            //Recset List Asserts
            Assert.AreEqual("aaa", _dataListViewModel.RecsetCollection[0].DisplayName, "Sort datalist left recset list unsorted");
            Assert.AreEqual("Car", _dataListViewModel.RecsetCollection[1].DisplayName, "Sort datalist left recset list unsorted");
            Assert.AreEqual("ttt", _dataListViewModel.RecsetCollection[2].DisplayName, "Sort datalist left recset list unsorted");
            Assert.AreEqual("zzz", _dataListViewModel.RecsetCollection[3].DisplayName, "Sort datalist left recset list unsorted");

            sortCleanup();
        }

        [TestMethod]
        public void SortTwiceExpectedSortsDescendingOrder()
        {
            sortInitialization();

            //Execute
            _dataListViewModel.SortCommand.Execute(null);
            //Execute Twice
            _dataListViewModel.SortCommand.Execute(null);

            //Scalar List Asserts
            Assert.AreEqual("zzz", _dataListViewModel.ScalarCollection[0].DisplayName, "Sort datalist left scalar list unsorted");
            Assert.AreEqual("ttt", _dataListViewModel.ScalarCollection[1].DisplayName, "Sort datalist left scalar list unsorted");
            Assert.AreEqual("Country", _dataListViewModel.ScalarCollection[2].DisplayName, "Sort datalist left scalar list unsorted");
            Assert.AreEqual("aaa", _dataListViewModel.ScalarCollection[3].DisplayName, "Sort datalist left scalar list unsorted");
            //Recset List Asserts
            Assert.AreEqual("zzz", _dataListViewModel.RecsetCollection[0].DisplayName, "Sort datalist left recset list unsorted");
            Assert.AreEqual("ttt", _dataListViewModel.RecsetCollection[1].DisplayName, "Sort datalist left recset list unsorted");
            Assert.AreEqual("Car", _dataListViewModel.RecsetCollection[2].DisplayName, "Sort datalist left recset list unsorted");
            Assert.AreEqual("aaa", _dataListViewModel.RecsetCollection[3].DisplayName, "Sort datalist left recset list unsorted");

            sortCleanup();
        }

        [TestMethod]
        public void SortLargeListOfScalarsExpectedLessThan60Milliseconds()
        {
            //Initialize
            for(var i = 5000; i > 0; i--)
            {
                _dataListViewModel.ScalarCollection.Add(DataListItemModelFactory.CreateDataListModel("testVar"+i.ToString().PadLeft(4, '0')));
            }
            var timeBefore = DateTime.Now;

            //Execute
            _dataListViewModel.SortCommand.Execute(null);

            //Assert
            Assert.AreEqual("Country", _dataListViewModel.ScalarCollection[0].DisplayName, "Sort datalist with large list failed");
            Assert.AreEqual("testVar1000", _dataListViewModel.ScalarCollection[1000].DisplayName, "Sort datalist with large list failed");
            Assert.AreEqual("testVar3000", _dataListViewModel.ScalarCollection[3000].DisplayName, "Sort datalist with large list failed");
            Assert.AreEqual("testVar5000", _dataListViewModel.ScalarCollection[5000].DisplayName, "Sort datalist with large list failed");
            Assert.IsTrue(DateTime.Now.Subtract(timeBefore) < TimeSpan.FromMilliseconds(60), "Sort datalist took longer than 60 milliseconds to sort 5000 variables");

            sortCleanup();
        }

        #endregion

    }
}
