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
using Warewolf.Tools.Specs.Toolbox.Database;

namespace Dev2.Activities.Specs.Toolbox.Resources
{
    [Binding]
    public class ODBCServerConnectorSteps
    {
        private DbSourceDefinition _greenPointSource;
        private DbAction _importOrderAction;
        private DbSourceDefinition _testingDbSource;
        private DbAction _getCountriesAction;

        [Given(@"I open workflow with ODBC connector")]
        public void GivenIOpenWorkflowWithODBC()
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

            ScenarioContext.Current.Add("ViewModel", odbcDatabaseDesignerViewModel);
            ScenarioContext.Current.Add("mockDatabaseInputViewModel", mockDatabaseInputViewModel);
            ScenarioContext.Current.Add("mockDbServiceModel", mockDbServiceModel);
        }

        [Then(@"Test Connector and Calculate Outputs window is open")]
        public void ThenTestConnectorAndCalculateOutputsWindowIsOpen()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }


        [Given(@"ODBC Source is Enabled")]
        public void GivenSourceIsEnabled()
        {
            var viewModel = GetViewModel<ODBCDatabaseDesignerViewModel>();
            Assert.IsTrue(viewModel.ActionRegion.IsEnabled);
        }

        T GetViewModel<T>()
        {
            return ScenarioContext.Current.Get<T>("ViewModel");
        }

        Mock<IManageServiceInputViewModel> GetInputViewModel()
        {
            return ScenarioContext.Current.Get<Mock<IManageServiceInputViewModel>>("mockServiceInputViewModel");
        }

        Mock<IDbServiceModel> GetDbServiceModel()
        {
            return ScenarioContext.Current.Get<Mock<IDbServiceModel>>("mockDbServiceModel");
        }

        [Given(@"Action iz Disable")]
        public void GivenActionIsDisabled()
        {
            var viewModel = GetViewModel<ODBCDatabaseDesignerViewModel>();
            Assert.IsFalse(viewModel.ActionRegion.IsEnabled);
        }

        [Given(@"ODBC Inputs are Disabled")]
        [When(@"ODBC Inputs are Disabled")]
        [Then(@"ODBC Inputs are Disabled")]
        public void GivenInputsIsDisabled()
        {
            var viewModel = GetViewModel<ODBCDatabaseDesignerViewModel>();
            var hasNoInputs = viewModel.InputArea == null || viewModel.InputArea.Inputs.Count == 0 || !viewModel.InputArea.IsEnabled;
            Assert.IsTrue(hasNoInputs);
        }

        [Given(@"ODBC Outputs are Disabled")]
        [When(@"ODBC Outputs are Disabled")]
        [Then(@"ODBC Outputs are Disabled")]
        public void GivenOutputsIsDisabled()
        {
            var viewModel = GetViewModel<ODBCDatabaseDesignerViewModel>();
            var hasNoOutputs = viewModel.OutputsRegion == null || viewModel.OutputsRegion.Outputs.Count == 0 || !viewModel.OutputsRegion.IsEnabled;
            Assert.IsTrue(hasNoOutputs);
        }

        [When(@"ODBC Action is changed to ""(.*)""")]
        public void WhenActionIsChangedTo(string procName)
        {
            _importOrderAction.Inputs = new List<IServiceInput> { new ServiceInput("ProductId", "") };
            GetViewModel<ODBCDatabaseDesignerViewModel>().ActionRegion.SelectedAction = _importOrderAction;
        }

        [When(@"ODBC Recordset Name is changed to ""(.*)""")]
        public void WhenRecordsetNameIsChangedTo(string recSetName)
        {
            GetViewModel<ODBCDatabaseDesignerViewModel>().OutputsRegion.RecordsetName = recSetName;
        }

        [Given(@"ODBC Action is ""(.*)""")]
        public void GivenActionIs(string actionName)
        {
            var selectedProcedure = GetViewModel<ODBCDatabaseDesignerViewModel>().CommandText = "dbo.Pr_CitiesGetCountries";
            Assert.IsNotNull(selectedProcedure);
            Assert.AreEqual(actionName, selectedProcedure);
        }

        [Given(@"ODBC Action is Enabled")]
        [Then(@"ODBC Action is Enabled")]
        [When(@"ODBC Action is Enabled")]
        public void ThenActionIsEnabled()
        {
            var viewModel = GetViewModel<ODBCDatabaseDesignerViewModel>();
            Assert.IsTrue(viewModel.ActionRegion.IsEnabled);
        }

        [Given(@"ODBC Inputs are Enabled")]
        [When(@"ODBC Inputs are Enabled")]
        [Then(@"ODBC Inputs are Enabled")]
        public void ThenInputsIsEnabled()
        {
            var viewModel = GetViewModel<ODBCDatabaseDesignerViewModel>();
            var hasInputs = viewModel.InputArea.Inputs != null || viewModel.InputArea.IsEnabled;
            Assert.IsTrue(hasInputs);
        }

        [Then(@"Mapping iz Enable")]
        [Given(@"Mapping iz Enable")]
        public void GivenMappingIsEnabled()
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"Validate ODBC is Enabled")]
        [When(@"Validate ODBC is Enabled")]
        [Then(@"Validate ODBC is Enabled")]
        public void ThenValidateIsEnabled()
        {
            var viewModel = GetViewModel<ODBCDatabaseDesignerViewModel>();
            Assert.IsTrue(viewModel.TestInputCommand.CanExecute(null));
        }

        [When(@"ODBC Inputs appear as")]
        [Then(@"ODBC Inputs appear as")]
        [Given(@"ODBC Inputs appear as")]
        public void ThenInputsAppearAs(Table table)
        {
            var viewModel = ScenarioContext.Current.Get<ODBCDatabaseDesignerViewModel>("ViewModel");
            int rowNum = 0;
            foreach (var row in table.Rows)
            {
                var inputValue = row["Input"];
                var value = row["Value"];
                var serviceInputs = viewModel.InputArea.Inputs.ToList();
                if (serviceInputs.Count > 0)
                {
                    var serviceInput = serviceInputs[rowNum];

                    Assert.AreEqual(inputValue, serviceInput.Name);
                    Assert.AreEqual(value, serviceInput.Value);
                }
                rowNum++;
            }
            //var viewModel = GetViewModel<ODBCDatabaseDesignerViewModel>();
            //DatabaseToolsSteps.AssertAgainstServiceInputs(table, viewModel.ToModel().Inputs);
        }        

        [When(@"I Select GreenPoint as ODBC Source")]
        public void WhenISelectAsSource()
        {
            _importOrderAction = new DbAction();
            _importOrderAction.Name = "Command";
            _importOrderAction.Inputs = new List<IServiceInput> { new ServiceInput("EID", "") };
            GetDbServiceModel().Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction> { _importOrderAction });
            GetViewModel<ODBCDatabaseDesignerViewModel>().SourceRegion.SelectedSource = _greenPointSource;
        }

        [Given(@"ODBC Source is localODBCTest")]
        public void GivenSourceIs()
        {
            _importOrderAction = new DbAction();
            _importOrderAction.Name = "Command";
            _importOrderAction.Inputs = new List<IServiceInput> { new ServiceInput("Prefix", "[[Prefix]]") };
            GetDbServiceModel().Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction> { _importOrderAction });
            GetViewModel<ODBCDatabaseDesignerViewModel>().SourceRegion.SelectedSource = _testingDbSource;
        }

        [When(@"I select ""(.*)"" as the ODBC action")]
        public void WhenISelectAsTheAction(string actionName)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("Column1"));
            dataTable.ImportRow(dataTable.LoadDataRow(new object[] { 1 }, true));
            GetDbServiceModel().Setup(model => model.TestService(It.IsAny<IDatabaseService>())).Returns(dataTable);
            GetViewModel<ODBCDatabaseDesignerViewModel>().ActionRegion.SelectedAction = _importOrderAction;
            GetViewModel<ODBCDatabaseDesignerViewModel>().CommandText = actionName;
        }

        [When(@"I click Test ODBC")]
        public void WhenIClickTest()
        {
            var viewModel = GetViewModel<ODBCDatabaseDesignerViewModel>();
            var testCommand = viewModel.ManageServiceInputViewModel.TestAction;
        }

        [When(@"I click OK on ODBC Test")]
        public void WhenIClickOK()
        {
            GetViewModel<ODBCDatabaseDesignerViewModel>().ManageServiceInputViewModel.OkAction = new Mock<Action>().Object;
            GetViewModel<ODBCDatabaseDesignerViewModel>().ManageServiceInputViewModel.OkAction();
        }

        [When(@"""(.*)"" is selected az the data source")]
        public void WhenIsSelectedAsTheDataSource(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"testing the action fails")]
        public void WhenTestingTheActionFails()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I click Validate ODBC")]
        public void WhenIClickValidate()
        {
            GetViewModel<ODBCDatabaseDesignerViewModel>().TestInputCommand.Execute(null);
        }

        [Then(@"Test ODBC Inputs appear as")]
        public void ThenTestInputsAppearAs(Table table)
        {
            int rowNum = 0;
            var viewModel = GetViewModel<ODBCDatabaseDesignerViewModel>();
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
            var vm = GetViewModel<ODBCDatabaseDesignerViewModel>();
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

        [Then(@"ODBC Outputs appear as")]
        public void ThenOutputsAppearAs(Table table)
        {
            var vm = GetViewModel<ODBCDatabaseDesignerViewModel>();
            var outputMappings = vm.OutputsRegion.Outputs;
            AssertAgainstOutputs(table, outputMappings);
        }

        private static void CheckODBCToolTestInputs(Table table, ICollection<IServiceOutputMapping> inputs)
        {
            Assert.IsNotNull(inputs);
            //TODO:Assert.AreEqual(table.Rows.Count, outputMappings.Count, "Wrong number of outputs in ODBC view model.");
            if (table.Rows.Count == 0)
            {
                if (inputs != null)
                {
                    Assert.AreEqual(inputs.Count, 0);
                }
            }
            else
            {
                var matched = table.Rows.Zip(inputs, (a, b) => new Tuple<TableRow, IServiceOutputMapping>(a, b));
                foreach (var a in matched)
                {
                    Assert.AreEqual(a.Item1.Keys.FirstOrDefault(), a.Item2.MappedFrom);
                }
            }
        }

        private static void AssertAgainstOutputs(Table table, ICollection<IServiceOutputMapping> outputs)
        {
            if (table.Rows.Count == 0)
            {
                if (outputs != null)
                {
                    Assert.AreEqual(outputs.Count, 0);
                }
            }
            else
            {
                var matched = table.Rows.Zip(outputs, (a, b) => new Tuple<TableRow, IServiceOutputMapping>(a, b));
                foreach (var a in matched)
                {
                    Assert.AreEqual(a.Item1[0], a.Item2.MappedFrom);
                    Assert.AreEqual(a.Item1[1], a.Item2.MappedTo);
                }
            }
        }

        [Then(@"ODBC Recordset Name equals ""(.*)""")]
        public void ThenRecordsetNameEquals(string recsetName)
        {
            Assert.AreEqual(recsetName, GetViewModel<ODBCDatabaseDesignerViewModel>().OutputsRegion.RecordsetName);
        }

        [When(@"ODBC Source is changed to GreenPoint")]
        public void WhenSourceIsChangedTo()
        {
            GetViewModel<ODBCDatabaseDesignerViewModel>().SourceRegion.SelectedSource = _greenPointSource;
        }
        
        [Given(@"I open New Workflow")]
        public void GivenIOpenNewWorkflow()
        {
            var viewModel = GetViewModel<ODBCDatabaseDesignerViewModel>();
            Assert.IsNotNull(viewModel);
        }
    }
}