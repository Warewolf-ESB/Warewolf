/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;
using Dev2.TO;

namespace Dev2.Activities.Specs.Toolbox.Data.CaseConversion
{
    [Binding]
    public class CaseConversionSteps : RecordSetBases
    {
        public CaseConversionSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            var caseConvert = new DsfCaseConvertActivity();

            TestStartNode = new FlowStep
                {
                    Action = caseConvert
                };

            var row = 1;

            var caseConversion = scenarioContext.Get<List<Tuple<string, string>>>("caseConversion");
            foreach(dynamic variable in caseConversion)
            {
                caseConvert.ConvertCollection.Add(new CaseConvertTO(variable.Item1, variable.Item2, variable.Item1, row));
                row++;
            }
            scenarioContext.Add("activity", caseConvert);
        }

        [Given(@"I have a case convert variable ""(.*)"" with a value of ""(.*)""")]
        [Given(@"I have a case convert variable ""(.*)"" with a value of ""(.*)""")]
        public void GivenIHaveACaseConvertVariableWithAValueOf(string variable, string value)
        {
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"variable ""(.*)"" with a value of ""(.*)""")]
        public void GivenVariableWithAValueOf(string variable, string value)
        {
            GivenIHaveACaseConvertVariableWithAValueOf(variable, value);
        }

        [Given(@"I convert a variable ""(.*)"" to ""(.*)""")]
        [Given(@"I convert a variable ""(.*)"" to ""(.*)""")]
        public void GivenIConvertAVariableTo(string variable, string toCase)
        {
            scenarioContext.TryGetValue("caseConversion", out List<Tuple<string, string>> caseConversion);

            if (caseConversion == null)
            {
                caseConversion = new List<Tuple<string, string>>();
                scenarioContext.Add("caseConversion", caseConversion);
            }

            caseConversion.Add(new Tuple<string, string>(variable, toCase));
        }

        [When(@"the case conversion tool is executed")]
        public void WhenTheCaseConversionToolIsExecuted()
        {
            BuildDataList();
            var result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Given(@"I have a CaseConversion recordset")]
        public void GivenIHaveACaseConversionRecordset(Table table)
        {
            var records = table.Rows.ToList();

            if (records.Count == 0)
            {
                var rs = table.Header.ToArray()[0];
                var field = table.Header.ToArray()[1];


                var isAdded = scenarioContext.TryGetValue("rs", out List<Tuple<string, string>> emptyRecordset);
                if (!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                    scenarioContext.Add("rs", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(rs, field));
            }

            foreach(TableRow record in records)
            {
                scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

                if (variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    scenarioContext.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(record[0], record[1]));
            }
        }

        [Then(@"the case convert result for this varibale ""(.*)"" will be")]
        public void ThenTheCaseConvertResultForThisVaribaleWillBe(string variable, Table table)
        {
            var recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, variable);
            var column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable);

            var result = scenarioContext.Get<IDSFDataObject>("result");
            var recordSetValues = RetrieveAllRecordSetFieldValues(result.Environment, recordset, column,
                                                                           out string error);
            recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();

            var tableRows = table.Rows.ToList();
            Assert.AreEqual(tableRows.Count, recordSetValues.Count);
            for(int i = 0; i < tableRows.Count; i++)
            {
                Assert.AreEqual(tableRows[i][1], recordSetValues[i]);
            }
        }

        [Then(@"the sentence will be ""(.*)""")]
        public void ThenTheSentenceWillBe(string value)
        {
            value = value.Replace('"', ' ').Trim();
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment( result.Environment,"[[var]]", out string actualValue, out string error);
            Assert.AreEqual(value, actualValue);
        }
    }
}
