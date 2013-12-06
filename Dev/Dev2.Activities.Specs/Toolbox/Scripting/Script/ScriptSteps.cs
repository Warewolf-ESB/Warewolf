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

        public ScriptSteps()
            : base(new List<Tuple<string, string>>())
        {
        }

        private DsfScriptingActivity _dsfScripting;
       
        private string _scriptToExecute;
        private enScriptType _language;
        
        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string>(ResultVariable, ""));

            _dsfScripting = new DsfScriptingActivity { Script = _scriptToExecute, ScriptType = _language, Result = ResultVariable };

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
        public void GivenIHaveThisScriptToExecute(string scriptToExecute)
        {
            _scriptToExecute = scriptToExecute;
        }

        [Given(@"I have selected the language as ""(.*)""")]
        public void GivenIHaveSelectedTheLanguageAs(string language)
        {
            _language = (enScriptType)Enum.Parse(typeof(enScriptType), language);
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
            Assert.IsTrue(actualValue.Contains(result));
        }
        
        [Then(@"script execution has ""(.*)"" error")]
        public void ThenScriptExecutionHasError(string anError)
        {
            var expected = anError.Equals("AN");
            var actual = !string.IsNullOrEmpty(FetchErrors(_result.DataListID));
            Assert.AreEqual(expected, actual);
        }
    }
}
