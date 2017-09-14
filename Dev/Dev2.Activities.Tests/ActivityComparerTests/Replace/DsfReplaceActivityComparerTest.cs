using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.Replace
{
    [TestClass]
    public class DsfReplaceActivityComparerTest
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentUniqueIds_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfReplaceActivity() { UniqueID = uniqueId };
            var activity1 = new DsfReplaceActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_EmptyActivityTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfReplaceActivity() { UniqueID = uniqueId };
            var activity1 = new DsfReplaceActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_DisplayName_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfReplaceActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var activity = new DsfReplaceActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Different_DisplayName_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfReplaceActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var activity = new DsfReplaceActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_Result_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfReplaceActivity() { UniqueID = uniqueId, Result = "a" };
            var activity = new DsfReplaceActivity() { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Different_Result_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfReplaceActivity() { UniqueID = uniqueId, Result = "A" };
            var activity = new DsfReplaceActivity() { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_FieldsToSearch_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfReplaceActivity() { UniqueID = uniqueId, FieldsToSearch = "a" };
            var activity = new DsfReplaceActivity() { UniqueID = uniqueId, FieldsToSearch = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Different_FieldsToSearch_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfReplaceActivity() { UniqueID = uniqueId, FieldsToSearch = "A" };
            var activity = new DsfReplaceActivity() { UniqueID = uniqueId, FieldsToSearch = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_Find_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfReplaceActivity() { UniqueID = uniqueId, Find = "a" };
            var activity = new DsfReplaceActivity() { UniqueID = uniqueId, Find = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Different_Find_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfReplaceActivity() { UniqueID = uniqueId, Find = "A" };
            var activity = new DsfReplaceActivity() { UniqueID = uniqueId, Find = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_ReplaceWith_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfReplaceActivity() { UniqueID = uniqueId, ReplaceWith = "a" };
            var activity = new DsfReplaceActivity() { UniqueID = uniqueId, ReplaceWith = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Different_ReplaceWith_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfReplaceActivity() { UniqueID = uniqueId, ReplaceWith = "A" };
            var activity = new DsfReplaceActivity() { UniqueID = uniqueId, ReplaceWith = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Same_CaseMatch_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfReplaceActivity() { UniqueID = uniqueId, CaseMatch = true };
            var activity = new DsfReplaceActivity() { UniqueID = uniqueId, CaseMatch = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DisplayName_Different_CaseMatch_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfReplaceActivity() { UniqueID = uniqueId, CaseMatch = false };
            var activity = new DsfReplaceActivity() { UniqueID = uniqueId, CaseMatch = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
    }
}