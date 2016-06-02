using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Moq;
using TechTalk.SpecFlow;

namespace Warewolf.ToolsSpecs.Toolbox.Resources.MySQL
{
    [Binding]
    public sealed class MySqlConnectorSteps
    {
        [Given(@"I drag in mysql connector tool")]
        public void GivenIDragInMysqlConnectorTool()
        {
            var mysqlActivity = new DsfMySqlDatabaseActivity();
            var modelItem = ModelItemUtils.CreateModelItem(mysqlActivity);

            var mockServiceInputViewModel = new Mock<IManageDatabaseInputViewModel>();

            //var mockDbServiceModel = new Mock<IDbServiceModel>();
            //var mockEnvironmentRepo = new Mock<IEnvironmentRepository>();
            //var mockEnvironmentModel = new Mock<IEnvironmentModel>();

            //mockEnvironmentModel.Setup(model => model.IsConnected).Returns(true);
            //mockEnvironmentModel.Setup(model => model.IsLocalHost).Returns(true);
            //mockEnvironmentModel.Setup(model => model.ID).Returns(Guid.Empty);
            //mockEnvironmentModel.Setup(model => model.IsLocalHostCheck()).Returns(false);
            //mockEnvironmentRepo.Setup(repository => repository.ActiveEnvironment).Returns(mockEnvironmentModel.Object);
            //mockEnvironmentRepo.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>())).Returns(mockEnvironmentModel.Object);

            //_postgresSqlSource = new DbSourceDefinition
            //{
            //    Name = "DemoPostgres",
            //    Type = enSourceType.PostgreSql,
            //    ServerName = "Localhost",
            //    UserName = "postgres",
            //    Password = "sa",
            //    AuthenticationType = Runtime.ServiceModel.Data.AuthenticationType.User
            //};

            //var dbSources = new ObservableCollection<IDbSource> { _postgresSqlSource };

            //mockDbServiceModel.Setup(model => model.RetrieveSources()).Returns(dbSources);
            //mockDbServiceModel.Setup(model => model.GetActions(_postgresSqlSource));
            //mockServiceInputViewModel.SetupAllProperties();

            //var postgresDesignerViewModel = new PostgreSqlDatabaseDesignerViewModel(modelItem, mockDbServiceModel.Object);

            //ScenarioContext.Current.Add("viewModel", postgresDesignerViewModel);
            //ScenarioContext.Current.Add("mockServiceInputViewModel", mockServiceInputViewModel);
            //ScenarioContext.Current.Add("mockDbServiceModel", mockDbServiceModel);
        }

        [Given(@"Source is enabled for mysql connector tool")]
        public void GivenSourceIsEnabledForMysqlConnectorTool()
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"Action is Not enabled for mysql connector tool")]
        public void GivenActionIsNotEnabledForMysqlConnectorTool()
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"Input is Not eabled for mysql connector tool")]
        public void GivenInputIsNotEabledForMysqlConnectorTool()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I select Source on mysql connector tool")]
        public void ThenISelectSourceOnMysqlConnectorTool()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Action is Enabled on mysql connector tool")]
        public void ThenActionIsEnabledOnMysqlConnectorTool()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Input is Not enabled for mysql connector tool")]
        public void ThenInputIsNotEnabledForMysqlConnectorTool()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I select Action for mysql connector tool")]
        public void ThenISelectActionForMysqlConnectorTool()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Input is enabled for mysql connector tool")]
        public void ThenInputIsEnabledForMysqlConnectorTool()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Inputs are ""(.*)"" for mysql connector tool")]
        public void ThenInputsAreForMysqlConnectorTool(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I click validate on mysql connector tool")]
        public void ThenIClickValidateOnMysqlConnectorTool()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I click Test on mysql connector tool")]
        public void WhenIClickTestOnMysqlConnectorTool()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"The Connector and Calculate Outputs appear for mysql connector tool")]
        public void WhenTheConnectorAndCalculateOutputsAppearForMysqlConnectorTool()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I click Okay on mysql connector tool")]
        public void ThenIClickOkayOnMysqlConnectorTool()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"The recordset name appear as ""(.*)"" on mysql connector tool")]
        public void ThenTheRecordsetNameAppearAsOnMysqlConnectorTool(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"I open an existing mysql connector tool")]
        public void GivenIOpenAnExistingMysqlConnectorTool()
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"Source is enabled and set to ""(.*)"" on mysql connector tool")]
        public void GivenSourceIsEnabledAndSetToOnMysqlConnectorTool(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"Action is Enabled and set to ""(.*)"" on mysql connector tool")]
        public void GivenActionIsEnabledAndSetToOnMysqlConnectorTool(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"Input is Not enabled for mysql connector tool")]
        public void GivenInputIsNotEnabledForMysqlConnectorTool()
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"Input is enabled and set to ""(.*)"" on mysql connector tool")]
        public void GivenInputIsEnabledAndSetToOnMysqlConnectorTool(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Given(@"Inputs are ""(.*)"" for mysql connector tool")]
        public void GivenInputsAreForMysqlConnectorTool(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"The outputs appear as ""(.*)"" on mysql connector tool")]
        public void ThenTheOutputsAppearAsOnMysqlConnectorTool(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I select Action for mysql connector tool")]
        public void WhenISelectActionForMysqlConnectorTool()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"The recordset name changes to ""(.*)"" for mysql connector tool")]
        public void ThenTheRecordsetNameChangesToForMysqlConnectorTool(string p0)
        {
            ScenarioContext.Current.Pending();
        }

    }
}
