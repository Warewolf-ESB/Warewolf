using System;
using System.Collections.Generic;
using Dev2.Activities.Sharepoint;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.Sharepoint
{
    [TestClass]
    public class SharepointCreateListItemActivityEqualityTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptySharepoint_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointActivity = new SharepointCreateListItemActivity() { UniqueID = uniqueId };
            var activity = new SharepointCreateListItemActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointActivity);
            //---------------Execute Test ----------------------
            var equals = sharepointActivity.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_EmptySharepoint_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCreateListItemActivity();
            var selectAndApplyActivity = new SharepointCreateListItemActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCreateListItemActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var selectAndApplyActivity = new SharepointCreateListItemActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCreateListItemActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var selectAndApplyActivity = new SharepointCreateListItemActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DisplayName_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCreateListItemActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var selectAndApplyActivity = new SharepointCreateListItemActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var itemActivity = new SharepointCreateListItemActivity() { UniqueID = uniqueId, Result = "a" };
            var activity = new SharepointCreateListItemActivity() { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(itemActivity);
            //---------------Execute Test ----------------------
            var @equals = itemActivity.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new SharepointCreateListItemActivity() { UniqueID = uniqueId, Result = "A" };
            var activity1 = new SharepointCreateListItemActivity() { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new SharepointCreateListItemActivity() { UniqueID = uniqueId, Result = "AAA" };
            var activity1 = new SharepointCreateListItemActivity() { UniqueID = uniqueId, Result = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SharepointList_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var itemActivity = new SharepointCreateListItemActivity() { UniqueID = uniqueId, SharepointList = "a" };
            var activity = new SharepointCreateListItemActivity() { UniqueID = uniqueId, SharepointList = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(itemActivity);
            //---------------Execute Test ----------------------
            var @equals = itemActivity.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SharepointList_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new SharepointCreateListItemActivity() { UniqueID = uniqueId, SharepointList = "A" };
            var activity1 = new SharepointCreateListItemActivity() { UniqueID = uniqueId, SharepointList = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SharepointList_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new SharepointCreateListItemActivity() { UniqueID = uniqueId, SharepointList = "AAA" };
            var activity1 = new SharepointCreateListItemActivity() { UniqueID = uniqueId, SharepointList = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SharepointServerResourceId_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid();
            var sharepointActivity = new SharepointCreateListItemActivity() { UniqueID = uniqueId.ToString(), SharepointServerResourceId = uniqueId };
            var sharepoint = new SharepointCreateListItemActivity() { UniqueID = uniqueId.ToString(), SharepointServerResourceId = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SharepointServerResourceId_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointActivity = new SharepointCreateListItemActivity() { UniqueID = uniqueId, SharepointServerResourceId = Guid.NewGuid() };
            var sharepoint = new SharepointCreateListItemActivity() { UniqueID = uniqueId, SharepointServerResourceId = Guid.NewGuid() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ReadListItems_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointActivity = new SharepointCreateListItemActivity() { UniqueID = uniqueId, ReadListItems = new List<SharepointReadListTo>() };
            var sharepoint = new SharepointCreateListItemActivity() { UniqueID = uniqueId, ReadListItems = new List<SharepointReadListTo>() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ReadListItems_Same_Object_IsEqual_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointCreateListItemActivity = new SharepointCreateListItemActivity()
            {
                UniqueID = uniqueId,
                ReadListItems = new List<SharepointReadListTo>()
                {
                    new SharepointReadListTo("a","a","a","a")
                }
            };
            var listItemActivity = new SharepointCreateListItemActivity()
            {
                UniqueID = uniqueId,
                ReadListItems = new List<SharepointReadListTo>()
                {
                    new SharepointReadListTo("a","a","a","a")
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointCreateListItemActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointCreateListItemActivity.Equals(listItemActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ReadListItems_Same_DifferentIndexNumbers_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new SharepointCreateListItemActivity()
            {
                UniqueID = uniqueId,
                ReadListItems = new List<SharepointReadListTo>()
                {
                    new SharepointReadListTo("a","a","a","a"){IndexNumber = 1},
                    new SharepointReadListTo("B","B","",""){IndexNumber = 2},
                }
            };
            var listItemActivity = new SharepointCreateListItemActivity()
            {
                UniqueID = uniqueId,
                ReadListItems = new List<SharepointReadListTo>()
                {
                    new SharepointReadListTo("B","B","",""){IndexNumber = 1},
                    new SharepointReadListTo("a","a","a","a"){IndexNumber = 2}
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(listItemActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ReadListItems_Same_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var itemActivity = new SharepointCreateListItemActivity()
            {
                UniqueID = uniqueId,
                ReadListItems = new List<SharepointReadListTo>()
                {
                    new SharepointReadListTo("A","a","","")
                }
            };
            var createListItemActivity = new SharepointCreateListItemActivity()
            {
                UniqueID = uniqueId,
                ReadListItems = new List<SharepointReadListTo>()
                {
                    new SharepointReadListTo("a","a","","")
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(itemActivity);
            //---------------Execute Test ----------------------
            var equals = itemActivity.Equals(createListItemActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Result_Different_Object_Is_Not_Equal_ContainsList()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var createListItemActivity = new SharepointCreateListItemActivity()
            {
                UniqueID = uniqueId,
                ReadListItems = new List<SharepointReadListTo>()
                {
                    new SharepointReadListTo("a","a","","")
                }

            };
            var listItemActivity = new SharepointCreateListItemActivity()
            {
                UniqueID = uniqueId,
                ReadListItems = new List<SharepointReadListTo>()
                {
                    new SharepointReadListTo("a","a","","")
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(createListItemActivity.Equals(listItemActivity));
            //---------------Execute Test ----------------------
            createListItemActivity.Result = "[[result]]";
            var equals = createListItemActivity.Equals(listItemActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


    }
}