#pragma warning disable
﻿using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Data;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.ServiceModel;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities.Sharepoint
{
    [ToolDescriptorInfo("SharepointLogo", "Read Folder", ToolType.Native, "8222E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Sharepoint", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_SharePoint_Read_Folder")]
    public class SharepointReadFolderItemActivity : DsfAbstractFileActivity,IEquatable<SharepointReadFolderItemActivity>
    {
        public SharepointReadFolderItemActivity() : base("SharePoint Read Folder Items")
        {
            ServerInputPath = string.Empty;
        }

        protected override bool AssignEmptyOutputsToRecordSet => true;
        
        [Inputs("Files")]
        [FindMissing]

        public bool IsFilesSelected
        {
            get;
            set;
        }
        
        [Inputs("Folders")]
        [FindMissing]
        public bool IsFoldersSelected
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the files and folders option.
        /// </summary>
    
        [Inputs("Files & Folders")]
        [FindMissing]
        public bool IsFilesAndFoldersSelected
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the input path.
        /// </summary>
        [Inputs("Server Input Path")]
        [FindMissing]
        public string ServerInputPath
        {
            get;
            set;
        }

        public SharepointSource SharepointSource { get; set; }

        public Guid SharepointServerResourceId { get; set; }
        public override IEnumerable<StateVariable> GetState()
        {
            return new[]
            {
                new StateVariable
                {
                    Name="SharepointServerResourceId",
                    Type = StateVariable.StateType.Input,
                    Value = SharepointServerResourceId.ToString()
                 },
                 new StateVariable
                {
                    Name="ServerInputPath",
                    Type = StateVariable.StateType.Input,
                    Value = ServerInputPath
                 },
                new StateVariable
                {
                    Name="IsFilesAndFoldersSelected",
                    Type = StateVariable.StateType.Input,
                    Value = IsFilesAndFoldersSelected.ToString()
                },
                new StateVariable
                {
                    Name="IsFoldersSelected",
                    Type = StateVariable.StateType.Input,
                    Value = IsFoldersSelected.ToString()
                },
                new StateVariable
                {
                    Name="IsFilesSelected",
                    Type = StateVariable.StateType.Input,
                    Value = IsFilesSelected.ToString()
                }
                ,
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = Result
                }
            };
        }
        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        [ExcludeFromCodeCoverage]
        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
        }

        [ExcludeFromCodeCoverage]
        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
        }

        [ExcludeFromCodeCoverage]
        public override IList<DsfForEachItem> GetForEachInputs() => null;

        [ExcludeFromCodeCoverage]
        public override IList<DsfForEachItem> GetForEachOutputs() => null;

        protected override IList<OutputTO> TryExecuteConcreteAction(IDSFDataObject context, out ErrorResultTO error, int update)
        {
            _debugInputs = new List<DebugItem>();
            error = new ErrorResultTO();
            IList<OutputTO> outputs = new List<OutputTO>();
            var colItr = new WarewolfListIterator();
           
            var sharepointSource = ResourceCatalog.GetResource<SharepointSource>(context.WorkspaceID, SharepointServerResourceId); 
            if (sharepointSource == null)
            {
                sharepointSource = SharepointSource;
                SharepointServerResourceId = sharepointSource.ResourceID;
            }

            ValidateRequest();

            var inputItr = new WarewolfIterator(context.Environment.Eval(ServerInputPath, update));
            colItr.AddVariableToIterateOn(inputItr);

            if (context.IsDebugMode())
            {
                AddDebugInputItem(ServerInputPath, "Input Path", context.Environment, update);
            }

            while (colItr.HasMoreData())
            {
                try
                {
                    ExecuteConcreteAction(outputs, colItr, sharepointSource, inputItr);
                }
                catch (Exception e)
                {
                    outputs.Add(DataListFactory.CreateOutputTO(null));
                    error.AddError(e.Message);
                    break;
                }
            }

            return outputs;
        }

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        private void ExecuteConcreteAction(IList<OutputTO> outputs, WarewolfListIterator colItr, SharepointSource sharepointSource, WarewolfIterator inputItr)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
        {
            var path = colItr.FetchNextValue(inputItr);

            if (DataListUtil.IsValueRecordset(Result) && DataListUtil.GetRecordsetIndexType(Result) != enRecordsetIndexType.Numeric)
            {
                if (DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
                {
                    var recsetName = DataListUtil.ExtractRecordsetNameFromValue(Result);
                    var fieldName = DataListUtil.ExtractFieldNameFromValue(Result);

                    if (IsFoldersSelected)
                    {
                        AddAllFolders(outputs, sharepointSource, path, recsetName, fieldName);
                    }
                    if (IsFilesSelected)
                    {
                        AddAllFiles(outputs, sharepointSource, path, recsetName, fieldName);
                    }

                    if (IsFilesAndFoldersSelected)
                    {
                        AddAllFilesAndFolders(outputs, sharepointSource, path, recsetName, fieldName);
                    }
                }
                else
                {
                    AddBlankIndexDebugOutputs(outputs, sharepointSource, path);
                }
            }
            else
            {
                if (IsFoldersSelected)
                {
                    var folders = GetSharePointFolders(sharepointSource, path);

                    var xmlList = string.Join(",", folders.Select(c => c));
                    outputs.Add(DataListFactory.CreateOutputTO(Result));
                    outputs.Last().OutputStrings.Add(xmlList);
                }
                if (IsFilesSelected)
                {
                    var files = GetSharePointFiles(sharepointSource, path);

                    var xmlList = string.Join(",", files.Select(c => c));
                    outputs.Add(DataListFactory.CreateOutputTO(Result));
                    outputs.Last().OutputStrings.Add(xmlList);
                }

                if (IsFilesAndFoldersSelected)
                {
                    var folderAndPathList = new List<string>();
                    folderAndPathList.AddRange(GetSharePointFiles(sharepointSource, path));
                    folderAndPathList.AddRange(GetSharePointFolders(sharepointSource, path));

                    var xmlList = string.Join(",", folderAndPathList.Select(c => c));
                    outputs.Add(DataListFactory.CreateOutputTO(Result));
                    outputs.Last().OutputStrings.Add(xmlList);
                }
            }
        }

        void AddBlankIndexDebugOutputs(IList<OutputTO> outputs, SharepointSource sharepointSource, string path)
        {
            if (DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Blank)
            {
                if (IsFoldersSelected)
                {
                    var folders = GetSharePointFolders(sharepointSource, path);

                    foreach (var folder in folders)
                    {
                        outputs.Add(DataListFactory.CreateOutputTO(Result, folder));
                    }
                }
                if (IsFilesSelected)
                {
                    var files = GetSharePointFiles(sharepointSource, path);

                    foreach (var file in files)
                    {
                        outputs.Add(DataListFactory.CreateOutputTO(Result, file));
                    }
                }

                if (IsFilesAndFoldersSelected)
                {
                    var folderAndPathList = new List<string>();
                    folderAndPathList.AddRange(GetSharePointFiles(sharepointSource, path));
                    folderAndPathList.AddRange(GetSharePointFolders(sharepointSource, path));

                    foreach (var fileAndfolder in folderAndPathList)
                    {
                        outputs.Add(DataListFactory.CreateOutputTO(Result, fileAndfolder));
                    }
                }
            }
        }

        private void AddAllFilesAndFolders(IList<OutputTO> outputs, SharepointSource sharepointSource, string path, string recsetName, string fieldName)
        {
            var folderAndPathList = new List<string>();
            folderAndPathList.AddRange(GetSharePointFiles(sharepointSource, path));
            folderAndPathList.AddRange(GetSharePointFolders(sharepointSource, path));

            var indexToUpsertTo = 1;

            foreach (var fileAndfolder in folderAndPathList)
            {
                var fullRecsetName = DataListUtil.CreateRecordsetDisplayValue(recsetName, fieldName,
                    indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
                outputs.Add(DataListFactory.CreateOutputTO(DataListUtil.AddBracketsToValueIfNotExist(fullRecsetName), fileAndfolder));
                indexToUpsertTo++;
            }
        }

        private void AddAllFiles(IList<OutputTO> outputs, SharepointSource sharepointSource, string path, string recsetName, string fieldName)
        {
            var files = GetSharePointFiles(sharepointSource, path);
            var indexToUpsertTo = 1;

            foreach (var file in files)
            {
                var fullRecsetName = DataListUtil.CreateRecordsetDisplayValue(recsetName, fieldName,
                    indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
                outputs.Add(DataListFactory.CreateOutputTO(DataListUtil.AddBracketsToValueIfNotExist(fullRecsetName), file));
                indexToUpsertTo++;
            }
        }

        private void AddAllFolders(IList<OutputTO> outputs, SharepointSource sharepointSource, string path, string recsetName, string fieldName)
        {
            var folders = GetSharePointFolders(sharepointSource, path);
            var indexToUpsertTo = 1;

            foreach (var folder in folders)
            {
                var fullRecsetName = DataListUtil.CreateRecordsetDisplayValue(recsetName, fieldName,
                                indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
                outputs.Add(DataListFactory.CreateOutputTO(DataListUtil.AddBracketsToValueIfNotExist(fullRecsetName), folder));
                indexToUpsertTo++;
            }
        }

        void ValidateRequest()
        {
            if (SharepointServerResourceId == Guid.Empty)
            {
                throw new ArgumentNullException(SharepointServerResourceId.ToString(), ErrorResource.InvalidSource);
            }
        }

        IEnumerable<string> GetSharePointFiles(SharepointSource sharepointSource, string path)
        {
            var sharepointHelper = sharepointSource.CreateSharepointHelper();
            var files = sharepointHelper.LoadFiles(path);
            return files;
        }

        IEnumerable<string> GetSharePointFolders(SharepointSource sharepointSource, string path)
        {
            var sharepointHelper = sharepointSource.CreateSharepointHelper();
            var folders = sharepointHelper.LoadFolders(path);
            return folders;
        }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        public bool Equals(SharepointReadFolderItemActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) && IsFilesSelected == other.IsFilesSelected && IsFoldersSelected == other.IsFoldersSelected && IsFilesAndFoldersSelected == other.IsFilesAndFoldersSelected && string.Equals(ServerInputPath, other.ServerInputPath) && Equals(SharepointSource, other.SharepointSource) && SharepointServerResourceId.Equals(other.SharepointServerResourceId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((SharepointReadFolderItemActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ IsFilesSelected.GetHashCode();
                hashCode = (hashCode * 397) ^ IsFoldersSelected.GetHashCode();
                hashCode = (hashCode * 397) ^ IsFilesAndFoldersSelected.GetHashCode();
                hashCode = (hashCode * 397) ^ (ServerInputPath != null ? ServerInputPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SharepointSource != null ? SharepointSource.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ SharepointServerResourceId.GetHashCode();
                return hashCode;
            }
        }
    }
}
