using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Dev2.Common;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.ServiceModel;
using Dev2.Util;
using Dropbox.Api;
using Dropbox.Api.Files;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;

// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Activities.DropBox2016.UploadActivity
{
    [ToolDescriptorInfo("DropBoxLogo", "Upload to Drop Box", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C8C9EA2E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Connectors", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfDropBoxUploadAcivtity : DsfBaseActivity
    {
        private DropboxClient _client;
        private bool _addMode;
        private bool _overWriteMode;
        protected FileMetadata FileMetadata;
        protected Exception Exception;
        protected IDropboxSingleExecutor<IDropboxResult> DropboxSingleExecutor;

        public DsfDropBoxUploadAcivtity()
        {
            // ReSharper disable once VirtualMemberCallInContructor
            DisplayName = "Upload to Drop Box";
            OverWriteMode = true;
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public OauthSource SelectedSource { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        [Inputs("Local File Path")]
        [FindMissing]
        public string FromPath { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        [Inputs("Path in the user's Dropbox")]
        [FindMissing]
        public string ToPath { get; set; }
        public bool OverWriteMode
        {
            get
            {
                return _overWriteMode;
            }
            set
            {
                _addMode = false;
                _overWriteMode = value;
            }
        }
        public bool AddMode
        {
            get
            {
                return _addMode;
            }
            set
            {
                _overWriteMode = false;
                _addMode = value;
            }
        }
        // ReSharper disable once MemberCanBeProtected.Global
        [Outputs("Upload Status")]
        public string FileSuccesResult { get; set; }

        [ExcludeFromCodeCoverage]
        private DropboxClient GetClient()
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
        protected override string PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            try
            {
                var writeMode = GetWriteMode();
                DropboxSingleExecutor = new DropBoxUpload(writeMode, ToPath, FromPath);
                var dropboxExecutionResult = DropboxSingleExecutor.ExecuteTask(GetClient());
                var dropboxSuccessResult = dropboxExecutionResult as DropboxSuccessResult;
                if (dropboxSuccessResult != null)
                {
                    FileMetadata = dropboxSuccessResult.GerFileMetadata();
                    return FileMetadata.PathDisplay;
                }
                if((dropboxExecutionResult as DropboxFailureResult)!=null)
                {
                    Exception = (dropboxExecutionResult as DropboxFailureResult).GetException();
                    return Exception.InnerException == null ? Exception.Message : Exception.InnerException.Message;
                }
                throw new Exception("An unkown exception occurred");
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e.Message, e);
                return e.Message;
            }
        }

        #region Overrides of DsfActivity

        public override string DisplayName { get; set; }

        #endregion

        public WriteMode GetWriteMode()
        {
            if (OverWriteMode)
                return WriteMode.Overwrite.Instance;
            return WriteMode.Add.Instance;
        }
    }
        #endregion
}