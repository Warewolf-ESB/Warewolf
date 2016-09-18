using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Switch;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.UIBindingTests.Core;

namespace Warewolf.UIBindingTests.Switch
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
    }
}
