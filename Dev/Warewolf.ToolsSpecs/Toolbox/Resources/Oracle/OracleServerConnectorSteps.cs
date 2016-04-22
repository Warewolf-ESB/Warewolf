using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Dev2.Activities.Designers2.Oracle;
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
    public class OracleServerConnectorSteps
    {
        private DbSourceDefinition _greenPointSource;
        private DbAction _importOrderAction;
        private DbSourceDefinition _testingDbSource;
        private DbAction _getCountriesAction;

        [Given(@"I drag a Oracle Server database connector")]
        public void GivenIDragAOracleServerDatabaseConnector()
        {
            var oracleServerActivity = new DsfOracleDatabaseActivity();
            var modelItem = ModelItemUtils.CreateModelItem(oracleServerActivity);
            var mockServiceInputViewModel = new Mock<IManageServiceInputViewModel>();
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
            mockServiceInputViewModel.SetupAllProperties();
            var oracleServerDesignerViewModel = new OracleDatabaseDesignerViewModel(modelItem, mockDbServiceModel.Object);

            ScenarioContext.Current.Add("viewModel", oracleServerDesignerViewModel);
            ScenarioContext.Current.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            ScenarioContext.Current.Add("mockDbServiceModel", mockDbServiceModel);
        }
        [Given(@"Source iss ""(.*)""")]
        public void GivenSourceIs(string sourceName)
        {
            var selectedSource = GetViewModel().SourceRegion.SelectedSource;
            Assert.IsNotNull(selectedSource);
            Assert.AreEqual<string>(sourceName, selectedSource.Name);
        }
        [Given(@"Source is Enable")]
        public void GivenSourceIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.SourceRegion.IsEnabled);
        }
        private static OracleDatabaseDesignerViewModel GetViewModel()
        {
            return ScenarioContext.Current.Get<OracleDatabaseDesignerViewModel>("viewModel");
        }

        private static Mock<IManageServiceInputViewModel> GetInputViewModel()
        {
            return ScenarioContext.Current.Get<Mock<IManageServiceInputViewModel>>("mockServiceInputViewModel");
        }

        private static Mock<IDbServiceModel> GetDbServiceModel()
        {
            return ScenarioContext.Current.Get<Mock<IDbServiceModel>>("mockDbServiceModel");
        }
        [Given(@"Action iss ""(.*)""")]
        public void GivenActionIs(string actionName)
        {
            var selectedProcedure = GetViewModel().ActionRegion.SelectedAction;
            Assert.IsNotNull(selectedProcedure);
            Assert.AreEqual<string>(actionName, selectedProcedure.Name);
        }
        [Given(@"Action is dbo\.Pr_CitiesGetCountries")]
        public void GivenActionIsDbo_Pr_CitiesGetCountries()
        {
            ScenarioContext.Current.Pending();
        }
        [Given(@"Action is Disable")]
        public void GivenActionIsDisabled()
        {
            var viewModel = GetViewModel();
            Assert.IsFalse(viewModel.ActionRegion.IsActionEnabled);
        }

        [Given(@"Inputs is Disable")]
        [When(@"Inputs is Disable")]
        [Then(@"Inputs is Disable")]
        public void GivenInputsIsDisabled()
        {
            var viewModel = GetViewModel().InputArea;
            var hasNoInputs = viewModel.Inputs == null || viewModel.Inputs.Count == 0 || !viewModel.IsEnabled;
            Assert.IsTrue(hasNoInputs);
        }

        
        [Given(@"Outputs is Disable")]
        [When(@"Outputs is Disable")]
        [Then(@"Outputs is Disable")]
        public void GivenOutputsIsDisabled()
        {
            var viewModel = GetViewModel().OutputsRegion;
            var hasNoOutputs = viewModel.Outputs == null || viewModel.Outputs.Count == 0 || !viewModel.OutputMappingEnabled;
            Assert.IsTrue(hasNoOutputs);
        }

        [Given(@"Validate is Disable")]
        [When(@"Validate is Disable")]
        [Then(@"Validate is Disable")]
        public void GivenValidateIsDisabled()
        {
            var viewModel = GetViewModel();
            Assert.IsFalse(viewModel.TestInputCommand.CanExecute());
        }
        [When(@"Source iz changed from to ""(.*)""")]
        public void WhenSourceIsChangedFromTo(string sourceName)
        {
            if (sourceName == "GreenPoint")
            {
                var viewModel = GetViewModel().SourceRegion;
                viewModel.SelectedSource = _greenPointSource;
            }
        }
        [Given(@"Action is Enable")]
        [Then(@"Action is Enable")]
        [When(@"Action is Enable")]
        public void ThenActionIsEnabled()
        {
            var viewModel = GetViewModel().ActionRegion;
            Assert.IsTrue(viewModel.IsActionEnabled);
        }
      

        [Given(@"Inputs is Enable")]
        [When(@"Inputs is Enable")]
        [Then(@"Inputs is Enable")]
        public void ThenInputsIsEnabled()
        {
            var viewModel = GetViewModel().InputArea;
            var hasInputs = viewModel.Inputs != null || viewModel.IsEnabled;
            Assert.IsTrue(hasInputs);
        }

        [Given(@"Validate is Enable")]
        [When(@"Validate is Enable")]
        [Then(@"Validate is Enable")]
        public void ThenValidateIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.TestInputCommand.CanExecute());
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

        [When(@"Action iz changed from to ""(.*)""")]
        public void WhenActionIsChangedFromTo(string procName)
        {
            if (procName == "dbo.ImportOrder")
            {
                GetViewModel().ActionRegion.SelectedAction = _importOrderAction;
            }
        }
        
        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
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
            var viewModel = GetViewModel().InputArea;
            int rowNum = 0;
            foreach (var row in table.Rows)
            {
                var inputValue = row["Input"];
                var value = row["Value"];
                var serviceInputs = viewModel.Inputs.ToList();
                var serviceInput = serviceInputs[rowNum];
                Assert.AreEqual<string>(inputValue, serviceInput.Name);
                Assert.AreEqual<string>(value, serviceInput.Value);
                rowNum++;
            }
        }

      

        [Given(@"I open ""(.*)"" service")]
        public void GivenIOpenService(string p0)
        {
            ScenarioContext.Current.Pending();
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

      

        [When(@"""(.*)"" is selected as the data source")]
        public void WhenIsSelectedAsTheDataSource(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"testing the action fails")]
        public void WhenTestingTheActionFails()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I click Validat")]
        public void WhenIClickValidate()
        {
           var result = GetViewModel().TestInputCommand.Execute();
        }


        [Then(@"the Test Connector and Calculate Outputs window is open")]
        public void ThenTheTestConnectorAndCalculateOutputsWindowIsOpened()
        {
            //GetInputViewModel().SetupProperty(model => model.Inputs,null);   
        }

        [Then(@"Test Inputs appear az")]
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


        [Then(@"Test Connector and Calculate Outputs outputs appear az")]
        public void ThenTestConnectorAndCalculateOutputsOutputsAppearAs(Table table)
        {
            var rowIdx = 0;
            foreach (var tableRow in table.Rows)
            {
                var viewModel = GetViewModel();
                var outputValue = tableRow["Column1"];
                var rows = viewModel.ManageServiceInputViewModel.TestResults.Rows;
                var dataRow = rows[rowIdx];
                var dataRowValue = dataRow[0].ToString();
                Assert.AreEqual<string>(outputValue, dataRowValue);
                rowIdx++;
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
            var mockServiceInputViewModel = new Mock<IManageServiceInputViewModel>();
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
            mockServiceInputViewModel.SetupAllProperties();
            var oracleDatabaseDesignerViewModel = new OracleDatabaseDesignerViewModel(modelItem, mockDbServiceModel.Object);

            ScenarioContext.Current.Add("viewModel", oracleDatabaseDesignerViewModel);
            ScenarioContext.Current.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            ScenarioContext.Current.Add("mockDbServiceModel", mockDbServiceModel);
        }
        [Then(@"Outputs appear az")]
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

        [Then(@"Recordset Name equalz ""(.*)""")]
        public void ThenRecordsetNameEquals(string recsetName)
        {
            Assert.AreEqual<string>(recsetName, GetViewModel().OutputsRegion.RecordsetName);
        }

       

      

     

       


      
  
    }
}