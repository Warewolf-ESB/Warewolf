using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common.Interfaces;
using Dev2.TO;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox._3rd_Party_Connectors.Sharepoint
{
    [Binding]
    public class CreateItemInListSteps : RecordSetBases
    {
        [Given(@"I map the list input fields as")]
        public void GivenIMapTheListInputFieldsAs(Table table)
        {
            var sharepointReadListTos = new List<SharepointReadListTo>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var row in table.Rows)
            {
                sharepointReadListTos.Add(new SharepointReadListTo(row["Variable"], row["Field Name"]));
            }
            ScenarioContext.Current.Add("sharepointReadListTos", sharepointReadListTos);
        }

        [Given(@"I have result variable as ""(.*)""")]
        public void GivenIHaveResultVariableAs(string resultVar)
        {
            ScenarioContext.Current.Add("resultVar",resultVar);
        }

        [Given(@"I have a variable ""(.*)"" with value ""(.*)""")]
        public void GivenIHaveAVariableWithValue(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value));
        }

        protected override void BuildDataList()
        {
            List<SharepointReadListTo> sharepointReadListTos;
            ISharepointSource sharepointServerSource;
            string sharepointList;
            List<SharepointSearchTo> searchCriteria;
            string resultVar;
            ScenarioContext.Current.TryGetValue("sharepointReadListTos", out sharepointReadListTos);
            ScenarioContext.Current.TryGetValue("sharepointServer", out sharepointServerSource);
            ScenarioContext.Current.TryGetValue("sharepointList", out sharepointList);
            ScenarioContext.Current.TryGetValue("searchCriteria", out searchCriteria);
            ScenarioContext.Current.TryGetValue("resultVar", out resultVar);
            BuildShapeAndTestData();
            var sharepointReadListActivity = new SharepointCreateListItemActivity
            {
                SharepointServerResourceId = sharepointServerSource.ResourceID,
                SharepointList = sharepointList,
                FieldValues = sharepointReadListTos,
                Result = resultVar
            };

            TestStartNode = new FlowStep
            {
                Action = sharepointReadListActivity
            };

            foreach (var field in sharepointReadListTos)
            {
                sharepointReadListActivity.FieldValues.Add(field);
            }
            ScenarioContext.Current.Add("activity", sharepointReadListActivity);
        }
    }

    public class SharepointCreateListItemActivity : DsfActivityAbstract<string>
    {
        public SharepointCreateListItemActivity()
        {
            DisplayName = "Sharepoint Create List Item";
            FieldValues = new List<SharepointReadListTo>();
        }

        public new string Result { get; set; }
        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return null;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return null;
        }

        protected override void ExecuteTool(IDSFDataObject dataObject)
        {
        }

        public Guid SharepointServerResourceId { get; set; }
        public string SharepointList { get; set; }
        public List<SharepointReadListTo> FieldValues { get; set; }
    }
}
