using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Controller;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Studio.AntiCorruptionLayer;
using Warewolf.Studio.ServerProxyLayer;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;
// ReSharper disable RedundantAssignment

namespace Warewolf.UIBindingTests.ServerSource
{
    [Binding]
    public class NewServerSourceSteps
    {
        string connectionErrorUnauthorized = "Connection Error: Unauthorized";

        [BeforeFeature("ServerSource")]
        public static void SetupForSystem()
        {
            Core.Utils.SetupResourceDictionary();
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
            Core.Utils.ShowTheViewForTesting(manageServerControl);
            FeatureContext.Current.Add(Core.Utils.ViewNameKey, manageServerControl);
            FeatureContext.Current.Add("updateManager", mockStudioUpdateManager);
            FeatureContext.Current.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
        }

        [BeforeScenario("ServerSource")]
        public void SetupForServerSource()
        {
            ScenarioContext.Current.Add(Core.Utils.ViewNameKey, FeatureContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey));
            ScenarioContext.Current.Add("updateManager", FeatureContext.Current.Get<Mock<IManageServerSourceModel>>("updateManager"));
            ScenarioContext.Current.Add("requestServiceNameViewModel", FeatureContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
            ScenarioContext.Current.Add("externalProcessExecutor", FeatureContext.Current.Get<Mock<IExternalProcessExecutor>>("externalProcessExecutor"));
        }

        [Given(@"I open New Server Source")]
        public void GivenIOpenNewServerSource()
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
            Assert.IsNotNull(manageServerControl);
            Assert.IsNotNull(manageServerControl.DataContext);
        }

        [Then(@"""(.*)"" tab is opened")]
        public void ThenTabIsOpened(string headerText)
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(headerText, viewModel.HeaderText);
        }

        [Then(@"selected protocol is ""(.*)""")]
        public void ThenSelectedProtocolIs(string protocol)
        {
            var view = Core.Utils.GetView<ManageServerControl>();
            view.SetProtocol(protocol);
            var viewModel = GetViewModel(view);
            Assert.AreEqual(protocol, viewModel.Protocol);
        }

