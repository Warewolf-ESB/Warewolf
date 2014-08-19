using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Activities.Specs.Composition.DBSource;
using Dev2.Common;
using Dev2.Data.Enums;
using Dev2.Data.ServiceModel;
using Dev2.Data.Util;
using Dev2.Diagnostics.Debug;
using Dev2.DynamicServices;
using Dev2.Messages;
using Dev2.Network;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services;
using Dev2.Session;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Network;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Threading;
using Dev2.TO;
using Dev2.Util;
using Dev2.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void ThenTheWorkflowExecutionHasError(string hasError)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates");

            if(hasError == "AN")
            {
                var hasErrorState = debugStates.FirstOrDefault(state => state.HasError);
                Assert.IsNotNull(hasErrorState);
            }
        }

        [Given(@"I have server a ""(.*)"" with workflow ""(.*)""")]
        public void GivenIHaveAWorkflowOnServer(string serverName, string workflow)
        {
            AppSettings.LocalHost = "http://localhost:3142";
            var environmentModel = EnvironmentRepository.Instance.Source;
            environmentModel.Connect();
            environmentModel.ResourceRepository.ForceLoad();

            // connect to the remove environment now ;)
            var remoteServerList = environmentModel.ResourceRepository.FindSourcesByType<Connection>(environmentModel, enSourceType.Dev2Server);
            if(remoteServerList != null && remoteServerList.Count > 0)
            {
                var remoteServer = remoteServerList.FirstOrDefault(r => r.ResourceName == serverName);

                if(remoteServer != null)
                {
                    ServerProxy connection;
                    if(remoteServer.AuthenticationType == AuthenticationType.Windows || remoteServer.AuthenticationType == AuthenticationType.Anonymous)
                    {
                        connection = new ServerProxy(new Uri(remoteServer.WebAddress));
                    }
                    else
                    {
                        //
                        // NOTE: Public needs to drop through to User for the rest of the framework to pick up properly behind the scenes ;)
                        //
                        connection = new ServerProxy(remoteServer.WebAddress, remoteServer.UserName, remoteServer.Password);
                    }

                    var newEnvironment = new EnvironmentModel(remoteServer.ResourceID, connection) { Name = remoteServer.ResourceName, Category = remoteServer.ResourcePath };
                    newEnvironment.Connect();
                    newEnvironment.ForceLoadResources();

                    // now find the bloody resource model for execution
                    var resourceModel = newEnvironment.ResourceRepository.Find(r => r.ResourceName == workflow).FirstOrDefault();

                    _debugWriterSubscriptionService = new SubscriptionService<DebugWriterWriteMessage>(newEnvironment.Connection.ServerEvents);

                    _debugWriterSubscriptionService.Subscribe(msg => Append(msg.DebugState));

                    Add(workflow, resourceModel);
                    Add("parentWorkflowName", workflow);
                    Add("environment", newEnvironment);
                    Add("resourceRepo", newEnvironment.ResourceRepository);
                    Add("debugStates", new List<IDebugState>());

                }
            }
        }

        [Given(@"I have a workflow ""(.*)""")]
        public void GivenIHaveAWorkflow(string workflowName)
        {
            AppSettings.LocalHost = "http://localhost:3142";
            IEnvironmentModel environmentModel = EnvironmentRepository.Instance.Source;
            environmentModel.Connect();
            var resourceModel = new ResourceModel(environmentModel) { Category = "Acceptance Tests\\" + workflowName, ResourceName = workflowName, ID = Guid.NewGuid(), ResourceType = Studio.Core.AppResources.Enums.ResourceType.WorkflowService };

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

        [Then(@"the '(.*)' in step (.*) for '(.*)' debug inputs as")]
        public void ThenTheInStepForDebugInputsAs(string toolName, int stepNumber, string forEachName, Table table)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates");
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(forEachName)).ID;

            if(parentWorkflowName == forEachName)
            {
                workflowId = Guid.Empty;
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID == workflowId && ds.DisplayName.Equals(toolName)).ToList();

            Assert.IsTrue(toolSpecificDebug.Count >= stepNumber);
            var debugToUse = DebugToUse(stepNumber, toolSpecificDebug);


            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugInputsAs(table, debugToUse.Inputs
                                                    .SelectMany(item => item.ResultsList).ToList());
        }

        [Then(@"the '(.*)' in ""(.*)"" in step (.*) for '(.*)' debug inputs as")]
        public void ThenTheInInStepForDebugInputsAs(string toolName, string sequenceName, int stepNumber, string forEachName, Table table)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates");
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(forEachName)).ID;

            if(parentWorkflowName == forEachName)
            {
                workflowId = Guid.Empty;
            }

            var sequenceDebug = debugStates.Where(ds => ds.ParentID == workflowId).ToList();
            Assert.IsTrue(sequenceDebug.Count >= stepNumber);

            var sequenceId = sequenceDebug[stepNumber - 1].ID;
            var sequenceIsInForEach = sequenceDebug.Any(state => state.ID == sequenceId);
            Assert.IsTrue(sequenceIsInForEach);

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID == sequenceId && ds.DisplayName.Equals(toolName)).ToList();

            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugInputsAs(table, toolSpecificDebug
                                                    .SelectMany(item => item.Inputs)
                                                    .SelectMany(item => item.ResultsList).ToList());

        }

        [Then(@"the '(.*)' in ""(.*)"" in step (.*) for '(.*)' debug outputs as")]
        public void ThenTheInInStepForDebugOutputsAs(string toolName, string sequenceName, int stepNumber, string forEachName, Table table)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates");
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(forEachName)).ID;

            if(parentWorkflowName == forEachName)
            {
                workflowId = Guid.Empty;
            }

            var sequenceDebug = debugStates.Where(ds => ds.ParentID == workflowId).ToList();
            Assert.IsTrue(sequenceDebug.Count >= stepNumber);

            var sequenceId = sequenceDebug[stepNumber - 1].ID;
            var sequenceIsInForEach = sequenceDebug.Any(state => state.ID == sequenceId);
            Assert.IsTrue(sequenceIsInForEach);

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID == sequenceId && ds.DisplayName.Equals(toolName)).ToList();

            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugInputsAs(table, toolSpecificDebug
                                                    .SelectMany(item => item.Outputs)
                                                    .SelectMany(item => item.ResultsList).ToList());
        }


        static IDebugState DebugToUse(int stepNumber, List<IDebugState> toolSpecificDebug)
        {
            var debugToUse = toolSpecificDebug[stepNumber - 1];
            return debugToUse;
        }

        [Then(@"the '(.*)' in step (.*) for '(.*)' debug outputs as")]
        public void ThenTheInStepForDebugOutputsAs(string toolName, int stepNumber, string forEachName, Table table)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates");
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(forEachName)).ID;

            if(parentWorkflowName == forEachName)
            {
                workflowId = Guid.Empty;
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID == workflowId && ds.DisplayName.Equals(toolName)).ToList();
            Assert.IsTrue(toolSpecificDebug.Count >= stepNumber);
            var debugToUse = DebugToUse(stepNumber, toolSpecificDebug);

            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugOutputAs(table, debugToUse.Outputs
                                                    .SelectMany(s => s.ResultsList).ToList());
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
                var remoteResourceModel = remoteEnvironment.ResourceRepository.FindSingle(model => model.ResourceName == remoteWf, true);
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
                    activity.DisplayName = remoteWf;
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

        [Given(@"""(.*)"" in '(.*)' contains Data Merge ""(.*)"" into ""(.*)"" as")]
        public void GivenInContainsDataMergeIntoAs(string sequenceName, string forEachName, string activityName, string resultVariable, Table table)
        {
            DsfDataMergeActivity activity = new DsfDataMergeActivity { Result = resultVariable, DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var variable = tableRow["Variable"];
                var type = tableRow["Type"];
                var at = tableRow["Using"];
                var padding = tableRow["Padding"];
                var alignment = tableRow["Alignment"];

                activity.MergeCollection.Add(new DataMergeDTO(variable, type, at, 1, padding, alignment, true));
            }
            CommonSteps.AddVariableToVariableList(resultVariable);
            AddActivityToSequenceInsideForEach(sequenceName, forEachName, activity);
        }

        static void AddActivityToSequenceInsideForEach(string sequenceName, string forEachName, Activity activity)
        {

            var activityList = CommonSteps.GetActivityList();
            var forEachActivity = activityList[forEachName] as DsfForEachActivity;
            if(forEachActivity != null)
            {
                var sequenceActivity = forEachActivity.DataFunc.Handler as DsfSequenceActivity;
                if(sequenceActivity != null && sequenceActivity.DisplayName == sequenceName)
                {
                    sequenceActivity.Activities.Add(activity);
                }
            }
        }

        [Given(@"""(.*)"" in '(.*)' contains Gather System Info ""(.*)"" as")]
        public void GivenInContainsGatherSystemInfoAs(string sequenceName, string forEachName, string activityName, Table table)
        {
            var activity = new DsfGatherSystemInformationActivity { DisplayName = activityName };
            foreach(var tableRow in table.Rows)
            {
                var variable = tableRow["Variable"];

                CommonSteps.AddVariableToVariableList(variable);

                enTypeOfSystemInformationToGather systemInfo = (enTypeOfSystemInformationToGather)Dev2EnumConverter.GetEnumFromStringDiscription(tableRow["Selected"], typeof(enTypeOfSystemInformationToGather));
                activity.SystemInformationCollection.Add(new GatherSystemInformationTO(systemInfo, variable, 1));
            }

            AddActivityToSequenceInsideForEach(sequenceName, forEachName, activity);
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

        [When(@"""(.*)"" is the active environment used to execute ""(.*)""")]
        public void WhenIsTheActiveEnvironmentUsedToExecute(string connectionName, string workflowName)
        {
            // environment
            IEnvironmentModel environmentModel;
            IResourceRepository repository;
            IContextualResourceModel resourceModel;

            TryGetValue(workflowName, out resourceModel);
            TryGetValue("environment", out environmentModel);
            TryGetValue("resourceRepo", out repository);

            ExecuteWorkflow(resourceModel);
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

            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(workflowName)).ID;

            if(parentWorkflowName == workflowName)
            {
                workflowId = Guid.Empty;
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID == workflowId && ds.DisplayName.Equals(toolName)).ToList();

            // Data Merge breaks our debug scheme, it only ever has 1 value, not the expected 2 ;)
            //bool isDataMergeDebug = toolSpecificDebug.Any(t => t.Name == "Data Merge");

            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugInputsAs(table, toolSpecificDebug
                                                    .SelectMany(s => s.Inputs)
                                                    .SelectMany(s => s.ResultsList).ToList());
        }


        [Then(@"the nested '(.*)' in WorkFlow '(.*)' debug inputs as")]
        public void ThenTheNestedInWorkFlowDebugInputsAs(string toolName, string workflowName, Table table)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates");

            var toolSpecificDebug =
                debugStates.Where(ds => ds.DisplayName.Equals(toolName)).ToList();

            // Data Merge breaks our debug scheme, it only ever has 1 value, not the expected 2 ;)
            //bool isDataMergeDebug = toolSpecificDebug.Any(t => t.Name == "Data Merge");

            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugInputsAs(table, toolSpecificDebug
                                                    .SelectMany(s => s.Inputs)
                                                    .SelectMany(s => s.ResultsList).ToList());
        }


        [Then(@"the '(.*)' in WorkFlow '(.*)' has  ""(.*)"" nested children")]
        public void ThenTheInWorkFlowHasNestedChildren(string toolName, string workflowName, int count)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates");

            var id =
                debugStates.Where(ds => ds.DisplayName.Equals(toolName)).ToList().Select(a => a.ID).First();
            var children = debugStates.Count(a => a.ParentID == id);
            Assert.AreEqual(children, count);
        }



        [Then(@"each nested debug item for '(.*)' in WorkFlow '(.*)' contains ""(.*)"" child                              \|")]
        public void ThenEachNestedDebugItemForInWorkFlowContainsChild(string toolName, string workFlowName, int childCount)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates");

            var id = debugStates.Where(ds => ds.DisplayName.Equals("DsfActivity")).ToList();
            id.ForEach(x => Assert.AreEqual(childCount, debugStates.Count(a => a.ParentID == x.ID && a.DisplayName == toolName)));

        }



        [Then(@"each ""(.*)"" contains debug outputs for ""(.*)"" as")]
        public void ThenEachContainsDebugOutputsForAs(string toolName, string nestedToolName, Table table)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);
            var debugStates = Get<List<IDebugState>>("debugStates");

            var id = debugStates.Where(ds => ds.DisplayName.Equals("DsfActivity")).ToList();
            id.ForEach(x => Assert.AreEqual(1, debugStates.Count(a => a.ParentID == x.ID && a.DisplayName == nestedToolName)));
        }


        T Get<T>(string keyName)
        {
            return ScenarioContext.Current.Get<T>(keyName);
        }

        void TryGetValue<T>(string keyName, out T value)
        {
            ScenarioContext.Current.TryGetValue(keyName, out value);
        }


        public string GetServerMemory()
        {
            var stringBuilder = new StringBuilder();
            var winQuery = new ObjectQuery("SELECT * FROM Win32_Process Where Name LIKE '%Warewolf Server.exe%'");
            var searcher = new ManagementObjectSearcher(winQuery);
            foreach(var o in searcher.Get())
            {
                var item = (ManagementObject)o;
                var memory = Convert.ToString(item["WorkingSetSize"]);
                stringBuilder.Append(memory);
            }
            return stringBuilder.ToString();
        }

        // ReSharper disable InconsistentNaming
        public double GetServerCPUUsage()
        {
            PerformanceCounter processorTimeCounter = new PerformanceCounter(
                    "Process",
                    "% Processor Time",
                    "Warewolf Server", true);
            processorTimeCounter.NextValue();
            Thread.Sleep(1000);
            return processorTimeCounter.NextValue() / Environment.ProcessorCount;
        }

        [Then(@"the server CPU usage is less than (.*)%")]
        public void ThenTheServerCPUUsageIsLessThan(int maxCpu)
        {
            var serverCpuUsage = GetServerCPUUsage();

            Assert.IsTrue(serverCpuUsage < maxCpu, "Warewolf Server CPU usage: " + serverCpuUsage.ToString(CultureInfo.InvariantCulture));
        }

        [Given(@"I get the server memory")]
        public void GivenIGetTheServerMemory()
        {
            var serverMemory = GetServerMemory();
            Add("BeforeServerMemory", serverMemory);
        }

        [Then(@"the server memory difference is less than (.*) mb")]
        public void ThenTheServerMemoryDifferenceIsLessThanMb(int maxDiff)
        {
            var serverMemoryBefore = Get<string>("BeforeServerMemory");
            var serverMemoryAfter = GetServerMemory();

            var serverMemAfter = Convert.ToDecimal(serverMemoryAfter) / 1024 / 1024;
            var serverMemBefore = Convert.ToDecimal(serverMemoryBefore) / 1024 / 1024;

            var diffInMem = serverMemAfter - serverMemBefore;

            Assert.IsTrue(diffInMem < maxDiff, "Warewolf Server memory usage: " + diffInMem.ToString(CultureInfo.InvariantCulture));
        }

        [Then(@"the '(.*)' in Workflow '(.*)' debug outputs as")]
        public void ThenTheInWorkflowDebugOutputsAs(string toolName, string workflowName, Table table)
        {
            Dictionary<string, Activity> activityList;
            string parentWorkflowName;
            TryGetValue("activityList", out activityList);
            TryGetValue("parentWorkflowName", out parentWorkflowName);

            var debugStates = Get<List<IDebugState>>("debugStates");
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(workflowName)).ID;

            if(parentWorkflowName == workflowName)
            {
                workflowId = Guid.Empty;
            }

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID == workflowId && ds.DisplayName.Equals(toolName)).ToList();

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

        [Given(@"""(.*)"" contains a Foreach ""(.*)"" as ""(.*)"" executions ""(.*)""")]
        public void GivenContainsAForeachAsExecutions(string parentName, string activityName, string numberOfExecutions, string executionCount)
        {
            enForEachType forEachType;
            Enum.TryParse(numberOfExecutions, true, out forEachType);
            var forEach = new DsfForEachActivity { DisplayName = activityName, ForEachType = forEachType };
            switch(forEachType)
            {
                case enForEachType.NumOfExecution:
                    forEach.NumOfExections = executionCount;
                    break;
                case enForEachType.InRecordset:
                    forEach.Recordset = executionCount;
                    break;
            }
            CommonSteps.AddActivityToActivityList(parentName, activityName, forEach);
            ScenarioContext.Current.Add(activityName, forEach);

        }


        [Given(@"""(.*)"" contains workflow ""(.*)"" with mapping as")]
        // ReSharper disable InconsistentNaming
        public void GivenContainsWorkflowWithMappingAs(string forEachName, string nestedWF, Table mappings)
        // ReSharper restore InconsistentNaming
        {
            var forEachAct = (DsfForEachActivity)ScenarioContext.Current[forEachName];
            IEnvironmentModel environmentModel = EnvironmentRepository.Instance.Source;
            environmentModel.Connect();
            environmentModel.LoadResources();
            var resource = environmentModel.ResourceRepository.Find(a => a.Category == @"Sprint12\11714Nested").FirstOrDefault();
            if(resource == null)
            {
                // ReSharper disable NotResolvedInText
                throw new ArgumentNullException("resource");
                // ReSharper restore NotResolvedInText
            }
            var dataMappingViewModel = GetDataMappingViewModel(resource, mappings);

            var inputMapping = dataMappingViewModel.GetInputString(dataMappingViewModel.Inputs);
            var outputMapping = dataMappingViewModel.GetOutputString(dataMappingViewModel.Outputs);
            var activity = new DsfActivity
            {

                ServiceName = resource.Category,
                UniqueID = resource.ID.ToString(),
                InputMapping = inputMapping,
                OutputMapping = outputMapping


            };

            var activityFunction = new ActivityFunc<string, bool> { Handler = activity, DisplayName = nestedWF };
            forEachAct.DataFunc = activityFunction;
            //ScenarioContext.Current.Pending();
        }


        [Given(@"""(.*)"" contains Find Record Index ""(.*)"" into result as ""(.*)""")]
        public void GivenContainsFindRecordIndexIntoResultAs(string parentName, string activityName, string result, Table table)
        {
            DsfFindRecordsMultipleCriteriaActivity act = new DsfFindRecordsMultipleCriteriaActivity { DisplayName = activityName, Result = result };
            foreach(var rule in table.Rows)
            {
                act.ResultsCollection.Add(new FindRecordsTO(rule[4], rule[3], 0));
                act.FieldsToSearch = String.IsNullOrEmpty(act.FieldsToSearch) ? rule[1] : "," + rule[1];
                act.RequireAllFieldsToMatch = rule[5].ToUpper().Trim() == "YES";
                act.RequireAllTrue = rule[6].ToUpper().Trim() == "YES";
            }
            CommonSteps.AddActivityToActivityList(parentName, activityName, act);
        }


        [Given(@"""(.*)"" contains Length ""(.*)"" on ""(.*)"" into '(.*)'")]
        public void GivenContainsLengthOnInto(string parentName, string activityName, string recordSet, string result)
        {
            DsfRecordsetLengthActivity len = new DsfRecordsetLengthActivity { DisplayName = activityName, RecordsLength = result, RecordsetName = recordSet };
            CommonSteps.AddActivityToActivityList(parentName, activityName, len);
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
