using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Text;
using ActivityUnitTests;
using Dev2.Common.Enums;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Scripting.Script
{
    [Binding]
    public class ScriptSteps : BaseActivityUnitTest
    {
        private DsfScriptingActivity _dsfScripting;
       
        private IDSFDataObject _result;
        private const string ResultVariable = "[[result]]";
        private readonly List<Tuple<string, string>> _variableList = new List<Tuple<string, string>>();
        private string _scriptToExecute;
        private enScriptType _language;
        private string _recordSetName;



        private void BuildDataList()
        {
            _dsfScripting = new DsfScriptingActivity { Script = _scriptToExecute, ScriptType = _language, Result = ResultVariable };

            TestStartNode = new FlowStep
            {
                Action = _dsfScripting
            };

            var data = new StringBuilder();
            var shape = new StringBuilder();
            data.Append("<root>");
            shape.Append("<root>");

            int row = 1;
            foreach (var variable in _variableList)
            {
                string variableName = DataListUtil.RemoveLanguageBrackets(variable.Item1);
                if (variableName.Contains("(") && variableName.Contains(")"))
                {
                    variableName = variableName.Replace("(", "").Replace(")", "").Replace("*", "");
                    var variableNameSplit = variableName.Split(".".ToCharArray());
                    shape.Append(string.Format("<{0}>", variableNameSplit[0]));
                    shape.Append(string.Format("<{0}/>", variableNameSplit[1]));
                    shape.Append(string.Format("</{0}>", variableNameSplit[0]));

                    data.Append(string.Format("<{0}>", variableNameSplit[0]));
                    data.Append(string.Format("<{0}>{1}</{0}>", variableNameSplit[1], variable.Item2));
                    data.Append(string.Format("</{0}>", variableNameSplit[0]));

                    _recordSetName = variableNameSplit[0];
                }
                else
                {
                    shape.Append(string.Format("<{0}/>", variableName));
                    data.Append(string.Format("<{0}>{1}</{0}>", variableName, variable.Item2));
                }
                row++;
            }

            data.Append(string.Format("<{0}></{0}>", DataListUtil.RemoveLanguageBrackets(ResultVariable)));
            shape.Append(string.Format("<{0}></{0}>", DataListUtil.RemoveLanguageBrackets(ResultVariable)));
            data.Append("</root>");
            shape.Append("</root>");

            CurrentDl = shape.ToString();
            TestData = data.ToString();
        }

        [Given(@"I have a variable ""(.*)"" with this value ""(.*)""")]
        public void GivenIHaveAVariableWithThisValue(string variable, string value)
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
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.IsTrue(actualValue.Contains(result));
        }
    }
}
