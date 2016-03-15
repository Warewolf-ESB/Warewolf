using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Activities.DropBox2016;
using Dev2.Activities.DropBox2016.UploadActivity;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dropbox.Api.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016.Upload
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DsfDropBoxUploadAcivtityTest
    {
        private static DsfDropBoxUploadAcivtity CreateDropboxActivity()
        {
            return new DsfDropBoxUploadAcivtity();
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
            Assert.AreEqual("Upload to Drop Box", boxUploadAcivtity.DisplayName);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateNewActivity_GivenIsNew_ShouldHaveType()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(boxUploadAcivtity);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual("Upload to Drop Box", boxUploadAcivtity.Type.Expression.ToString());

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetFindMissingType_GivenIsNew_ShouldSetDatagridAcitivity()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = CreateDropboxActivity();
            //---------------Assert Precondition----------------
            Assert.AreEqual("Upload to Drop Box", boxUploadAcivtity.Type.Expression.ToString());
            //---------------Execute Test ----------------------
            var enFindMissingType = boxUploadAcivtity.GetFindMissingType();
            //---------------Test Result -----------------------
            Assert.AreEqual(enFindMissingType.DataGridActivity, enFindMissingType);

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
            Assert.AreEqual(0,debugInputs.Count());
        }
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugInputs_GivenEnvironmentMockEnvironment_ShouldHaveOneDebugOutputs()
        {
            //---------------Set up test pack-------------------
            var boxUploadAcivtity = CreateDropboxActivity();
            var environment = new ExecutionEnvironment();
           
            //---------------Assert Precondition----------------
            Assert.AreEqual(WriteMode.Overwrite.Instance, boxUploadAcivtity.GetWriteMode());
            //---------------Execute Test ----------------------
            var debugInputs = boxUploadAcivtity.GetDebugInputs(environment, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(1,debugInputs.Count());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteTool_GivenWebRequestValid_ShouldSetFileSuccessResultToTrue()
        {
            //---------------Set up test pack-------------------
            var dsfDropBoxUploadAcivtityMock = new DsfDropBoxUploadAcivtityMock(It.IsAny<IDropboxSingleExecutor<FileMetadata>>());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            var esbChannel = new Mock<IEsbChannel>().Object;
            var datObj = new Mock<IDSFDataObject>().Object;
            // ReSharper disable once RedundantAssignment
            var errorResultTO = new ErrorResultTO();
            dsfDropBoxUploadAcivtityMock.IsUplodValidSuccess = true;
            dsfDropBoxUploadAcivtityMock.Execute(esbChannel, datObj, String.Empty, String.Empty, out  errorResultTO, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(GlobalConstants.DropBoxSucces ,dsfDropBoxUploadAcivtityMock.FileSuccesResult);
        }
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteTool_GivenWebRequestIsNotValid_ShouldSetFileSuccessResultToFalse()
        {
            //---------------Set up test pack-------------------
            var dsfDropBoxUploadAcivtityMock = new DsfDropBoxUploadAcivtityMock(It.IsAny<IDropboxSingleExecutor<FileMetadata>>());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfDropBoxUploadAcivtityMock);
            //---------------Execute Test ----------------------
            var esbChannel = new Mock<IEsbChannel>().Object;
            var datObj = new Mock<IDSFDataObject>().Object;
            // ReSharper disable once RedundantAssignment
            var errorResultTO = new ErrorResultTO();
            dsfDropBoxUploadAcivtityMock.IsUplodValidSuccess = false;
            dsfDropBoxUploadAcivtityMock.Execute(esbChannel, datObj, String.Empty, String.Empty, out  errorResultTO, 0);
            //---------------Test Result -----------------------
            Assert.AreNotEqual(GlobalConstants.DropBoxSucces,dsfDropBoxUploadAcivtityMock.FileSuccesResult);
        }
    }

    public class DsfDropBoxUploadAcivtityMock : DsfDropBoxUploadAcivtity
    {
        #region Overrides of DsfDropBoxUploadAcivtity

        public DsfDropBoxUploadAcivtityMock(IDropboxSingleExecutor<FileMetadata> singleExecutor)
        {
            _dropboxSingleExecutor = singleExecutor;
        }
        public void Execute(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            ExecutionImpl(esbChannel, dataObject, inputs, outputs, out tmpErrors, update);
        }
        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            tmpErrors = new ErrorResultTO();
            try
            {
               if(IsUplodValidSuccess)
                   FileSuccesResult = GlobalConstants.DropBoxSucces;

            }
            catch (Exception e)
            {
                dataObject.Environment.AddError(e.Message);
                Dev2Logger.Error(e.Message, e);
                FileSuccesResult = GlobalConstants.DropBoxFailure;
            }
        }

        public bool IsUplodValidSuccess { get; set; }

        #endregion
    }
}