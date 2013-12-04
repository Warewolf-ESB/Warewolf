using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.ControlFlow.Switch
{
    [Binding]
    public class SwitchSteps : RecordSetBases
    {
        private DsfFlowSwitchActivity _flowSwitch;

        public SwitchSteps()
            : base(new List<Tuple<string, string>>())
        {

        }

        private void BuildDataList()
        {
            BuildShapeAndTestData();
            _flowSwitch = new DsfFlowSwitchActivity();

            var stack = new Dev2Switch { SwitchVariable = ((List<Tuple<string, string>>)_variableList).First().Item1 };

            string modelData = stack.ToWebModel();



            //_flowSwitch.ExpressionText = string.Join("", GlobalConstants.InjectedDecisionHandler, "(\"", modelData, "\",", GlobalConstants.InjectedDecisionDataListVariable, ")");




            






            //TestStartNode = new FlowStep
            //{
            //    Action = new Assign<TResult>
            //    {
            //        To = new ArgumentReference<TResult> { ArgumentName = outputResultKey },
            //        Value = new InArgument<TResult>(_flowSwitch)
            //    }
            //};
        }

        [Given(@"I need to switch on variable ""(.*)"" with the value ""(.*)""")]
        public void GivenINeedToSwitchOnVariableWithTheValue(string variable, string value)
        {
            _variableList.Add(new Tuple<string, string>(variable, value));
        }
        
        [Given(@"the switch cases are")]
        public void GivenTheSwitchCasesAre(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            foreach (TableRow t in tableRows)
            {
               var dosomething = t[0];
            }
        }
        
        [When(@"the switch tool is executed")]
        public void WhenTheSwitchToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }
        
        [Then(@"the switch result should be ""(.*)""")]
        public void ThenTheSwitchResultShouldBe(string result)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.IsTrue(actualValue.Contains(result));
        }
    }
}
