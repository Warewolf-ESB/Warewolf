using TechTalk.SpecFlow;

namespace Warewolf.AcceptanceTesting.Explorer.Plugins
{
    [Binding]
    public class PluginServiceSteps
    {
        [BeforeFeature("PluginService")]
        public static void SetupForSystem()
        {
//            var bootStrapper = new UnityBootstrapperForPluginServiceConnectorTesting();
//            bootStrapper.Run();
//            var view = new ManageDatabaseServiceControl();
//            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
//            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Verifiable();
//            var mockDbServiceModel = new Mock<IDbServiceModel>();
//            SetupModel(mockDbServiceModel);
//            var viewModel = new ManageDatabaseServiceViewModel(mockDbServiceModel.Object, mockRequestServiceNameViewModel.Object);
//            view.DataContext = viewModel;
//
//            FeatureContext.Current.Add(Utils.ViewNameKey, view);
//            FeatureContext.Current.Add("viewModel", viewModel);
//            FeatureContext.Current.Add("model", mockDbServiceModel);
//            FeatureContext.Current.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);

        }

        [BeforeScenario("PluginService")]
        public void SetupForDatabaseService()
        {
//            ScenarioContext.Current.Add("view", FeatureContext.Current.Get<ManageDatabaseServiceControl>(Utils.ViewNameKey));
//            ScenarioContext.Current.Add("viewModel", FeatureContext.Current.Get<ManageDatabaseServiceViewModel>("viewModel"));
//            ScenarioContext.Current.Add("requestServiceNameViewModel", FeatureContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
//            ScenarioContext.Current.Add("model", FeatureContext.Current.Get<Mock<IDbServiceModel>>("model"));

        }

        [Given(@"I click New Plugin Service")]
        public void GivenIClick()
        {
            ScenarioContext.Current.Pending();
        }


        [Given(@"Select a source is focused")]
        public void GivenSelectASourceIsFocused()
        {
//            var view = Utils.GetView();
//            var isDataSourceFocused = view.IsSourceFocused();
//            Assert.IsTrue(isDataSourceFocused);
        }

    }
}
