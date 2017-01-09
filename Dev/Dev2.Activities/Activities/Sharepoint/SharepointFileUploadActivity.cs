using System;
using System.Activities;
using System.Collections.Generic;
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
    [ToolDescriptorInfo("SharepointLogo", "Upload File", ToolType.Native, "8226E59B-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Sharepoint", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_SharePoint_Upload_File")]
    public class SharepointFileUploadActivity : DsfAbstractFileActivity
    {
        public SharepointFileUploadActivity() : base("SharePoint Upload File")
        {
            ServerInputPath = string.Empty;
            LocalInputPath = string.Empty;
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
        /// <summary>
        /// Gets or sets the input path.
        /// </summary>
        [Inputs("Local Input Path")]
        [FindMissing]
        public string LocalInputPath
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

            var serverInputItr = new WarewolfIterator(dataObject.Environment.Eval(ServerInputPath, update));
            colItr.AddVariableToIterateOn(serverInputItr);

            var localInputItr = new WarewolfIterator(dataObject.Environment.Eval(LocalInputPath, update));
            colItr.AddVariableToIterateOn(localInputItr);

            if (dataObject.IsDebugMode())
            {
                AddDebugInputItem(ServerInputPath, "ServerInput Path", dataObject.Environment, update);
                AddDebugInputItem(LocalInputPath, "LocalInput Path", dataObject.Environment, update);
            }

            while (colItr.HasMoreData())
            {
                try
                {
                    var serverPath = colItr.FetchNextValue(serverInputItr);
                    var localPath = colItr.FetchNextValue(localInputItr);

                    if (DataListUtil.IsValueRecordset(Result) && DataListUtil.GetRecordsetIndexType(Result) != enRecordsetIndexType.Numeric)
                    {
                        if (DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
                        {
                            string recsetName = DataListUtil.ExtractRecordsetNameFromValue(Result);
                            string fieldName = DataListUtil.ExtractFieldNameFromValue(Result);

                            var newPath = UpdloadFile(sharepointSource, serverPath, localPath);

                            int indexToUpsertTo = 1;

                            foreach (var file in newPath)
                            {
                                string fullRecsetName = DataListUtil.CreateRecordsetDisplayValue(recsetName, fieldName,
                                    indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
                                outputs.Add(DataListFactory.CreateOutputTO(DataListUtil.AddBracketsToValueIfNotExist(fullRecsetName), file));
                                indexToUpsertTo++;
                            }
                        }
                        else if (DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Blank)
                        {
                            var newPath = UpdloadFile(sharepointSource, serverPath, localPath);

                            foreach (var folder in newPath)
                            {
                                outputs.Add(DataListFactory.CreateOutputTO(Result, folder));
                            }
                        }
                    }
                    else
                    {
                        var newPath = UpdloadFile(sharepointSource, serverPath, localPath);

                        string xmlList = string.Join(",", newPath.Select(c => c));
                        outputs.Add(DataListFactory.CreateOutputTO(Result));
                        outputs.Last().OutputStrings.Add(xmlList);
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

            if (string.IsNullOrEmpty(LocalInputPath))
            {
                throw new ArgumentNullException(LocalInputPath, ErrorResource.LocalInputPathEmpty);
            }
        }

        public IEnumerable<string> UpdloadFile(SharepointSource sharepointSource, string serverPath, string localPath)
        {
            var sharepointHelper = sharepointSource.CreateSharepointHelper();

            var newPath = sharepointHelper.UploadFile(serverPath, localPath);

            return new List<string> { newPath };
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
