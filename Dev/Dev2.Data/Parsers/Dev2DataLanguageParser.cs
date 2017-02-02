/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Data.Enums;
using Dev2.Data.Exceptions;
using Dev2.Data.Interfaces;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Interfaces;
using Warewolf.Resource.Errors;
// ReSharper disable NonLocalizedString
// ReSharper disable CatchAllClause
// ReSharper disable RedundantAssignment
// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable LoopCanBeConvertedToQuery

namespace Dev2.Data.Parsers
{
    /// <summary>
    /// The core language parser
    /// </summary>
    public class Dev2DataLanguageParser : IDev2DataLanguageParser, IDev2StudioDataLanguageParser
    {
        private const string CdataStart = @"<![CDATA[";
        private const string CdataEnd = @"]]>";
        private static volatile ConcurrentDictionary<Tuple<string, string>, IList<IIntellisenseResult>> _payloadCache = new ConcurrentDictionary<Tuple<string, string>, IList<IIntellisenseResult>>();
        private static volatile ConcurrentDictionary<string, IList<IIntellisenseResult>> _expressionCache = new ConcurrentDictionary<string, IList<IIntellisenseResult>>();

        private static IParserHelper _parserHelper;
        private static ICommonRecordSetUtil _recordSetUtil;

        #region Public Methods

        public Dev2DataLanguageParser()
        {
            _parserHelper = new ParserHelperUtil();
            _recordSetUtil = new CommonRecordSetUtil(this);
        }
        /// <summary>
        /// Parses the expression into parts.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="dataListParts">The data list parts.</param>
        /// <returns></returns>
        public IList<IIntellisenseResult> ParseExpressionIntoParts(string expression, IList<IDev2DataLanguageIntellisensePart> dataListParts)
        {
            return WrapAndClear(() =>
            {

                if (string.IsNullOrEmpty(expression) || dataListParts == null)
                {
                    return new List<IIntellisenseResult>();
                }
                var canCache = Regex.Matches(expression, "\\[\\[").Count == 1;
                if (canCache && _expressionCache.ContainsKey(expression))
                {
                    return _expressionCache[expression];
                }


                IList<IIntellisenseResult> result = PartsGeneration(expression, dataListParts, true);
                if (result != null && canCache && result.All(a => a.Type != enIntellisenseResultType.Error))
                    try
                    {
                        _expressionCache.TryAdd(expression, result);
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch { }

                return result;
            }, _expressionCache);
        }

        /// <summary>
        /// Used to extract intellisense options, and validate closed regions
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="dataList">The data list.</param>
        /// <param name="addCompleteParts">if set to <c>true</c> [add complete parts].</param>
        /// <param name="filterTo">The filter TO.</param>
        /// <param name="isFromIntellisense">if set to <c>true</c> [is from intellisense].</param>
        /// <returns></returns>
        public IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string payload, string dataList, bool addCompleteParts = false, IIntellisenseFilterOpsTO filterTo = null, bool isFromIntellisense = false)
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
                IList<IDev2DataLanguageIntellisensePart> parts = DataListFactory.GenerateIntellisensePartsFromDataList(dataList, filterTo);

