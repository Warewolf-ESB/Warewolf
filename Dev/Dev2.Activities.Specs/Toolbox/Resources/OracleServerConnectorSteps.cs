using Caliburn.Micro;
using Dev2.Activities.Designers2.Oracle;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Resources
{
    [Binding]
    public class OracleServerConnectorSteps
    {
        private DbSourceDefinition _greenPointSource;
        [Given(@"I drag a Oracle Server database connector")]
        public void GivenIDragAOracleServerDatabaseConnector()
        {
            var oracleServerActivity = new DsfOracleDatabaseActivity();
            var modelItem = ModelItemUtils.CreateModelItem(oracleServerActivity);
            var mockServiceInputViewModel = new Mock<IManageServiceInputViewModel>();
            var mockDbServiceModel = new Mock<IDbServiceModel>();
            var mockEnvironmentRepo = new Mock<IEnvironmentRepository>();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.ID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockEnvironmentRepo.Setup(repository => repository.ActiveEnvironment).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(mockEnvironmentModel.Object);

            _greenPointSource = new DbSourceDefinition
            {
                Name = "GreenPoint",
                Type = enSourceType.Oracle
            };

            var dbSources = new ObservableCollection<IDbSource> { _greenPointSource };
            mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(dbSources);
            mockServiceInputViewModel.SetupAllProperties();
            var oracleServerDesignerViewModel = new OracleDatabaseDesignerViewModel(modelItem, new Mock<IContextualResourceModel>().Object,
                                                                                        mockEnvironmentRepo.Object, new Mock<IEventAggregator>().Object, new SynchronousAsyncWorker(), mockServiceInputViewModel.Object, mockDbServiceModel.Object);

            ScenarioContext.Current.Add("viewModel", oracleServerDesignerViewModel);
            ScenarioContext.Current.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            ScenarioContext.Current.Add("mockDbServiceModel", mockDbServiceModel);
        }
        
        [Given(@"Source is ""(.*)""")]
        public void GivenSourceIs()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.SourceVisible);
        }

        private static OracleDatabaseDesignerViewModel GetViewModel()
        {
            return ScenarioContext.Current.Get<OracleDatabaseDesignerViewModel>("viewModel");
        }

        private static Mock<IManageServiceInputViewModel> GetInputViewModel()
        {
            return ScenarioContext.Current.Get<Mock<IManageServiceInputViewModel>>("mockServiceInputViewModel");
        }

        private static Mock<IDbServiceModel> GetDbServiceModel()
        {
            return ScenarioContext.Current.Get<Mock<IDbServiceModel>>("mockDbServiceModel");
        }

        [Given(@"Action is dbo\.Pr_CitiesGetCountries")]
        public void GivenActionIsDbo_Pr_CitiesGetCountries()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Given(@"Inputs is Enabled")]
        public void GivenInputsIsEnabled()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Given(@"Inputs appear as")]
        public void GivenInputsAppearAs(Table table)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Given(@"Validate is Enabled")]
        public void GivenValidateIsEnabled()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Given(@"Mapping is Enabled")]
        public void GivenMappingIsEnabled()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Given(@"I open Wolf(.*)")]
        public void GivenIOpenWolf(int p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I click ""(.*)""")]
        public void WhenIClick(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"inputs appear as")]
        public void ThenInputsAppearAs(Table table)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
