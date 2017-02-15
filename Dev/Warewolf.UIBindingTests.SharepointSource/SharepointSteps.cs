using System;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.SaveDialog;
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

namespace Warewolf.UIBindingTests.SharepointSource
{
    [Binding]
    public class SharepointSteps
    {
        string unableToContactServerTestFailedValueDoesNotFallWithinTheExpectedRange = "Unable to contact Server : Test Failed: Value does not fall within the expected range.";

        [BeforeFeature("SharepointSource")]
        public static void SetupForSystem()
        {
            Utils.SetupResourceDictionary();
            var manageSharepointServerSource = new SharepointServerSource();
            var mockStudioUpdateManager = new Mock<ISharePointSourceModel>();
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockEnvironmentModel = new Mock<Dev2.Studio.Core.Interfaces.IEnvironmentModel>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var manageSharepointServerSourceViewModel = new SharepointServerSourceViewModel(mockStudioUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker(), mockEnvironmentModel.Object);
            manageSharepointServerSource.DataContext = manageSharepointServerSourceViewModel;
            Utils.ShowTheViewForTesting(manageSharepointServerSource);
            FeatureContext.Current.Add(Utils.ViewNameKey, manageSharepointServerSource);
            FeatureContext.Current.Add(Utils.ViewModelNameKey, manageSharepointServerSourceViewModel);
            FeatureContext.Current.Add("updateManager", mockStudioUpdateManager);
            FeatureContext.Current.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
            FeatureContext.Current.Add("mockEnvironmentModel", mockEnvironmentModel);
        }

        [BeforeScenario("SharepointSource")]
        public void SetupForSharepointSource()
        {
            ScenarioContext.Current.Add(Utils.ViewNameKey, FeatureContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey));
            ScenarioContext.Current.Add("updateManager", FeatureContext.Current.Get<Mock<ISharePointSourceModel>>("updateManager"));
            ScenarioContext.Current.Add("requestServiceNameViewModel", FeatureContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
            ScenarioContext.Current.Add("mockEnvironmentModel", FeatureContext.Current.Get<Mock<Dev2.Studio.Core.Interfaces.IEnvironmentModel>>("mockEnvironmentModel"));
            ScenarioContext.Current.Add(Utils.ViewModelNameKey, FeatureContext.Current.Get<SharepointServerSourceViewModel>(Utils.ViewModelNameKey));
        }

        [Given(@"I open New Sharepoint Source")]
        public void GivenIOpenNewSharepointSource()
        {
            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            Assert.IsNotNull(manageSharepointServerSource);
            Assert.IsNotNull(manageSharepointServerSource.DataContext);
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
            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            var viewModel = ScenarioContext.Current.Get<SharepointServerSourceViewModel>("viewModel");
            Assert.AreEqual(title, viewModel.HeaderText);
            Assert.AreEqual(title, manageSharepointServerSource.GetHeaderText());
        }

        [Then(@"I type Address as ""(.*)""")]
        [When(@"I type Address as ""(.*)""")]
        [Given(@"I type Address as ""(.*)""")]
        public void ThenITypeAddressAs(string serverName)
        {
            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            manageSharepointServerSource.EnterServerName(serverName);
            var viewModel = ScenarioContext.Current.Get<SharepointServerSourceViewModel>("viewModel");
            Assert.AreEqual(serverName, viewModel.ServerName);
        }

        [Then(@"""(.*)"" is ""(.*)""")]
        [When(@"""(.*)"" is ""(.*)""")]
        [Given(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey), Utils.ViewNameKey);
        }

        [Then(@"I Select Authentication Type as ""(.*)""")]
        [When(@"I Select Authentication Type as ""(.*)""")]
        [Given(@"I Select Authentication Type as ""(.*)""")]
        public void ThenISelectAuthenticationTypeAs(string authenticationTypeString)
        {
            var authenticationType = String.Equals(authenticationTypeString, "Windows",
                StringComparison.InvariantCultureIgnoreCase)
                ? AuthenticationType.Windows
                : AuthenticationType.User;

            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            manageSharepointServerSource.SetAuthenticationType(authenticationType);
        }

