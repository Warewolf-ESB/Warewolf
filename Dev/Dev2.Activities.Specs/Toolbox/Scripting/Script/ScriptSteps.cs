
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
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common.Interfaces.Enums;
using Dev2.Data.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Scripting.Script
{
    [Binding]
    public class ScriptSteps : RecordSetBases
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

            string scriptToExecute;
            ScenarioContext.Current.TryGetValue("scriptToExecute", out scriptToExecute);
            enScriptType language;
            ScenarioContext.Current.TryGetValue("language", out language);

            var dsfScripting = new DsfScriptingActivity
                {
                    Script = scriptToExecute,
                    ScriptType = language,
                    Result = ResultVariable
                };

            TestStartNode = new FlowStep
                {
                    Action = dsfScripting
                };
            ScenarioContext.Current.Add("activity", dsfScripting);
        }

        [Given(@"I have a script variable ""(.*)"" with this value ""(.*)""")]
        public void GivenIHaveAScriptVariableWithThisValue(string variable, string value)
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

        [Given(@"I have this script to execute ""(.*)""")]
        public void GivenIHaveThisScriptToExecute(string scriptFileName)
        {
            string scriptToExecute;
            if(DataListUtil.IsEvaluated(scriptFileName))
            {
                scriptToExecute = scriptFileName;
            }
            else
            {
                string resourceName = string.Format("Dev2.Activities.Specs.Toolbox.Scripting.Script.testfiles.{0}",
                                                    scriptFileName);
                scriptToExecute = ReadFile(resourceName);
            }
            ScenarioContext.Current.Add("scriptToExecute", scriptToExecute);
        }

        [Given(@"I have selected the language as ""(.*)""")]
        public void GivenIHaveSelectedTheLanguageAs(string language)
        {
            ScenarioContext.Current.Add("language", (enScriptType)Enum.Parse(typeof(enScriptType), language));
        }

        [When(@"I execute the script tool")]
        public void WhenIExecuteTheScriptTool()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the script result should be ""(.*)""")]
        public void ThenTheScriptResultShouldBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(ResultVariable,out actualValue,out error);
            Assert.AreEqual(expectedResult, actualValue);
        }
    }
}
