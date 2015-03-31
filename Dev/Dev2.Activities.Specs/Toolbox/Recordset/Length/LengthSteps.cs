
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Xml.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Data.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

namespace Dev2.Activities.Specs.Toolbox.Recordset.Length
{
    [Binding]
    public class LengthSteps : RecordSetBases
    {
        //protected override void BuildDataList()
        //{
        //    List<Tuple<string, string>> variableList;
        //    ScenarioContext.Current.TryGetValue("variableList", out variableList);

        //    if(variableList == null)
        //    {
        //        variableList = new List<Tuple<string, string>>();
        //        ScenarioContext.Current.Add("variableList", variableList);
        //    }

        //    variableList.Add(new Tuple<string, string>(ResultVariable, ""));
        //    BuildShapeAndTestData();


        //}

        protected override void BuildDataList()
        {
            var shape = new XElement("root");
            var data = new XElement("root");

            // ReSharper disable NotAccessedVariable
            int row = 0;
            dynamic variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList != null)
            {
                foreach (dynamic variable in variableList)
                {
                    if (!string.IsNullOrEmpty(variable.Item1) && !string.IsNullOrEmpty(variable.Item2))
                    {
                        DataObject.Environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(variable.Item1), variable.Item2);
                    }
                    //Build(variable, shape, data, row);
                    row++;
                }
            }

            string recordSetName;
            ScenarioContext.Current.TryGetValue("recordset", out recordSetName);

            var recordset = ScenarioContext.Current.Get<string>("recordset");

            var length = new DsfRecordsetLengthActivity
            {
                RecordsetName = recordset,
                RecordsLength = ResultVariable
            };

            TestStartNode = new FlowStep
            {
                Action = length
            };
            ScenarioContext.Current.Add("activity", length);
            CurrentDl = shape.ToString();
            TestData = data.ToString();
        }

        [Given(@"I get  the length from a recordset that looks like with this shape")]
        public void GivenIGetTheLengthFromARecordsetThatLooksLikeWithThisShape(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();

            if(tableRows.Count == 0)
            {
                var rs = table.Header.ToArray()[0];

                List<Tuple<string, string>> emptyRecordset;

                bool isAdded = ScenarioContext.Current.TryGetValue("rs", out emptyRecordset);
                if(!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("rs", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(rs, "row"));
            }

            foreach(TableRow t in tableRows)
            {
                List<Tuple<string, string>> variableList;
                ScenarioContext.Current.TryGetValue("variableList", out variableList);

                if(variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(t[0], t[1]));
            }
        }

        [Given(@"get length on record ""(.*)""")]
        public void GivenGetLengthOnRecord(string recordset)
        {
            ScenarioContext.Current.Add("recordset", recordset);
        }

        [When(@"the length tool is executed")]
        public void WhenTheLengthToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the length result should be (.*)")]
        public void ThenTheLengthResultShouldBe(string expectedResult)
        {
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            string actualValue = ExecutionEnvironment.WarewolfEvalResultToString(result.Environment.Eval("[[result]]"));
            expectedResult = expectedResult.Replace('"', ' ').Trim();
           
            actualValue = string.IsNullOrEmpty(actualValue) ? "0" : actualValue;
            Assert.AreEqual(expectedResult, actualValue);
        }
    }
}
