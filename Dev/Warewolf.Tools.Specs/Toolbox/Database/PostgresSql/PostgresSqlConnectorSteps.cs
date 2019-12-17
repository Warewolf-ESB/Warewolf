using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.PostgreSql;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Core;
using Warewolf.Studio.ViewModels;
using Warewolf.Tools.Specs.Toolbox.Database;

namespace Dev2.Activities.Specs.Toolbox.Resources
{
    [Binding]
    public class PostgresSqlConnectorSteps : DatabaseToolsSteps
    {
        DbSourceDefinition _postgresSqlSource;
        DbAction _selectedAction;
        DbSourceDefinition _testingDbSource;
        DbAction _getEmployees;
        private Depends _containerOps;
        readonly ScenarioContext _scenarioContext;
        readonly CommonSteps _commonSteps;

        public PostgresSqlConnectorSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _commonSteps = new CommonSteps(scenarioContext);
        }

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

            _scenarioContext.Add("viewModel", postgresDesignerViewModel);
            _scenarioContext.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            _scenarioContext.Add("mockDbServiceModel", mockDbServiceModel);
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

            var rowNum = 0;
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

            _scenarioContext.Add("viewModel", postgresDesignerViewModel);
            _scenarioContext.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            _scenarioContext.Add("mockDbServiceModel", mockDbServiceModel);
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
            return _scenarioContext.Get<PostgreSqlDatabaseDesignerViewModel>("viewModel");
        }

        Mock<IDbServiceModel> GetDbServiceModel()
        {
            return _scenarioContext.Get<Mock<IDbServiceModel>>("mockDbServiceModel");
        }

        [Given(@"I have workflow ""(.*)"" with ""(.*)"" Postgres database connector")]
        public void GivenIHaveWorkflowWithPostgresDatabaseConnector(string workflowName, string activityName)
        {
            var environmentModel = _scenarioContext.Get<IServer>("server");
            environmentModel.Connect();
            CreateNewResourceModel(workflowName, environmentModel);
            CreateDBServiceModel(environmentModel);

            var dbServiceModel = _scenarioContext.Get<ManageDbServiceModel>("dbServiceModel");
            var posgreActivity = new DsfPostgreSqlActivity { DisplayName = activityName };
            var modelItem = ModelItemUtils.CreateModelItem(posgreActivity);
            var posgreDesignerViewModel = new PostgreSqlDatabaseDesignerViewModel(modelItem, dbServiceModel, new SynchronousAsyncWorker(), new ViewPropertyBuilder());
            var serviceInputViewModel = new ManageDatabaseServiceInputViewModel(posgreDesignerViewModel, posgreDesignerViewModel.Model);

            _commonSteps.AddActivityToActivityList(workflowName, activityName, posgreActivity);
            DebugWriterSubscribe(environmentModel);
            _scenarioContext.Add("viewModel", posgreDesignerViewModel);
            _scenarioContext.Add("parentName", workflowName);
        }

        [Given(@"Postgres Server Source is Enabled")]
        public void GivenPostgresServerSourceIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.SourceRegion.IsEnabled);
        }

        [Given(@"I Select ""(.*)"" as Postgres Source for ""(.*)""")]
        public void GivenISelectAsPostgresSourceFor(string sourceName, string activityName)
        {
            var proxyLayer = _scenarioContext.Get<StudioServerProxy>("proxyLayer");
            var vm = GetViewModel();
            Assert.IsNotNull(vm.SourceRegion);
            var dbSources = proxyLayer.QueryManagerProxy.FetchDbSources().ToList();
            Assert.IsNotNull(dbSources, "dbSources is null");
            var dbSource = dbSources.Single(source => source.Name == sourceName);
            Assert.IsNotNull(dbSource, "Source is null");
            vm.SourceRegion.SelectedSource = dbSource;
            SetDbSource(activityName, dbSource);
            Assert.IsNotNull(vm.SourceRegion.SelectedSource);
        }

        [Given(@"I Select ""(.*)"" as Postgres Server Action for ""(.*)""")]
        public void GivenISelectAsPostgresServerActionFor(string actionName, string activityName)
        {
            var vm = GetViewModel();
            Assert.IsNotNull(vm.ActionRegion);
            vm.ActionRegion.SelectedAction = vm.ActionRegion.Actions.FirstOrDefault(p => p.Name == actionName);
            SetDbAction(activityName, actionName);
            Assert.IsNotNull(vm.ActionRegion.SelectedAction);
        }

        [Given(@"this test depends on a remote Postgres database container")]
        public void GivenThisTestDependsOnARemotePostgresDatabaseContainer() => _containerOps = new Depends(Depends.ContainerType.PostGreSQL);

        [Given(@"Postgres Command Timeout is ""(.*)"" milliseconds for ""(.*)""")]
        [When(@"Postgres Command Timeout is ""(.*)"" milliseconds for ""(.*)""")]
        [Then(@"Postgres Command Timeout is ""(.*)"" milliseconds for ""(.*)""")]
        public void GivenPostgresCommandTimeoutIsMillisecondsFor(int timeout, string ActivityName)
        {         
            var vm = GetViewModel();
            Assert.IsNotNull(vm);
            vm.InputArea.CommandTimeout = timeout;
            SetCommandTimeout(ActivityName, timeout);
            Assert.AreEqual(timeout, vm.InputArea.CommandTimeout);
        }

        [Given(@"Postgres Server Inputs Are Enabled")]
        public void GivenPostgresServerInputsAreEnabled()
        {
            var viewModel = GetViewModel();
            var hasInputs = viewModel.InputArea.Inputs != null || viewModel.InputArea.IsEnabled;
            Assert.IsTrue(hasInputs);
        }

        [Given(@"Validate Postgres Server is Enabled")]
        public void GivenValidatePostgresServerIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.TestInputCommand.CanExecute());
        }

        [Given(@"I click Postgres Generate Outputs")]
        public void GivenIClickPostgresGenerateOutputs()
        {
            var viewModel = GetViewModel();
            viewModel.TestInputCommand.Execute();
        }

        [Given(@"I click Postgres Test")]
        public void GivenIClickPostgresTest()
        {
            var viewModel = GetViewModel();
            viewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            viewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
        }

        [Then(@"Postgres Server Outputs appear as")]
        public void ThenPostgresServerOutputsAppearAs(Table table)
        {
            var outputMappings = GetViewModel().OutputsRegion.Outputs;
            Assert.IsNotNull(outputMappings);
            var rowIdx = 0;
            foreach (var tableRow in table.Rows)
            {
                var mappedFrom = tableRow["Mapped From"];
                var mappedTo = tableRow["Mapped To"];
                var outputMapping = outputMappings.ToList()[rowIdx];
                Assert.AreEqual<string>(mappedFrom, outputMapping.MappedFrom);
                Assert.AreEqual<string>(mappedTo, outputMapping.MappedTo);
                rowIdx++;
            }
        }

        [Then(@"Postgres Server Recordset Name equals ""(.*)""")]
        public void ThenPostgresServerRecordsetNameEquals(string recsetName)
        {
            Assert.IsTrue(string.Equals(recsetName, GetViewModel().OutputsRegion.RecordsetName));
        }

        [When(@"Postgres Workflow ""(.*)"" containing dbTool is executed")]
        public void WhenPostgresWorkflowContainingDbToolIsExecuted(string workflowName)
        {
            WorkflowIsExecuted(workflowName);
        }

        [AfterScenario("@ExecutePostgresServerWithTimeout")]
        public void CleanUp()
        {
            CleanupForTimeOutSpecs();
            _containerOps?.Dispose();
        }
    }
}
