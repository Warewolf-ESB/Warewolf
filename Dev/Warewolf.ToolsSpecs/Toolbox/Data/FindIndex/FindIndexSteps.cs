
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
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Data.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Data.FindIndex
{
    [Binding]
    public class FindIndexSteps : RecordSetBases
    {
        protected override void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            var resultVariable = ResultVariable;
            string resultVar;
            if (ScenarioContext.Current.TryGetValue("resultVariable", out resultVar))
            {
                resultVariable = resultVar;
                var resultVars = resultVariable.Split(',');
                variableList.AddRange(Enumerable.Select<string, Tuple<string, string>>(resultVars, resVar => new Tuple<string, string>(resVar, "")));
            }
            BuildShapeAndTestData();

            string inField;
            ScenarioContext.Current.TryGetValue("inField", out inField);
            string index;
            ScenarioContext.Current.TryGetValue("index", out index);
            string characters;
            ScenarioContext.Current.TryGetValue("characters", out characters);
            string direction;
            ScenarioContext.Current.TryGetValue("direction", out direction);

            var findIndex = new DsfIndexActivity
                {
                    Result = resultVariable,
                    InField = inField,
                    Index = index,
                    Characters = characters,
                    Direction = direction
                };

            TestStartNode = new FlowStep
                {
                    Action = findIndex
                };
            ScenarioContext.Current.Add("activity", findIndex);
        }

        [Given(@"the sentence ""(.*)""")]
        public void GivenTheSentence(string inField)
        {
            ScenarioContext.Current.Add("inField", inField);
        }

        [Given(@"I selected Index ""(.*)""")]
        [Given(@"I have selected Index ""(.*)""")]
        public void GivenISelectedIndex(string index)
        {
            ScenarioContext.Current.Add("index", index);
        }

       
        [Given(@"I search for characters ""(.*)""")]
        public void GivenISearchForCharacters(string characters)
        {
            if(characters == "\" \"")
            {
                characters = ' '.ToString();
            }
            ScenarioContext.Current.Add("characters", characters);
        }

        [Given(@"I selected direction as ""(.*)""")]
        public void GivenISelectedDirectionAs(string direction)
        {
            ScenarioContext.Current.Add("direction", direction);
        }

        [Given(@"I have a findindex variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveAFindindexVariableEqualTo(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [When(@"the data find index tool is executed")]
        public void WhenTheDataFindIndexToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the find index result is ""(.*)""")]
        public void ThenTheFindIndexResultIs(string results)
        {
            string error;
            string actualValue;
            results = results.Replace("\"\"", "");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(results, actualValue);
        }

        [Then(@"the find index result is")]
        public void ThenTheFindIndexResultIs(Table table)
        {
            string error;
            string actualValue;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);

            List<string> records = actualValue.Split(',').ToList();
            List<TableRow> tableRows = table.Rows.ToList();

            Assert.AreEqual(tableRows.Count, records.Count);

            for (int i = 0; i < tableRows.Count; i++)
            {
                Assert.AreEqual(tableRows[i][0], records[i]);
            }
        }

        [Given(@"result variable as ""(.*)""")]
        public void GivenResultVariableAs(string resultVar)
        {
            ScenarioContext.Current.Add("resultVariable", resultVar);
        }
    }
}
