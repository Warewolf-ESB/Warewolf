using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common.Enums;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Scripting.Script
{
    [Binding]
    public class ScriptSteps : RecordSetBases
    {
        private DsfScriptingActivity _dsfScripting;

        private enScriptType _language;
        private string _scriptToExecute;

        public ScriptSteps()
            : base(new List<Tuple<string, string>>())
        {
        }

        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string>(ResultVariable, ""));

            _dsfScripting = new DsfScriptingActivity
                {
                    Script = _scriptToExecute,
                    ScriptType = _language,
                    Result = ResultVariable
                };

            TestStartNode = new FlowStep
                {
                    Action = _dsfScripting
                };
        }

        [Given(@"I have a script variable ""(.*)"" with this value ""(.*)""")]
        public void GivenIHaveAScriptVariableWithThisValue(string variable, string value)
        {
            _variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"I have this script to execute ""(.*)""")]
        public void GivenIHaveThisScriptToExecute(string scriptFileName)
        {
            string resourceName = string.Format("Dev2.Activities.Specs.Toolbox.Scripting.Script.testfiles.{0}",
                                                scriptFileName);
            _scriptToExecute = ReadFile(resourceName);
        }

        [Given(@"I have selected the language as ""(.*)""")]
        public void GivenIHaveSelectedTheLanguageAs(string language)
        {
            _language = (enScriptType) Enum.Parse(typeof (enScriptType), language);
        }

        [When(@"I execute the script tool")]
        public void WhenIExecuteTheScriptTool()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }

        [Then(@"the script result should be ""(.*)""")]
        public void ThenTheScriptResultShouldBe(string result)
        {
            string error;
            string actualValue;
            result = result.Replace("\"\"", "");
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(result, actualValue);
        }

        [Then(@"script execution has ""(.*)"" error")]
        public void ThenScriptExecutionHasError(string anError)
        {
            bool expected = anError.Equals("NO");
            bool actual = string.IsNullOrEmpty(FetchErrors(_result.DataListID));
            string message = string.Format("expected {0} error but an error was {1}", anError,
                                           actual ? "not found" : "found");
            Assert.AreEqual(expected, actual, message);
        }
    }
}