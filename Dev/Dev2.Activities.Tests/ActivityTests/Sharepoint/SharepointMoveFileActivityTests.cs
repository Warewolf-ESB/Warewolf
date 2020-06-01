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
    public class SharepointMoveFileActivityTests : BaseActivityUnitTest
    {
        SharepointMoveFileActivity CreateActivity()
        {
            return new SharepointMoveFileActivity();
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUploadActivity_Construct")]
        public void SharepointFileUploadActivity_Construct_GivenInstance_ShouldNotBeNull()
        {
            //------------Setup for test--------------------------
            var sharepointMoveFileActivity = CreateActivity();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointMoveFileActivity);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUpload_Execute")]
        public void SharepointSource_DoesNotExist_OnResourceCatalog_ShouldSetSharepointSource_ToGuidEmpty()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointMoveFile";
            var resourceId = Guid.NewGuid();
            var sharepointMoveFileActivity = new SharepointMoveFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Move]]",
                ServerInputPathFrom = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                ServerInputPathTo = "Hello World.bite",
                Overwrite = true
            };

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            var privateObject = new PrivateObject(sharepointMoveFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointMoveFileActivity.SharepointSource = mockSharepointSource.Object;

            Assert.AreEqual(resourceId, sharepointMoveFileActivity.SharepointServerResourceId);

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);

            Assert.AreEqual(Guid.Empty, sharepointMoveFileActivity.SharepointServerResourceId);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUpload_Execute")]
        public void SharepointSource_Exists_OnResourceCatalog_BlankRecordSet()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointMoveFile";
            var resourceId = Guid.NewGuid();
            var sharepointMoveFileActivity = new SharepointMoveFileActivity
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
            mockSharepointHelper.Setup(helper => helper.MoveFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns("Success");

            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);

            var privateObject = new PrivateObject(sharepointMoveFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointMoveFileActivity.SharepointSource = mockSharepointSource;

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
            const string activityName = "SharepointMoveFile";
            var resourceId = Guid.NewGuid();
            var sharepointMoveFileActivity = new SharepointMoveFileActivity
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
            mockSharepointHelper.Setup(helper => helper.MoveFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns("Success");

            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);

            var privateObject = new PrivateObject(sharepointMoveFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointMoveFileActivity.SharepointSource = mockSharepointSource;

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
            const string activityName = "SharepointMoveFile";
            var sharepointMoveFileActivity = new SharepointMoveFileActivity
            {
                DisplayName = activityName,
                Result = "[[Files(*).Name]]",
                ServerInputPathFrom = @"C:\ProgramData\Warewolf\Resources\Hello World.bite"
            };

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource.Object);

            var privateObject = new PrivateObject(sharepointMoveFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointMoveFileActivity.SharepointSource = mockSharepointSource.Object;

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
            const string activityName = "SharepointMoveFile";
            var resourceId = Guid.NewGuid();
            var sharepointMoveFileActivity = new SharepointMoveFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files(*).Name]]"
            };

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource.Object);

            var privateObject = new PrivateObject(sharepointMoveFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointMoveFileActivity.SharepointSource = mockSharepointSource.Object;

            //------------Execute Test---------------------------
            privateObject.Invoke("ValidateRequest");
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("sharepointMoveFileActivity_GetState")]
        public void SharepointMoveFileActivity_GetState()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointMoveFile";
            var sharepointServerResourceId = Guid.NewGuid();
            var result = "[[Move]]";
            var overwrite = true;
            var serverInputPathFrom = @"C:\ProgramData\Warewolf\Resources\Hello World.bite";
            var serverInputPathTo = "Hello World.bite";
            var sharepointMoveFileActivity = new SharepointMoveFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = sharepointServerResourceId,
                Result = result,
                ServerInputPathFrom = serverInputPathFrom,
                ServerInputPathTo = serverInputPathTo,
                Overwrite = overwrite
            };
            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource.Object);


            var privateObject = new PrivateObject(sharepointMoveFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointMoveFileActivity.SharepointSource = mockSharepointSource.Object;

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);

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
                    Name="ServerInputPathFrom",
                    Type = StateVariable.StateType.Input,
                    Value = serverInputPathFrom
                 },
                new StateVariable
                {
                    Name="ServerInputPathTo",
                    Type = StateVariable.StateType.Input,
                    Value = serverInputPathTo
                },
                new StateVariable
                {
                    Name="Overwrite",
                    Type = StateVariable.StateType.Input,
                    Value = overwrite.ToString()
                }
                ,
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = result
                }
            };
            //---------------Test Result -----------------------
            var stateItems = sharepointMoveFileActivity.GetState();
            Assert.AreEqual(5, stateItems.Count());
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
