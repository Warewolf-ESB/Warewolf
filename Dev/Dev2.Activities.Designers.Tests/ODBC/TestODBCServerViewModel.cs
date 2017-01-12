using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.ODBC;
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
// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests.ODBC
{
    [TestClass]
    public class TestODBCServerViewModel
    {
        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        public void ODBCServer_MethodName_Scenerio_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            var act = new DsfODBCDatabaseActivity
            {
                SourceId = mod.Sources[0].Id,
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("d", "e", "f") },
                ServiceName = "dsfBob"
            };

            //------------Execute Test---------------------------
            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);

            //------------Assert Results-------------------------
            Assert.IsTrue(ODBCServer.SourceRegion.IsEnabled);
            Assert.IsTrue(ODBCServer.ActionRegion.IsEnabled);
            Assert.IsTrue(ODBCServer.ErrorRegion.IsEnabled);
            ODBCServer.ValidateTestComplete();
            Assert.IsTrue(ODBCServer.OutputsRegion.IsEnabled);
        }

        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        [ExpectedException(typeof(NullReferenceException))]
        public void ODBCServer_MethodName_Scenerio_Result_oneParementer()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            var act = new DsfODBCDatabaseActivity
            {
                SourceId = mod.Sources[0].Id,
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("d", "e", "f") },
                ServiceName = "dsfBob"
            };

            //------------Execute Test---------------------------
            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act));

            //------------Assert Results-------------------------
            Assert.IsTrue(ODBCServer.SourceRegion.IsEnabled);
            Assert.IsTrue(ODBCServer.ActionRegion.IsEnabled);
            Assert.IsTrue(ODBCServer.ErrorRegion.IsEnabled);
            ODBCServer.ValidateTestComplete();
            Assert.IsTrue(ODBCServer.OutputsRegion.IsEnabled);
        }

        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        public void ODBCServer_GenerateOutputsVisible()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            var act = new DsfODBCDatabaseActivity
            {
                SourceId = mod.Sources[0].Id,
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("d", "e", "f") },
                ServiceName = "dsfBob"
            };

            //------------Execute Test---------------------------
            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            ODBCServer.GenerateOutputsVisible = true;

            //------------Assert Results-------------------------

            Assert.IsFalse(ODBCServer.OutputsRegion.IsEnabled);
        }


        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        public void ODBCServer_GenerateOutputsInVisible()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            var act = new DsfODBCDatabaseActivity
            {
                SourceId = mod.Sources[0].Id,
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("d", "e", "f") },
                ServiceName = "dsfBob"
            };

            //------------Execute Test---------------------------
            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            ODBCServer.GenerateOutputsVisible = false;

            //------------Assert Results-------------------------

            Assert.IsTrue(ODBCServer.OutputsRegion.IsEnabled);
        }


        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        public void ODBCServer_HasErrorVisible()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            var act = new DsfODBCDatabaseActivity
            {
                SourceId = mod.Sources[0].Id,
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("d", "e", "f") },
                ServiceName = "dsfBob"
            };

            //------------Execute Test---------------------------
            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            ODBCServer.ErrorMessage(new Exception("Test"), true);

            //------------Assert Results-------------------------

            Assert.IsTrue(ODBCServer.ErrorRegion.IsEnabled);
        }

        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        public void ODBCServer_TestProcedure()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            var act = new DsfODBCDatabaseActivity
            {
                SourceId = mod.Sources[0].Id,
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("d", "e", "f") },
                ServiceName = "dsfBob"
            };

            //------------Execute Test---------------------------
            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            ODBCServer.TestProcedure();

            //------------Assert Results-------------------------

            Assert.IsTrue(ODBCServer.ErrorRegion.IsEnabled);
        }
        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        public void ODBCServer_TestProcedureNoError()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            var act = new DsfODBCDatabaseActivity
            {
                SourceId = mod.Sources[0].Id,
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("d", "e", "f") },
                ServiceName = "dsfBob"
            };

            //------------Execute Test---------------------------
            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            ODBCServer.CommandText = "dsfBob";
            ODBCServer.TestProcedure();

            //------------Assert Results-------------------------

            Assert.IsFalse(ODBCServer.ErrorRegion.IsEnabled);
        }

        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        public void ODBCServer_LabelWidth()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            var act = new DsfODBCDatabaseActivity
            {
                SourceId = mod.Sources[0].Id,
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("d", "e", "f") },
                ServiceName = "dsfBob"
            };

            //------------Execute Test---------------------------
            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            var width = ODBCServer.LabelWidth;

            //------------Assert Results-------------------------

            Assert.IsNotNull(width);
        }

        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        public void ODBCServer_WorstError()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            var act = new DsfODBCDatabaseActivity
            {
                SourceId = mod.Sources[0].Id,
                Inputs = new List<IServiceInput>() { new ServiceInput("[[a]]", "asa") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("d", "e", "f") },
                ServiceName = "dsfBob"
            };

            //------------Execute Test---------------------------
            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            var err = ODBCServer.IsWorstErrorReadOnly;
            var errType = ODBCServer.WorstError;

            //------------Assert Results-------------------------
            Assert.IsNotNull(errType);
            Assert.IsTrue(err);
        }



        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        public void ODBCServer_MethodName_ValidateExpectErrors()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            var act = new DsfODBCDatabaseActivity();

            //------------Execute Test---------------------------
            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            ODBCServer.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(ODBCServer.Errors.Count, 1);
            Assert.AreEqual(ODBCServer.DesignValidationErrors.Count, 2);
        }

        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        public void ODBCServer_MethodName_ClearErrors()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            var act = new DsfODBCDatabaseActivity();

            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            //------------Execute Test---------------------------
            ODBCServer.ClearValidationMemoWithNoFoundError();
            //------------Assert Results-------------------------
            Assert.IsNull(ODBCServer.Errors);
            Assert.AreEqual(ODBCServer.DesignValidationErrors.Count, 1);
        }

        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        public void ODBCServer_Ctor_EmptyModelItem()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            var act = new DsfODBCDatabaseActivity();

            //------------Execute Test---------------------------
            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);

            //------------Assert Results-------------------------
            Assert.IsTrue(ODBCServer.SourceRegion.IsEnabled);
            Assert.IsFalse(ODBCServer.OutputsRegion.IsEnabled);
            Assert.IsFalse(ODBCServer.InputArea.IsEnabled);
            Assert.IsTrue(ODBCServer.ErrorRegion.IsEnabled);
        }

        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        public void ODBCServer_TestActionSetSource()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            var act = new DsfODBCDatabaseActivity();

            //------------Execute Test---------------------------
            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            ODBCServer.ManageServiceInputViewModel = new InputViewForTest(ODBCServer, mod);
            ODBCServer.SourceRegion.SelectedSource = ODBCServer.SourceRegion.Sources.First();

            //------------Assert Results-------------------------
            Assert.IsTrue(ODBCServer.SourceRegion.IsEnabled);
            Assert.IsFalse(ODBCServer.OutputsRegion.IsEnabled);
            Assert.IsTrue(ODBCServer.ErrorRegion.IsEnabled);
        }

        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        public void ODBCServer_TestActionSetSourceAndTestClickOkHasMappings()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            var act = new DsfODBCDatabaseActivity();

            //------------Execute Test---------------------------
            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);

            ODBCServer.SourceRegion.SelectedSource = ODBCServer.SourceRegion.Sources.First();
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(ODBCServer.SourceRegion.IsEnabled);
            Assert.IsTrue(ODBCServer.ErrorRegion.IsEnabled);
            Assert.AreEqual(0, ODBCServer.ManageServiceInputViewModel.Errors.Count);
        }

        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        public void ODBCServer_TestActionSetSourceAndTestClickOkHasMappingsErrorFromServer()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            mod.HasRecError = true;
            var act = new DsfODBCDatabaseActivity();

            //------------Execute Test---------------------------
            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            ODBCServer.SourceRegion.SelectedSource = ODBCServer.SourceRegion.Sources.First();
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(ODBCServer.ErrorRegion.IsEnabled);
            Assert.AreEqual(0, ODBCServer.ManageServiceInputViewModel.Errors.Count);
        }

        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        public void ODBCServer_TestActionSetSourceAndTestClickOkHasserialisationIssue()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            var act = new DsfODBCDatabaseActivity();

            //------------Execute Test---------------------------
            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);

            ODBCServer.SourceRegion.SelectedSource = ODBCServer.SourceRegion.Sources.First();
            ODBCServer.CommandText = "[[a]]";
