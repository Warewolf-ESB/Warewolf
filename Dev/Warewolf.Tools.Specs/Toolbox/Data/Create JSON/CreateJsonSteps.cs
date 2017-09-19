using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Data.Create_JSON
{
    public class CreateJsonSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public CreateJsonSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this.scenarioContext = scenarioContext;
        }

        [Given(@"I select variable ""(.*)"" with name ""(.*)""")]
        public void GivenISelectVariableWithName(string variable, string name)
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("toList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("toList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, name));
        }

        [Given(@"a result variable ""(.*)""")]
        public void GivenAResultVariable(string JsonString)
        {
            //JsonString = JsonString.Replace('"', ' ').Trim();
            scenarioContext.Add("JsonString", JsonString);
        }

        [When(@"the create json tool is executed")]
        public void WhenTheCreateJsonToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the value of ""(.*)"" should be ""(.*)""")]
        public void ThenTheValueOfShouldBe(string resultVariable, string expectedResult)
        {
            string error;
            string actualValue;
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(resultVariable),
                out actualValue, out error);
            if(string.IsNullOrEmpty(expectedResult))
            {
                Assert.IsTrue(string.IsNullOrEmpty(actualValue));
            }
            else
            {
                Assert.AreEqual(expectedResult, actualValue);
            }
        }

        [Given(@"I have a variable ""(.*)"" with value ""(.*)""")]
        public void GivenIHaveAVariableWithValue(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value));
        }
        protected override void BuildDataList()
        {
            List<Tuple<string, string>> toList;
            scenarioContext.TryGetValue("toList", out toList);

            if(toList == null)
            {
                toList = new List<Tuple<string, string>>();
                scenarioContext.Add("toList", toList);
            }

            // toList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            string json;
            scenarioContext.TryGetValue("JsonString", out json);

            var jsonTool = new DsfCreateJsonActivity
            {
                JsonString = json,
                JsonMappings = toList.Select(a => new JsonMappingTo { SourceName = a.Item1, DestinationName = a.Item2 }).ToList()
            };

            TestStartNode = new FlowStep
            {
                Action = jsonTool
            };
            scenarioContext.Add("activity", jsonTool);
        }
    }
}
