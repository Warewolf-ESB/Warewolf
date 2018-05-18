﻿using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
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
    [ToolDescriptorInfo("SharepointLogo", "Move File", ToolType.Native, "2246E59B-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Sharepoint", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_SharePoint_Move_File")]
    public class SharepointMoveFileActivity : DsfAbstractFileActivity,IEquatable<SharepointMoveFileActivity>
    {
        public SharepointMoveFileActivity() : base("SharePoint Move File")
        {
            ServerInputPathFrom = string.Empty;
            ServerInputPathTo = string.Empty;
        }
        protected override bool AssignEmptyOutputsToRecordSet => true;
        /// <summary>
        /// Gets or sets the input path.
        /// </summary>
        [Inputs("Server Input Path From")]
        [FindMissing]
        public string ServerInputPathFrom
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the input path.
        /// </summary>
        [Inputs("Server Input Path To")]
        [FindMissing]
        public string ServerInputPathTo
        {
            get;
            set;
        }

        [Inputs("Overwrite")]
        [FindMissing]
        public bool Overwrite { get; set; }

        public SharepointSource SharepointSource { get; set; }

        public Guid SharepointServerResourceId { get; set; }
        
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

            var serverInputFromItr = new WarewolfIterator(context.Environment.Eval(ServerInputPathFrom, update));
            colItr.AddVariableToIterateOn(serverInputFromItr);

            var serverInputFromTo = new WarewolfIterator(context.Environment.Eval(ServerInputPathTo, update));
            colItr.AddVariableToIterateOn(serverInputFromTo);

            if (context.IsDebugMode())
            {
                AddDebugInputItem(ServerInputPathFrom, "ServerInput Path From", context.Environment, update);
                AddDebugInputItem(ServerInputPathTo, "ServerInput Path To", context.Environment, update);
            }

            while (colItr.HasMoreData())
            {
                try
                {
                    ExecuteConcreteAction(outputs, colItr, sharepointSource, serverInputFromItr, serverInputFromTo);
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

        private void ExecuteConcreteAction(IList<OutputTO> outputs, WarewolfListIterator colItr, SharepointSource sharepointSource, WarewolfIterator serverInputFromItr, WarewolfIterator serverInputFromTo)
        {
            var serverPath = colItr.FetchNextValue(serverInputFromItr);
            var localPath = colItr.FetchNextValue(serverInputFromTo);

            if (DataListUtil.IsValueRecordset(Result) && DataListUtil.GetRecordsetIndexType(Result) != enRecordsetIndexType.Numeric)
            {
                if (DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
                {
                    var recsetName = DataListUtil.ExtractRecordsetNameFromValue(Result);
                    var fieldName = DataListUtil.ExtractFieldNameFromValue(Result);

                    var newPath = MoveFile(sharepointSource, serverPath, localPath);

                    var indexToUpsertTo = 1;

                    foreach (var file in newPath)
                    {
                        var fullRecsetName = DataListUtil.CreateRecordsetDisplayValue(recsetName, fieldName,
                            indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
                        outputs.Add(DataListFactory.CreateOutputTO(DataListUtil.AddBracketsToValueIfNotExist(fullRecsetName), file));
                        indexToUpsertTo++;
                    }
                }
                else
                {
                    AddBlankIndexDebugOutputs(outputs, sharepointSource, serverPath, localPath);
                }
            }
            else
            {
                var newPath = MoveFile(sharepointSource, serverPath, localPath);

                var xmlList = string.Join(",", newPath.Select(c => c));
                outputs.Add(DataListFactory.CreateOutputTO(Result));
                outputs.Last().OutputStrings.Add(xmlList);
            }
        }

        private void AddBlankIndexDebugOutputs(IList<OutputTO> outputs, SharepointSource sharepointSource, string serverPath, string localPath)
        {
            if (DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Blank)
            {
                var newPath = MoveFile(sharepointSource, serverPath, localPath);

                foreach (var folder in newPath)
                {
                    outputs.Add(DataListFactory.CreateOutputTO(Result, folder));
                }
            }
        }

        void ValidateRequest()
        {
            if (SharepointServerResourceId == Guid.Empty)
            {
                throw new ArgumentNullException(SharepointServerResourceId.ToString(), ErrorResource.InvalidSource);
            }

            if (string.IsNullOrEmpty(ServerInputPathFrom))
            {
                throw new ArgumentNullException(ServerInputPathFrom, string.Format(ErrorResource.ServerInputPathEmpty, "FROM"));
            }

            if (string.IsNullOrEmpty(ServerInputPathTo))
            {
                throw new ArgumentNullException(ServerInputPathTo, string.Format(ErrorResource.ServerInputPathEmpty, "TO"));
            }
        }

        public IEnumerable<string> MoveFile(SharepointSource sharepointSource, string serverPathFrom, string serverPathTo)
        {
            var sharepointHelper = sharepointSource.CreateSharepointHelper();

            var newPath = sharepointHelper.MoveFile(serverPathFrom, serverPathTo, Overwrite);

            return new List<string> { newPath };
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

        public bool Equals(SharepointMoveFileActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) && string.Equals(ServerInputPathFrom, other.ServerInputPathFrom) && string.Equals(ServerInputPathTo, other.ServerInputPathTo) && Overwrite == other.Overwrite && Equals(SharepointSource, other.SharepointSource) && SharepointServerResourceId.Equals(other.SharepointServerResourceId);
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

            return Equals((SharepointMoveFileActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (ServerInputPathFrom != null ? ServerInputPathFrom.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ServerInputPathTo != null ? ServerInputPathTo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Overwrite.GetHashCode();
                hashCode = (hashCode * 397) ^ (SharepointSource != null ? SharepointSource.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ SharepointServerResourceId.GetHashCode();
                return hashCode;
            }
        }
    }
}
