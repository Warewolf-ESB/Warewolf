using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities;
using Dev2.Activities.Designers2.Decision;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.AcceptanceTesting.Core;
// ReSharper disable All

namespace Warewolf.AcceptanceTesting.Decision
{
    [Binding]
    public class DecisionDialogSteps
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
            var large = new Large();
            FeatureContext.Current.Add("view",large);
            var vm = new DecisionDesignerViewModel(mi);
            FeatureContext.Current["viewModel"] = vm;
            var dataContext = vm;
            large.DataContext = dataContext;
            Utils.ShowTheViewForTesting(large);
        }

        static ModelItem CreateModelItem(IEnumerable<DecisionTO> items, string displayName = "Find")
        {
            var dec = new DsfDecision();
            var modelItem = ModelItemUtils.CreateModelItem(dec);
            FeatureContext.Current["decision"] = dec;
            modelItem.SetProperty("DisplayName", displayName);
            return modelItem;
        }

        [Given(@"a decision variable ""(.*)"" operation ""(.*)"" right  ""(.*)"" position ""(.*)""")]
        public void GivenADecisionVariableOperationRightPosition(string left, string match, string right, int pos)
        {
            var vm = (DecisionDesignerViewModel)FeatureContext.Current["viewModel"];
            ((DecisionTO)vm.Tos[pos]).SearchCriteria = right;
            ((DecisionTO)vm.Tos[pos]).MatchValue = left;
            ((DecisionTO)vm.Tos[pos]).From = right;
            ((DecisionTO)vm.Tos[pos]).SearchType = match;
        }

        [When(@"I change decision variable position ""(.*)"" to ""(.*)""")]
        public void WhenIChangeDecisionVariablePositionTo(int pos, string match)
        {
            var vm = (DecisionDesignerViewModel)FeatureContext.Current["viewModel"];
            ((DecisionTO)vm.Tos[pos]).MatchValue = match;
        }

        [Given(@"is ""(.*)"" ""(.*)"" ""(.*)""")]
        public void GivenIs(string left, string op, string right)
        {
           var tos = (List<DecisionTO>) ScenarioContext.Current["Tos"];
           tos.Add(new DecisionTO(left,op,right,tos.Count));
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
            var to =   ((DecisionTO)vm.Tos[0]);
            var vis = p0.Split(new char[] { ',' });
            switch(vis.Count())
            {
                case 2:
                    Assert.IsTrue(to.IsSearchCriteriaEnabled);
                    Assert.IsTrue(to.SearchType.Length>0);
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


        [Then(@"decision variable ""(.*)"" is not visible")]
        public void ThenDecisionVariableIsNotVisible(string var)
        {
            var vm = (DecisionDesignerViewModel)FeatureContext.Current["viewModel"];
            Assert.IsFalse(vm.Tos.Any(a => ((DecisionTO)a).SearchCriteria.Contains(var)));
        }

        [When(@"I select the ""(.*)"" button at position (.*)")]
        public void WhenISelectTheButtonAtPosition(string p0, int p1)
        {
            var vm = (DecisionDesignerViewModel)FeatureContext.Current["viewModel"];
            Assert.IsTrue(((DecisionTO)vm.Tos[p1]).CanDelete(((DecisionTO)vm.Tos[p1])));
            vm.DeleteRow((DecisionTO)vm.Tos[p1]);
        }

        [Then(@"""(.*)"" is removed from the decision")]
        public void ThenIsRemovedFromTheDecision(string var)
        {
            var vm = (DecisionDesignerViewModel)FeatureContext.Current["viewModel"];
            Assert.IsFalse(vm.Tos.Any(a=> ((DecisionTO)a).SearchCriteria.Contains(var)));
        }

        [Then(@"""(.*)"" is visible in Match field")]
        public void ThenIsVisibleInMatchField(string var)
        {
            var vm = (DecisionDesignerViewModel)FeatureContext.Current["viewModel"];
            vm.GetExpressionText();
            Assert.IsTrue(vm.ExpressionText.Contains(var));
        }
    }
}
