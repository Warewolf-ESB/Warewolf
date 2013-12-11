using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Scripting.Command
{
    [Binding]
    public class CommandSteps : RecordSetBases
    {
        private void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
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
            foreach (TableRow tableRow in commands)
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
            IDSFDataObject result = ExecuteProcess(throwException:false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the result of the command tool will be ""(.*)""")]
        public void ThenTheResultOfTheCommandToolWillBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace("\"\"", "");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.IsTrue(actualValue.Contains(actualValue));
        }

        [Given(@"I have a command variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveACommandVariableEqualTo(string variable, string value)
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

        [Then(@"command execution has ""(.*)"" error")]
        public void ThenCommandExecutionHasError(string anError)
        {
            bool expected = anError.Equals("NO");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            string fetchErrors = FetchErrors(result.DataListID);
            bool actual = string.IsNullOrEmpty(fetchErrors);
            string message = string.Format("expected {0} error but an error was {1}", anError,
                                           actual ? "not found" : "found" + fetchErrors);
            Assert.AreEqual(expected, actual, message);
        }
    }
}