#pragma warning disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Dev2.Common.ExtMethods;
using Dev2.Data.Exceptions;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Warewolf.Resource.Errors;

namespace Dev2.Data.Util
{
    class ParserHelperUtil : IParserHelper
    {
        #region Implementation of IParserHelper

        public bool ProcessOpenRegion(string payload, bool openRegion, int i, ref IParseTO currentNode, ref StringBuilder region, ref char cur)
        {
            if (openRegion)
            {
                openRegion = CloseNode(currentNode, i, region);

                currentNode = ProcessNode(payload, currentNode, ref region, ref openRegion);
                cur = '\0';
            }
            else
            {
                throw new Dev2DataLanguageParseError(ErrorResource.InvalidOpenRegion, 0, payload.Length, enIntellisenseErrorCode.SyntaxError);
            }
            return openRegion;
        }

        bool CloseNode(IParseTO currentNode, int i, StringBuilder region)
        {
            const bool OpenRegion = false;
            currentNode.EndIndex = i;
            currentNode.HangingOpen = false;
            currentNode.Payload = region.ToString();
            region.Clear();
            currentNode.EndIndex = i - 2;
            return OpenRegion;
        }

        public IParseTO CurrentNode(IParseTO currentNode, StringBuilder region, int i)
        {
            currentNode.Payload = region.ToString();
            region.Clear();
            IParseTO child = new ParseTO();
            currentNode.Child = child;
            child.HangingOpen = true;
            child.Parent = currentNode;
            child.EndIndex = -1;
            child.StartIndex = i;
            currentNode = child;
            return currentNode;
        }

        IParseTO ProcessNode(string payload, IParseTO currentNode, ref StringBuilder region, ref bool openRegion)
        {
            if (!currentNode.IsRoot)
            {
                currentNode = currentNode.Parent;
                region = new StringBuilder(currentNode.Payload);
                openRegion = true;
            }
            else
            {
                if (currentNode.IsRoot && !currentNode.IsLeaf && currentNode.Child.HangingOpen)
                {
                    throw new Dev2DataLanguageParseError(ErrorResource.InvalidSyntaxCreatingVariable, 0, payload.Length, enIntellisenseErrorCode.SyntaxError);
                }
            }
            return currentNode;
        }

        public bool ShouldAddToRegion(string payload, char cur, char prev, int i, bool shouldAddToRegion, char charToCheck)
        {
            if (cur == charToCheck && prev != charToCheck)
            {
                var checkIndex = i + 1;
                if (checkIndex < payload.Length && payload[checkIndex] == charToCheck)
                {
                    shouldAddToRegion = false;
                }

            }
            return shouldAddToRegion;
        }

        public bool CheckValidIndex(IParseTO to, string part, int start, int end)
        {
            if (int.TryParse(part, out int partAsInt))
            {
                if (partAsInt < 1)
                {
                    throw new Dev2DataLanguageParseError("Recordset index [ " + part + " ] is not greater than zero", to.StartIndex + start, to.EndIndex + end, enIntellisenseErrorCode.NonPositiveRecordsetIndex);
                }
            }
            else
            {
                var message = "Recordset index (" + part + ") contains invalid character(s)";
                throw new Dev2DataLanguageParseError(message, to.StartIndex + start, to.EndIndex + end, enIntellisenseErrorCode.NonNumericRecordsetIndex);
            }
            return true;
        }

        public bool CheckCurrentIndex(IParseTO to, int start, string raw, int end)
        {
            start += 1;
            var part = raw.Substring(start, raw.Length - (start + 1));

            if (!part.Contains(DataListUtil.OpeningSquareBrackets) && part != "*")
            {
                if (int.TryParse(part, out int partAsInt))
                {
                    if (partAsInt < 1)
                    {
                        throw new Dev2DataLanguageParseError(string.Format(ErrorResource.RecordsetIndexNotGreaterThanZero, part), to.StartIndex + start, to.EndIndex + end, enIntellisenseErrorCode.NonPositiveRecordsetIndex);
                    }
                }
                else
                {
                    var message = string.Format(ErrorResource.RecordsetIndexContainsInvalidCharecters, part);
                    throw new Dev2DataLanguageParseError(message, to.StartIndex + start, to.EndIndex + end, enIntellisenseErrorCode.NonNumericRecordsetIndex);
                }
            }

            return true;
        }

