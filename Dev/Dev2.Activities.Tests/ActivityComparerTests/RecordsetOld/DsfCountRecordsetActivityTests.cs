using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.RecordsetOld
{
    [TestClass]
    public class DsfCountRecordsetActivityTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptyCountRecordset_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfCountRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId };
            var countRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfCountRecordsetActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfCountRecordsetActivity.Equals(countRecordsetActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_EmptyCountRecordset_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var dsfCountRecordsetActivity = new DsfCountRecordsetActivity();
            var countRecordsetActivity = new DsfCountRecordsetActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfCountRecordsetActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfCountRecordsetActivity.Equals(countRecordsetActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var countRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var dsfCountRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(countRecordsetActivity);
            //---------------Execute Test ----------------------
            var @equals = countRecordsetActivity.Equals(dsfCountRecordsetActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var countRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var dsfCountRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(countRecordsetActivity);
            //---------------Execute Test ----------------------
            var @equals = countRecordsetActivity.Equals(dsfCountRecordsetActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var countRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var dsfCountRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(countRecordsetActivity);
            //---------------Execute Test ----------------------
            var @equals = countRecordsetActivity.Equals(dsfCountRecordsetActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void RecordsetName_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var countRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId, RecordsetName = "a" };
            var dsfCountRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId, RecordsetName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(countRecordsetActivity);
            //---------------Execute Test ----------------------
            var @equals = countRecordsetActivity.Equals(dsfCountRecordsetActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void RecordsetName_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var countRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId, RecordsetName = "A" };
            var dsfCountRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId, RecordsetName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(countRecordsetActivity);
            //---------------Execute Test ----------------------
            var @equals = countRecordsetActivity.Equals(dsfCountRecordsetActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void RecordsetName_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var countRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId, RecordsetName = "AAA" };
            var dsfCountRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId, RecordsetName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(countRecordsetActivity);
            //---------------Execute Test ----------------------
            var @equals = countRecordsetActivity.Equals(dsfCountRecordsetActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void CountNumber_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var countRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId, CountNumber = "a" };
            var dsfCountRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId, CountNumber = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(countRecordsetActivity);
            //---------------Execute Test ----------------------
            var @equals = countRecordsetActivity.Equals(dsfCountRecordsetActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void CountNumber_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var countRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId, CountNumber = "A" };
            var dsfCountRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId, CountNumber = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(countRecordsetActivity);
            //---------------Execute Test ----------------------
            var @equals = countRecordsetActivity.Equals(dsfCountRecordsetActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void CountNumber_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var countRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId, CountNumber = "AAA" };
            var dsfCountRecordsetActivity = new DsfCountRecordsetActivity() { UniqueID = uniqueId, CountNumber = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(countRecordsetActivity);
            //---------------Execute Test ----------------------
            var @equals = countRecordsetActivity.Equals(dsfCountRecordsetActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }
    }
}
