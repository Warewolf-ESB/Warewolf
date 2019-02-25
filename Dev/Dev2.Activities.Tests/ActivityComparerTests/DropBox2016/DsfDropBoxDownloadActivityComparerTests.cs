using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dev2.Activities.DropBox2016;
using Dev2.Activities.DropBox2016.DownloadActivity;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.State;
using Dev2.Common.Wrappers;
using Dev2.Data.ServiceModel;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityComparerTests.DropBox2016
{
    [TestClass]
    public class DsfDropBoxDownloadActivityComparerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxDownloadActivity_UniqueIDEquals_EmptyDropBoxDeleteActivities_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxDownloadActivity_UniqueID_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = Guid.NewGuid().ToString() };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxDownloadActivity_Equals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, DisplayName = "a" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, DisplayName = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxDownloadActivity_Equals_Given_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, DisplayName = "A" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, DisplayName = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxDownloadActivity_Equals_Given_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, DisplayName = "AAA" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, DisplayName = "aaa" };
            //---------------Assert DsfDropBoxDownloadActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxDownloadActivity_Result_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "a" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxDownloadActivity_Result_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "A" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxDownloadActivity_Result_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "AAA" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "aaa" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxDownloadActivity_FromPath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, FromPath = "a" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, FromPath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxDownloadActivity_FromPath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, FromPath = "A" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, FromPath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxDownloadActivity_FromPath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, FromPath = "AAA" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, FromPath = "aaa" };
            //---------------Assert DsfDropBoxDownloadActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxDownloadActivity_ToPath_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, ToPath = "a" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, ToPath = "a" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxDownloadActivity_ToPath_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, ToPath = "A" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, ToPath = "ass" };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxDownloadActivity_ToPath_Different_Object_Is_Not_Equal_CaseSensitive()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var multiAssign = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, ToPath = "AAA" };
            var multiAssign1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, ToPath = "aaa" };
            //---------------Assert DsfDropBoxDownloadActivity----------------
            Assert.IsNotNull(multiAssign);
            //---------------Execute Test ----------------------
            var @equals = multiAssign.Equals(multiAssign1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxDownloadActivity_DropBoxSource_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dropBoxDeleteActivity = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, Result = "A"};
            var dropBoxDeleteActivity1 = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, Result = "A" };
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
        public void DsfDropBoxDownloadActivity_DropBoxSource_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var dropBoxDeleteActivity = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, Result = "A" };
            var dropBoxDeleteActivity1 = new DsfDropBoxDownloadActivity { UniqueID = uniqueId, Result = "A"};
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
        public void DsfDropBoxDownloadActivity_OverwriteFile_Different_Object_Is_Not_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "A", };
            var rabbitMqActivity1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.OverwriteFile = true;
            rabbitMqActivity1.OverwriteFile = false;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxDownloadActivity_OverwriteFile_Same_Object_Is_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var rabbitMqActivity = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "A" };
            var rabbitMqActivity1 = new DsfDropBoxDownloadActivity() { UniqueID = uniqueId, Result = "A" };
            //---------------Assert Precondition----------------
            Assert.IsTrue(rabbitMqActivity.Equals(rabbitMqActivity1));
            //---------------Execute Test ----------------------
            rabbitMqActivity.OverwriteFile = true;
            rabbitMqActivity1.OverwriteFile = true;
            var @equals = rabbitMqActivity.Equals(rabbitMqActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DsfDropBoxDownloadActivity_GetState")]
        public void DsfDropBoxDownloadActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid();
            var selectedSource = new MockOAuthSource(uniqueId);

            //------------Setup for test--------------------------
            var dropBoxDownloadActivity = new DsfDropBoxDownloadActivity
            {
                SelectedSource = selectedSource,
                FromPath = "Path_From",
                ToPath = "Path_To",
                OverwriteFile = false,
                Result = "Downloaded"
            };
            //------------Execute Test---------------------------
            var stateItems = dropBoxDownloadActivity.GetState();
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
                    Name = "OverwriteFile",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = "Downloaded"
                }
            };

            var iter = dropBoxDownloadActivity.GetState().Select(
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
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_ObjectEquals_IsFalse_ExpectFalse()
        {
            //-----------------------Arrange----------------------------
            var mockLocalPathManager = new Mock<ILocalPathManager>();
            var obj = new object();

            var dsfDropBoxDownloadActivity = new DsfDropBoxDownloadActivity();
            
            obj = new DsfDropBoxDownloadActivity();
            //-----------------------Act--------------------------------
            //-----------------------Assert-----------------------------
            Assert.IsFalse(dsfDropBoxDownloadActivity.Equals(obj));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_ObjectEquals_IsTrue_ExpectTrue()
        {
            //-----------------------Arrange----------------------------
            var mockLocalPathManager = new Mock<ILocalPathManager>();
            var obj = new object();

            var dsfDropBoxDownloadActivity = new DsfDropBoxDownloadActivity();

            obj = dsfDropBoxDownloadActivity;
            //-----------------------Act--------------------------------
            //-----------------------Assert-----------------------------
            Assert.IsTrue(dsfDropBoxDownloadActivity.Equals(obj));
        }
        
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_ObjectEquals_IsNotExpectedObject_ExpectFalse()
        {
            //-----------------------Arrange----------------------------
            var mockLocalPathManager = new Mock<ILocalPathManager>();

            var dsfDropBoxDownloadActivity = new DsfDropBoxDownloadActivity();
            //-----------------------Act--------------------------------
            //-----------------------Assert-----------------------------
            Assert.IsFalse(dsfDropBoxDownloadActivity.Equals(new object()));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_Equals_ExpectFalse()
        {
            //-----------------------Arrange----------------------------
            var dsfDropBoxDownloadActivity = new DsfDropBoxDownloadActivity();
            //-----------------------Act--------------------------------
            //-----------------------Assert-----------------------------
            Assert.IsFalse(dsfDropBoxDownloadActivity.Equals(new DsfDropBoxDownloadActivity()));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_Equals_IsNull_ExpectFalse()
        {
            //-----------------------Arrange----------------------------
            var dsfDropBoxDownloadActivity = new DsfDropBoxDownloadActivity();
            //-----------------------Act--------------------------------
            //-----------------------Assert-----------------------------
            Assert.IsFalse(dsfDropBoxDownloadActivity.Equals(null));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_Equals_IsEqual_ExpectTrue()
        {
            //-----------------------Arrange----------------------------
            var dsfDropBoxDownloadActivity = new DsfDropBoxDownloadActivity();
            //-----------------------Act--------------------------------
            //-----------------------Assert-----------------------------
            Assert.IsTrue(dsfDropBoxDownloadActivity.Equals(dsfDropBoxDownloadActivity));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_GetDebugOutputs_ExecutionEnvironment_IsNull_ExpectFalse()
        {
            //-----------------------Arrange----------------------------
            //-----------------------Act--------------------------------
            var dsfDropBoxDownloadActivity = new DsfDropBoxDownloadActivity();
            //-----------------------Assert-----------------------------
            Assert.AreEqual(0, dsfDropBoxDownloadActivity.GetDebugInputs(null, 0).Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_GetDebugOutputs_ExecutionEnvironment_IsNotNull_ExpectTrue()
        {
            //-----------------------Arrange----------------------------
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            //-----------------------Act--------------------------------
            var dsfDropBoxDownloadActivity = new DsfDropBoxDownloadActivity();
            //-----------------------Assert-----------------------------
            Assert.AreEqual(1, dsfDropBoxDownloadActivity.GetDebugInputs(mockExecutionEnvironment.Object, 0).Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_LocalPathManager_SetProperty_AreEqual_ExpectTrue()
        {
            //-----------------------Arrange----------------------------
            var mockLocalPathManager = new Mock<ILocalPathManager>();

            //-----------------------Act--------------------------------
            var dsfDropBoxDownloadActivity = new DsfDropBoxDownloadActivity()
            {
                LocalPathManager = mockLocalPathManager.Object
            };
            //-----------------------Assert-----------------------------
            Assert.AreEqual(mockLocalPathManager.Object, dsfDropBoxDownloadActivity.LocalPathManager);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_ExecuteTool_FromPath_IsNullOrEmpty_VerifyMethodCall_3Times_ExpectTrue()
        {
            //-----------------------Arrange----------------------------
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            mockDSFDataObject.Setup(o => o.Environment.AddError(It.IsAny<string>()));

            var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity()
            {
                FromPath = null,
                ToPath = null
            };
            //-----------------------Act--------------------------------
            dsfDropBoxDownloadActivity.TestExecuteTool(mockDSFDataObject.Object, 0);
            //-----------------------Assert-----------------------------
            mockDSFDataObject.Verify(o => o.Environment.AddError(It.IsAny<string>()), Times.Exactly(3));
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_GetHashCode_PropertiesNull_IsNull_ExpectTrue()
        {
            //-----------------------Arrange----------------------------
            var dsfDropBoxDownloadActivity = new DsfDropBoxDownloadActivity();
            //-----------------------Act--------------------------------
            //-----------------------Assert-----------------------------
            Assert.IsNotNull(dsfDropBoxDownloadActivity.GetHashCode());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_GetHashCode_PropertiesNull_IsNotNull_ExpectTrue()
        {
            //-----------------------Arrange----------------------------
            var dsfDropBoxDownloadActivity = new DsfDropBoxDownloadActivity()
            {
                ToPath = "TestToPath",
                FromPath = "TestFromPath",
                SelectedSource = new DropBoxSource()
            };
            //-----------------------Act--------------------------------
            //-----------------------Assert-----------------------------
            Assert.IsNotNull(dsfDropBoxDownloadActivity.GetHashCode());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_PerformExecution_ExpectAreEqual()
        {
            //-----------------------Arrange----------------------------
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDropboxSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var mockDownloadResponse = new Mock<IDownloadResponse<FileMetadata>>();
            var dropboxDownloadSuccessResult     = new Mock<DropboxDownloadSuccessResult>();
            var mockFile = new Mock<IFile>();

            var task = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object);

            mockDownloadResponse.Setup(o => o.Response).Returns(new Mock<FileMetadata>().Object);
            task.Start();
            mockDSFDataObject.Setup(o => o.Environment.AddError(It.IsAny<string>()));
            mockDropboxClient.Setup(o => o.DownloadAsync(It.IsAny<DownloadArg>())).Returns(()=> { var t = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object); t.Start(); return t; });
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            mockDropboxSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxDownloadSuccessResult(mockDownloadResponse.Object));

            var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object)
            {
                DropboxFile = mockFile.Object
            };

            var dictionery = new Dictionary<string, string> { };
            dictionery.Add("ToPath", "TestToPath");
            dictionery.Add("FromPath", @"C:\Users\temp");
            //-----------------------Act--------------------------------
            var list = dsfDropBoxDownloadActivity.TestPerformExecution(dictionery);
            //-----------------------Assert-----------------------------
            Assert.AreEqual(1,list.Count);
            Assert.AreEqual("Success", list[0]);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxDownloadActivity))]
        public void DsfDropBoxDownloadActivity_PerformExecution_ExpectAreEqual1()
        {
            //-----------------------Arrange----------------------------
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockDropboxClient = new Mock<IDropboxClient>();
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDropboxSingleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var mockDownloadResponse = new Mock<IDownloadResponse<FileMetadata>>();
            var dropboxDownloadSuccessResult = new Mock<DropboxDownloadSuccessResult>();
            var mockFile = new Mock<IFile>();

            var task = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object);

            mockDownloadResponse.Setup(o => o.Response).Returns(new Mock<FileMetadata>().Object);
            task.Start();
            mockDSFDataObject.Setup(o => o.Environment.AddError(It.IsAny<string>()));
            mockDropboxClient.Setup(o => o.DownloadAsync(It.IsAny<DownloadArg>())).Returns(() => { var t = new Task<IDownloadResponse<FileMetadata>>(() => mockDownloadResponse.Object); t.Start(); return t; });
            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);
            mockDropboxSingleExecutor.Setup(o => o.ExecuteTask(It.IsAny<IDropboxClient>())).Returns(new DropboxFailureResult(new Exception()));

            var dsfDropBoxDownloadActivity = new TestDsfDropBoxDownloadActivity(mockDropboxClientFactory.Object)
            {
                DropboxFile = mockFile.Object
            };

            var dictionery = new Dictionary<string, string> { };
            dictionery.Add("ToPath", "TestToPath");
            dictionery.Add("FromPath", @"C:\Users\temp");
            //-----------------------Act--------------------------------
            var list = dsfDropBoxDownloadActivity.TestPerformExecution(dictionery);
            //-----------------------Assert-----------------------------
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Success", list[0]);
        }
    }

    class TestDsfDropBoxDownloadActivity : DsfDropBoxDownloadActivity
    {
        public TestDsfDropBoxDownloadActivity(IDropboxClientFactory dropboxClientFactory)
            :base(dropboxClientFactory)
        {

        }
        public TestDsfDropBoxDownloadActivity()
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
    }
}