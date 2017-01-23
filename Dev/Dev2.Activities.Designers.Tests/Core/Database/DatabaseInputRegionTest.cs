using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Dev2.Activities.Designers2.Core.ActionRegion;
using Dev2.Activities.Designers2.Core.CloneInputRegion;
using Dev2.Activities.Designers2.Core.InputRegion;
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;

// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests.Core.Database
{
    [TestClass]
    public class DatabaseInputRegionTest
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DatabaseInputRegion_Constructor")]
        public void DatabaseInputRegion_Constructor_Scenerio_Result()
        {
            var id = Guid.NewGuid();
            var act = new DsfSqlServerDatabaseActivity() { SourceId = id };

            var src = new Mock<IDbServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IDbSource>());
            DatabaseSourceRegion sourceRegion = new DatabaseSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfSqlServerDatabaseActivity()), enSourceType.SqlDatabase);
            DbActionRegion dbActionRegion = new DbActionRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfSqlServerDatabaseActivity()), sourceRegion, new SynchronousAsyncWorker());

            var region = new DatabaseInputRegion(ModelItemUtils.CreateModelItem(act), dbActionRegion);
            Assert.AreEqual(region.IsEnabled, false);
            Assert.AreEqual(region.Errors.Count, 0);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DatabaseInputRegion_Constructor")]
        public void DatabaseInputRegion_Constructor_TestInput_ConstructorEmpty()
        {
            var src = new Mock<IDbServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IDbSource>());

            var region = new DatabaseInputRegion();
            Assert.AreEqual(region.IsEnabled, false);
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DatabaseInputRegion_TestClone")]
        public void DatabaseInputRegion_TestClone()
        {
            var id = Guid.NewGuid();
            var act = new DsfSqlServerDatabaseActivity() { SourceId = id };
            var src = new Mock<IDbServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IDbSource>());
            DatabaseSourceRegion sourceRegion = new DatabaseSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfSqlServerDatabaseActivity()), enSourceType.SqlDatabase);
            DbActionRegion dbActionRegion = new DbActionRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfSqlServerDatabaseActivity()), sourceRegion, new SynchronousAsyncWorker());

            var region = new DatabaseInputRegion(ModelItemUtils.CreateModelItem(act), dbActionRegion);
            Assert.AreEqual(region.IsEnabled, false);
            Assert.AreEqual(region.Errors.Count, 0);
            var clone = region.CloneRegion() as DatabaseInputRegion;
            if (clone != null)
            {
                Assert.AreEqual(clone.IsEnabled, false);
                Assert.AreEqual(clone.Errors.Count, 0);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DatabaseInputRegion_Test")]
        public void DatabaseInputRegion_Test_InputAddHeader_ExpectHeightChanges()
        {
            var id = Guid.NewGuid();
            var act = new DsfSqlServerDatabaseActivity() { SourceId = id };
            var src = new Mock<IDbServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IDbSource>());
            DatabaseSourceRegion sourceRegion = new DatabaseSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfSqlServerDatabaseActivity()), enSourceType.SqlDatabase);
            DbActionRegion dbActionRegion = new DbActionRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfSqlServerDatabaseActivity()), sourceRegion, new SynchronousAsyncWorker());

            var region = new DatabaseInputRegion(ModelItemUtils.CreateModelItem(act), dbActionRegion);
            Assert.AreEqual(region.IsEnabled, false);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DatabaseInputRegion_Test")]
        public void DatabaseInputRegion_Test_InputAddHeader_ExpectHeightChangesPastThree()
        {
            var id = Guid.NewGuid();
            var act = new DsfSqlServerDatabaseActivity() { SourceId = id };
            var src = new Mock<IDbServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IDbSource>());
            DatabaseSourceRegion sourceRegion = new DatabaseSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfSqlServerDatabaseActivity()), enSourceType.SqlDatabase);
            DbActionRegion dbActionRegion = new DbActionRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfSqlServerDatabaseActivity()), sourceRegion, new SynchronousAsyncWorker());

            var region = new DatabaseInputRegion(ModelItemUtils.CreateModelItem(act), dbActionRegion);
            Assert.AreEqual(region.IsEnabled, false);
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DatabaseInputRegion_RestoreFromPrevious")]
        public void DatabaseInputRegion_RestoreFromPrevious_Restore_ExpectValuesChanged()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfSqlServerDatabaseActivity() { SourceId = id };
            var src = new Mock<IDbServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IDbSource>());
            DatabaseSourceRegion sourceRegion = new DatabaseSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfSqlServerDatabaseActivity()), enSourceType.SqlDatabase);
            DbActionRegion dbActionRegion = new DbActionRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfSqlServerDatabaseActivity()), sourceRegion, new SynchronousAsyncWorker());

            var region = new DatabaseInputRegion(ModelItemUtils.CreateModelItem(act), dbActionRegion);
            // ReSharper disable once UseObjectOrCollectionInitializer
            var regionToRestore = new DatabaseInputRegionClone();
            regionToRestore.IsEnabled = true;
            //------------Execute Test---------------------------
            region.RestoreRegion(regionToRestore);
            //------------Assert Results-------------------------

        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DatabaseInputRegion_SourceChanged")]
        public void DatabaseInputRegion_SourceChanged_UpdateValues()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfSqlServerDatabaseActivity() { SourceId = id };
            var src = new Mock<IDbServiceModel>();
            var lst = new ObservableCollection<IDbSource>() { new DbSourceDefinition() { Name = "bravo" }, new DbSourceDefinition() { Name = "johnny" } };
            src.Setup(a => a.RetrieveSources()).Returns(lst);
            DatabaseSourceRegion sourceRegion = new DatabaseSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfSqlServerDatabaseActivity()), enSourceType.SqlDatabase);
            DbActionRegion dbActionRegion = new DbActionRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfSqlServerDatabaseActivity()), sourceRegion, new SynchronousAsyncWorker());

            var region = new DatabaseInputRegion(ModelItemUtils.CreateModelItem(act), dbActionRegion);

            sourceRegion.SelectedSource = lst[0];
            Assert.AreEqual(region.Inputs.Count, 0);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UpdateOnActionSelection_GivenHasInputs_ShouldWriteToActiveDatalist()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IDataListViewModel>();
            mock.Setup(model => model.ScalarCollection).Returns(new ObservableCollection<IScalarItemModel>());
            DataListSingleton.SetDataList(mock.Object);

            var id = Guid.NewGuid();
            var act = new DsfSqlServerDatabaseActivity() { SourceId = id };
            var modelItem = ModelItemUtils.CreateModelItem(act);
            var actionRegion = new Mock<IActionToolRegion<IDbAction>>();
            actionRegion.Setup(region => region.SelectedAction).Returns(ValueFunction);

            //---------------Assert Precondition----------------

            // ReSharper disable once PossibleNullReferenceException
            var countBefore = DataListSingleton.ActiveDataList.ScalarCollection.Count;
            Assert.AreEqual(0, countBefore);
            //---------------Execute Test ----------------------
            var inputRegion = new DatabaseInputRegion(modelItem, actionRegion.Object);

            var methodInfo = typeof(DatabaseInputRegion).GetMethod("UpdateOnActionSelection", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(methodInfo);
            methodInfo.Invoke(inputRegion, new object[] { });
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UpdateOnActionSelection_GivenHasInputs_ShouldWriteToActiveDatalistAndPopulatesInputValues()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IDataListViewModel>();
            mock.Setup(model => model.ScalarCollection).Returns(new ObservableCollection<IScalarItemModel>());
            DataListSingleton.SetDataList(mock.Object);

            var id = Guid.NewGuid();
            var act = new DsfSqlServerDatabaseActivity() { SourceId = id };
            var modelItem = ModelItemUtils.CreateModelItem(act);
            var actionRegion = new Mock<IActionToolRegion<IDbAction>>();
            actionRegion.Setup(region => region.SelectedAction).Returns(ValueFunction);

            //---------------Assert Precondition----------------

            // ReSharper disable once PossibleNullReferenceException
            var countBefore = DataListSingleton.ActiveDataList.ScalarCollection.Count;
            Assert.AreEqual(0, countBefore);
            //---------------Execute Test ----------------------
            var inputRegion = new DatabaseInputRegion(modelItem, actionRegion.Object);

            var methodInfo = typeof(DatabaseInputRegion).GetMethod("UpdateOnActionSelection", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(methodInfo);
            methodInfo.Invoke(inputRegion, new object[] { });
            //---------------Test Result -----------------------
            Assert.AreEqual("[[name]]", inputRegion.Inputs.ToList()[0].Value);
            Assert.AreEqual("[[surname]]", inputRegion.Inputs.ToList()[1].Value);

        }

        private IDbAction ValueFunction()
        {
            return new DbAction
            {
                Name = "PrintName",
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput("name",""),
                    new ServiceInput("surname",""),
                },
                SourceId = Guid.NewGuid()
            };
        }
    }
}
