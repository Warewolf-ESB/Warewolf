using System;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Warewolf.Core;

namespace Dev2.Tests.Activities.ActivityComparerTests.Database
{
    [TestClass]
    public class DsfODBCDatabaseActivityEqualityTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void UniqueIDEquals_EmptyOdbcDatabase_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var odbcDatabase = new DsfODBCDatabaseActivity() { UniqueID = uniqueId };
            var activity = new DsfODBCDatabaseActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(odbcDatabase);
            //---------------Execute Test ----------------------
            var equals = odbcDatabase.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void UniqueIDDifferent_EmptyOdbcDatabase_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfODBCDatabaseActivity();
            var odbcDatabase = new DsfODBCDatabaseActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(odbcDatabase);
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
            var activity1 = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, SourceId = sourceId };
            var activity = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, SourceId = sourceId };
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
            var activity1 = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, SourceId = sourceId };
            var activity = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, SourceId = sourceId2 };
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
            var activity1 = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var odbcDatabase = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(odbcDatabase);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Different_DisplayName_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var odbcDatabase = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(odbcDatabase);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_CommandText_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, CommandText = "a" };
            var odbcDatabase = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, CommandText = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(odbcDatabase);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_CommandText_Different_Casing_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, CommandText = "A" };
            var odbcDatabase = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, CommandText = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(odbcDatabase);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Different_CommandText_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, CommandText = "A" };
            var odbcDatabase = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, CommandText = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(odbcDatabase);
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
            var activity = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, Inputs =  inputs};
            var activity1 = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, Inputs = inputs2 };
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
            var activity = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, Inputs =  inputs};
            var activity1 = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, Inputs = inputs2 };
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
            var activity = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, Inputs =  inputs};
            var activity1 = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, Inputs = inputs };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentOutputs_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var outputs = new List<Common.Interfaces.DB.IServiceOutputMapping>();
            var outputs2 = new List<Common.Interfaces.DB.IServiceOutputMapping>();
            outputs2.Add(new ServiceOutputMapping("a", "b", "c"));
            var activity = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, Outputs = outputs };
            var activity1 = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, Outputs = outputs2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameOutputs_DifferentIndexes_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var outputs = new List<Common.Interfaces.DB.IServiceOutputMapping>
            {
                new ServiceOutputMapping("d", "e", "f"),
                new ServiceOutputMapping("a", "b", "c")
            };
            var outputs2 = new List<Common.Interfaces.DB.IServiceOutputMapping>
            {
                new ServiceOutputMapping("a", "b", "c"),
                new ServiceOutputMapping("d", "e", "f")
            };
            var activity = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, Outputs = outputs };
            var activity1 = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, Outputs = outputs2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameOutputs_ActivityTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var outputs = new List<Common.Interfaces.DB.IServiceOutputMapping>();
            var activity = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, Outputs = outputs };
            var activity1 = new DsfODBCDatabaseActivity() { UniqueID = uniqueId, Outputs = outputs };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
    }
}
