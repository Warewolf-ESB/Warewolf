using Dev2.Core.Tests.Deploy;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.Deploy;
using Dev2.ViewModels.Deploy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Studio.Core.Specs.Deploy
{
    [Binding]
    public class DeploySteps : DeployViewModelTestBase
    {
        [Given(@"I have selected a work flow  that ""(.*)"" exist in the destination server")]
        public void GivenIHaveSelectedAWorkFlowThatExistInTheDestinationServer(string exists)
        {
            ScenarioContext.Current.Add("isResourceChecked", exists.Equals("does"));
        }
        
        [When(@"I click the deploy")]
        public void WhenIClickTheDeploy()
        {
            ViewModelDialogResults dialogResult;
            if (!ScenarioContext.Current.TryGetValue("dialogResults", out dialogResult))
            {
                dialogResult = ViewModelDialogResults.Cancel;
            }

            bool isResourceChecked = ScenarioContext.Current.Get<bool>("isResourceChecked");

            DeployViewModel deployViewModel;

            var deployStatsCalculator = SetupDeployViewModel(out deployViewModel);

            var isOverwriteMessageDisplayed = false;

            deployViewModel.ShowDialog = o =>
            {
                var viewModel = (DeployDialogViewModel)o;
                viewModel.DialogResult = dialogResult;
                isOverwriteMessageDisplayed = true;
            };

            SetupResources(deployStatsCalculator, isResourceChecked);
            deployViewModel.HasNoResourcesToDeploy = (i,o) =>  false; 
            deployViewModel.DeployCommand.Execute(null);

            ScenarioContext.Current.Add("isOverwriteMessageDisplayed", isOverwriteMessageDisplayed);
            ScenarioContext.Current.Add("deployedSuccessfully", deployViewModel.DeploySuccessfull);
        }

        [Given(@"the user selects ""(.*)"" when prompted to continue")]
        public void GivenTheUserSelectsWhenPromptedToContinue(string selected)
        {
            ScenarioContext.Current.Add("dialogResults", selected == "OK" ? ViewModelDialogResults.Okay : ViewModelDialogResults.Cancel);
        }
        
        [Then(@"the workflow ""(.*)"" be deployed on the destination server")]
        public void ThenTheWorkflowBeDeployedOnTheDestinationServer(string shouldBeDeployed)
        {
            bool deployedSuccessfully = ScenarioContext.Current.Get<bool>("deployedSuccessfully");
            Assert.AreEqual(shouldBeDeployed.Equals("should"), deployedSuccessfully);
        }
    }
}
