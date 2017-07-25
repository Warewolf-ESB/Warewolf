/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities.Scripting;
using Dev2.Common.Interfaces.Enums;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Scripting.Script
{
    [Binding]
    public class ScriptSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public ScriptSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException(nameof(scenarioContext));
            this.scenarioContext = scenarioContext;
        }

        [BeforeFeature("PythonFeature")]
        public static void SetupPython()
        {
            FeatureContext.Current.Add("pythonActivity", new DsfPythonActivity());
        }

        [BeforeFeature("JavascriptFeature")]
        public static void SetupJavascript()
        {
            FeatureContext.Current.Add("javascript", new DsfJavascriptActivity());
        }
        [BeforeFeature("RubyFeature")]
        public static void SetupRuby()
        {
            FeatureContext.Current.Add("rubyActivity", new DsfRubyActivity());
        }
        

        protected override void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            string scriptToExecute;
            scenarioContext.TryGetValue("scriptToExecute", out scriptToExecute);
            enScriptType language;
            scenarioContext.TryGetValue("language", out language);
            DsfJavascriptActivity javascriptActivity;
            scenarioContext.TryGetValue("javascript", out javascriptActivity);

            if (javascriptActivity != null)
            {
                javascriptActivity.Script = scriptToExecute;
                javascriptActivity.Result = ResultVariable;

                TestStartNode = new FlowStep
                {
                    Action = javascriptActivity
                };
                scenarioContext.Add("activity", javascriptActivity);
                return;
            }

            DsfPythonActivity pythonActivity;
            FeatureContext.Current.TryGetValue("pythonActivity", out pythonActivity);

            if (pythonActivity != null)
            {
                pythonActivity.Script = scriptToExecute;
                pythonActivity.Result = ResultVariable;

                TestStartNode = new FlowStep
                {
                    Action = pythonActivity
                };
                scenarioContext.Add("activity", pythonActivity);
                return;
            }

            DsfRubyActivity rubyActivity;
            FeatureContext.Current.TryGetValue("rubyActivity", out rubyActivity);

            if (rubyActivity != null)
            {
                rubyActivity.Script = scriptToExecute;
                rubyActivity.Result = ResultVariable;

                TestStartNode = new FlowStep
                {
                    Action = rubyActivity
                };
                scenarioContext.Add("activity", rubyActivity);
                return;
            }

            DsfScriptingActivity dsfScripting = new DsfScriptingActivity
            {
                Script = scriptToExecute,
                ScriptType = language,
                Result = ResultVariable
            };

            TestStartNode = new FlowStep
            {
                Action = dsfScripting
            };
            scenarioContext.Add("activity", dsfScripting);
        }

        [Given(@"I have a script variable ""(.*)"" with this value ""(.*)""")]
        public void GivenIHaveAScriptVariableWithThisValue(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"I have this script to execute ""(.*)""")]
        [Given(@"I have the script to execute ""(.*)""")]
        public void GivenIHaveThisScriptToExecute(string scriptFileName)
        {
            string scriptToExecute;
            if (DataListUtil.IsEvaluated(scriptFileName))
            {
                scriptToExecute = scriptFileName;
            }
            else
            {
                string resourceName = string.Format("Warewolf.ToolsSpecs.Toolbox.Scripting.Script.testfiles.{0}",
                                                    scriptFileName);
                scriptToExecute = ReadFile(resourceName);
            }
            scenarioContext.Add("scriptToExecute", scriptToExecute);
        }

        [Given(@"I have selected the language as ""(.*)""")]
        public void GivenIHaveSelectedTheLanguageAs(string language)
        {
            scenarioContext.Add("language", (enScriptType)Enum.Parse(typeof(enScriptType), language));
        }

        [When(@"I execute the script tool")]
        public void WhenIExecuteTheScriptTool()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the script result should be ""(.*)""")]
        public void ThenTheScriptResultShouldBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, ResultVariable, out actualValue, out error);
            if (string.IsNullOrEmpty(expectedResult))
            {
                Assert.IsTrue(string.IsNullOrEmpty(actualValue));
            }
            else
            {
                Assert.AreEqual(expectedResult, actualValue);
            }
        }
    }
}
