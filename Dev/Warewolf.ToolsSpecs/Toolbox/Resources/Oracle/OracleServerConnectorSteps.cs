using Dev2.Activities.Designers2.Oracle;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Dev2.Common.Interfaces.Core;
using Dev2.Threading;
using TechTalk.SpecFlow;
using Warewolf.Core;

namespace Dev2.Activities.Specs.Toolbox.Resources
{
    [Binding]
    public class OracleServerConnectorSteps
    {
        private readonly ScenarioContext scenarioContext;

        public OracleServerConnectorSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

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

        [Given(@"Source iss ""(.*)""")]
        public void GivenSourceIs(string sourceName)
        {
            var selectedSource = GetViewModel().SourceRegion.SelectedSource;
            Assert.IsNotNull(selectedSource);
            Assert.AreEqual(sourceName, selectedSource.Name);
        }

        [Given(@"Source is Enable")]
        public void GivenSourceIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.SourceRegion.IsEnabled);
        }

        [Given(@"I open New oracleDb Workflow")]
        public void GivenIOpenNewOracleDbWorkflow()
        {
            var oracleServerActivity = new DsfOracleDatabaseActivity();
            var modelItem = ModelItemUtils.CreateModelItem(oracleServerActivity);

            var mockInputArea = new Mock<IGenerateInputArea>();
            var mockOutputArea = new Mock<IGenerateOutputArea>();
            var mockDatabaseInputViewModel = new Mock<IManageDatabaseInputViewModel>();
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

        OracleDatabaseDesignerViewModel GetViewModel()
        {
            return scenarioContext.Get<OracleDatabaseDesignerViewModel>("viewModel");
        }

        Mock<IManageDatabaseInputViewModel> GetOutputViewModel()
        {
            return scenarioContext.Get<Mock<IManageDatabaseInputViewModel>>("mockDatabaseInputViewModel");
        }

        Mock<IManageServiceInputViewModel> GetInputViewModel()
        {
            return scenarioContext.Get<Mock<IManageServiceInputViewModel>>("mockServiceInputViewModel");
        }

        Mock<IDbServiceModel> GetDbServiceModel()
        {
            return scenarioContext.Get<Mock<IDbServiceModel>>("mockDbServiceModel");
        }

        [Given(@"Action iss ""(.*)""")]
        public void GivenActionIs(string actionName)
        {
            var selectedProcedure = GetViewModel().ActionRegion.SelectedAction;
            Assert.IsNotNull(selectedProcedure);
            Assert.AreEqual(actionName, selectedProcedure.Name);
        }

        [Given(@"Action is dbo\.Pr_CitiesGetCountries")]
        public void GivenActionIsDbo_Pr_CitiesGetCountries()
        {
            scenarioContext.Pending();
        }

        [Given(@"Action is Disable")]
        public void GivenActionIsDisabled()
        {
            var viewModel = GetViewModel();
            Assert.IsFalse(viewModel.ActionRegion.IsEnabled);
        }

        [Given(@"Inputs is Disable")]
        [When(@"Inputs is Disable")]
        [Then(@"Inputs is Disable")]
        public void GivenInputsIsDisabled()
        {
            var viewModel = GetViewModel();
            var hasNoInputs = viewModel.InputArea.Inputs == null || viewModel.InputArea.Inputs.Count == 0 || !viewModel.InputArea.IsEnabled;
            Assert.IsTrue(hasNoInputs);
        }

        [Given(@"Outputs is Disable")]
        [When(@"Outputs is Disable")]
        [Then(@"Outputs is Disable")]
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

        [When(@"Source iz changed from to ""(.*)""")]
        public void WhenSourceIsChangedFromTo(string sourceName)
        {
            if (sourceName == "GreenPoint")
            {
                var viewModel = GetViewModel();
                viewModel.SourceRegion.SelectedSource = _greenPointSource;
            }
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
            scenarioContext.Pending();
        }

        [Then(@"Mapping is Enable")]
        [Given(@"Mapping is Enable")]
        public void GivenMappingIsEnabled()
        {
            scenarioContext.Pending();
        }

        [When(@"Action iz changed from to ""(.*)""")]
        public void WhenActionIsChangedFromTo(string procName)
        {
            if (procName == "dbo.ImportOrder")
            {
                GetViewModel().ActionRegion.SelectedAction = _importOrderAction;
            }
        }

        [When(@"I click ""(.*)""")]
        public void WhenIClick(string p0)
        {
            scenarioContext.Pending();
        }
        
        [When(@"Recordset Name iz changed to ""(.*)""")]
        public void WhenRecordsetNameIsChangedTo(string recSetName)
        {
            GetViewModel().OutputsRegion.RecordsetName = recSetName;
        }

        [When(@"Inputs appear az")]
        [Then(@"Inputs appear az")]
        [Given(@"Inputs appear az")]
        public void ThenInputsAppearAs(Table table)
        {
            var viewModel = GetViewModel();
            int rowNum = 0;
            foreach (var row in table.Rows)
            {
                var inputValue = row["Input"];
                var value = row["Value"];
                var serviceInputs = viewModel.InputArea.Inputs.ToList();
                var serviceInput = serviceInputs[rowNum];
                Assert.AreEqual(inputValue, serviceInput.Name);
                Assert.AreEqual(value, serviceInput.Value);
                rowNum++;
            }
        }        

        [When(@"I Selected ""(.*)"" as Source")]
        public void WhenISelectAsSource(string sourceName)
        {
            if (sourceName == "GreenPoint")
            {
                _importOrderAction = new DbAction();
                _importOrderAction.Name = "HR.TESTPROC9";
                _importOrderAction.Inputs = new List<IServiceInput> { new ServiceInput("EID", "") };
                GetDbServiceModel().Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction> { _importOrderAction });
                GetViewModel().SourceRegion.SelectedSource = _greenPointSource;
            }
        }

        [When(@"I selected ""(.*)"" az the action")]
        public void WhenISelectAsTheAction(string actionName)
        {
            if (actionName == "HR.TESTPROC9")
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add(new DataColumn("Column1"));
                dataTable.ImportRow(dataTable.LoadDataRow(new object[] { 1 }, true));
                GetDbServiceModel().Setup(model => model.TestService(It.IsAny<IDatabaseService>())).Returns(dataTable);
                GetViewModel().ActionRegion.SelectedAction = _importOrderAction;
            }
        }

        [When(@"I click Testz")]
        public void WhenIClickTest()
        {            
            var testCommand = GetViewModel().ManageServiceInputViewModel.TestAction;
            testCommand();
        }

        [When(@"I click OKay")]
        public void WhenIClickOK()
        {
            GetViewModel().ManageServiceInputViewModel.OkAction();
        }

        [When(@"I click Validat")]
        public void WhenIClickValidate()
        {
            GetViewModel().TestInputCommand.Execute(null);
        }

        [When(@"Test Inputs appear az")]
        [Then(@"Test Inputs appear az")]
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

        [Then(@"Test Connector and Calculate Outputs outputs appear az")]
        public void ThenTestConnectorAndCalculateOutputsOutputsAppearAs(Table table)
        {
            var vm = GetViewModel();
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
                    Assert.AreEqual(a.Item1[0], a.Item2.MappedFrom);
                    Assert.AreEqual(a.Item1[1], a.Item2.MappedTo);

                }
            }
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

            scenarioContext.Add("viewModel", oracleDatabaseDesignerViewModel);
            scenarioContext.Add("mockDatabaseInputViewModel", mockDatabaseInputViewModel);
            scenarioContext.Add("mockDbServiceModel", mockDbServiceModel);
        }

        [Then(@"Outputs appear az")]
        public void ThenOutputsAppearAs(Table table)
        {
            var vm = GetViewModel();
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
                    Assert.AreEqual(a.Item1[0], a.Item2.MappedFrom);
                    Assert.AreEqual(a.Item1[1], a.Item2.MappedTo);
                }
            }
            var outputMappings = vm.OutputsRegion.Outputs;
            Assert.IsNotNull(outputMappings);
        }

        [Then(@"Recordset Name equalz ""(.*)""")]
        public void ThenRecordsetNameEquals(string recsetName)
        {
            if(string.IsNullOrEmpty(recsetName))            
                Assert.AreEqual(recsetName, GetViewModel().OutputsRegion.RecordsetName);
        }

        #region Private Methods

        void AddScenarioContext(OracleDatabaseDesignerViewModel oracleServerDesignerViewModel, Mock<IManageDatabaseInputViewModel> mockDatabaseInputViewModel, Mock<IDbServiceModel> mockDbServiceModel)
        {
            scenarioContext.Add("viewModel", oracleServerDesignerViewModel);
            scenarioContext.Add("mockDatabaseInputViewModel", mockDatabaseInputViewModel);
            scenarioContext.Add("mockDbServiceModel", mockDbServiceModel);
        }

        #endregion
    }
}