using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.ServiceModel;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.TO;
using Microsoft.SharePoint.Client;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Dev2.Util;

namespace Dev2.Activities.Sharepoint
{
    public class SharepointDeleteListItemActivity : DsfActivityAbstract<string>
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

        /// <summary>
        ///     Gets or sets the number of successful deletes.
        /// </summary>
        [Outputs("DeleteCount")]
        [FindMissing]
        public string DeleteCount { get; set; }
        public IList<SharepointReadListTo> ReadListItems { get; set; }
        public Guid SharepointServerResourceId { get; set; }
        public string SharepointList { get; set; }
        public List<SharepointSearchTo> FilterCriteria { get; set; }
        public bool RequireAllCriteriaToMatch { get; set; }

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

        readonly SharepointUtils _sharepointUtils;
        private int _indexCounter;

        protected override void ExecuteTool(IDSFDataObject dataObject)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            ErrorResultTO allErrors = new ErrorResultTO();
            try
            {
                var sharepointSource = ResourceCatalog.Instance.GetResource<SharepointSource>(dataObject.WorkspaceID, SharepointServerResourceId);
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
                ListItemCollection listItems;
                var sharepointHelper = sharepointSource.CreateSharepointHelper();
                var fields = sharepointHelper.LoadFieldsForList(SharepointList, true);
                using (var ctx = sharepointHelper.GetContext())
                {
                    var camlQuery = _sharepointUtils.BuildCamlQuery(env, FilterCriteria, fields, RequireAllCriteriaToMatch);
                    List list = sharepointHelper.LoadFieldsForList(SharepointList, ctx, false);
                    listItems = list.GetItems(camlQuery);
                    ctx.Load(listItems);
                    ctx.ExecuteQuery();
                }
                using (var ctx = sharepointHelper.GetContext())
                {
                    List list = ctx.Web.Lists.GetByTitle(SharepointList);
                    foreach (var item in listItems)
                        list.GetItemById(item.Id).DeleteObject();
                    list.Update();
                    ctx.ExecuteQuery();
                }
                var successfulDeleteCount = listItems.Count();
                dataObject.Environment.Assign(DeleteCount, successfulDeleteCount.ToString());
                env.CommitAssign();
                AddOutputDebug(dataObject);
            }
            catch (Exception e)
            {
                Dev2Logger.Log.Error("SharepointDeleteListItemActivity", e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    DisplayAndWriteError("SharepointDeleteListItemActivity", allErrors);
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

        void AddOutputDebug(IDSFDataObject dataObject)
        {
            if (dataObject.IsDebugMode())
            {
                AddDebugOutputItem(new DebugEvalResult(DeleteCount, string.Empty, dataObject.Environment));
            }
        }

        void AddInputDebug(IExecutionEnvironment env)
        {
            if (FilterCriteria != null && FilterCriteria.Any())
            {
                string requireAllCriteriaToMatch = RequireAllCriteriaToMatch ? "Yes" : "No";

                foreach (var varDebug in FilterCriteria)
                {
                    if(string.IsNullOrEmpty(varDebug.FieldName)) return;
                    DebugItem debugItem = new DebugItem();
                    AddDebugItem(new DebugItemStaticDataParams("", _indexCounter.ToString(CultureInfo.InvariantCulture)), debugItem);
                    var fieldName = varDebug.FieldName;
                    if (!string.IsNullOrEmpty(fieldName))
                    {
                        AddDebugItem(new DebugEvalResult(fieldName, "Field Name", env), debugItem);
                        //AddDebugItem(new DebugItemStaticDataParams(varDebug.FieldName, "Field Name"), debugItem);
                    }
                    var searchType = varDebug.SearchType;
                    if (!string.IsNullOrEmpty(searchType))
                    {
                        AddDebugItem(new DebugEvalResult(searchType, "Search Type", env), debugItem);
                    }
                    var valueToMatch = varDebug.ValueToMatch;
                    if (!string.IsNullOrEmpty(valueToMatch))
                    {
                        AddDebugItem(new DebugEvalResult(valueToMatch, "Value", env), debugItem);
                    }

                    AddDebugItem(new DebugEvalResult(requireAllCriteriaToMatch, "Require All Criteria To Match", env), debugItem);

                    _indexCounter++;
                    _debugInputs.Add(debugItem);
                }
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


    }
}
