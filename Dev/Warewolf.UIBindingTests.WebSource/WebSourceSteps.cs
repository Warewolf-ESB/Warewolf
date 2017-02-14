using System;
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
// ReSharper disable RedundantAssignment

namespace Warewolf.UIBindingTests.WebSource
{
    [Binding]
    public class WebSourceSteps
    {
        private readonly ScenarioContext scenarioContext;
        string illegalCharactersInPath = "Illegal characters in path.";

        public WebSourceSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        [BeforeFeature("WebSource")]
        public static void SetupForSystem()
        {
            Utils.SetupResourceDictionary();
            var manageWebserviceSourceControl = new ManageWebserviceSourceControl();
            var mockStudioUpdateManager = new Mock<IManageWebServiceSourceModel>();
            mockStudioUpdateManager.Setup(model => model.ServerName).Returns("localhost");
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var manageWebserviceSourceViewModel = new ManageWebserviceSourceViewModel(mockStudioUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker(), mockExecutor.Object);
            manageWebserviceSourceControl.DataContext = manageWebserviceSourceViewModel;
            Utils.ShowTheViewForTesting(manageWebserviceSourceControl);
            FeatureContext.Current.Add(Utils.ViewNameKey, manageWebserviceSourceControl);
            FeatureContext.Current.Add(Utils.ViewModelNameKey, manageWebserviceSourceViewModel);
            FeatureContext.Current.Add("updateManager", mockStudioUpdateManager);
            FeatureContext.Current.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
        }

        [BeforeScenario("WebSource")]
        public void SetupForWebSource()
        {
            scenarioContext.Add(Utils.ViewNameKey, FeatureContext.Current.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey));
            scenarioContext.Add("updateManager", FeatureContext.Current.Get<Mock<IManageWebServiceSourceModel>>("updateManager"));
            scenarioContext.Add("requestServiceNameViewModel", FeatureContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
            scenarioContext.Add("externalProcessExecutor", FeatureContext.Current.Get<Mock<IExternalProcessExecutor>>("externalProcessExecutor"));
            scenarioContext.Add(Utils.ViewModelNameKey, FeatureContext.Current.Get<ManageWebserviceSourceViewModel>(Utils.ViewModelNameKey));
        }

        [Then(@"""(.*)"" tab is opened")]
        public void ThenTabIsOpened(string headerText)
        {
            var viewModel = scenarioContext.Get<IDockAware>("viewModel");
            Assert.AreEqual(headerText, viewModel.Header);
        }

        [Then(@"title is ""(.*)""")]
        public void ThenTitleIs(string title)
        {
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            var viewModel = scenarioContext.Get<ManageWebserviceSourceViewModel>("viewModel");
            Assert.AreEqual(title, viewModel.HeaderText);
            Assert.AreEqual(title, manageWebserviceSourceControl.GetHeaderText());
        }

        [Given(@"I open New Web Source")]
        public void GivenIOpenNewWebSource()
        {
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            Assert.IsNotNull(manageWebserviceSourceControl);
            Assert.IsNotNull(manageWebserviceSourceControl.DataContext); 
        }

       
        [Given(@"I type Address as ""(.*)""")]
        [When(@"I change Address to ""(.*)""")]
        [Then(@"I type Address as ""(.*)""")]
        public void GivenITypeAddressAs(string address)
        {
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            manageWebserviceSourceControl.EnterServerName(address);
            var viewModel = scenarioContext.Get<ManageWebserviceSourceViewModel>("viewModel");
            Assert.AreEqual(address, viewModel.HostName);
        }

        [Given(@"I type Default Query as ""(.*)""")]
        [When(@"I type Default Query as ""(.*)""")]
        [Then(@"I type Default Query as ""(.*)""")]
        public void GivenITypeDefaultQueryAs(string defaultQuery)
        {
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            manageWebserviceSourceControl.EnterDefaultQuery(defaultQuery);
            var viewModel = scenarioContext.Get<ManageWebserviceSourceViewModel>("viewModel");
            Assert.AreEqual(defaultQuery, viewModel.DefaultQuery);
        }

