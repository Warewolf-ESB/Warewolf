using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Common;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.ServiceModel;
using Dev2.DataList.Contract;
using Dropbox.Api;
using Dropbox.Api.Files;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;

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

        }

        public OauthSource SelectedSource { get; set; }
        public string FromPath { get; set; }
        public string ToPath { get; set; }
        public bool OverWriteMode { get; set; }
        public bool UpdateMode { get; set; }
        public bool AddMode { get; set; }
        public Lazy<DropboxClient> ClientLazy = new Lazy<DropboxClient>(() =>
        {
            var dropboxClientConfig = new DropboxClientConfig(GlobalConstants.UserAgentString);
            DropboxClient dropboxClient = new DropboxClient(GlobalConstants.DropBoxApiKey, dropboxClientConfig);
            return dropboxClient;
        });



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
                var stream = GetFileStream();
                _dropboxSingleExecutor = new DropBoxUpload(false, null, true, writeMode, ToPath, stream);
                _dropboxSingleExecutor.ExecuteTask(ClientLazy.Value);
            }
            catch (Exception e)
            {
                dataObject.Environment.AddError(e.Message);
                Dev2Logger.Error(e.Message, e);
            }

        }
        [ExcludeFromCodeCoverage]
        private Stream GetFileStream()
        {
            using (var stream = new MemoryStream(File.ReadAllBytes(FromPath)))
            {
                return stream;
            }
        }

        private WriteMode GetWriteMode()
        {
            if (OverWriteMode)
                return new WriteMode().AsOverwrite;
            if (AddMode)
                return new WriteMode().AsAdd;
            return new WriteMode().AsUpdate;
        }
    }
        #endregion
}