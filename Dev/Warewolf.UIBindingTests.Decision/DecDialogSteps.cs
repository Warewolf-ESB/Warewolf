using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Activities.Designers2.Decision;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.TO;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.UIBindingTests.Core;

namespace Warewolf.UIBindingTests.Decision
{
    [Binding]
    public sealed class DecDialogSteps
    {
        static FeatureContext _featureContext;
        ScenarioContext _scenarioContext;

        public DecDialogSteps(ScenarioContext scenarioContext) => _scenarioContext = scenarioContext;
        
        [BeforeFeature()]
        public static void SetupForSystem(FeatureContext featureContext)
        {
            _featureContext = featureContext;
            Utils.SetupResourceDictionaryActivities();
            var tos = new List<DecisionTO>();
            _featureContext["Tos"] = tos;
            var stack = new Dev2DecisionStack();
            var mi = CreateModelItem(tos);
            _featureContext["modelItem"] = mi;
            var mockView = new Mock<IView>();

            _featureContext.Add("view", mockView.Object);
            
            var vm = new DecisionDesignerViewModel(mi);
            _featureContext["viewModel"] = vm;
            var dataContext = vm;
            mockView.SetupGet(view => view.DataContext).Returns(dataContext);
            Utils.ShowTheViewForTesting(mockView.Object);
            
        }
        static ModelItem CreateModelItem(IEnumerable<DecisionTO> items, string displayName = "Find")
        {
            var dec = new DsfDecision();
            var modelItem = ModelItemUtils.CreateModelItem(dec);
            _featureContext["decision"] = dec;
            modelItem.SetProperty("DisplayName", displayName);
            return modelItem;
        }

        [Given(@"drop a Decision tool onto the design surface")]
        public void GivenDropADecisionToolOntoTheDesignSurface()
        {
            var view = _featureContext.Get<IView>("view");
            _scenarioContext.Add("view", view);
            var vm = _featureContext.Get<DecisionDesignerViewModel>("viewModel");
            _scenarioContext.Add("viewModel", vm);
        }

        [Given(@"I have a workflow New Workflow")]
        public void GivenIHaveAWorkflowNewWorkflow()
        {
        }

        [Then(@"the Decision window is opened")]
        public void ThenTheDecisionWindowIsOpened()
        {
            //var decisionDesignerViewModel = _scenarioContext.Get<DecisionDesignerViewModel>("viewModel");
            var view = _scenarioContext.Get<IView>("view");
            Assert.IsNotNull(view);
        }

        [Then(@"""(.*)"" fields are ""(.*)""")]
        public void ThenFieldsAre(string p0, string p1)
        {
            var tuple = new Tuple<string, string>(p0, p1);
            _scenarioContext.Add("fields", tuple);
        }

        [Then(@"an empty row has been added")]
        public void ThenAnEmptyRowHasBeenAdded()
        {
            _scenarioContext.Pending();
        }

        [Then(@"the decision match variables ""(.*)""and match ""(.*)"" and to match""(.*)""")]
        public void ThenTheDecisionMatchVariablesAndMatchAndToMatch(string p0, string p1, string p2)
        {
            var vm = (DecisionDesignerViewModel)_featureContext["viewModel"];
            ((DecisionTO)vm.Tos[0]).MatchValue = p0;
            ((DecisionTO)vm.Tos[0]).From = p1;
            ((DecisionTO)vm.Tos[0]).To = p2;
        }

        [Then(@"MatchType  is ""(.*)""")]
        public void ThenMatchTypeIs(string p0)
        {
            var vm = (DecisionDesignerViewModel)_featureContext["viewModel"];
            ((DecisionTO)vm.Tos[0]).SearchType = p0;
        }

        [Then(@"the inputs are ""(.*)""")]
        public void ThenTheInputsAre(string p0)
        {
            var vm = (DecisionDesignerViewModel)_featureContext["viewModel"];
            var to = ((DecisionTO)vm.Tos[0]);
            var vis = p0.Split(new[] { ',' });
            switch (vis.Length)
            {
                case 2:
                    Assert.IsTrue(to.IsSearchCriteriaEnabled);
                    Assert.IsTrue(to.SearchType.Length > 0);
                    Assert.IsFalse(to.IsBetweenCriteriaVisible);
                    break;
                case 3:
                    Assert.IsTrue(to.IsSearchCriteriaVisible);
                    Assert.IsTrue(to.SearchType.Length > 0);
                    Assert.IsFalse(to.IsBetweenCriteriaVisible);
                    break;
                case 4:
                    Assert.IsTrue(to.IsSearchCriteriaVisible);
                    Assert.IsTrue(to.SearchType.Length > 0);
                    Assert.IsTrue(to.IsBetweenCriteriaVisible);
                    break;
                default:
                    Assert.Fail("unexpected test input");
                    break;
            }
        }

        [Then(@"a decision variable ""(.*)"" operation ""(.*)"" right  ""(.*)"" position ""(.*)""")]
        public void ThenADecisionVariableOperationRightPosition(string p0, string p1, string p2, int p3)
        {
            
        }

        [Then(@"the Decision tool window is closed")]
        public void ThenTheDecisionToolWindowIsClosed()
        {
            var vm = (DecisionDesignerViewModel)_featureContext["viewModel"];
            vm.Collapse();
        }

        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string p0, string p1)
        {
            _scenarioContext.Pending();
        }

        [When(@"I change decision variable position ""(.*)"" to ""(.*)""")]
        public void WhenIChangeDecisionVariablePositionTo(int p0, string p1)
        {
            _scenarioContext.Pending();
        }

        [When(@"""(.*)"" is selected")]
        public void WhenIsSelected(string p0)
        {
            _scenarioContext.Pending();
        }
        [Then(@"I open the Decision tool window")]
        public void ThenIOpenTheDecisionToolWindow()
        {
            _scenarioContext.Pending();
        }
        [Then(@"decision variable ""(.*)"" is not visible")]
        public void ThenDecisionVariableIsNotVisible(string p0)
        {
            _scenarioContext.Pending();
        }
        [Then(@"""(.*)"" is visible in Match field")]
        public void ThenIsVisibleInMatchField(string p0)
        {
            _scenarioContext.Pending();
        }
        [Then(@"""(.*)"" has a value of ""(.*)""")]
        public void ThenHasAValueOf(string p0, string p1)
        {
            _scenarioContext.Pending();
        }
        [Given(@"a decision variable ""(.*)"" value ""(.*)""")]
        public void GivenADecisionVariableValue(string p0, string p1)
        {
            var vm = (DecisionDesignerViewModel)_featureContext["viewModel"];
            vm.ExpressionText = p0;
            vm.DisplayText = p1;
        }

        [Given(@"Match Type equals ""(.*)""")]
        public void GivenMatchTypeEquals(string p0)
        {
            _scenarioContext.Pending();
        }

        [Given(@"""(.*)"" is selected")]
        public void GivenIsSelected(string p0)
        {
            _scenarioContext.Pending();
        }

        [Given(@"the Decision window is opened")]
        public void GivenTheDecisionWindowIsOpened()
        {
            _scenarioContext.Pending();
        }

        [Given(@"I select the ""(.*)"" menu")]
        public void GivenISelectTheMenu(string p0)
        {
            _scenarioContext.Pending();
        }

    }
}
