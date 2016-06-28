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

namespace Warewolf.AcceptanceTesting.DatabaseSource.PostgresSqlSource
{
    [Binding]
    public class NewPostgresDatabaseSourceSteps
    {
        [BeforeFeature("PostgreSource")]
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

        [BeforeScenario("PostgreSource")]
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

        [When(@"I Type Server as ""(.*)""")]
        [Then(@"I Type Server as ""(.*)""")]
        [Given(@"I Type Server as ""(.*)""")]
        public void WhenITypeServerAs(string p0)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SelectServer(p0);
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

        [Then(@"Type options has ""(.*)"" as the default")]
        public void ThenTypeOptionsHasAsTheDefault(string defaultDbType)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            Assert.IsTrue(manageDatabaseSourceControl.GetSelectedDbOption() == defaultDbType);
        }

        [Then(@"I select type ""(.*)""")]
        public void ThenISelectType(string type)
        {
            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            manageDatabaseSourceControl.SelectType(type);
        }

        [Then(@"Authentication type ""(.*)"" is ""(.*)""")]
        public void ThenAuthenticationTypeIs(string authenticationTypeString, string enabledString)
        {
            var expectedState = !String.Equals(enabledString, "Disabled", StringComparison.InvariantCultureIgnoreCase);

            var authenticationType = String.Equals(authenticationTypeString, "User",
                StringComparison.InvariantCultureIgnoreCase)
                ? AuthenticationType.Windows
                : AuthenticationType.User;

            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            var databaseAuthenticationEnabledState = manageDatabaseSourceControl.GetAuthenticationEnabledState(authenticationType);
            Assert.AreEqual(expectedState, databaseAuthenticationEnabledState);
        }

        [Then(@"database dropdown is ""(.*)""")]
        public void ThenDatabaseDropdownIs(string visibility)
        {
            var expectedVisibility = String.Equals(visibility, "Collapsed", StringComparison.InvariantCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;

            var manageDatabaseSourceControl = ScenarioContext.Current.Get<ManageDatabaseSourceControl>(Utils.ViewNameKey);
            var databaseDropDownVisibility = manageDatabaseSourceControl.GetDatabaseDropDownVisibility();
            Assert.AreEqual(expectedVisibility, databaseDropDownVisibility);
        }
    }
}
