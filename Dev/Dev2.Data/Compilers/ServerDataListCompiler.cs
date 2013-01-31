using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Interfaces;
using Dev2.DataList.Contract.TO;
using Dev2.DataList.Contract.Translators;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Enums;
using Dev2.MathOperations;
using Dev2.Server.Datalist.Auditing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Dev2.Server.Datalist
{

    internal class ServerDataListCompiler : IServerDataListCompiler
    {

        // Auditing objects...
        private readonly IDictionary<Guid, IDev2DataListAuditor> _auditing = new ConcurrentDictionary<Guid, IDev2DataListAuditor>();
        // Language Parser
        private readonly IDev2DataLanguageParser _parser = DataListFactory.CreateLanguageParser();
        // DataList Server
        private readonly IDataListServer _dlServer;

        private readonly static ConcurrentDictionary<Guid, IBinaryDataList> _cache = new ConcurrentDictionary<Guid, IBinaryDataList>();

        internal ServerDataListCompiler(IDataListServer dlServ)
        {

            _dlServer = dlServ;
        }

        #region Private Method
        private string CalcPrepValue(string eVal)
        {
            double tVal;

            if (!double.TryParse(eVal, out tVal))
            {
                eVal = "\"" + eVal + "\"";
            }

            return eVal;
        }
        #endregion

        public IBinaryDataListEntry Evaluate(NetworkContext ctx, Guid curDLID, enActionType typeOf, string expression, out ErrorResultTO errors, bool returnExpressionIfNoMatch = false)
        {

            ErrorResultTO allErrors = new ErrorResultTO();
            IBinaryDataListEntry result = Dev2BinaryDataListFactory.CreateEntry(string.Empty, string.Empty);
            errors = new ErrorResultTO();
            string error = string.Empty;

            IBinaryDataList theDL = TryFetchDataList(curDLID, out error);

            if (theDL != null)
            {

                if (typeOf == enActionType.User)
                {
                    result = InternalEvaluate(expression, theDL, false, out errors);
                    allErrors.MergeErrors(errors);
                }
                else if (typeOf == enActionType.System)
                {
                    // prefix eval with Dev2System
                    string realExpression = BuildSystemTag(expression);
                    if (!theDL.TryGetEntry(realExpression, out result, out error))
                    {
                        allErrors.AddError(error);
                    }
                }
                else if (typeOf == enActionType.Internal)
                {
                    // NOTE : Delete operation is the only internal op                     
                    IBinaryDataListEntry tmpEntry;
                    string recsetName = DataListUtil.ExtractRecordsetNameFromValue(expression);
                    bool res = false;
                    if (theDL.TryGetEntry(recsetName, out tmpEntry, out error))
                    {
                        errors.AddError(error);
                        string recsetIndexStr = DataListUtil.ExtractIndexRegionFromRecordset(expression);
                        res = tmpEntry.TryDeleteRows(recsetIndexStr);
                    }

                    TryPushDataList(theDL, out error);
                    errors.AddError(error);

                    IBinaryDataListEntry newDlEntry = Dev2BinaryDataListFactory.CreateEntry(GlobalConstants.EvalautionScalar, string.Empty);

                    errors.AddError(error);
                    if (res)
                    {
                        newDlEntry.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem("Success", GlobalConstants.EvalautionScalar), out error);
                        errors.AddError(error);

                    }
                    else
                    {
                        newDlEntry.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem("Failure", GlobalConstants.EvalautionScalar), out error);
                        errors.AddError(error);
                    }
                    result = newDlEntry;
                }
                else if(typeOf == enActionType.CalculateSubstitution)
                {
                    // Travis.Frisinger : 31.01.2013 - Added to properly levage the internal language   

                    // Break the expression up by , and sub in values?
                    IDev2DataLanguageParser parser = new Dev2DataLanguageParser();
                    // 
                    IList<IIntellisenseResult> myParts = parser.ParseExpressionIntoParts(expression, theDL.FetchIntellisenseParts());

                    // Fetch each DL expression in the master expression and evalaute
                    // Then build up the correct string to sub in ;)
                    foreach(IIntellisenseResult p in myParts)
                    {

                        // Ensure the expression exist and it is not a range operation
                        if(p.Type == enIntellisenseResultType.Selectable 
                            && expression.IndexOf(p.Option.DisplayValue, StringComparison.Ordinal) >= 0
                            && expression.IndexOf((p.Option.DisplayValue+":"), StringComparison.Ordinal)< 0
                            && expression.IndexOf((":"+p.Option.DisplayValue), StringComparison.Ordinal) < 0)
                        {
                            IBinaryDataListEntry bde = InternalEvaluate(p.Option.DisplayValue, theDL, returnExpressionIfNoMatch, out errors);
                            if(bde.IsRecordset)
                            {
                                // recordset op - build up the correct string to inject
                                IIndexIterator idxItr = bde.FetchRecordsetIndexes();
                                StringBuilder sb = new StringBuilder();

                                while(idxItr.HasMore())
                                {
                                    IList<IBinaryDataListItem> items = bde.FetchRecordAt(idxItr.FetchNextIndex(), out error);
                                    allErrors.AddError(error);
                                    foreach(IBinaryDataListItem itm in items)
                                    {
                                        //enRecordsetIndexType rType = DataListUtil.GetRecordsetIndexType(p.Option.RecordsetIndex);

                                        // && (rType == enRecordsetIndexType.Blank || rType == enRecordsetIndexType.Numeric) 
                                        if(itm.TheValue != string.Empty )
                                        {
                                            // if numeric leave it, else append ""
                                            string eVal = CalcPrepValue(itm.TheValue);
                                            sb.Append(eVal);
                                            sb.Append(",");
                                        }
                                    }
                                }

                                // Remove trailing ,
                                string toInject = sb.ToString();
                                toInject = toInject.Substring(0, (toInject.Length - 1));

                                expression = expression.Replace(p.Option.DisplayValue, toInject);

                            }
                            else
                            {
                                // scalar op
                                string eVal = CalcPrepValue(bde.FetchScalar().TheValue);
                                expression = expression.Replace(p.Option.DisplayValue, eVal);
                            }
                            allErrors.MergeErrors(errors);
                        }
                    }

                    //result = InternalEvaluate(expression, theDL, false, out errors);
                    allErrors.MergeErrors(errors);

                    IBinaryDataListEntry calcResult = Dev2BinaryDataListFactory.CreateEntry(GlobalConstants.EvalautionScalar, string.Empty);
                    IBinaryDataListItem calcItem = Dev2BinaryDataListFactory.CreateBinaryItem(expression, GlobalConstants.EvalautionScalar);
                    calcResult.TryPutScalar(calcItem, out error);
                    allErrors.AddError(error);
                    result = calcResult; // assign for return

                }

            }
            else
            {
                allErrors.AddError("Cannot locate the DataList for ID [ " + curDLID + " ]");
            }


            errors = allErrors;

            return result;
        }

        public Guid CloneDataList(Guid curDLID, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            string error = string.Empty;

            Guid res = GlobalConstants.NullDataListID;

            IBinaryDataList tmpDL = TryFetchDataList(curDLID, out error);
            if (error != string.Empty)
            {
                allErrors.AddError(error);
            }
            else
            {
                // Ensure we have a non-null tmpDL

                IBinaryDataList result = tmpDL.Clone(enTranslationDepth.Data, out errors);
                if (errors.HasErrors())
                {
                    allErrors.MergeErrors(errors);
                }
                TryPushDataList(result, out error);
                if (error != string.Empty)
                {
                    allErrors.AddError(error);
                }

                res = result.UID;
            }

            errors = allErrors;

            return res;
        }

        public IBinaryDataList FetchBinaryDataList(NetworkContext ctx, Guid curDLID, out ErrorResultTO errors)
        {

            string error = string.Empty;

            IBinaryDataList result = TryFetchDataList(curDLID, out error);
            errors = new ErrorResultTO();

            if (result == null)
            {
                errors.AddError(error);
            }

            return result;
        }

        public Guid Upsert(NetworkContext ctx, Guid curDLID, string expression, IBinaryDataListEntry value, out ErrorResultTO errors)
        {

            IDev2DataListUpsertPayloadBuilder<IBinaryDataListEntry> payload = Dev2DataListBuilderFactory.CreateBinaryDataListUpsertBuilder();
            payload.Add(expression, value);
            ErrorResultTO allErrors = new ErrorResultTO();
            errors = new ErrorResultTO();

            Guid result = Upsert(ctx, curDLID, payload, out errors);

            allErrors.MergeErrors(errors);
            errors = allErrors;

            return result;
        }

        public Guid Upsert(NetworkContext ctx, Guid curDLID, IList<string> expression, IList<string> values, out ErrorResultTO errors)
        {

            IDev2DataListUpsertPayloadBuilder<string> payload = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();
            errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();

            if (expression.Count == values.Count)
            {
                int pos = 0;
                foreach (string exp in expression)
                {
                    payload.Add(exp, values[pos]);
                    pos++;
                }
            }
            else
            {
                allErrors.AddError("Size mis-match, expressions count is not equal to values count");
            }

            Guid result = Upsert(ctx, curDLID, payload, out errors);

            allErrors.MergeErrors(errors);
            errors = allErrors;

            return result;
        }

        public Guid Upsert(NetworkContext ctx, Guid curDLID, IList<string> expressions, IList<IBinaryDataListEntry> values, out ErrorResultTO errors)
        {

            IDev2DataListUpsertPayloadBuilder<IBinaryDataListEntry> payload = Dev2DataListBuilderFactory.CreateBinaryDataListUpsertBuilder();
            errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();

            if (expressions.Count == values.Count)
            {
                int pos = 0;
                foreach (string exp in expressions)
                {
                    payload.Add(exp, values[pos]);
                    pos++;
                }
            }
            else
            {
                allErrors.AddError("Size mis-match, expressions count is not equal to values count");
            }

            Guid result = Upsert(ctx, curDLID, payload, out errors);

            allErrors.MergeErrors(errors);
            errors = allErrors;

            return result;
        }

        public Guid Upsert(NetworkContext ctx, Guid curDLID, IDev2DataListUpsertPayloadBuilder<string> payload, out ErrorResultTO errors)
        {
            return Upsert<string>(ctx, curDLID, payload, out errors);
        }

        public Guid Upsert(NetworkContext ctx, Guid curDLID, IDev2DataListUpsertPayloadBuilder<IBinaryDataListEntry> payload, out ErrorResultTO errors)
        {
            return Upsert<IBinaryDataListEntry>(ctx, curDLID, payload, out errors);
        }


        public Guid Shape(NetworkContext ctx, Guid curDLID, enDev2ArgumentType typeOf, string definitions, out ErrorResultTO errors)
        {

            IDev2LanguageParser parser;

            // Use eval and upsert to processes definitions
            Guid result = Guid.Empty;
            errors = new ErrorResultTO();

            if (typeOf == enDev2ArgumentType.Input)
            {
                parser = DataListFactory.CreateInputParser();
                IList<IDev2Definition> defs = parser.Parse(definitions);
                if (defs.Count > 0)
                {
                    result = InternalShape(ctx, curDLID, defs, typeOf, out errors);
                }
                else
                {
                    // default to a clone becuase there is nothing here ;)
                    string error = string.Empty;
                    IBinaryDataList tmp = TryFetchDataList(curDLID, out error);
                    if (error != string.Empty)
                    {
                        errors.AddError(error);
                    }
                    else
                    {
                        IBinaryDataList toPush = tmp.Clone(enTranslationDepth.Data, out errors);
                        toPush.ParentUID = curDLID;
                        TryPushDataList(toPush, out error);
                        if (error != string.Empty)
                        {
                            errors.AddError(error);
                        }
                        result = toPush.UID;
                    }
                }
            }
            else if (typeOf == enDev2ArgumentType.Output || typeOf == enDev2ArgumentType.Output_Append_Style)
            {
                parser = DataListFactory.CreateOutputParser();
                IList<IDev2Definition> defs = parser.Parse(definitions);
                if (defs.Count > 0)
                {
                    result = InternalShape(ctx, curDLID, defs, typeOf, out errors);
                }
                else
                {
                    // default to a union since there are no defs....
                    string error = string.Empty;
                    IBinaryDataList tmp = TryFetchDataList(curDLID, out error);
                    if (error != string.Empty)
                    {
                        errors.AddError(error);
                    }
                    else
                    {
                        Guid pID = tmp.ParentUID;
                        IBinaryDataList parentDL = TryFetchDataList(pID, out error);
                        if (error != string.Empty)
                        {
                            errors.AddError(error);
                        }
                        else
                        {
                            tmp = parentDL.Merge(tmp, enDataListMergeTypes.Union, enTranslationDepth.Data_With_Blank_OverWrite, false, out errors);
                            //tmp = parentDL.Merge(tmp, enDataListMergeTypes.Union, enTranslationDepth.Data, false, out errors);
                            TryPushDataList(tmp, out error);
                            if (error != string.Empty)
                            {
                                errors.AddError(error);
                            }
                            result = tmp.UID;
                        }
                    }
                }
            }

            return result;

        }

        public Guid Shape(NetworkContext ctx, Guid curDLID, enDev2ArgumentType typeOf, IList<IDev2Definition> definitions, out ErrorResultTO errors)
        {

            // Use eval and upsert to processes definitions
            Guid result = Guid.Empty;
            errors = new ErrorResultTO();

            if (typeOf == enDev2ArgumentType.Input)
            {
                result = InternalShape(ctx, curDLID, definitions, typeOf, out errors);
            }
            else if (typeOf == enDev2ArgumentType.Output)
            {
                result = InternalShape(ctx, curDLID, definitions, typeOf, out errors);
            }

            return result;
        }

        public Guid Merge(NetworkContext ctx, Guid leftID, Guid rightID, enDataListMergeTypes mergeType, enTranslationDepth depth, bool createNewList, out ErrorResultTO errors)
        {

            string error = string.Empty;
            errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            IBinaryDataList result;
            Guid returnVal = Guid.Empty;

            IBinaryDataList left = TryFetchDataList(leftID, out error);
            if (left == null)
            {
                allErrors.AddError(error);
            }

            IBinaryDataList right = TryFetchDataList(rightID, out error);
            if (right == null)
            {
                allErrors.AddError(error);
            }

            // alright to merge
            if (right != null && left != null)
            {
                result = left.Merge(right, mergeType, depth, createNewList, out errors);
                if (errors.HasErrors())
                {
                    allErrors.MergeErrors(errors);
                }
                else
                {
                    returnVal = result.UID;
                    // Push back into the server now ;)
                    if (!TryPushDataList(result, out error))
                    {
                        allErrors.AddError(error);
                    }
                }
            }
            else
            {
                allErrors.AddError("Cannot merge since both DataList cannot be found!");
            }

            // assign error var
            errors = allErrors;

            return returnVal;
        }

        public void ConditionalMerge(NetworkContext ctx, DataListMergeFrequency conditions,
            Guid destinationDatalistID, Guid sourceDatalistID, DataListMergeFrequency datalistMergeFrequency,
            enDataListMergeTypes datalistMergeType, enTranslationDepth datalistMergeDepth)
        {
            if (conditions.HasFlag(datalistMergeFrequency) && destinationDatalistID != Guid.Empty && sourceDatalistID != Guid.Empty)
            {
                ErrorResultTO errors;
                Merge(ctx, destinationDatalistID, sourceDatalistID, datalistMergeType, datalistMergeDepth, false, out errors);

                if (errors != null && errors.HasErrors())
                {
                    ErrorResultTO tmpErrors;
                    UpsertSystemTag(destinationDatalistID, enSystemTag.Error, errors.MakeDataListReady(), out tmpErrors);
                }
            }
        }

        public Guid TransferSystemTags(NetworkContext ctx, Guid parentDLID, Guid childDLID, bool parentToChild, out ErrorResultTO errors)
        {

            // TODO : Build transfer instruction payload and send through to Upsert...???

            // TODO : Build system tag transfer into Translator for XML...

            throw new NotImplementedException();
        }

        public IList<KeyValuePair<string, IBinaryDataListEntry>> FetchChanges(NetworkContext ctx, Guid id, StateType direction)
        {
            IList<KeyValuePair<string, IBinaryDataListEntry>> result = null;
            IDev2DataListAuditor auditor;

            if (_auditing.TryGetValue(id, out auditor))
            {
                if (direction == StateType.Before)
                {
                    result = auditor.FetchChanges("input");
                }
                else if (direction == StateType.After)
                {
                    result = auditor.FetchChanges("output");
                }
            }

            return result;
        }

        public bool DeleteDataListByID(Guid curDLID, bool onlyIfNotPersisted)
        {
            bool result = true;

            _dlServer.DeleteDataList(curDLID, onlyIfNotPersisted);

            return result;
        }

        public Guid ConvertTo(NetworkContext ctx, DataListFormat typeOf, byte[] payload, string shape, out ErrorResultTO errors)
        {

            // _repo
            IBinaryDataList result;
            Guid returnVal = Guid.Empty;
            ErrorResultTO allErrors = new ErrorResultTO();
            errors = new ErrorResultTO();
            string error = string.Empty;
            try
            {
                IDataListTranslator t = _dlServer.GetTranslator(typeOf);
                if (t != null)
                {
                    result = t.ConvertTo(payload, shape, out errors);
                    if (errors.HasErrors())
                    {
                        allErrors.MergeErrors(errors);
                    }

                    // set the uid and place in cache
                    returnVal = result.UID;

                    if (!TryPushDataList(result, out error))
                    {
                        allErrors.AddError(error);
                    }

                }
                else
                {
                    allErrors.AddError("Invalid DataListFormt [ " + typeOf + " ] ");
                }
            }
            catch (Exception e)
            {
                allErrors.AddError(e.Message);
            }

            // assign error var
            errors = allErrors;

            return returnVal;
        }

        public DataListTranslatedPayloadTO ConvertFrom(NetworkContext ctx, Guid curDLID, enTranslationDepth depth, DataListFormat typeOf, out ErrorResultTO errors)
        {
            IBinaryDataList result;
            DataListTranslatedPayloadTO returnVal = null;
            ErrorResultTO allErrors = new ErrorResultTO();
            errors = new ErrorResultTO();
            string error = string.Empty;

            result = TryFetchDataList(curDLID, out error);

            if (result != null)
            {
                try
                {

                    IDataListTranslator t = _dlServer.GetTranslator(typeOf);
                    if (t != null)
                    {
                        returnVal = t.ConvertFrom(result, out errors);
                        if (errors.HasErrors())
                        {
                            allErrors.MergeErrors(errors);
                        }
                    }
                    else
                    {
                        allErrors.AddError("Invalid DataListFormt [ " + typeOf + " ] ");
                    }
                }
                catch (Exception e)
                {
                    allErrors.AddError(e.Message);
                }
            }
            else
            {
                allErrors.AddError(error);
            }

            // assign error var
            errors = allErrors;

            return returnVal;
        }

        public void SetParentUID(Guid curDLID, Guid parentID, out ErrorResultTO errors)
        {
            string error = string.Empty;
            errors = new ErrorResultTO();
            IBinaryDataList bdl = TryFetchDataList(curDLID, out error);
            if (error != string.Empty)
            {
                errors.AddError(error);
            }
            if (bdl != null)
            {
                bdl.ParentUID = parentID;
                TryPushDataList(bdl, out error);
                if (error != string.Empty)
                {
                    errors.AddError(error);
                }
            }
        }

        public IList<DataListFormat> FetchTranslatorTypes()
        {
            return _dlServer.FetchTranslatorTypes();
        }

        public bool PersistResumableDataListChain(Guid childID)
        {
            bool result = false;

            result = _dlServer.PersistChildChain(childID);

            return result;
        }

        public Guid UpsertSystemTag(Guid curDLID, enSystemTag tag, string val, out ErrorResultTO errors)
        {

            return UpsertSystemTag<string>(curDLID, tag, val, out errors);
        }

        public Guid UpsertSystemTag(Guid curDLID, enSystemTag tag, IBinaryDataListEntry val, out ErrorResultTO errors)
        {

            return UpsertSystemTag<IBinaryDataListEntry>(curDLID, tag, val, out errors);
        }

        #region Private Methods


        /// <summary>
        /// Upserts the system tag.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="val">The val.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private Guid UpsertSystemTag<T>(Guid curDLID, enSystemTag tag, T val, out ErrorResultTO errors)
        {
            ErrorResultTO allErrors = new ErrorResultTO();
            errors = new ErrorResultTO();
            string error = string.Empty;
            string exp = GlobalConstants.SystemTagNamespace + "." + tag;
            IBinaryDataList bdl = TryFetchDataList(curDLID, out error);

            Guid result = GlobalConstants.NullDataListID;

            if (error != string.Empty)
            {
                allErrors.AddError(error);
                error = string.Empty;
            }

            if (bdl != null)
            {
                allErrors.MergeErrors(errors);
                IBinaryDataListItem itm = null;

                if (typeof(T) == typeof(string))
                {
                    itm = Dev2BinaryDataListFactory.CreateBinaryItem(val.ToString(), exp);

                }
                else if (typeof(T) == typeof(IBinaryDataListEntry))
                {
                    itm = ((IBinaryDataListEntry)val).FetchScalar();
                }

                IBinaryDataListEntry et;
                result = GlobalConstants.NullDataListID;

                if (bdl.TryCreateScalarTemplate(GlobalConstants.SystemTagNamespace, tag.ToString(), string.Empty, true, out error))
                {
                    bdl.TryGetEntry(exp, out et, out error);

                    if (et != null)
                    {
                        et.TryPutScalar(itm, out error);
                        result = bdl.UID;
                        allErrors.MergeErrors(errors);
                        TryPushDataList(bdl, out error);
                    }

                }

            }

            errors = allErrors;

            return result;
        }

        /// <summary>
        /// Fetches the single pass inject value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        private string FetchSinglePassInjectValue(IBinaryDataListEntry value, out string error)
        {
            string injectValue = string.Empty;
            error = string.Empty;

            if (!value.IsRecordset)
            {
                // scalar to row.field
                injectValue = value.FetchScalar().TheValue;
            }
            else
            {
                // row.field to row.field
                IBinaryDataListItem i = value.TryFetchLastIndexedRecordsetUpsertPayload(out error);
                injectValue = i.TheValue;
            }

            return injectValue;

        }

        /// <summary>
        /// Fetches the single pass inject value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        private string FetchIndexInjectValue(IBinaryDataListEntry value, int idx, out string error)
        {
            string injectValue = string.Empty;
            error = string.Empty;

            if (value.IsRecordset)
            {
                // row.field to row.field
                IBinaryDataListItem i = value.TryFetchIndexedRecordsetUpsertPayload(idx, out error);
                injectValue = i.TheValue;
            }

            return injectValue;

        }

        /// <summary>
        /// Internals the shape input.
        /// </summary>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="definitions">The definitions.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private Guid InternalShape(NetworkContext ctx, Guid curDLID, IList<IDev2Definition> definitions, enDev2ArgumentType typeOf, out ErrorResultTO errors)
        {

            Guid result = Guid.Empty;
            errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            string theShape = DataListUtil.GenerateDataListFromDefs(definitions);
            byte[] empty = new byte[0];
            string error = string.Empty;

            // alwasy an internal XML format ;)
            Guid shellID;
            if (typeOf == enDev2ArgumentType.Input)
            {
                shellID = ConvertTo(ctx, DataListFormat.CreateFormat(GlobalConstants._XML), empty, theShape, out errors);
            }
            else
            {
                // output shape, we already have the ID, fetch parentID for merge up ;)
                shellID = curDLID;
            }
            if (errors.HasErrors())
            {
                allErrors.MergeErrors(errors);
            }
            else
            {
                // now we have a IBinaryDataList, we need to set the ParentID and populate with data
                IBinaryDataList childDL = FetchBinaryDataList(ctx, shellID, out errors);
                if (errors.HasErrors())
                {
                    allErrors.MergeErrors(errors);
                }
                else
                {
                    // set parentID on input shape
                    if (typeOf == enDev2ArgumentType.Input)
                    {
                        childDL.ParentUID = curDLID;
                        TryPushDataList(childDL, out error);
                        if (error != string.Empty)
                        {
                            allErrors.AddError(error);
                        }
                    }

                    // now set the Func to execute depending upon direction ;)
                    Func<IDev2Definition, string> inputExpressionExtractor = (IDev2Definition def) => { return null; };
                    Func<IDev2Definition, string> outputExpressionExtractor = (IDev2Definition def) => { return null; };
                    Guid extractFromID = curDLID;
                    Guid pushToID = childDL.UID;

                    if (typeOf == enDev2ArgumentType.Input)
                    { // input shaping
                        inputExpressionExtractor = (IDev2Definition def) =>
                        {
                            string expression = string.Empty;
                            if (def.Value == string.Empty)
                            {
                                if (def.DefaultValue != string.Empty)
                                {
                                    expression = def.DefaultValue;
                                }
                            }
                            else
                            {
                                expression = def.RawValue;
                            }

                            return expression;
                        };

                        outputExpressionExtractor = (IDev2Definition def) =>
                        {
                            string expression = string.Empty;

                            if (def.RecordSetName != string.Empty)
                            {
                                expression = def.RecordSetName + "(*)." + def.Name; // star because we are overwriting everything in the new list ;)
                            }
                            else
                            {
                                expression = def.Name;
                            }

                            return "[[" + expression + "]]";
                        };

                    }
                    else
                    { // output shaping... value is target shape


                        if (typeOf == enDev2ArgumentType.Output)
                        {

                            inputExpressionExtractor = (IDev2Definition def) =>
                            {
                                string expression = string.Empty;

                                if (def.RecordSetName != string.Empty)
                                {
                                    expression = def.RecordSetName + "(*)." + def.Name; // star because we are fetching all to place into the parentDataList
                                }
                                else
                                {
                                    expression = def.Name;
                                }

                                return "[[" + expression + "]]";
                            };
                        }
                        else if (typeOf == enDev2ArgumentType.Output_Append_Style)
                        {
                            inputExpressionExtractor = (IDev2Definition def) =>
                            {
                                string expression = string.Empty;

                                if (def.RecordSetName != string.Empty)
                                {
                                    expression = def.RecordSetName + "()." + def.Name; // () because we are fetching last row to append
                                }
                                else
                                {
                                    expression = def.Name;
                                }

                                return "[[" + expression + "]]";
                            };
                        }

                        outputExpressionExtractor = (IDev2Definition def) =>
                        {
                            string expression = string.Empty;

                            if (def.Value != string.Empty)
                            {
                                expression = def.RawValue;
                            }

                            return expression;
                        };

                        // swap extract from and pushTo around
                        extractFromID = childDL.UID;
                        pushToID = childDL.ParentUID;
                    }

                    // Create/Fetch Auditing Object
                    IDev2DataListAuditor auditor = FetchAuditor(curDLID);

                    // build framed upsert expression ;)
                    IDev2DataListUpsertPayloadBuilder<IBinaryDataListEntry> toUpsert = Dev2DataListBuilderFactory.CreateBinaryDataListUpsertBuilder(false);

                    // populate with parentDL data as per the definitions ;)
                    foreach (IDev2Definition def in definitions)
                    {

                        string expression = inputExpressionExtractor(def);

                        if (expression != string.Empty)
                        {
                            // Evaluate from extractDL
                            IBinaryDataListEntry val = Evaluate(ctx, extractFromID, enActionType.User, expression, out errors);
                            allErrors.MergeErrors(errors);
                            if (val == null)
                            {
                                val = Dev2BinaryDataListFactory.CreateEntry(string.Empty, string.Empty);
                            }
                            // Push audit entry
                            if (typeOf == enDev2ArgumentType.Input)
                            {
                                auditor.PushChange(expression, val, "input");
                            }
                            else
                            {
                                auditor.PushChange(expression, val, "output");
                            }


                            // now upsert into the pushDL
                            string upsertExpression = outputExpressionExtractor(def);

                            toUpsert.Add(upsertExpression, val);
                            //Upsert(ctx, pushToID, upsertExpression, val, out errors);
                            //allErrors.MergeErrors(errors);
                        }
                        else if (expression == string.Empty && (typeOf == enDev2ArgumentType.Input) && def.IsRequired)
                        {
                            allErrors.AddError("Required input " + def.RawValue + " cannot be populated");
                        }
                    }

                    Upsert(ctx, pushToID, toUpsert, out errors);
                    allErrors.MergeErrors(errors);

                    //if (!allErrors.HasErrors()) {
                    result = pushToID;
                    //}

                    // Merge System tags too ;)
                    // exctractFromID, pushToID
                    foreach (enSystemTag t in TranslationConstants.systemTags)
                    {

                        IBinaryDataListEntry sysVal = Evaluate(ctx, extractFromID, enActionType.System, t.ToString(), out errors);
                        if (sysVal == null)
                        {
                            sysVal = Dev2BinaryDataListFactory.CreateEntry(string.Empty, string.Empty);

                        }
                        if (errors.HasErrors())
                        {
                            allErrors.MergeErrors(errors);
                        }

                        UpsertSystemTag(pushToID, t, sysVal, out errors);
                        allErrors.MergeErrors(errors);

                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Builds the system tag.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        private string BuildSystemTag(string tag)
        {
            return (GlobalConstants.SystemTagNamespace + "." + tag);
        }

        /// <summary>
        /// Builds the system tag.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        private string BuildSystemTag(enSystemTag tag)
        {
            return (GlobalConstants.SystemTagNamespace + "." + tag);
        }

        private bool TryPersistChildChain(Guid id)
        {
            bool result = false;

            // TODO : Persist the data ;)
            result = _dlServer.PersistChildChain(id);

            return result;
        }

        /// <summary>
        /// Tries the fetch data list.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        /// TODO : Replace with proper server coms
        private IBinaryDataList TryFetchDataList(Guid id, out string error)
        {

            ErrorResultTO errors = new ErrorResultTO();
            error = string.Empty;
            IBinaryDataList result = null;

            result = _dlServer.ReadDatalist(id, out errors);

            if (result == null)
            {
                error = "Cache miss for [ " + id + " ]";
            }

            return result;
        }

        /// <summary>
        /// Tries the push data list.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        /// TODO : Replace with proper server coms
        private bool TryPushDataList(IBinaryDataList payload, out string error)
        {
            bool r = true;
            error = string.Empty;
            ErrorResultTO errors = new ErrorResultTO();
            Guid key = payload.UID;

            if (!_dlServer.WriteDataList(payload.UID, payload, out errors))
            {
                error = "Failed to write DataList";
            }

            return r;
        }

        /// <summary>
        /// Determines whether [is calc evaluation] [the specified expression].
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        ///   <c>true</c> if [is calc evaluation] [the specified expression]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsCalcEvaluation(string expression, out string newExpression)
        {
            bool result = false;

            newExpression = string.Empty;

            if (expression.StartsWith(GlobalConstants.CalculateTextConvertPrefix))
            {
                if (expression.EndsWith(GlobalConstants.CalculateTextConvertSuffix))
                {
                    newExpression = expression.Substring(GlobalConstants.CalculateTextConvertPrefix.Length, expression.Length - (GlobalConstants.CalculateTextConvertSuffix.Length + GlobalConstants.CalculateTextConvertPrefix.Length));
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Internals the calc evaluation.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        private string InternalCalcEvaluation(string expression, IBinaryDataList bdl, out ErrorResultTO errors)
        {

            IFunctionEvaluator functionEvaluator = MathOpsFactory.CreateFunctionEvaluator();
            IEvaluationFunction evaluationFunctionTO = MathOpsFactory.CreateEvaluationExpressionTO(expression);

            string result = string.Empty;
            errors = new ErrorResultTO();
            try
            {
                // Replace with Data and Shape .....
                //byte[] data = ConvertFrom(null, bdl.UID, enTranslationDepth.Shape, DataListFormat.CreateFormat(GlobalConstants._XML), out errors);
                //string currentADL = Encoding.UTF8.GetString(data);
                //data = ConvertFrom(null, bdl.UID, enTranslationDepth.Data, DataListFormat.CreateFormat(GlobalConstants._XML), out errors);
                //string dataListShape = Encoding.UTF8.GetString(data);

                result = functionEvaluator.EvaluateFunction(evaluationFunctionTO, bdl.UID, out errors);
                //(evaluationFunctionTO, currentADL, dataListShape);
                // We can only return a single expression now....
            }
            catch { }

            return result;
        }

        /// <summary>
        /// Internals the evaluate.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        private IBinaryDataListEntry InternalEvaluate(string expression, IBinaryDataList bdl, bool toRoot, out ErrorResultTO errors)
        {
            IBinaryDataListEntry result = Dev2BinaryDataListFactory.CreateEntry(string.Empty, string.Empty);

            ErrorResultTO allErrors = new ErrorResultTO();
            string error = string.Empty;
            errors = new ErrorResultTO();
            string calcExp = string.Empty;

            if (IsCalcEvaluation(expression, out calcExp))
            {
                expression = calcExp;
                string r = InternalCalcEvaluation(expression, bdl, out errors);
                allErrors.MergeErrors(errors);
                result = Dev2BinaryDataListFactory.CreateEntry(GlobalConstants.EvalautionScalar, string.Empty);
                result.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem(r, GlobalConstants.EvalautionScalar), out error);
                if (error != string.Empty)
                {
                    allErrors.AddError(error);
                }
            }
            else
            {
                result = InternalDataListEvaluate(expression, bdl, toRoot, out errors);
                allErrors.MergeErrors(errors);
            }

            errors = allErrors;

            return result;
        }

        /// <summary>
        /// Fetches the evaluation iteration count.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        private int FetchEvaluationIterationCount(string expression)
        {
            int result = 1;

            char[] parts = expression.ToCharArray();

            int hits = 0;
            int pos = 0;
            while (pos < parts.Length)
            {
                if (parts[pos] == GlobalConstants.EvaluationToken)
                {
                    hits++;
                }
                pos++;
            }

            if (hits > 0)
            {
                result = (hits / 2);
            }

            return result;
        }

        /// <summary>
        /// Requires to root evaluation.
        /// </summary>
        /// <param name="bdl">The BDL.</param>
        /// <param name="toRoot">if set to <c>true</c> [to root].</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        private bool IsIterationDriven(IBinaryDataList bdl, out string error)
        {
            error = string.Empty;
            IBinaryDataListEntry entry;
            bool result = false;

            if (bdl.TryGetEntry(BuildSystemTag(enSystemTag.EvaluateIteration), out entry, out error))
            {
                bool.TryParse(entry.FetchScalar().TheValue, out result);
            }
            return result;
        }

        /// <summary>
        /// Requireses the token sub.
        /// </summary>
        /// <param name="bdl">The BDL.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        private bool RequiresTokenSub(IBinaryDataList bdl, out string error)
        {
            error = string.Empty;
            IBinaryDataListEntry entry;
            bool result = false;

            if (bdl.TryGetEntry(BuildSystemTag(enSystemTag.SubstituteTokens), out entry, out error))
            {
                bool.TryParse(entry.FetchScalar().TheValue, out result);
            }
            return result;
        }

        /// <summary>
        /// Requires the design time binding.
        /// </summary>
        /// <param name="bdl">The BDL.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        private bool RequiresDesignTimeBinding(IBinaryDataList bdl, out string error)
        {

            error = string.Empty;
            IBinaryDataListEntry entry;

            string serviceName = string.Empty;
            string parentServiceName = string.Empty;
            string Dev2DesignTimeBindingTag = string.Empty;

            if (bdl.TryGetEntry(BuildSystemTag(enSystemTag.Service), out entry, out error))
            {
                serviceName = entry.FetchScalar().TheValue;
            }

            if (bdl.TryGetEntry(BuildSystemTag(enSystemTag.ParentServiceName), out entry, out error))
            {
                parentServiceName = entry.FetchScalar().TheValue;
            }

            if (bdl.TryGetEntry(BuildSystemTag(enSystemTag.Dev2DesignTimeBinding), out entry, out error))
            {
                Dev2DesignTimeBindingTag = entry.FetchScalar().TheValue;
            }

            bool result = false;

            if (serviceName.ToLower().EndsWith(".wiz") || parentServiceName.ToLower().EndsWith(".wiz"))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Fetches the auditor.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        private IDev2DataListAuditor FetchAuditor(Guid id)
        {
            IDev2DataListAuditor result;

            lock (id.ToString())
            {
                if (!_auditing.TryGetValue(id, out result))
                {
                    result = Dev2AuditFactory.CreateAuditor();
                    _auditing[id] = result;
                }
            }

            return result;
        }

        /// <summary>
        /// Internals the data list evaluate.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        private IBinaryDataListEntry InternalDataListEvaluate(string expression, IBinaryDataList bdl, bool toRoot, out ErrorResultTO errors)
        {
            string result = string.Empty;
            IBinaryDataListEntry lastFetch = Dev2BinaryDataListFactory.CreateEntry(string.Empty, string.Empty);
            errors = new ErrorResultTO();
            string error = string.Empty;

            bool designTimeBinding = RequiresDesignTimeBinding(bdl, out error);
            //30-11-2012 - Massimo.Guerrera
            bool isIterationDriven = IsIterationDriven(bdl, out error);
            string eval = expression;
            int matchCnt = 0;
            int iterationCnt = 0;
            int iterationTotal = 0;

            // fetch total number of iterations
            if (isIterationDriven)
            {
                iterationTotal = FetchEvaluationIterationCount(expression);
            }

            bool tokenSub = RequiresTokenSub(bdl, out error);


            bool foundMatch = true;
            bool hasError = false;
            IDictionary<string, string> _notFoundReplaceParts = new Dictionary<string, string>();

            // evaluation functions ;)
            Func<bool> loopEval;
            if (toRoot)
            {
                loopEval = () => { return (!DataListUtil.isRootVariable(expression)); };
            }
            else if (isIterationDriven)
            {
                loopEval = () => { return (iterationCnt < iterationTotal && expression != string.Empty); };
            }
            else
            {
                loopEval = () => { return (isEvaluated(expression) && foundMatch); };
            }


            if (isEvaluated(eval))
            {
                // if evaluate to root do so, else evaluate the expression fully
                //while ( ((toRoot && !DataListUtil.isRootVariable(expression)) || (isRecursiveOp(expression) && foundMatch)) ) {
                IList<string> RecursiveVals = new List<string>();
                while (loopEval())
                {
                    foundMatch = false;
                    IList<IIntellisenseResult> expressionParts = _parser.ParseExpressionIntoParts(expression, bdl.FetchIntellisenseParts());
                    foreach (IIntellisenseResult p in expressionParts)
                    {

                        if (p.Type == enIntellisenseResultType.Error)
                        {
                            hasError = true;
                            errors.AddError(p.Message);
                            // attempt to remove the fragement not found if outside of design mode
                            if (!designTimeBinding)
                            {
                                expression = expression.Replace(p.Option.DisplayValue, "");
                            }
                        }
                        else if (p.Type == enIntellisenseResultType.Selectable && expression.Contains(p.Option.DisplayValue))
                        {

                            // Evaluate from the DataList
                            IBinaryDataListEntry val;
                            bool fetchOk = false;
                            if (p.Option.IsScalar)
                            {
                                fetchOk = bdl.TryGetEntry(p.Option.Field, out val, out error);
                            }
                            else
                            {
                                fetchOk = bdl.TryGetEntry(p.Option.Recordset, out val, out error);
                            }

                            if (!fetchOk)
                            {
                                errors.AddError(error);
                            }
                            else
                            {
                                matchCnt++;
                                foundMatch = true;
                                lastFetch = val.Clone(enTranslationDepth.Data, out error); // clone to avoid mutation issues
                                errors.AddError(error);


                            }

                            if (p.Option.IsScalar && val != null)
                            {
                                RecursiveVals.Add(p.Option.DisplayValue);
                                if (RecursiveVals.FirstOrDefault(c => c.Equals(val.FetchScalar().TheValue)) == null)
                                {
                                    //p.Option.DisplayValue != val.FetchScalar().TheValue
                                    expression = expression.Replace(p.Option.DisplayValue, val.FetchScalar().TheValue);
                                }
                                else
                                {

                                    return lastFetch;
                                }

                            }
                            else if (val != null)
                            {
                                // process the recordset
                                string idx = string.Empty;
                                if (p.Option.HasRecordsetIndex)
                                {
                                    idx = p.Option.RecordsetIndex;
                                }

                                enRecordsetIndexType idxType = DataListUtil.GetRecordsetIndexTypeRaw(idx);

                                if (idxType == enRecordsetIndexType.Numeric || idxType == enRecordsetIndexType.Blank)
                                {

                                    int myIdx = -1;
                                    if (idxType == enRecordsetIndexType.Numeric)
                                    {
                                        myIdx = Int32.Parse(idx);
                                    }
                                    else
                                    {
                                        myIdx = val.FetchLastRecordsetIndex();
                                    }

                                    if (p.Option.Field != null && p.Option.Field != string.Empty)
                                    {
                                        // we want an entry at a set location
                                        IBinaryDataListItem col = val.TryFetchRecordsetColumnAtIndex(p.Option.Field, myIdx, out error);
                                        if (error != string.Empty)
                                        {
                                            hasError = true;
                                            matchCnt--;
                                            errors.AddError(error);
                                            lastFetch = Dev2BinaryDataListFactory.CreateEntry(string.Empty, string.Empty); // set a blank match too ;)
                                            expression = expression.Replace(p.Option.DisplayValue, string.Empty); // blank the match to avoid looping ;)
                                        }
                                        else
                                        {
                                            // build up the result, via a strip all but method?
                                            lastFetch.MakeRecordsetEvaluateReady(myIdx, col.FieldName, out error);
                                            if (error != string.Empty)
                                            {
                                                errors.AddError(error);
                                            }
                                            // now evaluate the expression correctly to replace this token
                                            expression = expression.Replace(p.Option.DisplayValue, string.Empty);
                                        }
                                    }
                                    else
                                    {
                                        // they want the entire recordset? -- blank expression
                                        expression = expression.Replace(p.Option.DisplayValue, string.Empty);
                                        // Check for an index and remove all but this index ;)
                                        if (idxType == enRecordsetIndexType.Numeric || idxType == enRecordsetIndexType.Blank)
                                        {
                                            lastFetch.MakeRecordsetEvaluateReady(myIdx, GlobalConstants.AllColumns, out error);
                                            if (error != string.Empty)
                                            {
                                                errors.AddError(error);
                                            }
                                        }
                                        // else already handled because we fetched it all ;)

                                    }
                                }
                                else if (idxType == enRecordsetIndexType.Error)
                                {
                                    // we all it all
                                    errors.AddError("Invalid Recordset Index");
                                    foundMatch = false;
                                }
                                else if (idxType == enRecordsetIndexType.Star)
                                {
                                    // they want the whole thing, send it and blank expression
                                    if (p.Option.Field != null || p.Option.Field != string.Empty)
                                    {
                                        // keep only this field
                                        string field = p.Option.Field;
                                        if (field == string.Empty)
                                        {
                                            field = GlobalConstants.AllColumns;
                                        }
                                        lastFetch.MakeRecordsetEvaluateReady(GlobalConstants.AllIndexes, field, out error);
                                        if (error != string.Empty)
                                        {
                                            errors.AddError(error);
                                        }
                                    } // else we need a column match to process by evaluate

                                    expression = expression.Replace(p.Option.DisplayValue, string.Empty);
                                    if (expression != string.Empty && expression != " ")//Bug 7836
                                    {
                                        hasError = true;
                                        matchCnt--;
                                        errors.AddError("Attempt to use Recordset with * in a complex expression");
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (designTimeBinding)
                            {
                                // we need to keep unbound references in this mode
                                if (expression.IndexOf(p.Option.DisplayValue) >= 0)
                                {
                                    Guid tmp = _dlServer.IDProvider.AllocateID();
                                    string replace = string.Concat("Dev2", tmp.ToString());
                                    expression = expression.Replace(p.Option.DisplayValue, replace);
                                    _notFoundReplaceParts.Add(p.Option.DisplayValue, replace);
                                    foundMatch = true;
                                    matchCnt++;
                                }
                            }
                            else
                            {
                                // it needs to be evaluated, blank it
                                if (expression.Contains(p.Option.DisplayValue))
                                {
                                    foundMatch = true;
                                    matchCnt++;
                                    expression = expression.Replace(p.Option.DisplayValue, string.Empty);
                                }
                                else if (hasError)
                                {
                                    // we have a funny script segment that contains brackets ( or most likely... )
                                    //foundMatch = false;
                                    if (isIterationDriven)
                                    {
                                        matchCnt++; // force an interation
                                    }
                                }
                            }

                        }
                    }

                    // if iteration driven clear the list and inc the counter ;)
                    if (isIterationDriven && matchCnt > 0)
                    {
                        expressionParts = new List<IIntellisenseResult>(); // clear the list ;)
                        iterationCnt++;
                        matchCnt = 0;
                    }
                }
            }

            if (hasError)
            {
                // if has error, do not mutate expression
            }
            else
            {
                // nothing was evaluated, meep
                if (matchCnt == 0 && isEvaluated(expression) && !toRoot && !isIterationDriven)
                {
                    expression = string.Empty;
                }
                else if (matchCnt == 0 && !isEvaluated(expression) && !isIterationDriven)
                {
                    expression = eval;
                }
            }

            // now evaluate for {{ regions and execute
            if (expression.Contains("{{"))
            {
                string codepattern = @"(?<code>\{\{.*\}\})";
                string jScriptCode = string.Empty;
                Match match = Regex.Match(expression, codepattern, RegexOptions.Singleline);
                if (match.Success)
                {
                    jScriptCode = match.Value;
                    // Travis.Frisinger : 07.08.2012 
                    // if there is valid non JS code, extract it and swap the tmp in later ;)
                    string jsCode = string.Concat("Dev2_JS_HOLDER_", Guid.NewGuid());

                    expression = expression.Replace(jScriptCode, jsCode).Trim();

                    if (!string.IsNullOrEmpty(jScriptCode))
                    {
                        string codeToExecute = jScriptCode.Replace("\r\n", string.Empty).Replace("\n", string.Empty);
                        string returnVal = JScriptEvaluator.EvaluateToString(codeToExecute);
                        expression = expression.Replace(jsCode, returnVal.Trim());
                    }
                }
            }

            // keep the references to design time variables
            if (designTimeBinding)
            {
                foreach (string key in _notFoundReplaceParts.Keys)
                {
                    expression = expression.Replace(_notFoundReplaceParts[key], key);
                }
            }

            // finally, bundle up the expression as the result if it has not been evaluated fully ;)
            if (expression != string.Empty && expression != " ")//Bug 7836
            {
                // we just need to return the expression as is in a IBinaryDataListEntry
                IBinaryDataListEntry tmp = Dev2BinaryDataListFactory.CreateEntry(GlobalConstants.EvalautionScalar, string.Empty);
                tmp.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem(expression, GlobalConstants.EvalautionScalar), out error);
                lastFetch = tmp; // return the expression as the result now ;)
            }

            // super finally, remove the iteration evaluation since this is a wizard specific feature ;)
            if (isIterationDriven)
            {
                UpsertSystemTag(bdl.UID, enSystemTag.EvaluateIteration, "false", out errors);
            }

            return lastFetch;
        }

        /// <summary>
        /// Determines whether [is recursive op] [the specified expression].
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        ///   <c>true</c> if [is recursive op] [the specified expression]; otherwise, <c>false</c>.
        /// </returns>
        private bool isRecursiveOp(string expression)
        {
            bool result = false;

            if (expression.Contains("[["))
            {
                int start = expression.IndexOf("[[");
                if (expression.Contains("[[]]"))
                {
                    int startNaughty = expression.IndexOf("[[]]");
                    if (start != startNaughty)
                    {
                        result = true;
                    }
                }
                else
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the specified expression is evaluated.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        ///   <c>true</c> if the specified expression is evaluated; otherwise, <c>false</c>.
        /// </returns>
        private bool isEvaluated(string expression)
        {
            bool result = false;

            if (expression.Contains("[["))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Determines whether [is black listed design time expression] [the specified display expression].
        /// </summary>
        /// <param name="displayExpression">The display expression.</param>
        /// <param name="designTime">if set to <c>true</c> [design time].</param>
        /// <returns>
        ///   <c>true</c> if [is black listed design time expression] [the specified display expression]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsBlackListedDesignTimeExpression(string displayExpression, bool designTime)
        {

            bool result = false;

            if (designTime)
            {
                string[] blackList = { "[[Dev2WebServer]]" };


                int pos = 0;
                while (pos < blackList.Length && !result)
                {

                    if (displayExpression.Contains(blackList[pos]))
                    {
                        result = true;
                    }
                    pos++;
                }
            }

            return result;
        }


        private Guid Upsert<T>(NetworkContext ctx, Guid curDLID, IDev2DataListUpsertPayloadBuilder<T> payload, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            Dev2RecordsetIndexScope rsis = new Dev2RecordsetIndexScope();
            Dev2TokenConverter tc = new Dev2TokenConverter();

            Guid result = GlobalConstants.NullDataListID;
            IBinaryDataList bdl = FetchBinaryDataList(ctx, curDLID, out errors);
            allErrors.MergeErrors(errors);
            errors.ClearErrors();

            IIntellisenseResult part;
            IBinaryDataListEntry entry;
            string error = string.Empty;
            int toRemoveFromGap = -1;

            if (bdl != null)
            {
                // Fetch will force a commit if any frames are hanging ;)
                foreach (IDataListPayloadIterationFrame<T> f in payload.FetchFrames())
                {
                    // iterate per frame fetching frame items
                    while (f.HasData())
                    {
                        DataListPayloadFrameTO<T> frameItem = f.FetchNextFrameItem();

                        // find the part to use
                        part = tc.ParseTokenForMatch(frameItem.Expression, bdl.FetchIntellisenseParts());

                        // recusive eval ;)
                        if (part == null)
                        {
                            IBinaryDataListEntry tmpItem = InternalDataListEvaluate(frameItem.Expression, bdl, true,
                                                                                    out errors);
                            allErrors.MergeErrors(errors);
                            errors.ClearErrors();

                            // now find the correct token based upon the eval
                            if (tmpItem != null)
                            {
                                part = tc.ParseTokenForMatch(tmpItem.FetchScalar().TheValue,
                                                             bdl.FetchIntellisenseParts());
                            }
                        }

                        // now we have the part, build up the item to push into the entry....
                        if (part != null)
                        {
                            // Process evaluated values via some Generic magic....
                            IBinaryDataListEntry evaluatedValue = null;
                            if (typeof(T) == typeof(string))
                            {

                                evaluatedValue = InternalEvaluate(frameItem.Value.ToString(), bdl, false, out errors);

                                //evaluatedValue = Evaluate(ctx, curDLID, enActionType.User, frameItem.Value.ToString(),
                                // out errors);
                            }
                            else if (typeof(T) == typeof(IBinaryDataListEntry))
                            {
                                evaluatedValue = (IBinaryDataListEntry)frameItem.Value;
                            }

                            allErrors.MergeErrors(errors);

                            // check entry cache based upon type ;)
                            if (part.Option.IsScalar)
                            {
                                bdl.TryGetEntry(part.Option.Field, out entry, out error);
                                allErrors.AddError(error);
                                if (entry != null)
                                {
                                    if (evaluatedValue != null)
                                    {
                                        if (!evaluatedValue.IsRecordset)
                                        {
                                            entry.TryPutScalar(
                                                Dev2BinaryDataListFactory.CreateBinaryItem(
                                                    evaluatedValue.FetchScalar().TheValue, part.Option.Field), out error);
                                            allErrors.AddError(error);
                                        }
                                        else
                                        {
                                            // process it as a recordset to scalar ie last value is placed ;)
                                            entry.TryPutScalar(
                                                Dev2BinaryDataListFactory.CreateBinaryItem(
                                                    evaluatedValue.TryFetchLastIndexedRecordsetUpsertPayload(out error)
                                                                  .TheValue, part.Option.Field), out error);
                                            allErrors.AddError(error);
                                        }
                                    } // else do nothing
                                }
                            }
                            else
                            {
                                bdl.TryGetEntry(part.Option.Recordset, out entry, out error);
                                allErrors.AddError(error);
                                if (entry != null)
                                {
                                    int idx = rsis.FetchRecordsetIndex(part, entry, payload.IsIterativePayload());
                                    enRecordsetIndexType idxType =
                                        DataListUtil.GetRecordsetIndexTypeRaw(part.Option.RecordsetIndex);

                                    if (idx > 0)
                                    {
                                        if (!evaluatedValue.IsRecordset)
                                        {
                                            // we have a scalar to recordset....
                                            if (idxType == enRecordsetIndexType.Star)
                                            {
                                                if (!payload.IsIterativePayload())
                                                {
                                                    // scalar to star
                                                    IIndexIterator ii = entry.FetchRecordsetIndexes();
                                                    while (ii.HasMore())
                                                    {
                                                        int next = ii.FetchNextIndex();
                                                        entry.TryPutRecordItemAtIndex(
                                                            Dev2BinaryDataListFactory.CreateBinaryItem(
                                                                evaluatedValue.FetchScalar().TheValue, part.Option.Field),
                                                            next, out error);
                                                        allErrors.AddError(error);
                                                    }
                                                }
                                                else
                                                {
                                                    // we need to move the iteration overwrite indexs ?
                                                    entry.TryPutRecordItemAtIndex(
                                                        Dev2BinaryDataListFactory.CreateBinaryItem(
                                                            evaluatedValue.FetchScalar().TheValue, part.Option.Recordset,
                                                            part.Option.Field, idx), idx, out error);
                                                    allErrors.AddError(error);
                                                }
                                            }
                                            else
                                            {
                                                // scalar to index
                                                IBinaryDataListItem item =
                                                    Dev2BinaryDataListFactory.CreateBinaryItem(
                                                        evaluatedValue.FetchScalar().TheValue, part.Option.Recordset,
                                                        part.Option.Field, idx);
                                                entry.TryPutRecordItemAtIndex(item, idx, out error);
                                                allErrors.AddError(error);
                                            }
                                        }
                                        else
                                        {
                                            //IList<int> starPopIdx = new List<int>();
                                            int starPopIdxPos = 0;

                                            // field to field move
                                            if (part.Option.Field != null && part.Option.Field != string.Empty)
                                            {

                                                IIndexIterator idxItr = evaluatedValue.FetchRecordsetIndexes();
                                                IIndexIterator starPopIdx = entry.FetchRecordsetIndexes();
                                                IList<int> populateIdxs = new List<int> { idx };

                                                if (idxType == enRecordsetIndexType.Star)
                                                {

                                                    starPopIdx = entry.FetchRecordsetIndexes().Clone();

                                                    int gapAdd = 0;
                                                    if (starPopIdx.Count > 0)
                                                    {
                                                        //starPopIdx.Remove(idx);
                                                        toRemoveFromGap = idx;
                                                        starPopIdx.AddGap(idx);
                                                        gapAdd += 1;
                                                    }

                                                    if (idxItr.Count == 1)
                                                    {
                                                        IIndexIterator newIdxItr =
                                                            Dev2BinaryDataListFactory.CreateLoopedIndexIterator(
                                                                idxItr.MinIndex(), (starPopIdx.Count + gapAdd));
                                                        idxItr = newIdxItr; // swap for the repeat ;)
                                                    }
                                                }


                                                // now push the Value data into the recordset
                                                //foreach (int k in iterateIdxs)
                                                while (idxItr.HasMore())
                                                {
                                                    int next = idxItr.FetchNextIndex();
                                                    //IList<IBinaryDataListItem> itms = evaluatedValue.FetchRecordAt(k, out error);
                                                    IList<IBinaryDataListItem> itms = evaluatedValue.FetchRecordAt(
                                                        next, out error);

                                                    //IBinaryDataListItem itms = evaluatedValue.TryFetchLastIndexedRecordsetUpsertPayload(out error);
                                                    allErrors.AddError(error);
                                                    // TODO : Handle * -> () correctly ;)

                                                    foreach (int index in populateIdxs)
                                                    {
                                                        allErrors.AddError(error);
                                                        if (itms != null && itms.Count == 1)
                                                        {
                                                            IBinaryDataListItem itm =
                                                                Dev2BinaryDataListFactory.CreateBinaryItem(
                                                                    itms[0].TheValue, part.Option.Recordset,
                                                                    part.Option.Field, index);
                                                            entry.TryPutRecordItemAtIndex(itm, index, out error);
                                                            allErrors.AddError(error);
                                                        }
                                                        else if (itms != null && itms.Count > 1)
                                                        {
                                                            // all good move it
                                                            foreach (IBinaryDataListItem i in itms)
                                                            {
                                                                IBinaryDataListItem itm =
                                                                    Dev2BinaryDataListFactory.CreateBinaryItem(
                                                                        i.TheValue, part.Option.Recordset,
                                                                        part.Option.Field, index);
                                                                entry.TryPutRecordItemAtIndex(itm, index, out error);
                                                                allErrors.AddError(error);
                                                            }
                                                        }
                                                    }

                                                    // we need to roll the index to keep the ship moving...
                                                    if (entry.IsRecordset && idxType == enRecordsetIndexType.Blank &&
                                                        populateIdxs.Count > 0)
                                                    {
                                                        populateIdxs[0]++;
                                                    }
                                                    else if (idxType == enRecordsetIndexType.Star)
                                                    {
                                                        // handle * iteration ;)

                                                        if (starPopIdxPos < starPopIdx.Count)
                                                        {
                                                            populateIdxs.Clear();
                                                            //populateIdxs.Add(starPopIdx[starPopIdxPos]);
                                                            populateIdxs.Add(starPopIdx.FetchNextIndex());
                                                            starPopIdxPos++;
                                                        }
                                                        else
                                                        {
                                                            // we might still have data being fed into this, inc 
                                                            populateIdxs[0]++;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                // ensure column match on transfer, at least a subset is being moved....
                                                if (entry.HasColumns(evaluatedValue.Columns))
                                                {
                                                    IIndexIterator ii = evaluatedValue.FetchRecordsetIndexes();
                                                    while (ii.HasMore())
                                                    {
                                                        int next = ii.FetchNextIndex();
                                                        IList<IBinaryDataListItem> itms =
                                                            evaluatedValue.FetchRecordAt(next, out error);
                                                        allErrors.AddError(error);
                                                        // all good move it
                                                        foreach (IBinaryDataListItem i in itms)
                                                        {
                                                            //IBinaryDataListItem itm = Dev2BinaryDataListFactory.CreateBinaryItem(i.TheValue, loc.Option.Recordset, loc.Option.Field, i.ItemCollectionIndex.ToString());
                                                            entry.TryPutRecordItemAtIndex(i, i.ItemCollectionIndex,
                                                                                          out error);
                                                            allErrors.AddError(error);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    allErrors.AddError("Field mis-match on Recordset to Recordset transfer");
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        allErrors.AddError("Invalid Recordset index [ " + idx + " ]");
                                    }

                                    // Reset the Gaps 
                                    if (toRemoveFromGap > 0)
                                    {
                                        entry.FetchRecordsetIndexes().RemoveGap(toRemoveFromGap);
                                        toRemoveFromGap = -1;
                                    }
                                }
                            }
                        }
                        else
                        {

                            string token = DataListUtil.StripBracketsFromValue(frameItem.Expression);

                            // system expression ;)
                            if (token.IndexOf(GlobalConstants.SystemTagNamespace) >= 0 ||
                                DataListUtil.isSystemTag(token))
                            {

                                if (token.IndexOf(GlobalConstants.SystemTagNamespace) < 0)
                                {
                                    token = DataListUtil.BuildSystemTagForDataList(token, false);
                                }

                                IBinaryDataListEntry theEntry;
                                bdl.TryGetEntry(token, out theEntry, out error);
                                if (error != string.Empty)
                                {
                                    allErrors.AddError(error);
                                }
                                else
                                {
                                    IBinaryDataListEntry evalautedValue = null;
                                    if (typeof(T) == typeof(string))
                                    {
                                        evalautedValue = Evaluate(ctx, curDLID, enActionType.User,
                                                                  frameItem.Value.ToString(), out errors);
                                    }
                                    else if (typeof(T) == typeof(IBinaryDataListEntry))
                                    {
                                        evalautedValue = (IBinaryDataListEntry)frameItem.Value;
                                    }

                                    if (evalautedValue != null)
                                    {
                                        theEntry.TryPutScalar(evalautedValue.FetchScalar(), out error);
                                    }
                                }
                            }
                            else
                            {

                                allErrors.AddError("Invalid Region " + frameItem.Expression);
                            }
                        }
                    }

                    // move index values
                    rsis.MoveIndexesToNextPosition();
                }



                // Now flush all the entries to the bdl for this iteration ;)
                if (TryPushDataList(bdl, out error))
                {
                    // TODO : Remove the gap add ;) 

                    result = bdl.UID;
                }
                allErrors.AddError(error);

            }

            errors = allErrors;

            return result;
        }


        #endregion

    }
}
