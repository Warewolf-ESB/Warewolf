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
using Dev2.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using static Dev2.Tests.Activities.ActivityTests.Sharepoint.SharepointCopyFileActivityTests;

namespace Dev2.Tests.Activities.ActivityTests.Sharepoint
{
    [TestClass]
    public class SharepointReadListActivityTests : BaseActivityUnitTest
    {
        SharepointReadListActivity CreateActivity()
        {
            return new SharepointReadListActivity();
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointReadListActivity_Construct")]
        public void SharepointReadListActivity_Construct_GivenInstance_ShouldNotBeNull()
        {
            //------------Setup for test--------------------------
            var sharepointReadListActivity = CreateActivity();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointReadListActivity);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointReadList_Execute")]
        public void SharepointSource_DoesNotExist_OnResourceCatalog_ShouldSetSharepointSource_ToGuidEmpty()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointReadList";
            var resourceId = Guid.NewGuid();
            var sharepointReadListActivity = new SharepointReadListActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                ReadListItems = new List<SharepointReadListTo>(),
                FilterCriteria = new List<SharepointSearchTo>(),
                RequireAllCriteriaToMatch = true,
                SharepointUtils = new SharepointUtils()
            };

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            var privateObject = new PrivateObject(sharepointReadListActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);

            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);

            Assert.AreEqual(resourceId, sharepointReadListActivity.SharepointServerResourceId);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointReadList_Execute")]
        public void SharepointSource_Exists_OnResourceCatalog_BlankRecordSet()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointReadList";
            var resourceId = Guid.NewGuid();
            var sharepointReadListActivity = new SharepointReadListActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                ReadListItems = new List<SharepointReadListTo>(),
                FilterCriteria = new List<SharepointSearchTo>(),
                RequireAllCriteriaToMatch = true,
                SharepointUtils = new SharepointUtils()
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

            var privateObject = new PrivateObject(sharepointReadListActivity);
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
        [TestCategory("SharepointReadListActivity_GetState")]
        public void SharepointReadListActivity_GetState()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointReadList";
            var resourceId = Guid.NewGuid();
            var filterCriteria = new List<SharepointSearchTo>()
                {
                    new SharepointSearchTo("A","A","",1)
                };

            var sharepointList = "SharepointList";
            var requireAllCriteriaToMatch = true;
            var readListItems = new List<SharepointReadListTo>();
            var sharepointUtils = new SharepointUtils();
            var sharepointReadListActivity = new SharepointReadListActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                ReadListItems = readListItems,
                FilterCriteria = filterCriteria,
                SharepointList = sharepointList,
                RequireAllCriteriaToMatch = requireAllCriteriaToMatch,
                SharepointUtils = sharepointUtils
            };
            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());
            var resourceCatalog = new Mock<IResourceCatalog>();
            var privateObject = new PrivateObject(sharepointReadListActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            var serializer = new Dev2JsonSerializer();
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
                    Type = StateVariable.StateType.InputOutput,
                    Value =  ActivityHelper.GetSerializedStateValueFromCollection(readListItems)
                 },
                new StateVariable
                {
                    Name="FilterCriteria",
                    Type = StateVariable.StateType.Input,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(filterCriteria)
                },
                new StateVariable
                {
                    Name="RequireAllCriteriaToMatch",
                    Type = StateVariable.StateType.Input,
                    Value = requireAllCriteriaToMatch.ToString()
                },
                new StateVariable
                {
                    Name="SharepointList",
                    Type = StateVariable.StateType.Input,
                    Value = sharepointList
                }
                
            };
            //---------------Test Result -----------------------
            var stateItems = sharepointReadListActivity.GetState();
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
