using System;
using System.Collections.Generic;
using System.Linq;
using ActivityUnitTests;
using Dev2.Activities.Sharepoint;
using Dev2.Common.Interfaces;
using Dev2.Common.State;
using Dev2.Communication;
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
    public class SharepointCreateListItemActivityTests : BaseActivityUnitTest
    {
        SharepointCreateListItemActivity CreateActivity()
        {
            return new SharepointCreateListItemActivity();
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointCreateListItemActivity_Construct")]
        public void SharepointCreateListItemActivity_Construct_GivenInstance_ShouldNotBeNull()
        {
            //------------Setup for test--------------------------
            var sharepointCreateListItemActivity = CreateActivity();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointCreateListItemActivity);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointCreateListItem_Execute")]
        public void SharepointSource_DoesNotExist_OnResourceCatalog_ShouldSetSharepointSource_ToGuidEmpty()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointCreateListItem";
            var resourceId = Guid.NewGuid();
            var sharepointCreateListItemActivity = new SharepointCreateListItemActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                ReadListItems = new List<SharepointReadListTo>()
            };

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            var privateObject = new PrivateObject(sharepointCreateListItemActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);

            Assert.AreEqual(resourceId, sharepointCreateListItemActivity.SharepointServerResourceId);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointCreateListItem_Execute")]
        public void SharepointSource_Exists_OnResourceCatalog_BlankRecordSet()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointCreateListItem";
            var resourceId = Guid.NewGuid();
            var sharepointCreateListItemActivity = new SharepointCreateListItemActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                ReadListItems = new List<SharepointReadListTo>()
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

            var privateObject = new PrivateObject(sharepointCreateListItemActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("SharepointCreateListItem_GetState")]
        public void SharepointCreateListItem_GetState()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointCreateListItem";
            var resourceId = Guid.NewGuid();
            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");
            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointHelper = new Mock<ISharepointHelper>();
            mockSharepointHelper.Setup(helper => helper.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns("Success");
            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };
            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);

            var sharepointList = "List";
            var uniqueId = Guid.NewGuid().ToString();
            var result = "[[result]]";
            var readListItems = new List<SharepointReadListTo>()
                {
                    new SharepointReadListTo("a","a","a","a")
                };
            var sharepointCreateListItemActivity = new SharepointCreateListItemActivity
            {
                UniqueID = uniqueId,
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                ReadListItems = readListItems,
                Result = result,
                SharepointList = sharepointList
            };
            var privateObject = new PrivateObject(sharepointCreateListItemActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Result--------------------------
            var serializer = new Dev2JsonSerializer();
            var inputReadListItems = serializer.Serialize(readListItems);
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
                    Name="ReadListItems",
                    Type = StateVariable.StateType.Input,
                    Value = inputReadListItems
                },
                  new StateVariable
                {
                    Name="SharepointList",
                    Type = StateVariable.StateType.Input,
                    Value = sharepointList
                },
                  new StateVariable
                {
                    Name="UniqueID",
                    Type = StateVariable.StateType.Input,
                    Value = uniqueId
                },
                 new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = result
                 }
            };
            var stateItems = sharepointCreateListItemActivity.GetState();
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
