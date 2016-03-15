using System;
using System.Activities;
using System.Collections.Generic;
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

namespace Dev2.Activities.DropBox2016.UploadActivity
{
    [ToolDescriptorInfo("DropBoxLogo", "Upload to Drop Box", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C8C9EA2E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Connectors", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfDropBoxUploadAcivtity : DsfActivity
    {
        private IDropboxSingleExecutor<FileMetadata> _dropboxSingleExecutor;
        public DsfDropBoxUploadAcivtity()
        {
            // ReSharper disable once VirtualMemberCallInContructor
            DisplayName = "Upload to Drop Box";
            Type = "Upload to Drop Box";
            OverWriteMode = true;
        }

        public OauthSource SelectedSource { get; set; }
        public string FromPath { get; set; }
        public string ToPath { get; set; }
        public bool OverWriteMode { get; set; }
        public bool UpdateMode { get; set; }
        public bool AddMode { get; set; }

        private DropboxClient GetClient()
        {
            return _client ?? (_client = new DropboxClient(SelectedSource.Secret, new DropboxClientConfig(GlobalConstants.UserAgentString)));
        }

        #region Overrides of DsfActivity

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            tmpErrors = new ErrorResultTO();
            try
            {
                var writeMode = GetWriteMode();
                _dropboxSingleExecutor = new DropBoxUpload(false, null, true, writeMode, ToPath,FromPath);
                var fileMetadata = _dropboxSingleExecutor.ExecuteTask(GetClient());
                _uploadedFilePath = fileMetadata.PathLower;
                
            }
            catch (Exception e)
            {
                dataObject.Environment.AddError(e.Message);
                Dev2Logger.Error(e.Message, e);
            }

        }
        private string _uploadedFilePath;
        private DropboxClient _client;

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            if (env == null) return _debugInputs;
            base.GetDebugInputs(env, update);
            DebugItem debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Uploaded File Path"), debugItem);
            AddDebugItem(new DebugEvalResult(_uploadedFilePath, "", env, update), debugItem);
            _debugInputs.Add(debugItem);


            return _debugInputs;
        }

        private WriteMode GetWriteMode()
        {
            if (OverWriteMode)
                return WriteMode.Overwrite.Instance;
            if (AddMode)
                return WriteMode.Add.Instance;
            return new WriteMode().AsUpdate;
        }
    }
        #endregion
}