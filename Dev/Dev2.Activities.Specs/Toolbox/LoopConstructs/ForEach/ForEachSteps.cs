using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Activities.Specs.BaseTypes;
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
        private string _activity;
        private DsfForEachActivity _dsfForEach;
        private enForEachType _foreachType;
        private string _inMapFrom;
        private string _inMapTo;
        private string _outMapFrom;
        private string _outMapTo;
        private string _recordSet;
        private string r = "[[r().v]]";

        private void BuildDataList()
        {
            var variableList = ScenarioContext.Current.Get<List<Tuple<string, string>>>("variableList");
            variableList.Add(new Tuple<string, string>(r, ""));
            variableList.Add(new Tuple<string, string>(_inMapTo, ""));
            variableList.Add(new Tuple<string, string>(_outMapTo, ""));

            BuildShapeAndTestData();

            dynamic activity;

            if (_activity.Equals("Tool"))
            {
                activity = new DsfRandomActivity
                    {
                        Result = r,
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
                        OutputMapping = BuildOutputMappings(),
                    };
            }

            var activityFunction = new ActivityFunc<string, bool> {Handler = activity};

            _dsfForEach = new DsfForEachActivity
                {
                    ForEachType = _foreachType,
                    Recordset = _recordSet,
                    DataFunc = activityFunction
                };

            TestStartNode = new FlowStep
                {
                    Action = _dsfForEach
                };
        }

        private string BuildInputMappings()
        {
            var inputMappings = new StringBuilder();
            inputMappings.Append("<Inputs>");

            string inRecordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, _inMapTo);
            string inColumn = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, _inMapTo);

            inputMappings.Append(string.Format("<Input Name=\"{0}\" Source=\"{1}\" Recordset=\"{2}\"/>", inColumn,
                                               _inMapFrom, inRecordset));

            inputMappings.Append("</Inputs>");
            return inputMappings.ToString();
        }

        private string BuildOutputMappings()
        {
            var outputMappings = new StringBuilder();
            outputMappings.Append("<Outputs>");

            string inRecordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, _outMapFrom);
            string inColumn = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, _outMapFrom);

            outputMappings.Append(string.Format(
                "<Output Name=\"{0}\" MapsTo=\"{1}\" Value=\"{1}\" Recordset=\"{2}\"/>", inColumn,
                _outMapTo, inRecordset));

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
            _foreachType = (enForEachType) Enum.Parse(typeof (enForEachType), foreachType);
            _recordSet = recordSet;
        }

        [Given(@"the underlying dropped activity is a\(n\) ""(.*)""")]
        public void GivenTheUnderlyingDroppedActivityIsAN(string activity)
        {
            _activity = activity;
        }

        [When(@"the foreach tool is executed")]
        public void WhenTheForeachToolIsExecuted()
        {
             ScenarioContext.Current.Pending();
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(throwException:false);
            ScenarioContext.Current.Add("result", result);
        }

        [Given(@"I Map the input recordset ""(.*)"" to ""(.*)""")]
        public void GivenIMapTheInputRecordsetTo(string inMapFrom, string inMapTo)
        {
            _inMapFrom = inMapFrom;
            _inMapTo = inMapTo;
        }

        [Given(@"I Map the output recordset ""(.*)"" to ""(.*)""")]
        public void GivenIMapTheOutputRecordsetTo(string outMapFrom, string outMapTo)
        {
            _outMapFrom = outMapFrom;
            _outMapTo = outMapTo;
        }

        [Then(@"the recordset ""(.*)"" will have data as")]
        public void ThenTheRecordsetWillHaveDataAs(string resRecordset, Table table)
        {
            string recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, resRecordset);
            string column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, resRecordset);

            string error;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
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

        //[Then(@"the foreach will loop over (.*) records")]
        //public void ThenTheForeachWillLoopOverRecords(int numOfIterations)
        //{
        //    string error;
        //    var recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, r);
        //    var column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, r);
        //      var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
        //    var recordSetValues = RetrieveAllRecordSetFieldValues(result.DataListID, recordset, column, out error);
        //    recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();
        //    Assert.AreEqual(numOfIterations, recordSetValues.Count);
        //}
    }
}