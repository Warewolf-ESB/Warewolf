using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.DataList.Contract;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Activities.XML;
using Moq;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Resources.Workflow
{
    [Binding]
    public class WorkflowExecutionSteps : RecordSetBases
    {
        [Given(@"I have a ""(.*)"" workflow")]
        public void GivenIHaveAWorkflow(string workflowName)
        {
            var workflowXML = XmlResource.Fetch(workflowName);
            var workflow = new Resource(workflowXML);
            ScenarioContext.Current.Add("Workflow", workflow);
        }

        [Given(@"the input variable ""(.*)"" as ""(.*)""")]
        public void GivenTheInputVariableAs(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            if(!ScenarioContext.Current.TryGetValue("variableList", out variableList))
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value));

            var inputMapping = string.Format("<Inputs><Input Name=\"a\" Source=\"{0}\" /></Inputs>", variable);
            ScenarioContext.Current.Add("inputMapping", inputMapping);

        }

        [Given(@"output variable is ""(.*)""")]
        public void GivenOutputVariableIs(string variable)
        {
            List<Tuple<string, string>> variableList;
            if(!ScenarioContext.Current.TryGetValue("variableList", out variableList))
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, ""));
            var outputMapping = string.Format("<Outputs><Output Name=\"b\" MapsTo=\"{0}\" Value=\"[[b]]\" /></Outputs>", variable);
            ScenarioContext.Current.Add("outputMapping", outputMapping);
        }

        [When(@"I execute the workflow")]
        public void WhenIExecuteTheWorkflow()
        {
            BuildDataList();
            var dataObject = ScenarioContext.Current.Get<IDSFDataObject>("dataObject");
            var esbChannel = ScenarioContext.Current.Get<EsbServicesEndpoint>("esbChannel");
            IDSFDataObject result = ExecuteProcess(dataObject, true, esbChannel, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the output variable be ""(.*)""")]
        public void ThenTheOutputVariableBe(string expectedResult)
        {
            ScenarioContext.Current.Add("expectedResult", expectedResult);
        }


        #region Overrides of RecordSetBases

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            var resource = ScenarioContext.Current.Get<Resource>("Workflow");
            ResourceCatalog.Instance.SaveResource(Guid.Empty, XmlResource.Fetch(resource.ResourceName).ToStringBuilder());
            Guid dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), TestData, CurrentDl, out errors);

            //var dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "", shape, out errors);
            var dataObj = new Mock<IDSFDataObject>();
            dataObj.SetupAllProperties();
            dataObj.Setup(d => d.DataListID).Returns(dataListID);
            dataObj.Setup(d => d.ResourceID).Returns(resource.ResourceID);
            dataObj.Setup(d => d.ServiceName).Returns(resource.ResourceName);
            dataObj.Setup(d => d.ParentThreadID).Returns(9);
            dataObj.Setup(d => d.WorkspaceID).Returns(Guid.Empty);
            dataObj.Setup(d => d.IsDebugMode()).Returns(true);

            var esbChannel = new EsbServicesEndpoint();
            ScenarioContext.Current.Add("esbChannel", esbChannel);
            ExecutionID = dataListID;
            var workflowActivity = new DsfActivity();
            workflowActivity.ResourceID = resource.ResourceID;
            workflowActivity.ServiceName = resource.ResourceName;
            workflowActivity.OutputMapping = ScenarioContext.Current.Get<string>("outputMapping");
            workflowActivity.InputMapping = ScenarioContext.Current.Get<string>("inputMapping");
            ScenarioContext.Current.Add("activity", workflowActivity);
            TestStartNode = new FlowStep
            {
                Action = workflowActivity
            };
            ScenarioContext.Current.Add("dataObject", dataObj.Object);
        }

        #endregion
    }
}
