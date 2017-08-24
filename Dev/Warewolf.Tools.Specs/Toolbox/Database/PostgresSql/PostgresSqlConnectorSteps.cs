using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.PostgreSql;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Core;
using Warewolf.Tools.Specs.Toolbox.Database;

namespace Dev2.Activities.Specs.Toolbox.Resources
{
    [Binding]
    public class PostgresSqlConnectorSteps
    {
        private DbSourceDefinition _postgresSqlSource;
        private DbAction _selectedAction;
        private DbSourceDefinition _testingDbSource;
        private DbAction _getEmployees;

        [Given(@"I drag a PostgresSql Server database connector")]
        public void GivenIDragAPostgresSqlServerDatabaseConnector()
        {
            var postgresActivity = new DsfPostgreSqlActivity();
            var modelItem = ModelItemUtils.CreateModelItem(postgresActivity);

            var mockServiceInputViewModel = new Mock<IManageServiceInputViewModel>();
            var mockDbServiceModel = new Mock<IDbServiceModel>();
            var mockEnvironmentRepo = new Mock<IServerRepository>();
            var mockEnvironmentModel = new Mock<IServer>();

            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.EnvironmentID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockEnvironmentRepo.Setup(repository => repository.ActiveServer).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IServer, bool>>>())).Returns(mockEnvironmentModel.Object);

            _postgresSqlSource = new DbSourceDefinition
            {
                Name = "DemoPostgres",
                Type = enSourceType.PostgreSQL,
                ServerName = "Localhost",
                UserName = "postgres",
                Password = "sa",
                AuthenticationType = Runtime.ServiceModel.Data.AuthenticationType.User
            };

            var dbSources = new ObservableCollection<IDbSource> { _postgresSqlSource };

            mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(dbSources);
            mockDbServiceModel.Setup(model => model.GetActions(_postgresSqlSource));
            mockServiceInputViewModel.SetupAllProperties();

            var postgresDesignerViewModel = new PostgreSqlDatabaseDesignerViewModel(modelItem, mockDbServiceModel.Object, new SynchronousAsyncWorker(), new ViewPropertyBuilder());

