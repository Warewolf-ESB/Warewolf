using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Data.Enums;
using Dev2.Data.Interfaces;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Interfaces;

namespace Dev2.Data.Parsers
{
    /// <summary>
    /// The core language parser ;)
    /// </summary>
    public class Dev2DataLanguageParser : IDev2DataLanguageParser, IDev2StudioDataLanguageParser
    {
        private const string _cdataStart = "<![CDATA[";
        private const string _cdataEnd = "]]>";

        #region Public Methods


        /// <summary>
        /// Parses the expression into parts.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="dataListParts">The data list parts.</param>
        /// <returns></returns>
        public IList<IIntellisenseResult> ParseExpressionIntoParts(string expression, IList<IDev2DataLanguageIntellisensePart> dataListParts)
        {
            IList<IIntellisenseResult> result = PartsGeneration(expression, dataListParts, true);

            return result;
        }

        /// <summary>
        /// Used to extract intellisense options, and validate closed regions
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="dataList">The data list.</param>
        /// <param name="addCompleteParts">if set to <c>true</c> [add complete parts].</param>
        /// <param name="filterTO">The filter TO.</param>
        /// <param name="isFromIntellisense">if set to <c>true</c> [is from intellisense].</param>
        /// <returns></returns>
        public IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string payload, string dataList, bool addCompleteParts = false, IntellisenseFilterOpsTO filterTO = null, bool isFromIntellisense = false)
        {
            IList<IDev2DataLanguageIntellisensePart> parts = DataListFactory.GenerateIntellisensePartsFromDataList(dataList, filterTO);

            IList<IIntellisenseResult> result = PartsGeneration(payload, parts, addCompleteParts, isFromIntellisense);

            return result;
        }

