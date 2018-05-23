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
    public class SharepointReadFolderItemActivityTests : BaseActivityUnitTest
    {
        SharepointReadFolderItemActivity CreateActivity()
        {
            return new SharepointReadFolderItemActivity();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUploadActivity_Construct")]
        public void SharepointFileUploadActivity_Construct_GivenInstance_ShouldNotBeNull()
        {
            //------------Setup for test--------------------------
            var sharepointReadFolderItemActivity = CreateActivity();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointReadFolderItemActivity);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUpload_Execute")]
        public void SharepointSource_DoesNotExist_OnResourceCatalog_ShouldSetSharepointSource_ToGuidEmpty()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointReadFolderItem";
            var resourceId = Guid.NewGuid();
            var sharepointReadFolderItemActivity = new SharepointReadFolderItemActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Move]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite"
            };

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            var privateObject = new PrivateObject(sharepointReadFolderItemActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointReadFolderItemActivity.SharepointSource = mockSharepointSource.Object;

            Assert.AreEqual(resourceId, sharepointReadFolderItemActivity.SharepointServerResourceId);

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);

            Assert.AreEqual(Guid.Empty, sharepointReadFolderItemActivity.SharepointServerResourceId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUpload_Execute")]
        public void SharepointSource_Exists_OnResourceCatalog_BlankRecordSet_IsFilesSelected()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointReadFolderItem";
            var resourceId = Guid.NewGuid();
            var sharepointReadFolderItemActivity = new SharepointReadFolderItemActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files().Name]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                IsFilesSelected = true
            };

            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");

            var resourceCatalog = new Mock<IResourceCatalog>();

            var mockSharepointHelper = new Mock<ISharepointHelper>();
            mockSharepointHelper.Setup(helper => helper.LoadFiles(It.IsAny<string>())).Returns(new List<string> { "Success" });

            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);

            var privateObject = new PrivateObject(sharepointReadFolderItemActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointReadFolderItemActivity.SharepointSource = mockSharepointSource;

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);
            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUpload_Execute")]
        public void SharepointSource_Exists_OnResourceCatalog_BlankRecordSet_IsFoldersSelected()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointReadFolderItem";
            var resourceId = Guid.NewGuid();
            var sharepointReadFolderItemActivity = new SharepointReadFolderItemActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files().Name]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                IsFoldersSelected = true
            };

            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");

            var resourceCatalog = new Mock<IResourceCatalog>();

            var mockSharepointHelper = new Mock<ISharepointHelper>();
            mockSharepointHelper.Setup(helper => helper.LoadFolders(It.IsAny<string>())).Returns(new List<string> { "Success" });

            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);

            var privateObject = new PrivateObject(sharepointReadFolderItemActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointReadFolderItemActivity.SharepointSource = mockSharepointSource;

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);
            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUpload_Execute")]
        public void SharepointSource_Exists_OnResourceCatalog_BlankRecordSet_IsFilesAndFoldersSelected()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointReadFolderItem";
            var resourceId = Guid.NewGuid();
            var sharepointReadFolderItemActivity = new SharepointReadFolderItemActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files().Name]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                IsFilesAndFoldersSelected = true
            };

            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");

            var resourceCatalog = new Mock<IResourceCatalog>();

            var mockSharepointHelper = new Mock<ISharepointHelper>();
            mockSharepointHelper.Setup(helper => helper.LoadFiles(It.IsAny<string>())).Returns(new List<string> { "Success" });
            mockSharepointHelper.Setup(helper => helper.LoadFolders(It.IsAny<string>())).Returns(new List<string> { "Success" });

            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);

            var privateObject = new PrivateObject(sharepointReadFolderItemActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointReadFolderItemActivity.SharepointSource = mockSharepointSource;

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);
            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result[0]);
            Assert.AreEqual("Success", result[1]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUpload_Execute")]
        public void SharepointSource_Exists_OnResourceCatalog_StarRecordSet_IsFilesSelected()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointReadFolderItem";
            var resourceId = Guid.NewGuid();
            var sharepointReadFolderItemActivity = new SharepointReadFolderItemActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files().Name]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                IsFilesSelected = true
            };

            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");

            var resourceCatalog = new Mock<IResourceCatalog>();

            var mockSharepointHelper = new Mock<ISharepointHelper>();
            mockSharepointHelper.Setup(helper => helper.LoadFiles(It.IsAny<string>())).Returns(new List<string> { "Success" });

            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);

            var privateObject = new PrivateObject(sharepointReadFolderItemActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointReadFolderItemActivity.SharepointSource = mockSharepointSource;

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);
            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUpload_Execute")]
        public void SharepointSource_Exists_OnResourceCatalog_StarRecordSet_IsFoldersSelected()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointReadFolderItem";
            var resourceId = Guid.NewGuid();
            var sharepointReadFolderItemActivity = new SharepointReadFolderItemActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files().Name]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                IsFoldersSelected = true
            };

            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");

            var resourceCatalog = new Mock<IResourceCatalog>();

            var mockSharepointHelper = new Mock<ISharepointHelper>();
            mockSharepointHelper.Setup(helper => helper.LoadFolders(It.IsAny<string>())).Returns(new List<string> { "Success" });

            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);

            var privateObject = new PrivateObject(sharepointReadFolderItemActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointReadFolderItemActivity.SharepointSource = mockSharepointSource;

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);
            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUpload_Execute")]
        public void SharepointSource_Exists_OnResourceCatalog_StarRecordSet_IsFilesAndFoldersSelected()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointReadFolderItem";
            var resourceId = Guid.NewGuid();
            var sharepointReadFolderItemActivity = new SharepointReadFolderItemActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files().Name]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                IsFilesAndFoldersSelected = true
            };

            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");

            var resourceCatalog = new Mock<IResourceCatalog>();

            var mockSharepointHelper = new Mock<ISharepointHelper>();
            mockSharepointHelper.Setup(helper => helper.LoadFiles(It.IsAny<string>())).Returns(new List<string> { "Success" });
            mockSharepointHelper.Setup(helper => helper.LoadFolders(It.IsAny<string>())).Returns(new List<string> { "Success" });

            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);

            var privateObject = new PrivateObject(sharepointReadFolderItemActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointReadFolderItemActivity.SharepointSource = mockSharepointSource;

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);
            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result[0]);
            Assert.AreEqual("Success", result[1]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUpload_Execute")]
        [ExpectedException(typeof(TargetInvocationException))]
        public void SharepointFileUploadActivity_ValidateRequest_SharepointServerResourceId_EmptyGuid()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointReadFolderItem";
            var sharepointReadFolderItemActivity = new SharepointReadFolderItemActivity
            {
                DisplayName = activityName,
                Result = "[[Files(*).Name]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite"
            };

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource.Object);

            var privateObject = new PrivateObject(sharepointReadFolderItemActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointReadFolderItemActivity.SharepointSource = mockSharepointSource.Object;

            //------------Execute Test---------------------------
            privateObject.Invoke("ValidateRequest");
        }
    }
}
