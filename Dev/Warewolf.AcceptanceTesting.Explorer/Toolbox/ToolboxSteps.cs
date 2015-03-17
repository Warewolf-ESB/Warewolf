using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Warewolf.AcceptanceTesting.Explorer.Toolbox
{
    [Binding]    
    public class ToolboxSteps
    {
        [BeforeFeature("Toolbox")]
        public static void SetupToolboxDependencies()
        {
            var bootstrapper = new UnityBootstrapperForToolboxTesting();
            bootstrapper.Run();
            FeatureContext.Current.Add("container", bootstrapper.Container);
            var view = bootstrapper.Container.Resolve<IToolboxView>();
            var viewModel = bootstrapper.Container.Resolve<IToolboxViewModel>();
            Utils.ShowTheViewForTesting(view);
            FeatureContext.Current.Add(Utils.ViewNameKey, view);
            FeatureContext.Current.Add(Utils.ViewModelNameKey, viewModel);
        }


        [BeforeScenario("Toolbox")]
        public void SetupForToolbox()
        {
            ScenarioContext.Current.Add(Utils.ViewNameKey, FeatureContext.Current.Get<IToolboxView>(Utils.ViewNameKey));
            ScenarioContext.Current.Add(Utils.ViewModelNameKey, FeatureContext.Current.Get<IToolboxViewModel>(Utils.ViewModelNameKey));
            
        }

        [Given(@"Toolbox is loaded")]
        public void GivenToolboxIsLoaded()
        {
            var view = Utils.GetView<IToolboxView>();
            Assert.IsNotNull(view);
        }

        [When(@"I search for ""(.*)"" in the toolbox")]
        public void WhenISearchForInTheToolbox(string searchTerm)
        {
            var view = Utils.GetView<IToolboxView>();
            var viewModel = Utils.GetViewModel<IToolboxViewModel>();
            view.EnterSearch(searchTerm);
            Assert.IsFalse(string.IsNullOrEmpty(viewModel.SearchTerm));

        }

        [Then(@"""(.*)"" is visible")]
        public void ThenIsVisible(string toolName)
        {
            var view = Utils.GetView<IToolboxView>();
            var isVisible = view.CheckToolIsVisible(toolName);
            Assert.IsTrue(isVisible);
        }

        [Then(@"all tools are not visible")]
        public void ThenAllToolsAreNotVisible()
        {
            var view = Utils.GetView<IToolboxView>();
            var isVisible = view.CheckAllToolsNotVisible();
            Assert.IsTrue(isVisible);
        }

        [Then(@"""(.*)"" is not visible")]
        public void ThenIsNotVisible(string toolName)
        {
            var view = Utils.GetView<IToolboxView>();
            var isVisible = view.CheckToolIsVisible(toolName);
            Assert.IsFalse(isVisible);
        }

        [When(@"I clear the toolbox filter")]
        public void WhenIClearTheToolboxFilter()
        {
            var view = Utils.GetView<IToolboxView>();
            view.ClearFilter();            
        }

        [Then(@"all tools are visible")]
        public void ThenAllToolsAreVisible()
        {
            var view = Utils.GetView<IToolboxView>();
            var toolCount = view.GetToolCount();
            Assert.AreEqual(9,toolCount);
        }

    }
}