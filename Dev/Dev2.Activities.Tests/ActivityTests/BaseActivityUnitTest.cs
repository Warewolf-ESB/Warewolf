
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Data.Decision;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Execution;
using Dev2.Workspaces;
using Microsoft.VisualBasic.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace ActivityUnitTests
// ReSharper restore CheckNamespace
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class BaseActivityUnitTest
    {

        public IEsbWorkspaceChannel DsfChannel;
        public Mock<IEsbWorkspaceChannel> MockChannel;
        public static IDataListCompiler Compiler;
        public BaseActivityUnitTest()
        {
            CustomContainer.Register<IActivityParser>(new ActivityParser());
            CallBackData = "Default Data";
            TestStartNode = new FlowStep
            {
                Action = new DsfCommentActivity()
            };
           DataObject = new DsfDataObject("",Guid.NewGuid());
        }

        public Guid ExecutionId { get; set; }

        public string TestData { get; set; }

        public string CurrentDl { get; set; }

        public FlowStep TestStartNode { get; set; }

        public IDSFDataObject DataObject { get; set; }

        public string CallBackData { get; set; }

        public DynamicActivity FlowchartProcess
        {
            get
            {
                var activity = new DynamicActivity { Implementation = () => FlowchartActivityBuilder.Implementation };
                foreach(DynamicActivityProperty prop in FlowchartActivityBuilder.Properties)
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
                    Properties = {
                    new DynamicActivityProperty{Name = "AmbientDataList",Type = typeof(InOutArgument<List<string>>)}
                    ,new DynamicActivityProperty{ Name = "ParentWorkflowInstanceId", Type = typeof(InOutArgument<Guid>)}
                    ,new DynamicActivityProperty{ Name = "ParentServiceName", Type = typeof(InOutArgument<string>)}
                },
                    Implementation = new Flowchart
                    {

                        Variables = {
                         new Variable<List<string>>{Name = "InstructionList"},
                         new Variable<string>{Name = "LastResult"},
                         new Variable<bool>{Name = "HasError"},
                         new Variable<string>{Name = "ExplicitDataList"},
                         new Variable<bool>{Name = "IsValid"},
                         //new Variable<UnlimitedObject>{Name = "d"},
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

        public IDSFDataObject ExecuteProcess(IDSFDataObject dataObject = null, bool isDebug = false, IEsbChannel channel = null, bool isRemoteInvoke = false, bool throwException = true, bool isDebugMode = false, Guid currentEnvironmentId = default(Guid), bool overrideRemote = false)
        {

            
                var svc = new ServiceAction { Name = "TestAction", ServiceName = "UnitTestService" };
                svc.SetActivity(FlowchartProcess);
                Mock<IEsbChannel> mockChannel = new Mock<IEsbChannel>();

                if(CurrentDl == null)
                {
                    CurrentDl = TestData;
                }

                var errors = new ErrorResultTO();
                if(ExecutionId == Guid.Empty)
                {

                    if(dataObject != null)
                    {
                        dataObject.ExecutingUser = User;
                        dataObject.DataList = new StringBuilder(CurrentDl);
                    }

                }

                if(errors.HasErrors())
                {
                    string errorString = errors.FetchErrors().Aggregate(string.Empty, (current, item) => current + item);

                    if(throwException)
                    {
                        throw new Exception(errorString);
                    }
                }

                if(dataObject == null)
                {

                    dataObject = new DsfDataObject(CurrentDl, ExecutionId)
                    {
                        // NOTE: WorkflowApplicationFactory.InvokeWorkflowImpl() will use HostSecurityProvider.Instance.ServerID 
                        //       if this is NOT provided which will cause the tests to fail!
                        ServerID = Guid.NewGuid(),
                        ExecutingUser = User,
                        IsDebug = isDebugMode,
                        EnvironmentID = currentEnvironmentId,
                        IsRemoteInvokeOverridden = overrideRemote,
                        DataList = new StringBuilder(CurrentDl)
                    };

                }
                ExecutionEnvironmentUtils.UpdateEnvironmentFromXmlPayload(DataObject, new StringBuilder(TestData), CurrentDl);
                dataObject.IsDebug = isDebug;

                // we now need to set a thread ID ;)
                dataObject.ParentThreadID = 1;

                if(isRemoteInvoke)
                {
                    dataObject.RemoteInvoke = true;
                    dataObject.RemoteInvokerID = Guid.NewGuid().ToString();
                }

                var esbChannel = mockChannel.Object;
                if(channel != null)
                {
                    esbChannel = channel;
                }
            dataObject.ExecutionToken = new ExecutionToken();
                WfExecutionContainer wfec = new WfExecutionContainer(svc, dataObject, WorkspaceRepository.Instance.ServerWorkspace, esbChannel);

                errors.ClearErrors();
            CustomContainer.Register<IActivityParser>(new ActivityParser());
            if(dataObject.ResourceID == Guid.Empty)
            {
                dataObject.ResourceID = Guid.NewGuid();
            }
            dataObject.Environment = DataObject.Environment;
            wfec.Eval(FlowchartProcess,dataObject.ResourceID,dataObject);
            DataObject = dataObject;
            return dataObject;
        }

        #region ForEach Execution

        /// <summary>
        /// The ForEach Activity requires the data returned from an activity
        /// We will mock the DSF channel to return something that we expect is shaped.
        /// </summary>
        /// <returns></returns>
        public Mock<IEsbChannel> ExecuteForEachProcess(out IDSFDataObject dataObject, bool isDebug = false, int nestingLevel = 0)
        {
            var svc = new ServiceAction { Name = "ForEachTestAction", ServiceName = "UnitTestService" };
            var mockChannel = new Mock<IEsbChannel>();
            svc.SetActivity(FlowchartProcess);

            if(CurrentDl == null)
            {
                CurrentDl = TestData;
            }

            Compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid exId = Compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), new StringBuilder(TestData), new StringBuilder(CurrentDl), out errors);


            if(errors.HasErrors())
            {
                string errorString = errors.FetchErrors().Aggregate(string.Empty, (current, item) => current + item);

                throw new Exception(errorString);
            }

            dataObject = new DsfDataObject(CurrentDl, exId)
            {
                // NOTE: WorkflowApplicationFactory.InvokeWorkflowImpl() will use HostSecurityProvider.Instance.ServerID 
                //       if this is NOT provided which will cause the tests to fail!
                ServerID = Guid.NewGuid(),
                IsDebug = isDebug,
                ForEachNestingLevel = nestingLevel,
                ParentThreadID = 1
            };


            // we need to set this now ;)

            mockChannel.Setup(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors)).Verifiable();
            CustomContainer.Register<IActivityParser>(new ActivityParser());
            WfExecutionContainer wfec = new WfExecutionContainer(svc, dataObject, WorkspaceRepository.Instance.ServerWorkspace, mockChannel.Object);

            errors.ClearErrors();
            wfec.Eval(FlowchartProcess,Guid.NewGuid(),dataObject);

            return mockChannel;
        }

        /// <summary>
        /// The ForEach Activity requires the data returned from an activity
        /// We will mock the DSF channel to return something that we expect is shaped.
        /// </summary>
        /// <returns></returns>
        public void ExecuteForEachProcessForReal(out IDSFDataObject dataObject)
        {
            var svc = new ServiceAction { Name = "ForEachTestAction", ServiceName = "UnitTestService" };
            IEsbChannel channel = new EsbServicesEndpoint();

            svc.SetActivity(FlowchartProcess);

            if(CurrentDl == null)
            {
                CurrentDl = TestData;
            }

            Compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid exId = Compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), new StringBuilder(TestData), new StringBuilder(CurrentDl), out errors);


            if(errors.HasErrors())
            {
                string errorString = errors.FetchErrors().Aggregate(string.Empty, (current, item) => current + item);

                throw new Exception(errorString);
            }

            dataObject = new DsfDataObject(CurrentDl, exId)
            {
                // NOTE: WorkflowApplicationFactory.InvokeWorkflowImpl() will use HostSecurityProvider.Instance.ServerID 
                //       if this is NOT provided which will cause the tests to fail!
                ServerID = Guid.NewGuid(),
                ParentThreadID = 1
            };


            // we need to set this now ;)
            WfExecutionContainer wfec = new WfExecutionContainer(svc, dataObject, WorkspaceRepository.Instance.ServerWorkspace, channel);

            errors.ClearErrors();
            dataObject.DataListID = wfec.Execute(out errors);

        }

        #endregion ForEach Execution

        #region Activity Debug Input/Output Test Methods

        public dynamic CheckActivityDebugInputOutput<T>(DsfNativeActivity<T> activity, string dataListShape, string dataListWithData, out List<DebugItem> inputResults, out List<DebugItem> outputResults, bool isRemoteInvoke = false)
        {

            TestStartNode = new FlowStep
            {
                Action = activity
            };

            TestData = dataListWithData;
            CurrentDl = dataListShape;
            if (DataObject.Environment == null)
            {
                DataObject.Environment = new ExecutionEnvironment();
            }
            
            inputResults = null;
            outputResults = null;
            var result = ExecuteProcess(null, true, null, isRemoteInvoke);
            if(result != null)
            {
                inputResults = activity.GetDebugInputs(result.Environment);
                outputResults = activity.GetDebugOutputs(result.Environment);

               
            }
            return result;
        }

        public dynamic CheckPathOperationActivityDebugInputOutput<T>(DsfNativeActivity<T> activity, string dataListShape,
                                                  string dataListWithData, out List<DebugItem> inputResults, out List<DebugItem> outputResults, IPrincipal user = null)
        {
            TestStartNode = new FlowStep
            {
                Action = activity
            };

            TestData = dataListWithData;
            CurrentDl = dataListShape;
            User = user;
            if (DataObject.Environment == null)
            {
                DataObject.Environment = new ExecutionEnvironment();
            }
            var result = ExecuteProcess(null, true);
            inputResults = null;
            outputResults = null;
            if(result != null)
            {
                inputResults = activity.GetDebugInputs(result.Environment);
                outputResults = activity.GetDebugOutputs(result.Environment);

                
            }
            return result;
        }

        public IPrincipal User { get; set; }

        public bool CreateDataListWithRecsetAndCreateShape(List<string> recsetData, string recsetName, string fieldName, out string dataListShape, out string dataListWithData)
        {
            const bool Result = false;

            dataListShape = "<ADL>";
            dataListWithData = recsetData.Aggregate("<ADL>", (current, rowData) => string.Concat(current, "<", recsetName, ">", "<", fieldName, ">", rowData, "</", fieldName, ">", "</", recsetName, ">"));
            #region Create DataList With Data

            dataListWithData = string.Concat(dataListWithData, "<res></res>", "</ADL>");

            #endregion

            #region Create Shape

            dataListShape = string.Concat(dataListShape, "<", recsetName, ">", "<", fieldName, ">", "</", fieldName, ">", "</", recsetName, ">", "<res></res></ADL>");

            #endregion

            return Result;
        }

        public bool CreateDataListWithMultipleRecsetAndCreateShape(List<List<string>> recsetData, List<string> recsetName, List<string> fieldName, out string dataListShape, out string dataListWithData)
        {
            dataListShape = "<ADL>";
            dataListWithData = "<ADL>";
            #region Create DataList With Data

            for(int i = 0; i < recsetData.Count; i++)
            {
                dataListWithData = recsetData[i].Aggregate(dataListWithData, (current, rowData) => string.Concat(current, "<", recsetName[i], ">", "<", fieldName[i], ">", rowData, "</", fieldName[i], ">", "</", recsetName[i], ">"));
                dataListShape = string.Concat(dataListShape, "<", recsetName[i], ">", "<", fieldName[i], ">", "</", fieldName[i], ">", "</", recsetName[i], ">");
            }

            dataListWithData = string.Concat(dataListWithData, "<res></res>", "</ADL>");

            #endregion

            #region Create Shape

            dataListShape = string.Concat(dataListShape, "<res></res></ADL>");

            #endregion

            return false;
        }
        #endregion

        #region Retrieve DataList Values

        public bool GetScalarValueFromEnvironment(IExecutionEnvironment env, string fieldToRetrieve, out string result, out string error)
        {

            error = "";
            result = null;
            if (fieldToRetrieve == GlobalConstants.ErrorPayload)
            {
                result = env.FetchErrors();
                return true;
            }
            try
            {
                result = ExecutionEnvironment.WarewolfEvalResultToString(env.Eval(DataListUtil.AddBracketsToValueIfNotExist(fieldToRetrieve)));
            }
            catch( Exception err)
            {
                error = err.Message;
            }
            return true;
        }

        public bool GetRecordSetFieldValueFromDataList(IExecutionEnvironment environment, string recordSet, string fieldNameToRetrieve, out IList<string> result, out string error)
        {
            bool isCool = true;
            var variableName = recordSet;
            result = new List<string>();
            error = "";
            try
            {
                if (!string.IsNullOrEmpty(fieldNameToRetrieve))
                {
                    variableName = DataListUtil.CreateRecordsetDisplayValue(recordSet, fieldNameToRetrieve, "*");
                }
                var warewolfEvalResult = environment.Eval(DataListUtil.AddBracketsToValueIfNotExist(variableName));

                if (warewolfEvalResult == null)
                {
                    return false;
                }
                var listResult = warewolfEvalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;
                if (listResult != null)
                {
                    foreach(var res in listResult.Item)
                    {
                        result.Add(ExecutionEnvironment.WarewolfAtomToString(res));
                    }
                }               
            }
            catch(Exception e)
            {
                error = e.Message;
            }
            
            if(!string.IsNullOrEmpty(error))
            {
                isCool = false;
            }

            return isCool;
        }

        protected List<string> RetrieveAllRecordSetFieldValues(IExecutionEnvironment environment, string recordSetName, string fieldToRetrieve, out string error)
        {
            var retVals = environment.EvalAsListOfStrings("[[" + recordSetName + "(*)." + fieldToRetrieve + "]]");
            error = "";
            var retrieveAllRecordSetFieldValues = (List<string>)retVals;
            return retrieveAllRecordSetFieldValues.Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        #endregion Retrieve DataList Values
    }
}
