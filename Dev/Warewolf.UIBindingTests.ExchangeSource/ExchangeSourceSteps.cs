using System;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.UIBindingTests.Core;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;
using Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter;
using Warewolf.Studio.ServerProxyLayer;

namespace Warewolf.UIBindingTests.ExchangeSource
{
    [Binding]
    public class ExchangeSourceSteps
    {
        [BeforeFeature("ExchangeSource")]
        public static void SetupForSystem()
        {
            Utils.SetupResourceDictionary();
            var manageExhangeSourceControl = new ManageExchangeSourceControl();
            var mockStudioUpdateManager = new Mock<IManageExchangeSourceModel>();
            mockStudioUpdateManager.Setup(model => model.ServerName).Returns("localhost");
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);

            task.Start();

            var manageExchangeSourceViewModel = new ManageExchangeSourceViewModel(mockStudioUpdateManager.Object, task, mockEventAggregator.Object);
            manageExhangeSourceControl.DataContext = manageExchangeSourceViewModel;
            Utils.ShowTheViewForTesting(manageExhangeSourceControl);
            FeatureContext.Current.Add(Utils.ViewNameKey, manageExhangeSourceControl);
            FeatureContext.Current.Add(Utils.ViewModelNameKey, manageExchangeSourceViewModel);
            FeatureContext.Current.Add("updateManager", mockStudioUpdateManager);
            FeatureContext.Current.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
            task.Wait();
            task.Dispose();
        }

        [BeforeScenario("ExchangeSource")]
        public void SetupForDatabaseSource()
        {
            ScenarioContext.Current.Add(Utils.ViewNameKey, FeatureContext.Current.Get<ManageExchangeSourceControl>(Utils.ViewNameKey));
            ScenarioContext.Current.Add("updateManager", FeatureContext.Current.Get<Mock<IManageExchangeSourceModel>>("updateManager"));
            ScenarioContext.Current.Add("requestServiceNameViewModel", FeatureContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
            ScenarioContext.Current.Add(Utils.ViewModelNameKey, FeatureContext.Current.Get<ManageExchangeSourceViewModel>(Utils.ViewModelNameKey));
        }

        [Given(@"I open a new exchange source")]
        public void GivenIOpenANewExchangeSource()
        {
            var manageExchangeSourceControl = ScenarioContext.Current.Get<ManageExchangeSourceControl>(Utils.ViewNameKey);
            Assert.IsNotNull(manageExchangeSourceControl);
            Assert.IsNotNull(manageExchangeSourceControl.DataContext);
        }

        [Then(@"""(.*)"" tab is Opened")]
        public void ThenTabIsOpened(string headerText)
        {
            var viewModel = ScenarioContext.Current.Get<IDockAware>("viewModel");
            // ReSharper disable once RedundantNameQualifier
            Assert.AreEqual(headerText, ((ManageExchangeSourceViewModel)viewModel).HeaderText);
        }

        [Then(@"Title is ""(.*)""")]
        public void ThenTitleIs(string title)
        {
            var manageExchangeSourceViewModel = ScenarioContext.Current.Get<ManageExchangeSourceViewModel>(Utils.ViewModelNameKey);
            Assert.AreEqual(manageExchangeSourceViewModel.HeaderText, title);
        }

        [When(@"I Type Auto Discover as ""(.*)""")]
        public void WhenITypeAutoDiscoverAs(string url)
        {
            var manageExchangeSourceViewModel = ScenarioContext.Current.Get<ManageExchangeSourceViewModel>(Utils.ViewModelNameKey);
            manageExchangeSourceViewModel.AutoDiscoverUrl = url;
        }

        [When(@"I Type User Name as ""(.*)""")]
        public void WhenITypeUserNameAs(string username)
        {
            var manageExchangeSourceViewModel = ScenarioContext.Current.Get<ManageExchangeSourceViewModel>(Utils.ViewModelNameKey);
            manageExchangeSourceViewModel.UserName = username;
        }

        [When(@"I Type Password as ""(.*)""")]
        public void WhenITypePasswordAs(string password)
        {
            var manageExchangeSourceViewModel = ScenarioContext.Current.Get<ManageExchangeSourceViewModel>(Utils.ViewModelNameKey);
            manageExchangeSourceViewModel.Password = password;
        }

        [When(@"I Type TimeOut as ""(.*)""")]
        public void WhenITypeTimeOutAs(int timeout)
        {
            var manageExchangeSourceViewModel = ScenarioContext.Current.Get<ManageExchangeSourceViewModel>(Utils.ViewModelNameKey);
            manageExchangeSourceViewModel.Timeout = timeout;
        }

        [When(@"I Type To Email as ""(.*)""")]
        public void WhenITypeToEmailAs(string toEmail)
        {
            var manageExchangeSourceViewModel = ScenarioContext.Current.Get<ManageExchangeSourceViewModel>(Utils.ViewModelNameKey);
            manageExchangeSourceViewModel.EmailTo = toEmail;
        }

        [Then(@"I click on the Test Button")]
        public void ThenIClickOnTheTestButton()
        {
            var manageExchangeSourceViewModel = ScenarioContext.Current.Get<ManageExchangeSourceViewModel>(Utils.ViewModelNameKey);
            manageExchangeSourceViewModel.SendCommand.Execute(null);
        }

        [When(@"the error message is ""(.*)""")]
        [Then(@"the error message is ""(.*)""")]
        public void WhenTheErrorMessageIs(string errorMessage)
        {
            var viewModel = ScenarioContext.Current.Get<ManageExchangeSourceViewModel>(Utils.ViewModelNameKey);
            Assert.AreEqual(errorMessage, viewModel.TestMessage);
        }

        [When(@"Send is ""(.*)""")]
        [Then(@"Send is ""(.*)""")]
        public void WhenSendIs(string successString)
        {
            var mockUpdateManager = ScenarioContext.Current.Get<Mock<IManageExchangeSourceModel>>("updateManager");
            var isSuccess = String.Equals(successString, "Successful", StringComparison.InvariantCultureIgnoreCase);
            if (isSuccess)
            {
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<IExchangeSource>()));
            }
            else
            {
                var theRequestFailedTheRemoteServerReturnedAnErrorUnauthorized = "The request failed. The remote server returned an error: (401) Unauthorized.";
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<IExchangeSource>()))
                    .Throws(new WarewolfTestException(theRequestFailedTheRemoteServerReturnedAnErrorUnauthorized, new Exception(theRequestFailedTheRemoteServerReturnedAnErrorUnauthorized)));
            }
            var manageExchangeSourceControl = ScenarioContext.Current.Get<ManageExchangeSourceControl>(Utils.ViewNameKey);
            manageExchangeSourceControl.TestSend();
            Thread.Sleep(3000);
        }

        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey), Utils.ViewNameKey);
        }
    }
}
