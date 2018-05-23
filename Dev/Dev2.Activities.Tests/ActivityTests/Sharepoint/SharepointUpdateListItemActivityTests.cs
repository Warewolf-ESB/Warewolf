using System;
using System.Collections.Generic;
using ActivityUnitTests;
using Dev2.Activities.Sharepoint;
using Dev2.Common.Interfaces;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Interfaces;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using static Dev2.Tests.Activities.ActivityTests.Sharepoint.SharepointCopyFileActivityTests;

namespace Dev2.Tests.Activities.ActivityTests.Sharepoint
{
    [TestClass]
    public class SharepointUpdateListItemActivityTests : BaseActivityUnitTest
    {
        SharepointUpdateListItemActivity CreateActivity()
        {
            return new SharepointUpdateListItemActivity();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointUpdateListItemActivity_Construct")]
        public void SharepointUpdateListItemActivity_Construct_GivenInstance_ShouldNotBeNull()
        {
            //------------Setup for test--------------------------
            var sharepointUpdateListItemActivity = CreateActivity();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointUpdateListItemActivity);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointUpdateListItem_Execute")]
        public void SharepointSource_DoesNotExist_OnResourceCatalog_ShouldSetSharepointSource_ToGuidEmpty()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointUpdateListItem";
            var resourceId = Guid.NewGuid();
            var sharepointUpdateListItemActivity = new SharepointUpdateListItemActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                ReadListItems = new List<SharepointReadListTo>(),
                FilterCriteria = new List<SharepointSearchTo>(),
                RequireAllCriteriaToMatch = true
            };

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            var privateObject = new PrivateObject(sharepointUpdateListItemActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);

            Assert.AreEqual(resourceId, sharepointUpdateListItemActivity.SharepointServerResourceId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointUpdateListItem_Execute")]
        public void SharepointSource_Exists_OnResourceCatalog_BlankRecordSet()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointUpdateListItem";
            var resourceId = Guid.NewGuid();
            var sharepointUpdateListItemActivity = new SharepointUpdateListItemActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                ReadListItems = new List<SharepointReadListTo>(),
                FilterCriteria = new List<SharepointSearchTo>(),
                RequireAllCriteriaToMatch = true
            };

            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");

            var resourceCatalog = new Mock<IResourceCatalog>();

            var mockSharepointHelper = new Mock<ISharepointHelper>();
            mockSharepointHelper.Setup(helper => helper.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns("Success");

            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);

            var privateObject = new PrivateObject(sharepointUpdateListItemActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);
            Assert.IsNotNull(result);
        }
    }
}
