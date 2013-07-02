using Dev2.Composition;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Dev2.Studio.Core.Interfaces;
using System.Collections.Generic;
using Moq;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Factories;
using System.Collections.ObjectModel;

using Dev2.Core.Tests.Utils;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Core.Tests {

    // Sashen - 16:10:2012 : This class under test requires clarification on certain behaviours.

    /// <summary>
    ///This is a result class for InputOutputViewModelTest and is intended
    ///to contain all InputOutputViewModelTest Unit Tests
    ///</summary>
    [TestClass()]
    public class InputOutputViewModelTest {

        #region Local Test variables

        InputOutputViewModel _inputOutputViewModel;

        #endregion Local Test variables

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the result context which provides
        ///information about and functionality for the current result run.
        ///</summary>
        public TestContext TestContext {
            get { 
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional result attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first result in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each result
        [TestInitialize()]
        public void MyTestInitialize() 
        {
            ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();
            InputOutputViewModelTestObject testObject = new InputOutputViewModelTestObject();
            _inputOutputViewModel = new InputOutputViewModel(testObject.Name, testObject.Value, testObject.MapsTo, testObject.DefaultValue, testObject.Required, testObject.RecordSetName);

        }

        #endregion Additional result attributes

        #region Mock Creation

        private void MockCreation() {
            Mock<IContextualResourceModel> resource = Dev2MockFactory.SetupResourceModelMock();
            resource.Setup(res => res.DataList).Returns(StringResourcesTest.xmlDataList);
            OptomizedObservableCollection<IDataListItemModel> Items = new OptomizedObservableCollection<IDataListItemModel>();

            Mock<IDataListViewModel> dataListViewModel = Dev2MockFactory.SetupDataListViewModel();
            Mock<IMainViewModel> mockMainViewModel = Dev2MockFactory.SetupMainViewModel();
            dataListViewModel.Setup(dataList => dataList.DataList).Returns(Items);

            //Juries 8810 TODO
            //mockMainViewModel.Setup(mainVM => mainVM.ActiveDataList.DataList).Returns(dataListViewModel.Object.DataList);
            dataListViewModel.Setup(c => c.Resource).Returns(resource.Object);



            Mock<IDataListItemModel> item = Dev2MockFactory.SetupDataListItemViewModel();
            Items.Add(item.Object);

            string xmlDataList = StringResourcesTest.xmlDataList;
            DataListSingleton.SetDataList(dataListViewModel.Object);
            //inputOutputViewModel = new InputOutputViewModel(name, value, mapsTo, defaultValue, required, recordSetName);
        }
        //
        //Use TestCleanup to run code after each result has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        #region CTOR Tests

        /// <summary>
        ///A result for InputOutputViewModel Constructor
        ///</summary>
        [TestMethod]
        public void InputOutputViewModelConstructor() {

            //Assert.AreEqual(inputOutputViewModel.Value, inputOutputViewModel.SelectedDataListItem.Name);
        }        


        // Travis  : PBI 5779
        [TestMethod()]
        public void DefaultWithEmptyToNull_Expect_EmptyToNullString()
        {
            string name = "vehicleColor";
            string xmlDataList = StringResourcesTest.xmlDataList;
            string value = "vehicleColor";
            string mapsTo = "testMapsTo";
            string defaultValue = string.Empty;
            string recordSetName = "testRecSetName";
            bool required = true;
            _inputOutputViewModel = new InputOutputViewModel(name, value, mapsTo, defaultValue, required, recordSetName, true);
            string actual = _inputOutputViewModel.DisplayDefaultValue;
            Assert.AreEqual("Empty to NULL", actual);
        }


        // Travis  : PBI 5779
        [TestMethod()]
        public void DefaultWithEmptyToNull_Expect_EmptyString()
        {
            string name = "vehicleColor";
            string xmlDataList = StringResourcesTest.xmlDataList;
            string value = "vehicleColor";
            string mapsTo = "testMapsTo";
            string defaultValue = string.Empty;
            string recordSetName = "testRecSetName";
            bool required = true;
            _inputOutputViewModel = new InputOutputViewModel(name, value, mapsTo, defaultValue, required, recordSetName, false);
            string actual = _inputOutputViewModel.DisplayDefaultValue;
            Assert.AreEqual(string.Empty, actual);
        }

        // Travis  : PBI 5779
        [TestMethod()]
        public void DefaultWithEmptyToNullDefaultValueSet_Expect_DefaultValue()
        {
            string name = "vehicleColor";
            string xmlDataList = StringResourcesTest.xmlDataList;
            string value = "vehicleColor";
            string mapsTo = "testMapsTo";
            string defaultValue = "default val";
            string recordSetName = "testRecSetName";
            bool required = true;
            _inputOutputViewModel = new InputOutputViewModel(name, value, mapsTo, defaultValue, required, recordSetName, true);
            string actual = _inputOutputViewModel.DisplayDefaultValue;
            Assert.AreEqual("Default: default val", actual);
        }


        /// <summary>
        ///A result for DisplayName
        ///</summary>
        [TestMethod]
        public void DisplayName() {

            string actual = _inputOutputViewModel.DisplayName;
            Assert.AreEqual("testRecSetName(*).vehicleColor", actual);
        }

        //BUG - This test replicates a big issue with the logic in the Input Mapper creation

        //[TestMethod]
        //public void DisplayNameScalarNullRecordSet_Expected_ScalarNotationSetAsDisplayName() {
        //    InputOutputViewModelTestObject testObject = new InputOutputViewModelTestObject();
        //    testObject.Name = null;
        //    SetInputOutputMappingViewModelFromTestMappingObject(testObject);
        //    Assert.IsFalse(Unlimited.Framework.DataList.DataListUtil.IsValueRecordset(_inputOutputViewModel.DisplayName));
        //}

        [TestMethod]
        public void DisplayNameScalarEmptyStringRecordSet_Expected_ScalarNotationSetAsDisplayName() {
            InputOutputViewModelTestObject testObject = new InputOutputViewModelTestObject();
            testObject.RecordSetName = string.Empty;
            SetInputOutputMappingViewModelFromTestMappingObject(testObject);
            
            Assert.IsFalse(DataListUtil.IsValueRecordset(_inputOutputViewModel.DisplayName));
        }


        #region Tests Currently Containing unknown expecteds

        [TestMethod]
        public void DisplayNameRecordSet_Expected_DisplayNameIsRecordSetNotation() {
            InputOutputViewModelTestObject testObject = new InputOutputViewModelTestObject();
            testObject.Name = string.Empty;
            testObject.RecordSetName = "TestRC";
            testObject.Value = "val";
            SetInputOutputMappingViewModelFromTestMappingObject(testObject); ;

            Assert.IsTrue(DataListUtil.IsValueRecordset(_inputOutputViewModel.DisplayName));
        }


        //BUG - This test replicates a big issue with the logic in the Input Mapper creation

        //[TestMethod]
        //public void DisplayNameRecordSetNullField_Expected_ValueIsNotRecordSet() {
        //    InputOutputViewModelTestObject testObject = new InputOutputViewModelTestObject();
        //    testObject.RecordSetName = null;
        //    SetInputOutputMappingViewModelFromTestMappingObject(testObject); ;

        //    Assert.IsFalse(Unlimited.Framework.DataList.DataListUtil.IsValueRecordset(_inputOutputViewModel.DisplayName));
        //}

        #endregion Tests Currently Containing unknown expecteds

        #endregion CTOR Tests

        #region MapsTo Property Tests
        /// <summary>
        ///A result for MapsTo
        ///</summary>
        [TestMethod()]
        public void MapsTo_ValueSupplied_Expected_MapsToFieldcorrectlySet() {
            string expected = "testMapsTo";
            string actual = _inputOutputViewModel.MapsTo;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void MapsTo_EmptyValueSupplied_Expected_MapsToFieldcorrectlySet() {
            InputOutputViewModelTestObject testObject = new InputOutputViewModelTestObject();
            testObject.MapsTo = string.Empty;
            SetInputOutputMappingViewModelFromTestMappingObject(testObject);

            string actual = _inputOutputViewModel.MapsTo;
            Assert.AreEqual(string.Empty, actual);
        }

        #endregion MapsTo Property Tests

        #region Name Property Tests

        /// <summary>
        ///A result for Name
        ///</summary>
        [TestMethod()]
        public void Name_ValueSuppliedForName_Expected_NamePropertySetToValueSupplied() {
            string expected = "vehicleColor";
            string actual = _inputOutputViewModel.Name;
            Assert.AreEqual(expected, actual);
        }


        // Not too sure what the expected is here
        //[TestMethod]
        //public void Name_EmptyValueSupplied_Expected() {
        //    InputOutputViewModelTestObject testObject = new InputOutputViewModelTestObject();
        //    testObject.Name = string.Empty;
        //    SetInputOutputMappingViewModelFromTestMappingObject(testObject);

        //    string actual = _inputOutputViewModel.Name;
        //    Assert.Inconclusive();
        //}

        #endregion Name Property Tests

        #region Value Tests

        /// <summary>
        ///A result for SelectedDataListItem
        ///</summary>
        [TestMethod]
        public void SelectedDataListItem() {            
            string actual = _inputOutputViewModel.Value;
            Assert.AreEqual(_inputOutputViewModel.Value, actual);
        }

        /// <summary>
        ///A result for Value
        ///</summary>
        [TestMethod]
        public void Value() {
            string expected = "vehicleColor";
            string actual = _inputOutputViewModel.Value;
            Assert.AreEqual(expected, actual);
        }

        #endregion Value Tests

        #region GetGenerationTO Tests

        [TestMethod]
        public void GetGenerationTO_ValidViewModel_Expected_Dev2DefinitionCreatedFromInputOutputViewModel() {
            IDev2Definition dev2Definition = _inputOutputViewModel.GetGenerationTO();
            Assert.IsTrue(dev2Definition != null);
        }

        [TestMethod]
        public void GetGenerationTO_InvalidViewModel_Expected_NullDev2DefinitionCreated() {
            InputOutputViewModelTestObject testObject = new InputOutputViewModelTestObject();
            testObject.Name = null;
            SetInputOutputMappingViewModelFromTestMappingObject(testObject);
            IDev2Definition actual = _inputOutputViewModel.GetGenerationTO();
            // Not sure of the outcome here, it is a null name value, and the name is the most essential part of this
            Assert.IsNotNull(actual);
        }

        #endregion GetGenerationTO Tests

        #region Test Methods

        

        private void SetInputOutputMappingViewModel(string name, string value, string mapsTo, string defaultValue, bool required, string recordSetName) {
            _inputOutputViewModel = InputOutputViewModelFactory.CreateInputOutputViewModel(name, value, mapsTo, defaultValue, required, recordSetName);
        }

        private void SetInputOutputMappingViewModelFromTestMappingObject(InputOutputViewModelTestObject mappingObject) {
            _inputOutputViewModel = InputOutputViewModelFactory.CreateInputOutputViewModel(mappingObject.Name, mappingObject.Value, mappingObject.MapsTo, mappingObject.DefaultValue, mappingObject.Required, mappingObject.RecordSetName);
        }

        #endregion Test Methods
    }
}
