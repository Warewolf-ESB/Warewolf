using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.RabbitMQ;
using Dev2.Common.SaveDialog;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;
using Warewolf.UIBindingTests.Core;
using Warewolf.UnitTestAttributes;


namespace Warewolf.UIBindingTests.RabbitMqSource
{
    [Binding]
    public class RabbitMqSourceSteps
    {
        string failedNoneOfTheSpecifiedEndpointsWereReachable = "Failed: None of the specified endpoints were reachable";
        public static Depends _containerOps;
        static FeatureContext _featureContext;
        readonly ScenarioContext _scenarioContext;

        public RabbitMqSourceSteps(ScenarioContext scenarioContext) => _scenarioContext = scenarioContext;

        [BeforeFeature("RabbitMqSource")]
        public static void SetupForSystem(FeatureContext featureContext)
        {
            _featureContext = featureContext;
            Utils.SetupResourceDictionary();
            var manageRabbitMqSourceControl = new ManageRabbitMQSourceControl();
            var mockStudioUpdateManager = new Mock<IRabbitMQSourceModel>();
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var manageRabbitMqSourceViewModel = new ManageRabbitMQSourceViewModel(mockStudioUpdateManager.Object, task);
            manageRabbitMqSourceControl.DataContext = manageRabbitMqSourceViewModel;
            Utils.ShowTheViewForTesting(manageRabbitMqSourceControl);
            _featureContext.Add(Utils.ViewNameKey, manageRabbitMqSourceControl);
            _featureContext.Add(Utils.ViewModelNameKey, manageRabbitMqSourceViewModel);
            _featureContext.Add("updateManager", mockStudioUpdateManager);
            _featureContext.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
            _featureContext.Add("externalProcessExecutor", mockExecutor);
        }

        [BeforeScenario("RabbitMqSource")]
        public void SetupForRabbitMqSource()
        {
            _scenarioContext.Add(Utils.ViewNameKey, _featureContext.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey));
            _scenarioContext.Add("updateManager", _featureContext.Get<Mock<IRabbitMQSourceModel>>("updateManager"));
            _scenarioContext.Add("requestServiceNameViewModel", _featureContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
            _scenarioContext.Add("externalProcessExecutor", _featureContext.Get<Mock<IExternalProcessExecutor>>("externalProcessExecutor"));
            _scenarioContext.Add(Utils.ViewModelNameKey, _featureContext.Get<ManageRabbitMQSourceViewModel>(Utils.ViewModelNameKey));
        }

        [AfterScenario]
        public void CleanUp() => _containerOps?.Dispose();

        [Given(@"I open New RabbitMq Source")]
        public void GivenIOpenNewRabbitMqSource()
        {
            var manageRabbitMqSourceControl = _scenarioContext.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            Assert.IsNotNull(manageRabbitMqSourceControl);
        }

        [Then(@"""(.*)"" tab is opened")]
        public void ThenTabIsOpened(string headerText)
        {
            var viewModel = _scenarioContext.Get<IDockAware>("viewModel");
            Assert.AreEqual(headerText, viewModel.Header);
        }

        [Then(@"the title is ""(.*)""")]
        public void ThenTheTitleIs(string title)
        {
            var manageRabbitMqSourceControl = _scenarioContext.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            var viewModel = _scenarioContext.Get<ManageRabbitMQSourceViewModel>("viewModel");
            Assert.AreEqual(title, viewModel.HeaderText);
            Assert.AreEqual(title, manageRabbitMqSourceControl.GetHeaderText());
        }

