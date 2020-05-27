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
    public class SharepointDeleteFileActivityTests : BaseActivityUnitTest
    {
        SharepointDeleteFileActivity CreateActivity()
        {
            return new SharepointDeleteFileActivity();
        }

        [TestMethod]
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointDeleteFile_Execute")]
        [ExpectedException(typeof(ArgumentNullException))]
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
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointDeleteFile_Execute")]
        [ExpectedException(typeof(ArgumentNullException))]
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


        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("SharepointDeleteFile_GetState")]
        public void SharepointDeleteFile_GetState()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointDeleteFile";
            var resourceId = Guid.NewGuid();
            var result = "[[result]]";
            var serverInputPath = "serverPath";
            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");
            var sharepointDeleteFileActivity = new SharepointDeleteFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = result,
                ServerInputPath = serverInputPath
            };
            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();
            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource.Object);
            var privateObject = new PrivateObject(sharepointDeleteFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointDeleteFileActivity.SharepointSource = mockSharepointSource.Object;
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Execute Test---------------------------
            var expectedResults = new[]
            {
                 new StateVariable
                {
                    Name="SharepointServerResourceId",
                    Type = StateVariable.StateType.Input,
                    Value = resourceId.ToString()
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
            var stateItems = sharepointDeleteFileActivity.GetState();
            Assert.AreEqual(3, stateItems.Count());
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