        [Given(@"I open ""(.*)"" web source")]
        public void GivenIOpenWebSource(string resourceName)
        {
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            var mockStudioUpdateManager = new Mock<IManageWebServiceSourceModel>();
            mockStudioUpdateManager.Setup(model => model.ServerName).Returns("localhost");
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();

            var webServiceSourceDefinition = new WebServiceSourceDefinition
            {
                Name = "Test",
                HostName = "http://RSAKLFSVRTFSBLD/IntegrationTestSite",
                DefaultQuery = "/GetCountries.ashx?extension=json&prefix=a",
                UserName = "IntegrationTester",
                Password = "I73573r0"
            };
            mockStudioUpdateManager.Setup(model => model.FetchSource(It.IsAny<Guid>()))
                .Returns(webServiceSourceDefinition);
            var manageWebserviceSourceViewModel = new ManageWebserviceSourceViewModel(mockStudioUpdateManager.Object, mockEventAggregator.Object,webServiceSourceDefinition, new SynchronousAsyncWorker(), mockExecutor.Object);
            manageWebserviceSourceControl.DataContext = manageWebserviceSourceViewModel;
            scenarioContext.Remove("viewModel");
            scenarioContext.Add("viewModel",manageWebserviceSourceViewModel);
        }

        [Given(@"Address is ""(.*)""")]
        [When(@"Address is ""(.*)""")]
        [Then(@"Address is ""(.*)""")]
        public void GivenAddressIs(string address)
        {
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            var viewModel = scenarioContext.Get<ManageWebserviceSourceViewModel>("viewModel");
            Assert.AreEqual(address, viewModel.HostName);
            Assert.AreEqual(address, manageWebserviceSourceControl.GetAddress());
        }

        [Given(@"Default Query is ""(.*)""")]
        [When(@"Default Query is ""(.*)""")]
        [Then(@"Default Query is ""(.*)""")]
        public void GivenDefaultQueryIs(string defaultQuery)
        {
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            var viewModel = scenarioContext.Get<ManageWebserviceSourceViewModel>("viewModel");
            Assert.AreEqual(defaultQuery, viewModel.DefaultQuery);
            Assert.AreEqual(defaultQuery, manageWebserviceSourceControl.GetDefaultQuery());
        }

        [Given(@"Username is ""(.*)""")]
        public void GivenUsernameIs(string userName)
        {
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            var viewModel = scenarioContext.Get<ManageWebserviceSourceViewModel>("viewModel");
            Assert.AreEqual(userName, viewModel.UserName);
            Assert.AreEqual(userName, manageWebserviceSourceControl.GetUsername());
        }

