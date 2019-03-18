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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.Exceptions;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Warewolf.Resource.Errors;

namespace Dev2.Data.Parsers
{
    public class Dev2DataLanguageParser : IDev2DataLanguageParser, IDev2StudioDataLanguageParser
    {
        readonly IParserHelper _parserHelper;
        static volatile ConcurrentDictionary<Tuple<string, string>, IList<IIntellisenseResult>> _payloadCache = new ConcurrentDictionary<Tuple<string, string>, IList<IIntellisenseResult>>();
        static volatile ConcurrentDictionary<string, IList<IIntellisenseResult>> _expressionCache = new ConcurrentDictionary<string, IList<IIntellisenseResult>>();
        private readonly DataLanguageParserImplementation _dataLanguageParser;

        public Dev2DataLanguageParser()
            : this(new ParserHelperUtil())
        {
        }

        public Dev2DataLanguageParser(IParserHelper parserHelper)
        {
            _parserHelper = parserHelper;
            _dataLanguageParser = new DataLanguageParserImplementation(_parserHelper, this);
        }

        public IList<IParseTO> MakeParts(string payload) => MakeParts(payload, false);
        public IList<IParseTO> MakeParts(string payload, bool addCompleteParts)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return new List<IParseTO>();
            }

            IList<IParseTO> result = new List<IParseTO>();

            var region = new StringBuilder();
            var openRegion = false;
            IParseTO currentNode = new ParseTO { Parent = null, HangingOpen = true };
            var root = currentNode;
            var methodPayload = payload.Replace("]].[[", "]][[");

            var payloadIndex = RetrievePayloadIndex(payload, result, ref region, ref openRegion, ref currentNode, ref root, methodPayload);

            if (openRegion)
            {
                currentNode.EndIndex = payloadIndex - 1;
                currentNode.Payload = region.ToString();
            }

            if (!result.Contains(root))
            {
                result.Add(root);
            }

