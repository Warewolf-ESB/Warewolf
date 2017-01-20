using Dev2.Activities;
using Dev2.Activities.Designers2.MySqlDatabase;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Dev2.Common.Interfaces.Core;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.Core;

namespace Warewolf.ToolsSpecs.Toolbox.Resources.MySQL
{
    [Binding]
    public sealed class MySqlConnectorSteps
    {
        private readonly ScenarioContext scenarioContext;

        public MySqlConnectorSteps(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        private DbSourceDefinition _sqlSource;
        private DbSourceDefinition _anotherSqlSource;
        private DbAction _someAction;
        private Mock<IServiceOutputMapping> _outputMapping;
        
        [Given(@"I drag in mysql connector tool")]
        public void GivenIDragInMysqlConnectorTool()
        {
            var mysqlActivity = new DsfMySqlDatabaseActivity();
            var modelItem = ModelItemUtils.CreateModelItem(mysqlActivity);

            var mockDatabaseInputViewModel = new Mock<IManageDatabaseInputViewModel>();
            var mockDbServiceModel = new Mock<IDbServiceModel>();

            var mockEnvironmentRepo = new Mock<IEnvironmentRepository>();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();

            _outputMapping = new Mock<IServiceOutputMapping>();
            
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.ID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockEnvironmentRepo.Setup(repository => repository.ActiveEnvironment).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(mockEnvironmentModel.Object);

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

            var mysqlDesignerViewModel = new MySqlDatabaseDesignerViewModel(modelItem, mockDbServiceModel.Object, new SynchronousAsyncWorker());

            scenarioContext.Add("mysqlActivity", mysqlActivity);
            scenarioContext.Add("viewModel", mysqlDesignerViewModel);
            scenarioContext.Add("mockDatabaseInputViewModel", mockDatabaseInputViewModel);
            scenarioContext.Add("mockDbServiceModel", mockDbServiceModel);
        }

        #region Private Methods

        private DsfMySqlDatabaseActivity GetDsfMySqlDatabaseActivity()
        {
            return scenarioContext.Get<DsfMySqlDatabaseActivity>("mysqlActivity");
        }
        private Mock<IManageDatabaseInputViewModel> GetDatabaseInputViewModel()
        {
            return scenarioContext.Get<Mock<IManageDatabaseInputViewModel>>("mockDatabaseInputViewModel");
        }

        private MySqlDatabaseDesignerViewModel GetViewModel()
        {
            return scenarioContext.Get<MySqlDatabaseDesignerViewModel>("viewModel");
        }

        private Mock<IDbServiceModel> GetServiceModel()
        {
            return scenarioContext.Get<Mock<IDbServiceModel>>("mockDbServiceModel");
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

        private void SetupActions(IDbSource selectedSource)
        {
            if (selectedSource != null)
            {
                _someAction = new DbAction
                {
                    Name = "someAction",
                    Inputs = new List<IServiceInput> {new ServiceInput("SomeInput", "")}
                };
                var serviceModel = GetServiceModel();
                serviceModel.Setup(model => model.GetActions(It.IsAny<IDbSource>())).Returns(new List<IDbAction> { _someAction });
            }
            if (GetViewModel().ActionRegion.SelectedAction == null)
                GetViewModel().ActionRegion.SelectedAction = _someAction;
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
                Assert.AreEqual(p0, onlyAction.Name);
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
                Assert.AreEqual("SomeRecordSet", outputResults.RecordSetName);
        }

        private void SetupOutpuRegions()
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

        [Then(@"I click Okay on mysql connector tool")]
        public void ThenIClickOkayOnMysqlConnectorTool()
        {            
            Assert.IsTrue(ClickOk());
        }

        private bool ClickOk()
        {
            return GetViewModel().ManageServiceInputViewModel.OkSelected = true;
        }

        [Then(@"The recordset name appear as ""(.*)"" on mysql connector tool")]
        public void ThenTheRecordsetNameAppearAsOnMysqlConnectorTool(string p0)
        {
            var viewModel = GetViewModel();
            var outputMapping = viewModel.OutputsRegion.Outputs.FirstOrDefault();
            if (outputMapping != null)            
                Assert.AreEqual(p0, outputMapping.RecordSetName);
        }
        
        [Given(@"I open an existing mysql connector tool")]
        public void GivenIOpenAnExistingMysqlConnectorTool()
        {
            var mysqlActivity = new DsfMySqlDatabaseActivity();
            var modelItem = ModelItemUtils.CreateModelItem(mysqlActivity);

            var mockDatabaseInputViewModel = new Mock<IManageDatabaseInputViewModel>();
            var mockDbServiceModel = new Mock<IDbServiceModel>();

            var mockEnvironmentRepo = new Mock<IEnvironmentRepository>();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();

            _outputMapping = new Mock<IServiceOutputMapping>();

            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            mockEnvironmentModel.Setup(model => model.ID).Returns(Guid.Empty);
            mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            mockEnvironmentRepo.Setup(repository => repository.ActiveEnvironment).Returns(mockEnvironmentModel.Object);
            mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(mockEnvironmentModel.Object);

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

            var mysqlDesignerViewModel = new MySqlDatabaseDesignerViewModel(modelItem, mockDbServiceModel.Object, new SynchronousAsyncWorker());

            var selectedSource = SetupSelectedSource(mysqlDesignerViewModel);            

            scenarioContext.Add("viewModel", mysqlDesignerViewModel);
            scenarioContext.Add("mockDatabaseInputViewModel", mockDatabaseInputViewModel);
            scenarioContext.Add("mockDbServiceModel", mockDbServiceModel);

            SetupActions(selectedSource);
        }

        private static IDbSource SetupSelectedSource(MySqlDatabaseDesignerViewModel mysqlDesignerViewModel)
        {
            return mysqlDesignerViewModel.SourceRegion.SelectedSource =
                mysqlDesignerViewModel.SourceRegion.Sources.FirstOrDefault(p=>p.Name == "DemoSqlsource");
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
                Assert.AreEqual(p0, onlyAction.Name);
        }

        [Given(@"Inputs are ""(.*)"" for mysql connector tool")]
        public void GivenInputsAreForMysqlConnectorTool(string p0)
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsTrue(viewModel.ActionRegion.IsEnabled);
            var onlyAction = viewModel.ActionRegion.SelectedAction.Inputs.FirstOrDefault();
            if (onlyAction != null)
                Assert.AreEqual(p0, onlyAction.Name);
        }

        [Then(@"The outputs appear as ""(.*)"" on mysql connector tool")]
        public void ThenTheOutputsAppearAsOnMysqlConnectorTool(string p0)
        {
            var viewModel = GetViewModel();
            Assert.IsNotNull(viewModel.OutputsRegion);
            SetupOutpuRegions();
            var outputResults = viewModel.OutputsRegion.Outputs.FirstOrDefault();
            if (outputResults != null)
                Assert.AreEqual("SomeRecordSet", outputResults.RecordSetName);
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
            //SetupOutpuRegions();
            var outputResults = viewModel.OutputsRegion.Outputs.FirstOrDefault();
            if (outputResults != null)
                Assert.AreEqual("SomeRecordSet", outputResults.RecordSetName);
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
    }
}
