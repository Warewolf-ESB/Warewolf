using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Tests.Activities.ActivityComparerTests.Unique
{
    [TestClass]
    public class DsfUniqueActivityComparerTest
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Empty_UniqueActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var activity = new DsfUniqueActivity();
            var activity1 = new DsfUniqueActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentResult_UniqueActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var activity = new DsfUniqueActivity(){ UniqueID = uniqId, Result = "A" };
            var activity1 = new DsfUniqueActivity() { UniqueID = uniqId, Result = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameResult_Different_Casing_UniqueActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var activity = new DsfUniqueActivity(){ UniqueID = uniqId, Result = "A" };
            var activity1 = new DsfUniqueActivity() { UniqueID = uniqId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameResult_UniqueActivity_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var activity = new DsfUniqueActivity(){ UniqueID = uniqId, Result = "" };
            var activity1 = new DsfUniqueActivity() { UniqueID = uniqId, Result = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferntResultFields_UniqueActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var activity = new DsfUniqueActivity() { UniqueID = uniqId, ResultFields= "A" };
            var activity1 = new DsfUniqueActivity() { UniqueID = uniqId, ResultFields = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameResultFields_Different_Casing_UniqueActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var activity = new DsfUniqueActivity() { UniqueID = uniqId, ResultFields= "A" };
            var activity1 = new DsfUniqueActivity() { UniqueID = uniqId, ResultFields = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameResultFields_UniqueActivity_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var activity = new DsfUniqueActivity() { UniqueID = uniqId, ResultFields= "" };
            var activity1 = new DsfUniqueActivity() { UniqueID = uniqId, ResultFields = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameInFields_UniqueActivity_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var activity = new DsfUniqueActivity() { UniqueID = uniqId, InFields = "" };
            var activity1 = new DsfUniqueActivity() { UniqueID = uniqId, InFields = "" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_SameInFields_Different_Casing_UniqueActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var activity = new DsfUniqueActivity() { UniqueID = uniqId, InFields = "A" };
            var activity1 = new DsfUniqueActivity() { UniqueID = uniqId, InFields = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentInFields_UniqueActivity_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqId = Guid.NewGuid().ToString();
            var activity = new DsfUniqueActivity() { UniqueID = uniqId, InFields = "A" };
            var activity1 = new DsfUniqueActivity() { UniqueID = uniqId, InFields = "B" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
    }
}