using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq.Expressions;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Oracle;
using Dev2.Activities.Designers2.PostgreSql;
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
using TechTalk.SpecFlow;
using Warewolf.Core;

namespace Dev2.Activities.Specs.Toolbox.Resources
{
    [Binding]
    public class PostgresSqlConnectorSteps
    {
        private DbSourceDefinition postgresSqlSource;
        private DbAction selectedAction;
        //private DbSourceDefinition _testingDbSource;
        //private DbAction getmEmployeesAction;


        [Given(@"I drag a PostgresSql Server database connector")]
        public void GivenIDragAPostgresSqlServerDatabaseConnector()
        {
            var postgresActivity = new DsfPostgreSqlActivity();
            var modelItem = ModelItemUtils.CreateModelItem(postgresActivity);
          
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

            postgresSqlSource = new DbSourceDefinition
            {
                Name = "DemoPostgres",
                Type = enSourceType.PostgreSql,
                ServerName = "Localhost",
                UserName = "postgres",
                Password = "sa",
                AuthenticationType = Runtime.ServiceModel.Data.AuthenticationType.User
            };

            var dbSources = new ObservableCollection<IDbSource> { postgresSqlSource };
            
            mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(dbSources);
            mockDbServiceModel.Setup(model => model.GetActions(postgresSqlSource));
            mockServiceInputViewModel.SetupAllProperties();
          
            var postgresDesignerViewModel = new PostgreSqlDatabaseDesignerViewModel(modelItem, mockDbServiceModel.Object);
            
            ScenarioContext.Current.Add("viewModel", postgresDesignerViewModel);
            ScenarioContext.Current.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            ScenarioContext.Current.Add("mockDbServiceModel", mockDbServiceModel);
        }
        
        [When(@"I select ""(.*)"" as the source")]
        public void WhenISelectedAsTheSource(string sourceName)
        {
            if (sourceName == "DemoPostgres")
            {
                selectedAction = new DbAction();
                selectedAction.Name = "getemployees";
                selectedAction.Inputs = new List<IServiceInput> { new ServiceInput("EID", "") };
                GetDbServiceModel().Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction> { selectedAction });
                GetViewModel().SourceRegion.SelectedSource = postgresSqlSource;
            }
        }

        [When(@"I select ""(.*)"" as the action")]
        public void WhenISelectedAsTheAction(string actionName)
        {
            if (actionName == "getemployees")
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add(new DataColumn("Column1"));
                dataTable.ImportRow(dataTable.LoadDataRow(new object[] { 1 }, true));
                GetDbServiceModel().Setup(model => model.TestService(It.IsAny<IDatabaseService>())).Returns(dataTable);
                GetViewModel().ActionRegion.SelectedAction = selectedAction;
            }
        }

        [Then(@"Inputs is Enabled")]
        public void ThenInputsIsEnabled()
        {
            var viewModel = GetViewModel();
            var hasInputs = viewModel.InputArea.Inputs != null || viewModel.InputArea.IsVisible;
            Assert.IsTrue(hasInputs);
        }


        private static PostgreSqlDatabaseDesignerViewModel GetViewModel()
        {
            return ScenarioContext.Current.Get<PostgreSqlDatabaseDesignerViewModel>("viewModel");
        }

        private static Mock<IDbServiceModel> GetDbServiceModel()
        {
            return ScenarioContext.Current.Get<Mock<IDbServiceModel>>("mockDbServiceModel");
        }
    }
}
