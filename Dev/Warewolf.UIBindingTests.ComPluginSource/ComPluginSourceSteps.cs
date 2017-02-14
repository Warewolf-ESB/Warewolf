using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.SaveDialog;
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

    [BeforeFeature("ComPluginSource")]
    public static void SetupForSystem()
    {
        
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
       
        FeatureContext.Current.Add(Utils.ViewNameKey, sourceControl);
        FeatureContext.Current.Add(Utils.ViewModelNameKey, viewModel);
        FeatureContext.Current.Add("updateManager", mockStudioUpdateManager);
        FeatureContext.Current.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
        FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
        FeatureContext.Current.Add("eventAggregator", mockEventAggregator);
        FeatureContext.Current.Add("synchronousAsyncWorker", mockSynchronousAsyncWorker);

    }

    private static ManageComPluginSourceControl CreateSourceControl()
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
        ScenarioContext.Current.Add(Utils.ViewNameKey, FeatureContext.Current.Get<ManageComPluginSourceControl>(Utils.ViewNameKey));
        ScenarioContext.Current.Add("updateManager", FeatureContext.Current.Get<Mock<IManageComPluginSourceModel>>("updateManager"));
        ScenarioContext.Current.Add("requestServiceNameViewModel", FeatureContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
        ScenarioContext.Current.Add(Utils.ViewModelNameKey, FeatureContext.Current.Get<ManageComPluginSourceViewModel>(Utils.ViewModelNameKey));
    }
    [AfterScenario("ComPluginSource")]
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

    [Then(@"""(.*)"" tab is opened")]
    public void ThenTabIsOpened(string headerText)
    {
        var viewModel = ScenarioContext.Current.Get<IDockAware>("viewModel");
        Assert.AreEqual(headerText, viewModel.Header);
    }

    [Then(@"title is ""(.*)""")]
    public void ThenTitleIs(string title)
    {
        var sourceControl = ScenarioContext.Current.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        var viewModel = ScenarioContext.Current.Get<ManageComPluginSourceViewModel>("viewModel");
        Assert.AreEqual(title, viewModel.HeaderText);
        Assert.AreEqual(title, sourceControl.GetHeaderText());
    }
    [Then(@"Footerlabel is ""(.*)""")]
    public void ThenFooterlabelIs(string assembly)
    {
        var managePluginSourceControl = ScenarioContext.Current.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        Assert.AreEqual(assembly, managePluginSourceControl.GetAssemblyName());
    }

    [When(@"I click")]
    public void WhenIClick(Table table)
    {
        var sourceControl = ScenarioContext.Current.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
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
        Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey), Utils.ViewNameKey);
    }

    [When(@"I change Assembly to ""(.*)""")]
    public void WhenIChangeAssemblyTo(string assemblyName)
    {
        var sourceControl = ScenarioContext.Current.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        sourceControl.SetAssemblyName(assemblyName);
        var viewModel = ScenarioContext.Current.Get<ManageComPluginSourceViewModel>("viewModel");
        var assemblyNameOnViewModel = viewModel.AssemblyName;
        var isSameAsViewModel = assemblyName.Equals(assemblyNameOnViewModel, StringComparison.OrdinalIgnoreCase);
        Assert.IsTrue(isSameAsViewModel);
    }
    [When(@"I save as ""(.*)""")]
    public void WhenISaveAs(string resourceName)
    {
        var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
        mockRequestServiceNameViewModel.Setup(model => model.ResourceName).Returns(new ResourceName("", resourceName));
        mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Returns(MessageBoxResult.OK);
        var manageWebserviceSourceControl = ScenarioContext.Current.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        manageWebserviceSourceControl.PerformSave();
    }
    [Then(@"the save dialog is opened")]
    public void ThenTheSaveDialogIsOpened()
    {
        var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
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

        var managePluginSourceControl = ScenarioContext.Current.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        var mockStudioUpdateManager = FeatureContext.Current.Get<Mock<IManageComPluginSourceModel>>("updateManager").Object;
        var mockEventAggregator = FeatureContext.Current.Get<Mock<IEventAggregator>>("eventAggregator").Object;
        var mockSynchronousAsyncWorker = FeatureContext.Current.Get<Mock<SynchronousAsyncWorker>>("synchronousAsyncWorker").Object;

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
            ScenarioContext.Current.Remove("viewModel");
            ScenarioContext.Current.Add("viewModel", managePluginSourceViewModel);
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
        var sourceControl = ScenarioContext.Current.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        var assemblyNameOnControl = sourceControl.GetAssemblyName();
        var isSameAsControl = assemblyName.Equals(assemblyNameOnControl, StringComparison.OrdinalIgnoreCase);
        Assert.IsTrue(isSameAsControl);
        var viewModel = ScenarioContext.Current.Get<ManageComPluginSourceViewModel>("viewModel");
        var assemblyNameOnViewModel = viewModel.SelectedDll.FullName;
        var isSameAsViewModel = assemblyName.Equals(assemblyNameOnViewModel, StringComparison.OrdinalIgnoreCase);
        Assert.IsTrue(isSameAsViewModel);
    }

    [Given(@"I click ""(.*)""")]
    [When(@"I click ""(.*)""")]
    [Then(@"I click ""(.*)""")]
    public void ThenIClick(string itemName)
    {
        var sourceControl = ScenarioContext.Current.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        var dllListingModel = sourceControl.SelectItem(itemName);
        Assert.IsNotNull(dllListingModel);
        var viewModel = ScenarioContext.Current.Get<ManageComPluginSourceViewModel>("viewModel");
        Assert.AreEqual(dllListingModel, viewModel.SelectedDll);
        Assert.IsTrue(viewModel.SelectedDll.IsExpanded);
    }
    [When(@"I save Plugin source")]
    public void WhenISavePluginSource()
    {
        var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
        mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Verifiable();
        var managePluginSourceControl = ScenarioContext.Current.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        managePluginSourceControl.PerformSave();
    }

    [Given(@"DLLs is ""(.*)""")]
    public void GivenDLLsIs(string p0)
    {
        var managePluginSourceControl = ScenarioContext.Current.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
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
        var manageWebserviceSourceControl = ScenarioContext.Current.Get<ManageComPluginSourceControl>(Utils.ViewNameKey);
        manageWebserviceSourceControl.ClearFilter();
    }




}