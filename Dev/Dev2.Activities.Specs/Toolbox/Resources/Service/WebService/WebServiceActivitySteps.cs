using System;
using System.Activities.Statements;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.DataList.Contract;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Activities.XML;
using Moq;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Resources.Service.WebService
{
    [Binding]
    public class WebServiceActivitySteps : RecordSetBases
    {
        [Given(@"I have the Get Cities Webservice")]
        public void GivenIHaveTheGetCitiesWebservice()
        {
            var webSourceXML = XmlResource.Fetch("Google_Address_Lookup");
            var webSource = new WebSource(webSourceXML);
            var webService = new Runtime.ServiceModel.Data.WebService { Source = webSource, RequestUrl = webSource.DefaultQuery };
            ErrorResultTO errors;
            WebServices.ExecuteRequest(webService, false, out errors);
            IOutputDescription outputDescription = webService.GetOutputDescription();
            webService.OutputDescription = outputDescription;
            outputDescription.ToRecordsetList(webService.Recordsets);
            webService.OutputSpecification = webService.GetOutputString(webService.Recordsets);

            ScenarioContext.Current.Add("WebService", webService);
        }

        [When(@"I execute the activity")]
        public void WhenIExecuteTheActivity()
        {
            BuildDataList();
            var dataObject = ScenarioContext.Current.Get<IDSFDataObject>("dataObject");
            IDSFDataObject result = ExecuteProcess(dataObject, true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the result should be ""(.*)""")]
        public void ThenTheResultShouldBe(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        #region Overrides of RecordSetBases

        protected override void BuildDataList()
        {
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            var webService = ScenarioContext.Current.Get<Runtime.ServiceModel.Data.WebService>("WebService");
            webService.ResourceID = Guid.NewGuid();
            webService.ResourceName = "Google Maps Service";
            ResourceCatalog.Instance.SaveResource(Guid.Empty, webService.Source);
            ResourceCatalog.Instance.SaveResource(Guid.Empty, webService);
            var shape = compiler.ShapeDev2DefinitionsToDataList(webService.OutputSpecification, enDev2ArgumentType.Output, false, out errors);

            var dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "", shape, out errors);
            var dataObj = new Mock<IDSFDataObject>();
            dataObj.SetupAllProperties();
            dataObj.Setup(d => d.DataListID).Returns(dataListID);
            dataObj.Setup(d => d.ResourceID).Returns(webService.ResourceID);
            dataObj.Setup(d => d.ServiceName).Returns(webService.ResourceName);
            dataObj.Setup(d => d.ParentThreadID).Returns(9);
            dataObj.Setup(d => d.WorkspaceID).Returns(Guid.Empty);
            dataObj.Setup(d => d.IsDebugMode()).Returns(true);
            //            var serviceExecution = new WebserviceExecution(dataObj.Object, true);
            //
            //            var webSource = webService.Source as WebSource;
            //
            //            Assert.IsNotNull(webSource);
            //            serviceExecution.Service = webService;
            //            serviceExecution.Source = webSource;
            //            serviceExecution.InstanceOutputDefintions = webService.OutputSpecification;

            var webServiceActivity = new DsfWebserviceActivity();
            webServiceActivity.OutputMapping = webService.OutputSpecification;
            ExecutionID = dataListID;
            CurrentDl = shape;
            TestData = "";
            TestStartNode = new FlowStep
            {
                Action = webServiceActivity
            };
            ScenarioContext.Current.Add("dataObject", dataObj.Object);
        }

        #endregion
    }
}
