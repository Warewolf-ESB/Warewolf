using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.SaveDialog;
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

namespace Warewolf.UIBindingTests.EmailSource
{
    [Binding]
    public class EmailSourceSteps
    {
        [BeforeFeature("EmailSource")]
        public static void SetupForSystem()
        {
            Utils.SetupResourceDictionary();
            var manageEmailSourceControl = new ManageEmailSourceControl();
            var mockStudioUpdateManager = new Mock<IManageEmailSourceModel>();
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var manageEmailSourceViewModel = new ManageEmailSourceViewModel(mockStudioUpdateManager.Object, task, mockEventAggregator.Object);
            manageEmailSourceControl.DataContext = manageEmailSourceViewModel;
            Utils.ShowTheViewForTesting(manageEmailSourceControl);
            FeatureContext.Current.Add(Utils.ViewNameKey, manageEmailSourceControl);
            FeatureContext.Current.Add(Utils.ViewModelNameKey, manageEmailSourceViewModel);
            FeatureContext.Current.Add("updateManager", mockStudioUpdateManager);
            FeatureContext.Current.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
        }

        [BeforeScenario("EmailSource")]
        public void SetupForEmailSource()
        {
            ScenarioContext.Current.Add(Utils.ViewNameKey, FeatureContext.Current.Get<ManageEmailSourceControl>(Utils.ViewNameKey));
            ScenarioContext.Current.Add("updateManager", FeatureContext.Current.Get<Mock<IManageEmailSourceModel>>("updateManager"));
            ScenarioContext.Current.Add("requestServiceNameViewModel", FeatureContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
            ScenarioContext.Current.Add("externalProcessExecutor", FeatureContext.Current.Get<Mock<IExternalProcessExecutor>>("externalProcessExecutor"));
            ScenarioContext.Current.Add(Utils.ViewModelNameKey, FeatureContext.Current.Get<ManageEmailSourceViewModel>(Utils.ViewModelNameKey));
        }

        [Given(@"I open New Email Source")]
        public void GivenIOpenNewEmailSource()
        {
            var manageEmailSourceControl = ScenarioContext.Current.Get<ManageEmailSourceControl>(Utils.ViewNameKey);
            Assert.IsNotNull(manageEmailSourceControl);
            Assert.IsNotNull(manageEmailSourceControl.DataContext); 
        }

        [Given(@"I open ""(.*)""")]
        public void GivenIOpen(string p0)
        {
            var manageEmailSourceControl = ScenarioContext.Current.Get<ManageEmailSourceControl>(Utils.ViewNameKey);
            var mockStudioUpdateManager = new Mock<IManageEmailSourceModel>();
            mockStudioUpdateManager.Setup(model => model.ServerName).Returns("localhost");
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();

            var emailServiceSourceDefinition = new EmailServiceSourceDefinition
            {
                ResourceName = "Test Email Source",
                HostName = "smtp.gmail.com",
                UserName = "warewolf@dev2.co.za",
                Password = "Dev_tech*",
                EnableSsl = false,
                Port = 25,
                Timeout = 100,
                EmailFrom = "warewolf@dev2.co.za",
                EmailTo = "info@dev2.co.za"
            };
            mockStudioUpdateManager.Setup(model => model.FetchSource(It.IsAny<Guid>()))
                .Returns(emailServiceSourceDefinition);
            var manageEmailSourceViewModel = new ManageEmailSourceViewModel(mockStudioUpdateManager.Object, mockEventAggregator.Object, emailServiceSourceDefinition,new SynchronousAsyncWorker());
            manageEmailSourceControl.DataContext = manageEmailSourceViewModel;
            ScenarioContext.Current.Remove("viewModel");
            ScenarioContext.Current.Add("viewModel", manageEmailSourceViewModel);
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
            var manageEmailSourceControl = ScenarioContext.Current.Get<ManageEmailSourceControl>(Utils.ViewNameKey);
            var viewModel = ScenarioContext.Current.Get<ManageEmailSourceViewModel>("viewModel");
            Assert.AreEqual(title, viewModel.HeaderText);
            Assert.AreEqual(title, manageEmailSourceControl.GetHeaderText());
        }

        [Then(@"""(.*)"" input is ""(.*)""")]
        [When(@"""(.*)"" input is ""(.*)""")]
        public void ThenInputIs(string controlName, string value)
        {
            var manageEmailSourceControl = ScenarioContext.Current.Get<ManageEmailSourceControl>(Utils.ViewNameKey);
            Assert.AreEqual(value, manageEmailSourceControl.GetInputValue(controlName));
        }

        [Then(@"I type Host as ""(.*)""")]
        public void ThenITypeHostAs(string hostname)
        {
            var manageEmailSourceControl = ScenarioContext.Current.Get<ManageEmailSourceControl>(Utils.ViewNameKey);
            manageEmailSourceControl.EnterHostName(hostname);
            var viewModel = ScenarioContext.Current.Get<ManageEmailSourceViewModel>("viewModel");
            Assert.AreEqual(hostname, viewModel.HostName);
        }

        [Then(@"I type Username as ""(.*)""")]
        [When(@"I type Username as ""(.*)""")]
        public void ThenITypeUsernameAs(string username)
        {
            var manageEmailSourceControl = ScenarioContext.Current.Get<ManageEmailSourceControl>(Utils.ViewNameKey);
            manageEmailSourceControl.EnterUserName(username);

            var viewModel = ScenarioContext.Current.Get<ManageEmailSourceViewModel>("viewModel");
            Assert.AreEqual(username, viewModel.UserName);
        }

        [Then(@"I type Password as ""(.*)""")]
        public void ThenITypePasswordAs(string password)
        {
            var manageEmailSourceControl = ScenarioContext.Current.Get<ManageEmailSourceControl>(Utils.ViewNameKey);
            manageEmailSourceControl.EnterPassword(password);
            var viewModel = ScenarioContext.Current.Get<ManageEmailSourceViewModel>("viewModel");
            Assert.AreEqual(password, viewModel.Password);
        }

        [Then(@"the error message is ""(.*)""")]
        public void ThenTheErrorMessageIs(string errorMessage)
        {
            var viewModel = ScenarioContext.Current.Get<ManageEmailSourceViewModel>("viewModel");
            Assert.AreEqual(errorMessage, viewModel.TestMessage);
        }

        [Then(@"I type From as ""(.*)""")]
        [When(@"I type From as ""(.*)""")]
        public void ThenITypeFromAs(string emailFrom)
        {
            var manageEmailSourceControl = ScenarioContext.Current.Get<ManageEmailSourceControl>(Utils.ViewNameKey);
            manageEmailSourceControl.EnterEmailFrom(emailFrom);
            var viewModel = ScenarioContext.Current.Get<ManageEmailSourceViewModel>("viewModel");
            Assert.AreEqual(emailFrom, viewModel.EmailFrom);
        }

        [Then(@"From input is ""(.*)""")]
        public void ThenFromInputIs(string p0)
        {
            var viewModel = ScenarioContext.Current.Get<ManageEmailSourceViewModel>("viewModel");
            Assert.AreEqual(p0,viewModel.EmailFrom);
        }


        [Then(@"I type To as ""(.*)""")]
        public void ThenITypeToAs(string emailTo)
        {
            var manageEmailSourceControl = ScenarioContext.Current.Get<ManageEmailSourceControl>(Utils.ViewNameKey);
            manageEmailSourceControl.EnterEmailTo(emailTo);
            var viewModel = ScenarioContext.Current.Get<ManageEmailSourceViewModel>("viewModel");
            Assert.AreEqual(emailTo, viewModel.EmailTo);
        }

        [Then(@"""(.*)"" is ""(.*)""")]
        [When(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey), Utils.ViewNameKey);
        }

