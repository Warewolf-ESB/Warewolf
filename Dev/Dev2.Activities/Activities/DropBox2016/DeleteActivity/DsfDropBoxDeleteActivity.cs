using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.ServiceModel;
using Dev2.Util;
using Dropbox.Api;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dev2.Activities.DropBox2016.DeleteActivity
{
    [ToolDescriptorInfo("Dropbox", "Delete", ToolType.Native, "8AC94835-0A28-4166-A53A-D7B07730C135", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Storage: Dropbox", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Dropbox_Delete")]
    public class DsfDropBoxDeleteActivity : DsfBaseActivity
    {
        private DropboxClient _client;
        protected Exception Exception;
        protected IDropboxSingleExecutor<IDropboxResult> DropboxSingleExecutor;

        public DsfDropBoxDeleteActivity()
        {
            // ReSharper disable once VirtualMemberCallInContructor
            DisplayName = "Delete from Dropbox";
        }

        public OauthSource SelectedSource { get; set; }

        [Inputs("Path in the user's Dropbox")]
        [FindMissing]
        public string DeletePath { get; set; }

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
            _client = new DropboxClient(SelectedSource.AccessToken, new DropboxClientConfig(GlobalConstants.UserAgentString) { HttpClient = httpClient });
            return _client;
        }

        protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            DropboxSingleExecutor = new DropboxDelete(evaluatedValues["DeletePath"]);
            var dropboxExecutionResult = DropboxSingleExecutor.ExecuteTask(GetClient());
            var dropboxSuccessResult = dropboxExecutionResult as DropboxDeleteSuccessResult;
            if (dropboxSuccessResult != null)
            {
                dropboxSuccessResult.GerFileMetadata();
                return new List<string> { GlobalConstants.DropBoxSuccess };
            }
            var dropboxFailureResult = dropboxExecutionResult as DropboxFailureResult;
            if (dropboxFailureResult != null)
            {
                Exception = dropboxFailureResult.GetException();
            }
            var executionError = Exception.InnerException?.Message ?? Exception.Message;
            throw new Exception(executionError);
        }

        #region Overrides of DsfNativeActivity<string>

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.StaticActivity;
        }

        #endregion Overrides of DsfNativeActivity<string>

        #region Overrides of DsfBaseActivity

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            if (string.IsNullOrEmpty(DeletePath))
            {
                dataObject.Environment.AddError(ErrorResource.DropBoxConfirmCorrectFileLocation);
                return;
            }
            base.ExecuteTool(dataObject, update);
        }

        #endregion Overrides of DsfBaseActivity
    }
}