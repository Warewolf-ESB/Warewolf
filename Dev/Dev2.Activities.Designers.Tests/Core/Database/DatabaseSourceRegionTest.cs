using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;



namespace Dev2.Activities.Designers.Tests.Core.Database
{
    [TestClass]
    public class DatabaseSourceRegionTest
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DatabaseSourceRegion_Constructor")]
        public void DatabaseSourceRegion_Constructor_Scenerio_Result()
        {
            //------------Setup for test--------------------------
            var src = new Mock<IDbServiceModel>();
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IDbSource>());

            //------------Execute Test---------------------------
            DatabaseSourceRegion region = new DatabaseSourceRegion(src.Object, ModelItemUtils.CreateModelItem(new DsfSqlServerDatabaseActivity()), enSourceType.SqlDatabase);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, region.Errors.Count);
            Assert.AreEqual(region.LabelWidth, 46);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DatabaseSourceRegion_ConstructorWithSelectedSource")]
        public void DatabaseSourceRegion_ConstructorWithSelectedSource_Scenerio_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfSqlServerDatabaseActivity() { SourceId = id };
            var src = new Mock<IDbServiceModel>();
            var dbsrc = new DbSourceDefinition() { Id = id };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IDbSource>() { dbsrc });

            //------------Execute Test---------------------------
            DatabaseSourceRegion region = new DatabaseSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act), enSourceType.SqlDatabase);

            //------------Assert Results-------------------------
            Assert.AreEqual(dbsrc, region.SelectedSource);
            Assert.IsTrue(region.CanEditSource());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DatabaseSourceRegion_ChangeSourceSomethingChanged")]
        public void DatabaseSourceRegion_ChangeSourceSomethingChanged_ExpectedChange_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfSqlServerDatabaseActivity() { SourceId = id };
            var src = new Mock<IDbServiceModel>();
            var dbsrc = new DbSourceDefinition() { Id = id };
            var evt = false;
            var s2 = new DbSourceDefinition() { Id = Guid.NewGuid() };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IDbSource>() { dbsrc, s2 });
            
            //------------Execute Test---------------------------
            DatabaseSourceRegion region = new DatabaseSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act), enSourceType.SqlDatabase);
            region.SomethingChanged += (a, b) => { evt = true; };
            region.SelectedSource = s2;

            //------------Assert Results-------------------------
            Assert.IsTrue(evt);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DatabaseSourceRegion_ChangeSourceSomethingChanged")]
        public void DatabaseSourceRegion_ChangeSourceSomethingChanged_RestoreRegion_Result()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfSqlServerDatabaseActivity() { SourceId = id };
            var src = new Mock<IDbServiceModel>();
            var dbsrc = new DbSourceDefinition() { Id = id, Name = "bob" };

            var s2 = new DbSourceDefinition() { Id = Guid.NewGuid(), Name = "bob" };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IDbSource>() { dbsrc, s2 });

            //------------Execute Test---------------------------
            DatabaseSourceRegion region = new DatabaseSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act), enSourceType.SqlDatabase);

            var clone1 = new Mock<IToolRegion>();
            var clone2 = new Mock<IToolRegion>();
            var dep1 = new Mock<IToolRegion>();
            dep1.Setup(a => a.CloneRegion()).Returns(clone1.Object);

            var dep2 = new Mock<IToolRegion>();
            dep2.Setup(a => a.CloneRegion()).Returns(clone2.Object);
            region.Dependants = new List<IToolRegion> { dep1.Object, dep2.Object };
            region.SelectedSource = s2;
            region.SelectedSource = dbsrc;

            //------------Assert Results-------------------------
            dep1.Verify(a => a.RestoreRegion(clone1.Object),Times.Never);
            dep2.Verify(a => a.RestoreRegion(clone2.Object), Times.Never);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DatabaseSourceRegion_ChangeSourceSomethingChanged")]
        public void DatabaseSourceRegion_ChangeSourceSomethingChanged_RegionsNotRestored_Invalid()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfSqlServerDatabaseActivity() { SourceId = id };
            var src = new Mock<IDbServiceModel>();
            var dbsrc = new DbSourceDefinition() { Id = id };

            var s2 = new DbSourceDefinition() { Id = Guid.NewGuid() };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IDbSource>() { dbsrc, s2 });
            
            //------------Execute Test---------------------------
            DatabaseSourceRegion region = new DatabaseSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act), enSourceType.SqlDatabase);

            var clone1 = new Mock<IToolRegion>();
            var clone2 = new Mock<IToolRegion>();
            var dep1 = new Mock<IToolRegion>();
            dep1.Setup(a => a.CloneRegion()).Returns(clone1.Object);

            var dep2 = new Mock<IToolRegion>();
            dep2.Setup(a => a.CloneRegion()).Returns(clone2.Object);
            region.Dependants = new List<IToolRegion> { dep1.Object, dep2.Object };
            region.SelectedSource = s2;
            region.SelectedSource = dbsrc;

            //------------Assert Results-------------------------
            dep1.Verify(a => a.RestoreRegion(clone1.Object), Times.Never);
            dep2.Verify(a => a.RestoreRegion(clone2.Object), Times.Never);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DatabaseSourceRegion_ChangeSourceSomethingChanged")]
        public void DatabaseSourceRegion_ChangeSourceSomethingChanged_CloneRegion_ExpectedClone()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfSqlServerDatabaseActivity() { SourceId = id };
            var src = new Mock<IDbServiceModel>();
            var dbsrc = new DbSourceDefinition() { Id = id };
            var s2 = new DbSourceDefinition() { Id = Guid.NewGuid() };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IDbSource>() { dbsrc, s2 });
            
            //------------Execute Test---------------------------
            DatabaseSourceRegion region = new DatabaseSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act), enSourceType.SqlDatabase);
            var cloned = region.CloneRegion();

            //------------Assert Results-------------------------
            Assert.AreEqual(((DatabaseSourceRegion)cloned).SelectedSource, region.SelectedSource);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DatabaseSourceRegion_ChangeSourceSomethingChanged")]
        public void DatabaseSourceRegion_ChangeSourceSomethingChanged_RestoreRegion_ExpectedRestore()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var act = new DsfSqlServerDatabaseActivity() { SourceId = id };
            var src = new Mock<IDbServiceModel>();
            var dbsrc = new DbSourceDefinition() { Id = id };
            var s2 = new DbSourceDefinition() { Id = Guid.NewGuid() };
            src.Setup(a => a.RetrieveSources()).Returns(new ObservableCollection<IDbSource>() { dbsrc, s2 });
            
            //------------Execute Test---------------------------
            DatabaseSourceRegion region = new DatabaseSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act), enSourceType.SqlDatabase);
            
            DatabaseSourceRegion regionToRestore = new DatabaseSourceRegion(src.Object, ModelItemUtils.CreateModelItem(act), enSourceType.SqlDatabase);
            regionToRestore.SelectedSource = s2;

            region.RestoreRegion(regionToRestore);

            //------------Assert Results-------------------------
            Assert.AreEqual(region.SelectedSource, s2);
        }
    }
}
