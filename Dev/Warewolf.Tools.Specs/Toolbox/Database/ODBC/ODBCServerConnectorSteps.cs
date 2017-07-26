using Dev2.Activities.Designers2.ODBC;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Dev2.Common.Interfaces.Core;
using TechTalk.SpecFlow;
using Warewolf.Core;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Interfaces;

namespace Dev2.Activities.Specs.Toolbox.Resources
{
    [Binding]
    public class ODBCServerConnectorSteps
    {
        private readonly ScenarioContext scenarioContext;

        public ODBCServerConnectorSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        private DbSourceDefinition _greenPointSource;
        private DbAction _importOrderAction;
        private DbSourceDefinition _testingDbSource;
        private DbAction _getCountriesAction;

        [Given(@"I drag a ODBC Server database connector")]
        public void GivenIDragAODBCServerDatabaseConnector()
        {
            Assert.IsNotNull(GetViewModel());
        }

        [Given(@"I open workflow with ODBC connector")]
        public void GivenIOpenWolf()
        {
            var sourceId = Guid.NewGuid();
            var inputs = new List<IServiceInput> { new ServiceInput("Prefix", "[[Prefix]]") };
            var outputs = new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping("CountryID", "CountryID", "dbo_Pr_CitiesGetCountries"),
                new ServiceOutputMapping("Description", "Description", "dbo_Pr_CitiesGetCountries")
            };
            var odbcServerActivity = new DsfODBCDatabaseActivity
            {
                SourceId = sourceId,
                CommandText = "dbo.Pr_CitiesGetCountries",
                Inputs = inputs,
                Outputs = outputs
            };
            var modelItem = ModelItemUtils.CreateModelItem(odbcServerActivity);
            
