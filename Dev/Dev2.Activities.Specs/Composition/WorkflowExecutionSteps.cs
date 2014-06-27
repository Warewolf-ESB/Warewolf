using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Activities.Specs.Composition.DBSource;
using Dev2.Data.Util;
using Dev2.Diagnostics.Debug;
using Dev2.Messages;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services;
using Dev2.Session;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Network;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Threading;
using Dev2.TO;
using Dev2.Util;
using Dev2.Utilities;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Composition
{
    [Binding]
    public class WorkflowExecutionSteps : RecordSetBases
    {
        private SubscriptionService<DebugWriterWriteMessage> _debugWriterSubscriptionService;
        private readonly AutoResetEvent _resetEvt = new AutoResetEvent(false);
        protected override void BuildDataList()
        {
            BuildShapeAndTestData();
        }

        [Then(@"the workflow execution has ""(.*)"" error")]
        public void ThenTheWorkflowExecutionHasError(string p0)
        {
        }

        [Given(@"I have a workflow ""(.*)""")]
        public void GivenIHaveAWorkflow(string workflowName)
        {
            AppSettings.LocalHost = "http://localhost:3142";
            IEnvironmentModel environmentModel = EnvironmentRepository.Instance.Source;
            environmentModel.Connect();
            var resourceModel = new ResourceModel(environmentModel) { Category = "Acceptance Tests\\" + workflowName, ResourceName = workflowName, ID = Guid.NewGuid(), ResourceType = ResourceType.WorkflowService };

            environmentModel.ResourceRepository.Add(resourceModel);
            _debugWriterSubscriptionService = new SubscriptionService<DebugWriterWriteMessage>(environmentModel.Connection.ServerEvents);

            _debugWriterSubscriptionService.Subscribe(msg => Append(msg.DebugState));
            Add(workflowName, resourceModel);
            Add("parentWorkflowName", workflowName);
            Add("environment", environmentModel);
            Add("resourceRepo", environmentModel.ResourceRepository);
            Add("debugStates", new List<IDebugState>());
        }

        void Add(string key, object value)
        {
            ScenarioContext.Current.Add(key, value);
        }

        void Append(IDebugState debugState)
        {
            List<IDebugState> debugStates;
            string workflowName;
            TryGetValue("debugStates", out debugStates);
            TryGetValue("parentWorkflowName", out workflowName);

            debugStates.Add(debugState);
            if(debugState.IsFinalStep() && debugState.DisplayName.Equals(workflowName))
                _resetEvt.Set();

        }

        [Given(@"""(.*)"" contains a ""(.*)"" service ""(.*)"" with mappings")]
        public void GivenContainsADatabaseServiceWithMappings(string wf, string serviceType, string serviceName, Table table)
        {
            IEnvironmentModel environmentModel = EnvironmentRepository.Instance.Source;
            ResourceRepository repository = new ResourceRepository(environmentModel);
            repository.Load();
            var resource = repository.FindSingle(r => r.ResourceName.Equals(serviceName));


            var activity = GetServiceActivity(serviceType);
            if(activity != null)
            {
                var outputSb = GetOutputMapping(table, resource);
                var inputSb = GetInputMapping(table, resource);
                var outputMapping = outputSb.ToString();
                var inputMapping = inputSb.ToString();
                resource.Outputs = outputMapping;
                resource.Inputs = inputMapping;

                activity.ResourceID = resource.ID;
                activity.ServiceName = resource.ResourceName;
                activity.DisplayName = serviceName;
                activity.OutputMapping = outputMapping;
                activity.InputMapping = inputMapping;
                CommonSteps.AddActivityToActivityList(wf, serviceName, activity);
            }
        }

        [Given(@"""(.*)"" contains ""(.*)"" from server ""(.*)"" with mapping as")]
        public void GivenContainsFromServerWithMappingAs(string wf, string remoteWf, string server, Table mappings)
        {
            var remoteEnvironment = EnvironmentRepository.Instance.FindSingle(model => model.Name == server);
            if(remoteEnvironment != null)
            {
                remoteEnvironment.Connect();
                remoteEnvironment.ForceLoadResources();
                var remoteResourceModel = remoteEnvironment.ResourceRepository.FindSingle(model => model.Category.ToLower() == remoteWf.ToLower(), true);
                if(remoteResourceModel != null)
                {
                    var dataMappingViewModel = GetDataMappingViewModel(remoteResourceModel, mappings);

                    var inputMapping = dataMappingViewModel.GetInputString(dataMappingViewModel.Inputs);
                    var outputMapping = dataMappingViewModel.GetOutputString(dataMappingViewModel.Outputs);

                    var activity = new DsfWorkflowActivity();
                    remoteResourceModel.Outputs = outputMapping;
                    remoteResourceModel.Inputs = inputMapping;
                    var remoteServerId = remoteEnvironment.ID;
                    activity.ServiceServer = remoteServerId;
                    activity.EnvironmentID = remoteServerId;
                    activity.ServiceUri = remoteEnvironment.Connection.AppServerUri.ToString();
                    activity.ResourceID = remoteResourceModel.ID;
                    activity.ServiceName = remoteResourceModel.Category;
                    activity.DisplayName = remoteResourceModel.Category;
                    activity.OutputMapping = outputMapping;
                    activity.InputMapping = inputMapping;
                    CommonSteps.AddActivityToActivityList(wf, remoteWf, activity);
                }
            }
        }

        static DataMappingViewModel GetDataMappingViewModel(IResourceModel remoteResourceModel, Table mappings)
        {
            var webActivity = new WebActivity { ResourceModel = remoteResourceModel as ResourceModel };
            DataMappingViewModel dataMappingViewModel = new DataMappingViewModel(webActivity);
            foreach(var tableRow in mappings.Rows)
            {
                var output = tableRow["Output from Service"];
                var toVariable = tableRow["To Variable"];
                if(!string.IsNullOrEmpty(output) && !string.IsNullOrEmpty(toVariable))
                {
                    var inputOutputViewModel = dataMappingViewModel.Outputs.FirstOrDefault(model => model.DisplayName == output);
                    if(inputOutputViewModel != null)
                    {
                        inputOutputViewModel.Value = toVariable;
                        CommonSteps.AddVariableToVariableList(toVariable);
                    }
                }

                var input = tableRow["Input to Service"];
                var fromVariable = tableRow["From Variable"];

                if(!string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(fromVariable))
                {
                    var inputOutputViewModel = dataMappingViewModel.Inputs.FirstOrDefault(model => model.DisplayName == input);
                    if(inputOutputViewModel != null)
                    {
                        inputOutputViewModel.MapsTo = fromVariable;
                        CommonSteps.AddVariableToVariableList(fromVariable);
                    }
                }

            }
            return dataMappingViewModel;
        }

        static DsfActivity GetServiceActivity(string serviceType)
        {
            DsfActivity activity = null;
            switch(serviceType)
            {
                case "database":
                    activity = new DsfDatabaseActivity();
                    break;
                case "plugin":
                    activity = new DsfPluginActivity();
                    break;
                case "webservice":
                    activity = new DsfWebserviceActivity();
                    break;
                case "workflow":
                    activity = new DsfWorkflowActivity();
                    break;
            }
            return activity;
        }

        static StringBuilder GetOutputMapping(Table table, IResourceModel resource)
        {
            var outputSb = new StringBuilder();
            outputSb.Append("<Outputs>");

            foreach(var tableRow in table.Rows)
            {
                var output = tableRow["Output from Service"];
                var toVariable = tableRow["To Variable"];

                CommonSteps.AddVariableToVariableList(toVariable);

                if(resource != null)
                {

                    var outputs = XDocument.Parse(resource.Outputs);

                    string recordsetName;
                    string fieldName;

                    if(DataListUtil.IsValueRecordset(output))
                    {
                        recordsetName = DataListUtil.ExtractRecordsetNameFromValue(output);
                        fieldName = DataListUtil.ExtractFieldNameFromValue(output);
                    }
                    else
                    {
                        recordsetName = fieldName = output;
                    }

                    var element = (from elements in outputs.Descendants("Output")
                                   where (string)elements.Attribute("Recordset") == recordsetName &&
                                         (string)elements.Attribute("OriginalName") == fieldName
                                   select elements).SingleOrDefault();

                    if(element != null)
                    {
                        element.SetAttributeValue("Value", toVariable);
                    }

                    outputSb.Append(element);
                }
            }

            outputSb.Append("</Outputs>");
            return outputSb;
        }

        [Given(@"""(.*)"" contains a Sequence ""(.*)"" as")]
        public void GivenContainsASequenceAs(string parentName, string activityName)
        {
            var dsfSequence = new DsfSequenceActivity { DisplayName = activityName };
            CommonSteps.AddActivityToActivityList(parentName, activityName, dsfSequence);
        }


        static StringBuilder GetInputMapping(Table table, IResourceModel resource)
        {
            var inputSb = new StringBuilder();
            inputSb.Append("<Inputs>");

            foreach(var tableRow in table.Rows)
            {
                var input = tableRow["Input to Service"];
                var fromVariable = tableRow["From Variable"];

                CommonSteps.AddVariableToVariableList(fromVariable);

                if(resource != null)
                {
                    var inputs = XDocument.Parse(resource.Inputs);

                    string recordsetName;
                    XElement element;
                    if(DataListUtil.IsValueRecordset(input))
                    {
                        recordsetName = DataListUtil.ExtractRecordsetNameFromValue(input);
                        string fieldName = DataListUtil.ExtractFieldNameFromValue(input);

                        element = (from elements in inputs.Descendants("Input")
                                   where (string)elements.Attribute("Recordset") == recordsetName &&
                                         (string)elements.Attribute("OriginalName") == fieldName
                                   select elements).SingleOrDefault();

                        if(element != null)
                        {
                            element.SetAttributeValue("Value", fromVariable);
                        }

                        inputSb.Append(element);
                    }
                    else
                    {
                        recordsetName = input;

                        element = (from elements in inputs.Descendants("Input")
                                   where (string)elements.Attribute("Name") == recordsetName
                                   select elements).SingleOrDefault();

                        if(element != null)
                        {
                            element.SetAttributeValue("Source", fromVariable);
                        }
                    }

                    if(element != null)
                    {
                        inputSb.Append(element);
                    }
                }
            }

            inputSb.Append("</Inputs>");
            return inputSb;
        }

        [When(@"""(.*)"" is executed")]
        public void WhenIsExecuted(string workflowName)
        {
            BuildDataList();

            var activityList = CommonSteps.GetActivityList();

            var flowSteps = new List<FlowStep>();

            TestStartNode = new FlowStep();
            flowSteps.Add(TestStartNode);

            foreach(var activity in activityList)
            {
                if(TestStartNode.Action == null)
                {
                    TestStartNode.Action = activity.Value;
                }
                else
                {
                    var flowStep = new FlowStep { Action = activity.Value };
                    flowSteps.Last().Next = flowStep;
                    flowSteps.Add(flowStep);
                }
            }

            IContextualResourceModel resourceModel;
            IEnvironmentModel environmentModel;
            IResourceRepository repository;
            TryGetValue(workflowName, out resourceModel);
            TryGetValue("environment", out environmentModel);
            TryGetValue("resourceRepo", out repository);

            string currentDl = CurrentDl;
            resourceModel.DataList = currentDl.Replace("root", "DataList");
            WorkflowHelper helper = new WorkflowHelper();
            StringBuilder xamlDefinition = helper.GetXamlDefinition(FlowchartActivityBuilder);
            resourceModel.WorkflowXaml = xamlDefinition;

            repository.Save(resourceModel, false);
            repository.SaveToServer(resourceModel);

            ExecuteWorkflow(resourceModel);
        }


        [Then(@"the '(.*)' in WorkFlow '(.*)' debug inputs as")]
        public void ThenTheInWorkFlowDebugInputsAs(string toolName, string workflowName, Table table)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates");

            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(workflowName, StringComparison.InvariantCultureIgnoreCase)).ID;

            if(parentWorkflowName == workflowName)
            {
                workflowId = Guid.Empty;
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID == workflowId && ds.DisplayName.Equals(toolName, StringComparison.InvariantCultureIgnoreCase)).ToList();

            // Data Merge breaks our debug scheme, it only ever has 1 value, not the expected 2 ;)
            //bool isDataMergeDebug = toolSpecificDebug.Any(t => t.Name == "Data Merge");

            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugInputsAs(table, toolSpecificDebug
                                                    .SelectMany(s => s.Inputs)
                                                    .SelectMany(s => s.ResultsList).ToList());
        }

        T Get<T>(string keyName)
        {
            return ScenarioContext.Current.Get<T>(keyName);
        }

        void TryGetValue<T>(string keyName, out T value)
        {
            ScenarioContext.Current.TryGetValue(keyName, out value);
        }

        [Then(@"the '(.*)' in Workflow '(.*)' debug outputs as")]
        public void ThenTheInWorkflowDebugOutputsAs(string toolName, string workflowName, Table table)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates");
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(workflowName, StringComparison.InvariantCultureIgnoreCase)).ID;

            if(parentWorkflowName == workflowName)
            {
                workflowId = Guid.Empty;
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID == workflowId && ds.DisplayName.Equals(toolName, StringComparison.InvariantCultureIgnoreCase)).ToList();

            // Data Merge breaks our debug scheme, it only ever has 1 value, not the expected 2 ;)
            bool isDataMergeDebug = toolSpecificDebug.Count == 1 && toolSpecificDebug.Any(t => t.Name == "Data Merge");

            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugOutputAs(table, toolSpecificDebug
                                                    .SelectMany(s => s.Outputs)
                                                    .SelectMany(s => s.ResultsList).ToList(), isDataMergeDebug);
        }

        [Given(@"""(.*)"" contains an SQL Bulk Insert ""(.*)"" using database ""(.*)"" and table ""(.*)"" and KeepIdentity set ""(.*)"" and Result set ""(.*)"" as")]
        public void GivenContainsAnSqlBulkInsertAs(string workflowName, string activityName, string dbSrcName, string tableName, string keepIdentity, string result, Table table)
        {
            //ScenarioContext.Current.Pending();

            // Fetch source from source name ;)
            var resourceXml = XmlFetch.Fetch(dbSrcName);
            if(resourceXml != null)
            {

                // extract keepIdentity value ;)
                bool keepIdentityBool;
                bool.TryParse(keepIdentity, out keepIdentityBool);

                var dbSource = new DbSource(resourceXml);
                // Configure activity ;)
                var dsfSqlBulkInsert = new DsfSqlBulkInsertActivity { Result = result, DisplayName = activityName, TableName = tableName, Database = dbSource, KeepIdentity = keepIdentityBool };
                // build input mapping
                var mappings = new List<DataColumnMapping>();

                var pos = 1;
                // ReSharper disable LoopCanBeConvertedToQuery
                foreach(var row in table.Rows)
                // ReSharper restore LoopCanBeConvertedToQuery
                {
                    var outputColumn = row["Column"];
                    var inputColumn = row["Mapping"];
                    var isNullableStr = row["IsNullable"];
                    var dataTypeName = row["DataTypeName"];
                    var maxLengthStr = row["MaxLength"];
                    var isAutoIncrementStr = row["IsAutoIncrement"];

                    bool isNullable;
                    bool isAutoIncrement;
                    int maxLength;
                    bool.TryParse(isNullableStr, out isNullable);
                    bool.TryParse(isAutoIncrementStr, out isAutoIncrement);
                    Int32.TryParse(maxLengthStr, out maxLength);
                    SqlDbType dataType;
                    Enum.TryParse(dataTypeName, true, out dataType);

                    var mapping = new DataColumnMapping { IndexNumber = pos, InputColumn = inputColumn, OutputColumn = new DbColumn { ColumnName = outputColumn, IsAutoIncrement = isAutoIncrement, IsNullable = isNullable, MaxLength = maxLength, SqlDataType = dataType } };
                    mappings.Add(mapping);
                    pos++;

                }

                dsfSqlBulkInsert.InputMappings = mappings;

                CommonSteps.AddActivityToActivityList(workflowName, activityName, dsfSqlBulkInsert);
                CommonSteps.AddVariableToVariableList(result);

            }
            else
            {
                throw new Exception("Invalid Source Name [ " + dbSrcName + " ]. Ensure it has been properly added to the DBSource directory in this project.");
            }
        }


        [Given(@"""(.*)"" contains an Sort ""(.*)"" as")]
        public void GivenContainsAnSortAs(string parentName, string activityName, Table table)
        {
            var dsfSort = new DsfSortRecordsActivity { DisplayName = activityName, SortField = table.Rows[0][0], SelectedSort = table.Rows[0][1] };
            CommonSteps.AddActivityToActivityList(parentName, activityName, dsfSort);
        }


        [Given(@"""(.*)"" contains an Delete ""(.*)"" as")]
        // ReSharper disable InconsistentNaming
        public void GivenContainsAnDeleteAs(string parentName, string activityName, Table table)
        // ReSharper restore InconsistentNaming
        {
            var del = new DsfPathDelete { InputPath = table.Rows[0][0], Result = table.Rows[0][1], DisplayName = activityName };
            CommonSteps.AddVariableToVariableList(table.Rows[0][1]);
            CommonSteps.AddActivityToActivityList(parentName, activityName, del);
        }

        public void ExecuteWorkflow(IContextualResourceModel resourceModel)
        {
            if(resourceModel == null || resourceModel.Environment == null)
            {
                return;
            }

            var debugTo = new DebugTO { XmlData = "<DataList></DataList>", SessionID = Guid.NewGuid(), IsDebugMode = true };

            var clientContext = resourceModel.Environment.Connection;
            if(clientContext != null)
            {
                var dataList = XElement.Parse(debugTo.XmlData);
                dataList.Add(new XElement("BDSDebugMode", debugTo.IsDebugMode));
                dataList.Add(new XElement("DebugSessionID", debugTo.SessionID));
                dataList.Add(new XElement("EnvironmentID", resourceModel.Environment.ID));
                WebServer.Send(WebServerMethod.POST, resourceModel, dataList.ToString(), new TestAsyncWorker());
                _resetEvt.WaitOne();
            }
        }
        [AfterScenario]
        public void AfterScenario()
        {
            if(_debugWriterSubscriptionService != null)
            {
                _debugWriterSubscriptionService.Unsubscribe();
                _debugWriterSubscriptionService.Dispose();
            }
            if(_resetEvt != null)
            {
                _resetEvt.Close();
            }
        }
    }
}
