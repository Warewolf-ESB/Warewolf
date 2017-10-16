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

        public IEnumerable<SharepointReadListTo> GetValidReadListItems(IList<SharepointReadListTo> sharepointReadListTos)
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
                    var buildQueryFromTo = BuildQueryFromTo(sharepointSearchTo, env, sharepointFieldTo,update);
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

        IEnumerable<string> BuildQueryFromTo(SharepointSearchTo sharepointSearchTo, IExecutionEnvironment env, ISharepointFieldTo sharepointFieldTo,int update)
        {
            var warewolfEvalResult = env.Eval(sharepointSearchTo.ValueToMatch, update);
            var fieldType = sharepointFieldTo.GetFieldType();
            if (sharepointSearchTo.SearchType == "In")
            {
                var startSearchTerm = string.Format("{0}<FieldRef Name=\"{1}\"></FieldRef>", SharepointSearchOptions.GetStartTagForSearchOption(sharepointSearchTo.SearchType), sharepointSearchTo.InternalName);
               
                startSearchTerm+="<Values>";
                if(warewolfEvalResult.IsWarewolfAtomListresult)
                {
                    if (warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult listResult)
                    {
                        foreach (var warewolfAtom in listResult.Item)
                        {
                            var valueString = warewolfAtom.ToString();
                            if (valueString.Contains(","))
                            {
                                var listOfValues = valueString.Split(',');
                                startSearchTerm = listOfValues.Select(listOfValue => CastWarewolfValueToCorrectType(listOfValue, sharepointFieldTo.Type)).Aggregate(startSearchTerm, (current, value) => current + string.Format("<Value Type=\"{0}\">{1}</Value>", fieldType, value));
                            }
                            else
                            {
                                var value = CastWarewolfValueToCorrectType(valueString, sharepointFieldTo.Type);
                                startSearchTerm += string.Format("<Value Type=\"{0}\">{1}</Value>", fieldType, value);
                            }
                        }
                    }
                }
                else
                {
                    if (warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult scalarResult)
                    {
                        var valueString = scalarResult.Item.ToString();
                        if (valueString.Contains(","))
                        {
                            var listOfValues = valueString.Split(',');
                            startSearchTerm = listOfValues.Select(listOfValue => CastWarewolfValueToCorrectType(listOfValue, sharepointFieldTo.Type)).Aggregate(startSearchTerm, (current, value) => current + string.Format("<Value Type=\"{0}\">{1}</Value>", fieldType, value));
                        }
                        else
                        {
                            var value = CastWarewolfValueToCorrectType(valueString, sharepointFieldTo.Type);
                            startSearchTerm += string.Format("<Value Type=\"{0}\">{1}</Value>", fieldType, value);
                        }
                    }
                }
                startSearchTerm += "</Values>";
                startSearchTerm += SharepointSearchOptions.GetEndTagForSearchOption(sharepointSearchTo.SearchType);
                yield return startSearchTerm;
            }
            else
            {
                
                WarewolfIterator iterator = new WarewolfIterator(warewolfEvalResult);
                while (iterator.HasMoreData())
                {
                    yield return string.Format("{0}<FieldRef Name=\"{1}\"></FieldRef><Value Type=\"{2}\">{3}</Value>{4}", SharepointSearchOptions.GetStartTagForSearchOption(sharepointSearchTo.SearchType), sharepointSearchTo.InternalName, fieldType, CastWarewolfValueToCorrectType(iterator.GetNextValue(), sharepointFieldTo.Type), SharepointSearchOptions.GetEndTagForSearchOption(sharepointSearchTo.SearchType));
                }
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
                    throw new ArgumentException("Unrecognized type: " + type);
            }
            return returnValue;
        }
    }
}