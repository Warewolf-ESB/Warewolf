using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Data.Interfaces;
using Dev2.Data.Parsers;
using Dev2.Data.TO;
using Dev2.DataList.Contract;

namespace Dev2.Data.Util
{
    internal class CommonRecordSetUtil : ICommonRecordSetUtil
    {
        private Dev2DataLanguageParser _dev2DataLanguageParser;
        const string EmptyBrackets = "()";
        public CommonRecordSetUtil()
        {

        }
        public CommonRecordSetUtil(Dev2DataLanguageParser dev2DataLanguageParser)
        {
            _dev2DataLanguageParser = dev2DataLanguageParser;
        }
        #region Implementation of ICommonRecordSetUtil

        public string ReplaceRecordBlankWithStar(string fullRecSetName)
        {
            var blankIndex = fullRecSetName.IndexOf(EmptyBrackets, StringComparison.Ordinal);
            if (blankIndex != -1)
            {
                return fullRecSetName.Replace(EmptyBrackets, "(*)");
            }
            return fullRecSetName;
        }

        public string ReplaceRecordsetBlankWithStar(string fullRecSetName)
        {
            var blankIndex = fullRecSetName.IndexOf("().", StringComparison.Ordinal);
            if (blankIndex != -1)
            {
                return fullRecSetName.Replace("().", "(*).");
            }
            return fullRecSetName;
        }

        public string ReplaceRecordsetBlankWithIndex(string fullRecSetName, int length)
        {
            var blankIndex = fullRecSetName.IndexOf("().", StringComparison.Ordinal);
            if (blankIndex != -1)
            {
                return fullRecSetName.Replace("().", $"({length}).");
            }
            return fullRecSetName;
        }

        public string CreateRecordsetDisplayValue(string recsetName, string colName, string indexNum)
        {
            return string.Concat(recsetName, DataListUtil.RecordsetIndexOpeningBracket, indexNum, ").", colName);
        }

        public string RemoveRecordsetBracketsFromValue(string value)
        {
            return value.Replace(EmptyBrackets, "");
        }

        public enRecordsetIndexType GetRecordsetIndexType(string expression)
        {
            enRecordsetIndexType result = enRecordsetIndexType.Error;

            string idx = ExtractIndexRegionFromRecordset(expression);
            if (idx == "*")
            {
                result = enRecordsetIndexType.Star;
            }
            else if (string.IsNullOrEmpty(idx))
            {
                result = enRecordsetIndexType.Blank;
            }
            else
            {
                int convertIntTest;
                if (Int32.TryParse(idx, out convertIntTest))
                {
                    result = enRecordsetIndexType.Numeric;
                }
            }

            return result;
        }

        public bool IsStarIndex(string rs)
        {
            if (string.IsNullOrEmpty(rs))
            {
                return false;
            }

            return ExtractIndexRegionFromRecordset(rs) == "*";
        }

        public string ExtractIndexRegionFromRecordset(string rs)
        {
            string result = string.Empty;

            int start = rs.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
            if (start > 0)
            {
                int end = rs.LastIndexOf(DataListUtil.RecordsetIndexClosingBracket, StringComparison.Ordinal);
                if (end < 0)
                {
                    end = rs.Length;
                }

                start += 1;
                result = rs.Substring(start, end - start);
            }

            return result;
        }

        public string MakeValueIntoHighLevelRecordset(string value, bool starNotation)
        {
            var inject = EmptyBrackets;

            if (starNotation)
            {
                inject = "(*)";
            }

            string result = DataListUtil.StripBracketsFromValue(value);

            if (result.EndsWith(DataListUtil.RecordsetIndexOpeningBracket))
            {
                result = string.Concat(result, DataListUtil.RecordsetIndexClosingBracket);
            }
            else if (result.EndsWith(DataListUtil.RecordsetIndexClosingBracket))
            {
                return result.Replace(DataListUtil.RecordsetIndexClosingBracket, inject);
            }
            else if (!result.EndsWith(EmptyBrackets))
            {
                result = string.Concat(result, inject);
            }
            return result;
        }

