using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Services;
using Dev2.Session;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Network;
using Dev2.Threading;
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
            var resourceModel = new ResourceModel(environmentModel);
            resourceModel.Category = "Acceptance Tests";
            resourceModel.ResourceName = workflowName;
            resourceModel.ID = Guid.NewGuid();
            resourceModel.ResourceType = ResourceType.WorkflowService;
            ResourceRepository repository = new ResourceRepository(environmentModel);
            repository.Add(resourceModel);
            _debugWriterSubscriptionService = new SubscriptionService<DebugWriterWriteMessage>(environmentModel.Connection.ServerEvents);

            _debugWriterSubscriptionService.Subscribe(msg => Append(msg.DebugState));
            ScenarioContext.Current.Add(workflowName, resourceModel);
            ScenarioContext.Current.Add("environment", environmentModel);
            ScenarioContext.Current.Add("resourceRepo", repository);
            ScenarioContext.Current.Add("debugStates", new List<IDebugState>());
        }

        void Append(IDebugState debugState)
        {
            List<IDebugState> debugStates;
            ScenarioContext.Current.TryGetValue("debugStates", out debugStates);

            debugStates.Add(debugState);
            if(debugState.IsFinalStep())
                _resetEvt.Set();

        }

        [Given(@"""(.*)"" contains a ""(.*)"" service ""(.*)"" with mappings")]
        public void GivenContainsADatabaseServiceWithMappings(string wf, string serviceType, string serviceName, Table table)
        {
            IEnvironmentModel environmentModel = EnvironmentRepository.Instance.Source;
            ResourceRepository repository = new ResourceRepository(environmentModel);
            repository.Load();
            var resource = repository.Find(r => r.ResourceName.Equals(serviceName)).ToList();

            var outputSb = GetMapping(table, resource);
            var outputMapping = outputSb.ToString();
            resource[0].Outputs = outputMapping;

            var activity = GetServiceActivity(serviceType);
            if(activity != null)
            {
                activity.ResourceID = resource[0].ID;
                activity.ServiceName = resource[0].ResourceName;
                activity.DisplayName = serviceName;
                activity.OutputMapping = outputMapping;
                CommonSteps.AddActivityToActivityList(serviceName, activity);
            }
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
                case "web":
                    activity = new DsfWebserviceActivity();
                    break;
                case "workflow":
                    activity = new DsfWorkflowActivity();
                    break;
            }
            return activity;
        }

        static StringBuilder GetMapping(Table table, List<IResourceModel> resource)
        {
            var outputSb = new StringBuilder();
            outputSb.Append("<Outputs>");

            foreach(var tableRow in table.Rows)
            {
                var input = tableRow["Input to Service"];
                var fromVariable = tableRow["From Variable"];
                var output = tableRow["Output from Service"];
                var toVariable = tableRow["To Variable"];

               // CommonSteps.AddVariableToVariableList(output);
              //  CommonSteps.AddVariableToVariableList(fromVariable);
               // CommonSteps.AddVariableToVariableList(input);
                CommonSteps.AddVariableToVariableList(toVariable);

                if(resource.Count > 0)
                {
                    var outputs = XDocument.Parse(resource[0].Outputs);

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
                    var flowStep = new FlowStep();
                    flowStep.Action = activity.Value;
                    flowSteps.Last().Next = flowStep;
                    flowSteps.Add(flowStep);
                }
            }

            IContextualResourceModel resourceModel;
            IEnvironmentModel environmentModel;
            IResourceRepository repository;
            ScenarioContext.Current.TryGetValue(workflowName, out resourceModel);
            ScenarioContext.Current.TryGetValue("environment", out environmentModel);
            ScenarioContext.Current.TryGetValue("resourceRepo", out repository);

            string currentDl = CurrentDl;
            resourceModel.DataList = currentDl.Replace("root", "DataList");
            WorkflowHelper helper = new WorkflowHelper();
            StringBuilder xamlDefinition = helper.GetXamlDefinition(FlowchartActivityBuilder);
            resourceModel.WorkflowXaml = xamlDefinition;

            repository.Save(resourceModel);
            repository.SaveToServer(resourceModel);

            ExecuteWorkflow(resourceModel);
        }

        [Then(@"the '(.*)' in WorkFlow '(.*)' debug inputs as")]
        public void ThenTheInWorkFlowDebugInputsAs(string toolName, string workflowName, Table table)
        {
            Dictionary<string, Activity> activityList;
            ScenarioContext.Current.TryGetValue("activityList", out activityList);

            var debugStates = ScenarioContext.Current.Get<List<IDebugState>>("debugStates");

            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(workflowName)).ID;

            var toolSpecificDebug =
                debugStates.Where(ds => ds.OriginalInstanceID == workflowId && ds.DisplayName.Equals(toolName)).ToList();

            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugInputsAs(table, toolSpecificDebug
                                                    .SelectMany(s => s.Inputs)
                                                    .SelectMany(s => s.ResultsList).ToList());
        }

        [Then(@"the '(.*)' in Workflow '(.*)' debug outputs as")]
        public void ThenTheInWorkflowDebugOutputsAs(string toolName, string workflowName, Table table)
        {
            Dictionary<string, Activity> activityList;
            ScenarioContext.Current.TryGetValue("activityList", out activityList);

            var debugStates = ScenarioContext.Current.Get<List<IDebugState>>("debugStates");
            var workflowId = debugStates.First(wf => wf.DisplayName.Equals(workflowName)).ID;

            var toolSpecificDebug =
                debugStates.Where(ds => ds.OriginalInstanceID == workflowId && ds.DisplayName.Equals(toolName)).ToList();

            var commonSteps = new CommonSteps();
            commonSteps.ThenTheDebugOutputAs(table, toolSpecificDebug
                                                    .SelectMany(s => s.Outputs)
                                                    .SelectMany(s => s.ResultsList).ToList());
        }

        public void ExecuteWorkflow(IContextualResourceModel resourceModel)
        {
            if(resourceModel == null || resourceModel.Environment == null)
            {
                return;
            }

            var debugTO = new DebugTO();
            debugTO.XmlData = "<DataList></DataList>";
            debugTO.SessionID = Guid.NewGuid();
            debugTO.IsDebugMode = true;

            var clientContext = resourceModel.Environment.Connection;
            if(clientContext != null)
            {
                var dataList = XElement.Parse(debugTO.XmlData);
                dataList.Add(new XElement("BDSDebugMode", debugTO.IsDebugMode));
                dataList.Add(new XElement("DebugSessionID", debugTO.SessionID));
                dataList.Add(new XElement("EnvironmentID", resourceModel.Environment.ID));
                WebServer.Send(WebServerMethod.POST, resourceModel, dataList.ToString(), new TestAsyncWorker());
                _resetEvt.WaitOne();
            }
        }
        [AfterScenario]
        public void AfterScenario()
        {
            _debugWriterSubscriptionService.Unsubscribe();
            _debugWriterSubscriptionService.Dispose();
            _resetEvt.Close();            
        }

    }
}