        [Then(@"TestQuery is ""(.*)""")]
        public void ThenTestQueryIs(string testQuery)
        {
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            var viewModel = scenarioContext.Get<ManageWebserviceSourceViewModel>("viewModel");
            Assert.AreEqual(testQuery, viewModel.TestDefault);
            var queryEqual = testQuery.Equals(manageWebserviceSourceControl.GetTestDefault(), StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(queryEqual);
        }

        [Given(@"Password is ""(.*)""")]
        public void GivenPasswordIs(string password)
        {
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            var viewModel = scenarioContext.Get<ManageWebserviceSourceViewModel>("viewModel");
            Assert.AreEqual(password, viewModel.UserName);
            Assert.AreEqual(password, manageWebserviceSourceControl.GetPassword());
        }

        [When(@"I click TestQuery")]
        public void WhenIClickTestQuery()
        {
            var mockExecutor = scenarioContext.Get<Mock<IExternalProcessExecutor>>("externalProcessExecutor");
            mockExecutor.Setup(executor => executor.OpenInBrowser(It.IsAny<Uri>())).Callback<Uri>(uri => scenarioContext.Add("uriCalled", uri)).Verifiable();
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            manageWebserviceSourceControl.ClickHyperLink();
        }

        [When(@"I Cancel the Test")]
        public void WhenICancelTheTest()
        {
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            manageWebserviceSourceControl.CancelTest();
        }


        [Then(@"the browser window opens with ""(.*)""")]
        public void ThenTheBrowserWindowOpensWith(string expectedUriCalled)
        {
            var mockExecutor = scenarioContext.Get<Mock<IExternalProcessExecutor>>("externalProcessExecutor");
            mockExecutor.Verify();
            var uri = scenarioContext.Get<Uri>("uriCalled");
            var correctUriCalled = String.Equals(expectedUriCalled, uri.ToString(), StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(correctUriCalled);
        }

        [Given(@"Validation message is thrown")]
        [When(@"Validation message is thrown")]
        [Then(@"Validation message is thrown")]
        public void WhenValidationMessageIsThrown()
        {
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            var viewModel = scenarioContext.Get<ManageWebserviceSourceViewModel>("viewModel");
            var errorMessageFromControl = manageWebserviceSourceControl.GetErrorMessage();
            var errorMessageOnViewModel = viewModel.TestMessage;
            Assert.AreNotEqual(string.IsNullOrEmpty(errorMessageFromControl), errorMessageOnViewModel);
            var isErrorMessage = !errorMessageOnViewModel.Contains("Passed");
            Assert.IsTrue(isErrorMessage);
        }

        [Then(@"Validation message is ""(.*)""")]
        public void ThenValidationMessageIs(string msg)
        {
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            var viewModel = scenarioContext.Get<ManageWebserviceSourceViewModel>("viewModel");
            var errorMessageFromControl = manageWebserviceSourceControl.GetErrorMessage();
            var errorMessageOnViewModel = viewModel.TestMessage;
            var isErrorMessageOnControl = errorMessageFromControl.Equals(msg, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(isErrorMessageOnControl);
            var isErrorMessage = errorMessageOnViewModel.Equals(msg,StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(isErrorMessage);
        }

        [When(@"Validation message is Not thrown")]
        public void WhenValidationMessageIsNotThrown()
        {
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            var viewModel = scenarioContext.Get<ManageWebserviceSourceViewModel>("viewModel");
            var errorMessageFromControl = manageWebserviceSourceControl.GetErrorMessage();
            var errorMessageOnViewModel = viewModel.TestMessage;
            var isErrorMessageOnViewModel = !errorMessageOnViewModel.Contains("Passed");
            var isErrorMessageOnControl = !errorMessageFromControl.Contains("Passed");
            Assert.IsFalse(isErrorMessageOnViewModel);
            Assert.IsFalse(isErrorMessageOnControl);
        }

        [Given(@"Username field is ""(.*)""")]
        [When(@"Username field is ""(.*)""")]
        [Then(@"Username field is ""(.*)""")]
        public void WhenUsernameFieldIs(string visibility)
        {
            var expectedVisibility = String.Equals(visibility, "Collapsed", StringComparison.InvariantCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            var databaseDropDownVisibility = manageWebserviceSourceControl.GetUsernameVisibility();
            Assert.AreEqual(expectedVisibility, databaseDropDownVisibility);
        }

        [Given(@"Password field is ""(.*)""")]
        [When(@"Password field is ""(.*)""")]
        [Then(@"Password field is ""(.*)""")]
        public void WhenPasswordFieldIs(string visibility)
        {
            var expectedVisibility = String.Equals(visibility, "Collapsed", StringComparison.InvariantCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            var databaseDropDownVisibility = manageWebserviceSourceControl.GetPasswordVisibility();
            Assert.AreEqual(expectedVisibility, databaseDropDownVisibility);
        }


        [Given(@"I type Username as ""(.*)""")]
        [When(@"I type Username as ""(.*)""")]
        [Then(@"I type Username as ""(.*)""")]
        public void GivenITypeUsernameAs(string userName)
        {
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            manageWebserviceSourceControl.EnterUserName(userName);
            var viewModel = scenarioContext.Get<ManageWebserviceSourceViewModel>("viewModel");
            Assert.AreEqual(userName, viewModel.UserName);
        }

        [When(@"Username field as ""(.*)""")]
        public void WhenUsernameFieldAs(string userName)
        {
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            var viewModel = scenarioContext.Get<ManageWebserviceSourceViewModel>("viewModel");
            Assert.AreEqual(userName, viewModel.UserName);
            Assert.AreEqual(userName, manageWebserviceSourceControl.GetUsername());
        }

        [When(@"Password field as ""(.*)""")]
        public void WhenPasswordFieldAs(string password)
        {
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            var viewModel = scenarioContext.Get<ManageWebserviceSourceViewModel>("viewModel");
            Assert.AreEqual(password, viewModel.Password);
            Assert.AreEqual(password, manageWebserviceSourceControl.GetPassword());
        }


        [Given(@"I type Password as ""(.*)""")]
        [When(@"I type Password as ""(.*)""")]
        [Then(@"I type Password as ""(.*)""")]
        public void WhenITypePasswordAs(string password)
        {
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            manageWebserviceSourceControl.EnterPassword(password);
            var viewModel = scenarioContext.Get<ManageWebserviceSourceViewModel>("viewModel");
            Assert.AreEqual(password, viewModel.Password);
        }

        [When(@"the error message is ""(.*)""")]
        public void WhenTheErrorMessageIs(string errorMessage)
        {
            errorMessage = "Exception: " + illegalCharactersInPath + Environment.NewLine + Environment.NewLine +
                           "Inner Exception: " + illegalCharactersInPath;

            var viewModel = ScenarioContext.Current.Get<ManageWebserviceSourceViewModel>("viewModel");
            Assert.AreEqual(errorMessage, viewModel.TestMessage);
        }

        [Given(@"""(.*)"" is ""(.*)""")]
        [When(@"""(.*)"" is ""(.*)""")]
        [Then(@"""(.*)"" is ""(.*)""")]
        public void GivenIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, scenarioContext.Get<ICheckControlEnabledView>(Utils.ViewNameKey), Utils.ViewNameKey);
        }

        [Then(@"Test Connecton is ""(.*)""")]
        [When(@"Test Connecton is ""(.*)""")]
        public void ThenTestConnectonIs(string successString)
        {
            var mockUpdateManager = scenarioContext.Get<Mock<IManageWebServiceSourceModel>>("updateManager");
            var isSuccess = String.Equals(successString, "Successful", StringComparison.InvariantCultureIgnoreCase);
            var isLongRunning = String.Equals(successString, "Long Running", StringComparison.InvariantCultureIgnoreCase);
            if (isSuccess)
            {
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<IWebServiceSource>()));
            }
            else if(isLongRunning)
            {
                var viewModel = scenarioContext.Get<ManageWebserviceSourceViewModel>("viewModel");
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<IWebServiceSource>()));
                viewModel.AsyncWorker = new AsyncWorker();
            }
            else
            {
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<IWebServiceSource>()))
                    .Throws(new WarewolfTestException(illegalCharactersInPath, new Exception(illegalCharactersInPath)));
            }
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            manageWebserviceSourceControl.PerformTestConnection();
        }

        [When(@"I save the source")]
        public void WhenISaveTheSource()
        {
            var mockRequestServiceNameViewModel = scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Verifiable();
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            manageWebserviceSourceControl.PerformSave();

        }

        [Then(@"the save dialog is opened")]
        public void ThenTheSaveDialogIsOpened()
        {
            var mockRequestServiceNameViewModel = scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Verify();
        }

        [When(@"I save as ""(.*)""")]
        public void WhenISaveAs(string resourceName)
        {
            var mockRequestServiceNameViewModel = scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ResourceName).Returns(new ResourceName("", resourceName));
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            manageWebserviceSourceControl.PerformSave();
        }


        [Given(@"I Select Authentication Type as ""(.*)""")]
        [When(@"I Select Authentication Type as ""(.*)""")]
        [Then(@"I Select Authentication Type as ""(.*)""")]
        [Then(@"Select Authentication Type as ""(.*)""")]
        [When(@"I edit Authentication Type as ""(.*)""")]
        [Given(@"Select Authentication Type as ""(.*)""")]
        public void ThenSelectAuthenticationTypeAs(string authenticationTypeString)
        {
            var authenticationType = String.Equals(authenticationTypeString, "User",
                StringComparison.OrdinalIgnoreCase)
                ? AuthenticationType.User
                : AuthenticationType.Anonymous;

            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            manageWebserviceSourceControl.SetAuthenticationType(authenticationType);
            var viewModel = scenarioContext.Get<ManageWebserviceSourceViewModel>("viewModel");
            Assert.AreEqual(authenticationType, viewModel.AuthenticationType);
        }

        [AfterScenario("WebSource")]
        public void Cleanup()
        {
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var mockUpdateManager = scenarioContext.Get<Mock<IManageWebServiceSourceModel>>("updateManager");
            var mockRequestServiceNameViewModel = scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            var mockEventAggregator = new Mock<IEventAggregator>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var viewModel = new ManageWebserviceSourceViewModel(mockUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker(), mockExecutor.Object);
            var manageWebserviceSourceControl = scenarioContext.Get<ManageWebserviceSourceControl>(Utils.ViewNameKey);
            manageWebserviceSourceControl.DataContext = viewModel;
            FeatureContext.Current.Remove("viewModel");
            FeatureContext.Current.Add("viewModel", viewModel);
            FeatureContext.Current.Remove("externalProcessExecutor");
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
        }
    }
}