                IList<IDev2DataLanguageIntellisensePart> additionalParts = new List<IDev2DataLanguageIntellisensePart>();
                if (filterTo != null && filterTo.FilterType == enIntellisensePartType.RecordsetsOnly)
                {
                    additionalParts = DataListFactory.GenerateIntellisensePartsFromDataList(dataList, new IntellisenseFilterOpsTO { FilterCondition = filterTo.FilterCondition, FilterType = enIntellisensePartType.All });
                }
                IList<IIntellisenseResult> result = PartsGeneration(payload, parts, addCompleteParts, isFromIntellisense, additionalParts);
                if (result != null && result.Count > 0 && result.All(a => a.Type != enIntellisenseResultType.Error))
                    try
                    {
                        _payloadCache.TryAdd(key, result);
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch { }
                return result;
            }, _payloadCache);

        }

        // ReSharper disable once InconsistentNaming
        private T WrapAndClear<T, U>(Func<T> runFunc, ConcurrentDictionary<U, T> clearIfException)
        {
            try
            {
                return runFunc();
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
                clearIfException.Clear();
                throw;
            }
        }



        /// <summary>
        /// Return a list of data list parts from the evaluated region
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public IList<string> ParseForActivityDataItems(string payload)
        {

            if (string.IsNullOrEmpty(payload))
            {
                return new List<string>();
            }

            IList<string> result = new List<string>();

            IList<ParseTO> parserList = MakeParts(payload); //Always Start from 0

            foreach (var parseObject in parserList)
            {
                var parseRef = parseObject;

                // build up a complete list of parts as per the payload
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

        #endregion

        #region Private Methods

        private IList<IIntellisenseResult> PartsGeneration(string payload, IList<IDev2DataLanguageIntellisensePart> parts, bool addCompleteParts, bool isFromIntellisense = false, IList<IDev2DataLanguageIntellisensePart> additionalParts = null)
        {
            IList<IIntellisenseResult> result = new List<IIntellisenseResult>();
            try
            {
                // remove the CDATA region first ;)
                if (payload.Contains(CdataStart))
                {
                    payload = payload.Replace(CdataStart, "");

                    int idx = payload.LastIndexOf(CdataEnd, StringComparison.Ordinal);

                    payload = payload.Substring(0, idx);
                }

                // short-circuit this case ;)
                if (payload.Equals("[[]]"))
                {
                    result.Add(IntellisenseFactory.CreateErrorResult(0, 4, null, ErrorResource.VariableIsMissing, enIntellisenseErrorCode.SyntaxError, true));
                    return result;
                }

                if (payload.Contains(DataListUtil.OpeningSquareBrackets))
                {

                    IList<ParseTO> rootItems = MakeParts(payload, addCompleteParts);

                    // we only want the last hanging open for evaluation
                    ParseTO magicRegion = null;

                    // we want to evaluate each closed region for validity
                    IList<ParseTO> evalParts = new List<ParseTO>();

                    rootItems
                        .ToList()
                        .ForEach(rootItem =>
                        {
                            ParseTO eval = rootItem;

                            while (eval != null)
                            {

                                // evaluate to find the last haging region
                                if (eval.HangingOpen)
                                {
                                    magicRegion = eval;
                                }

                                // evaluate to find all closed regions
                                if (!eval.HangingOpen && eval != magicRegion)
                                {
                                    evalParts.Add(eval);
                                }

                                eval = eval.Child;
                            }
                        });

                    // do the last hanging region intellisense injection
                    if (magicRegion != null)
                    {
                        result = isFromIntellisense ? ExtractActualIntellisenseOptions(magicRegion, parts, false) : ExtractIntellisenseOptions(magicRegion, parts, false);

                    }

                    // now process each closed region
                    evalParts
                        .ToList()
                        .ForEach(evalPart =>
                        {
                            IList<IIntellisenseResult> tmp = ExtractIntellisenseOptions(evalPart, parts, !isFromIntellisense && addCompleteParts, additionalParts);
                            if (tmp != null)
                            {
                                result = result.Union(tmp).ToList();
                            }
                        });
                }
            }
            catch (Dev2DataLanguageParseError e)
            {
                IDataListVerifyPart p = IntellisenseFactory.CreateDataListValidationScalarPart(payload);
                result.Add(IntellisenseFactory.CreateErrorResult(e.StartIndex, e.EndIndex, p, e.Message, e.ErrorCode, true));
            }

            return result;
        }

        /// <summary>
        /// Used to extract a syntax tree - Creates a list of trees for parsing
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="addCompleteParts">Setting this will allow open regions</param>
        /// <returns></returns>
        public IList<ParseTO> MakeParts(string payload, bool addCompleteParts = false)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return new List<ParseTO>();
            }

            char prev = '\0';
            StringBuilder region = new StringBuilder();
            bool openRegion = false;
            ParseTO currentNode = new ParseTO { Parent = null, HangingOpen = true };
            ParseTO root = currentNode;
            int i;
            payload = payload.Replace("]].[[", "]][[");
            IList<ParseTO> result = new List<ParseTO>();

            for (i = 0; i < payload.Length; i++)
            {
                var cur = payload[i];
                var shouldAddToRegion = ShouldAddToRegion(payload, cur, prev, i, true, '[');
                shouldAddToRegion = ShouldAddToRegion(payload, cur, prev, i, shouldAddToRegion, ']');

                if (cur == '[' && prev == '[')
                {
                    // we have an open region, evaluate
                    currentNode = openRegion ? CurrentNode(currentNode, region, i) : ParseTO(currentNode, i, result, ref root, ref openRegion);
                    cur = '\0';
                }

                if (cur == ']' && prev == ']')
                {
                    openRegion = ProcessOpenRegion(payload, openRegion, i, ref currentNode, ref region, ref cur);
                }


                if (openRegion && shouldAddToRegion && cur != '\0')
                {
                    region.Append(cur);
                }
                prev = cur;
            }

            if (openRegion)
            {
                currentNode.EndIndex = i - 1;
                currentNode.Payload = region.ToString();
            }

            // add last tree to list
            if (!result.Contains(root))
            {
                result.Add(root);
            }

            if (root.HangingOpen && addCompleteParts) //we have an open region but we evaluating for closed regions
            {
                throw new Dev2DataLanguageParseError(ErrorResource.InvalidCloseRegion, 0, payload.Length, enIntellisenseErrorCode.SyntaxError);
            }
            return result;
        }

        private static ParseTO ParseTO(ParseTO currentNode, int i, IList<ParseTO> result, ref ParseTO root, ref bool openRegion) //=> _parserHelper.ParseTO(currentNode, i, result, ref root, ref openRegion);
        {
            if (currentNode == root && !root.HangingOpen)
            {
                ParseTO newRoot = new ParseTO { HangingOpen = true, Parent = null, StartIndex = i, EndIndex = -1 };
                result.Add(root);
                root = newRoot;
                currentNode = root;
            }
            openRegion = true;
            currentNode.StartIndex = i + 1;
            return currentNode;
        }

        private static bool ProcessOpenRegion(string payload, bool openRegion, int i, ref ParseTO currentNode, ref StringBuilder region, ref char cur) => _parserHelper.ProcessOpenRegion(payload, openRegion, i, ref currentNode, ref region, ref cur);

        private static ParseTO CurrentNode(ParseTO currentNode, StringBuilder region, int i) => _parserHelper.CurrentNode(currentNode, region, i);

        private static bool ShouldAddToRegion(string payload, char cur, char prev, int i, bool shouldAddToRegion, char charToCheck) => _parserHelper.ShouldAddToRegion(payload, cur, prev, i, shouldAddToRegion, charToCheck);

        /// <summary>
        /// Extracts the actual intellisense options.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="refParts">The ref parts.</param>
        /// <param name="addCompleteParts">if set to <c>true</c> [add complete parts].</param>
        /// <returns></returns>
        private IList<IIntellisenseResult> ExtractActualIntellisenseOptions(ParseTO payload, IEnumerable<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts)
        {
            StringBuilder tmp = new StringBuilder(payload.Payload);
            IList<IIntellisenseResult> result = new List<IIntellisenseResult>();

            // region to evaluate
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (payload != null)
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            {
                string[] parts = tmp.ToString().Split('.');
                string search = parts[0].ToLower();
                bool isRs = search.Contains(DataListUtil.RecordsetIndexOpeningBracket);

                if (search.Contains(DataListUtil.RecordsetIndexOpeningBracket))
                {
                    isRs = true;
                    int pos = search.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
                    search = search.Substring(0, search.Length - (search.Length - pos));
                }

                try
                {
                    IEnumerable<IIntellisenseResult> results = CreateResultsGeneric(refParts, payload, parts.Length == 1 ? search : parts[1], addCompleteParts);

                    // we need to search recordset fields to filter ;)
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
                        // we want all options ;)
                        foreach (IIntellisenseResult res in results)
                        {
                            result.Add(res);
                        }
                    }
                }
                catch (Dev2DataLanguageParseError e)
                {
                    result.Add(AddErrorToResults(isRs, parts[0], e, !payload.HangingOpen));
                }
            }

            // filter out dups in the list
            IList<IIntellisenseResult> realResults = new List<IIntellisenseResult>();

            result
                .ToList()
                .ForEach(r =>
                {

                    bool addToFinal = true;

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

        /// <summary>
        /// Creates the results generic.
        /// </summary>
        /// <param name="refParts">The ref parts.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="search">The search.</param>
        /// <param name="addCompleteParts">if set to <c>true</c> [add complete parts].</param>
        /// <returns></returns>
        private IEnumerable<IIntellisenseResult> CreateResultsGeneric(IEnumerable<IDev2DataLanguageIntellisensePart> refParts, ParseTO payload, string search, bool addCompleteParts)
        {
            IList<IIntellisenseResult> result = new List<IIntellisenseResult>();

            if (payload.HangingOpen)
            {
                bool hasIndex = false;
                var openBraceIndex = search.LastIndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
                var closeBraceIndex = search.LastIndexOf(DataListUtil.RecordsetIndexClosingBracket, StringComparison.Ordinal);
                if (openBraceIndex != -1 && openBraceIndex < search.Length && closeBraceIndex > openBraceIndex)
                {
                    hasIndex = true;
                }

                if (payload.Payload.Contains(DataListUtil.RecordsetIndexOpeningBracket) && !payload.Payload.Contains(DataListUtil.RecordsetIndexClosingBracket) && payload.Child == null && !hasIndex)
                {
                    AddIndex(refParts, payload, search, result);
                }
                else
                {
                    foreach (IDev2DataLanguageIntellisensePart t in refParts)
                    {
                        string match = t.Name.ToLower();

                        if (t.Children != null && t.Children.Count > 0)
                        {
                            AddFieldOptions(payload, search, addCompleteParts, match, t, result);
                        }
                        else
                        {
                            if (!match.Contains(search)) continue;
                            AddFoundItems(payload, t, result);
                        }
                    }
                }
            }

            return result;
        }

        private static void AddFoundItems(ParseTO payload, IDev2DataLanguageIntellisensePart t, IList<IIntellisenseResult> result)
        {
            if (payload.Parent != null && payload.Parent.Payload.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal) >= 0)
            {
                IDataListVerifyPart part = IntellisenseFactory.CreateDataListValidationScalarPart(t.Name, !string.IsNullOrEmpty(t.Description) ? t.Description : " Use row at this index");

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

        private static void AddFieldOptions(ParseTO payload, string search, bool addCompleteParts, string match, IDev2DataLanguageIntellisensePart t, IList<IIntellisenseResult> result)
        {
            IDataListVerifyPart part;
            // only add hanging open if we want incomplete parts
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

        private static void AddIndex(IEnumerable<IDev2DataLanguageIntellisensePart> refParts, ParseTO payload, string search, IList<IIntellisenseResult> result)
        {
            //// allow the user to 
            foreach (IDev2DataLanguageIntellisensePart t in refParts)
            {
                // add closed recordset
                if (t.Children == null)
                {
                    // add index via scalar option
                    IDataListVerifyPart prt = IntellisenseFactory.CreateDataListValidationRecordsetPart(search, "", "", DataListUtil.OpeningSquareBrackets + t.Name + DataListUtil.ClosingSquareBrackets);
                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, prt, prt.Description));
                }
            }
        }

        /// <summary>
        /// Extracts the intellisense options.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="refParts">The ref parts.</param>
        /// <param name="addCompleteParts">if set to <c>true</c> [add complete parts].</param>
        /// <param name="additionalParts"></param>
        /// <returns></returns>
        /// <exception cref="Dev2DataLanguageParseError">Invalid syntax - [[ + payload.Payload + ]] is a recordset with out the (). Please use [[ + payload.Payload + ()]] instead.</exception>
        private IList<IIntellisenseResult> ExtractIntellisenseOptions(ParseTO payload, IList<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts, IList<IDev2DataLanguageIntellisensePart> additionalParts = null)
        {
            StringBuilder tmp = new StringBuilder(payload.Payload);
            IList<IIntellisenseResult> result = new List<IIntellisenseResult>();


            if (payload.Payload == string.Empty && payload.HangingOpen) //we have [[
            {
                ProcessForOnlyOpenRegion(payload, refParts, result);
            }
            else
            {
                // region to evaluate
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

            // filter out dups in the list
            IList<IIntellisenseResult> realResults = new List<IIntellisenseResult>();

            result.ToList().ForEach(r => ProcessResults(realResults, r));

            return result;
        }

        private static void ProcessResults(IList<IIntellisenseResult> realResults, IIntellisenseResult r) => _parserHelper.ProcessResults(realResults, r);

        private void ProcessRegion(ParseTO payload, IList<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts, StringBuilder tmp, IList<IIntellisenseResult> result, IList<IDev2DataLanguageIntellisensePart> additionalParts = null)
        {
            const bool EmptyOk = false;
            if (payload != null)
            {
                var tmpString = tmp.ToString();
                string[] parts = tmpString.Split('.');
                string search = parts[0].ToLower();
                string rawSearch = search;
                bool isRs = false;

                search = RemoveRecordSetBraces(search, ref isRs);
                if (AddFieldResult(payload, result, tmpString, parts, isRs))
                {
                    return;
                }

                if (parts.Length == 1)
                {
                    MatchNonFieldVariables(payload, refParts, addCompleteParts, tmp, result, additionalParts, isRs, rawSearch, search, EmptyOk, parts);
                }
                else if (parts.Length == 2)
                {

                    MatchFieldVariables(payload, refParts, addCompleteParts, result, parts, isRs, rawSearch, search, EmptyOk);
                }
                else
                {
                    IDataListVerifyPart part = IntellisenseFactory.CreateDataListValidationRecordsetPart(parts[0], parts[1]);
                    result.Add(IntellisenseFactory.CreateErrorResult(payload.StartIndex, payload.EndIndex, part, "Invalid Notation - Extra dots detected", enIntellisenseErrorCode.SyntaxError, !payload.HangingOpen));
                }
            }
        }

        private void MatchFieldVariables(ParseTO payload, IList<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts, IList<IIntellisenseResult> result, string[] parts, bool isRs, string rawSearch, string search, bool emptyOk)
        {
            ParseTO tmpTo = new ParseTO { Payload = parts[0], StartIndex = 0, EndIndex = parts[0].Length - 1 };

            var isRecName = isRs && rawSearch.Contains(DataListUtil.RecordsetIndexOpeningBracket) && rawSearch.EndsWith(DataListUtil.RecordsetIndexClosingBracket);

            const string DisplayString = "Recordset";
            if ((!isRecName || parts[1] == string.Empty) && payload.Child == null)
            {
                IList<IIntellisenseResult> intellisenseResults;
                if (ValidateName(rawSearch, DisplayString, result, out intellisenseResults))
                {
                    return;
                }
            }
            else
            {
                IList<IIntellisenseResult> intellisenseResults;
                if (ValidateName(search, DisplayString, result, out intellisenseResults))
                {
                    return;
                }
            }

            try
            {
                IsValidIndex(tmpTo);
            }
            catch (Dev2DataLanguageParseError e)
            {
                result.Add(AddErrorToResults(isRs, parts[0], e, !payload.HangingOpen));
            }

            IDev2DataLanguageIntellisensePart recordsetPart = refParts.FirstOrDefault(c => c.Name.ToLower() == search && c.Children != null);

            string display = parts[0];
            string partName = parts[0];
            int start = display.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
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
                IDataListVerifyPart part = IntellisenseFactory.CreateDataListValidationRecordsetPart(parts[0], "." + parts[1], true);
                result.Add(IntellisenseFactory.CreateErrorResult(payload.StartIndex, payload.EndIndex, part, " [[" + display + "]] contains a space, this is an invalid character for a variable name", enIntellisenseErrorCode.SyntaxError, !payload.HangingOpen));
            }
        }

        private string ProcessValidPartNameContainingFields(ParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, string[] parts, string search, bool emptyOk, string partName, IDev2DataLanguageIntellisensePart recordsetPart, string display)
        {
            if (partName.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal) >= 0)
            {
                partName = partName.Substring(0, partName.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal));
            }

            if (recordsetPart == null)
            {
                IDataListVerifyPart part = IntellisenseFactory.CreateDataListValidationRecordsetPart(partName, parts[1], "");
                result.Add(IntellisenseFactory.CreateErrorResult(payload.StartIndex, parts[0].Length - 1, part, DataListUtil.OpeningSquareBrackets + display + "]] does not exist in your variable list", enIntellisenseErrorCode.NeitherRecordsetNorFieldFound, !payload.HangingOpen));
            }
            else if (recordsetPart.Children != null && recordsetPart.Children.Count > 0)
            {
                if (ProcessFieldsForRecordSet(payload, addCompleteParts, result, parts, out search, out emptyOk, display, recordsetPart, partName))
                    return search;
            }

            if (result.Count == 0 && !emptyOk)
            {
                IDataListVerifyPart part = IntellisenseFactory.CreateDataListValidationRecordsetPart(parts[0], search);
                result.Add(char.IsNumber(search[0]) ? IntellisenseFactory.CreateErrorResult(payload.StartIndex, parts[0].Length - 1, part, "Invalid Expression: Recordset Field [ " + search + " ] starts with a number", enIntellisenseErrorCode.SyntaxError, !payload.HangingOpen) : IntellisenseFactory.CreateErrorResult(parts[0].Length, payload.EndIndex, part, "Recordset Field [ " + search + " ] does not exist for [ " + parts[0] + " ]", enIntellisenseErrorCode.FieldNotFound, !payload.HangingOpen));
            }
            return search;
        }

        private bool ProcessFieldsForRecordSet(ParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, string[] parts, out string search, out bool emptyOk, string display, IDev2DataLanguageIntellisensePart recordsetPart, string partName)
            => _parserHelper.ProcessFieldsForRecordSet(payload, addCompleteParts, result, parts, out search, out emptyOk, display, recordsetPart, partName);
        

        private void MatchNonFieldVariables(ParseTO payload, IList<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts, StringBuilder tmp, IList<IIntellisenseResult> result, IList<IDev2DataLanguageIntellisensePart> additionalParts, bool isRs, string rawSearch, string search, bool emptyOk, string[] parts)
        {
            try
            {
                var isRecName = isRs && rawSearch.Contains(DataListUtil.RecordsetIndexOpeningBracket) && rawSearch.EndsWith(DataListUtil.RecordsetIndexClosingBracket);
                if (!payload.HangingOpen)
                {
                    if (!isRecName)
                    {
                        if (ScalarMatch(result, isRs, rawSearch))
                            return;
                    }
                    else
                    {
                        if (RecordsetMatch(result, rawSearch, search))
                            return;
                    }
                }
                if ((rawSearch.Contains(DataListUtil.RecordsetIndexOpeningBracket) && IsValidIndex(payload)) || !rawSearch.Contains(DataListUtil.RecordsetIndexOpeningBracket))
                {
                    foreach (IDev2DataLanguageIntellisensePart t1 in refParts)
                    {
                        emptyOk = MatchVariablesWithNoFields(payload, refParts, addCompleteParts, result, isRs, rawSearch, search, emptyOk, parts, t1);
                    }

                    // now check recordset fields and add results
                    refParts.ToList().ForEach(pt =>
                    {
                        MatchChildren(payload, result, search, pt);
                    });

                    // final evaluation of the issue
                    if (result.Count == 0 && !emptyOk)
                    {
                        FinalEvaluation(payload, tmp, result, additionalParts, isRs);
                    }
                }
            }
            catch (Dev2DataLanguageParseError e)
            {
                result.Add(AddErrorToResults(isRs, parts[0], e, !payload.HangingOpen));
            }
        }

        private bool RecordsetMatch(IList<IIntellisenseResult> result, string rawSearch, string search)
        {
            IIntellisenseResult intellisenseResult = ValidateName(search == string.Empty ? rawSearch : search, "Variable"); //search is empty if there is a scalar value with start open (

            if (intellisenseResult != null)
            {
                result.Add(intellisenseResult);
                return true;
            }
            return false;
        }

        private bool ScalarMatch(IList<IIntellisenseResult> result, bool isRs, string rawSearch)
        {
            var intellisenseResult = ValidateName(rawSearch, isRs && !rawSearch.StartsWith(DataListUtil.RecordsetIndexOpeningBracket) ? "Recordset" : "Variable");
            if (intellisenseResult != null)
            {
                result.Add(intellisenseResult);
                return true;
            }
            return false;
        }

        private static void FinalEvaluation(ParseTO payload, StringBuilder tmp, IList<IIntellisenseResult> result, IList<IDev2DataLanguageIntellisensePart> additionalParts, bool isRs)
        {
            string display = tmp.ToString().Replace("]", "");
            IDataListVerifyPart part;

            enIntellisenseErrorCode code = enIntellisenseErrorCode.RecordsetNotFound;
            if (!isRs)
            {
                code = display.IndexOf(' ') >= 0 ? enIntellisenseErrorCode.SyntaxError : enIntellisenseErrorCode.ScalarNotFound;

                part = IntellisenseFactory.CreateDataListValidationScalarPart(display);
            }
            else
            {
                // extract (x)
                int start = display.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal);
                display = display.Substring(0, start);
                display += "()";
                part = IntellisenseFactory.CreateDataListValidationRecordsetPart(display, "");
            }
            // add error
            if (additionalParts == null)
                result.Add(!display.Contains(' ') ? IntellisenseFactory.CreateErrorResult(payload.StartIndex, payload.EndIndex, part, " [[" + display + "]] does not exist in your variable list", code, !payload.HangingOpen) : IntellisenseFactory.CreateErrorResult(payload.StartIndex, payload.EndIndex, part, " [[" + display + "]] contains a space, this is an invalid character for a variable name", code, !payload.HangingOpen));
            else if (!additionalParts.Select(a => a.Name).Contains(display))
            {
                result.Add(!display.Contains(' ') ? IntellisenseFactory.CreateErrorResult(payload.StartIndex, payload.EndIndex, part, " [[" + display + "]] does not exist in your variable list", code, !payload.HangingOpen) : IntellisenseFactory.CreateErrorResult(payload.StartIndex, payload.EndIndex, part, " [[" + display + "]] contains a space, this is an invalid character for a variable name", code, !payload.HangingOpen));
            }
        }

        private static void MatchChildren(ParseTO payload, IList<IIntellisenseResult> result, string search, IDev2DataLanguageIntellisensePart pt)
        {
            // now eval each set of children
            pt.Children?.ToList().ForEach(child =>
            {
                string match = child.Name.ToLower();

                // add each child match
                if (match.Contains(search))
                {
                    IDataListVerifyPart resultPt = IntellisenseFactory.CreateDataListValidationRecordsetPart(pt.Name, child.Name, pt.Description + " / " + child.Description + " Select this recordset field field");
                    IIntellisenseResult tmpChild = IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, resultPt, resultPt.Description);

                    // only add if not picked up already
                    if (result
                        .ToList()
                        .Find(r => r.Option.DisplayValue == tmpChild.Option.DisplayValue) == null)
                    {
                        result.Add(tmpChild);
                    }
                }
            });
        }

        private bool MatchVariablesWithNoFields(ParseTO payload, IList<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts, IList<IIntellisenseResult> result, bool isRs, string rawSearch, string search, bool emptyOk, string[] parts, IDev2DataLanguageIntellisensePart t1)
        {
            string match = t1.Name.ToLower();

            if (match.Contains(search) && (match != search))
            {
                if (t1.Children != null && t1.Children.Count > 0)
                {
                    ProcessRecordSetFields(payload, addCompleteParts, result, t1);
                }
                else
                {
                    ProcessNonRecordsetFields(payload, result, t1);
                }
            }
            else if (match == search && isRs)
            {
                if (rawSearch.Contains(DataListUtil.RecordsetIndexOpeningBracket) && rawSearch.Contains(DataListUtil.RecordsetIndexClosingBracket))
                {
                    emptyOk = RecordsetMatch(payload, addCompleteParts, result, rawSearch, search, emptyOk, parts, t1);
                }
                else
                {
                    emptyOk = ProcessForChild(payload, refParts, result, search, t1);
                }
            }
            else if (match == search && !isRs)
            {
                if (t1.Children != null && t1.Children.Count > 0)
                {
                    ReturnFieldMatchForRecordSet(payload, result, t1);
                }
                else
                {
                    // handle scalar matches
                    emptyOk = HandleScalarMatches(payload, addCompleteParts, result, search, t1, match);
                }
            }
            return emptyOk;
        }

        private static bool ProcessForChild(ParseTO payload, IList<IDev2DataLanguageIntellisensePart> refParts, IList<IIntellisenseResult> result, string search, IDev2DataLanguageIntellisensePart t1)
        {
            bool emptyOk = false;
            bool isHangingChild = payload.Child != null && payload.Child.HangingOpen;

            if (!payload.IsLeaf && !isHangingChild)
            {
                OpenRecordsetItem(payload, result, t1);
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

        private static bool HandleScalarMatches(ParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, string search, IDev2DataLanguageIntellisensePart t1, string match)
        {
            bool emptyOk = false;
            if (search != match || (search == match && addCompleteParts))
            {
                // user wants to set index via a scalar, allow it
                if (payload.Parent != null && payload.Parent.Payload.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal) >= 0)
                {
                    IDataListVerifyPart p = IntellisenseFactory.CreateDataListValidationScalarPart(t1.Name);
                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, p, " / Select a specific row "));
                }
                else
                {
                    IDataListVerifyPart p = IntellisenseFactory.CreateDataListValidationScalarPart(t1.Name);
                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, p, t1.Description));
                }
            }
            else
            {
                emptyOk = true;
            }
            return emptyOk;
        }

        private static void ReturnFieldMatchForRecordSet(ParseTO payload, IList<IIntellisenseResult> result, IDev2DataLanguageIntellisensePart t1)
        {
            // we hav a recordset, return options
            IDataListVerifyPart part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t1.Name, "", t1.Description);
            result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
            // add all children
            IList<IDev2DataLanguageIntellisensePart> children = t1.Children;
            if (children != null)
            {
                foreach (IDev2DataLanguageIntellisensePart t in children)
                {
                    part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t1.Name, t.Name, t.Description + " / Use a field of the Recordset");
                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                }
            }
        }

        private static void OpenRecordsetItem(ParseTO payload, IList<IIntellisenseResult> result, IDev2DataLanguageIntellisensePart t1)
            => _recordSetUtil.OpenRecordsetItem(payload, result, t1);
       

        private static bool RecordsetMatch(ParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, string rawSearch, string search, bool emptyOk, string[] parts, IDev2DataLanguageIntellisensePart t1)
            => _recordSetUtil.RecordsetMatch(payload, addCompleteParts, result, rawSearch, search, emptyOk, parts, t1);
       
        private static void ProcessNonRecordsetFields(ParseTO payload, IList<IIntellisenseResult> result, IDev2DataLanguageIntellisensePart t1)
            => _recordSetUtil.ProcessNonRecordsetFields(payload, result, t1);
        

        private void ProcessRecordSetFields(ParseTO payload, bool addCompleteParts, IList<IIntellisenseResult> result, IDev2DataLanguageIntellisensePart t1)
            => _recordSetUtil.ProcessRecordSetFields(payload, addCompleteParts, result, t1);
      

        private bool AddFieldResult(ParseTO payload, IList<IIntellisenseResult> result, string tmpString, string[] parts, bool isRs)
            => _parserHelper.AddFieldResult(payload, result, tmpString, parts, isRs);
      

        private static string RemoveRecordSetBraces(string search, ref bool isRs) => _recordSetUtil.RemoveRecordSetBraces(search, ref isRs);

        private void ProcessForOnlyOpenRegion(ParseTO payload, IEnumerable<IDev2DataLanguageIntellisensePart> refParts, IList<IIntellisenseResult> result)
        {
            bool addAll = !(payload.Parent != null && payload.Parent.IsRecordSet);

            // opened region, return the entire list
            refParts.ToList().ForEach(part =>
                {
                    // only add children of recordset if parent not a region within a recordset

                    if (part.Children != null && part.Children.Count > 0 && addAll)
                    {
                        // add recordset
                        //19.09.2012: massimo.guerrera - Added the description for the data list item
                        IDataListVerifyPart tmpPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(part.Name, "", part.Description + " / Select this record set");
                        result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.StartIndex + 2, tmpPart, tmpPart.Description));
                        // add each child
                        part.Children
                            .ToList()
                            .ForEach(child =>
                            {
                                //19.09.2012: massimo.guerrera - Added the description for the data list item
                                tmpPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(part.Name, child.Name, child.Description + " / Select this record set field");
                                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.StartIndex + 2, tmpPart, part.Description + Environment.NewLine + child.Description));
                            });
                    }
                    else
                    {
                        // add scalar
                        if (part.Children == null)
                        {
                            // Travis.Frisinger : 19.10.2012  - Improved Intellisense results
                            if (payload.Parent != null && payload.Parent.Payload.IndexOf(DataListUtil.RecordsetIndexOpeningBracket, StringComparison.Ordinal) >= 0 || (part.Name.Contains('(') && part.Name.Contains(')')))
                            {
                                // add recordset descriptions
                                IDataListVerifyPart tmpPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(string.Empty, part.Name, true);
                                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.StartIndex + 2, tmpPart, tmpPart.Description + " / Select this record set"));
                            }
                            else
                            {
                                //19.09.2012: massimo.guerrera - Added the description for the data list item
                                IDataListVerifyPart tmpPart = IntellisenseFactory.CreateDataListValidationScalarPart(part.Name, part.Description + " / Select this variable");
                                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.StartIndex + 2, tmpPart, tmpPart.Description));
                            }
                        }
                    }
                });
        }        

        private bool ValidateName(string rawSearch, string displayString, IList<IIntellisenseResult> result, out IList<IIntellisenseResult> intellisenseResults)
            => _parserHelper.ValidateName(rawSearch, displayString, result, out intellisenseResults);

        public IIntellisenseResult ValidateName(string name, string displayString) => _parserHelper.ValidateName(name, displayString);
       
        /// <summary>
        /// Adds the error to results.
        /// </summary>
        /// <param name="isRs">if set to <c>true</c> [is RS].</param>
        /// <param name="part">The part.</param>
        /// <param name="e">The e.</param>
        /// <param name="isOpen">if set to <c>true</c> [is open].</param>
        /// <returns></returns>
        private IIntellisenseResult AddErrorToResults(bool isRs, string part, Dev2DataLanguageParseError e, bool isOpen) => _parserHelper.AddErrorToResults(isRs, part, e, isOpen);


        /// <summary>
        /// Determines whether [is valid index] [the specified to].
        /// </summary>
        /// <param name="to">To.</param>
        /// <returns>
        ///   <c>true</c> if [is valid index] [the specified to]; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="Dev2DataLanguageParseError">
        /// Recordset index [  + part +  ] is not greater than zero
        /// or
        /// or
        /// Recordset index [  + part +  ] is not greater than zero
        /// or
        /// </exception>
        private bool IsValidIndex(ParseTO to) => _parserHelper.IsValidIndex(to);

        // ReSharper disable once UnusedMember.Local
        private bool CheckValidIndex(ParseTO to, string part, int start, int end) => _parserHelper.CheckValidIndex(to, part, start, end);
        // ReSharper disable once UnusedMember.Local
        private bool CheckCurrentIndex(ParseTO to, int start, string raw, int end) => _parserHelper.CheckCurrentIndex(to, start, raw, end);
        #endregion
    }
}
