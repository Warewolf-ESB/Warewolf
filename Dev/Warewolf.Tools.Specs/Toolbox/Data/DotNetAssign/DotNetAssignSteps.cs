using Dev2.Activities.PathOperations;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage.Interfaces;
using Warewolf.Tools.Specs.BaseTypes;

namespace Warewolf.Tools.Specs.Toolbox.Data.DotNetAssign
{
    public class DotNetAssignSteps : RecordSetBases
    {
        readonly ScenarioContext _scenarioContext;

        public DotNetAssignSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this._scenarioContext = scenarioContext;
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            _scenarioContext.TryGetValue("fieldCollection", out List<ActivityDTO> fieldCollection);

            var multiAssign = new DsfDotNetMultiAssignActivity();

            TestStartNode = new FlowStep
            {
                Action = multiAssign
            };

            foreach (var field in fieldCollection)
            {
                multiAssign.FieldsCollection.Add(field);
            }
            _scenarioContext.Add("activity", multiAssign);
        }

        [Given(@"I dotnetassign the value ""(.*)"" to a variable ""(.*)""")]
        public void GivenIDotnetassignTheValueKimToAVariable(string value, string variable)
        {
            value = value.Replace('"', ' ').Trim();

            if (value.StartsWith("="))
            {
                value = value.Replace("=", "");
                value = string.Format("!~calculation~!{0}!~~calculation~!", value);
            }

            _scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            _scenarioContext.TryGetValue("fieldCollection", out List<ActivityDTO> fieldCollection);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                _scenarioContext.Add("variableList", variableList);
            }

            if (fieldCollection == null)
            {
                fieldCollection = new List<ActivityDTO>();
                _scenarioContext.Add("fieldCollection", fieldCollection);
            }

            fieldCollection.Add(new ActivityDTO(variable, value, 1, true));
        }

        [When(@"the dotnetassign tool is executed")]
        public void WhenTheDotnetassignToolIsExecuted()
        {
            BuildDataList();
            var result = ExecuteProcess(isDebug: true, throwException: false);
            _scenarioContext.Add("result", result);
        }

        [Given(@"the debug inputs count equals ""(.*)""")]
        [When(@"the debug inputs count equals ""(.*)""")]
        [Then(@"the debug inputs count equals ""(.*)""")]
        public void ThenTheDebugInputsCountEquals(int expectedCount)
        {
            _scenarioContext.TryGetValue("activity", out object baseAct);
            var containsKey = _scenarioContext.ContainsKey("activity");
            if (containsKey)
            {
                var dsfActivityAbstract = containsKey ? _scenarioContext.Get<DsfActivityAbstract<string>>("activity") : null;
                var result = _scenarioContext.Get<IDSFDataObject>("result");
                if (!result.Environment.HasErrors())
                {
                    var inputDebugItems = GetInputDebugItems(dsfActivityAbstract, result.Environment);
                    foreach (var inputDebug in inputDebugItems)
                    {
                        var resultListCount = inputDebug.ResultsList.Count;
                        Assert.AreEqual(expectedCount, resultListCount, "Expected Result List count to be " + expectedCount.ToString() + " but got " + resultListCount.ToString()); 
                    }
                }
            }
        }
        public List<DebugItem> GetInputDebugItems(Activity act, IExecutionEnvironment env)
        {
            var result = _scenarioContext.Get<IDSFDataObject>("result");
            if (act is DsfActivityAbstract<string> dsfActivityAbstractString)
            {
                return DebugItemResults(dsfActivityAbstractString, result.Environment);
            }
            var activity = _scenarioContext.Get<DsfActivityAbstract<string>>("activity");
            return DebugItemResults(activity, env);
        }
        List<DebugItem> DebugItemResults<T>(DsfActivityAbstract<T> dsfActivityAbstractString, IExecutionEnvironment dl)
        {
            var debugInputs = dsfActivityAbstractString.GetDebugInputs(dl, 0);
            return debugInputs.ToList();
        }
    }
}
