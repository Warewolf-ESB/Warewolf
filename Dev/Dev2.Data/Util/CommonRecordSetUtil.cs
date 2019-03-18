#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.DataList.Contract;

namespace Dev2.Data.Util
{
    class CommonRecordSetUtil : ICommonRecordSetUtil
    {
        const string EmptyBrackets = "()";

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
        public string ReplaceObjectBlankWithIndex(string fullRecSetName, int length)
        {
            var blankIndex = fullRecSetName.IndexOf("()", StringComparison.Ordinal);
            if (blankIndex != -1)
            {
                return fullRecSetName.Replace("()", $"({length})");
            }
            return fullRecSetName;
        }

        public string CreateRecordsetDisplayValue(string recsetName, string colName, string indexNum) => string.Concat(recsetName, DataListUtil.RecordsetIndexOpeningBracket, indexNum, ").", colName);

        public string RemoveRecordsetBracketsFromValue(string value) => value.Replace(EmptyBrackets, "");

        public enRecordsetIndexType GetRecordsetIndexType(string expression)
        {
            var result = enRecordsetIndexType.Error;

            var idx = ExtractIndexRegionFromRecordset(expression);
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
                if (Int32.TryParse(idx, out int convertIntTest))
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
            var result = string.Empty;

            var start = rs.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
            if (start > 0)
            {
                var end = rs.LastIndexOf(DataListUtil.RecordsetIndexClosingBracket, StringComparison.Ordinal);
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

            var result = DataListUtil.StripBracketsFromValue(value);

            if (result.EndsWith(DataListUtil.RecordsetIndexOpeningBracket))
            {
                result = string.Concat(result, DataListUtil.RecordsetIndexClosingBracket);
            }
            else if (result.EndsWith(DataListUtil.RecordsetIndexClosingBracket))
            {
                return result.Replace(DataListUtil.RecordsetIndexClosingBracket, inject);
            }
            else
            {
                if (!result.EndsWith(EmptyBrackets))
                {
                    result = string.Concat(result, inject);
                }
            }
            return result;
        }

        public string ExtractFieldNameOnlyFromValue(string value)
        {
            var result = string.Empty;
            var dotIdx = value.LastIndexOf(".", StringComparison.Ordinal);
            var closeIdx = value.Contains("]]") ? value.LastIndexOf("]]", StringComparison.Ordinal) : value.Length;
            if (dotIdx > 0)
            {
                result = value.Substring(dotIdx + 1, closeIdx - dotIdx - 1);
            }

            return result;
        }

        public string ExtractFieldNameFromValue(string value)
        {
            var result = string.Empty;
            value = DataListUtil.StripBracketsFromValue(value);
            var dotIdx = value.LastIndexOf(".", StringComparison.Ordinal);
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
            var result = string.Empty;

            var openBracket = value.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
            if (openBracket > 0)
            {
                result = value.Substring(0, openBracket);
            }

            return result;
        }

        public bool IsValueRecordsetWithFields(string value) => !string.IsNullOrEmpty(value) && value.Contains(").");

        public bool IsValueRecordset(string value)
        {
            var result = false;

            if (!string.IsNullOrEmpty(value) && value.Contains(DataListUtil.RecordsetIndexOpeningBracket) && value.Contains(DataListUtil.RecordsetIndexClosingBracket))
            {
                result = true;
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

            var extractIndexRegionFromRecordset = $"({index})";
            return string.IsNullOrEmpty(extractIndexRegionFromRecordset) ? expression :
                                        expression.Replace(extractIndexRegionFromRecordset, "(*)");
        }

        public string ReplaceRecordsetIndexWithBlank(string expression)
        {
            var firstOpenBracket = expression.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
            var firstCloseBracket = expression.IndexOf(DataListUtil.RecordsetIndexClosingBracket, StringComparison.Ordinal);            
            if (firstOpenBracket > firstCloseBracket)
            {
                return EmptyBrackets;
            }

            var index = ExtractIndexRegionFromRecordset(expression);

            if (string.IsNullOrEmpty(index))
            {
                return expression;
            }

            var extractIndexRegionFromRecordset = $"({index})";
            return string.IsNullOrEmpty(extractIndexRegionFromRecordset) ? expression :
                                        expression.Replace(extractIndexRegionFromRecordset, EmptyBrackets);
        }

        public string RemoveRecordSetBraces(string search, ref bool isRs)
        {
            if (search.Contains(DataListUtil.RecordsetIndexOpeningBracket))
            {
                isRs = true;
                var pos = search.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
                search = search.Substring(0, search.Length - (search.Length - pos));
            }
            return search;
        }

        public void ProcessRecordSetFields(IParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, IDev2DataLanguageIntellisensePart t1)
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

        public void ProcessNonRecordsetFields(IParseTO payload, IList<IIntellisenseResult> result, IDev2DataLanguageIntellisensePart t1)
        {
            if (payload.Parent != null && payload.Parent.Payload.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal) >= 0)
            {
                var part = IntellisenseFactory.CreateDataListValidationScalarPart(t1.Name, t1.Description + " / Use row at this index");

                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
            }
            else
            {
                IDataListVerifyPart part;
                part = t1.Name.Contains('(') && t1.Name.Contains(')') ? IntellisenseFactory.CreateDataListValidationRecordsetPart(string.Empty, t1.Name, true) : IntellisenseFactory.CreateDataListValidationScalarPart(t1.Name, t1.Description);

                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
            }
        }

        public void ProcessRecordSetMatch(IParseTO payload, IList<IIntellisenseResult> result, string rawSearch, string search, IDev2DataLanguageIntellisensePart t1)
        {
            // only process if it is an open region
            // we need to add all children
            string idx;
            idx = !payload.IsLeaf && !payload.Child.HangingOpen ? DataListUtil.OpeningSquareBrackets + payload.Child.Payload + DataListUtil.ClosingSquareBrackets : DataListUtil.ExtractIndexRegionFromRecordset(rawSearch);
            // add general closed recordset
            var rsName = search;
            if (idx == string.Empty)
            {
                rsName = payload.Payload;
            }
            var part = IntellisenseFactory.CreateDataListValidationRecordsetPart(rsName, "", t1.Description + " / Select a specific row", idx);
            result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));

