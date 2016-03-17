using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.ServiceModel;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dropbox.Api;
using Dropbox.Api.Files;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Storage;
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Activities.DropBox2016.UploadActivity
{
    [ToolDescriptorInfo("DropBoxLogo", "Upload to Dropbox", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C8C9EA2E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Connectors", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfDropBoxUploadAcivtity : DsfActivity
    {
        private DropboxClient _client;
        private bool _addMode;
        private bool _overWriteMode;
        protected FileMetadata _fileMetadata;
        protected Exception _exception;
        protected IDropboxSingleExecutor<IDropboxResult> DropboxSingleExecutor;

        public DsfDropBoxUploadAcivtity()
        {
            // ReSharper disable once VirtualMemberCallInContructor
            DisplayName = "Upload to Dropbox";
            Type = "Upload to Dropbox";
            OverWriteMode = true;
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public OauthSource SelectedSource { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string FromPath { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
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
        public string FileSuccesResult { get; set; }

        [ExcludeFromCodeCoverage]
        private DropboxClient GetClient()
        {
            return _client ?? (_client = new DropboxClient(SelectedSource.Secret, new DropboxClientConfig(GlobalConstants.UserAgentString)));
        }

        #region Overrides of DsfActivity

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

        [ExcludeFromCodeCoverage]
        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            tmpErrors = new ErrorResultTO();
            try
            {
                dataObject.Environment.Assign(FileSuccesResult, string.Empty, update);
                var writeMode = GetWriteMode();
                DropboxSingleExecutor = new DropBoxUpload(writeMode, ToPath, FromPath);
                var dropboxExecutionResult = DropboxSingleExecutor.ExecuteTask(GetClient());
                var dropboxSuccessResult = dropboxExecutionResult as DropboxSuccessResult;
                if (dropboxSuccessResult != null)
                {
                    _fileMetadata = dropboxSuccessResult.GerFileMetadata();
                    dataObject.Environment.Assign(FileSuccesResult, GlobalConstants.DropBoxSucces, update);
                }
                var dropboxFailureResult = dropboxExecutionResult as DropboxFailureResult;
                // ReSharper disable once InvertIf
                if (dropboxFailureResult != null)
                {
                    _exception = dropboxFailureResult.GetException();
                    dataObject.Environment.Assign(FileSuccesResult, GlobalConstants.DropBoxFailure, update);
                }
            }
            catch (Exception e)
            {
                dataObject.Environment.AddError(e.Message);
                Dev2Logger.Error(e.Message, e);
                dataObject.Environment.Assign(FileSuccesResult, GlobalConstants.DropBoxFailure, update);
            }

        }



        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            if (env == null) return _debugInputs;
            base.GetDebugInputs(env, update);

            if (FromPath != null)
            {
                AddDebugInputItem(new DebugEvalResult(FromPath, "Local File Path", env, update));
            }
            if (ToPath != null)
            {
                AddDebugInputItem(new DebugEvalResult(ToPath, "Path in the user's Dropbox", env, update));
            }

            return _debugInputs;
        }

        #region Overrides of DsfActivity

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            if (env == null) return _debugOutputs;
            base.GetDebugInputs(env, update);

            if(_fileMetadata != null)
            {
                AddDebugOutputItem(new DebugItemStaticDataParams(string.IsNullOrEmpty(_fileMetadata.Name) ? "No File" : _fileMetadata.Name, "Uploaded File"));
            }
            if (_exception != null)
            {
                AddDebugOutputItem(new DebugItemStaticDataParams(string.IsNullOrEmpty(_exception.Message) ? "exception Occured" : _exception.Message, "Uploaded File"));
            }
            return _debugOutputs;
        }

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