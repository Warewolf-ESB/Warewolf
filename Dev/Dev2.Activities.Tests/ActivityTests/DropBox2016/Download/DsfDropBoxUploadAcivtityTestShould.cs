using System;
using System.Collections.Generic;
using Dev2.Activities.DropBox2016;
using Dev2.Activities.DropBox2016.DownloadActivity;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Activities.DropBox2016.UploadActivity;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dropbox.Api;
using Dropbox.Api.Babel;
using Dropbox.Api.Files;
using Moq;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016.Download
{
    public class DsfDropBoxDownloadActivityMock : DsfDropBoxDownloadActivity
    {

        public DsfDropBoxDownloadActivityMock(IDropboxSingleExecutor<IDropboxResult> singleExecutor)
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

        public IDownloadResponse<FileMetadata> FileResult
        {
            get
            {
                return base.Response;
            }
        }

        #region Overrides of DsfDropBoxUploadActivity

        // ReSharper disable once RedundantOverridenMember
        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            base.ExecuteTool(dataObject, update);
        }

        #endregion

        public void SetBaseMetadata(IDownloadResponse<FileMetadata> metadata)
        {
            Response = metadata;
        }
        public string PerfomMockExecution()
        {

            var mock = new Mock<IDropBoxUpload>();
            mock.Setup(upload => upload.ExecuteTask(TestConstant.DropboxClientInstance.Value))
            .Returns(new DropboxUploadSuccessResult(TestConstant.FileMetadataInstance.Value));

            DropboxSingleExecutor = mock.Object;
            var dropboxExecutionResult = mock.Object.ExecuteTask(TestConstant.DropboxClientInstance.Value);
            var dropboxSuccessResult = dropboxExecutionResult as DropboxDownloadSuccessResult;
            if (dropboxSuccessResult != null)
            {
                Response = dropboxSuccessResult.GetDownloadResponse();
                return Response.Response.PathDisplay;
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
                    Response = ((DropboxDownloadSuccessResult)dropboxResult).GetDownloadResponse();
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
}