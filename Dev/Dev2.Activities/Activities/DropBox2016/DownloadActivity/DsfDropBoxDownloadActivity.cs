using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Data.ServiceModel;
using Dev2.Util;
using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Data;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dropbox.Api.Stone;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;



namespace Dev2.Activities.DropBox2016.DownloadActivity
{
    [ToolDescriptorInfo("Dropbox", "Download", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090D8C8EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Storage: Dropbox", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Dropbox_Download")]
    public class DsfDropBoxDownloadActivity : DsfBaseActivity, IDisposable,IEquatable<DsfDropBoxDownloadActivity>
    {
        public DsfDropBoxDownloadActivity()
        {
            
            DisplayName = "Download from Dropbox";
            OverwriteFile = false;
            
            DropboxFile = new FileWrapper();
        }

        protected DsfDropBoxDownloadActivity(IDropboxClientWrapper dropboxClientWrapper)
            :this()
        {
            _dropboxClientWrapper = dropboxClientWrapper;
        }

        public virtual IFile DropboxFile { get; set; }
        private DropboxClient _client;
        protected IDownloadResponse<FileMetadata> Response;
        protected Exception Exception;
        private ILocalPathManager _localPathManager;
        private IDropboxClientWrapper _dropboxClientWrapper;

        public virtual IDropboxSingleExecutor<IDropboxResult> GetDropboxSingleExecutor(IDropboxSingleExecutor<IDropboxResult> singleExecutor)
        {
            return singleExecutor;
        }

        public virtual ILocalPathManager LocalPathManager
        {
            set
            {
                _localPathManager = value;
            }
            get
            {
                return GetLocalPathManager();
            }
        }

        public virtual ILocalPathManager GetLocalPathManager()
        {
            return _localPathManager;
        }
                
        public OauthSource SelectedSource { get; set; }
                
        [Inputs("Path in the user's Dropbox")]
        [FindMissing]
        public string ToPath { get; set; }

        public bool OverwriteFile { get; set; }

        
        [Inputs("Local File Path")]
        [FindMissing]
        public string FromPath { get; set; }
        
        protected DropboxClient GetClient()
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
            evaluatedValues.TryGetValue("ToPath", out var localToPath);
            evaluatedValues.TryGetValue("FromPath", out var localFromPath);
            IDropboxSingleExecutor<IDropboxResult> dropBoxDownLoad = new DropBoxDownLoad(localToPath);
            var dropboxSingleExecutor = GetDropboxSingleExecutor(dropBoxDownLoad);
            _dropboxClientWrapper = _dropboxClientWrapper ?? new DropboxClientWrapper(GetClient());
            var dropboxExecutionResult = dropboxSingleExecutor.ExecuteTask(_dropboxClientWrapper);
            if (dropboxExecutionResult is DropboxDownloadSuccessResult dropboxSuccessResult)
            {
                Response = dropboxSuccessResult.GetDownloadResponse();
                var bytes = Response.GetContentAsByteArrayAsync().Result;
                if (Response.Response.IsFile)
                {
                    LocalPathManager = new LocalPathManager(localFromPath);
                    var validFolder = LocalPathManager.GetFullFileName();
                    var fileExist = LocalPathManager.FileExist();
                    if (fileExist && !OverwriteFile)
                    {
                        throw new Exception(ErrorResource.DropBoxDestinationFileAlreadyExist);
                    }

                    DropboxFile.WriteAllBytes(validFolder, bytes);
                }
                return new List<string> { GlobalConstants.DropBoxSuccess };
            }
            if (dropboxExecutionResult is DropboxFailureResult dropboxFailureResult)
            {
                Exception = dropboxFailureResult.GetException();
            }
            var executionError = Exception.InnerException?.Message ?? Exception.Message;
            if (executionError.Contains("not_file"))
            {
                executionError = ErrorResource.DropBoxFilePathMissing;
            }
            throw new Exception(executionError);
        }
        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            if (env == null)
            {
                return new List<DebugItem>();
            }
            base.GetDebugInputs(env, update);

            DebugItem debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Overwrite Local"), debugItem);
            string value = OverwriteFile ? "True" : "False";
            AddDebugItem(new DebugEvalResult(value, "", env, update), debugItem);
            _debugInputs.Add(debugItem);

            return _debugInputs;

        }

        public bool Equals(DsfDropBoxDownloadActivity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            var isSourceEqual = CommonEqualityOps.AreObjectsEqual<IResource>(SelectedSource, other.SelectedSource);
            return base.Equals(other) 
                && isSourceEqual
                && string.Equals(ToPath, other.ToPath) 
                && string.Equals(DisplayName, other.DisplayName) 
                && OverwriteFile == other.OverwriteFile
                && string.Equals(FromPath, other.FromPath);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DsfDropBoxDownloadActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (SelectedSource != null ? SelectedSource.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ToPath != null ? ToPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ OverwriteFile.GetHashCode();
                hashCode = (hashCode * 397) ^ (FromPath != null ? FromPath.GetHashCode() : 0);
                return hashCode;
            }
        }
        public void Dispose()
        {
            _client.Dispose();
        }
    }

        #endregion Overrides of DsfActivity
}