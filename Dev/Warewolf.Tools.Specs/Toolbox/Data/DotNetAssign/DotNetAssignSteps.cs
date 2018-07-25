using System;
using System.Activities.Statements;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Warewolf.Tools.Specs.Toolbox.Data.DotNetAssign
{
    public class DotNetAssignSteps : RecordSetBases
    {
        readonly ScenarioContext scenarioContext;

        public DotNetAssignSteps(ScenarioContext scenarioContext)
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
            BuildShapeAndTestData();

            scenarioContext.TryGetValue("fieldCollection", out List<ActivityDTO> fieldCollection);

            var multiAssign = new DsfDotNetMultiAssignActivity();

            TestStartNode = new FlowStep
            {
                Action = multiAssign
            };

            foreach (var field in fieldCollection)
            {
                multiAssign.FieldsCollection.Add(field);
            }
            scenarioContext.Add("activity", multiAssign);
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

            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            scenarioContext.TryGetValue("fieldCollection", out List<ActivityDTO> fieldCollection);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            if (fieldCollection == null)
            {
                fieldCollection = new List<ActivityDTO>();
                scenarioContext.Add("fieldCollection", fieldCollection);
            }

            fieldCollection.Add(new ActivityDTO(variable, value, 1, true));
        }

        [When(@"the dotnetassign tool is executed")]
        public void WhenTheDotnetassignToolIsExecuted()
        {
            BuildDataList();
            var result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

    }
}
