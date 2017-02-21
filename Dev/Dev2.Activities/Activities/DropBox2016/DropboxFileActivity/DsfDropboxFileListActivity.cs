using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Dropbox;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.ServiceModel;
using Dev2.Factories;
using Dev2.Util;
using Dropbox.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Storage;

namespace Dev2.Activities.DropBox2016.DropboxFileActivity
{
    [ToolDescriptorInfo ("Dropbox", "List Contents", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090D8C8EA3E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Storage: Dropbox", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Dropbox_List_Contents")]
    public class DsfDropboxFileListActivity : DsfBaseActivity
    {
        // ReSharper disable MemberCanBePrivate.Global
        public IDropboxFactory DropboxFactory { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public OauthSource SelectedSource { get; set; }

        public List<string> Files { get; set; }
        private DropboxClient _dropboxClient;
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

        // ReSharper restore MemberCanBePrivate.Global

        private DsfDropboxFileListActivity(IDropboxFactory dropboxFactory)
        {
            DropboxFactory = dropboxFactory;
            // ReSharper disable VirtualMemberCallInContructor
            DisplayName = "List Dropbox Contents";
            Files = new List<string>();
            IsFilesSelected = true;
            IncludeDeleted = false;
            IsRecursive = false;
            IncludeMediaInfo = false;
        }

        public DsfDropboxFileListActivity()
            : this(new DropboxFactory())
        {
        }

        public DropboxClient GetDropboxClient()
        {
            if (_dropboxClient != null)
            {
                return _dropboxClient;
            }
            _dropboxClient = DropboxFactory.CreateWithSecret(SelectedSource.AccessToken);
            return _dropboxClient;
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            if (string.IsNullOrWhiteSpace(ToPath))
            {
                ToPath = string.Empty;
            }
            base.ExecuteTool(dataObject, update);
        }

        public virtual IDropboxSingleExecutor<IDropboxResult> GetDropboxSingleExecutor(IDropboxSingleExecutor<IDropboxResult> singleExecutor)
        {
            return singleExecutor;
        }

        protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            var toPath = evaluatedValues["ToPath"];

            IDropboxSingleExecutor<IDropboxResult> dropboxFileRead = new DropboxFileRead(IsRecursive, toPath, IncludeMediaInfo, IncludeDeleted);
            var dropboxSingleExecutor = GetDropboxSingleExecutor(dropboxFileRead);
            var dropboxExecutionResult = dropboxSingleExecutor.ExecuteTask(GetDropboxClient());
            var dropboxSuccessResult = dropboxExecutionResult as DropboxListFolderSuccesResult;
            if (dropboxSuccessResult != null)
            {
                var listFolderResult = dropboxSuccessResult.GetListFolderResulResult();
                var metadatas = listFolderResult.Entries;
                if (IncludeDeleted)
                {
                    Files.AddRange(listFolderResult.Entries.Where(metadata => metadata.IsDeleted).Select(metadata => metadata.PathLower).ToList());
                }
                if (IsFoldersSelected)
                    Files.AddRange(metadatas.Where(metadata => metadata.IsFolder).Select(metadata => metadata.PathLower).ToList());
                if (IsFilesSelected)
                    Files.AddRange(metadatas.Where(metadata => metadata.IsFile).Select(metadata => metadata.PathLower).ToList());
                if (IsFilesAndFoldersSelected)
                {
                    Files.AddRange(metadatas.Where(metadata => metadata.IsFolder).Select(metadata => metadata.PathLower).ToList());
                    Files.AddRange(metadatas.Where(metadata => metadata.IsFile).Select(metadata => metadata.PathLower).ToList());
                }

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

        protected override void AssignResult(IDSFDataObject dataObject, int update)
        {
            foreach (var file in Files)
            {
                dataObject.Environment.Assign(Result, file, update);
            }
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.StaticActivity;
        }

        #region Overrides of DsfBaseActivity
        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            if (env == null)
            {
                return new List<DebugItem>();
            }
            base.GetDebugInputs(env, update);

            DebugItem debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Folders"), debugItem);
            string value = IsFoldersSelected ? "True" : "False";
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


        #endregion
    }
}