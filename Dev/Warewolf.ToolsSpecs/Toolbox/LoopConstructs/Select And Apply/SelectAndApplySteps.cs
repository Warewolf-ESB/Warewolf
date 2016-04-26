using Dev2;
using Dev2.Activities;
using Dev2.Activities.SelectAndApply;
using Dev2.Activities.Specs.Toolbox.LoopConstructs.ForEach;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Warewolf.ToolsSpecs.Toolbox.LoopConstructs.Select_And_Apply
{
    [Binding]
    public class SelectAndApplySteps : RecordSetBases
    {
        [Given(@"There is a complexobject in the datalist with this shape")]
        public void GivenThereIsAComplexobjectInTheDatalistWithThisShape(Table table)
        {
            List<TableRow> rows = table.Rows.ToList();

            if (rows.Count == 0)
            {
                var obj = table.Header.ToArray()[0];
                var field = table.Header.ToArray()[1];

                List<Tuple<string, string>> emptyRecordset;

                bool isAdded = ScenarioContext.Current.TryGetValue("obj", out emptyRecordset);
                if (!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("obj", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(obj, field));
            }

            foreach (TableRow tableRow in rows)
            {
                List<Tuple<string, string>> objList;
                ScenarioContext.Current.TryGetValue("objList", out objList);

                if (objList == null)
                {
                    objList = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("objList", objList);
                }
                objList.Add(new Tuple<string, string>(tableRow[0], tableRow[1]));
            }
        }

        [Then(@"the selectAndApply executes (.*) times")]
        public void ThenTheSelectAndApplyExecutesTimes(int numOfIterations)
        {
            var act = ScenarioContext.Current.Get<SelectAndApplyTestTool>("innerActivity");
            Assert.AreEqual(act.Called, numOfIterations);
        }

        [Given(@"Alias is ""(.*)""")]
        public void GivenAliasIs(string alias)
        {
            ScenarioContext.Current["alias"] = alias;
        }

        [Given(@"Datasource is ""(.*)""")]
        public void GivenDatasourceIs(string datasource)
        {
            ScenarioContext.Current["datasource"] = datasource;
        }

        [Given(@"the underlying dropped activity is a mocked test tool")]
        public void GivenTheUnderlyingDroppedActivityIsAMockedTestTool()
        {
            SelectAndApplyTestTool innerActivity = new SelectAndApplyTestTool();
            ScenarioContext.Current.Add("innerActivity", innerActivity);
        }

        [Given(@"I use a Number Format tool configured as")]
        public void GivenIUseANumberFormatToolConfiguredAs(Table table)
        {
            var activity = new DsfNumberFormatActivity();
            activity.Expression = table.Rows[0]["Number"];
            activity.DecimalPlacesToShow = table.Rows[0]["Decimals to show"];
            activity.RoundingType = table.Rows[0]["Rounding"];
            activity.RoundingDecimalPlaces = table.Rows[0]["Rounding Value"];
            activity.Result = table.Rows[0]["Result"];

            ScenarioContext.Current.Add("innerActivity", activity);
        }

        [When(@"the selectAndApply tool is executed")]
        public void WhenTheSelectAndApplyToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false, channel: new mockEsb());
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"""(.*)"" has a value of ""(.*)""")]
        public void ThenHasAValueOf(string item, string value)
        {
            var warewolfEvalResult = DataObject.Environment.Eval(item, 0);
            if (warewolfEvalResult.IsWarewolfAtomResult)
            {
                var result = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                if (result != null)
                {
                    Assert.AreEqual(value, result.Item.ToString());
                }
            }

            if (warewolfEvalResult.IsWarewolfAtomListresult)
            {
                var result = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                if (result != null && result.Item.Count == 1)
                {
                    var warewolfAtom = result.Item[0];
                    Assert.AreEqual(value, warewolfAtom.ToString());
                }
            }
        }

        private const string ResultRecordsetVariable = "[[r().v]]";

        protected override void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            BuildShapeAndTestData();
            string recordSet;
            if (!ScenarioContext.Current.TryGetValue("datasource", out recordSet))
            {
                recordSet = string.Empty;
            }

            var selectAndApplyTool = new DsfSelectAndApplyActivity();
            var innerActivity = ScenarioContext.Current.Get<Activity>("innerActivity");
            var alias = ScenarioContext.Current.Get<string>("alias");

            selectAndApplyTool.DataSource = recordSet;
            selectAndApplyTool.Alias = alias;
            selectAndApplyTool.ApplyActivityFunc.Handler = innerActivity;
            TestStartNode = new FlowStep
            {
                Action = selectAndApplyTool
            };

            ScenarioContext.Current.Add("activity", selectAndApplyTool);
        }
    }

    internal class SelectAndApplyTestTool : DsfNativeActivity<string>
    {
        public SelectAndApplyTestTool()
            : base(false, "")
        {
            DisplayName = "SelectAndApplyTestTool";
            Called = 0;
        }

        public int Called { get; private set; }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            Called++;
        }

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
            return new List<DsfForEachItem>();
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return new List<DsfForEachItem>();
        }
    }
}