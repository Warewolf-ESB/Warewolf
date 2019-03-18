#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
using Dev2.Common;

namespace Dev2.Activities.Sharepoint
{
    public class SharepointUtils
    {
        public static IEnumerable<SharepointReadListTo> GetValidReadListItems(IList<SharepointReadListTo> sharepointReadListTos)
        {
            if (sharepointReadListTos == null)
            {
                return new List<SharepointReadListTo>();
            }
            return sharepointReadListTos.Where(to => !string.IsNullOrEmpty(to.VariableName));
        }

        public CamlQuery BuildCamlQuery(IExecutionEnvironment env, List<SharepointSearchTo> sharepointSearchTos, List<ISharepointFieldTo> fields, int update) => BuildCamlQuery(env, sharepointSearchTos, fields, update, true);

        public CamlQuery BuildCamlQuery(IExecutionEnvironment env, List<SharepointSearchTo> sharepointSearchTos, List<ISharepointFieldTo> fields, int update, bool requireAllCriteriaToMatch)
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
            return camlQuery;
        }

        IEnumerable<string> BuildQueryFromTo(SharepointSearchTo sharepointSearchTo, IExecutionEnvironment env, ISharepointFieldTo sharepointFieldTo, int update)
        {
            var warewolfEvalResult = env.Eval(sharepointSearchTo.ValueToMatch, update);
            var fieldType = sharepointFieldTo.GetFieldType();
            if (sharepointSearchTo.SearchType == "In")
            {
                var startSearchTerm = $"{SharepointSearchOptions.GetStartTagForSearchOption(sharepointSearchTo.SearchType)}<FieldRef Name=\"{sharepointSearchTo.InternalName}\"></FieldRef>";

                startSearchTerm += "<Values>";
                if (warewolfEvalResult.IsWarewolfAtomListresult)
                {
                    startSearchTerm = AddAtomListResult(sharepointFieldTo, warewolfEvalResult, fieldType, startSearchTerm);
                }
                else
                {
                    startSearchTerm = AddAtomResult(sharepointFieldTo, warewolfEvalResult, fieldType, startSearchTerm);
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

        private static string AddAtomResult(ISharepointFieldTo sharepointFieldTo, CommonFunctions.WarewolfEvalResult warewolfEvalResult, string fieldType, string startSearchTerm)
        {
            if (warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult scalarResult)
            {
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
            }

            return startSearchTerm;
        }

        private static string AddAtomListResult(ISharepointFieldTo sharepointFieldTo, CommonFunctions.WarewolfEvalResult warewolfEvalResult, string fieldType, string startSearchTerm)
        {
            if (warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult listResult)
            {
                foreach (var warewolfAtom in listResult.Item)
                {
                    var valueString = warewolfAtom.ToString();
                    if (valueString.Contains(","))
                    {
                        var listOfValues = valueString.Split(',');
                        startSearchTerm = listOfValues.Select(listOfValue => CastWarewolfValueToCorrectType(listOfValue, sharepointFieldTo.Type)).Aggregate(startSearchTerm, (current, value) => current + $"<Value Type=\"{fieldType}\">{value}</Value>");
                    }
                    else
                    {
                        var value = CastWarewolfValueToCorrectType(valueString, sharepointFieldTo.Type);
                        startSearchTerm += string.Format("<Value Type=\"{0}\">{1}</Value>", fieldType, value);
                    }
                }
            }

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
                    Dev2Logger.Info("No Cast type for the Sharepoint Property Name: " + type, GlobalConstants.WarewolfInfo);
                    break;
            }
            return returnValue;
        }
    }
}