        public string ExtractFieldNameOnlyFromValue(string value)
        {
            string result = string.Empty;
            int dotIdx = value.LastIndexOf(".", StringComparison.Ordinal);
            int closeIdx = value.Contains("]]") ? value.LastIndexOf("]]", StringComparison.Ordinal) : value.Length;
            if (dotIdx > 0)
            {
                result = value.Substring(dotIdx + 1, closeIdx - dotIdx - 1);
            }

            return result;
        }

        public string ExtractFieldNameFromValue(string value)
        {
            string result = string.Empty;
            value = DataListUtil.StripBracketsFromValue(value);
            int dotIdx = value.LastIndexOf(".", StringComparison.Ordinal);
            if (dotIdx > 0)
            {
                result = value.Substring(dotIdx + 1);
            }

            return result;
        }

        public string ExtractRecordsetNameFromValue(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            value = DataListUtil.StripBracketsFromValue(value);
            string result = string.Empty;

            int openBracket = value.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
            if (openBracket > 0)
            {
                result = value.Substring(0, openBracket);
            }

            return result;
        }

        public bool IsValueRecordsetWithFields(string value)
        {
            return !string.IsNullOrEmpty(value) && value.Contains(").");
        }

        public bool IsValueRecordset(string value)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(value))
            {
                if (value.Contains(DataListUtil.RecordsetIndexOpeningBracket) && value.Contains(DataListUtil.RecordsetIndexClosingBracket))
                {
                    result = true;
                }
            }

