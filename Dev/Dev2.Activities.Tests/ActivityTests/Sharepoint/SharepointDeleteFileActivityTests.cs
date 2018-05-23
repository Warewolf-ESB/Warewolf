using System;
using System.Collections.Generic;
using System.Reflection;
using ActivityUnitTests;
using Dev2.Activities.Sharepoint;
using Dev2.Common.Interfaces;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using static Dev2.Tests.Activities.ActivityTests.Sharepoint.SharepointCopyFileActivityTests;

namespace Dev2.Tests.Activities.ActivityTests.Sharepoint
{
    [TestClass]
    public class SharepointDeleteFileActivityTests : BaseActivityUnitTest
    {
        SharepointDeleteFileActivity CreateActivity()
        {
            return new SharepointDeleteFileActivity();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointDeleteFileActivity_Construct")]
        public void SharepointDeleteFileActivity_Construct_GivenInstance_ShouldNotBeNull()
        {
            //------------Setup for test--------------------------
            var sharepointDeleteFileActivity = CreateActivity();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointDeleteFileActivity);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointDeleteFile_Execute")]
        public void SharepointSource_DoesNotExist_OnResourceCatalog_ShouldSetSharepointSource_ToGuidEmpty()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointDeleteFile";
            var resourceId = Guid.NewGuid();
            var sharepointDeleteFileActivity = new SharepointDeleteFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Deleted]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite"
            };

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            var privateObject = new PrivateObject(sharepointDeleteFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointDeleteFileActivity.SharepointSource = mockSharepointSource.Object;

            Assert.AreEqual(resourceId, sharepointDeleteFileActivity.SharepointServerResourceId);

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);

            Assert.AreEqual(Guid.Empty, sharepointDeleteFileActivity.SharepointServerResourceId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointDeleteFile_Execute")]
        public void SharepointSource_Exists_OnResourceCatalog_BlankRecordSet()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointDeleteFile";
            var resourceId = Guid.NewGuid();
            var sharepointDeleteFileActivity = new SharepointDeleteFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files(*).Name]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite"
            };

            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");

            var resourceCatalog = new Mock<IResourceCatalog>();

            var mockSharepointHelper = new Mock<ISharepointHelper>();
            mockSharepointHelper.Setup(helper => helper.Delete(It.IsAny<string>())).Returns("Success");

            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);

            var privateObject = new PrivateObject(sharepointDeleteFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointDeleteFileActivity.SharepointSource = mockSharepointSource;

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);
            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointDeleteFile_Execute")]
        public void SharepointSource_Exists_OnResourceCatalog_StarRecordSet()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointDeleteFile";
            var resourceId = Guid.NewGuid();
            var sharepointDeleteFileActivity = new SharepointDeleteFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files(*).Name]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite"
            };

            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");

            var resourceCatalog = new Mock<IResourceCatalog>();

            var mockSharepointHelper = new Mock<ISharepointHelper>();
            mockSharepointHelper.Setup(helper => helper.Delete(It.IsAny<string>())).Returns("Success");

            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);

            var privateObject = new PrivateObject(sharepointDeleteFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointDeleteFileActivity.SharepointSource = mockSharepointSource;

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);
            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointDeleteFile_Execute")]
        [ExpectedException(typeof(TargetInvocationException))]
        public void SharepointDeleteFileActivity_ValidateRequest_SharepointServerResourceId_EmptyGuid()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointDeleteFile";
            var sharepointDeleteFileActivity = new SharepointDeleteFileActivity
            {
                DisplayName = activityName,
                Result = "[[Files(*).Name]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite"
            };

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource.Object);

            var privateObject = new PrivateObject(sharepointDeleteFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointDeleteFileActivity.SharepointSource = mockSharepointSource.Object;

            //------------Execute Test---------------------------
            privateObject.Invoke("ValidateRequest");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointDeleteFile_Execute")]
        [ExpectedException(typeof(TargetInvocationException))]
        public void SharepointDeleteFileActivity_ValidateRequest_ServerInputPath_IsEmpty()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointDeleteFile";
            var resourceId = Guid.NewGuid();
            var sharepointDeleteFileActivity = new SharepointDeleteFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files(*).Name]]"
            };

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource.Object);

            var privateObject = new PrivateObject(sharepointDeleteFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointDeleteFileActivity.SharepointSource = mockSharepointSource.Object;

            //------------Execute Test---------------------------
            privateObject.Invoke("ValidateRequest");
        }
    }
}
