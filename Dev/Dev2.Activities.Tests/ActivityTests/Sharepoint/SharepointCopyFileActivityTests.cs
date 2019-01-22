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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SharepointCopyFileActivity))]
        public void SharepointCopyFileActivity_Construct_GivenInstance_ShouldNotBeNull()
        {
            //------------Setup for test--------------------------
            var sharepointCopyFileActivity = CreateActivity();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointCopyFileActivity);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SharepointCopyFileActivity))]
        public void SharepointCopyFileActivity_SharepointSource_DoesNotExist_OnResourceCatalog_ShouldSetSharepointSource_ToGuidEmpty()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointCopyFile";

            var resourceId = Guid.NewGuid();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();
            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());
            var sharepointCopyFileActivity = new SharepointCopyFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Downloaded]]",
                ServerInputPathFrom = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                ServerInputPathTo = "Hello World.bite",
                Overwrite = true
            };
            var privateObject = new PrivateObject(sharepointCopyFileActivity);
            
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointCopyFileActivity.SharepointSource = mockSharepointSource.Object;
            //------------Execute Test---------------------------
            Assert.AreEqual(resourceId, sharepointCopyFileActivity.SharepointServerResourceId);

            privateObject.Invoke("ExecuteTool", dataObj, 0);

            Assert.AreEqual(Guid.Empty, sharepointCopyFileActivity.SharepointServerResourceId);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SharepointCopyFileActivity))]
        public void SharepointCopyFileActivity_SharepointSource_Exists_OnResourceCatalog_BlankRecordSet()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointCopyFile";

            var resourceId = Guid.NewGuid();
            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointHelper = new Mock<ISharepointHelper>();
            var sharepointCopyFileActivity = new SharepointCopyFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files().Name]]",
                ServerInputPathFrom = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                ServerInputPathTo = "Hello World.bite",
                Overwrite = true
            };
            var privateObject = new PrivateObject(sharepointCopyFileActivity);

            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };
            
            mockSharepointHelper.Setup(helper => helper.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns("Success");
            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);
            
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SharepointCopyFileActivity))]
        public void SharepointCopyFileActivity_SharepointSource_Exists_OnResourceCatalog_StarRecordSet()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointCopyFile";

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointHelper = new Mock<ISharepointHelper>();

            var resourceId = Guid.NewGuid();
            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");
            var sharepointCopyFileActivity = new SharepointCopyFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files(*).Name]]",
                ServerInputPathFrom = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                ServerInputPathTo = "Hello World.bite",
                Overwrite = true
            };
            var privateObject = new PrivateObject(sharepointCopyFileActivity);

            var mockSharepointSource = new MockSharepointSource
            {
                MockSharepointHelper = mockSharepointHelper.Object
            };
            
            mockSharepointHelper.Setup(helper => helper.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns("Success");
            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource);

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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SharepointCopyFileActivity))]
        [ExpectedException(typeof(TargetInvocationException))]
        public void SharepointCopyFileActivity_ValidateRequest_SharepointServerResourceId_EmptyGuid()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointCopyFile";

            var resourceCatalog = new Mock<IResourceCatalog>();

            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");
            var mockSharepointSource = new Mock<SharepointSource>();
            var sharepointCopyFileActivity = new SharepointCopyFileActivity
            {
                DisplayName = activityName,
                Result = "[[Files(*).Name]]",
                ServerInputPathFrom = @"C:\ProgramData\Warewolf\Resources\Hello World.bite",
                ServerInputPathTo = "Hello World.bite",
                Overwrite = true
            };
            var privateObject = new PrivateObject(sharepointCopyFileActivity);

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource.Object);
            
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointCopyFileActivity.SharepointSource = mockSharepointSource.Object;
            //------------Execute Test---------------------------
            privateObject.Invoke("ValidateRequest");
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);

            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result[0]);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SharepointCopyFileActivity))]
        [ExpectedException(typeof(TargetInvocationException))]
        public void SharepointCopyFileActivity_ValidateRequest_ServerInputPathFrom_IsEmpty_ExpectAreEqual_Success()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointCopyFile";

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            var resourceId = Guid.NewGuid();
            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");
            var sharepointCopyFileActivity = new SharepointCopyFileActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = resourceId,
                Result = "[[Files(*).Name]]",
                ServerInputPathTo = "Hello World.bite",
                Overwrite = true
            };

            var privateObject = new PrivateObject(sharepointCopyFileActivity);

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource.Object);

            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointCopyFileActivity.SharepointSource = mockSharepointSource.Object;
            //------------Execute Test---------------------------
            privateObject.Invoke("ValidateRequest");
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);

            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result[0]);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SharepointCopyFileActivity))]
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
                Overwrite = true,

            };

            var resourceCatalog = new Mock<IResourceCatalog>();
            var mockSharepointSource = new Mock<SharepointSource>();

            resourceCatalog.Setup(r => r.GetResource<SharepointSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockSharepointSource.Object);

            var dataObj = new DsfDataObject("", Guid.NewGuid(), "");
            var privateObject = new PrivateObject(sharepointCopyFileActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            sharepointCopyFileActivity.SharepointSource = mockSharepointSource.Object;

            //------------Execute Test---------------------------
            privateObject.Invoke("ValidateRequest");
            //------------Assert Result--------------------------
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "Files", "Name", out IList<string> result, out string error);

            Assert.IsNotNull(result);
            Assert.AreEqual("Success", result[0]);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("SharepointCopyFileActivity_GetState")]
        public void SharepointCopyFileActivity_GetState_ReturnsStateVariable()
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
            //------------Execute Test---------------------------
            var stateItems = sharepointCopyFileActivity.GetState();
            //------------Assert Results-------------------------
            Assert.AreEqual(4, stateItems.Count());
            Assert.AreEqual("ServerInputPathFrom", stateItems.ToList()[0].Name);
            Assert.AreEqual("ServerInputPathTo", stateItems.ToList()[1].Name);
            Assert.AreEqual("Overwrite", stateItems.ToList()[2].Name);
            Assert.AreEqual("Result", stateItems.ToList()[3].Name);

            Assert.AreEqual(StateVariable.StateType.Input, stateItems.ToList()[0].Type);
            Assert.AreEqual(StateVariable.StateType.Input, stateItems.ToList()[1].Type);
            Assert.AreEqual(StateVariable.StateType.Input, stateItems.ToList()[2].Type);
            Assert.AreEqual(StateVariable.StateType.Output, stateItems.ToList()[3].Type);

            Assert.AreEqual("C:\\ProgramData\\Warewolf\\Resources\\Hello World.bite", stateItems.ToList()[0].Value);
            Assert.AreEqual("Hello World.bite", stateItems.ToList()[1].Value);
            Assert.AreEqual("True", stateItems.ToList()[2].Value);
            Assert.AreEqual("[[Files(*).Name]]", stateItems.ToList()[3].Value);
        }

        //--------------------

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDEquals_EmptySharepointCopyFile_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointCopyFileActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId };
            var copyFileActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointCopyFileActivity);
            //---------------Execute Test ----------------------
            var equals = sharepointCopyFileActivity.Equals(copyFileActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UniqueIDDifferent_EmptySharepointCopyFile_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCopyFileActivity();
            var selectAndApplyActivity = new SharepointCopyFileActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var selectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var selectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var selectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServerInputPathFrom_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathFrom = "a" };
            var selectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathFrom = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServerInputPathFrom_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathFrom = "A" };
            var selectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathFrom = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServerInputPathFrom_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathFrom = "AAA" };
            var selectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathFrom = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServerInputPathTo_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathTo = "a" };
            var selectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathTo = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServerInputPathTo_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dsfSelectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathTo = "A" };
            var selectAndApplyActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathTo = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfSelectAndApplyActivity);
            //---------------Execute Test ----------------------
            var @equals = dsfSelectAndApplyActivity.Equals(selectAndApplyActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SharepointServerResourceId_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid();
            var sharepointCopyFileActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId.ToString(), SharepointServerResourceId = uniqueId };
            var sharepoint = new SharepointCopyFileActivity() { UniqueID = uniqueId.ToString(), SharepointServerResourceId = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointCopyFileActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointCopyFileActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SharepointServerResourceId_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointCopyFileActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, SharepointServerResourceId = Guid.NewGuid() };
            var sharepoint = new SharepointCopyFileActivity() { UniqueID = uniqueId, SharepointServerResourceId = Guid.NewGuid() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointCopyFileActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointCopyFileActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServerInputPathTo_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointCopyFileActivity = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathTo = "AAA" };
            var sharepoint = new SharepointCopyFileActivity() { UniqueID = uniqueId, ServerInputPathTo = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(sharepointCopyFileActivity);
            //---------------Execute Test ----------------------
            var @equals = sharepointCopyFileActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Overwrite_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var activity = new SharepointCopyFileActivity { UniqueID = uniqueId, Result = "A", Overwrite = true };
            var activity1 = new SharepointCopyFileActivity { UniqueID = uniqueId, Result = "A", Overwrite = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(activity.Equals(activity1));
            //---------------Execute Test ----------------------
            activity.Overwrite = true;
            activity1.Overwrite = false;
            var @equals = activity.Equals(activity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Overwrite_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var sharepointCopyFileActivity = new SharepointCopyFileActivity { UniqueID = uniqueId, Result = "A", Overwrite = true };
            var sharepoint = new SharepointCopyFileActivity { UniqueID = uniqueId, Result = "A", Overwrite = true };
            //---------------Assert Precondition----------------
            Assert.IsTrue(sharepointCopyFileActivity.Equals(sharepoint));
            //---------------Execute Test ----------------------
            sharepointCopyFileActivity.Overwrite = true;
            sharepoint.Overwrite = true;
            var @equals = sharepointCopyFileActivity.Equals(sharepoint);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }
    }
}
