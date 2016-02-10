using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Switch;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.AcceptanceTesting.Core;

namespace Warewolf.AcceptanceTesting.Switch
{
    [Binding]
    public class SwitchDialogSteps
    {

        [BeforeFeature()]
        public static void SetupForSystem()
        {
            Utils.SetupResourceDictionaryActivities();
    
            Dev2DecisionStack stack = new Dev2DecisionStack();
            var mi = CreateModelItem("");
            FeatureContext.Current["modelItem"] = mi;
            var vm = new SwitchDesignerViewModel(mi,"");
            var view = new ConfigureSwitch();
            FeatureContext.Current.Add("view", view);
            FeatureContext.Current["viewModel"] = vm;
            var dataContext = vm;
            view.DataContext = dataContext;
            Utils.ShowTheViewForTesting(view);
        }

        static ModelItem CreateModelItem(string swvar, string displayName = "Switch")
        {

            var dec = new DsfFlowSwitchActivity();
            var modelItem = ModelItemUtils.CreateModelItem(dec);



            FeatureContext.Current["switch"] = dec;
            modelItem.SetProperty("DisplayName", displayName);

            return modelItem;
        }

        [Given(@"I open a ""(.*)""")]
        public void GivenIOpenA(string p0)
        {

        }

        [Given(@"drop a ""(.*)"" tool onto the design surface")]
        public void GivenDropAToolOntoTheDesignSurface(string p0)
        {
            
        }

        [When(@"I set the switch arm as ""(.*)""")]
        public void WhenISetTheSwitchArmAs(string p0)
        {
            var vm = FeatureContext.Current["viewModel"] as SwitchDesignerViewModel;
            if(vm != null)
            {
                vm.SwitchExpression = p0;
            }
        }

        [Then(@"""(.*)"" is the display text")]
        public void ThenIsTheDisplayText(string p0)
        {
            var vm = FeatureContext.Current["viewModel"] as SwitchDesignerViewModel;
            
            Assert.IsTrue(vm != null && vm.Switch.SwitchExpression==p0);
        }

        [When(@"a validation error is shown")]
        public void WhenAValidationErrorIsShown()
        {

        }

        [Then(@"the Switch tool window is opened")]
        public void ThenTheSwitchToolWindowIsOpened()
        {
         
        }

        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string control, string state)
        {
            var view = FeatureContext.Current["view"] as ConfigureSwitch;
            if(view != null)
            {
                switch(control)
                {
                    case "Variable to Switch on":
                        view.CheckSwitchVariableState(state);
                        break;
                    case "Display text":
                        view.CheckDisplayState(state);
                        break;

                }
            }
        }

        [Then(@"I have variable ""(.*)"" equals ""(.*)""")]
        public void ThenIHaveVariableEquals(string p0, int p1)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"""(.*)"" equals ""(.*)""")]
        public void ThenEquals(string p0, string variable)
        {
            var view = FeatureContext.Current["view"] as ConfigureSwitch;
            if(view != null)
            {
                view.SetVariableToSwitchOn(variable);
            }
        }

        [Then(@"""(.*)"" is selected")]
        public void ThenIsSelected(string p0)
        {
           // from popup
        }

        [Then(@"the switch tool window is closed")]
        public void ThenTheSwitchToolWindowIsClosed()
        {
         // popup
        }



        [Then(@"I have variable ""(.*)"" equals ""(.*)""")]
        public void ThenIHaveVariableEquals(string p0, string variable)
        {
            var view = FeatureContext.Current["view"] as ConfigureSwitch;
            if(view != null)
            {
                view.SetVariableToSwitchOn(variable);
            }
        }

        [Then(@"""(.*)"" is changed to ""(.*)""")]
        public void ThenIsChangedTo(string p0, string variable)
        {
            var view = FeatureContext.Current["view"] as ConfigureSwitch;
            if(view != null)
            {
                Assert.AreEqual(variable,view.GetDisplayName());
            }
        }

        [Then(@"I set the default arm")]
        public void ThenISetTheDefaultArm()
        {
     
        }

        [Then(@"I set the switch arm as ""(.*)""")]
        public void ThenISetTheSwitchArmAs(string p0)
        {
           var vm =   FeatureContext.Current["viewModel"] as SwitchDesignerViewModel;
            if(vm != null)
            {
                vm.SwitchExpression = p0;
                PrivateObject po = new PrivateObject(vm);
                po.Invoke("Initialise", new object[0]);
            }
        }

        [Then(@"""(.*)"" is visible on the switch arm")]
        public void ThenIsVisibleOnTheSwitchArm(int p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"""(.*)"" is visible")]
        public void ThenIsVisible(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"""(.*)"" is visible in the variable list")]
        public void ThenIsVisibleInTheVariableList(string p0)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
