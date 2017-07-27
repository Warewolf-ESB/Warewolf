using Dev2.Activities.Designers2.Oracle;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;
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
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using TechTalk.SpecFlow;
using Warewolf.Core;
using Warewolf.Tools.Specs.Toolbox.Database;

namespace Dev2.Activities.Specs.Toolbox.Resources
{
    [Binding]
    public class OracleServerConnectorSteps
    {
        private DbSourceDefinition _greenPointSource;
        private DbAction _importOrderAction;
        private DbSourceDefinition _testingDbSource;
        private DbAction _getCountriesAction;

        [Given(@"I drag a Oracle Server database connector")]
        public void GivenIDragAOracleServerDatabaseConnector()
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
        }

        [Given(@"Oracle Source is ""(.*)""")]
        public void GivenSourceIs(string sourceName)
        {
            var selectedSource = GetViewModel().SourceRegion.SelectedSource;
            Assert.IsNotNull(selectedSource);
            Assert.AreEqual(sourceName, selectedSource.Name);
        }

        [Given(@"Oracle Source is Enabled")]
        public void GivenSourceIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.SourceRegion.IsEnabled);
        }

        OracleDatabaseDesignerViewModel GetViewModel()
        {
            return ScenarioContext.Current.Get<OracleDatabaseDesignerViewModel>("viewModel");
        }

        Mock<IManageDatabaseInputViewModel> GetOutputViewModel()
        {
            return ScenarioContext.Current.Get<Mock<IManageDatabaseInputViewModel>>("mockDatabaseInputViewModel");
        }

        Mock<IManageServiceInputViewModel> GetInputViewModel()
        {
            return ScenarioContext.Current.Get<Mock<IManageServiceInputViewModel>>("mockServiceInputViewModel");
        }

        Mock<IDbServiceModel> GetDbServiceModel()
        {
            return ScenarioContext.Current.Get<Mock<IDbServiceModel>>("mockDbServiceModel");
        }

        [Given(@"Oracle Action is ""(.*)""")]
        public void GivenActionIs(string actionName)
        {
            var selectedProcedure = GetViewModel().ActionRegion.SelectedAction;
            Assert.IsNotNull(selectedProcedure);
            Assert.AreEqual(actionName, selectedProcedure.Name);
        }

        [Given(@"Action is dbo\.Pr_CitiesGetCountries")]
        public void GivenActionIsDbo_Pr_CitiesGetCountries()
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"Oracle Action is Disabled")]
        public void GivenActionIsDisabled()
        {
            var viewModel = GetViewModel();
            Assert.IsFalse(viewModel.ActionRegion.IsEnabled);
        }

        [Given(@"Oracle Inputs are Disabled")]
        [When(@"Oracle Inputs are Disabled")]
        [Then(@"Oracle Inputs are Disabled")]
        public void GivenInputsIsDisabled()
        {
            var viewModel = GetViewModel();
            var hasNoInputs = viewModel.InputArea.Inputs == null || viewModel.InputArea.Inputs.Count == 0 || !viewModel.InputArea.IsEnabled;
            Assert.IsTrue(hasNoInputs);
        }

        [Given(@"Oracle Outputs are Disabled")]
        [When(@"Oracle Outputs are Disabled")]
        [Then(@"Oracle Outputs are Disabled")]
        public void GivenOutputsIsDisabled()
        {
            var viewModel = GetViewModel();
            var hasNoOutputs = viewModel.OutputsRegion.Outputs == null || viewModel.OutputsRegion.Outputs.Count == 0 || !viewModel.OutputsRegion.IsEnabled;
            Assert.IsTrue(hasNoOutputs);
        }

        [Given(@"Validate is Disable")]
        [When(@"Validate is Disable")]
        [Then(@"Validate is Disable")]
        public void GivenValidateIsDisabled()
        {
            var viewModel = GetViewModel();
            Assert.IsFalse(viewModel.TestInputCommand.CanExecute(null));
        }

        [When(@"Oracle Source is changed from to GreenPoint")]
        public void WhenSourceIsChangedFromTo()
        {
            var viewModel = GetViewModel();
            viewModel.SourceRegion.SelectedSource = _greenPointSource;
        }

        [Given(@"Action is Enable")]
        [Then(@"Action is Enable")]
        [When(@"Action is Enable")]
        public void ThenActionIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.ActionRegion.IsEnabled);
        }

        [Given(@"Inputs is Enable")]
        [When(@"Inputs is Enable")]
        [Then(@"Inputs is Enable")]
        public void ThenInputsIsEnabled()
        {
            var viewModel = GetViewModel();
            var hasInputs = viewModel.InputArea.Inputs != null || viewModel.InputArea.IsEnabled;
            Assert.IsTrue(hasInputs);
        }

        [Given(@"Validate is Enable")]
        [When(@"Validate is Enable")]
        [Then(@"Validate is Enable")]
        public void ThenValidateIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.TestInputCommand.CanExecute(null));
        }

        [Given(@"Validate is Enable")]
        public void GivenValidateIsEnabled()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Mapping is Enable")]
        [Given(@"Mapping is Enable")]
        public void GivenMappingIsEnabled()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"Oracle Action is changed from to dbo.ImportOrder")]
        public void WhenActionIsChangedFromTo()
        {
            GetViewModel().ActionRegion.SelectedAction = _importOrderAction;
        }

        [When(@"I click ""(.*)""")]
        public void WhenIClick(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"Oracle Recordset Name is changed to ""(.*)""")]
        public void WhenRecordsetNameIsChangedTo(string recSetName)
        {
            GetViewModel().OutputsRegion.RecordsetName = recSetName;
        }     

        [When(@"I Selected GreenPoint as Source")]
        public void WhenISelectAsSource()
        {
            _importOrderAction = new DbAction();
            _importOrderAction.Name = "HR.TESTPROC9";
            _importOrderAction.Inputs = new List<IServiceInput> { new ServiceInput("EID", "") };
            GetDbServiceModel().Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction> { _importOrderAction });
            GetViewModel().SourceRegion.SelectedSource = _greenPointSource;
        }

        [When(@"I select HR.TESTPROC9 as the Oracle action")]
        public void WhenISelectAsTheAction()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("Column1"));
            dataTable.ImportRow(dataTable.LoadDataRow(new object[] { 1 }, true));
            GetDbServiceModel().Setup(model => model.TestService(It.IsAny<IDatabaseService>())).Returns(dataTable);
            var oracleDatabaseDesignerViewModel = GetViewModel();
            oracleDatabaseDesignerViewModel.ActionRegion.SelectedAction = _importOrderAction;
        }

        [When(@"I click Oracle Tests")]
        public void WhenIClickTest()
        {            
            var testCommand = GetViewModel().ManageServiceInputViewModel.TestAction;
            testCommand();
        }

        [When(@"I click Oracle OK")]
        public void WhenIClickOK()
        {
            GetViewModel().ManageServiceInputViewModel.OkAction();
        }

        [When(@"I click Oracle Validate")]
        public void WhenIClickValidate()
        {
            GetViewModel().TestInputCommand.Execute(null);
        }

        [Given(@"Test Oracle Inputs appear as")]
        [Then(@"Test Oracle Inputs appear as")]
        public void ThenTestInputsAppearAs(Table table)
        {
            int rowNum = 0;
            var viewModel = GetViewModel();
            foreach (var row in table.Rows)
            {
                var inputValue = row["EID"];
                var serviceInputs = viewModel.InputArea.Inputs.ToList();
                var serviceInput = serviceInputs[rowNum];
                serviceInput.Value = inputValue;
                rowNum++;
            }
        }
        
        [Given(@"Oracle Inputs appear as")]
        [Then(@"Oracle Inputs appear as")]
        public void ThenInputsAppearAs(Table table)
        {
            var vm = GetViewModel();
            DatabaseToolsSteps.AssertAgainstServiceInputs(table, vm.InputArea.Inputs);
        }

        [Given(@"I open workflow with Oracle connector")]
        public void GivenIOpenWolf()
        {
            var sourceId = Guid.NewGuid();
            var inputs = new List<IServiceInput> { new ServiceInput("Prefix", "[[Prefix]]") };
            var outputs = new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping("CountryID", "CountryID", "dbo_Pr_CitiesGetCountries"),
                new ServiceOutputMapping("Description", "Description", "dbo_Pr_CitiesGetCountries")
            };
            var oracleServerActivity = new DsfOracleDatabaseActivity
            {
                SourceId = sourceId,
                ProcedureName = "dbo.Pr_CitiesGetCountries",
                Inputs = inputs,
                Outputs = outputs
            };
            var modelItem = ModelItemUtils.CreateModelItem(oracleServerActivity);
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
                Type = enSourceType.Oracle
            };

            _testingDbSource = new DbSourceDefinition
            {
                Name = "testingDBSrc",
                Type = enSourceType.Oracle,
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
            mockDatabaseInputViewModel.SetupAllProperties();
            var oracleDatabaseDesignerViewModel = new OracleDatabaseDesignerViewModel(modelItem, mockDbServiceModel.Object, new SynchronousAsyncWorker());

            ScenarioContext.Current.Add("viewModel", oracleDatabaseDesignerViewModel);
            ScenarioContext.Current.Add("mockDatabaseInputViewModel", mockDatabaseInputViewModel);
            ScenarioContext.Current.Add("mockDbServiceModel", mockDbServiceModel);
        }

        [Given(@"I open New Workflow containing an Oracle Connector")]
        public void GivenIOpenNewOracleDbWorkflow()
        {
            var oracleServerActivity = new DsfOracleDatabaseActivity();
            var modelItem = ModelItemUtils.CreateModelItem(oracleServerActivity);

            var mockInputArea = new Mock<IGenerateInputArea>();
            var mockOutputArea = new Mock<IGenerateOutputArea>();
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
                Type = enSourceType.Oracle,
                ServerName = "Localhost",
                UserName = "system",
                Password = "P@ssword123",
                AuthenticationType = Runtime.ServiceModel.Data.AuthenticationType.User
            };

            var dbSources = new ObservableCollection<IDbSource> { _greenPointSource };
            mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(dbSources);

            var mockAction = new Mock<Action>(MockBehavior.Default);

            mockDatabaseInputViewModel.SetupGet(model => model.InputArea).Returns(mockInputArea.Object);
            mockDatabaseInputViewModel.SetupGet(model => model.OutputArea).Returns(mockOutputArea.Object);
            mockDatabaseInputViewModel.Setup(model => model.TestAction).Returns(mockAction.Object);
            mockDatabaseInputViewModel.Setup(model => model.OkAction).Returns(mockAction.Object);

            var oracleServerDesignerViewModel = new OracleDatabaseDesignerViewModel(modelItem, mockDbServiceModel.Object, new SynchronousAsyncWorker());
            oracleServerDesignerViewModel.ManageServiceInputViewModel = mockDatabaseInputViewModel.Object;

            AddScenarioContext(oracleServerDesignerViewModel, mockDatabaseInputViewModel, mockDbServiceModel);
        }

        [Then(@"Oracle Outputs appear as")]
        public void ThenOutputsAppearAs(Table table)
        {
            var vm = GetViewModel();
            var outputMappings = vm.OutputsRegion.Outputs;
            Assert.IsNotNull(outputMappings);
            //TODO:Assert.AreEqual(table.Rows.Count, vm.OutputsRegion.Outputs.Count, "Wrong number of outputs in Oracle view model.");
            if (table.Rows.Count == 0)
            {
                Assert.IsNotNull(vm.OutputsRegion.Outputs != null);
                Assert.AreEqual(vm.OutputsRegion.Outputs.Count, 0);
            }
            else
            {
                var matched = table.Rows.Zip(vm.OutputsRegion.Outputs, (a, b) => new Tuple<TableRow, IServiceOutputMapping>(a, b));
                foreach (var a in matched)
                {
                    Assert.AreEqual(a.Item1[0], a.Item2.MappedFrom);
                    Assert.AreEqual(a.Item1[1], a.Item2.MappedTo);
                }
            }
        }

        [Then(@"Oracle Recordset Name equals ""(.*)""")]
        public void ThenRecordsetNameEquals(string recsetName)
        {
            var oracleDatabaseDesignerViewModel = GetViewModel();
            //TODO:Assert.AreEqual(recsetName, oracleDatabaseDesignerViewModel.OutputsRegion.RecordsetName);
        }

        #region Private Methods

        void AddScenarioContext(OracleDatabaseDesignerViewModel oracleServerDesignerViewModel, Mock<IManageDatabaseInputViewModel> mockDatabaseInputViewModel, Mock<IDbServiceModel> mockDbServiceModel)
        {
            ScenarioContext.Current.Add("viewModel", oracleServerDesignerViewModel);
            ScenarioContext.Current.Add("mockDatabaseInputViewModel", mockDatabaseInputViewModel);
            ScenarioContext.Current.Add("mockDbServiceModel", mockDbServiceModel);
        }

        #endregion
    }
}