﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Caliburn.Micro;
using Dev2.Activities.Designers2.SqlServerDatabase;
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
using TechTalk.SpecFlow;
using Warewolf.Core;

namespace Dev2.Activities.Specs.Toolbox.Resources
{
    [Binding]
    public class DatabaseConnectorSteps
    {
        private DbSourceDefinition _greenPointSource;
        private DbAction _importOrderAction;

        [Given(@"I open New Workflow")]
        public void GivenIOpenNewWorkflow()
        {
        }
        
        [Given(@"I drag a Sql Server database connector")]
        public void GivenIDragASqlServerDatabaseConnector()
        {
            var sqlServerActivity = new DsfSqlServerDatabaseActivity();
            var modelItem = ModelItemUtils.CreateModelItem(sqlServerActivity);
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
                Type = enSourceType.SqlDatabase
            };

            var dbSources = new ObservableCollection<IDbSource> { _greenPointSource };
            mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(dbSources);
            mockServiceInputViewModel.SetupAllProperties();
            var sqlServerDesignerViewModel = new SqlServerDatabaseDesignerViewModel(modelItem,new Mock<IContextualResourceModel>().Object,
                                                                                        mockEnvironmentRepo.Object, new Mock<IEventAggregator>().Object,new SynchronousAsyncWorker(), mockServiceInputViewModel.Object,mockDbServiceModel.Object);
            
            ScenarioContext.Current.Add("viewModel",sqlServerDesignerViewModel);
            ScenarioContext.Current.Add("mockServiceInputViewModel",mockServiceInputViewModel);
            ScenarioContext.Current.Add("mockDbServiceModel",mockDbServiceModel);
        }

