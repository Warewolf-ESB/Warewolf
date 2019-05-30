/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Data;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Instrumentation;
using Dev2.Studio.Core;
using Dev2.Studio.Core.DataList;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;



namespace Dev2.Core.Tests
{
    [TestClass]
    [TestCategory("Studio Datalist Core")]
    public class DataListViewModelTests
    {

        #region Initialization

        [TestInitialize]
        public void Initialize()
        {
            AppUsageStats.LocalHost = "http://localhost:3142";
        }

        public static (Mock<IContextualResourceModel>, DataListViewModel) Setup()
        {
            var _mockResourceModel = Dev2MockFactory.SetupResourceModelMock();

            var _dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);

            _dataListViewModel.InitializeDataListViewModel(_mockResourceModel.Object);
            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();

            var carRecordset = DataListItemModelFactory.CreateRecordSetItemModel("Car", "A recordset of information about a car");
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Make", "Make of vehicle", carRecordset));
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Model", "Model of vehicle", carRecordset));
            carRecordset.Input = true;
            carRecordset.Output = true;
            _dataListViewModel.Add(carRecordset);
            _dataListViewModel.Add(DataListItemModelFactory.CreateScalarItemModel("Country", "name of Country", enDev2ColumnArgumentDirection.Both));

            DataListSingleton.SetDataList(_dataListViewModel);
            return (_mockResourceModel, _dataListViewModel);
        }

        #endregion Initialize

        #region AddMode Missing Tests

