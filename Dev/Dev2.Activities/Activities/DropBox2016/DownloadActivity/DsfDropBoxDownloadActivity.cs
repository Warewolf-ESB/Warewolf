#pragma warning disable
 /*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Data.ServiceModel;
using Dev2.Util;
using System;
using System.Collections.Generic;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Data;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using Dev2.Common.State;

namespace Dev2.Activities.DropBox2016.DownloadActivity
{
    [ToolDescriptorInfo("Dropbox", "Download", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090D8C8EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Storage: Dropbox", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Dropbox_Download")]
    public class DsfDropBoxDownloadActivity : DsfDropBoxBaseActivity, IDisposable,IEquatable<DsfDropBoxDownloadActivity>
    {
        public DsfDropBoxDownloadActivity()
            : this(new DropboxClientWrapperFactory())
        {
            DisplayName = "Download from Dropbox";
            OverwriteFile = false;
            
            DropboxFile = new FileWrapper();
        }

        protected DsfDropBoxDownloadActivity(IDropboxClientFactory dropboxClientFactory)
            : base(dropboxClientFactory)
        {
        }

        public virtual IFile DropboxFile { get; set; }

        protected Exception Exception;
        ILocalPathManager _localPathManager;

        public virtual IDropboxSingleExecutor<IDropboxResult> GetDropboxSingleExecutor(IDropboxSingleExecutor<IDropboxResult> singleExecutor) => singleExecutor;

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

        public virtual ILocalPathManager GetLocalPathManager() => _localPathManager;

        public OauthSource SelectedSource { get; set; }
                
        [Inputs("Path in the user's Dropbox")]
        [FindMissing]
        public string ToPath { get; set; }

        public bool OverwriteFile { get; set; }
        
        [Inputs("Local File Path")]
        [FindMissing]
        public string FromPath { get; set; }
        
        public override enFindMissingType GetFindMissingType() => enFindMissingType.StaticActivity;

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

        //All units used here has been unit tested seperately
        protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            evaluatedValues.TryGetValue("ToPath", out var localToPath);
            evaluatedValues.TryGetValue("FromPath", out var localFromPath);
            IDropboxSingleExecutor<IDropboxResult> dropBoxDownLoad = new DropBoxDownLoad(localToPath);
            var dropboxSingleExecutor = GetDropboxSingleExecutor(dropBoxDownLoad);
            SetupDropboxClient(SelectedSource?.AccessToken);
            var dropboxExecutionResult = dropboxSingleExecutor.ExecuteTask(_dropboxClient);
            if (dropboxExecutionResult is DropboxDownloadSuccessResult dropboxSuccessResult)
            {
                var response = dropboxSuccessResult;
                var bytes = response.GetContentAsByteArrayAsync().Result;
                if (response.Response.IsFile)
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

            var debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Overwrite Local"), debugItem);
            var value = OverwriteFile ? "True" : "False";
            AddDebugItem(new DebugEvalResult(value, "", env, update), debugItem);
            _debugInputs.Add(debugItem);

            return _debugInputs;
        }

        public bool Equals(DsfDropBoxDownloadActivity other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var isSourceEqual = CommonEqualityOps.AreObjectsEqual<IResource>(SelectedSource, other.SelectedSource);
#pragma warning disable S1067 // Expressions should not be too complex
            return base.Equals(other) 
                && isSourceEqual
                && string.Equals(ToPath, other.ToPath) 
                && string.Equals(DisplayName, other.DisplayName) 
                && OverwriteFile == other.OverwriteFile
                && string.Equals(FromPath, other.FromPath);
#pragma warning restore S1067 // Expressions should not be too complex
        }

        public override bool Equals(object obj)
        {
            if (obj is DsfDropBoxDownloadActivity)
            {
                return Equals((DsfDropBoxDownloadActivity) obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (SelectedSource != null ? SelectedSource.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ToPath != null ? ToPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ OverwriteFile.GetHashCode();
                hashCode = (hashCode * 397) ^ (FromPath != null ? FromPath.GetHashCode() : 0);
                return hashCode;
            }
        }
        public void Dispose()
        {
            _dropboxClient.Dispose();
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[]
            {
                new StateVariable
                {
                    Name = "SelectedSource.ResourceID",
                    Type = StateVariable.StateType.Input,
                    Value = SelectedSource.ResourceID.ToString()
                },
                new StateVariable
                {
                    Name = "FromPath",
                    Type = StateVariable.StateType.Input,
                    Value = FromPath
                },
                new StateVariable
                {
                    Name = "ToPath",
                    Type = StateVariable.StateType.Input,
                    Value = ToPath
                },
                new StateVariable
                {
                    Name = "OverwriteFile",
                    Type = StateVariable.StateType.Input,
                    Value = OverwriteFile.ToString()
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = Result
                }
            };
        }
    }
}