            if (root.HangingOpen && addCompleteParts)
            {
                throw new Dev2DataLanguageParseError(ErrorResource.InvalidCloseRegion, 0, methodPayload.Length, enIntellisenseErrorCode.SyntaxError);
            }
            return result;
        }

        private int RetrievePayloadIndex(string payload, IList<IParseTO> result, ref StringBuilder region, ref bool openRegion, ref IParseTO currentNode, ref IParseTO root, string methodPayload)
        {
            var previousPayload = '\0';
            int payloadIndex;
            for (payloadIndex = 0; payloadIndex < methodPayload.Length; payloadIndex++)
            {
                var currentPayload = methodPayload[payloadIndex];
                var shouldAddToRegion = _parserHelper.ShouldAddToRegion(methodPayload, currentPayload, previousPayload, payloadIndex, true, '[');
                shouldAddToRegion = _parserHelper.ShouldAddToRegion(methodPayload, currentPayload, previousPayload, payloadIndex, shouldAddToRegion, ']');

                if (currentPayload == '[' && previousPayload == '[')
                {
                    currentNode = openRegion ? _parserHelper.CurrentNode(currentNode, region, payloadIndex) : ParseTO(currentNode, payloadIndex, result, ref root, ref openRegion);
                    currentPayload = '\0';
                }

                if (currentPayload == ']' && previousPayload == ']')
                {
                    openRegion = _parserHelper.ProcessOpenRegion(payload, openRegion, payloadIndex, ref currentNode, ref region, ref currentPayload);
                }

                if (openRegion && shouldAddToRegion && currentPayload != '\0')
                {
                    region.Append(currentPayload);
                }
                previousPayload = currentPayload;
            }

            return payloadIndex;
        }

        IParseTO ParseTO(IParseTO currentNode, int i, IList<IParseTO> result, ref IParseTO root, ref bool openRegion)
        {
            if (currentNode == root && !root.HangingOpen)
            {
                IParseTO newRoot = new ParseTO { HangingOpen = true, Parent = null, StartIndex = i, EndIndex = -1 };
                result.Add(root);
                root = newRoot;
                currentNode = root;
            }
            openRegion = true;
            currentNode.StartIndex = i + 1;
            return currentNode;
        }

        public IIntellisenseResult ValidateName(string name, string displayString) => _parserHelper.ValidateName(name, displayString);

        public IList<IIntellisenseResult> ParseExpressionIntoParts(string expression, IList<IDev2DataLanguageIntellisensePart> parts) 
            => WrapAndClear(() =>
        {
            return ExpressionIntoParts(expression, parts);
        }, _expressionCache);

        IList<IIntellisenseResult> ExpressionIntoParts(string expression, IList<IDev2DataLanguageIntellisensePart> parts)
        {
            var inputVariables = string.IsNullOrEmpty(expression) || parts == null;
            if (inputVariables)
            {
                return new List<IIntellisenseResult>();
            }

            var canCache = Regex.Matches(expression, "\\[\\[").Count == 1;
            var expressionCacheContainsKey = _expressionCache.ContainsKey(expression);
            if (canCache && expressionCacheContainsKey)
            {
                return _expressionCache[expression];
            }

            var result = _dataLanguageParser.PartsGeneration(expression, parts, true, false, null);
            var resultWithoutError = result != null && result.All(a => a.Type != enIntellisenseResultType.Error);
            if (resultWithoutError && canCache)
            {
                try
                {
                    _expressionCache.TryAdd(expression, result);
                }
                catch (Exception e)
                {
                    Dev2Logger.Warn(e.Message, GlobalConstants.WarewolfWarn);
                }
            }

            return result;
        }

        public IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string payload, string dataList)
            => ParseDataLanguageForIntellisense(payload, dataList, false, null, false);

        public IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string payload, string dataList, bool addCompleteParts)
            => ParseDataLanguageForIntellisense(payload, dataList, addCompleteParts, null, false);

        public IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string payload, string dataList, bool addCompleteParts, IIntellisenseFilterOpsTO filterTo)
            => ParseDataLanguageForIntellisense(payload, dataList, addCompleteParts, filterTo, false);

        public IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string payload, string dataList, bool addCompleteParts, IIntellisenseFilterOpsTO filterTo, bool isFromIntellisense)
        {
            return WrapAndClear(() =>
            {
                var key = new Tuple<string, string>(payload, dataList);
                if (string.IsNullOrEmpty(payload))
                {
                    return new List<IIntellisenseResult>();
                }
                if (_payloadCache.ContainsKey(key))
                {
                    return _payloadCache[key];
                }
                var parts = DataListFactory.GenerateIntellisensePartsFromDataList(dataList, filterTo);

                IList<IDev2DataLanguageIntellisensePart> additionalParts = new List<IDev2DataLanguageIntellisensePart>();
                if (filterTo != null && filterTo.FilterType == enIntellisensePartType.RecordsetsOnly)
                {
                    additionalParts = DataListFactory.GenerateIntellisensePartsFromDataList(dataList, new IntellisenseFilterOpsTO { FilterCondition = filterTo.FilterCondition, FilterType = enIntellisensePartType.None });
                }
                var result = _dataLanguageParser.PartsGeneration(payload, parts, addCompleteParts, isFromIntellisense, additionalParts);
                if (result != null && result.Count > 0 && result.All(a => a.Type != enIntellisenseResultType.Error))
                {
                    try
                    {
                        _payloadCache.TryAdd(key, result);
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Warn(e.Message, GlobalConstants.WarewolfWarn);
                    }
                }

                return result;
            }, _payloadCache);
        }

        T WrapAndClear<T, U>(Func<T> runFunc, ConcurrentDictionary<U, T> clearIfException)
        {
            try
            {
                return runFunc();
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                clearIfException.Clear();
                throw;
            }
        }

        public IList<string> ParseForActivityDataItems(string payload)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return new List<string>();
            }

            IList<string> result = new List<string>();

            var parserList = MakeParts(payload);

            foreach (var parseObject in parserList)
            {
                var parseRef = parseObject;
                while (parseRef != null)
                {
                    if (!result.Contains(parseRef.Payload))
                    {
                        result.Add(parseRef.Payload);
                    }
                    parseRef = parseRef.Child;
                }
            }

            return result;
        }
    }

    public class DataLanguageParserImplementation
    {
        const string CdataStart = @"<![CDATA[";
        const string CdataEnd = @"]]>";

        readonly IParserHelper _parserHelper;
        static ICommonRecordSetUtil _recordSetUtil = new CommonRecordSetUtil();
        readonly IDev2DataLanguageParser _dev2DataLanguageParser;
        readonly IExtract _extract;

        public DataLanguageParserImplementation(IParserHelper parserHelper, IDev2DataLanguageParser dev2DataLanguageParser)
            : this (parserHelper, dev2DataLanguageParser, new Extract(parserHelper, new Match(parserHelper)))
        {

        }

        internal DataLanguageParserImplementation(IParserHelper parserHelper, IDev2DataLanguageParser dev2DataLanguageParser, IExtract extract)
        {
            _parserHelper = parserHelper;
            _dev2DataLanguageParser = dev2DataLanguageParser;
            _extract = extract;
        }

        public IList<IIntellisenseResult> PartsGeneration(string payload, IList<IDev2DataLanguageIntellisensePart> parts, bool addCompleteParts, bool isFromIntellisense, IList<IDev2DataLanguageIntellisensePart> additionalParts)
        {
            IList<IIntellisenseResult> result = new List<IIntellisenseResult>();
            try
            {
                if (payload.Contains(CdataStart))
                {
                    payload = payload.Replace(CdataStart, "");
                    var idx = payload.LastIndexOf(CdataEnd, StringComparison.Ordinal);
                    payload = payload.Substring(0, idx);
                }

                if (payload.Equals("[[]]"))
                {
                    result.Add(IntellisenseFactory.CreateErrorResult(0, 4, null, ErrorResource.VariableIsMissing, enIntellisenseErrorCode.SyntaxError, true));
                    return result;
                }

                if (payload.Contains(DataListUtil.OpeningSquareBrackets))
                {
                    var rootItems = _dev2DataLanguageParser.MakeParts(payload, addCompleteParts);
                    IParseTO magicRegion = null;
                    IList<IParseTO> evalParts = new List<IParseTO>();
                    magicRegion = GetMagicRegion(rootItems, magicRegion, evalParts);
                    if (magicRegion != null)
                    {
                        result = isFromIntellisense ? _extract.TryExtractActualIntellisenseOptions(magicRegion, parts, false) : _extract.ExtractIntellisenseOptions(magicRegion, parts, false);
                    }
                    evalParts
                        .ToList()
                        .ForEach(evalPart =>
                        {
                            var tmp = _extract.ExtractIntellisenseOptions(evalPart, parts, !isFromIntellisense && addCompleteParts, additionalParts);
                            if (tmp != null)
                            {
                                result = result.Union(tmp).ToList();
                            }
                        });
                }
            }
            catch (Dev2DataLanguageParseError e)
            {
                var p = IntellisenseFactory.CreateDataListValidationScalarPart(payload);
                result.Add(IntellisenseFactory.CreateErrorResult(e.StartIndex, e.EndIndex, p, e.Message, e.ErrorCode, true));
            }
            return result;
        }

        private static IParseTO GetMagicRegion(IList<IParseTO> rootItems, IParseTO magicRegion, IList<IParseTO> evalParts)
        {
            rootItems.ToList()
                .ForEach(rootItem =>
                {
                    var eval = rootItem;
                    while (eval != null)
                    {
                        if (eval.HangingOpen)
                        {
                            magicRegion = eval;
                        }
                        if (!eval.HangingOpen && eval != magicRegion)
                        {
                            evalParts.Add(eval);
                        }
                        eval = eval.Child;
                    }
                });
            return magicRegion;
        }

        static void FinalEvaluation(IParseTO payload, StringBuilder tmp, IList<IIntellisenseResult> result, IList<IDev2DataLanguageIntellisensePart> additionalParts, bool isRs)
        {
            var display = tmp.ToString().Replace("]", "");
            IDataListVerifyPart part;

            var code = enIntellisenseErrorCode.RecordsetNotFound;
            if (!isRs)
            {
                code = display.IndexOf(' ') >= 0 ? enIntellisenseErrorCode.SyntaxError : enIntellisenseErrorCode.ScalarNotFound;

                part = IntellisenseFactory.CreateDataListValidationScalarPart(display);
            }
            else
            {
                var start = display.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
                display = display.Substring(0, start);
                display += "()";
                part = IntellisenseFactory.CreateDataListValidationRecordsetPart(display, "");
            }
            if (additionalParts == null)
            {
                result.Add(!display.Contains(' ') ? IntellisenseFactory.CreateErrorResult(payload.StartIndex, payload.EndIndex, part, " [[" + display + "]] does not exist in your variable list", code, !payload.HangingOpen) : IntellisenseFactory.CreateErrorResult(payload.StartIndex, payload.EndIndex, part, " [[" + display + "]] contains a space, this is an invalid character for a variable name", code, !payload.HangingOpen));
            }
            else
            {
                if (!additionalParts.Select(a => a.Name).Contains(display))
                {
                    result.Add(!display.Contains(' ') ? IntellisenseFactory.CreateErrorResult(payload.StartIndex, payload.EndIndex, part, " [[" + display + "]] does not exist in your variable list", code, !payload.HangingOpen) : IntellisenseFactory.CreateErrorResult(payload.StartIndex, payload.EndIndex, part, " [[" + display + "]] contains a space, this is an invalid character for a variable name", code, !payload.HangingOpen));
                }
            }
        }

        internal static bool ProcessForChild(IParseTO payload, IList<IDev2DataLanguageIntellisensePart> refParts, IList<IIntellisenseResult> result, string search, IDev2DataLanguageIntellisensePart t1)
        {
            var emptyOk = false;
            var isHangingChild = payload.Child != null && payload.Child.HangingOpen;

            if (!payload.IsLeaf && !isHangingChild)
            {
                _recordSetUtil.OpenRecordsetItem(payload, result, t1);
            }
            else
            {
                if (payload.Child == null)
                {
                    IDataListVerifyPart prt;
                    foreach (IDev2DataLanguageIntellisensePart t in refParts.Where(t => t.Children == null))
                    {
                        prt = IntellisenseFactory.CreateDataListValidationRecordsetPart(search, "", " / Select a specific row", DataListUtil.OpeningSquareBrackets + t.Name + DataListUtil.ClosingSquareBrackets);
                        result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, prt, prt.Description));
                    }
                    prt = IntellisenseFactory.CreateDataListValidationRecordsetPart(search, "", " / Reference all rows in the Recordset ", "*");
                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, prt, prt.Description));
                }
                emptyOk = true;
            }
            return emptyOk;
        }

        internal static class Add
        {
            internal static void AddIndex(IEnumerable<IDev2DataLanguageIntellisensePart> refParts, IParseTO payload, string search, IList<IIntellisenseResult> result)
            {
                foreach (IDev2DataLanguageIntellisensePart t in refParts)
                {
                    if (t.Children == null)
                    {
                        var prt = IntellisenseFactory.CreateDataListValidationRecordsetPart(search, "", "", DataListUtil.OpeningSquareBrackets + t.Name + DataListUtil.ClosingSquareBrackets);
                        result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, prt, prt.Description));
                    }
                }
            }

            internal static void AddFoundItems(IEnumerable<IDev2DataLanguageIntellisensePart> refParts, IParseTO payload, string search, bool addCompleteParts, IList<IIntellisenseResult> result)
            {
                foreach (IDev2DataLanguageIntellisensePart t in refParts)
                {
                    var match = t.Name.ToLower();

                    if (t.Children != null && t.Children.Count > 0)
                    {
                        AddFieldOptions(payload, search, addCompleteParts, match, t, result);
                    }
                    else
                    {
                        if (!match.Contains(search))
                        {
                            continue;
                        }

                        AddFoundItems(payload, t, result);
                    }
                }
            }
            internal static void AddFieldOptions(IParseTO payload, string search, bool addCompleteParts, string match, IDev2DataLanguageIntellisensePart t, IList<IIntellisenseResult> result)
            {
                IDataListVerifyPart part;
                if (!addCompleteParts && match.Contains(search))
                {
                    part = IntellisenseFactory.CreateDataListValidationScalarPart(t.Name + DataListUtil.RecordsetIndexOpeningBracket, !string.IsNullOrEmpty(t.Description) ? t.Description : " Select a specific row");

                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));

                    foreach (IDev2DataLanguageIntellisensePart t1 in t.Children)
                    {
                        part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t.Name, t1.Name, !string.IsNullOrEmpty(t1.Description) ? t1.Description : " Input: Use last row, Result: Append new record");
                        result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                        part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t.Name, t1.Name, !string.IsNullOrEmpty(t1.Description) ? t1.Description : " Use all the rows", "*");
                        result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                    }
                    return;
                }
                foreach (IDev2DataLanguageIntellisensePart t1 in t.Children)
                {
                    if (t1.Name.Contains(search))
                    {
                        part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t.Name, t1.Name, !string.IsNullOrEmpty(t1.Description) ? t1.Description : " Input: Use last row, Result: Append new record");
                        result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                        part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t.Name, t1.Name, !string.IsNullOrEmpty(t1.Description) ? t1.Description : " Use all the rows", "*");
                        result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                    }
                }
            }

            internal static void AddFoundItems(IParseTO payload, IDev2DataLanguageIntellisensePart t, IList<IIntellisenseResult> result)
            {
                if (payload.Parent != null && payload.Parent.Payload.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal) >= 0)
                {
                    var part = IntellisenseFactory.CreateDataListValidationScalarPart(t.Name, !string.IsNullOrEmpty(t.Description) ? t.Description : " Use row at this index");

                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                }
                else
                {
                    IDataListVerifyPart part;
                    if (t.Name.Contains('(') && t.Name.Contains(')'))
                    {
                        part = IntellisenseFactory.CreateDataListValidationScalarPart(t.Name, t.Description);
                    }
                    else
                    {
                        part = IntellisenseFactory.CreateDataListValidationScalarPart(t.Name, t.Description);
                    }

                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                }
            }
        }

        internal interface IMatch
        {
            void MatchNonFieldVariables(IParseTO payload, IList<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts, StringBuilder tmp, IList<IIntellisenseResult> result, IList<IDev2DataLanguageIntellisensePart> additionalParts, bool isRs, string rawSearch, string search, bool emptyOk, string[] parts);
            void MatchFieldVariables(IParseTO payload, IList<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts, IList<IIntellisenseResult> result, string[] parts, bool isRs, string rawSearch, string search, bool emptyOk);
        }

        internal class Match : IMatch
        {
            readonly IParserHelper _parserHelper;
            internal Match(IParserHelper parserHelper)
            {
                _parserHelper = parserHelper;
            }

            public void MatchFieldVariables(IParseTO payload, IList<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts, IList<IIntellisenseResult> result, string[] parts, bool isRs, string rawSearch, string search, bool emptyOk)
            {
                var tmpTo = new ParseTO { Payload = parts[0], StartIndex = 0, EndIndex = parts[0].Length - 1 };

                var isRecName = isRs && rawSearch.Contains(DataListUtil.RecordsetIndexOpeningBracket) && rawSearch.EndsWith(DataListUtil.RecordsetIndexClosingBracket);

                const string DisplayString = "Recordset";
                if ((!isRecName || parts[1] == string.Empty) && payload.Child == null)
                {
                    if (_parserHelper.ValidateName(rawSearch, DisplayString, result, out IList<IIntellisenseResult> intellisenseResults))
                    {
                        return;
                    }
                }
                else
                {
                    if (_parserHelper.ValidateName(search, DisplayString, result, out IList<IIntellisenseResult> intellisenseResults))
                    {
                        return;
                    }
                }

                try
                {
                    _parserHelper.IsValidIndex(tmpTo);
                }
                catch (Dev2DataLanguageParseError e)
                {
                    result.Add(_parserHelper.AddErrorToResults(isRs, parts[0], e, !payload.HangingOpen));
                }

                var recordsetPart = refParts.FirstOrDefault(c => c.Name.ToLower() == search && c.Children != null);

                var display = parts[0];
                var partName = parts[0];
                var start = display.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
                if (start >= 0 && recordsetPart == null)
                {
                    display = display.Substring(0, start);
                    display += "()";
                }

                if (partName.IndexOf(' ') < 0)
                {
                    search = ProcessValidPartNameContainingFields(payload, addCompleteParts, result, parts, search, emptyOk, partName, recordsetPart, display);
                }
                else
                {
                    var part = IntellisenseFactory.CreateDataListValidationRecordsetPart(parts[0], "." + parts[1], true);
                    result.Add(IntellisenseFactory.CreateErrorResult(payload.StartIndex, payload.EndIndex, part, " [[" + display + "]] contains a space, this is an invalid character for a variable name", enIntellisenseErrorCode.SyntaxError, !payload.HangingOpen));
                }
            }

            public void MatchNonFieldVariables(IParseTO payload, IList<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts, StringBuilder tmp, IList<IIntellisenseResult> result, IList<IDev2DataLanguageIntellisensePart> additionalParts, bool isRs, string rawSearch, string search, bool emptyOk, string[] parts)
            {
                try
                {
                    var isRecName = isRs;
                    isRecName &= rawSearch.Contains(DataListUtil.RecordsetIndexOpeningBracket);
                    isRecName &= rawSearch.EndsWith(DataListUtil.RecordsetIndexClosingBracket);
                    if (!isRecName && !payload.HangingOpen && ScalarMatch(result, isRs, rawSearch))
                    {
                        return;
                    }
                    if (isRecName && !payload.HangingOpen && RecordsetMatch(result, rawSearch, search))
                    {
                        return;
                    }
                    if ((rawSearch.Contains(DataListUtil.RecordsetIndexOpeningBracket) && _parserHelper.IsValidIndex(payload)) || !rawSearch.Contains(DataListUtil.RecordsetIndexOpeningBracket))
                    {
                        foreach (IDev2DataLanguageIntellisensePart t1 in refParts)
                        {
                            emptyOk = MatchVariablesWithNoFields(payload, refParts, addCompleteParts, result, isRs, rawSearch, search, emptyOk, parts, t1);
                        }
                        refParts.ToList().ForEach(pt =>
                        {
                            MatchChildren(payload, result, search, pt);
                        });
                        if (result.Count == 0 && !emptyOk)
                        {
                            FinalEvaluation(payload, tmp, result, additionalParts, isRs);
                        }
                    }
                }
                catch (Dev2DataLanguageParseError e)
                {
                    result.Add(_parserHelper.AddErrorToResults(isRs, parts[0], e, !payload.HangingOpen));
                }
            }

            internal static bool MatchVariablesWithNoFields(IParseTO payload, IList<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts, IList<IIntellisenseResult> result, bool isRs, string rawSearch, string search, bool emptyOk, string[] parts, IDev2DataLanguageIntellisensePart t1)
            {
                var match = t1.Name.ToLower();

                if (match.Contains(search) && (match != search))
                {
                    if (t1.Children != null && t1.Children.Count > 0)
                    {
                        _recordSetUtil.ProcessRecordSetFields(payload, addCompleteParts, result, t1);
                    }
                    else
                    {
                        _recordSetUtil.ProcessNonRecordsetFields(payload, result, t1);
                    }
                }
                else if (match == search && isRs)
                {
                    emptyOk = rawSearch.Contains(DataListUtil.RecordsetIndexOpeningBracket) && rawSearch.Contains(DataListUtil.RecordsetIndexClosingBracket) ? _recordSetUtil.RecordsetMatch(payload, addCompleteParts, result, rawSearch, search, emptyOk, parts, t1) : ProcessForChild(payload, refParts, result, search, t1);
                }
                else
                {
                    if (match == search && !isRs)
                    {
                        if (t1.Children != null && t1.Children.Count > 0)
                        {
                            ReturnFieldMatchForRecordSet(payload, result, t1);
                        }
                        else
                        {
                            emptyOk = HandleScalarMatches(payload, addCompleteParts, result, search, t1, match);
                        }
                    }
                }
                return emptyOk;
            }

            internal static void MatchChildren(IParseTO payload, IList<IIntellisenseResult> result, string search, IDev2DataLanguageIntellisensePart pt)
            {
                pt.Children?.ToList().ForEach(child =>
                {
                    var match = child.Name.ToLower();

                    if (match.Contains(search))
                    {
                        var resultPt = IntellisenseFactory.CreateDataListValidationRecordsetPart(pt.Name, child.Name, pt.Description + " / " + child.Description + " Select this recordset field field");
                        var tmpChild = IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, resultPt, resultPt.Description);

                        if (result
                            .ToList()
                            .Find(r => r.Option.DisplayValue == tmpChild.Option.DisplayValue) == null)
                        {
                            result.Add(tmpChild);
                        }
                    }
                });
            }

            internal bool ScalarMatch(IList<IIntellisenseResult> result, bool isRs, string rawSearch)
            {
                var intellisenseResult = _parserHelper.ValidateName(rawSearch, isRs && !rawSearch.StartsWith(DataListUtil.RecordsetIndexOpeningBracket) ? "Recordset" : "Variable");
                if (intellisenseResult != null)
                {
                    result.Add(intellisenseResult);
                    return true;
                }
                return false;
            }

            internal static bool HandleScalarMatches(IParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, string search, IDev2DataLanguageIntellisensePart t1, string match)
            {
                var emptyOk = false;
                if (search != match || (search == match && addCompleteParts))
                {
                    if (payload.Parent != null && payload.Parent.Payload.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal) >= 0)
                    {
                        var p = IntellisenseFactory.CreateDataListValidationScalarPart(t1.Name);
                        result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, p, " / Select a specific row "));
                    }
                    else
                    {
                        var p = IntellisenseFactory.CreateDataListValidationScalarPart(t1.Name);
                        result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, p, t1.Description));
                    }
                }
                else
                {
                    emptyOk = true;
                }
                return emptyOk;
            }

            internal bool RecordsetMatch(IList<IIntellisenseResult> result, string rawSearch, string search)
            {
                var intellisenseResult = _parserHelper.ValidateName(search == string.Empty ? rawSearch : search, "Variable"); //search is empty if there is a scalar value with start open (

                if (intellisenseResult != null)
                {
                    result.Add(intellisenseResult);
                    return true;
                }
                return false;
            }

            internal static void ReturnFieldMatchForRecordSet(IParseTO payload, IList<IIntellisenseResult> result, IDev2DataLanguageIntellisensePart t1)
            {
                var part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t1.Name, "", t1.Description);
                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                var children = t1.Children;
                if (children != null)
                {
                    foreach (IDev2DataLanguageIntellisensePart t in children)
                    {
                        part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t1.Name, t.Name, t.Description + " / Use a field of the Recordset");
                        result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                    }
                }
            }

            internal string ProcessValidPartNameContainingFields(IParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, string[] parts, string search, bool emptyOk, string partName, IDev2DataLanguageIntellisensePart recordsetPart, string display)
            {
                if (partName.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal) >= 0)
                {
                    partName = partName.Substring(0, partName.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal));
                }

                if (recordsetPart == null)
                {
                    var part = IntellisenseFactory.CreateDataListValidationRecordsetPart(partName, parts[1], "");
                    result.Add(IntellisenseFactory.CreateErrorResult(payload.StartIndex, parts[0].Length - 1, part, DataListUtil.OpeningSquareBrackets + display + "]] does not exist in your variable list", enIntellisenseErrorCode.NeitherRecordsetNorFieldFound, !payload.HangingOpen));
                }
                else
                {
                    var processForRecordset = _parserHelper.ProcessFieldsForRecordSet(payload, addCompleteParts, result, parts, out search, out emptyOk, display, recordsetPart, partName);
                    if (recordsetPart.Children != null && recordsetPart.Children.Count > 0 && processForRecordset)
                    {
                        return search;
                    }
                }

                if (result.Count == 0 && !emptyOk)
                {
                    var part = IntellisenseFactory.CreateDataListValidationRecordsetPart(parts[0], search);
                    result.Add(char.IsNumber(search[0]) ? IntellisenseFactory.CreateErrorResult(payload.StartIndex, parts[0].Length - 1, part, "Invalid Expression: Recordset Field [ " + search + " ] starts with a number", enIntellisenseErrorCode.SyntaxError, !payload.HangingOpen) : IntellisenseFactory.CreateErrorResult(parts[0].Length, payload.EndIndex, part, "Recordset Field [ " + search + " ] does not exist for [ " + parts[0] + " ]", enIntellisenseErrorCode.FieldNotFound, !payload.HangingOpen));
                }
                return search;
            }
        }

        internal interface IExtract
        {
            IList<IIntellisenseResult> TryExtractActualIntellisenseOptions(IParseTO payload, IEnumerable<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts);
            IList<IIntellisenseResult> ExtractIntellisenseOptions(IParseTO payload, IList<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts, IList<IDev2DataLanguageIntellisensePart> additionalParts = null);
        }

        internal class Extract : IExtract
        {
            readonly IParserHelper _parserHelper;
            readonly IMatch _match;
            internal Extract(IParserHelper parserHelper, IMatch match)
            {
                _parserHelper = parserHelper;
                _match = match;
            }

            public IList<IIntellisenseResult> TryExtractActualIntellisenseOptions(IParseTO payload, IEnumerable<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts)
            {
                var tmp = new StringBuilder(payload.Payload);
                IList<IIntellisenseResult> result = new List<IIntellisenseResult>();

                if (payload != null)
                {
                    var parts = tmp.ToString().Split('.');
                    var search = parts[0].ToLower();
                    var isRs = search.Contains(DataListUtil.RecordsetIndexOpeningBracket);

                    if (search.Contains(DataListUtil.RecordsetIndexOpeningBracket))
                    {
                        isRs = true;
                        var pos = search.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
                        search = search.Substring(0, search.Length - (search.Length - pos));
                    }

                    try
                    {
                        ExtractActualIntellisenseOptions(payload, refParts, addCompleteParts, result, parts, search);
                    }
                    catch (Dev2DataLanguageParseError e)
                    {
                        result.Add(_parserHelper.AddErrorToResults(isRs, parts[0], e, !payload.HangingOpen));
                    }
                }

                IList<IIntellisenseResult> realResults = new List<IIntellisenseResult>();

                result.ToList()
                    .ForEach(r =>
                    {
                        var addToFinal = true;

                        realResults
                            .ToList()
                            .ForEach(rr =>
                            {
                                if (rr.Option.DisplayValue == r.Option.DisplayValue)
                                {
                                    addToFinal = false;
                                }
                            });

                        if (addToFinal)
                        {
                            realResults.Add(r);
                        }
                    });

                return result;
            }

            internal static void ExtractActualIntellisenseOptions(IParseTO payload, IEnumerable<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts, IList<IIntellisenseResult> result, string[] parts, string search)
            {
                var results = CreateResultsGeneric(refParts, payload, parts.Length == 1 ? search : parts[1], addCompleteParts);

                if (parts.Length == 2)
                {
                    var cmp = parts[1].ToLower();

                    foreach (IIntellisenseResult res in results)
                    {
                        if (res.Option.Field.ToLower().IndexOf(cmp, StringComparison.Ordinal) >= 0)
                        {
                            result.Add(res);
                        }
                    }
                }
                else
                {
                    foreach (IIntellisenseResult res in results)
                    {
                        result.Add(res);
                    }
                }
            }

            internal static IEnumerable<IIntellisenseResult> CreateResultsGeneric(IEnumerable<IDev2DataLanguageIntellisensePart> refParts, IParseTO payload, string search, bool addCompleteParts)
            {
                IList<IIntellisenseResult> result = new List<IIntellisenseResult>();

                if (payload.HangingOpen)
                {
                    var hasIndex = false;
                    var openBraceIndex = search.LastIndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
                    var closeBraceIndex = search.LastIndexOf(DataListUtil.RecordsetIndexClosingBracket, StringComparison.Ordinal);
                    if (openBraceIndex != -1 && openBraceIndex < search.Length && closeBraceIndex > openBraceIndex)
                    {
                        hasIndex = true;
                    }

                    var isRecordsetIndexOpeningBracket = payload.Payload.Contains(DataListUtil.RecordsetIndexOpeningBracket);
                    var isRecordsetIndexClosingBracket = !payload.Payload.Contains(DataListUtil.RecordsetIndexClosingBracket);
                    var isPayloadChildEmpty = payload.Child == null;

                    if (isRecordsetIndexOpeningBracket && isRecordsetIndexClosingBracket && isPayloadChildEmpty && !hasIndex)
                    {
                        Add.AddIndex(refParts, payload, search, result);
                    }
                    else
                    {
                        Add.AddFoundItems(refParts, payload, search, addCompleteParts, result);
                    }
                }

                return result;
            }

            public IList<IIntellisenseResult> ExtractIntellisenseOptions(IParseTO payload, IList<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts, IList<IDev2DataLanguageIntellisensePart> additionalParts = null)
            {
                var tmp = new StringBuilder(payload.Payload);
                IList<IIntellisenseResult> result = new List<IIntellisenseResult>();

                if (payload.Payload == string.Empty && payload.HangingOpen)
                {
                    ProcessForOnlyOpenRegion(payload, refParts, result);
                }
                else
                {
                    if (tmp.Length > 0)
                    {
                        ProcessRegion(payload, refParts, addCompleteParts, tmp, result, additionalParts);
                    }
                    else
                    {
                        if (payload.Child == null)
                        {
                            result.Add(IntellisenseFactory.CreateErrorResult(0, 4, null, ErrorResource.VariableIsMissing, enIntellisenseErrorCode.SyntaxError, true));
                            return result;
                        }
                    }
                }

                IList<IIntellisenseResult> realResults = new List<IIntellisenseResult>();
                result.ToList().ForEach(r => _parserHelper.ProcessResults(realResults, r));
                return result;
            }

            internal static void ProcessForOnlyOpenRegion(IParseTO payload, IEnumerable<IDev2DataLanguageIntellisensePart> refParts, IList<IIntellisenseResult> result)
            {
                var parentIsRecordset = payload.Parent?.IsRecordSet ?? false;
                refParts.ToList().ForEach(part =>
                {
                    if (part.Children != null && part.Children.Count > 0 && !parentIsRecordset)
                    {
                        var tmpPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(part.Name, "", part.Description + " / Select this record set");
                        result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.StartIndex + 2, tmpPart, tmpPart.Description));
                        part.Children.ToList()
                        .ForEach(child =>
                        {
                            tmpPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(part.Name, child.Name, child.Description + " / Select this record set field");
                            result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.StartIndex + 2, tmpPart, part.Description + Environment.NewLine + child.Description));
                        });
                    }
                    else
                    {
                        if (part.Children == null)
                        {
                            if (payload.Parent != null && payload.Parent.Payload.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal) >= 0 || (part.Name.Contains('(') && part.Name.Contains(')')))
                            {
                                var tmpPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(string.Empty, part.Name, true);
                                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.StartIndex + 2, tmpPart, tmpPart.Description + " / Select this record set"));
                            }
                            else
                            {
                                var tmpPart = IntellisenseFactory.CreateDataListValidationScalarPart(part.Name, part.Description + " / Select this variable");
                                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.StartIndex + 2, tmpPart, tmpPart.Description));
                            }
                        }
                    }
                });
            }

            internal void ProcessRegion(IParseTO payload, IList<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts, StringBuilder tmp, IList<IIntellisenseResult> result, IList<IDev2DataLanguageIntellisensePart> additionalParts = null)
            {
                const bool EmptyOk = false;
                if (payload != null)
                {
                    var tmpString = tmp.ToString();
                    var parts = tmpString.Split('.');
                    var search = parts[0].ToLower();
                    var rawSearch = search;
                    var isRs = false;

                    search = _recordSetUtil.RemoveRecordSetBraces(search, ref isRs);

                    if (_parserHelper.AddFieldResult(payload, result, tmpString, parts, isRs))
                    {
                        return;
                    }

                    if (parts.Length == 1)
                    {
                        _match.MatchNonFieldVariables(payload, refParts, addCompleteParts, tmp, result, additionalParts, isRs, rawSearch, search, EmptyOk, parts);
                    }
                    else if (parts.Length == 2)
                    {
                        _match.MatchFieldVariables(payload, refParts, addCompleteParts, result, parts, isRs, rawSearch, search, EmptyOk);
                    }
                    else
                    {
                        var part = IntellisenseFactory.CreateDataListValidationRecordsetPart(parts[0], parts[1]);
                        result.Add(IntellisenseFactory.CreateErrorResult(payload.StartIndex, payload.EndIndex, part, "Invalid Notation - Extra dots detected", enIntellisenseErrorCode.SyntaxError, !payload.HangingOpen));
                    }
                }
            }
        }
    }
}