using System;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;
using Warewolf.UIBindingTests.Core;

namespace Warewolf.UIBindingTests.WcfSource
{
    [Binding]
    public class WcfSourceSteps
    {
        static FeatureContext _featureContext;
        readonly ScenarioContext _scenarioContext;

        public WcfSourceSteps(ScenarioContext scenarioContext) => _scenarioContext = scenarioContext;
        
        [BeforeFeature("WcfSource")]
        public static void SetupForSystem(FeatureContext featureContext)
        {
            _featureContext = featureContext;
            Utils.SetupResourceDictionary();
            var manageWcfSourceControl = new ManageWcfSourceControl();
            var mockStudioUpdateManager = new Mock<IWcfSourceModel>();
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var manageWcfSourceViewModel = new ManageWcfSourceViewModel(mockStudioUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker(), new Mock<IServer>().Object);
            manageWcfSourceControl.DataContext = manageWcfSourceViewModel;
            Utils.ShowTheViewForTesting(manageWcfSourceControl);
            _featureContext.Add(Utils.ViewNameKey, manageWcfSourceControl);
            _featureContext.Add(Utils.ViewModelNameKey, manageWcfSourceViewModel);
            _featureContext.Add("updateManager", mockStudioUpdateManager);
            _featureContext.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
            _featureContext.Add("externalProcessExecutor", mockExecutor);
        }

        [BeforeScenario("WcfSource")]
        public void SetupForWcfSource()
        {
            _scenarioContext.Add(Utils.ViewNameKey, _featureContext.Get<ManageWcfSourceControl>(Utils.ViewNameKey));
            _scenarioContext.Add("updateManager", _featureContext.Get<Mock<IWcfSourceModel>>("updateManager"));
            _scenarioContext.Add("requestServiceNameViewModel", _featureContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
            _scenarioContext.Add("externalProcessExecutor", _featureContext.Get<Mock<IExternalProcessExecutor>>("externalProcessExecutor"));
            _scenarioContext.Add(Utils.ViewModelNameKey, _featureContext.Get<ManageWcfSourceViewModel>(Utils.ViewModelNameKey));
        }

        [Given(@"I open New Wcf Source")]
        public void GivenIOpenNewWcfSource()
        {
            var manageWcfSourceControl = _scenarioContext.Get<ManageWcfSourceControl>(Utils.ViewNameKey);
            Assert.IsNotNull(manageWcfSourceControl);
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
            var manageWcfSourceControl = _scenarioContext.Get<ManageWcfSourceControl>(Utils.ViewNameKey);
            var viewModel = _scenarioContext.Get<ManageWcfSourceViewModel>("viewModel");
            Assert.AreEqual(title, viewModel.HeaderText);
            Assert.AreEqual(title, manageWcfSourceControl.GetHeaderText());
        }

        [Then(@"""(.*)"" input is ""(.*)""")]
        public void ThenInputIs(string controlName, string value)
        {
            var manageWcfSourceControl = _scenarioContext.Get<ManageWcfSourceControl>(Utils.ViewNameKey);
            Assert.AreEqual(value, manageWcfSourceControl.GetInputValue(controlName));
        }

        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, _scenarioContext.Get<ICheckControlEnabledView>(Utils.ViewNameKey), Utils.ViewNameKey);
        }

        [Then(@"I type WCF Endpoint Url as ""(.*)""")]
        public void ThenITypeWCFEndpointUrlAs(string endpointUrl)
        {
            var manageWcfSourceControl = _scenarioContext.Get<ManageWcfSourceControl>(Utils.ViewNameKey);
            manageWcfSourceControl.EnterEndpointUrl(endpointUrl);
            var viewModel = _scenarioContext.Get<ManageWcfSourceViewModel>("viewModel");
            Assert.AreEqual(endpointUrl, viewModel.EndpointUrl);
        }

        [Then(@"Send is ""(.*)""")]
        public void ThenSendIs(string successString)
        {
            var mockUpdateManager = _scenarioContext.Get<Mock<IWcfSourceModel>>("updateManager");
            var isSuccess = String.Equals(successString, "Successful", StringComparison.InvariantCultureIgnoreCase);
            if (isSuccess)
            {
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<IWcfServerSource>()));
            }
            else
            {
                mockUpdateManager.Setup(model => model.TestConnection(It.IsAny<IWcfServerSource>()))
                    .Throws(new WarewolfTestException("Invalid URI: The format of the URI could not be determined.", null));
            }
            var manageWcfSourceControl = _scenarioContext.Get<ManageWcfSourceControl>(Utils.ViewNameKey);
            manageWcfSourceControl.TestSend();
        }

        [Then(@"the error message is ""(.*)""")]
        public void ThenTheErrorMessageIs(string errorMessage)
        {
            var viewModel = _scenarioContext.Get<ManageWcfSourceViewModel>("viewModel");
            Assert.AreEqual(errorMessage, viewModel.TestMessage);
        }
    }
}
