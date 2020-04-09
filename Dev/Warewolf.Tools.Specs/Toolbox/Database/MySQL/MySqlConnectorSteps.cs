using Dev2.Activities;
using Dev2.Activities.Designers2.MySqlDatabase;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Studio.Core.Activities.Utils;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Core;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.Core;
using Warewolf.Tools.Specs.Toolbox.Database;
using Warewolf.Studio.ViewModels;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Studio.Core;
using Warewolf.UnitTestAttributes;

namespace Warewolf.ToolsSpecs.Toolbox.Resources.MySQL
{
    [Binding]
    public sealed class MySqlConnectorSteps : DatabaseToolsSteps
    {
        DbSourceDefinition _sqlSource;
        DbSourceDefinition _anotherSqlSource;
        DbAction _someAction;
        Mock<IServiceOutputMapping> _outputMapping;
        readonly ScenarioContext _scenarioContext;
        readonly CommonSteps _commonSteps;
        static Depends _containerOps;

        public MySqlConnectorSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _commonSteps = new CommonSteps(_scenarioContext);
        }

        [Given(@"I drag in mysql connector tool")]
        public void GivenIDragInMysqlConnectorTool()
        {
            var mysqlActivity = new DsfMySqlDatabaseActivity();
            var modelItem = ModelItemUtils.CreateModelItem(mysqlActivity);

            var mockDatabaseInputViewModel = new Mock<IManageDatabaseInputViewModel>();
            var mockDbServiceModel = new Mock<IDbServiceModel>();

            var mockEnvironmentRepo = new Mock<IServerRepository>();
            var mockEnvironmentModel = new Mock<IServer>();

            _outputMapping = new Mock<IServiceOutputMapping>();

            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.EnvironmentID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockEnvironmentRepo.Setup(repository => repository.ActiveServer).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IServer, bool>>>())).Returns(mockEnvironmentModel.Object);

            _sqlSource = new DbSourceDefinition
            {
                Name = "DemoSqlsource",
                Type = enSourceType.MySqlDatabase,
                ServerName = "Localhost",
                UserName = "user",
                Password = "userPassword",
                AuthenticationType = Dev2.Runtime.ServiceModel.Data.AuthenticationType.User
            };
            _anotherSqlSource = new DbSourceDefinition
            {
                Name = "AnotherSqlSource",
                Type = enSourceType.MySqlDatabase,
                ServerName = "Localhost",
                UserName = "user",
                Password = "userPassword",
                AuthenticationType = Dev2.Runtime.ServiceModel.Data.AuthenticationType.User
            };

            var dbSources = new ObservableCollection<IDbSource> { _sqlSource, _anotherSqlSource };

            mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(dbSources);
            mockDbServiceModel.Setup(model => model.GetActions(_sqlSource));
            mockDatabaseInputViewModel.SetupAllProperties();
            mockDatabaseInputViewModel.Setup(model => model.OkSelected).Returns(true);

            var mysqlDesignerViewModel = new MySqlDatabaseDesignerViewModel(modelItem, mockDbServiceModel.Object, new SynchronousAsyncWorker(), new ViewPropertyBuilder());

            _scenarioContext.Add("mysqlActivity", mysqlActivity);
            _scenarioContext.Add("viewModel", mysqlDesignerViewModel);
            _scenarioContext.Add("mockDatabaseInputViewModel", mockDatabaseInputViewModel);
            _scenarioContext.Add("mockDbServiceModel", mockDbServiceModel);
        }

        #region Private Methods

        MySqlDatabaseDesignerViewModel GetViewModel()
        {
            return _scenarioContext.Get<MySqlDatabaseDesignerViewModel>("viewModel");
        }

        Mock<IDbServiceModel> GetServiceModel()
        {
            return _scenarioContext.Get<Mock<IDbServiceModel>>("mockDbServiceModel");
        }

        #endregion

        [Given(@"Source is enabled for mysql connector tool")]
        public void GivenSourceIsEnabledForMysqlConnectorTool()
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsNotNull(viewModel.SourceRegion);
            Assert.IsTrue(viewModel.SourceRegion.IsEnabled);
        }

        [Given(@"Action is Not enabled for mysql connector tool")]
        public void GivenActionIsNotEnabledForMysqlConnectorTool()
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.ActionRegion.IsActionEnabled);
        }

        [Given(@"Input is Not eabled for mysql connector tool")]
        public void GivenInputIsNotEabledForMysqlConnectorTool()
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.InputArea.IsEnabled);
        }

        [Then(@"I select ""(.*)"" Source on mysql connector tool")]
        public void ThenISelectSourceOnMysqlConnectorTool(string p0)
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
            viewModel.SourceRegion.SelectedSource = viewModel.SourceRegion.Sources.FirstOrDefault(p => p.Name == p0);
            Assert.IsNotNull(viewModel.SourceRegion.SelectedSource);
            Assert.AreEqual(p0, viewModel.SourceRegion.SelectedSource.Name);
        }


        [Then(@"Action is Enabled on mysql connector tool")]
        public void ThenActionIsEnabledOnMysqlConnectorTool()
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsTrue(viewModel.ActionRegion.IsActionEnabled);
            Assert.IsNull(viewModel.ActionRegion.SelectedAction);
        }

        void SetupActions(IDbSource selectedSource)
        {
            if (selectedSource != null)
            {
                _someAction = new DbAction
                {
                    Name = "someAction",
                    Inputs = new List<IServiceInput> { new ServiceInput("SomeInput", "") }
                };
                var serviceModel = GetServiceModel();
                serviceModel.Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction> { _someAction });
            }
            if (GetViewModel().ActionRegion.SelectedAction == null)
            {
                GetViewModel().ActionRegion.SelectedAction = _someAction;
            }
        }

        [Then(@"Input is Not enabled for mysql connector tool")]
        public void ThenInputIsNotEnabledForMysqlConnectorTool()
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.InputArea.IsEnabled);
        }

        [Then(@"I select ""(.*)"" Action for mysql connector tool")]
        public void ThenISelectActionForMysqlConnectorTool(string p0)
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
            SetupActions(viewModel.SourceRegion.SelectedSource);
            viewModel.ActionRegion.SelectedAction = _someAction;
            Assert.IsNotNull(viewModel.ActionRegion.SelectedAction);
        }


        [Then(@"Input is enabled for mysql connector tool")]
        public void ThenInputIsEnabledForMysqlConnectorTool()
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsTrue(viewModel.InputArea.IsEnabled);
        }

        [Given(@"Input is enabled for existing mysql connector tool")]
        public void GivenInputIsEnabledForExistingMysqlConnectorTool()
        {
            ThenInputIsEnabledForMysqlConnectorTool();
        }

        [Then(@"Inputs are ""(.*)"" for mysql connector tool")]
        public void ThenInputsAreForMysqlConnectorTool(string p0)
        {
            var viewModel = GetViewModel();
            var onlyAction = viewModel.ActionRegion.SelectedAction.Inputs.FirstOrDefault();
            if (onlyAction != null)
            {
                Assert.AreEqual(p0, onlyAction.Name);
            }
        }

        [Then(@"I click validate on mysql connector tool")]
        public void ThenIClickValidateOnMysqlConnectorTool()
        {
            var viewModel = GetViewModel();
            viewModel.Validate();
        }

        [When(@"I click Test on mysql connector tool")]
        public void WhenIClickTestOnMysqlConnectorTool()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.TestInputCommand.CanExecute());
            var serviceModel = GetServiceModel();
            var executeResult = viewModel.TestInputCommand.Execute();
            Assert.AreEqual("RanToCompletion", executeResult.Status.ToString());
        }

        [When(@"The Connector and Calculate Outputs appear for mysql connector tool")]
        public void WhenTheConnectorAndCalculateOutputsAppearForMysqlConnectorTool()
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsNotNull(viewModel.OutputsRegion);
            SetupOutpuRegions();
            var outputResults = viewModel.OutputsRegion.Outputs.FirstOrDefault();
            if (outputResults != null)
            {
                Assert.AreEqual("SomeRecordSet", outputResults.RecordSetName);
            }
        }

        void SetupOutpuRegions()
        {
            GetViewModel().OutputsRegion.Outputs = new IServiceOutputMapping[]
            {
                new ServiceOutputMapping
                {
                    MappedFrom = "MappingFrom",
                    MappedTo = "MappedTo",
                    RecordSetName = "SomeRecordSet"
                }
            };
        }

        [Then(@"I click OK on mysql connector tool")]
        public void ThenIClickOkayOnMysqlConnectorTool()
        {
            Assert.IsTrue(ClickOk());
        }

        bool ClickOk()
        {
            return GetViewModel().ManageServiceInputViewModel.OkSelected = true;
        }

        [Then(@"The recordset name appear as ""(.*)"" on mysql connector tool")]
        public void ThenTheRecordsetNameAppearAsOnMysqlConnectorTool(string p0)
        {
            var viewModel = GetViewModel();
            var outputMapping = viewModel.OutputsRegion.Outputs.FirstOrDefault();
            if (outputMapping != null)
            {
                Assert.AreEqual(p0, outputMapping.RecordSetName);
            }
        }

        [Given(@"I open an existing mysql connector tool")]
        public void GivenIOpenAnExistingMysqlConnectorTool()
        {
            var mysqlActivity = new DsfMySqlDatabaseActivity();
            var modelItem = ModelItemUtils.CreateModelItem(mysqlActivity);

            var mockDatabaseInputViewModel = new Mock<IManageDatabaseInputViewModel>();
            var mockDbServiceModel = new Mock<IDbServiceModel>();

            var mockEnvironmentRepo = new Mock<IServerRepository>();
            var mockEnvironmentModel = new Mock<IServer>();

            _outputMapping = new Mock<IServiceOutputMapping>();

            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.EnvironmentID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockEnvironmentRepo.Setup(repository => repository.ActiveServer).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IServer, bool>>>())).Returns(mockEnvironmentModel.Object);

            _sqlSource = new DbSourceDefinition
            {
                Name = "DemoSqlsource",
                Type = enSourceType.MySqlDatabase,
                ServerName = "Localhost",
                UserName = "user",
                Password = "userPassword",
                AuthenticationType = Dev2.Runtime.ServiceModel.Data.AuthenticationType.User
            };
            _anotherSqlSource = new DbSourceDefinition
            {
                Name = "AnotherSqlSource",
                Type = enSourceType.MySqlDatabase,
                ServerName = "Localhost",
                UserName = "user",
                Password = "userPassword",
                AuthenticationType = Dev2.Runtime.ServiceModel.Data.AuthenticationType.User
            };
            var dbSources = new ObservableCollection<IDbSource> { _sqlSource, _anotherSqlSource };

            mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(dbSources);
            mockDbServiceModel.Setup(model => model.GetActions(_sqlSource));
            mockDatabaseInputViewModel.SetupAllProperties();

            mockDatabaseInputViewModel.Setup(model => model.OkSelected).Returns(true);

            var mysqlDesignerViewModel = new MySqlDatabaseDesignerViewModel(modelItem, mockDbServiceModel.Object, new SynchronousAsyncWorker(), new ViewPropertyBuilder());

            var selectedSource = SetupSelectedSource(mysqlDesignerViewModel);

            _scenarioContext.Add("viewModel", mysqlDesignerViewModel);
            _scenarioContext.Add("mockDatabaseInputViewModel", mockDatabaseInputViewModel);
            _scenarioContext.Add("mockDbServiceModel", mockDbServiceModel);

            SetupActions(selectedSource);
        }

        static IDbSource SetupSelectedSource(MySqlDatabaseDesignerViewModel mysqlDesignerViewModel)
        {
            return mysqlDesignerViewModel.SourceRegion.SelectedSource =
                mysqlDesignerViewModel.SourceRegion.Sources.FirstOrDefault(p => p.Name == "DemoSqlsource");
        }

        [Given(@"Source is enabled and set to ""(.*)"" on mysql connector tool")]
        public void GivenSourceIsEnabledAndSetToOnMysqlConnectorTool(string p0)
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsNotNull(viewModel.SourceRegion);
            Assert.IsTrue(viewModel.SourceRegion.IsEnabled);
            Assert.AreEqual(p0, viewModel.SourceRegion.SelectedSource.Name);
        }

        [Given(@"Action is Enabled and set to ""(.*)"" on mysql connector tool")]
        public void GivenActionIsEnabledAndSetToOnMysqlConnectorTool(string p0)
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsTrue(viewModel.ActionRegion.IsActionEnabled);
            Assert.IsNotNull(viewModel.ActionRegion.SelectedAction);
            Assert.AreEqual(p0, viewModel.ActionRegion.SelectedAction.Name);
        }

        [Given(@"Input is Not enabled for mysql connector tool")]
        public void GivenInputIsNotEnabledForMysqlConnectorTool()
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.ActionRegion.IsEnabled);
        }

        [Given(@"Input is enabled and set to ""(.*)"" on mysql connector tool")]
        public void GivenInputIsEnabledAndSetToOnMysqlConnectorTool(string p0)
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsTrue(viewModel.ActionRegion.IsEnabled);
            var onlyAction = viewModel.ActionRegion.SelectedAction.Inputs.FirstOrDefault();
            if (onlyAction != null)
            {
                Assert.AreEqual(p0, onlyAction.Name);
            }
        }

        [Given(@"Inputs are ""(.*)"" for mysql connector tool")]
        public void GivenInputsAreForMysqlConnectorTool(string p0)
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsTrue(viewModel.ActionRegion.IsEnabled);
            var onlyAction = viewModel.ActionRegion.SelectedAction.Inputs.FirstOrDefault();
            if (onlyAction != null)
            {
                Assert.AreEqual(p0, onlyAction.Name);
            }
        }

        [Then(@"The outputs appear as ""(.*)"" on mysql connector tool")]
        public void ThenTheOutputsAppearAsOnMysqlConnectorTool(string p0)
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel.OutputsRegion);
            SetupOutpuRegions();
            var outputResults = viewModel.OutputsRegion.Outputs.FirstOrDefault();
            if (outputResults != null)
            {
                Assert.AreEqual("SomeRecordSet", outputResults.RecordSetName);
            }
        }

        [When(@"I select ""(.*)"" Action for mysql connector tool")]
        public void WhenISelectActionForMysqlConnectorTool(string p0)
        {
            ThenISelectActionForMysqlConnectorTool(p0);
        }

        [Then(@"The recordset name changes to ""(.*)"" for mysql connector tool")]
        public void ThenTheRecordsetNameChangesToForMysqlConnectorTool(string p0)
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel.OutputsRegion);
            var outputResults = viewModel.OutputsRegion.Outputs.FirstOrDefault();
            if (outputResults != null)
            {
                Assert.AreEqual("SomeRecordSet", outputResults.RecordSetName);
            }
        }

        [Given(@"Mysql server is Enabled")]
        public void GivenMysqlServerIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.SourceRegion.IsEnabled);
        }

        [Given(@"Mysql Server Inputs Are Enabled")]
        public void GivenMysqlServerInputsAreEnabled()
        {
            var viewModel = GetViewModel();
            var hasInputs = viewModel.InputArea.Inputs != null || viewModel.InputArea.IsEnabled;
            Assert.IsTrue(hasInputs);
        }

        [Given(@"Validate MySql Server is Enabled")]
        public void GivenValidateMySqlServerIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.TestInputCommand.CanExecute());
        }

        [Given(@"I click MySql Generate Outputs")]
        public void GivenIClickMySqlGenerateOutputs()
        {
            var viewModel = GetViewModel();
            viewModel.TestInputCommand.Execute();
        }

        [Given(@"I click Test on Mysql")]
        public void GivenIClickTestOnMysql()
        {
            var viewModel = GetViewModel();
            viewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            viewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
        }

        [Given(@"Mysql Server Recordset Name equals ""(.*)""")]
        [When(@"Mysql Server Recordset Name equals ""(.*)""")]
        [Then(@"Mysql Server Recordset Name equals ""(.*)""")]
        public void ThenMysqlServerRecordsetNameEquals(string recsetName)
        {
            Assert.IsTrue(string.Equals(recsetName, GetViewModel().OutputsRegion.RecordsetName), $"Actual recordset name {GetViewModel().OutputsRegion.RecordsetName} does not equal expected recordset name {recsetName}.");
        }

        [Then(@"Mysql Server Outputs appear as")]
        public void ThenMysqlServerOutputsAppearAs(Table table)
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

        [Then(@"Action on mysql connector tool is null")]
        public void ThenActionOnMysqlConnectorToolIsNull()
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsTrue(viewModel.ActionRegion.IsActionEnabled);
        }

        [Then(@"Inputs on mysql connector tool is null")]
        public void ThenInputsOnMysqlConnectorToolIsNull()
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(0, viewModel.OutputsRegion.Outputs.Count);
        }
        [When(@"MySql Workflow ""(.*)"" containing dbTool is executed")]
        public void WhenMySqlWorkflowContainingDbToolIsExecuted(string workflowName)
        {
            WorkflowIsExecuted(workflowName);
        }

        [Given(@"I have workflow ""(.*)"" with ""(.*)"" MySql database connector")]
        public void GivenIHaveWorkflowWithMySqlDatabaseConnector(string workflowName, string activityName)
        {
            var environmentModel = _scenarioContext.Get<IServer>("server");
            environmentModel.Connect();
            _containerOps = new Depends(Depends.ContainerType.MySQL);
            CreateNewResourceModel(workflowName, environmentModel);
            CreateDBServiceModel(environmentModel);

            var dbServiceModel = _scenarioContext.Get<ManageDbServiceModel>("dbServiceModel");
            var mySqlActivity = new DsfMySqlDatabaseActivity { DisplayName = activityName };
            var modelItem = ModelItemUtils.CreateModelItem(mySqlActivity);
            var mysqlDesignerViewModel = new MySqlDatabaseDesignerViewModel(modelItem, dbServiceModel, new SynchronousAsyncWorker(), new ViewPropertyBuilder());
            var serviceInputViewModel = new ManageDatabaseServiceInputViewModel(mysqlDesignerViewModel, mysqlDesignerViewModel.Model);

            _commonSteps.AddActivityToActivityList(workflowName, activityName, mySqlActivity);
            DebugWriterSubscribe(environmentModel);
            _scenarioContext.Add("viewModel", mysqlDesignerViewModel);
            _scenarioContext.Add("parentName", workflowName);
        }

        [Given(@"I Select ""(.*)"" as MySql Server Source for ""(.*)""")]
        public void GivenISelectAsMySqlServerSourceFor(string sourceName, string activityName)
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

        [Given(@"I Select ""(.*)"" as MySql Server Action for ""(.*)""")]
        public void GivenISelectAsMySqlServerActionFor(string actionName, string activityName)
        {
            var vm = GetViewModel();
            Assert.IsNotNull(vm.ActionRegion);
            vm.ActionRegion.SelectedAction = vm.ActionRegion.Actions.FirstOrDefault(p => p.Name == actionName);
            SetDbAction(activityName, actionName);
        }

        [Given(@"MySql Command Timeout is ""(.*)"" millisenconds for ""(.*)""")]
        [When(@"MySql Command Timeout is ""(.*)"" millisenconds for ""(.*)""")]
        [Then(@"MySql Command Timeout is ""(.*)"" millisenconds for ""(.*)""")]
        public void GivenMySqlCommandTimeoutIsMillisencondsFor(int timeout, string activityName)
        {
            var vm = GetViewModel();
            Assert.IsNotNull(vm);
            vm.InputArea.CommandTimeout = timeout;
            SetCommandTimeout(activityName, timeout);
            Assert.AreEqual(timeout, vm.InputArea.CommandTimeout);
        }
        
        [AfterScenario("@ExecuteMySqlServerWithTimeout")]
        public void CleanUp()
        {
            CleanupForTimeOutSpecs();
            _containerOps?.Dispose();
        }
    }
}
