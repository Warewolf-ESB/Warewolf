using System;
using System.Collections.Generic;
using Dev2.Activities.Sharepoint;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityComparerTests.Sharepoint
{
    [TestClass]
    public class SharepointDeleteListItemActivityEqualityTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptySharepoint_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointActivity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId };
            var activity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId };
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
            var dsfSelectAndApplyActivity = new SharepointDeleteListItemActivity();
            var selectAndApplyActivity = new SharepointDeleteListItemActivity();
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
            var dsfSelectAndApplyActivity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var selectAndApplyActivity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, DisplayName = "a" };
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
            var dsfSelectAndApplyActivity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var selectAndApplyActivity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, DisplayName = "ass" };
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
            var dsfSelectAndApplyActivity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var selectAndApplyActivity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DeleteCount_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var itemActivity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, DeleteCount = "a" };
            var activity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, DeleteCount = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(itemActivity);
            //---------------Execute Test ----------------------
            var @equals = itemActivity.Equals(activity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DeleteCount_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, DeleteCount = "A" };
            var activity1 = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, DeleteCount = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DeleteCount_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, DeleteCount = "AAA" };
            var activity1 = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, DeleteCount = "aaa" };
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
            var itemActivity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, SharepointList = "a" };
            var activity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, SharepointList = "a" };
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
            var activity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, SharepointList = "A" };
            var activity1 = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, SharepointList = "ass" };
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
            var activity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, SharepointList = "AAA" };
            var activity1 = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, SharepointList = "aaa" };
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
            var sharepointActivity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId.ToString(), SharepointServerResourceId = uniqueId };
            var sharepoint = new SharepointDeleteListItemActivity() { UniqueID = uniqueId.ToString(), SharepointServerResourceId = uniqueId };
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
            var sharepointActivity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, SharepointServerResourceId = Guid.NewGuid() };
            var sharepoint = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, SharepointServerResourceId = Guid.NewGuid() };
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
            var sharepointActivity = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, ReadListItems = new List<SharepointReadListTo>() };
            var sharepoint = new SharepointDeleteListItemActivity() { UniqueID = uniqueId, ReadListItems = new List<SharepointReadListTo>() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ReadListItems_Same_Object_IsEqual_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointDeleteListItemActivity = new SharepointDeleteListItemActivity()
            {
                UniqueID = uniqueId,
                ReadListItems = new List<SharepointReadListTo>()
                {
                    new SharepointReadListTo("a","a","a","a")
                }
            };
            var listItemActivity = new SharepointDeleteListItemActivity()
            {
                UniqueID = uniqueId,
                ReadListItems = new List<SharepointReadListTo>()
                {
                    new SharepointReadListTo("a","a","a","a")
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointDeleteListItemActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointDeleteListItemActivity.Equals(listItemActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ReadListItems_Same_DifferentIndexNumbers_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new SharepointDeleteListItemActivity()
            {
                UniqueID = uniqueId,
                ReadListItems = new List<SharepointReadListTo>()
                {
                    new SharepointReadListTo("a","a","a","a"){IndexNumber = 1},
                    new SharepointReadListTo("B","B","",""){IndexNumber = 2},
                }
            };
            var listItemActivity = new SharepointDeleteListItemActivity()
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
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ReadListItems_Same_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var itemActivity = new SharepointDeleteListItemActivity()
            {
                UniqueID = uniqueId,
                ReadListItems = new List<SharepointReadListTo>()
                {
                    new SharepointReadListTo("A","a","","")
                }
            };
            var createListItemActivity = new SharepointDeleteListItemActivity()
            {
                UniqueID = uniqueId,
                ReadListItems = new List<SharepointReadListTo>()
                {
                    new SharepointReadListTo("a","a","","")
                },
                FilterCriteria = new List<SharepointSearchTo>()
                {

                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(itemActivity);
            //---------------Execute Test ----------------------
            var equals = itemActivity.Equals(createListItemActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FilterCriteria_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointActivity = new SharepointDeleteListItemActivity()
            {
                UniqueID = uniqueId,
                ReadListItems = new List<SharepointReadListTo>()
            };
            var sharepoint = new SharepointDeleteListItemActivity()
            {
                UniqueID = uniqueId
                ,
                ReadListItems = new List<SharepointReadListTo>()
                ,
                FilterCriteria = new List<SharepointSearchTo>()
                {

                }

            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FilterCriteria_Same_Object_IsEqual_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointDeleteListItemActivity = new SharepointDeleteListItemActivity()
            {
                UniqueID = uniqueId,
                ReadListItems = new List<SharepointReadListTo>()
                {
                    new SharepointReadListTo("a","a","a","a")
                },
                FilterCriteria = new List<SharepointSearchTo>()
                {
                    new SharepointSearchTo("a","a","",1)
                }
            };
            var listItemActivity = new SharepointDeleteListItemActivity()
            {
                UniqueID = uniqueId,
                ReadListItems = new List<SharepointReadListTo>()
                {
                    new SharepointReadListTo("a","a","a","a")
                },
                FilterCriteria = new List<SharepointSearchTo>()
                {
                    new SharepointSearchTo("a","a","",1)
                }

            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointDeleteListItemActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointDeleteListItemActivity.Equals(listItemActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FilterCriteria_Same_DifferentIndexNumbers_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new SharepointDeleteListItemActivity()
            {
                UniqueID = uniqueId,
                FilterCriteria = new List<SharepointSearchTo>()
                {
                    new SharepointSearchTo("a","a","a",1),
                    new SharepointSearchTo("B","B","",2)
                }
            };
            var listItemActivity = new SharepointDeleteListItemActivity()
            {
                UniqueID = uniqueId,
                FilterCriteria = new List<SharepointSearchTo>()
                {
                    new SharepointSearchTo("B","B","",1),
                    new SharepointSearchTo("a","a","a",2)
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity);
            //---------------Execute Test ----------------------
            var @equals = activity.Equals(listItemActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FilterCriteria_Same_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var itemActivity = new SharepointDeleteListItemActivity()
            {
                UniqueID = uniqueId,
                
                FilterCriteria = new List<SharepointSearchTo>()
                {
                    new SharepointSearchTo("A","a","",1)
                },
            };
            var createListItemActivity = new SharepointDeleteListItemActivity()
            {
                UniqueID = uniqueId,
                
                FilterCriteria = new List<SharepointSearchTo>()
                {
                    new SharepointSearchTo("A","A","",1)
                },
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(itemActivity);
            //---------------Execute Test ----------------------
            var equals = itemActivity.Equals(createListItemActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RequireAllCriteriaToMatch_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new SharepointDeleteListItemActivity { UniqueID = uniqueId, RequireAllCriteriaToMatch = true };
            var activity1 = new SharepointDeleteListItemActivity { UniqueID = uniqueId, RequireAllCriteriaToMatch = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(activity.Equals(activity1));
            //---------------Execute Test ----------------------
            activity.RequireAllCriteriaToMatch = true;
            activity1.RequireAllCriteriaToMatch = false;
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void RequireAllCriteriaToMatch_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointCopyFileActivity = new SharepointDeleteListItemActivity { UniqueID = uniqueId, RequireAllCriteriaToMatch = true };
            var sharepoint = new SharepointDeleteListItemActivity { UniqueID = uniqueId, RequireAllCriteriaToMatch = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(sharepointCopyFileActivity.Equals(sharepoint));
            //---------------Execute Test ----------------------
            sharepointCopyFileActivity.RequireAllCriteriaToMatch = true;
            sharepoint.RequireAllCriteriaToMatch = true;
            var @equals = sharepointCopyFileActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }


    }
}