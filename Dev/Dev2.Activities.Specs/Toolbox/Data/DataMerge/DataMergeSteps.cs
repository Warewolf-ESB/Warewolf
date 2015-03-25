
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
using Dev2.Data.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Data.DataMerge
{
    [Binding]
    public class DataMergeSteps : RecordSetBases
    {
        protected override void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            var dataMerge = new DsfDataMergeActivity { Result = ResultVariable };

            List<Tuple<string, string, string, string, string>> mergeCollection;
            ScenarioContext.Current.TryGetValue("mergeCollection", out mergeCollection);

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

            ScenarioContext.Current.Add("activity", dataMerge);
        }

        [Given(@"a merge variable ""(.*)"" equal to ""(.*)""")]
        [Given(@"a merge variable '(.*)' equal to ""(.*)""")]
        public void GivenAMergeVariableEqualTo(string variable, string value)
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

        [Given(
            @"an Input ""(.*)"" and merge type ""(.*)"" and string at as ""(.*)"" and Padding ""(.*)"" and Alignment ""(.*)"""
            )]
        public void GivenAnInputAndMergeTypeAndStringAtAsAndPaddingAndAlignment(string input, string mergeType,
                                                                                string stringAt, string padding,
                                                                                string alignment)
        {
            List<Tuple<string, string, string, string, string>> mergeCollection;
            ScenarioContext.Current.TryGetValue("mergeCollection", out mergeCollection);

            if(mergeCollection == null)
            {
                mergeCollection = new List<Tuple<string, string, string, string, string>>();
                ScenarioContext.Current.Add("mergeCollection", mergeCollection);
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

        [When(@"the data merge tool is executed")]
        public void WhenTheDataMergeToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the merged result is ""(.*)""")]
        public void ThenTheMergedResultIs(string value)
        {
            string error;
            string actualValue;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, ResultVariable,
                                       out actualValue, out error);
            if(string.IsNullOrEmpty(value))
            {
                Assert.IsNull(actualValue);
            }
            else
            {
                Assert.AreEqual(value, actualValue);
            }
        }

        [Then(@"the merged result is the same as file ""(.*)""")]
        public void ThenTheMergedResultIsTheSameAsFile(string fileName)
        {
            string resourceName = string.Format("Dev2.Activities.Specs.Toolbox.Data.DataMerge.{0}",
                                                fileName);
            string value = ReadFile(resourceName);
            string error;
            string actualValue;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, ResultVariable,
                                       out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }
    }
}
