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

// ReSharper disable InconsistentNaming
namespace Warewolf.UIBindingTests.PluginSource
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
            var pluginSourceModel = new Mock<IManagePluginSourceModel>();
            pluginSourceModel.Setup(model => model.FetchSource(It.IsAny<Guid>()))
                             .Returns(new PluginSourceDefinition()
                             {
                                 Name = "Test",
                                 GACAssemblyName = "GAC:AuditPolicyGPManagedStubs, Version=6.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
                             });
            pluginSourceModel.Setup(model => model.ServerName).Returns("localhost");
            pluginSourceModel.Setup(model => model.GetDllListings(null)).Returns(BuildBaseListing());
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var mockSynchronousAsyncWorker = new Mock<SynchronousAsyncWorker>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var viewModel = new ManagePluginSourceViewModel(pluginSourceModel.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker());
            sourceControl.DataContext = viewModel;
            Utils.ShowTheViewForTesting(sourceControl);
            FeatureContext.Current.Add(Utils.ViewNameKey, sourceControl);
            FeatureContext.Current.Add(Utils.ViewModelNameKey, viewModel);
            FeatureContext.Current.Add("updateManager", pluginSourceModel);
            FeatureContext.Current.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
            FeatureContext.Current.Add("eventAggregator", mockEventAggregator);
            FeatureContext.Current.Add("synchronousAsyncWorker", mockSynchronousAsyncWorker);
        }

        static IList<IFileListing> BuildBaseListing()
        {
            var listing = new List<IFileListing>();
            var fileSystemListing = new DllListing { FullName = "", IsDirectory = true, Name = "File System" };
            _dllListingForGac = new DllListing
            {
                FullName = "GAC:AuditPolicyGPManagedStubs, Version=6.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
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
                FullName = "C:\\",
                IsDirectory = true,
                Name = "C:\\",
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
            var dDrive = new DllListing { FullName = "D:\\", IsDirectory = true, Name = "D:\\" };
            fileSystemListing.Children = new List<IFileListing> { cDrive, dDrive };
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
            Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey), Utils.ViewNameKey);
        }

        [Then(@"ConfigFile textbox is ""(.*)""")]
        public void ThenConfigFileTextboxIs(string enabledString)
        {
            var viewModel = ScenarioContext.Current.Get<ManagePluginSourceViewModel>("viewModel");
            switch (enabledString)
            {
                case "Enabled":
                    Assert.IsTrue(viewModel.CanSelectConfigFiles);
                    break;
                case "Disabled":
                    Assert.IsFalse(viewModel.CanSelectConfigFiles);
                    break;
            }
        }

        [Then(@"ConfigFileButton button is ""(.*)""")]
        public void ThenConfigFileButtonButtonIs(string enabledString)
        {
            var viewModel = ScenarioContext.Current.Get<ManagePluginSourceViewModel>("viewModel");
            switch (enabledString)
            {
                case "Enabled":
                    Assert.IsTrue(viewModel.CanSelectConfigFiles);
                    break;
                case "Disabled":
                    Assert.IsFalse(viewModel.CanSelectConfigFiles);
                    break;
            }
        }

        [Then(@"I type ""(.*)"" in ""(.*)""")]
        [When(@"I type ""(.*)"" in ""(.*)""")]
        public void ThenITypeIn(string input, string controlName)
        {
            string assemblyNameOnViewModel = null;
            var viewModel = ScenarioContext.Current.Get<ManagePluginSourceViewModel>("viewModel");
            var sourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            switch (controlName)
            {
                case "AssemblyName":
                    viewModel.FileSystemAssemblyName = input;
                    assemblyNameOnViewModel = viewModel.FileSystemAssemblyName;
                    break;
                case "ConfigFile":
                    viewModel.ConfigFilePath = input;
                    assemblyNameOnViewModel = viewModel.ConfigFilePath;
                    break;
                case "GacAssemblyName":
                    viewModel.GACAssemblyName = input;
                    assemblyNameOnViewModel = viewModel.GACAssemblyName;
                    break;
            }

            sourceControl.SetTextBoxValue(controlName, input);
            var isSameAsViewModel = input.Equals(assemblyNameOnViewModel, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(isSameAsViewModel);
        }

        [Then(@"""(.*)"" value is ""(.*)""")]
        public void ThenValueIs(string controlName, string assemblyName)
        {
            var viewModel = ScenarioContext.Current.Get<ManagePluginSourceViewModel>("viewModel");
            switch (controlName)
            {
                case "AssemblyName":
                    Assert.AreEqual(assemblyName, viewModel.FileSystemAssemblyName);
                    break;
                case "ConfigFile":
                    Assert.AreEqual(assemblyName, viewModel.ConfigFilePath);
                    break;
                case "GacAssemblyName":
                    Assert.AreEqual(assemblyName, viewModel.GACAssemblyName);
                    break;
            }
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

            pluginSrc.GACAssemblyName = _dllListingForGac.FullName;
            pluginSrc.FileSystemAssemblyName = _dllListingForFile.FullName;

            var managePluginSourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            var mockStudioUpdateManager = FeatureContext.Current.Get<Mock<IManagePluginSourceModel>>("updateManager").Object;
            var mockEventAggregator = FeatureContext.Current.Get<Mock<IEventAggregator>>("eventAggregator").Object;
            var mockSynchronousAsyncWorker = FeatureContext.Current.Get<Mock<SynchronousAsyncWorker>>("synchronousAsyncWorker").Object;

            try
            {
                var managePluginSourceViewModel = new ManagePluginSourceViewModel(mockStudioUpdateManager, mockEventAggregator, pluginSrc, new SynchronousAsyncWorker());

                managePluginSourceControl.DataContext = managePluginSourceViewModel;
                ScenarioContext.Current.Remove("viewModel");
                ScenarioContext.Current.Add("viewModel", managePluginSourceViewModel);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        [When(@"I save Plugin source")]
        public void WhenISavePluginSource()
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Verifiable();
            var managePluginSourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            managePluginSourceControl.PerformSave();
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
            var viewModel = new ManagePluginSourceViewModel(mockUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker());
            var managePluginSourceControl = ScenarioContext.Current.Get<ManagePluginSourceControl>(Utils.ViewNameKey);
            managePluginSourceControl.DataContext = viewModel;
            FeatureContext.Current.Remove("viewModel");
            FeatureContext.Current.Add("viewModel", viewModel);
            FeatureContext.Current.Remove("externalProcessExecutor");
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
        }
    }
}
