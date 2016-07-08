using System;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;

namespace Dev2.Data.Util
{
    internal class CommonRecordSetUtil : ICommonRecordSetUtil
    {
        #region Implementation of ICommonRecordSetUtil

        public string ReplaceRecordBlankWithStar(string fullRecSetName)
        {
            var blankIndex = fullRecSetName.IndexOf("()", StringComparison.Ordinal);
            if (blankIndex != -1)
            {
                return fullRecSetName.Replace("()", $"({"*"})");
            }
            return fullRecSetName;
        }

        public string ReplaceRecordsetBlankWithStar(string fullRecSetName)
        {
            var blankIndex = fullRecSetName.IndexOf("().", StringComparison.Ordinal);
            if (blankIndex != -1)
            {
                return fullRecSetName.Replace("().", $"({"*"}).");
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
            return value.Replace("()", "");
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
            var inject = "()";

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
            else if (!result.EndsWith("()"))
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
            var index = ExtractIndexRegionFromRecordset(expression);

            if (string.IsNullOrEmpty(index))
            {
                return expression;
            }

            string extractIndexRegionFromRecordset = $"({index})";
            return string.IsNullOrEmpty(extractIndexRegionFromRecordset) ? expression :
                                        expression.Replace(extractIndexRegionFromRecordset, "()");
        }

        #endregion
    }
}