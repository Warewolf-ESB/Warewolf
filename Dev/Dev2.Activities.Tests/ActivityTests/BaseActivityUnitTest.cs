using Dev2;
using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.Runtime.InterfaceImplementors;
using Dev2.Tests.Activities;
using Microsoft.VisualBasic.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Framework;

namespace ActivityUnitTests
{
    [TestClass]
    public class BaseActivityUnitTest
    {

        public IFrameworkWorkspaceChannel _dsfChannel;
        public IFrameworkSecurityContext _securityContext;
        public Uri _dsfAdddress = new Uri("http://localhost:77/dsf");
        private dynamic _testData;
        private Guid _executionId;
        private dynamic _currentDL;
        private string _callBackData = "Default Data";
        public Mock<IFrameworkWorkspaceChannel> _mockChannel;
        public IDataListCompiler _compiler;

        public BaseActivityUnitTest() { }

        FlowStep _testStartNode = new FlowStep
        {
            Action = new DsfCommentActivity()
        };

        public Guid ExecutionID { get { return _executionId; } set { _executionId = value; } }

        public dynamic TestData
        {

            get
            {
                return _testData;
            }
            set
            {
                _testData = value;
            }
        }

        public dynamic CurrentDL
        {

            get
            {
                return _currentDL;
            }
            set
            {
                _currentDL = value;
            }
        }

        public FlowStep TestStartNode
        {
            get
            {
                return _testStartNode;
            }
            set
            {
                _testStartNode = value;
            }
        }

        public string CallBackData
        {
            set
            {
                _callBackData = value;
            }
            get
            {
                return _callBackData;
            }
        }

        public DynamicActivity FlowchartProcess
        {
            get
            {
                var activity = new DynamicActivity();
                activity.Implementation = () => FlowchartActivityBuilder.Implementation;
                foreach (DynamicActivityProperty prop in FlowchartActivityBuilder.Properties)
                {
                    activity.Properties.Add(prop);
                }

                return activity;
            }
        }

        public ActivityBuilder FlowchartActivityBuilder
        {
            get
            {
                var builder = new ActivityBuilder()
                {
                    //Name = _workflowModel.ResourceName,
                    //Name = _workflowModel.ResourceName, //string.Format("{0}.{1}.{2}.{3}", _workflowModel.Module, _workflowModel.SubModule, _workflowModel.Action, _workflowModel.ResourceName),
                    Properties = {
                    new DynamicActivityProperty{Name = "AmbientDataList",Type = typeof(InOutArgument<List<string>>)}
                    ,new DynamicActivityProperty{ Name = "ParentWorkflowInstanceId", Type = typeof(InOutArgument<Guid>)}
                    ,new DynamicActivityProperty{ Name = "ParentServiceName", Type = typeof(InOutArgument<string>)}
                },
                    Implementation = new Flowchart
                    {
                        //DisplayName = _workflowModel.ResourceName,//string.Format("{0}.{1}.{2}.{3}",_workflowModel.Module, _workflowModel.SubModule, _workflowModel.Action, _workflowModel.ResourceName),
                        Variables = {
                         new Variable<List<string>>{Name = "InstructionList"},
                         new Variable<string>{Name = "LastResult"},
                         new Variable<bool>{Name = "HasError"},
                         new Variable<string>{Name = "ExplicitDataList"},
                         new Variable<bool>{Name = "IsValid"},
                         new Variable<UnlimitedObject>{Name = "d"},
                         new Variable<Util>{ Name = "t"}

                        }
                        ,
                        StartNode = TestStartNode
                    }
                };


                VisualBasicSettings vbs = new VisualBasicSettings
                {
                    ImportReferences =     {
                        new VisualBasicImportReference {
                            Assembly = "Unlimited.Framework",
                            Import = "Unlimited.Framework"
                        },
                        new VisualBasicImportReference{
                             Assembly = "Unlimited.Applications.BusinessDesignStudio.Activities",
                             Import = "Unlimited.Applications.BusinessDesignStudio.Activities"

                        }
                    }
                };


                VisualBasic.SetSettings(builder, vbs);

                return builder;
            }

        }

