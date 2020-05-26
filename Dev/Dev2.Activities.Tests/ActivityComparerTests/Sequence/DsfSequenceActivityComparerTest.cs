using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.Sequence
{
    [TestClass]
    public class DsfSequenceActivityComparerTest
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_DifferentUniqueIds_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfSequenceActivity() { UniqueID = uniqueId };
            var activity1 = new DsfSequenceActivity() { UniqueID = uniqueId };
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
            var activity = new DsfSequenceActivity() { UniqueID = uniqueId };
            var activity1 = new DsfSequenceActivity() { UniqueID = uniqueId };
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
        public void Equals_Given_Same_DisplayName_ActivityTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfSequenceActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var activity1 = new DsfSequenceActivity() { UniqueID = uniqueId, DisplayName = "a" };
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
        public void Equals_Given_Different_DisplayName_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new DsfSequenceActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var activity1 = new DsfSequenceActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sanele Mthembu")]
        public void Equals_Given_Same_Activities_ActivityTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activities = new System.Collections.ObjectModel.Collection<System.Activities.Activity>();
            var activity = new DsfSequenceActivity() { UniqueID = uniqueId, Activities = activities };
            var activity1 = new DsfSequenceActivity() { UniqueID = uniqueId, Activities = activities };
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
        public void Equals_Given_Different_Activities_ActivityTools_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activities = new System.Collections.ObjectModel.Collection<System.Activities.Activity>
            {
                new DsfDataMergeActivity
                {
                }
            };
            var activities2 = new System.Collections.ObjectModel.Collection<System.Activities.Activity>
            {
                new DsfDataSplitActivity
                {
                }
            };

            var activity = new DsfSequenceActivity() { UniqueID = uniqueId, Activities = activities };
            var activity1 = new DsfSequenceActivity() { UniqueID = uniqueId, Activities = activities2 };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void GetChildrenNodes_Given_Activities_ActivityTools()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activities = new System.Collections.ObjectModel.Collection<System.Activities.Activity>
            {
                new DsfDataMergeActivity
                {
                }
            };
            var activities2 = new System.Collections.ObjectModel.Collection<System.Activities.Activity>
            {
                new DsfDataSplitActivity
                {
                }
            };

            var activity = new DsfSequenceActivity() { UniqueID = uniqueId, Activities = activities };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity.GetChildrenNodes());
            //---------------Execute Test ----------------------
            var nodes = activity.GetChildrenNodes();
            //---------------Test Result -----------------------
            Assert.AreEqual(1, nodes.Count());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("DsfSequenceActivity_GetState")]
        public void DsfSequenceActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            //------------Setup for test--------------------------
            var act = new DsfSequenceActivity();
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, stateItems.Count());

        }
    }
}