        [TestMethod]
        public void AddMissingDataListItems_AddRecordSet_ExpectedNewRecordSetCreatedonRootNode()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("Province");
            part.Setup(c => c.DisplayValue).Returns("[[Province]]");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.Field).Returns("");
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts);
            Assert.IsTrue(_dataListViewModel.RecsetCollectionCount == 3);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void AddMissingDataListItems_AddRecordSet_WithDuplicateName_ExpectedError()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var scalarPart = new Mock<IDataListVerifyPart>();
            scalarPart.Setup(c => c.Field).Returns("Province");
            scalarPart.Setup(c => c.DisplayValue).Returns("[[Province]]");
            scalarPart.Setup(c => c.Description).Returns("A state in a republic");
            scalarPart.Setup(c => c.IsScalar).Returns(true);
            parts.Add(scalarPart.Object);

            var recordSetPart = new Mock<IDataListVerifyPart>();
            recordSetPart.Setup(c => c.Recordset).Returns("Province");
            recordSetPart.Setup(c => c.DisplayValue).Returns("[[Province]]");
            recordSetPart.Setup(c => c.Description).Returns("A state in a republic");
            recordSetPart.Setup(c => c.IsScalar).Returns(false);
            recordSetPart.Setup(c => c.Field).Returns("name");
            parts.Add(recordSetPart.Object);

            _dataListViewModel.AddMissingDataListItems(parts);
            Assert.IsTrue(_dataListViewModel.RecsetCollectionCount == 3);
            var duplicateRecSet = _dataListViewModel.RecsetCollection.FirstOrDefault(o => o.DisplayName == "Province" && o.HasError);
            Assert.IsNotNull(duplicateRecSet);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void AddMissingDataListItems_AddVariables_WithDuplicateName_ValidateExactErrors()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var scalarPart = new Mock<IDataListVerifyPart>();
            scalarPart.Setup(c => c.Field).Returns("Province");
            scalarPart.Setup(c => c.DisplayValue).Returns("Province");
            scalarPart.Setup(c => c.Description).Returns("A state in a republic");
            scalarPart.Setup(c => c.IsScalar).Returns(true);
            scalarPart.Setup(c => c.IsJson).Returns(false);
            parts.Add(scalarPart.Object);

            var recordSetPart = new Mock<IDataListVerifyPart>();
            recordSetPart.Setup(c => c.Recordset).Returns("Province");
            recordSetPart.Setup(c => c.DisplayValue).Returns("Province");
            recordSetPart.Setup(c => c.Description).Returns("A state in a republic");
            recordSetPart.Setup(c => c.IsScalar).Returns(false);
            recordSetPart.Setup(c => c.IsJson).Returns(false);
            recordSetPart.Setup(c => c.Field).Returns("name");
            parts.Add(recordSetPart.Object);

            var objectPart = new Mock<IDataListVerifyPart>();
            objectPart.Setup(c => c.DisplayValue).Returns("Province.name");
            objectPart.Setup(c => c.Description).Returns("A state in a republic");
            objectPart.Setup(c => c.IsScalar).Returns(false);
            objectPart.Setup(c => c.IsJson).Returns(true);
            parts.Add(objectPart.Object);

            // validate all variables have 'HasError' is true
            _dataListViewModel.AddMissingDataListItems(parts);

            var duplicateScalar = _dataListViewModel.ScalarCollection.FirstOrDefault(o => o.DisplayName == "Province" && o.HasError);
            Assert.IsNotNull(duplicateScalar);
            var duplicateRecSet = _dataListViewModel.RecsetCollection.FirstOrDefault(o => o.DisplayName == "Province" && o.HasError);
            Assert.IsNotNull(duplicateRecSet);
            var duplicateObject = _dataListViewModel.ComplexObjectCollection.FirstOrDefault(o => o.DisplayName == "Province" && o.HasError);
            Assert.IsNotNull(duplicateObject);

            // remove scalar and validate recordset and object 'HasError' is true
            _dataListViewModel.ScalarCollection.Remove(duplicateScalar);

            duplicateScalar = _dataListViewModel.ScalarCollection.FirstOrDefault(o => o.DisplayName == "Province");
            Assert.IsNull(duplicateScalar);
            duplicateRecSet = _dataListViewModel.RecsetCollection.FirstOrDefault(o => o.DisplayName == "Province" && o.HasError);
            Assert.IsNotNull(duplicateRecSet);
            duplicateObject = _dataListViewModel.ComplexObjectCollection.FirstOrDefault(o => o.DisplayName == "Province" && o.HasError);
            Assert.IsNotNull(duplicateObject);

            // remove recordset and validate object 'HasError' is false
            _dataListViewModel.RecsetCollection.Remove(duplicateRecSet);

            duplicateScalar = _dataListViewModel.ScalarCollection.FirstOrDefault(o => o.DisplayName == "Province");
            Assert.IsNull(duplicateScalar);
            duplicateRecSet = _dataListViewModel.RecsetCollection.FirstOrDefault(o => o.DisplayName == "Province");
            Assert.IsNull(duplicateRecSet);
            duplicateObject = _dataListViewModel.ComplexObjectCollection.FirstOrDefault(o => o.DisplayName == "Province" && !o.HasError);
            Assert.IsNotNull(duplicateObject);
        }

        [TestMethod]
        public void AddMissingDataListItems_AddRecordSetWhenDataListContainsScalarWithSameName()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();
            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("Province");
            part.Setup(c => c.DisplayValue).Returns("[[Province]]");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.IsJson).Returns(false);
            part.Setup(c => c.Field).Returns("");
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts);
            _dataListViewModel.AddMissingDataListItems(parts);
            Assert.AreEqual(5, _dataListViewModel.DataList.Count);
            Assert.IsTrue(!_dataListViewModel.DataList[3].HasError);
        }

        [TestMethod]
        public void AddMissingScalarItemWhereItemsAlreadyExistsInDataListExpectedNoItemsAdded()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();

            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Field).Returns("Province");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(true);
            part.Setup(c => c.IsJson).Returns(false);
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts);
            //Second add trying to add the same items to the data list again
            _dataListViewModel.AddMissingDataListItems(parts);
            Assert.AreEqual("Country", _dataListViewModel.ScalarCollection[0].DisplayName);
            Assert.AreEqual("Province", _dataListViewModel.ScalarCollection[1].DisplayName);
            Assert.AreEqual(string.Empty, _dataListViewModel.ScalarCollection[2].DisplayName);
            Assert.AreEqual("Car", _dataListViewModel.RecsetCollection[0].DisplayName);
        }

        [TestMethod]
        public void AddMissingRecordsetItemWhereItemsAlreadyExistsInDataListExpectedNoItemsAdded()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();

            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("Province");
            part.Setup(c => c.DisplayValue).Returns("[[Province]]");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.Field).Returns("");
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts);
            //Second add trying to add the same items to the data list again
            _dataListViewModel.AddMissingDataListItems(parts);
            Assert.AreEqual(3, _dataListViewModel.RecsetCollectionCount);
            Assert.AreEqual("Country", _dataListViewModel.ScalarCollection[0].DisplayName);
            Assert.AreEqual(string.Empty, _dataListViewModel.ScalarCollection[1].DisplayName);
            Assert.AreEqual("Car", _dataListViewModel.RecsetCollection[0].DisplayName);
            Assert.AreEqual("Province", _dataListViewModel.RecsetCollection[1].DisplayName);
        }

        [TestMethod]
        public void AddMissingRecordsetChildItemWhereItemsAlreadyExistsInDataListExpectedNoItemsAdded()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();

            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("Province");
            part.Setup(c => c.DisplayValue).Returns("[[Province]]");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.Field).Returns("field1");
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts);
            //Second add trying to add the same items to the data list again            
            _dataListViewModel.AddMissingDataListItems(parts);
            Assert.AreEqual(2, _dataListViewModel.RecsetCollection[1].Children.Count);
            Assert.AreEqual("field1", _dataListViewModel.RecsetCollection[1].Children[0].DisplayName);
        }
        [TestMethod]
        public void WriteDataListToResourceModel_ShouldContainAllVariables()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var personObject = new ComplexObjectItemModel("Person");
            personObject.Children.Add(new ComplexObjectItemModel("Age"));
            personObject.Children.Add(new ComplexObjectItemModel("Name"));
            var schoolObject = new ComplexObjectItemModel("School");
            schoolObject.Children.Add(new ComplexObjectItemModel("Name"));
            schoolObject.Children.Add(new ComplexObjectItemModel("Location"));
            personObject.Children.Add(schoolObject);
            _dataListViewModel.Add(personObject);

            const string expectedResult = @"<DataList><Car Description=""A recordset of information about a car"" IsEditable=""True"" ColumnIODirection=""Both"" ><Make Description=""Make of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" /><Model Description=""Model of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" /></Car><Country Description=""name of Country"" IsEditable=""True"" ColumnIODirection=""Both"" /><Person Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ><Age Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Age><Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name><School Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ><Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name><Location Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Location></School></Person></DataList>";
            StringAssert.Contains(expectedResult, @"<Car Description=""A recordset of information about a car"" IsEditable=""True"" ColumnIODirection=""Both"" ><Make Description=""Make of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" /><Model Description=""Model of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" /></Car><Country Description=""name of Country"" IsEditable=""True"" ColumnIODirection=""Both"" />");
            StringAssert.Contains(expectedResult, @"<Country Description=""name of Country"" IsEditable=""True"" ColumnIODirection=""Both"" />");
            StringAssert.Contains(expectedResult, @"<Person Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ><Age Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Age><Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name><School Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ><Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name><Location Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Location></School></Person>");
        }

        [TestMethod]
        public void WriteDataListToResourceModel_WithJsonArray_ShouldContainAllVariablesWithJsonArraySetTrue()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var personObject = new ComplexObjectItemModel("Person");
            personObject.Children.Add(new ComplexObjectItemModel("Age"));
            personObject.Children.Add(new ComplexObjectItemModel("Name"));
            var schoolObject = new ComplexObjectItemModel("Schools") { IsArray = true };
            schoolObject.Children.Add(new ComplexObjectItemModel("Name"));
            schoolObject.Children.Add(new ComplexObjectItemModel("Location"));
            personObject.Children.Add(schoolObject);
            _dataListViewModel.Add(personObject);

            const string expectedResult = @"<DataList><Car Description=""A recordset of information about a car"" IsEditable=""True"" ColumnIODirection=""Both"" ><Make Description=""Make of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" /><Model Description=""Model of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" /></Car><Country Description=""name of Country"" IsEditable=""True"" ColumnIODirection=""Both"" /><Person Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ><Age Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Age><Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name><Schools Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""True"" ColumnIODirection=""None"" ><Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name><Location Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Location></Schools></Person></DataList>";
            StringAssert.Contains(expectedResult, @"<Car Description=""A recordset of information about a car"" IsEditable=""True"" ColumnIODirection=""Both"" ><Make Description=""Make of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" /><Model Description=""Model of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" /></Car><Country Description=""name of Country"" IsEditable=""True"" ColumnIODirection=""Both"" />");
            StringAssert.Contains(expectedResult, @"<Country Description=""name of Country"" IsEditable=""True"" ColumnIODirection=""Both"" />");
            StringAssert.Contains(expectedResult, @"<Person Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ><Age Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Age><Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name><Schools Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""True"" ColumnIODirection=""None"" ><Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name><Location Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Location></Schools></Person>");
        }

        [TestMethod]
        public void ConvertDataListStringToCollections_DataListWithComplexObject_ShouldPopulateComplexObject()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            const string expectedResult = @"<DataList><Car Description=""A recordset of information about a car"" IsEditable=""True"" ColumnIODirection=""Both"" ><Make Description=""Make of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" /><Model Description=""Model of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" /></Car><Country Description=""name of Country"" IsEditable=""True"" ColumnIODirection=""Both"" /><Person Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ><Age Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Age><Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name><Schools Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ><Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name><Location Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Location></Schools></Person></DataList>";
            _dataListViewModel.ConvertDataListStringToCollections(expectedResult);
            Assert.IsNotNull(_dataListViewModel.ComplexObjectCollection);
            var personObject = _dataListViewModel.ComplexObjectCollection.FirstOrDefault(model => model.Name == "@Person");
            Assert.IsNotNull(personObject);
            Assert.IsNotNull(personObject.Children);
            Assert.IsNotNull(personObject.Children.FirstOrDefault(model => model.DisplayName == "Name"));
            Assert.IsNotNull(personObject.Children.FirstOrDefault(model => model.DisplayName == "Age"));
            var schools = personObject.Children.FirstOrDefault(model => model.DisplayName == "Schools");
            Assert.IsNotNull(schools);
            Assert.IsFalse(schools.IsArray);
            Assert.IsNotNull(schools.Children.FirstOrDefault(model => model.DisplayName == "Name"));
            Assert.IsNotNull(schools.Children.FirstOrDefault(model => model.DisplayName == "Location"));
        }

        [TestMethod]
        public void ConvertDataListStringToCollections_DataListWithComplexObjectHasArray_ShouldPopulateComplexObjectSetNameWithBrackets()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            const string expectedResult = @"<DataList><Car Description=""A recordset of information about a car"" IsEditable=""True"" ColumnIODirection=""Both"" ><Make Description=""Make of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" /><Model Description=""Model of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" /></Car><Country Description=""name of Country"" IsEditable=""True"" ColumnIODirection=""Both"" /><Person Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ><Age Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Age><Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name><Schools Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""True"" ColumnIODirection=""None"" ><Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name><Location Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Location></Schools></Person></DataList>";
            _dataListViewModel.ConvertDataListStringToCollections(expectedResult);
            Assert.IsNotNull(_dataListViewModel.ComplexObjectCollection);
            var personObject = _dataListViewModel.ComplexObjectCollection.FirstOrDefault(model => model.Name == "@Person");
            Assert.IsNotNull(personObject);
            Assert.IsNotNull(personObject.Children);
            Assert.IsNotNull(personObject.Children.FirstOrDefault(model => model.DisplayName == "Name"));
            Assert.IsNotNull(personObject.Children.FirstOrDefault(model => model.DisplayName == "Age"));
            var schools = personObject.Children.FirstOrDefault(model => model.DisplayName == "Schools()");
            Assert.IsNotNull(schools);
            Assert.IsTrue(schools.IsArray);
            Assert.IsNotNull(schools.Children.FirstOrDefault(model => model.DisplayName == "Name"));
            Assert.IsNotNull(schools.Children.FirstOrDefault(model => model.DisplayName == "Location"));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ConvertDataListStringToCollections_DataListWithAddScalarWithSearchText_ShouldPopulateScalarSetNameWithBrackets()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();


            _dataListViewModel.SearchText = "Name";
            var scalarHandler = new ScalarHandler(_dataListViewModel);

            const string dataList = @"<DataList><Name Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><Message Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /></DataList>";

            var xDoc = new XmlDocument();
            xDoc.LoadXml(dataList);
            if (xDoc.DocumentElement == null)
            {
                return;
            }

            var children = xDoc.DocumentElement.ChildNodes;
            foreach (XmlNode child in children)
            {
                scalarHandler.AddScalars(child);
            }

            Assert.IsNotNull(_dataListViewModel.ScalarCollection);
            Assert.AreEqual(3, _dataListViewModel.ScalarCollectionCount);
            var nameScalar = _dataListViewModel.ScalarCollection.FirstOrDefault(model => model.Name == "Name");
            Assert.IsNotNull(nameScalar);
            Assert.IsTrue(nameScalar.IsVisible);
            var messageScalar = _dataListViewModel.ScalarCollection.FirstOrDefault(model => model.Name == "Message");
            Assert.IsNotNull(messageScalar);
            Assert.IsFalse(messageScalar.IsVisible);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ConvertDataListStringToCollections_DataListWithAddScalarWithNullAttributeAndSearchText_ShouldPopulateScalarSetNameWithBrackets()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part2 = new Mock<IDataListVerifyPart>();
            part2.Setup(c => c.Field).Returns("Message");
            part2.Setup(c => c.Description).Returns("Duplicate testing");
            part2.Setup(c => c.IsScalar).Returns(true);
            parts.Add(part2.Object);
            var dataListItemModels = CreateScalarListItems(parts);
            foreach (var dataListItemModel in dataListItemModels)
            {
                _dataListViewModel.Add(dataListItemModel);
            }

            _dataListViewModel.SearchText = "#text";
            var scalarHandler = new ScalarHandler(_dataListViewModel);

            var xDoc = new XmlDocument();
            var mockNode = xDoc.CreateTextNode("InnerText");

            scalarHandler.AddScalars(mockNode);

            Assert.IsNotNull(_dataListViewModel.ScalarCollection);
            Assert.AreEqual(3, _dataListViewModel.ScalarCollectionCount);
            var textScalar = _dataListViewModel.ScalarCollection.FirstOrDefault(model => model.Name == "#text");
            Assert.IsNotNull(textScalar);
            var messageScalar = _dataListViewModel.ScalarCollection.FirstOrDefault(model => model.Name == "Message");
            Assert.IsNotNull(messageScalar);
            Assert.IsFalse(messageScalar.IsVisible);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void AddRecordSetNamesIfMissing_FixNamingIfContainsDot_Expected_Change()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();


            const string recsetName = "recset";
            const string firstFieldName = "f1.";
            var recSetDataModel = CreateRecsetDataListModelWithTwoFields(recsetName, firstFieldName, "f2");
            _dataListViewModel.Add(recSetDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateRecsetPart(recsetName, firstFieldName);
            dataListParts.Add(part.Object);
            _dataListViewModel.RecsetCollection[1].Children[0].DisplayName = ".f1";
            _dataListViewModel.AddRecordsetNamesIfMissing();

            Assert.AreEqual(2, _dataListViewModel.RecsetCollectionCount);
            Assert.AreEqual("f1", _dataListViewModel.RecsetCollection[1].Children[0].DisplayName);
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ConvertDataListStringToCollections_DataListWithAddScalarWithNullAttribute_ShouldPopulateScalarSetNameWithBrackets()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();


            const string dataList = @"<DataList><Name Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><Message Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /></DataList>";

            var xDoc = new XmlDocument();
            xDoc.LoadXml(dataList);
            if (xDoc.DocumentElement == null)
            {
                return;
            }
            var mockNode = xDoc.CreateTextNode("InnerText");
            xDoc.DocumentElement.AppendChild(mockNode);

            var scalarHandler = new ScalarHandler(_dataListViewModel);
            var children = xDoc.DocumentElement.ChildNodes;
            foreach (XmlNode child in children)
            {
                scalarHandler.AddScalars(child);
            }

            Assert.IsNotNull(_dataListViewModel.ScalarCollection);
            Assert.AreEqual(4, _dataListViewModel.ScalarCollectionCount);
            var nameScalar = _dataListViewModel.ScalarCollection.FirstOrDefault(model => model.Name == "Name");
            Assert.IsNotNull(nameScalar);
            Assert.IsTrue(nameScalar.IsVisible);
            var messageScalar = _dataListViewModel.ScalarCollection.FirstOrDefault(model => model.Name == "Message");
            Assert.IsNotNull(messageScalar);
            Assert.IsTrue(messageScalar.IsVisible);
            var textScalar = _dataListViewModel.ScalarCollection.FirstOrDefault(model => model.Name == "#text");
            Assert.IsNotNull(textScalar);
        }
        #endregion AddMode Missing Tests

        #region RemoveUnused Tests

        [TestMethod]
        public void RemoveUnusedDataListItems_RemoveScalars_ExpectedItemRemovedFromDataList()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Field).Returns("testing");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(true);
            parts.Add(part.Object);

            // Mock Setup            

            //Juries 8810 TODO
            _dataListViewModel.AddMissingDataListItems(parts);
            var beforeCount = _dataListViewModel.DataList.Count;
            parts.Add(part.Object);
            _dataListViewModel.SetIsUsedDataListItems(parts, false);
            _dataListViewModel.RemoveUnusedDataListItems();
            var afterCount = _dataListViewModel.DataList.Count;
            Assert.IsTrue(beforeCount > afterCount);
        }

        [TestMethod]
        public void SetUnusedDataListItemsWhenTwoScalarsSameNameExpectedBothMarkedAsUnused()
        {
            //---------------------------Setup----------------------------------------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

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
            var dataListItemModels = CreateScalarListItems(parts);
            foreach (var dataListItemModel in dataListItemModels)
            {
                _dataListViewModel.Add(dataListItemModel);
            }

            //-------------------------Execute Test ------------------------------------------
            _dataListViewModel.SetIsUsedDataListItems(parts, false);
            //-------------------------Assert Resule------------------------------------------
            var actual = _dataListViewModel.DataList.Count(model => !model.IsUsed && !string.IsNullOrEmpty(model.DisplayName));
            Assert.AreEqual(2, actual);
        }

        [TestMethod]
        public void Sort_ScalarCollection()
        {
            //---------------------------Setup----------------------------------------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            _dataListViewModel.Add(DataListItemModelFactory.CreateScalarItemModel("Name", "your firstname", enDev2ColumnArgumentDirection.Both));
            _dataListViewModel.Add(DataListItemModelFactory.CreateScalarItemModel("Surname", "your lastname", enDev2ColumnArgumentDirection.Both));
            _dataListViewModel.Add(DataListItemModelFactory.CreateScalarItemModel("Age", "your age", enDev2ColumnArgumentDirection.Both));
            Assert.AreEqual("Country", _dataListViewModel.ScalarCollection[0].Name);
            Assert.AreEqual("Name", _dataListViewModel.ScalarCollection[1].Name);
            Assert.AreEqual("Surname", _dataListViewModel.ScalarCollection[2].Name);
            Assert.AreEqual("Age", _dataListViewModel.ScalarCollection[3].Name);
            //-------------------------Execute Test ------------------------------------------
            _dataListViewModel.SortCommand.Execute(null);
            //-------------------------Assert Result------------------------------------------
            Assert.AreEqual("Age", _dataListViewModel.ScalarCollection[0].Name);
            Assert.AreEqual("Country", _dataListViewModel.ScalarCollection[1].Name);
            Assert.AreEqual("Name", _dataListViewModel.ScalarCollection[2].Name);
            Assert.AreEqual("Surname", _dataListViewModel.ScalarCollection[3].Name);
        }

        [TestMethod]
        public void Sort_RecordSetCollection()
        {
            //---------------------------Setup----------------------------------------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            _dataListViewModel.Add(new RecordSetItemModel("Name"));
            _dataListViewModel.Add(new RecordSetItemModel("Surname"));
            _dataListViewModel.Add(new RecordSetItemModel("Age"));
            _dataListViewModel.Add(new RecordSetItemModel("Race"));
            Assert.AreEqual("Car", _dataListViewModel.RecsetCollection[0].Name);
            Assert.AreEqual("Name", _dataListViewModel.RecsetCollection[1].Name);
            Assert.AreEqual("Surname", _dataListViewModel.RecsetCollection[2].Name);
            Assert.AreEqual("Age", _dataListViewModel.RecsetCollection[3].Name);
            Assert.AreEqual("Race", _dataListViewModel.RecsetCollection[4].Name);
            //-------------------------Execute Test ------------------------------------------
            _dataListViewModel.SortCommand.Execute(null);
            //-------------------------Assert Result------------------------------------------
            Assert.AreEqual("Age", _dataListViewModel.RecsetCollection[0].DisplayName);
            Assert.AreEqual("Car", _dataListViewModel.RecsetCollection[1].DisplayName);
            Assert.AreEqual("Name", _dataListViewModel.RecsetCollection[2].DisplayName);
            Assert.AreEqual("Race", _dataListViewModel.RecsetCollection[3].DisplayName);
            Assert.AreEqual("Surname", _dataListViewModel.RecsetCollection[4].DisplayName);
        }

        [TestMethod]
        public void Sort_RecordSetFieldsCollection()
        {
            //---------------------------Setup----------------------------------------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var car = _dataListViewModel.RecsetCollection.Single();
            car.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Driver", "Whos is Driving it", car));
            car.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Year_Bought", "When the car was bought", car));
            car.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Customer", "Customer who bought the car", car));
            car.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Sales_Agent", "Sales person", car));
            Assert.AreEqual("Make", car.Children[0].DisplayName);
            Assert.AreEqual("Model", car.Children[1].DisplayName);
            Assert.AreEqual("Driver", car.Children[2].DisplayName);
            Assert.AreEqual("Year_Bought", car.Children[3].DisplayName);
            Assert.AreEqual("Customer", car.Children[4].DisplayName);
            Assert.AreEqual("Sales_Agent", car.Children[5].DisplayName);
            //-------------------------Execute Test ------------------------------------------
            _dataListViewModel.SortCommand.Execute(null);
            //-------------------------Assert Result------------------------------------------
            Assert.AreEqual("Customer", car.Children[0].DisplayName);
            Assert.AreEqual("Driver", car.Children[1].DisplayName);
            Assert.AreEqual("Make", car.Children[2].DisplayName);
            Assert.AreEqual("Model", car.Children[3].DisplayName);
            Assert.AreEqual("Sales_Agent", car.Children[4].DisplayName);
            Assert.AreEqual("Year_Bought", car.Children[5].DisplayName);
        }

        [TestMethod]
        public void SetUnusedDataListItemsWhenTwoRecsetsSameNameExpectedBothMarkedAsUnused()
        {
            //---------------------------Setup----------------------------------------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

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

            IRecordSetItemModel mod = new RecordSetItemModel("testing");
            mod.Children.Add(new RecordSetFieldItemModel("f1", mod));
            IRecordSetItemModel mod2 = new RecordSetItemModel("testing");
            mod2.Children.Add(new RecordSetFieldItemModel("f2", mod2));

            _dataListViewModel.Add(mod);
            _dataListViewModel.Add(mod2);

            //-------------------------Execute Test ------------------------------------------
            _dataListViewModel.SetIsUsedDataListItems(parts, false);
            //-------------------------Assert Resule------------------------------------------
            var actual = _dataListViewModel.DataList.Count(model => !model.IsUsed);
            Assert.AreEqual(2, actual);
        }

        [TestMethod]
        public void RemoveUnusedDataListItems_RemoveMalformedScalar_ExpectedItemNotRemovedFromDataList()
        {
            //TO DO: Implement Logic for the AddMode Malformed Scalar test method
        }

        [TestMethod]
        public void RemoveUnusedDataListItems_RemoveMalformedRecordSet_ExpectedRecordSetRemove()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("Province");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(false);
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts);
            var beforeCount = _dataListViewModel.DataList.Count;
            parts.Add(part.Object);
            _dataListViewModel.SetIsUsedDataListItems(parts, false);
            _dataListViewModel.RemoveUnusedDataListItems();
            var afterCount = _dataListViewModel.DataList.Count;
            Assert.IsTrue(beforeCount > afterCount);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_HasUnusedDataListItems")]
        public void DataListViewModel_HasUnusedDataListItems_RemoveMalformedRecordSet_ExpectedRecordSetRemove()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("Province");
            part.Setup(c => c.Description).Returns("A state in a republic");
            part.Setup(c => c.IsScalar).Returns(false);
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts);
            var beforeCount = _dataListViewModel.DataList.Count;
            parts.Add(part.Object);
            _dataListViewModel.SetIsUsedDataListItems(parts, false);

            var canExec = _dataListViewModel.FindUnusedAndMissingCommand.CanExecute(_dataListViewModel);
            Assert.IsTrue(canExec);
            _dataListViewModel.FindUnusedAndMissingCommand.Execute(_dataListViewModel);
            var afterCount = _dataListViewModel.DataList.Count;
            Assert.IsTrue(beforeCount > afterCount);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("DataListViewModel_HasUnusedDataListItems")]
        public void DataListViewModel_HasOpenBracket_BeforeAnyClosing_ExpectedComplexObjectNotToBeAdded()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.DisplayValue).Returns("type)()");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.IsJson).Returns(true);
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts);
            parts.Add(part.Object);
            _dataListViewModel.SetIsUsedDataListItems(parts, false);
            Assert.AreEqual(0, _dataListViewModel.ComplexObjectCollectionCount);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("DataListViewModel_HasUnusedDataListItems")]
        public void DataListViewModel_NameAroundBracket_ExpectedComplexObjectToBeAdded()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.DisplayValue).Returns("(type())");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.IsJson).Returns(true);
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts);
            parts.Add(part.Object);
            Assert.AreEqual(1, _dataListViewModel.ComplexObjectCollectionCount);
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_HasUnusedDataListItems")]
        public void DataListViewModel_HasUnusedDataListItems_RemoveMalformedComplexObject_ExpectedComplexObjectRemove()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.DisplayValue).Returns("Parent.School()");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.IsJson).Returns(true);
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts);
            var beforeCount = _dataListViewModel.DataList.Count;
            parts.Add(part.Object);
            _dataListViewModel.SetIsUsedDataListItems(parts, false);
            _dataListViewModel.ComplexObjectCollection[0].IsUsed = false;

            var canExec = _dataListViewModel.FindUnusedAndMissingCommand.CanExecute(_dataListViewModel);
            Assert.IsTrue(canExec);
            _dataListViewModel.FindUnusedAndMissingCommand.Execute(_dataListViewModel);
            var afterCount = _dataListViewModel.DataList.Count;
            Assert.AreEqual(beforeCount, afterCount);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_HasNoUnusedDataListItems")]
        public void DataListViewModel_HasNoUnusedDataListItems_NoMalformedComplexObject_ExpectedComplexObjectSame()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.DisplayValue).Returns("Parent.School()");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.IsJson).Returns(true);
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts);
            var beforeCount = _dataListViewModel.DataList.Count;
            parts.Add(part.Object);
            _dataListViewModel.SetIsUsedDataListItems(parts, false);

            var canExec = _dataListViewModel.FindUnusedAndMissingCommand.CanExecute(_dataListViewModel);
            Assert.IsFalse(canExec);
            _dataListViewModel.FindUnusedAndMissingCommand.Execute(_dataListViewModel);
            var afterCount = _dataListViewModel.DataList.Count;
            Assert.AreEqual(beforeCount, afterCount);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_Handle")]
        public void DataListViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------  
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            //------------Execute Test---------------------------
            _dataListViewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_AddMissingDataListItems")]
        public void DataListViewModel_AddMissingDataListItems_ComplexObject_ExpectedAddComplexObjectIsArray()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();

            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.DisplayValue).Returns("Parent.School()");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.IsJson).Returns(true);
            parts.Add(part.Object);

            _dataListViewModel.AddMissingDataListItems(parts);
            //Second add trying to add the same items to the data list again            
            _dataListViewModel.AddMissingDataListItems(parts);
            Assert.AreEqual(1, _dataListViewModel.ComplexObjectCollection[0].Children.Count);
            Assert.AreEqual("School()", _dataListViewModel.ComplexObjectCollection[0].Children[0].DisplayName);
            Assert.IsTrue(_dataListViewModel.ComplexObjectCollection[0].Children[0].IsArray);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_GenerateComplexObject")]
        public void DataListViewModel_GenerateComplexObject_FromJson()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            Assert.AreEqual(0, _dataListViewModel.ComplexObjectCollectionCount);

            const string jsonString = "{\"Name\":\"\",\"Age\":\"\",\"School\":[{\"Name\":\"\",\"Location\":\"\"}],\"Gender\":\"\"}";
            _dataListViewModel.GenerateComplexObjectFromJson("@Person", jsonString);

            Assert.AreEqual(1, _dataListViewModel.ComplexObjectCollectionCount);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_GenerateComplexObject")]
        public void DataListViewModel_GenerateComplexObject_FromXML_ShouldCreateComplexObjectVariables()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            Assert.AreEqual(0, _dataListViewModel.ComplexObjectCollectionCount);

            const string xmlString = "<Person><Name></Name><Age></Age><School><Name></Name><Location></Location></School><Gender></Gender></Person>";
            _dataListViewModel.GenerateComplexObjectFromJson("@Person", xmlString);

            Assert.AreEqual(1, _dataListViewModel.ComplexObjectCollectionCount);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_GenerateComplexObject")]
        public void DataListViewModel_GenerateComplexObject_ParentContainsDotFromJson()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            Assert.AreEqual(0, _dataListViewModel.ComplexObjectCollectionCount);

            const string jsonString = "{\"Name\":\"\",\"Age\":\"\",\"School\":[{\"Name\":\"\",\"Location\":\"\"}],\"Gender\":\"\"}";
            _dataListViewModel.GenerateComplexObjectFromJson("@Persons.Person", jsonString);

            Assert.AreEqual(1, _dataListViewModel.ComplexObjectCollectionCount);
        }

        #endregion RemoveUnused Tests

        #region RemoveRowIfEmpty Tests

        [TestMethod]
        public void RemoveRowIfEmpty_ExpectedCountofDataListItemsReduceByOne()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            _dataListViewModel.AddBlankRow(new DataListItemModel("Test"));
            var beforeCount = _dataListViewModel.ScalarCollectionCount;
            _dataListViewModel.ScalarCollection[0].Description = string.Empty;
            _dataListViewModel.ScalarCollection[0].DisplayName = string.Empty;
            _dataListViewModel.RemoveBlankRows(_dataListViewModel.ScalarCollection[0]);
            var afterCount = _dataListViewModel.ScalarCollectionCount;

            Assert.IsTrue(beforeCount > afterCount);
        }

        #endregion RemoveRowIfEmpty Tests

        #region AddRowIfAllCellsHaveData Tests

        /// <summary>
        ///     Testing that there is always a blank row in the data list
        /// </summary>
        [TestMethod]
        public void AddRowIfAllCellsHaveData_AllDataListRowsContainingData_Expected_RowAdded()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var beforeCount = _dataListViewModel.DataList.Count;
            _dataListViewModel.AddBlankRow(_dataListViewModel.ScalarCollection[0]);
            var afterCount = _dataListViewModel.DataList.Count;
            Assert.IsTrue(afterCount >= beforeCount);
        }

        /// <summary>
        ///     Tests that no rows are added to the datalistItem collection if there is already a blank row
        /// </summary>
        [TestMethod]
        public void AddRowIfAllCellsHaveData_BlankRowAlreadyExists_Expected_NoRowsAdded()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            _dataListViewModel.AddBlankRow(_dataListViewModel.ScalarCollection[0]);
            var beforeCount = _dataListViewModel.DataList.Count;
            _dataListViewModel.AddBlankRow(_dataListViewModel.ScalarCollection[0]);
            var afterCount = _dataListViewModel.DataList.Count;

            Assert.AreEqual(beforeCount, afterCount);
        }

        #endregion AddRowIfAllCellsHaveData Tests

        #region AddRecordsetNamesIfMissing Tests

        [TestMethod]
        public void AddRecordSetNamesIfMissing_DataListContainingRecordSet_Expected_Positive()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            _dataListViewModel.AddRecordsetNamesIfMissing();

            Assert.AreEqual(1, _dataListViewModel.RecsetCollectionCount);
            Assert.AreEqual("Make", _dataListViewModel.RecsetCollection[0].Children[0].DisplayName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_AddRecordSetNamesIfMissing")]
        public void AddRecordSetNamesIfMissing_DataListContainingRecordSet_WithDoubleBracketedRecsetName_Expected_Positive()
        {

            //------------Setup for test--------------------------

            var _mockResourceModel = Dev2MockFactory.SetupResourceModelMock();

            var _dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            _dataListViewModel.InitializeDataListViewModel(_mockResourceModel.Object);
            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();

            var carRecordset = DataListItemModelFactory.CreateRecordSetItemModel("[[Car]]", "A recordset of information about a car");
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Make", "Make of vehicle", carRecordset));
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Model", "Model of vehicle", carRecordset));

            _dataListViewModel.Add(carRecordset);
            _dataListViewModel.Add(DataListItemModelFactory.CreateScalarItemModel("Country", "name of Country"));

            DataListSingleton.SetDataList(_dataListViewModel);
            //------------Execute Test---------------------------
            _dataListViewModel.AddRecordsetNamesIfMissing();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, _dataListViewModel.RecsetCollectionCount);
            Assert.AreEqual("Car", _dataListViewModel.RecsetCollection[0].DisplayName);
            Assert.AreEqual("Make", _dataListViewModel.RecsetCollection[0].Children[0].DisplayName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_AddRecordSetNamesIfMissing")]
        public void AddRecordSetNamesIfMissing_DataListContainingRecordSet_WithSingleBracketedRecsetName_Expected_Positive()
        {

            //------------Setup for test--------------------------

            var _mockResourceModel = Dev2MockFactory.SetupResourceModelMock();

            var _dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            _dataListViewModel.InitializeDataListViewModel(_mockResourceModel.Object);
            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();

            var carRecordset = DataListItemModelFactory.CreateRecordSetItemModel("[Car]", "A recordset of information about a car");
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Make", "Make of vehicle", carRecordset));
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Model", "Model of vehicle", carRecordset));

            _dataListViewModel.Add(carRecordset);
            _dataListViewModel.Add(DataListItemModelFactory.CreateScalarItemModel("Country", "name of Country"));

            DataListSingleton.SetDataList(_dataListViewModel);
            //------------Execute Test---------------------------
            _dataListViewModel.AddRecordsetNamesIfMissing();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, _dataListViewModel.RecsetCollectionCount);
            Assert.AreEqual("Car", _dataListViewModel.RecsetCollection[0].DisplayName);
            Assert.AreEqual("Make", _dataListViewModel.RecsetCollection[0].Children[0].DisplayName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_AddRecordSetNamesIfMissing")]
        public void AddRecordSetNamesIfMissing_DataListContainingRecordSet_WithDoubleBracketedScalar_Expected_Positive()
        {

            //------------Setup for test--------------------------

            var _mockResourceModel = Dev2MockFactory.SetupResourceModelMock();

            var _dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            _dataListViewModel.InitializeDataListViewModel(_mockResourceModel.Object);
            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();

            var carRecordset = DataListItemModelFactory.CreateRecordSetItemModel("[[Car]]", "A recordset of information about a car");
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Make", "Make of vehicle", carRecordset));
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Model", "Model of vehicle", carRecordset));

            _dataListViewModel.Add(carRecordset);
            _dataListViewModel.Add(DataListItemModelFactory.CreateScalarItemModel("[[Country]]", "name of Country"));

            DataListSingleton.SetDataList(_dataListViewModel);
            //------------Execute Test---------------------------
            _dataListViewModel.WriteToResourceModel();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, _dataListViewModel.ScalarCollectionCount);
            Assert.AreEqual("Country", _dataListViewModel.ScalarCollection[0].DisplayName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_WriteResourceModel")]
        public void WriteResourceModel_DataListContainingScalarWithError_WithDoubleBracketedScalar_Expected_Positive()
        {

            //------------Setup for test--------------------------

            var _mockResourceModel = Dev2MockFactory.SetupResourceModelMock();

            var _dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            _dataListViewModel.InitializeDataListViewModel(_mockResourceModel.Object);
            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();

            var carRecordset = DataListItemModelFactory.CreateRecordSetItemModel("[[Car]]", "A recordset of information about a car");
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Make", "Make of vehicle", carRecordset));
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Model", "Model of vehicle", carRecordset));

            _dataListViewModel.Add(carRecordset);

            var scalarDataListItemWithError = DataListItemModelFactory.CreateScalarItemModel("[[Country]]", "name of Country");
            scalarDataListItemWithError.HasError = true;
            scalarDataListItemWithError.ErrorMessage = "This is an Error";
            _dataListViewModel.Add(scalarDataListItemWithError);
            _dataListViewModel.Add(scalarDataListItemWithError);
            _dataListViewModel.ValidateVariableNamesForUI(scalarDataListItemWithError);
            DataListSingleton.SetDataList(_dataListViewModel);
            //------------Execute Test---------------------------
            var xmlDataList = _dataListViewModel.WriteToResourceModel();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, _dataListViewModel.ScalarCollectionCount);
            Assert.IsTrue(_dataListViewModel.ScalarCollection[0].DisplayName == "Country");
            Assert.IsTrue(_dataListViewModel.ScalarCollection[1].DisplayName == "Country");
            Assert.IsFalse(xmlDataList.Contains("Country"));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_AddRecordSetNamesIfMissing")]
        public void AddRecordSetNamesIfMissing_DataListContainingRecordSet_WithSingleBracketedScalar_Expected_Positive()
        {

            //------------Setup for test--------------------------

            var _mockResourceModel = Dev2MockFactory.SetupResourceModelMock();

            var _dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            _dataListViewModel.InitializeDataListViewModel(_mockResourceModel.Object);
            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();

            var carRecordset = DataListItemModelFactory.CreateRecordSetItemModel("[Car]", "A recordset of information about a car");
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Make", "Make of vehicle", carRecordset));
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Model", "Model of vehicle", carRecordset));

            _dataListViewModel.Add(carRecordset);
            _dataListViewModel.Add(DataListItemModelFactory.CreateScalarItemModel("[Country]", "name of Country"));

            DataListSingleton.SetDataList(_dataListViewModel);
            //------------Execute Test---------------------------
            _dataListViewModel.WriteToResourceModel();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, _dataListViewModel.ScalarCollectionCount);
            Assert.AreEqual("Country", _dataListViewModel.ScalarCollection[0].DisplayName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_AddRecordSetNamesIfMissing")]
        public void AddRecordSetNamesIfMissing_DataListContainingRecordSet_WithRoundBracketedScalar_Expected_Positive()
        {

            //------------Setup for test--------------------------

            var _mockResourceModel = Dev2MockFactory.SetupResourceModelMock();

            var _dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            _dataListViewModel.InitializeDataListViewModel(_mockResourceModel.Object);
            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();

            var carRecordset = DataListItemModelFactory.CreateRecordSetItemModel("[Car]", "A recordset of information about a car");
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Make", "Make of vehicle", carRecordset));
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Model", "Model of vehicle", carRecordset));

            _dataListViewModel.Add(carRecordset);
            _dataListViewModel.Add(DataListItemModelFactory.CreateScalarItemModel("Country()", "name of Country"));

            DataListSingleton.SetDataList(_dataListViewModel);
            //------------Execute Test---------------------------
            _dataListViewModel.WriteToResourceModel();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, _dataListViewModel.ScalarCollectionCount);
            Assert.AreEqual("Country", _dataListViewModel.ScalarCollection[0].DisplayName);
        }

        #endregion AddRecordsetNamesIfMissing Tests

        #region AddMode Tests

        [TestMethod]
        public void AddMissingDataListItemsAndThenAddManualy_AddRecordSetWhenDataListContainsRecordsertWithSameName()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("ab");
            part.Setup(c => c.DisplayValue).Returns("[[ab()]]");
            part.Setup(c => c.Description).Returns("");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.Field).Returns("");
            parts.Add(part.Object);

            var part2 = new Mock<IDataListVerifyPart>();
            part2.Setup(c => c.Recordset).Returns("ab");
            part2.Setup(c => c.DisplayValue).Returns("[[ab().c]]");
            part2.Setup(c => c.Description).Returns("");
            part2.Setup(c => c.IsScalar).Returns(false);
            part2.Setup(c => c.Field).Returns("c");
            parts.Add(part2.Object);

            _dataListViewModel.AddMissingDataListItems(parts);

            IRecordSetFieldItemModel item = new RecordSetFieldItemModel("ab().c");
            item.DisplayName = "c";
            item.Parent = _dataListViewModel.RecsetCollection[0];

            _dataListViewModel.RecsetCollection[0].Children.Insert(1, item);

            _dataListViewModel.RemoveBlankRows(item);
            _dataListViewModel.AddRecordsetNamesIfMissing();
            _dataListViewModel.ValidateVariableNamesForUI(item);

            Assert.IsTrue(_dataListViewModel.RecsetCollection[0].Children[0].HasError);
            Assert.AreEqual(StringResources.ErrorMessageDuplicateValue, _dataListViewModel.RecsetCollection[0].Children[0].ErrorMessage);
            Assert.IsTrue(_dataListViewModel.RecsetCollection[0].Children[1].HasError);
            Assert.AreEqual(StringResources.ErrorMessageDuplicateValue, _dataListViewModel.RecsetCollection[0].Children[1].ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_HasErrors")]
        public void DataListViewModel_HasErrors_FieldNamesDuplicated_HasErrorsTrue()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("ab");
            part.Setup(c => c.DisplayValue).Returns("[[ab()]]");
            part.Setup(c => c.Description).Returns("");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.Field).Returns("");
            parts.Add(part.Object);

            var part2 = new Mock<IDataListVerifyPart>();
            part2.Setup(c => c.Recordset).Returns("ab");
            part2.Setup(c => c.DisplayValue).Returns("[[ab().c]]");
            part2.Setup(c => c.Description).Returns("");
            part2.Setup(c => c.IsScalar).Returns(false);
            part2.Setup(c => c.Field).Returns("c");
            parts.Add(part2.Object);

            _dataListViewModel.AddMissingDataListItems(parts);

            IRecordSetFieldItemModel item = new RecordSetFieldItemModel("ab().c");
            item.DisplayName = "c";
            item.Parent = _dataListViewModel.RecsetCollection[0];

            _dataListViewModel.RecsetCollection[0].Children.Insert(1, item);

            _dataListViewModel.RemoveBlankRows(item);
            _dataListViewModel.AddRecordsetNamesIfMissing();
            //------------Execute Test---------------------------
            _dataListViewModel.ValidateVariableNamesForUI(item);
            //------------Assert Results-------------------------
            Assert.IsTrue(_dataListViewModel.HasErrors);
            StringAssert.Contains(_dataListViewModel.DataListErrorMessage, _dataListViewModel.RecsetCollection[0].Children[0].ErrorMessage);
            StringAssert.Contains(_dataListViewModel.DataListErrorMessage, _dataListViewModel.RecsetCollection[0].Children[1].ErrorMessage);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DataListViewModel_HasErrors")]
        public void DataListViewModel_HasErrors_ArrayObjAndObjNameIsDuplicated_HasErrorsTrue()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();
            _dataListViewModel.ComplexObjectCollection.Clear();

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.DisplayValue).Returns("[[@ab()]]");
            part.Setup(c => c.Description).Returns("");
            part.Setup(c => c.IsJson).Returns(true);
            parts.Add(part.Object);

            var part2 = new Mock<IDataListVerifyPart>();
            part2.Setup(c => c.DisplayValue).Returns("[[@ab]]");
            part2.Setup(c => c.Description).Returns("");
            part2.Setup(c => c.IsJson).Returns(true);
            parts.Add(part2.Object);

            _dataListViewModel.AddMissingDataListItems(parts);

            IComplexObjectItemModel item = new ComplexObjectItemModel("[[@ab()]]");

            _dataListViewModel.ComplexObjectCollection.Insert(1, item);

            //------------Execute Test---------------------------
            _dataListViewModel.ValidateVariableNamesForUI(item);
            //------------Assert Results-------------------------
            Assert.IsTrue(_dataListViewModel.HasErrors);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DataListViewModel_HasErrors")]
        public void DataListViewModel_HasErrors_ArrayObjAndObjNameIsDuplicated_HasErrorsTrue_OnCollectionChanged()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();
            _dataListViewModel.ComplexObjectCollection.Clear();

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.DisplayValue).Returns("[[@ab()]]");
            part.Setup(c => c.Description).Returns("");
            part.Setup(c => c.IsJson).Returns(true);
            parts.Add(part.Object);

            var part2 = new Mock<IDataListVerifyPart>();
            part2.Setup(c => c.DisplayValue).Returns("[[@ab]]");
            part2.Setup(c => c.Description).Returns("");
            part2.Setup(c => c.IsJson).Returns(true);
            parts.Add(part2.Object);

            _dataListViewModel.AddMissingDataListItems(parts);

            IComplexObjectItemModel item = new ComplexObjectItemModel("[[@ab()]]");

            _dataListViewModel.ComplexObjectCollection.Insert(1, item);

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsTrue(_dataListViewModel.HasErrors);
        }


        [TestMethod]
        public void AddMissingDataListItemsAndThenAddManualy_IsNotField_AddRecordSetWhenDataListContainsRecordsertWithSameName()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("ab");
            part.Setup(c => c.DisplayValue).Returns("[[ab()]]");
            part.Setup(c => c.Description).Returns("");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.Field).Returns("");
            parts.Add(part.Object);

            var part2 = new Mock<IDataListVerifyPart>();
            part2.Setup(c => c.Recordset).Returns("ab");
            part2.Setup(c => c.DisplayValue).Returns("[[ab().c]]");
            part2.Setup(c => c.Description).Returns("");
            part2.Setup(c => c.IsScalar).Returns(false);
            part2.Setup(c => c.Field).Returns("c");
            parts.Add(part2.Object);

            _dataListViewModel.AddMissingDataListItems(parts);

            IRecordSetItemModel item = new RecordSetItemModel("de()");
            item.DisplayName = "de";
            item.Children.Add(new RecordSetFieldItemModel("gh", item));
            _dataListViewModel.RecsetCollection.Insert(1, item);

            _dataListViewModel.RemoveBlankRows(item);
            _dataListViewModel.AddRecordsetNamesIfMissing();
            _dataListViewModel.ValidateVariableNamesForUI(item);

            Assert.IsFalse(_dataListViewModel.RecsetCollection[1].HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void ValidateNames_ItemToAddTrueWithRecordSetWhenDataListContainsRecordsertWithSameName_ShouldReturnError()
        {
            //------------Setup for test--------------------------
            var (item, _dataListViewModel) = SetupForValidateNamesDuplicateRecordSetFieldsTests();

            _dataListViewModel.RecsetCollection[0].Children.Insert(1, item);

            _dataListViewModel.RemoveBlankRows(item);
            _dataListViewModel.AddRecordsetNamesIfMissing();
            //------------Execute Test---------------------------
            _dataListViewModel.ValidateVariableNamesForUI(item);
            //------------Assert Results-------------------------
            Assert.IsTrue(_dataListViewModel.RecsetCollection[0].Children[0].HasError);
            Assert.AreEqual(StringResources.ErrorMessageDuplicateValue, _dataListViewModel.RecsetCollection[0].Children[0].ErrorMessage);
            Assert.IsTrue(_dataListViewModel.RecsetCollection[0].Children[1].HasError);
            Assert.AreEqual(StringResources.ErrorMessageDuplicateValue, _dataListViewModel.RecsetCollection[0].Children[1].ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void ValidateNames_WhenAddItemFalseAndItemExist_ShouldCauseErrorToShow()
        {
            //------------Setup for test--------------------------
            var (item, _dataListViewModel) = SetupForValidateNamesDuplicateRecordSetFieldsTests();
            _dataListViewModel.RecsetCollection[0].Children.Insert(1, item);

            _dataListViewModel.RemoveBlankRows(item);
            //------------Execute Test---------------------------
            _dataListViewModel.ValidateVariableNamesForUI(item);
            //------------Assert Results-------------------------
            Assert.IsTrue(_dataListViewModel.RecsetCollection[0].Children[0].HasError);
            Assert.AreEqual(StringResources.ErrorMessageDuplicateValue, _dataListViewModel.RecsetCollection[0].Children[0].ErrorMessage);
            Assert.IsTrue(_dataListViewModel.RecsetCollection[0].Children[1].HasError);
            Assert.AreEqual(StringResources.ErrorMessageDuplicateValue, _dataListViewModel.RecsetCollection[0].Children[1].ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void ValidateNames_WhenAddItemFalseAndItemNotExist_ShouldNotCauseErrorToShow()
        {
            //------------Setup for test--------------------------
            var (item, _dataListViewModel) = SetupForValidateNamesDuplicateRecordSetFieldsTests();

            _dataListViewModel.RemoveBlankRows(item);
            //------------Execute Test---------------------------
            _dataListViewModel.ValidateVariableNamesForUI(item);
            //------------Assert Results-------------------------
            Assert.IsFalse(_dataListViewModel.RecsetCollection[0].Children[0].HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void ValidateNames_WhenAddItemTrueAndItemNotExist_ShouldNotCauseErrorToShow()
        {
            //------------Setup for test--------------------------
            var (item, _dataListViewModel) = SetupForValidateNamesDuplicateRecordSetFieldsTests();
            _dataListViewModel.RemoveBlankRows(item);
            //------------Execute Test---------------------------
            _dataListViewModel.ValidateVariableNamesForUI(item);
            //------------Assert Results-------------------------
            Assert.IsFalse(_dataListViewModel.RecsetCollection[0].Children[0].HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void ValidateNames_ItemToAddTrueWithScalarWhenDataListContainsScalarWithSameName_ShouldReturnError()
        {
            //------------Setup for test--------------------------
            var (item, _dataListViewModel) = SetupForValidateNamesDuplicateScalarTests();

            _dataListViewModel.ScalarCollection.Insert(1, item);

            _dataListViewModel.RemoveBlankRows(item);
            //------------Execute Test---------------------------
            _dataListViewModel.ValidateVariableNamesForUI(item);
            //------------Assert Results-------------------------
            Assert.IsTrue(_dataListViewModel.ScalarCollection[0].HasError);
            Assert.AreEqual(StringResources.ErrorMessageDuplicateValue, _dataListViewModel.ScalarCollection[0].ErrorMessage);
            Assert.IsTrue(_dataListViewModel.ScalarCollection[1].HasError);
            Assert.AreEqual(StringResources.ErrorMessageDuplicateValue, _dataListViewModel.ScalarCollection[1].ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void ValidateNames_WhenAddItemFalseAndScalarItemExist_ShouldCauseErrorToShow()
        {
            //------------Setup for test--------------------------
            var (item, _dataListViewModel) = SetupForValidateNamesDuplicateScalarTests();
            _dataListViewModel.ScalarCollection.Insert(1, item);

            _dataListViewModel.RemoveBlankRows(item);
            //------------Execute Test---------------------------
            _dataListViewModel.ValidateVariableNamesForUI(item);
            //------------Assert Results-------------------------
            Assert.IsTrue(_dataListViewModel.ScalarCollection[0].HasError);
            Assert.AreEqual(StringResources.ErrorMessageDuplicateValue, _dataListViewModel.ScalarCollection[0].ErrorMessage);
            Assert.IsTrue(_dataListViewModel.ScalarCollection[1].HasError);
            Assert.AreEqual(StringResources.ErrorMessageDuplicateValue, _dataListViewModel.ScalarCollection[1].ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void ValidateNames_WhenAddItemFalseAndScalarItemNotExist_ShouldNotCauseErrorToShow()
        {
            //------------Setup for test--------------------------
            var (item, _dataListViewModel) = SetupForValidateNamesDuplicateScalarTests();

            _dataListViewModel.RemoveBlankRows(item);
            //------------Execute Test---------------------------
            _dataListViewModel.ValidateVariableNamesForUI(item);
            //------------Assert Results-------------------------
            Assert.IsFalse(_dataListViewModel.ScalarCollection[0].HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void ValidateNames_WhenAddItemTrueAndScalarItemNotExist_ShouldNotCauseErrorToShow()
        {
            //------------Setup for test--------------------------
            var (item, _dataListViewModel) = SetupForValidateNamesDuplicateScalarTests();
            _dataListViewModel.RemoveBlankRows(item);
            //------------Execute Test---------------------------
            _dataListViewModel.ValidateVariableNamesForUI(item);
            //------------Assert Results-------------------------
            Assert.IsFalse(_dataListViewModel.ScalarCollection[0].HasError);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("DataListViewModel_TrackEvent_InputVariableCheckboxCommand")]
        public void TrackEvent_InputVariableCheckboxCommand()
        {
            var _applicationTrackerMock = new Mock<IApplicationTracker>();
            _applicationTrackerMock.Setup(controller => controller.TrackEvent(It.IsAny<string>(), It.IsAny<string>()));
            CustomContainer.Register(_applicationTrackerMock.Object);
            //------------Setup for test--------------------------
            var (item, _dataListViewModel) = SetupForValidateNamesDuplicateScalarTests();
            _dataListViewModel.InputVariableCheckboxCommand.Execute(null);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            _applicationTrackerMock.Verify(controller => controller.TrackEvent(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("DataListViewModel_TrackEvent_OutputVariableCheckboxCommand")]
        public void TrackEvent_OutputVariableCheckboxCommand()
        {
            var _applicationTrackerMock = new Mock<IApplicationTracker>();
            _applicationTrackerMock.Setup(controller => controller.TrackEvent(It.IsAny<string>(), It.IsAny<string>()));
            CustomContainer.Register(_applicationTrackerMock.Object);
            //------------Setup for test--------------------------
            var (item, _dataListViewModel) = SetupForValidateNamesDuplicateScalarTests();
            _dataListViewModel.OutputVariableCheckboxCommand.Execute(null);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            _applicationTrackerMock.Verify(controller => controller.TrackEvent(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("DataListViewModel_UpdateDataListItems_TrackCustomEvent")]
        public void DataListViewModel_UpdateDataListItems_MissingScalarWorkflowItems_TrackCustomEvent()
        {
            var _applicationTrackerMock = new Mock<IApplicationTracker>();
            _applicationTrackerMock.Setup(controller => controller.TrackCustomEvent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            CustomContainer.Register(_applicationTrackerMock.Object);
            //------------Setup for test--------------------------
            var resourceModel = new Mock<IResourceModel>().Object;
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            const string complexObject = "testing";
            const string complexObjectChild1 = "item(1)";
            const string complexObjectChild2 = "new";
            var complexObjectDataModel = CreateComplexObjectDataListModel(complexObject);
            complexObjectDataModel.Children.Add(CreateComplexObjectDataListModel(complexObjectChild1, complexObjectDataModel));
            complexObjectDataModel.Children.Add(CreateComplexObjectDataListModel(complexObjectChild2, complexObjectDataModel));
            dataListViewModel.Add(complexObjectDataModel);
            var parts = new List<IDataListVerifyPart>
            {
                CreateComplexObjectPart(complexObject).Object,
                CreateComplexObjectPart(complexObjectChild1).Object,
                CreateComplexObjectPart(complexObjectChild2).Object
            };
            //------------Execute Test---------------------------
            dataListViewModel.UpdateDataListItems(resourceModel, parts);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListViewModel.ComplexObjectCollection[0].IsUsed);
            Assert.IsTrue(dataListViewModel.ComplexObjectCollection[0].Children[0].IsUsed);
            Assert.IsTrue(dataListViewModel.ComplexObjectCollection[0].Children[1].IsUsed);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            _applicationTrackerMock.Verify(controller => controller.TrackCustomEvent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce());
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("DataListViewModel_UpdateDataListItems_TrackCustomEvent")]
        public void DataListViewModel_UpdateDataListItems_NoMissingScalarWorkflowItems_TrackCustomEvent()
        {
            var _applicationTrackerMock = new Mock<IApplicationTracker>();
            _applicationTrackerMock.Setup(controller => controller.TrackCustomEvent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            CustomContainer.Register(_applicationTrackerMock.Object);
            //------------Setup for test--------------------------
            var resourceModel = new Mock<IResourceModel>().Object;
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            const string scalarName = "scalar";
            var scalarItem = new ScalarItemModel(scalarName) { IsUsed = false };
            dataListViewModel.Add(scalarItem);
            var parts = new List<IDataListVerifyPart> { CreateScalarPart(scalarName).Object };
            //------------Execute Test---------------------------
            dataListViewModel.UpdateDataListItems(resourceModel, parts);
            //------------Assert Results-------------------------
            _applicationTrackerMock.Verify(controller => controller.TrackCustomEvent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce());
        }
        #endregion AddMode Tests


        #region WriteDataToResourceModel Tests

        [TestMethod]
        public void WriteDataListToResourceModel_ScalarAnsrecset_Expected_Positive()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var result = _dataListViewModel.WriteToResourceModel();

            const string expectedResult = @"<DataList><Country Description=""name of Country"" IsEditable=""True"" ColumnIODirection=""Both"" /><Car Description=""A recordset of information about a car"" IsEditable=""True"" ColumnIODirection=""Both"" ><Make Description=""Make of vehicle"" IsEditable=""True"" ColumnIODirection=""Both"" /><Model Description=""Model of vehicle"" IsEditable=""True"" ColumnIODirection=""Both"" /></Car></DataList>";

            Assert.AreEqual(expectedResult, result);
        }

        #endregion WriteDataToResourceModel Tests

        #region Internal Test Methods
        void SortInitialization(DataListViewModel _dataListViewModel)
        {
            _dataListViewModel.Add(DataListItemModelFactory.CreateScalarItemModel("zzz"));
            _dataListViewModel.Add(DataListItemModelFactory.CreateScalarItemModel("ttt"));
            _dataListViewModel.Add(DataListItemModelFactory.CreateScalarItemModel("aaa"));
            var recordSetItemModel = DataListItemModelFactory.CreateRecordSetItemModel("zzz");
            recordSetItemModel.Children.Add(new RecordSetFieldItemModel("aaa", recordSetItemModel));
            recordSetItemModel.Children.Add(new RecordSetFieldItemModel("xxx", recordSetItemModel));
            recordSetItemModel.Children.Add(new RecordSetFieldItemModel("bbb", recordSetItemModel));
            _dataListViewModel.Add(recordSetItemModel);
            _dataListViewModel.Add(DataListItemModelFactory.CreateRecordSetItemModel("ttt"));
            _dataListViewModel.Add(DataListItemModelFactory.CreateRecordSetItemModel("aaa"));

            const string complexObject = "testing";
            const string complexObjectChild = "item";
            var complexObjectDataModel = CreateComplexObjectDataListModel(complexObject);
            complexObjectDataModel.Input = true;
            complexObjectDataModel.Output = true;
            complexObjectDataModel.Children.Add(CreateComplexObjectDataListModel(complexObjectChild, complexObjectDataModel));
            _dataListViewModel.Add(complexObjectDataModel);
        }

        public static void SortCleanup(DataListViewModel _dataListViewModel)
        {
            _dataListViewModel.ScalarCollection.Clear();
            _dataListViewModel.RecsetCollection.Clear();

            var carRecordset = DataListItemModelFactory.CreateRecordSetItemModel("Car", "A recordset of information about a car");
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Make", "Make of vehicle", carRecordset));
            carRecordset.Children.Add(DataListItemModelFactory.CreateRecordSetFieldItemModel("Model", "Model of vehicle", carRecordset));

            _dataListViewModel.Add(carRecordset);
            _dataListViewModel.Add(DataListItemModelFactory.CreateScalarItemModel("Country", "name of Country"));
        }

        #endregion Internal Test Methods

        #region Sort

        [TestMethod]
        public void SortOnceExpectedSortsAscendingOrder()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            SortInitialization(_dataListViewModel);

            Assert.IsNotNull(_dataListViewModel.ScalarCollection);
            Assert.IsNotNull(_dataListViewModel.RecsetCollection);
            Assert.IsNotNull(_dataListViewModel.ComplexObjectCollection);

            Assert.IsTrue(_dataListViewModel.ScalarCollectionCount > 0);
            Assert.IsTrue(_dataListViewModel.RecsetCollectionCount > 0);
            Assert.IsTrue(_dataListViewModel.ComplexObjectCollectionCount >= 1);

            Assert.IsTrue(_dataListViewModel.CanSortItems);
            //Execute
            _dataListViewModel.SortCommand.Execute(null);
            Assert.IsFalse(_dataListViewModel.IsSorting);

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

            SortCleanup(_dataListViewModel);
        }

        [TestMethod]
        public void SortOnceExpectedSorts_ShouldSortRecsetfields()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            SortInitialization(_dataListViewModel);

            Assert.IsNotNull(_dataListViewModel.ScalarCollection);
            Assert.IsNotNull(_dataListViewModel.RecsetCollection);
            Assert.IsNotNull(_dataListViewModel.ComplexObjectCollection);

            Assert.IsTrue(_dataListViewModel.ScalarCollectionCount > 0);
            Assert.IsTrue(_dataListViewModel.RecsetCollectionCount > 0);
            Assert.IsTrue(_dataListViewModel.ComplexObjectCollectionCount >= 1);

            Assert.IsTrue(_dataListViewModel.CanSortItems);
            //Execute
            _dataListViewModel.SortCommand.Execute(null);
            Assert.IsFalse(_dataListViewModel.IsSorting);

            //Recset List Asserts
            Assert.AreEqual("aaa", _dataListViewModel.RecsetCollection[0].DisplayName, "Sort datalist left recset list unsorted");
            Assert.AreEqual("Car", _dataListViewModel.RecsetCollection[1].DisplayName, "Sort datalist left recset list unsorted");
            Assert.AreEqual("ttt", _dataListViewModel.RecsetCollection[2].DisplayName, "Sort datalist left recset list unsorted");
            Assert.AreEqual("zzz", _dataListViewModel.RecsetCollection[3].DisplayName, "Sort datalist left recset list unsorted");
            Assert.AreEqual("aaa", _dataListViewModel.RecsetCollection[3].Children[0].DisplayName, "Sort datalist left recset list unsorted");
            Assert.AreEqual("bbb", _dataListViewModel.RecsetCollection[3].Children[1].DisplayName, "Sort datalist left recset list unsorted");
            Assert.AreEqual("xxx", _dataListViewModel.RecsetCollection[3].Children[2].DisplayName, "Sort datalist left recset list unsorted");
            SortCleanup(_dataListViewModel);
        }

        [TestMethod]
        public void SortTwiceExpectedSortsDescendingOrder()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            SortInitialization(_dataListViewModel);

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

            SortCleanup(_dataListViewModel);
        }

        [TestMethod]
        public void SortTwiceExpectedSortsDescendingOrder_IncludingRecordsetFields()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            SortInitialization(_dataListViewModel);

            //Execute
            _dataListViewModel.SortCommand.Execute(null);
            //Execute Twice
            _dataListViewModel.SortCommand.Execute(null);

            //Recset List Asserts
            Assert.AreEqual("zzz", _dataListViewModel.RecsetCollection[0].DisplayName, "Sort datalist left recset list unsorted");
            Assert.AreEqual("xxx", _dataListViewModel.RecsetCollection[0].Children[0].DisplayName, "Sort datalist left recset list unsorted");
            Assert.AreEqual("bbb", _dataListViewModel.RecsetCollection[0].Children[1].DisplayName, "Sort datalist left recset list unsorted");
            Assert.AreEqual("aaa", _dataListViewModel.RecsetCollection[0].Children[2].DisplayName, "Sort datalist left recset list unsorted");
            Assert.AreEqual("ttt", _dataListViewModel.RecsetCollection[1].DisplayName, "Sort datalist left recset list unsorted");
            Assert.AreEqual("Car", _dataListViewModel.RecsetCollection[2].DisplayName, "Sort datalist left recset list unsorted");
            Assert.AreEqual("aaa", _dataListViewModel.RecsetCollection[3].DisplayName, "Sort datalist left recset list unsorted");

            SortCleanup(_dataListViewModel);
        }

        #endregion

        #region Validator tests

        #region RecordSet Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_AddingAFieldToAEmptyRecordset_ErrorRemoved()
        {
            //------------Setup for test--------------------------            
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var child = DataListItemModelFactory.CreateRecordSetFieldItemModel("Child");
            var originalchild = DataListItemModelFactory.CreateRecordSetFieldItemModel("");
            var parent = DataListItemModelFactory.CreateRecordSetItemModel("RecordSet");
            parent.Children.Add(originalchild);


            //------------Execute Test---------------------------

            child.Parent = parent;
            _dataListViewModel.Add(parent);
            _dataListViewModel.ValidateVariableNamesForUI(parent);
            Assert.IsTrue(parent.HasError);
            Assert.AreEqual(StringResources.ErrorMessageEmptyRecordSet, parent.ErrorMessage);

            _dataListViewModel.RecsetCollection.Last().Children[0] = child;
            _dataListViewModel.ValidateVariableNamesForUI(child);

            //------------Assert Results-------------------------
            Assert.IsFalse(parent.HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_RecordsetWithValidNameWithBrackets_ShouldNotHaveError()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var dataListItemModel = DataListItemModelFactory.CreateRecordSetItemModel("TestScalar()", children: new OptomizedObservableCollection<IRecordSetFieldItemModel> { DataListItemModelFactory.CreateRecordSetFieldItemModel("Child") });
            //------------Execute Test---------------------------
            _dataListViewModel.Add(dataListItemModel);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataListItemModel.HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_RecordsetWithValidNameWithInvalidCharacter_ShouldNotHaveError()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var dataListItemModel = DataListItemModelFactory.CreateRecordSetItemModel("TestScalar().", children: new OptomizedObservableCollection<IRecordSetFieldItemModel> { DataListItemModelFactory.CreateRecordSetFieldItemModel("Child") });
            //------------Execute Test---------------------------
            _dataListViewModel.Add(dataListItemModel);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListItemModel.HasError);
            Assert.AreEqual("Recordset name [[TestScalar.]] contains invalid character(s). Only use alphanumeric _ and - ", dataListItemModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_RecordsetFieldWithValidNameWithInvalidCharacter_ShouldNotHaveError()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var dataListItemModel = DataListItemModelFactory.CreateRecordSetItemModel("TestScalar()", children: new OptomizedObservableCollection<IRecordSetFieldItemModel> { DataListItemModelFactory.CreateRecordSetFieldItemModel("Child@") });
            //------------Execute Test---------------------------
            _dataListViewModel.Add(dataListItemModel);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel.Children[0]);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListItemModel.Children[0].HasError);
            Assert.AreEqual("Recordset field name Child@ contains invalid character(s). Only use alphanumeric _ and - ", dataListItemModel.Children[0].ErrorMessage);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_RecordSetWithNoItems_HasErrorTrue()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();


            var child = DataListItemModelFactory.CreateRecordSetFieldItemModel("");
            var parent = DataListItemModelFactory.CreateRecordSetItemModel("RecordSet");

            parent.Children.Add(child);
            _dataListViewModel.Add(parent);
            _dataListViewModel.ValidateVariableNamesForUI(parent);
            Assert.IsTrue(parent.HasError);
            Assert.AreEqual(StringResources.ErrorMessageEmptyRecordSet, parent.ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_HasErrors")]
        public void DataListViewModel_HasErrors_RecordSetWithNoItems_HasErrorTrue()
        {
            //------------Setup------------------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();


            var child = DataListItemModelFactory.CreateRecordSetFieldItemModel("");
            var parent = DataListItemModelFactory.CreateRecordSetItemModel("RecordSet");

            parent.Children.Add(child);
            _dataListViewModel.Add(parent);
            //----------------------Execute--------------------------------
            _dataListViewModel.ValidateVariableNamesForUI(parent);
            //----------------------Assert---------------------------------
            Assert.IsTrue(_dataListViewModel.HasErrors);
            StringAssert.Contains(_dataListViewModel.DataListErrorMessage, "[[RecordSet]] : Recordset must contain one or more field(s).");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_HasNoErrors")]
        public void DataListViewModel_HasNoErrors_RecordSetWithNoItems_HasErrorFalse()
        {
            //------------Setup------------------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();


            //----------------------Execute--------------------------------
            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();
            //----------------------Assert---------------------------------
            Assert.IsFalse(_dataListViewModel.HasErrors);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_RecordSetWithItems_HasErrorFalse()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();


            var child = DataListItemModelFactory.CreateRecordSetFieldItemModel("Child");
            var parent = DataListItemModelFactory.CreateRecordSetItemModel("RecordSet");
            parent.Children.Add(child);
            _dataListViewModel.Add(parent);
            _dataListViewModel.ValidateVariableNamesForUI(parent);
            Assert.IsFalse(parent.HasError);
        }
        #endregion

        #region Scalar Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_ItemHasInvalidChar_ErrorNotRemovedFromDuplicateCheck()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel();
            IDataListItemModel dataListItemModel = new DataListItemModel("test@");
            dataListItemModel.HasError = true;
            dataListItemModel.ErrorMessage = StringResources.ErrorMessageInvalidChar;
            //------------Execute Test---------------------------
            dataListViewModel.ValidateVariableNamesForUI(dataListItemModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(StringResources.ErrorMessageInvalidChar, dataListItemModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataListViewModel_HasErrors")]
        public void DataListViewModel_HasErrors_ItemHasInvalidChar_ErrorNotRemovedFromDuplicateCheck()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel();
            IScalarItemModel dataListItemModel = new ScalarItemModel("test@");
            dataListItemModel.HasError = true;
            dataListItemModel.ErrorMessage = StringResources.ErrorMessageInvalidChar;
            dataListViewModel.Add(dataListItemModel);
            //------------Execute Test---------------------------
            dataListViewModel.ValidateVariableNamesForUI(dataListItemModel);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListViewModel.HasErrors);
            StringAssert.Contains(dataListViewModel.DataListErrorMessage, dataListItemModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_WithInvalidScalarName_ShouldHaveError()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var dataListItemModel = DataListItemModelFactory.CreateScalarItemModel("TestScalar!");
            //------------Execute Test---------------------------
            _dataListViewModel.Add(dataListItemModel);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListItemModel.HasError);
            Assert.AreEqual("Scalar name [[TestScalar!]] contains invalid character(s). Only use alphanumeric _ and - ", dataListItemModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_WithInvalidScalarNameWithDot_ShouldHaveError()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var dataListItemModel = DataListItemModelFactory.CreateScalarItemModel("TestScalar.");
            //------------Execute Test---------------------------
            _dataListViewModel.Add(dataListItemModel);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListItemModel.HasError);
            Assert.AreEqual("Scalar name [[TestScalar.]] contains invalid character(s). Only use alphanumeric _ and - ", dataListItemModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_WithInvalidScalarNameContainsDotAndExtraText_ShouldHaveError()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var dataListItemModel = DataListItemModelFactory.CreateScalarItemModel("TestScalar.ad");
            //------------Execute Test---------------------------
            _dataListViewModel.Add(dataListItemModel);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListItemModel.HasError);
            Assert.AreEqual("Scalar name [[TestScalar.ad]] contains invalid character(s). Only use alphanumeric _ and - ", dataListItemModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_WithInvalidScalarNameWithBrackets_ShouldHaveError()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var dataListItemModel = DataListItemModelFactory.CreateScalarItemModel("TestScalar()");
            //------------Execute Test---------------------------
            _dataListViewModel.Add(dataListItemModel);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListItemModel.HasError);
            Assert.AreEqual("Scalar name [[TestScalar()]] contains invalid character(s). Only use alphanumeric _ and - ", dataListItemModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_WithInvalidScalarNameWithUnderScore_ShouldNotHaveError()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var dataListItemModel = DataListItemModelFactory.CreateScalarItemModel("TestScalar_1");
            //------------Execute Test---------------------------
            _dataListViewModel.Add(dataListItemModel);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataListItemModel.HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_WithInvalidScalarNameWithNumber_ShouldNotHaveError()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var dataListItemModel = DataListItemModelFactory.CreateScalarItemModel("TestScalar1");
            //------------Execute Test---------------------------
            _dataListViewModel.Add(dataListItemModel);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataListItemModel.HasError);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_ScalarsWithDuplicateName_ItemHasErrorTrue()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var dataListItemModel1 = DataListItemModelFactory.CreateScalarItemModel("TestScalar1");
            var dataListItemModel2 = DataListItemModelFactory.CreateScalarItemModel("TestScalar1");
            //------------Execute Test---------------------------
            _dataListViewModel.Add(dataListItemModel1);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel1);
            _dataListViewModel.Add(dataListItemModel2);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel2);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListItemModel1.HasError);
            Assert.IsTrue(dataListItemModel2.HasError);
            Assert.AreEqual(StringResources.ErrorMessageDuplicateValue, dataListItemModel1.ErrorMessage);
            Assert.AreEqual(StringResources.ErrorMessageDuplicateValue, dataListItemModel2.ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_HasErrors")]
        public void DataListViewModel_HasErrors_ScalarsWithDuplicateName_ItemHasErrorTrue()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var dataListItemModel1 = DataListItemModelFactory.CreateScalarItemModel("TestScalar1");
            var dataListItemModel2 = DataListItemModelFactory.CreateScalarItemModel("TestScalar1");
            //------------Execute Test---------------------------
            _dataListViewModel.Add(dataListItemModel1);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel1);
            _dataListViewModel.Add(dataListItemModel2);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel2);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListItemModel1.HasError);
            Assert.IsTrue(dataListItemModel2.HasError);
            Assert.IsTrue(_dataListViewModel.HasErrors);
            StringAssert.Contains(_dataListViewModel.DataListErrorMessage, dataListItemModel1.ErrorMessage);
            StringAssert.Contains(_dataListViewModel.DataListErrorMessage, dataListItemModel2.ErrorMessage);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_RemoveScalarsWithDuplicateName_ItemHasErrorFalse()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var dataListItemModel1 = DataListItemModelFactory.CreateScalarItemModel("TestScalar1");
            var dataListItemModel2 = DataListItemModelFactory.CreateScalarItemModel("TestScalar1");
            //------------Execute Test---------------------------
            _dataListViewModel.Add(dataListItemModel1);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel1);
            _dataListViewModel.Add(dataListItemModel2);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel2);

            Assert.IsTrue(dataListItemModel1.HasError);
            Assert.IsTrue(dataListItemModel2.HasError);
            Assert.AreEqual(StringResources.ErrorMessageDuplicateValue, dataListItemModel1.ErrorMessage);
            Assert.AreEqual(StringResources.ErrorMessageDuplicateValue, dataListItemModel2.ErrorMessage);

            var dataListItemModel = _dataListViewModel.ScalarCollection.FirstOrDefault(c => c.DisplayName == "TestScalar1");
            Assert.IsNotNull(dataListItemModel);
            dataListItemModel.DisplayName = "TestScalar2";



            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel1);

            //------------Assert Results-------------------------

            Assert.IsFalse(dataListItemModel1.HasError);

        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_ScalarsWithUniqueName_ItemHasErrorFalse()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var dataListItemModel1 = DataListItemModelFactory.CreateScalarItemModel("TestScalar1");
            var dataListItemModel2 = DataListItemModelFactory.CreateScalarItemModel("TestScalar2");
            //------------Execute Test---------------------------
            _dataListViewModel.Add(dataListItemModel1);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel1);
            _dataListViewModel.Add(dataListItemModel2);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel2);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataListItemModel1.HasError);
            Assert.IsFalse(dataListItemModel2.HasError);
        }

        #endregion

        #region Mixed Scalar and Recordset Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_AddRecordsetWithDuplicateScalar_RecordsetHasDuplicateVariableErrorMessage()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var dataListItemModel1 = DataListItemModelFactory.CreateScalarItemModel("TestScalar");
            var newItem = DataListItemModelFactory.CreateRecordSetItemModel("TestScalar");
            var newItemsChild = DataListItemModelFactory.CreateRecordSetFieldItemModel("Field");
            newItem.Children.Add(newItemsChild);
            //------------Execute Test---------------------------
            _dataListViewModel.Add(dataListItemModel1);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel1);
            _dataListViewModel.Add(newItem);
            _dataListViewModel.ValidateVariableNamesForUI(newItem);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListItemModel1.HasError);
            Assert.IsTrue(newItem.HasError);
            Assert.AreEqual(StringResources.ErrorMessageDuplicateValue, dataListItemModel1.ErrorMessage);
            Assert.AreEqual(StringResources.ErrorMessageDuplicateValue, newItem.ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_HasErrors")]
        public void DataListViewModel_HasErrors_AddRecordsetWithDuplicateScalar_RecordsetHasDuplicateVariableErrorMessage()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var dataListItemModel1 = DataListItemModelFactory.CreateScalarItemModel("TestScalar");
            var newItem = DataListItemModelFactory.CreateRecordSetItemModel("TestScalar");
            var newItemsChild = DataListItemModelFactory.CreateRecordSetFieldItemModel("Field");
            newItem.Children.Add(newItemsChild);
            //------------Execute Test---------------------------
            _dataListViewModel.Add(dataListItemModel1);
            _dataListViewModel.ValidateVariableNamesForUI(dataListItemModel1);
            _dataListViewModel.Add(newItem);
            _dataListViewModel.ValidateVariableNamesForUI(newItem);
            //------------Assert Results-------------------------
            Assert.IsTrue(_dataListViewModel.HasErrors);
            StringAssert.Contains(_dataListViewModel.DataListErrorMessage, dataListItemModel1.ErrorMessage);
            StringAssert.Contains(_dataListViewModel.DataListErrorMessage, newItem.ErrorMessage);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_AddScalarWithDuplicateRecordset_ScalarHasDuplicateRecordsetErrorMessage()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var existingRecordset = DataListItemModelFactory.CreateRecordSetItemModel("TestRecordset");
            var existingRecordsetChild = DataListItemModelFactory.CreateRecordSetFieldItemModel("Field");
            existingRecordset.Children.Add(existingRecordsetChild);

            var newItem = DataListItemModelFactory.CreateScalarItemModel("TestRecordset");
            //------------Execute Test---------------------------
            _dataListViewModel.Add(existingRecordset);
            _dataListViewModel.ValidateVariableNamesForUI(existingRecordset);
            _dataListViewModel.Add(newItem);
            _dataListViewModel.ValidateVariableNamesForUI(newItem);
            //------------Assert Results-------------------------
            Assert.IsTrue(existingRecordset.HasError);
            Assert.IsTrue(newItem.HasError);
            Assert.AreEqual(StringResources.ErrorMessageDuplicateValue, existingRecordset.ErrorMessage);
            Assert.AreEqual(StringResources.ErrorMessageDuplicateValue, newItem.ErrorMessage);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ValidateNames")]
        public void DataListViewModel_ValidateNames_WhenScalarAndRecordSetFieldHasSameName_NoDuplicateItemMessage()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            var existingRecordset = DataListItemModelFactory.CreateRecordSetItemModel("rec");
            var existingRecordsetChild = DataListItemModelFactory.CreateRecordSetFieldItemModel("a");
            existingRecordset.Children.Add(existingRecordsetChild);

            var newItem = DataListItemModelFactory.CreateScalarItemModel("a");
            //------------Execute Test---------------------------            
            _dataListViewModel.Add(existingRecordset);
            _dataListViewModel.ValidateVariableNamesForUI(existingRecordset);
            _dataListViewModel.Add(newItem);
            _dataListViewModel.ValidateVariableNamesForUI(newItem);
            //------------Assert Results-------------------------
            Assert.IsNull(newItem.ErrorMessage, "No Duplicate message should be shown for fields and scalars with the same name.");
            Assert.IsNull(existingRecordsetChild.ErrorMessage, "No Duplicate message should be shown for fields and scalars with the same name.");
            Assert.IsNull(existingRecordset.ErrorMessage, "No Duplicate message should be shown for fields and scalars with the same name.");
        }

        #endregion

        #endregion

        [TestMethod]
        [TestCategory("DataListViewModel_CanSortItems")]
        public void DataListViewModel_UnitTest_CanSortItemsWhereEmptyCollections_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel();
            //------------Execute Test---------------------------
            var canSortItems = dataListViewModel.CanSortItems;
            //------------Assert Results-------------------------
            Assert.IsFalse(canSortItems);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_ClearCollections")]
        public void DataListViewModel_ClearCollections_WhenHasItems_ClearsBaseCollection()
        {
            //------------Setup for test--------------------------
            var (_mockResourceModel, _dataListViewModel) = Setup();

            SortInitialization(_dataListViewModel);
            //------------Precondition---------------------------
            Assert.AreEqual(3, _dataListViewModel.BaseCollection.Count);
            //------------Execute Test---------------------------
            _dataListViewModel.ClearCollections();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, _dataListViewModel.BaseCollection.Count);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_SetUnusedDataListItems")]
        public void DataListViewModel_SetUnusedDataListItems_HasRecsetsWithFieldsThatMatchParts_ShouldSetChildrenIsUsedFalse()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object) { BaseCollection = new OptomizedObservableCollection<DataListHeaderItemModel>() };
            const string recsetName = "recset";
            const string firstFieldName = "f1";
            var recSetDataModel = CreateRecsetDataListModelWithTwoFields(recsetName, firstFieldName, "f2");
            dataListViewModel.Add(recSetDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateRecsetPart(recsetName, firstFieldName);
            dataListParts.Add(part.Object);
            //------------Execute Test---------------------------
            dataListViewModel.SetIsUsedDataListItems(dataListParts, false);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataListViewModel.RecsetCollection[0].Children[0].IsUsed);
            Assert.IsTrue(dataListViewModel.RecsetCollection[0].Children[1].IsUsed);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_SetUsedDataListItems")]
        public void DataListViewModel_SetUsedDataListItems_HasComplexObjectsWithFieldsThatMatchParts_ShouldSetChildrenIsUsedTrue()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object) { BaseCollection = new OptomizedObservableCollection<DataListHeaderItemModel>() };
            const string complexObject = "testing";
            const string complexObjectChild = "item";
            var complexObjectDataModel = CreateComplexObjectDataListModel(complexObject);
            complexObjectDataModel.Children.Add(CreateComplexObjectDataListModel(complexObjectChild, complexObjectDataModel));
            dataListViewModel.Add(complexObjectDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateComplexObjectPart(complexObject);
            dataListParts.Add(part.Object);
            //------------Execute Test---------------------------
            dataListViewModel.SetIsUsedDataListItems(dataListParts, false);
            //------------Assert Results-------------------------
            Assert.IsFalse(dataListViewModel.ComplexObjectCollection[0].IsUsed);
            Assert.IsTrue(dataListViewModel.ComplexObjectCollection[0].Children[0].IsUsed);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_SetInputOutput")]
        public void DataListViewModel_SetInputOutput_HasComplexObjects_ExpectedHasInputOutput()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object) { BaseCollection = new OptomizedObservableCollection<DataListHeaderItemModel>() };
            const string complexObject = "testing";
            const string complexObjectChild = "item";
            var complexObjectDataModel = CreateComplexObjectDataListModel(complexObject);
            complexObjectDataModel.Input = true;
            complexObjectDataModel.Output = true;
            complexObjectDataModel.Children.Add(CreateComplexObjectDataListModel(complexObjectChild, complexObjectDataModel));
            dataListViewModel.Add(complexObjectDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateComplexObjectPart(complexObject);
            dataListParts.Add(part.Object);
            //------------Execute Test---------------------------
            dataListViewModel.SetIsUsedDataListItems(dataListParts, false);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListViewModel.ComplexObjectCollection[0].Input);
            Assert.IsTrue(dataListViewModel.ComplexObjectCollection[0].Output);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_RemoveDataListItem")]
        public void DataListViewModel_RemoveDataListItem_WithComplexObjectItem_ShouldRemoveFromComplexObjectItemCollection()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            const string complexObject = "testing";
            const string complexObjectChild = "item";
            var complexObjectDataModel = CreateComplexObjectDataListModel(complexObject);
            complexObjectDataModel.Input = true;
            complexObjectDataModel.Output = true;
            complexObjectDataModel.Children.Add(CreateComplexObjectDataListModel(complexObjectChild, complexObjectDataModel));
            dataListViewModel.Add(complexObjectDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateComplexObjectPart(complexObject);
            dataListParts.Add(part.Object);
            //----------------------Precondition----------------------------
            Assert.AreEqual(1, dataListViewModel.ComplexObjectCollectionCount);

            complexObjectDataModel.IsUsed = false;
            complexObjectDataModel.Children[0].IsUsed = false;
            //------------Execute Test---------------------------
            dataListViewModel.RemoveDataListItem(complexObjectDataModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, dataListViewModel.ComplexObjectCollectionCount);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_RemoveDuplicateDataListItem")]
        public void DataListViewModel_RemoveDuplicateDataListItem_WithComplexObjectItem_ShouldRemoveFromComplexObjectItemCollection()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            const string complexObject = "testing";
            const string complexObjectChild = "item";
            var complexObjectDataModel = CreateComplexObjectDataListModel(complexObject);
            complexObjectDataModel.Input = true;
            complexObjectDataModel.Output = true;
            complexObjectDataModel.Children.Add(CreateComplexObjectDataListModel(complexObjectChild, complexObjectDataModel));
            dataListViewModel.Add(complexObjectDataModel);
            dataListViewModel.Add(complexObjectDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateComplexObjectPart(complexObject);
            dataListParts.Add(part.Object);
            //----------------------Precondition----------------------------
            Assert.AreEqual(2, dataListViewModel.ComplexObjectCollectionCount);

            complexObjectDataModel.IsUsed = false;
            complexObjectDataModel.Children[0].IsUsed = false;
            //------------Execute Test---------------------------
            dataListViewModel.RemoveDataListItem(complexObjectDataModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dataListViewModel.ComplexObjectCollectionCount);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_RemoveDataListItem")]
        public void DataListViewModel_RemoveDataListItem_WithComplexObjectItem_ShouldNotRemoveFromComplexObjectItemCollection()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            const string complexObject = "testing";
            const string complexObjectChild = "item";
            var complexObjectDataModel = CreateComplexObjectDataListModel(complexObject);
            complexObjectDataModel.Input = true;
            complexObjectDataModel.Output = true;
            complexObjectDataModel.Children.Add(CreateComplexObjectDataListModel(complexObjectChild, complexObjectDataModel));
            dataListViewModel.Add(complexObjectDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateComplexObjectPart(complexObject);
            dataListParts.Add(part.Object);
            //----------------------Precondition----------------------------
            Assert.AreEqual(1, dataListViewModel.ComplexObjectCollectionCount);

            //------------Execute Test---------------------------
            dataListViewModel.RemoveDataListItem(complexObjectDataModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dataListViewModel.ComplexObjectCollectionCount);
            Assert.IsTrue(dataListViewModel.ComplexObjectCollection[0].IsUsed);
            Assert.IsTrue(dataListViewModel.ComplexObjectCollection[0].Children[0].IsUsed);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_UpdateDataListItems")]
        public void DataListViewModel_UpdateDataListItems_NoMissingComplexObjectsWorkflowItems_ShouldMarkComplexObjectsValuesUsedTrue()
        {
            //------------Setup for test--------------------------
            var resourceModel = new Mock<IResourceModel>().Object;
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            const string complexObject = "testing";
            const string complexObjectChild1 = "item(1)";
            const string complexObjectChild2 = "new";
            var complexObjectDataModel = CreateComplexObjectDataListModel(complexObject);
            complexObjectDataModel.Children.Add(CreateComplexObjectDataListModel(complexObjectChild1, complexObjectDataModel));
            complexObjectDataModel.Children.Add(CreateComplexObjectDataListModel(complexObjectChild2, complexObjectDataModel));
            dataListViewModel.Add(complexObjectDataModel);
            var parts = new List<IDataListVerifyPart>
            {
                CreateComplexObjectPart(complexObject).Object,
                CreateComplexObjectPart(complexObjectChild1).Object,
                CreateComplexObjectPart(complexObjectChild2).Object
            };
            //------------Execute Test---------------------------
            dataListViewModel.UpdateDataListItems(resourceModel, parts);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListViewModel.ComplexObjectCollection[0].IsUsed);
            Assert.IsTrue(dataListViewModel.ComplexObjectCollection[0].Children[0].IsUsed);
            Assert.IsTrue(dataListViewModel.ComplexObjectCollection[0].Children[1].IsUsed);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_ViewComplexObject")]
        public void DataListViewModel_ViewComplexObject_NoMissingComplexObjectsWorkflowItems_ShouldShowViewComplexObjectsTrue()
        {
            //------------Setup for test--------------------------
            var resourceModel = new Mock<IResourceModel>().Object;
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            const string complexObject = "testing";
            const string complexObjectChild1 = "item()";
            const string complexObjectChild2 = "new";
            var complexObjectDataModel = CreateComplexObjectDataListModel(complexObject);
            complexObjectDataModel.Children.Add(CreateComplexObjectDataListModel(complexObjectChild1, complexObjectDataModel));
            complexObjectDataModel.Children.Add(CreateComplexObjectDataListModel(complexObjectChild2, complexObjectDataModel));
            dataListViewModel.Add(complexObjectDataModel);
            var parts = new List<IDataListVerifyPart> { CreateComplexObjectPart(complexObject).Object };
            //------------Execute Test---------------------------
            dataListViewModel.UpdateDataListItems(resourceModel, parts);
            var canExec = dataListViewModel.ViewComplexObjectsCommand.CanExecute(dataListViewModel.ComplexObjectCollection[0]);
            //------------Assert Results-------------------------
            Assert.IsTrue(canExec);
            Assert.IsNotNull(dataListViewModel.ComplexObjectCollection[0]);
            var mockJsonObjectsView = new Mock<IJsonObjectsView>();
            mockJsonObjectsView.Setup(view => view.ShowJsonString(It.IsAny<string>())).Verifiable();
            CustomContainer.RegisterInstancePerRequestType<IJsonObjectsView>(() => mockJsonObjectsView.Object);
            dataListViewModel.ViewComplexObjectsCommand.Execute(dataListViewModel.ComplexObjectCollection[0]);
            mockJsonObjectsView.Verify(model => model.ShowJsonString(It.IsAny<string>()), Times.Once());
            CustomContainer.DeRegister<IJsonObjectsView>();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_DeleteComplexObject")]
        public void DataListViewModel_DeleteComplexObject_NoMissingComplexObjectsWorkflowItems_ShouldShoDeleteComplexObjectsTrue()
        {
            //------------Setup for test--------------------------
            var resourceModel = new Mock<IResourceModel>().Object;
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            const string complexObject = "testing";
            const string complexObjectChild1 = "item()";
            const string complexObjectChild2 = "new";
            var complexObjectDataModel = CreateComplexObjectDataListModel(complexObject);
            complexObjectDataModel.Children.Add(CreateComplexObjectDataListModel(complexObjectChild1, complexObjectDataModel));
            complexObjectDataModel.Children.Add(CreateComplexObjectDataListModel(complexObjectChild2, complexObjectDataModel));
            dataListViewModel.Add(complexObjectDataModel);
            var parts = new List<IDataListVerifyPart> { CreateComplexObjectPart(complexObject).Object };
            //------------Execute Test---------------------------
            dataListViewModel.UpdateDataListItems(resourceModel, parts);
            complexObjectDataModel.IsUsed = false;
            complexObjectDataModel.Children[0].IsUsed = false;
            complexObjectDataModel.Children[1].IsUsed = false;

            var canExec = dataListViewModel.DeleteCommand.CanExecute(dataListViewModel.ComplexObjectCollection[0]);
            Assert.IsTrue(canExec);

            dataListViewModel.DeleteCommand.Execute(complexObjectDataModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, dataListViewModel.ComplexObjectCollectionCount);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_RemoveDataListItem")]
        public void DataListViewModel_RemoveDataListItem_WithNullItem_ShouldDoNothing()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            const string recsetName = "recset";
            const string firstFieldName = "f1";
            var recSetDataModel = CreateRecsetDataListModelWithTwoFields(recsetName, firstFieldName, "f2");
            dataListViewModel.Add(recSetDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateRecsetPart(recsetName, firstFieldName);
            dataListParts.Add(part.Object);
            //----------------------Precondition----------------------------
            Assert.AreEqual(1, dataListViewModel.RecsetCollectionCount);
            //------------Execute Test---------------------------
            dataListViewModel.RemoveDataListItem(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dataListViewModel.RecsetCollectionCount);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_RemoveDataListItem")]
        public void DataListViewModel_RemoveDataListItem_WithScalarItem_ShouldRemoveFromScalarCollection()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            var scalarItem = new ScalarItemModel("scalar");
            dataListViewModel.Add(scalarItem);
            //----------------------Precondition----------------------------
            Assert.AreEqual(1, dataListViewModel.ScalarCollectionCount);
            //------------Execute Test---------------------------
            dataListViewModel.RemoveDataListItem(scalarItem);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, dataListViewModel.ScalarCollectionCount);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_RemoveDuplicateDataListItem")]
        public void DataListViewModel_RemoveDuplicateDataListItem_WithScalarItem_ShouldRemoveFromScalarCollection()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            var scalarItem = new ScalarItemModel("scalar");
            dataListViewModel.Add(scalarItem);
            dataListViewModel.Add(scalarItem);
            //----------------------Precondition----------------------------
            Assert.AreEqual(2, dataListViewModel.ScalarCollectionCount);
            //------------Execute Test---------------------------
            dataListViewModel.RemoveDataListItem(scalarItem);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dataListViewModel.ScalarCollectionCount);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_RemoveDataListItem")]
        public void DataListViewModel_RemoveDataListItem_WithRecsetItem_ShouldRemoveFromRecsetCollection()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            const string recsetName = "recset";
            const string firstFieldName = "f1";
            var recSetDataModel = CreateRecsetDataListModelWithTwoFields(recsetName, firstFieldName, "f2");
            dataListViewModel.Add(recSetDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateRecsetPart(recsetName, firstFieldName);
            dataListParts.Add(part.Object);
            //----------------------Precondition----------------------------
            Assert.AreEqual(1, dataListViewModel.RecsetCollectionCount);
            //------------Execute Test---------------------------
            dataListViewModel.RemoveDataListItem(recSetDataModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, dataListViewModel.RecsetCollectionCount);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_RemoveDuplicateDataListItem")]
        public void DataListViewModel_RemoveDuplicateDataListItem_WithRecsetItem_ShouldRemoveFromRecsetCollection()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            const string recsetName = "recset";
            const string firstFieldName = "f1";
            var recSetDataModel = CreateRecsetDataListModelWithTwoFields(recsetName, firstFieldName, "f2");
            dataListViewModel.Add(recSetDataModel);
            dataListViewModel.Add(recSetDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateRecsetPart(recsetName, firstFieldName);
            dataListParts.Add(part.Object);
            //----------------------Precondition----------------------------
            Assert.AreEqual(2, dataListViewModel.RecsetCollectionCount);
            //------------Execute Test---------------------------
            dataListViewModel.RemoveDataListItem(recSetDataModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dataListViewModel.RecsetCollectionCount);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_RemoveDataListItem")]
        public void DataListViewModel_RemoveDataListItem_WithRecsetFieldItem_ShouldRemoveFromRecsetChildrenCollection()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            const string recsetName = "recset";
            const string firstFieldName = "f1";
            var recSetDataModel = DataListItemModelFactory.CreateRecordSetItemModel(recsetName, "A recordset of information about a car");
            var firstFieldDataListItemModel = CreateRecordSetFieldDataListModel(firstFieldName, recSetDataModel);
            recSetDataModel.Children.Add(firstFieldDataListItemModel);
            recSetDataModel.Children.Add(CreateRecordSetFieldDataListModel("f2", recSetDataModel));
            dataListViewModel.Add(recSetDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateRecsetPart(recsetName, firstFieldName);
            dataListParts.Add(part.Object);
            //----------------------Precondition----------------------------
            Assert.AreEqual(2, dataListViewModel.RecsetCollection[0].Children.Count);
            //------------Execute Test---------------------------
            dataListViewModel.RemoveDataListItem(firstFieldDataListItemModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dataListViewModel.RecsetCollection[0].Children.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_RemoveDuplicateDataListItem")]
        public void DataListViewModel_RemoveDuplicateDataListItem_WithRecsetFieldItem_ShouldRemoveFromRecsetChildrenCollection()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            const string recsetName = "recset";
            const string firstFieldName = "f1";
            var recSetDataModel = DataListItemModelFactory.CreateRecordSetItemModel(recsetName, "A recordset of information about a car");
            var firstFieldDataListItemModel = CreateRecordSetFieldDataListModel(firstFieldName, recSetDataModel);
            recSetDataModel.Children.Add(firstFieldDataListItemModel);
            recSetDataModel.Children.Add(CreateRecordSetFieldDataListModel("f2", recSetDataModel));
            dataListViewModel.Add(recSetDataModel);
            dataListViewModel.Add(recSetDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateRecsetPart(recsetName, firstFieldName);
            dataListParts.Add(part.Object);
            //----------------------Precondition----------------------------
            Assert.AreEqual(2, dataListViewModel.RecsetCollection[0].Children.Count);
            //------------Execute Test---------------------------
            dataListViewModel.RemoveDataListItem(firstFieldDataListItemModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dataListViewModel.RecsetCollection[0].Children.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_MissingDataListParts")]
        public void DataListViewModel_MissingDataListParts_ScalarPartNotInDataList_ShouldReturnPartInList()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            const string scalarName = "scalar";
            var parts = new List<IDataListVerifyPart> { CreateScalarPart(scalarName).Object };
            //----------------------Precondition----------------------------
            //------------Execute Test---------------------------
            var missingDataListParts = dataListViewModel.MissingDataListParts(parts);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, missingDataListParts.Count);
            Assert.AreEqual(scalarName, missingDataListParts[0].Field);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_MissingDataListParts")]
        public void DataListViewModel_MissingDataListParts_ScalarPartInDataList_ShouldNotReturnPartInList()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            const string scalarName = "scalar";
            var scalarItem = new ScalarItemModel(scalarName);
            dataListViewModel.Add(scalarItem);
            var parts = new List<IDataListVerifyPart> { CreateScalarPart(scalarName).Object };
            //----------------------Precondition----------------------------
            Assert.AreEqual(1, dataListViewModel.ScalarCollectionCount);
            //------------Execute Test---------------------------
            var missingDataListParts = dataListViewModel.MissingDataListParts(parts);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, missingDataListParts.Count);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_MissingDataListParts")]
        public void DataListViewModel_MissingDataListParts_WithRecsetPartNotInDataList_ShouldReturnPartInList()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            const string recsetName = "recset";
            const string firstFieldName = "f1";
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateRecsetPart(recsetName, firstFieldName);
            dataListParts.Add(part.Object);
            //----------------------Precondition----------------------------
            //------------Execute Test---------------------------
            var missingDataListParts = dataListViewModel.MissingDataListParts(dataListParts);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, missingDataListParts.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_MissingDataListParts")]
        public void DataListViewModel_MissingDataListParts_WithRecsetFieldPartNotInDataList_ShouldReturnPartInList()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            const string recsetName = "recset";
            const string firstFieldName = "f1";
            var recSetDataModel = DataListItemModelFactory.CreateRecordSetItemModel(recsetName, "A recordset of information about a car");
            var firstFieldDataListItemModel = CreateRecordSetFieldDataListModel(firstFieldName, recSetDataModel);
            recSetDataModel.Children.Add(firstFieldDataListItemModel);
            dataListViewModel.Add(recSetDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateRecsetPart(recsetName, "f2");
            dataListParts.Add(part.Object);
            //----------------------Precondition----------------------------
            Assert.AreEqual(1, dataListViewModel.RecsetCollection[0].Children.Count);
            //------------Execute Test---------------------------
            var missingDataListParts = dataListViewModel.MissingDataListParts(dataListParts);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, missingDataListParts.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_MissingDataListParts")]
        public void DataListViewModel_MissingDataListParts_WithRecsetPartInDataList_ShouldNotReturnPartInList()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            const string recsetName = "recset";
            const string firstFieldName = "f1";
            var recSetDataModel = DataListItemModelFactory.CreateRecordSetItemModel(recsetName, "A recordset of information about a car");
            var firstFieldDataListItemModel = CreateRecordSetFieldDataListModel(firstFieldName, recSetDataModel);
            recSetDataModel.Children.Add(firstFieldDataListItemModel);
            recSetDataModel.Children.Add(CreateRecordSetFieldDataListModel("f2", recSetDataModel));
            dataListViewModel.Add(recSetDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateRecsetPart(recsetName, firstFieldName);
            dataListParts.Add(part.Object);
            //----------------------Precondition----------------------------
            Assert.AreEqual(2, dataListViewModel.RecsetCollection[0].Children.Count);
            //------------Execute Test---------------------------
            var missingDataListParts = dataListViewModel.MissingDataListParts(dataListParts);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, missingDataListParts.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_MissingDataListParts")]
        public void DataListViewModel_MissingDataListParts_WithRecsetFieldPartInDataList_ShouldNotReturnPartInList()
        {
            //------------Setup for test--------------------------
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            const string recsetName = "recset";
            const string firstFieldName = "f1";
            var recSetDataModel = DataListItemModelFactory.CreateRecordSetItemModel(recsetName, "A recordset of information about a car");
            var firstFieldDataListItemModel = CreateRecordSetFieldDataListModel(firstFieldName, recSetDataModel);
            recSetDataModel.Children.Add(firstFieldDataListItemModel);
            recSetDataModel.Children.Add(CreateRecordSetFieldDataListModel("f2", recSetDataModel));
            dataListViewModel.Add(recSetDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateRecsetPart(recsetName, firstFieldName);
            dataListParts.Add(part.Object);
            //----------------------Precondition----------------------------
            Assert.AreEqual(2, dataListViewModel.RecsetCollection[0].Children.Count);
            //------------Execute Test---------------------------
            var missingDataListParts = dataListViewModel.MissingDataListParts(dataListParts);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, missingDataListParts.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_UpdateDataListItems")]
        public void DataListViewModel_UpdateDataListItems_NoMissingScalarWorkflowItems_ShouldMarkScalarValuesUsedTrue()
        {
            //------------Setup for test--------------------------
            var resourceModel = new Mock<IResourceModel>().Object;
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            const string scalarName = "scalar";
            var scalarItem = new ScalarItemModel(scalarName) { IsUsed = false };
            dataListViewModel.Add(scalarItem);
            var parts = new List<IDataListVerifyPart> { CreateScalarPart(scalarName).Object };
            //------------Execute Test---------------------------
            dataListViewModel.UpdateDataListItems(resourceModel, parts);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListViewModel.ScalarCollection[0].IsUsed);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_UpdateDataListItems")]
        public void DataListViewModel_UpdateDataListItems_WithNoMissingRecsetWorkflowItems_ShouldMarkRecsetValueIsUsedTrue()
        {
            //------------Setup for test--------------------------
            var resourceModel = new Mock<IResourceModel>().Object;
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            const string recsetName = "recset";
            const string firstFieldName = "f1";
            var recSetDataModel = DataListItemModelFactory.CreateRecordSetItemModel(recsetName, "A recordset of information about a car");
            recSetDataModel.IsUsed = false;
            var firstFieldDataListItemModel = CreateRecordSetFieldDataListModel(firstFieldName, recSetDataModel);
            recSetDataModel.Children.Add(firstFieldDataListItemModel);
            dataListViewModel.Add(recSetDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateRecsetPart(recsetName, firstFieldName);
            dataListParts.Add(part.Object);
            //------------Execute Test---------------------------
            dataListViewModel.UpdateDataListItems(resourceModel, dataListParts);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListViewModel.RecsetCollection[0].IsUsed);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListViewModel_UpdateDataListItems")]
        public void DataListViewModel_UpdateDataListItems_WithNoMissingRecsetFieldWorkflowItemsIsUsedFalse_ShouldMarkRecsetFieldValueIsUsedTrue()
        {
            //------------Setup for test--------------------------
            var resourceModel = new Mock<IResourceModel>().Object;
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            const string recsetName = "recset";
            const string firstFieldName = "f1";
            var recSetDataModel = DataListItemModelFactory.CreateRecordSetItemModel(recsetName, "A recordset of information about a car");
            var firstFieldDataListItemModel = CreateRecordSetFieldDataListModel(firstFieldName, recSetDataModel);
            recSetDataModel.IsUsed = false;
            firstFieldDataListItemModel.IsUsed = false;
            recSetDataModel.Children.Add(firstFieldDataListItemModel);
            dataListViewModel.Add(recSetDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateRecsetPart(recsetName, firstFieldName);
            dataListParts.Add(part.Object);
            //------------Execute Test---------------------------
            dataListViewModel.UpdateDataListItems(resourceModel, dataListParts);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListViewModel.RecsetCollection[0].Children[0].IsUsed);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_UpdateDataListItems")]
        public void DataListViewModel_UpdateDataListItems_WithNoChildrenRecsetFieldWorkflowItemsIsUsedFalse_ShouldMarkRecsetFieldValueIsUsedTrue()
        {
            //------------Setup for test--------------------------
            var resourceModel = new Mock<IResourceModel>().Object;
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            const string recsetName = "recset";
            const string firstFieldName = "f1";
            var recSetDataModel = DataListItemModelFactory.CreateRecordSetItemModel(recsetName, "A recordset of information about a car");
            var firstFieldDataListItemModel = CreateRecordSetFieldDataListModel(firstFieldName, recSetDataModel);
            recSetDataModel.IsUsed = false;
            firstFieldDataListItemModel.IsUsed = false;
            dataListViewModel.Add(recSetDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateRecsetPart(recsetName, firstFieldName);
            dataListParts.Add(part.Object);
            //------------Execute Test---------------------------
            dataListViewModel.UpdateDataListItems(resourceModel, dataListParts);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListViewModel.RecsetCollection[0].Children[0].IsUsed);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataListViewModel_UpdateDataListItems")]
        public void DataListViewModel_UpdateDataListItems_WithNoChildrenRecsetFieldWorkflowItemsIsUsedTrue_ShouldMarkRecsetFieldValueIsUsedTrue()
        {
            //------------Setup for test--------------------------
            var resourceModel = new Mock<IResourceModel>().Object;
            var dataListViewModel = new DataListViewModel(new Mock<IEventAggregator>().Object);
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            const string recsetName = "recset";
            const string firstFieldName = "f1";
            var recSetDataModel = DataListItemModelFactory.CreateRecordSetItemModel(recsetName, "A recordset of information about a car");
            var firstFieldDataListItemModel = CreateRecordSetFieldDataListModel(firstFieldName, recSetDataModel);
            recSetDataModel.IsUsed = true;
            firstFieldDataListItemModel.IsUsed = true;
            dataListViewModel.Add(recSetDataModel);
            var dataListParts = new List<IDataListVerifyPart>();
            var part = CreateRecsetPart(recsetName, firstFieldName);
            dataListParts.Add(part.Object);
            //------------Execute Test---------------------------
            dataListViewModel.UpdateDataListItems(resourceModel, dataListParts);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataListViewModel.RecsetCollection[0].Children[0].IsUsed);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void InitializeDataListViewModel_GivenNull_ShouldReturn()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var eventAggregator = new Mock<IEventAggregator>();
            var dataListViewModel = new DataListViewModel(eventAggregator.Object);
            dataListViewModel.InitializeDataListViewModel(null);

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetComplexObjectParentIsUsed_GivenParentAndChild_ShouldSetAllToIsUsed()
        {
            //---------------Set up test pack-------------------
            var resourceModel = new Mock<IResourceModel>().Object;
            var eventAggregator = new Mock<IEventAggregator>();
            var dataListViewModel = new DataListViewModel(eventAggregator.Object);
            dataListViewModel.InitializeDataListViewModel(resourceModel);

            var parent = CreateComplexObjectDataListModel("Child");
            var child = CreateComplexObjectDataListModel("Child", parent);
            dataListViewModel.Add(child);
            var handler = new ComplexObjectHandler(dataListViewModel);
            var p = new PrivateObject(handler);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            p.Invoke("SetComplexObjectParentIsUsed", child);
            //---------------Test Result -----------------------
            Assert.IsTrue(parent.IsUsed);
            Assert.IsTrue(child.IsUsed);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnDispose_ShouldClearAndKillTheResource()
        {
            //---------------Set up test pack-------------------
            var resourceModel = new Mock<IResourceModel>().Object;
            var eventAggregator = new Mock<IEventAggregator>();
            var dataListViewModel = new DataListViewModel(eventAggregator.Object);
            dataListViewModel.InitializeDataListViewModel(resourceModel);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            dataListViewModel.Dispose();
            //---------------Test Result -----------------------
            Assert.IsTrue(dataListViewModel.Resource == null);
            Assert.IsNotNull(dataListViewModel.ScalarCollection.Single());
            Assert.IsNotNull(dataListViewModel.RecsetCollection.Single());
            Assert.AreEqual(0, dataListViewModel.ComplexObjectCollectionCount);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDataListString_GivenComplexobjectModel_ShouldBuildXmlCorrectly()
        {
            //---------------Set up test pack-------------------
            var resourceModel = new Mock<IResourceModel>().Object;
            var eventAggregator = new Mock<IEventAggregator>();
            var dataListViewModel = new DataListViewModel(eventAggregator.Object);
            dataListViewModel.InitializeDataListViewModel(resourceModel);

            IComplexObjectItemModel item = new ComplexObjectItemModel("Name", null, enDev2ColumnArgumentDirection.Input, "Name", null, false, "", true, true, false, false);
            dataListViewModel.Add(item);
            var privateObject = new PrivateObject(dataListViewModel);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                //---------------Test Result -----------------------
                var invoke = privateObject.Invoke("GetDataListString");
                var document = new XmlDocument();
                document.LoadXml(invoke.ToString());
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        (IRecordSetFieldItemModel, DataListViewModel) SetupForValidateNamesDuplicateRecordSetFieldsTests()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Recordset).Returns("ab");
            part.Setup(c => c.DisplayValue).Returns("[[ab()]]");
            part.Setup(c => c.Description).Returns("");
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.Field).Returns("");
            part.Setup(c => c.IsJson).Returns(false);
            parts.Add(part.Object);

            var part2 = new Mock<IDataListVerifyPart>();
            part2.Setup(c => c.Recordset).Returns("ab");
            part2.Setup(c => c.DisplayValue).Returns("[[ab().c]]");
            part2.Setup(c => c.Description).Returns("");
            part2.Setup(c => c.IsScalar).Returns(false);
            part2.Setup(c => c.Field).Returns("c");
            part.Setup(c => c.IsJson).Returns(false);
            parts.Add(part2.Object);

            _dataListViewModel.AddMissingDataListItems(parts);

            IRecordSetFieldItemModel item = new RecordSetFieldItemModel("ab().c");
            item.DisplayName = "c";
            item.Parent = _dataListViewModel.RecsetCollection[0];
            return (item, _dataListViewModel);
        }

        (IScalarItemModel, DataListViewModel) SetupForValidateNamesDuplicateScalarTests()
        {
            var (_mockResourceModel, _dataListViewModel) = Setup();

            _dataListViewModel.RecsetCollection.Clear();
            _dataListViewModel.ScalarCollection.Clear();

            IScalarItemModel item = new ScalarItemModel("ab");
            item.DisplayName = "ab";
            _dataListViewModel.ScalarCollection.Insert(0, item);
            return (item, _dataListViewModel);
        }


        static IRecordSetItemModel CreateRecsetDataListModelWithTwoFields(string recsetName, string firstFieldName, string secondFieldName)
        {
            var recSetDataModel = DataListItemModelFactory.CreateRecordSetItemModel(recsetName, "A recordset of information about a car");
            recSetDataModel.Children.Add(CreateRecordSetFieldDataListModel(firstFieldName, recSetDataModel));
            recSetDataModel.Children.Add(CreateRecordSetFieldDataListModel(secondFieldName, recSetDataModel));
            return recSetDataModel;
        }

        static IRecordSetFieldItemModel CreateRecordSetFieldDataListModel(string fieldName, IRecordSetItemModel recSetDataModel)
        {
            var fieldDataListModel = DataListItemModelFactory.CreateRecordSetFieldItemModel(fieldName, "", recSetDataModel);
            fieldDataListModel.DisplayName = recSetDataModel.DisplayName + "()." + fieldName;
            return fieldDataListModel;
        }

        static IComplexObjectItemModel CreateComplexObjectDataListModel(string displayName, IComplexObjectItemModel parent = null)
        {
            IComplexObjectItemModel complexObject = new ComplexObjectItemModel(displayName, parent);
            return complexObject;
        }

        static Mock<IDataListVerifyPart> CreateComplexObjectPart(string fieldName)
        {
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Field).Returns(fieldName);
            part.Setup(c => c.DisplayValue).Returns(fieldName);
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.IsJson).Returns(true);
            return part;
        }

        static Mock<IDataListVerifyPart> CreateRecsetPart(string recsetName, string fieldName)
        {
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Field).Returns(fieldName);
            part.Setup(c => c.Recordset).Returns(recsetName);
            part.Setup(c => c.IsScalar).Returns(false);
            part.Setup(c => c.IsJson).Returns(false);
            return part;
        }

        static Mock<IDataListVerifyPart> CreateScalarPart(string name)
        {
            var part = new Mock<IDataListVerifyPart>();
            part.Setup(c => c.Field).Returns(name);
            part.Setup(c => c.IsScalar).Returns(true);
            part.Setup(c => c.IsJson).Returns(false);
            return part;
        }


        IEnumerable<IScalarItemModel> CreateScalarListItems(IEnumerable<IDataListVerifyPart> parts)
        {
            return (from part in parts where part.IsScalar select DataListItemModelFactory.CreateScalarItemModel(part.Field, part.Description)).ToList();
        }
    }
}
