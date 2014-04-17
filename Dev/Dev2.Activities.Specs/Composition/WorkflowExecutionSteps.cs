using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Dev2.Activities.Specs.BaseTypes;
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
        protected override void BuildDataList()
        {
            BuildShapeAndTestData();
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
            var debugWriterSubscriptionService = new SubscriptionService<DebugWriterWriteMessage>(environmentModel.Connection.ServerEvents);
            debugWriterSubscriptionService.Subscribe(msg => Append(msg.DebugState));
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
        }

        [Given(@"workflow ""(.*)"" contains an Assign ""(.*)"" as")]
        public void GivenWorkflowContainsAnAssignAs(string workflowName, string activityName, Table table)
        {

            DsfMultiAssignActivity assignActivity = new DsfMultiAssignActivity { DisplayName = activityName };

            foreach(var tableRow in table.Rows)
            {
                var value = tableRow["value"];
                var variable = tableRow["variable"];

                value = value.Replace('"', ' ').Trim();

                if(value.StartsWith("="))
                {
                    value = value.Replace("=", "");
                    value = string.Format("!~calculation~!{0}!~~calculation~!", value);
                }

                List<ActivityDTO> fieldCollection;
                ScenarioContext.Current.TryGetValue("fieldCollection", out fieldCollection);

                CommonSteps.AddVariableToVariableList(variable);

                assignActivity.FieldsCollection.Add(new ActivityDTO(variable, value, 1, true));
            }
            CommonSteps.AddActivityToActivityList(activityName, assignActivity);
        }

        [When(@"""(.*)"" is executed")]
        public void WhenIsExecuted(string workflowName)
        {
            BuildDataList();

            var activityList = CommonSteps.GetActivityList();
            TestStartNode = new FlowStep();
            foreach(var activity in activityList)
            {
                if(TestStartNode.Action == null)
                {
                    TestStartNode.Action = activity.Value;
                }
            }

            IContextualResourceModel resourceModel;
            IEnvironmentModel environmentModel;
            IResourceRepository repository;
            ScenarioContext.Current.TryGetValue(workflowName, out resourceModel);
            ScenarioContext.Current.TryGetValue("environment", out environmentModel);
            ScenarioContext.Current.TryGetValue("resourceRepo", out repository);

            resourceModel.DataList = CurrentDl;
            WorkflowHelper helper = new WorkflowHelper();
            StringBuilder xamlDefinition = helper.GetXamlDefinition(FlowchartActivityBuilder);
            resourceModel.WorkflowXaml = xamlDefinition;

            repository.SaveToServer(resourceModel);

            ExecuteWorkflow(resourceModel);
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
                //
                dataList.Add(new XElement("BDSDebugMode", debugTO.IsDebugMode));
                dataList.Add(new XElement("DebugSessionID", debugTO.SessionID));
                dataList.Add(new XElement("EnvironmentID", resourceModel.Environment.ID));
                WebServer.Send(WebServerMethod.POST, resourceModel, dataList.ToString(), new TestAsyncWorker());
            }
        }
    }
}
