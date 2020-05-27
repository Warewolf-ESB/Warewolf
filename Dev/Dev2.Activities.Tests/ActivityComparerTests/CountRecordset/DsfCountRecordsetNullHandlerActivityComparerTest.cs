using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.CountRecords
{
    [TestClass]
    public class DsfCountRecordsetNullHandlerActivityComparerTest
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentUniqueIds_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId };
            var activity1 = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_EmptyActivityTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId };
            var activity1 = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DisplayName_Same_DisplayName_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var activity = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DisplayName_Different_DisplayName_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var activity = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_RecordsetName_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId, RecordsetName = "a" };
            var activity = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId, RecordsetName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_RecordsetName_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId, RecordsetName = "A" };
            var activity = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId, RecordsetName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_CountNumber_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId, CountNumber = "a" };
            var activity = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId, CountNumber = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_CountNumber_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId, CountNumber = "A" };
            var activity = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId, CountNumber = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_TreatNullAsZero_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId, TreatNullAsZero = true };
            var activity = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId, TreatNullAsZero = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Different_TreatNullAsZero_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId, TreatNullAsZero = true };
            var activity = new DsfCountRecordsetNullHandlerActivity() { UniqueID = uniqueId, TreatNullAsZero = false};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
    }
}