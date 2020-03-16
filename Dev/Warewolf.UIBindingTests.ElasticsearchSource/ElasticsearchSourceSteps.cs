/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.SaveDialog;
using Dev2.Infrastructure.Tests;
using Dev2.Runtime.ServiceModel.Data;
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
using Warewolf.Test.Agent;
using Warewolf.UIBindingTests.Core;
using Warewolf.UnitTestAttributes;

namespace Warewolf.UIBindingTests.ElasticsearchSource
{
    [Binding]
    public sealed class ElasticsearchSourceSteps
    {
        readonly ScenarioContext _scenarioContext;
        string _illegalCharactersInPath = "Illegal characters in path.";
        public static Depends _containerOps;

        public ElasticsearchSourceSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this._scenarioContext = scenarioContext;
        }
        void TypeDependencyHostName(Depends dependency)
        {
            var elasticSourceControl = _scenarioContext.Get<ElasticsearchSourceControl>(Utils.ViewNameKey);
            elasticSourceControl.EnterHostName(dependency.Container.IP);
            var viewModel = _scenarioContext.Get<ElasticsearchSourceViewModel>("viewModel");
            Assert.AreEqual(dependency.Container.IP, viewModel.HostName);
        }

        [BeforeFeature(@"ElasticsearchSource")]
        public static void SetupForSystem()
        {
            Utils.SetupResourceDictionary();
            var ElasticsearchSourceControl = new ElasticsearchSourceControl();
            var mockStudioUpdateManager = new Mock<IElasticsearchSourceModel>();
            mockStudioUpdateManager.Setup(model => model.ServerName).Returns("rsaklfwynand");
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var ElasticsearchSourceViewModel = new ElasticsearchSourceViewModel(mockStudioUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker(), mockExecutor.Object);
            ElasticsearchSourceControl.DataContext = ElasticsearchSourceViewModel;
            Utils.ShowTheViewForTesting(ElasticsearchSourceControl);
            FeatureContext.Current.Add(Utils.ViewNameKey, ElasticsearchSourceControl);
            FeatureContext.Current.Add(Utils.ViewModelNameKey, ElasticsearchSourceViewModel);
            FeatureContext.Current.Add("updateManager", mockStudioUpdateManager);
            FeatureContext.Current.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
        }