        [Then(@"server port is ""(.*)""")]
        public void ThenServerPortIs(int port)
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
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

            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
            manageServerControl.SetAuthenticationType(authenticationType);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(authenticationType, viewModel.AuthenticationType);
        }

        [Given(@"""(.*)"" is ""(.*)""")]
        [When(@"""(.*)"" is ""(.*)""")]
        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string controlName, string enabledString)
        {
            Core.Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Core.Utils.ViewNameKey), Core.Utils.ViewNameKey);
        }

        [Given(@"I type Server as ""(.*)""")]
        public void GivenITypeServerAs(string serverName)
        {
            if (serverName == "Incorrect")
            {

            }
            else
            {
                var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
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
            var view = Core.Utils.GetView<ManageServerControl>();
            view.SetProtocol(protocol);
            var viewModel = GetViewModel(view);
            Assert.AreEqual(protocol, viewModel.Protocol);
        }

        [Given(@"I enter server port as ""(.*)""")]
        public void GivenIEnterServerPortAs(int port)
        {
            var view = Core.Utils.GetView<ManageServerControl>();
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

            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
            manageServerControl.SetAuthenticationType(authenticationType);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(authenticationType, viewModel.AuthenticationType);
        }

        [Given(@"I open ""(.*)"" server source")]
        public void GivenIOpenServerSource(string editingServerSource)
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
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


            mockStudioUpdateManager.Setup(model => model.FetchSource(It.IsAny<Guid>()))
                .Returns(serverSource);
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
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(serverName, viewModel.ServerName.Name);
        }

        [When(@"I Test Connection to remote server")]
        public void WhenITestConnectionToRemoteServer()
        {
            var view = Core.Utils.GetView<ManageServerControl>();
            view.TestAction();
        }

        [When(@"I enter Username as ""(.*)""")]
        public void WhenIEnterUsernameAs(string username)
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
            manageServerControl.EnterUserName(username);
            Assert.AreEqual(username, manageServerControl.GetUsername());
        }

        [When(@"I enter Password as ""(.*)""")]
        public void WhenIEnterPasswordAs(string password)
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
            manageServerControl.EnterPassword(password);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(password, viewModel.Password);
        }

        [Then(@"the error message is ""(.*)""")]
        public void ThenTheErrorMessageIs(string errorMessage)
        {
            errorMessage = "Exception: " + connectionErrorUnauthorized + Environment.NewLine + Environment.NewLine +
                           "Inner Exception: " + connectionErrorUnauthorized;

            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(errorMessage, viewModel.TestMessage);
        }

        [Then(@"validation message is ""(.*)""")]
        public void ThenValidationMessageIs(string errorMsg)
        {
            string newErrorMsg = errorMsg;

            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
            var viewModel = GetViewModel(manageServerControl);
            var errorMessageFromControl = manageServerControl.GetErrorMessage();
            var errorMessageOnViewModel = viewModel.TestMessage;

            if (!string.IsNullOrWhiteSpace(newErrorMsg) && newErrorMsg != "Passed")
            {
                newErrorMsg = "Exception: " + errorMsg + Environment.NewLine + Environment.NewLine + "Inner Exception: " + errorMsg;
            }

            var isErrorMessageOnControl = errorMessageFromControl.Equals(newErrorMsg, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(isErrorMessageOnControl);
            if (string.IsNullOrWhiteSpace(errorMsg))
            {
                Assert.AreEqual(errorMsg, "");
            }
            else
            {
                var isErrorMessage = errorMessageOnViewModel.Equals(newErrorMsg, StringComparison.OrdinalIgnoreCase);
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

            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
            var userNameVisibility = manageServerControl.GetUsernameVisibility();
            Assert.AreEqual(expectedVisibility, userNameVisibility);
        }

        [Then(@"server Password field is ""(.*)""")]
        public void ThenServerPasswordFieldIs(string visibility)
        {
            var expectedVisibility = String.Equals(visibility, "Collapsed", StringComparison.InvariantCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;

            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
            var passwordVisibility = manageServerControl.GetPasswordVisibility();
            Assert.AreEqual(expectedVisibility, passwordVisibility);
        }

        [When(@"I save the server source")]
        [Then(@"I save the server source")]
        public void WhenISaveTheServerSource()
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Verifiable();
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
            manageServerControl.PerformSave();
        }

        [When(@"Test Connecton is ""(.*)""")]
        [Then(@"Test Connecton is ""(.*)""")]
        public void WhenTestConnectonIs(string successString)
        {
            var mockUpdateManager = ScenarioContext.Current.Get<Mock<IManageServerSourceModel>>("updateManager");
            var isSuccess = String.Equals(successString, "Passed", StringComparison.InvariantCultureIgnoreCase);
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
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
                    .Throws(new WarewolfTestException(connectionErrorUnauthorized, new Exception(connectionErrorUnauthorized)));
            }
            manageServerControl.TestAction();
        }


        [Then(@"server Username is ""(.*)""")]
        public void ThenServerUsernameIs(string username)
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(username, viewModel.UserName);
            Assert.AreEqual(username, manageServerControl.GetUsername());
        }

        [Then(@"server Password is is ""(.*)""")]
        public void ThenServerPasswordIsIs(string password)
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
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

            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
            manageServerControl.SetAuthenticationType(authenticationType);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(authenticationType, viewModel.AuthenticationType);
        }

        [Then(@"tab name is ""(.*)""")]
        public void ThenTabNameIs(string headerText)
        {
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
            var viewModel = GetViewModel(manageServerControl);
            Assert.AreEqual(headerText, viewModel.Header);
        }

        [Given(@"Warewolf server is running")]
        public void GivenWarewolfServerIsRunning()
        {

            var environmentModel = EnvironmentRepository.Instance.Source;
            if (!environmentModel.IsConnected)
                environmentModel.Connect();
            var controllerFactory = new CommunicationControllerFactory();
            var environmentConnection = environmentModel.Connection;
            var manager = new StudioResourceUpdateManager
                (controllerFactory, environmentConnection);
            var proxyLayer = new StudioServerProxy(controllerFactory, environmentConnection);
            ScenarioContext.Current.Add("environmentModel", environmentModel);
            ScenarioContext.Current.Add("studioResourceUpdateManager", manager);
            ScenarioContext.Current.Add("proxyLayer", proxyLayer);
        }

        [Given(@"I create new server source to Gendev as ""(.*)""")]
        public void GivenICreateNewServerSourceToGendevAs(string sourceName)
        {
            var serverSource = new Dev2.Common.Interfaces.Core.ServerSource
            {
                Name = sourceName
            };
            ScenarioContext.Current.Add("serverSource", serverSource);
        }

        [When(@"I change the values as")]
        public void WhenIChangeTheValuesAs(Table table)
        {
            var protocol = table.Rows[0]["Protocol"];
            var serverName = table.Rows[0]["ServerName"];
            var suthentication = table.Rows[0]["Authentication"];
            string port = table.Rows[0]["port"];
            var serverSource = ScenarioContext.Current.Get<IServerSource>("serverSource");
            var resourceId = Guid.NewGuid();

            if (!ScenarioContext.Current.ContainsKey("resourceId"))
            {
                ScenarioContext.Current.Add("resourceId", resourceId);
                serverSource.ID = resourceId;
            }
           
            serverSource.Address = $"{protocol}://{serverName}:{port}";
            if (suthentication == "Public")
            {
                serverSource.AuthenticationType = AuthenticationType.Public;
            }
            else if (suthentication == "Windows")
            {
                serverSource.AuthenticationType = AuthenticationType.Windows;
            }
            else if (suthentication == "User")
            {
                serverSource.AuthenticationType = AuthenticationType.User;
            }
        }

        [When(@"I save ""(.*)""")]
        public void WhenISave(string sourceName)
        {
            var serverSource = ScenarioContext.Current.Get<IServerSource>("serverSource");
            var studioResourceUpdateManager = ScenarioContext.Current.Get<StudioResourceUpdateManager>("studioResourceUpdateManager");
            studioResourceUpdateManager.Save(serverSource);
        }

        [When(@"I open ""(.*)""")]
        public void WhenIOpen(string p0)
        {
            var guid = ScenarioContext.Current.Get<Guid>("resourceId");
            var environmentModel = ScenarioContext.Current.Get<IEnvironmentModel>("environmentModel");
            IContextualResourceModel loadContextualResourceModel = environmentModel.ResourceRepository.LoadContextualResourceModel(guid);
            if(ScenarioContext.Current.ContainsKey("resourceModel"))
                ScenarioContext.Current["resourceModel"] = loadContextualResourceModel;
            else
                ScenarioContext.Current.Add("resourceModel", loadContextualResourceModel);
        }

        [Then(@"the server source has correct values as")]
        public void ThenTheServerSourceHasCorrectValuesAs(Table table)
        {
            var protocol = table.Rows[0]["Protocol"];
            var serverName = table.Rows[0]["ServerName"];
            var authentication = table.Rows[0]["Authentication"];
            var port = table.Rows[0]["port"];
            var resourceModel = ScenarioContext.Current.Get<IContextualResourceModel>("resourceModel");
            var hasCorrectProtocol = resourceModel.WorkflowXaml.ToString().Contains(protocol);
            var hasCorrectserverName = resourceModel.WorkflowXaml.ToString().Contains(serverName);
            var hasCorrectport = resourceModel.WorkflowXaml.ToString().Contains(port);

            Assert.IsTrue(hasCorrectProtocol);
            Assert.IsTrue(hasCorrectserverName);
            Assert.IsTrue(hasCorrectport);
            
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
            var manageServerControl = ScenarioContext.Current.Get<ManageServerControl>(Core.Utils.ViewNameKey);
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
