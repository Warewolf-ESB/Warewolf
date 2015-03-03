using System;
using System.Windows;
using TechTalk.SpecFlow;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;

namespace Warewolf.AcceptanceTesting.Explorer.DBService_Specs
{
    [Binding]
    // ReSharper disable once InconsistentNaming
    public class DBServiceSteps
    {
        [BeforeFeature("DBService")]
        public static void SetupForSystem()
        {
            var bootStrapper = new UnityBootstrapperForDatabaseServiceConnectorTesting();
            bootStrapper.Run();
            var view = new ManageDatabaseServiceControl();
            var viewModel = new ManageDatabaseServiceViewModel(true, new[] {""});
            view.DataContext = viewModel;
            Utils.ShowTheViewForTesting(view);
            FeatureContext.Current.Add("view", view);
            FeatureContext.Current.Add("viewModel", viewModel);
           
        }

        [BeforeScenario("DBService")]
        public void SetupForDatabaseService()
        {
            ScenarioContext.Current.Add("view", FeatureContext.Current.Get<ManageDatabaseServiceControl>("view"));
            ScenarioContext.Current.Add("viewModel", FeatureContext.Current.Get<ManageDatabaseServiceViewModel>("viewModel"));
        }

        [Given(@"I click New Data Base Service Connector")]
        public void GivenIClickNewDataBaseServiceConnector()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I select ""(.*)"" as data source")]
        public void WhenISelectAsDataSource(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I select ""(.*)"" as the action")]
        public void WhenISelectAsTheAction(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I test the action")]
        public void WhenITestTheAction()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I save")]
        public void WhenISave()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"""(.*)"" tab is opened")]
        public void ThenTabIsOpened(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Data Source is focused")]
        public void ThenDataSourceIsFocused()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"inputs are")]
        public void ThenInputsAre(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"outputs are")]
        public void ThenOutputsAre(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"input mappings are")]
        public void ThenInputMappingsAre(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"output mappings are")]
        public void ThenOutputMappingsAre(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Save Dialog is opened")]
        public void ThenSaveDialogIsOpened()
        {
            ScenarioContext.Current.Pending();
        }
    }
}
