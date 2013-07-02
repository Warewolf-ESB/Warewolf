using Caliburn.Micro;
using Dev2.Composition;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Session;
using Dev2.Studio.AppResources.Messages;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.Workflow;
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

      //  Mock<IContextualResourceModel> _mockResource = new Mock<IContextualResourceModel>();
        private Guid _resourceID = Guid.Parse("2b975c6d-670e-49bb-ac4d-fb1ce578f66a");
        private Guid _serverID = Guid.Parse("51a58300-7e9d-4927-a57b-e5d700b11b55");
        private string _resourceName = "TestWorkflow";

        /// <summary>
        /// We are exporting the MEF IoC container so that we can inject dependencies into other classes
        /// </summary>

        private TestContext testContextInstance;

        private WorkflowInputDataViewModel _workflowInputDataviewModel;

        private Mock<IEventAggregator> _eventAggregator;

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

        [TestInitialize()]
        public void EnvironmentTestsInitialize() 
        {
            _eventAggregator = new Mock<IEventAggregator>();
            ImportService.CurrentContext = CompositionInitializer.InializeWithEventAggregator(_eventAggregator.Object);
        }
        #endregion

        #region LoadInputs Tests    
  

       [TestMethod]
        public void LoadInputs_Expected_Inputs_Loaded()
       {
           var mockResouce = GetMockResource();
           var _serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
           _serviceDebugInfo.SetupGet(s => s.ServiceInputData).Returns(StringResourcesTest.DebugInputWindow_XMLData);
            _workflowInputDataviewModel = new WorkflowInputDataViewModel(_serviceDebugInfo.Object);
            _workflowInputDataviewModel.LoadWorkflowInputs();
            IList<IDataListItem> testDataListItems = GetInputTestDataDataNames();
            for (int i = 1; i < _workflowInputDataviewModel.WorkflowInputs.Count; i++)
            {
                Assert.AreEqual(testDataListItems[i].DisplayValue, _workflowInputDataviewModel.WorkflowInputs[i].DisplayValue);
                Assert.AreEqual(testDataListItems[i].Value, _workflowInputDataviewModel.WorkflowInputs[i].Value);
            }
        }

        [TestMethod]
        public void LoadInputs_BlankXMLData_Expected_Blank_Inputs()
       {
           var mockResouce = GetMockResource();
           var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
           serviceDebugInfo.SetupGet(s => s.ServiceInputData).Returns("<DataList></DataList>");
            _workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object);
            _workflowInputDataviewModel.LoadWorkflowInputs();
            foreach (var input in _workflowInputDataviewModel.WorkflowInputs)
            {
                Assert.AreEqual(string.Empty, input.Value);
            }
            Assert.IsTrue(_workflowInputDataviewModel.WorkflowInputs.Count == 4);          
        }

        [TestMethod]
        public void LoadInputs_BlankDataList_Expected_Blank_Inputs()
        {
            var mockResouce = GetMockResource();
            mockResouce.SetupGet(s => s.DataList).Returns("<DataList></DataList>");
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            _workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object);
            _workflowInputDataviewModel.LoadWorkflowInputs();
            Assert.IsTrue(_workflowInputDataviewModel.WorkflowInputs.Count == 0);
        }


        //2013.01.22: Ashley Lewis - Bug 7837 
        [TestMethod]
        public void Save_EmptyDataList_Expected_NoErrors()
        {
            var mockResouce = GetMockResource();
            mockResouce.SetupGet(s => s.DataList).Returns(string.Empty);
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            _workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object);
            _workflowInputDataviewModel.LoadWorkflowInputs();
            _workflowInputDataviewModel.Save();
            Assert.AreEqual("", _workflowInputDataviewModel.DebugTO.Error);
        }

        [TestMethod]
        public void Cancel_NullDataList_Expected_NoErrors()
        {
            var mockResouce = GetMockResource();
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            _workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object);
            _workflowInputDataviewModel.LoadWorkflowInputs();
            _workflowInputDataviewModel.Cancel();
            Assert.AreEqual("", _workflowInputDataviewModel.DebugTO.Error);
        }

        [TestMethod]
        public void LoadInputs_NullDataList_Expected_Blank_Inputs()
        {
            var mockResouce = GetMockResource();
            mockResouce.SetupGet(s => s.DataList).Returns(string.Empty);
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            _workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object);
            _workflowInputDataviewModel.LoadWorkflowInputs();
            Assert.IsTrue(_workflowInputDataviewModel.WorkflowInputs.Count == 0);
        }
        #endregion LoadInputs Tests

        #region SetWorkflowInputData

        [TestMethod]
        public void SetWorkflowInputData_ExtraRows_Expected_Row_Available()
        {
            var mockResouce = GetMockResource();
            var _serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            _serviceDebugInfo.SetupGet(s => s.ServiceInputData).Returns(StringResourcesTest.DebugInputWindow_XMLData);
            _workflowInputDataviewModel = new WorkflowInputDataViewModel(_serviceDebugInfo.Object);
            _workflowInputDataviewModel.LoadWorkflowInputs();
            OptomizedObservableCollection<IDataListItem> inputValues = GetInputTestDataDataNames();

            // Cannot perform Collection Assert due to use of mocks for datalist items to remove dependancies during test
            for (int i = 0; i < _workflowInputDataviewModel.WorkflowInputs.Count; i++)
            {
                Assert.AreEqual(inputValues[i].DisplayValue, _workflowInputDataviewModel.WorkflowInputs[i].DisplayValue);
                Assert.AreEqual(inputValues[i].Value, _workflowInputDataviewModel.WorkflowInputs[i].Value);
                Assert.AreEqual(inputValues[i].IsRecordset, _workflowInputDataviewModel.WorkflowInputs[i].IsRecordset);
                Assert.AreEqual(inputValues[i].RecordsetIndex, _workflowInputDataviewModel.WorkflowInputs[i].RecordsetIndex);
                Assert.AreEqual(inputValues[i].Field, _workflowInputDataviewModel.WorkflowInputs[i].Field);
            }          
        }      

        #endregion SetWorkflowInputData

        #region CloseWorkflowTest
        [TestMethod]
        public void CloseInputExpectFinishMessage()
        {
           _eventAggregator.Setup(evt => evt.Publish(It.IsAny<SetDebugStatusMessage>()))
               .Callback<object>((o) =>
                   {
                       var msg = (SetDebugStatusMessage) o;
                       Assert.IsTrue(msg.DebugStatus == DebugStatus.Finished);
                       var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.Workflow,
                                                                            _resourceID, _serverID);
                       Assert.IsTrue(msg.WorkSurfaceKey.Equals(workSurfaceKey));
                   }).Verifiable();

           var mockResouce = GetMockResource();
           var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            _workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object);
            _workflowInputDataviewModel.ViewClosed();
            _eventAggregator.Verify(evt => evt.Publish(It.IsAny<SetDebugStatusMessage>()), Times.Once());
        }

        #endregion

        #region Private Methods

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

        private Mock<IContextualResourceModel> GetMockResource()
        {
            var mockResource = new Mock<IContextualResourceModel>();
            mockResource.SetupGet(r => r.ServerID).Returns(_serverID);
            mockResource.SetupGet(r => r.ResourceName).Returns(_resourceName);
            mockResource.SetupGet(r => r.WorkflowXaml).Returns(StringResourcesTest.DebugInputWindow_WorkflowXaml);
            mockResource.SetupGet(r => r.ID).Returns(_resourceID);
            mockResource.SetupGet(r => r.DataList).Returns(StringResourcesTest.DebugInputWindow_DataList);
            return mockResource;
        }

        private Mock<IServiceDebugInfoModel> GetMockServiceDebugInfo(Mock<IContextualResourceModel> mockResouce)
        {
            Mock<IServiceDebugInfoModel> _serviceDebugInfo = new Mock<IServiceDebugInfoModel>();
            _serviceDebugInfo.SetupGet(sd => sd.DebugModeSetting).Returns(DebugMode.DebugInteractive);
            _serviceDebugInfo.SetupGet(sd => sd.ResourceModel).Returns(mockResouce.Object);
            _serviceDebugInfo.SetupGet(sd => sd.RememberInputs).Returns(false);
            return _serviceDebugInfo;
        }

        #endregion Private Methods

    }
}
