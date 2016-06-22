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
using Warewolf.AcceptanceTesting.Core;

namespace Warewolf.AcceptanceTesting.Decision
{
    [Binding]
    public sealed class DecDialogSteps
    {
        [BeforeFeature()]
        public static void SetupForSystem()
        {
            Utils.SetupResourceDictionaryActivities();
            var tos = new List<DecisionTO>();
            FeatureContext.Current["Tos"] = tos;
            Dev2DecisionStack stack = new Dev2DecisionStack();
            var mi = CreateModelItem(tos);
            FeatureContext.Current["modelItem"] = mi;
            var mockView = new Mock<IView>();

            FeatureContext.Current.Add("view", mockView.Object);
            
            var vm = new DecisionDesignerViewModel(mi);
            FeatureContext.Current["viewModel"] = vm;
            var dataContext = vm;
            mockView.SetupGet(view => view.DataContext).Returns(dataContext);
            Utils.ShowTheViewForTesting(mockView.Object);
            
        }
        static ModelItem CreateModelItem(IEnumerable<DecisionTO> items, string displayName = "Find")
        {
            var dec = new DsfDecision();
            var modelItem = ModelItemUtils.CreateModelItem(dec);
            FeatureContext.Current["decision"] = dec;
            modelItem.SetProperty("DisplayName", displayName);
            return modelItem;
        }

        [Given(@"drop a Decision tool onto the design surface")]
        public void GivenDropADecisionToolOntoTheDesignSurface()
        {
            var view = FeatureContext.Current.Get<IView>("view");
            ScenarioContext.Current.Add("view", view);
            var vm = FeatureContext.Current.Get<DecisionDesignerViewModel>("viewModel");
            ScenarioContext.Current.Add("viewModel", vm);
        }

        [Given(@"I have a workflow New Workflow")]
        public void GivenIHaveAWorkflowNewWorkflow()
        {
        }

        [Then(@"the Decision window is opened")]
        public void ThenTheDecisionWindowIsOpened()
        {
            //var decisionDesignerViewModel = ScenarioContext.Current.Get<DecisionDesignerViewModel>("viewModel");
            var view = ScenarioContext.Current.Get<IView>("view");
            Assert.IsNotNull(view);
        }

        [Then(@"""(.*)"" fields are ""(.*)""")]
        public void ThenFieldsAre(string p0, string p1)
        {
            Tuple<string, string> tuple = new Tuple<string, string>(p0, p1);
            ScenarioContext.Current.Add("fields", tuple);
        }

        [Then(@"an empty row has been added")]
        public void ThenAnEmptyRowHasBeenAdded()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the decision match variables ""(.*)""and match ""(.*)"" and to match""(.*)""")]
        public void ThenTheDecisionMatchVariablesAndMatchAndToMatch(string p0, string p1, string p2)
        {
            var vm = (DecisionDesignerViewModel)FeatureContext.Current["viewModel"];
            ((DecisionTO)vm.Tos[0]).MatchValue = p0;
            ((DecisionTO)vm.Tos[0]).From = p1;
            ((DecisionTO)vm.Tos[0]).To = p2;
        }

        [Then(@"MatchType  is ""(.*)""")]
        public void ThenMatchTypeIs(string p0)
        {
            var vm = (DecisionDesignerViewModel)FeatureContext.Current["viewModel"];
            ((DecisionTO)vm.Tos[0]).SearchType = p0;
        }

        [Then(@"the inputs are ""(.*)""")]
        public void ThenTheInputsAre(string p0)
        {
            var vm = (DecisionDesignerViewModel)FeatureContext.Current["viewModel"];
            var to = ((DecisionTO)vm.Tos[0]);
            var vis = p0.Split(new char[] { ',' });
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
            var vm = (DecisionDesignerViewModel)FeatureContext.Current["viewModel"];
            vm.Collapse();
        }

        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I change decision variable position ""(.*)"" to ""(.*)""")]
        public void WhenIChangeDecisionVariablePositionTo(int p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"""(.*)"" is selected")]
        public void WhenIsSelected(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        [Then(@"I open the Decision tool window")]
        public void ThenIOpenTheDecisionToolWindow()
        {
            ScenarioContext.Current.Pending();
        }
        [Then(@"decision variable ""(.*)"" is not visible")]
        public void ThenDecisionVariableIsNotVisible(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        [Then(@"""(.*)"" is visible in Match field")]
        public void ThenIsVisibleInMatchField(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        [Then(@"""(.*)"" has a value of ""(.*)""")]
        public void ThenHasAValueOf(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }
        [Given(@"a decision variable ""(.*)"" value ""(.*)""")]
        public void GivenADecisionVariableValue(string p0, string p1)
        {
            var vm = (DecisionDesignerViewModel)FeatureContext.Current["viewModel"];
            vm.ExpressionText = p0;
            vm.DisplayText = p1;
        }

        [Given(@"Match Type equals ""(.*)""")]
        public void GivenMatchTypeEquals(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"""(.*)"" is selected")]
        public void GivenIsSelected(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"the Decision window is opened")]
        public void GivenTheDecisionWindowIsOpened()
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"I select the ""(.*)"" menu")]
        public void GivenISelectTheMenu(string p0)
        {
            ScenarioContext.Current.Pending();
        }

    }
}
