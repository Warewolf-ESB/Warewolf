using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common;
using Dev2.Data.Decision;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.ControlFlow.Decision
{
    [Binding]
    public class DecisionSteps : RecordSetBases
    {

        //private DsfFlowDecisionActivity _decisionActivity;
        private Dev2DecisionMode _mode;
        private enDecisionType _decision;
        private string _modelData;

        public DecisionSteps()
            : base(new List<Tuple<string, string>>())
        {

        }

        private void BuildDataList()
        {
            BuildShapeAndTestData();

            var decisionActivity = new DsfFlowDecisionActivity();

            var variables = (from v in (List<Tuple<string, string>>) _variableList select v.Item1).ToList();

            var dds = new Dev2DecisionStack { TheStack = new List<Dev2Decision>(), Mode = _mode };
            var dev2Decision = new Dev2Decision
                {
                    EvaluationFn = _decision,
                    Col1 = variables[0] ?? string.Empty,
                    Col2 = variables[1] ?? string.Empty
                   // Col3 = variables[2] ?? string.Empty
                };
            dds.AddModelItem(dev2Decision);

            _modelData = dds.ToVBPersistableModel();
            decisionActivity.ExpressionText = string.Join("", GlobalConstants.InjectedDecisionHandler, "(\"", _modelData, "\",", GlobalConstants.InjectedDecisionDataListVariable, ")");
            
            TestStartNode = new FlowStep
            {
                Action = decisionActivity
            };
        }
        
        [Given(@"I need to take a decision on variable ""(.*)"" with the value ""(.*)""")]
        public void GivenINeedToTakeADecisionOnVariableWithTheValue(string variable, string value)
        {
            _variableList.Add(new Tuple<string, string>(variable, value));
        }
        
        [Given(@"I want to find out if the \[\[A]] ""(.*)"" \[\[B]]")]
        public void GivenIWantToFindOutIfTheAB(string decision)
        {
            _decision = (enDecisionType)Enum.Parse(typeof(enDecisionType), decision);
        }

        [Given(@"the decision mode is ""(.*)""")]
        public void GivenTheDecisionModeIs(string mode)
        {
            _mode = (Dev2DecisionMode)Enum.Parse(typeof(Dev2DecisionMode), mode);
        }
        
        [When(@"the decision tool is executed")]
        public void WhenTheDecisionToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }
        
        [Then(@"the decision result should be ""(.*)""")]
        public void ThenTheDecisionResultShouldBe(string result)
        {
            var actual = new Dev2DataListDecisionHandler().ExecuteDecisionStack(_modelData, new List<string> { _result.DataListID.ToString() });
            bool expected = Boolean.Parse(result);
            Assert.AreEqual(expected , actual);
        }
    }
}
