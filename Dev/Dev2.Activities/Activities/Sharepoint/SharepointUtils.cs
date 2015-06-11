using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Data;
using Dev2.TO;
using Microsoft.SharePoint.Client;
using Warewolf.Storage;

namespace Dev2.Activities.Sharepoint
{
    public class SharepointUtils
    {
        
        public IEnumerable<SharepointReadListTo> GetValidReadListItems(IList<SharepointReadListTo> sharepointReadListTos)
        {
            if(sharepointReadListTos == null)
            {
                return new List<SharepointReadListTo>();
            }
            return sharepointReadListTos.Where(to => !string.IsNullOrEmpty(to.VariableName));
        }

        public CamlQuery BuildCamlQuery(IExecutionEnvironment env, List<SharepointSearchTo> sharepointSearchTos)
        {
            var camlQuery = CamlQuery.CreateAllItemsQuery();
            var validFilters = new List<SharepointSearchTo>();
            if(sharepointSearchTos != null)
            {
                validFilters = sharepointSearchTos.Where(to => !string.IsNullOrEmpty(to.FieldName) && !string.IsNullOrEmpty(to.ValueToMatch)).ToList();
            }
            var filterCount = validFilters.Count;
            if (filterCount > 0)
            {
                var queryString = new StringBuilder("<View><Query><Where>");
                if(filterCount > 1)
                {
                    queryString.Append("<And>");
                }
                foreach (var sharepointSearchTo in validFilters)
                {
                    var buildQueryFromTo = BuildQueryFromTo(sharepointSearchTo, env);
                    if(buildQueryFromTo != null)
                    {
                        queryString.AppendLine(string.Join(Environment.NewLine,buildQueryFromTo));
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

        public IEnumerable<string> BuildQueryFromTo(SharepointSearchTo sharepointSearchTo, IExecutionEnvironment env)
        {
            WarewolfIterator iterator = new WarewolfIterator(env.Eval(sharepointSearchTo.ValueToMatch));
            while(iterator.HasMoreData())
            {
                yield return string.Format("{0}<FieldRef Name=\"{1}\"></FieldRef><Value Type=\"Text\">{2}</Value>{3}", SharepointSearchOptions.GetStartTagForSearchOption(sharepointSearchTo.SearchType), sharepointSearchTo.FieldName, iterator.GetNextValue(), SharepointSearchOptions.GetEndTagForSearchOption(sharepointSearchTo.SearchType));    
            }
        }
    }
}