        /// <summary>
        /// Used to compare data list items to IO mapping
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="dataList"></param>
        /// <returns></returns>
        public IList<IIntellisenseResult> ParseForMissingDataListItems(IList<IDataListVerifyPart> parts, string dataList)
        {

            IList<IIntellisenseResult> result = new List<IIntellisenseResult>();
            IList<IDev2DataLanguageIntellisensePart> dlParts = DataListFactory.GenerateIntellisensePartsFromDataList(dataList);

            foreach(IDataListVerifyPart t in parts)
            {
                string tmpVal = t.DisplayValue;
                if(tmpVal.Contains("[[") || tmpVal.Contains("]]"))
                {
                    // it is a data region, evaluate it was such
                    tmpVal = tmpVal.Replace("]]", "").Replace("[[", "");
                    ParseTO tmp = new ParseTO { Payload = tmpVal, StartIndex = 2, EndIndex = (tmpVal.Length - 1), HangingOpen = false, Parent = null, Child = null };

                    IList<IIntellisenseResult> tmpResult = ExtractIntellisenseOptions(tmp, dlParts, false);
                    result = result.Union(tmpResult).ToList();
                }
                else
                {
                    if(tmpVal.Contains("[[") && !tmpVal.Contains("]]"))
                    {
                        IDataListVerifyPart p = IntellisenseFactory.CreateDataListValidationScalarPart(tmpVal);
                        result.Add(IntellisenseFactory.CreateErrorResult(0, tmpVal.Length, p, "Invalid syntax - Open ( [[ ) without a Close ( ]] ).", enIntellisenseErrorCode.SyntaxError, true));
                    }
                    else if(tmpVal.Contains("]]") && !tmpVal.Contains("[["))
                    {
                        IDataListVerifyPart p = IntellisenseFactory.CreateDataListValidationScalarPart(tmpVal);
                        result.Add(IntellisenseFactory.CreateErrorResult(0, tmpVal.Length, p, "Invalid syntax - Close ( ]] ) without an Open ( [[ ).", enIntellisenseErrorCode.SyntaxError, true));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Return a list of data list parts from the evaluated region
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public IList<String> ParseForActivityDataItems(string payload)
        {
            IList<String> result = new List<String>();

            IList<ParseTO> parserList = MakeParts(payload); //Always Start from 0

            foreach(var parseObject in parserList)
            {
                var parseRef = parseObject;

                // build up a complete list of parts as per the payload
                while(parseRef != null)
                {
                    if(!result.Contains(parseRef.Payload))
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

        private IList<IIntellisenseResult> PartsGeneration(string payload, IList<IDev2DataLanguageIntellisensePart> parts, bool addCompleteParts, bool isFromIntellisense = false)
        {
            IList<IIntellisenseResult> result = new List<IIntellisenseResult>();
            try
            {
                // remove the CDATA region first ;)
                if(payload.Contains(_cdataStart))
                {
                    payload = payload.Replace(_cdataStart, "");

                    int idx = payload.LastIndexOf(_cdataEnd, StringComparison.Ordinal);

                    payload = payload.Substring(0, idx);
                }

                if(payload.Contains("[["))
                {

                    IList<ParseTO> rootItems = MakeParts(payload);

                    // we only want the last hanging open for evaluation
                    ParseTO magicRegion = null;

                    // we want to evaluate each closed region for validity
                    IList<ParseTO> evalParts = new List<ParseTO>();

                    rootItems
                        .ToList()
                        .ForEach(rootItem =>
                        {
                            ParseTO eval = rootItem;

                            while(eval != null)
                            {

                                // evaluate to find the last haging region
                                if(eval.HangingOpen)
                                {
                                    magicRegion = eval;
                                }

                                // evaluate to find all closed regions
                                if(!eval.HangingOpen && eval != magicRegion)
                                {
                                    evalParts.Add(eval);
                                }

                                eval = eval.Child;
                            }
                        });

                    // do the last hanging region intellisense injection
                    if(magicRegion != null)
                    {
                        if(isFromIntellisense)
                        {
                            result = ExtractActualIntellisenseOptions(magicRegion, parts, false);
                        }
                        else
                        {
                            result = ExtractIntellisenseOptions(magicRegion, parts, false);
                        }

                    }

                    // now process each closed region
                    evalParts
                        .ToList()
                        .ForEach(evalPart =>
                        {
                            IList<IIntellisenseResult> tmp;
                            if(isFromIntellisense)
                            {
                                tmp = ExtractActualIntellisenseOptions(evalPart, parts, false);
                            }
                            else
                            {
                                tmp = ExtractIntellisenseOptions(evalPart, parts, addCompleteParts);
                            }
                            if(tmp != null)
                            {
                                result = result.Union(tmp).ToList();
                            }
                        });
                }
            }
            catch(Dev2DataLanguageParseError e)
            {
                IDataListVerifyPart p = IntellisenseFactory.CreateDataListValidationScalarPart(payload);
                result.Add(IntellisenseFactory.CreateErrorResult(e.StartIndex, e.EndIndex, p, e.Message, e.ErrorCode, true));
            }

            return result;
        }

        public IList<ParseTO> MakePartsWithOutRecsetIndex(string payload)
        {
            //remove index
            var recordSetIndex = DataListUtil.ExtractIndexRegionFromRecordset(payload);
            IList<ParseTO> results;
            if(!string.IsNullOrEmpty(recordSetIndex))
            {
                results = MakeParts(payload);
                foreach(ParseTO t in results.Where(t => t.Child != null && !string.IsNullOrEmpty(t.Child.Payload)))
                {
                    //replace index
                    t.Child.Payload = recordSetIndex;
                }
            }
            else
            {
                results = MakeParts(payload);
            }
            return results;
        }

        /// <summary>
        /// Used to extract a syntax tree - Creates a list of trees for parsing
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public IList<ParseTO> MakeParts(string payload)
        {
            char prev = '\0';
            StringBuilder region = new StringBuilder();
            bool openRegion = false;
            ParseTO currentNode = new ParseTO { Parent = null, HangingOpen = true };
            ParseTO root = currentNode;
            int i;

            IList<ParseTO> result = new List<ParseTO>();

            for(i = 0; i < payload.Length; i++)
            {
                char cur = payload[i];

                if(cur == '[' && prev == '[')
                {
                    // we have an open region, evaluate
                    if(openRegion)
                    {
                        // time to branch
                        currentNode.Payload = region.ToString();
                        region.Clear();
                        ParseTO child = new ParseTO();
                        currentNode.Child = child;
                        child.HangingOpen = true;
                        child.Parent = currentNode;
                        child.EndIndex = -1;
                        child.StartIndex = i;
                        currentNode = child;
                    }
                    else
                    {
                        if(currentNode == root && !root.HangingOpen)
                        {
                            // we have populated a root, now we need to allocate a new root
                            ParseTO newRoot = new ParseTO { HangingOpen = true, Parent = null, StartIndex = i, EndIndex = -1 };
                            // finally add current root to List
                            result.Add(root);
                            root = newRoot;
                            currentNode = root;
                        }
                        openRegion = true;
                        currentNode.StartIndex = (i + 1);
                    }
                }

                if(cur == ']' && prev == ']')
                {
                    if(openRegion)
                    {
                        openRegion = false;
                        currentNode.EndIndex = i;
                        currentNode.HangingOpen = false;
                        currentNode.Payload = region.ToString();
                        region.Clear();
                        currentNode.EndIndex = (i - 2);

                        if(!currentNode.IsRoot)
                        {
                            currentNode = currentNode.Parent;
                            region = new StringBuilder(currentNode.Payload);
                            openRegion = true;
                        }
                        else if(currentNode.IsRoot && (!currentNode.IsLeaf && currentNode.Child.HangingOpen))
                        {
                            throw new Dev2DataLanguageParseError("Invalid syntax - You need to close ( ]] ) your Data List Reference", 0, payload.Length, enIntellisenseErrorCode.SyntaxError);
                        }
                    }
                    else
                    {
                        throw new Dev2DataLanguageParseError("Invalid syntax - You have a close ( ]] ) without a related open ( [[ )", 0, payload.Length, enIntellisenseErrorCode.SyntaxError);
                    }
                }

                if(openRegion && cur != '[' && cur != ']')
                {
                    region.Append(cur);
                }
                prev = cur;
            }

            if(openRegion)
            {
                currentNode.EndIndex = (i - 1);
                currentNode.Payload = region.ToString();
            }

            // add last tree to list
            if(!result.Contains(root))
            {
                result.Add(root);
            }

            return result;
        }

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
            if(payload != null)
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            {
                string[] parts = tmp.ToString().Split('.');
                string search = parts[0].ToLower();
                bool isRs = false;

                // remove ()
                if(search.Contains("("))
                {
                    isRs = true;
                    int pos = search.IndexOf("(", StringComparison.Ordinal);
                    search = search.Substring(0, (search.Length - (search.Length - pos)));
                }

                try
                {
                    var results = CreateResultsGeneric(refParts, payload, search, addCompleteParts);

                    // we need to search recordset fields to filter ;)
                    if(parts.Length == 2)
                    {
                        var cmp = parts[1].ToLower();

                        foreach(IIntellisenseResult res in results)
                        {
                            if(res.Option.Field.ToLower().IndexOf(cmp, StringComparison.Ordinal) >= 0)
                            {
                                result.Add(res);
                            }
                        }
                    }
                    else
                    {
                        // we want all options ;)
                        foreach(IIntellisenseResult res in results)
                        {
                            result.Add(res);
                        }
                    }
                }
                catch(Dev2DataLanguageParseError e)
                {
                    result.Add(AddErrorToResults(isRs, parts[0], e, (!payload.HangingOpen)));
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
                            if(rr.Option.DisplayValue == r.Option.DisplayValue)
                            {
                                addToFinal = false;
                            }

                        });

                    if(addToFinal)
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

            if(payload.HangingOpen)
            {

                if(payload.Payload.Contains("(") && !payload.Payload.Contains(")") && payload.Child == null)
                {
                    // allow the user to 
                    foreach(IDev2DataLanguageIntellisensePart t in refParts)
                    {
                        // add closed recordset
                        if(t.Children == null)
                        {
                            // add index via scalar option
                            IDataListVerifyPart prt = IntellisenseFactory.CreateDataListValidationRecordsetPart(search, "", "", "[[" + t.Name + "]]");
                            result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, prt, prt.Description));
                        }
                    }
                }
                else
                {
                    foreach(IDev2DataLanguageIntellisensePart t in refParts)
                    {
                        string match = t.Name.ToLower();

                        if(t.Children != null && t.Children.Count > 0)
                        {
                            IDataListVerifyPart part;
                            // only add hanging open if we want incomplete parts
                            if(!addCompleteParts && match.Contains(search))
                            {
                                part = IntellisenseFactory.CreateDataListValidationScalarPart(t.Name + "(", ((!string.IsNullOrEmpty(t.Description)) ? t.Description : " Select a specific row"));

                                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));

                                foreach(IDev2DataLanguageIntellisensePart t1 in t.Children)
                                {
                                    part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t.Name, t1.Name, ((!string.IsNullOrEmpty(t1.Description)) ? t1.Description : " Input: Use last row, Result: Append new record"));
                                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                                    part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t.Name, t1.Name, ((!string.IsNullOrEmpty(t1.Description)) ? t1.Description : " Use all the rows"), "*");
                                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                                }
                                continue;
                            }
                            foreach(IDev2DataLanguageIntellisensePart t1 in t.Children)
                            {
                                if(t1.Name.Contains(search))
                                {
                                    part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t.Name, t1.Name, ((!string.IsNullOrEmpty(t1.Description)) ? t1.Description : " Input: Use last row, Result: Append new record"));
                                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                                    part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t.Name, t1.Name, ((!string.IsNullOrEmpty(t1.Description)) ? t1.Description : " Use all the rows"), "*");
                                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                                }
                            }
                        }
                        else
                        {
                            if(match.Contains(search))
                            {
                                if(payload.Parent != null && payload.Parent.Payload.IndexOf("(", StringComparison.Ordinal) >= 0)
                                {
                                    IDataListVerifyPart part = IntellisenseFactory.CreateDataListValidationScalarPart(t.Name, (!string.IsNullOrEmpty(t.Description)) ? t.Description : " Use row at this index");

                                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                                }
                                else
                                {
                                    IDataListVerifyPart part;
                                    if(t.Name.Contains('(') && t.Name.Contains(')'))
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
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Extracts the intellisense options.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="refParts">The ref parts.</param>
        /// <param name="addCompleteParts">if set to <c>true</c> [add complete parts].</param>
        /// <returns></returns>
        /// <exception cref="Dev2DataLanguageParseError">Invalid syntax - [[ + payload.Payload + ]] is a recordset with out the (). Please use [[ + payload.Payload + ()]] instead.</exception>
        private IList<IIntellisenseResult> ExtractIntellisenseOptions(ParseTO payload, IList<IDev2DataLanguageIntellisensePart> refParts, bool addCompleteParts)
        {
            StringBuilder tmp = new StringBuilder(payload.Payload);
            IList<IIntellisenseResult> result = new List<IIntellisenseResult>();

            bool emptyOk = false;

            if(payload.Payload == string.Empty && payload.HangingOpen)
            {
                bool addAll = !(payload.Parent != null && payload.Parent.IsRecordSet);

                // opened region, return the entire list
                refParts
                    .ToList()
                    .ForEach(part =>
                    {
                        // only add children of recordset if parent not a region within a recordset

                        if(part.Children != null && part.Children.Count > 0 && addAll)
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
                            if(part.Children == null)
                            {
                                // Travis.Frisinger : 19.10.2012  - Improved Intellisense results
                                if(payload.Parent != null && payload.Parent.Payload.IndexOf("(", StringComparison.Ordinal) >= 0 || (part.Name.Contains('(') && part.Name.Contains(')')))
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
            else
            {
                // region to evaluate
                if(tmp.Length > 0)
                {

                    // ReSharper disable ConditionIsAlwaysTrueOrFalse
                    if(payload != null)
                    // ReSharper restore ConditionIsAlwaysTrueOrFalse
                    {
                        string[] parts = tmp.ToString().Split('.');
                        string search = parts[0].ToLower();
                        string rawSearch = search;
                        bool isRS = false;

                        // remove ()
                        if(search.Contains("("))
                        {
                            isRS = true;
                            int pos = search.IndexOf("(", StringComparison.Ordinal);
                            search = search.Substring(0, (search.Length - (search.Length - pos)));
                        }

                        if(parts.Length == 1)
                        {
                            // we have scalar or recordset name lookup happening

                            // record set with valid index or scalar
                            try
                            {
                                if((rawSearch.Contains("(") && IsValidIndex(payload)) || (!rawSearch.Contains("(")))
                                {


                                    // match all RS and scalars
                                    foreach(IDev2DataLanguageIntellisensePart t1 in refParts)
                                    {
                                        string match = t1.Name.ToLower();

                                        if(match.Contains(search) && (match != search))
                                        {

                                            if(t1.Children != null && t1.Children.Count > 0)
                                            {

                                                IDataListVerifyPart part;

                                                // only add hanging open if we want incomplete parts
                                                if(!addCompleteParts)
                                                {
                                                    part = IntellisenseFactory.CreateDataListValidationScalarPart(t1.Name + "(", (t1.Description + " / Select a specific row or Close"));

                                                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                                                }

                                                part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t1.Name, "", t1.Description + " / Takes all rows ", "*");
                                                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));

                                                part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t1.Name, "", t1.Description + " / Take last row");

                                                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                                                // add all children for them
                                                foreach(IDev2DataLanguageIntellisensePart t in t1.Children)
                                                {
                                                    part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t1.Name, t.Name, (t1.Description + " / Use the field of a Recordset"));
                                                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                                                }
                                            }
                                            else
                                            {

                                                if(payload.Parent != null && payload.Parent.Payload.IndexOf("(", StringComparison.Ordinal) >= 0)
                                                {
                                                    IDataListVerifyPart part = IntellisenseFactory.CreateDataListValidationScalarPart(t1.Name, t1.Description + " / Use row at this index");

                                                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                                                }
                                                else
                                                {
                                                    IDataListVerifyPart part;
                                                    if(t1.Name.Contains('(') && t1.Name.Contains(')'))
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
                                        }
                                        else if(match == search && isRS)
                                        {
                                            if(rawSearch.Contains("(") && rawSearch.Contains(")"))
                                            {

                                                if(payload.HangingOpen)
                                                { // only process if it is an open region
                                                    // we need to add all children
                                                    string idx;
                                                    if(!payload.IsLeaf && !payload.Child.HangingOpen)
                                                    {
                                                        idx = "[[" + payload.Child.Payload + "]]";
                                                    }
                                                    else
                                                    {
                                                        // we need to extract the index
                                                        idx = DataListUtil.ExtractIndexRegionFromRecordset(rawSearch);
                                                    }
                                                    // add general closed recordset
                                                    string rsName = search;
                                                    if(idx == string.Empty)
                                                    {
                                                        rsName = payload.Payload;
                                                    }
                                                    IDataListVerifyPart part = IntellisenseFactory.CreateDataListValidationRecordsetPart(rsName, "", t1.Description + " / Select a specific row", idx);
                                                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));

                                                    IList<IDev2DataLanguageIntellisensePart> children = t1.Children;
                                                    if(children != null)
                                                    {
                                                        foreach(IDev2DataLanguageIntellisensePart t in children)
                                                        {
                                                            part = IntellisenseFactory.CreateDataListValidationRecordsetPart(rsName, t.Name, t.Description + " / Select a specific field at a specific row", idx);
                                                            result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    // add in recordset with index if around
                                                    if(addCompleteParts)
                                                    {
                                                        IDataListVerifyPart p;

                                                        string idx = DataListUtil.ExtractIndexRegionFromRecordset(parts[0]);
                                                        string recset = DataListUtil.ExtractRecordsetNameFromValue(parts[0]);

                                                        if(payload.Child != null)
                                                        {
                                                            p = IntellisenseFactory.CreateDataListValidationRecordsetPart(recset, string.Empty, t1.Description, payload.Child.Payload);
                                                        }
                                                        else
                                                        {
                                                            p = IntellisenseFactory.CreateDataListValidationRecordsetPart(recset, string.Empty, t1.Description, idx);
                                                        }

                                                        result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, p, p.Description));

                                                    }
                                                    else
                                                    {
                                                        emptyOk = true;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // hanging open recordset, allow the user to close it

                                                // hanging RS with closed idx
                                                bool isHangingChild = payload.Child != null && payload.Child.HangingOpen;

                                                if(!payload.IsLeaf && !isHangingChild)
                                                {

                                                    // clean up  [[recset([[scalar).f2]]
                                                    string indx = payload.Child.Payload;
                                                    int end = indx.IndexOf(")", StringComparison.Ordinal);
                                                    if(end > 0)
                                                    {
                                                        // malformed index -- correct it
                                                        indx = indx.Substring(0, end);
                                                    }

                                                    indx = DataListUtil.AddBracketsToValueIfNotExist(indx);

                                                    string rs = payload.Payload;
                                                    end = rs.IndexOf("(", StringComparison.Ordinal);
                                                    if(end > 0)
                                                    {
                                                        rs = rs.Substring(0, end);
                                                    }

                                                    IDataListVerifyPart prt = IntellisenseFactory.CreateDataListValidationRecordsetPart(rs, "", " / Select a specific row", indx);

                                                    result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, prt, prt.Description));

                                                    // now add all fields to collection too ;)
                                                    if(t1.Children != null)
                                                    {
                                                        IList<IDev2DataLanguageIntellisensePart> cParts = t1.Children;
                                                        foreach(IDev2DataLanguageIntellisensePart t in cParts)
                                                        {
                                                            prt = IntellisenseFactory.CreateDataListValidationRecordsetPart(rs, t.Name, " / Select a specific row", indx);
                                                            result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, prt, prt.Description));
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if(isRS && payload.Child == null)
                                                    {
                                                        // allow the user to 
                                                        IDataListVerifyPart prt;
                                                        foreach(IDev2DataLanguageIntellisensePart t in refParts.Where(t => t.Children == null))
                                                        {
                                                            // add index via scalar option
                                                            prt = IntellisenseFactory.CreateDataListValidationRecordsetPart(search, "", " / Select a specific row", "[[" + t.Name + "]]");
                                                            result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, prt, prt.Description));
                                                        }

                                                        // add star option
                                                        prt = IntellisenseFactory.CreateDataListValidationRecordsetPart(search, "", " / Reference all rows in the Recordset ", "*");
                                                        result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, prt, prt.Description));
                                                    }

                                                    emptyOk = true;
                                                }
                                            }
                                        }
                                        else if(match == search && !isRS)
                                        {

                                            // check to see if match is a recordset and return it as such
                                            if(t1.Children != null && t1.Children.Count > 0)
                                            {
                                                // we hav a recordset, return options
                                                IDataListVerifyPart part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t1.Name, "", t1.Description);
                                                result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                                                // add all children
                                                IList<IDev2DataLanguageIntellisensePart> children = t1.Children;
                                                if(children != null)
                                                {
                                                    foreach(IDev2DataLanguageIntellisensePart t in children)
                                                    {
                                                        part = IntellisenseFactory.CreateDataListValidationRecordsetPart(t1.Name, t.Name, t.Description + " / Use a field of the Recordset");
                                                        result.Add(IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, part, part.Description));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // handle scalar matches
                                                if(search != match || (search == match && addCompleteParts))
                                                {
                                                    // user wants to set index via a scalar, allow it
                                                    if(payload.Parent != null && payload.Parent.Payload.IndexOf("(", StringComparison.Ordinal) >= 0)
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
                                            }
                                        }
                                    }

                                    // now check recordset fields and add results
                                    refParts
                                        .ToList()
                                        .ForEach(pt =>
                                        {
                                            if(pt.Children != null)
                                            {
                                                // now eval each set of children
                                                pt.Children
                                                    .ToList()
                                                    .ForEach(child =>
                                                    {
                                                        string match = child.Name.ToLower();

                                                        // add each child match
                                                        if(match.Contains(search))
                                                        {
                                                            IDataListVerifyPart resultPt = IntellisenseFactory.CreateDataListValidationRecordsetPart(pt.Name, child.Name, pt.Description + " / " + child.Description + " Select this recordset field field");
                                                            IIntellisenseResult tmpChild = IntellisenseFactory.CreateSelectableResult(payload.StartIndex, payload.EndIndex, resultPt, resultPt.Description);

                                                            // only add if not picked up already
                                                            if(result
                                                                .ToList()
                                                                .Find(r => r.Option.DisplayValue == tmpChild.Option.DisplayValue) == null)
                                                            {

                                                                result.Add(tmpChild);
                                                            }
                                                        }
                                                    });
                                            }
                                        });

                                    // final evaluation of the issue
                                    if(result.Count == 0 && !emptyOk)
                                    {
                                        string display = tmp.ToString().Replace("]", "");
                                        IDataListVerifyPart part;

                                        enIntellisenseErrorCode code = enIntellisenseErrorCode.RecordsetNotFound;
                                        if(!isRS)
                                        {
                                            if(display.IndexOf(' ') >= 0)
                                            {
                                                code = enIntellisenseErrorCode.SyntaxError;
                                            }
                                            else
                                            {
                                                code = enIntellisenseErrorCode.ScalarNotFound;
                                            }

                                            part = IntellisenseFactory.CreateDataListValidationScalarPart(display);

                                        }
                                        else
                                        {
                                            // extract (x)
                                            int start = display.IndexOf("(", StringComparison.Ordinal);
                                            display = display.Substring(0, start);
                                            display += "()";
                                            part = IntellisenseFactory.CreateDataListValidationRecordsetPart(display, "");

                                        }
                                        // add error
                                        if(!display.Contains(' '))
                                        {
                                            result.Add(IntellisenseFactory.CreateErrorResult(payload.StartIndex, payload.EndIndex, part, " [[" + display + "]] does not exist in your Data List", code, (!payload.HangingOpen)));
                                        }
                                        else
                                        {
                                            result.Add(IntellisenseFactory.CreateErrorResult(payload.StartIndex, payload.EndIndex, part, " [[" + display + "]] contains a space, this is an invalid character for a variable name", code, (!payload.HangingOpen)));
                                        }
                                    }
                                }
                            }
                            catch(Dev2DataLanguageParseError e)
                            {
                                result.Add(AddErrorToResults(isRS, parts[0], e, (!payload.HangingOpen)));
                            }

                        }
                        else if(parts.Length == 2)
                        { // recordset with field lookup

                            // Check index
                            ParseTO tmpTO = new ParseTO { Payload = parts[0], StartIndex = 0, EndIndex = (parts[0].Length - 1) };

                            try
                            {
                                IsValidIndex(tmpTO);
                            }
                            catch(Dev2DataLanguageParseError e)
                            {
                                result.Add(AddErrorToResults(isRS, parts[0], e, (!payload.HangingOpen)));
                            }

                            IDev2DataLanguageIntellisensePart recordsetPart = refParts.FirstOrDefault(c => c.Name.ToLower() == search && c.Children != null);

                            string display = parts[0];
                            string partName = parts[0];
                            int start = display.IndexOf("(", StringComparison.Ordinal);
                            if(start >= 0 && recordsetPart == null)
                            {
                                display = display.Substring(0, start);
                                display += "()";
                            }

                            if(partName.IndexOf(' ') < 0)
                            {
                                if(partName.IndexOf("(", StringComparison.Ordinal) >= 0)
                                {
                                    partName = partName.Substring(0, partName.IndexOf("(", StringComparison.Ordinal));
                                }

                                if(recordsetPart == null)
                                {
                                    // add error
                                    IDataListVerifyPart part = IntellisenseFactory.CreateDataListValidationRecordsetPart(partName, parts[1], "");


                                    result.Add(IntellisenseFactory.CreateErrorResult(payload.StartIndex, (parts[0].Length - 1), part, "[[" + display + "]] does not exist in your Data List", enIntellisenseErrorCode.NeitherRecordsetNorFieldFound, (!payload.HangingOpen)));
                                }
                                else if(recordsetPart.Children != null && recordsetPart.Children.Count > 0)
                                {
                                    // search for matching children
                                    search = parts[1].ToLower();
                                    foreach(IDev2DataLanguageIntellisensePart t in recordsetPart.Children)
                                    {
                                        string match = t.Name.ToLower();
                                        if(match.Contains(search) && ((match != search) || (match == search && addCompleteParts)))
                                        {
                                            string index;

                                            // set recordset index
                                            if(payload.Child != null)
                                            {
                                                index = payload.Child.Payload;
                                            }
                                            else
                                            {
                                                // we have an * or int index
                                                index = DataListUtil.ExtractIndexRegionFromRecordset(parts[0]);
                                            }

                                            IDataListVerifyPart part = IntellisenseFactory.CreateDataListValidationRecordsetPart(partName, t.Name, t.Description, index);

                                            result.Add(IntellisenseFactory.CreateSelectableResult((parts[0].Length), payload.EndIndex, part, part.Description));
                                        }
                                        else if(match == search)
                                        {
                                            emptyOk = true;
                                        }
                                    }
                                }

                                if(result.Count == 0 && !emptyOk)
                                {
                                    IDataListVerifyPart part = IntellisenseFactory.CreateDataListValidationRecordsetPart(parts[0], search);
                                    if(Char.IsNumber(search[0]))
                                    {
                                        result.Add(IntellisenseFactory.CreateErrorResult(payload.StartIndex, (parts[0].Length - 1), part, "Invalid Expression: Recordset Field [ " + search + " ] starts with a number", enIntellisenseErrorCode.SyntaxError, (!payload.HangingOpen)));
                                    }
                                    else
                                    {
                                        result.Add(IntellisenseFactory.CreateErrorResult((parts[0].Length), payload.EndIndex, part, "Recordset Field [ " + search + " ] does not exist for [ " + parts[0] + " ]", enIntellisenseErrorCode.FieldNotFound, (!payload.HangingOpen)));
                                    }
                                }
                            }
                            else
                            {
                                IDataListVerifyPart part = IntellisenseFactory.CreateDataListValidationRecordsetPart(parts[0], "." + parts[1], true);
                                result.Add(IntellisenseFactory.CreateErrorResult(payload.StartIndex, payload.EndIndex, part, " [[" + display + "]] contains a space, this is an invalid character for a variable name", enIntellisenseErrorCode.SyntaxError, (!payload.HangingOpen)));
                            }
                        }
                        else
                        {
                            // we have major issues, throw exception
                            IDataListVerifyPart part = IntellisenseFactory.CreateDataListValidationRecordsetPart(parts[0], parts[1]);
                            result.Add(IntellisenseFactory.CreateErrorResult(payload.StartIndex, payload.EndIndex, part, "Invalid Notation - Extra dots detected", enIntellisenseErrorCode.SyntaxError, (!payload.HangingOpen)));
                        }
                    }
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
                            if(rr.Option.DisplayValue == r.Option.DisplayValue)
                            {
                                addToFinal = false;
                            }

                        });

                    if(addToFinal)
                    {
                        realResults.Add(r);
                    }

                });