        public IIntellisenseResult ValidateName(string name, string displayString)
        {
            displayString = displayString ?? string.Empty;

            if (!string.IsNullOrEmpty(name))
            {
                var dataListVerifyPart = new DataListVerifyPart(name, "");
                var displayName = displayString == "Recordset field" ? name : DataListUtil.OpeningSquareBrackets + name + DataListUtil.ClosingSquareBrackets;
                var intellisenseResult = IntellisenseFactory.CreateErrorResult(1, 1, dataListVerifyPart, displayString + " name " + displayName + " contains invalid character(s). Only use alphanumeric _ and - ", enIntellisenseErrorCode.SyntaxError, true);
                try
                {
                    if (!string.IsNullOrEmpty(name))
                    {
#pragma warning disable S134 // Control flow statements "if", "switch", "for", "foreach", "while", "do"  and "try" should not be nested too deeply
                        if (Char.IsNumber(name[0]))
                        {
                            return IntellisenseFactory.CreateErrorResult(1, 1, dataListVerifyPart, displayString + " name " + displayName + " begins with a number", enIntellisenseErrorCode.SyntaxError, true);
                        }
                        if (name.Contains(":"))
                        {
                            return intellisenseResult;
                        }
                        if (name.Contains("."))
                        {
                            return intellisenseResult;
                        }
                        if (name.Contains(' '))
                        {
                            return intellisenseResult;
                        }
                        if (name.ContainsUnicodeCharacter())
                        {
                            return intellisenseResult;
                        }
#pragma warning restore S134 // Control flow statements "if", "switch", "for", "foreach", "while", "do"  and "try" should not be nested too deeply
                        XmlConvert.VerifyName(name);
                    }
                }
                catch (Exception ex)
                {
                    return intellisenseResult;
                }
            }
            return null;
        }

        public bool ProcessFieldsForRecordSet(IParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, string[] parts, out string search, out bool emptyOk, string display, IDev2DataLanguageIntellisensePart recordsetPart, string partName)
        {
            emptyOk = false;
            search = "";
            if (parts[0].IndexOf(DataListUtil.RecordsetIndexClosingBracket, StringComparison.Ordinal) <= 0)
            {
                // its an error ;)
                var part = IntellisenseFactory.CreateDataListValidationRecordsetPart(parts[0], "." + parts[1], true);
                result.Add(IntellisenseFactory.CreateErrorResult(payload.StartIndex, payload.EndIndex, part, " [[" + display + "]] is a malformed recordset", enIntellisenseErrorCode.InvalidRecordsetNotation, !payload.HangingOpen));
            }
            else
            {
                search = parts[1].ToLower();
                var intellisenseResult = ValidateName(search, "Recordset field");
                if (intellisenseResult != null)
                {
                    result.Add(intellisenseResult);
                    return true;
                }
                foreach (IDev2DataLanguageIntellisensePart t in recordsetPart.Children)
                {
                    var match = t.Name.ToLower();
                    if (match.Contains(search) && ((match != search) || (match == search && addCompleteParts)))
                    {
                        var index = payload.Child != null ? payload.Child.Payload : DataListUtil.ExtractIndexRegionFromRecordset(parts[0]);

                        var part = IntellisenseFactory.CreateDataListValidationRecordsetPart(partName, t.Name, t.Description, index);
                        result.Add(IntellisenseFactory.CreateSelectableResult(parts[0].Length, payload.EndIndex, part, part.Description));
                    }
                    else
                    {
                        ShouldUpdateEmptyOk(search, ref emptyOk, match);
                    }
                }
            }
            return false;
        }