            ScenarioContext.Current.Add("viewModel", postgresDesignerViewModel);
            ScenarioContext.Current.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            ScenarioContext.Current.Add("mockDbServiceModel", mockDbServiceModel);
        }

        [When(@"I select DemoPostgres as the source")]
        public void WhenISelectedAsTheSource()
        {
            _selectedAction = new DbAction();
            _selectedAction.Name = "getemployees";
            _selectedAction.Inputs = new List<IServiceInput> { new ServiceInput("fname", "") };
            GetDbServiceModel().Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction> { _selectedAction });
            GetViewModel().SourceRegion.SelectedSource = _postgresSqlSource;
        }

        [When(@"I select getemployees as the action")]
        public void WhenISelectedAsTheAction()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("name"));
            dataTable.Columns.Add(new DataColumn("salary"));
            dataTable.Columns.Add(new DataColumn("age"));
            dataTable.ImportRow(dataTable.LoadDataRow(new object[] { "Bill", 4200, 45 }, true));
            GetDbServiceModel().Setup(model => model.TestService(It.IsAny<IDatabaseService>())).Returns(dataTable);
            GetViewModel().ActionRegion.SelectedAction = _selectedAction;
        }

        [Then(@"Inputs Are Enabled for PostgresSql")]
        public void ThenInputsIsEnabledForPostgresSql()
        {
            var viewModel = GetViewModel();
            var hasInputs = viewModel.InputArea.Inputs != null;
            Assert.IsTrue(hasInputs);
        }

        [Given(@"I Enter a value as the input")]
        public void GivenIEnterAsTheInput(Table table)
        {

            int rowNum = 0;
            var viewModel = GetViewModel();
            viewModel.TestProcedure();

            foreach (var row in table.Rows)
            {
                var inputValue = row["fname"];
                var serviceInputs = viewModel.ManageServiceInputViewModel.InputArea.Inputs.ToList();
                var serviceInput = serviceInputs[rowNum];
                serviceInput.Value = inputValue;
                rowNum++;
            }
        }

        [Then(@"Test button is Enabled")]
        public void ThenValidateIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.TestInputCommand.CanExecute());
        }

        [Then(@"button is clicked")]
        public void ThenButtonIsClicked(Table table)
        {
            var viewModel = GetViewModel();
            viewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
        }

        [Then(@"Test Connector and Calculate Outputs outputs appear as")]
        public void ThenOutPutsAppearAs(Table table)
        {
            var rowIdx = 0;
            foreach (var tableRow in table.Rows)
            {
                var viewModel = GetViewModel();
                var nameValue = tableRow["name"];
                var salaryValue = tableRow["salary"];
                var ageValue = tableRow["age"];
                var rows = viewModel.ManageServiceInputViewModel.TestResults.Rows;
                var dataRow = rows[rowIdx];
                var dataNameValue = dataRow[0].ToString();
                var dataSalaryValue = dataRow[1].ToString();
                var dataAgeValue = dataRow[2].ToString();
                Assert.AreEqual(nameValue, dataNameValue);
                Assert.AreEqual(salaryValue, dataSalaryValue);
                Assert.AreEqual(ageValue, dataAgeValue);
                rowIdx++;
            }
        }

        [Given(@"I Open workflow with PostgreSql connector")]
        public void GivenIOpenWorkflowWithPostgreSqlConnector()
        {
            var sourceId = Guid.NewGuid();
            var inputs = new List<IServiceInput> { new ServiceInput("Prefix", "[[Prefix]]") };
            var outputs = new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping("name", "name", "getemployees"),
            };

            var postgresActivity = new DsfPostgreSqlActivity();

            var modelItem = ModelItemUtils.CreateModelItem(postgresActivity);
            var mockServiceInputViewModel = new Mock<IManageServiceInputViewModel>();
            var mockDbServiceModel = new Mock<IDbServiceModel>();
            var mockEnvironmentRepo = new Mock<IServerRepository>();
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.EnvironmentID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockEnvironmentRepo.Setup(repository => repository.ActiveServer).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IServer, bool>>>())).Returns(mockEnvironmentModel.Object);

            _postgresSqlSource = new DbSourceDefinition
            {
                Name = "postgressql",
                Type = enSourceType.PostgreSQL,
                ServerName = "Localhost",
                UserName = "postgres",
                Password = "sa",
                AuthenticationType = Runtime.ServiceModel.Data.AuthenticationType.User
            };

            _testingDbSource = new DbSourceDefinition
            {
                Name = "testingDBSrc",
                Type = enSourceType.PostgreSQL,
                Id = sourceId
            };

            _selectedAction = new DbAction
            {
                Name = "getemployees",
                Inputs = new List<IServiceInput> { new ServiceInput("fname", "") }
            };

            _getEmployees = new DbAction { Name = "getemployees" };
            _getEmployees.Inputs = inputs;
            var dbSources = new ObservableCollection<IDbSource> { _testingDbSource, _postgresSqlSource };
            mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(dbSources);
            mockDbServiceModel.Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction> { _getEmployees, _selectedAction });
            mockServiceInputViewModel.SetupAllProperties();
            var postgresDesignerViewModel = new PostgreSqlDatabaseDesignerViewModel(modelItem, mockDbServiceModel.Object, new SynchronousAsyncWorker(), new ViewPropertyBuilder());

            ScenarioContext.Current.Add("viewModel", postgresDesignerViewModel);
            ScenarioContext.Current.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            ScenarioContext.Current.Add("mockDbServiceModel", mockDbServiceModel);
        }

        [Given(@"PostgresSql Source Is Enabled")]
        public void GivenSourceIsEnable()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.IsValid);
        }

        [Given(@"PostgresSql Source Is ""(.*)""")]
        public void GivenSourceIs(string sourceName)
        {
            var selectedSource = GetViewModel().SourceRegion.SelectedSource = _postgresSqlSource;
            Assert.IsNotNull(selectedSource);
            Assert.AreEqual(sourceName, selectedSource.Name);
        }

        [Given(@"PostgresSql Action Is Enabled")]
        public void GivenActionIsEnable()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.ActionRegion.IsActionEnabled);
        }

        [Given(@"PostgresSql Action Is ""(.*)""")]
        public void GivenActionIs(string actionName)
        {
            var vm = GetViewModel();
            var selectedProcedure = vm.ActionRegion.SelectedAction = _selectedAction;
            Assert.IsNotNull(selectedProcedure);
            Assert.AreEqual(actionName, selectedProcedure.Name);
        }

        [Given(@"PostgresSql Inputs Are Enabled")]
        public void GivenInputsIsEnable()
        {
            var viewModel = GetViewModel();
            var hasInputs = viewModel.InputArea.Inputs != null;
            Assert.IsTrue(hasInputs);
        }

        [Given(@"PostgresSql Inputs appear As")]
        [Then(@"PostgresSql Inputs appear As")]
        public void ThenInputsAppearAs(Table table)
        {
            var vm = GetViewModel();
            DatabaseToolsSteps.AssertAgainstServiceInputs(table, vm.InputArea.Inputs);
        }

        [Given(@"Test PostgresSql Inputs appear As")]
        [Then(@"Test PostgresSql Inputs appear As")]
        public void TestInputsAppear(Table table)
        {
            var vm = GetViewModel();
            DatabaseToolsSteps.AssertAgainstServiceInputs(table, vm.ToModel().Inputs);
        }

        [Then(@"Validate PostgresSql Is Enabled")]
        public void ThenValidateIsEnable()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.ManageServiceInputViewModel.TestCommand.CanExecute(null));
            viewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
        }

        PostgreSqlDatabaseDesignerViewModel GetViewModel()
        {
            return ScenarioContext.Current.Get<PostgreSqlDatabaseDesignerViewModel>("viewModel");
        }

        Mock<IDbServiceModel> GetDbServiceModel()
        {
            return ScenarioContext.Current.Get<Mock<IDbServiceModel>>("mockDbServiceModel");
        }
    }
}
