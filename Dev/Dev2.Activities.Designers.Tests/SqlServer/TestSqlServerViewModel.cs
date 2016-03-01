using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
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
        public void SqlServer_MethodName_Scenerio_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new SqlServerModel();
            var act = new DsfSqlServerDatabaseActivity
            {
                SourceId = mod.Sources[0].Id,
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("d", "e", "f") },
                ServiceName = "dsfBob"
            };

            //------------Execute Test---------------------------
            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);

            //------------Assert Results-------------------------
            Assert.AreEqual(280, sqlServer.DesignMaxHeight);
            Assert.AreEqual(280, sqlServer.DesignMinHeight);
            Assert.AreEqual(280, sqlServer.DesignHeight);
            Assert.IsTrue(sqlServer.SourceRegion.IsVisible);
            Assert.IsTrue(sqlServer.ActionRegion.IsVisible);
            Assert.IsTrue(sqlServer.InputArea.IsVisible);
            Assert.IsTrue(sqlServer.ErrorRegion.IsVisible);
            sqlServer.ValidateTestComplete();
            Assert.IsTrue(sqlServer.OutputsRegion.IsVisible);
        }

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
            Assert.AreEqual(sqlServer.Errors.Count, 1);
            Assert.AreEqual(sqlServer.DesignValidationErrors.Count, 2);
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
            Assert.AreEqual(175, sqlServer.DesignMaxHeight);
            Assert.AreEqual(175, sqlServer.DesignMinHeight);
            Assert.AreEqual(175, sqlServer.DesignHeight);
            Assert.IsTrue(sqlServer.SourceRegion.IsVisible);
            Assert.IsFalse(sqlServer.OutputsRegion.IsVisible);
            Assert.IsFalse(sqlServer.InputArea.IsVisible);
            Assert.IsTrue(sqlServer.ErrorRegion.IsVisible);
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
            Assert.AreEqual(175, sqlServer.DesignMaxHeight);
            Assert.AreEqual(175, sqlServer.DesignMinHeight);
            Assert.AreEqual(175, sqlServer.DesignHeight);
            Assert.IsTrue(sqlServer.SourceRegion.IsVisible);
            Assert.IsFalse(sqlServer.OutputsRegion.IsVisible);
            Assert.IsFalse(sqlServer.InputArea.IsVisible);
            Assert.IsTrue(sqlServer.ErrorRegion.IsVisible);
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
            sqlServer.ManageServiceInputViewModel.IsVisible = true;
            sqlServer.ManageServiceInputViewModel.SetInitialVisibility();
            sqlServer.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            sqlServer.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.AreEqual(400, sqlServer.DesignMaxHeight);
            Assert.AreEqual(375, sqlServer.DesignMinHeight);
            Assert.AreEqual(375, sqlServer.DesignHeight);
            Assert.IsTrue(sqlServer.SourceRegion.IsVisible);
            Assert.IsTrue(sqlServer.OutputsRegion.IsVisible);
            Assert.IsTrue(sqlServer.InputArea.IsVisible);
            Assert.IsTrue(sqlServer.ErrorRegion.IsVisible);
            Assert.IsFalse(sqlServer.ManageServiceInputViewModel.InputArea.IsVisible);
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
            sqlServer.ManageServiceInputViewModel.IsVisible = true;
            sqlServer.ManageServiceInputViewModel.SetInitialVisibility();
            sqlServer.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            sqlServer.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            
            //------------Assert Results-------------------------
            Assert.IsTrue(sqlServer.ErrorRegion.IsVisible);
        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SqlServer_MethodName")]
        public void SqlServer_TestActionSetSourceAndTestClickOkHasserialisationIssue()
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
#pragma warning disable 4014
            sqlServer.TestInputCommand.Execute();
            sqlServer.ManageServiceInputViewModel.TestCommand.Execute(null);
            sqlServer.ManageServiceInputViewModel.IsVisible = true;
            sqlServer.ManageServiceInputViewModel.SetInitialVisibility();
            sqlServer.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            
            //------------Assert Results-------------------------
            Assert.AreEqual(sqlServer.OutputsRegion.Outputs.First().MappedFrom, "Result");
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
            sqlServer.ManageServiceInputViewModel.IsVisible = true;
            sqlServer.ManageServiceInputViewModel.SetInitialVisibility();
            sqlServer.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            sqlServer.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.AreEqual(400, sqlServer.DesignMaxHeight);
            Assert.AreEqual(375, sqlServer.DesignMinHeight);
            Assert.AreEqual(375, sqlServer.DesignHeight);
            Assert.IsTrue(sqlServer.SourceRegion.IsVisible);
            Assert.IsTrue(sqlServer.OutputsRegion.IsVisible);
            Assert.IsTrue(sqlServer.InputArea.IsVisible);
            Assert.IsTrue(sqlServer.ErrorRegion.IsVisible);
            Assert.AreEqual(1, sqlServer.ManageServiceInputViewModel.InputArea.Inputs.Count);
            Assert.IsTrue(sqlServer.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[a]]");
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
            sqlServer.ManageServiceInputViewModel.IsVisible = true;
            sqlServer.ManageServiceInputViewModel.SetInitialVisibility();
            sqlServer.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            sqlServer.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.AreEqual(400, sqlServer.DesignMaxHeight);
            Assert.AreEqual(375, sqlServer.DesignMinHeight);
            Assert.AreEqual(375, sqlServer.DesignHeight);
            Assert.IsTrue(sqlServer.SourceRegion.IsVisible);
            Assert.IsTrue(sqlServer.OutputsRegion.IsVisible);
            Assert.IsTrue(sqlServer.InputArea.IsVisible);
            Assert.IsTrue(sqlServer.ErrorRegion.IsVisible);
            Assert.IsTrue(sqlServer.ManageServiceInputViewModel.InputArea.Inputs.Count == 2);
            Assert.IsTrue(sqlServer.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[b]]");
            Assert.IsTrue(sqlServer.ManageServiceInputViewModel.InputArea.Inputs.Last().Name == "[[a]]");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SqlServer_MethodName")]
        public void SqlServer_TestActionSetSourceAndTestClickOkHasQueryStringAndHeadersRecSet()
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
            sqlServer.ManageServiceInputViewModel.IsVisible = true;
            sqlServer.ManageServiceInputViewModel.SetInitialVisibility();
            sqlServer.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            sqlServer.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.AreEqual(400, sqlServer.DesignMaxHeight);
            Assert.AreEqual(375, sqlServer.DesignMinHeight);
            Assert.AreEqual(375, sqlServer.DesignHeight);
            Assert.IsTrue(sqlServer.SourceRegion.IsVisible);
            Assert.IsTrue(sqlServer.OutputsRegion.IsVisible);
            Assert.IsTrue(sqlServer.InputArea.IsVisible);
            Assert.IsTrue(sqlServer.ErrorRegion.IsVisible);
            Assert.IsTrue(sqlServer.ManageServiceInputViewModel.InputArea.Inputs.Count == 2);
            Assert.IsTrue(sqlServer.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[b().a]]");
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
                Name = "",
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

        [ExcludeFromCodeCoverage]
        public void CreateNewSource()
        {
        }
        [ExcludeFromCodeCoverage]
        public void EditSource(IDbSource selectedSource)
        {
        }

        public DataTable TestService(IDatabaseService inputValues)
        {
            return _updateRepository.TestDbService(inputValues);
        }

        public IEnumerable<IServiceOutputMapping> GetDbOutputMappings(IDbAction action)
        {
            yield break;
        }
        [ExcludeFromCodeCoverage]
        public void SaveService(IDatabaseService toModel)
        {
        }
        [ExcludeFromCodeCoverage]
        public IStudioUpdateManager UpdateRepository
        {
            get
            {
                return _updateRepository;
            }
        }

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
