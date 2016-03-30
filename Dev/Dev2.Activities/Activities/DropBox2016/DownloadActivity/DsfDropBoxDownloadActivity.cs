using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.ServiceModel;
using Dev2.Util;
using Dropbox.Api;
using Dropbox.Api.Babel;
using Dropbox.Api.Files;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;

// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Activities.DropBox2016.DownloadActivity
{
    [ToolDescriptorInfo("DropBoxLogo", "DOWNLOAD", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090D8C8EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Storage", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfDropBoxDownloadActivity : DsfBaseActivity
    {
        private DropboxClient _client;
        protected IDownloadResponse<FileMetadata> Response;
        protected Exception Exception;
        protected IDropboxSingleExecutor<IDropboxResult> DropboxSingleExecutor;

        public DsfDropBoxDownloadActivity()
        {
            // ReSharper disable once VirtualMemberCallInContructor
            DisplayName = "DOWNLOAD from Dropbox";
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public OauthSource SelectedSource { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        [Inputs("Path in the user's Dropbox")]
        [FindMissing]
        public string ToPath { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        [Inputs("Local File Path")]
        [FindMissing]
        public string FromPath { get; set; }

        // ReSharper disable once MemberCanBeProtected.Global

        [ExcludeFromCodeCoverage]
        protected virtual DropboxClient GetClient()
        {
            if (_client != null)
            {

                return _client;
            }
            var httpClient = new HttpClient(new WebRequestHandler { ReadWriteTimeout = 10 * 1000 })
            {
                Timeout = TimeSpan.FromMinutes(20)
            };
            _client = new DropboxClient(SelectedSource.Secret, new DropboxClientConfig(GlobalConstants.UserAgentString) { HttpClient = httpClient });
            return _client;
        }

        #region Overrides of DsfActivity

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.StaticActivity;
        }

        #region Overrides of DsfBaseActivity

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            if (string.IsNullOrEmpty(FromPath))
            {
                dataObject.Environment.AddError("Please confirm that the correct file location has been entered");
                return;
            }
            if (string.IsNullOrEmpty(ToPath))
            {
                dataObject.Environment.AddError("Please confirm that the correct file destination has been entered");
                return;
            }
            base.ExecuteTool(dataObject, update);
        }

        #endregion

        [ExcludeFromCodeCoverage]
        //All units used here has been unit tested seperately 
        protected override string PerformExecution(Dictionary<string, string> evaluatedValues)
        {

            DropboxSingleExecutor = new DropBoxDownLoad(evaluatedValues["ToPath"]);
            var dropboxExecutionResult = DropboxSingleExecutor.ExecuteTask(GetClient());
            var dropboxSuccessResult = dropboxExecutionResult as DropboxDownloadSuccessResult;
            if (dropboxSuccessResult != null)
            {
                Response = dropboxSuccessResult.GetDownloadResponse();
                var bytes = Response.GetContentAsByteArrayAsync().Result;
                if (Response.Response.IsFile)
                {
                    File.WriteAllBytes(FromPath, bytes);
                }
                return Response.Response.Name;
            }
            var dropboxFailureResult = dropboxExecutionResult as DropboxFailureResult;
            if (dropboxFailureResult != null)
            {
                Exception = dropboxFailureResult.GetException();
            }
            var executionError = Exception.InnerException == null ? Exception.Message : Exception.InnerException.Message;
            if (executionError.Contains("not_file"))
            {
                executionError = "Please specify the path of file in Dropbox";
            }
            throw new Exception(executionError);
        }

        #region Overrides of DsfActivity

        public override string DisplayName { get; set; }

        #endregion
    }

        #endregion
}