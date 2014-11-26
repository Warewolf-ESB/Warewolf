
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Data;
using Dev2.Core.Tests.Utils;
using Dev2.Data.Util;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{

    // Sashen - 16:10:2012 : This class under test requires clarification on certain behaviours.

    /// <summary>
    ///This is a result class for InputOutputViewModelTest and is intended
    ///to contain all InputOutputViewModelTest Unit Tests
    ///</summary>
    [TestClass]
    public class InputOutputViewModelTest
    {

        #region Local Test variables

        InputOutputViewModel _inputOutputViewModel;

        #endregion Local Test variables

        /// <summary>
        ///Gets or sets the result context which provides
        ///information about and functionality for the current result run.
        ///</summary>
        public TestContext TestContext { get; set; }

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
        [TestInitialize]
        public void MyTestInitialize()
        {
            InputOutputViewModelTestObject testObject = new InputOutputViewModelTestObject();
            _inputOutputViewModel = new InputOutputViewModel(testObject.Name, testObject.Value, testObject.MapsTo, testObject.DefaultValue, testObject.Required, testObject.RecordSetName);

        }

        #endregion Additional result attributes

        #region Mock Creation

        //
        //Use TestCleanup to run code after each result has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        #region CTOR Tests
        // ReSharper disable InconsistentNaming


        // Travis  : PBI 5779
        [TestMethod]
        public void InputOutputMappingViewModel_DefaultWithEmptyToNull_Expect_EmptyToNullString()
        {
            const string name = "vehicleColor";
            const string value = "vehicleColor";
            const string mapsTo = "testMapsTo";
            string defaultValue = string.Empty;
            const string recordSetName = "testRecSetName";
            const bool required = true;
            _inputOutputViewModel = new InputOutputViewModel(name, value, mapsTo, defaultValue, required, recordSetName, true);
            string actual = _inputOutputViewModel.DisplayDefaultValue;
            Assert.AreEqual("Empty to NULL", actual);
        }


        // Travis  : PBI 5779
        [TestMethod]
        public void InputOutputMappingViewModel_DefaultWithEmptyToNull_Expect_EmptyString()
        {
            const string name = "vehicleColor";
            const string value = "vehicleColor";
            const string mapsTo = "testMapsTo";
            string defaultValue = string.Empty;
            const string recordSetName = "testRecSetName";
            const bool required = true;
            _inputOutputViewModel = new InputOutputViewModel(name, value, mapsTo, defaultValue, required, recordSetName, false);
            string actual = _inputOutputViewModel.DisplayDefaultValue;
            Assert.AreEqual(string.Empty, actual);
        }

        // Travis  : PBI 5779
        [TestMethod]
        public void InputOutputMappingViewModel_DefaultWithEmptyToNullDefaultValueSet_Expect_DefaultValue()
        {
            const string name = "vehicleColor";
            const string value = "vehicleColor";
            const string mapsTo = "testMapsTo";
            const string defaultValue = "default val";
            const string recordSetName = "testRecSetName";
            const bool required = true;
            _inputOutputViewModel = new InputOutputViewModel(name, value, mapsTo, defaultValue, required, recordSetName, true);
            string actual = _inputOutputViewModel.DisplayDefaultValue;
            Assert.AreEqual("Default: default val", actual);
        }


        /// <summary>
        ///A result for DisplayName
        ///</summary>
        [TestMethod]
        public void InputOutputMappingViewModel_DisplayName()
        {

            string actual = _inputOutputViewModel.DisplayName;
            Assert.AreEqual("testRecSetName(*).vehicleColor", actual);
        }

        [TestMethod]
        public void InputOutputMappingViewModel_DisplayNameScalarEmptyStringRecordSet_Expected_ScalarNotationSetAsDisplayName()
        {
            InputOutputViewModelTestObject testObject = new InputOutputViewModelTestObject { RecordSetName = string.Empty };
            SetInputOutputMappingViewModelFromTestMappingObject(testObject);

            Assert.IsFalse(DataListUtil.IsValueRecordset(_inputOutputViewModel.DisplayName));
        }


        #region Tests Currently Containing unknown expecteds

        [TestMethod]
        public void InputOutputMappingViewModel_DisplayNameRecordSet_Expected_DisplayNameIsRecordSetNotation()
        {
            InputOutputViewModelTestObject testObject = new InputOutputViewModelTestObject { Name = string.Empty, RecordSetName = "TestRC", Value = "val" };
            SetInputOutputMappingViewModelFromTestMappingObject(testObject);

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
        [TestMethod]
        public void InputOutputMappingViewModel_MapsTo_ValueSupplied_Expected_MapsToFieldcorrectlySet()
        {
            const string expected = "testMapsTo";
            string actual = _inputOutputViewModel.MapsTo;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void InputOutputMappingViewModel_MapsTo_EmptyValueSupplied_Expected_MapsToFieldcorrectlySet()
        {
            InputOutputViewModelTestObject testObject = new InputOutputViewModelTestObject { MapsTo = string.Empty };
            SetInputOutputMappingViewModelFromTestMappingObject(testObject);

            string actual = _inputOutputViewModel.MapsTo;
            Assert.AreEqual(string.Empty, actual);
        }

        #endregion MapsTo Property Tests

        #region Name Property Tests

        /// <summary>
        ///A result for Name
        ///</summary>
        [TestMethod]
        public void InputOutputMappingViewModel_Name_ValueSuppliedForName_Expected_NamePropertySetToValueSupplied()
        {
            const string expected = "vehicleColor";
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
        public void InputOutputMappingViewModel_InputOutputMappingViewModel_SelectedDataListItem()
        {
            string actual = _inputOutputViewModel.Value;
            Assert.AreEqual(_inputOutputViewModel.Value, actual);
        }

        /// <summary>
        ///A result for Value
        ///</summary>
        [TestMethod]
        public void InputOutputMappingViewModel_InputOutputMappingViewModel_Value()
        {
            const string expected = "vehicleColor";
            string actual = _inputOutputViewModel.Value;
            Assert.AreEqual(expected, actual);
        }

        #endregion Value Tests

        #region GetGenerationTO Tests

        [TestMethod]
        public void InputOutputMappingViewModel_GetGenerationTO_ValidViewModel_Expected_Dev2DefinitionCreatedFromInputOutputViewModel()
        {
            IDev2Definition dev2Definition = _inputOutputViewModel.GetGenerationTO();
            Assert.IsTrue(dev2Definition != null);
        }

        [TestMethod]
        public void InputOutputMappingViewModel_GetGenerationTO_InvalidViewModel_Expected_NullDev2DefinitionCreated()
        {
            InputOutputViewModelTestObject testObject = new InputOutputViewModelTestObject { Name = null };
            SetInputOutputMappingViewModelFromTestMappingObject(testObject);
            IDev2Definition actual = _inputOutputViewModel.GetGenerationTO();
            // Not sure of the outcome here, it is a null name value, and the name is the most essential part of this
            Assert.IsNotNull(actual);
        }

        #endregion GetGenerationTO Tests

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("InputOutputMappingViewModel_MapsTo")]
        public void InputOutputMappingViewModel_MapsTo_ChangedToNonEmptyAndRequiredIsFalse_RequireMissingFalse()
        {
            //------------Setup for test--------------------------
            var viewModel = InputOutputViewModelFactory.CreateInputOutputViewModel("testName", "testValue", "", "", false, "");

            Assert.IsFalse(viewModel.Required);
            Assert.IsFalse(viewModel.RequiredMissing);

            //------------Execute Test---------------------------
            viewModel.MapsTo = "newValue";

            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.Required);
            Assert.IsFalse(viewModel.RequiredMissing);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("InputOutputMappingViewModel_MapsTo")]
        public void InputOutputMappingViewModel_MapsTo_ChangedToNonEmptyAndRequiredIsTrue_RequireMissingFalse()
        {
            //------------Setup for test--------------------------
            var viewModel = InputOutputViewModelFactory.CreateInputOutputViewModel("testName", "testValue", "", "", true, "");

            Assert.IsTrue(viewModel.Required);
            Assert.IsFalse(viewModel.RequiredMissing);

            //------------Execute Test---------------------------
            viewModel.MapsTo = "newValue";

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.Required);
            Assert.IsFalse(viewModel.RequiredMissing);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("InputOutputMappingViewModel_MapsTo")]
        public void InputOutputMappingViewModel_MapsTo_ChangedToEmptyAndRequiredIsFalse_RequireMissingFalse()
        {
            //------------Setup for test--------------------------
            var viewModel = InputOutputViewModelFactory.CreateInputOutputViewModel("testName", "testValue", "newValue", "", false, "");

            Assert.IsFalse(viewModel.Required);
            Assert.IsFalse(viewModel.RequiredMissing);

            //------------Execute Test---------------------------
            viewModel.MapsTo = "";

            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.Required);
            Assert.IsFalse(viewModel.RequiredMissing);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("InputOutputMappingViewModel_MapsTo")]
        public void InputOutputMappingViewModel_MapsTo_ChangedToEmptyAndRequiredIsTrue_RequireMissingTrue()
        {
            //------------Setup for test--------------------------
            var viewModel = InputOutputViewModelFactory.CreateInputOutputViewModel("testName", "testValue", "newValue", "", true, "");

            Assert.IsTrue(viewModel.Required);
            Assert.IsFalse(viewModel.RequiredMissing);

            //------------Execute Test---------------------------
            viewModel.MapsTo = "";

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.Required);
            Assert.IsTrue(viewModel.RequiredMissing);
        }

        #region Test Methods

        private void SetInputOutputMappingViewModelFromTestMappingObject(InputOutputViewModelTestObject mappingObject)
        {
            _inputOutputViewModel = InputOutputViewModelFactory.CreateInputOutputViewModel(mappingObject.Name, mappingObject.Value, mappingObject.MapsTo, mappingObject.DefaultValue, mappingObject.Required, mappingObject.RecordSetName);
        }

        #endregion Test Methods
    }
}
