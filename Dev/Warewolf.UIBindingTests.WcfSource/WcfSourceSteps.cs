using System;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter;
using Warewolf.Studio.ServerProxyLayer;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;
using Warewolf.UIBindingTests.Core;

namespace Warewolf.UIBindingTests.WcfSource
{
    [Binding]
    public class WcfSourceSteps
    {
        [BeforeFeature("WcfSource")]
        public static void SetupForSystem()
        {
            Utils.SetupResourceDictionary();
            var manageWcfSourceControl = new ManageWcfSourceControl();
            var mockStudioUpdateManager = new Mock<IWcfSourceModel>();
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var manageWcfSourceViewModel = new ManageWcfSourceViewModel(mockStudioUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker(), new Mock<IEnvironmentModel>().Object);
            manageWcfSourceControl.DataContext = manageWcfSourceViewModel;
            Utils.ShowTheViewForTesting(manageWcfSourceControl);
            FeatureContext.Current.Add(Utils.ViewNameKey, manageWcfSourceControl);
            FeatureContext.Current.Add(Utils.ViewModelNameKey, manageWcfSourceViewModel);
            FeatureContext.Current.Add("updateManager", mockStudioUpdateManager);
            FeatureContext.Current.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
        }

        [BeforeScenario("WcfSource")]
        public void SetupForWcfSource()
        {
            ScenarioContext.Current.Add(Utils.ViewNameKey, FeatureContext.Current.Get<ManageWcfSourceControl>(Utils.ViewNameKey));
            ScenarioContext.Current.Add("updateManager", FeatureContext.Current.Get<Mock<IWcfSourceModel>>("updateManager"));
            ScenarioContext.Current.Add("requestServiceNameViewModel", FeatureContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
            ScenarioContext.Current.Add("externalProcessExecutor", FeatureContext.Current.Get<Mock<IExternalProcessExecutor>>("externalProcessExecutor"));
            ScenarioContext.Current.Add(Utils.ViewModelNameKey, FeatureContext.Current.Get<ManageWcfSourceViewModel>(Utils.ViewModelNameKey));
        }

        [Given(@"I open New Wcf Source")]
        public void GivenIOpenNewWcfSource()
        {
            var manageWcfSourceControl = ScenarioContext.Current.Get<ManageWcfSourceControl>(Utils.ViewNameKey);
            Assert.IsNotNull(manageWcfSourceControl);
            Assert.IsNotNull(manageWcfSourceControl.DataContext);
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
            var manageWcfSourceControl = ScenarioContext.Current.Get<ManageWcfSourceControl>(Utils.ViewNameKey);
            var viewModel = ScenarioContext.Current.Get<ManageWcfSourceViewModel>("viewModel");
            Assert.AreEqual(title, viewModel.HeaderText);
            Assert.AreEqual(title, manageWcfSourceControl.GetHeaderText());
        }

        [Then(@"""(.*)"" input is ""(.*)""")]
        public void ThenInputIs(string controlName, string value)
        {
            var manageWcfSourceControl = ScenarioContext.Current.Get<ManageWcfSourceControl>(Utils.ViewNameKey);
            Assert.AreEqual(value, manageWcfSourceControl.GetInputValue(controlName));
        }

        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, ScenarioContext.Current.Get<ICheckControlEnabledView>(Utils.ViewNameKey), Utils.ViewNameKey);
        }

        [Then(@"I type WCF Endpoint Url as ""(.*)""")]
        public void ThenITypeWCFEndpointUrlAs(string endpointUrl)
        {
            var manageWcfSourceControl = ScenarioContext.Current.Get<ManageWcfSourceControl>(Utils.ViewNameKey);
            manageWcfSourceControl.EnterEndpointUrl(endpointUrl);
            var viewModel = ScenarioContext.Current.Get<ManageWcfSourceViewModel>("viewModel");
            Assert.AreEqual(endpointUrl, viewModel.EndpointUrl);
        }

        [Then(@"Send is ""(.*)""")]
        public void ThenSendIs(string successString)
        {
            var mockUpdateManager = ScenarioContext.Current.Get<Mock<IWcfSourceModel>>("updateManager");
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
            var manageWcfSourceControl = ScenarioContext.Current.Get<ManageWcfSourceControl>(Utils.ViewNameKey);
            manageWcfSourceControl.TestSend();
        }

        [Then(@"the error message is ""(.*)""")]
        public void ThenTheErrorMessageIs(string errorMessage)
        {
            var viewModel = ScenarioContext.Current.Get<ManageWcfSourceViewModel>("viewModel");
            Assert.AreEqual(errorMessage, viewModel.TestMessage);
        }
    }
}
