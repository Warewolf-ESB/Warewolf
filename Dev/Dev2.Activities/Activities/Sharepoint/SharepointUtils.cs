using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
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
            if (sharepointReadListTos == null)
            {
                return new List<SharepointReadListTo>();
            }
            return sharepointReadListTos.Where(to => !string.IsNullOrEmpty(to.VariableName));
        }

        public CamlQuery BuildCamlQuery(IExecutionEnvironment env, List<SharepointSearchTo> sharepointSearchTos, List<ISharepointFieldTo> fields, bool requireAllCriteriaToMatch = true)
        {
            var camlQuery = CamlQuery.CreateAllItemsQuery();
            var validFilters = new List<SharepointSearchTo>();
            if (sharepointSearchTos != null)
            {
                validFilters = sharepointSearchTos.Where(to => !string.IsNullOrEmpty(to.FieldName) && !string.IsNullOrEmpty(to.ValueToMatch)).ToList();
            }
            var filterCount = validFilters.Count;
            if (filterCount > 0)
            {
                var queryString = new StringBuilder("<View><Query><Where>");
                if (filterCount > 1)
                {
                    queryString.Append(requireAllCriteriaToMatch ? "<And>" : "<Or>");
                }
                foreach (var sharepointSearchTo in validFilters)
                {
                    var searchTo = sharepointSearchTo;
                    var sharepointFieldTo = fields.FirstOrDefault(to => to.InternalName == searchTo.InternalName);
                    var buildQueryFromTo = BuildQueryFromTo(sharepointSearchTo, env, sharepointFieldTo);
                    if (buildQueryFromTo != null)
                    {
                        queryString.AppendLine(string.Join(Environment.NewLine, buildQueryFromTo));
                    }
                }
                if (filterCount > 1)
                {
                    queryString.Append(requireAllCriteriaToMatch ? "</And>" : "</Or>");
                }
                queryString.Append("</Where></Query></View>");
                camlQuery.ViewXml = queryString.ToString();

            }
            return camlQuery;
        }

        IEnumerable<string> BuildQueryFromTo(SharepointSearchTo sharepointSearchTo, IExecutionEnvironment env, ISharepointFieldTo sharepointFieldTo)
        {
            WarewolfIterator iterator = new WarewolfIterator(env.Eval(sharepointSearchTo.ValueToMatch));
            while (iterator.HasMoreData())
            {
                var fieldType = sharepointFieldTo.GetFieldType();
                yield return string.Format("{0}<FieldRef Name=\"{1}\"></FieldRef><Value Type=\"{2}\">{3}</Value>{4}", SharepointSearchOptions.GetStartTagForSearchOption(sharepointSearchTo.SearchType), sharepointSearchTo.InternalName, fieldType, CastWarewolfValueToCorrectType(iterator.GetNextValue(), sharepointFieldTo.Type), SharepointSearchOptions.GetEndTagForSearchOption(sharepointSearchTo.SearchType));
            }
        }

        public object CastWarewolfValueToCorrectType(object value, SharepointFieldType type)
        {
            object returnValue = null;
            switch (type)
            {
                case SharepointFieldType.Boolean:
                    returnValue = Convert.ToBoolean(value);
                    break;
                case SharepointFieldType.Currency:
                    returnValue = Convert.ToDecimal(value, CultureInfo.CurrentCulture.NumberFormat);
                    break;
                case SharepointFieldType.DateTime:
                    returnValue = Convert.ToDateTime(value, CultureInfo.CurrentCulture.DateTimeFormat);
                    break;
                case SharepointFieldType.Integer:
                case SharepointFieldType.Number:
                    returnValue = Convert.ToInt32(value);
                    break;
                case SharepointFieldType.Text:
                case SharepointFieldType.Note:
                    returnValue = value.ToString();
                    break;
            }
            return returnValue;
        }
    }
}