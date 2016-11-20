using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2.Activities.DropBox2016;
using Dev2.Activities.DropBox2016.DownloadActivity;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Activities.DropBox2016.UploadActivity;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.ServiceModel;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dropbox.Api;
using Dropbox.Api.Babel;
using Dropbox.Api.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedVariable

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016.Download
{
    [TestClass]
    public class DsfDropBoxDownloadAcivtityTestShould
    {
        private static DsfDropBoxDownloadActivity CreateDropboxActivity()
        {
            return new DsfDropBoxDownloadActivity();
        }

        private static IExecutionEnvironment CreateExecutionEnvironment()
        {
            return new ExecutionEnvironment();
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfDropBoxUpload_GivenNewInstance_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var dropBoxDownloadActivity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropBoxDownloadActivity);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateNewActivity_GivenIsNew_ShouldHaveDisplayName()
        {
            //---------------Set up test pack-------------------
            var dropBoxDownloadActivity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropBoxDownloadActivity);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual("Download from Dropbox", dropBoxDownloadActivity.DisplayName);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DropboxFile_GivenIsNew_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var dropBoxDownloadActivity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            Assert.AreEqual("Download from Dropbox", dropBoxDownloadActivity.DisplayName);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropBoxDownloadActivity.DropboxFile);

        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetFindMissingType_GivenIsNew_ShouldSetDatagridAcitivity()
        {
            //---------------Set up test pack-------------------
            var dropBoxDownloadActivity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var enFindMissingType = dropBoxDownloadActivity.GetFindMissingType();
            //---------------Test Result -----------------------
            Assert.AreEqual(enFindMissingType.StaticActivity, enFindMissingType);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugInputs_GivenEnvironmentIsNull_ShouldHaveNoDebugOutputs()
        {
            //---------------Set up test pack-------------------
            var dropBoxDownloadActivity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugInputs = dropBoxDownloadActivity.GetDebugInputs(null, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugInputs.Count());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugInputs_GivenEnvironmentMockEnvironmentAndFromPath_ShouldHaveOneDebugOutputs()
        {
            //---------------Set up test pack-------------------
            var dropBoxDownloadActivity = CreateDropboxActivity();
            dropBoxDownloadActivity.FromPath = "Random.txt";
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var debugInputs = dropBoxDownloadActivity.GetDebugInputs(CreateExecutionEnvironment(), 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, debugInputs.Count());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugInputs_GivenEnvironmentMockEnvironmentAndToPathNotExecuted_ShouldHaveOneDebugOutputs()
        {
            //---------------Set up test pack-------------------
            var dropBoxDownloadActivity = CreateDropboxActivity();
            dropBoxDownloadActivity.ToPath = "Random.txt";
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var debugInputs = dropBoxDownloadActivity.GetDebugInputs(CreateExecutionEnvironment(), 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, debugInputs.Count());
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugInputs_GivenEnvironmentMockEnvironmentAndToPathAndFromPath_ShouldHaveTwoDebugOutputs()
        {
            //---------------Set up test pack-------------------
            var dropBoxDownloadActivity = CreateDropboxActivity();
            var environment = CreateExecutionEnvironment();
            dropBoxDownloadActivity.ToPath = "Random.txt";
            dropBoxDownloadActivity.FromPath = "Random.txt";
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var debugInputs = dropBoxDownloadActivity.GetDebugInputs(environment, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, debugInputs.Count());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugOutputs_GivenNullEnvironment_ShouldHaveNoDebugOutPuts()
        {
            //---------------Set up test pack-------------------
            var dropBoxDownloadActivity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugOutputs = dropBoxDownloadActivity.GetDebugOutputs(null, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugOutputs.Count());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugOutputs_GivenFileMetadataIsNull_ShouldHaveNoDebugOutPuts()
        {
            //---------------Set up test pack-------------------
            var dropBoxDownloadActivity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugOutputs = dropBoxDownloadActivity.GetDebugOutputs(CreateExecutionEnvironment(), 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugOutputs.Count());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugOutputs_GivenFileMetadataIsNotNull_ShouldHaveOneDebugOutPuts()
        {
            //---------------Set up test pack-------------------
            var dropBoxDownloadActivity = new Mock<DsfDropBoxUploadActivity>();
            dropBoxDownloadActivity.SetupAllProperties();
            dropBoxDownloadActivity.Setup(acivtity => acivtity.GetDebugOutputs(It.IsAny<IExecutionEnvironment>(), It.IsAny<int>()))
                .Returns(new List<DebugItem>()
                {
                    new DebugItem()
                });
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugOutputs = dropBoxDownloadActivity.Object.GetDebugOutputs(CreateExecutionEnvironment(), 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, debugOutputs.Count());
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugOutputs_GivenWebRequestSuccess_ShouldCorrectDebugValue()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dropBoxDownloadActivityMock = new DsfDropBoxDownloadActivityMock() { IsUplodValidSuccess = true };
            dropBoxDownloadActivityMock.GetDropboxSingleExecutor(mockExecutor.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropBoxDownloadActivityMock);
            //---------------Execute Test ----------------------
            var esbChannel = new Mock<IEsbChannel>().Object;
            var datObj = new Mock<IDSFDataObject>().Object;
            var executionEnvironment = new Mock<IExecutionEnvironment>().Object;
            // ReSharper disable once RedundantAssignment
            var errorResultTO = new ErrorResultTO();
            dropBoxDownloadActivityMock.Execute(esbChannel, datObj, String.Empty, String.Empty, out  errorResultTO, 0);
            var debugOutputs = dropBoxDownloadActivityMock.GetDebugOutputs(executionEnvironment, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugOutputs.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteTool_GivenNoFromPath_ShouldAddError()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxDownloadSuccessResult(TestConstant.FileDownloadResponseInstance.Value));
            var dropBoxDownloadActivityMock = new DsfDropBoxDownloadActivityMock();
            dropBoxDownloadActivityMock.GetDropboxSingleExecutor(mockExecutor.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropBoxDownloadActivityMock);
            //---------------Execute Test ----------------------
            var datObj = new Mock<IDSFDataObject>();
            var executionEnvironment = new Mock<IExecutionEnvironment>();
            datObj.Setup(o => o.Environment).Returns(executionEnvironment.Object);
            // ReSharper disable once RedundantAssignment
            IDSFDataObject dataObject = datObj.Object;
            dropBoxDownloadActivityMock.Execute(dataObject, 0);
            //---------------Test Result -----------------------
            executionEnvironment.Verify(environment => environment.AddError("Please confirm that the correct file location has been entered"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteTool_GivenNoToPath_ShouldAddError()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dropBoxDownloadActivityMock = new DsfDropBoxDownloadActivityMock() { FromPath = "File.txt" };
            dropBoxDownloadActivityMock.GetDropboxSingleExecutor(mockExecutor.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropBoxDownloadActivityMock);
            //---------------Execute Test ----------------------
            var datObj = new Mock<IDSFDataObject>();
            var executionEnvironment = new Mock<IExecutionEnvironment>();
            datObj.Setup(o => o.Environment).Returns(executionEnvironment.Object);
            // ReSharper disable once RedundantAssignment
            IDSFDataObject dataObject = datObj.Object;
            dropBoxDownloadActivityMock.Execute(dataObject, 0);
            //---------------Test Result -----------------------
            executionEnvironment.Verify(environment => environment.AddError("Please confirm that the correct file destination has been entered"));
        }
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteTool_GivenAllPaths_ShouldExecuteTool()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dropBoxDownloadActivityMock = new DsfDropBoxDownloadActivityMock() { FromPath = "File.txt" , ToPath = "Test.a"};
            dropBoxDownloadActivityMock.GetDropboxSingleExecutor(mockExecutor.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropBoxDownloadActivityMock);
            //---------------Execute Test ----------------------
            var datObj = new Mock<IDSFDataObject>();
            var executionEnvironment = new Mock<IExecutionEnvironment>();
            datObj.Setup(o => o.Environment).Returns(executionEnvironment.Object);
            // ReSharper disable once RedundantAssignment
            IDSFDataObject dataObject = datObj.Object;
            var dev2Activity = dropBoxDownloadActivityMock.Execute(dataObject, 0);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(ArgumentException))]
        public void PerformExecution_GivenNoPaths_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxDownloadSuccessResult(TestConstant.FileDownloadResponseInstance.Value));
            var dropBoxDownloadActivityMock = new DsfDropBoxDownloadActivityMock();
            dropBoxDownloadActivityMock.GetDropboxSingleExecutor(mockExecutor.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropBoxDownloadActivityMock);
            //---------------Execute Test ----------------------
            dropBoxDownloadActivityMock.PerfomBaseExecution(new Dictionary<string, string>()
            {
                {"FromPath",""},
                {"ToPath",""}
            });
            //---------------Test Result -----------------------
            Assert.Fail("Exception Not Thrown");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PerformExecution_GivenPaths_ShouldNotThrowException()
        {
            var singleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var mockResponse = new Mock<IDownloadResponse<FileMetadata>>();
            var mockFile = new Mock<IFile>();
            mockFile.Setup(file => file.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>()));
            var succesResult = new Mock<DropboxDownloadSuccessResult>(It.IsAny<IDownloadResponse<FileMetadata>>());

            succesResult.Setup(result => result.GetDownloadResponse())
                .Returns(mockResponse.Object);
            succesResult.SetupGet(result => result.GetDownloadResponse().Response)
                .Returns(new FileMetadata());

            singleExecutor.Setup(executor => executor.ExecuteTask(It.IsAny<DropboxClient>()))
                .Returns(succesResult.Object);


            var localPathManager = new Mock<ILocalPathManager>();
            Func<string> tempFileName = Path.GetTempFileName;
            localPathManager.Setup(manager => manager.GetFullFileName()).Returns(tempFileName);

            var activity = new Mock<DsfDropBoxDownloadActivityMockForFiles>();
            activity.Setup(downloadActivity => downloadActivity.GetDropboxSingleExecutor(It.IsAny<IDropboxSingleExecutor<IDropboxResult>>()))
                .Returns(singleExecutor.Object);
            activity.SetupGet(downloadActivity => downloadActivity.LocalPathManager).Returns(localPathManager.Object);
            activity.SetupGet(files => files.DropboxFile).Returns(mockFile.Object);
            const string homeExe = @"\home.exe";
            var execution = activity.Object.PerfomBaseExecution(new Dictionary<string, string>()
            {
                {"FromPath",homeExe},
                {"ToPath","Home"}
            });

            Assert.AreEqual(GlobalConstants.DropBoxSuccess, execution);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(Exception), "Test Exception")]
        public void PerformExecution_GivenNoDropboxFilePaths_ShouldThrowException()
        {
            var singleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var exception = new Exception("Test Exception");
            var mockFile = new Mock<IFile>();
            mockFile.Setup(file => file.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>()));
            var succesResult = new Mock<DropboxFailureResult>(exception);

            succesResult.Setup(result => result.GetException())
                .Returns(exception);

            singleExecutor.Setup(executor => executor.ExecuteTask(It.IsAny<DropboxClient>()))
                .Returns(succesResult.Object);


            var localPathManager = new Mock<ILocalPathManager>();
            Func<string> tempFileName = Path.GetTempFileName;
            localPathManager.Setup(manager => manager.GetFullFileName()).Returns(tempFileName);

            var activity = new Mock<DsfDropBoxDownloadActivityMockForFiles>();
            activity.Setup(downloadActivity => downloadActivity.GetDropboxSingleExecutor(It.IsAny<IDropboxSingleExecutor<IDropboxResult>>()))
                .Returns(singleExecutor.Object);
            activity.Setup(downloadActivity => downloadActivity.GetLocalPathManager()).Returns(localPathManager.Object);
            activity.SetupGet(files => files.DropboxFile).Returns(mockFile.Object);
            const string homeExe = @"\home.exe";
            var execution = activity.Object.PerfomBaseExecution(new Dictionary<string, string>()
            {
                {"FromPath",homeExe},
                {"ToPath","Home"}
            });

            Assert.Fail("Exception not Thrown");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PerformExecution_Givennot_fileException_ShouldHaveValidMessage()
        {
            try
            {
                var singleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
                var exception = new Exception("Test Exception not_file");
                var mockFile = new Mock<IFile>();
                mockFile.Setup(file => file.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>()));
                var succesResult = new Mock<DropboxFailureResult>(exception);

                succesResult.Setup(result => result.GetException())
                    .Returns(exception);

                singleExecutor.Setup(executor => executor.ExecuteTask(It.IsAny<DropboxClient>()))
                    .Returns(succesResult.Object);

                var localPathManager = new Mock<ILocalPathManager>();
                Func<string> tempFileName = Path.GetTempFileName;
                localPathManager.Setup(manager => manager.GetFullFileName()).Returns(tempFileName);

                var activity = new Mock<DsfDropBoxDownloadActivityMockForFiles>();
                activity.Setup(downloadActivity => downloadActivity.GetDropboxSingleExecutor(It.IsAny<IDropboxSingleExecutor<IDropboxResult>>()))
                    .Returns(singleExecutor.Object);
                activity.Setup(downloadActivity => downloadActivity.GetLocalPathManager()).Returns(localPathManager.Object);
                activity.SetupGet(files => files.DropboxFile).Returns(mockFile.Object);
                const string homeExe = @"\home.exe";
                var execution = activity.Object.PerfomBaseExecution(new Dictionary<string, string>()
            {
                {"FromPath",homeExe},
                {"ToPath","Home"}
            });

                Assert.Fail("Exception not Thrown");
            }
            catch (Exception ex)
            {

                Assert.AreEqual(Warewolf.Resource.Errors.ErrorResource.DropBoxFilePathMissing, ex.Message);
            }

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PerformExecution_GivenFileExistAndOverwriteFalse_ShouldHaveValidMessage()
        {
            try
            {
                var singleExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
                var mockResponse = new Mock<IDownloadResponse<FileMetadata>>();
                var mockFile = new Mock<IFile>();
                mockFile.Setup(file => file.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>()));
                mockFile.Setup(file => file.Exists(It.IsAny<string>()))
                    .Returns(true);
                var succesResult = new Mock<DropboxDownloadSuccessResult>(It.IsAny<IDownloadResponse<FileMetadata>>());

                succesResult.Setup(result => result.GetDownloadResponse())
                    .Returns(mockResponse.Object);
                succesResult.SetupGet(result => result.GetDownloadResponse().Response)
                            .Returns(new FileMetadata());
                singleExecutor.Setup(executor => executor.ExecuteTask(It.IsAny<DropboxClient>()))
                    .Returns(succesResult.Object);

                var localPathManager = new Mock<ILocalPathManager>();
                
                Func<string> tempFileName = Path.GetTempFileName;

                localPathManager.Setup(manager => manager.GetFullFileName()).Returns(tempFileName);
                localPathManager.Setup(manager => manager.FileExist()).Returns(true);

                var activity = new Mock<DsfDropBoxDownloadActivityMockForFiles>();
                activity.Setup(downloadActivity => downloadActivity.GetDropboxSingleExecutor(It.IsAny<IDropboxSingleExecutor<IDropboxResult>>()))
                    .Returns(singleExecutor.Object);
                activity.SetupGet(downloadActivity => downloadActivity.LocalPathManager).Returns(localPathManager.Object);
                activity.SetupGet(files => files.DropboxFile).Returns(mockFile.Object);
                const string homeExe = @"\home.exe";
                var execution = activity.Object.PerfomBaseExecution(new Dictionary<string, string>()
            {
                {"FromPath",homeExe},
                {"ToPath","Home"}
            });

                Assert.Fail("Exception not Thrown");
            }
            catch (Exception ex)
            {

                Assert.AreEqual("Destination File already exists and overwrite is set to false", ex.Message);
            }

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetLocalPathManager_GivenLocalManagerIsSet_ShouldReturnLocalManager()
        {
            //---------------Set up test pack-------------------
            var activity = new Mock<DsfDropBoxDownloadActivityMockForFiles>();
            var mock = new Mock<ILocalPathManager>();
            activity.SetupGet(files => files.LocalPathManager).Returns(mock.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(activity.Object.LocalPathManager, mock.Object);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetLocalPathManager_GivenLocalManagerIsSet_ShouldCorrectLocalManager()
        {
            //---------------Set up test pack-------------------
            var activity = new DsfDropBoxDownloadActivity();
            var mock = new Mock<ILocalPathManager>();
            activity.LocalPathManager = mock.Object;
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(activity.LocalPathManager, activity.GetLocalPathManager());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugInputs_GivenValues_ShouldAddDebugInputs()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dropBoxDownloadActivityMock = new DsfDropBoxDownloadActivityMock() { FromPath = "File.txt", ToPath = "Test.a" };
            dropBoxDownloadActivityMock.GetDropboxSingleExecutor(mockExecutor.Object);
            dropBoxDownloadActivityMock.SelectedSource =
                new DropBoxSource
                {
                    AccessToken = "Test"
                };
            dropBoxDownloadActivityMock.GetDropboxSingleExecutor(mockExecutor.Object);
            dropBoxDownloadActivityMock.OverwriteFile = true;
            dropBoxDownloadActivityMock.ToPath = @"C\test.tst";
            dropBoxDownloadActivityMock.FromPath = @"C\test.tst";
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var mockExecutionEnv = new Mock<IExecutionEnvironment>();
            List<DebugItem> debugInputs = dropBoxDownloadActivityMock.GetDebugInputs(mockExecutionEnv.Object, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, debugInputs.Count());
        }
    }

    public class DsfDropBoxDownloadActivityMockForFiles : DsfDropBoxDownloadActivity
    {
        public string PerfomBaseExecution(Dictionary<string, string> dictionaryValues)
        {
            // ReSharper disable once RedundantBaseQualifier
            var perfomBaseExecution = base.PerformExecution(dictionaryValues);
            return perfomBaseExecution[0];
        }
    }
}