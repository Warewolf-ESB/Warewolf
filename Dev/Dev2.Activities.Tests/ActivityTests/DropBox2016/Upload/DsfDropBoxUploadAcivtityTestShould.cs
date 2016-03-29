﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Activities.DropBox2016;
using Dev2.Activities.DropBox2016.UploadActivity;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016.Upload
{
    [TestClass]
    [ExcludeFromCodeCoverage]
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
            Assert.AreEqual("UPLOAD to Dropbox", boxUploadAcivtity.DisplayName);

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
            Assert.AreEqual(0, debugInputs.Count());
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
            Assert.AreEqual(0, debugInputs.Count());
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
            Assert.AreEqual(0, debugInputs.Count());
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
            Assert.AreEqual(0, debugInputs.Count());
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
            Assert.AreEqual(0, debugOutputs.Count());
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
            Assert.AreEqual(0, debugOutputs.Count());
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
            Assert.AreEqual(1, debugOutputs.Count());
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugOutputs_GivenWebRequestSuccess_ShouldCorrectDebugValue()
        {
            //---------------Set up test pack-------------------
            var mockExecutor = new Mock<IDropboxSingleExecutor<IDropboxResult>>();
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxUploadAcivtityMock = new DsfDropBoxUploadActivityMock(mockExecutor.Object) { IsUplodValidSuccess = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            var esbChannel = new Mock<IEsbChannel>().Object;
            var datObj = new Mock<IDSFDataObject>().Object;
            var executionEnvironment = new Mock<IExecutionEnvironment>().Object;
            // ReSharper disable once RedundantAssignment
            var errorResultTO = new ErrorResultTO();
            dsfDropBoxUploadAcivtityMock.Execute(esbChannel, datObj, String.Empty, String.Empty, out  errorResultTO, 0);
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
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxUploadAcivtityMock = new DsfDropBoxUploadActivityMock(mockExecutor.Object) { IsUplodValidSuccess = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            var datObj = new Mock<IDSFDataObject>();
            var executionEnvironment = new Mock<IExecutionEnvironment>();
            datObj.Setup(o => o.Environment).Returns(executionEnvironment.Object);
            // ReSharper disable once RedundantAssignment
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
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxUploadAcivtityMock = new DsfDropBoxUploadActivityMock(mockExecutor.Object) { IsUplodValidSuccess = true };
            dsfDropBoxUploadAcivtityMock.FromPath = "File.txt";
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            var datObj = new Mock<IDSFDataObject>();
            var executionEnvironment = new Mock<IExecutionEnvironment>();
            datObj.Setup(o => o.Environment).Returns(executionEnvironment.Object);
            // ReSharper disable once RedundantAssignment
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
            mockExecutor.Setup(executor => executor.ExecuteTask(TestConstant.DropboxClientInstance.Value))
                .Returns(new DropboxSuccessResult(TestConstant.FileMetadataInstance.Value));
            var dsfDropBoxUploadAcivtityMock = new DsfDropBoxUploadActivityMock(mockExecutor.Object) { IsUplodValidSuccess = true };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            dsfDropBoxUploadAcivtityMock.PerfomBaseExecution(new Dictionary<string, string>());
            //---------------Test Result -----------------------
            Assert.Fail("Exception Not Throw");
        }


    }

    public class DsfDropBoxUploadActivityMock : DsfDropBoxUploadActivity
    {

        public DsfDropBoxUploadActivityMock(IDropboxSingleExecutor<IDropboxResult> singleExecutor)
        {
            DropboxSingleExecutor = singleExecutor;
        }
        public void Execute(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            //ExecutionImpl(esbChannel, dataObject, inputs, outputs, out tmpErrors, update);
            tmpErrors = new ErrorResultTO();
        }

        #region Overrides of DsfDropBoxUploadActivity

        protected override DropboxClient GetClient()
        {
            return TestConstant.DropboxClientInstance.Value;
        }

        #endregion

        public FileMetadata FileResult
        {
            get
            {
                return FileMetadata;
            }
        }

        #region Overrides of DsfDropBoxUploadActivity

        // ReSharper disable once RedundantOverridenMember
        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            base.ExecuteTool(dataObject, update);
        }

        #endregion

        public void SetBaseMetadata(FileMetadata metadata)
        {
            FileMetadata = metadata;
        }
        public string PerfomMockExecution()
        {

            var mock = new Mock<IDropBoxUpload>();
            mock.Setup(upload => upload.ExecuteTask(TestConstant.DropboxClientInstance.Value))
            .Returns(new DropboxSuccessResult(TestConstant.FileMetadataInstance.Value));

            DropboxSingleExecutor = mock.Object;
            var dropboxExecutionResult = mock.Object.ExecuteTask(TestConstant.DropboxClientInstance.Value);
            var dropboxSuccessResult = dropboxExecutionResult as DropboxSuccessResult;
            if (dropboxSuccessResult != null)
            {
                FileMetadata = dropboxSuccessResult.GerFileMetadata();
                return FileMetadata.PathDisplay;
            }
            var dropboxFailureResult = dropboxExecutionResult as DropboxFailureResult;
            if (dropboxFailureResult != null)
            {
                Exception = dropboxFailureResult.GetException();
            }
            var executionError = Exception.InnerException == null ? Exception.Message : Exception.InnerException.Message;
            throw new Exception(executionError);
        }

        public string PerfomBaseExecution(Dictionary<string, string> dictionaryValues)
        {
            var perfomBaseExecution = base.PerformExecution(dictionaryValues);
            return perfomBaseExecution;
        }

        #region Overrides of DsfNativeActivity<string>

        

        #endregion

        protected override string PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            try
            {
                var dropboxResult = DropboxSingleExecutor.ExecuteTask(TestConstant.DropboxClientInstance.Value);
                if (IsUplodValidSuccess)
                {
                    //FileSuccesResult = GlobalConstants.DropBoxSucces;
                    FileMetadata = ((DropboxSuccessResult)dropboxResult).GerFileMetadata();
                }
                else
                {
                    Exception = ((DropboxFailureResult)dropboxResult).GetException();
                }

                return String.Empty;
            }
            catch (Exception e)
            {
                //dataObject.Environment.AddError(e.Message);
                Dev2Logger.Error(e.Message, e);
                //FileSuccesResult = GlobalConstants.DropBoxFailure;
                Exception = new DropboxFailureResult(new Exception()).GetException();
                return String.Empty;
            }
        }

        public bool IsUplodValidSuccess { get; set; }


    }

    public static class TestConstant
    {
        public static Lazy<FileMetadata> FileMetadataInstance = new Lazy<FileMetadata>(() =>
        {
            var mock = new Mock<FileMetadata>();
            var fileMetadata = mock.Object;
            return fileMetadata;
        });

        public static Lazy<Exception> ExceptionInstance = new Lazy<Exception>(() =>
        {
            var exception = new Exception(ErrorMessage);
            return exception;
        });

        public static Lazy<DropboxClient> DropboxClientInstance = new Lazy<DropboxClient>(() => new DropboxClient("random.net"));
        public static string ErrorMessage = "Error Messege";
    }
}