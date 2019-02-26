using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dev2.Activities.DropBox2016;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Activities.DropBox2016.UploadActivity;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.State;
using Dev2.Common.Wrappers;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Tests.Activities.ActivityTests.DropBox2016.DropboxFiles;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityComparerTests.DropBox2016
{
    [TestClass]
    public class DsfDropBoxUploadActivityComparerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_UniqueIDEquals_EmptyDropBoxDeleteActivities_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity() { UniqueID = uniqueId };
            var multiAssign1 = new DsfDropBoxUploadActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_UniqueID_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity() { UniqueID = uniqueId };
            var multiAssign1 = new DsfDropBoxUploadActivity() { UniqueID = Guid.NewGuid().ToString() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var multiAssign1 = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var multiAssign1 = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var multiAssign1 = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert DsfDropBoxUploadActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_Result_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, Result = "a" };
            var multiAssign1 = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, Result = "A" };
            var multiAssign1 = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, Result = "AAA" };
            var multiAssign1 = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, Result = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_FromPath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, FromPath = "a" };
            var multiAssign1 = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, FromPath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_FromPath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, FromPath = "A" };
            var multiAssign1 = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, FromPath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_FromPath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, FromPath = "AAA" };
            var multiAssign1 = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, FromPath = "aaa" };
            //---------------Assert DsfDropBoxUploadActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_ToPath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, ToPath = "a" };
            var multiAssign1 = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, ToPath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_ToPath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, ToPath = "A" };
            var multiAssign1 = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, ToPath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_ToPath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, ToPath = "AAA" };
            var multiAssign1 = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, ToPath = "aaa" };
            //---------------Assert DsfDropBoxUploadActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_DropBoxSource_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dropBoxDeleteActivity = new DsfDropBoxUploadActivity { UniqueID = uniqueId, Result = "A"};
            var dropBoxDeleteActivity1 = new DsfDropBoxUploadActivity { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1));
            //---------------Execute Test ----------------------
            dropBoxDeleteActivity.SelectedSource = new DropBoxSource()
            {
                ResourceID = Guid.NewGuid()
            };
            dropBoxDeleteActivity1.SelectedSource = new DropBoxSource();
            var @equals = dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_DropBoxSource_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dropBoxDeleteActivity = new DsfDropBoxUploadActivity { UniqueID = uniqueId, Result = "A" };
            var dropBoxDeleteActivity1 = new DsfDropBoxUploadActivity { UniqueID = uniqueId, Result = "A"};
            //---------------Assert Precondition----------------
            Assert.IsTrue(dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1));
            //---------------Execute Test ----------------------
            dropBoxDeleteActivity.SelectedSource = new DropBoxSource();
            dropBoxDeleteActivity1.SelectedSource = new DropBoxSource();
            var @equals = dropBoxDeleteActivity.Equals(dropBoxDeleteActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_AddMode_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.AddMode = true;
            rabbitMqActivity1.AddMode = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_AddMode_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.AddMode = true;
            rabbitMqActivity1.AddMode = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_OverWriteMode_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.OverWriteMode = true;
            rabbitMqActivity1.OverWriteMode = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_OverWriteMode_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfDropBoxUploadActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.OverWriteMode = true;
            rabbitMqActivity1.OverWriteMode = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid();
            var selectedSource = new MockOAuthSource(uniqueId);

            //------------Setup for test--------------------------
            var dropBoxUploadActivity = new DsfDropBoxUploadActivity
            {
                SelectedSource = selectedSource,
                FromPath = "Path_From",
                ToPath = "Path_To",
                OverWriteMode = false,
                Result = "Uploaded"
            };
            //------------Execute Test---------------------------
            var stateItems = dropBoxUploadActivity.GetState();
            Assert.AreEqual(5, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "SelectedSource.ResourceID",
                    Type = StateVariable.StateType.Input,
                    Value = uniqueId.ToString()
                },
                new StateVariable
                {
                    Name = "FromPath",
                    Type = StateVariable.StateType.Input,
                    Value = "Path_From"
                },
                new StateVariable
                {
                    Name = "ToPath",
                    Type = StateVariable.StateType.Input,
                    Value = "Path_To"
                },
                new StateVariable
                {
                    Name = "OverWriteMode",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = "Uploaded"
                }
            };

            var iter = dropBoxUploadActivity.GetState().Select(
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_ObjectEquals_IsFalse_ExpectFalse()
        {
            //-----------------------Arrange----------------------------
            var obj = new object();

            var dsfDropBoxUploadActivity = new DsfDropBoxUploadActivity();

            obj = new DsfDropBoxUploadActivity();
            //-----------------------Act--------------------------------
            //-----------------------Assert-----------------------------
            Assert.IsFalse(dsfDropBoxUploadActivity.Equals(obj));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_ObjectEquals_IsTrue_ExpectTrue()
        {
            //-----------------------Arrange----------------------------
            var obj = new object();

            var dsfDropBoxUploadActivity = new DsfDropBoxUploadActivity();

            obj = dsfDropBoxUploadActivity;
            //-----------------------Act--------------------------------
            //-----------------------Assert-----------------------------
            Assert.IsTrue(dsfDropBoxUploadActivity.Equals(obj));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_ObjectEquals_IsNotExpectedObject_ExpectFalse()
        {
            //-----------------------Arrange----------------------------
            var dsfDropBoxUploadActivity = new DsfDropBoxUploadActivity();
            //-----------------------Act--------------------------------
            //-----------------------Assert-----------------------------
            Assert.IsFalse(dsfDropBoxUploadActivity.Equals(new object()));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_Equals_ExpectFalse()
        {
            //-----------------------Arrange----------------------------
            var dsfDropBoxUploadActivity = new DsfDropBoxUploadActivity();
            //-----------------------Act--------------------------------
            //-----------------------Assert-----------------------------
            Assert.IsFalse(dsfDropBoxUploadActivity.Equals(new DsfDropBoxUploadActivity()));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_Equals_IsNull_ExpectFalse()
        {
            //-----------------------Arrange----------------------------
            var dsfDropBoxUploadActivity = new DsfDropBoxUploadActivity();
            //-----------------------Act--------------------------------
            //-----------------------Assert-----------------------------
            Assert.IsFalse(dsfDropBoxUploadActivity.Equals(null));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropBoxUploadActivity_Equals_IsEqual_ExpectTrue()
        {
            //-----------------------Arrange----------------------------
            var dsfDropBoxUploadActivity = new DsfDropBoxUploadActivity();
            //-----------------------Act--------------------------------
            //-----------------------Assert-----------------------------
            Assert.IsTrue(dsfDropBoxUploadActivity.Equals(dsfDropBoxUploadActivity));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropboxFileListActivity_GetHashCode_Properties_NotNull_ExpertTrue()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var dsfDropBoxUploadActivity = new DsfDropBoxUploadActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = new DropBoxSource(),
                ToPath = "TestToPath",
                DisplayName = "TestDisplayName"
            };
            //--------------------------Act--------------------------------
            var getHash = dsfDropBoxUploadActivity.GetHashCode();
            //--------------------------Assert-----------------------------
            Assert.IsNotNull(getHash);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropboxFileListActivity_GetHashCode_Properties_IsNull_ExpertTrue()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();

            var dsfDropboxFileListActivity = new DsfDropBoxUploadActivity(mockDropboxClientFactory.Object);
            //--------------------------Act--------------------------------
            var getHash = dsfDropboxFileListActivity.GetHashCode();
            //--------------------------Assert-----------------------------
            Assert.IsNotNull(getHash);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_IsFilesSelected_True_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDownloadResponse = new Mock<IDownloadResponse<FileMetadata>>();
            var mockDropboxSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();

            var listFolderResult = new ListFolderResult(new List<Metadata>() { new Mock<Metadata>().Object }, "TestCusor", false);

            var task = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object);
            
            mockDownloadResponse.Setup(o => o.Response).Returns(new Mock<FileMetadata>().Object);
            task.Start();
            mockDropboxClient.Setup(o => o.DownloadAsync(It.IsAny<DownloadArg>())).Returns(() => { var t = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object); t.Start(); return t; });
            mockDropboxSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxListFolderSuccesResult(listFolderResult));
           
            var dictionery = new Dictionary<string, string> { };
            dictionery.Add("ToPath", @"C:\Users\temp\testToPath\");
            dictionery.Add("FromPath", @"C:\Users\temp");

            var dsfDropboxFileListActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = new DropBoxSource(),
            };
            //--------------------------Act--------------------------------
            var listPerformExecution = dsfDropboxFileListActivity.TestPerformExecution(dictionery);
            //--------------------------Assert-----------------------------
            Assert.AreEqual(1, listPerformExecution.Count);
            Assert.AreEqual("Success", listPerformExecution[0]);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_IsFoldersSelected_True_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDownloadResponse = new Mock<IDownloadResponse<FileMetadata>>();
            var mockDropboxSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();

            var listFolderResult = new ListFolderResult(new List<Metadata>() { new Mock<Metadata>().Object }, "TestCusor", false);

            var task = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object);
            
            mockDownloadResponse.Setup(o => o.Response).Returns(new Mock<FileMetadata>().Object);
            task.Start();
            mockDropboxClient.Setup(o => o.DownloadAsync(It.IsAny<DownloadArg>())).Returns(() => { var t = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object); t.Start(); return t; });
            mockDropboxSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxListFolderSuccesResult(listFolderResult));

            var dictionery = new Dictionary<string, string> { };
            dictionery.Add("ToPath", @"C:\Users\temp\testToPath\");
            dictionery.Add("FromPath", @"C:\Users\temp");

            var dsfDropboxFileListActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = new DropBoxSource(),
            };
            //--------------------------Act--------------------------------
            var listPerformExecution = dsfDropboxFileListActivity.TestPerformExecution(dictionery);
            //--------------------------Assert-----------------------------
            Assert.AreEqual(1, listPerformExecution.Count);
            Assert.AreEqual("Success", listPerformExecution[0]);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_IncludeDeleted_True_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDownloadResponse = new Mock<IDownloadResponse<FileMetadata>>();
            var mockDropboxSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();

            var listFolderResult = new ListFolderResult(new List<Metadata>() { new Mock<Metadata>().Object }, "TestCusor", false);

            var task = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object);

            var metaList = new ListFolderResult();
            mockDownloadResponse.Setup(o => o.Response).Returns(new Mock<FileMetadata>().Object);
            task.Start();
            mockDropboxClient.Setup(o => o.DownloadAsync(It.IsAny<DownloadArg>())).Returns(() => { var t = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object); t.Start(); return t; });
            mockDropboxSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxListFolderSuccesResult(listFolderResult));

            var dictionery = new Dictionary<string, string> { };
            dictionery.Add("ToPath", @"C:\Users\temp\testToPath\");
            dictionery.Add("FromPath", @"C:\temp");

            var dsfDropboxFileListActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = new DropBoxSource(),
            };
            //--------------------------Act--------------------------------
            var listPerformExecution = dsfDropboxFileListActivity.TestPerformExecution(dictionery);
            //--------------------------Assert-----------------------------
            Assert.AreEqual(1, listPerformExecution.Count);
            Assert.AreEqual("Success", listPerformExecution[0]);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_IsFilesAndFoldersSelected_True_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDownloadResponse = new Mock<IDownloadResponse<FileMetadata>>();
            var mockDropboxSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();

            var listFolderResult = new ListFolderResult(new List<Metadata>() { new Mock<Metadata>().Object }, "TestCusor", false);

            var task = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object);
            
            mockDownloadResponse.Setup(o => o.Response).Returns(new Mock<FileMetadata>().Object);
            task.Start();
            mockDropboxClient.Setup(o => o.DownloadAsync(It.IsAny<DownloadArg>())).Returns(() => { var t = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object); t.Start(); return t; });
            mockDropboxSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxListFolderSuccesResult(listFolderResult));

            var dictionery = new Dictionary<string, string> { };
            dictionery.Add("ToPath", @"C:\Users\temp\testToPath\");
            dictionery.Add("FromPath", @"C:\Users\temp");

            var dsfDropboxFileListActivity = new TestDsfDropBoxUploadActivity(mockDropboxClientFactory.Object)
            {
                SelectedSource = new DropBoxSource(),
            };
            //--------------------------Act--------------------------------
            var listPerformExecution = dsfDropboxFileListActivity.TestPerformExecution(dictionery);
            //--------------------------Assert-----------------------------
            Assert.AreEqual(1, listPerformExecution.Count);
            Assert.AreEqual("Success", listPerformExecution[0]);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_GetDebugInputs_Null_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();

            var dsfDropboxFileListActivity = new DsfDropBoxUploadActivity(mockDropboxClientFactory.Object);
            //--------------------------Act--------------------------------
            var list = dsfDropboxFileListActivity.GetDebugInputs(null, 0);
            //--------------------------Assert-----------------------------
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_GetDebugInputs_NotNull_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory= new Mock<IDropboxClientFactory>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

            var dsfDropboxFileListActivity = new DsfDropBoxUploadActivity(mockDropboxClientFactory.Object);
            //--------------------------Act--------------------------------
            var list = dsfDropboxFileListActivity.GetDebugInputs(mockExecutionEnvironment.Object, 0);
            //--------------------------Assert-----------------------------
            Assert.AreEqual(2,list.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxUploadActivity))]
        public void DsfDropboxFileListActivity_PerformExecution_GetDebugInputs_NotNull_WithSetProperties_ExpertSuccess()
        {
            //--------------------------Arrange----------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

            var dsfDropboxFileListActivity = new DsfDropBoxUploadActivity(mockDropboxClientFactory.Object);
            //--------------------------Act--------------------------------
            var list = dsfDropboxFileListActivity.GetDebugInputs(mockExecutionEnvironment.Object, 0);
            //--------------------------Assert-----------------------------
            Assert.AreEqual(2, list.Count);
        }


        class TestDsfDropBoxUploadActivity : DsfDropBoxUploadActivity
        {
            public Mock<IDropboxSingleExecutor<IDropboxResult>> MockSingleExecutor { get; internal set; }

            public TestDsfDropBoxUploadActivity(IDropboxClientFactory dropboxClientFactory)
                : base(dropboxClientFactory)
            {

            }

            public TestDsfDropBoxUploadActivity()
                : this(new DropboxClientWrapperFactory())
            {

            }

            public void TestExecuteTool(IDSFDataObject dataObject, int update)
            {
                base.ExecuteTool(dataObject, update);
            }

            public List<string> TestPerformExecution(Dictionary<string, string> evaluatedValues)
            {
                return base.PerformExecution(evaluatedValues);
            }

            public void TestAssignResult(IDSFDataObject dSFDataObject, int update)
            {
                base.AssignResult(dSFDataObject, update);
            }
        }
    }
}