        [When(@"the save dialog is opened")]
        public void WhenTheSaveDialogIsOpened()
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Verify();
        }

        [When(@"I click ""(.*)""")]
        public void WhenIClick(string control)
        {
            var manageEmailSourceControl = ScenarioContext.Current.Get<ManageEmailSourceControl>(Utils.ViewNameKey);
            manageEmailSourceControl.TestSend();
        }

        [When(@"I save as ""(.*)""")]
        public void WhenISaveAs(string resourceName)
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ResourceName).Returns(new ResourceName("", resourceName));
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            var manageEmailSourceControl = ScenarioContext.Current.Get<ManageEmailSourceControl>(Utils.ViewNameKey);
            manageEmailSourceControl.PerformSave();
        }

        [Then(@"the save dialog is opened")]
        public void ThenTheSaveDialogIsOpened()
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Verify();
        }

        [When(@"Send is ""(.*)""")]
        [Then(@"Send is ""(.*)""")]
        public void WhenSendIs(string successString)
        {
            var mockUpdateManager = ScenarioContext.Current.Get<Mock<IManageEmailSourceModel>>("updateManager");
            var isSuccess = String.Equals(successString, "Successful", StringComparison.InvariantCultureIgnoreCase);
            if (isSuccess)
            {
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<IEmailServiceSource>()));
            }
            else
            {
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<IEmailServiceSource>()))
                    .Throws(new WarewolfTestException("Failed to Send: One or more errors occurred", null));
            }
            var manageEmailSourceControl = ScenarioContext.Current.Get<ManageEmailSourceControl>(Utils.ViewNameKey);
            manageEmailSourceControl.TestSend();
            Thread.Sleep(3000);
        }

        [Given(@"I edit ""(.*)""")]
        public void GivenIEdit(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [AfterScenario("EmailSource")]
        public void Cleanup()
        {
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var mockUpdateManager = ScenarioContext.Current.Get<Mock<IManageEmailSourceModel>>("updateManager");
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            var mockEventAggregator = new Mock<IEventAggregator>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var viewModel = new ManageEmailSourceViewModel(mockUpdateManager.Object, task, mockEventAggregator.Object);
            var manageEmailSourceControl = ScenarioContext.Current.Get<ManageEmailSourceControl>(Utils.ViewNameKey);
            manageEmailSourceControl.DataContext = viewModel;
            FeatureContext.Current.Remove("viewModel");
            FeatureContext.Current.Add("viewModel", viewModel);
            FeatureContext.Current.Remove("externalProcessExecutor");
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);

        }
    }
}
