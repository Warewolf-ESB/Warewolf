using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.PostgreSql;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase.Database;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Core;
// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests.PostgresSqlTests
{
    [TestClass]
    public class PostgresSqlDatabaseDesignerViewModelTests
    {
        public const string TestOwner = "Bernardt Joubert";
        public const string Category = "PostgresSql";

        public PostgreSqlDatabaseDesignerViewModel GetViewModel()
        {
            var mod = new PostgreSqlModel();
            var act = new DsfPostgreSqlActivity();

            //------------Execute Test---------------------------
            return new PostgreSqlDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void PostgresSql_Instantiate_New_ReturnsNewViewModel()
        {
            var model = GetViewModel();

            Assert.IsNotNull(model);
        }


        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void PostgreSql_MethodName_ValidateExpectErrors()
        {
            //------------Setup for test--------------------------
            var model = GetViewModel();
            model.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, model.Errors.Count);
            Assert.AreEqual(2, model.DesignValidationErrors.Count);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void PostgreSql_MethodName_ClearErrors()
        {
            //------------Setup for test--------------------------
            var model = GetViewModel();
            model.Validate();

            //------------Execute Test---------------------------
            model.ClearValidationMemoWithNoFoundError();
            //------------Assert Results-------------------------
            Assert.IsNotNull(model.Errors);
            Assert.AreEqual(model.DesignValidationErrors.Count, 1);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void PostgreSql_TestActionSetSource()
        {
            //------------Setup for test--------------------------
            var mod = new PostgreSqlModel();
            var act = new DsfPostgreSqlActivity();

            //------------Execute Test---------------------------

            var model = new PostgreSqlDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            model.ManageServiceInputViewModel = new InputViewForTest(model, mod);
            model.SourceRegion.SelectedSource = model.SourceRegion.Sources.First();

            //------------Assert Results-------------------------
            Assert.IsTrue(model.SourceRegion.IsEnabled);
            Assert.IsFalse(model.OutputsRegion.IsEnabled);
            Assert.IsFalse(model.InputArea.IsEnabled);
            Assert.IsTrue(model.ErrorRegion.IsEnabled);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void PostgreSql_TestActionSetSourceAndTestClickOkHasMappings()
        {
            //------------Setup for test--------------------------
            var mod = new PostgreSqlModel();
            var act = new DsfPostgreSqlActivity();

            //------------Execute Test---------------------------
            var model = new PostgreSqlDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            model.ManageServiceInputViewModel = new InputViewForTest(model, mod);
            model.SourceRegion.SelectedSource = model.SourceRegion.Sources.First();
#pragma warning disable 4014
            model.TestInputCommand.Execute();
            model.ManageServiceInputViewModel.TestCommand.Execute(null);
            model.ManageServiceInputViewModel.IsEnabled = true;
            model.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            model.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(model.SourceRegion.IsEnabled);
            Assert.IsFalse(model.OutputsRegion.IsEnabled);
            Assert.IsFalse(model.InputArea.IsEnabled);
            Assert.IsTrue(model.ErrorRegion.IsEnabled);
            Assert.IsTrue(model.ManageServiceInputViewModel.InputArea.IsEnabled);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void PostgreSql_TestActionSetSourceAndTestClickOkHasMappingsErrorFromServer()
        {
            //------------Setup for test--------------------------
            var mod = new PostgreSqlModel();
            mod.HasRecError = true;
            var act = new DsfPostgreSqlActivity();

            //------------Execute Test---------------------------
            var model = new PostgreSqlDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            model.ManageServiceInputViewModel = new InputViewForTest(model, mod);
            model.SourceRegion.SelectedSource = model.SourceRegion.Sources.First();
#pragma warning disable 4014
            model.TestInputCommand.Execute();
            model.ManageServiceInputViewModel.TestCommand.Execute(null);
            model.ManageServiceInputViewModel.IsEnabled = true;
            model.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            model.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(model.ErrorRegion.IsEnabled);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void PostgreSql_TestActionSetSourceAndTestClickOkHasHeaders()
        {
            //------------Setup for test--------------------------
            var mod = new PostgreSqlModel();
            var act = new DsfPostgreSqlActivity();

            //------------Execute Test---------------------------
            var model = new PostgreSqlDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            model.ManageServiceInputViewModel = new InputViewForTest(model, mod);
            model.SourceRegion.SelectedSource = model.SourceRegion.Sources.First();
            model.ActionRegion.SelectedAction = model.ActionRegion.Actions.First();
            model.InputArea.Inputs.Add(new ServiceInput("[[a]]", "asa"));
#pragma warning disable 4014
            model.TestInputCommand.Execute();
            model.ManageServiceInputViewModel.TestCommand.Execute(null);
            model.ManageServiceInputViewModel.IsEnabled = true;
            model.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            model.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsFalse(model.SourceRegion.IsEnabled);
            Assert.IsFalse(model.OutputsRegion.IsEnabled);
            Assert.IsFalse(model.InputArea.IsEnabled);
            Assert.IsFalse(model.ErrorRegion.IsEnabled);
            Assert.AreEqual(2, model.ManageServiceInputViewModel.InputArea.Inputs.Count);
            Assert.IsTrue(model.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[fname]]");
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SqlServer_Refresh")]
        public void PostgreSql_Refresh_ShouldLoadRefreshActions()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new PostgreSqlModel();
            var act = new DsfPostgreSqlActivity();
            var sqlServer = new PostgreSqlDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            sqlServer.ManageServiceInputViewModel = new InputViewForTest(sqlServer, mod);
            sqlServer.SourceRegion.SelectedSource = sqlServer.SourceRegion.Sources.First();
            //------------Execute Test---------------------------
            sqlServer.ActionRegion.RefreshActionsCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsTrue(sqlServer.SourceRegion.IsEnabled);
            Assert.AreEqual(1, sqlServer.ActionRegion.Actions.Count);
        }

        [TestMethod]
        [Owner(TestOwner)]
        [TestCategory(Category)]
        public void PostgreSql_TestActionSetSourceAndTestClickOkHasQueryStringAndHeaders()
        {
            //------------Setup for test--------------------------
            var mod = new PostgreSqlModel();
            var act = new DsfPostgreSqlActivity();

            //------------Execute Test---------------------------
            var model = new PostgreSqlDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            model.ManageServiceInputViewModel = new InputViewForTest(model, mod);
            model.SourceRegion.SelectedSource = model.SourceRegion.Sources.First();
            model.ActionRegion.SelectedAction = model.ActionRegion.Actions.First();
            model.InputArea.Inputs.Add(new ServiceInput("[[a]]", "asa"));
#pragma warning disable 4014
            model.TestInputCommand.Execute();
#pragma warning restore 4014

            model.ManageServiceInputViewModel.TestCommand.Execute(null);
            model.ManageServiceInputViewModel.IsEnabled = true;
            model.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            model.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsFalse(model.SourceRegion.IsEnabled);
            Assert.IsFalse(model.OutputsRegion.IsEnabled);
            Assert.IsFalse(model.InputArea.IsEnabled);
            Assert.IsFalse(model.ErrorRegion.IsEnabled);
            Assert.IsTrue(model.ManageServiceInputViewModel.InputArea.Inputs.Count == 2);
            Assert.IsTrue(model.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[fname]]");
            Assert.IsTrue(model.ManageServiceInputViewModel.InputArea.Inputs.Last().Name == "[[a]]");
        }
    }

    public class PostgreSqlModel : IDbServiceModel
    {
#pragma warning disable 649
        private IStudioUpdateManager _updateRepository;
#pragma warning restore 649
#pragma warning disable 169
        private IQueryManager _queryProxy;
#pragma warning restore 169

        private readonly ObservableCollection<IDbSource> _sources = new ObservableCollection<IDbSource>
        {
            new DbSourceDefinition()
            {
                Name = "DemoPostgres",
                Type = enSourceType.PostgreSQL,
                ServerName = "Localhost",
                UserName = "postgres",
                Password = "sa",
                AuthenticationType = AuthenticationType.User
            }
        };

        private readonly ObservableCollection<IDbAction> _actions = new ObservableCollection<IDbAction>
        {
            new DbAction()
            {
                Name = "getemployees",
                Inputs = new List<IServiceInput>() { new ServiceInput("[[fname]]", "Bill") }
            }
        };

        public ObservableCollection<IDbAction> _refreshActions = new ObservableCollection<IDbAction>
        {
            new DbAction()
            {
                Name = "mob",
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") }
            },
            new DbAction()
            {
                Name = "arefreshOne",
                Inputs = new List<IServiceInput>() { new ServiceInput("[[b]]", "bsb") }
            }
        };

        public ICollection<IDbAction> RefreshActions(IDbSource source)
        {
            return RefreshActionsList;
        }
        public ICollection<IDbAction> RefreshActionsList => _refreshActions;
        public bool HasRecError { get; set; }

        #region Implementation of IDbServiceModel

        public ObservableCollection<IDbSource> RetrieveSources()
        {
            return Sources;
        }

        public ObservableCollection<IDbSource> Sources => _sources;

        public ICollection<IDbAction> GetActions(IDbSource source)
        {
            return Actions;
        }

        public ICollection<IDbAction> Actions => _actions;

        public void CreateNewSource(enSourceType type)
        {
        }
        public void EditSource(IDbSource selectedSource, enSourceType type)
        {
        }

        public DataTable TestService(IDatabaseService inputValues)
        {
            return _updateRepository.TestDbService(inputValues);
        }

        public IStudioUpdateManager UpdateRepository => _updateRepository;

        #endregion
    }

    public class InputViewForTest : ManageDatabaseServiceInputViewModel
    {
        public InputViewForTest(IDatabaseServiceViewModel model, IDbServiceModel serviceModel)
            : base(model, serviceModel)
        {
        }
    }
}
