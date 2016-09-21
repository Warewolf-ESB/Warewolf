using Dev2;
using Dev2.Activities;
using Dev2.Activities.SelectAndApply;
using Dev2.Activities.Specs.Toolbox.LoopConstructs.ForEach;
using Dev2.Common.Interfaces;
using Dev2.Data.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Interfaces;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Warewolf.ToolsSpecs.Toolbox.LoopConstructs.Select_And_Apply
{
    [Binding]
    public class SelectAndApplySteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public SelectAndApplySteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        [Given(@"There is a complexobject in the datalist with this shape")]
        public void GivenThereIsAComplexobjectInTheDatalistWithThisShape(Table table)
        {
            List<TableRow> rows = table.Rows.ToList();

            if (rows.Count == 0)
            {
                var obj = table.Header.ToArray()[0];
                var field = table.Header.ToArray()[1];

                List<Tuple<string, string>> emptyRecordset;

                bool isAdded = scenarioContext.TryGetValue("obj", out emptyRecordset);
                if (!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                    scenarioContext.Add("obj", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(obj, field));
            }

            foreach (TableRow tableRow in rows)
            {
                List<Tuple<string, string>> objList;
                scenarioContext.TryGetValue("objList", out objList);

                if (objList == null)
                {
                    objList = new List<Tuple<string, string>>();
                    scenarioContext.Add("objList", objList);
                }
                objList.Add(new Tuple<string, string>(tableRow[0], tableRow[1]));
            }
        }

        [Then(@"the selectAndApply executes (.*) times")]
        public void ThenTheSelectAndApplyExecutesTimes(int numOfIterations)
        {
            var act = scenarioContext.Get<SelectAndApplyTestTool>("innerActivity");
            Assert.AreEqual(act.Called, numOfIterations);
        }

        [Given(@"Alias is ""(.*)""")]
        public void GivenAliasIs(string alias)
        {
            scenarioContext["alias"] = alias;
        }

        [Given(@"Datasource is ""(.*)""")]
        public void GivenDatasourceIs(string datasource)
        {
            scenarioContext["datasource"] = datasource;
        }

        [Given(@"the underlying dropped activity is a\(n\) ""(.*)"" tool")]
        public void GivenTheUnderlyingDroppedActivityIsANTool(string tool)
        {
            IDev2Activity innerActivity = null;
            switch (tool)
            {
                case "SelectAndApplyTestTool":
                    innerActivity = new SelectAndApplyTestTool();
                    
                    break;
                case "Activity":
                    innerActivity = new DsfActivity
                    {
                        InputMapping = BuildInputMappings(),
                        OutputMapping = BuildOutputMappings(),
                        ServiceName = "SpecflowForeachActivityTest"
                    };
                    break;
            }
            scenarioContext.Add("innerActivity", innerActivity);
        }

        [Given(@"I use a Number Format tool configured as")]
        public void GivenIUseANumberFormatToolConfiguredAs(Table table)
        {
            DsfNumberFormatActivity numberFormatActivity = new DsfNumberFormatActivity();
            numberFormatActivity.Expression = table.Rows[0]["Number"];
            numberFormatActivity.DecimalPlacesToShow = table.Rows[0]["Decimals to show"];
            numberFormatActivity.RoundingType = table.Rows[0]["Rounding"];
            numberFormatActivity.RoundingDecimalPlaces = table.Rows[0]["Rounding Value"];
            numberFormatActivity.Result = table.Rows[0]["Result"];

            scenarioContext.Add("innerActivity", numberFormatActivity);
        }

        [When(@"the selectAndApply tool is executed")]
        public void WhenTheSelectAndApplyToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false, channel: new mockEsb(scenarioContext));
            scenarioContext.Add("result", result);
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
            else if (warewolfEvalResult.IsWarewolfAtomListresult)
            {
                var result = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                if (result != null && result.Item.Count == 1)
                {
                    var warewolfAtom = result.Item[0];
                    Assert.AreEqual(value, warewolfAtom.ToString());
                }
                else
                {
                    Assert.Fail("Result not matched");
                }
            }
            else
            {
                Assert.Fail("Result not matched");
            }
        }

        [Given(@"I have selected the selectAndApply type as ""(.*)"" and used ""(.*)""")]
        public void GivenIHaveSelectedTheSelectAndApplyTypeAsAndUsed(string foreachType, string recordSet)
        {
            var forEachType = (enForEachType)Enum.Parse(typeof(enForEachType), foreachType);
            scenarioContext.Add("foreachType", forEachType);
            switch (forEachType)
            {
                case enForEachType.InRecordset:
                    scenarioContext.Add("recordset", recordSet);
                    break;
            }
        }
        
        protected override void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            BuildShapeAndTestData();
            string datasource;
            if (!scenarioContext.TryGetValue("datasource", out datasource))
            {
                datasource = string.Empty;
            }

            string alias;
            if (!scenarioContext.TryGetValue("alias", out alias))
            {
                alias = string.Empty;
            }

            Activity innerActivity = scenarioContext.Get<Activity>("innerActivity");

            DsfSelectAndApplyActivity selectAndApplyTool = new DsfSelectAndApplyActivity();
            selectAndApplyTool.DataSource = datasource;
            selectAndApplyTool.Alias = alias;
            selectAndApplyTool.ApplyActivityFunc.Handler = innerActivity;
            TestStartNode = new FlowStep
            {
                Action = selectAndApplyTool
            };

            scenarioContext.Add("activity", selectAndApplyTool);
        }

        private string BuildInputMappings()
        {
            var inputMappings = new StringBuilder();
            inputMappings.Append("<Inputs>");

            var inMapTo = scenarioContext.Get<string>("inMapTo");
            string inRecordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, inMapTo);
            string inColumn = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, inMapTo);

            var inMapFrom = scenarioContext.Get<string>("inMapFrom");
            inputMappings.Append(string.Format("<Input Name=\"{0}\" Source=\"{1}\" Recordset=\"{2}\"/>", inColumn,
                                               inMapFrom, inRecordset));

            inputMappings.Append("</Inputs>");
            return inputMappings.ToString();
        }

        private string BuildOutputMappings()
        {
            var outputMappings = new StringBuilder();
            outputMappings.Append("<Outputs>");

            var outMapFrom = scenarioContext.Get<string>("outMapFrom");
            string inRecordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, outMapFrom);
            string inColumn = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, outMapFrom);

            var outMapTo = scenarioContext.Get<string>("outMapTo");
            outputMappings.Append(string.Format(
                "<Output Name=\"{0}\" MapsTo=\"{1}\" Value=\"{1}\" Recordset=\"{2}\"/>", inColumn,
                outMapTo, inRecordset));

            outputMappings.Append("</Outputs>");
            return outputMappings.ToString();
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

        public override List<string> GetOutputs()
        {
            return new List<string>();
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
