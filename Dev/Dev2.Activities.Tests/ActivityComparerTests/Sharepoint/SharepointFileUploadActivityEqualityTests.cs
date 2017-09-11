using System;
using Dev2.Activities.Sharepoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.Sharepoint
{
    [TestClass]
    public class SharepointFileUploadActivityEqualityTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptySharepointCopyFile_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointFileUploadActivity = new SharepointFileUploadActivity() { UniqueID = uniqueId };
            var copyFileActivity = new SharepointFileUploadActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointFileUploadActivity);
            //---------------Execute Test ----------------------
            var equals = sharepointFileUploadActivity.Equals(copyFileActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_EmptySharepointCopyFile_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new SharepointFileUploadActivity();
            var activity = new SharepointFileUploadActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new SharepointFileUploadActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var activity = new SharepointFileUploadActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new SharepointFileUploadActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var activity = new SharepointFileUploadActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new SharepointFileUploadActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var activity = new SharepointFileUploadActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServerInputPath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new SharepointFileUploadActivity() { UniqueID = uniqueId, ServerInputPath = "a" };
            var activity = new SharepointFileUploadActivity() { UniqueID = uniqueId, ServerInputPath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServerInputPath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new SharepointFileUploadActivity() { UniqueID = uniqueId, ServerInputPath = "A" };
            var activity = new SharepointFileUploadActivity() { UniqueID = uniqueId, ServerInputPath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServerInputPath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new SharepointFileUploadActivity() { UniqueID = uniqueId, ServerInputPath = "AAA" };
            var activity = new SharepointFileUploadActivity() { UniqueID = uniqueId, ServerInputPath = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void LocalInputPath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new SharepointFileUploadActivity() { UniqueID = uniqueId, LocalInputPath = "a" };
            var activity = new SharepointFileUploadActivity() { UniqueID = uniqueId, LocalInputPath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void LocalInputPath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new SharepointFileUploadActivity() { UniqueID = uniqueId, LocalInputPath = "A" };
            var activity = new SharepointFileUploadActivity() { UniqueID = uniqueId, LocalInputPath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SharepointServerResourceId_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid();
            var sharepointFileUploadActivity = new SharepointFileUploadActivity() { UniqueID = uniqueId.ToString(), SharepointServerResourceId = uniqueId };
            var sharepoint = new SharepointFileUploadActivity() { UniqueID = uniqueId.ToString(), SharepointServerResourceId = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointFileUploadActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointFileUploadActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SharepointServerResourceId_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointFileUploadActivity = new SharepointFileUploadActivity() { UniqueID = uniqueId, SharepointServerResourceId = Guid.NewGuid() };
            var sharepoint = new SharepointFileUploadActivity() { UniqueID = uniqueId, SharepointServerResourceId = Guid.NewGuid() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointFileUploadActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointFileUploadActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void LocalInputPath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointFileUploadActivity = new SharepointFileUploadActivity() { UniqueID = uniqueId, LocalInputPath = "AAA" };
            var sharepoint = new SharepointFileUploadActivity() { UniqueID = uniqueId, LocalInputPath = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointFileUploadActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointFileUploadActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
       
    }
}