using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Threading;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter;
using Warewolf.Studio.ServerProxyLayer;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;
using Warewolf.UIBindingTests.Core;

// ReSharper disable RedundantAssignment

namespace Warewolf.UIBindingTests.Odbc
{
    [Binding]
    public class NewDatabaseSourceSteps
    {
        string loginFailedForUserTest = "Login failed for user 'test'";
        [BeforeFeature("DbSource")]
        public static void SetupForSystem()
        {
            Utils.SetupResourceDictionary();
            var manageDatabaseSourceControl = new ManageDatabaseSourceControl();
            var mockStudioUpdateManager = new Mock<IManageDatabaseSourceModel>();
            mockStudioUpdateManager.Setup(model => model.GetComputerNames()).Returns(new List<string> { "TEST", "RSAKLFSVRGENDEV", "RSAKLFSVRPDC", "RSAKLFSVRTFSBLD", "RSAKLFSVRWRWBLD" });
            mockStudioUpdateManager.Setup(model => model.ServerName).Returns("localhost");
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var manageDatabaseSourceViewModel = new ManageOdbcSourceViewModel(mockStudioUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker());
            manageDatabaseSourceControl.DataContext = manageDatabaseSourceViewModel;
            Utils.ShowTheViewForTesting(manageDatabaseSourceControl);
            FeatureContext.Current.Add(Utils.ViewNameKey, manageDatabaseSourceControl);
            FeatureContext.Current.Add(Utils.ViewModelNameKey, manageDatabaseSourceViewModel);
            FeatureContext.Current.Add("updateManager", mockStudioUpdateManager);
            FeatureContext.Current.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
        }

        [BeforeScenario("ODBCSource")]
        public void SetupForDatabaseSource()
        {
            ScenarioContext.Current.Add(Utils.ViewNameKey, FeatureContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey));
            ScenarioContext.Current.Add("updateManager", FeatureContext.Current.Get<Mock<IManageDatabaseSourceModel>>("updateManager"));
            ScenarioContext.Current.Add("requestServiceNameViewModel", FeatureContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
            ScenarioContext.Current.Add(Utils.ViewModelNameKey, FeatureContext.Current.Get<ManageOdbcSourceViewModel>(Utils.ViewModelNameKey));
        }

        [When(@"I click ""(.*)""")]
        public void WhenIClick(string ConectTo)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.Test();
        }

