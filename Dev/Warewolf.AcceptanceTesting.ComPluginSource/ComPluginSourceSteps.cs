using System.Collections.Generic;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.AcceptanceTesting.Core;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;

[Binding]
public class COMPluginSourceSteps
{
    [BeforeFeature("ComPluginSource")]
    public static void SetupForSystem()
    {
        Utils.SetupResourceDictionary();
        var sourceControl = new ManageComPluginSourceControl();
        var mockStudioUpdateManager = new Mock<IManageComPluginSourceModel>();
        mockStudioUpdateManager.Setup(model => model.ServerName).Returns("localhost");
        mockStudioUpdateManager.Setup(model => model.GetComDllListings(null)).Returns(BuildBaseListing());
        var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
        var mockEventAggregator = new Mock<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator>();
        var mockExecutor = new Mock<IExternalProcessExecutor>();
        var mockSynchronousAsyncWorker = new Mock<SynchronousAsyncWorker>();
        var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
        task.Start();
        var viewModel = new ManageComPluginSourceViewModel(mockStudioUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker());
        sourceControl.DataContext = viewModel;
        Utils.ShowTheViewForTesting(sourceControl);
        FeatureContext.Current.Add(Utils.ViewNameKey, sourceControl);
        FeatureContext.Current.Add(Utils.ViewModelNameKey, viewModel);
        FeatureContext.Current.Add("updateManager", mockStudioUpdateManager);
        FeatureContext.Current.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
        FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
        FeatureContext.Current.Add("eventAggregator", mockEventAggregator);
        FeatureContext.Current.Add("synchronousAsyncWorker", mockSynchronousAsyncWorker);

    }
    static IList<IFileListing> BuildBaseListing()
    {
        var listing = new List<IFileListing>();
        return listing;
    }
    [BeforeScenario("ComPluginSource")]
    public void SetupForPluginSource()
    {
        ScenarioContext.Current.Add(Utils.ViewNameKey, FeatureContext.Current.Get<ManageComPluginSourceControl>(Utils.ViewNameKey));
        ScenarioContext.Current.Add("updateManager", FeatureContext.Current.Get<Mock<IManageComPluginSourceModel>>("updateManager"));
        ScenarioContext.Current.Add("requestServiceNameViewModel", FeatureContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
        ScenarioContext.Current.Add(Utils.ViewModelNameKey, FeatureContext.Current.Get<ManageComPluginSourceViewModel>(Utils.ViewModelNameKey));
    }
    [AfterScenario("PluginSource")]
    public void Cleanup()
    {
        var mockExecutor = new Mock<IExternalProcessExecutor>();
        var mockUpdateManager = ScenarioContext.Current.Get<Mock<IManageComPluginSourceModel>>("updateManager");
        mockUpdateManager.Setup(model => model.GetComDllListings(null)).Returns(BuildBaseListing());
        var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
        var mockEventAggregator = new Mock<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator>();
        var mockViewModel = ScenarioContext.Current.Get<ManageComPluginSourceViewModel>(Utils.ViewModelNameKey);
        var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
        task.Start();
        var viewModel = new ManageComPluginSourceViewModel(mockUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker(), mockViewModel.DispatcherAction);
        var managePluginSourceControl = ScenarioContext.Current.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        managePluginSourceControl.DataContext = viewModel;
        FeatureContext.Current.Remove("viewModel");
        FeatureContext.Current.Add("viewModel", viewModel);
        FeatureContext.Current.Remove("externalProcessExecutor");
        FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
    }

    [Given(@"I open New COMPlugin Source")]
    [When(@"I open New COMPlugin Source")]
    public void GivenIOpenNewPluginSource()
    {
        var sourceControl = ScenarioContext.Current.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        Assert.IsNotNull(sourceControl);
        Assert.IsNotNull(sourceControl.DataContext);
    }

}