using System.Diagnostics;
using System.IO;
using Dev2;
using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Decision;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB;
using Dev2.Tests.Activities;
using Microsoft.VisualBasic.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Framework;
using Dev2.Runtime.ESB.Execution;

// ReSharper disable CheckNamespace
namespace ActivityUnitTests
// ReSharper restore CheckNamespace
{
    [TestClass]
    public class BaseActivityUnitTest
    {

        public IEsbWorkspaceChannel DsfChannel;
        public IFrameworkSecurityContext SecurityContext;
        public Uri DsfAdddress = new Uri("http://localhost:77/dsf");
        public Mock<IEsbWorkspaceChannel> MockChannel;
        public IDataListCompiler Compiler;

        

        public BaseActivityUnitTest()
        {
            CallBackData = "Default Data";
            TestStartNode = new FlowStep
            {
                Action = new DsfCommentActivity()
            };
        }

        public Guid ExecutionID { get; set; }

        public dynamic TestData { get; set; }

        public dynamic CurrentDl { get; set; }

        public FlowStep TestStartNode { get; set; }

        public string CallBackData { get; set; }

        public DynamicActivity FlowchartProcess
        {
            get
            {
                var activity = new DynamicActivity { Implementation = () => FlowchartActivityBuilder.Implementation };
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
                var builder = new ActivityBuilder
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
                         new Variable<Util>{ Name = "t"},
                         new Variable<Dev2DataListDecisionHandler>{Name = "Dev2DecisionHandler"}

                        }
                        ,
                        StartNode = TestStartNode
                    }
                };


                var vbs = new VisualBasicSettings
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

        public dynamic ExecuteProcess(DsfDataObject dataObject = null)
        {

            var svc = new ServiceAction { Name = "TestAction", ServiceName = "UnitTestService" };
            svc.SetActivity(FlowchartProcess);
            Mock<IEsbChannel> mockChannel = new Mock<IEsbChannel>();

            if (CurrentDl == null)
            {
                CurrentDl = TestData;
            }

            var errors = new ErrorResultTO();
            if (ExecutionID == Guid.Empty)
            {
                Compiler = DataListFactory.CreateDataListCompiler();

                ExecutionID = Compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), TestData, CurrentDl, out errors);
                if(dataObject != null)
                {
                    dataObject.DataListID = ExecutionID;
                }
                
            }

            if (errors.HasErrors())
            {
                string errorString = errors.FetchErrors().Aggregate(string.Empty, (current, item) => current + item);

                throw new Exception(errorString);
            }

            if (dataObject == null)
            {

                dataObject = new DsfDataObject(CurrentDl, ExecutionID)
                {
                    // NOTE: WorkflowApplicationFactory.InvokeWorkflowImpl() will use HostSecurityProvider.Instance.ServerID 
                    //       if this is NOT provided which will cause the tests to fail!
                    ServerID = Guid.NewGuid()
                };
            }

            WfExecutionContainer wfec = new WfExecutionContainer(svc, dataObject, Dev2.Workspaces.WorkspaceRepository.Instance.ServerWorkspace, mockChannel.Object);

            errors.ClearErrors();
            dataObject.DataListID = wfec.Execute(out errors);

            return dataObject;
        }


        #region ForEach Execution

        /// <summary>
        /// The ForEach Activity requires the data returned from an activity
        /// We will mock the DSF channel to return something that we expect is shaped.
        /// </summary>
        /// <returns></returns>
        public Mock<IEsbChannel> ExecuteForEachProcess(out IDSFDataObject dataObject)
        {
            var svc = new ServiceAction { Name = "ForEachTestAction", ServiceName = "UnitTestService" };
            var mockChannel = new Mock<IEsbChannel>();
            svc.SetActivity(FlowchartProcess);

            if (CurrentDl == null)
            {
                CurrentDl = TestData;
            }

            Compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid exID = Compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), TestData, CurrentDl, out errors);
            if (errors.HasErrors())
            {
                string errorString = errors.FetchErrors().Aggregate(string.Empty, (current, item) => current + item);

                throw new Exception(errorString);
            }

            dataObject = new DsfDataObject(CurrentDl, exID)
            {
                // NOTE: WorkflowApplicationFactory.InvokeWorkflowImpl() will use HostSecurityProvider.Instance.ServerID 
                //       if this is NOT provided which will cause the tests to fail!
                ServerID = Guid.NewGuid()
            };


            mockChannel.Setup(c=>c.ExecuteTransactionallyScopedRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), out errors)).Verifiable();

            WfExecutionContainer wfec = new WfExecutionContainer(svc, dataObject, Dev2.Workspaces.WorkspaceRepository.Instance.ServerWorkspace, mockChannel.Object);

            errors.ClearErrors();
            dataObject.DataListID = wfec.Execute(out errors);

            return mockChannel;
        }


        #endregion ForEach Execution

        #region Activity Debug Input/Output Test Methods

        public void CheckActivityDebugInputOutput<T>(DsfNativeActivity<T> activity, string dataListShape,
                                                  string dataListWithData, out IList<IDebugItem> inputResults, out IList<IDebugItem> outputResults)
        {
            ErrorResultTO errors;
            TestStartNode = new FlowStep
            {
                Action = activity
            };

            TestData = dataListWithData;
            CurrentDl = dataListShape;

            Compiler = DataListFactory.CreateDataListCompiler();
            ExecutionID = Compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), TestData, CurrentDl, out errors);
            IBinaryDataList dl = Compiler.FetchBinaryDataList(ExecutionID, out errors);
            inputResults = activity.GetDebugInputs(dl);
            ExecuteProcess();
            outputResults = activity.GetDebugOutputs(dl);
        }

        public void CheckPathOperationActivityDebugInputOutput<T>(DsfNativeActivity<T> activity, string dataListShape,
                                                  string dataListWithData, out IList<IDebugItem> inputResults, out IList<IDebugItem> outputResults)
        {
            ErrorResultTO errors;
            TestStartNode = new FlowStep
            {
                Action = activity
            };

            TestData = dataListWithData;
            CurrentDl = dataListShape;

            Compiler = DataListFactory.CreateDataListCompiler();
            ExecutionID = Compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), TestData, CurrentDl, out errors);
            IBinaryDataList dl = Compiler.FetchBinaryDataList(ExecutionID, out errors);
            inputResults = activity.GetDebugInputs(dl);
            outputResults = activity.GetDebugOutputs(dl);
        }
        #endregion

        #region Retrieve DataList Values

        public bool GetScalarValueFromDataList(Guid dataListId, string fieldToRetrieve, out string result, out string error)
        {
            ErrorResultTO errorResult;
            IBinaryDataListEntry entry;
            bool fine = Compiler.FetchBinaryDataList(dataListId, out errorResult).TryGetEntry(fieldToRetrieve, out entry, out error);
            if (entry != null && fine)
            {
                IBinaryDataListItem item = entry.FetchScalar();
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
            IList<IBinaryDataListItem> dLItems = new List<IBinaryDataListItem>();
            ErrorResultTO errorResult;
            IBinaryDataListEntry entry;
            bool isCool = true;
            IBinaryDataList bdl = Compiler.FetchBinaryDataList(dataListId, out errorResult);
            bdl.TryGetEntry(recordSet, out entry, out error);

            IIndexIterator idxItr = entry.FetchRecordsetIndexes();
            while (idxItr.HasMore())
            {
                var fetchNextIndex = idxItr.FetchNextIndex();
                dLItems.Add(entry.TryFetchRecordsetColumnAtIndex(fieldNameToRetrieve, fetchNextIndex, out error).Clone());

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
            var retVals = new List<string>();
            IList<IBinaryDataListItem> dataListItems;
            if (GetRecordSetFieldValueFromDataList(dataListId, recordSetName, fieldToRetrieve, out dataListItems, out error))
            {
                retVals.AddRange(dataListItems.Select(item => item.TheValue));
            }
            return retVals;
        }

        #endregion Retrieve DataList Values
    }
}
