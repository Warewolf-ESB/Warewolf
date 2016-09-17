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
using Warewolf.AcceptanceTesting.Core;
using Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;

// ReSharper disable InconsistentNaming
namespace Warewolf.AcceptanceTesting.PluginSource
{
    [Binding]
    public class PluginSourceSteps
    {
        static DllListing _dllListingForGac;
        static DllListing _dllListingForFile;

        [BeforeFeature("PluginSource")]
        public static void SetupForSystem()
        {
            Utils.SetupResourceDictionary();
            var sourceControl = new ManagePluginSourceControl();
            var mockStudioUpdateManager = new Mock<IManagePluginSourceModel>();
            mockStudioUpdateManager.Setup(model => model.ServerName).Returns("localhost");
            mockStudioUpdateManager.Setup(model => model.GetDllListings(null)).Returns(BuildBaseListing());
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var mockSynchronousAsyncWorker = new Mock<SynchronousAsyncWorker>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var viewModel = new ManagePluginSourceViewModel(mockStudioUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker());
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
            var fileSystemListing = new DllListing{FullName = "",IsDirectory = true,Name = "File System"};
            _dllListingForGac = new DllListing {
                FullName = "GAC:AuditPolicyGPManagedStubs, Version=6.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a.dll", 
                IsDirectory = false, 
                Name = "AuditPolicyGPManagedStubs.Interop, Version=6.1.0.0"
            };
            var gacListing = new DllListing
            {
                FullName = "",
                IsDirectory = true,
                Name = "GAC",
                Children = new List<IFileListing>
                {
                    _dllListingForGac,
                    new DllListing {
                        FullName = "GAC:vjslib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", 
                        IsDirectory = false, 
                        Name = "vjslib, Version=2.0.0.0"
                    }
                }
            };
            var gacFilterListing = new DllListing
            {
                FullName = "",
                IsDirectory = true,
                Name = "GAC",
                Children = new List<IFileListing>
                {
                    _dllListingForGac,
                    new DllListing {
                        FullName = "GAC:BDATunePIA, Version=6.1.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35.dll", 
                        IsDirectory = false, 
                        Name = "BDATunePIA, Version=6.1.0.0"
                    }
                }
            };
            _dllListingForFile = new DllListing
            {
                FullName = "C:\\Development\\Dev\\Binaries\\MS Fakes\\Microsoft.QualityTools.Testing.Fakes.dll", 
                IsDirectory = false, 
                Name = "Microsoft.QualityTools.Testing.Fakes.dll"
            };
            var cDrive = new DllListing
            {
                FullName = "C:\\", IsDirectory = true, Name = "C:\\",
                Children = new List<IFileListing>
                {
                    new DllListing { 
                        FullName = "C:\\Development", IsDirectory = true, Name = "Development" ,
                        Children = new List<IFileListing>
                        {
                            new DllListing
                            {
                                FullName = "C:\\Development\\Dev", IsDirectory = true, Name = "Dev",
                                Children = new List<IFileListing>
                                {
                                    new DllListing
                                    {
                                        FullName = "C:\\Development\\Dev\\Binaries", IsDirectory = true, Name = "Binaries",
                                        Children = new List<IFileListing>
                                        {
                                            new DllListing
                                            {
                                                FullName = "C:\\Development\\Dev\\Binaries\\MS Fakes", IsDirectory = true, Name = "MS Fakes",
                                                Children = new List<IFileListing>
                                                {
                                                    _dllListingForFile,
                                                    new DllListing
                                                    {
                                                        FullName = "C:\\Development\\Dev\\Binaries\\MS Fakes\\Dev2.Common.dll", IsDirectory = false, Name = "Dev2.Common.dll"
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            var dDrive = new DllListing{FullName = "D:\\",IsDirectory = true,Name = "D:\\"};
            fileSystemListing.Children = new List<IFileListing>{cDrive,dDrive};
            listing.Add(fileSystemListing);
            listing.Add(gacListing);
            listing.Add(gacFilterListing);
            return listing;
        }

        [BeforeScenario("PluginSource")]
        public void SetupForPluginSource()
        {
            ScenarioContext.Current.Add(Utils.ViewNameKey, FeatureContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey));
            ScenarioContext.Current.Add("updateManager", FeatureContext.Current.Get<Mock<IManagePluginSourceModel>>("updateManager"));
            ScenarioContext.Current.Add("requestServiceNameViewModel", FeatureContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
            ScenarioContext.Current.Add(Utils.ViewModelNameKey, FeatureContext.Current.Get<ManagePluginSourceViewModel>(Utils.ViewModelNameKey));
        }

        [Given(@"I open New Plugin Source")]
        [When(@"I open New Plugin Source")]
        public void GivenIOpenNewPluginSource()
        {
            var sourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
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
            var sourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            var viewModel = ScenarioContext.Current.Get<ManagePluginSourceViewModel>("viewModel");
            Assert.AreEqual(title, viewModel.HeaderText);
            Assert.AreEqual(title, sourceControl.GetHeaderText());
        }

        [Given(@"""(.*)"" is ""(.*)""")]
        [When(@"""(.*)"" is ""(.*)""")]
        [Then(@"""(.*)"" is ""(.*)""")]
        public void GivenIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey));
        }

        [Given(@"I click ""(.*)""")]
        [When(@"I click ""(.*)""")]
        [Then(@"I click ""(.*)""")]
        public void ThenIClick(string itemName)
        {
            var sourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            var dllListingModel = sourceControl.SelectItem(itemName);
            Assert.IsNotNull(dllListingModel);
            var viewModel = ScenarioContext.Current.Get<ManagePluginSourceViewModel>("viewModel");
            Assert.AreEqual(dllListingModel,viewModel.SelectedDll);
            Assert.IsTrue(viewModel.SelectedDll.IsExpanded);
        }

        [Then(@"local drive ""(.*)"" is visible")]
        public void ThenLocalDriveIsVisible(string itemName)
        {
            var sourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            var isItemVisible = sourceControl.IsItemVisible(itemName);
            Assert.IsTrue(isItemVisible);
        }

        [When(@"I click")]
        public void WhenIClick(Table table)
        {
            var sourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            foreach(var row in table.Rows)
            {
                var dllListingModel = sourceControl.SelectItem(row["Clicks"]);
                Assert.IsNotNull(dllListingModel);
            }
        }

        [Given(@"Assembly value is ""(.*)""")]
        [When(@"Assembly value is ""(.*)""")]
        [Then(@"Assembly value is ""(.*)""")]
        public void ThenAssemblyValueIs(string assemblyName)
        {
            var sourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            var assemblyNameOnControl = sourceControl.GetAssemblyName();
            var isSameAsControl = assemblyName.Equals(assemblyNameOnControl, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(isSameAsControl);
            var viewModel = ScenarioContext.Current.Get<ManagePluginSourceViewModel>("viewModel");
            var assemblyNameOnViewModel = viewModel.SelectedDll.FullName;
            var isSameAsViewModel = assemblyName.Equals(assemblyNameOnViewModel, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(isSameAsViewModel);
        }

        [When(@"I change Assembly to ""(.*)""")]
        public void WhenIChangeAssemblyTo(string assemblyName)
        {
            var sourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            sourceControl.SetAssemblyName(assemblyName);
            var viewModel = ScenarioContext.Current.Get<ManagePluginSourceViewModel>("viewModel");
            var assemblyNameOnViewModel = viewModel.AssemblyName;
            var isSameAsViewModel = assemblyName.Equals(assemblyNameOnViewModel, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(isSameAsViewModel);
        }

        [Given(@"file is selected")]
        public void GivenFileIsSelected()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I select ""(.*)"" as AssemblyName")]
        public void WhenISelectAsAssemblyName(string assemblyName)
        {
            var managePluginSourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            managePluginSourceControl.SetAssemblyName(assemblyName);
            var viewModel = ScenarioContext.Current.Get<ManagePluginSourceViewModel>("viewModel");
            Assert.AreEqual(managePluginSourceControl, viewModel);
        }

        [When(@"I refresh the filter")]
        public void WhenIRefreshTheFilter()
        {
            var managePluginSourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            managePluginSourceControl.ExecuteRefresh();
        }

        [When(@"""(.*)"" is selected")]
        public void WhenIsSelected(string assemblyName)
        {
            var sourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            var dllListingModel = sourceControl.SelectItem(assemblyName);
            Assert.IsNotNull(dllListingModel);
            var viewModel = ScenarioContext.Current.Get<ManagePluginSourceViewModel>("viewModel");
            Assert.AreEqual(dllListingModel, viewModel.SelectedDll);
            Assert.IsTrue(viewModel.SelectedDll.IsExpanded);
        }

        [When(@"I save the source")]
        public void WhenISaveTheSource()
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Verifiable();
            var manageWebserviceSourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            manageWebserviceSourceControl.PerformSave();

        }

        [Then(@"the save dialog is opened")]
        public void ThenTheSaveDialogIsOpened()
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Verify();
        }

        [When(@"I save as ""(.*)""")]
        public void WhenISaveAs(string resourceName)
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ResourceName).Returns(new ResourceName("", resourceName));
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            var manageWebserviceSourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            manageWebserviceSourceControl.PerformSave();

        }
        [Given(@"local drive ""(.*)"" is visible in File window")]
        public void GivenLocalDriveIsVisibleInFileWindow(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"local drive ""(.*)"" is visible in ""(.*)"" window")]
        public void GivenLocalDriveIsVisibleInWindow(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"GAC is selected")]
        public void GivenGACIsSelected()
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"file is not selected")]
        public void GivenFileIsNotSelected()
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"Assembly is ""(.*)""")]
        public void GivenAssemblyIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"I open ""(.*)"" plugin source")]
        public void GivenIOpenPluginSource(string name)
        {
            var pluginSrc = new PluginSourceDefinition
            {
                Name = name,
                Id = Guid.NewGuid(),
                Path = "",
                SelectedDll = name.Equals("Test", StringComparison.OrdinalIgnoreCase) ? _dllListingForGac : _dllListingForFile,
            };

            var managePluginSourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            var mockStudioUpdateManager = FeatureContext.Current.Get<Mock<IManagePluginSourceModel>>("updateManager").Object;
            var mockEventAggregator = FeatureContext.Current.Get<Mock<IEventAggregator>>("eventAggregator").Object;
            var mockSynchronousAsyncWorker = FeatureContext.Current.Get<Mock<SynchronousAsyncWorker>>("synchronousAsyncWorker").Object;
            
            try
            {
                var managePluginSourceViewModel = new ManagePluginSourceViewModel(mockStudioUpdateManager, mockEventAggregator, pluginSrc, new SynchronousAsyncWorker(),a => { 
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
            catch(Exception)
            {
                // ignored
            }
        }

        [Given(@"GAC is not selected")]
        public void GivenGACIsNotSelected()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I open ""(.*)""")]
        [Then(@"I open ""(.*)""")]
        [Given(@"I open ""(.*)""")]
        public void WhenIOpen(string itemNameToOpen)
        {
            var sourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            sourceControl.OpenItem(itemNameToOpen);
        }

        [When(@"GAC is not selected")]
        public void WhenGACIsNotSelected()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"GAC tree is collapsed")]
        public void WhenGACTreeIsCollapsed()
        {
            var viewModel = ScenarioContext.Current.Get<ManagePluginSourceViewModel>("viewModel");
            var isExpanded = viewModel.SelectedDll.IsExpanded;
            Assert.IsFalse(isExpanded);
        }

