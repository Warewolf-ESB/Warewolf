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
using Warewolf.UIBindingTests.Core;
using Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter;
using Warewolf.Studio.ServerProxyLayer;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;

namespace Warewolf.UIBindingTests.DatabaseSource
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
            mockStudioUpdateManager.Setup(model => model.GetComputerNames()).Returns(new List<string> { "TEST", "RSAKLFSVRGENDEV", "RSAKLFSVRSBSPDC", "RSAKLFSVRTFSBLD", "RSAKLFSVRWRWBLD" });
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
           Assert.AreEqual( manageDatabaseSourceControl.GetHeader(),p0);
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
                ServerName = "RSAKLFSVRGENDEV",
                AuthenticationType = AuthenticationType.Windows
            };
            FeatureContext.Current["dbsrc"] = dbsrc;
            var mockEventAggregator = new Mock<IEventAggregator>();
            var viewModel = new ManageDatabaseSourceViewModel(upd, mockEventAggregator.Object, dbsrc, new SynchronousAsyncWorker());
            var manageDatabaseSourceViewModel = manageDatabaseSourceControl.DataContext as ManageDatabaseSourceViewModel;
            if(manageDatabaseSourceViewModel != null)
            {
                Utils.ResetViewModel<ManageDatabaseSourceViewModel, IDbSource>(viewModel, manageDatabaseSourceViewModel);
            }
        }

        [Given(@"type option has ""(.*)"" selected")]
        public void GivenTypeOptionHasSelected(string dbType)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            var manageDatabaseSourceViewModel = manageDatabaseSourceControl.DataContext as ManageDatabaseSourceViewModel;
            if (manageDatabaseSourceViewModel != null)
            {
                manageDatabaseSourceViewModel.ServerType =
                    manageDatabaseSourceViewModel.Types.FirstOrDefault(value => value.Name == dbType);
            }
            manageDatabaseSourceControl.SelectType(dbType);
        }

        [Given(@"Server as ""(.*)""")]
        public void GivenServerAs(string server)
        {
            var db = FeatureContext.Current.Get<IDbSource>("dbsrc");
            db.ServerName = server;
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SelectServer("server");
        }

        [When(@"I Edit Server as ""(.*)""")]
        public void WhenIEditServerAs(string server)
        {
            var db = FeatureContext.Current.Get<IDbSource>("dbsrc");
            db.ServerName = server;
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SelectServer("server");
        }

        [Given(@"Authentication Type is selected as ""(.*)""")]
        public void GivenAuthenticationTypeIsSelectedAs(string authstr)
        {
            var db = FeatureContext.Current.Get<IDbSource>("dbsrc");
            var authp = Enum.Parse(typeof(AuthenticationType),authstr);
            db.AuthenticationType = (AuthenticationType)authp;

            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SetAuthenticationType((AuthenticationType)authp);
            // ReSharper disable PossibleNullReferenceException
            (manageDatabaseSourceControl.DataContext as ManageDatabaseSourceViewModel).AuthenticationType = (AuthenticationType)authp;
            // ReSharper restore PossibleNullReferenceException
        }

        [Then(@"Authentication Type is selected as ""(.*)""")]
        public void ThenAuthenticationTypeIsSelectedAs(string authstr)
        {
            var db = FeatureContext.Current.Get<IDbSource>("dbsrc");
            var authp = Enum.Parse(typeof(AuthenticationType), authstr);
            db.AuthenticationType = (AuthenticationType)authp;

            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);

            var manageDatabaseSourceViewModel = manageDatabaseSourceControl.DataContext as ManageDatabaseSourceViewModel;
            if(manageDatabaseSourceViewModel != null)
            {
                Assert.AreEqual( manageDatabaseSourceViewModel.AuthenticationType ,(AuthenticationType)authp);
            }
        }

        [Given(@"Username field is ""(.*)""")]
        public void GivenUsernameFieldIs(string user)
        {
            var db = FeatureContext.Current.Get<IDbSource>("dbsrc");
            db.UserName = user;
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.EnterUserName(user);
        }

        [Given(@"Password field is ""(.*)""")]
        public void GivenPasswordFieldIs(string pwd)
        {
            var db = FeatureContext.Current.Get<IDbSource>("dbsrc");
            db.Password = pwd;
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.EnterPassword(pwd);
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

        [When(@"I type Server as ""(.*)""")]
        public void WhenITypeServerAs(string p0)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SelectServer(p0);
        }

        [Then(@"I type Select The Server as ""(.*)""")]
        public void ThenITypeSelectTheServerAs(string p0)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SelectServer(p0);
        }

        [Then(@"the intellisense contains these options")]
        public void ThenTheIntellisenseContainsTheseOptions(Table table)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);

            var rows = table.Rows[0].Values;
            foreach(var server in rows)
            {
                manageDatabaseSourceControl.VerifyServerExistsintComboBox(server);   
            }
        }

        [Then(@"type options contains")]
        public void ThenTypeOptionsContains(Table table)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);

            var rows = table.Rows[0].Values;

            var serverOptions = manageDatabaseSourceControl.GetServerOptions();
            Assert.IsTrue( serverOptions.All(a=>rows.Contains(a)));
        }

        [Then(@"type options has ""(.*)"" as the default")]
        public void ThenTypeOptionsHasAsTheDefault(string defaultDbType)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);

            Assert.IsTrue(manageDatabaseSourceControl.GetSelectedDbOption() == defaultDbType);
        }        

        [Given(@"I type Server as ""(.*)""")]
        public void GivenITypeServerAs(string serverName)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SelectServer(serverName);
            //manageDatabaseSourceControl.EnterServerName(serverName);
            var viewModel = ScenarioContext.Current.Get<ManageDatabaseSourceViewModel>("viewModel");
            Assert.AreEqual(serverName, viewModel.ServerName.Name);
        }

        [Given(@"Database dropdown is ""(.*)""")]
        [When(@"Database dropdown is ""(.*)""")]
        [Then(@"Database dropdown is ""(.*)""")]
        public void GivenDropdownIs(string visibility)
        {
            var expectedVisibility = String.Equals(visibility, "Collapsed", StringComparison.InvariantCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;

            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            var databaseDropDownVisibility = manageDatabaseSourceControl.GetDatabaseDropDownVisibility();
            Assert.AreEqual(expectedVisibility,databaseDropDownVisibility);
        }

        [Given(@"""(.*)"" is ""(.*)""")]
        [When(@"""(.*)"" is ""(.*)""")]
        [Then(@"""(.*)"" is ""(.*)""")]
        public void GivenIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey));
        }

        [Given(@"I Select Authentication Type as ""(.*)""")]
        [When(@"I Select Authentication Type as ""(.*)""")]
        [Then(@"I Select Authentication Type as ""(.*)""")]
        public void GivenISelectAuthenticationTypeAs(string authenticationTypeString)
        {
            var authenticationType = String.Equals(authenticationTypeString, "Windows",
                StringComparison.InvariantCultureIgnoreCase)
                ? AuthenticationType.Windows
                : AuthenticationType.User;

            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SetAuthenticationType(authenticationType);
        }

        [Given(@"I select ""(.*)"" as Database")]
        [When(@"I select ""(.*)"" as Database")]
        [Then(@"I select ""(.*)"" as Database")]
        public void WhenISelectAsDatabase(string databaseName)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SelectDatabase(databaseName);
            var viewModel = (ManageDatabaseSourceViewModel) manageDatabaseSourceControl.DataContext;
            Assert.AreEqual(databaseName,viewModel.DatabaseName);
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

        [Then(@"Username field is ""(.*)""")]
        public void ThenUsernameFieldIs(string visibility)
        {
            var expectedVisibility = String.Equals(visibility, "Collapsed", StringComparison.InvariantCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;

            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            var databaseDropDownVisibility = manageDatabaseSourceControl.GetUsernameVisibility();
            Assert.AreEqual(expectedVisibility, databaseDropDownVisibility);
        }

        [Then(@"Username is ""(.*)""")]
        public void ThenUsernameIs(string userName)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);

            Assert.AreEqual(userName, manageDatabaseSourceControl.GetUsername());
        }

        [Then(@"Password  is ""(.*)""")]
        public void ThenPasswordIs(string password)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);

            Assert.AreEqual(password, manageDatabaseSourceControl.GetPassword());
        }

        [Then(@"Password field is ""(.*)""")]
        public void ThenPasswordFieldIs(string visibility)
        {
            var expectedVisibility = String.Equals(visibility, "Collapsed", StringComparison.InvariantCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;

            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            var databaseDropDownVisibility = manageDatabaseSourceControl.GetPasswordVisibility();
            Assert.AreEqual(expectedVisibility, databaseDropDownVisibility);
        }

        [Given(@"I type Username as ""(.*)""")]
        [When(@"I type Username as ""(.*)""")]
        [Then(@"I type Username as ""(.*)""")]
        public void WhenITypeUsernameAs(string userName)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.EnterUserName(userName);
            var viewModel = ScenarioContext.Current.Get<ManageDatabaseSourceViewModel>("viewModel");
            Assert.AreEqual(userName,viewModel.UserName);
        }

        [Given(@"I type Password as ""(.*)""")]
        [When(@"I type Password as ""(.*)""")]
        [Then(@"I type Password as ""(.*)""")]
        public void WhenITypePasswordAs(string password)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.EnterPassword(password);
            var viewModel = ScenarioContext.Current.Get<ManageDatabaseSourceViewModel>("viewModel");
            Assert.AreEqual(password,viewModel.Password);
        }

        [Then(@"the error message is ""(.*)""")]
        public void ThenTheErrorMessageIs(string errorMessage)
        {
            errorMessage = "Exception: " + loginFailedForUserTest + Environment.NewLine + Environment.NewLine +
                           "Inner Exception: " + loginFailedForUserTest;
            var viewModel = ScenarioContext.Current.Get<ManageDatabaseSourceViewModel>("viewModel");
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
                    .Returns(new List<string> {"Dev2TestingDB"});
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
            var viewModel = ScenarioContext.Current.Get<ManageDatabaseSourceViewModel>("viewModel");
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

        [AfterScenario("DbSource")]
        public void Cleanup()
        {
            var mockUpdateManager = ScenarioContext.Current.Get<Mock<IManageDatabaseSourceModel>>("updateManager");
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            var mockEventAggregator = new Mock<IEventAggregator>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var viewModel = new ManageDatabaseSourceViewModel(mockUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker());
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            var manageDatabaseSourceViewModel = manageDatabaseSourceControl.DataContext as ManageDatabaseSourceViewModel;
            if(manageDatabaseSourceViewModel != null)
            {
                Utils.ResetViewModel<ManageDatabaseSourceViewModel, IDbSource>(viewModel, manageDatabaseSourceViewModel);
                manageDatabaseSourceViewModel.DBSource = null;
            }
        }

        [When(@"I click ""(.*)""")]
        public void WhenIClick(string ConectTo)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.Test();
        }

        [When(@"I change type option from ""(.*)"" to ""(.*)""")]
        public void WhenIChangeTypeOptionFromTo(string p0, string p1)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            Assert.AreEqual(p0, manageDatabaseSourceControl.GetSelectedDbOption());
            manageDatabaseSourceControl.SelectType(p1);
            manageDatabaseSourceControl.SelectDatabase(string.Empty);
            var viewModel = (ManageDatabaseSourceViewModel)manageDatabaseSourceControl.DataContext;
            Assert.AreEqual(p1, manageDatabaseSourceControl.GetSelectedDbOption());
            Assert.AreEqual(string.Empty, viewModel.DatabaseName);
        }

        [Then(@"I select type ""(.*)""")]
        public void ThenISelectType(string p0)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SelectType(p0);
            var viewModel = (ManageDatabaseSourceViewModel)manageDatabaseSourceControl.DataContext;
            Assert.AreEqual(p0, manageDatabaseSourceControl.GetSelectedDbOption());
        }

        [Then(@"Authentication type ""(.*)"" is ""(.*)""")]
        public void ThenAuthenticationTypeIs(string p0, string p1)
        {
            Utils.CheckControlEnabled(p0, p1, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey));
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