            return result;
        }

        public string ReplaceRecordsetIndexWithStar(string expression)
        {
            var index = ExtractIndexRegionFromRecordset(expression);

            if (string.IsNullOrEmpty(index))
            {
                return expression;
            }

            string extractIndexRegionFromRecordset = $"({index})";
            return string.IsNullOrEmpty(extractIndexRegionFromRecordset) ? expression :
                                        expression.Replace(extractIndexRegionFromRecordset, "(*)");
        }

        public string ReplaceRecordsetIndexWithBlank(string expression)
        {
            var firstOpenBracket = expression.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
            var firstCloseBracket = expression.IndexOf(DataListUtil.RecordsetIndexClosingBracket, StringComparison.Ordinal);            
            if (firstOpenBracket > firstCloseBracket)
                return EmptyBrackets;
            var index = ExtractIndexRegionFromRecordset(expression);

            if (string.IsNullOrEmpty(index))
                return expression;

            string extractIndexRegionFromRecordset = $"({index})";
            return string.IsNullOrEmpty(extractIndexRegionFromRecordset) ? expression :
                                        expression.Replace(extractIndexRegionFromRecordset, EmptyBrackets);
        }

        public string RemoveRecordSetBraces(string search, ref bool isRs)
        {
            if (search.Contains(DataListUtil.RecordsetIndexOpeningBracket))
            {
                isRs = true;
                int pos = search.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
                search = search.Substring(0, search.Length - (search.Length - pos));
            }
            return search;
        }

        public void ProcessRecordSetFields(ParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, IDev2DataLanguageIntellisensePart t1)
        {
            IDataListVerifyPart part;

            // only add hanging open if we want incomplete parts
            if (!addCompleteParts)
            {
                part = IntellisenseFactory.CreateDataListValidationScalarPart(t1.Name + DataListUtil.RecordsetIndexOpeningBracket, t1.Description + " / Select a specific row or Close");

                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
            }

            part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t1.Name, "", t1.Description + " / Takes all rows ", "*");
            result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));

            part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t1.Name, "", t1.Description + " / Take last row");

            result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
            // add all children for them
            foreach (IDev2DataLanguageIntellisensePart t in t1.Children)
            {
                part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t1.Name, t.Name, t1.Description + " / Use the field of a Recordset");
                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
            }
        }

        public void ProcessNonRecordsetFields(ParseTO payload, IList<IIntellisenseResult> result, IDev2DataLanguageIntellisensePart t1)
        {
            if (payload.Parent != null && payload.Parent.Payload.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal) >= 0)
            {
                IDataListVerifyPart part = IntellisenseFactory.CreateDataListValidationScalarPart(t1.Name, t1.Description + " / Use row at this index");

                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
            }
            else
            {
                IDataListVerifyPart part;
                if (t1.Name.Contains('(') && t1.Name.Contains(')'))
                {
                    part = IntellisenseFactory.CreateDataListValidationRecordsetPart(string.Empty, t1.Name, true);
                }
                else
                {
                    part = IntellisenseFactory.CreateDataListValidationScalarPart(t1.Name, t1.Description);
                }

                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
            }
        }

        public void ProcessRecordSetMatch(ParseTO payload, IList<IIntellisenseResult> result, string rawSearch, string search, IDev2DataLanguageIntellisensePart t1)
        {
            // only process if it is an open region
            // we need to add all children
            string idx;
            if (!payload.IsLeaf && !payload.Child.HangingOpen)
            {
                idx = DataListUtil.OpeningSquareBrackets + payload.Child.Payload + DataListUtil.ClosingSquareBrackets;
            }
            else
            {
                // we need to extract the index
                idx = DataListUtil.ExtractIndexRegionFromRecordset(rawSearch);
            }
            // add general closed recordset
            string rsName = search;
            if (idx == string.Empty)
            {
                rsName = payload.Payload;
            }
            IDataListVerifyPart part = IntellisenseFactory.CreateDataListValidationRecordsetPart(rsName, "", t1.Description + " / Select a specific row", idx);
            result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));

            IList<IDev2DataLanguageIntellisensePart> children = t1.Children;
            if (children != null)
            {
                foreach (IDev2DataLanguageIntellisensePart t in children)
                {
                    part = IntellisenseFactory.CreateDataListValidationRecordsetPart(rsName, t.Name, t.Description + " / Select a specific field at a specific row", idx);
                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                }
            }
        }

        public bool AddRecordSetIndex(ParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, string[] parts, IDev2DataLanguageIntellisensePart t1, bool emptyOk)
        {
            if (addCompleteParts)
            {
                string idx = DataListUtil.ExtractIndexRegionFromRecordset(parts[0]);
                string recset = DataListUtil.ExtractRecordsetNameFromValue(parts[0]);

                IDataListVerifyPart p = IntellisenseFactory.CreateDataListValidationRecordsetPart(recset, string.Empty, t1.Description, payload.Child != null ? payload.Child.Payload : idx);

                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, p, p.Description));
            }
            else
            {
                emptyOk = true;
            }
            return emptyOk;
        }

        public bool RecordsetMatch(ParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, string rawSearch, string search, bool emptyOk, string[] parts, IDev2DataLanguageIntellisensePart t1)
        {
            if (payload.HangingOpen)
            {
                ProcessRecordSetMatch(payload, result, rawSearch, search, t1);
            }
            else
            {
                // add in recordset with index if around
                emptyOk = AddRecordSetIndex(payload, addCompleteParts, result, parts, t1, emptyOk);
            }
            return emptyOk;
        }

        public void OpenRecordsetItem(ParseTO payload, IList<IIntellisenseResult> result, IDev2DataLanguageIntellisensePart t1)
        {
            if (payload.Child != null)
            {
                string indx = payload.Child.Payload;
                int end = indx.IndexOf(DataListUtil.RecordsetIndexClosingBracket, StringComparison.Ordinal);
                if (end > 0)
                {
                    // malformed index -- correct it
                    indx = indx.Substring(0, end);
                }

                indx = DataListUtil.AddBracketsToValueIfNotExist(indx);

                string rs = payload.Payload;
                end = rs.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
                if (end > 0)
                {
                    rs = rs.Substring(0, end);
                }

                IDataListVerifyPart prt = IntellisenseFactory.CreateDataListValidationRecordsetPart(rs, "", " / Select a specific row", indx);

                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, prt, prt.Description));

                // now add all fields to collection too ;)
                if (t1.Children != null)
                {
                    IList<IDev2DataLanguageIntellisensePart> cParts = t1.Children;
                    foreach (IDev2DataLanguageIntellisensePart t in cParts)
                    {
                        prt = IntellisenseFactory.CreateDataListValidationRecordsetPart(rs, t.Name, " / Select a specific row", indx);
                        result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, prt, prt.Description));
                    }
                }
            }
        }

        #endregion
    }
}