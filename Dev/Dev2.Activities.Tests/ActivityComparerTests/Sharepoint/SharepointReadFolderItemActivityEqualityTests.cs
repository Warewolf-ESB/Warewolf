using System;
using Dev2.Activities.Sharepoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.Sharepoint
{
    [TestClass]
    public class SharepointReadFolderItemActivityEqualityTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptySharepointCopyFile_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointReadFolderItemActivity = new SharepointReadFolderItemActivity() { UniqueID = uniqueId };
            var copyFileActivity = new SharepointReadFolderItemActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointReadFolderItemActivity);
            //---------------Execute Test ----------------------
            var equals = sharepointReadFolderItemActivity.Equals(copyFileActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_EmptySharepointCopyFile_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new SharepointReadFolderItemActivity();
            var activity = new SharepointReadFolderItemActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var activity = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var activity = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var activity = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }[TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, Result = "a" };
            var activity = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, Result = "A" };
            var activity = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity1);
            //---------------Execute Test ----------------------
            var @equals = activity1.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity1 = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, Result = "AAA" };
            var activity = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, Result = "aaa" };
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
            var activity1 = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, ServerInputPath = "a" };
            var activity = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, ServerInputPath = "a" };
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
            var activity1 = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, ServerInputPath = "A" };
            var activity = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, ServerInputPath = "ass" };
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
            var activity1 = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, ServerInputPath = "AAA" };
            var activity = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, ServerInputPath = "aaa" };
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
            var sharepointReadFolderItemActivity = new SharepointReadFolderItemActivity() { UniqueID = uniqueId.ToString(), SharepointServerResourceId = uniqueId };
            var sharepoint = new SharepointReadFolderItemActivity() { UniqueID = uniqueId.ToString(), SharepointServerResourceId = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointReadFolderItemActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointReadFolderItemActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SharepointServerResourceId_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointReadFolderItemActivity = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, SharepointServerResourceId = Guid.NewGuid() };
            var sharepoint = new SharepointReadFolderItemActivity() { UniqueID = uniqueId, SharepointServerResourceId = Guid.NewGuid() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointReadFolderItemActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointReadFolderItemActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsFilesAndFoldersSelected_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new SharepointReadFolderItemActivity { UniqueID = uniqueId, Result = "A", IsFilesAndFoldersSelected = true };
            var activity1 = new SharepointReadFolderItemActivity { UniqueID = uniqueId, Result = "A", IsFilesAndFoldersSelected = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(activity.Equals(activity1));
            //---------------Execute Test ----------------------
            activity.IsFilesAndFoldersSelected = true;
            activity1.IsFilesAndFoldersSelected = false;
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsFilesAndFoldersSelected_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointFileDownLoadActivity = new SharepointReadFolderItemActivity { UniqueID = uniqueId, Result = "A", IsFilesAndFoldersSelected = true };
            var sharepoint = new SharepointReadFolderItemActivity { UniqueID = uniqueId, Result = "A", IsFilesAndFoldersSelected = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(sharepointFileDownLoadActivity.Equals(sharepoint));
            //---------------Execute Test ----------------------
            sharepointFileDownLoadActivity.IsFilesAndFoldersSelected = true;
            sharepoint.IsFilesAndFoldersSelected = true;
            var @equals = sharepointFileDownLoadActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsFoldersSelected_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new SharepointReadFolderItemActivity { UniqueID = uniqueId, Result = "A", IsFoldersSelected = true };
            var activity1 = new SharepointReadFolderItemActivity { UniqueID = uniqueId, Result = "A", IsFoldersSelected = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(activity.Equals(activity1));
            //---------------Execute Test ----------------------
            activity.IsFoldersSelected = true;
            activity1.IsFoldersSelected = false;
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsFoldersSelected_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointFileDownLoadActivity = new SharepointReadFolderItemActivity { UniqueID = uniqueId, Result = "A", IsFoldersSelected = true };
            var sharepoint = new SharepointReadFolderItemActivity { UniqueID = uniqueId, Result = "A", IsFoldersSelected = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(sharepointFileDownLoadActivity.Equals(sharepoint));
            //---------------Execute Test ----------------------
            sharepointFileDownLoadActivity.IsFoldersSelected = true;
            sharepoint.IsFoldersSelected = true;
            var @equals = sharepointFileDownLoadActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsFilesSelected_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new SharepointReadFolderItemActivity { UniqueID = uniqueId, Result = "A", IsFilesSelected = true };
            var activity1 = new SharepointReadFolderItemActivity { UniqueID = uniqueId, Result = "A", IsFilesSelected = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(activity.Equals(activity1));
            //---------------Execute Test ----------------------
            activity.IsFoldersSelected = true;
            activity1.IsFoldersSelected = false;
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsFilesSelected_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointFileDownLoadActivity = new SharepointReadFolderItemActivity { UniqueID = uniqueId, Result = "A", IsFilesSelected = true };
            var sharepoint = new SharepointReadFolderItemActivity { UniqueID = uniqueId, Result = "A", IsFilesSelected = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(sharepointFileDownLoadActivity.Equals(sharepoint));
            //---------------Execute Test ----------------------
            sharepointFileDownLoadActivity.IsFilesSelected = true;
            sharepoint.IsFilesSelected = true;
            var @equals = sharepointFileDownLoadActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

    }
}