#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Data.ServiceModel;
using Dev2.Util;
using System;
using System.Collections.Generic;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Wrappers;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using Dev2.Common.State;

namespace Dev2.Activities.DropBox2016.UploadActivity
{
    [ToolDescriptorInfo("Dropbox", "Upload", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C8C9EA2E", "Dev2.Activities", "1.0.0.0", "Legacy", "Storage: Dropbox", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Dropbox_Upload")]
    public class DsfDropBoxUploadActivity : DsfDropBoxBaseActivity, IEquatable<DsfDropBoxUploadActivity>
    {
        bool _addMode;
        bool _overWriteMode;
        protected Exception Exception;
        protected IDropboxSingleExecutor<IDropboxResult> DropboxSingleExecutor;

        public DsfDropBoxUploadActivity()
            : this(new DropboxClientWrapperFactory())
        {            
        }

        public DsfDropBoxUploadActivity(IDropboxClientFactory clientWrapperFactory)
            :base(clientWrapperFactory)
        {
            DisplayName = "Upload to Dropbox";
            OverWriteMode = true;
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
            get => _overWriteMode;
            set
            {
                _addMode = !value;
                _overWriteMode = value;
            }
        }

        public bool AddMode
        {
            get => _addMode;
            set
            {
                _overWriteMode = !value;
                _addMode = value;
            }
        }

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
            DropboxSingleExecutor = new DropBoxUpload(OverWriteMode, evaluatedValues["ToPath"], evaluatedValues["FromPath"]);
            SetupDropboxClient(SelectedSource.AccessToken);
            var dropboxExecutionResult = DropboxSingleExecutor.ExecuteTask(_dropboxClient);
            if (dropboxExecutionResult is DropboxUploadSuccessResult dropboxSuccessResult)
            {
                return new List<string> { GlobalConstants.DropBoxSuccess };
            }
            if (dropboxExecutionResult is DropboxFailureResult dropboxFailureResult)
            {
                Exception = dropboxFailureResult.GetException();
            }
            var executionError = Exception.InnerException?.Message ?? Exception.Message;
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
            AddDebugItem(new DebugItemStaticDataParams("", "OverWrite"), debugItem);
            var value = OverWriteMode ? "True" : "False";
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
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

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
            if (obj is DsfDropBoxUploadActivity)
            {
                return Equals((DsfDropBoxUploadActivity) obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (SelectedSource != null ? SelectedSource.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FromPath != null ? FromPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ToPath != null ? ToPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ OverWriteMode.GetHashCode();
                hashCode = (hashCode * 397) ^ AddMode.GetHashCode();
                return hashCode;
            }
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
                    Name = "OverWriteMode",
                    Type = StateVariable.StateType.Input,
                    Value = OverWriteMode.ToString()
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