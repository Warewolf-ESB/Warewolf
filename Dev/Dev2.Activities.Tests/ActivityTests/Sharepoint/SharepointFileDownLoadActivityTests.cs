﻿using System;
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
    public class SharepointFileDownLoadActivityTests : BaseActivityUnitTest
    {
        SharepointFileDownLoadActivity CreateActivity()
        {
            return new SharepointFileDownLoadActivity();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileDownLoadActivity_Construct")]
        public void SharepointFileDownLoadActivity_Construct_GivenInstance_ShouldNotBeNull()
        {
            //------------Setup for test--------------------------
            var sharepointFileDownLoadActivity = CreateActivity();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointFileDownLoadActivity);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileDownLoad_Execute")]
        public void SharepointSource_DoesNotExist_OnResourceCatalog_ShouldSetSharepointSource_ToGuidEmpty()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointFileDownLoad";
            var resourceId = Guid.NewGuid();
            var sharepointFileDownLoadActivity = new SharepointFileDownLoadActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Deleted]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite"
            };

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            var privateObject = new PrivateObject(sharepointFileDownLoadActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointFileDownLoadActivity.SharepointSource = mockSharepointSource.Object;

            Assert.AreEqual(resourceId, sharepointFileDownLoadActivity.SharepointServerResourceId);

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);

            Assert.AreEqual(Guid.Empty, sharepointFileDownLoadActivity.SharepointServerResourceId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileDownLoad_Execute")]
        public void SharepointSource_Exists_OnResourceCatalog_BlankRecordSet()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointFileDownLoad";
            var resourceId = Guid.NewGuid();
            var sharepointFileDownLoadActivity = new SharepointFileDownLoadActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files().Name]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                LocalInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite"
            };

            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");

            var resourceCatalog = new Mock<IResourceCatalog>();

            var mockSharepointHelper = new Mock<ISharepointHelper>();
            mockSharepointHelper.Setup(helper => helper.DownLoadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns("Success");

            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);

            var privateObject = new PrivateObject(sharepointFileDownLoadActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointFileDownLoadActivity.SharepointSource = mockSharepointSource;

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);
            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileDownLoad_Execute")]
        public void SharepointSource_Exists_OnResourceCatalog_StarRecordSet()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointFileDownLoad";
            var resourceId = Guid.NewGuid();
            var sharepointFileDownLoadActivity = new SharepointFileDownLoadActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files().Name]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                LocalInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite"
            };

            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");

            var resourceCatalog = new Mock<IResourceCatalog>();

            var mockSharepointHelper = new Mock<ISharepointHelper>();
            mockSharepointHelper.Setup(helper => helper.DownLoadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns("Success");

            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);

            var privateObject = new PrivateObject(sharepointFileDownLoadActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointFileDownLoadActivity.SharepointSource = mockSharepointSource;

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);
            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileDownLoad_Execute")]
        [ExpectedException(typeof(TargetInvocationException))]
        public void SharepointFileDownLoadActivity_ValidateRequest_SharepointServerResourceId_EmptyGuid()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointFileDownLoad";
            var sharepointFileDownLoadActivity = new SharepointFileDownLoadActivity
            {
                DisplayName = activityName,
                Result = "[[Files(*).Name]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite"
            };

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource.Object);

            var privateObject = new PrivateObject(sharepointFileDownLoadActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointFileDownLoadActivity.SharepointSource = mockSharepointSource.Object;

            //------------Execute Test---------------------------
            privateObject.Invoke("ValidateRequest");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileDownLoad_Execute")]
        [ExpectedException(typeof(TargetInvocationException))]
        public void SharepointFileDownLoadActivity_ValidateRequest_ServerInputPath_IsEmpty()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointFileDownLoad";
            var resourceId = Guid.NewGuid();
            var sharepointFileDownLoadActivity = new SharepointFileDownLoadActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files(*).Name]]"
            };

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource.Object);

            var privateObject = new PrivateObject(sharepointFileDownLoadActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointFileDownLoadActivity.SharepointSource = mockSharepointSource.Object;

            //------------Execute Test---------------------------
            privateObject.Invoke("ValidateRequest");
        }
    }
}