        [Then(@"""(.*)"" input is ""(.*)""")]
        public void ThenInputIs(string controlName, string value)
        {
            var manageRabbitMqSourceControl = _scenarioContext.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            Assert.AreEqual(value, manageRabbitMqSourceControl.GetInputValue(controlName));
        }

        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, _scenarioContext.Get<ICheckControlEnabledView>(Utils.ViewNameKey), Utils.ViewNameKey);
        }

        [Then(@"I type Host as ""(.*)""")]
        public void ThenITypeHostAs(string hostname)
        {
            if (hostname == "test-rabbitmq")
            {
                _containerOps = new Depends(Depends.ContainerType.RabbitMQ);
            }
            var manageRabbitMqSourceControl = _scenarioContext.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            manageRabbitMqSourceControl.EnterHostName(hostname);
            var viewModel = _scenarioContext.Get<ManageRabbitMQSourceViewModel>("viewModel");
            Assert.AreEqual(hostname, viewModel.HostName);
        }

        [Then(@"I type Username as ""(.*)""")]
        public void ThenITypeUsernameAs(string username)
        {
            var manageRabbitMqSourceControl = _scenarioContext.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            manageRabbitMqSourceControl.EnterUserName(username);
            var viewModel = _scenarioContext.Get<ManageRabbitMQSourceViewModel>("viewModel");
            Assert.AreEqual(username, viewModel.UserName);
        }

        [Then(@"I type Password as ""(.*)""")]
        public void ThenITypePasswordAs(string password)
        {
            var manageRabbitMqSourceControl = _scenarioContext.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            manageRabbitMqSourceControl.EnterPassword(password);
            var viewModel = _scenarioContext.Get<ManageRabbitMQSourceViewModel>("viewModel");
            Assert.AreEqual(password, viewModel.Password);
        }

        [When(@"I click ""(.*)""")]
        public void WhenIClick(string control)
        {
            var manageRabbitMqSourceControl = _scenarioContext.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            manageRabbitMqSourceControl.TestPublish();
        }

        [When(@"Send is ""(.*)""")]
        [Then(@"Send is ""(.*)""")]
        public void WhenSendIs(string successString)
        {
            var mockUpdateManager = _scenarioContext.Get<Mock<IRabbitMQSourceModel>>("updateManager");
            var isSuccess = String.Equals(successString, "Successful", StringComparison.InvariantCultureIgnoreCase);
            if (isSuccess)
            {
                mockUpdateManager.Setup(manager => manager.TestSource(It.IsAny<IRabbitMQServiceSourceDefinition>()));
            }
            else
            {
                mockUpdateManager.Setup(model => model.TestSource(It.IsAny<IRabbitMQServiceSourceDefinition>()))
                    .Throws(new WarewolfTestException(failedNoneOfTheSpecifiedEndpointsWereReachable, new Exception(failedNoneOfTheSpecifiedEndpointsWereReachable)));
            }
            var manageRabbitMqSourceControl = _scenarioContext.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            manageRabbitMqSourceControl.TestPublish();
        }

        [When(@"I save as ""(.*)""")]
        public void WhenISaveAs(string resourceName)
        {
            var mockRequestServiceNameViewModel = _scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ResourceName).Returns(new ResourceName("", resourceName));
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            var manageRabbitMqSourceControl = _scenarioContext.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            manageRabbitMqSourceControl.PerformSave();
        }

        [When(@"the save dialog is opened")]
        public void WhenTheSaveDialogIsOpened()
        {
            var mockRequestServiceNameViewModel = _scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Verify();
        }

        [Then(@"the error message is ""(.*)""")]
        public void ThenTheErrorMessageIs(string errorMessage)
        {
            errorMessage = "Exception: " + failedNoneOfTheSpecifiedEndpointsWereReachable + Environment.NewLine +
                           Environment.NewLine + "Inner Exception: " + failedNoneOfTheSpecifiedEndpointsWereReachable;

            var viewModel = _scenarioContext.Get<ManageRabbitMQSourceViewModel>("viewModel");
            Assert.AreEqual(errorMessage, viewModel.TestErrorMessage);
        }

        [AfterScenario("RabbitMqSource")]
        public void Cleanup()
        {
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var mockUpdateManager = _scenarioContext.Get<Mock<IRabbitMQSourceModel>>("updateManager");
            var mockRequestServiceNameViewModel = _scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var viewModel = new ManageRabbitMQSourceViewModel(mockUpdateManager.Object, task);
            var manageRabbitMqSourceControl = _scenarioContext.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            manageRabbitMqSourceControl.DataContext = viewModel;
            _featureContext.Remove("viewModel");
            _featureContext.Add("viewModel", viewModel);
            _featureContext.Remove("externalProcessExecutor");
            _featureContext.Add("externalProcessExecutor", mockExecutor);

        }
    }
}
