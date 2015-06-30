using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities.Sharepoint;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common.Interfaces;
using Dev2.Data.ServiceModel;
using Dev2.TO;
using Microsoft.SharePoint.Client;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox._3rd_Party_Connectors.Sharepoint
{
    [Binding]
    public class UpdateItemInListSteps : RecordSetBases
    {
        [Given(@"I map the list update fields as")]
        public void GivenIMapTheListUpdateFieldsAs(Table table)
        {
            var sharepointReadListTos = new List<SharepointReadListTo>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var row in table.Rows)
            {
                var fieldName = row["Field Name"];
                var internalName = fieldName;
                if (fieldName == "Name")
                {
                    internalName = "mk7s";
                }
                sharepointReadListTos.Add(new SharepointReadListTo(row["Variable"], fieldName, internalName,""));
            }
            ScenarioContext.Current.Add("sharepointReadListTos", sharepointReadListTos);
        }

        [When(@"the sharepoint update list item tool is executed")]
        public void WhenTheSharepointUpdateListItemToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [AfterScenario("sharepoint")]
        public void AfterScenario()
        {
            DeleteAllItems();
        }

        static void DeleteAllItems()
        {
            SharepointSource sharepointServerSource;
            string sharepointList;
            ScenarioContext.Current.TryGetValue("sharepointServer", out sharepointServerSource);
            ScenarioContext.Current.TryGetValue("sharepointList", out sharepointList);
            using(var ctx = sharepointServerSource.CreateSharepointHelper().GetContext())
            {
                var list = ctx.Web.Lists.GetByTitle(sharepointList);
                var listItems = list.GetItems(CamlQuery.CreateAllItemsQuery());
                ctx.Load(listItems);
                ctx.ExecuteQuery();
                var totalListItems = listItems.Count;
                if(totalListItems > 0)
                {
                    for(var counter = totalListItems - 1; counter > -1; counter--)
                    {
                        listItems[counter].DeleteObject();
                        ctx.ExecuteQuery();
                    }
                }
            }
        }

        [Given(@"all items are deleted from the list")]
        public void GivenAllItemsAreDeletedFromTheList()
        {
            DeleteAllItems();
        }


        [Given(@"I create the follwing items in the list")]
        [Then(@"I create the follwing items in the list")]
        public void GivenICreateItemsInTheList(Table table)
        {
            SharepointSource sharepointServerSource;
            string sharepointList;
            ScenarioContext.Current.TryGetValue("sharepointServer", out sharepointServerSource);
            ScenarioContext.Current.TryGetValue("sharepointList", out sharepointList);
            GivenIMapTheListUpdateFieldsAs(table);
        }

        protected override void BuildDataList()
        {
            List<SharepointReadListTo> sharepointReadListTos;
            ISharepointSource sharepointServerSource;
            string sharepointList;
            string resultVar;
            List<SharepointSearchTo> searchCriteria;
            bool requireAllCriteriaToMatch;
            ScenarioContext.Current.TryGetValue("sharepointReadListTos", out sharepointReadListTos);
            ScenarioContext.Current.TryGetValue("sharepointServer", out sharepointServerSource);
            ScenarioContext.Current.TryGetValue("sharepointList", out sharepointList);
            ScenarioContext.Current.TryGetValue("resultVar", out resultVar);
            ScenarioContext.Current.TryGetValue("searchCriteria", out searchCriteria);
            if (!ScenarioContext.Current.TryGetValue("requireAllCriteriaToMatch", out requireAllCriteriaToMatch))
            {
                requireAllCriteriaToMatch = true;
            }
            BuildShapeAndTestData();
            var sharepointUpdateListItemActivity = new SharepointUpdateListItemActivity
            {
                SharepointServerResourceId = sharepointServerSource.ResourceID,
                SharepointList = sharepointList,
                ReadListItems = sharepointReadListTos,
                RequireAllCriteriaToMatch = requireAllCriteriaToMatch,
                FilterCriteria = searchCriteria,
                Result = resultVar
            };

            TestStartNode = new FlowStep
            {
                Action = sharepointUpdateListItemActivity
            };

            ScenarioContext.Current.Add("activity", sharepointUpdateListItemActivity);
        }
    }
}
