using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.ServiceModel;
using Dev2.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.TO;
using Microsoft.SharePoint.Client;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;
using WarewolfParserInterop;

namespace Dev2.Activities.Sharepoint
{
    public class SharepointReadListActivity : DsfActivityAbstract<string>
    {

        public SharepointReadListActivity()
        {
            DisplayName = "Sharepoint Read List Item";
            ReadListItems = new List<ISharepointReadListTo>();
            FilterCriteria = new List<SharepointSearchTo>();
            RequireAllCriteriaToMatch = true;
        }

        public IList<ISharepointReadListTo> ReadListItems { get; set; }
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
        int _indexCounter = 1;
        protected override void ExecuteTool(IDSFDataObject dataObject)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            _indexCounter = 1;

            if(ReadListItems != null && ReadListItems.Count > 0)
            {
                var sharepointSource = ResourceCatalog.Instance.GetResource<SharepointSource>(dataObject.WorkspaceID, SharepointServerResourceId);
                var env = dataObject.Environment;
                if (dataObject.IsDebugMode())
                {
                    AddInputDebug(env);
                }
                using(var ctx = new ClientContext(sharepointSource.Server))
                {
                    ctx.Credentials = CredentialCache.DefaultNetworkCredentials;
                    List list = ctx.Web.Lists.GetByTitle(SharepointList);
                    var camlQuery = BuildCamlQuery();
                    var listItems = list.GetItems(camlQuery);
                    ctx.Load(list.Fields);
                    ctx.Load(listItems);
                    ctx.ExecuteQuery();
                    foreach(var listItem in listItems)
                    {
                        foreach(var sharepointReadListTo in ReadListItems)
                        {
                            var variableName = sharepointReadListTo.VariableName;
                            var fieldToName = sharepointReadListTo.FieldName;
                            var fieldName = list.Fields.FirstOrDefault(field => field.Title == fieldToName);
                            if(fieldName != null)
                            {
                                var listItemValue = listItem[fieldName.InternalName].ToString();
                                env.AssignWithFrame(new AssignValue(variableName, listItemValue));                                
                            }
                        }                        
                    }                    
                }
                env.CommitAssign();
                AddOutputDebug(dataObject, env);
            }
        }

        CamlQuery BuildCamlQuery()
        {
            var camlQuery = CamlQuery.CreateAllItemsQuery();
            if (FilterCriteria != null && FilterCriteria.Count > 0)
            {
                var filterCount = FilterCriteria.Count;
                var queryString = new StringBuilder("<View><Query><Where>");
                if(filterCount > 1)
                {
                    queryString.Append("<And>");
                }
                foreach(var sharepointSearchTo in FilterCriteria)
                {
                    var buildQueryFromTo = BuildQueryFromTo(sharepointSearchTo);
                    if(buildQueryFromTo != null)
                    {
                        queryString.AppendLine(buildQueryFromTo);
                    }
                }
                if (filterCount > 1)
                {
                    queryString.Append("</And>");
                }
                queryString.Append("</Where></Query></View>");
                camlQuery.ViewXml = queryString.ToString();
               
            }
            return camlQuery;
        }

        string BuildQueryFromTo(SharepointSearchTo sharepointSearchTo)
        {
            return string.Format("{0}<FieldRef Name=\"{1}\"></FieldRef><Value Type=\"Text\">{2}</Value>{3}", SharepointSearchOptions.GetStartTagForSearchOption(sharepointSearchTo.SearchType), sharepointSearchTo.FieldName, sharepointSearchTo.ValueToMatch, SharepointSearchOptions.GetEndTagForSearchOption(sharepointSearchTo.SearchType));
        }

        void AddOutputDebug(IDSFDataObject dataObject, IExecutionEnvironment env)
        {
            if(dataObject.IsDebugMode())
            {
                var outputIndex = 1;
                foreach(var varDebug in ReadListItems)
                {
                    var debugItem = new DebugItem();
                    AddDebugItem(new DebugItemStaticDataParams("", outputIndex.ToString(CultureInfo.InvariantCulture)), debugItem);
                    var variable = varDebug.VariableName.Replace("().", "(*).");
                    AddDebugItem(new DebugEvalResult(variable, "", env), debugItem);
                    _debugOutputs.Add(debugItem);
                    outputIndex++;
                }
            }
        }

        void AddInputDebug(IExecutionEnvironment env)
        {
            foreach(var varDebug in ReadListItems)
            {
                DebugItem debugItem = new DebugItem();
                AddDebugItem(new DebugItemStaticDataParams("", _indexCounter.ToString(CultureInfo.InvariantCulture)), debugItem);
                var variableName = varDebug.VariableName;
                if(!string.IsNullOrEmpty(variableName))
                {
                    AddDebugItem(new DebugEvalResult(variableName, "Variable", env), debugItem);
                    AddDebugItem(new DebugItemStaticDataParams(varDebug.FieldName, "Field Name"), debugItem);
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

       
    }
}