            return result;
        }

        /// <summary>
        /// Adds the error to results.
        /// </summary>
        /// <param name="isRS">if set to <c>true</c> [is RS].</param>
        /// <param name="part">The part.</param>
        /// <param name="e">The e.</param>
        /// <param name="isOpen">if set to <c>true</c> [is open].</param>
        /// <returns></returns>
        private IIntellisenseResult AddErrorToResults(bool isRS, string part, Dev2DataLanguageParseError e, bool isOpen)
        {
            // add error
            IDataListVerifyPart pTO;
            if(isRS)
            {
                int start = part.IndexOf("(", StringComparison.Ordinal);
                string rs = part;
                if(start >= 0)
                {
                    rs = rs.Substring(0, start);
                }
                pTO = IntellisenseFactory.CreateDataListValidationRecordsetPart(rs, "", "");
            }
            else
            {
                pTO = IntellisenseFactory.CreateDataListValidationScalarPart(part.Replace("]", ""));
            }

            return IntellisenseFactory.CreateErrorResult(e.StartIndex, e.EndIndex, pTO, e.Message, e.ErrorCode, isOpen);
        }

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
        private bool IsValidIndex(ParseTO to)
        {
            bool result = false;
            string raw = to.Payload;
            int start = raw.IndexOf("(", StringComparison.Ordinal);
            int end = raw.IndexOf(")", StringComparison.Ordinal);

            // for ranges
            if(raw.IndexOf(":", StringComparison.Ordinal) > 0)
            {
                result = true;
            }
            else if(raw.IndexOf(",", StringComparison.Ordinal) > 0)
            {
                result = true; // added for calc operations
            }
            else
            {

                // no index
                if((end - start) == 1)
                {
                    result = true;
                }
                else if((start > 0 && end < 0) && ((raw.Length - 1) == start))
                { // another no index case
                    result = true;
                }
                else
                {
                    if(start > 0 && end < 0)
                    {
                        // we have index, just no )
                        string part = raw.Substring((start + 1), (raw.Length - (start + 1)));

                        if(part.Contains("[["))
                        {
                            result = true;
                        }
                        else
                        {
                            int partAsInt;
                            if(int.TryParse(part, out partAsInt))
                            {
                                if(partAsInt >= 1)
                                {
                                    result = true;
                                }
                                else
                                {
                                    throw new Dev2DataLanguageParseError("Recordset index [ " + part + " ] is not greater than zero", (to.StartIndex + start), (to.EndIndex + end), enIntellisenseErrorCode.NonPositiveRecordsetIndex);
                                }
                            }
                            else
                            {
                                string message = "Recordset index [ " + part + " ] is not numeric";
                                throw new Dev2DataLanguageParseError(message, (to.StartIndex + start), (to.EndIndex + end), enIntellisenseErrorCode.NonNumericRecordsetIndex);
                            }
                        }
                    }
                    else if(start > 0 && end > start)
                    {
                        // we have index with ( and )
                        start += 1;
                        string part = raw.Substring(start, (raw.Length - (start + 1)));

                        if(part.Contains("[[") || part == "*")
                        {
                            result = true;
                        }
                        else
                        {
                            int partAsInt;
                            if(int.TryParse(part, out partAsInt))
                            {
                                if(partAsInt >= 1)
                                {
                                    result = true;
                                }
                                else
                                {
                                    throw new Dev2DataLanguageParseError("Recordset index [ " + part + " ] is not greater than zero", (to.StartIndex + start), (to.EndIndex + end), enIntellisenseErrorCode.NonPositiveRecordsetIndex);
                                }
                            }
                            else
                            {
                                string message = "Recordset index [ " + part + " ] is not numeric";
                                throw new Dev2DataLanguageParseError(message, (to.StartIndex + start), (to.EndIndex + end), enIntellisenseErrorCode.NonNumericRecordsetIndex);
                            }
                        }
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
