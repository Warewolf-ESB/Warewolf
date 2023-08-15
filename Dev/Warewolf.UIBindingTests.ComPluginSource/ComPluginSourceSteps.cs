using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.SaveDialog;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.UIBindingTests.Core;
using Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;

[Binding]
public class COMPluginSourceSteps
{
    static FeatureContext _featureContext;
    readonly ScenarioContext _scenarioContext;

    public COMPluginSourceSteps(ScenarioContext scenarioContext) => _scenarioContext = scenarioContext;

    [BeforeFeature("ComPluginSource")]
    public static void SetupForSystem(FeatureContext featureContext)
    {
        _featureContext = featureContext;
        Utils.SetupResourceDictionary();

        var sourceControl = CreateSourceControl();
       
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
       
        _featureContext.Add(Utils.ViewNameKey, sourceControl);
        _featureContext.Add(Utils.ViewModelNameKey, viewModel);
        _featureContext.Add("updateManager", mockStudioUpdateManager);
        _featureContext.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
        _featureContext.Add("externalProcessExecutor", mockExecutor);
        _featureContext.Add("eventAggregator", mockEventAggregator);
        _featureContext.Add("synchronousAsyncWorker", mockSynchronousAsyncWorker);
    }

    static ManageComPluginSourceControl CreateSourceControl()
    {
        return new ManageComPluginSourceControl();
    }

    static IList<IFileListing> BuildBaseListing()
    {
        var listing = new List<IFileListing>
        {
            new DllListing
            {
                Name = "Development",
                ClsId = "DevClsid"
            }
        };
        return listing;
    }

