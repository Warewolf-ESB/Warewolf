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
using Dev2.Data.ServiceModel;
using Dev2.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Wrappers;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Storage.Interfaces;
using Dev2.Common.State;

namespace Dev2.Activities.DropBox2016.DropboxFileActivity
{
    [ToolDescriptorInfo("Dropbox", "List Contents", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090D8C8EA3E", "Dev2.Activities", "1.0.0.0", "Legacy", "Storage: Dropbox", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Dropbox_List_Contents")]
    public class DsfDropboxFileListActivity : DsfDropBoxBaseActivity, IEquatable<DsfDropboxFileListActivity>
    {
        public OauthSource SelectedSource { get; set; }

        public List<string> Files { get; set; }
        public Exception Exception { get; set; }

        [FindMissing]
        public bool IncludeMediaInfo { get; set; }

        [FindMissing]
        public bool IsRecursive { get; set; }

        [FindMissing]
        public bool IncludeDeleted { get; set; }

        [Inputs("Path in the user's Dropbox")]
        [FindMissing]
        public string ToPath { get; set; }

        [FindMissing]
        public bool IsFilesSelected { get; set; }

        [FindMissing]
        public bool IsFoldersSelected { get; set; }

        [FindMissing]
        public bool IsFilesAndFoldersSelected { get; set; }
        
        public DsfDropboxFileListActivity(IDropboxClientFactory dropboxClientFactory)
            :base(dropboxClientFactory)
        {
            DisplayName = "List Dropbox Contents";
            Files = new List<string>();
            IsFilesSelected = true;
            IncludeDeleted = false;
            IsRecursive = false;
            IncludeMediaInfo = false;
        }

        public DsfDropboxFileListActivity()
            : this(new DropboxClientWrapperFactory())
        {
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            if (string.IsNullOrWhiteSpace(ToPath))
            {
                ToPath = string.Empty;
            }
            base.ExecuteTool(dataObject, update);
        }

        public virtual IDropboxSingleExecutor<IDropboxResult> GetDropboxSingleExecutor(IDropboxSingleExecutor<IDropboxResult> singleExecutor) => singleExecutor;

        protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            evaluatedValues.TryGetValue("ToPath", out var toPath);

            IDropboxSingleExecutor<IDropboxResult> dropboxFileRead = new DropboxFileRead(IsRecursive, toPath, IncludeMediaInfo, IncludeDeleted);
            var dropboxSingleExecutor = GetDropboxSingleExecutor(dropboxFileRead);
            SetupDropboxClient(SelectedSource.AccessToken);
            var dropboxExecutionResult = dropboxSingleExecutor.ExecuteTask(_dropboxClient);
            if (dropboxExecutionResult is DropboxListFolderSuccesResult dropboxSuccessResult)
            {
                var listFolderResult = dropboxSuccessResult.GetListFolderResult();
                var metadatas = listFolderResult.Entries;
                if (IncludeDeleted)
                {
                    Files.AddRange(listFolderResult.Entries.Where(metadata => metadata.IsDeleted).Select(metadata => metadata.PathLower).ToList());
                }
                if (IsFoldersSelected)
                {
                    Files.AddRange(metadatas.Where(metadata => metadata.IsFolder).Select(metadata => metadata.PathLower).ToList());
                }

                if (IsFilesSelected)
                {
                    Files.AddRange(metadatas.Where(metadata => metadata.IsFile).Select(metadata => metadata.PathLower).ToList());
                }

                if (IsFilesAndFoldersSelected)
                {
                    Files.AddRange(metadatas.Where(metadata => metadata.IsFolder).Select(metadata => metadata.PathLower).ToList());
                    Files.AddRange(metadatas.Where(metadata => metadata.IsFile).Select(metadata => metadata.PathLower).ToList());
                }

                return new List<string> { GlobalConstants.DropBoxSuccess };
            }
            if (dropboxExecutionResult is DropboxFailureResult dropboxFailureResult)
            {
                Exception = dropboxFailureResult.GetException();
            }
            var executionError = Exception.InnerException?.Message ?? Exception.Message;

            throw new Exception(executionError);
        }

        protected override void AssignResult(IDSFDataObject dataObject, int update)
        {
            foreach (var file in Files)
            {
                dataObject.Environment.Assign(Result, file, update);
            }
        }

        public override enFindMissingType GetFindMissingType() => enFindMissingType.StaticActivity;

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            if (env == null)
            {
                return new List<DebugItem>();
            }
            base.GetDebugInputs(env, update);

            var debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Folders"), debugItem);
            var value = IsFoldersSelected ? "True" : "False";
            AddDebugItem(new DebugEvalResult(value, "", env, update), debugItem);
            _debugInputs.Add(debugItem);

            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Files"), debugItem);
            value = IsFilesSelected ? "True" : "False";
            AddDebugItem(new DebugEvalResult(value, "", env, update), debugItem);
            _debugInputs.Add(debugItem);

            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Files and Folders"), debugItem);
            value = IsFilesAndFoldersSelected ? "True" : "False";
            AddDebugItem(new DebugEvalResult(value, "", env, update), debugItem);
            _debugInputs.Add(debugItem);

            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Recursive"), debugItem);
            value = IsRecursive ? "True" : "False";
            AddDebugItem(new DebugEvalResult(value, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
            return _debugInputs;
        }

        public bool Equals(DsfDropboxFileListActivity other)
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
            var eq = base.Equals(other);
            eq &= isSourceEqual;
            eq &= Files.SequenceEqual(other.Files, StringComparer.Ordinal);
            eq &= IncludeMediaInfo == other.IncludeMediaInfo;
            eq &= IsRecursive == other.IsRecursive;
            eq &= IncludeDeleted == other.IncludeDeleted;
            eq &= string.Equals(ToPath, other.ToPath);
            eq &= string.Equals(DisplayName, other.DisplayName);
            eq &= IsFilesSelected == other.IsFilesSelected;
            eq &= IsFoldersSelected == other.IsFoldersSelected;
            eq &= IsFilesAndFoldersSelected == other.IsFilesAndFoldersSelected;
            return eq;
        }

        public override bool Equals(object obj)
        {
            if (obj is DsfDropboxFileListActivity)
            {
                return Equals((DsfDropboxFileListActivity)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (SelectedSource != null ? SelectedSource.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Files != null ? Files.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IncludeMediaInfo.GetHashCode();
                hashCode = (hashCode * 397) ^ IsRecursive.GetHashCode();
                hashCode = (hashCode * 397) ^ IncludeDeleted.GetHashCode();
                hashCode = (hashCode * 397) ^ (ToPath != null ? ToPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsFilesSelected.GetHashCode();
                hashCode = (hashCode * 397) ^ IsFoldersSelected.GetHashCode();
                hashCode = (hashCode * 397) ^ IsFilesAndFoldersSelected.GetHashCode();
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
                    Name = "ToPath",
                    Type = StateVariable.StateType.Input,
                    Value = ToPath
                },
                new StateVariable
                {
                    Name = "IsFilesSelected",
                    Type = StateVariable.StateType.Input,
                    Value = IsFilesSelected.ToString()
                },
                new StateVariable
                {
                    Name = "IsFoldersSelected",
                    Type = StateVariable.StateType.Input,
                    Value = IsFoldersSelected.ToString()
                },
                new StateVariable
                {
                    Name = "IsFilesAndFoldersSelected",
                    Type = StateVariable.StateType.Input,
                    Value = IsFilesAndFoldersSelected.ToString()
                },
                new StateVariable
                {
                    Name = "IsRecursive",
                    Type = StateVariable.StateType.Input,
                    Value = IsRecursive.ToString()
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