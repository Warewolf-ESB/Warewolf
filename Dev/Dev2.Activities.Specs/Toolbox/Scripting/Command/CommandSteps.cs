using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Scripting.Command
{
    [Binding]
    public class CommandSteps : RecordSetBases
    {
        private DsfExecuteCommandLineActivity _commandLine;
        private string _commandToExecute;

        public CommandSteps()
            : base(new List<Tuple<string, string>>())
        {
        }

        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string>(ResultVariable, ""));

            _commandLine = new DsfExecuteCommandLineActivity { CommandFileName = _commandToExecute, CommandResult = ResultVariable, };

            TestStartNode = new FlowStep
            {
                Action = _commandLine
            };
        }
        
        [Given(@"I have a drive ""(.*)"" with this value ""(.*)""")]
        public void GivenIHaveADriveWithThisValue(string variable, string value)
        {
            _variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"I have this command script to execute ""(.*)""")]
        public void GivenIHaveThisCommandScriptToExecute(string commandToExecute)
        {
            _commandToExecute = commandToExecute;
        }
        
        [When(@"the command tool is executed")]
        public void WhenTheCommandToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }
        
        [Then(@"the result of the command tool will be ""(.*)""")]
        public void ThenTheResultOfTheCommandToolWillBe(string result)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.IsTrue(actualValue.Contains(result));
        }
    }
}
