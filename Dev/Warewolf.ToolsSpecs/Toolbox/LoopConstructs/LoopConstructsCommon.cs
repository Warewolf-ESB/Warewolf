
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
        [Given(@"There is a recordset in the datalist with this shape")]
        public void GivenThereIsARecordsetInTheDatalistWithThisShape(Table table)
        {
            List<TableRow> rows = table.Rows.ToList();

            if (rows.Count == 0)
            {
                var rs = table.Header.ToArray()[0];
                var field = table.Header.ToArray()[1];

                List<Tuple<string, string>> emptyRecordset;

                bool isAdded = ScenarioContext.Current.TryGetValue("rs", out emptyRecordset);
                if (!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("rs", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(rs, field));
            }

            foreach (TableRow tableRow in rows)
            {
                List<Tuple<string, string>> variableList;
                ScenarioContext.Current.TryGetValue("variableList", out variableList);

                if (variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(tableRow[0], tableRow[1]));
            }
        }

        [Given(@"I Map the input recordset ""(.*)"" to ""(.*)""")]
        public void GivenIMapTheInputRecordsetTo(string inMapFrom, string inMapTo)
        {
            ScenarioContext.Current.Add("inMapFrom", inMapFrom);
            ScenarioContext.Current.Add("inMapTo", inMapTo);
        }

        [Given(@"I Map the output recordset ""(.*)"" to ""(.*)""")]
        public void GivenIMapTheOutputRecordsetTo(string outMapFrom, string outMapTo)
        {
            ScenarioContext.Current.Add("outMapFrom", outMapFrom);
            ScenarioContext.Current.Add("outMapTo", outMapTo);
        }

        [Then(@"The mapping uses the following indexes")]
        public void ThenTheMappingUsesTheFollowingIndexes(Table table)
        {

            var updateValues = ScenarioContext.Current.Get<List<int>>("indexUpdate").Select(a => a.ToString());
            foreach (var tableRow in table.Rows)
            {
                Assert.IsTrue(updateValues.Contains(tableRow[0]));

            }
        }

        [Given(@"I have a variable ""(.*)"" with the value ""(.*)""")]
        public void GivenIHaveAVariableWithTheValue(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            if (!string.IsNullOrEmpty(variable))
            {
                variableList.Add(new Tuple<string, string>(variable, value));
            }
        }
    }
}
