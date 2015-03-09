using System;
using System.Collections.Generic;
using System.Data;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TechTalk.SpecFlow;
using Warewolf.AcceptanceTesting.Explorer.DBServiceSpecs;
using Warewolf.Core;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;

namespace Warewolf.AcceptanceTesting.Explorer.DBService_Specs
{
    [Binding]
    // ReSharper disable once InconsistentNaming
    public class DBServiceSteps
    {
        private static DbSourceDefinition _demoDbSourceDefinition;
        private static DbSourceDefinition _testDbSourceDefinition;
        private static DbAction _dbAction;

        [BeforeFeature("DBService")]
        public static void SetupForSystem()
        {
            var bootStrapper = new UnityBootstrapperForDatabaseServiceConnectorTesting();
            bootStrapper.Run();
            var view = new ManageDatabaseServiceControl();
            var mockRequestServiceNameViewModel = new Mock<IRequestServiceNameViewModel>();
            mockRequestServiceNameViewModel.Setup(model => model.ShowSaveDialog()).Verifiable();
            var mockDbServiceModel = new Mock<IDbServiceModel>();
            SetupModel(mockDbServiceModel);
            var viewModel = new ManageDatabaseServiceViewModel(mockDbServiceModel.Object, mockRequestServiceNameViewModel.Object);
            view.DataContext = viewModel;
            Utils.ShowTheViewForTesting(view);
            FeatureContext.Current.Add(Utils.ViewNameKey, view);
            FeatureContext.Current.Add("viewModel", viewModel);
            FeatureContext.Current.Add("model",mockDbServiceModel);
            FeatureContext.Current.Add("requestServiceNameViewModel", mockRequestServiceNameViewModel);
           
        }

        private static void SetupModel(Mock<IDbServiceModel> mockDbServiceModel)
        {
            _demoDbSourceDefinition = new DbSourceDefinition
            {
                Name = "DemoDB",
                ServerName = "gendev"
            };
            _testDbSourceDefinition = new DbSourceDefinition
            {
                Name = "Testing DB",
                ServerName = "tfsbld"
            };
            mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(new List<IDbSource>
            {
                _demoDbSourceDefinition,
                _testDbSourceDefinition
            });
            _dbAction = new DbAction
            {
                Name = "dbo.ConverToint",
                Inputs = new List<IDbInput>{new DbInput("charValue","1")}
            };
            mockDbServiceModel.Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction>
            {
                _dbAction
            });
            var dataTable = new DataTable("dbo_ConverrToInt");
            dataTable.Columns.Add("Result",typeof(int));
            dataTable.LoadDataRow(new object[] {"1"}, true);
            mockDbServiceModel.Setup(model => model.TestService(It.IsAny<IDatabaseService>()))
                .Returns(dataTable);
        }

        [BeforeScenario("DBService")]
        public void SetupForDatabaseService()
        {
            ScenarioContext.Current.Add("view", FeatureContext.Current.Get<ManageDatabaseServiceControl>(Utils.ViewNameKey));
            ScenarioContext.Current.Add("viewModel", FeatureContext.Current.Get<ManageDatabaseServiceViewModel>("viewModel"));
            ScenarioContext.Current.Add("requestServiceNameViewModel", FeatureContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel"));
            ScenarioContext.Current.Add("model", FeatureContext.Current.Get<Mock<IDbServiceModel>>("model"));

        }

        [Given(@"I click New Data Base Service Connector")]
        public void GivenIClickNewDataBaseServiceConnector()
        {
            var view = GetView();
            Assert.IsNotNull(view);
            Assert.IsNotNull(view.DataContext);
            Assert.IsInstanceOfType(view.DataContext,typeof(ManageDatabaseServiceViewModel));
        }

        [When(@"I select ""(.*)"" as data source")]
        public void WhenISelectAsDataSource(string dbSourceName)
        {
            var view = GetView();
            view.SelectDbSource(_demoDbSourceDefinition);
            var viewModel = GetViewModel();
            Assert.AreEqual(dbSourceName,viewModel.SelectedSource.Name);
        }

        [Given(@"I select ""(.*)"" as the action")]
        [When(@"I select ""(.*)"" as the action")]
        [Then(@"I select ""(.*)"" as the action")]
        public void WhenISelectAsTheAction(string actionName)
        {
            var view = GetView();
            view.SelectDbAction(_dbAction);
            var viewModel = GetViewModel();
            Assert.AreEqual(actionName,viewModel.SelectedAction.Name);
        }

        private static ManageDatabaseServiceViewModel GetViewModel()
        {
            var viewModel = ScenarioContext.Current.Get<ManageDatabaseServiceViewModel>("viewModel");
            return viewModel;
        }

        private static ManageDatabaseServiceControl GetView()
        {
            var view = ScenarioContext.Current.Get<ManageDatabaseServiceControl>(Utils.ViewNameKey);
            return view;
        }

        [When(@"I test the action")]
        public void WhenITestTheAction()
        {
            var view = GetView();
            view.TestAction();
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel.TestResults);
            
        }

