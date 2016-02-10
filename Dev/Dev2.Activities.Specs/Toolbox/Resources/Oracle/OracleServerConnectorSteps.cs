using Caliburn.Micro;
using Dev2.Activities.Designers2.Oracle;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using TechTalk.SpecFlow;
using Warewolf.Core;

namespace Dev2.Activities.Specs.Toolbox.Resources
{
    [Binding]
    public class OracleServerConnectorSteps
    {
        private DbSourceDefinition _greenPointSource;
        private DbAction _importOrderAction;
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
            var oracleServerDesignerViewModel = new OracleDatabaseDesignerViewModel(modelItem, new Mock<IContextualResourceModel>().Object,
                                                                                        mockEnvironmentRepo.Object, new Mock<IEventAggregator>().Object, new SynchronousAsyncWorker(), mockServiceInputViewModel.Object, mockDbServiceModel.Object);

            ScenarioContext.Current.Add("viewModel", oracleServerDesignerViewModel);
            ScenarioContext.Current.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            ScenarioContext.Current.Add("mockDbServiceModel", mockDbServiceModel);
        }
        
        [Given(@"Source is ""(.*)""")]
        public void GivenSourceIs(string name)
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.SourceVisible);
        }
        [Given(@"Source is Enable")]
        public void GivenSourceIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.SourceVisible);
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

        [Given(@"Action is dbo\.Pr_CitiesGetCountries")]
        public void GivenActionIsDbo_Pr_CitiesGetCountries()
        {
            ScenarioContext.Current.Pending();
        }
        [Given(@"Action is Disable")]
        public void GivenActionIsDisabled()
        {
            var viewModel = GetViewModel();
            Assert.IsFalse(viewModel.ActionVisible);
        }

        [Given(@"Inputs is Disable")]
        public void GivenInputsIsDisabled()
        {
            var viewModel = GetViewModel();
            var hasNoInputs = viewModel.Inputs == null || viewModel.Inputs.Count == 0 || !viewModel.InputsVisible;
            Assert.IsTrue(hasNoInputs);
        }

        [Given(@"Outputs is Disable")]
        public void GivenOutputsIsDisabled()
        {
            var viewModel = GetViewModel();
            var hasNoOutputs = viewModel.Outputs == null || viewModel.Outputs.Count == 0 || !viewModel.OutputsVisible;
            Assert.IsTrue(hasNoOutputs);
        }

        [Given(@"Validate is Disable")]
        public void GivenValidateIsDisabled()
        {
            var viewModel = GetViewModel();
            Assert.IsFalse(viewModel.TestInputCommand.CanExecute(null));
        }

        [Given(@"Action is Enable")]
        [Then(@"Action is Enable")]
        [When(@"Action is Enable")]
        public void ThenActionIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.ActionVisible);
        }
        [Given(@"Inputs is Enable")]
        public void GivenInputsIsEnabled()
        {
            ScenarioContext.Current.Pending();
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
        
      
        
        [When(@"I click ""(.*)""")]
        public void WhenIClick(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Validate is Enable")]
        public void ThenValidateIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.TestInputCommand.CanExecute(null));
        }

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
                var serviceInputs = viewModel.Inputs.ToList();
                var serviceInput = serviceInputs[rowNum];
                Assert.AreEqual(inputValue, serviceInput.Name);
                Assert.AreEqual(value, serviceInput.Value);
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
                GetViewModel().SelectedSource = _greenPointSource;
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
                GetViewModel().SelectedProcedure = _importOrderAction;
            }
        }

    


        [Then(@"Inputs is Enable")]
        public void ThenInputsIsEnabled()
        {
            var viewModel = GetViewModel();
            var hasInputs = viewModel.Inputs != null || viewModel.InputsVisible;
            Assert.IsTrue(hasInputs);
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
            GetViewModel().TestInputCommand.Execute(null);
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
                Assert.AreEqual(outputValue, dataRowValue);
                rowIdx++;
            }
        }

        [Then(@"Outputs appear az")]
        public void ThenOutputsAppearAs(Table table)
        {
            var outputMappings = GetViewModel().Outputs;
            Assert.IsNotNull(outputMappings);
            var rowIdx = 0;
            foreach (var tableRow in table.Rows)
            {
                var mappedFrom = tableRow["Mapped From"];
                var mappedTo = tableRow["Mapped To"];
                var outputMapping = outputMappings.ToList()[rowIdx];
                Assert.AreEqual(mappedFrom, outputMapping.MappedFrom);
                Assert.AreEqual(mappedTo, outputMapping.MappedTo);
            }
        }

        [Then(@"Recordset Name equalz ""(.*)""")]
        public void ThenRecordsetNameEquals(string recsetName)
        {
            Assert.AreEqual(recsetName, GetViewModel().RecordsetName);
        }

       

      

     

       


      
  
    }
}