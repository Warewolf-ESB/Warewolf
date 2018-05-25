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

namespace Dev2.Tests.Activities.ActivityTests.Sharepoint
{
    [TestClass]
    public partial class SharepointCopyFileActivityTests : BaseActivityUnitTest
    {
        SharepointCopyFileActivity CreateActivity()
        {
            return new SharepointCopyFileActivity();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointCopyFileActivity_Construct")]
        public void SharepointCopyFileActivity_Construct_GivenInstance_ShouldNotBeNull()
        {
            //------------Setup for test--------------------------
            var sharepointCopyFileActivity = CreateActivity();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointCopyFileActivity);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointCopyFile_Execute")]
        public void SharepointSource_DoesNotExist_OnResourceCatalog_ShouldSetSharepointSource_ToGuidEmpty()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointCopyFile";
            var resourceId = Guid.NewGuid();
            var sharepointCopyFileActivity = new SharepointCopyFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Downloaded]]",
                ServerInputPathFrom = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                ServerInputPathTo = "Hello World.bite",
                Overwrite = true
            };

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            var privateObject = new PrivateObject(sharepointCopyFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointCopyFileActivity.SharepointSource = mockSharepointSource.Object;

            Assert.AreEqual(resourceId, sharepointCopyFileActivity.SharepointServerResourceId);

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);

            Assert.AreEqual(Guid.Empty, sharepointCopyFileActivity.SharepointServerResourceId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointCopyFile_Execute")]
        public void SharepointSource_Exists_OnResourceCatalog_BlankRecordSet()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointCopyFile";
            var resourceId = Guid.NewGuid();
            var sharepointCopyFileActivity = new SharepointCopyFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files().Name]]",
                ServerInputPathFrom = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                ServerInputPathTo = "Hello World.bite",
                Overwrite = true
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

            var privateObject = new PrivateObject(sharepointCopyFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointCopyFileActivity.SharepointSource = mockSharepointSource;

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);
            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointCopyFile_Execute")]
        public void SharepointSource_Exists_OnResourceCatalog_StarRecordSet()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointCopyFile";
            var resourceId = Guid.NewGuid();
            var sharepointCopyFileActivity = new SharepointCopyFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files(*).Name]]",
                ServerInputPathFrom = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                ServerInputPathTo = "Hello World.bite",
                Overwrite = true
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

            var privateObject = new PrivateObject(sharepointCopyFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointCopyFileActivity.SharepointSource = mockSharepointSource;

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);
            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointCopyFile_Execute")]
        [ExpectedException(typeof(TargetInvocationException))]
        public void SharepointCopyFileActivity_ValidateRequest_SharepointServerResourceId_EmptyGuid()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointCopyFile";
            var sharepointCopyFileActivity = new SharepointCopyFileActivity
            {
                DisplayName = activityName,
                Result = "[[Files(*).Name]]",
                ServerInputPathFrom = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                ServerInputPathTo = "Hello World.bite",
                Overwrite = true
            };

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource.Object);

            var privateObject = new PrivateObject(sharepointCopyFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointCopyFileActivity.SharepointSource = mockSharepointSource.Object;

            //------------Execute Test---------------------------
            privateObject.Invoke("ValidateRequest");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointCopyFile_Execute")]
        [ExpectedException(typeof(TargetInvocationException))]
        public void SharepointCopyFileActivity_ValidateRequest_ServerInputPathFrom_IsEmpty()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointCopyFile";
            var resourceId = Guid.NewGuid();
            var sharepointCopyFileActivity = new SharepointCopyFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files(*).Name]]",
                ServerInputPathTo = "Hello World.bite",
                Overwrite = true
            };

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource.Object);

            var privateObject = new PrivateObject(sharepointCopyFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointCopyFileActivity.SharepointSource = mockSharepointSource.Object;

            //------------Execute Test---------------------------
            privateObject.Invoke("ValidateRequest");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointCopyFile_Execute")]
        [ExpectedException(typeof(TargetInvocationException))]
        public void SharepointCopyFileActivity_ValidateRequest_ServerInputPathTo_IsEmpty()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointCopyFile";
            var resourceId = Guid.NewGuid();
            var sharepointCopyFileActivity = new SharepointCopyFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files(*).Name]]",
                ServerInputPathFrom = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                Overwrite = true
            };

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource.Object);

            var privateObject = new PrivateObject(sharepointCopyFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointCopyFileActivity.SharepointSource = mockSharepointSource.Object;

            //------------Execute Test---------------------------
            privateObject.Invoke("ValidateRequest");
        }
    }
}