        [BeforeScenario(@"ElasticsearchSource")]
        public void SetupForElasticsearchSource()
        {
            _scenarioContext.Add(Utils.ViewNameKey, FeatureContext.Current.Get<ElasticsearchSourceControl>(Utils.ViewNameKey));
            _scenarioContext.Add("updateManager", FeatureContext.Current.Get<Mock<IElasticsearchSourceModel>>("updateManager"));
            _scenarioContext.Add("requestServiceNameViewModel", FeatureContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
            _scenarioContext.Add("externalProcessExecutor", FeatureContext.Current.Get<Mock<IExternalProcessExecutor>>("externalProcessExecutor"));
            _scenarioContext.Add(Utils.ViewModelNameKey, FeatureContext.Current.Get<ElasticsearchSourceViewModel>(Utils.ViewModelNameKey));
        }
        [Given(@"I open New Elasticsearch Source")]
        public void GivenIOpenNewElasticsearchSource()
        {
            var elasticsearchSourceControl = _scenarioContext.Get<ElasticsearchSourceControl>(Utils.ViewNameKey);
            Assert.IsNotNull(elasticsearchSourceControl);
        }

        [Then(@"""(.*)"" tab is opened")]
        public void ThenTabIsOpened(string headerText)
        {
            var viewModel = _scenarioContext.Get<IDockAware>("viewModel");
            Assert.AreEqual(headerText, viewModel.Header);
        }

        [Then(@"title is ""(.*)""")]
        public void ThenTitleIs(string title)
        {
            var elasticsearchSourceControl = _scenarioContext.Get<ElasticsearchSourceControl>(Utils.ViewNameKey);
            var viewModel = _scenarioContext.Get<ElasticsearchSourceViewModel>("viewModel");
            Assert.AreEqual(title, viewModel.HeaderText);
            Assert.AreEqual(title, elasticsearchSourceControl.GetHeaderText());
        }

        [Given(@"I type HostName as a valid anonymous Elasticsearch server")]
        public void GivenITypeHostNameAsAValidAnonymousElasticsearchServer()
        {
            _containerOps = new Depends(Depends.ContainerType.Elasticsearch);
            TypeDependencyHostName(new Depends(Depends.ContainerType.Elasticsearch));
        }

        [Then(@"server port is ""(.*)""")]
        public void ThenServerPortIs(int port)
        {
            var viewModel = _scenarioContext.Get<ElasticsearchSourceViewModel>("viewModel");
            var elasticsearchSourceControl = _scenarioContext.Get<ElasticsearchSourceControl>(Utils.ViewNameKey);
            Assert.AreEqual(port.ToString(), viewModel.Port);
            Assert.AreEqual(port.ToString(), elasticsearchSourceControl.GetPort());
        }

        [Given(@"I type port number as ""(.*)""")]
        [Then(@"I type port number as ""(.*)""")]
        [When(@"I change port number to ""(.*)""")]
        public void ThenITypePortNumberAs(string portNumber)
        {
            var elasticsearchSourceControl = _scenarioContext.Get<ElasticsearchSourceControl>(Utils.ViewNameKey);
            elasticsearchSourceControl.EnterPortNumber(portNumber);
            var viewModel = _scenarioContext.Get<ElasticsearchSourceViewModel>("viewModel");
            Assert.AreEqual(portNumber, viewModel.Port);
        }


        [Given(@"I Select Authentication Type as ""(.*)""")]
        [When(@"I Select Authentication Type as ""(.*)""")]
        [Then(@"I Select Authentication Type as ""(.*)""")]
        [Then(@"Select Authentication Type as ""(.*)""")]
        [When(@"I edit Authentication Type as ""(.*)""")]
        [Given(@"Select Authentication Type as ""(.*)""")]
        public void ThenISelectAuthenticationTypeAs(string authenticationTypeString)
        {
            var authenticationType = String.Equals(authenticationTypeString, "Password",
             StringComparison.OrdinalIgnoreCase)
             ? AuthenticationType.Password
             : AuthenticationType.Anonymous;

            var elasticsearchSourceControl = _scenarioContext.Get<ElasticsearchSourceControl>(Utils.ViewNameKey);
            elasticsearchSourceControl.SetAuthenticationType(authenticationType);
            var viewModel = _scenarioContext.Get<ElasticsearchSourceViewModel>("viewModel");
            Assert.AreEqual(authenticationType, viewModel.AuthenticationType);
        }

        [Given(@"Password field is ""(.*)""")]
        [When(@"Password field is ""(.*)""")]
        [Then(@"Password field is ""(.*)""")]
        public void ThenPasswordFieldIs(string visibility)
        {
            var expectedVisibility = String.Equals(visibility, "Collapsed", StringComparison.InvariantCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;
            var elasticsearchSourceControl = _scenarioContext.Get<ElasticsearchSourceControl>(Utils.ViewNameKey);
            var databaseDropDownVisibility = elasticsearchSourceControl.GetPasswordVisibility();
            Assert.AreEqual(expectedVisibility, databaseDropDownVisibility);
        }

        [Then(@"Test Connecton is ""(.*)""")]
        [When(@"Test Connecton is ""(.*)""")]
        public void WhenTestConnectonIs(string successString)
        {
            var mockUpdateManager = _scenarioContext.Get<Mock<IElasticsearchSourceModel>>("updateManager");
            var isSuccess = String.Equals(successString, "Successful", StringComparison.InvariantCultureIgnoreCase);
            var isLongRunning = String.Equals(successString, "Long Running", StringComparison.InvariantCultureIgnoreCase);
            if (isSuccess)
            {
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<IElasticsearchSourceDefinition>()));
            }
            else if (isLongRunning)
            {
                var viewModel = _scenarioContext.Get<ElasticsearchSourceViewModel>("viewModel");
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<IElasticsearchSourceDefinition>()));
                viewModel.AsyncWorker = new AsyncWorker();
            }
            else
            {
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<IElasticsearchSourceDefinition>()))
                    .Throws(new WarewolfTestException(_illegalCharactersInPath, new Exception(_illegalCharactersInPath)));
            }
            var elasticsearchSourceControl = _scenarioContext.Get<ElasticsearchSourceControl>(Utils.ViewNameKey);
            elasticsearchSourceControl.PerformTestConnection();
        }


        [When(@"I save as ""(.*)""")]
        public void WhenISaveAs(string resourceName)
        {
            var mockRequestServiceNameViewModel = _scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ResourceName).Returns(new ResourceName("", resourceName));
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            var elasticsearchSourceControl = _scenarioContext.Get<ElasticsearchSourceControl>(Utils.ViewNameKey);
            elasticsearchSourceControl.PerformSave();
        }

        [Then(@"the save dialog is opened")]
        public void ThenTheSaveDialogIsOpened()
        {
            var mockRequestServiceNameViewModel = _scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Verify();
        }

        [Given(@"I type HostName as a valid Elasticsearch server")]
        public void GivenITypeHostNameAsAValidElasticsearchServer()
        {
            _containerOps = new Depends(Depends.ContainerType.Elasticsearch);
            TypeDependencyHostName(new Depends(Depends.ContainerType.Elasticsearch));
        }

        [Given(@"""(.*)"" is ""(.*)""")]
        [When(@"""(.*)"" is ""(.*)""")]
        [Then(@"""(.*)"" is ""(.*)""")]
        public void GivenIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, _scenarioContext.Get<ICheckControlEnabledView>(Utils.ViewNameKey), Utils.ViewNameKey);
        }

        [Given(@"I type Password")]
        [When(@"I type Password")]
        [Then(@"I type Password")]
        public void GivenITypePassword()
        {
            var elasticsearchSourceControl = _scenarioContext.Get<ElasticsearchSourceControl>(Utils.ViewNameKey);
            elasticsearchSourceControl.EnterPassword("pass123");
            var viewModel = _scenarioContext.Get<ElasticsearchSourceViewModel>("viewModel");
            Assert.AreEqual("pass123", viewModel.Password);
        }

        [When(@"I save the source")]
        public void WhenISaveTheSource()
        {
            var mockRequestServiceNameViewModel = _scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Verifiable();
            var elasticsearchSourceControl = _scenarioContext.Get<ElasticsearchSourceControl>(Utils.ViewNameKey);
            elasticsearchSourceControl.PerformSave();
        }

        [Given(@"I type HostName as ""(.*)""")]
        [Then(@"I type HostName as ""(.*)""")]
        [When(@"I change HostName to ""(.*)""")]
        public void GivenITypeHostNameAs(string hostName)
        {
            var elasticsearchSourceControl = _scenarioContext.Get<ElasticsearchSourceControl>(Utils.ViewNameKey);
            elasticsearchSourceControl.EnterHostName(hostName);
            var viewModel = _scenarioContext.Get<ElasticsearchSourceViewModel>("viewModel");
            Assert.AreEqual(hostName, viewModel.HostName);
        }

        [Given(@"Validation message is thrown")]
        [When(@"Validation message is thrown")]
        [Then(@"Validation message is thrown")]
        public void WhenValidationMessageIsThrown()
        {
            var elasticsearchSourceControl = _scenarioContext.Get<ElasticsearchSourceControl>(Utils.ViewNameKey);
            var viewModel = _scenarioContext.Get<ElasticsearchSourceViewModel>("viewModel");
            var errorMessageFromControl = elasticsearchSourceControl.GetErrorMessage();
            var errorMessageOnViewModel = viewModel.TestMessage;
            Assert.AreNotEqual(string.IsNullOrEmpty(errorMessageFromControl), errorMessageOnViewModel);
            var isErrorMessage = !errorMessageOnViewModel.Contains("Passed");
            Assert.IsTrue(isErrorMessage);
        }

        [When(@"Validation message is Not thrown")]
        public void WhenValidationMessageIsNotThrown()
        {
            var elasticsearchSourceControl = _scenarioContext.Get<ElasticsearchSourceControl>(Utils.ViewNameKey);
            var viewModel = _scenarioContext.Get<ElasticsearchSourceViewModel>("viewModel");
            var errorMessageFromControl = elasticsearchSourceControl.GetErrorMessage();
            var errorMessageOnViewModel = viewModel.TestMessage;
            var isErrorMessageOnViewModel = !errorMessageOnViewModel.Contains("Passed");
            var isErrorMessageOnControl = !errorMessageFromControl.Contains("Passed");
            Assert.IsFalse(isErrorMessageOnViewModel);
            Assert.IsFalse(isErrorMessageOnControl);
        }

        [Given(@"I open ""(.*)"" Elasticsearch source")]
        public void GivenIOpenElasticsearchSource(string resourceName)
        {
            var elasticsearchSourceControl = _scenarioContext.Get<ElasticsearchSourceControl>(Utils.ViewNameKey);
            var mockStudioUpdateManager = new Mock<IElasticsearchSourceModel>();
            mockStudioUpdateManager.Setup(model => model.ServerName).Returns("rsaklfwynand");
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();

            var elasticsearchSourceDefinition = new ElasticsearchSourceDefinition
            {
                Name = "Test-Elasticsearch",
                HostName = "http://rsaklfwynand",
                Password = "pass123",
                Port = "9200"
            };
            mockStudioUpdateManager.Setup(model => model.FetchSource(It.IsAny<Guid>())).Returns(elasticsearchSourceDefinition);
            var elasticsearchSourceViewModel = new ElasticsearchSourceViewModel(mockStudioUpdateManager.Object, mockEventAggregator.Object, elasticsearchSourceDefinition, new SynchronousAsyncWorker(), mockExecutor.Object);
            elasticsearchSourceControl.DataContext = elasticsearchSourceViewModel;
            _scenarioContext.Remove("viewModel");
            _scenarioContext.Add("viewModel", elasticsearchSourceViewModel);
        }

        [Given(@"HostName is ""(.*)""")]
        [When(@"HostName is ""(.*)""")]
        [Then(@"HostName is ""(.*)""")]
        public void ThenHostNameIs(string hostName)
        {
            var elasticsearchSourceControl = _scenarioContext.Get<ElasticsearchSourceControl>(Utils.ViewNameKey);
            var viewModel = _scenarioContext.Get<ElasticsearchSourceViewModel>("viewModel");
            Assert.AreEqual(hostName, viewModel.HostName);
            Assert.AreEqual(hostName, elasticsearchSourceControl.GetHostName());
        }

        [AfterScenario(@"ElasticsearchSource")]
        public void Cleanup()
        {
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var mockUpdateManager = _scenarioContext.Get<Mock<IElasticsearchSourceModel>>("updateManager");
            var mockRequestServiceNameViewModel = _scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            var mockEventAggregator = new Mock<IEventAggregator>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var viewModel = new ElasticsearchSourceViewModel(mockUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker(), mockExecutor.Object);
            var elasticsearchSourceControl = _scenarioContext.Get<ElasticsearchSourceControl>(Utils.ViewNameKey);
            elasticsearchSourceControl.DataContext = viewModel;
            FeatureContext.Current.Remove("viewModel");
            FeatureContext.Current.Add("viewModel", viewModel);
            FeatureContext.Current.Remove("externalProcessExecutor");
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
        }
    }
}
