using Dev2.Composition;
using Dev2.Core.Tests.Utils;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.Studio;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models.DataList;
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
            _dataListViewModel.RecsetCollection = _recsetCollection;
            _dataListViewModel.ScalarCollection = _scallarCollection;

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

            //Mock Setup
            Mock<IMainViewModel> mockMainViewModel = Dev2MockFactory.MainViewModel;

            //Juries 8810 TODO
            //mockMainViewModel.Setup(c => c.ActiveDataList).Returns(_dataListViewModel);

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

        //massimo.guerrera - Commented out till bug 5024 is done
        //[TestMethod]
        //public void AddMissingDataListItems_AddRecordSetWhenDataListContainsScalarWithSameName()
        //{
        //    IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
        //    var part = new Mock<IDataListVerifyPart>();
        //    part.Setup(c => c.Recordset).Returns("Province");
        //    part.Setup(c => c.DisplayValue).Returns("[[Province]]");
        //    part.Setup(c => c.Description).Returns("A state in a republic");
        //    part.Setup(c => c.IsScalar).Returns(false);
        //    part.Setup(c => c.Field).Returns("");
        //    parts.Add(part.Object);

        //    _dataListViewModel.AddMissingDataListItems(parts, false);
        //    _dataListViewModel.AddMissingDataListItems(parts);

        //    Assert.IsTrue(_dataListViewModel.DataList.Count == 5 && !_dataListViewModel.DataList[3].HasError);
        //}

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
            _dataListViewModel.RemoveUnusedDataListItems(parts, false);
            int afterCount = _dataListViewModel.DataList.Count;
            Assert.IsTrue(beforeCount > afterCount);
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
            _dataListViewModel.RemoveUnusedDataListItems(parts, false);
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

        #endregion Internal Test Methods


    }
}
