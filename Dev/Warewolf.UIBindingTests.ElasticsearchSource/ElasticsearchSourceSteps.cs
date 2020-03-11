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
            mockStudioUpdateManager.Setup(model => model.ServerName).Returns("localhost");
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
            ScenarioContext.Current.Pending();
        }

        [Then(@"""(.*)"" tab is opened")]
        public void ThenTabIsOpened(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"title is ""(.*)""")]
        public void ThenTitleIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"I type HostName as a valid anonymous Elasticsearch server")]
        public void GivenITypeHostNameAsAValidAnonymousElasticsearchServer()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"server port is ""(.*)""")]
        public void ThenServerPortIs(int p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I type port number as ""(.*)""")]
        public void ThenITypePortNumberAs(int p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I Select Authentication Type as ""(.*)""")]
        public void ThenISelectAuthenticationTypeAs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Password field is ""(.*)""")]
        public void ThenPasswordFieldIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"Test Connecton is ""(.*)""")]
        public void WhenTestConnectonIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"""(.*)"" is ""(.*)""")]
        public void WhenIs(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I save as ""(.*)""")]
        public void WhenISaveAs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"the save dialog is opened")]
        public void ThenTheSaveDialogIsOpened()
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"I type HostName as a valid Elasticsearch server")]
        public void GivenITypeHostNameAsAValidElasticsearchServer()
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"I type port number as ""(.*)""")]
        public void GivenITypePortNumberAs(int p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"""(.*)"" is ""(.*)""")]
        public void GivenIs(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"I Select Authentication Type as ""(.*)""")]
        public void GivenISelectAuthenticationTypeAs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"Password field is ""(.*)""")]
        public void GivenPasswordFieldIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"I type Password")]
        public void GivenITypePassword()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I save the source")]
        public void WhenISaveTheSource()
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"I type HostName as ""(.*)""")]
        public void GivenITypeHostNameAs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"Validation message is thrown")]
        public void WhenValidationMessageIsThrown()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"Validation message is Not thrown")]
        public void WhenValidationMessageIsNotThrown()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I Select Authentication Type as ""(.*)""")]
        public void WhenISelectAuthenticationTypeAs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"Password field is ""(.*)""")]
        public void WhenPasswordFieldIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"I open ""(.*)"" Elasticsearch source")]
        public void GivenIOpenElasticsearchSource(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"HostName is ""(.*)""")]
        public void ThenHostNameIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Select Authentication Type as ""(.*)""")]
        public void ThenSelectAuthenticationTypeAs(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I change HostName to ""(.*)""")]
        public void WhenIChangeHostNameTo(string p0)
        {
            ScenarioContext.Current.Pending();
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
