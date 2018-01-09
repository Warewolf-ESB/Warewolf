/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Data.Util;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Scripting.Command
{
    [Binding]
    public class CommandSteps : RecordSetBases
    {
        readonly ScenarioContext scenarioContext;

        public CommandSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
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

            var resultVariable = ResultVariable;
            if (scenarioContext.TryGetValue("resVar", out string resVar))
            {
                resultVariable = resVar;
            }
            variableList.Add(new Tuple<string, string>(resultVariable, ""));
            BuildShapeAndTestData();

            scenarioContext.TryGetValue("commandToExecute", out string commandToExecute);

            var commandLine = new DsfExecuteCommandLineActivity
                {
                    CommandFileName = commandToExecute,
                    CommandResult = resultVariable,
                };

            TestStartNode = new FlowStep
                {
                    Action = commandLine
                };
            scenarioContext.Add("activity", commandLine);
        }

        [Given(@"I have this command script to execute ""(.*)""")]
        [Given(@"I have this command script to execute '(.*)'")]
        public void GivenIHaveThisCommandScriptToExecute(string commandToExecute)
        {
            scenarioContext.Add("commandToExecute", commandToExecute);
        }

        [Given(@"I have these command scripts to execute in a single execution run")]
        public void GivenIHaveTheseCommandScriptsToExecuteInASingleExecutionRun(Table table)
        {
            var commands = table.Rows.ToList();
            var commandBuilder = new StringBuilder();
            foreach(TableRow tableRow in commands)
            {
                commandBuilder.AppendLine(tableRow[0]);
            }
            var commandToExecute = commandBuilder.ToString();
            scenarioContext.Add("commandToExecute", commandToExecute);
        }

        [When(@"the command tool is executed")]
        public void WhenTheCommandToolIsExecuted()
        {
            BuildDataList();
            var result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the result of the command tool will be ""(.*)""")]
        [Then(@"the result of the command tool will be '(.*)'")]
        public void ThenTheResultOfTheCommandToolWillBe(string expectedResult)
        {
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out string actualValue, out string error);
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
        [Given(@"I have a command variable '(.*)' equal to '(.*)'")]
        public void GivenIHaveACommandVariableEqualTo(string variable, string value)
        {
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"I have a command result equal to ""(.*)""")]
        [Given(@"I have a command result equal to '(.*)'")]
        public void GivenIHaveACommandResultEqualTo(string resultVar)
        {
            scenarioContext.Add("resVar", resultVar);
        }

    }
}
