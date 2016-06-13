﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.SqlServerDatabase;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase.Database;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Core;
// ReSharper disable UnusedVariable
// ReSharper disable UseObjectOrCollectionInitializer

// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests.SqlServer
{
    [TestClass]
    public class TestSqlServerViewModel
    {


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SqlServer_MethodName")]
        public void SqlServer_MethodName_ValidateExpectErrors()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new SqlServerModel();
            var act = new DsfSqlServerDatabaseActivity();

            //------------Execute Test---------------------------
            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            sqlServer.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, sqlServer.Errors.Count);
            Assert.AreEqual(2, sqlServer.DesignValidationErrors.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SqlServer_MethodName")]
        public void SqlServer_MethodName_ClearErrors()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new SqlServerModel();
            var act = new DsfSqlServerDatabaseActivity();

            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            //------------Execute Test---------------------------
            sqlServer.ClearValidationMemoWithNoFoundError();
            //------------Assert Results-------------------------
            Assert.IsNull(sqlServer.Errors);
            Assert.AreEqual(sqlServer.DesignValidationErrors.Count, 1);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SqlServer_MethodName")]
        public void SqlServer_Ctor_EmptyModelItem()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new SqlServerModel();
            var act = new DsfSqlServerDatabaseActivity();

            //------------Execute Test---------------------------
            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);

            //------------Assert Results-------------------------
            Assert.IsTrue(sqlServer.SourceRegion.IsEnabled);
            Assert.IsFalse(sqlServer.OutputsRegion.IsEnabled);
            Assert.IsFalse(sqlServer.InputArea.IsEnabled);
            Assert.IsTrue(sqlServer.ErrorRegion.IsEnabled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SqlServer_MethodName")]
        public void SqlServer_TestActionSetSource()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new SqlServerModel();
            var act = new DsfSqlServerDatabaseActivity();

            //------------Execute Test---------------------------
            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            sqlServer.ManageServiceInputViewModel = new InputViewForTest(sqlServer, mod);
            sqlServer.SourceRegion.SelectedSource = sqlServer.SourceRegion.Sources.First();

            //------------Assert Results-------------------------
            Assert.IsTrue(sqlServer.SourceRegion.IsEnabled);
            Assert.IsFalse(sqlServer.OutputsRegion.IsEnabled);
            Assert.IsFalse(sqlServer.InputArea.IsEnabled);
            Assert.IsTrue(sqlServer.ErrorRegion.IsEnabled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SqlServer_MethodName")]
        public void SqlServer_TestActionSetSourceAndTestClickOkHasMappings()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new SqlServerModel();
            var act = new DsfSqlServerDatabaseActivity();

            //------------Execute Test---------------------------
            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            sqlServer.ManageServiceInputViewModel = new InputViewForTest(sqlServer, mod);
            sqlServer.SourceRegion.SelectedSource = sqlServer.SourceRegion.Sources.First();
#pragma warning disable 4014
            sqlServer.TestInputCommand.Execute();
            sqlServer.ManageServiceInputViewModel.TestCommand.Execute(null);
            sqlServer.ManageServiceInputViewModel.IsEnabled = true;
            sqlServer.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            sqlServer.ManageServiceInputViewModel.Model = new DatabaseService() { Action = new DbAction() { Inputs = new List<IServiceInput>(), Name = "bob" } };

            sqlServer.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(sqlServer.SourceRegion.IsEnabled);
            Assert.IsTrue(sqlServer.OutputsRegion.IsEnabled);
            Assert.IsTrue(sqlServer.InputArea.IsEnabled);
            Assert.IsTrue(sqlServer.ErrorRegion.IsEnabled);
            Assert.IsFalse(sqlServer.ManageServiceInputViewModel.InputArea.IsEnabled);
            Assert.AreEqual(0, sqlServer.ManageServiceInputViewModel.Errors.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SqlServer_MethodName")]
        public void SqlServer_TestActionSetSourceAndTestClickOkHasMappingsErrorFromServer()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new SqlServerModel();
            mod.HasRecError = true;
            var act = new DsfSqlServerDatabaseActivity();

            //------------Execute Test---------------------------
            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            sqlServer.ManageServiceInputViewModel = new InputViewForTest(sqlServer, mod);
            sqlServer.SourceRegion.SelectedSource = sqlServer.SourceRegion.Sources.First();
#pragma warning disable 4014
            sqlServer.TestInputCommand.Execute();
            sqlServer.ManageServiceInputViewModel.TestCommand.Execute(null);
            sqlServer.ManageServiceInputViewModel.IsEnabled = true;
            sqlServer.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            sqlServer.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            
            //------------Assert Results-------------------------
            Assert.IsTrue(sqlServer.ErrorRegion.IsEnabled);
            Assert.AreEqual(1, sqlServer.ManageServiceInputViewModel.Errors.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SqlServer_MethodName")]
        public void SqlServer_TestActionSetSourceAndTestClickOkHasHeaders()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new SqlServerModel();
            var act = new DsfSqlServerDatabaseActivity();

            //------------Execute Test---------------------------
            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            sqlServer.ManageServiceInputViewModel = new InputViewForTest(sqlServer, mod);
            sqlServer.SourceRegion.SelectedSource = sqlServer.SourceRegion.Sources.First();
            sqlServer.ActionRegion.SelectedAction = sqlServer.ActionRegion.Actions.First();
            sqlServer.InputArea.Inputs.Add(new ServiceInput("[[a]]", "asa"));
#pragma warning disable 4014
            sqlServer.TestInputCommand.Execute();
            sqlServer.ManageServiceInputViewModel.TestCommand.Execute(null);
            sqlServer.ManageServiceInputViewModel.IsEnabled = true;
            sqlServer.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            sqlServer.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(sqlServer.SourceRegion.IsEnabled);
            Assert.IsTrue(sqlServer.OutputsRegion.IsEnabled);
            Assert.IsTrue(sqlServer.InputArea.IsEnabled);
            Assert.IsTrue(sqlServer.ErrorRegion.IsEnabled);
            Assert.AreEqual(2, sqlServer.ManageServiceInputViewModel.InputArea.Inputs.Count);
            Assert.IsTrue(sqlServer.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[a]]");
            Assert.AreEqual(0, sqlServer.ManageServiceInputViewModel.Errors.Count);
        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SqlServer_MethodName")]
        public void SqlServer_TestActionSetSourceAndTestClickOkHasQueryStringAndHeaders()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new SqlServerModel();
            var act = new DsfSqlServerDatabaseActivity();

            //------------Execute Test---------------------------
            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            sqlServer.ManageServiceInputViewModel = new InputViewForTest(sqlServer, mod);
            sqlServer.SourceRegion.SelectedSource = sqlServer.SourceRegion.Sources.First();
            sqlServer.ActionRegion.SelectedAction = sqlServer.ActionRegion.Actions.First();
            sqlServer.InputArea.Inputs.Add(new ServiceInput("[[a]]", "asa"));
#pragma warning disable 4014
            sqlServer.TestInputCommand.Execute();
            sqlServer.ManageServiceInputViewModel.TestCommand.Execute(null);
            sqlServer.ManageServiceInputViewModel.IsEnabled = true;
            sqlServer.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            sqlServer.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(sqlServer.SourceRegion.IsEnabled);
            Assert.IsTrue(sqlServer.OutputsRegion.IsEnabled);
            Assert.IsTrue(sqlServer.InputArea.IsEnabled);
            Assert.IsTrue(sqlServer.ErrorRegion.IsEnabled);
            Assert.IsTrue(sqlServer.ManageServiceInputViewModel.InputArea.Inputs.Count == 2);
            Assert.IsTrue(sqlServer.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[a]]");
            Assert.IsTrue(sqlServer.ManageServiceInputViewModel.InputArea.Inputs.Last().Name == "[[a]]");
            Assert.AreEqual(0, sqlServer.ManageServiceInputViewModel.Errors.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SqlServer_TestAction")]
        public void SqlServer_TestActionSetSourceHasRecSet()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new SqlServerModel();
            var act = new DsfSqlServerDatabaseActivity();

            //------------Execute Test---------------------------
            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            sqlServer.ManageServiceInputViewModel = new InputViewForTest(sqlServer, mod);
            sqlServer.SourceRegion.SelectedSource = sqlServer.SourceRegion.Sources.First();
            sqlServer.ActionRegion.SelectedAction = sqlServer.ActionRegion.Actions.First();
            sqlServer.InputArea.Inputs.Add(new ServiceInput("[[a]]", "asa"));
#pragma warning disable 4014
            sqlServer.TestInputCommand.Execute();
            sqlServer.ManageServiceInputViewModel.TestCommand.Execute(null);
            sqlServer.ManageServiceInputViewModel.IsEnabled = true;
            sqlServer.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            sqlServer.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(sqlServer.SourceRegion.IsEnabled);
            Assert.IsTrue(sqlServer.OutputsRegion.IsEnabled);
            Assert.IsTrue(sqlServer.InputArea.IsEnabled);
            Assert.IsTrue(sqlServer.ErrorRegion.IsEnabled);
            Assert.IsTrue(sqlServer.ManageServiceInputViewModel.InputArea.Inputs.Count == 2);
            Assert.IsTrue(sqlServer.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[a]]");
            Assert.IsTrue(sqlServer.ManageServiceInputViewModel.InputArea.Inputs.Last().Name == "[[a]]");
        }

    }

    public class SqlServerModel : IDbServiceModel
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
                Type = enSourceType.SqlDatabase,
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

        public bool HasRecError { get; set; }

        #region Implementation of IDbServiceModel

        public ObservableCollection<IDbSource> RetrieveSources()
        {
            return Sources;
        }

        public ObservableCollection<IDbSource> Sources
        {
            get
            {
                return _sources;
            }
        }

        public ICollection<IDbAction> GetActions(IDbSource source)
        {
            return Actions;
        }

        public ICollection<IDbAction> Actions
        {
            get
            {
                return _actions;
            }
        }

        public void CreateNewSource()
        {
        }
        public void EditSource(IDbSource selectedSource)
        {
        }

        public DataTable TestService(IDatabaseService inputValues)
        {
            if(ThrowsTestError)
                throw new Exception("bob");
            DataTable dt = new DataTable();
            dt.Columns.Add("a");
            dt.Columns.Add("b");
            dt.Columns.Add("c");
            dt.TableName = "bob";
            return dt;

        }

        public IStudioUpdateManager UpdateRepository
        {
            get
            {
                return _updateRepository;
            }
        }
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
