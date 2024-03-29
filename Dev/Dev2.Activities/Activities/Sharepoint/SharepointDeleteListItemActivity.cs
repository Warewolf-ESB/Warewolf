#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Comparer;
using Dev2.Data.ServiceModel;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.TO;
using Dev2.Util;
using Microsoft.SharePoint.Client;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Storage.Interfaces;
using Dev2.Utilities;


namespace Dev2.Activities.Sharepoint
{
    [ToolDescriptorInfo("SharepointLogo", "Delete List Item(s)", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Sharepoint", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_SharePoint_Delete_List_Item")]
    public class SharepointDeleteListItemActivity : DsfActivityAbstract<string>, IEquatable<SharepointDeleteListItemActivity>
    {

        public SharepointDeleteListItemActivity()
        {
            DisplayName = "Sharepoint Delete List Item";
            FilterCriteria = new List<SharepointSearchTo>();
            ReadListItems = new List<SharepointReadListTo>();
            RequireAllCriteriaToMatch = true;
            _sharepointUtils = new SharepointUtils();
            _indexCounter = 1;
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
                    Name="FilterCriteria",
                    Type = StateVariable.StateType.Input,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(FilterCriteria)
                 },
                new StateVariable
                {
                    Name="ReadListItems",
                    Type = StateVariable.StateType.Input,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(ReadListItems)
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
                 }
                 ,new StateVariable
                {
                    Name="DeleteCount",
                    Type = StateVariable.StateType.Output,
                    Value = DeleteCount
                 }
            };
        }
        [Outputs("DeleteCount")]
        [FindMissing]
        public string DeleteCount { get; set; }
        public IList<SharepointReadListTo> ReadListItems { get; set; }
        public Guid SharepointServerResourceId { get; set; }
        public string SharepointList { get; set; }
        public List<SharepointSearchTo> FilterCriteria { get; set; }
        public bool RequireAllCriteriaToMatch { get; set; }

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        public override List<string> GetOutputs() => new List<string> { DeleteCount };

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
        }

        public override IList<DsfForEachItem> GetForEachInputs() => null;

        public override IList<DsfForEachItem> GetForEachOutputs() => null;

        public override enFindMissingType GetFindMissingType() => enFindMissingType.MixedActivity;

        readonly SharepointUtils _sharepointUtils;
        int _indexCounter;

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            var allErrors = new ErrorResultTO();
            try
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
                ListItemCollection listItems;
                var sharepointHelper = sharepointSource.CreateSharepointHelper();
                var fields = sharepointHelper.LoadFieldsForList(SharepointList, true);
                using (var ctx = sharepointHelper.GetContext())
                {
                    var camlQuery = _sharepointUtils.BuildCamlQuery(env, FilterCriteria, fields, update, RequireAllCriteriaToMatch);
                    var list = sharepointHelper.LoadFieldsForList(SharepointList, ctx, false);
                    listItems = list.GetItems(camlQuery);
                    ctx.Load(listItems);
                    ctx.ExecuteQuery();
                }
                using (var ctx = sharepointHelper.GetContext())
                {
                    var list = ctx.Web.Lists.GetByTitle(SharepointList);
                    foreach (var item in listItems)
                    {
                        list.GetItemById(item.Id).DeleteObject();
                    }

                    list.Update();
                    ctx.ExecuteQuery();
                }
                var successfulDeleteCount = listItems.Count();
                if (!string.IsNullOrWhiteSpace(DeleteCount))
                {
                    dataObject.Environment.Assign(DeleteCount, successfulDeleteCount.ToString(), update);
                    env.CommitAssign();
                    AddOutputDebug(dataObject, update);
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("SharepointDeleteListItemActivity", e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }
            finally
            {
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                    DisplayAndWriteError(dataObject,DisplayName, allErrors);
                }
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        void AddOutputDebug(IDSFDataObject dataObject, int update)
        {
            if (dataObject.IsDebugMode())
            {
                AddDebugOutputItem(new DebugEvalResult(DeleteCount, string.Empty, dataObject.Environment, update));
            }
        }

        void AddInputDebug(IExecutionEnvironment env, int update)
        {
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


        public bool Equals(SharepointDeleteListItemActivity other)
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
                && string.Equals(DeleteCount, other.DeleteCount)
                && ReadListItems.SequenceEqual(other.ReadListItems, new SharepointReadListToComparer())
                && SharepointServerResourceId.Equals(other.SharepointServerResourceId)
                && string.Equals(SharepointList, other.SharepointList)
                && string.Equals(DisplayName, other.DisplayName)
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

            return Equals((SharepointDeleteListItemActivity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (DeleteCount != null ? DeleteCount.GetHashCode() : 0);
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
