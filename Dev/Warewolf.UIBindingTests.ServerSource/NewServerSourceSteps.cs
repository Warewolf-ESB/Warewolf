using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Threading;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.UIBindingTests.Core;
using Warewolf.Studio.ServerProxyLayer;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;

namespace Warewolf.UIBindingTests.ServerSource
{
    [Binding]
    public class NewServerSourceSteps
    {
        [BeforeFeature("ServerSource")]
        public static void SetupForSystem()
        {
            Utils.SetupResourceDictionary();
            var manageServerControl = new ManageServerControl();
            var mockStudioUpdateManager = new Mock<IManageServerSourceModel>();
            mockStudioUpdateManager.Setup(model => model.ServerName).Returns("localhost");
            mockStudioUpdateManager.Setup(model => model.GetComputerNames()).Returns(new List<string> { "rsaklfhuggspc", "barney", "SANDBOX-1" });
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var manageServerSourceViewModel = new ManageNewServerViewModel(mockStudioUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker(), mockExecutor.Object);
            manageServerControl.DataContext = manageServerSourceViewModel;
            Utils.ShowTheViewForTesting(manageServerControl);
            FeatureContext.Current.Add(Utils.ViewNameKey, manageServerControl);
            FeatureContext.Current.Add("updateManager", mockStudioUpdateManager);
            FeatureContext.Current.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
        }

        [BeforeScenario("ServerSource")]
        public void SetupForServerSource()
        {
            ScenarioContext.Current.Add(Utils.ViewNameKey, FeatureContext.Current.Get<ManageServerControl>(Utils.ViewNameKey));
            ScenarioContext.Current.Add("updateManager", FeatureContext.Current.Get<Mock<IManageServerSourceModel>>("updateManager"));
            ScenarioContext.Current.Add("requestServiceNameViewModel", FeatureContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
            ScenarioContext.Current.Add("externalProcessExecutor", FeatureContext.Current.Get<Mock<IExternalProcessExecutor>>("externalProcessExecutor"));
        }

        [Given(@"I open New Server Source")]
        public void GivenIOpenNewServerSource()
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            Assert.IsNotNull(manageServerControl);
            Assert.IsNotNull(manageServerControl.DataContext);
        }

        [Then(@"""(.*)"" tab is opened")]
        public void ThenTabIsOpened(string headerText)
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(headerText, viewModel.HeaderText);
        }

        [Then(@"selected protocol is ""(.*)""")]
        public void ThenSelectedProtocolIs(string protocol)
        {
            var view = Utils.GetView<ManageServerControl>();
            view.SetProtocol(protocol);
            var viewModel = GetViewModel(view);
            Assert.AreEqual(protocol, viewModel.Protocol);
        }

        [Then(@"server port is ""(.*)""")]
        public void ThenServerPortIs(int port)
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(port.ToString(), viewModel.SelectedPort);
            Assert.AreEqual(port.ToString(), manageServerControl.GetPort());
        }

        [Then(@"Authentication Type selected is ""(.*)""")]
        public void ThenAuthenticationTypeSelectedIs(string authenticationTypeString)
        {
            var authenticationType = String.Equals(authenticationTypeString, "User",
                StringComparison.OrdinalIgnoreCase)
                ? AuthenticationType.User
                : AuthenticationType.Windows;

            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            manageServerControl.SetAuthenticationType(authenticationType);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(authenticationType, viewModel.AuthenticationType);
        }

