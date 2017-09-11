using System;
using Dev2.Activities.Sharepoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.Sharepoint
{
    [TestClass]
    public class SharepointCopyFileActivityEqualityTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptySharepointCopyFile_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointCopyFileActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId };
            var copyFileActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointCopyFileActivity);
            //---------------Execute Test ----------------------
            var equals = sharepointCopyFileActivity.Equals(copyFileActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_EmptySharepointCopyFile_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCopyFileActivity();
            var selectAndApplyActivity = new SharepointCopyFileActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var selectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var selectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var selectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServerInputPathFrom_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathFrom = "a" };
            var selectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathFrom = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServerInputPathFrom_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathFrom = "A" };
            var selectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathFrom = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServerInputPathFrom_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathFrom = "AAA" };
            var selectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathFrom = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServerInputPathTo_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathTo = "a" };
            var selectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathTo = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServerInputPathTo_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathTo = "A" };
            var selectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathTo = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SharepointServerResourceId_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid();
            var sharepointCopyFileActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId.ToString(), SharepointServerResourceId = uniqueId };
            var sharepoint = new SharepointCopyFileActivity() { UniqueID = uniqueId.ToString(), SharepointServerResourceId = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointCopyFileActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointCopyFileActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SharepointServerResourceId_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointCopyFileActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, SharepointServerResourceId = Guid.NewGuid() };
            var sharepoint = new SharepointCopyFileActivity() { UniqueID = uniqueId, SharepointServerResourceId = Guid.NewGuid() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointCopyFileActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointCopyFileActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServerInputPathTo_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointCopyFileActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathTo = "AAA" };
            var sharepoint = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathTo = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointCopyFileActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointCopyFileActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Overwrite_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new SharepointCopyFileActivity { UniqueID = uniqueId, Result = "A", Overwrite = true };
            var activity1 = new SharepointCopyFileActivity { UniqueID = uniqueId, Result = "A", Overwrite = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(activity.Equals(activity1));
            //---------------Execute Test ----------------------
            activity.Overwrite = true;
            activity1.Overwrite = false;
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Overwrite_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointCopyFileActivity = new SharepointCopyFileActivity { UniqueID = uniqueId, Result = "A",  Overwrite = true };
            var sharepoint = new SharepointCopyFileActivity { UniqueID = uniqueId, Result = "A", Overwrite = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(sharepointCopyFileActivity.Equals(sharepoint));
            //---------------Execute Test ----------------------
            sharepointCopyFileActivity.Overwrite = true;
            sharepoint.Overwrite = true;
            var @equals = sharepointCopyFileActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }
    }
}