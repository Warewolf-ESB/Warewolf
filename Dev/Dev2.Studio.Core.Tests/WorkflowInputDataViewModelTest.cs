using Dev2.Composition;
using Dev2.Session;
using Dev2.Studio.Core.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests {
    /// <summary>
    ///This is a result class for WorkflowInputDataViewModelTest and is intended
    ///to contain all WorkflowInputDataViewModelTest Unit Tests
    ///</summary>
    [TestClass()]
    public class WorkflowInputDataViewModelTest { 

        WorkflowInputDataViewModel _workflowInputDataviewModel;
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
        [TestMethod]
        public void LoadInputs_Expected_Inputs_Loaded()
        {
            _workflowInputDataviewModel = new WorkflowInputDataViewModel(debugTO);
            _workflowInputDataviewModel.LoadWorkflowInputs();
            Assert.IsTrue(_workflowInputDataviewModel.WorkflowInputs.Count == 10);
        }

        [TestMethod]
        public void LoadInputs_BlankXMLData_Expected_Blank_Inputs()
        {
            debugTO.XmlData = string.Empty;
            _workflowInputDataviewModel = new WorkflowInputDataViewModel(debugTO);
            _workflowInputDataviewModel.LoadWorkflowInputs();
            Assert.IsTrue(_workflowInputDataviewModel.WorkflowInputs.Count == 4);
        }

        [TestMethod]
        public void LoadInputs_BlankDataList_Expected_Blank_Inputs()
        {
            debugTO.DataList = "<DataList></DataList>";
            debugTO.XmlData = string.Empty;
            _workflowInputDataviewModel = new WorkflowInputDataViewModel(debugTO);
            _workflowInputDataviewModel.LoadWorkflowInputs();
            Assert.IsTrue(_workflowInputDataviewModel.WorkflowInputs.Count == 0);
        }


        //2013.01.22: Ashley Lewis - Bug 7837 
        [TestMethod]
        public void Save_NullDataList_Expected_NoErrors()
        {
            debugTO.DataList = null;
            debugTO.XmlData = string.Empty;
            _workflowInputDataviewModel = new WorkflowInputDataViewModel(debugTO);
            _workflowInputDataviewModel.LoadWorkflowInputs();
            _workflowInputDataviewModel.Save();
            Assert.AreEqual("", _workflowInputDataviewModel.DebugTO.Error);
        }
        [TestMethod]
        public void Cancel_NullDataList_Expected_NoErrors()
        {
            debugTO.DataList = null;
            debugTO.XmlData = string.Empty;
            _workflowInputDataviewModel = new WorkflowInputDataViewModel(debugTO);
            _workflowInputDataviewModel.LoadWorkflowInputs();
            _workflowInputDataviewModel.Cancel();
            Assert.AreEqual("", _workflowInputDataviewModel.DebugTO.Error);
        }
        [TestMethod]
        public void LoadInputs_NullDataList_Expected_Blank_Inputs()
        {
            debugTO.DataList = null;
            debugTO.XmlData = string.Empty;
            _workflowInputDataviewModel = new WorkflowInputDataViewModel(debugTO);
            _workflowInputDataviewModel.LoadWorkflowInputs();
            Assert.IsTrue(_workflowInputDataviewModel.WorkflowInputs.Count == 0);
        }
        #endregion LoadInputs Tests

        #region SetXML


        #endregion SetXML

        #region SetWorkflowInputData
        [TestMethod]
        public void SetWorkflowInputData_ExtraRows_Expected_Row_Available()
        {
            debugTO.XmlData = @"<DataList>
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
    <Field1>Field1Data5</Field1>
    <Field2>Field2Data5</Field2>
  </Recset>
</DataList>";
            _workflowInputDataviewModel = new WorkflowInputDataViewModel(debugTO);
            _workflowInputDataviewModel.LoadWorkflowInputs();
            Assert.IsTrue(_workflowInputDataviewModel.WorkflowInputs.Count == 14);
        }
        

        #endregion SetWorkflowInputData

    }
}
