using Dev2.Composition;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Session;
using Dev2.Studio.Core;
using Dev2.Studio.Core.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Dev2.Core.Tests {
    /// <summary>
    ///This is a result class for WorkflowInputDataViewModelTest and is intended
    ///to contain all WorkflowInputDataViewModelTest Unit Tests
    ///</summary>
    [TestClass()]
    public class WorkflowInputDataViewModelTest {
        DebugTO debugTO = new DebugTO();

        /// <summary>
        /// We are exporting the MEF IoC container so that we can inject dependencies into other classes
        /// </summary>

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
        public void EnvironmentTestsInitialize() 
        {
            ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();

            debugTO.ServiceName="TestWorkflow";
            debugTO.WorkflowID="TestWorkflow";
            debugTO.DataList=StringResourcesTest.DebugInputWindow_DataList;
            debugTO.XmlData=StringResourcesTest.DebugInputWindow_XMLData;
            debugTO.IsDebugMode=true;
            debugTO.WorkflowXaml=StringResourcesTest.DebugInputWindow_WorkflowXaml;
            debugTO.RememberInputs=false;            
            
            //_worfflowInputDataviewModel = new WorkflowInputDataViewModel(_mockDebug.Object);
        }
        //
        //Use TestCleanup to run code after each result has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        #region LoadInputs Tests      
        // Sashen.Naidoo: 9144 + 9147 : Fixed tests and updated code to cater for issues with Debugging
        //                              When debug data hydrated the debug window, the display fields of record sets were incorrect.
        [TestMethod]
        public void LoadInputs_Expected_Inputs_Loaded()
        {
            //Juries TODO
            //_workflowInputDataviewModel = new WorkflowInputDataViewModel(debugTO);
            //_workflowInputDataviewModel.LoadWorkflowInputs();
            //IList<IDataListItem> testDataListItems = GetInputTestDataDataNames();
            //for (int i = 1; i < _workflowInputDataviewModel.WorkflowInputs.Count; i++)
            //{
            //    Assert.AreEqual(testDataListItems[i].DisplayValue, _workflowInputDataviewModel.WorkflowInputs[i].DisplayValue);
            //    Assert.AreEqual(testDataListItems[i].Value, _workflowInputDataviewModel.WorkflowInputs[i].Value);
            //}
        }

        [TestMethod]
        public void LoadInputs_BlankXMLData_Expected_Blank_Inputs()
        {
            //Juries TODO
            //debugTO.XmlData = string.Empty;
            //_workflowInputDataviewModel = new WorkflowInputDataViewModel(debugTO);
            //_workflowInputDataviewModel.LoadWorkflowInputs();
            //foreach (var input in _workflowInputDataviewModel.WorkflowInputs)
            //{
            //    Assert.AreEqual(string.Empty, input.Value);
            //}
            //Assert.IsTrue(_workflowInputDataviewModel.WorkflowInputs.Count == 4);
            
        }

        [TestMethod]
        public void LoadInputs_BlankDataList_Expected_Blank_Inputs()
        {
            //Juries TODO
            //debugTO.DataList = "<DataList></DataList>";
            //debugTO.XmlData = string.Empty;
            //_workflowInputDataviewModel = new WorkflowInputDataViewModel(debugTO);
            //_workflowInputDataviewModel.LoadWorkflowInputs();
            //Assert.IsTrue(_workflowInputDataviewModel.WorkflowInputs.Count == 0);
        }


        //2013.01.22: Ashley Lewis - Bug 7837 
        [TestMethod]
        public void Save_NullDataList_Expected_NoErrors()
        {
            //Juries TODO
            //debugTO.DataList = null;
            //debugTO.XmlData = string.Empty;
            //_workflowInputDataviewModel = new WorkflowInputDataViewModel(debugTO);
            //_workflowInputDataviewModel.LoadWorkflowInputs();
            //_workflowInputDataviewModel.Save();
            //Assert.AreEqual("", _workflowInputDataviewModel.DebugTO.Error);
        }
        [TestMethod]
        public void Cancel_NullDataList_Expected_NoErrors()
        {
            //Juries TODO
            //debugTO.DataList = null;
            //debugTO.XmlData = string.Empty;
            //_workflowInputDataviewModel = new WorkflowInputDataViewModel(debugTO);
            //_workflowInputDataviewModel.LoadWorkflowInputs();
            //_workflowInputDataviewModel.Cancel();
            //Assert.AreEqual("", _workflowInputDataviewModel.DebugTO.Error);
        }
        [TestMethod]
        public void LoadInputs_NullDataList_Expected_Blank_Inputs()
        {
            //Juries TODO
            //debugTO.DataList = null;
            //debugTO.XmlData = string.Empty;
            //_workflowInputDataviewModel = new WorkflowInputDataViewModel(debugTO);
            //_workflowInputDataviewModel.LoadWorkflowInputs();
            //Assert.IsTrue(_workflowInputDataviewModel.WorkflowInputs.Count == 0);
        }
        #endregion LoadInputs Tests

        #region SetXML


        #endregion SetXML

        #region SetWorkflowInputData

        [TestMethod]
        public void SetWorkflowInputData_ExtraRows_Expected_Row_Available()
        {
            //Juries TODO
            //debugTO.XmlData = GetTestXMLData();
            //_workflowInputDataviewModel = new WorkflowInputDataViewModel(debugTO);
            //_workflowInputDataviewModel.LoadWorkflowInputs();
            //OptomizedObservableCollection<IDataListItem> inputValues = GetInputTestDataDataNames();

            //// Cannot perform Collection Assert due to use of mocks for datalist items to remove dependancies during test
            //for (int i = 0; i < _workflowInputDataviewModel.WorkflowInputs.Count; i++)
            //{
            //    Assert.AreEqual(inputValues[i].DisplayValue, _workflowInputDataviewModel.WorkflowInputs[i].DisplayValue);
            //    Assert.AreEqual(inputValues[i].Value, _workflowInputDataviewModel.WorkflowInputs[i].Value);
            //    Assert.AreEqual(inputValues[i].IsRecordset, _workflowInputDataviewModel.WorkflowInputs[i].IsRecordset);
            //    Assert.AreEqual(inputValues[i].RecordsetIndex, _workflowInputDataviewModel.WorkflowInputs[i].RecordsetIndex);
            //    Assert.AreEqual(inputValues[i].Field, _workflowInputDataviewModel.WorkflowInputs[i].Field);
            //}
            
        }

        

        #endregion SetWorkflowInputData

        #region Private Methods

        private string GetTestXMLData()
        {
            return @"<DataList>
  <scalar1>ScalarData1</scalar1>
  <scalar2>ScalarData2</scalar2>
  <Recset>
    <Field1>Field1Data1</Field1>
    <Field2>Field2Data1</Field2>
  </Recset>
  <Recset>
    <Field1>Field1Data2</Field1>
    <Field2>Field2Data2</Field2>
  </Recset>
  <Recset>
    <Field1>Field1Data3</Field1>
    <Field2>Field2Data3</Field2>
  </Recset>
  <Recset>
    <Field1>Field1Data4</Field1>
    <Field2>Field2Data4</Field2>
  </Recset>
<Recset>
    <Field1>Field1Data5</Field1>
    <Field2>Field2Data5</Field2>
  </Recset>
<Recset>
    <Field1>Field1Data6</Field1>
    <Field2>Field2Data6</Field2>
  </Recset>
</DataList>";
        }

        private OptomizedObservableCollection<IDataListItem> GetInputTestDataDataNames()
        {
            IDataListItem dataListItem = new DataListItem();
            int numberOfRecords = 6;
            int numberOfRecordFields = 2;
            OptomizedObservableCollection<IDataListItem> items = new OptomizedObservableCollection<IDataListItem>();
            items.AddRange(GetDataListItemScalar());
            items.AddRange(CreateTestDataListItemRecords(numberOfRecords, numberOfRecordFields));

            return items;


        }

        private IList<IDataListItem> GetDataListItemScalar()
        {
            IList<IDataListItem> scalars = new OptomizedObservableCollection<IDataListItem> 
                                                                            {  CreateScalar("scalar1", "ScalarData1")
                                                                             , CreateScalar("scalar2", "ScalarData2")
                                                                            };
            return scalars;

        }

        private IList<IDataListItem> CreateTestDataListItemRecords(int numberOfRecords, int recordFieldCount)
        {           
            IList<IDataListItem> recordSets = new List<IDataListItem>();          
            for (int i = 1; i <= numberOfRecords; i++)
            {
                for (int j = 1; j <= recordFieldCount; j++)
                {
                    recordSets.Add(CreateRecord("Recset", "Field" + (j).ToString(), "Field" + (j).ToString() + "Data" + (i).ToString(), i));
                }
            }

            return recordSets;

        }

        private IDataListItem CreateScalar(string scalarName, string scalarValue)
        {
            Mock<IDataListItem> item = new Mock<IDataListItem>();
            item.Setup(itemName => itemName.DisplayValue).Returns(scalarName);
            item.Setup(itemName => itemName.Field).Returns(scalarName);
            item.Setup(itemName => itemName.RecordsetIndexType).Returns(enRecordsetIndexType.Numeric);
            item.Setup(itemName => itemName.Value).Returns(scalarValue);

            return item.Object;
        }

        private IDataListItem CreateRecord(string recordSetName, string recordSetField, string recordSetValue, int recordSetIndex)
        {
            Mock<IDataListItem> records = new Mock<IDataListItem>();
            records.Setup(rec => rec.DisplayValue).Returns(recordSetName + "(" + recordSetIndex + ")." + recordSetField);
            records.Setup(rec => rec.Field).Returns(recordSetField);
            records.Setup(rec => rec.RecordsetIndex).Returns(Convert.ToString(recordSetIndex));
            records.Setup(rec => rec.RecordsetIndexType).Returns(enRecordsetIndexType.Numeric);
            records.Setup(rec => rec.Recordset).Returns(recordSetName);
            records.Setup(rec => rec.Value).Returns(recordSetValue);
            records.Setup(rec => rec.IsRecordset).Returns(true);

            return records.Object;
        }


        #endregion Private Methods

    }
}
