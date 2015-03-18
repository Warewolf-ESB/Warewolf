using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Deploy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;

namespace Warewolf.AcceptanceTesting.Explorer.Deploy
{
    [Binding]
    public class DeployTabSteps
    {
        [BeforeFeature("Deploy")]
        public static void SetupForSystem()
        {
            var bootStrapper = new UnityBootstrapperForDatabaseSourceConnectorTesting();
            bootStrapper.Run();
            var databaseSourceControlView = new Mock<IDeployViewControl>();
            var manageDatabaseSourceControlViewModel = new Mock<IDeployViewModel>();
            databaseSourceControlView.Object.DataContext = manageDatabaseSourceControlViewModel;
            Utils.ShowTheViewForTesting(databaseSourceControlView.Object);
            FeatureContext.Current.Add(Utils.ViewNameKey, databaseSourceControlView.Object);
            FeatureContext.Current.Add(Utils.ViewModelNameKey, manageDatabaseSourceControlViewModel.Object);
        }

        [BeforeScenario("Deploy")]
        public void SetupForDatabaseSource()
        {
            ScenarioContext.Current.Add(Utils.ViewNameKey, FeatureContext.Current.Get<IDeployViewControl>(Utils.ViewNameKey));
            ScenarioContext.Current.Add(Utils.ViewModelNameKey, FeatureContext.Current.Get<IDeployViewModel>(Utils.ViewModelNameKey));
        }

        [Given(@"I have deploy tab opened")]
        public void GivenIHaveDeployTabOpened()
        {
            var view = Utils.GetView<IDeployViewControl>();
            Assert.IsNotNull(view);
        }

        [Given(@"selected Source Server is ""(.*)""")]
        [When(@"selected Source Server is ""(.*)""")]
        public void GivenSelectedSourceServerIs(string sourceServerName)
        {
            var view = Utils.GetView<IDeployViewControl>();
            view.SelectSourceServer(sourceServerName);
            var viewModel = Utils.GetViewModel<IDeployViewModel>();
            Assert.AreEqual(viewModel.SelectedSourceModel.Server.ResourceName,sourceServerName);
        }


        [Given(@"selected Destination Server is ""(.*)""")]
        [When(@"selected Destination Server is ""(.*)""")]
        public void GivenSelectedDestinationServerIs(string destinationServer)
        {
            var view = Utils.GetView<IDeployViewControl>();
            view.SelectDestinationServer(destinationServer);
            var viewModel = Utils.GetViewModel<IDeployViewModel>();
            Assert.AreEqual(viewModel.SelectedDestinationModel.Server.ResourceName, destinationServer);
        }

        [Given(@"I select ""(.*)"" from Source Server")]
        [When(@"I select ""(.*)"" from Source Server")]
        public void GivenISelectFromSourceServer(string resourceName)
        {
            var view = Utils.GetView<IDeployViewControl>();
            view.SelectSourceResource(resourceName);
            var viewModel = Utils.GetViewModel<IDeployViewModel>();
            var selectedItems = viewModel.SelectedSourceModel.SelectedItems;
            Assert.AreEqual(1,selectedItems.Count());
        }

        [Given(@"I deploy")]
        [When(@"I deploy")]
        public void GivenIDeploy()
        {
            var view = Utils.GetView<IDeployViewControl>();
            view.Deploy();
        }
             
        [When(@"I click OK on ""(.*)"" popup")]
        public void WhenIClickOKOnPopup(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I click Cancel on ""(.*)"" popup")]
        public void WhenIClickCancelOnPopup(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I click ""(.*)""")]
        public void WhenIClick(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I Select All Dependecies""")]
        public void WhenISelectAllDependecies()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I type ""(.*)"" in Destination Server filter")]
        public void WhenITypeInDestinationServerFilter(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I clear filter on Destination Server")]
        public void WhenIClearFilterOnDestinationServer()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I type ""(.*)"" in Source Server filter")]
        public void WhenITypeInSourceServerFilter(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I clear filter on Source Server")]
        public void WhenIClearFilterOnSourceServer()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Destination Server edit is ""(.*)""")]
        public void ThenDestinationServerEditIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"""(.*)"" is visibe")]
        public void ThenIsVisibe(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the validation message as ""(.*)""")]
        public void ThenTheValidationMessageAs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"deploy is successfull")]
        public void ThenDeployIsSuccessfull()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"""(.*)"" is visible on Destination Server")]
        public void ThenIsVisibleOnDestinationServer(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"""(.*)"" popup is shown")]
        public void ThenPopupIsShown(string p0, Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"deploy is not successfull")]
        public void ThenDeployIsNotSuccessfull()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"""(.*)"" from Source Server is ""(.*)""")]
        public void ThenFromSourceServerIs(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"""(.*)"" from Destination Server is ""(.*)""")]
        public void ThenFromDestinationServerIs(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I select ""(.*)"" from Destination Server")]
        public void ThenISelectFromDestinationServer(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I select ""(.*)"" from Source Server")]
        public void ThenISelectFromSourceServer(string p0)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