#pragma warning disable 4014


            //------------Assert Results-------------------------
            Assert.IsTrue(ODBCServer.ErrorRegion.IsEnabled);
            Assert.AreEqual(0, ODBCServer.ManageServiceInputViewModel.Errors.Count);
        }

        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        public void ODBCServer_TestActionSetSourceAndTestClickOkHasHeaders()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            var act = new DsfODBCDatabaseActivity();

            //------------Execute Test---------------------------
            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            ODBCServer.ManageServiceInputViewModel = new InputViewForTest(ODBCServer, mod);
            ODBCServer.SourceRegion.SelectedSource = ODBCServer.SourceRegion.Sources.First();
            ODBCServer.CommandText = "[[a]]";

#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(ODBCServer.SourceRegion.IsEnabled);
            Assert.IsTrue(ODBCServer.ErrorRegion.IsEnabled);
            Assert.AreEqual(0, ODBCServer.ManageServiceInputViewModel.Errors.Count);
        }

        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        public void ODBCServer_TestActionSetSourceAndTestClickOkHasQueryStringAndHeaders()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            var act = new DsfODBCDatabaseActivity();

            //------------Execute Test---------------------------
            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            ODBCServer.SourceRegion.SelectedSource = ODBCServer.SourceRegion.Sources.First();
            ODBCServer.CommandText = "[[a]]";
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(ODBCServer.SourceRegion.IsEnabled);
            Assert.IsTrue(ODBCServer.ErrorRegion.IsEnabled);
            Assert.AreEqual(0, ODBCServer.ManageServiceInputViewModel.Errors.Count);
        }

        [TestMethod]
        [TestCategory("ODBCServer_MethodName")]
        public void ODBCServer_TestActionSetSourceAndTestClickOkHasQueryStringAndHeadersRecSet()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new ODBCServerModel();
            var act = new DsfODBCDatabaseActivity();

            //------------Execute Test---------------------------
            var ODBCServer = new ODBCDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod);
            ODBCServer.ManageServiceInputViewModel = new InputViewForTest(ODBCServer, mod);
            ODBCServer.SourceRegion.SelectedSource = ODBCServer.SourceRegion.Sources.First();
            ODBCServer.CommandText = "[[a]]";
#pragma warning restore 4014

            //------------Assert Results-------------------------
            Assert.IsTrue(ODBCServer.SourceRegion.IsEnabled);
            Assert.IsTrue(ODBCServer.ErrorRegion.IsEnabled);
            Assert.AreEqual(0, ODBCServer.ManageServiceInputViewModel.Errors.Count);
        }

    }

    public class ODBCServerModel : IDbServiceModel
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
                Type = enSourceType.ODBC,
                UserName = "johnny",
                Password = "bravo",
                AuthenticationType = AuthenticationType.User,
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
