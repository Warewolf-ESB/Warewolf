﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Dev2.Activities.Designers2.SqlServerDatabase;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.Core;

namespace Dev2.Activities.Specs.Toolbox.Resources
{
    [Binding]
    public class DatabaseConnectorSteps
    {
        private DbSourceDefinition _greenPointSource;
        private DbAction _importOrderAction;
        private DbSourceDefinition _testingDbSource;
        private DbAction _getCountriesAction;

        [Given(@"I open New Workflow")]
        public void GivenIOpenNewWorkflow()
        {
        }
        
        [Given(@"I drag a Sql Server database connector")]
        public void GivenIDragASqlServerDatabaseConnector()
        {
            var sqlServerActivity = new DsfSqlServerDatabaseActivity();
            var modelItem = ModelItemUtils.CreateModelItem(sqlServerActivity);
            var mockServiceInputViewModel = new Mock<IManageDatabaseInputViewModel>();
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
                Type = enSourceType.SqlDatabase
            };

            var dbSources = new ObservableCollection<IDbSource> { _greenPointSource };
            mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(dbSources);
            mockServiceInputViewModel.SetupAllProperties();
            var sqlServerDesignerViewModel = new SqlServerDatabaseDesignerViewModel(modelItem,mockDbServiceModel.Object);
            
            ScenarioContext.Current.Add("viewModel",sqlServerDesignerViewModel);
            ScenarioContext.Current.Add("mockServiceInputViewModel",mockServiceInputViewModel);
            ScenarioContext.Current.Add("mockDbServiceModel",mockDbServiceModel);
        }

        [When(@"Source is changed from to ""(.*)""")]
        public void WhenSourceIsChangedFromTo(string sourceName)
        {
            if(sourceName == "GreenPoint")
            {
                GetViewModel().SourceRegion.SelectedSource = _greenPointSource;
            }
        }

        [When(@"Action is changed from to ""(.*)""")]
        public void WhenActionIsChangedFromTo(string procName)
        {
            if(procName == "dbo.ImportOrder")
            {
                GetViewModel().ActionRegion.SelectedAction = _importOrderAction;
            }
        }

        [When(@"Recordset Name is changed to ""(.*)""")]
        public void WhenRecordsetNameIsChangedTo(string recSetName)
        {
            GetViewModel().OutputsRegion.RecordsetName = recSetName;
        }

