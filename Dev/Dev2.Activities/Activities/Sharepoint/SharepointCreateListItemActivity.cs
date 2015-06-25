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
using Dev2.Data;
using Dev2.Data.ServiceModel;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.TO;
using Dev2.Util;
using Microsoft.SharePoint.Client;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

namespace Dev2.Activities.Sharepoint
{
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
        public new string Result { get; set; }
        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject);
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

        protected override void ExecuteTool(IDSFDataObject dataObject)
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
                    var sharepointSource = ResourceCatalog.Instance.GetResource<SharepointSource>(dataObject.WorkspaceID, SharepointServerResourceId);
                    Dictionary<string, IWarewolfIterator> listOfIterators = new Dictionary<string, IWarewolfIterator>();
                    if (sharepointSource == null)
                    {
                        var contents = ResourceCatalog.Instance.GetResourceContents(dataObject.WorkspaceID, SharepointServerResourceId);
                        sharepointSource = new SharepointSource(contents.ToXElement());
                    }
                    var env = dataObject.Environment;
                    if (dataObject.IsDebugMode())
                    {
                        AddInputDebug(env);
                    }
                    var sharepointHelper = sharepointSource.CreateSharepointHelper();
                    var fields = sharepointHelper.LoadFieldsForList(SharepointList, true);
                    using (var ctx = sharepointHelper.GetContext())
                    {
                        var list = sharepointHelper.LoadFieldsForList(SharepointList, ctx, true);
                        var iteratorList = new WarewolfListIterator();
                        foreach(var sharepointReadListTo in sharepointReadListTos)
                        {
                            var warewolfIterator = new WarewolfIterator(env.Eval(sharepointReadListTo.VariableName));
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
                    env.Assign(Result,"Success");
                    AddOutputDebug(dataObject, env);
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Log.Error("SharepointReadListActivity", e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    dataObject.Environment.Assign(Result, "Failed");
                    DisplayAndWriteError("SharepointReadListActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                }
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before);
                    DispatchDebugState(dataObject, StateType.After);
                }
            }
        }

        void AddOutputDebug(IDSFDataObject dataObject, IExecutionEnvironment env)
        {
            if(dataObject.IsDebugMode() && !string.IsNullOrEmpty(Result))
            {
                var debugItem = new DebugItem();
                AddDebugItem(new DebugEvalResult(Result, "", env), debugItem);
                _debugOutputs.Add(debugItem);
            }
        }

        void AddInputDebug(IExecutionEnvironment env)
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
                    AddDebugItem(new DebugEvalResult(variableName, "Variable", env), debugItem);
                }
                _indexCounter++;
                _debugInputs.Add(debugItem);
            }
        }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList)
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