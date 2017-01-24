using System.Collections.Generic;
using Dev2.Activities.Designers.Tests.SqlServer;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.SqlServerDatabase;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Core;
// ReSharper disable UseObjectOrCollectionInitializer

// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests.Core.Database
{
    [TestClass]
    public class ManageDatabaseServiceInputViewModelTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OutputsRegion_Ctor")]
        public void ManageDatabaseServiceInputViewModel_Ctor()
        {
            var mod = new SqlServerModel();
            var act = new DsfSqlServerDatabaseActivity
            {
                SourceId = mod.Sources[0].Id,
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("d", "e", "f") },
                ServiceName = "dsfBob"
            };

            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());

            //------------Assert Results-------------------------
            ManageDatabaseServiceInputViewModel vm = new ManageDatabaseServiceInputViewModel(sqlServer, mod);
            Assert.IsNotNull(vm.CloseCommand);
            Assert.IsNotNull(vm.CloseCommand);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OutputsRegion_Ctor")]
        public void ManageDatabaseServiceInputViewModel_TestAction()
        {
            bool called = false;
            bool calledOk = false;

            var mod = new SqlServerModel();
            var act = new DsfSqlServerDatabaseActivity
            {
                SourceId = mod.Sources[0].Id,
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("d", "e", "f") },
                ServiceName = "dsfBob"
            };

            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());

            ManageDatabaseServiceInputViewModel vm = new ManageDatabaseServiceInputViewModel(sqlServer, mod);
            vm.TestAction = () => { called = true; };
            vm.OkAction = () =>
            {
                calledOk = true;
            };
            vm.TestAction();
            vm.OkAction();

            //------------Assert Results-------------------------

            Assert.IsTrue(called);
            Assert.IsTrue(calledOk);
        }
        

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_MethodName")]
        public void ManageDatabaseServiceInputViewModel_PropertyChangedHandler()
        {
            //------------Setup for test--------------------------
            var mod = new SqlServerModel();

            var act = new DsfSqlServerDatabaseActivity();
            var called = false;
            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            var inputview = new ManageDatabaseServiceInputViewModel(sqlServer, mod);
            inputview.PropertyChanged += (sender, args) => called = true;
            inputview.Model = new DatabaseService();
            //------------Execute Test---------------------------
            inputview.ExecuteTest();

            //------------Assert Results-------------------------
            Assert.IsTrue(called);


        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_MethodName")]
        public void ManageDatabaseServiceInputViewModelCloneRegion_ReturnsNull()
        {
            //------------Setup for test--------------------------
            var mod = new SqlServerModel();

            var act = new DsfSqlServerDatabaseActivity();
            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            var inputview = new ManageDatabaseServiceInputViewModel(sqlServer, mod);
            inputview.Model = new DatabaseService();

            //------------Execute Test---------------------------
            var clone = inputview.CloneRegion();

            //------------Assert Results-------------------------
            Assert.AreEqual(inputview, clone);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_MethodName")]
        public void ManageDatabaseServiceInputViewModelTestAction_Exception()
        {
            //------------Setup for test--------------------------
            var mod = new SqlServerModel(){ThrowsTestError = true};
            mod.HasRecError = true;

            var act = new DsfSqlServerDatabaseActivity();
            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            var inputview = new ManageDatabaseServiceInputViewModel(sqlServer, mod);
            inputview.Model = null;

            //------------Execute Test---------------------------
            inputview.ExecuteTest();

            //------------Assert Results-------------------------
            Assert.IsTrue(inputview.Errors.Count == 1);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_MethodName")]
        public void ManageDatabaseServiceInputViewModelOkAction_Exception()
        {
            //------------Setup for test--------------------------
            var mod = new SqlServerModel();
            mod.HasRecError = true;

            var act = new DsfSqlServerDatabaseActivity();
            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            var inputview = new ManageDatabaseServiceInputViewModel(sqlServer, mod);
            sqlServer.OutputsRegion.Outputs = null;

            //------------Execute Test---------------------------
            inputview.ExecuteOk();

            //------------Assert Results-------------------------
            Assert.IsTrue(inputview.Errors.Count == 1);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_MethodName")]
        public void ManageDatabaseServiceInputViewModel_RestoreRegion_DoesNothing()
        {
            //------------Setup for test--------------------------
            var mod = new SqlServerModel();

            var act = new DsfSqlServerDatabaseActivity();
            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            var inputview = new ManageDatabaseServiceInputViewModel(sqlServer, mod);
            inputview.Model = new DatabaseService();

            //------------Execute Test---------------------------
            inputview.RestoreRegion(null);

            //------------Assert Results-------------------------
            Assert.IsTrue(true, "Error RestoreRegion should do nothing");

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_MethodName")]
        public void ManageDatabaseServiceInputViewModel_TestActionSetSourceAndTestClickOk()
        {
            //------------Setup for test--------------------------
            var mod = new SqlServerModel();

            var act = new DsfSqlServerDatabaseActivity();

            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            var inputview = new ManageDatabaseServiceInputViewModel(sqlServer, mod);
            inputview.Model = new DatabaseService(){Source = new DbSourceDefinition(), Action = new DbAction(){Inputs = new List<IServiceInput>(),Name ="bob"},};
            inputview.ExecuteTest();
            //------------Execute Test---------------------------

            Assert.IsTrue(inputview.TestPassed);
            Assert.IsFalse(inputview.TestFailed);
            Assert.AreEqual(string.Empty, inputview.TestMessage);
            Assert.IsFalse(inputview.ShowTestMessage);
            inputview.ExecuteOk();
            //------------Execute Ok---------------------------
            Assert.IsTrue(sqlServer.SourceRegion.IsEnabled);
            Assert.IsTrue(sqlServer.InputArea.IsEnabled);
            Assert.IsTrue(sqlServer.OutputsRegion.IsEnabled);
            Assert.IsTrue(sqlServer.ErrorRegion.IsEnabled);
            Assert.IsFalse(sqlServer.ManageServiceInputViewModel.InputArea.IsEnabled);

            Assert.IsFalse(inputview.TestPassed);
            Assert.IsFalse(inputview.TestFailed);
            Assert.AreEqual(string.Empty, inputview.TestMessage);
            Assert.IsFalse(inputview.ShowTestMessage);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SqlServer_MethodName")]
        public void ManageDatabaseServiceInputViewModel_TestActionSetSourceAndTestClickClose()
        {
            //------------Setup for test--------------------------
            var mod = new SqlServerModel();

            var act = new DsfSqlServerDatabaseActivity();

            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());
            var inputview = new ManageDatabaseServiceInputViewModel(sqlServer, mod);
            inputview.Model = new DatabaseService();
            inputview.ExecuteClose();
            //------------Execute Ok---------------------------
            Assert.IsNull(inputview.OutputArea.Outputs);
            Assert.IsTrue(sqlServer.SourceRegion.IsEnabled);
            Assert.IsFalse(sqlServer.OutputsRegion.IsEnabled);
            Assert.IsTrue(sqlServer.InputArea.IsEnabled);
            Assert.IsTrue(sqlServer.ErrorRegion.IsEnabled);
            Assert.IsFalse(sqlServer.ManageServiceInputViewModel.InputArea.IsEnabled);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OutputsRegion_Ctor")]
        public void ManageDatabaseServiceInputViewModel_Properties()
        {
            var mod = new SqlServerModel();
            var act = new DsfSqlServerDatabaseActivity()
            {
                SourceId = mod.Sources[0].Id,
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("d", "e", "f") },
                ServiceName = "dsfBob"
            };

            var sqlServer = new SqlServerDatabaseDesignerViewModel(ModelItemUtils.CreateModelItem(act), mod, new SynchronousAsyncWorker());

            ManageDatabaseServiceInputViewModel vm = new ManageDatabaseServiceInputViewModel(sqlServer, mod);
            var lst = new List<IServiceInput>();
            vm.InputArea.Inputs = lst;
            Assert.AreEqual(lst, vm.InputArea.Inputs);
            var lsto = new List<IServiceOutputMapping>();
            vm.OutputArea.Outputs = lsto;
            Assert.AreEqual(lsto, vm.OutputArea.Outputs);
            vm.TestResultsAvailable = true;
            Assert.IsTrue(vm.TestResultsAvailable);
            vm.OkSelected = true;
            Assert.IsTrue(vm.OkSelected);
            vm.IsTestResultsEmptyRows = true;
            Assert.IsTrue(vm.IsTestResultsEmptyRows);
            vm.IsTesting = true;
            Assert.IsTrue(vm.IsTesting);
            Assert.IsNotNull(vm.Model);
        }
    }
}
