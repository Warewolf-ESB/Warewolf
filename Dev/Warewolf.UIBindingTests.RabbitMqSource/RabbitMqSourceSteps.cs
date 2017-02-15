using System;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.RabbitMQ;
using Dev2.Common.Interfaces.SaveDialog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter;
using Warewolf.Studio.ServerProxyLayer;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;
using Warewolf.UIBindingTests.Core;
// ReSharper disable RedundantAssignment

namespace Warewolf.UIBindingTests.RabbitMqSource
{
    [Binding]
    public class RabbitMqSourceSteps
    {
        string failedNoneOfTheSpecifiedEndpointsWereReachable = "Failed: None of the specified endpoints were reachable";

        [BeforeFeature("RabbitMqSource")]
        public static void SetupForSystem()
        {
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
            FeatureContext.Current.Add(Utils.ViewNameKey, manageRabbitMqSourceControl);
            FeatureContext.Current.Add(Utils.ViewModelNameKey, manageRabbitMqSourceViewModel);
            FeatureContext.Current.Add("updateManager", mockStudioUpdateManager);
            FeatureContext.Current.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
        }

        [BeforeScenario("RabbitMqSource")]
        public void SetupForRabbitMqSource()
        {
            ScenarioContext.Current.Add(Utils.ViewNameKey, FeatureContext.Current.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey));
            ScenarioContext.Current.Add("updateManager", FeatureContext.Current.Get<Mock<IRabbitMQSourceModel>>("updateManager"));
            ScenarioContext.Current.Add("requestServiceNameViewModel", FeatureContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
            ScenarioContext.Current.Add("externalProcessExecutor", FeatureContext.Current.Get<Mock<IExternalProcessExecutor>>("externalProcessExecutor"));
            ScenarioContext.Current.Add(Utils.ViewModelNameKey, FeatureContext.Current.Get<ManageRabbitMQSourceViewModel>(Utils.ViewModelNameKey));
        }

        [Given(@"I open New RabbitMq Source")]
        public void GivenIOpenNewRabbitMqSource()
        {
            var manageRabbitMqSourceControl = ScenarioContext.Current.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            Assert.IsNotNull(manageRabbitMqSourceControl);
            Assert.IsNotNull(manageRabbitMqSourceControl.DataContext);
        }

        [Then(@"""(.*)"" tab is opened")]
        public void ThenTabIsOpened(string headerText)
        {
            var viewModel = ScenarioContext.Current.Get<IDockAware>("viewModel");
            Assert.AreEqual(headerText, viewModel.Header);
        }

        [Then(@"the title is ""(.*)""")]
        public void ThenTheTitleIs(string title)
        {
            var manageRabbitMqSourceControl = ScenarioContext.Current.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            var viewModel = ScenarioContext.Current.Get<ManageRabbitMQSourceViewModel>("viewModel");
            Assert.AreEqual(title, viewModel.HeaderText);
            Assert.AreEqual(title, manageRabbitMqSourceControl.GetHeaderText());
        }

        [Then(@"""(.*)"" input is ""(.*)""")]
        public void ThenInputIs(string controlName, string value)
        {
            var manageRabbitMqSourceControl = ScenarioContext.Current.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            Assert.AreEqual(value, manageRabbitMqSourceControl.GetInputValue(controlName));
        }

        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey), Utils.ViewNameKey);
        }

        [Then(@"I type Host as ""(.*)""")]
        public void ThenITypeHostAs(string hostname)
        {
            var manageRabbitMqSourceControl = ScenarioContext.Current.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            manageRabbitMqSourceControl.EnterHostName(hostname);
            var viewModel = ScenarioContext.Current.Get<ManageRabbitMQSourceViewModel>("viewModel");
            Assert.AreEqual(hostname, viewModel.HostName);
        }

        [Then(@"I type Username as ""(.*)""")]
        public void ThenITypeUsernameAs(string username)
        {
            var manageRabbitMqSourceControl = ScenarioContext.Current.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            manageRabbitMqSourceControl.EnterUserName(username);
            var viewModel = ScenarioContext.Current.Get<ManageRabbitMQSourceViewModel>("viewModel");
            Assert.AreEqual(username, viewModel.UserName);
        }

        [Then(@"I type Password as ""(.*)""")]
        public void ThenITypePasswordAs(string password)
        {
            var manageRabbitMqSourceControl = ScenarioContext.Current.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            manageRabbitMqSourceControl.EnterPassword(password);
            var viewModel = ScenarioContext.Current.Get<ManageRabbitMQSourceViewModel>("viewModel");
            Assert.AreEqual(password, viewModel.Password);
        }

        [When(@"I click ""(.*)""")]
        public void WhenIClick(string control)
        {
            var manageRabbitMqSourceControl = ScenarioContext.Current.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            manageRabbitMqSourceControl.TestPublish();
        }

        [When(@"Send is ""(.*)""")]
        [Then(@"Send is ""(.*)""")]
        public void WhenSendIs(string successString)
        {
            var mockUpdateManager = ScenarioContext.Current.Get<Mock<IRabbitMQSourceModel>>("updateManager");
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
            var manageRabbitMqSourceControl = ScenarioContext.Current.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            manageRabbitMqSourceControl.TestPublish();
        }

        [When(@"I save as ""(.*)""")]
        public void WhenISaveAs(string resourceName)
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ResourceName).Returns(new ResourceName("", resourceName));
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            var manageRabbitMqSourceControl = ScenarioContext.Current.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            manageRabbitMqSourceControl.PerformSave();
        }

        [When(@"the save dialog is opened")]
        public void WhenTheSaveDialogIsOpened()
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Verify();
        }

        [Then(@"the error message is ""(.*)""")]
        public void ThenTheErrorMessageIs(string errorMessage)
        {
            errorMessage = "Exception: " + failedNoneOfTheSpecifiedEndpointsWereReachable + Environment.NewLine +
                           Environment.NewLine + "Inner Exception: " + failedNoneOfTheSpecifiedEndpointsWereReachable;

            var viewModel = ScenarioContext.Current.Get<ManageRabbitMQSourceViewModel>("viewModel");
            Assert.AreEqual(errorMessage, viewModel.TestErrorMessage);
        }

        [AfterScenario("RabbitMqSource")]
        public void Cleanup()
        {
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var mockUpdateManager = ScenarioContext.Current.Get<Mock<IRabbitMQSourceModel>>("updateManager");
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var viewModel = new ManageRabbitMQSourceViewModel(mockUpdateManager.Object, task);
            var manageRabbitMqSourceControl = ScenarioContext.Current.Get<ManageRabbitMQSourceControl>(Utils.ViewNameKey);
            manageRabbitMqSourceControl.DataContext = viewModel;
            FeatureContext.Current.Remove("viewModel");
            FeatureContext.Current.Add("viewModel", viewModel);
            FeatureContext.Current.Remove("externalProcessExecutor");
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);

        }
    }
}