        public dynamic ExecuteProcess()
        {

            ServiceAction svc = new ServiceAction { Name = "testAction", ServiceName = "UnitTestService" };

            svc.SetActivity(FlowchartProcess);

            Mock<IFrameworkDataChannel> mockChannel = new Mock<IFrameworkDataChannel>();

            mockChannel.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), GlobalConstants.NullDataListID)).Callback((string xml, Guid dlId, Guid id) => ExecuteCallBack(xml)).Returns(() => result);

            var invoker = new DynamicServicesInvoker(mockChannel.Object, null, false, Dev2.Workspaces.WorkspaceRepository.Instance.ServerWorkspace);

            if (CurrentDL == null)
            {
                CurrentDL = TestData;
            }

            ErrorResultTO errors = new ErrorResultTO();
            if (ExecutionID == Guid.Empty)
            {
                _compiler = DataListFactory.CreateDataListCompiler();
                ExecutionID = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), TestData, CurrentDL, out errors);
            }
            if (errors.HasErrors())
            {
                string errorString = string.Empty;
                foreach (string item in errors.FetchErrors())
                {
                    errorString += item;
                }

                throw new Exception(errorString);
            }

            DsfDataObject dataObject = new DsfDataObject(CurrentDL, ExecutionID)
            {
                // NOTE: WorkflowApplicationFactory.InvokeWorkflowImpl() will use HostSecurityProvider.Instance.ServerID 
                //       if this is NOT provided which will cause the tests to fail!
                ServerID = Guid.NewGuid()
            };
            invoker.WorkflowApplication(svc, dataObject, TestData);
            return dataObject;
        }


        #region ForEach Execution

        /// <summary>
        /// The ForEach Activity requires the data returned from an activity
        /// We will mock the DSF channel to return something that we expect is shaped.
        /// </summary>
        /// <returns></returns>
        public dynamic ExecuteForEachProcess()
        {
            try
            {
                ServiceAction svc = new ServiceAction { Name = "testAction", ServiceName = "UnitTestService" };

                svc.SetActivity(FlowchartProcess);




                if (CurrentDL == null)
                {
                    CurrentDL = TestData;
                }
                //UnlimitedObject.GetStringXmlDataAsUnlimitedObject(CurrentDL)
                _compiler = DataListFactory.CreateDataListCompiler();
                ErrorResultTO errors = new ErrorResultTO();
                Guid exID = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), TestData, CurrentDL, out errors);
                if (errors.HasErrors())
                {
                    string errorString = string.Empty;
                    foreach (string item in errors.FetchErrors())
                    {
                        errorString += item;
                    }

                    throw new Exception(errorString);
                }

                DsfDataObject dataObject = new DsfDataObject(CurrentDL, exID)
                {
                    // NOTE: WorkflowApplicationFactory.InvokeWorkflowImpl() will use HostSecurityProvider.Instance.ServerID 
                    //       if this is NOT provided which will cause the tests to fail!
                    ServerID = Guid.NewGuid() 
                };
                _mockChannel = new Mock<IFrameworkWorkspaceChannel>();
                _mockChannel.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                    .Returns((string svcName, Guid wsId, Guid executionID) => { return executionID.ToString(); }).Verifiable();

                var invoker = new DynamicServicesInvoker(_mockChannel.Object, null, false, Dev2.Workspaces.WorkspaceRepository.Instance.ServerWorkspace);

                invoker.WorkflowApplication(svc, dataObject, TestData);
                return dataObject;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion ForEach Execution


        // return for the callback
        string result = "<error>nop</error>";

        /*
         * After much fing around it appears the mock is staticly evaluated and the result cannot be changed for a callback return value
         * 
         */
        public void ExecuteCallBack(string xml)
        {
            UnlimitedObject serviceRequest = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(xml);

            string partName = serviceRequest.GetValue("WebPartServiceName");

            Console.WriteLine("Part : " + partName);

            if (partName == "HtmlWidget")
            {
                string payload = (new UnlimitedObject(XElement.Parse(xml)).XPath("//Value/node()").Inner());

                result = ActivityStrings.webpartTemplate.Replace("!REPLACE!", CallBackData);
            }

        }

        private void ExecuteForEachInnerActivityCallback(Guid dataListID)
        {

        }

        #region Activity Debug Input/Output Test Methods

        public void CheckActivityDebugInputOutput<T>(DsfNativeActivity<T> activity, string dataListShape,
                                                  string dataListWithData, out IList<IDebugItem> inputResults, out IList<IDebugItem> outputResults)
        {
            ErrorResultTO errors = new ErrorResultTO();
            TestStartNode = new FlowStep
            {
                Action = activity
            };

            TestData = dataListWithData;
            CurrentDL = dataListShape;

            _compiler = DataListFactory.CreateDataListCompiler();
            ExecutionID = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), TestData, CurrentDL, out errors);
            IBinaryDataList dl = _compiler.FetchBinaryDataList(ExecutionID, out errors);
            inputResults = activity.GetDebugInputs(dl);
            ExecuteProcess();
            outputResults = activity.GetDebugOutputs(dl);
        }

        public void CheckPathOperationActivityDebugInputOutput<T>(DsfNativeActivity<T> activity, string dataListShape,
                                                  string dataListWithData, out IList<IDebugItem> inputResults, out IList<IDebugItem> outputResults)
        {
            ErrorResultTO errors = new ErrorResultTO();
            TestStartNode = new FlowStep
            {
                Action = activity
            };

            TestData = dataListWithData;
            CurrentDL = dataListShape;

            _compiler = DataListFactory.CreateDataListCompiler();
            ExecutionID = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), TestData, CurrentDL, out errors);
            IBinaryDataList dl = _compiler.FetchBinaryDataList(ExecutionID, out errors);
            inputResults = activity.GetDebugInputs(dl);
            outputResults = activity.GetDebugOutputs(dl);
        }
        #endregion

        #region Retrieve DataList Values

        public bool GetScalarValueFromDataList(Guid dataListId, string fieldToRetrieve, out string result, out string error)
        {
            ErrorResultTO errorResult = new ErrorResultTO();
            IBinaryDataListEntry entry = null;
            bool fine = _compiler.FetchBinaryDataList(dataListId, out errorResult).TryGetEntry(fieldToRetrieve, out entry, out error);
            IBinaryDataListItem item = null;
            if (entry != null && fine)
            {
                item = entry.FetchScalar();
                result = item.TheValue;
            }
            else
            {
                result = string.Empty;
            }

            return true;
        }

        public bool GetRecordSetFieldValueFromDataList(Guid dataListId, string recordSet, string fieldNameToRetrieve, out IList<IBinaryDataListItem> result, out string error)
        {
            string results = string.Empty;
            IList<IBinaryDataListItem> dLItems = new List<IBinaryDataListItem>();
            ErrorResultTO errorResult = new ErrorResultTO();
            IBinaryDataListEntry entry = null;
            bool isCool = true;
            IBinaryDataList bdl = _compiler.FetchBinaryDataList(dataListId, out errorResult);
            bool fine = bdl.TryGetEntry(recordSet, out entry, out error);

            IIndexIterator idxItr = entry.FetchRecordsetIndexes();
            while (idxItr.HasMore())
            {
                dLItems.Add(entry.TryFetchRecordsetColumnAtIndex(fieldNameToRetrieve, idxItr.FetchNextIndex(), out error));
            }

            //foreach(int recordSetEntry in entry.FetchRecordsetIndexes()) {
            //    dLItems.Add(entry.TryFetchRecordsetColumnAtIndex(fieldNameToRetrieve, recordSetEntry, out error));
            //}
            result = dLItems;
            if (!string.IsNullOrEmpty(error))
            {
                isCool = false;
            }

            return isCool;
        }

        protected List<string> RetrieveAllRecordSetFieldValues(Guid dataListId, string recordSetName, string fieldToRetrieve, out string error)
        {
            List<string> retVals = new List<string>();
            IList<IBinaryDataListItem> dataListItems = new List<IBinaryDataListItem>();
            if (GetRecordSetFieldValueFromDataList(dataListId, recordSetName, fieldToRetrieve, out dataListItems, out error))
            {
                foreach (IBinaryDataListItem item in dataListItems)
                {
                    retVals.Add(item.TheValue);
                }
            }
            return retVals;
        }


        #endregion Retrieve DataList Values
    }
}
