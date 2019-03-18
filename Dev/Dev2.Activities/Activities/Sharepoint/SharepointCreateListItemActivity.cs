#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Communication;
using Dev2.Comparer;
using Dev2.Data;
using Dev2.Data.ServiceModel;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.TO;
using Dev2.Util;
using Microsoft.SharePoint.Client;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities.Sharepoint
{
    [ToolDescriptorInfo("SharepointLogo", "Create List Item(s)", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Sharepoint", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_SharePoint_Create_List_Item")]
    public class SharepointCreateListItemActivity : DsfActivityAbstract<string>, IEquatable<SharepointCreateListItemActivity>
    {
        readonly SharepointUtils _sharepointUtils;

        public SharepointCreateListItemActivity()
        {
            DisplayName = "Sharepoint Create List Item";
            ReadListItems = new List<SharepointReadListTo>();
            _sharepointUtils = new SharepointUtils();
        }

        [FindMissing]

        public new string Result { get; set; }
        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        public override List<string> GetOutputs() => new List<string> { Result };

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

        public override enFindMissingType GetFindMissingType() => enFindMissingType.MixedActivity;

        int _indexCounter = 1;

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            _indexCounter = 1;
            var allErrors = new ErrorResultTO();
            try
            {
                var sharepointReadListTos = SharepointUtils.GetValidReadListItems(ReadListItems).ToList();
                if (sharepointReadListTos.Any())
                {
                    TryExecute(dataObject, update, sharepointReadListTos);
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("SharepointCreateListItemActivity", e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }
            finally
            {
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    dataObject.Environment.Assign(Result, "Failed", update);
                    DisplayAndWriteError("SharepointCreateListItemActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                }
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        private void TryExecute(IDSFDataObject dataObject, int update, List<SharepointReadListTo> sharepointReadListTos)
        {
            var sharepointSource = ResourceCatalog.GetResource<SharepointSource>(dataObject.WorkspaceID, SharepointServerResourceId);
            var listOfIterators = new Dictionary<string, IWarewolfIterator>();
            if (sharepointSource == null)
            {
                var contents = ResourceCatalog.GetResourceContents(dataObject.WorkspaceID, SharepointServerResourceId);
                sharepointSource = new SharepointSource(contents.ToXElement());
            }
            var env = dataObject.Environment;
            if (dataObject.IsDebugMode())
            {
                AddInputDebug(env, update);
            }
            var sharepointHelper = sharepointSource.CreateSharepointHelper();
            var fields = sharepointHelper.LoadFieldsForList(SharepointList, true);
            using (var ctx = sharepointHelper.GetContext())
            {
                var list = sharepointHelper.LoadFieldsForList(SharepointList, ctx, true);
                var iteratorList = new WarewolfListIterator();
                foreach (var sharepointReadListTo in sharepointReadListTos)
                {
                    var warewolfIterator = new WarewolfIterator(env.Eval(sharepointReadListTo.VariableName, update));
                    iteratorList.AddVariableToIterateOn(warewolfIterator);
                    listOfIterators.Add(sharepointReadListTo.FieldName, warewolfIterator);
                }
                while (iteratorList.HasMoreData())
                {
                    var itemCreateInfo = new ListItemCreationInformation();
                    var listItem = list.AddItem(itemCreateInfo);
                    foreach (var warewolfIterator in listOfIterators)
                    {
                        var sharepointFieldTo = fields.FirstOrDefault(to => to.Name == warewolfIterator.Key);
                        if (sharepointFieldTo != null)
                        {
                            object value = warewolfIterator.Value.GetNextValue();
                            value = SharepointUtils.CastWarewolfValueToCorrectType(value, sharepointFieldTo.Type);
                            listItem[sharepointFieldTo.InternalName] = value;
                        }
                    }
                    listItem.Update();
                    ctx.ExecuteQuery();
                }
            }
            if (!string.IsNullOrEmpty(Result))
            {
                env.Assign(Result, "Success", update);
                AddOutputDebug(dataObject, env, update);
            }
        }

        void AddOutputDebug(IDSFDataObject dataObject, IExecutionEnvironment env, int update)
        {
            if (dataObject.IsDebugMode() && !string.IsNullOrEmpty(Result))
            {
                var debugItem = new DebugItem();
                AddDebugItem(new DebugEvalResult(Result, "", env, update), debugItem);
                _debugOutputs.Add(debugItem);
            }
        }

        void AddInputDebug(IExecutionEnvironment env, int update)
        {
            var validItems = SharepointUtils.GetValidReadListItems(ReadListItems).ToList();
            foreach (var varDebug in validItems)
            {
                var debugItem = new DebugItem();
                AddDebugItem(new DebugItemStaticDataParams("", _indexCounter.ToString(CultureInfo.InvariantCulture)), debugItem);
                var variableName = varDebug.VariableName;
                if (!string.IsNullOrEmpty(variableName))
                {
                    AddDebugItem(new DebugItemStaticDataParams(varDebug.FieldName, "Field Name"), debugItem);
                    AddDebugItem(new DebugEvalResult(variableName, "Variable", env, update), debugItem);
                }
                _indexCounter++;
                _debugInputs.Add(debugItem);
            }
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

        public Guid SharepointServerResourceId { get; set; }
        public string SharepointList { get; set; }
        public List<SharepointReadListTo> ReadListItems { get; set; }

        public override IEnumerable<StateVariable> GetState()
        {
            var serializer = new Dev2JsonSerializer();
            var readListItems = serializer.Serialize(ReadListItems);
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
                    Name="ReadListItems",
                    Type = StateVariable.StateType.Input,
                    Value = readListItems
                },
                  new StateVariable
                {
                    Name="SharepointList",
                    Type = StateVariable.StateType.Input,
                    Value = SharepointList
                },
                  new StateVariable
                {
                    Name="UniqueID",
                    Type = StateVariable.StateType.Input,
                    Value = UniqueID
                },
                 new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = Result
                 }
            };
        }

        public bool Equals(SharepointCreateListItemActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other)
                   && string.Equals(Result, other.Result)
                   && SharepointServerResourceId.Equals(other.SharepointServerResourceId)
                   && string.Equals(SharepointList, other.SharepointList)
                   && string.Equals(DisplayName, other.DisplayName)
                   && ReadListItems.SequenceEqual(other.ReadListItems, new SharepointReadListToComparer());
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

            return Equals((SharepointCreateListItemActivity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ SharepointServerResourceId.GetHashCode();
                hashCode = (hashCode * 397) ^ (SharepointList != null ? SharepointList.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ReadListItems != null ? ReadListItems.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}