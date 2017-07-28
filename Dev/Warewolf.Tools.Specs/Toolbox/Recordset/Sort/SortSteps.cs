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

namespace Warewolf.ToolsSpecs.Toolbox.Recordset.Sort
{
    [Binding]
    public class SortSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public SortSteps(ScenarioContext scenarioContext)
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

            var recordsetName = scenarioContext.Get<string>("recordset");
            var sortOrder = scenarioContext.Get<string>("sortOrder");
            var sortRecords = new DsfSortRecordsActivity
                {
                    SortField = recordsetName,
                    SelectedSort = sortOrder
                };

            TestStartNode = new FlowStep
                {
                    Action = sortRecords
                };
            scenarioContext.Add("activity", sortRecords);
        }

        [Given(@"I have the following recordset to sort")]
        public void GivenIHaveTheFollowingRecordsetToSort(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();

            if(tableRows.Count == 0)
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

        [Given(@"I sort a record ""(.*)""")]
        public void GivenISortARecord(string recordset)
        {
            scenarioContext.Add("recordset", recordset);
        }

        [Given(@"my sort order is ""(.*)""")]
        public void GivenMySortOrderIs(string sortOrder)
        {
            scenarioContext.Add("sortOrder", sortOrder);
        }

        [When(@"the sort records tool is executed")]
        public void WhenTheSortRecordsToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the sorted recordset ""(.*)""  will be")]
        public void ThenTheSortedRecordsetWillBe(string variable, Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            var recordSets = DataObject.Environment.Eval(variable, 0);
            if (recordSets.IsWarewolfAtomListresult)
            {
                // ReSharper disable PossibleNullReferenceException
                var recordSetValues = (recordSets as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult).Item.ToList();
                // ReSharper restore PossibleNullReferenceException
                Assert.AreEqual<int>(tableRows.Count, recordSetValues.Count);

                for (int i = 0; i < tableRows.Count; i++)
                {
                    Assert.AreEqual<string>(tableRows[i][1], ExecutionEnvironment.WarewolfAtomToString(recordSetValues[i]));
                }
            }


        }
    }
}
