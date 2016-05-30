using System.Collections.Generic;
using System.Text;
using Caliburn.Micro;
using Dev2.Data.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming

namespace Dev2.Core.Tests
{
    partial class DataListViewModelTests
    {

        #region Initialization



        void SetupForComplexObjects()
        {

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataList);
            mockResourceModel.Setup(res => res.WorkflowXaml).Returns(new StringBuilder(StringResources.xmlServiceDefinition));
            mockResourceModel.Setup(resModel => resModel.ResourceName).Returns("Test");
            mockResourceModel.Setup(resModel => resModel.DisplayName).Returns("TestResource");
            mockResourceModel.Setup(resModel => resModel.Category).Returns("Testing");
            mockResourceModel.Setup(resModel => resModel.IconPath).Returns("");
            mockResourceModel.Setup(resModel => resModel.ResourceType).Returns(ResourceType.WorkflowService);
            mockResourceModel.Setup(resModel => resModel.DataTags).Returns("WFI1,WFI2,WFI3");
            mockResourceModel.Setup(resModel => resModel.Environment).Returns(Dev2MockFactory.SetupEnvironmentModel(mockResourceModel, new List<IResourceModel>()).Object);



            _mockResourceModel = mockResourceModel;

            _dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            _dataListViewModel.InitializeDataListViewModel(_mockResourceModel.Object);
            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();
            _dataListViewModel.ComplexObjectCollection.Clear();

            DataListSingleton.SetDataList(_dataListViewModel);
        }