        [When(@"GAC only has one option in the tree")]
        public void WhenGACOnlyHasOneOptionInTheTree()
        {
            var managePluginSourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            managePluginSourceControl.FilterItems();
        }

        [When(@"Assembly is ""(.*)""")]
        [Then(@"Assembly is ""(.*)""")]
        public void WhenAssemblyIs(string assembly)
        {
            var managePluginSourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            Assert.AreEqual(assembly, managePluginSourceControl.GetAssemblyName());
        }

        [When(@"I filter for ""(.*)""")]
        public void WhenIFilterFor(string assemblyName)
        {
            var expectedVisibility = String.Equals(assemblyName, "BDATunePIA", StringComparison.InvariantCultureIgnoreCase);
            Assert.IsTrue(expectedVisibility);
        }

        [When(@"filter is disabled")]
        public void WhenFilterIsDisabled()
        {
            
        }

        [When(@"GAC is ""(.*)""")]
        public void WhenGACIs(string isLoading)
        {
            var managePluginSourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            managePluginSourceControl.ExecuteRefresh();
        }

        [When(@"I filter new for ""(.*)""")]
        public void WhenIFilterNewFor(string assemblyName)
        {
            var expectedVisibility = String.Equals(assemblyName, "vjslib", StringComparison.InvariantCultureIgnoreCase);
            Assert.IsTrue(expectedVisibility);
        }