        [Then(@"Username field is ""(.*)""")]
        [When(@"Username field is ""(.*)""")]
        [Given(@"Username field is ""(.*)""")]
        public void ThenUsernameFieldIs(string visibility)
        {
            var expectedVisibility = String.Equals(visibility, "Collapsed", StringComparison.InvariantCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;

            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            var databaseDropDownVisibility = manageSharepointServerSource.GetUsernameVisibility();
            Assert.AreEqual(expectedVisibility, databaseDropDownVisibility);
        }

        [Then(@"Password field is ""(.*)""")]
        [When(@"Password field is ""(.*)""")]
        [Given(@"Password field is ""(.*)""")]
        public void ThenPasswordFieldIs(string visibility)
        {
            var expectedVisibility = String.Equals(visibility, "Collapsed", StringComparison.InvariantCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;

            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            var databaseDropDownVisibility = manageSharepointServerSource.GetPasswordVisibility();
            Assert.AreEqual(expectedVisibility, databaseDropDownVisibility);
        }

        [When(@"the error message is ""(.*)""")]
        public void WhenTheErrorMessageIs(string errorMessage)
        {
            errorMessage = "Exception: " + unableToContactServerTestFailedValueDoesNotFallWithinTheExpectedRange +
                           Environment.NewLine + Environment.NewLine + "Inner Exception: " +
                           unableToContactServerTestFailedValueDoesNotFallWithinTheExpectedRange;

            var viewModel = ScenarioContext.Current.Get<SharepointServerSourceViewModel>("viewModel");
            Assert.AreEqual(errorMessage, viewModel.TestMessage);
        }

        [When(@"Test Connecton is ""(.*)""")]
        public void WhenTestConnectonIs(string successString)
        {
            var mockUpdateManager = ScenarioContext.Current.Get<Mock<ISharePointSourceModel>>("updateManager");
            var isSuccess = String.Equals(successString, "Successful", StringComparison.InvariantCultureIgnoreCase);
            var isLongRunning = String.Equals(successString, "Long Running", StringComparison.InvariantCultureIgnoreCase);
            if (isSuccess)
            {
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<ISharepointServerSource>()));
            }
            else if (isLongRunning)
            {
                var viewModel = ScenarioContext.Current.Get<SharepointServerSourceViewModel>("viewModel");
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<ISharepointServerSource>()));
                viewModel.AsyncWorker = new AsyncWorker();
            }
            else
            {

                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<ISharepointServerSource>()))
                    .Throws(new WarewolfTestException(unableToContactServerTestFailedValueDoesNotFallWithinTheExpectedRange, new Exception(unableToContactServerTestFailedValueDoesNotFallWithinTheExpectedRange)));
            }
            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            manageSharepointServerSource.PerformTestConnection();
        }

        [When(@"I save as ""(.*)""")]
        public void WhenISaveAs(string resourceName)
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Returns(MessageBoxResult.OK).Verifiable();
            mockRequestServiceNameViewModel.Setup(a => a.ResourceName).Returns(new ResourceName("", resourceName));
            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            manageSharepointServerSource.PerformSave();
        }

        [Then(@"the save dialog is opened")]
        public void ThenTheSaveDialogIsOpened()
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Verify();
        }

        [Then(@"Validation message is thrown")]
        [When(@"Validation message is thrown")]
        public void ThenValidationMessageIsThrown()
        {
            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            var viewModel = ScenarioContext.Current.Get<SharepointServerSourceViewModel>("viewModel");
            var errorMessageFromControl = manageSharepointServerSource.GetErrorMessage();
            var errorMessageOnViewModel = viewModel.TestMessage;
            Assert.IsFalse(string.IsNullOrEmpty(errorMessageFromControl));
            var isErrorMessage = !errorMessageOnViewModel.Contains("Passed");
            Assert.IsTrue(isErrorMessage);
        }

        [Then(@"Validation message is ""(.*)""")]
        public void ThenValidationMessageIs(string message)
        {
            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            var viewModel = ScenarioContext.Current.Get<SharepointServerSourceViewModel>("viewModel");
            var errorMessageFromControl = manageSharepointServerSource.GetErrorMessage();
            var errorMessageOnViewModel = viewModel.TestMessage;
            var isErrorMessageOnControl = errorMessageFromControl.Equals(message, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(isErrorMessageOnControl);
            var isErrorMessage = errorMessageOnViewModel.Equals(message, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(isErrorMessage);
        }

        [When(@"Validation message is Not thrown")]
        public void WhenValidationMessageIsNotThrown()
        {
            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            var viewModel = ScenarioContext.Current.Get<SharepointServerSourceViewModel>("viewModel");
            var errorMessageFromControl = manageSharepointServerSource.GetErrorMessage();
            var errorMessageOnViewModel = viewModel.TestMessage;
            var isErrorMessageOnViewModel = errorMessageOnViewModel.Contains("Passed");
            var isErrorMessageOnControl = errorMessageFromControl.Contains("Passed");
            Assert.IsFalse(isErrorMessageOnViewModel);
            Assert.IsFalse(isErrorMessageOnControl);
        }

        [When(@"I Cancel the Test")]
        public void WhenICancelTheTest()
        {
            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            manageSharepointServerSource.CancelTest();
        }

        [Given(@"I type Username as ""(.*)""")]
        public void GivenITypeUsernameAs(string userName)
        {
            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            manageSharepointServerSource.EnterUserName(userName);
            var viewModel = ScenarioContext.Current.Get<SharepointServerSourceViewModel>("viewModel");
            Assert.AreEqual(userName, viewModel.UserName);
        }

        [Given(@"I type Password as ""(.*)""")]
        public void GivenITypePasswordAs(string password)
        {
            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            manageSharepointServerSource.EnterPassword(password);
            var viewModel = ScenarioContext.Current.Get<SharepointServerSourceViewModel>("viewModel");
            Assert.AreEqual(password, viewModel.Password);
        }

        [When(@"Username field as ""(.*)""")]
        public void WhenUsernameFieldAs(string userName)
        {
            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            var viewModel = ScenarioContext.Current.Get<SharepointServerSourceViewModel>("viewModel");
            Assert.AreEqual(userName, viewModel.UserName);
            Assert.AreEqual(userName, manageSharepointServerSource.GetUsername());
        }

        [When(@"Password field as ""(.*)""")]
        public void WhenPasswordFieldAs(string password)
        {
            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            var viewModel = ScenarioContext.Current.Get<SharepointServerSourceViewModel>("viewModel");
            Assert.AreEqual(password, viewModel.Password);
            Assert.AreEqual(password, manageSharepointServerSource.GetPassword());
        }

        [When(@"I save the source")]
        public void WhenISaveTheSource()
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Verifiable();
            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            manageSharepointServerSource.PerformSave();
        }

        [Given(@"I open ""(.*)"" Sharepoint source")]
        public void GivenIOpenSharepointSource(string p0)
        {
            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            var mockStudioUpdateManager = new Mock<ISharePointSourceModel>();

            mockStudioUpdateManager.Setup(model => model.ServerName).Returns("localhost");
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<Dev2.Studio.Core.Interfaces.IEnvironmentModel>();

            var sharePointServiceSourceDefinition = new SharePointServiceSourceDefinition
            {
                Name = "Test",
                Server = "http://rsaklfsvrsharep",
                AuthenticationType = AuthenticationType.Windows,
                UserName = "IntegrationTester",
                Password = "I73573r0"
            };
            mockStudioUpdateManager.Setup(model => model.FetchSource(It.IsAny<Guid>()))
                .Returns(sharePointServiceSourceDefinition);
            var manageSharepointServerSourceViewModel = new SharepointServerSourceViewModel(mockStudioUpdateManager.Object, mockEventAggregator.Object, sharePointServiceSourceDefinition, new SynchronousAsyncWorker(), mockExecutor.Object);
            manageSharepointServerSource.DataContext = manageSharepointServerSourceViewModel;
            ScenarioContext.Current.Remove("viewModel");
            ScenarioContext.Current.Add("viewModel", manageSharepointServerSourceViewModel);
        }

        [Then(@"Address is ""(.*)""")]
        public void ThenAddressIs(string address)
        {
            var manageSharepointServerSource = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            var viewModel = ScenarioContext.Current.Get<SharepointServerSourceViewModel>("viewModel");
            Assert.AreEqual(address, viewModel.ServerName);
            Assert.AreEqual(address, manageSharepointServerSource.GetAddress());
        }

        [Then(@"Authentication Type is ""(.*)""")]
        public void ThenAuthenticationTypeIs(string authenticationType)
        {
            var viewModel = ScenarioContext.Current.Get<SharepointServerSourceViewModel>("viewModel");
            Assert.AreEqual(authenticationType, viewModel.AuthenticationType.ToString());
        }

        [AfterScenario("SharepointSource")]
        public void Cleanup()
        {
            var mockExecutor = new Mock<Dev2.Studio.Core.Interfaces.IEnvironmentModel>();
            var mockUpdateManager = ScenarioContext.Current.Get<Mock<ISharePointSourceModel>>("updateManager");
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            var mockEventAggregator = new Mock<IEventAggregator>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var viewModel = new SharepointServerSourceViewModel(mockUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker(), mockExecutor.Object);
            var manageWebserviceSourceControl = ScenarioContext.Current.Get<SharepointServerSource>(Utils.ViewNameKey);
            manageWebserviceSourceControl.DataContext = viewModel;
            FeatureContext.Current.Remove("viewModel");
            FeatureContext.Current.Add("viewModel", viewModel);
            FeatureContext.Current.Remove("externalProcessExecutor");
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);

        }

    }
}
