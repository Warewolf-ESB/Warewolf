using System;
using System.Collections.Generic;
using System.Linq;
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
using Warewolf.AcceptanceTesting.Core;
using Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter;
using Warewolf.Studio.ServerProxyLayer;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;

namespace Warewolf.AcceptanceTesting.DatabaseSource
{
    [Binding]
    public class NewPostgresDatabaseSourceSteps
    {

        [BeforeFeature("DbSource")]
        public static void SetupForSystem()
        {
            Utils.SetupResourceDictionary();
            var manageDatabaseSourceControl = new ManageDatabaseSourceControl();
            var mockStudioUpdateManager = new Mock<IManageDatabaseSourceModel>();
            mockStudioUpdateManager.Setup(model => model.GetComputerNames()).Returns(new List<string> { "Test", "admin", "postgres" });
            mockStudioUpdateManager.Setup(model => model.ServerName).Returns("localhost");
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var manageDatabaseSourceViewModel = new ManageDatabaseSourceViewModel(mockStudioUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker());
            manageDatabaseSourceControl.DataContext = manageDatabaseSourceViewModel;
            Utils.ShowTheViewForTesting(manageDatabaseSourceControl);
            FeatureContext.Current.Add(Utils.ViewNameKey, manageDatabaseSourceControl);
            FeatureContext.Current.Add(Utils.ViewModelNameKey, manageDatabaseSourceViewModel);
            FeatureContext.Current.Add("updateManager", mockStudioUpdateManager);
            FeatureContext.Current.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
        }

        [BeforeScenario("DbSource")]
        public void SetupForDatabaseSource()
        {
            ScenarioContext.Current.Add(Utils.ViewNameKey, FeatureContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey));
            ScenarioContext.Current.Add("updateManager", FeatureContext.Current.Get<Mock<IManageDatabaseSourceModel>>("updateManager"));
            ScenarioContext.Current.Add("requestServiceNameViewModel", FeatureContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
            ScenarioContext.Current.Add(Utils.ViewModelNameKey, FeatureContext.Current.Get<ManageDatabaseSourceViewModel>(Utils.ViewModelNameKey));
        }

        [Given(@"I open New database Source")]
        public void GivenIOpenNewDatabaseSource()
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            Assert.IsNotNull(manageDatabaseSourceControl);
            Assert.IsNotNull(manageDatabaseSourceControl.DataContext);
        }
        
        [Given(@"I Type Server as ""(.*)""")]
        public void GivenITypeServerAs(string serverName)
        {
            if (serverName == "Incorrect")
            {

            }
            else
            {
                var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
                manageDatabaseSourceControl.EnterServerName(serverName);
                var viewModel = ScenarioContext.Current.Get<ManageDatabaseSourceViewModel>("viewModel");
                Assert.AreEqual(serverName, viewModel.ServerName.Name);
            }
        }
        
        [Given(@"I Select Authentication type as ""(.*)""")]
        public void GivenISelectAuthenticationTypeAs(string authenticationTypeString)
        {
            var authenticationType = String.Equals(authenticationTypeString, "User",
                StringComparison.InvariantCultureIgnoreCase)
                ? AuthenticationType.Windows
                : AuthenticationType.User;

            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SetAuthenticationType(authenticationType);
        }
        
