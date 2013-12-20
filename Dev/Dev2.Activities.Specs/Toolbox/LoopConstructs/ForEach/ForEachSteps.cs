using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common;
using Dev2.Common.Enums;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.LoopConstructs.ForEach
{
    [Binding]
    public class ForEachSteps : RecordSetBases
    {
        private const string ResultRecordsetVariable = "[[r().v]]";

        private void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultRecordsetVariable, ""));

            string inMapTo;
            if (ScenarioContext.Current.TryGetValue("inMapTo", out inMapTo))
            {
                variableList.Add(new Tuple<string, string>(inMapTo, ""));
            }

            string outMapTo;
            if (ScenarioContext.Current.TryGetValue("outMapTo", out outMapTo))
            {
                variableList.Add(new Tuple<string, string>(outMapTo, ""));
            }

            BuildShapeAndTestData();

            var activityType = ScenarioContext.Current.Get<string>("activityType");

            dynamic activity;

            if (activityType.Equals("Tool"))
            {
                activity = new DsfRandomActivity
                    {
                        Result = ResultRecordsetVariable,
                        RandomType = enRandomType.Numbers,
                        From = "0",
                        To = "100"
                    };
            }
            else
            {
                activity = new DsfActivity
                    {
                        InputMapping = BuildInputMappings(),
                        //InputMapping = "<Inputs><Input Name=\"data\" Source=\"[[rs().row]]\" Recordset=\"test\" /></Inputs>",
                        //InputMapping = "<Inputs><Input Name=\"test\" Source=\"[[rs().row]]\" Recordset=\"data\" /></Inputs>",
                        //InputMapping = "<Inputs><Input Name=\"data\" Source=\"[[rs().row]]\" Recordset=\"test\" /></Inputs>",
                        OutputMapping = BuildOutputMappings(),
                        //OutputMapping = "<Outputs><Output Name=\"data\" MapsTo=\"[[res().data]]\" Value=\"[[res().data]]\" Recordset=\"test\" /></Outputs>"
                        //OutputMapping = "<Outputs><Output Name=\"test\" MapsTo=\"[[res().data]]\" Value=\"[[res().data]]\" Recordset=\"data\" /></Outputs>"
                        //OutputMapping = "<Outputs><Output Name=\"data\" MapsTo=\"[[test().data]]\" Value=\"[[test().data]]\" Recordset=\"res\" /></Outputs>",
                    };
            }

            var activityFunction = new ActivityFunc<string, bool> {Handler = activity};
            var foreachType = ScenarioContext.Current.Get<enForEachType>("foreachType");

            string recordSet;
            if (!ScenarioContext.Current.TryGetValue("recordset", out recordSet))
            {
                recordSet = string.Empty;
            }

            string from;
            if (!ScenarioContext.Current.TryGetValue("from", out from))
            {
                from = string.Empty;
            }

            string to;
            if (!ScenarioContext.Current.TryGetValue("to", out to))
            {
                to = string.Empty;
            }

            string numberAs;
            if (!ScenarioContext.Current.TryGetValue("numberAs", out numberAs))
            {
                numberAs = string.Empty;
            }

            var dsfForEach = new DsfForEachActivity
                {
                    ForEachType = foreachType,
                    Recordset = recordSet,
                    From = from,
                    To = to,
                    CsvIndexes = numberAs,
                    NumOfExections = numberAs,
                    DataFunc = activityFunction
                };

            TestStartNode = new FlowStep
                {
                    Action = dsfForEach
                };
        }

        private string BuildInputMappings()
        {
            var inputMappings = new StringBuilder();
            inputMappings.Append("<Inputs>");

            var inMapTo = ScenarioContext.Current.Get<string>("inMapTo");
            string inRecordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, inMapTo);
            string inColumn = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, inMapTo);

            var inMapFrom = ScenarioContext.Current.Get<string>("inMapFrom");
            inputMappings.Append(string.Format("<Input Name=\"{0}\" Source=\"{1}\" Recordset=\"{2}\"/>", inColumn,
                                               inMapFrom, inRecordset));

            inputMappings.Append("</Inputs>");
            return inputMappings.ToString();
        }

        private string BuildOutputMappings()
        {
            var outputMappings = new StringBuilder();
            outputMappings.Append("<Outputs>");

            var outMapFrom = ScenarioContext.Current.Get<string>("outMapFrom");
            string inRecordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, outMapFrom);
            string inColumn = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, outMapFrom);

            var outMapTo = ScenarioContext.Current.Get<string>("outMapTo");
            outputMappings.Append(string.Format(
                "<Output Name=\"{0}\" MapsTo=\"{1}\" Value=\"{1}\" Recordset=\"{2}\"/>", inColumn,
                outMapTo, inRecordset));

            outputMappings.Append("</Outputs>");
            return outputMappings.ToString();
        }

        [Given(@"I there is a recordset in the datalist with this shape")]
        public void GivenIThereIsARecordsetInTheDatalistWithThisShape(Table table)
        {
            List<TableRow> rows = table.Rows.ToList();

            if (rows.Count == 0)
            {
                var rs = table.Header.ToArray()[0];
                var field = table.Header.ToArray()[1];

                List<Tuple<string, string>> emptyRecordset;

                bool isAdded = ScenarioContext.Current.TryGetValue("rs", out emptyRecordset);
                if (!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                     ScenarioContext.Current.Add("rs", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(rs, field));
            }

            foreach (TableRow tableRow in rows)
            {
                List<Tuple<string, string>> variableList;
                ScenarioContext.Current.TryGetValue("variableList", out variableList);

                if (variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(tableRow[0], tableRow[1]));
            }
        }

        [Given(@"I have selected the foreach type as ""(.*)"" and used ""(.*)""")]
        public void GivenIHaveSelectedTheForeachTypeAsAndUsed(string foreachType, string recordSet)
        {
            ScenarioContext.Current.Add("foreachType", (enForEachType)Enum.Parse(typeof(enForEachType), foreachType));
            ScenarioContext.Current.Add("recordset", recordSet);
        }

        [Given(@"I have selected the foreach type as ""(.*)"" from (.*) to (.*)")]
        public void GivenIHaveSelectedTheForeachTypeAsFromTo(string foreachType, string from, string to)
        {
            ScenarioContext.Current.Add("foreachType", (enForEachType)Enum.Parse(typeof(enForEachType), foreachType));
            ScenarioContext.Current.Add("from", from);
            ScenarioContext.Current.Add("to", to);
        }

        [Given(@"I have selected the foreach type as ""(.*)"" as ""(.*)""")]
        public void GivenIHaveSelectedTheForeachTypeAsAs(string foreachType, string numberAs)
        {
            ScenarioContext.Current.Add("foreachType", (enForEachType)Enum.Parse(typeof(enForEachType), foreachType));
            ScenarioContext.Current.Add("numberAs", numberAs);
        }

        [Given(@"the underlying dropped activity is a\(n\) ""(.*)""")]
        public void GivenTheUnderlyingDroppedActivityIsAN(string activityType)
        {
            ScenarioContext.Current.Add("activityType", activityType);
        }

        [When(@"the foreach tool is executed")]
        public void WhenTheForeachToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(throwException:false);
            ScenarioContext.Current.Add("result", result);
        }

        [Given(@"I Map the input recordset ""(.*)"" to ""(.*)""")]
        public void GivenIMapTheInputRecordsetTo(string inMapFrom, string inMapTo)
        {
            ScenarioContext.Current.Add("inMapFrom", inMapFrom);
            ScenarioContext.Current.Add("inMapTo", inMapTo);
        }

        [Given(@"I Map the output recordset ""(.*)"" to ""(.*)""")]
        public void GivenIMapTheOutputRecordsetTo(string outMapFrom, string outMapTo)
        {
            ScenarioContext.Current.Add("outMapFrom", outMapFrom);
            ScenarioContext.Current.Add("outMapTo", outMapTo);
        }

        [Then(@"the recordset ""(.*)"" will have data as")]
        public void ThenTheRecordsetWillHaveDataAs(string resRecordset, Table table)
        {
            string recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, resRecordset);
            string column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, resRecordset);
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            
            ErrorResultTO errors;
            var compiler  = DataListFactory.CreateDataListCompiler();
            compiler.ConvertFrom(result.DataListID, DataListFormat.CreateFormat(GlobalConstants._XML),
                                        enTranslationDepth.Data, out errors);


            string error;
            List<string> recordSetValues = RetrieveAllRecordSetFieldValues(result.DataListID, recordset, column,
                                                                           out error);
            recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();

            List<TableRow> tableRows = table.Rows.ToList();
            Assert.AreEqual(tableRows.Count, recordSetValues.Count);
            for (int i = 0; i < tableRows.Count; i++)
            {
                Assert.AreEqual(tableRows[i][1], recordSetValues[i]);
            }
        }

        [Then(@"the foreach executes (.*) times")]
        public void ThenTheForeachExecutesTimes(int numOfIterations)
        {
            string error;
            var recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, ResultRecordsetVariable);
            var column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, ResultRecordsetVariable);
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            var recordSetValues = RetrieveAllRecordSetFieldValues(result.DataListID, recordset, column, out error);
            recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();
            Assert.AreEqual(numOfIterations, recordSetValues.Count);
        }

        [Then(@"the foreach execution has ""(.*)"" error")]
        public void ThenTheForeachExecutionHasError(string anError)
        {
            bool expected = anError.Equals("NO");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            string fetchErrors = FetchErrors(result.DataListID);
            bool actual = string.IsNullOrEmpty(fetchErrors);
            string message = string.Format("expected {0} error but it {1}", anError.ToLower(),
                                           actual ? "did not occur" : "did occur" + fetchErrors);
            Assert.IsTrue(expected == actual, message);
        }
    }
}