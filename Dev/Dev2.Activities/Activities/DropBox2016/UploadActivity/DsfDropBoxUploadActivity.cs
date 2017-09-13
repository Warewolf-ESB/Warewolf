using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.ServiceModel;
using Dev2.Util;
using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;



namespace Dev2.Activities.DropBox2016.UploadActivity
{
    [ToolDescriptorInfo("Dropbox", "Upload", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C8C9EA2E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Storage: Dropbox", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Dropbox_Upload")]
    public class DsfDropBoxUploadActivity : DsfBaseActivity,IEquatable<DsfDropBoxUploadActivity>
    {
        private IDropboxClientWrapper _clientWrapper;
        private DropboxClient _client;
        private bool _addMode;
        private bool _overWriteMode;
        protected FileMetadata FileMetadata;
        protected Exception Exception;
        protected IDropboxSingleExecutor<IDropboxResult> DropboxSingleExecutor;

        public DsfDropBoxUploadActivity()
        {
            
            DisplayName = "Upload to Dropbox";
            OverWriteMode = true;
        }

        public DsfDropBoxUploadActivity(IDropboxClientWrapper clientWrapper)
            :this()
        {
            _clientWrapper = clientWrapper;
        }

        
        public OauthSource SelectedSource { get; set; }

        
        [Inputs("Local File Path")]
        [FindMissing]
        public string FromPath { get; set; }

        
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
                _addMode = !value;
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
                _overWriteMode = !value;
                _addMode = value;
            }
        }

        

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
                dataObject.Environment.AddError(ErrorResource.DropBoxConfirmCorrectFileLocation);
            }
            if (string.IsNullOrEmpty(ToPath))
            {
                dataObject.Environment.AddError(ErrorResource.DropBoxConfirmCorrectFileDestination);
            }
            base.ExecuteTool(dataObject, update);
        }

        #endregion Overrides of DsfBaseActivity
        
        //All units used here has been unit tested seperately
        protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            var writeMode = GetWriteMode();
            DropboxSingleExecutor = new DropBoxUpload(writeMode, evaluatedValues["ToPath"], evaluatedValues["FromPath"]);
            _clientWrapper = _clientWrapper ?? new DropboxClientWrapper(GetClient());
            var dropboxExecutionResult = DropboxSingleExecutor.ExecuteTask(_clientWrapper);
            var dropboxSuccessResult = dropboxExecutionResult as DropboxUploadSuccessResult;
            if (dropboxSuccessResult != null)
            {
                FileMetadata = dropboxSuccessResult.GerFileMetadata();
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


        public WriteMode GetWriteMode()
        {
            if (OverWriteMode)
                return WriteMode.Overwrite.Instance;
            return WriteMode.Add.Instance;
        }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            if (env == null)
            {
                return new List<DebugItem>();
            }
            base.GetDebugInputs(env, update);

            DebugItem debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "OverWrite"), debugItem);
            string value = OverWriteMode ? "True" : "False";
            AddDebugItem(new DebugEvalResult(value, "", env, update), debugItem);
            _debugInputs.Add(debugItem);

            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Add"), debugItem);
            value = AddMode ? "True" : "False";
            AddDebugItem(new DebugEvalResult(value, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
           
            return _debugInputs;

        }

        public bool Equals(DsfDropBoxUploadActivity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            var isSourceEqual = CommonEqualityOps.AreObjectsEqual<IResource>(SelectedSource,other.SelectedSource);
            return base.Equals(other) 
                && isSourceEqual
                && string.Equals(FromPath, other.FromPath) 
                && string.Equals(DisplayName, other.DisplayName) 
                && Equals(OverWriteMode,other.OverWriteMode)
                && Equals(AddMode,other.AddMode)
                && string.Equals(ToPath, other.ToPath);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DsfDropBoxUploadActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (SelectedSource != null ? SelectedSource.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FromPath != null ? FromPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ToPath != null ? ToPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ OverWriteMode.GetHashCode();
                hashCode = (hashCode * 397) ^ AddMode.GetHashCode();
                return hashCode;
            }
        }
    }

        #endregion Overrides of DsfActivity
}