        #endregion Initialize
        [TestMethod]
        public void AddMissingDataListItems_AddComplexObject_ExpectedNewComplexObjectCreatedonRootNode()
        {
            SetupForComplexObjects();
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("Province");
            part.Setup(c => c.DisplayValue).Returns("[[Province]]");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.Field).Returns("");
            part.Setup(c => c.IsJson).Returns(true);
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts, false);
            Assert.AreEqual(2, _dataListViewModel.ComplexObjectCollection.Count);
        }

        [TestMethod]
        public void AddMissingDataListItems_AddComplexObjectWhenDataListContainsScalarWithSameName()
        {
            SetupForComplexObjects();
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("Province");
            part.Setup(c => c.DisplayValue).Returns("[[Province]]");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.IsJson).Returns(true);
            part.Setup(c => c.Field).Returns("");
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts, false);
            _dataListViewModel.AddMissingDataListItems(parts);
            Assert.AreEqual(4, _dataListViewModel.DataList.Count);
            Assert.IsTrue(!_dataListViewModel.DataList[3].HasError);
        }
        [TestMethod]
        public void AddMissingComplexObjectItemWhereItemsAlreadyExistsInDataListExpectedNoItemsAdded()
        {
            SetupForComplexObjects();
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();

            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("Province");
            part.Setup(c => c.DisplayValue).Returns("[[Province]]");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.Field).Returns("");
            part.Setup(c => c.IsJson).Returns(true);
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts, false);
            //Second add trying to add the same items to the data list again
            _dataListViewModel.AddMissingDataListItems(parts, false);
            Assert.AreEqual(2, _dataListViewModel.ComplexObjectCollection.Count);
            Assert.AreEqual(string.Empty, _dataListViewModel.ScalarCollection[0].DisplayName);
            Assert.AreEqual("Province()", _dataListViewModel.ComplexObjectCollection[0].DisplayName);
            Assert.AreEqual("Car()", _dataListViewModel.ComplexObjectCollection[1].DisplayName);
        }

        [TestMethod]
        public void RemoveUnusedDataListItems_RemoveMalformedComplexObject_ExpectedComplexObjectRemove()
        {
            SetupForComplexObjects();
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("Province");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.IsJson).Returns(true);
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts, false);
            int beforeCount = _dataListViewModel.DataList.Count;
            parts.Add(part.Object);
            _dataListViewModel.SetIsUsedDataListItems(parts, false);
            _dataListViewModel.RemoveUnusedDataListItems();
            int afterCount = _dataListViewModel.DataList.Count;
            Assert.IsTrue(beforeCount > afterCount);
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_AddRecordSetNamesIfMissing")]
        public void AddComplexObjectNamesIfMissing_DataListContainingComplexObject_WithDoubleBracketedComplexObjectName_Expected_Positive()
        {

            //------------Setup for test--------------------------

            _mockResourceModel = Dev2MockFactory.SetupResourceModelMock();

            _dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            _dataListViewModel.InitializeDataListViewModel(_mockResourceModel.Object);
            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();

            IComplexObjectItemModel carObject = DataListItemModelFactory.CreateComplexObjectItemModel("Car");
            
            var makeObject = DataListItemModelFactory.CreateComplexObjectItemModel("Make");

            makeObject.Parent = carObject;
            carObject.Children.Add(makeObject);
            var carModel = DataListItemModelFactory.CreateComplexObjectItemModel("Model");
            carObject.Children.Add(carModel);
            carModel.Parent = carObject;


            _dataListViewModel.ComplexObjectCollection.Add(carObject);
            _dataListViewModel.ScalarCollection.Add(DataListItemModelFactory.CreateScalarItemModel("Country", "name of Country"));

            DataListSingleton.SetDataList(_dataListViewModel);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(2, _dataListViewModel.ComplexObjectCollection.Count);
            Assert.AreEqual("Car", _dataListViewModel.ComplexObjectCollection[1].DisplayName);
            Assert.AreEqual("Car.Make", _dataListViewModel.ComplexObjectCollection[1].Children[0].DisplayName);
        }

        [TestMethod]
        public void AddMissingComplexObjectChildItemWhereItemsAlreadyExistsInDataListExpectedNoItemsAdded()
        {
            SetupForComplexObjects();
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();

            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("Province");
            part.Setup(c => c.DisplayValue).Returns("[[Province]]");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.Field).Returns("field1");
            part.Setup(c => c.IsJson).Returns(true);
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts, false);
            //Second add trying to add the same items to the data list again            
            _dataListViewModel.AddMissingDataListItems(parts, false);
            Assert.AreEqual(1, _dataListViewModel.ComplexObjectCollection[0].Children.Count);
            Assert.AreEqual("Province.field1", _dataListViewModel.ComplexObjectCollection[0].Children[0].DisplayName);
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_AddRecordSetNamesIfMissing")]
        public void AddComplexObjectNamesIfMissing_DataListContainingComplexObject_WithSingleBracketedComplexObjectName_Expected_Positive()
        {

            //------------Setup for test--------------------------

            _mockResourceModel = Dev2MockFactory.SetupResourceModelMock();

            _dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            _dataListViewModel.InitializeDataListViewModel(_mockResourceModel.Object);
            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();
            _dataListViewModel.ComplexObjectCollection.Clear();

            IComplexObjectItemModel carRecordset = DataListItemModelFactory.CreateComplexObjectItemModel("[Car]");
            var carmake = DataListItemModelFactory.CreateComplexObjectItemModel("Make");
            carmake.Parent = carRecordset;
            carRecordset.Children.Add(carmake);
            
            var model = DataListItemModelFactory.CreateComplexObjectItemModel("Model");
            model.Parent = carRecordset;
            carRecordset.Children.Add(model);

            _dataListViewModel.ComplexObjectCollection.Add(carRecordset);
            _dataListViewModel.ScalarCollection.Add(DataListItemModelFactory.CreateScalarItemModel("Country", "name of Country"));

            DataListSingleton.SetDataList(_dataListViewModel);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(1, _dataListViewModel.ComplexObjectCollection.Count);
            Assert.AreEqual("Car", _dataListViewModel.ComplexObjectCollection[0].DisplayName);
            Assert.AreEqual("Car.Make", _dataListViewModel.ComplexObjectCollection[0].Children[0].DisplayName);
        }

    }
}
