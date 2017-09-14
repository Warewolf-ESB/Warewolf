using System;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Warewolf.Core;

namespace Dev2.Tests.Activities.ActivityComparerTests.Database
{
    [TestClass]
    public class DsfSqlServerDatabaseActivityEqualityTests
    {
        [TestMethod]
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
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_SourceId_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sourceId = Guid.NewGuid(); ;
            var activity1 = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, SourceId = sourceId };
            var activity = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, SourceId = sourceId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_SourceId_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sourceId = Guid.NewGuid(); ;
            var sourceId2 = Guid.NewGuid(); ;
            var activity1 = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, SourceId = sourceId };
            var activity = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, SourceId = sourceId2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_DisplayName_IsEqual()
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
        [Owner("Sanele Mthembu")]
        public void DisplayName_Different_DisplayName_Is_Not_Equal()
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
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_ProcedureName_IsEqual()
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
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_ProcedureName_Different_Casing_IsNotEqual()
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
        [Owner("Sanele Mthembu")]
        public void DisplayName_Different_ProcedureName_Is_Not_Equal()
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

        
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentInputs_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var inputs = new List<Common.Interfaces.DB.IServiceInput>
            {
                new ServiceInput("Input1", "[[InputValue1]]")
            };
            var inputs2 = new List<Common.Interfaces.DB.IServiceInput>();
            var activity = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, Inputs =  inputs};
            var activity1 = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, Inputs = inputs2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameInputsDifferentIndexes_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var inputs = new List<Common.Interfaces.DB.IServiceInput>
            {
                new ServiceInput("Input2", "[[InputValue2]]"),
                new ServiceInput("Input1", "[[InputValue1]]")                
            };
            var inputs2 = new List<Common.Interfaces.DB.IServiceInput>
            {
                new ServiceInput("Input1", "[[InputValue1]]"),
                new ServiceInput("Input2", "[[InputValue2]]")
            };
            var activity = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, Inputs =  inputs};
            var activity1 = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, Inputs = inputs2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameInputs_ActivityTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var inputs = new List<Common.Interfaces.DB.IServiceInput>();
            var activity = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, Inputs =  inputs};
            var activity1 = new DsfSqlServerDatabaseActivity() { UniqueID = uniqueId, Inputs = inputs };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
    }
}