        [Given(@"I Open ""(.*)""")]
        public void GivenIOpen(string name)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            var upd = FeatureContext.Current.Get<Mock<IManageDatabaseSourceModel>>("updateManager").Object;
            var dbsrc = new DbSourceDefinition
            {
                Name = name,
                Id = Guid.NewGuid(),
                ServerName = "localhost",
                AuthenticationType = AuthenticationType.User
            };
            FeatureContext.Current["dbsrc"] = dbsrc;
            var mockEventAggregator = new Mock<IEventAggregator>();
            var viewModel = new ManageDatabaseSourceViewModel(upd, mockEventAggregator.Object, dbsrc, new SynchronousAsyncWorker());
            var manageDatabaseSourceViewModel = manageDatabaseSourceControl.DataContext as ManageDatabaseSourceViewModel;
            if (manageDatabaseSourceViewModel != null)
            {
                Utils.ResetViewModel<ManageDatabaseSourceViewModel, IDbSource>(viewModel, manageDatabaseSourceViewModel);
            }
        }
        
        [Given(@"server as ""(.*)""")]
        public void GivenServerAs(string server)
        {
            var db = FeatureContext.Current.Get<IDbSource>("dbsrc");
            db.ServerName = server;
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SelectServer("server");
        }
        
        [Given(@"Username field Is ""(.*)""")]
        public void GivenUsernameFieldIs(string user)
        {
            var db = FeatureContext.Current.Get<IDbSource>("dbsrc");
            db.UserName = user;
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.EnterUserName(user);
        }
        
        [Given(@"Password field Is ""(.*)""")]
        public void GivenPasswordFieldIs(string pwd)
        {
            var db = FeatureContext.Current.Get<IDbSource>("dbsrc");
            db.Password = pwd;
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.EnterPassword(pwd);
        }
        
        [Given(@"""(.*)"" connection is ""(.*)""")]
        public void GivenConnectionIs(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Given(@"Database ""(.*)"" Is selected")]
        public void GivenDatabaseIsSelected(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Given(@"""(.*)"" save is ""(.*)""")]
        public void GivenSaveIs(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I Type Server as ""(.*)""")]
        public void WhenITypeServerAs(string p0)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SelectServer(p0);
        }
        
        [When(@"I Click test connnection ""(.*)""")]
        public void WhenIClickTestConnnection(string p0)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.Test();
        }
        
        [When(@"I save the source As ""(.*)""")]
        public void WhenISaveTheSourceAs(string name)
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Returns(MessageBoxResult.OK).Verifiable();
            mockRequestServiceNameViewModel.Setup(a => a.ResourceName).Returns(new ResourceName("", name));
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.PerformSave();
        }
        
        [When(@"I Type Username as ""(.*)""")]
        public void WhenITypeUsernameAs(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I Type Password as ""(.*)""")]
        public void WhenITypePasswordAs(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"Test connecton is ""(.*)""")]
        public void WhenTestConnectonIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I Edit server as ""(.*)""")]
        public void WhenIEditServerAs(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I cancel the Test")]
        public void WhenICancelTheTest()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"""(.*)"" tab is Opened")]
        public void ThenTabIsOpened(string headerText)
        {
            var viewModel = ScenarioContext.Current.Get<IDockAware>("viewModel");
            Assert.AreEqual(headerText, viewModel.Header);
        }
        
        [Then(@"Title is ""(.*)""")]
        public void ThenTitleIs(string p0)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            Assert.AreEqual(manageDatabaseSourceControl.GetHeader(), p0);
        }
        
        [Then(@"the Intellisense contains these options")]
        public void ThenTheIntellisenseContainsTheseOptions(Table table)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);

            var rows = table.Rows[0].Values;
            foreach (var server in rows)
            {
                manageDatabaseSourceControl.VerifyServerExistsintComboBox(server);
            }
        }
        
        [Then(@"Type options contains")]
        public void ThenTypeOptionsContains(Table table)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);

            var rows = table.Rows[0].Values;

            Assert.IsTrue(manageDatabaseSourceControl.GetServerOptions().All(a => rows.Contains(a)));
        }
        
        [Then(@"I Type Select The Server as ""(.*)""")]
        public void ThenITypeSelectTheServerAs(string p0)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SelectServer(p0);
        }
        
        [Then(@"Type options has ""(.*)"" as the default")]
        public void ThenTypeOptionsHasAsTheDefault(string defaultDbType)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);

            Assert.IsTrue(manageDatabaseSourceControl.GetSelectedDbOption() == defaultDbType);
        }
       
        
        [Then(@"I Select Authentication type as ""(.*)""")]
        public void ThenISelectAuthenticationTypeAs(string authenticationTypeString)
        {
            var authenticationType = String.Equals(authenticationTypeString, "User",
                StringComparison.InvariantCultureIgnoreCase)
                ? AuthenticationType.Windows
                : AuthenticationType.User;

            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SetAuthenticationType(authenticationType);
        }


        [Then(@"""(.*)"" Con Is ""(.*)""")]
        public void ThenConIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey));
        }
        
        [Then(@"""(.*)"" Cancel Is ""(.*)""")]
        public void ThenCancelIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey));
        }
        
        [Then(@"""(.*)"" Disabled Is ""(.*)""")]
        public void ThenDisabledIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey));
        }
        
        [Then(@"Test Connecton Is ""(.*)""")]
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
                var viewModel = ScenarioContext.Current.Get<ManageDatabaseSourceViewModel>("viewModel");
                mockUpdateManager.Setup(manager => manager.TestDbConnection(It.IsAny<IDbSource>()));
                viewModel.AsyncWorker = new AsyncWorker();
            }
            else
            {
                mockUpdateManager.Setup(manager => manager.TestDbConnection(It.IsAny<IDbSource>()))
                    .Throws(new WarewolfTestException("Server not found", null));

            }
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.PerformTestConnection();
            Thread.Sleep(1000);
        }
        
        [Then(@"I select ""(.*)"" As Database")]
        public void ThenISelectAsDatabase(string databaseName)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SelectDatabase(databaseName);
            var viewModel = (ManageDatabaseSourceViewModel)manageDatabaseSourceControl.DataContext;
            Assert.AreEqual(databaseName, viewModel.DatabaseName);
        }
        
        [Then(@"""(.*)"" Save Is Enabled ""(.*)""")]
        public void ThenSaveIsEnabled(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey));
        }
        
        [Then(@"The save dialog is opened")]
        public void ThenTheSaveDialogIsOpened()
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Verify();
        }
        
        [Then(@"""(.*)"" Save Tab is opened")]
        public void ThenSaveTabIsOpened(string headerText)
        {
            var viewModel = ScenarioContext.Current.Get<IDockAware>("viewModel");
            Assert.AreEqual(headerText, viewModel.Header);
        }
        
        [Then(@"""(.*)"" Is the tab Header")]
        public void ThenIsTheTabHeader(string p0)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            Assert.AreEqual(manageDatabaseSourceControl.GetHeader(), p0);
        }
        
        [Then(@"username field is ""(.*)""")]
        public void ThenUsernameFieldIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        [Given(@"Authentication type is selected as ""(.*)""")]
        public void GivenAuthenticationTypeIsSelectedAs(string authenticationTypeString)
        {
            var authenticationType = String.Equals(authenticationTypeString, "Windows",
                 StringComparison.InvariantCultureIgnoreCase)
                 ? AuthenticationType.Windows
                 : AuthenticationType.User;

            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SetAuthenticationType(authenticationType);
        }


        [Then(@"password field is ""(.*)""")]
        public void ThenPasswordFieldIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"database dropdown is ""(.*)""")]
        public void ThenDatabaseDropdownIs(string visibility)
        {
            var expectedVisibility = String.Equals(visibility, "Collapsed", StringComparison.InvariantCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;

            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            var databaseDropDownVisibility = manageDatabaseSourceControl.GetDatabaseDropDownVisibility();
            Assert.AreEqual(expectedVisibility, databaseDropDownVisibility);
        }
        
        [Then(@"""(.*)"" Connection is ""(.*)""")]
        public void ThenConnectionIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey));
        }
        
        [Then(@"""(.*)"" Save is ""(.*)""")]
        public void ThenSaveIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey));
        }

        [Then(@"Authentication type is selected as ""(.*)""")]
        public void ThenAuthenticationTypeIsSelectedAs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Username Is ""(.*)""")]
        public void ThenUsernameIs(string visibility)
        {
            var expectedVisibility = String.Equals(visibility, "Collapsed", StringComparison.InvariantCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;

            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            var databaseDropDownVisibility = manageDatabaseSourceControl.GetUsernameVisibility();
            Assert.AreEqual(expectedVisibility, databaseDropDownVisibility);
        }
        
        [Then(@"Password  Is ""(.*)""")]
        public void ThenPasswordIs(string visibility)
        {
            var expectedVisibility = String.Equals(visibility, "Collapsed", StringComparison.InvariantCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;

            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            var databaseDropDownVisibility = manageDatabaseSourceControl.GetPasswordVisibility();
            Assert.AreEqual(expectedVisibility, databaseDropDownVisibility);
        }
        
        [Then(@"the Validation message as ""(.*)""")]
        public void ThenTheValidationMessageAs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

    }
}
