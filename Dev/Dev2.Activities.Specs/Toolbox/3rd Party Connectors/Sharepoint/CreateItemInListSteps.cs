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
    public class CreateItemInListSteps : RecordSetBases
    {
        [Given(@"I map the list input fields as")]
        [Then(@"I map the list input fields as")]
        public void GivenIMapTheListInputFieldsAs(Table table)
        {
            var sharepointReadListTos = new List<SharepointReadListTo>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var row in table.Rows)
            {
                var fieldName = row["Field Name"];
                var internalName = fieldName;
                if(fieldName == "Name")
                {
                    internalName = "mk7s";
                }
                sharepointReadListTos.Add(new SharepointReadListTo(row["Variable"], fieldName, internalName,""));
            }
            if (ScenarioContext.Current.ContainsKey("sharepointReadListTos"))
            {
                ScenarioContext.Current.Remove("sharepointReadListTos");
            }
            ScenarioContext.Current.Add("sharepointReadListTos", sharepointReadListTos);
        }

        [Given(@"I have result variable as ""(.*)""")]
        public void GivenIHaveResultVariableAs(string resultVar)
        {
            ScenarioContext.Current.Add("resultVar",resultVar);
        }

        [When(@"the sharepoint create list item tool is executed")]
        public void WhenTheSharepointCreateListItemToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            if(ScenarioContext.Current.ContainsKey("result"))
            {
                ScenarioContext.Current.Remove("result");
            }
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
            var sharepointReadListActivity = new SharepointCreateListItemActivity
            {
                SharepointServerResourceId = sharepointServerSource.ResourceID,
                SharepointList = sharepointList,
                ReadListItems = sharepointReadListTos,
                Result = resultVar
            };

            TestStartNode = new FlowStep
            {
                Action = sharepointReadListActivity
            };
            if(ScenarioContext.Current.ContainsKey("activity"))
            {
                ScenarioContext.Current.Remove("activity");
            }
            ScenarioContext.Current.Add("activity", sharepointReadListActivity);
        }
    }
}
