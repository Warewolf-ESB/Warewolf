using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Data.CaseConversion
{
    [Binding]
    public class CaseConversionSteps : RecordSetBases
    {
        private DsfCaseConvertActivity _caseConvert;

        private void BuildDataList()
        {
            BuildShapeAndTestData();

            _caseConvert = new DsfCaseConvertActivity();

            TestStartNode = new FlowStep
                {
                    Action = _caseConvert
                };

            int row = 1;

            var variableList = ScenarioContext.Current.Get<List<Tuple<string, string, string>>>("variableList");
            foreach (dynamic variable in variableList)
            {
                _caseConvert.ConvertCollection.Add(new CaseConvertTO(variable.Item1, variable.Item3, variable.Item1, row));
                row++;
            }
        }

        [Given(@"I convert a sentence ""(.*)"" to ""(.*)""")]
        public void GivenIConvertASentenceTo(string sentence, string toCase)
        {
            List<Tuple<string, string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string, string>("[[var]]", sentence, toCase));
        }

        [Given(@"I convert a variable ""(.*)"" to ""(.*)""")]
        public void GivenIConvertAVariableTo(string variable, string toCase)
        {
            List<Tuple<string, string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string, string>(variable, "", toCase));
        }

        [When(@"the case conversion tool is executed")]
        public void WhenTheCaseConversionToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess();
            ScenarioContext.Current.Add("result", result);
        }

        [Given(@"I have a CaseConversion recordset")]
        public void GivenIHaveACaseConversionRecordset(Table table)
        {
            List<TableRow> records = table.Rows.ToList();
            foreach (TableRow record in records)
            {
                List<Tuple<string, string, string>> variableList;
                ScenarioContext.Current.TryGetValue("variableList", out variableList);

                if (variableList == null)
                {
                    variableList = new List<Tuple<string, string, string>>();
                    ScenarioContext.Current.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string, string>(record[0], record[1], ""));
            }
        }

        [Then(@"the case convert result for this varibale ""(.*)"" will be")]
        public void ThenTheCaseConvertResultForThisVaribaleWillBe(string variable, Table table)
        {
            string recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, variable);
            string column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable);

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

        [Then(@"the sentence will be ""(.*)""")]
        public void ThenTheSentenceWillBe(string value)
        {
            string error;
            string actualValue;
            value = value.Replace("\"\"", "");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, "var", out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }

        [Then(@"the case convert execution has ""(.*)"" error")]
        public void ThenTheCaseConvertExecutionHasError(string anError)
        {
            bool expected = anError.Equals("NO");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            bool actual = string.IsNullOrEmpty(FetchErrors(result.DataListID));
            string message = string.Format("expected {0} error but an error was {1}", anError,
                                           actual ? "not found" : "found");
            Assert.AreEqual(expected, actual, message);
        }
    }
}