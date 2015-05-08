
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
using System.Text;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Data.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Scripting.Command
{
    [Binding]
    public class CommandSteps : RecordSetBases
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

            string commandToExecute;
            ScenarioContext.Current.TryGetValue("commandToExecute", out commandToExecute);

            var commandLine = new DsfExecuteCommandLineActivity
                {
                    CommandFileName = commandToExecute,
                    CommandResult = ResultVariable,
                };

            TestStartNode = new FlowStep
                {
                    Action = commandLine
                };
            ScenarioContext.Current.Add("activity", commandLine);
        }

        [Given(@"I have this command script to execute ""(.*)""")]
        public void GivenIHaveThisCommandScriptToExecute(string commandToExecute)
        {
            ScenarioContext.Current.Add("commandToExecute", commandToExecute);
        }

        [Given(@"I have these command scripts to execute in a single execution run")]
        public void GivenIHaveTheseCommandScriptsToExecuteInASingleExecutionRun(Table table)
        {
            List<TableRow> commands = table.Rows.ToList();
            var commandBuilder = new StringBuilder();
            foreach(TableRow tableRow in commands)
            {
                commandBuilder.AppendLine(tableRow[0]);
            }
            var commandToExecute = commandBuilder.ToString();
            ScenarioContext.Current.Add("commandToExecute", commandToExecute);
        }

        [When(@"the command tool is executed")]
        public void WhenTheCommandToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the result of the command tool will be ""(.*)""")]
        public void ThenTheResultOfTheCommandToolWillBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            if(string.IsNullOrEmpty(expectedResult))
            {
                Assert.IsTrue(string.IsNullOrEmpty(actualValue));
            }
            else
            {
                Assert.IsTrue(actualValue.Contains(actualValue));
            }
        }

        [Given(@"I have a command variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveACommandVariableEqualTo(string variable, string value)
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
    }
}
