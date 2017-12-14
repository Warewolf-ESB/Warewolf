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
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;
using Warewolf.Tools.Specs.BaseTypes;

namespace Warewolf.ToolsSpecs.Toolbox.Recordset
{
    [Binding]
    public class DeleteSteps : RecordSetBases
    {
        readonly ScenarioContext scenarioContext;

        public DeleteSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException(nameof(scenarioContext));
            }

            this.scenarioContext = scenarioContext;
        }

        protected override void BuildDataList()
        {
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            var recordset = scenarioContext.Get<string>("recordset");
            scenarioContext.TryGetValue("treaNullAsZero", out bool treaNullAsZero);
            scenarioContext.TryGetValue("activityMode", out DsfActivityAbstract<string> delete);
            if (delete != null)
            {
                delete = new DsfDeleteRecordNullHandlerActivity
            {
                RecordsetName = recordset,
                Result = ResultVariable,
                TreatNullAsZero = treaNullAsZero
            };
            }
            else
            {
                delete = new DsfDeleteRecordActivity
                {
                    RecordsetName = recordset,
                    Result = ResultVariable,
                };
            }

            TestStartNode = new FlowStep
                {
                    Action = delete
                };
            scenarioContext.Add("activity", delete);
        }

        [Given(@"I have the following recordset")]
        public void GivenIHaveTheFollowingRecordset(Table table)
        {
            var tableRows = table.Rows.ToList();

            if (tableRows.Count == 0)
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

            foreach(TableRow t in tableRows)
            {
                scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

                if (variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    scenarioContext.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(t[0], t[1]));
            }
        }

        [Given(@"an index ""(.*)"" exists with a value ""(.*)""")]
        public void GivenAnIndexExistsWithAValue(string variable, string value)
        {
            value = value.Replace('"', ' ').Trim();
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"I have a delete variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveADeleteVariableEqualTo(string variable, string value)
        {
            value = value.Replace('"', ' ').Trim();
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value));
        }


        [When(@"the delete tool is executed")]
        public void WhenTheDeleteToolIsExecuted()
        {
            BuildDataList();
            var result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the delete result should be ""(.*)""")]
        public void ThenTheDeleteResultShouldBe(string expectedResult)
        {
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, ResultVariable,
                                       out string actualValue, out string error);
            //if(string.IsNullOrEmpty(expectedResult))
            //{
            //    expectedResult = null;
            //}
            Assert.AreEqual(expectedResult, actualValue);
        }

        [Then(@"the recordset ""(.*)"" will be as follows")]
        public void ThenTheRecordsetWillBeAsFollows(string recordset, Table table)
        {
            var tableRows = table.Rows.ToList();

            var recordSets = DataObject.Environment.Eval(recordset, 0);
            if (recordSets.IsWarewolfAtomListresult)
            {
                
                var recordSetValues = (recordSets as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult).Item.ToList();
                
                Assert.AreEqual<int>(tableRows.Count, recordSetValues.Count);

                for (int i = 0; i < tableRows.Count; i++)
                {
                    Assert.AreEqual<string>(tableRows[i][1], ExecutionEnvironment.WarewolfAtomToString( recordSetValues[i]));
                }
            }
            else
            {
                Assert.Fail("evaluation resulted in a scalar or a atom");
            }
        }

        [Given(@"I delete a record ""(.*)""")]
        public void GivenIDeleteARecord(string recordset)
        {
            scenarioContext.Add("recordset", recordset);
        }

        [Given(@"delete treat null as Empty Recordset is not selected")]
        public void GivenDeleteTreatNullAsEmptyRecordsetIsNotSelected()
        {
            scenarioContext.Add("treaNullAsZero", false);
        }

        [Given(@"Treat Null as Empty Recordset is selected")]
        public void GivenTreatNullAsEmptyRecordsetIsSelected()
        {
            scenarioContext.Add("treaNullAsZero", true);
        }



    }
}
