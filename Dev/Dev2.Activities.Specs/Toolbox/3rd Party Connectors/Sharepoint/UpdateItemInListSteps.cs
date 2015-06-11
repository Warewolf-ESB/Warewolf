using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities.Sharepoint;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common.Interfaces;
using Dev2.TO;
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
                sharepointReadListTos.Add(new SharepointReadListTo(row["Variable"], fieldName, internalName));
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

        protected override void BuildDataList()
        {
            List<SharepointReadListTo> sharepointReadListTos;
            ISharepointSource sharepointServerSource;
            string sharepointList;
            string resultVar;
            ScenarioContext.Current.TryGetValue("sharepointReadListTos", out sharepointReadListTos);
            ScenarioContext.Current.TryGetValue("sharepointServer", out sharepointServerSource);
            ScenarioContext.Current.TryGetValue("sharepointList", out sharepointList);
            ScenarioContext.Current.TryGetValue("resultVar", out resultVar);
            BuildShapeAndTestData();
            var sharepointUpdateListItemActivity = new SharepointUpdateListItemActivity
            {
                SharepointServerResourceId = sharepointServerSource.ResourceID,
                SharepointList = sharepointList,
                UpdateValues = sharepointReadListTos,
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
