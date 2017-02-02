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
using System.Xml.Linq;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;
using Warewolf.Tools.Specs.BaseTypes;
using WarewolfParserInterop;

// ReSharper disable NotAccessedVariable

namespace Warewolf.ToolsSpecs.Toolbox.Recordset
{
    [Binding]
    public class LengthSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public LengthSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        protected override void BuildDataList()
        {
            var shape = new XElement("root");
            var data = new XElement("root");

            int row = 0;
            dynamic variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if (variableList != null)
            {
 foreach (dynamic variable in variableList)
                    {
                        if (!string.IsNullOrEmpty(variable.Item1) && !string.IsNullOrEmpty(variable.Item2))
                        {
                            string value = variable.Item2 == "blank" ? "" : variable.Item2;
                            if (value.ToUpper() == "NULL")
                            {
                                DataObject.Environment.AssignDataShape(variable.Item1);
                            }
                            else
                            {
                                DataObject.Environment.AssignWithFrame(new AssignValue(  DataListUtil.AddBracketsToValueIfNotExist(variable.Item1), value), 0);
                            }
                        }
                        row++;
                    }
                DataObject.Environment.CommitAssign();
            }

            string recordSetName;
            scenarioContext.TryGetValue("recordset", out recordSetName);

            var recordset = scenarioContext.Get<string>("recordset");
            bool treaNullAsZero;
            scenarioContext.TryGetValue("treaNullAsZero", out treaNullAsZero);

            //var length = new DsfRecordsetNullhandlerLengthActivity
            //{
            //    RecordsetName = recordset,
            //    RecordsLength = ResultVariable, 
            //    TreatNullAsZero = treaNullAsZero
            //};
            DsfActivityAbstract<string> length;
            scenarioContext.TryGetValue("activityMode", out length);
            if (length != null)

                length = new DsfRecordsetNullhandlerLengthActivity
                {
                    RecordsetName = recordset,
                    RecordsLength = ResultVariable,
                    TreatNullAsZero = treaNullAsZero
                };
            else
            {
               length = new DsfRecordsetLengthActivity
               {
                   RecordsetName = recordset,
                   RecordsLength = ResultVariable,
               };
            }

            TestStartNode = new FlowStep
            {
                Action = length
            };
            scenarioContext.Add("activity", length);
            CurrentDl = shape.ToString();
            TestData = data.ToString();
        }

        [Given(@"I get the length from a recordset that looks like with this shape")]
        public void GivenIGetTheLengthFromARecordsetThatLooksLikeWithThisShape(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();

            if(tableRows.Count == 0)
            {
                var rs = table.Header.ToArray()[0];

                List<Tuple<string, string>> emptyRecordset;

                bool isAdded = scenarioContext.TryGetValue("rs", out emptyRecordset);
                if(!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                    scenarioContext.Add("rs", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(rs, "row"));
            }

            foreach(TableRow t in tableRows)
            {
                List<Tuple<string, string>> variableList;
                scenarioContext.TryGetValue("variableList", out variableList);

                if(variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    scenarioContext.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(t[0], t[1]));
            }
        }

        [Given(@"get length on record ""(.*)""")]
        public void GivenGetLengthOnRecord(string recordset)
        {
            scenarioContext.Add("recordset", recordset);
        }

        [When(@"the length tool is executed")]
        public void WhenTheLengthToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the length result should be (.*)")]
        public void ThenTheLengthResultShouldBe(string expectedResult)
        {
            var result = scenarioContext.Get<IDSFDataObject>("result");
            string actualValue = ExecutionEnvironment.WarewolfEvalResultToString(result.Environment.Eval("[[result]]",0));
            expectedResult = expectedResult.Replace('"', ' ').Trim();
           
            actualValue = string.IsNullOrEmpty(actualValue) ? "0" : actualValue;
            Assert.AreEqual(expectedResult, actualValue);
        }

        [Given(@"Length Treat Null as Empty Recordset is not selected")]
        public void GivenLengthTreatNullAsEmptyRecordsetIsNotSelected()
        {
            scenarioContext.Add("treaNullAsZero", false);
        }

        [Given(@"Length Treat Null as Empty Recordset is selected")]
        public void GivenLengthTreatNullAsEmptyRecordsetIsSelected()
        {
            scenarioContext.Add("treaNullAsZero", true);
        }

    }
}