        [Given(@"Source is Enabled")]
        public void GivenSourceIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.SourceVisible);
        }

        private static SqlServerDatabaseDesignerViewModel GetViewModel()
        {
            return ScenarioContext.Current.Get<SqlServerDatabaseDesignerViewModel>("viewModel");
        }

        private static Mock<IManageServiceInputViewModel> GetInputViewModel()
        {
            return ScenarioContext.Current.Get<Mock<IManageServiceInputViewModel>>("mockServiceInputViewModel");
        }

        private static Mock<IDbServiceModel> GetDbServiceModel()
        {
            return ScenarioContext.Current.Get<Mock<IDbServiceModel>>("mockDbServiceModel");
        }

        [Given(@"Action is Disabled")]
        public void GivenActionIsDisabled()
        {
            var viewModel = GetViewModel();
            Assert.IsFalse(viewModel.ActionVisible);
        }

        [Given(@"Inputs is Disabled")]
        public void GivenInputsIsDisabled()
        {
            var viewModel = GetViewModel();
            var hasNoInputs = viewModel.Inputs == null || viewModel.Inputs.Count == 0 || !viewModel.InputsVisible;
            Assert.IsTrue(hasNoInputs);
        }

        [Given(@"Outputs is Disabled")]
        public void GivenOutputsIsDisabled()
        {
            var viewModel = GetViewModel();
            var hasNoOutputs = viewModel.Outputs == null || viewModel.Outputs.Count == 0 || !viewModel.OutputsVisible;
            Assert.IsTrue(hasNoOutputs);
        }

        [Given(@"Validate is Disabled")]
        public void GivenValidateIsDisabled()
        {
            var viewModel = GetViewModel();
            Assert.IsFalse(viewModel.TestInputCommand.CanExecute(null));
        }

        [Given(@"Action is Enabled")]
        [Then(@"Action is Enabled")]
        [When(@"Action is Enabled")]
        public void ThenActionIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.ActionVisible);
        }

        [Then(@"Inputs is Enabled")]
        public void ThenInputsIsEnabled()
        {
            var viewModel = GetViewModel();
            var hasInputs = viewModel.Inputs != null  || viewModel.InputsVisible;
            Assert.IsTrue(hasInputs);
        }

        [Then(@"Validate is Enabled")]
        public void ThenValidateIsEnabled()
        {
            var viewModel = GetViewModel();
            Assert.IsTrue(viewModel.TestInputCommand.CanExecute(null));
        }

        [Then(@"Inputs appear as")]
        public void ThenInputsAppearAs(Table table)
        {
            var viewModel = GetViewModel();
            int rowNum = 0;
            foreach(var row in table.Rows)
            {
                var inputValue = row["Input"];
                var value = row["Value"];
                var serviceInputs = viewModel.Inputs.ToList();
                var serviceInput = serviceInputs[rowNum];
                Assert.AreEqual(inputValue,serviceInput.Name);
                Assert.AreEqual(value,serviceInput.Value);
                rowNum++;
            }
        }

        [Given(@"I open ""(.*)""")]
        public void GivenIOpenWolf(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Given(@"I open ""(.*)"" service")]
        public void GivenIOpenService(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Given(@"""(.*)"" tab is opened")]
        public void GivenTabIsOpened(string p0)
        {
            ScenarioContext.Current.Pending();
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
                GetViewModel().SelectedSource = _greenPointSource;
            }
        }
        
        [When(@"I select ""(.*)"" as the action")]
        public void WhenISelectAsTheAction(string actionName)
        {
            if (actionName == "dbo.ImportOrder")
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add(new DataColumn("Column1"));
                dataTable.ImportRow(dataTable.LoadDataRow(new object[]{1},true));
                GetDbServiceModel().Setup(model => model.TestService(It.IsAny<IDatabaseService>())).Returns(dataTable);
                GetViewModel().SelectedProcedure = _importOrderAction;
            }
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
        
        [When(@"""(.*)"" is changed from ""(.*)"" to ""(.*)""")]
        public void WhenIsChangedFromTo(string p0, string p1, string p2)
        {
            ScenarioContext.Current.Pending();
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
        
        [When(@"I click Validate")]
        public void WhenIClickValidate()
        {
            GetViewModel().TestInputCommand.Execute(null);
        }


        [Then(@"the Test Connector and Calculate Outputs window is opened")]
        public void ThenTheTestConnectorAndCalculateOutputsWindowIsOpened()
        {
            //GetInputViewModel().SetupProperty(model => model.Inputs,null);   
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
                Assert.AreEqual(outputValue,dataRowValue);
                rowIdx++;
            }
        }
        
        [Then(@"Outputs appear as")]
        public void ThenOutputsAppearAs(Table table)
        {
            var outputMappings = GetViewModel().Outputs;
            Assert.IsNotNull(outputMappings);
            var rowIdx = 0;
            foreach(var tableRow in table.Rows)
            {
                var mappedFrom = tableRow["Mapped From"];
                var mappedTo = tableRow["Mapped To"];
                var outputMapping = outputMappings.ToList()[rowIdx];
                Assert.AreEqual(mappedFrom,outputMapping.MappedFrom);
                Assert.AreEqual(mappedTo,outputMapping.MappedTo);
            }
        }

        [Then(@"Recordset Name equals ""(.*)""")]
        public void ThenRecordsetNameEquals(string recsetName)
        {
            Assert.AreEqual(recsetName,GetViewModel().RecordsetName);
        }
        
        [Then(@"""(.*)"" tab is opened")]
        public void ThenTabIsOpened(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"""(.*)"" equals ""(.*)""")]
        public void ThenEquals(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"""(.*)"" mappings are")]
        public void ThenMappingsAre(string p0, Table table)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"mappings are")]
        public void ThenMappingsAre(Table table)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"input mappings are")]
        public void ThenInputMappingsAre(Table table)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"Data Source is focused")]
        public void ThenDataSourceIsFocused()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"""(.*)"" is selected as the action")]
        public void ThenIsSelectedAsTheAction(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"Inspect Data Connector hyper link is ""(.*)""")]
        public void ThenInspectDataConnectorHyperLinkIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"inputs are")]
        public void ThenInputsAre(Table table)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"output mappings are")]
        public void ThenOutputMappingsAre(Table table)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the ""(.*)"" window is opened")]
        public void ThenTheWindowIsOpened(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"""(.*)"" outputs appear as")]
        public void ThenOutputsAppearAs(string p0, Table table)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
