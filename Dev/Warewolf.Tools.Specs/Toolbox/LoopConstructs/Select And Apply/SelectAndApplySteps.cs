#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using Dev2;
using Dev2.Activities;
using Dev2.Activities.SelectAndApply;
using Dev2.Activities.Specs.Toolbox.LoopConstructs.ForEach;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Data.Interfaces.Enums;
using Dev2.Interfaces;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;
using Dev2.Common.State;

namespace Warewolf.ToolsSpecs.Toolbox.LoopConstructs.Select_And_Apply
{
    [Binding]
    public class SelectAndApplySteps : RecordSetBases
    {
        readonly ScenarioContext scenarioContext;

        public SelectAndApplySteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this.scenarioContext = scenarioContext;
        }

        [Given(@"There is a complexobject in the datalist with this shape")]
        public void GivenThereIsAComplexobjectInTheDatalistWithThisShape(Table table)
        {
            var rows = table.Rows.ToList();

            if (rows.Count == 0)
            {
                var obj = table.Header.ToArray()[0];
                var field = table.Header.ToArray()[1];


                var isAdded = scenarioContext.TryGetValue("obj", out List<Tuple<string, string>> emptyRecordset);
                if (!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                    scenarioContext.Add("obj", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(obj, field));
            }

            foreach (TableRow tableRow in rows)
            {
                scenarioContext.TryGetValue("objList", out List<Tuple<string, string>> objList);

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
                default:
                    break;
            }
            scenarioContext.Add("innerActivity", innerActivity);
        }

        [Given(@"I use a Number Format tool configured as")]
        public void GivenIUseANumberFormatToolConfiguredAs(Table table)
        {
            var numberFormatActivity = new DsfNumberFormatActivity();
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
            var result = ExecuteProcess(isDebug: true, throwException: false, channel: new mockEsb(scenarioContext));
            scenarioContext.Add("result", result);
        }

        [Then(@"""(.*)"" has a value of ""(.*)""")]
        public void ThenHasAValueOf(string item, string value)
        {
            var warewolfEvalResult = DataObject.Environment.Eval(item, 0);
            if (warewolfEvalResult.IsWarewolfAtomResult)
            {
                if (warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult result)
                {
                    Assert.AreEqual(value, result.Item.ToString());
                }
            }
            else if (warewolfEvalResult.IsWarewolfAtomListresult)
            {
                if (warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult result && result.Item.Count == 1)
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
                case enForEachType.InRange:
                    break;
                case enForEachType.InCSV:
                    break;
                case enForEachType.NumOfExecution:
                    break;
                default:
                    break;
            }
        }
        
        protected override void BuildDataList()
        {
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            BuildShapeAndTestData();
            if (!scenarioContext.TryGetValue("datasource", out string datasource))
            {
                datasource = string.Empty;
            }

            if (!scenarioContext.TryGetValue("alias", out string alias))
            {
                alias = string.Empty;
            }

            var innerActivity = scenarioContext.Get<Activity>("innerActivity");

            var selectAndApplyTool = new DsfSelectAndApplyActivity();
            selectAndApplyTool.DataSource = datasource;
            selectAndApplyTool.Alias = alias;
            selectAndApplyTool.ApplyActivityFunc.Handler = innerActivity;
            TestStartNode = new FlowStep
            {
                Action = selectAndApplyTool
            };

            scenarioContext.Add("activity", selectAndApplyTool);
        }

        string BuildInputMappings()
        {
            var inputMappings = new StringBuilder();
            inputMappings.Append("<Inputs>");

            var inMapTo = scenarioContext.Get<string>("inMapTo");
            var inRecordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, inMapTo);
            var inColumn = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, inMapTo);

            var inMapFrom = scenarioContext.Get<string>("inMapFrom");
            inputMappings.Append(string.Format("<Input Name=\"{0}\" Source=\"{1}\" Recordset=\"{2}\"/>", inColumn,
                                               inMapFrom, inRecordset));

            inputMappings.Append("</Inputs>");
            return inputMappings.ToString();
        }

        string BuildOutputMappings()
        {
            var outputMappings = new StringBuilder();
            outputMappings.Append("<Outputs>");

            var outMapFrom = scenarioContext.Get<string>("outMapFrom");
            var inRecordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, outMapFrom);
            var inColumn = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, outMapFrom);

            var outMapTo = scenarioContext.Get<string>("outMapTo");
            outputMappings.Append(string.Format(
                "<Output Name=\"{0}\" MapsTo=\"{1}\" Value=\"{1}\" Recordset=\"{2}\"/>", inColumn,
                outMapTo, inRecordset));

            outputMappings.Append("</Outputs>");
            return outputMappings.ToString();
        }
    }

    class SelectAndApplyTestTool : DsfNativeActivity<string>
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

        public override IEnumerable<StateVariable> GetState()
        {
            return new StateVariable[0];
        }

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

        public bool Equals(SelectAndApplyTestTool other)
        {
            return ReferenceEquals(this, other);
        }
        public override bool Equals(object obj)
        {
            if (obj is SelectAndApplyTestTool instance)
            {
                return Equals(instance);
            }
            return false;
        }
    }
}
