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
using Dev2.Data.ServiceModel;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.TO;
using Microsoft.SharePoint.Client;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Storage;
using WarewolfParserInterop;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dev2.Activities.Sharepoint
{
    [ToolDescriptorInfo("SharepointLogo", "Read List Item(s)", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Sharepoint", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_SharePoint_Read_List_Item")]
    public class SharepointReadListActivity : DsfActivityAbstract<string>
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

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject,0);
        }


        public override List<string> GetOutputs()
        {
            return ReadListItems.Select(to => to.VariableName).ToList();
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

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.MixedActivity;
        }

        int _indexCounter = 1;

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            _indexCounter = 1;
            ErrorResultTO allErrors = new ErrorResultTO();
            try
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
                        List list = ctx.Web.Lists.GetByTitle(SharepointList);
                        var listItems = list.GetItems(camlQuery);
                        ctx.Load(listItems);
                        ctx.ExecuteQuery();
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
                                        Dev2Logger.Error(e);
                                        //Ignore sharepoint exception on retrieval not all fields can be retrieved.
                                    }
                                    var correctedVariable = variableName;
                                    if (DataListUtil.IsValueRecordset(variableName) && DataListUtil.IsStarIndex(variableName))
                                    {
                                        correctedVariable = DataListUtil.ReplaceStarWithFixedIndex(variableName, index);
                                    }
                                    env.AssignWithFrame(new AssignValue(correctedVariable, listItemValue), update);
                                }
                            }
                            index++;
                        }
                    }
                    env.CommitAssign();
                    AddOutputDebug(dataObject, env, update);
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("SharepointReadListActivity", e);
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

        object GetSharepointValue(object sharepointValue)
        {
            var type = sharepointValue.GetType();
            var val = sharepointValue;
            if(type == typeof(FieldUserValue))
            {
                var fieldValue = sharepointValue as FieldUserValue;
                if(fieldValue != null)
                {
                    return fieldValue.LookupValue;
                }
            }
            else if(type == typeof(FieldLookupValue))
            {
                var fieldValue = sharepointValue as FieldLookupValue;
                if (fieldValue != null)
                {
                    return fieldValue.LookupValue;
                }
            }
            else if (type == typeof(FieldUrlValue))
            {
                var fieldValue = sharepointValue as FieldUrlValue;
                if (fieldValue != null)
                {
                    return fieldValue.Url;
                }
            }
            else if (type == typeof(FieldGeolocationValue))
            {
                var fieldValue = sharepointValue as FieldGeolocationValue;
                if (fieldValue != null)
                {
                    return string.Join(",",fieldValue.Longitude,fieldValue.Latitude,fieldValue.Altitude,fieldValue.Measure);
                }
            }
            else if (type == typeof(FieldLookupValue[]))
            {
                var fieldValue = sharepointValue as FieldLookupValue[];
                if (fieldValue != null)
                {
                    var returnString = string.Join(",",fieldValue.Select(value => value.LookupValue));
                    return returnString;
                }
            }
            else if (type == typeof(FieldUserValue[]))
            {
                var fieldValue = sharepointValue as FieldLookupValue[];
                if (fieldValue != null)
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

        void AddInputDebug(IExecutionEnvironment env, int update)
        {
            var validItems = SharepointUtils.GetValidReadListItems(ReadListItems).ToList();
            foreach (var varDebug in validItems)
            {
                DebugItem debugItem = new DebugItem();
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
                string requireAllCriteriaToMatch = RequireAllCriteriaToMatch ? "Yes" : "No";

                foreach (var varDebug in FilterCriteria)
                {
                    if (string.IsNullOrEmpty(varDebug.FieldName)) return;
                    DebugItem debugItem = new DebugItem();
                    AddDebugItem(new DebugItemStaticDataParams("", _indexCounter.ToString(CultureInfo.InvariantCulture)), debugItem);
                    var fieldName = varDebug.FieldName;
                    if (!string.IsNullOrEmpty(fieldName))
                    {
                        AddDebugItem(new DebugEvalResult(fieldName, "Field Name", env, update), debugItem);
                        //AddDebugItem(new DebugItemStaticDataParams(varDebug.FieldName, "Field Name"), debugItem);
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