        [Given(@"""(.*)"" is ""(.*)""")]
        [When(@"""(.*)"" is ""(.*)""")]
        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey));
        }

        [Given(@"I type Server as ""(.*)""")]
        public void GivenITypeServerAs(string serverName)
        {
            if (serverName == "Incorrect")
            {

            }
            else
            {
                var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
                manageServerControl.EnterServerName(serverName);
                var viewModel = GetViewModel(manageServerControl);
                if (viewModel != null)
                {
                    Assert.AreEqual(serverName, viewModel.ServerName.Name);
                }
            }
        }

        static ManageNewServerViewModel GetViewModel(ManageServerControl manageDatabaseSourceControl)
        {
            var viewModel = manageDatabaseSourceControl.DataContext as ManageNewServerViewModel;
            return viewModel;
        }

        [Given(@"I select protocol as ""(.*)""")]
        public void GivenISelectProtocolAs(string protocol)
        {
            var view = Utils.GetView<ManageServerControl>();
            view.SetProtocol(protocol);
            var viewModel = GetViewModel(view);
            Assert.AreEqual(protocol, viewModel.Protocol);
        }

        [Given(@"I enter server port as ""(.*)""")]
        public void GivenIEnterServerPortAs(int port)
        {
            var view = Utils.GetView<ManageServerControl>();
            view.SetPort(port.ToString());
            var viewModel = GetViewModel(view);
            Assert.AreEqual(port.ToString(), viewModel.SelectedPort);
        }

        [Given(@"Authentication Type as ""(.*)""")]
        public void GivenAuthenticationTypeAs(string authenticationTypeString)
        {
            AuthenticationType authenticationType;
            switch (authenticationTypeString)
            {
                case "User":
                    authenticationType = String.Equals(authenticationTypeString, "User", StringComparison.OrdinalIgnoreCase)
                        ? AuthenticationType.User : AuthenticationType.Windows;
                    break;
                case "Windows":
                    authenticationType = String.Equals(authenticationTypeString, "Windows", StringComparison.OrdinalIgnoreCase)
                        ? AuthenticationType.Windows : AuthenticationType.Public;
                    break;
                case "Public":
                    authenticationType = String.Equals(authenticationTypeString, "Public", StringComparison.OrdinalIgnoreCase)
                        ? AuthenticationType.Public : AuthenticationType.Windows;
                    break;
                default:
                    authenticationType = String.Equals(authenticationTypeString, "Windows", StringComparison.OrdinalIgnoreCase)
                        ? AuthenticationType.Windows : AuthenticationType.Public;
                    break;
            }

            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            manageServerControl.SetAuthenticationType(authenticationType);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(authenticationType, viewModel.AuthenticationType);
        }

        [Given(@"I open ""(.*)"" server source")]
        public void GivenIOpenServerSource(string editingServerSource)
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            var mockStudioUpdateManager = new Mock<IManageServerSourceModel>();
            mockStudioUpdateManager.Setup(model => model.ServerName).Returns("localhost");
            mockStudioUpdateManager.Setup(model => model.GetComputerNames()).Returns(new List<string> { "rsaklfhuggspc", "barney", "SANDBOX-1" });
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();

            var serverSourceDefinition = new Dev2.Common.Interfaces.Core.ServerSource
            {
                Name = "ServerSource",
                Address = "https://SANDBOX-1:3143",
                ServerName = "SANDBOX-1",
                AuthenticationType = AuthenticationType.User,
                UserName = "Integrationtester",
                Password = "I73573r0"
            };
            var externalServerSourceDefinition = new Dev2.Common.Interfaces.Core.ServerSource
            {
                Name = "TestWarewolf",
                Address = "http://test-warewolf.cloudapp.net:3142",
                ServerName = "test-warewolf.cloudapp.net",
                AuthenticationType = AuthenticationType.Public
            };

            Dev2.Common.Interfaces.Core.ServerSource serverSource;

            switch (editingServerSource)
            {
                case "ServerSource":
                    serverSource = serverSourceDefinition;
                    break;
                case "TestWarewolf":
                    serverSource = externalServerSourceDefinition;
                    break;
                default:
                    serverSource = serverSourceDefinition;
                    break;
            }



            FeatureContext.Current["svrsrc"] = serverSource;
            var viewModel = GetViewModel(manageServerControl);
            var manageServerSourceViewModel = new ManageNewServerViewModel(mockStudioUpdateManager.Object, mockEventAggregator.Object, serverSource, new SynchronousAsyncWorker(), mockExecutor.Object);

            manageServerControl.EnterPassword(manageServerSourceViewModel.Password);
            manageServerControl.EnterUserName(manageServerSourceViewModel.UserName);
            manageServerControl.SetPort(manageServerSourceViewModel.SelectedPort);
            manageServerControl.SetProtocol(manageServerSourceViewModel.Protocol);
            manageServerControl.SetAuthenticationType(manageServerSourceViewModel.AuthenticationType);
            manageServerControl.EnterServerName(manageServerSourceViewModel.ServerName.Name);
            manageServerControl.SelectServer(manageServerSourceViewModel.ServerName.Name);
            viewModel.Password = manageServerSourceViewModel.Password;
            viewModel.UserName = manageServerSourceViewModel.UserName;
            viewModel.Protocol = manageServerSourceViewModel.Protocol;
            viewModel.AuthenticationType = manageServerSourceViewModel.AuthenticationType;
            viewModel.ServerName = manageServerSourceViewModel.ServerName;
            viewModel.Header = manageServerSourceViewModel.Header;
            viewModel.HeaderText = manageServerSourceViewModel.HeaderText;
            viewModel.Item = manageServerSourceViewModel.Item;
        }

        [Then(@"Server as ""(.*)""")]
        public void ThenServerAs(string serverName)
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(serverName, viewModel.ServerName.Name);
        }

        [When(@"I Test Connection to remote server")]
        public void WhenITestConnectionToRemoteServer()
        {
            var view = Utils.GetView<ManageServerControl>();
            view.TestAction();
        }

        [When(@"I enter Username as ""(.*)""")]
        public void WhenIEnterUsernameAs(string username)
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            manageServerControl.EnterUserName(username);
            Assert.AreEqual(username, manageServerControl.GetUsername());
        }

        [When(@"I enter Password as ""(.*)""")]
        public void WhenIEnterPasswordAs(string password)
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            manageServerControl.EnterPassword(password);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(password, viewModel.Password);
        }

        [Then(@"validation message is ""(.*)""")]
        public void ThenValidationMessageIs(string errorMsg)
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            var viewModel = GetViewModel(manageServerControl);
            var errorMessageFromControl = manageServerControl.GetErrorMessage();
            var errorMessageOnViewModel = viewModel.TestMessage;
            var isErrorMessageOnControl = errorMessageFromControl.Equals(errorMsg, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(isErrorMessageOnControl);
            if (string.IsNullOrWhiteSpace(errorMsg))
            {
                Assert.AreEqual(errorMsg, "");
            }
            else
            {
                var isErrorMessage = errorMessageOnViewModel.Equals(errorMsg, StringComparison.OrdinalIgnoreCase);
                Assert.IsTrue(isErrorMessage);
            }
        }

        [Then(@"the save dialog is opened")]
        [When(@"the save dialog is opened")]
        public void ThenTheSaveDialogIsOpened()
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Verify();
        }

        [Then(@"server Username field is ""(.*)""")]
        public void ThenServerUsernameFieldIs(string visibility)
        {
            var expectedVisibility = String.Equals(visibility, "Collapsed", StringComparison.InvariantCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;

            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            var userNameVisibility = manageServerControl.GetUsernameVisibility();
            Assert.AreEqual(expectedVisibility, userNameVisibility);
        }

        [Then(@"server Password field is ""(.*)""")]
        public void ThenServerPasswordFieldIs(string visibility)
        {
            var expectedVisibility = String.Equals(visibility, "Collapsed", StringComparison.InvariantCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;

            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            var passwordVisibility = manageServerControl.GetPasswordVisibility();
            Assert.AreEqual(expectedVisibility, passwordVisibility);
        }

        [When(@"I save the server source")]
        [Then(@"I save the server source")]
        public void WhenISaveTheServerSource()
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Verifiable();
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            manageServerControl.PerformSave();
        }

        [When(@"Test Connecton is ""(.*)""")]
        [Then(@"Test Connecton is ""(.*)""")]
        public void WhenTestConnectonIs(string successString)
        {
            var mockUpdateManager = ScenarioContext.Current.Get<Mock<IManageServerSourceModel>>("updateManager");
            var isSuccess = String.Equals(successString, "Passed", StringComparison.InvariantCultureIgnoreCase);
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            var isLongRunning = String.Equals(successString, "Long Running", StringComparison.InvariantCultureIgnoreCase);
            if (isSuccess)
            {
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<IServerSource>()));
            }
            else if (isLongRunning)
            {
                var viewModel = GetViewModel(manageServerControl);
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<IServerSource>()));
                viewModel.AsyncWorker = new AsyncWorker();
            }
            else
            {
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<IServerSource>()))
                    .Throws(new WarewolfTestException("Connection Error: Unauthorized", null));
            }
            manageServerControl.TestAction();
        }


        [Then(@"server Username is ""(.*)""")]
        public void ThenServerUsernameIs(string username)
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(username, viewModel.UserName);
            Assert.AreEqual(username, manageServerControl.GetUsername());
        }

        [Then(@"server Password is is ""(.*)""")]
        public void ThenServerPasswordIsIs(string password)
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(password, viewModel.Password);
            Assert.AreEqual(password, manageServerControl.GetPassword());
        }

        [Then(@"Authentication Type as ""(.*)""")]
        public void ThenAuthenticationTypeAs(string authenticationTypeString)
        {
            var authenticationType = String.Equals(authenticationTypeString, "Public",
                StringComparison.OrdinalIgnoreCase)
                ? AuthenticationType.Public
                : AuthenticationType.Windows;

            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            manageServerControl.SetAuthenticationType(authenticationType);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(authenticationType, viewModel.AuthenticationType);
        }

        [Then(@"tab name is ""(.*)""")]
        public void ThenTabNameIs(string headerText)
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(headerText, viewModel.Header);
        }

        [AfterScenario("ServerSource")]
        public void Cleanup()
        {
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var mockUpdateManager = ScenarioContext.Current.Get<Mock<IManageServerSourceModel>>("updateManager");
            mockUpdateManager.Setup(model => model.ServerName).Returns("localhost");
            mockUpdateManager.Setup(model => model.GetComputerNames()).Returns(new List<string> { "rsaklfhuggspc", "barney", "SANDBOX-1" });
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            var mockEventAggregator = new Mock<IEventAggregator>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var viewModel = new ManageNewServerViewModel(mockUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker(), mockExecutor.Object);
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Utils.ViewNameKey);
            var originalViewModel = GetViewModel(manageServerControl);
            manageServerControl.EnterPassword(viewModel.Password);
            manageServerControl.EnterUserName(viewModel.UserName);
            manageServerControl.SetPort(viewModel.SelectedPort);
            manageServerControl.SetProtocol(viewModel.Protocol);
            manageServerControl.SetAuthenticationType(viewModel.AuthenticationType);
            manageServerControl.EnterServerName(viewModel.ServerName.Name);
            originalViewModel.Password = viewModel.Password;
            originalViewModel.UserName = viewModel.UserName;
            originalViewModel.Protocol = viewModel.Protocol;
            originalViewModel.AuthenticationType = viewModel.AuthenticationType;
            originalViewModel.ServerName = viewModel.ServerName;

            FeatureContext.Current.Remove("externalProcessExecutor");
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);

        }
    }
}
