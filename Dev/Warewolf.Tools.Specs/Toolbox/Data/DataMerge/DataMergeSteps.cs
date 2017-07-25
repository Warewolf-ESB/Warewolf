/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Text;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Data.DataMerge
{
    [Binding]
    public class DataMergeSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public DataMergeSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        protected override void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            var dataMerge = new DsfDataMergeActivity { Result = ResultVariable };

            List<Tuple<string, string, string, string, string>> mergeCollection;
            scenarioContext.TryGetValue("mergeCollection", out mergeCollection);

            int row = 1;
            foreach(var variable in mergeCollection)
            {
                dataMerge.MergeCollection.Add(new DataMergeDTO(variable.Item1, variable.Item2, variable.Item3, row,
                                                                variable.Item4, variable.Item5));
                row++;
            }

            TestStartNode = new FlowStep
                {
                    Action = dataMerge
                };

            scenarioContext.Add("activity", dataMerge);
        }

        [Given(@"a merge variable ""(.*)"" equal to ""(.*)""")]
        [Given(@"a merge variable ""(.*)"" equal to ""(.*)""")]
        public void GivenAMergeVariableEqualTo(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(
            @"an Input ""(.*)"" and merge type ""(.*)"" and string at as ""(.*)"" and Padding ""(.*)"" and Alignment ""(.*)"""
            )]
        public void GivenAnInputAndMergeTypeAndStringAtAsAndPaddingAndAlignment(string input, string mergeType,
                                                                                string stringAt, string padding,
                                                                                string alignment)
        {
            List<Tuple<string, string, string, string, string>> mergeCollection;
            scenarioContext.TryGetValue("mergeCollection", out mergeCollection);

            if(mergeCollection == null)
            {
                mergeCollection = new List<Tuple<string, string, string, string, string>>();
                scenarioContext.Add("mergeCollection", mergeCollection);
            }

            mergeCollection.Add(new Tuple<string, string, string, string, string>(input, mergeType, stringAt, padding,
                                                                                   alignment));
        }

        [Given(@"a merge recordset")]
        public void GivenAMergeRecordset(Table table)
        {
            List<TableRow> records = table.Rows.ToList();

            if(records.Count == 0)
            {
                var rs = table.Header.ToArray()[0];
                var field = table.Header.ToArray()[1];

                List<Tuple<string, string>> emptyRecordset;

                bool isAdded = scenarioContext.TryGetValue("rs", out emptyRecordset);
                if(!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                    scenarioContext.Add("rs", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(rs, field));
            }

            foreach(TableRow record in records)
            {
                List<Tuple<string, string>> variableList;
                scenarioContext.TryGetValue("variableList", out variableList);

                if(variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    scenarioContext.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(record[0], record[1]));
            }
        }

        [When(@"the data merge tool is executed")]
        public void WhenTheDataMergeToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the merged result is ""(.*)""")]
        public void ThenTheMergedResultIs(string value)
        {

            string error;
            string actualValue;
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, ResultVariable,
                                       out actualValue, out error);
            FixBreaks(ref value, ref actualValue);
            if (string.IsNullOrEmpty(value))
            {
                Assert.IsTrue(string.IsNullOrEmpty(actualValue));
            }
            else
            {
                Assert.AreEqual(value, actualValue);
            }
        }
        private void FixBreaks(ref string expected, ref string actual)
        {
            expected = new StringBuilder(expected).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
            actual = new StringBuilder(actual).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
        }

        [Then(@"the merged result is the same as file ""(.*)""")]
        public void ThenTheMergedResultIsTheSameAsFile(string fileName)
        {
            string resourceName = string.Format("Warewolf.ToolsSpecs.Toolbox.Data.DataMerge.{0}",
                                                fileName);
            var readFile = ReadFile(resourceName);
            string value = readFile;
            string error;
            string actualValue;
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, ResultVariable,
                                       out actualValue, out error);
            FixBreaks(ref value, ref actualValue);
            Assert.AreEqual(value, actualValue);
        }
    }
}