        private static void ShouldUpdateEmptyOk(string search,ref bool emptyOk, string match)
        {
            if (match == search)
            {
                emptyOk = true;
            }
        }

        public void ProcessResults(IList<IIntellisenseResult> realResults, IIntellisenseResult intellisenseResult)
        {
            var addToFinal = true;

            realResults
                .ToList()
                .ForEach(rr =>
                {
                    if (rr.Option.DisplayValue == intellisenseResult.Option.DisplayValue)
                    {
                        addToFinal = false;
                    }
                });

            if (addToFinal)
            {
                realResults.Add(intellisenseResult);
            }
        }

        public bool ValidateName(string rawSearch, string displayString, IList<IIntellisenseResult> result,out IList<IIntellisenseResult> intellisenseResults)
        {
            var intellisenseResult = ValidateName(rawSearch, displayString);
            if (intellisenseResult != null)
            {
                result.Add(intellisenseResult);
                {
                    intellisenseResults = result;
                    return true;
                }
            }
            intellisenseResults = null;
            return false;
        }

        public bool AddFieldResult(IParseTO payload, IList<IIntellisenseResult> result, string tmpString, string[] parts, bool isRs)
        {
            if ((tmpString.EndsWith(".") || tmpString.StartsWith(".") || (parts.Length > 1 && !isRs)) && payload.Child == null)
            {
                var intellisenseResult = ValidateName(tmpString, isRs && !tmpString.StartsWith(DataListUtil.RecordsetIndexOpeningBracket) ? "Recordset" : "Variable");
                if (intellisenseResult != null)
                {
                    result.Add(intellisenseResult);
                    return true;
                }
            }
            return false;
        }

        public bool IsValidIndex(IParseTO to)
        {
            var result = false;
            var raw = to.Payload;
            var start = raw.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
            var end = raw.LastIndexOf(DataListUtil.RecordsetIndexClosingBracket, StringComparison.Ordinal);

            if (end - start == 1 || (start > 0 && end < 0 && (raw.Length - 1 == start)))
            {
                result = true;
            }
            else
            {
                if (start > 0 && end < 0)
                {
                    var part = raw.Substring(start + 1, raw.Length - (start + 1));

                    result = part.Contains(DataListUtil.OpeningSquareBrackets) || CheckValidIndex(to, part, start, end);
                    if (end < 0)
                    {
                        var message = "Recordset [ " + raw + " ] does not contain a matching ')'";
                        throw new Dev2DataLanguageParseError(message, to.StartIndex + start, to.EndIndex + end, enIntellisenseErrorCode.InvalidRecordsetNotation);
                    }
                }
                else
                {
                    if (start > 0 && end > start)
                    {
                        result = CheckCurrentIndex(to, start, raw, end);
                    }
                }
            }

            return result;
        }

        public IIntellisenseResult AddErrorToResults(bool isRs, string part, IDev2DataLangaugeParseError dev2DataLanguageParseError, bool isOpen)
        {
            // add error
            IDataListVerifyPart pTo;
            if (isRs)
            {
                var start = part.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
                var rs = part;
                if (start >= 0)
                {
                    rs = rs.Substring(0, start);
                }
                pTo = IntellisenseFactory.CreateDataListValidationRecordsetPart(rs, "", "");
            }
            else
            {
                pTo = IntellisenseFactory.CreateDataListValidationScalarPart(part.Replace("]", ""));
            }

            return IntellisenseFactory.CreateErrorResult(dev2DataLanguageParseError.StartIndex, dev2DataLanguageParseError.EndIndex, pTo, dev2DataLanguageParseError.Message, dev2DataLanguageParseError.ErrorCode, isOpen);
        }

        #endregion
    }
}