        [Then(@"""(.*)"" tab is opened")]
        public void ThenTabIsOpened(string headerText)
        {
            var viewModel = ScenarioContext.Current.Get<IDockAware>("viewModel");
            Assert.AreEqual(headerText, viewModel.Header);
        }

        [Then(@"title is ""(.*)""")]
        public void ThenTitleIs(string p0)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            Assert.AreEqual(manageDatabaseSourceControl.GetHeader(), p0);
        }

        [Then(@"""(.*)"" is the tab Header")]
        public void ThenIsTheTabHeader(string p0)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            Assert.AreEqual(manageDatabaseSourceControl.GetTabHeader(), p0);
        }

        [Given(@"I open New Database Source")]
        public void GivenIOpenNewDatabaseSource()
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            Assert.IsNotNull(manageDatabaseSourceControl);
            Assert.IsNotNull(manageDatabaseSourceControl.DataContext);
        }

        [Given(@"I open ""(.*)""")]
        [When(@"I open ""(.*)""")]
        public void GivenIOpen(string name)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            var upd = FeatureContext.Current.Get<Mock<IManageDatabaseSourceModel>>("updateManager").Object;
            var dbsrc = new DbSourceDefinition
            {
                Name = name,
                Id = Guid.NewGuid(),
                ServerName = "Localhost",
                AuthenticationType = AuthenticationType.Windows
            };
            FeatureContext.Current["dbsrc"] = dbsrc;
            var mockEventAggregator = new Mock<IEventAggregator>();
            var viewModel = new ManageOdbcSourceViewModel(upd, mockEventAggregator.Object, dbsrc, new SynchronousAsyncWorker());
            var manageDatabaseSourceViewModel = manageDatabaseSourceControl.DataContext as ManageOdbcSourceViewModel;
            if (manageDatabaseSourceViewModel != null)
            {
                Utils.ResetViewModel<ManageOdbcSourceViewModel, IDbSource>(viewModel, manageDatabaseSourceViewModel);
            }
        }

        [Given(@"Database ""(.*)"" is selected")]
        public void GivenDatabaseIsSelected(string dbName)
        {
            var db = FeatureContext.Current.Get<IDbSource>("dbsrc");
            db.DbName = dbName;
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SelectDatabase(dbName);
        }

        [Then(@"Database ""(.*)"" is selected")]
        public void ThenDatabaseIsSelected(string dbName)
        {
            var db = FeatureContext.Current.Get<IDbSource>("dbsrc");
            db.DbName = dbName;
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SelectDatabase(dbName);
        }
        

        [Given(@"Database dropdown is ""(.*)""")]
        [When(@"Database dropdown is ""(.*)""")]
        [Then(@"Database dropdown is ""(.*)""")]
        public void GivenDropdownIs(string visibility)
        {
            var expectedVisibility = String.Equals(visibility, "Collapsed", StringComparison.InvariantCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;

            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            var databaseDropDownVisibility = manageDatabaseSourceControl.GetDatabaseDropDownVisibility();
            Assert.AreEqual(expectedVisibility, databaseDropDownVisibility);
        }

        [Given(@"""(.*)"" is ""(.*)""")]
        [When(@"""(.*)"" is ""(.*)""")]
        [Then(@"""(.*)"" is ""(.*)""")]
        public void GivenIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey), Utils.ViewNameKey);
        }

        [Given(@"I select ""(.*)"" as Database")]
        [When(@"I select ""(.*)"" as Database")]
        [Then(@"I select ""(.*)"" as Database")]
        public void WhenISelectAsDatabase(string databaseName)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SelectDatabase(databaseName);
            var viewModel = (ManageOdbcSourceViewModel)manageDatabaseSourceControl.DataContext;
            Assert.AreEqual(databaseName, viewModel.DatabaseName);
        }

        [When(@"I save the source")]
        public void WhenISaveTheSource()
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Verifiable();
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.PerformSave();
        }

        [When(@"I save the source as ""(.*)""")]
        public void WhenISaveTheSourceAs(string name)
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Returns(MessageBoxResult.OK).Verifiable();
            mockRequestServiceNameViewModel.Setup(a => a.ResourceName).Returns(new ResourceName("", name));
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.PerformSave();
        }
        

        [Then(@"the error message is ""(.*)""")]
        public void ThenTheErrorMessageIs(string errorMessage)
        {
            errorMessage = "Exception: " + loginFailedForUserTest + Environment.NewLine + Environment.NewLine +
                           "Inner Exception: " + loginFailedForUserTest;
            var viewModel = ScenarioContext.Current.Get<ManageOdbcSourceViewModel>("viewModel");
            Assert.AreEqual(errorMessage, viewModel.TestMessage);
        }


        [Then(@"Test Connecton is ""(.*)""")]
        [When(@"Test Connecton is ""(.*)""")]
        public void ThenTestConnectonIs(string successString)
        {
            var mockUpdateManager = ScenarioContext.Current.Get<Mock<IManageDatabaseSourceModel>>("updateManager");
            var isSuccess = String.Equals(successString, "Successful", StringComparison.InvariantCultureIgnoreCase);
            var isLongRunning = String.Equals(successString, "Long Running", StringComparison.InvariantCultureIgnoreCase);
            if (isSuccess)
            {
                mockUpdateManager.Setup(manager => manager.TestDbConnection(It.IsAny<IDbSource>()))
                    .Returns(new List<string> { "Dev2TestingDB" });
            }
            else if (isLongRunning)
            {
                var viewModel = ScenarioContext.Current.Get<ManageOdbcSourceViewModel>("viewModel");
                mockUpdateManager.Setup(manager => manager.TestDbConnection(It.IsAny<IDbSource>()));
                viewModel.AsyncWorker = new AsyncWorker();
            }
            else
            {
                mockUpdateManager.Setup(manager => manager.TestDbConnection(It.IsAny<IDbSource>()))
                    .Throws(new WarewolfTestException(loginFailedForUserTest, new Exception(loginFailedForUserTest)));
            }
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.PerformTestConnection();
            Thread.Sleep(1000);
        }

        [When(@"I Cancel the Test")]
        public void WhenICancelTheTest()
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.CancelTest();
        }

        [Then(@"the validation message as ""(.*)""")]
        public void ThenTheValidationMessageAs(string message)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            var viewModel = ScenarioContext.Current.Get<ManageOdbcSourceViewModel>("viewModel");
            var errorMessageFromControl = manageDatabaseSourceControl.GetErrorMessage();
            var errorMessageOnViewModel = viewModel.TestMessage;
            var isErrorMessageOnControl = errorMessageFromControl.Equals(message, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(isErrorMessageOnControl);
            var isErrorMessage = errorMessageOnViewModel.Equals(message, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(isErrorMessage);
        }

        [Then(@"the save dialog is opened")]
        public void ThenTheSaveDialogIsOpened()
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Verify();
        }

        [AfterScenario("ODBCSource")]
        public void Cleanup()
        {
            DisposeResources();
        }
        
        private static void DisposeResources()
        {
            var mockUpdateManager = ScenarioContext.Current.Get<Mock<IManageDatabaseSourceModel>>("updateManager");
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            var mockEventAggregator = new Mock<IEventAggregator>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var viewModel = new ManageOdbcSourceViewModel(mockUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker());
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            var manageDatabaseSourceViewModel = manageDatabaseSourceControl.DataContext as ManageOdbcSourceViewModel;
            if (manageDatabaseSourceViewModel != null)
            {
                Utils.ResetViewModel<ManageOdbcSourceViewModel, IDbSource>(viewModel, manageDatabaseSourceViewModel);
                manageDatabaseSourceViewModel.DatabaseName = null;
            }
        }

        [AfterFeature("DbSource")]
        public static void FeaureCleanup()
        {
            DisposeResources();
        }

        [Then(@"database dropdown is ""(.*)""")]
        public void ThenDatabaseDropdownIs(string p0)
        {
            var expectedVisibility = String.Equals(p0, "Collapsed", StringComparison.InvariantCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            var databaseDropDownVisibility = manageDatabaseSourceControl.GetDatabaseDropDownVisibility();
            Assert.AreEqual(expectedVisibility, databaseDropDownVisibility);
        }
    }
}
