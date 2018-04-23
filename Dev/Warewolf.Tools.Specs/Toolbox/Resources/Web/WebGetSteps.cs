using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dev2.Activities.Designers2.Web_Service_Delete;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebService;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Activities;
using TechTalk.SpecFlow;
using Dev2.Activities.Designers2.Web_Service_Get;

namespace Warewolf.Tools.Specs.Toolbox.Resources.Web
{
    [Binding]
    public class WebGetSteps
    {
        readonly ScenarioContext scenarioContext;

        public WebGetSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }
            this.scenarioContext = scenarioContext;
        }        
        WebServiceSourceDefinition _weblocalhostsource;        


        [Given(@"I drag Web Get Request Connector Tool onto the design surface")]
        public void GivenIDragWebGetRequestConnectorToolOntoTheDesignSurface()
        {
            var activity = new DsfWebGetActivity();
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            var mockServiceInputViewModel = new Mock<IManageWebServiceInputViewModel>();
            var mockServiceModel = new Mock<IWebServiceModel>();
            var mockEnvironmentRepo = new Mock<IServerRepository>();
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.EnvironmentID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockEnvironmentRepo.Setup(repository => repository.ActiveServer).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IServer, bool>>>())).Returns(mockEnvironmentModel.Object);

            _weblocalhostsource = new WebServiceSourceDefinition
            {
                Name = "LocalhostSource"
            };
            var sources = new ObservableCollection<IWebServiceSource> { _weblocalhostsource };
            mockServiceModel.Setup(model => model.RetrieveSources()).Returns(sources);
            mockServiceModel.Setup(model => model.EditSource(It.IsAny<IWebServiceSource>())).Verifiable();
            mockServiceInputViewModel.SetupAllProperties();
            var viewModel = new WebServiceGetViewModel(modelItem, mockServiceModel.Object);

            scenarioContext.Add("viewModel", viewModel);
            scenarioContext.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            scenarioContext.Add("mockServiceModel", mockServiceModel);
        }

        [Given(@"New Source is Enabled")]
        public void GivenNewSourceIsEnabled()
        {
            var canExecuteNewCommand = GetViewModel().SourceRegion.NewSourceCommand.CanExecute(null);
            Assert.IsTrue(canExecuteNewCommand);
        }

        [Given(@"Edit Source is Disabled")]
        public void GivenEditSourceIsDisabled()
        {
            var canExecuteNewCommand = GetViewModel().SourceRegion.EditSourceCommand.CanExecute(null);
            Assert.IsFalse(canExecuteNewCommand);
        }

        [When(@"I Select ""(.*)"" as a Get web Source")]
        public void WhenISelectAsAGetWebSource(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        WebServiceGetViewModel GetViewModel()
        {
            return scenarioContext.Get<WebServiceGetViewModel>("viewModel");
        }
    }
}
