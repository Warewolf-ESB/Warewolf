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
    public class ReadItemFromListSteps : RecordSetBases
    {
        [Given(@"I have a sharepoint source to ""(.*)""")]
        public void GivenIHaveASharepointSourceTo(string server)
        {
            if(ScenarioContext.Current.ContainsKey("sharepointServer")) return;
            var source = new SharepointSource { Server = server, ResourceName = "localSharepointServerSource", ResourceID = Guid.NewGuid(), ResourceType = ResourceType.SharepointServerSource };
            ResourceCatalog.Instance.SaveResource(Guid.Empty, source);
            ScenarioContext.Current.Add("sharepointServer",source);
        }

        [Given(@"I select ""(.*)"" list")]
        public void GivenISelectList(string sharepointList)
        {
            if (ScenarioContext.Current.ContainsKey("sharepointList")) return;
            ScenarioContext.Current.Add("sharepointList",sharepointList);
        }

        [Given(@"I map the list fields as")]
        [Then(@"I map the list fields as")]
        public void GivenIMapTheListFieldsAs(Table table)
        {
            var sharepointReadListTos = new List<SharepointReadListTo>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(var row in table.Rows)
            {
                sharepointReadListTos.Add(new SharepointReadListTo(row["Variable"], row["Field Name"],"",""));
            }
           
            ScenarioContext.Current.Add("sharepointReadListTos",sharepointReadListTos);
        }

        [When(@"the sharepoint tool is executed")]
        public void WhenTheSharepointToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"scenerio is clean")]
        public void ThenScenerioIsClean()
        {
            if (ScenarioContext.Current.ContainsKey("sharepointReadListTos"))
            {
                ScenarioContext.Current.Remove("sharepointReadListTos");
            }
            if (ScenarioContext.Current.ContainsKey("result"))
            {
                ScenarioContext.Current.Remove("result");
            }
            if (ScenarioContext.Current.ContainsKey("activity"))
            {
                ScenarioContext.Current.Remove("activity");
            }
        }

        [Given(@"do not require all criteria to match")]
        public void GivenDoNotRequireAllCriteriaToMatch()
        {
            ScenarioContext.Current.Add("requireAllCriteriaToMatch",false);
        }

        [Given(@"search criteria as")]
        public void GivenSearchCriteriaAs(Table table)
        {
            var searchCriteria = new List<SharepointSearchTo>();
            foreach(var row in table.Rows)
            {
                var searchTo = new SharepointSearchTo();
                var fieldName = row["Field Name"];
                var internalName = fieldName;
                if (fieldName == "Name")
                {
                    internalName = "mk7s";
                }
                var searchType = row["Search Type"];
                searchTo.InternalName = internalName;
                searchTo.FieldName = fieldName;
                searchTo.SearchType = searchType;
                var valueMatch = row["Value"];
                if(!string.IsNullOrEmpty(valueMatch))
                {
                    searchTo.ValueToMatch = valueMatch;
                }
                var fromMatch = row["From"];
                if (!string.IsNullOrEmpty(fromMatch))
                {
                    searchTo.From = fromMatch;
                }
                var toMatch = row["To"];
                if (!string.IsNullOrEmpty(toMatch))
                {
                    searchTo.To = toMatch;
                }
                searchCriteria.Add(searchTo);
            }
            ScenarioContext.Current.Add("searchCriteria",searchCriteria);
        }

        protected override void BuildDataList()
        {
            List<SharepointReadListTo> sharepointReadListTos;
            ISharepointSource sharepointServerSource;
            string sharepointList;
            List<SharepointSearchTo> searchCriteria;
            bool requireAllCriteriaToMatch;
            ScenarioContext.Current.TryGetValue("sharepointReadListTos", out sharepointReadListTos);
            ScenarioContext.Current.TryGetValue("sharepointServer", out sharepointServerSource);
            ScenarioContext.Current.TryGetValue("sharepointList", out sharepointList);
            ScenarioContext.Current.TryGetValue("searchCriteria", out searchCriteria);
            if(!ScenarioContext.Current.TryGetValue("requireAllCriteriaToMatch", out requireAllCriteriaToMatch))
            {
                requireAllCriteriaToMatch = true;
            }

            var sharepointReadListActivity = new SharepointReadListActivity
            {
                SharepointServerResourceId = sharepointServerSource.ResourceID,
                SharepointList = sharepointList,
                FilterCriteria = searchCriteria,
                RequireAllCriteriaToMatch = requireAllCriteriaToMatch
            };

            TestStartNode = new FlowStep
            {
                Action = sharepointReadListActivity
            };

            foreach (var field in sharepointReadListTos)
            {
                sharepointReadListActivity.ReadListItems.Add(field);
            }
            ScenarioContext.Current.Add("activity", sharepointReadListActivity);
        }
    }
}
