using System;
using System.Collections.Generic;
using Dev2.Activities.DropBox2016;
using Dev2.Activities.DropBox2016.DownloadActivity;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;
using Moq;



namespace Dev2.Tests.Activities.ActivityTests.DropBox2016.Download
{
    public class DsfDropBoxDownloadActivityMock : DsfDropBoxDownloadActivity
    {

        
        public void Execute(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            
            tmpErrors = new ErrorResultTO();
        }

        #region Overrides of DsfDropBoxUploadActivity
        

        #endregion

        public IDownloadResponse<FileMetadata> FileResult => base.Response;

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
                var dropboxResult = GetDropboxSingleExecutor(It.IsAny<IDropboxSingleExecutor<IDropboxResult>>()).ExecuteTask(TestConstant.DropboxClientInstance.Value);
                if (IsUplodValidSuccess)
                {
                    
                    Response = ((DropboxDownloadSuccessResult)dropboxResult).GetDownloadResponse();
                }
                else
                {
                    Exception = ((DropboxFailureResult)dropboxResult).GetException();
                }

                return new List<string> { string.Empty };
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e.Message, e, GlobalConstants.WarewolfError);
                Exception = new DropboxFailureResult(new Exception()).GetException();
                return new List<string> { string.Empty };
            }
        }

        public bool IsUplodValidSuccess { get; set; }


    }
}