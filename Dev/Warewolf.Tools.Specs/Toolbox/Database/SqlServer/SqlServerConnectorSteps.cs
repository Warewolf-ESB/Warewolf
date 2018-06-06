using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ActivityUnitTests;
using Dev2.Activities.Designers.Tests.SqlServer;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.SqlServerDatabase;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Controller;
using Dev2.Interfaces;
using Dev2.Runtime.Hosting;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using Dev2.Studio.ViewModels;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Core;
using Warewolf.Studio.ViewModels;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Resources
{
    [Binding]
    public class SQLServerConnectorSteps :   RecordSetBases
    
    {

        DbSourceDefinition sqlsource;
        DbAction _importOrderAction;
        DbSourceDefinition _testingDbSource;
        DbAction _getCountriesAction;
        readonly ScenarioContext _scenarioContext;
        readonly CommonSteps _commonSteps;

        public SQLServerConnectorSteps(ScenarioContext scenarioContext)
           : base(scenarioContext)
        {
            _scenarioContext = scenarioContext ?? throw new ArgumentNullException(nameof(scenarioContext));
            _commonSteps = new CommonSteps(_scenarioContext);
           
        }
        [Given(@"I drag a Sql Server database connector")]
        public void GivenIDragASqlServerDatabaseConnector()
        {
            var sqlServerActivity = new DsfSqlServerDatabaseActivity();
            var modelItem = ModelItemUtils.CreateModelItem(sqlServerActivity);
            var mockServiceInputViewModel = new Mock<IManageDatabaseInputViewModel>();
            var mockDbServiceModel = new Mock<IDbServiceModel>();
            var mockEnvironmentRepo = new Mock<IServerRepository>();
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.EnvironmentID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);

            mockEnvironmentRepo.Setup(repository => repository.ActiveServer).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IServer, bool>>>())).Returns(mockEnvironmentModel.Object);

            sqlsource = new DbSourceDefinition
            {
                Name = "GreenPoint",
                Type = enSourceType.SqlDatabase
            };

            var dbSources = new ObservableCollection<IDbSource> { sqlsource };
            mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(dbSources);
            mockServiceInputViewModel.SetupAllProperties();
            var sqlServerDesignerViewModel = new SqlServerDatabaseDesignerViewModel(modelItem, mockDbServiceModel.Object, new SynchronousAsyncWorker(), new ViewPropertyBuilder());

            ScenarioContext.Current.Add("viewModel", sqlServerDesignerViewModel);
            ScenarioContext.Current.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            ScenarioContext.Current.Add("mockDbServiceModel", mockDbServiceModel);
        }

        [When(@"Source is changed from to ""(.*)""")]
        public void WhenSourceIsChangedFromTo(string sourceName)
        {
            GetViewModel().SourceRegion.SelectedSource = GetViewModel().SourceRegion.Sources.FirstOrDefault(p => p.Name == sourceName);
        }

        [When(@"Action is changed from to ""(.*)""")]
        public void WhenActionIsChangedFromTo(string procName)
        {
            if (procName == "dbo.ImportOrder")
            {
                GetViewModel().ActionRegion.SelectedAction = _importOrderAction;
            }
        }

        [When(@"Recordset Name is changed to ""(.*)""")]
        public void WhenRecordsetNameIsChangedTo(string recSetName)
        {
            GetViewModel().OutputsRegion.RecordsetName = recSetName;
        }

        [Given(@"Sql Server Source is Enabled")]
        public void GivenSourceIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.SourceRegion.IsEnabled);
        }
        [Given(@"Action is Disabled")]
        public void GivenActionIsDisabled()
        {
            var viewModel = GetViewModel();
            Assert.IsFalse(viewModel.ActionRegion.IsEnabled);
        }

        [Given(@"Inputs is Disabled")]
        [When(@"Inputs is Disabled")]
        [Then(@"Inputs is Disabled")]
        public void GivenInputsIsDisabled()
        {
            var viewModel = GetViewModel();
            var hasNoInputs = viewModel.InputArea.Inputs == null || viewModel.InputArea.Inputs.Count == 0 || !viewModel.InputArea.IsEnabled;
            Assert.IsTrue(hasNoInputs);
        }

        [Given(@"Outputs is Disabled")]
        [When(@"Outputs is Disabled")]
        [Then(@"Outputs is Disabled")]
        public void GivenOutputsIsDisabled()
        {
            var viewModel = GetViewModel();
            var hasNoOutputs = viewModel.OutputsRegion.Outputs == null || viewModel.OutputsRegion.Outputs.Count == 0 || !viewModel.OutputsRegion.IsEnabled;
            Assert.IsTrue(hasNoOutputs);
        }

        [Given(@"Validate is Disabled")]
        [Then(@"Validate is Disabled")]
        public void GivenValidateIsDisabled()
        {
            var viewModel = GetViewModel();
            Assert.IsFalse(viewModel.TestInputCommand.CanExecute(null));
        }

        [Given(@"Sql Server Action is Enabled")]
        [Then(@"Sql Server Action is Enabled")]
        [When(@"Sql Server Action is Enabled")]
        public void ThenActionIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.ActionRegion.IsEnabled);
        }


        [Given(@"Sql Server Inputs Are Enabled")]
        [When(@"Sql Server Inputs Are Enabled")]
        [Then(@"Sql Server Inputs Are Enabled")]
        public void ThenInputsIsEnabled()
        {
            var viewModel = GetViewModel();
            var hasInputs = viewModel.InputArea.Inputs != null || viewModel.InputArea.IsEnabled;
            Assert.IsTrue(hasInputs);
        }

        [Given(@"Validate Sql Server is Enabled")]
        [When(@"Validate Sql Server is Enabled")]
        [Then(@"Validate Sql Server is Enabled")]
        public void ThenValidateIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.TestInputCommand.CanExecute(null));
        }

        [Given(@"Sql Server Inputs appear as")]
        [When(@"Sql Server Inputs appear as")]
        [Then(@"Sql Server Inputs appear as")]
        public void ThenInputsAppearAs(Table table)
        {
            var viewModel = GetViewModel();
            var rowNum = 0;
            foreach (var row in table.Rows)
            {
                var inputValue = row["Input"];
                var value = row["Value"];
                var serviceInputs = viewModel.InputArea.Inputs.ToList();
                var serviceInput = serviceInputs[rowNum];
                Assert.AreEqual<string>(inputValue, serviceInput.Name);
                Assert.AreEqual<string>(value, serviceInput.Value);
                rowNum++;
            }
        }
        static List<IServiceInput> GetServiceInputs(Table table)
        {
            return table.Rows.Select(a => new ServiceInput(a["ParameterName"], a["ParameterValue"]))
                .Cast<IServiceInput>()
                .ToList();
        }
        [Given(@"I have workflow with database connector")]
        public void GivenIHaveWorkflowWithDatabaseConnector()
        {
            var procedureName = "dbo.Pr_CitiesGetCountries";
            var sourceId = Guid.NewGuid();
            var inputs = new List<IServiceInput> { new ServiceInput("Prefix", "[[Prefix]]") };
            var resourceId = "b9184f70-64ea-4dc5-b23b-02fcd5f91082".ToGuid();
            //Load Source based on the name
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            var environmentConnection = environmentModel.Connection;
            var controllerFactory = new CommunicationControllerFactory();
            var _proxyLayer = new StudioServerProxy(controllerFactory, environmentConnection);
            var mock = new Mock<IShellViewModel>();
            var dbServiceModel = new ManageDbServiceModel(new StudioResourceUpdateManager(controllerFactory, environmentConnection)
                                                                                    , _proxyLayer.QueryManagerProxy
                                                                                    , mock.Object
                                                                                    , environmentModel);
            var dbSources = _proxyLayer.QueryManagerProxy.FetchDbSources().ToList();
            var dbSource = dbSources.Single(source => source.Id == resourceId);

            var databaseService = new DatabaseService
            {
                Source = dbSource,
                Inputs = inputs,
                Action = new DbAction()
                {
                    Name = procedureName,
                    SourceId = dbSource.Id,
                    Inputs = inputs,
                    ExecuteAction = procedureName
                },
                Name = procedureName,
                Id = dbSource.Id
            };
            var testResults = dbServiceModel.TestService(databaseService);
            var sqlServerActivity = new DsfSqlServerDatabaseActivity
            {
                ProcedureName = procedureName,
                DisplayName = procedureName,
                SourceId = dbSource.Id,
                Outputs = new List<IServiceOutputMapping>(),
                Inputs = databaseService.Inputs
            };

            var mappings = new List<IServiceOutputMapping>();
            var modelItem = ModelItemUtils.CreateModelItem(sqlServerActivity);

            var sqlServerDesignerViewModel = new SqlServerDatabaseDesignerViewModel(modelItem, dbServiceModel,new SynchronousAsyncWorker(), new ViewPropertyBuilder());
            var serviceInputViewModel = new ManageDatabaseServiceInputViewModel(sqlServerDesignerViewModel, sqlServerDesignerViewModel.Model);

            sqlServerActivity.Outputs = mappings;
            sqlServerActivity.ProcedureName = procedureName;
            _commonSteps.AddActivityToActivityList(procedureName, procedureName, sqlServerActivity);

            ScenarioContext.Current.Add("viewModel", sqlServerDesignerViewModel);
            ScenarioContext.Current.Add("ServiceInputViewModel", serviceInputViewModel);
            ScenarioContext.Current.Add("DbServiceModel", databaseService);
        }
        static Mock<IServer> SetupEnvironment()
        {
            var newSelectedConnection = new Mock<IServer>();
            var newSelectedConnectionEnvironmentId = Guid.NewGuid();
            newSelectedConnection.SetupGet(it => it.DisplayName).Returns("Nonlocalhost");
            newSelectedConnection.SetupGet(it => it.EnvironmentID).Returns(newSelectedConnectionEnvironmentId);
            newSelectedConnection.SetupGet(it => it.HasLoaded).Returns(true);
            newSelectedConnection.SetupGet(it => it.IsConnected).Returns(true);
            newSelectedConnection.SetupGet(it => it.UpdateRepository).Returns(new Mock<IStudioUpdateManager>().Object);
            newSelectedConnection.SetupGet(it => it.QueryProxy).Returns(new Mock<IQueryManager>().Object);

            var env = new Mock<IServer>();
            env.Setup(a => a.IsLocalHost).Returns(true);
            env.Setup(a => a.IsConnected).Returns(true);
            env.Setup(a => a.CanStudioExecute).Returns(true);
            env.Setup(a => a.Name).Returns("TestEnvironment");
            env.Setup(a => a.DisplayName).Returns("TestEnvironment");
            return env;
        }
        [Given(@"I open workflow with database connector")]
        public void GivenIOpenWolf()
        {
            var sourceId = Guid.NewGuid();
            var inputs = new List<IServiceInput> { new ServiceInput("Prefix", "[[Prefix]]") };
            var outputs = new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping("CountryID", "CountryID", "dbo_Pr_CitiesGetCountries"),
                new ServiceOutputMapping("Description", "Description", "dbo_Pr_CitiesGetCountries")
            };
            var sqlServerActivity = new DsfSqlServerDatabaseActivity
            {
                SourceId = sourceId,
                ProcedureName = "dbo.Pr_CitiesGetCountries",
                Inputs = inputs,
                Outputs = outputs
            };
            var modelItem = ModelItemUtils.CreateModelItem(sqlServerActivity);
            var mockServiceInputViewModel = new Mock<IManageDatabaseInputViewModel>();
            var mockDbServiceModel = new Mock<IDbServiceModel>();
            var mockEnvironmentRepo = new Mock<IServerRepository>();
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.EnvironmentID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockEnvironmentRepo.Setup(repository => repository.ActiveServer).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IServer, bool>>>())).Returns(mockEnvironmentModel.Object);

            sqlsource = new DbSourceDefinition
            {
                Name = "GreenPoint",
                Type = enSourceType.SqlDatabase
            };

            _testingDbSource = new DbSourceDefinition
            {
                Name = "testingDBSrc",
                Type = enSourceType.SqlDatabase,
                Id = sourceId
            };
            _importOrderAction = new DbAction
            {
                Name = "dbo.ImportOrder",
                Inputs = new List<IServiceInput> { new ServiceInput("ProductId", "") }
            };

            _getCountriesAction = new DbAction { Name = "dbo.Pr_CitiesGetCountries" };
            _getCountriesAction.Inputs = inputs;
            var dbSources = new ObservableCollection<IDbSource> { _testingDbSource, sqlsource };
            mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(dbSources);
            mockDbServiceModel.Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction> { _getCountriesAction, _importOrderAction });
            mockServiceInputViewModel.SetupAllProperties();
            var sqlServerDesignerViewModel = new SqlServerDatabaseDesignerViewModel(modelItem, mockDbServiceModel.Object, new SynchronousAsyncWorker(), new ViewPropertyBuilder());

            ScenarioContext.Current.Add("viewModel", sqlServerDesignerViewModel);
            ScenarioContext.Current.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            ScenarioContext.Current.Add("mockDbServiceModel", mockDbServiceModel);
        }

        [Given(@"Sql Server Source is ""(.*)""")]
        public void GivenSourceIs(string sourceName)
        {
            var selectedSource = GetViewModel().SourceRegion.SelectedSource;
            Assert.IsNotNull(selectedSource);
            Assert.AreEqual<string>(sourceName, selectedSource.Name);
        }

        [Given(@"Sql Server Action is ""(.*)""")]
        public void GivenActionIs(string actionName)
        {
            var selectedProcedure = GetViewModel().ActionRegion.SelectedAction;
            Assert.IsNotNull(selectedProcedure);
            Assert.AreEqual<string>(actionName, selectedProcedure.Name);
        }


        [When(@"I Select ""(.*)"" as Source")]
        public void WhenISelectAsSource(string sourceName)
        {
            if (sourceName == "GreenPoint")
            {
                _importOrderAction = new DbAction();
                _importOrderAction.Name = "dbo.ImportOrder";
                _importOrderAction.Inputs = new List<IServiceInput> { new ServiceInput("ProductId", "") };
                GetDbServiceModel().Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction> { _importOrderAction });
                GetViewModel().SourceRegion.SelectedSource = sqlsource;
            }
        }

        [When(@"I select dbo.ImportOrder as the action")]
        public void WhenISelectImportOrderAsTheAction()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("Column1"));
            dataTable.ImportRow(dataTable.LoadDataRow(new object[] { 1 }, true));
            GetDbServiceModel().Setup(model => model.TestService(It.IsAny<IDatabaseService>())).Returns(dataTable);
            GetViewModel().ActionRegion.SelectedAction = _importOrderAction;
        }

        [When(@"I click Test")]
        public void WhenIClickTest()
        {
            var testCommand = GetViewModel().ManageServiceInputViewModel.TestAction;
            testCommand();
        }

        [When(@"I click OK")]
        public void WhenIClickOK()
        {
            GetViewModel().ManageServiceInputViewModel.OkAction();
        }

        [When(@"I click Validate")]
        public void WhenIClickValidate()
        {
            GetViewModel().TestInputCommand.Execute(null);
        }

        [Then(@"Test Sql Server Inputs appear as")]
        public void ThenTestInputsAppearAs(Table table)
        {
            var rowNum = 0;
            var viewModel = GetViewModel();
            foreach (var row in table.Rows)
            {
                var inputValue = row["ProductId"];
                var serviceInputs = viewModel.ManageServiceInputViewModel.Inputs.ToList();
                var serviceInput = serviceInputs[rowNum];
                serviceInput.Value = inputValue;
                rowNum++;
            }
        }

        [Then(@"Test Connector and Calculate Outputs Sql Server Outputs appear as")]
        public void ThenTestConnectorAndCalculateOutputsOutputsAppearAs(Table table)
        {
            var rowIdx = 0;
            foreach (var tableRow in table.Rows)
            {
                var outputValue = tableRow["Column1"];
                var rows = GetViewModel().ManageServiceInputViewModel.TestResults.Rows;
                var dataRow = rows[rowIdx];
                var dataRowValue = dataRow[0].ToString();
                Assert.AreEqual<string>(outputValue, dataRowValue);
                rowIdx++;
            }
        }

        [Then(@"Sql Server Outputs appear as")]
        public void ThenOutputsAppearAs(Table table)
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

        [Then(@"Sql Server Recordset Name equals ""(.*)""")]
        public void ThenRecordsetNameEquals(string recsetName)
        {
            if (!string.IsNullOrEmpty(recsetName))
            {
                Assert.AreEqual<string>(recsetName, GetViewModel().OutputsRegion.RecordsetName);
            }
        }

        [Given(@"I have a workflow ""(.*)""")]
        public void GivenIHaveAWorkflow(string p0)
        {
            var sourceId = Guid.NewGuid();
            var inputs = new List<IServiceInput> { new ServiceInput("Prefix", "[[Prefix]]") };
            var outputs = new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping("CountryID", "CountryID", "dbo_Pr_CitiesGetCountries"),
                new ServiceOutputMapping("Description", "Description", "dbo_Pr_CitiesGetCountries")
            };

            var sqlServerActivity = new DsfSqlServerDatabaseActivity
            {
                SourceId = sourceId,
                ProcedureName = "dbo.Pr_CitiesGetCountries",
                Inputs = inputs,
                Outputs = outputs
            };
            //sqlServerActivity.Execute(new Mock<IDSFDataObject>().Object, 0);
            var modelItem = ModelItemUtils.CreateModelItem(sqlServerActivity);
            var mockServiceInputViewModel = new Mock<IManageDatabaseInputViewModel>();
            var mockDbServiceModel = new Mock<IDbServiceModel>();
            var mockEnvironmentRepo = new Mock<IServerRepository>();
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.EnvironmentID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockEnvironmentRepo.Setup(repository => repository.ActiveServer).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IServer, bool>>>())).Returns(mockEnvironmentModel.Object);

            sqlsource = new DbSourceDefinition
            {
                Name = "GreenPoint",
                Type = enSourceType.SqlDatabase
            };

            _testingDbSource = new DbSourceDefinition
            {
                Name = "testingDBSrc",
                Type = enSourceType.SqlDatabase,
                Id = sourceId,
                ServerName = "localhost"
            };
            _importOrderAction = new DbAction
            {
                Name = "dbo.ImportOrder",
                Inputs = new List<IServiceInput> { new ServiceInput("ProductId", "") }
            };

            _getCountriesAction = new DbAction { Name = "dbo.Pr_CitiesGetCountries" };
            _getCountriesAction.Inputs = inputs;
            var dbSources = new ObservableCollection<IDbSource> { _testingDbSource, sqlsource };
            mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(dbSources);

            var privateObject = new PrivateObject(sqlServerActivity);

            mockDbServiceModel.Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction> { _getCountriesAction, _importOrderAction });
            mockServiceInputViewModel.SetupAllProperties();

            var sqlServerDesignerViewModel = new SqlServerDatabaseDesignerViewModel(modelItem, mockDbServiceModel.Object, new SynchronousAsyncWorker(), new ViewPropertyBuilder());

            ScenarioContext.Current.Add("viewModel", sqlServerDesignerViewModel);
            ScenarioContext.Current.Add("privateObject", privateObject);
            ScenarioContext.Current.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            ScenarioContext.Current.Add("mockDbServiceModel", mockDbServiceModel);
        }

        [Given(@"""(.*)"" contains ""(.*)"" from server ""(.*)"" with mapping as")]
        public void GivenContainsFromServerWithMappingAs(string p0, string p1, string p2, Table table)
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsTrue(viewModel.SourceRegion.Sources.Any(p => p.ServerName == p2));
            Assert.IsNotNull(table);
        }


        [When(@"""(.*)"" is executed")]
        public void WhenIsExecuted(string p0)
        {
            GetViewModel().ManageServiceInputViewModel.TestCommand.Execute(null);
            Assert.IsTrue(true);
        }

        [Then(@"The Sql Server step ""(.*)"" in Workflow ""(.*)"" debug outputs appear as")]
        public void ThenTheSqlsERVERInWorkflowDebugOutputsAs(string p0, string p1, Table table)
        {
            var viewModel = GetViewModel().ErrorRegion.Errors;
            if (table != null && viewModel.Count > 0)
            {
                Assert.IsTrue(table.Rows[0].Values.ToString() == p0);
            }
        }

        [Then(@"the workflow containing the Sql Server connector has ""(.*)"" execution error")]
        public void ThenTheSqlsERVERWorkflowExecutionHasError(string p0)
        {
            Assert.IsNotNull(GetViewModel().ManageServiceInputViewModel.Errors);
        }

        [Given(@"And ""(.*)"" contains ""(.*)"" from server ""(.*)"" with Mapping To as")]
        public void GivenAndContainsFromServerWithMappingToAs(string p0, string p1, string p2, Table table)
        {
            Assert.IsTrue(true);
        }

        #region Private Methods

        SqlServerDatabaseDesignerViewModel GetViewModel()
        {
            return ScenarioContext.Current.Get<SqlServerDatabaseDesignerViewModel>("viewModel");
        }

        PrivateObject GetSqlServerPrivateObject()
        {
            return ScenarioContext.Current.Get<PrivateObject>("privateObject");
        }

        Mock<IManageDatabaseInputViewModel> GetInputViewModel()
        {
            return ScenarioContext.Current.Get<Mock<IManageDatabaseInputViewModel>>("mockServiceInputViewModel");
        }

        Mock<IDbServiceModel> GetDbServiceModel()
        {
            return ScenarioContext.Current.Get<Mock<IDbServiceModel>>("mockDbServiceModel");
        }

        IDbServiceModel GetRealDbServiceModel()
        {
            return ScenarioContext.Current.Get<IDbServiceModel>("DbServiceModel");
        }


        [Then(@"the workflow execution has ""(.*)"" error")]
        public void ThenTheWorkflowExecutionHasError(string p0)
        {
            Assert.AreEqual(0, GetViewModel().ErrorRegion.Errors.Count);
        }
        [Given(@"I click Test")]
        public void GivenIClickTest()
        {
            var sqlServiceGetViewModel = GetViewModel();
            sqlServiceGetViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
        }

        [Given(@"I click Sql Generate Outputs")]
        public void GivenIClickSqlGenerateOutputs()
        {
            var webServiceGetViewModel = GetViewModel();
            webServiceGetViewModel.TestInputCommand.Execute(null);
        }

        [Given(@"Prefix is set to ""(.*)""")]
        public void GivenPrefixIsSetTo(string prefix)
        {
            var webServiceGetViewModel = GetViewModel();

            webServiceGetViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
        }

        [When(@"Sql Server is executed")]
        public void WhenSqlServerIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        protected override void BuildDataList()
        {
            throw new NotImplementedException();
        }

        protected override List<IDebugItemResult> GetDebugInputItemResults(Activity activity)
        {
            return base.GetDebugInputItemResults(activity);
        }

        protected override List<IDebugItemResult> GetDebugOutputItemResults(Activity activity)
        {
            return base.GetDebugOutputItemResults(activity);
        }
        #endregion
    }
}