        [Given(@"Source is Enabled")]
        public void GivenSourceIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.SourceRegion.IsEnabled);
        }

        private static SqlServerDatabaseDesignerViewModel GetViewModel()
        {
            return ScenarioContext.Current.Get<SqlServerDatabaseDesignerViewModel>("viewModel");
        }

        private static Mock<IManageDatabaseInputViewModel> GetInputViewModel()
        {
            return ScenarioContext.Current.Get<Mock<IManageDatabaseInputViewModel>>("mockServiceInputViewModel");
        }

        private static Mock<IDbServiceModel> GetDbServiceModel()
        {
            return ScenarioContext.Current.Get<Mock<IDbServiceModel>>("mockDbServiceModel");
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
        [When(@"Validate is Disabled")]
        [Then(@"Validate is Disabled")]
        public void GivenValidateIsDisabled()
        {
            var viewModel = GetViewModel();
            //Assert.IsFalse(viewModel.TestInputCommand.CanExecute(null));
            Assert.Fail();
        }

        [Given(@"Action is Enabled")]
        [Then(@"Action is Enabled")]
        [When(@"Action is Enabled")]
        public void ThenActionIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.ActionRegion.IsEnabled);
        }

        [Given(@"Inputs is Enabled")]
        [When(@"Inputs is Enabled")]
        [Then(@"Inputs is Enabled")]
        public void ThenInputsIsEnabled()
        {
            var viewModel = GetViewModel();
            var hasInputs = viewModel.InputArea.Inputs != null  || viewModel.InputArea.IsEnabled;
            Assert.IsTrue(hasInputs);
        }

        [Given(@"Validate is Enabled")]
        [When(@"Validate is Enabled")]
        [Then(@"Validate is Enabled")]
        public void ThenValidateIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.TestInputCommand.CanExecute());
        }

        [Given(@"Inputs appear as")]
        [When(@"Inputs appear as")]
        [Then(@"Inputs appear as")]
        public void ThenInputsAppearAs(Table table)
        {
            var viewModel = GetViewModel();
            int rowNum = 0;
            foreach(var row in table.Rows)
            {
                var inputValue = row["Input"];
                var value = row["Value"];
                var serviceInputs = viewModel.InputArea.Inputs.ToList();
                var serviceInput = serviceInputs[rowNum];
                Assert.AreEqual<string>(inputValue,serviceInput.Name);
                Assert.AreEqual<string>(value,serviceInput.Value);
                rowNum++;
            }
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
            var dbSources = new ObservableCollection<IDbSource> { _testingDbSource, _greenPointSource };
            mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(dbSources);
            mockDbServiceModel.Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction> { _getCountriesAction, _importOrderAction });
            mockServiceInputViewModel.SetupAllProperties();
            var sqlServerDesignerViewModel = new SqlServerDatabaseDesignerViewModel(modelItem, mockDbServiceModel.Object);

            ScenarioContext.Current.Add("viewModel", sqlServerDesignerViewModel);
            ScenarioContext.Current.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            ScenarioContext.Current.Add("mockDbServiceModel", mockDbServiceModel);
        }

        [Given(@"Source is ""(.*)""")]
        public void GivenSourceIs(string sourceName)
        {
            var selectedSource = GetViewModel().SourceRegion.SelectedSource;
            Assert.IsNotNull(selectedSource);
            Assert.AreEqual<string>(sourceName,selectedSource.Name);
        }

        [Given(@"Action is ""(.*)""")]
        public void GivenActionIs(string actionName)
        {
            var selectedProcedure = GetViewModel().ActionRegion.SelectedAction;
            Assert.IsNotNull(selectedProcedure);
            Assert.AreEqual<string>(actionName,selectedProcedure.Name);
        }

        
        [When(@"I Select ""(.*)"" as Source")]
        public void WhenISelectAsSource(string sourceName)
        {
            if(sourceName == "GreenPoint")
            {
                _importOrderAction = new DbAction();
                _importOrderAction.Name = "dbo.ImportOrder";
                _importOrderAction.Inputs = new List<IServiceInput>{new ServiceInput("ProductId","")};
                GetDbServiceModel().Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction> { _importOrderAction });
                GetViewModel().SourceRegion.SelectedSource = _greenPointSource;
            }
        }

        [When(@"I select dbo.ImportOrder as the action")]
        public void WhenISelectImportOrderAsTheAction()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("Column1"));
            dataTable.ImportRow(dataTable.LoadDataRow(new object[]{1},true));
            GetDbServiceModel().Setup(model => model.TestService(It.IsAny<IDatabaseService>())).Returns(dataTable);
            GetViewModel().ActionRegion.SelectedAction = _importOrderAction;
        }

        [When(@"I select new_procedure as the action")]
        public void WhenISelectnew_procedureAsTheAction()
        {
            ScenarioContext.Current.Pending();
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
            //GetViewModel().TestInputCommand.Execute(null);
            Assert.Fail();
        }


        [Then(@"the Test Connector and Calculate Outputs window is opened")]
        public void ThenTheTestConnectorAndCalculateOutputsWindowIsOpened()
        {            
        }

        [Then(@"Test Inputs appear as")]
        public void ThenTestInputsAppearAs(Table table)
        {
            int rowNum = 0;
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
        
        [Then(@"Test Connector and Calculate Outputs outputs appear as")]
        public void ThenTestConnectorAndCalculateOutputsOutputsAppearAs(Table table)
        {
            var rowIdx = 0;
            foreach(var tableRow in table.Rows)
            {
                var outputValue = tableRow["Column1"];
                var rows = GetViewModel().ManageServiceInputViewModel.TestResults.Rows;
                var dataRow = rows[rowIdx];
                var dataRowValue = dataRow[0].ToString();
                Assert.AreEqual<string>(outputValue,dataRowValue);
                rowIdx++;
            }
        }
        
        [Then(@"Outputs appear as")]
        public void ThenOutputsAppearAs(Table table)
        {
            var outputMappings = GetViewModel().OutputsRegion.Outputs;
            Assert.IsNotNull(outputMappings);
            var rowIdx = 0;
            foreach(var tableRow in table.Rows)
            {
                var mappedFrom = tableRow["Mapped From"];
                var mappedTo = tableRow["Mapped To"];
                var outputMapping = outputMappings.ToList()[rowIdx];
                Assert.AreEqual<string>(mappedFrom,outputMapping.MappedFrom);
                Assert.AreEqual<string>(mappedTo,outputMapping.MappedTo);
                rowIdx++;
            }
        }

        [Then(@"Recordset Name equals ""(.*)""")]
        public void ThenRecordsetNameEquals(string recsetName)
        {
            Assert.AreEqual<string>(recsetName,GetViewModel().OutputsRegion.RecordsetName);
        }        
    }
}
