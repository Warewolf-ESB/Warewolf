#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
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
    [ToolDescriptorInfo("SharepointLogo", "Delete File", ToolType.Native, "2246E59B-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Sharepoint", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_SharePoint_Delete_File")]
    public class SharepointDeleteFileActivity : DsfAbstractFileActivity,IEquatable<SharepointDeleteFileActivity>
    {
        public SharepointDeleteFileActivity() : base("SharePoint Delete File")
        {
            ServerInputPath = string.Empty;
        }
        protected override bool AssignEmptyOutputsToRecordSet => true;
        
        [Inputs("Server Input Path")]
        [FindMissing]
        public string ServerInputPath
        {
            get;
            set;
        }
        
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

            var serverInputItr = new WarewolfIterator(context.Environment.Eval(ServerInputPath, update));
            colItr.AddVariableToIterateOn(serverInputItr);

            if (context.IsDebugMode())
            {
                AddDebugInputItem(ServerInputPath, "ServerInput Path", context.Environment, update);
            }

            while (colItr.HasMoreData())
            {
                try
                {
                    ExecuteConcreteAction(outputs, colItr, sharepointSource, serverInputItr);
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

        private void ExecuteConcreteAction(IList<OutputTO> outputs, WarewolfListIterator colItr, SharepointSource sharepointSource, WarewolfIterator serverInputItr)
        {
            var serverPath = colItr.FetchNextValue(serverInputItr);

            if (DataListUtil.IsValueRecordset(Result) && DataListUtil.GetRecordsetIndexType(Result) != enRecordsetIndexType.Numeric)
            {
                if (DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
                {
                    var recsetName = DataListUtil.ExtractRecordsetNameFromValue(Result);
                    var fieldName = DataListUtil.ExtractFieldNameFromValue(Result);

                    var result = Delete(sharepointSource, serverPath);

                    var indexToUpsertTo = 1;

                    foreach (var file in result)
                    {
                        var fullRecsetName = DataListUtil.CreateRecordsetDisplayValue(recsetName, fieldName,
                            indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
                        outputs.Add(DataListFactory.CreateOutputTO(DataListUtil.AddBracketsToValueIfNotExist(fullRecsetName), file));
                        indexToUpsertTo++;
                    }
                }
                else
                {
                    if (DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Blank)
                    {
                        AddBlankIndexDebugOutputs(outputs, sharepointSource, serverPath);
                    }
                }
            }
            else
            {
                var result = Delete(sharepointSource, serverPath);

                var xmlList = string.Join(",", result.Select(c => c));
                outputs.Add(DataListFactory.CreateOutputTO(Result));
                outputs.Last().OutputStrings.Add(xmlList);
            }
        }

        private void AddBlankIndexDebugOutputs(IList<OutputTO> outputs, SharepointSource sharepointSource, string serverPath)
        {
            var result = Delete(sharepointSource, serverPath);

            foreach (var folder in result)
            {
                outputs.Add(DataListFactory.CreateOutputTO(Result, folder));
            }
        }

        void ValidateRequest()
        {
            if (SharepointServerResourceId == Guid.Empty)
            {
                throw new ArgumentNullException(SharepointServerResourceId.ToString(), ErrorResource.InvalidSource);
            }

            if (string.IsNullOrEmpty(ServerInputPath))
            {
                throw new ArgumentNullException(ServerInputPath, ErrorResource.ServerInputPathEmpty);
            }
        }

        public IEnumerable<string> Delete(SharepointSource sharepointSource, string serverPath)
        {
            var sharepointHelper = sharepointSource.CreateSharepointHelper();

            var result = sharepointHelper.Delete(serverPath);

            return new List<string> { result };
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
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = Result
                 }
            };
        }

        public bool Equals(SharepointDeleteFileActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var isSourceEqual = CommonEqualityOps.AreObjectsEqual<IResource>(SharepointSource, other.SharepointSource);
            return base.Equals(other) 
                && string.Equals(ServerInputPath, other.ServerInputPath)
                && string.Equals(DisplayName, other.DisplayName)
                && isSourceEqual
                && SharepointServerResourceId.Equals(other.SharepointServerResourceId);
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

            return Equals((SharepointDeleteFileActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (ServerInputPath != null ? ServerInputPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SharepointSource != null ? SharepointSource.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ SharepointServerResourceId.GetHashCode();
                return hashCode;
            }
        }
    }
}
