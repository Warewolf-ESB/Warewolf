using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.MySqlDatabase;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase.Database;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;
// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests.MySql
{
    [TestClass]
    public class TestMySqlViewModel
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MySql_MethodName")]
        public void MySql_MethodName_ValidateExpectErrors()
        {
            //------------Setup for test--------------------------
            var mod = new MySqlModel();
            var act = new DsfMySqlDatabaseActivity();

            //------------Execute Test---------------------------
            var mySql = new MySqlDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            mySql.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, mySql.Errors.Count);
            Assert.AreEqual(2, mySql.DesignValidationErrors.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MySql_MethodName")]
        public void MySql_MethodName_ClearErrors()
        {
            //------------Setup for test--------------------------
            var mod = new MySqlModel();
            var act = new DsfMySqlDatabaseActivity();

            var mySql = new MySqlDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            //------------Execute Test---------------------------
            mySql.ClearValidationMemoWithNoFoundError();
            //------------Assert Results-------------------------
            Assert.IsNull(mySql.Errors);
            Assert.AreEqual(mySql.DesignValidationErrors.Count, 1);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MySql_MethodName")]
        public void MySql_Ctor_EmptyModelItem()
        {
            //------------Setup for test--------------------------
            var mod = new MySqlModel();
            var act = new DsfMySqlDatabaseActivity();

            //------------Execute Test---------------------------
            var mySql = new MySqlDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());

            //------------Assert Results-------------------------
            Assert.IsTrue(mySql.SourceRegion.IsEnabled);
            Assert.IsFalse(mySql.OutputsRegion.IsEnabled);
            Assert.IsFalse(mySql.InputArea.IsEnabled);
            Assert.IsTrue(mySql.ErrorRegion.IsEnabled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MySql_MethodName")]
        public void MySql_TestActionSetSource()
        {
            //------------Setup for test--------------------------
            var mod = new MySqlModel();
            var act = new DsfMySqlDatabaseActivity();

            //------------Execute Test---------------------------
            var mySql = new MySqlDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            mySql.ManageServiceInputViewModel = new InputViewForTest(mySql, mod);
            mySql.SourceRegion.SelectedSource = mySql.SourceRegion.Sources.First();

            //------------Assert Results-------------------------
            Assert.IsTrue(mySql.SourceRegion.IsEnabled);
            Assert.IsFalse(mySql.OutputsRegion.IsEnabled);
            Assert.IsFalse(mySql.InputArea.IsEnabled);
            Assert.IsTrue(mySql.ErrorRegion.IsEnabled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MySql_MethodName")]
        public void MySql_TestActionSetSourceAndTestClickOkHasMappings()
        {
            //------------Setup for test--------------------------
            var mod = new MySqlModel();
            var act = new DsfMySqlDatabaseActivity();

            //------------Execute Test---------------------------
            var mySql = new MySqlDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            mySql.ManageServiceInputViewModel = new InputViewForTest(mySql, mod);
            mySql.SourceRegion.SelectedSource = mySql.SourceRegion.Sources.First();
#pragma warning disable 4014
            mySql.TestInputCommand.Execute();
            mySql.ManageServiceInputViewModel.TestCommand.Execute(null);
            mySql.ManageServiceInputViewModel.IsEnabled = true;
            mySql.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            mySql.ManageServiceInputViewModel.Model = new DatabaseService() { Action = new DbAction() { Inputs = new List<IServiceInput>(), Name = "bob" } };

            mySql.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(mySql.SourceRegion.IsEnabled);
            Assert.IsTrue(mySql.OutputsRegion.IsEnabled);
            Assert.IsTrue(mySql.InputArea.IsEnabled);
            Assert.IsTrue(mySql.ErrorRegion.IsEnabled);
            Assert.IsFalse(mySql.ManageServiceInputViewModel.InputArea.IsEnabled);
            Assert.AreEqual(0, mySql.ManageServiceInputViewModel.Errors.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MySql_MethodName")]
        public void MySql_TestActionSetSourceAndTestClickOkHasMappingsErrorFromServer()
        {
            //------------Setup for test--------------------------
            var mod = new MySqlModel();
            mod.HasRecError = true;
            var act = new DsfMySqlDatabaseActivity();

            //------------Execute Test---------------------------
            var mySql = new MySqlDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            mySql.ManageServiceInputViewModel = new InputViewForTest(mySql, mod);
            mySql.SourceRegion.SelectedSource = mySql.SourceRegion.Sources.First();
#pragma warning disable 4014
            mySql.TestInputCommand.Execute();
            mySql.ManageServiceInputViewModel.TestCommand.Execute(null);
            mySql.ManageServiceInputViewModel.IsEnabled = true;
            mySql.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            mySql.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(mySql.ErrorRegion.IsEnabled);
            Assert.AreNotEqual(0, mySql.ManageServiceInputViewModel.Errors.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MySql_Handle")]
        public void MySql_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            var mod = new MySqlModel();
            mod.HasRecError = true;
            var act = new DsfMySqlDatabaseActivity();
            var viewModel = new MySqlDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MySql_MethodName")]
        public void MySql_TestActionSetSourceAndTestClickOkHasHeaders()
        {
            //------------Setup for test--------------------------
            var mod = new MySqlModel();
            var act = new DsfMySqlDatabaseActivity();

            //------------Execute Test---------------------------
            var mySql = new MySqlDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            mySql.ManageServiceInputViewModel = new InputViewForTest(mySql, mod);
            mySql.SourceRegion.SelectedSource = mySql.SourceRegion.Sources.First();
            mySql.ActionRegion.SelectedAction = mySql.ActionRegion.Actions.First();
            mySql.InputArea.Inputs.Add(new ServiceInput("[[a]]", "asa"));
#pragma warning disable 4014
            mySql.TestInputCommand.Execute();
            mySql.ManageServiceInputViewModel.TestCommand.Execute(null);
            mySql.ManageServiceInputViewModel.IsEnabled = true;
            mySql.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            mySql.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(mySql.SourceRegion.IsEnabled);
            Assert.IsTrue(mySql.OutputsRegion.IsEnabled);
            Assert.IsTrue(mySql.InputArea.IsEnabled);
            Assert.IsTrue(mySql.ErrorRegion.IsEnabled);
            Assert.AreEqual(2, mySql.ManageServiceInputViewModel.InputArea.Inputs.Count);
            Assert.IsTrue(mySql.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[a]]");
            Assert.AreEqual(0, mySql.ManageServiceInputViewModel.Errors.Count);
        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MySql_MethodName")]
        public void MySql_TestActionSetSourceAndTestClickOkHasQueryStringAndHeaders()
        {
            //------------Setup for test--------------------------
            var mod = new MySqlModel();
            var act = new DsfMySqlDatabaseActivity();

            //------------Execute Test---------------------------
            var mySql = new MySqlDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            mySql.ManageServiceInputViewModel = new InputViewForTest(mySql, mod);
            mySql.SourceRegion.SelectedSource = mySql.SourceRegion.Sources.First();
            mySql.ActionRegion.SelectedAction = mySql.ActionRegion.Actions.First();
            mySql.InputArea.Inputs.Add(new ServiceInput("[[a]]", "asa"));
#pragma warning disable 4014
            mySql.TestInputCommand.Execute();
            mySql.ManageServiceInputViewModel.TestCommand.Execute(null);
            mySql.ManageServiceInputViewModel.IsEnabled = true;
            mySql.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            mySql.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(mySql.SourceRegion.IsEnabled);
            Assert.IsTrue(mySql.OutputsRegion.IsEnabled);
            Assert.IsTrue(mySql.InputArea.IsEnabled);
            Assert.IsTrue(mySql.ErrorRegion.IsEnabled);
            Assert.IsTrue(mySql.ManageServiceInputViewModel.InputArea.Inputs.Count == 2);
            Assert.IsTrue(mySql.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[a]]");
            Assert.IsTrue(mySql.ManageServiceInputViewModel.InputArea.Inputs.Last().Name == "[[a]]");
            Assert.AreEqual(0, mySql.ManageServiceInputViewModel.Errors.Count);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SqlServer_Refresh")]
        public void MySql_Refresh_ShouldLoadRefreshActions()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new MySqlModel();
            var act = new DsfMySqlDatabaseActivity();
            var sqlServer = new MySqlDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            sqlServer.ManageServiceInputViewModel = new InputViewForTest(sqlServer, mod);
            sqlServer.SourceRegion.SelectedSource = sqlServer.SourceRegion.Sources.First();
            sqlServer.ActionRegion.IsRefreshing = false;
            //------------Execute Test---------------------------
            sqlServer.ActionRegion.RefreshActionsCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsTrue(sqlServer.SourceRegion.IsEnabled);
            Assert.AreEqual(1, sqlServer.ActionRegion.Actions.Count);
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MySql_TestAction")]
        public void MySql_TestActionSetSourceHasRecSet()
        {
            //------------Setup for test--------------------------
            var mod = new MySqlModel();
            var act = new DsfMySqlDatabaseActivity();

            //------------Execute Test---------------------------
            var mySql = new MySqlDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            mySql.ManageServiceInputViewModel = new InputViewForTest(mySql, mod);
            mySql.SourceRegion.SelectedSource = mySql.SourceRegion.Sources.First();
            mySql.ActionRegion.SelectedAction = mySql.ActionRegion.Actions.First();
            mySql.InputArea.Inputs.Add(new ServiceInput("[[a]]", "asa"));
#pragma warning disable 4014
            mySql.TestInputCommand.Execute();
            mySql.ManageServiceInputViewModel.TestCommand.Execute(null);
            mySql.ManageServiceInputViewModel.IsEnabled = true;
            mySql.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            mySql.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(mySql.SourceRegion.IsEnabled);
            Assert.IsTrue(mySql.OutputsRegion.IsEnabled);
            Assert.IsTrue(mySql.InputArea.IsEnabled);
            Assert.IsTrue(mySql.ErrorRegion.IsEnabled);
            Assert.IsTrue(mySql.ManageServiceInputViewModel.InputArea.Inputs.Count == 2);
            Assert.IsTrue(mySql.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[a]]");
            Assert.IsTrue(mySql.ManageServiceInputViewModel.InputArea.Inputs.Last().Name == "[[a]]");
        }

    }

    public class MySqlModel : IDbServiceModel
    {
#pragma warning disable 649
        private IStudioUpdateManager _updateRepository;
#pragma warning restore 649
#pragma warning disable 169
        private IQueryManager _queryProxy;
#pragma warning restore 169

        public ObservableCollection<IDbSource> _sources = new ObservableCollection<IDbSource>
        {
            new DbSourceDefinition()
            {
                ServerName = "localServer",
                Type = enSourceType.MySqlDatabase,
                UserName = "johnny",
                Password = "bravo",
                AuthenticationType = AuthenticationType.Public,
                DbName = "",
                Name = "j_bravo",
                Path = "",
                Id = Guid.NewGuid()
            }
        };

        public ObservableCollection<IDbAction> _actions = new ObservableCollection<IDbAction>
        {
            new DbAction()
            {
                Name = "mob",
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") }
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

        public ICollection<IDbAction> RefreshActions(IDbSource source)
        {
            return RefreshActionsList;
        }

        public ICollection<IDbAction> Actions => _actions;
        public ICollection<IDbAction> RefreshActionsList => _refreshActions;

        public void CreateNewSource(enSourceType type)
        {
        }
        public void EditSource(IDbSource selectedSource, enSourceType type)
        {
        }

        public DataTable TestService(IDatabaseService inputValues)
        {
            if (ThrowsTestError)
                throw new Exception("bob");
            if (HasRecError)
            {
                return null;
            }
            DataTable dt = new DataTable();
            dt.Columns.Add("a");
            dt.Columns.Add("b");
            dt.Columns.Add("c");
            dt.TableName = "bob";
            return dt;

        }

        public IStudioUpdateManager UpdateRepository => _updateRepository;
        public bool ThrowsTestError { get; set; }

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