    [BeforeScenario("ComPluginSource")]
    public void SetupForPluginSource()
    {
        _scenarioContext.Add(Utils.ViewNameKey, _featureContext.Get<ManageComPluginSourceControl>(Utils.ViewNameKey));
        _scenarioContext.Add("updateManager", _featureContext.Get<Mock<IManageComPluginSourceModel>>("updateManager"));
        _scenarioContext.Add("requestServiceNameViewModel", _featureContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
        _scenarioContext.Add(Utils.ViewModelNameKey, _featureContext.Get<ManageComPluginSourceViewModel>(Utils.ViewModelNameKey));
    }
    [AfterScenario("ComPluginSource")]
    public void Cleanup()
    {
        var mockExecutor = new Mock<IExternalProcessExecutor>();
        var mockUpdateManager = _scenarioContext.Get<Mock<IManageComPluginSourceModel>>("updateManager");
        mockUpdateManager.Setup(model => model.GetComDllListings(null)).Returns(BuildBaseListing());
        var mockRequestServiceNameViewModel = _scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
        var mockEventAggregator = new Mock<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator>();
        var mockViewModel = _scenarioContext.Get<ManageComPluginSourceViewModel>(Utils.ViewModelNameKey);
        var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
        task.Start();
        var viewModel = new ManageComPluginSourceViewModel(mockUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker(), mockViewModel.DispatcherAction);
        var managePluginSourceControl = _scenarioContext.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        managePluginSourceControl.DataContext = viewModel;
        _featureContext.Remove("viewModel");
        _featureContext.Add("viewModel", viewModel);
        _featureContext.Remove("externalProcessExecutor");
        _featureContext.Add("externalProcessExecutor", mockExecutor);
    }

    [Given(@"I open New COMPlugin Source")]
    [When(@"I open New COMPlugin Source")]
    public void GivenIOpenNewPluginSource()
    {
        var sourceControl = _scenarioContext.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        Assert.IsNotNull(sourceControl);
    }

    [Then(@"""(.*)"" tab is opened")]
    public void ThenTabIsOpened(string headerText)
    {
        var viewModel = _scenarioContext.Get<IDockAware>("viewModel");
        Assert.AreEqual(headerText, viewModel.Header);
    }

    [Then(@"title is ""(.*)""")]
    public void ThenTitleIs(string title)
    {
        var sourceControl = _scenarioContext.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        var viewModel = _scenarioContext.Get<ManageComPluginSourceViewModel>("viewModel");
        Assert.AreEqual(title, viewModel.HeaderText);
        Assert.AreEqual(title, sourceControl.GetHeaderText());
    }
    [Then(@"Footerlabel is ""(.*)""")]
    public void ThenFooterlabelIs(string assembly)
    {
        var managePluginSourceControl = _scenarioContext.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        Assert.AreEqual(assembly, managePluginSourceControl.GetAssemblyName());
    }

    [When(@"I click")]
    public void WhenIClick(Table table)
    {
        var sourceControl = _scenarioContext.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        foreach (var row in table.Rows)
        {
            var dllListingModel = sourceControl.SelectItem(row["Clicks"]);
            Assert.IsNotNull(dllListingModel);
        }
    }

    [Given(@"""(.*)"" is ""(.*)""")]
    [When(@"""(.*)"" is ""(.*)""")]
    [Then(@"""(.*)"" is ""(.*)""")]
    public void GivenIs(string controlName, string enabledString)
    {
        Utils.CheckControlEnabled(controlName, enabledString, _scenarioContext.Get<ICheckControlEnabledView>(Utils.ViewNameKey), Utils.ViewNameKey);
    }

    [When(@"I change Assembly to ""(.*)""")]
    public void WhenIChangeAssemblyTo(string assemblyName)
    {
        var sourceControl = _scenarioContext.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        sourceControl.SetAssemblyName(assemblyName);
        var viewModel = _scenarioContext.Get<ManageComPluginSourceViewModel>("viewModel");
        var assemblyNameOnViewModel = viewModel.AssemblyName;
        var isSameAsViewModel = assemblyName.Equals(assemblyNameOnViewModel, StringComparison.OrdinalIgnoreCase);
        Assert.IsTrue(isSameAsViewModel);
    }
    [When(@"I save as ""(.*)""")]
    public void WhenISaveAs(string resourceName)
    {
        var mockRequestServiceNameViewModel = _scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
        mockRequestServiceNameViewModel.Setup(model => model.ResourceName).Returns(new ResourceName("", resourceName));
        mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Returns(MessageBoxResult.OK);
        var manageWebserviceSourceControl = _scenarioContext.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        manageWebserviceSourceControl.PerformSave();
    }
    [Then(@"the save dialog is opened")]
    public void ThenTheSaveDialogIsOpened()
    {
        var mockRequestServiceNameViewModel = _scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
        mockRequestServiceNameViewModel.Verify();
    }

    [Given(@"I open ""(.*)"" plugin source")]
    public void GivenIOpenPluginSource(string name)
    {
        var pluginSrc = new ComPluginSourceDefinition
        {
            ResourceName = name,
            Id = Guid.NewGuid(),
            ResourcePath = "",
            ClsId = Guid.NewGuid().ToString(),
            Is32Bit = true,
            SelectedDll = new DllListing() {ClsId = Guid.NewGuid().ToString(), Name = "Object", Is32Bit = false, FullName = "System.Object", IsDirectory = false},
        };

        var managePluginSourceControl = _scenarioContext.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        var mockStudioUpdateManager = _featureContext.Get<Mock<IManageComPluginSourceModel>>("updateManager").Object;
        var mockEventAggregator = _featureContext.Get<Mock<IEventAggregator>>("eventAggregator").Object;

        try
        {
            var managePluginSourceViewModel = new ManageComPluginSourceViewModel(mockStudioUpdateManager, mockEventAggregator, pluginSrc, new SynchronousAsyncWorker(), a => {
                try
                {
                    a.Invoke();
                }
                catch
                {
                    // ignored
                }
            });

            managePluginSourceControl.DataContext = managePluginSourceViewModel;
            _scenarioContext.Remove("viewModel");
            _scenarioContext.Add("viewModel", managePluginSourceViewModel);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    [Given(@"FooterLabel value is ""(.*)""")]
    [When(@"FooterLabel value is ""(.*)""")]
    [Then(@"FooterLabel value is ""(.*)""")]
    public void ThenAssemblyValueIs(string assemblyName)
    {
        var sourceControl = _scenarioContext.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        var assemblyNameOnControl = sourceControl.GetAssemblyName();
        var isSameAsControl = assemblyName.Equals(assemblyNameOnControl, StringComparison.OrdinalIgnoreCase);
        Assert.IsTrue(isSameAsControl);
        var viewModel = _scenarioContext.Get<ManageComPluginSourceViewModel>("viewModel");
        var assemblyNameOnViewModel = viewModel.SelectedDll.FullName;
        var isSameAsViewModel = assemblyName.Equals(assemblyNameOnViewModel, StringComparison.OrdinalIgnoreCase);
        Assert.IsTrue(isSameAsViewModel);
    }

    [Given(@"I click ""(.*)""")]
    [When(@"I click ""(.*)""")]
    [Then(@"I click ""(.*)""")]
    public void ThenIClick(string itemName)
    {
        var sourceControl = _scenarioContext.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        var dllListingModel = sourceControl.SelectItem(itemName);
        Assert.IsNotNull(dllListingModel);
        var viewModel = _scenarioContext.Get<ManageComPluginSourceViewModel>("viewModel");
        Assert.AreEqual(dllListingModel, viewModel.SelectedDll);
        Assert.IsTrue(viewModel.SelectedDll.IsExpanded);
    }
    [When(@"I save Plugin source")]
    public void WhenISavePluginSource()
    {
        var mockRequestServiceNameViewModel = _scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
        mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Verifiable();
        var managePluginSourceControl = _scenarioContext.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        managePluginSourceControl.PerformSave();
    }

    [Given(@"DLLs is ""(.*)""")]
    public void GivenDLLsIs(string p0)
    {
        var managePluginSourceControl = _scenarioContext.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        managePluginSourceControl.ExecuteRefresh();
    }
   
    [Given(@"I filter for ""(.*)""")]
    public void GivenIFilterFor(string assemblyName)
    {

        var expectedVisibility = String.Equals(assemblyName, "Development", StringComparison.InvariantCultureIgnoreCase);
        Assert.IsTrue(expectedVisibility);
    }

    [When(@"I ""(.*)"" the filter")]
    public void WhenITheFilter(string p0)
    {
        var manageWebserviceSourceControl = _scenarioContext.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        manageWebserviceSourceControl.ClearFilter();
    }




}