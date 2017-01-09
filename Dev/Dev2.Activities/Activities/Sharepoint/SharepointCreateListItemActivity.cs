using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
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
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dev2.Activities.Sharepoint
{
    [ToolDescriptorInfo("SharepointLogo", "Create List Item(s)", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Sharepoint", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_SharePoint_Create_List_Item")]
    public class SharepointCreateListItemActivity : DsfActivityAbstract<string>
    {
        readonly SharepointUtils _sharepointUtils;

        public SharepointCreateListItemActivity()
        {
            DisplayName = "Sharepoint Create List Item";
            ReadListItems = new List<SharepointReadListTo>();
            _sharepointUtils = new SharepointUtils();
        }

        [FindMissing]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public new string Result { get; set; }
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
            return new List<string> { Result };
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
                var sharepointReadListTos = _sharepointUtils.GetValidReadListItems(ReadListItems).ToList();
                if (sharepointReadListTos.Any())
                {
                    var sharepointSource = ResourceCatalog.GetResource<SharepointSource>(dataObject.WorkspaceID, SharepointServerResourceId);
                    Dictionary<string, IWarewolfIterator> listOfIterators = new Dictionary<string, IWarewolfIterator>();
                    if (sharepointSource == null)
                    {
                        var contents = ResourceCatalog.GetResourceContents(dataObject.WorkspaceID, SharepointServerResourceId);
                        sharepointSource = new SharepointSource(contents.ToXElement());
                    }
                    var env = dataObject.Environment;
                    if (dataObject.IsDebugMode())
                    {
                        AddInputDebug(env,update);
                    }
                    var sharepointHelper = sharepointSource.CreateSharepointHelper();
                    var fields = sharepointHelper.LoadFieldsForList(SharepointList, true);
                    using (var ctx = sharepointHelper.GetContext())
                    {
                        var list = sharepointHelper.LoadFieldsForList(SharepointList, ctx, true);
                        var iteratorList = new WarewolfListIterator();
                        foreach(var sharepointReadListTo in sharepointReadListTos)
                        {
                            var warewolfIterator = new WarewolfIterator(env.Eval(sharepointReadListTo.VariableName,update));
                            iteratorList.AddVariableToIterateOn(warewolfIterator);
                            listOfIterators.Add(sharepointReadListTo.FieldName,warewolfIterator);
                        }
                        while(iteratorList.HasMoreData())
                        {
                            var itemCreateInfo = new ListItemCreationInformation();
                            var listItem = list.AddItem(itemCreateInfo);
                            foreach(var warewolfIterator in listOfIterators)
                            {
                                var sharepointFieldTo = fields.FirstOrDefault(to => to.Name == warewolfIterator.Key);
                                if(sharepointFieldTo != null)
                                {
                                    object value = warewolfIterator.Value.GetNextValue();
                                    value = _sharepointUtils.CastWarewolfValueToCorrectType(value, sharepointFieldTo.Type);
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
            }
            catch (Exception e)
            {
                Dev2Logger.Error("SharepointCreateListItemActivity", e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    dataObject.Environment.Assign(Result, "Failed",update);
                    DisplayAndWriteError("SharepointCreateListItemActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                }
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before,update);
                    DispatchDebugState(dataObject, StateType.After,update);
                }
            }
        }

        void AddOutputDebug(IDSFDataObject dataObject, IExecutionEnvironment env,int update)
        {
            if(dataObject.IsDebugMode() && !string.IsNullOrEmpty(Result))
            {
                var debugItem = new DebugItem();
                AddDebugItem(new DebugEvalResult(Result, "", env,update), debugItem);
                _debugOutputs.Add(debugItem);
            }
        }

        void AddInputDebug(IExecutionEnvironment env,int update)
        {
            var validItems = _sharepointUtils.GetValidReadListItems(ReadListItems).ToList();
            foreach (var varDebug in validItems)
            {
                DebugItem debugItem = new DebugItem();
                AddDebugItem(new DebugItemStaticDataParams("", _indexCounter.ToString(CultureInfo.InvariantCulture)), debugItem);
                var variableName = varDebug.VariableName;
                if (!string.IsNullOrEmpty(variableName))
                {
                    AddDebugItem(new DebugItemStaticDataParams(varDebug.FieldName, "Field Name"), debugItem);
                    AddDebugItem(new DebugEvalResult(variableName, "Variable", env,update), debugItem);
                }
                _indexCounter++;
                _debugInputs.Add(debugItem);
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

        public Guid SharepointServerResourceId { get; set; }
        public string SharepointList { get; set; }
        public List<SharepointReadListTo> ReadListItems { get; set; }
    }
}