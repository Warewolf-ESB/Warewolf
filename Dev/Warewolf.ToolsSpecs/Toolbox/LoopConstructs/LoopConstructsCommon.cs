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
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Warewolf.ToolsSpecs.Toolbox.LoopConstructs
{
    [Binding]
    public class LoopConstructsCommon
    {
        private readonly ScenarioContext scenarioContext;

        public LoopConstructsCommon(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        [Given(@"There is a recordset in the datalist with this shape")]
        public void GivenThereIsARecordsetInTheDatalistWithThisShape(Table table)
        {
            List<TableRow> rows = table.Rows.ToList();

            if (rows.Count == 0)
            {
                var rs = table.Header.ToArray()[0];
                var field = table.Header.ToArray()[1];

                List<Tuple<string, string>> emptyRecordset;

                bool isAdded = scenarioContext.TryGetValue("rs", out emptyRecordset);
                if (!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                    scenarioContext.Add("rs", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(rs, field));
            }

            foreach (TableRow tableRow in rows)
            {
                List<Tuple<string, string>> variableList;
                scenarioContext.TryGetValue("variableList", out variableList);

                if (variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    scenarioContext.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(tableRow[0], tableRow[1]));
            }
        }

        [Given(@"I Map the input recordset ""(.*)"" to ""(.*)""")]
        public void GivenIMapTheInputRecordsetTo(string inMapFrom, string inMapTo)
        {
            scenarioContext.Add("inMapFrom", inMapFrom);
            scenarioContext.Add("inMapTo", inMapTo);
        }

        [Given(@"I Map the output recordset ""(.*)"" to ""(.*)""")]
        public void GivenIMapTheOutputRecordsetTo(string outMapFrom, string outMapTo)
        {
            scenarioContext.Add("outMapFrom", outMapFrom);
            scenarioContext.Add("outMapTo", outMapTo);
        }

        [Then(@"The mapping uses the following indexes")]
        public void ThenTheMappingUsesTheFollowingIndexes(Table table)
        {

            var updateValues = scenarioContext.Get<List<int>>("indexUpdate").Select(a => a.ToString());
            foreach (var tableRow in table.Rows)
            {
                Assert.IsTrue(updateValues.Contains(tableRow[0]));

            }
        }

        [Given(@"I have a variable ""(.*)"" with the value ""(.*)""")]
        public void GivenIHaveAVariableWithTheValue(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
            if (!string.IsNullOrEmpty(variable))
            {
                variableList.Add(new Tuple<string, string>(variable, value));
            }
        }
    }
}
