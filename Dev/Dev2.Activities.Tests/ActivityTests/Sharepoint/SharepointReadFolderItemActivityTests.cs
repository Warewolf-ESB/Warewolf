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
    public class SharepointReadFolderItemActivityTests : BaseActivityUnitTest
    {
        SharepointReadFolderItemActivity CreateActivity()
        {
            return new SharepointReadFolderItemActivity();
        }

        [TestMethod]
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("SharepointFileUpload_Execute")]
        [ExpectedException(typeof(ArgumentNullException))]
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

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("SharepointReadFolderItemActivity_GetState")]
        public void SharepointReadFolderItemActivity_GetState_IsFilesSelected()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointReadFolderItem";
            var sharepointServerResourceId = Guid.NewGuid();
            var result = "[[Files().Name]]";
            var isFilesSelected = true;
            var serverInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite";
            var sharepointReadFolderItemActivity = new SharepointReadFolderItemActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = sharepointServerResourceId,
                Result = result,
                ServerInputPath = serverInputPath,
                IsFilesSelected = isFilesSelected
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
                    Name="ServerInputPath",
                    Type = StateVariable.StateType.Input,
                    Value = serverInputPath
                 },
                 new StateVariable
                {
                    Name="IsFilesAndFoldersSelected",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name="IsFoldersSelected",
                    Type = StateVariable.StateType.Input,
                    Value =  "False"
                },
                new StateVariable
                {
                    Name="IsFilesSelected",
                    Type = StateVariable.StateType.Input,
                    Value = isFilesSelected.ToString()
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = result
                }
            };
            //---------------Test Result -----------------------
            var stateItems = sharepointReadFolderItemActivity.GetState();
            Assert.AreEqual(6, stateItems.Count());
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

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("SharepointReadFolderItemActivity_GetState")]
        public void SharepointReadFolderItemActivity_GetState_IsFoldersSelected()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointReadFolderItem";
            var sharepointServerResourceId = Guid.NewGuid();
            var result = "[[Files().Name]]";
            var isFoldersSelected = true;
            var serverInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite";
            var sharepointReadFolderItemActivity = new SharepointReadFolderItemActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = sharepointServerResourceId,
                Result = result,
                ServerInputPath = serverInputPath,
                IsFoldersSelected = isFoldersSelected
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
                    Name="ServerInputPath",
                    Type = StateVariable.StateType.Input,
                    Value = serverInputPath
                 },
                new StateVariable
                {
                    Name="IsFilesAndFoldersSelected",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name="IsFoldersSelected",
                    Type = StateVariable.StateType.Input,
                    Value = isFoldersSelected.ToString()
                },
                new StateVariable
                {
                    Name="IsFilesSelected",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = result
                }
            };
            //---------------Test Result -----------------------
            var stateItems = sharepointReadFolderItemActivity.GetState();
            Assert.AreEqual(6, stateItems.Count());
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

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("SharepointReadFolderItemActivity_GetState")]
        public void SharepointReadFolderItemActivity_GetState_IsFilesAndFoldersSelected()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointReadFolderItem";
            var sharepointServerResourceId = Guid.NewGuid();
            var result = "[[Files().Name]]";
            var isFilesAndFoldersSelected = true;
            var serverInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite";

            var sharepointReadFolderItemActivity = new SharepointReadFolderItemActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = sharepointServerResourceId,
                Result = result,
                ServerInputPath = serverInputPath,
                IsFilesAndFoldersSelected = isFilesAndFoldersSelected
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
                    Name="ServerInputPath",
                    Type = StateVariable.StateType.Input,
                    Value = serverInputPath
                 },
                 new StateVariable
                {
                    Name="IsFilesAndFoldersSelected",
                    Type = StateVariable.StateType.Input,
                    Value = isFilesAndFoldersSelected.ToString()
                },
                new StateVariable
                {
                    Name="IsFoldersSelected",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name="IsFilesSelected",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = result
                }
            };
            //---------------Test Result -----------------------
            var stateItems = sharepointReadFolderItemActivity.GetState();
            Assert.AreEqual(6, stateItems.Count());
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

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory("SharepointReadFolderItemActivity_GetState")]
        public void SharepointReadFolderItemActivity_GetState_NoneSelected()
        {
            //------------Setup for test--------------------------
            const string activityName = "SharepointReadFolderItem";
            var sharepointServerResourceId = Guid.NewGuid();
            var result = "[[Files().Name]]";
            var serverInputPath = @"C:\ProgramData\Warewolf\Resources\Hello World.bite";

            var sharepointReadFolderItemActivity = new SharepointReadFolderItemActivity
            {
                DisplayName = activityName,
                SharepointServerResourceId = sharepointServerResourceId,
                Result = result,
                ServerInputPath = serverInputPath
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
                    Name="ServerInputPath",
                    Type = StateVariable.StateType.Input,
                    Value = serverInputPath
                 },
                 new StateVariable
                {
                    Name="IsFilesAndFoldersSelected",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name="IsFoldersSelected",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name="IsFilesSelected",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = result
                }
            };
            //---------------Test Result -----------------------
            var stateItems = sharepointReadFolderItemActivity.GetState();
            Assert.AreEqual(6, stateItems.Count());
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