            var mockServiceInputViewModel = new Mock<IManageServiceInputViewModel>();
            var mockDatabaseInputViewModel = new Mock<IManageDatabaseInputViewModel>();
            var mockDbServiceModel = new Mock<IDbServiceModel>();
            var mockEnvironmentRepo = new Mock<IServerRepository>();
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.EnvironmentID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockEnvironmentRepo.Setup(repository => repository.ActiveServer).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IServer, bool>>>())).Returns(mockEnvironmentModel.Object);

            _greenPointSource = new DbSourceDefinition
            {
                Name = "GreenPoint",
                Type = enSourceType.ODBC
            };

            _testingDbSource = new DbSourceDefinition
            {
                Name = "testingDBSrc",
                Type = enSourceType.ODBC,
                Id = sourceId
            };
            _importOrderAction = new DbAction
            {
                Name = "dbo.ImportOrder",
                Inputs = new List<IServiceInput> { new ServiceInput("ProductId", "") }
            };

            _getCountriesAction = new DbAction { Name = "dbo.Pr_CitiesGetCountries" };
            _getCountriesAction.Inputs = inputs;
            var dbSources = new ObservableCollection<IDbSource> { _testingDbSource, _greenPointSource };
            
            mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(dbSources);
            mockDbServiceModel.Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction> { _getCountriesAction, _importOrderAction });
            mockServiceInputViewModel.SetupAllProperties();

            var mockAction = new Mock<Action>(MockBehavior.Default);
            var mockOkAction = new Mock<Action>(MockBehavior.Default);
            mockDatabaseInputViewModel.Setup(model => model.TestAction).Returns(mockAction.Object);
            mockDatabaseInputViewModel.Setup(model => model.OkAction).Returns(mockOkAction.Object);
            var odbcDatabaseDesignerViewModel = new ODBCDatabaseDesignerViewModel(modelItem, mockDbServiceModel.Object);

            scenarioContext.Add("viewModel", odbcDatabaseDesignerViewModel);
            scenarioContext.Add("mockDatabaseInputViewModel", mockDatabaseInputViewModel);
            scenarioContext.Add("mockDbServiceModel", mockDbServiceModel);
        }
        [Given(@"I open a new Workflow")]
        public void GivenIOpenANewWorkflow()
        {
            var sourceId = Guid.NewGuid();
            var inputs = new List<IServiceInput> { new ServiceInput("Prefix", "[[Prefix]]") };
            var outputs = new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping("CountryID", "CountryID", "dbo_Pr_CitiesGetCountries"),
                new ServiceOutputMapping("Description", "Description", "dbo_Pr_CitiesGetCountries")
            };
            var odbcServerActivity = new DsfODBCDatabaseActivity
            {
                SourceId = sourceId,
                CommandText = "dbo.Pr_CitiesGetCountries",
                Inputs = inputs,
                Outputs = outputs              
            };
            
            var modelItem = ModelItemUtils.CreateModelItem(odbcServerActivity);            
            var mockServiceInputViewModel = new Mock<IManageServiceInputViewModel>();
            var mockDatabaseInputViewModel = new Mock<IManageDatabaseInputViewModel>();
            var mockDbServiceModel = new Mock<IDbServiceModel>();
            var mockEnvironmentRepo = new Mock<IServerRepository>();
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.EnvironmentID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockEnvironmentRepo.Setup(repository => repository.ActiveServer).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IServer, bool>>>())).Returns(mockEnvironmentModel.Object);

            var mockInputArea = new Mock<IGenerateInputArea>();
            var mockOutputArea = new Mock<IGenerateOutputArea>();
            
            mockDatabaseInputViewModel.SetupGet(model => model.InputArea).Returns(mockInputArea.Object);
            mockDatabaseInputViewModel.SetupGet(model => model.OutputArea).Returns(mockOutputArea.Object);            

            _greenPointSource = new DbSourceDefinition
            {
                Name = "GreenPoint",
                Type = enSourceType.ODBC
            };

            _testingDbSource = new DbSourceDefinition
            {
                Name = "testingDBSrc",
                Type = enSourceType.ODBC,
                Id = sourceId
            };
            _importOrderAction = new DbAction
            {
                Name = "dbo.ImportOrder",
                Inputs = new List<IServiceInput> { new ServiceInput("ProductId", "") }
            };

            _getCountriesAction = new DbAction { Name = "dbo.Pr_CitiesGetCountries" };
            _getCountriesAction.Inputs = inputs;
            var dbSources = new ObservableCollection<IDbSource> { _testingDbSource, _greenPointSource };
            mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(dbSources);
            mockDbServiceModel.Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction>
            {
                _getCountriesAction, _importOrderAction, new DbAction() {Name = "TestAcstion"}
            });
            mockServiceInputViewModel.SetupAllProperties();
            
            var odbcDatabaseDesignerViewModel = new ODBCDatabaseDesignerViewModel(modelItem,mockDbServiceModel.Object);
            
            scenarioContext.Add("viewModel", odbcDatabaseDesignerViewModel);
            scenarioContext.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            scenarioContext.Add("mockDatabaseInputViewModel", mockDatabaseInputViewModel);
            scenarioContext.Add("mockDbServiceModel", mockDbServiceModel);
        }

        [Then(@"Test Connector and Calculate Outputs window is open")]
        public void ThenTestConnectorAndCalculateOutputsWindowIsOpen()
        {
            scenarioContext.Pending();
        }

        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string p0, string p1)
        {
            scenarioContext.Pending();
        }


        [Given(@"ODBC Source is Enabled")]
        public void GivenSourceIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.ActionRegion.IsEnabled);
        }

        ODBCDatabaseDesignerViewModel GetViewModel()
        {
            return scenarioContext.Get<ODBCDatabaseDesignerViewModel>("viewModel");
        }

        Mock<IManageServiceInputViewModel> GetInputViewModel()
        {
            return scenarioContext.Get<Mock<IManageServiceInputViewModel>>("mockServiceInputViewModel");
        }

        Mock<IDbServiceModel> GetDbServiceModel()
        {
            return scenarioContext.Get<Mock<IDbServiceModel>>("mockDbServiceModel");
        }

        [Given(@"Action iz Disable")]
        public void GivenActionIsDisabled()
        {
            var viewModel = GetViewModel();
            Assert.IsFalse(viewModel.ActionRegion.IsEnabled);
        }

        [Given(@"ODBC Inputs are Disabled")]
        [When(@"ODBC Inputs are Disabled")]
        [Then(@"ODBC Inputs are Disabled")]
        public void GivenInputsIsDisabled()
        {
            var viewModel = GetViewModel();
            var hasNoInputs = viewModel.InputArea == null || viewModel.InputArea.Inputs.Count == 0 || !viewModel.InputArea.IsEnabled;
            Assert.IsTrue(hasNoInputs);
        }

        [Given(@"ODBC Outputs are Disabled")]
        [When(@"ODBC Outputs are Disabled")]
        [Then(@"ODBC Outputs are Disabled")]
        public void GivenOutputsIsDisabled()
        {
            var viewModel = GetViewModel();
            var hasNoOutputs = viewModel.OutputsRegion == null || viewModel.OutputsRegion.Outputs.Count == 0 || !viewModel.OutputsRegion.IsEnabled;
            Assert.IsTrue(hasNoOutputs);
        }

        [When(@"ODBC Action is changed to ""(.*)""")]
        public void WhenActionIsChangedTo(string procName)
        {
            _importOrderAction.Inputs = new List<IServiceInput> { new ServiceInput("ProductId", "") };
            GetViewModel().ActionRegion.SelectedAction = _importOrderAction;
        }

        [When(@"ODBC Recordset Name is changed to ""(.*)""")]
        public void WhenRecordsetNameIsChangedTo(string recSetName)
        {
            GetViewModel().OutputsRegion.RecordsetName = recSetName;
        }

        [Given(@"ODBC Action is ""(.*)""")]
        public void GivenActionIs(string actionName)
        {
            var selectedProcedure = GetViewModel().CommandText = "dbo.Pr_CitiesGetCountries";
            Assert.IsNotNull(selectedProcedure);
            Assert.AreEqual(actionName, selectedProcedure);
        }

        [Given(@"ODBC Action is Enabled")]
        [Then(@"ODBC Action is Enabled")]
        [When(@"ODBC Action is Enabled")]
        public void ThenActionIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.ActionRegion.IsEnabled);
        }

        [Given(@"ODBC Inputs are Enabled")]
        [When(@"ODBC Inputs are Enabled")]
        [Then(@"ODBC Inputs are Enabled")]
        public void ThenInputsIsEnabled()
        {
            var viewModel = GetViewModel();
            var hasInputs = viewModel.InputArea.Inputs != null || viewModel.InputArea.IsEnabled;
            Assert.IsTrue(hasInputs);
        }

        [Then(@"Mapping iz Enable")]
        [Given(@"Mapping iz Enable")]
        public void GivenMappingIsEnabled()
        {
            scenarioContext.Pending();
        }

        [Given(@"Validate ODBC is Enabled")]
        [When(@"Validate ODBC is Enabled")]
        [Then(@"Validate ODBC is Enabled")]
        public void ThenValidateIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.TestInputCommand.CanExecute(null));
        }

        [When(@"ODBC Inputs appear as")]
        [Then(@"ODBC Inputs appear as")]
        [Given(@"ODBC Inputs appear as")]
        public void ThenInputsAppearAs(Table table)
        {
            var viewModel = GetViewModel();
            int rowNum = 0;
            foreach (var row in table.Rows)
            {
                var inputValue = row["Input"];
                var value = row["Value"];
                var serviceInputs = viewModel.InputArea.Inputs.ToList();
                Assert.IsTrue(serviceInputs.Count > 0, "Number of inputs to ODBC tool is less than or equal 0.");
                var serviceInput = serviceInputs[rowNum];
                Assert.AreEqual(inputValue, serviceInput.Name);
                Assert.AreEqual(value, serviceInput.Value);
                rowNum++;
            }
        }

        [When(@"I Select GreenPoint as ODBC Source")]
        public void WhenISelectAsSource()
        {
            _importOrderAction = new DbAction();
            _importOrderAction.Name = "Command";
            _importOrderAction.Inputs = new List<IServiceInput> { new ServiceInput("EID", "") };
            GetDbServiceModel().Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction> { _importOrderAction });
            GetViewModel().SourceRegion.SelectedSource = _greenPointSource;
        }

        [Given(@"ODBC Source is localODBCTest")]
        public void GivenSourceIs()
        {
            _importOrderAction = new DbAction();
            _importOrderAction.Name = "Command";
            _importOrderAction.Inputs = new List<IServiceInput> { new ServiceInput("Prefix", "[[Prefix]]") };
            GetDbServiceModel().Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction> { _importOrderAction });
            GetViewModel().SourceRegion.SelectedSource = _testingDbSource;
        }

        [When(@"I select ""(.*)"" as the ODBC action")]
        public void WhenISelectAsTheAction(string actionName)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("Column1"));
            dataTable.ImportRow(dataTable.LoadDataRow(new object[] { 1 }, true));
            GetDbServiceModel().Setup(model => model.TestService(It.IsAny<IDatabaseService>())).Returns(dataTable);
            GetViewModel().ActionRegion.SelectedAction = _importOrderAction;
            GetViewModel().CommandText = actionName;
        }

        [When(@"I click Test ODBC")]
        public void WhenIClickTest()
        {
            var viewModel = GetViewModel();
            var testCommand = viewModel.ManageServiceInputViewModel.TestAction;            
        }

        [When(@"I click OK on ODBC Test")]
        public void WhenIClickOK()
        {
            GetViewModel().ManageServiceInputViewModel.OkAction = new Mock<Action>().Object;
            GetViewModel().ManageServiceInputViewModel.OkAction();
        }

        [When(@"""(.*)"" is selected az the data source")]
        public void WhenIsSelectedAsTheDataSource(string p0)
        {
            scenarioContext.Pending();
        }

        [When(@"testing the action fails")]
        public void WhenTestingTheActionFails()
        {
            scenarioContext.Pending();
        }

        [When(@"I click Validate ODBC")]
        public void WhenIClickValidate()
        {
            GetViewModel().TestInputCommand.Execute(null);
        }

        [Then(@"Test ODBC Inputs appear as")]
        public void ThenTestInputsAppearAs(Table table)
        {
            int rowNum = 0;
            var viewModel = GetViewModel();
            foreach (var row in table.Rows)
            {
                var inputValue = row["EID"];
                var serviceInputs = viewModel.ManageServiceInputViewModel.Inputs.ToList();
                var serviceInput = serviceInputs[rowNum];
                serviceInput.Value = inputValue;
                rowNum++;
            }
        }

        [Then(@"Test ODBC Connector Calculate Outputs outputs appear as")]
        public void ThenTestConnectorAndCalculateOutputsOutputsAppearAs(Table table)
        {
            CheckODBCToolOutputs(table);
        }

        [Then(@"ODBC Outputs appear as")]
        public void ThenOutputsAppearAs(Table table)
        {
            CheckODBCToolOutputs(table);
        }

        private void CheckODBCToolOutputs(Table table)
        {
            var vm = GetViewModel();
            Assert.AreEqual(table.Rows.Count, vm.OutputsRegion.Outputs.Count, "Wrong number of outputs in ODBC view model.");
            if (table.Rows.Count == 0)
            {
                if (vm.OutputsRegion.Outputs != null)
                    Assert.AreEqual(vm.OutputsRegion.Outputs.Count, 0);
            }
            else
            {
                var matched = table.Rows.Zip(vm.OutputsRegion.Outputs, (a, b) => new Tuple<TableRow, IServiceOutputMapping>(a, b));
                foreach (var a in matched)
                {
                    Assert.AreEqual(a.Item1.Keys.FirstOrDefault(), a.Item2.MappedFrom);
                }
            }
        }

        [Then(@"ODBC Recordset Name equals ""(.*)""")]
        public void ThenRecordsetNameEquals(string recsetName)
        {
            Assert.AreEqual(recsetName, GetViewModel().OutputsRegion.RecordsetName);
        }

        [When(@"ODBC Source is changed to GreenPoint")]
        public void WhenSourceIsChangedFromTo(string sourceName)
        {
            GetViewModel().SourceRegion.SelectedSource = _greenPointSource;
        }
        
        [Given(@"I open New Workflow")]
        public void GivenIOpenNewWorkflow()
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
        }
    }
}