using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Dev2.Activities.DropBox2016;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Activities.DropBox2016.UploadActivity;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dropbox.Api.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;



namespace Dev2.Tests.Activities.ActivityTests.DropBox2016.Upload
{
    [TestClass]
    public class DsfDropBoxUploadAcivtityTestShould
    {
        private static DsfDropBoxUploadActivity CreateDropboxActivity()
        {
            return new DsfDropBoxUploadActivity();
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
            var boxUploadAcivtity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(boxUploadAcivtity);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateNewActivity_GivenIsNew_ShouldHaveDisplayName()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(boxUploadAcivtity);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual("Upload to Dropbox", boxUploadAcivtity.DisplayName);

        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetFindMissingType_GivenIsNew_ShouldSetDatagridAcitivity()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var enFindMissingType = boxUploadAcivtity.GetFindMissingType();
            //---------------Test Result -----------------------
            Assert.AreEqual(enFindMissingType.StaticActivity, enFindMissingType);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetWriteMode_GivenOverwritemodeIsTrue_ShouldReturnOverwriteModeInstance()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var writeMode = boxUploadAcivtity.GetWriteMode();
            //---------------Test Result -----------------------
            Assert.AreEqual(WriteMode.Overwrite.Instance, writeMode);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetWriteMode_GivenAddmodeIsTrue_ShouldReturnAddModeInstance()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            boxUploadAcivtity.AddMode = true;
            var writeMode = boxUploadAcivtity.GetWriteMode();
            //---------------Test Result -----------------------
            Assert.IsTrue(boxUploadAcivtity.AddMode);
            Assert.AreEqual(WriteMode.Add.Instance, writeMode);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugInputs_GivenEnvironmentIsNull_ShouldHaveNoDebugOutputs()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            Assert.AreEqual(WriteMode.Overwrite.Instance, boxUploadAcivtity.GetWriteMode());
            //---------------Execute Test ----------------------
            var debugInputs = boxUploadAcivtity.GetDebugInputs(null, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugInputs.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugInputs_GivenEnvironmentMockEnvironmentAndFromPath_ShouldHaveOneDebugOutputs()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = CreateDropboxActivity();
            boxUploadAcivtity.FromPath = "Random.txt";
            //---------------Assert Precondition----------------
            Assert.AreEqual(WriteMode.Overwrite.Instance, boxUploadAcivtity.GetWriteMode());

            //---------------Execute Test ----------------------
            var debugInputs = boxUploadAcivtity.GetDebugInputs(CreateExecutionEnvironment(), 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(2, debugInputs.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugInputs_GivenEnvironmentMockEnvironmentAndToPathNotExecuted_ShouldHaveOneDebugOutputs()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = CreateDropboxActivity();
            boxUploadAcivtity.ToPath = "Random.txt";
            //---------------Assert Precondition----------------
            Assert.AreEqual(WriteMode.Overwrite.Instance, boxUploadAcivtity.GetWriteMode());

            //---------------Execute Test ----------------------
            var debugInputs = boxUploadAcivtity.GetDebugInputs(CreateExecutionEnvironment(), 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(2, debugInputs.Count);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugInputs_GivenEnvironmentMockEnvironmentAndToPathAndFromPath_ShouldHaveTwoDebugOutputs()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = CreateDropboxActivity();
            var environment = CreateExecutionEnvironment();
            boxUploadAcivtity.ToPath = "Random.txt";
            boxUploadAcivtity.FromPath = "Random.txt";
            //---------------Assert Precondition----------------
            Assert.AreEqual(WriteMode.Overwrite.Instance, boxUploadAcivtity.GetWriteMode());

            //---------------Execute Test ----------------------
            var debugInputs = boxUploadAcivtity.GetDebugInputs(environment, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(2, debugInputs.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugOutputs_GivenNullEnvironment_ShouldHaveNoDebugOutPuts()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugOutputs = boxUploadAcivtity.GetDebugOutputs(null, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugOutputs.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugOutputs_GivenFileMetadataIsNull_ShouldHaveNoDebugOutPuts()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugOutputs = boxUploadAcivtity.GetDebugOutputs(CreateExecutionEnvironment(), 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugOutputs.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugOutputs_GivenFileMetadataIsNotNull_ShouldHaveOneDebugOutPuts()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = new Mock<DsfDropBoxUploadActivity>();
            boxUploadAcivtity.SetupAllProperties();
            boxUploadAcivtity.Setup(acivtity => acivtity.GetDebugOutputs(It.IsAny<IExecutionEnvironment>(), It.IsAny<int>()))
                .Returns(new List<DebugItem>()
                {
                    new DebugItem()
                });
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugOutputs = boxUploadAcivtity.Object.GetDebugOutputs(CreateExecutionEnvironment(), 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, debugOutputs.Count);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugOutputs_GivenWebRequestSuccess_ShouldCorrectDebugValue()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var clientWrapper = new Mock<IDropboxClientWrapper>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxUploadAcivtityMock = new DsfDropBoxUploadActivityMock(mockExecutor.Object, clientWrapper.Object) { IsUplodValidSuccess = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            var esbChannel = new Mock<IEsbChannel>().Object;
            var datObj = new Mock<IDSFDataObject>().Object;
            var executionEnvironment = new Mock<IExecutionEnvironment>().Object;
            
            var errorResultTO = new ErrorResultTO();
            dsfDropBoxUploadAcivtityMock.Execute(esbChannel, datObj, String.Empty, String.Empty, out errorResultTO, 0);
            var debugOutputs = dsfDropBoxUploadAcivtityMock.GetDebugOutputs(executionEnvironment, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugOutputs.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteTool_GivenNoFromPath_ShouldAddError()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var clientWrapper = new Mock<IDropboxClientWrapper>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxUploadAcivtityMock = new DsfDropBoxUploadActivityMock(mockExecutor.Object, clientWrapper.Object) { IsUplodValidSuccess = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            var datObj = new Mock<IDSFDataObject>();
            var executionEnvironment = new Mock<IExecutionEnvironment>();
            datObj.Setup(o => o.Environment).Returns(executionEnvironment.Object);
            
            IDSFDataObject dataObject = datObj.Object;
            dsfDropBoxUploadAcivtityMock.Execute(dataObject, 0);
            //---------------Test Result -----------------------
            executionEnvironment.Verify(environment => environment.AddError("Please confirm that the correct file location has been entered"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteTool_GivenNoToPath_ShouldAddError()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var clientWrapper = new Mock<IDropboxClientWrapper>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxUploadAcivtityMock = new DsfDropBoxUploadActivityMock(mockExecutor.Object, clientWrapper.Object) { IsUplodValidSuccess = true };
            dsfDropBoxUploadAcivtityMock.FromPath = "File.txt";
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            var datObj = new Mock<IDSFDataObject>();
            var executionEnvironment = new Mock<IExecutionEnvironment>();
            datObj.Setup(o => o.Environment).Returns(executionEnvironment.Object);
            
            IDSFDataObject dataObject = datObj.Object;
            dsfDropBoxUploadAcivtityMock.Execute(dataObject, 0);
            //---------------Test Result -----------------------
            executionEnvironment.Verify(environment => environment.AddError("Please confirm that the correct file destination has been entered"));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void PerformExecution_GivenNoPaths_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var clientWrapper = new Mock<IDropboxClientWrapper>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxUploadAcivtityMock = new DsfDropBoxUploadActivityMock(mockExecutor.Object, clientWrapper.Object) { IsUplodValidSuccess = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            dsfDropBoxUploadAcivtityMock.PerfomBaseExecution(new Dictionary<string, string>());
            //---------------Test Result -----------------------
            Assert.Fail("Exception Not Throw");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PerformExecution_GivenPaths_ShouldPassthrough()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var clientWrapper = new Mock<IDropboxClientWrapper>();
            clientWrapper.Setup(wrapper => wrapper.UploadAsync(It.IsAny<string>(), It.IsAny<WriteMode>(), It.IsAny<bool>(), null, It.IsAny<bool>(), It.IsAny<MemoryStream>()))
                .Returns(Task.FromResult(TestConstant.FileMetadataInstance.Value));
            mockExecutor.Setup(executor => executor.ExecuteTask(clientWrapper.Object))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxUploadAcivtityMock = new DsfDropBoxUploadActivityMock(mockExecutor.Object, clientWrapper.Object)
            {
                IsUplodValidSuccess = true
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            var location = Assembly.GetExecutingAssembly().Location;
            dsfDropBoxUploadAcivtityMock.PerfomBaseExecution(new Dictionary<string, string>()
            {
                {"ToPath","a" },
                {"FromPath",location },
            });
            //---------------Test Result -----------------------
            clientWrapper.Verify(wrapper => wrapper.UploadAsync(It.IsAny<string>(), It.IsAny<WriteMode>(), It.IsAny<bool>(), null, It.IsAny<bool>(), It.IsAny<MemoryStream>()));


        }

       

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugInputs_GivenEnvironment_ShouldhaveDebugInputs()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            var clientWrapper = new Mock<IDropboxClientWrapper>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxUploadAcivtityMock = new DsfDropBoxUploadActivityMock(mockExecutor.Object, clientWrapper.Object)
            {
                IsUplodValidSuccess = true,
                ToPath = "DDD",
                FromPath = "DDD",
                AddMode = true
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            var mock = new Mock<IExecutionEnvironment>();
            var debugInputs = dsfDropBoxUploadAcivtityMock.GetDebugInputs(mock.Object, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(2, debugInputs.Count);
        }


    }

    public class DsfDropBoxUploadActivityMock : DsfDropBoxUploadActivity
    {
        private readonly IDropboxClientWrapper _dropboxClientWrapper;

        public DsfDropBoxUploadActivityMock(IDropboxSingleExecutor<IDropboxResult> singleExecutor, IDropboxClientWrapper dropboxClientWrapper)
            : base(dropboxClientWrapper)
        {
            _dropboxClientWrapper = dropboxClientWrapper;
            DropboxSingleExecutor = singleExecutor;
        }

        public void Execute(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            tmpErrors = new ErrorResultTO();
        }

        #region Overrides of DsfDropBoxUploadActivity

        #endregion

        public FileMetadata FileResult => FileMetadata;

        #region Overrides of DsfDropBoxUploadActivity

        
        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            base.ExecuteTool(dataObject, update);
        }

        #endregion

        public string PerfomBaseExecution(Dictionary<string, string> dictionaryValues)
        {
            var perfomBaseExecution = base.PerformExecution(dictionaryValues);
            return perfomBaseExecution[0];
        }

        #region Overrides of DsfNativeActivity<string>



        #endregion

        protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            try
            {
                var dropboxResult = DropboxSingleExecutor.ExecuteTask(_dropboxClientWrapper);
                if (IsUplodValidSuccess)
                {
                    FileMetadata = ((DropboxUploadSuccessResult)dropboxResult).GerFileMetadata();
                }
                else
                {
                    Exception = ((DropboxFailureResult)dropboxResult).GetException();
                }

                return new List<string> { string.Empty };
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e.Message, e, "Warewolf Error");
                Exception = new DropboxFailureResult(new Exception()).GetException();
                return new List<string> { string.Empty };
            }
        }

        public bool IsUplodValidSuccess { get; set; }


    }


}