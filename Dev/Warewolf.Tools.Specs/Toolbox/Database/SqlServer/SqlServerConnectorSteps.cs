using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using ActivityUnitTests;
using Dev2.Activities.Designers.Tests.SqlServer;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.SqlServerDatabase;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Controller;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Messages;
using Dev2.Runtime.Hosting;
using Dev2.Services;
using Dev2.Session;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Threading;
using Dev2.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Core;
using Warewolf.Studio.ViewModels;
using Warewolf.Tools.Specs.BaseTypes;
using WarewolfParserInterop;

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
        readonly AutoResetEvent _resetEvt = new AutoResetEvent(false);
        const int EnvironmentConnectionTimeout = 15;

        SubscriptionService<DebugWriterWriteMessage> _debugWriterSubscriptionService;

        [BeforeScenario]
        public void Setup()
        {
            if (_debugWriterSubscriptionService != null)
            {
                _debugWriterSubscriptionService.Unsubscribe();
                _debugWriterSubscriptionService.Dispose();
            }
        }

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
            var workflowName = "SqlWorkflowForTimeout";
            var procedureName = "dbo.Pr_CitiesGetCountries";
            var sourceId = Guid.NewGuid();
            var inputs = new List<IServiceInput> { new ServiceInput("Prefix", "S") };
            var resourceId = "b9184f70-64ea-4dc5-b23b-02fcd5f91082".ToGuid();
            //Load Source based on the name
            var environmentModel = ServerRepository.Instance.Source;
            environmentModel.Connect();
            var resourceModel = new ResourceModel(environmentModel)
            {
                ID = Guid.NewGuid(),
                ResourceName = workflowName,
                Category = "Acceptance Tests\\" + workflowName,
                ResourceType = ResourceType.WorkflowService
            };
            environmentModel.ResourceRepository.Add(resourceModel);
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

            _debugWriterSubscriptionService = new SubscriptionService<DebugWriterWriteMessage>(environmentModel.Connection.ServerEvents);

            _debugWriterSubscriptionService.Subscribe(msg => Append(msg.DebugState));


            ScenarioContext.Current.Add("debugStates", new List<IDebugState>());
            ScenarioContext.Current.Add("resourceModel", resourceModel);
            ScenarioContext.Current.Add("viewModel", sqlServerDesignerViewModel);
            ScenarioContext.Current.Add("ServiceInputViewModel", serviceInputViewModel);
            ScenarioContext.Current.Add("server", environmentModel);
            ScenarioContext.Current.Add("resourceRepo", environmentModel.ResourceRepository);
            ScenarioContext.Current.Add("DbServiceModel", databaseService);
            ScenarioContext.Current.Add("parentName", workflowName);
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
        [Given(@"Workflow ""(.*)"" debug outputs as")]
        [When(@"Workflow ""(.*)"" debug outputs as")]
        [Then(@"Workflow ""(.*)"" debug outputs as")]
        public void ThenWorkflowDebugOutputsAs(string workflowName, Table table)
        {
            var toolName = "Pr_CitiesGetCountries";
            var debugStates = Get<List<IDebugState>>("debugStates");
            var workflowId = Guid.Empty;

            var toolSpecificDebug =
                debugStates.Where(ds => ds.ParentID.GetValueOrDefault() == workflowId && ds.DisplayName.Equals(toolName)).ToList();
            if (!toolSpecificDebug.Any())
            {
                toolSpecificDebug =
                debugStates.Where(ds => ds.DisplayName.Equals(toolName)).ToList();
            }
            // Data Merge breaks our debug scheme, it only ever has 1 value, not the expected 2 ;)
            var isDataMergeDebug = toolSpecificDebug.Count == 1 && toolSpecificDebug.Any(t => t.Name == "Data Merge");
            IDebugState outputState;
            if (toolSpecificDebug.Count > 1 && toolSpecificDebug.Any(state => state.StateType == StateType.End))
            {
                outputState = toolSpecificDebug.FirstOrDefault(state => state.StateType == StateType.End);
            }
            else
            {
                outputState = toolSpecificDebug.FirstOrDefault();
            }

            if (outputState != null && outputState.Outputs != null)
            {
                var SelectResults = outputState.Outputs.SelectMany(s => s.ResultsList);
                if (SelectResults != null && SelectResults.ToList() != null)
                {
                    _commonSteps.ThenTheDebugOutputAs(table, SelectResults.ToList(), isDataMergeDebug);
                    return;
                }
                Assert.Fail(outputState.Outputs.ToList() + " debug outputs found on " + workflowName + " does not include " + toolName + ".");
            }
            Assert.Fail("No debug output found for " + workflowName + ".");
        }

        [Given(@"I click Test")]
        public void GivenIClickTest()
        {
            var sqlServiceGetViewModel = GetViewModel();
            sqlServiceGetViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            sqlServiceGetViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
        }

        [Given(@"I click Sql Generate Outputs")]
        public void GivenIClickSqlGenerateOutputs()
        {
            var sqlServerGetViewModel = GetViewModel();
            sqlServerGetViewModel.TestInputCommand.Execute(null);
        }

        [Given(@"Prefix is set to ""(.*)""")]
        public void GivenPrefixIsSetTo(string prefix)
        {
            var sqlGetViewModel = GetViewModel();
            sqlGetViewModel.InputArea.Inputs.Single().Value = prefix;
        }

        [When(@"Sql Server is executed")]
        public void WhenSqlServerIsExecuted()
        {
            var resourceModel = SaveAWorkflow("SqlWorkflowForTimeout");
            ExecuteWorkflow(resourceModel);
        }
     
        private IContextualResourceModel SaveAWorkflow(string parentName)
        {
            TryGetValue("parentName", out string parentWorkflowName);
            var workflowName = string.IsNullOrEmpty(parentWorkflowName) ? parentName : parentWorkflowName;

            Get<List<IDebugState>>("debugStates").Clear();
            BuildDataList();

            var activityList = _commonSteps.GetActivityList();

            var flowSteps = new List<FlowStep>();

            TestStartNode = new FlowStep();
            flowSteps.Add(TestStartNode);
            if (activityList != null)
            {
                foreach (var activity in activityList)
                {
                    if (TestStartNode.Action == null)
                    {
                        TestStartNode.Action = activity.Value;
                    }
                    else
                    {
                        var flowStep = new FlowStep { Action = activity.Value };
                        flowSteps.Last().Next = flowStep;
                        flowSteps.Add(flowStep);
                    }
                }
            }
            TryGetValue("resourceModel", out IContextualResourceModel resourceModel);
            TryGetValue("server", out IServer server);
            TryGetValue("resourceRepo", out IResourceRepository repository);

            var currentDl = CurrentDl;
              resourceModel.DataList = currentDl.Replace("root", "DataList");
            var helper = new WorkflowHelper();
            var xamlDefinition = helper.GetXamlDefinition(FlowchartActivityBuilder);
            resourceModel.WorkflowXaml = xamlDefinition;
            repository.Save(resourceModel);
            repository.SaveToServer(resourceModel);

            return resourceModel;
        }

        public void ExecuteWorkflow(IContextualResourceModel resourceModel)
        {
            if (resourceModel?.Environment == null)
            {
                return;
            }

            var debugTo = new DebugTO { XmlData = "<DataList></DataList>", SessionID = Guid.NewGuid(), IsDebugMode = true };
            EnsureEnvironmentConnected(resourceModel.Environment, EnvironmentConnectionTimeout);
            var clientContext = resourceModel.Environment.Connection;
            if (clientContext != null)
            {
                var dataList = XElement.Parse(debugTo.XmlData);
                dataList.Add(new XElement("BDSDebugMode", debugTo.IsDebugMode));
                dataList.Add(new XElement("DebugSessionID", debugTo.SessionID));
                dataList.Add(new XElement("EnvironmentID", resourceModel.Environment.EnvironmentID));
                WebServer.Send(resourceModel, dataList.ToString(), new SynchronousAsyncWorker());
                _resetEvt.WaitOne(3000);                
            }
        }
        [When(@"the workflow execution has ""(.*)"" error")]
        public void WhenTheWorkflowExecutionHasError(string hasError)
        {
            TryGetValue("activityList", out Dictionary<string, Activity> activityList);
            var debugStates = Get<List<IDebugState>>("debugStates").ToList();

            if (hasError == "AN")
            {
                var innerWfHasErrorState = debugStates.FirstOrDefault(state => state.HasError && state.DisplayName.Equals("SqlWorkflowForTimeout"));
                Assert.IsNotNull(innerWfHasErrorState);
            }
            else
            {
                debugStates.ForEach(p => Assert.IsFalse(p.HasError));
            }
        }
        void EnsureEnvironmentConnected(IServer server, int timeout)
        {
            if (timeout <= 0)
            {
                _scenarioContext.Add("ConnectTimeoutCountdown", EnvironmentConnectionTimeout);
                throw new TimeoutException("Connection to Warewolf server \"" + server.Name + "\" timed out.");
            }

            if (!server.IsConnected && !server.Connection.IsConnected)
            {
                server.Connect();
            }

            if (!server.IsConnected && !server.Connection.IsConnected)
            {
                Thread.Sleep(GlobalConstants.NetworkTimeOut);
                timeout--;
                EnsureEnvironmentConnected(server, timeout);
            }
        }
        protected override void BuildDataList()
        {
            BuildShapeAndTestData();
        }
        protected void BuildShapeAndTestData()
        {
            var shape = new XElement("root");
            var data = new XElement("root");
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(new ResourceModel(null));
            DataListSingleton.SetDataList(dataListViewModel);

            var row = 0;
            _scenarioContext.TryGetValue("variableList", out dynamic variableList);
            if (variableList != null)
            {
                try
                {
                    foreach (dynamic variable in variableList)
                    {
                        var variableName = DataListUtil.AddBracketsToValueIfNotExist(variable.Item1);
                        if (!string.IsNullOrEmpty(variable.Item1) && !string.IsNullOrEmpty(variable.Item2))
                        {
                            string value = variable.Item2 == "blank" ? "" : variable.Item2;
                            if (value.ToUpper() == "NULL")
                            {
                                DataObject.Environment.AssignDataShape(variable.Item1);
                            }
                            else
                            {
                                DataObject.Environment.Assign(variableName, value, 0);
                            }
                        }
                        if (DataListUtil.IsValueScalar(variableName))
                        {
                            var scalarName = DataListUtil.RemoveLanguageBrackets(variableName);
                            var scalarItemModel = new ScalarItemModel(scalarName);
                            if (!scalarItemModel.HasError)
                            {
                                DataListSingleton.ActiveDataList.Add(scalarItemModel);
                            }
                        }
                        if (DataListUtil.IsValueRecordsetWithFields(variableName))
                        {
                            var rsName = DataListUtil.ExtractRecordsetNameFromValue(variableName);
                            var fieldName = DataListUtil.ExtractFieldNameOnlyFromValue(variableName);
                            var rs = DataListSingleton.ActiveDataList.RecsetCollection.FirstOrDefault(model => model.Name == rsName);
                            if (rs == null)
                            {
                                var recordSetItemModel = new RecordSetItemModel(rsName);
                                DataListSingleton.ActiveDataList.Add(recordSetItemModel);
                                recordSetItemModel.Children.Add(new RecordSetFieldItemModel(fieldName,
                                    recordSetItemModel));
                            }
                            else
                            {
                                var recordSetFieldItemModel = rs.Children.FirstOrDefault(model => model.Name == fieldName);
                                if (recordSetFieldItemModel == null)
                                {
                                    rs.Children.Add(new RecordSetFieldItemModel(fieldName, rs));
                                }
                            }
                        }
                        //Build(variable, shape, data, row);
                        row++;
                    }
                    DataListSingleton.ActiveDataList.WriteToResourceModel();
                }

                catch

                {

                }
            }

            var isAdded = _scenarioContext.TryGetValue("rs", out List<Tuple<string, string>> emptyRecordset);
            if (isAdded)
            {
                foreach (Tuple<string, string> emptyRecord in emptyRecordset)
                {
                    DataObject.Environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(emptyRecord.Item1), emptyRecord.Item2, 0);
                }
            }

            _scenarioContext.TryGetValue("objList", out dynamic objList);
            if (objList != null)
            {
                try
                {
                    foreach (dynamic variable in objList)
                    {
                        if (!string.IsNullOrEmpty(variable.Item1) && !string.IsNullOrEmpty(variable.Item2))
                        {
                            string value = variable.Item2 == "blank" ? "" : variable.Item2;
                            if (value.ToUpper() == "NULL")
                            {
                                DataObject.Environment.AssignDataShape(variable.Item1);
                            }
                            else
                            {
                                DataObject.Environment.AssignJson(new AssignValue(DataListUtil.AddBracketsToValueIfNotExist(variable.Item1), value), 0);
                            }
                        }
                    }
                }

                catch

                {

                }
            }

            CurrentDl = shape.ToString();
            TestData = data.ToString();
        }
        T Get<T>(string keyName)
        {
            return _scenarioContext.Get<T>(keyName);
        }

        void TryGetValue<T>(string keyName, out T value)
        {
            _scenarioContext.TryGetValue(keyName, out value);
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

        protected override List<IDebugItemResult> GetDebugInputItemResults(Activity activity)
        {
            return base.GetDebugInputItemResults(activity);
        }

        protected override List<IDebugItemResult> GetDebugOutputItemResults(Activity activity)
        {
            return base.GetDebugOutputItemResults(activity);
        }

        void Append(IDebugState debugState)
        {
            TryGetValue("debugStates", out List<IDebugState> debugStates);
            TryGetValue("debugStatesDuration", out List<IDebugState> debugStatesDuration);
            TryGetValue("parentName", out string workflowName);
            TryGetValue("server", out IServer server);
            if (debugStatesDuration == null)
            {
                debugStatesDuration = new List<IDebugState>();
                Add("debugStatesDuration", debugStatesDuration);
            }
            if (debugState.WorkspaceID == server.Connection.WorkspaceID)
            {
                if (debugState.StateType != StateType.Duration)
                {
                    debugStates.Add(debugState);
                }
                else
                {
                    debugStatesDuration.Add(debugState);
                }
            }
            if (debugState.IsFinalStep() && debugState.DisplayName.Equals(workflowName))
            {
                _resetEvt.Set();
            }
        }
        void Add(string key, object value) => _scenarioContext.Add(key, value);

        [Given(@"Sql Connection Timeout is ""(.*)""")]
        public void GivenSqlConnectionTimeoutIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        [Given(@"Sql Command Timeout is ""(.*)""")]
        public void GivenSqlCommandTimeoutIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }


        #endregion
    }
}
