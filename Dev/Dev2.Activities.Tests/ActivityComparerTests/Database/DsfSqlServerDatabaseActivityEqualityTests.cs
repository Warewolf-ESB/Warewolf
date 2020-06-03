using System;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Services.Execution;
using System.Runtime.InteropServices.ComTypes;
using Dev2.Runtime.ServiceModel.Data;
using Moq;
using Dev2.Runtime.ESB.Execution;

namespace Dev2.Tests.Activities.ActivityComparerTests.Database
{
    [TestClass]
    public class DsfSqlServerDatabaseActivityEqualityTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void UniqueIDEquals_EmptySqlDatabase_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sqlDatabase = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId };
            var activity = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sqlDatabase);
            //---------------Execute Test ----------------------
            var equals = sqlDatabase.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void UniqueIDDifferent_EmptySqlDatabase_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfSqlServerDatabaseActivity();
            var sqlDatabase = new DsfSqlServerDatabaseActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(sqlDatabase);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_DisplayName_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var sqlDatabase = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(sqlDatabase);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_DisplayName_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var sqlDatabase = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(sqlDatabase);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_ProcedureName_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, ProcedureName = "a" };
            var sqlDatabase = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, ProcedureName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(sqlDatabase);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_ProcedureName_Different_Casing_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, ProcedureName = "A" };
            var sqlDatabase = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, ProcedureName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(sqlDatabase);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_ProcedureName_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, ProcedureName = "A" };
            var sqlDatabase = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, ProcedureName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(sqlDatabase);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
    }
}