        [When(@"I save")]
        public void WhenISave()
        {
            var view = GetView();
            view.Save();
           
        }

        [Given(@"""(.*)"" tab is opened")]
        [When(@"""(.*)"" tab is opened")]
        [Then(@"""(.*)"" tab is opened")]
        public void ThenTabIsOpened(string tabName)
        {
            var viewModel = GetViewModel();
            Assert.AreEqual("*"+tabName,viewModel.Header);
        }

        [Given(@"Data Source is focused")]
        [When(@"Data Source is focused")]
        [Then(@"Data Source is focused")]
        public void ThenDataSourceIsFocused()
        {
            var view = GetView();
            var isDataSourceFocused = view.IsDataSourceFocused();
            Assert.IsTrue(isDataSourceFocused);
        }

        [Given(@"inputs are")]
        [When(@"inputs are")]
        [Then(@"inputs are")]
        public void ThenInputsAre(Table table)
        {
            var view = GetView();
            var inputs = view.GetInputs();
            foreach (var input in inputs.SourceCollection)
            {
                var dbInput = input as IDbInput;
                if (dbInput != null)
                {
                    Assert.AreEqual(dbInput.Value,table.Rows[0][dbInput.Name]);
                }
            }
        }

        [Given(@"outputs are")]
        [When(@"outputs are")]
        [Then(@"outputs are")]
        public void ThenOutputsAre(Table table)
        {
            var view = GetView();
            var outputs = view.GetOutputs();
        }

        [Given(@"input mappings are")]
        [When(@"input mappings are")]
        [Then(@"input mappings are")]
        public void ThenInputMappingsAre(Table table)
        {
            var view = GetView();
            var inputMappings = view.GetInputMappings();
        }

        [Given(@"output mappings are")]
        [When(@"output mappings are")]
        [Then(@"output mappings are")]
        public void ThenOutputMappingsAre(Table table)
        {
            var view = GetView();
            var outputMappings = view.GetOutputMappings();
        }

        [Given(@"I open ""(.*)"" service")]
        public void GivenIOpenService(string serviceName)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"""(.*)"" is selected as the data source")]
        public void GivenIsSelectedAsTheDataSource(string selectedDataSourceName)
        {
            var view = GetView();
            var selectedDataSource = view.GetSelectedDataSource();
            Assert.AreEqual(selectedDataSourceName,selectedDataSource.Name);
        }

        [Given(@"Inspect Data Connector hyper link is ""(.*)""")]
        [When(@"Inspect Data Connector hyper link is ""(.*)""")]
        [Then(@"Inspect Data Connector hyper link is ""(.*)""")]
        public void ThenInspectDataConnectorHyperLinkIs(string p0)
        {
        }

        [Then(@"""(.*)"" is saved")]
        public void ThenIsSaved(string p0)
        {
            
        }

        [Then(@"Save Dialog is not opened")]
        public void ThenSaveDialogIsNotOpened()
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Verify(model => model.ShowSaveDialog(),Times.Never());
        }

        [When(@"Execution fails")]
        public void WhenExecutionFails()
        {
            var dbServiceModel = ScenarioContext.Current.Get<Mock<IDbServiceModel>>("model");
            dbServiceModel.Setup(model => model.TestService(It.IsAny<IDatabaseService>()))
                .Throws(new Exception("Test Failed"));
        }

        [Given(@"""(.*)"" is selected as the action")]
        public void GivenIsSelectedAsTheAction(string selectedActionName)
        {
            var view = GetView();
            var selectedAction = view.GetSelectedAction();
            Assert.AreEqual(selectedActionName,selectedAction.Name);

        }

        [Then(@"Save Dialog is opened")]
        public void ThenSaveDialogIsOpened()
        {
            var mockRequestServiceNameViewModel = ScenarioContext.Current.Get<Mock<IRequestServiceNameViewModel>>("requestServiceNameViewModel");
            mockRequestServiceNameViewModel.Verify();
        }
    }
}