            var children = t1.Children;
            if (children != null)
            {
                foreach (IDev2DataLanguageIntellisensePart t in children)
                {
                    part = IntellisenseFactory.CreateDataListValidationRecordsetPart(rsName, t.Name, t.Description + " / Select a specific field at a specific row", idx);
                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                }
            }
        }

        public bool AddRecordSetIndex(IParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, string[] parts, IDev2DataLanguageIntellisensePart t1, bool emptyOk)
        {
            if (addCompleteParts)
            {
                var idx = DataListUtil.ExtractIndexRegionFromRecordset(parts[0]);
                var recset = DataListUtil.ExtractRecordsetNameFromValue(parts[0]);

                var p = IntellisenseFactory.CreateDataListValidationRecordsetPart(recset, string.Empty, t1.Description, payload.Child != null ? payload.Child.Payload : idx);

                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, p, p.Description));
            }
            else
            {
                emptyOk = true;
            }
            return emptyOk;
        }

        public bool RecordsetMatch(IParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, string rawSearch, string search, bool emptyOk, string[] parts, IDev2DataLanguageIntellisensePart t1)
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

        public void OpenRecordsetItem(IParseTO payload, IList<IIntellisenseResult> result, IDev2DataLanguageIntellisensePart t1)
        {
            if (payload.Child != null)
            {
                var indx = payload.Child.Payload;
                var end = indx.IndexOf(DataListUtil.RecordsetIndexClosingBracket, StringComparison.Ordinal);
                if (end > 0)
                {
                    // malformed index -- correct it
                    indx = indx.Substring(0, end);
                }

                indx = DataListUtil.AddBracketsToValueIfNotExist(indx);

                var rs = payload.Payload;
                end = rs.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
                if (end > 0)
                {
                    rs = rs.Substring(0, end);
                }

                var prt = IntellisenseFactory.CreateDataListValidationRecordsetPart(rs, "", " / Select a specific row", indx);

                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, prt, prt.Description));

                // now add all fields to collection too ;)
                if (t1.Children != null)
                {
                    var cParts = t1.Children;
                    foreach (IDev2DataLanguageIntellisensePart t in cParts)
                    {
                        prt = IntellisenseFactory.CreateDataListValidationRecordsetPart(rs, t.Name, " / Select a specific row", indx);
                        result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, prt, prt.Description));
                    }
                }
            }
        }
    }
}