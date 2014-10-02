
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            var caseConvert = new DsfCaseConvertActivity();

            TestStartNode = new FlowStep
                {
                    Action = caseConvert
                };

            int row = 1;

            var caseConversion = ScenarioContext.Current.Get<List<Tuple<string, string>>>("caseConversion");
            foreach(dynamic variable in caseConversion)
            {
                caseConvert.ConvertCollection.Add(new CaseConvertTO(variable.Item1, variable.Item2, variable.Item1, row));
                row++;
            }
            ScenarioContext.Current.Add("activity", caseConvert);
        }

        [Given(@"I have a case convert variable ""(.*)"" with a value of ""(.*)""")]
        public void GivenIHaveACaseConvertVariableWithAValueOf(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"variable ""(.*)"" with a value of ""(.*)""")]
        public void GivenVariableWithAValueOf(string variable, string value)
        {
            GivenIHaveACaseConvertVariableWithAValueOf(variable, value);
        }


        [Given(@"I convert a variable ""(.*)"" to ""(.*)""")]
        [Given(@"I convert a variable '(.*)' to '(.*)'")]
        public void GivenIConvertAVariableTo(string variable, string toCase)
        {
            List<Tuple<string, string>> caseConversion;
            ScenarioContext.Current.TryGetValue("caseConversion", out caseConversion);

            if(caseConversion == null)
            {
                caseConversion = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("caseConversion", caseConversion);
            }

            caseConversion.Add(new Tuple<string, string>(variable, toCase));
        }

        [When(@"the case conversion tool is executed")]
        public void WhenTheCaseConversionToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Given(@"I have a CaseConversion recordset")]
        public void GivenIHaveACaseConversionRecordset(Table table)
        {
            List<TableRow> records = table.Rows.ToList();

            if(records.Count == 0)
            {
                var rs = table.Header.ToArray()[0];
                var field = table.Header.ToArray()[1];

                List<Tuple<string, string>> emptyRecordset;

                bool isAdded = ScenarioContext.Current.TryGetValue("rs", out emptyRecordset);
                if(!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("rs", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(rs, field));
            }

            foreach(TableRow record in records)
            {
                List<Tuple<string, string>> variableList;
                ScenarioContext.Current.TryGetValue("variableList", out variableList);

                if(variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(record[0], record[1]));
            }
        }

        [Then(@"the case convert result for this varibale ""(.*)"" will be")]
        public void ThenTheCaseConvertResultForThisVaribaleWillBe(string variable, Table table)
        {
            string recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, variable);
            string column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable);

            string error;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            List<string> recordSetValues = RetrieveAllRecordSetFieldValues(result.DataListID, recordset, column,
                                                                           out error);
            recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();

            List<TableRow> tableRows = table.Rows.ToList();
            Assert.AreEqual(tableRows.Count, recordSetValues.Count);
            for(int i = 0; i < tableRows.Count; i++)
            {
                Assert.AreEqual(tableRows[i][1], recordSetValues[i]);
            }
        }

        [Then(@"the sentence will be ""(.*)""")]
        public void ThenTheSentenceWillBe(string value)
        {
            string error;
            string actualValue;
            value = value.Replace('"', ' ').Trim();
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, "var", out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }
    }
}