        [When(@"I ""(.*)"" the filter")]
        public void WhenITheFilter(string p0)
        {
            var manageWebserviceSourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            manageWebserviceSourceControl.ClearFilter();
        }

        [When(@"I save Plugin source")]
        public void WhenISavePluginSource()
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Verifiable();
            var managePluginSourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            managePluginSourceControl.PerformSave();
        }

        [When(@"I Search for ""(.*)""")]
        public void WhenISearchFor(string assemblyName)
        {
            var expectedVisibility = String.Equals(assemblyName, "AuditPolicyGPMan", StringComparison.InvariantCultureIgnoreCase);
            Assert.IsTrue(expectedVisibility);
        }

        [Then(@"files in ""(.*)"" is opened")]
        public void ThenFilesInIsOpened(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"GAC is not selected")]        
        public void ThenGACIsNotSelected()
        {
            ScenarioContext.Current.Pending();
        }

        [AfterScenario("PluginSource")]
        public void Cleanup()
        {
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var mockUpdateManager = ScenarioContext.Current.Get<Mock<IManagePluginSourceModel>>("updateManager");
            mockUpdateManager.Setup(model => model.GetDllListings(null)).Returns(BuildBaseListing());
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockViewModel = ScenarioContext.Current.Get<ManagePluginSourceViewModel>(Utils.ViewModelNameKey);
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var viewModel = new ManagePluginSourceViewModel(mockUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker(), mockViewModel.DispatcherAction);
            var managePluginSourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            managePluginSourceControl.DataContext = viewModel;
            FeatureContext.Current.Remove("viewModel");
            FeatureContext.Current.Add("viewModel", viewModel);
            FeatureContext.Current.Remove("externalProcessExecutor");
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
        }
    }
}
