#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
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