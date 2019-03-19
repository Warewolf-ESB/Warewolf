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
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities.Sharepoint
{
    [ToolDescriptorInfo("SharepointLogo", "Update List Item(s)", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Sharepoint", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_SharePoint_Update_List_Item")]
    public class SharepointUpdateListItemActivity : DsfActivityAbstract<string>,IEquatable<SharepointUpdateListItemActivity>
    {
        readonly SharepointUtils _sharepointUtils;

        public SharepointUpdateListItemActivity()
        {
            DisplayName = "Sharepoint Update List Item";
            ReadListItems = new List<SharepointReadListTo>();
            FilterCriteria = new List<SharepointSearchTo>();
            RequireAllCriteriaToMatch = true;
            _sharepointUtils = new SharepointUtils();
        }

        public List<SharepointSearchTo> FilterCriteria { get; set; }
        public bool RequireAllCriteriaToMatch { get; set; }
        [FindMissing]
        public new string Result { get; set; }
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
                    Name="SharepointList",
                    Type = StateVariable.StateType.Input,
                    Value = SharepointList
                },
                  new StateVariable
                {
                    Name="RequireAllCriteriaToMatch",
                    Type = StateVariable.StateType.Input,
                    Value = RequireAllCriteriaToMatch.ToString()
                },
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
            ExecuteTool(dataObject,0);
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
                ExecuteConcreteAction(dataObject, update);
            }
            catch (Exception e)
            {
                Dev2Logger.Error("SharepointUpdateListItemActivity", e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }
            finally
            {
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    dataObject.Environment.Assign(Result, "Failed", update);
                    DisplayAndWriteError("SharepointUpdateListItemActivity", allErrors);
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
                    var camlQuery = _sharepointUtils.BuildCamlQuery(env, FilterCriteria, fields, update, RequireAllCriteriaToMatch);
                    var list = ctx.Web.Lists.GetByTitle(SharepointList);
                    var listItems = list.GetItems(camlQuery);
                    ctx.Load(listItems);
                    ctx.ExecuteQuery();
                    var iteratorList = new WarewolfListIterator();
                    foreach (var sharepointReadListTo in sharepointReadListTos)
                    {
                        var warewolfIterator = new WarewolfIterator(env.Eval(sharepointReadListTo.VariableName, update));
                        iteratorList.AddVariableToIterateOn(warewolfIterator);
                        listOfIterators.Add(sharepointReadListTo.InternalName, warewolfIterator);
                    }
                    foreach (var listItem in listItems)
                    {

                        foreach (var warewolfIterator in listOfIterators)
                        {
                            listItem[warewolfIterator.Key] = warewolfIterator.Value.GetNextValue();
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
        }

        void AddOutputDebug(IDSFDataObject dataObject, IExecutionEnvironment env, int update)
        {
            if (dataObject.IsDebugMode())
            {
                var debugItem = new DebugItem();
                AddDebugItem(new DebugEvalResult(Result, "", env, update), debugItem);
                _debugOutputs.Add(debugItem);
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
                    AddDebugItem(new DebugItemStaticDataParams(varDebug.FieldName, "Field Name"), debugItem);
                    AddDebugItem(new DebugEvalResult(variableName, "Variable", env, update), debugItem);
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

        public Guid SharepointServerResourceId { get; set; }
        public string SharepointList { get; set; }
        public List<SharepointReadListTo> ReadListItems { get; set; }

        public bool Equals(SharepointUpdateListItemActivity other)
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
                && FilterCriteria.SequenceEqual( other.FilterCriteria, new SharepointSearchToComparer())
                && RequireAllCriteriaToMatch == other.RequireAllCriteriaToMatch 
                && string.Equals(Result, other.Result) 
                && SharepointServerResourceId.Equals(other.SharepointServerResourceId) 
                && string.Equals(SharepointList, other.SharepointList)
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

            return Equals((SharepointUpdateListItemActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_sharepointUtils != null ? _sharepointUtils.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _indexCounter;
                hashCode = (hashCode * 397) ^ (FilterCriteria != null ? FilterCriteria.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ RequireAllCriteriaToMatch.GetHashCode();
                hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ SharepointServerResourceId.GetHashCode();
                hashCode = (hashCode * 397) ^ (SharepointList != null ? SharepointList.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ReadListItems != null ? ReadListItems.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}