using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data;
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

namespace Dev2.Activities.Sharepoint
{
    [ToolDescriptorInfo("SharepointLogo", "Read Folder", ToolType.Native, "8222E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Sharepoint", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_SharePoint_Read_Folder")]
    public class SharepointReadFolderItemActivity : DsfAbstractFileActivity
    {
        public SharepointReadFolderItemActivity() : base("SharePoint Read Folder Items")
        {
            ServerInputPath = string.Empty;
        }

        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        /// <summary>
        /// Gets or sets the files option.
        /// </summary>
        [Inputs("Files")]
        [FindMissing]

        public bool IsFilesSelected
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the folders otion.
        /// </summary>
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
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return null;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return null;
        }


        protected override IList<OutputTO> ExecuteConcreteAction(IDSFDataObject dataObject, out ErrorResultTO allErrors, int update)
        {
           
            _debugInputs = new List<DebugItem>();
            allErrors = new ErrorResultTO();
            IList<OutputTO> outputs = new List<OutputTO>();
            var colItr = new WarewolfListIterator();
           
            var sharepointSource = ResourceCatalog.GetResource<SharepointSource>(dataObject.WorkspaceID, SharepointServerResourceId); 
            if (sharepointSource == null)
            {
                sharepointSource = SharepointSource;
                SharepointServerResourceId = sharepointSource.ResourceID;
            }

            ValidateRequest();

            var inputItr = new WarewolfIterator(dataObject.Environment.Eval(ServerInputPath, update));
            colItr.AddVariableToIterateOn(inputItr);

            if (dataObject.IsDebugMode())
            {
                AddDebugInputItem(ServerInputPath, "Input Path", dataObject.Environment, update);
            }

            while (colItr.HasMoreData())
            {
                try
                {
                    var path = colItr.FetchNextValue(inputItr);

                    if (DataListUtil.IsValueRecordset(Result) && DataListUtil.GetRecordsetIndexType(Result) != enRecordsetIndexType.Numeric)
                    {
                        if (DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
                        {
                            string recsetName = DataListUtil.ExtractRecordsetNameFromValue(Result);
                            string fieldName = DataListUtil.ExtractFieldNameFromValue(Result);

                            if (IsFoldersSelected)
                            {
                                var folders = GetSharePointFolders(sharepointSource, path);
                                int indexToUpsertTo = 1;

                                foreach (var folder in folders)
                                {
                                    string fullRecsetName = DataListUtil.CreateRecordsetDisplayValue(recsetName, fieldName,
                                                    indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
                                    outputs.Add(DataListFactory.CreateOutputTO(DataListUtil.AddBracketsToValueIfNotExist(fullRecsetName), folder));
                                    indexToUpsertTo++;
                                }
                            }
                            if (IsFilesSelected)
                            {
                                var files = GetSharePointFiles(sharepointSource, path);
                                int indexToUpsertTo = 1;

                                foreach (var file in files)
                                {
                                    string fullRecsetName = DataListUtil.CreateRecordsetDisplayValue(recsetName, fieldName,
                                        indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
                                    outputs.Add(DataListFactory.CreateOutputTO(DataListUtil.AddBracketsToValueIfNotExist(fullRecsetName), file));
                                    indexToUpsertTo++;
                                }
                            }

                            if (IsFilesAndFoldersSelected)
                            {
                                var folderAndPathList = new List<string>();
                                folderAndPathList.AddRange(GetSharePointFiles(sharepointSource, path));
                                folderAndPathList.AddRange(GetSharePointFolders(sharepointSource, path));

                                int indexToUpsertTo = 1;

                                foreach (var fileAndfolder in folderAndPathList)
                                {
                                    string fullRecsetName = DataListUtil.CreateRecordsetDisplayValue(recsetName, fieldName,
                                        indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
                                    outputs.Add(DataListFactory.CreateOutputTO(DataListUtil.AddBracketsToValueIfNotExist(fullRecsetName), fileAndfolder));
                                    indexToUpsertTo++;
                                }
                            }
                        }
                        else if (DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Blank)
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
                    else
                    {
                        if (IsFoldersSelected)
                        {
                            var folders = GetSharePointFolders(sharepointSource, path);

                            string xmlList = string.Join(",", folders.Select(c => c));
                            outputs.Add(DataListFactory.CreateOutputTO(Result));
                            outputs.Last().OutputStrings.Add(xmlList);
                        }
                        if (IsFilesSelected)
                        {
                            var files = GetSharePointFiles(sharepointSource, path);

                            string xmlList = string.Join(",", files.Select(c => c));
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
                catch (Exception e)
                {
                    outputs.Add(DataListFactory.CreateOutputTO(null));
                    allErrors.AddError(e.Message);
                    break;
                }
            }

            return outputs;
        }

        private void ValidateRequest()
        {
            if (SharepointServerResourceId == Guid.Empty)
            {
                throw new ArgumentNullException(SharepointServerResourceId.ToString(), ErrorResource.InvalidSource);
            }
        }

        private IEnumerable<string> GetSharePointFiles(SharepointSource sharepointSource, string path)
        {
            var sharepointHelper = sharepointSource.CreateSharepointHelper();
            var files = sharepointHelper.LoadFiles(path);
            return files;
        }

        private IEnumerable<string> GetSharePointFolders(SharepointSource sharepointSource, string path)
        {
            var sharepointHelper = sharepointSource.CreateSharepointHelper();
            var folders = sharepointHelper.LoadFolders(path);
            return folders;
        }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList, int update)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }
    }
}
