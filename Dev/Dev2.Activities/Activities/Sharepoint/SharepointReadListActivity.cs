#pragma warning disable
using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Communication;
using Dev2.Comparer;
using Dev2.Data.ServiceModel;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.TO;
using Dev2.Utilities;
using Microsoft.SharePoint.Client;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;

namespace Dev2.Activities.Sharepoint
{
    [ToolDescriptorInfo("SharepointLogo", "Read List Item(s)", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Sharepoint", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_SharePoint_Read_List_Item")]
    public class SharepointReadListActivity : DsfActivityAbstract<string>, IEquatable<SharepointReadListActivity>
    {
        public SharepointReadListActivity()
        {
            DisplayName = "Sharepoint Read List Item";
            ReadListItems = new List<SharepointReadListTo>();
            FilterCriteria = new List<SharepointSearchTo>();
            RequireAllCriteriaToMatch = true;
            SharepointUtils = new SharepointUtils();
        }

        public IList<SharepointReadListTo> ReadListItems { get; set; }
        public Guid SharepointServerResourceId { get; set; }
        public string SharepointList { get; set; }
        public List<SharepointSearchTo> FilterCriteria { get; set; }
        public bool RequireAllCriteriaToMatch { get; set; }
        public SharepointUtils SharepointUtils { get; set; }
        public override IEnumerable<StateVariable> GetState()
        {
            var serializer = new Dev2JsonSerializer();
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
                    Type = StateVariable.StateType.InputOutput,
                    Value =  ActivityHelper.GetSerializedStateValueFromCollection(ReadListItems)
                 },
                new StateVariable
                {
                    Name="FilterCriteria",
                    Type = StateVariable.StateType.Input,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(FilterCriteria)
                },
                new StateVariable
                {
                    Name="RequireAllCriteriaToMatch",
                    Type = StateVariable.StateType.Input,
                    Value = RequireAllCriteriaToMatch.ToString()
                },
                new StateVariable
                {
                    Name="SharepointList",
                    Type = StateVariable.StateType.Input,
                    Value = SharepointList
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

        public override List<string> GetOutputs() => ReadListItems.Select(to => to.VariableName).ToList();

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
                ExecuteConcreteAction(dataObject, update);
            }
            catch (Exception e)
            {
                Dev2Logger.Error("SharepointReadListActivity", e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }
            finally
            {
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    DisplayAndWriteError("SharepointReadListActivity", allErrors);
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

        private void ExecuteConcreteAction(IDSFDataObject dataObject, int update)
        {
            var sharepointReadListTos = SharepointUtils.GetValidReadListItems(ReadListItems).ToList();
            if (sharepointReadListTos.Any())
            {
                var sharepointSource = ResourceCatalog.GetResource<SharepointSource>(dataObject.WorkspaceID, SharepointServerResourceId);
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
                var fields = sharepointHelper.LoadFieldsForList(SharepointList, false);
                using (var ctx = sharepointHelper.GetContext())
                {
                    var camlQuery = SharepointUtils.BuildCamlQuery(env, FilterCriteria, fields, update);
                    var list = ctx.Web.Lists.GetByTitle(SharepointList);
                    var listItems = list.GetItems(camlQuery);
                    ctx.Load(listItems);
                    ctx.ExecuteQuery();
                    AddItemList(update, sharepointReadListTos, env, fields, listItems);
                }
                env.CommitAssign();
                AddOutputDebug(dataObject, env, update);
            }
        }

        private void AddItemList(int update, List<SharepointReadListTo> sharepointReadListTos, IExecutionEnvironment env, List<Common.Interfaces.Infrastructure.SharedModels.ISharepointFieldTo> fields, ListItemCollection listItems)
        {
            var index = 1;
            foreach (var listItem in listItems)
            {
                foreach (var sharepointReadListTo in sharepointReadListTos)
                {
                    var variableName = sharepointReadListTo.VariableName;
                    var fieldToName = sharepointReadListTo.FieldName;
                    var fieldName = fields.FirstOrDefault(field => field.Name == fieldToName);
                    if (fieldName != null)
                    {
                        TryAddField(update, env, index, listItem, variableName, fieldName);
                    }
                }
                index++;
            }
        }

        private void TryAddField(int update, IExecutionEnvironment env, int index, ListItem listItem, string variableName, Common.Interfaces.Infrastructure.SharedModels.ISharepointFieldTo fieldName)
        {
            var listItemValue = "";
            try
            {
                var sharepointValue = listItem[fieldName.InternalName];

                if (sharepointValue != null)
                {
                    var sharepointVal = GetSharepointValue(sharepointValue);
                    listItemValue = sharepointVal.ToString();
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                //Ignore sharepoint exception on retrieval not all fields can be retrieved.
            }
            var correctedVariable = variableName;
            if (DataListUtil.IsValueRecordset(variableName) && DataListUtil.IsStarIndex(variableName))
            {
                correctedVariable = DataListUtil.ReplaceStarWithFixedIndex(variableName, index);
            }
            env.AssignWithFrame(new AssignValue(correctedVariable, listItemValue), update);
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        object GetSharepointValue(object sharepointValue)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            var type = sharepointValue.GetType();
            var val = sharepointValue;
            if (type == typeof(FieldUserValue))
            {
                if (sharepointValue is FieldUserValue fieldValue)
                {
                    return fieldValue.LookupValue;
                }
            }
            else if (type == typeof(FieldLookupValue))
            {
                if (sharepointValue is FieldLookupValue fieldValue)
                {
                    return fieldValue.LookupValue;
                }
            }
            else if (type == typeof(FieldUrlValue))
            {
                if (sharepointValue is FieldUrlValue fieldValue)
                {
                    return fieldValue.Url;
                }
            }
            else if (type == typeof(FieldGeolocationValue))
            {
                if (sharepointValue is FieldGeolocationValue fieldValue)
                {
                    return string.Join(",", fieldValue.Longitude, fieldValue.Latitude, fieldValue.Altitude, fieldValue.Measure);
                }
            }
            else if (type == typeof(FieldLookupValue[]))
            {
                if (sharepointValue is FieldLookupValue[] fieldValue)
                {
                    var returnString = string.Join(",", fieldValue.Select(value => value.LookupValue));
                    return returnString;
                }
            }
            else
            {
                if (type == typeof(FieldUserValue[]) && sharepointValue is FieldLookupValue[] fieldValue)
                {
                    var returnString = string.Join(",", fieldValue.Select(value => value.LookupValue));
                    return returnString;
                }

            }
            return val;
        }

        void AddOutputDebug(IDSFDataObject dataObject, IExecutionEnvironment env, int update)
        {
            if (dataObject.IsDebugMode())
            {
                var outputIndex = 1;
                var validItems = SharepointUtils.GetValidReadListItems(ReadListItems).ToList();
                foreach (var varDebug in validItems)
                {
                    var debugItem = new DebugItem();
                    AddDebugItem(new DebugItemStaticDataParams("", outputIndex.ToString(CultureInfo.InvariantCulture)), debugItem);
                    var variable = varDebug.VariableName.Replace("().", "(*).");
                    AddDebugItem(new DebugEvalResult(variable, "", env, update), debugItem);
                    _debugOutputs.Add(debugItem);
                    outputIndex++;
                }
            }
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        void AddInputDebug(IExecutionEnvironment env, int update)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            var validItems = SharepointUtils.GetValidReadListItems(ReadListItems).ToList();
            foreach (var varDebug in validItems)
            {
                var debugItem = new DebugItem();
                AddDebugItem(new DebugItemStaticDataParams("", _indexCounter.ToString(CultureInfo.InvariantCulture)), debugItem);
                var variableName = varDebug.VariableName;
                if (!string.IsNullOrEmpty(variableName))
                {
                    AddDebugItem(new DebugEvalResult(variableName, "Variable", env, update), debugItem);
                    AddDebugItem(new DebugItemStaticDataParams(varDebug.FieldName, "Field Name"), debugItem);
                }
                _indexCounter++;
                _debugInputs.Add(debugItem);
            }
            if (FilterCriteria != null && FilterCriteria.Any())
            {
                var requireAllCriteriaToMatch = RequireAllCriteriaToMatch ? "Yes" : "No";

                foreach (var varDebug in FilterCriteria)
                {
                    if (string.IsNullOrEmpty(varDebug.FieldName))
                    {
                        return;
                    }

                    var debugItem = new DebugItem();
                    AddDebugItem(new DebugItemStaticDataParams("", _indexCounter.ToString(CultureInfo.InvariantCulture)), debugItem);
                    var fieldName = varDebug.FieldName;
                    if (!string.IsNullOrEmpty(fieldName))
                    {
                        AddDebugItem(new DebugEvalResult(fieldName, "Field Name", env, update), debugItem);

                    }
                    var searchType = varDebug.SearchType;
                    if (!string.IsNullOrEmpty(searchType))
                    {
                        AddDebugItem(new DebugEvalResult(searchType, "Search Type", env, update), debugItem);
                    }
                    var valueToMatch = varDebug.ValueToMatch;
                    if (!string.IsNullOrEmpty(valueToMatch))
                    {
                        AddDebugItem(new DebugEvalResult(valueToMatch, "Value", env, update), debugItem);
                    }

                    AddDebugItem(new DebugEvalResult(requireAllCriteriaToMatch, "Require All Criteria To Match", env, update), debugItem);

                    _indexCounter++;
                    _debugInputs.Add(debugItem);
                }
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

        public bool Equals(SharepointReadListActivity other)
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
                   && ReadListItems.SequenceEqual(other.ReadListItems, new SharepointReadListToComparer())
                   && SharepointServerResourceId.Equals(other.SharepointServerResourceId)
                   && string.Equals(SharepointList, other.SharepointList)
                   && FilterCriteria.SequenceEqual(other.FilterCriteria, new SharepointSearchToComparer())
                   && RequireAllCriteriaToMatch == other.RequireAllCriteriaToMatch;
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

            return Equals((SharepointReadListActivity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (ReadListItems != null ? ReadListItems.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ SharepointServerResourceId.GetHashCode();
                hashCode = (hashCode * 397) ^ (SharepointList != null ? SharepointList.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FilterCriteria != null ? FilterCriteria.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ RequireAllCriteriaToMatch.GetHashCode();
                return hashCode;
            }
        }
    }
}