/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Warewolf.UIBindingTests
{
    [Binding]
    public class RedisSourceSteps
    {
        readonly ScenarioContext _scenarioContext;
        string _illegalCharactersInPath = "Illegal characters in path.";
        public static Depends _containerOps;


        public RedisSourceSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this._scenarioContext = scenarioContext;
        }

        [BeforeFeature(@"RedisSource")]
        public static void SetupForSystem()
        {
            Utils.SetupResourceDictionary();
            var redisSourceControl = new RedisSourceControl();
            var mockStudioUpdateManager = new Mock<IRedisSourceModel>();
            mockStudioUpdateManager.Setup(model => model.ServerName).Returns("localhost");
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var redisSourceViewModel = new RedisSourceViewModel(mockStudioUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker(), mockExecutor.Object);
            redisSourceControl.DataContext = redisSourceViewModel;
            Utils.ShowTheViewForTesting(redisSourceControl);
            FeatureContext.Current.Add(Utils.ViewNameKey, redisSourceControl);
            FeatureContext.Current.Add(Utils.ViewModelNameKey, redisSourceViewModel);
            FeatureContext.Current.Add("updateManager", mockStudioUpdateManager);
            FeatureContext.Current.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
        }

        [BeforeScenario(@"RedisSource")]
        public void SetupForRedisSource()
        {
            _scenarioContext.Add(Utils.ViewNameKey, FeatureContext.Current.Get<RedisSourceControl>(Utils.ViewNameKey));
            _scenarioContext.Add("updateManager", FeatureContext.Current.Get<Mock<IRedisSourceModel>>("updateManager"));
            _scenarioContext.Add("requestServiceNameViewModel", FeatureContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
            _scenarioContext.Add("externalProcessExecutor", FeatureContext.Current.Get<Mock<IExternalProcessExecutor>>("externalProcessExecutor"));
            _scenarioContext.Add(Utils.ViewModelNameKey, FeatureContext.Current.Get<RedisSourceViewModel>(Utils.ViewModelNameKey));
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
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            var viewModel = _scenarioContext.Get<RedisSourceViewModel>("viewModel");
            Assert.AreEqual(title, viewModel.HeaderText);
            Assert.AreEqual(title, redisSourceControl.GetHeaderText());
        }

        [Given(@"I open New Redis Source")]
        public void GivenIOpenNewRedisSource()
        {
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            Assert.IsNotNull(redisSourceControl);
        }

        [Given(@"I type HostName as a valid redis server")]
        public void GivenITypeHostNameAsValidRedisServer()
        {
            _containerOps = new Depends(Depends.ContainerType.Redis);
            TypeDependencyHostName(Depends.ContainerType.Redis);
        }

        [Given(@"I type HostName as a valid anonymous redis server")]
        public void GivenITypeHostNameAsValidAnonymousRedisServer()
        {
            _containerOps = new Depends(Depends.ContainerType.AnonymousRedis);
            TypeDependencyHostName(Depends.ContainerType.AnonymousRedis);
        }

        void TypeDependencyHostName(Depends.ContainerType containerType)
        {
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            redisSourceControl.EnterHostName(Depends.GetAddress(containerType));
            var viewModel = _scenarioContext.Get<RedisSourceViewModel>("viewModel");
            Assert.AreEqual(Depends.GetAddress(containerType), viewModel.HostName);
        }

        [Given(@"I type HostName as ""(.*)""")]
        [Then(@"I type HostName as ""(.*)""")]
        [When(@"I change HostName to ""(.*)""")]
        public void ThenITypeHostNameAs(string hostName)
        {
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            redisSourceControl.EnterHostName(hostName);
            var viewModel = _scenarioContext.Get<RedisSourceViewModel>("viewModel");
            Assert.AreEqual(hostName, viewModel.HostName);
        }

        [Then(@"server port is ""(.*)""")]
        public void ThenServerPortIs(int port)
        {
            var viewModel = _scenarioContext.Get<RedisSourceViewModel>("viewModel");
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            Assert.AreEqual(port.ToString(), viewModel.Port);
            Assert.AreEqual(port.ToString(), redisSourceControl.GetPort());
        }

        [Given(@"I type port number as ""(.*)""")]
        [Then(@"I type port number as ""(.*)""")]
        [When(@"I change port number to ""(.*)""")]
        public void ThenITypePortNumberAs(string portNumber)
        {
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            redisSourceControl.EnterPortNumber(portNumber);
            var viewModel = _scenarioContext.Get<RedisSourceViewModel>("viewModel");
            Assert.AreEqual(portNumber, viewModel.Port);
        }

        [Given(@"I open ""(.*)"" redis source")]
        public void GivenIOpenRedisSource(string resourceName)
        {
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            var mockStudioUpdateManager = new Mock<IRedisSourceModel>();
            mockStudioUpdateManager.Setup(model => model.ServerName).Returns("localhost");
            var mockEventAggregator = new Mock<IEventAggregator>();
            var mockExecutor = new Mock<IExternalProcessExecutor>();

            var redisSourceDefinition = new RedisSourceDefinition
            {
                Name = "Test-Redis",
                HostName = "http://TFSBLD.premier.local/IntegrationTestSite",
                Password = "pass123",
                Port = "6379"
            };
            mockStudioUpdateManager.Setup(model => model.FetchSource(It.IsAny<Guid>()))
                .Returns(redisSourceDefinition);
            var redisSourceViewModel = new RedisSourceViewModel(mockStudioUpdateManager.Object, mockEventAggregator.Object, redisSourceDefinition, new SynchronousAsyncWorker(), mockExecutor.Object);
            redisSourceControl.DataContext = redisSourceViewModel;
            _scenarioContext.Remove("viewModel");
            _scenarioContext.Add("viewModel", redisSourceViewModel);
        }

        [Given(@"HostName is ""(.*)""")]
        [When(@"HostName is ""(.*)""")]
        [Then(@"HostName is ""(.*)""")]
        public void GivenHostNameIs(string hostName)
        {
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            var viewModel = _scenarioContext.Get<RedisSourceViewModel>("viewModel");
            Assert.AreEqual(hostName, viewModel.HostName);
            Assert.AreEqual(hostName, redisSourceControl.GetHostName());
        }

        [Given(@"Password is ""(.*)""")]
        public void GivenPasswordIs(string password)
        {
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            Assert.AreEqual(password, redisSourceControl.GetPassword());
        }

        [When(@"I Cancel the Test")]
        public void WhenICancelTheTest()
        {
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            redisSourceControl.CancelTest();
        }

        [Given(@"Validation message is thrown")]
        [When(@"Validation message is thrown")]
        [Then(@"Validation message is thrown")]
        public void WhenValidationMessageIsThrown()
        {
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            var viewModel = _scenarioContext.Get<RedisSourceViewModel>("viewModel");
            var errorMessageFromControl = redisSourceControl.GetErrorMessage();
            var errorMessageOnViewModel = viewModel.TestMessage;
            Assert.AreNotEqual(string.IsNullOrEmpty(errorMessageFromControl), errorMessageOnViewModel);
            var isErrorMessage = !errorMessageOnViewModel.Contains("Passed");
            Assert.IsTrue(isErrorMessage);
        }

        [Then(@"Validation message is ""(.*)""")]
        public void ThenValidationMessageIs(string msg)
        {
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            var viewModel = _scenarioContext.Get<RedisSourceViewModel>("viewModel");
            var errorMessageFromControl = redisSourceControl.GetErrorMessage();
            var errorMessageOnViewModel = viewModel.TestMessage;
            var isErrorMessageOnControl = errorMessageFromControl.Equals(msg, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(isErrorMessageOnControl);
            var isErrorMessage = errorMessageOnViewModel.Equals(msg, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(isErrorMessage);
        }

        [When(@"Validation message is Not thrown")]
        public void WhenValidationMessageIsNotThrown()
        {
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            var viewModel = _scenarioContext.Get<RedisSourceViewModel>("viewModel");
            var errorMessageFromControl = redisSourceControl.GetErrorMessage();
            var errorMessageOnViewModel = viewModel.TestMessage;
            var isErrorMessageOnViewModel = !errorMessageOnViewModel.Contains("Passed");
            var isErrorMessageOnControl = !errorMessageFromControl.Contains("Passed");
            Assert.IsFalse(isErrorMessageOnViewModel);
            Assert.IsFalse(isErrorMessageOnControl);
        }

        [Given(@"Password field is ""(.*)""")]
        [When(@"Password field is ""(.*)""")]
        [Then(@"Password field is ""(.*)""")]
        public void WhenPasswordFieldIs(string visibility)
        {
            var expectedVisibility = String.Equals(visibility, "Collapsed", StringComparison.InvariantCultureIgnoreCase) ? Visibility.Collapsed : Visibility.Visible;
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            var databaseDropDownVisibility = redisSourceControl.GetPasswordVisibility();
            Assert.AreEqual(expectedVisibility, databaseDropDownVisibility);
        }

        [When(@"Password field")]
        public void WhenPasswordField()
        {
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            var viewModel = _scenarioContext.Get<RedisSourceViewModel>("viewModel");
            Assert.AreEqual("pass123", viewModel.Password);
            Assert.AreEqual("pass123", redisSourceControl.GetPassword());
        }

        [Given(@"I type Password")]
        [When(@"I type Password")]
        [Then(@"I type Password")]
        public void WhenITypePassword()
        {
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            redisSourceControl.EnterPassword("pass123");
            var viewModel = _scenarioContext.Get<RedisSourceViewModel>("viewModel");
            Assert.AreEqual("pass123", viewModel.Password);
        }

        [When(@"the error message is ""(.*)""")]
        public void WhenTheErrorMessageIs(string errorMessage)
        {
            errorMessage = "Exception: " + _illegalCharactersInPath + Environment.NewLine + Environment.NewLine +
                           "Inner Exception: " + _illegalCharactersInPath;

            var viewModel = ScenarioContext.Current.Get<RedisSourceViewModel>("viewModel");
            Assert.AreEqual(errorMessage, viewModel.TestMessage);
        }

        [Given(@"""(.*)"" is ""(.*)""")]
        [When(@"""(.*)"" is ""(.*)""")]
        [Then(@"""(.*)"" is ""(.*)""")]
        public void GivenIs(string controlName, string enabledString)
        {
            Utils.CheckControlEnabled(controlName, enabledString, _scenarioContext.Get<ICheckControlEnabledView>(Utils.ViewNameKey), Utils.ViewNameKey);
        }

        [Then(@"Test Connecton is ""(.*)""")]
        [When(@"Test Connecton is ""(.*)""")]
        public void ThenTestConnectonIs(string successString)
        {
            var mockUpdateManager = _scenarioContext.Get<Mock<IRedisSourceModel>>("updateManager");
            var isSuccess = String.Equals(successString, "Successful", StringComparison.InvariantCultureIgnoreCase);
            var isLongRunning = String.Equals(successString, "Long Running", StringComparison.InvariantCultureIgnoreCase);
            if (isSuccess)
            {
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<IRedisServiceSource>()));
            }
            else if (isLongRunning)
            {
                var viewModel = _scenarioContext.Get<RedisSourceViewModel>("viewModel");
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<IRedisServiceSource>()));
                viewModel.AsyncWorker = new AsyncWorker();
            }
            else
            {
                mockUpdateManager.Setup(manager => manager.TestConnection(It.IsAny<IRedisServiceSource>()))
                    .Throws(new WarewolfTestException(_illegalCharactersInPath, new Exception(_illegalCharactersInPath)));
            }
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            redisSourceControl.PerformTestConnection();
        }

        [When(@"I save the source")]
        public void WhenISaveTheSource()
        {
            var mockRequestServiceNameViewModel = _scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Verifiable();
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            redisSourceControl.PerformSave();
        }

        [Then(@"the save dialog is opened")]
        public void ThenTheSaveDialogIsOpened()
        {
            var mockRequestServiceNameViewModel = _scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Verify();
        }

        [When(@"I save as ""(.*)""")]
        public void WhenISaveAs(string resourceName)
        {
            var mockRequestServiceNameViewModel = _scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Setup(model => model.ResourceName).Returns(new ResourceName("", resourceName));
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Returns(MessageBoxResult.OK);
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            redisSourceControl.PerformSave();
        }

        [Given(@"I Select Authentication Type as ""(.*)""")]
        [When(@"I Select Authentication Type as ""(.*)""")]
        [Then(@"I Select Authentication Type as ""(.*)""")]
        [Then(@"Select Authentication Type as ""(.*)""")]
        [When(@"I edit Authentication Type as ""(.*)""")]
        [Given(@"Select Authentication Type as ""(.*)""")]
        public void ThenSelectAuthenticationTypeAs(string authenticationTypeString)
        {
            var authenticationType = String.Equals(authenticationTypeString, "Password",
                StringComparison.OrdinalIgnoreCase)
                ? AuthenticationType.Password
                : AuthenticationType.Anonymous;

            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            redisSourceControl.SetAuthenticationType(authenticationType);
            var viewModel = _scenarioContext.Get<RedisSourceViewModel>("viewModel");
            Assert.AreEqual(authenticationType, viewModel.AuthenticationType);
        }

        [AfterScenario(@"RedisSource")]
        public void Cleanup()
        {
            var mockExecutor = new Mock<IExternalProcessExecutor>();
            var mockUpdateManager = _scenarioContext.Get<Mock<IRedisSourceModel>>("updateManager");
            var mockRequestServiceNameViewModel = _scenarioContext.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            var mockEventAggregator = new Mock<IEventAggregator>();
            var task = new Task<IRequestServiceNameViewModel>(() => mockRequestServiceNameViewModel.Object);
            task.Start();
            var viewModel = new RedisSourceViewModel(mockUpdateManager.Object, task, mockEventAggregator.Object, new SynchronousAsyncWorker(), mockExecutor.Object);
            var redisSourceControl = _scenarioContext.Get<RedisSourceControl>(Utils.ViewNameKey);
            redisSourceControl.DataContext = viewModel;
            FeatureContext.Current.Remove("viewModel");
            FeatureContext.Current.Add("viewModel", viewModel);
            FeatureContext.Current.Remove("externalProcessExecutor");
            FeatureContext.Current.Add("externalProcessExecutor", mockExecutor);
        }
    }
}
