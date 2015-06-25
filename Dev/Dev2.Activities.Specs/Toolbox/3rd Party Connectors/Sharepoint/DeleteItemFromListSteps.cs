using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities.Sharepoint;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Hosting;
using Dev2.TO;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox._3rd_Party_Connectors.Sharepoint
{
    [Binding]
    public class DeleteItemFromListSteps : RecordSetBases
    {
        [BeforeScenario]
        public void Setup()
        {
            /*
            // ensure that the test data is correct and available since we are going to be deleting
            string server = "http://rsaklfsvrsharep/",
                sharepointList = "AcceptanceTesting";
            Guid sharepointServerId = Guid.NewGuid();
            var source = new SharepointSource { Server = server, ResourceName = "localSharepointServerSource", ResourceID = sharepointServerId, ResourceType = ResourceType.SharepointServerSource };
            ResourceCatalog.Instance.SaveResource(sharepointServerId, source);
            var readTool = new SharepointReadListActivity
            {
                SharepointServerResourceId = source.ResourceID,
                SharepointList = sharepointList,
                RequireAllCriteriaToMatch = true
            };
            readTool.Execute()
             * */
        }
        [AfterScenario]
        public void CleanUp()
        {
        }
        [When(@"the sharepoint delete item from list tool is executed")]
        public void WhenTheSharepointDeleteItemFromListToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            if (ScenarioContext.Current.ContainsKey("result"))
                ScenarioContext.Current.Remove("result");
            ScenarioContext.Current.Add("result", result);
        }
        [Then(@"clear the activity")]
        public void ThenClearTheActivity()
        {
            WhenTheActivityIsCleared();
            if (ScenarioContext.Current.ContainsKey("resultVar"))
                ScenarioContext.Current.Remove("resultVar");
        }

        [When(@"the activity is cleared")]
        public void WhenTheActivityIsCleared()
        {
            if (ScenarioContext.Current.ContainsKey("result"))
                ScenarioContext.Current.Remove("result");

            if (ScenarioContext.Current.ContainsKey("activity"))
                ScenarioContext.Current.Remove("activity");
        }


        protected override void BuildDataList()
        {
            ISharepointSource sharepointServerSource;
            string sharepointList;
            List<SharepointSearchTo> searchCriteria;
            string resultVar;
            bool requireAllCriteriaToMatch;
            ScenarioContext.Current.TryGetValue("sharepointServer", out sharepointServerSource);
            ScenarioContext.Current.TryGetValue("sharepointList", out sharepointList);
            ScenarioContext.Current.TryGetValue("searchCriteria", out searchCriteria);
            ScenarioContext.Current.TryGetValue("resultVar", out resultVar);
            if (!ScenarioContext.Current.TryGetValue("requireAllCriteriaToMatch", out requireAllCriteriaToMatch))
            {
                requireAllCriteriaToMatch = true;
            }

            var deleteTool = new SharepointDeleteListItemActivity
            {
                SharepointServerResourceId = sharepointServerSource.ResourceID,
                SharepointList = sharepointList,
                FilterCriteria = searchCriteria,
                RequireAllCriteriaToMatch = requireAllCriteriaToMatch,
                DeleteCount = resultVar
            };

            TestStartNode = new FlowStep
            {
                Action = deleteTool
            };

            if (ScenarioContext.Current.ContainsKey("activity"))
                ScenarioContext.Current.Remove("activity");
            ScenarioContext.Current.Add("activity", deleteTool);
        }
    }
}
