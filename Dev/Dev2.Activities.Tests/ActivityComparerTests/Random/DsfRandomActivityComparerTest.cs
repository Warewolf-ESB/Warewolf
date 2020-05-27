using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Dev2.Common.Interfaces.Enums;

namespace Dev2.Tests.Activities.ActivityComparerTests.Random
{
    [TestClass]
    public class DsfRandomActivityComparerTest
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentUniqueIds_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfRandomActivity() { UniqueID = uniqueId };
            var activity1 = new DsfRandomActivity() { UniqueID = uniqueId };
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
            var activity = new DsfRandomActivity() { UniqueID = uniqueId };
            var activity1 = new DsfRandomActivity() { UniqueID = uniqueId };
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
        public void Equals_Given_Same_DisplayName_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfRandomActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var activity = new DsfRandomActivity() { UniqueID = uniqueId, DisplayName = "a" };
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
        public void Equals_Given_Different_DisplayName_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfRandomActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var activity = new DsfRandomActivity() { UniqueID = uniqueId, DisplayName = "ass" };
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
        public void Equals_Given_Same_Length_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfRandomActivity() { UniqueID = uniqueId, Length = "a" };
            var activity = new DsfRandomActivity() { UniqueID = uniqueId, Length = "a" };
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
        public void Equals_Given_Different_Length_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfRandomActivity() { UniqueID = uniqueId, Length = "A" };
            var activity = new DsfRandomActivity() { UniqueID = uniqueId, Length = "ass" };
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
        public void Equals_Given_Same_From_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfRandomActivity() { UniqueID = uniqueId, From = "a" };
            var activity = new DsfRandomActivity() { UniqueID = uniqueId, From = "a" };
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
        public void Equals_Given_Different_From_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfRandomActivity() { UniqueID = uniqueId, From = "A" };
            var activity = new DsfRandomActivity() { UniqueID = uniqueId, From = "ass" };
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
        public void Equals_Given_Same_To_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfRandomActivity() { UniqueID = uniqueId, To = "a" };
            var activity = new DsfRandomActivity() { UniqueID = uniqueId, To = "a" };
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
        public void Equals_Given_Different_To_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfRandomActivity() { UniqueID = uniqueId, To = "A" };
            var activity = new DsfRandomActivity() { UniqueID = uniqueId, To = "ass" };
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
        public void Equals_Given_Same_Result_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfRandomActivity() { UniqueID = uniqueId, Result = "a" };
            var activity = new DsfRandomActivity() { UniqueID = uniqueId, Result = "a" };
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
        public void Equals_Given_Different_Result_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfRandomActivity() { UniqueID = uniqueId, Result = "A" };
            var activity = new DsfRandomActivity() { UniqueID = uniqueId, Result = "ass" };
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
        public void Equals_Given_Same_RandomType_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            
            var activity1 = new DsfRandomActivity() { UniqueID = uniqueId, RandomType = enRandomType.Numbers };
            var activity = new DsfRandomActivity() { UniqueID = uniqueId, RandomType = enRandomType.Numbers };
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
        public void Equals_Given_Different_RandomType_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new DsfRandomActivity() { UniqueID = uniqueId, RandomType = enRandomType.Numbers };
            var activity = new DsfRandomActivity() { UniqueID = uniqueId, RandomType = enRandomType.Letters };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
    }
}