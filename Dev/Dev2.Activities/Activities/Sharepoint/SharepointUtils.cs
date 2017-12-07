using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Data;
using Dev2.TO;
using Microsoft.SharePoint.Client;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities.Sharepoint
{
    public static class SharepointUtils
    {

        public static IEnumerable<SharepointReadListTo> GetValidReadListItems(IList<SharepointReadListTo> sharepointReadListTos)
        {
            if (sharepointReadListTos == null)
            {
                return new List<SharepointReadListTo>();
            }
            return sharepointReadListTos.Where(to => !string.IsNullOrEmpty(to.VariableName));
        }

        public static CamlQuery BuildCamlQuery(IExecutionEnvironment env, List<SharepointSearchTo> sharepointSearchTos, List<ISharepointFieldTo> fields, int update) => BuildCamlQuery(env, sharepointSearchTos, fields, update, true);

        public static CamlQuery BuildCamlQuery(IExecutionEnvironment env, List<SharepointSearchTo> sharepointSearchTos, List<ISharepointFieldTo> fields, int update, bool requireAllCriteriaToMatch)
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
                BuildQueryString(env, fields, update, requireAllCriteriaToMatch, camlQuery, validFilters, filterCount);

            }
            return camlQuery;
        }

        static void BuildQueryString(IExecutionEnvironment env, List<ISharepointFieldTo> fields, int update, bool requireAllCriteriaToMatch, CamlQuery camlQuery, List<SharepointSearchTo> validFilters, int filterCount)
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
                var buildQueryFromTo = BuildQueryFromTo(sharepointSearchTo, env, sharepointFieldTo, update);
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

        static IEnumerable<string> BuildQueryFromTo(SharepointSearchTo sharepointSearchTo, IExecutionEnvironment env, ISharepointFieldTo sharepointFieldTo,int update)
        {
            var warewolfEvalResult = env.Eval(sharepointSearchTo.ValueToMatch, update);
            var fieldType = sharepointFieldTo.GetFieldType();
            if (sharepointSearchTo.SearchType == "In")
            {
                var startSearchTerm = $"{SharepointSearchOptions.GetStartTagForSearchOption(sharepointSearchTo.SearchType)}<FieldRef Name=\"{sharepointSearchTo.InternalName}\"></FieldRef>";

                startSearchTerm+="<Values>";
                if(warewolfEvalResult.IsWarewolfAtomListresult)
                {
                    if (warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult listResult)
                    {
                        startSearchTerm = GetSearchTermFromListResult(sharepointFieldTo, fieldType, startSearchTerm, listResult);
                    }
                }
                else
                {
                    if (warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult scalarResult)
                    {
                        startSearchTerm = GetStartSearchTermFromScalarResult(sharepointFieldTo, fieldType, startSearchTerm, scalarResult);
                    }
                }
                startSearchTerm += "</Values>";
                startSearchTerm += SharepointSearchOptions.GetEndTagForSearchOption(sharepointSearchTo.SearchType);
                yield return startSearchTerm;
            }
            else
            {

                var iterator = new WarewolfIterator(warewolfEvalResult);
                while (iterator.HasMoreData())
                {
                    yield return $"{SharepointSearchOptions.GetStartTagForSearchOption(sharepointSearchTo.SearchType)}<FieldRef Name=\"{sharepointSearchTo.InternalName}\"></FieldRef><Value Type=\"{fieldType}\">{CastWarewolfValueToCorrectType(iterator.GetNextValue(), sharepointFieldTo.Type)}</Value>{SharepointSearchOptions.GetEndTagForSearchOption(sharepointSearchTo.SearchType)}";
                }
            }
        }

        static string GetStartSearchTermFromScalarResult(ISharepointFieldTo sharepointFieldTo, string fieldType, string searchTerm, CommonFunctions.WarewolfEvalResult.WarewolfAtomResult scalarResult)
        {
            var startSearchTerm = searchTerm;
            var valueString = scalarResult.Item.ToString();
            if (valueString.Contains(","))
            {
                var listOfValues = valueString.Split(',');
                startSearchTerm = listOfValues.Select(listOfValue => CastWarewolfValueToCorrectType(listOfValue, sharepointFieldTo.Type)).Aggregate(startSearchTerm, (current, value) => current + $"<Value Type=\"{fieldType}\">{value}</Value>");
            }
            else
            {
                var value = CastWarewolfValueToCorrectType(valueString, sharepointFieldTo.Type);
                startSearchTerm += $"<Value Type=\"{fieldType}\">{value}</Value>";
            }

            return startSearchTerm;
        }

        static string GetSearchTermFromListResult(ISharepointFieldTo sharepointFieldTo, string fieldType, string searchTerm, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult listResult)
        {
            var startSearchTerm = searchTerm;
            var builder = new StringBuilder();
            builder.Append(startSearchTerm);
            foreach (var warewolfAtom in listResult.Item)
            {
                var valueString = warewolfAtom.ToString();
                if (valueString.Contains(","))
                {
                    var listOfValues = valueString.Split(',');
                    startSearchTerm = listOfValues
                                      .Select(listOfValue => CastWarewolfValueToCorrectType(listOfValue, sharepointFieldTo.Type))
                                      .Aggregate(startSearchTerm, (current, value) => current + $"<Value Type=\"{fieldType}\">{value}</Value>");
                }
                else
                {
                    var value = CastWarewolfValueToCorrectType(valueString, sharepointFieldTo.Type);
                    builder.Append($"<Value Type=\"{fieldType}\">{value}</Value>");
                }
            }
            startSearchTerm = builder.ToString();
            return startSearchTerm;
        }

        public static object CastWarewolfValueToCorrectType(object value, SharepointFieldType type)
        {
            object returnValue = null;
            switch (type)
            {
                case SharepointFieldType.Boolean:
                    returnValue = Convert.ToBoolean(value);
                    break;
                case SharepointFieldType.Number:
                case SharepointFieldType.Currency:
                    returnValue = Convert.ToDecimal(value, CultureInfo.InvariantCulture.NumberFormat);
                    break;
                case SharepointFieldType.DateTime:
                    returnValue = Convert.ToDateTime(value, CultureInfo.InvariantCulture);
                    break;
                case SharepointFieldType.Integer:
                    returnValue = Convert.ToInt32(value);
                    break;
                case SharepointFieldType.Text:
                case SharepointFieldType.Note:
                    returnValue = value.ToString();
                    break;
                default:
                    break;
            }
            return returnValue;
        }
    }
}