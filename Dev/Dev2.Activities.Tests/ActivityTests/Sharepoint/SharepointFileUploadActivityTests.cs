using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ActivityUnitTests;
using Dev2.Activities.Sharepoint;
using Dev2.Common.Interfaces;
using Dev2.Common.State;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using static Dev2.Tests.Activities.ActivityTests.Sharepoint.SharepointCopyFileActivityTests;

namespace Dev2.Tests.Activities.ActivityTests.Sharepoint
{
    [TestClass]
    public class SharepointFileUploadActivityTests : BaseActivityUnitTest
    {
        SharepointFileUploadActivity CreateActivity()
        {
            return new SharepointFileUploadActivity();
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUploadActivity_Construct")]
        public void SharepointFileUploadActivity_Construct_GivenInstance_ShouldNotBeNull()
        {
            //------------Setup for test--------------------------
            var sharepointFileUploadActivity = CreateActivity();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointFileUploadActivity);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUpload_Execute")]
        public void SharepointSource_DoesNotExist_OnResourceCatalog_ShouldSetSharepointSource_ToGuidEmpty()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointFileUpload";
            var resourceId = Guid.NewGuid();
            var sharepointFileUploadActivity = new SharepointFileUploadActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Deleted]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite"
            };

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            var privateObject = new PrivateObject(sharepointFileUploadActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointFileUploadActivity.SharepointSource = mockSharepointSource.Object;

            Assert.AreEqual(resourceId, sharepointFileUploadActivity.SharepointServerResourceId);

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);

            Assert.AreEqual(Guid.Empty, sharepointFileUploadActivity.SharepointServerResourceId);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUpload_Execute")]
        public void SharepointSource_Exists_OnResourceCatalog_BlankRecordSet()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointFileUpload";
            var resourceId = Guid.NewGuid();
            var sharepointFileUploadActivity = new SharepointFileUploadActivity
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
            mockSharepointHelper.Setup(helper => helper.UploadFile(It.IsAny<string>(), It.IsAny<string>())).Returns("Success");

            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);

            var privateObject = new PrivateObject(sharepointFileUploadActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointFileUploadActivity.SharepointSource = mockSharepointSource;

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);
            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result[0]);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUpload_Execute")]
        public void SharepointSource_Exists_OnResourceCatalog_StarRecordSet()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointFileUpload";
            var resourceId = Guid.NewGuid();
            var sharepointFileUploadActivity = new SharepointFileUploadActivity
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
            mockSharepointHelper.Setup(helper => helper.UploadFile(It.IsAny<string>(), It.IsAny<string>())).Returns("Success");

            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);

            var privateObject = new PrivateObject(sharepointFileUploadActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointFileUploadActivity.SharepointSource = mockSharepointSource;

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);
            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result[0]);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUpload_Execute")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointFileUploadActivity_ValidateRequest_SharepointServerResourceId_EmptyGuid()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointFileUpload";
            var sharepointFileUploadActivity = new SharepointFileUploadActivity
            {
                DisplayName = activityName,
                Result = "[[Files(*).Name]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite"
            };

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource.Object);

            var privateObject = new PrivateObject(sharepointFileUploadActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointFileUploadActivity.SharepointSource = mockSharepointSource.Object;

            //------------Execute Test---------------------------
            privateObject.Invoke("ValidateRequest");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUpload_Execute")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointFileUploadActivity_ValidateRequest_ServerInputPath_IsEmpty()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointFileUpload";
            var resourceId = Guid.NewGuid();
            var sharepointFileUploadActivity = new SharepointFileUploadActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files(*).Name]]"
            };

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource.Object);

            var privateObject = new PrivateObject(sharepointFileUploadActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointFileUploadActivity.SharepointSource = mockSharepointSource.Object;

            //------------Execute Test---------------------------
            privateObject.Invoke("ValidateRequest");
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("SharepointFileUploadGetState")]
        public void SharepointFileUpload_GetState()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointFileUpload";
            var sharepointServerResourceId = Guid.NewGuid();
            var result = "[[Files().Name]]";
            var serverInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite";
            var localInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite";
            var sharepointFileUploadActivity = new SharepointFileUploadActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = sharepointServerResourceId,
                Result = "[[Files().Name]]",
                ServerInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                LocalInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite"
            };

            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");
            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointHelper = new Mock<ISharepointHelper>();
            mockSharepointHelper.Setup(helper => helper.UploadFile(It.IsAny<string>(), It.IsAny<string>())).Returns("Success");
            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };
            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);

            var privateObject = new PrivateObject(sharepointFileUploadActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointFileUploadActivity.SharepointSource = mockSharepointSource;

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Result--------------------------
            var expectedResults = new[]
           {
                new StateVariable
                {
                    Name="SharepointServerResourceId",
                    Type = StateVariable.StateType.Input,
                    Value = sharepointServerResourceId.ToString()
                 },
                 new StateVariable
                {
                    Name="LocalInputPath",
                    Type = StateVariable.StateType.Input,
                    Value = localInputPath
                 },
                new StateVariable
                {
                    Name="ServerInputPath",
                    Type = StateVariable.StateType.Input,
                    Value = serverInputPath
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = result
                }
            };
            //---------------Test Result -----------------------
            var stateItems = sharepointFileUploadActivity.GetState();
            Assert.AreEqual(4, stateItems.Count());
            var iter = stateItems.Select(
                (item, index) => new
                {
                    value = item,
                    expectValue = expectedResults[index]
                }
                );

            //------------Assert Results-------------------------
            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }
    }
}
