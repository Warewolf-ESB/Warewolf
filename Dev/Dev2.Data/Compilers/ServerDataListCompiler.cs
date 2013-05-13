using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.Data.SystemTemplates;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Interfaces;
using Dev2.DataList.Contract.TO;
using Dev2.DataList.Contract.Translators;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Enums;
using Dev2.MathOperations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Dev2.Server.Datalist
{

    /// <summary>
    /// Server DataList compiler
    /// </summary>
    internal class ServerDataListCompiler : IServerDataListCompiler
    {

        private readonly IDev2DataLanguageParser _parser = DataListFactory.CreateLanguageParser();
        // DataList Server
        private readonly IDataListServer _dlServer;

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

        /// <summary>
        /// PLEASE NOTE THE META-DATA RETURNED FROM THIS METHOD IS TRANSIENT, THIS MEANS IT WILL ONLY BE EVENTUALLY CONSISTENT
        /// DO NOT RELY UPON IT FOR UI DRIVEN DETAILS, PLEASE WRITE YOUR OWN HELPER METHODS TO ACHIEVE YOUR GOAL
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="curDLID"></param>
        /// <param name="typeOf"></param>
        /// <param name="expression"></param>
        /// <param name="errors"></param>
        /// <param name="returnExpressionIfNoMatch"></param>
        /// <returns></returns>
        public IBinaryDataListEntry Evaluate(NetworkContext ctx, Guid curDLID, enActionType typeOf, string expression, out ErrorResultTO errors, bool returnExpressionIfNoMatch = false)
        {

            ErrorResultTO allErrors = new ErrorResultTO();
            errors = new ErrorResultTO();
            string error = string.Empty;

            IBinaryDataList theDL = TryFetchDataList(curDLID, out error);
            IBinaryDataListEntry result = null;
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

                    IBinaryDataListEntry newDlEntry = Dev2BinaryDataListFactory.CreateEntry(GlobalConstants.EvalautionScalar, string.Empty, theDL.UID);

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
                else if (typeOf == enActionType.CalculateSubstitution)
                {
                    // Travis.Frisinger : 31.01.2013 - Added to properly levage the internal language   

                    // Break the expression up by , and sub in values?
                    IDev2DataLanguageParser parser = new Dev2DataLanguageParser();
                    IList<IIntellisenseResult> myParts = parser.ParseExpressionIntoParts(expression, theDL.FetchIntellisenseParts());

                    // Fetch each DL expression in the master expression and evalaute
                    // Then build up the correct string to sub in ;)
                    foreach (IIntellisenseResult p in myParts)
                    {

                        // Ensure the expression exist and it is not a range operation
                        if (p.Type == enIntellisenseResultType.Selectable
                            && expression.IndexOf(p.Option.DisplayValue, StringComparison.Ordinal) >= 0
                            && expression.IndexOf((p.Option.DisplayValue + ":"), StringComparison.Ordinal) < 0
                            && expression.IndexOf((":" + p.Option.DisplayValue), StringComparison.Ordinal) < 0)
                        {
                            IBinaryDataListEntry bde = InternalEvaluate(p.Option.DisplayValue, theDL, returnExpressionIfNoMatch, out errors);
                            if (bde != null)
                            {
                                if (bde.IsRecordset)
                                {
                                    // recordset op - build up the correct string to inject
                                    IIndexIterator idxItr = bde.FetchRecordsetIndexes();
                                    StringBuilder sb = new StringBuilder();

                                    while (idxItr.HasMore())
                                    {
                                        IList<IBinaryDataListItem> items = bde.FetchRecordAt(idxItr.FetchNextIndex(), out error);
                                        allErrors.AddError(error);
                                        foreach (IBinaryDataListItem itm in items)
                                        {
                                            if (itm.TheValue != string.Empty)
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

                                    //2013.02.08: Ashley Lewis - Bug 8725, Task 8797: Avoid index out of range exception on blank record set
                                    toInject = toInject.Length > 0 ? toInject.Substring(0, (toInject.Length - 1)) : "\"\"";

                                    expression = expression.Replace(p.Option.DisplayValue, toInject);

                                }
                                else
                                {
                                    // scalar op
                                    string eVal = CalcPrepValue(bde.FetchScalar().TheValue);
                                    expression = expression.Replace(p.Option.DisplayValue, eVal);
                                }
                            }
                            allErrors.MergeErrors(errors);
                        }
                    }

                    allErrors.MergeErrors(errors);

                    IBinaryDataListEntry calcResult = Dev2BinaryDataListFactory.CreateEntry(GlobalConstants.EvalautionScalar, string.Empty, theDL.UID);
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
            var allErrors = new ErrorResultTO();
            string error = string.Empty;

            Guid res = GlobalConstants.NullDataListID;

            IBinaryDataList tmpDl = TryFetchDataList(curDLID, out error);
            if (error != string.Empty)
            {
                allErrors.AddError(error);
            }
            else
            {
                // Ensure we have a non-null tmpDL

                IBinaryDataList result = tmpDl.Clone(enTranslationDepth.Data, out errors);
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
            var allErrors = new ErrorResultTO();

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
            // Use eval and upsert to processes definitions
            Guid result = Guid.Empty;
            errors = new ErrorResultTO();

            switch (typeOf)
            {
                case enDev2ArgumentType.Input:
                    result = PerformInputShaping(ctx, curDLID, typeOf, definitions, ref errors, result);
                break;
                case enDev2ArgumentType.Output_Append_Style:
                case enDev2ArgumentType.Output:
                    result = PerformOutputShaping(ctx, curDLID, typeOf, definitions, ref errors, result);
                break;
            }

            return result;

        }

        Guid PerformOutputShaping(NetworkContext ctx, Guid curDLID, enDev2ArgumentType typeOf, string definitions, ref ErrorResultTO errors, Guid result)
        {
            IDev2LanguageParser parser = DataListFactory.CreateOutputParser();
            IList<IDev2Definition> defs = parser.Parse(definitions);
            if (defs.Count > 0)
            {
                result = InternalShape(ctx, curDLID, defs, typeOf, out errors);
            }
            else
            {
                // default to a union since there are no defs....
                result = UnionDataList(curDLID, ref errors, result);
            }
            return result;
        }

        Guid PerformInputShaping(NetworkContext ctx, Guid curDLID, enDev2ArgumentType typeOf, string definitions, ref ErrorResultTO errors, Guid result)
        {
            IDev2LanguageParser parser = DataListFactory.CreateInputParser();
            IList<IDev2Definition> defs = parser.Parse(definitions);
            if (defs.Count > 0)
            {
                result = InternalShape(ctx, curDLID, defs, typeOf, out errors);
            }
            else
            {
                // default to a clone becuase there is nothing here ;)
                result = CloneDataList(curDLID, ref errors, result);
            }
            return result;
        }

        Guid CloneDataList(Guid curDLID, ref ErrorResultTO errors, Guid result)
        {
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
            return result;
        }

        Guid UnionDataList(Guid curDLID, ref ErrorResultTO errors, Guid result)
        {
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
                    TryPushDataList(tmp, out error);
                    if (error != string.Empty)
                    {
                        errors.AddError(error);
                    }
                    result = tmp.UID;
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
                    // Push back into the server now ;)
                    if (!TryPushDataList(result, out error))
                    {
                        allErrors.AddError(error);
                    }
                }
                returnVal = result.UID;
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
            return null;
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

                    if (result != null)
                    {
                        // set the uid and place in cache
                        returnVal = result.UID;

                        if (!TryPushDataList(result, out error))
                        {
                            allErrors.AddError(error);
                        }
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

        public string ConvertAndFilter(NetworkContext ctx, Guid curDLID, string filterShape, DataListFormat typeOf, out ErrorResultTO errors)
        {
            IBinaryDataList result;
            string res = string.Empty;
            ErrorResultTO allErrors = new ErrorResultTO();
            string error = string.Empty;

            result = TryFetchDataList(curDLID, out error);

            if (result != null)
            {
                try
                {

                    IDataListTranslator t = _dlServer.GetTranslator(typeOf);
                    if (t != null)
                    {
                        res = t.ConvertAndFilter(result, filterShape, out errors);
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

            return res;
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
            IBinaryDataList bdl = TryFetchDataList(curDLID, out error);

            Guid result = GlobalConstants.NullDataListID;

            if (error != string.Empty)
            {
                allErrors.AddError(error);
                error = string.Empty;
            }

            if (bdl != null)
            {
                string tt = DataListUtil.BuildSystemTagForDataList(tag, false);
                allErrors.MergeErrors(errors);
                IBinaryDataListItem itm = null;

                if (typeof(T) == typeof(string))
                {
                    itm = Dev2BinaryDataListFactory.CreateBinaryItem(val.ToString(), tt);

                }
                else if (typeof(T) == typeof(IBinaryDataListEntry))
                {
                    itm = ((IBinaryDataListEntry)val).FetchScalar();
                }

                IBinaryDataListEntry et;
                result = GlobalConstants.NullDataListID;

                
                if (bdl.TryCreateScalarTemplate(string.Empty, tt, string.Empty, true, out error))
                {
                    bdl.TryGetEntry(tt, out et, out error);

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
            var allErrors = new ErrorResultTO();
            string theShape = DataListUtil.GenerateDataListFromDefs(definitions);
            byte[] empty = new byte[0];
            string error;

            // alwasy an internal XML format ;)
            Guid shellId;
            if (typeOf == enDev2ArgumentType.Input)
            {
                shellId = ConvertTo(ctx, DataListFormat.CreateFormat(GlobalConstants._XML), empty, theShape, out errors);
            }
            else
            {
                // output shape, we already have the ID, fetch parentID for merge up ;)
                shellId = curDLID;
            }
            if (errors.HasErrors())
            {
                allErrors.MergeErrors(errors);
            }
            else
            {
                // now we have a IBinaryDataList, we need to set the ParentID and populate with data
                IBinaryDataList childDL = FetchBinaryDataList(ctx, shellId, out errors);
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
                    Guid extractFromId = curDLID;
                    Guid pushToId = childDL.UID;

                    if (typeOf == enDev2ArgumentType.Input)
                    { // input shaping
                        inputExpressionExtractor = def =>
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

                        outputExpressionExtractor = def =>
                        {
                            string expression;

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

                            inputExpressionExtractor = def =>
                            {
                                string expression;

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
                            inputExpressionExtractor = def =>
                            {
                                string expression;

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

                        outputExpressionExtractor = def =>
                        {
                            string expression = string.Empty;

                            if (def.Value != string.Empty)
                            {
                                expression = def.RawValue;
                            }

                            return expression;
                        };

                        // swap extract from and pushTo around
                        extractFromId = childDL.UID;
                        pushToId = childDL.ParentUID;
                    }

                    // build framed upsert expression ;)
                    var toUpsert = Dev2DataListBuilderFactory.CreateBinaryDataListUpsertBuilder(false);

                    // populate with parentDL data as per the definitions ;)
                    foreach (IDev2Definition def in definitions)
                    {

                        string expression = inputExpressionExtractor(def);

                        if (expression != string.Empty)
                        {
                            // Evaluate from extractDL
                            IBinaryDataListEntry val = Evaluate(ctx, extractFromId, enActionType.User, expression, out errors);
                            allErrors.MergeErrors(errors);
                            if (val == null)
                            {
                                string errorTmp;
                                val = DataListConstants.baseEntry.Clone(enTranslationDepth.Shape, pushToId, out errorTmp);
                            }

                            // now upsert into the pushDL
                            string upsertExpression = outputExpressionExtractor(def);
                            toUpsert.Add(upsertExpression, val);
                        }
                        else if (expression == string.Empty && (typeOf == enDev2ArgumentType.Input) && def.IsRequired)
                        {
                            allErrors.AddError("Required input " + def.RawValue + " cannot be populated");
                        }
                    }

                    Upsert(ctx, pushToId, toUpsert, out errors);
                    allErrors.MergeErrors(errors);

                    result = pushToId;

                    // Merge System tags too ;)
                    // exctractFromID, pushToID
                    foreach (enSystemTag t in TranslationConstants.systemTags)
                    {
                        IBinaryDataListEntry sysVal = Evaluate(ctx, extractFromId, enActionType.System, t.ToString(), out errors);
                        if (sysVal == null)
                        {
                            string errorTmp;
                            sysVal = DataListConstants.baseEntry.Clone(enTranslationDepth.Shape, pushToId, out errorTmp);
                        }
                        if (errors.HasErrors())
                        {
                            allErrors.MergeErrors(errors);
                        }

                        UpsertSystemTag(pushToId, t, sysVal, out errors);
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
        public bool TryPushDataList(IBinaryDataList payload, out string error)
        {
            bool r = true;
            error = string.Empty;
            ErrorResultTO errors = new ErrorResultTO();
            Guid key = payload.UID;

            if (!_dlServer.WriteDataList(payload.UID, payload, out errors))
            {
                error = "Failed to write DataList";
                r = false; //Bug 8796, r was never being set to false >.<
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
                result = functionEvaluator.EvaluateFunction(evaluationFunctionTO, bdl.UID, out errors);
            }
            catch(Exception e)
            {
                errors.AddError(e.Message);
            }

            return result;
        }

        /// <summary>
        /// Internals the evaluate.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        private IBinaryDataListEntry InternalEvaluate(string expression, IBinaryDataList bdl, bool toRoot, out ErrorResultTO errors)
        {
            IBinaryDataListEntry result;

            ErrorResultTO allErrors = new ErrorResultTO();
            string error;
            errors = new ErrorResultTO();
            string calcExp = string.Empty;

            if (IsCalcEvaluation(expression, out calcExp))
            {
                expression = calcExp;
                string r = InternalCalcEvaluation(expression, bdl, out errors);
                allErrors.MergeErrors(errors);
                result = Dev2BinaryDataListFactory.CreateEntry(GlobalConstants.EvalautionScalar, string.Empty, bdl.UID);
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
        /// Determines whether the specified payload is evaluated.
        /// NOTE: This method also exist in the DataListUtil class
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>
        ///   <c>true</c> if the specified payload is evaluated; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEvaluated(string payload)
        {
            var result = payload.IndexOf("[[", StringComparison.Ordinal) >= 0;

            return result;
        }

        /// <summary>
        /// Internals the data list evaluate.
        /// PLEASE NOTE THE META-DATA RETURNED FROM THIS METHOD IS TRANSIENT, THIS MEANS IT WILL ONLY BE EVENTUALLY CONSISTENT
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        private IBinaryDataListEntry InternalDataListEvaluate(string expression, IBinaryDataList bdl, bool toRoot, out ErrorResultTO errors)
        {
            if (IsEvaluated(expression))
            {
                string lastFetch2;

                IBinaryDataListEntry lastFetch = null;
                ErrorResultTO allErrors = new ErrorResultTO();
                
                errors = new ErrorResultTO();
                string error = string.Empty;
                IDictionary<int, bool> deferedReads = new Dictionary<int, bool>(10);

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
                    IList<string> RecursiveVals = new List<string>();
                    while (loopEval())
                    {
                        foundMatch = false;
                        IList<IIntellisenseResult> expressionParts = _parser.ParseExpressionIntoParts(expression, bdl.FetchIntellisenseParts());
                        foreach (IIntellisenseResult p in expressionParts)
                        {
                            string displayValue = p.Option.DisplayValue;
                            string field = p.Option.Field;
                            string rs = p.Option.Recordset;

                            if (p.Type == enIntellisenseResultType.Error)
                            {
                                hasError = true;
                                allErrors.AddError(p.Message);
                                // attempt to remove the fragement not found if outside of design mode
                                if (!designTimeBinding)
                                {
                                    expression = expression.Replace(displayValue, "");
                                }
                            }
                            else if (p.Type == enIntellisenseResultType.Selectable && expression.Contains(displayValue))
                            {

                                // Evaluate from the DataList
                                IBinaryDataListEntry val;
                                bool fetchOk = false;
                                if (p.Option.IsScalar)
                                {
                                    fetchOk = bdl.TryGetEntry(field, out val, out error);
                                    allErrors.AddError(error);
                                }
                                else
                                {
                                    fetchOk = bdl.TryGetEntry(rs, out val, out error);
                                    allErrors.AddError(error);
                                }

                                if (fetchOk)
                                {
                                    matchCnt++;
                                    foundMatch = true;
                                    lastFetch = val.Clone(enTranslationDepth.Data, bdl.UID, out error); // clone to avoid mutation issues
                                    allErrors.AddError(error);
                                }

                                if (p.Option.IsScalar && val != null)
                                {
                                    var itm = val.FetchScalar();
                                    var theValue = itm.TheValue;


                                    RecursiveVals.Add(displayValue);
                                    if (RecursiveVals.FirstOrDefault(c => c.Equals(theValue)) == null)
                                    {
                                        //expression = expression.Replace(p.Option.DisplayValue, val.FetchScalar().TheValue);
                                        //2013.02.13: Ashley Lewis - Bug 8725, Task 8913 - handle escape characters being inserted into expressions
                                        expression = expression.StartsWith("{")
                                                        ? expression.Replace(displayValue, theValue.Replace("\"", "\\\""))
                                                        : expression.Replace(displayValue, theValue);

                                        // set defered read action
                                        if (itm.IsDeferredRead)
                                        {
                                            deferedReads[expression.GetHashCode()] = true;
                                        }
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

                                        int myIdx;
                                        if (idxType == enRecordsetIndexType.Numeric)
                                        {
                                            myIdx = Int32.Parse(idx);
                                        }
                                        else
                                        {
                                            myIdx = val.FetchLastRecordsetIndex();
                                        }

                                        if (!string.IsNullOrEmpty(field))
                                        {
                                            // we want an entry at a set location
                                            if (error != string.Empty)
                                            {
                                                hasError = true;
                                                matchCnt--;
                                                allErrors.AddError(error);
                                                lastFetch = Dev2BinaryDataListFactory.CreateEntry(GlobalConstants.NullEntryNamespace, string.Empty, bdl.UID); // set a blank match too ;)
                                                expression = expression.Replace(p.Option.DisplayValue, string.Empty); // blank the match to avoid looping ;)
                                            }
                                            else
                                            {
                                                lastFetch2 = lastFetch.TryFetchRecordsetColumnAtIndex(field, myIdx, out error).TheValue;
                                                // build up the result, via a strip all but method?
                                                lastFetch.MakeRecordsetEvaluateReady(myIdx, field, out error);
                                                allErrors.AddError(error);

                                                IBinaryDataListItem valT = lastFetch.TryFetchRecordsetColumnAtIndex(field, myIdx, out error);

                                                string subVal = string.Empty;

                                                if (val != null)
                                                {
                                                    if(valT.TheValue == string.Empty && lastFetch2 != string.Empty)
                                                    {
                                                        subVal = lastFetch2;
                                                    }
                                                    else
                                                    {
                                                        subVal = valT.TheValue;
                                                    }
                                                }

                                                //expression = expression.Replace(p.Option.DisplayValue, subVal);
                                                //2013.02.13: Ashley Lewis - Bug 8725, Task 8913 - handle escape characters being inserted into expressions
                                                expression = expression.StartsWith("{")
                                                    ? expression.Replace(displayValue, subVal.Replace("\"", "\\\""))
                                                    : expression = expression.Replace(displayValue, subVal);
                                            }
                                        }
                                        else
                                        {
                                            // they want the entire recordset? -- blank expression
                                            expression = expression.Replace(displayValue, string.Empty);
                                            // Check for an index and remove all but this index ;)
                                            if (idxType == enRecordsetIndexType.Numeric || idxType == enRecordsetIndexType.Blank)
                                            {
                                                lastFetch.MakeRecordsetEvaluateReady(myIdx, GlobalConstants.AllColumns, out error);
                                                allErrors.AddError(error);
                                            }
                                            // else already handled because we fetched it all ;)

                                        }
                                    }
                                    else if (idxType == enRecordsetIndexType.Error)
                                    {
                                        // we all it all
                                        allErrors.AddError("Invalid Recordset Index");
                                        foundMatch = false;
                                    }
                                    else if (idxType == enRecordsetIndexType.Star)
                                    {
                                        // they want the whole thing, send it and blank expression
                                        if (!string.IsNullOrEmpty(field))
                                        {
                                            lastFetch.MakeRecordsetEvaluateReady(GlobalConstants.AllIndexes, field, out error);
                                            allErrors.AddError(error);
                                        } // else we need a column match to process by evaluate

                                        // break into parts for append ;)
                                        string token = p.Option.DisplayValue;
                                        string[] expParts = BreakStaticExpressionIntoPreAndPostPart(expression, token);
                                        expression = expression.Replace(token, string.Empty);
                                        

                                        // Bug 7835
                                        if(!string.IsNullOrEmpty(expression) && !IsEvaluated(expression))
                                        {
                                            lastFetch = EvaluateComplexExpression(lastFetch, expParts, out errors);
                                            allErrors.MergeErrors(errors);
                                            expression = string.Empty; // all good to blank it ;)
                                        }else if(!string.IsNullOrEmpty(expression) && IsEvaluated(expression))
                                        {

                                            // we need to evalaute the pre and post portions of the string to get the ordering right ;)
                                            if(expParts[0] != null)
                                            {
                                                expParts[0] = EvaluateExpressionPart(expParts[0], bdl, out errors);
                                                allErrors.MergeErrors(errors);
                                                errors.ClearErrors();
                                            }

                                            if(expParts[1] != null)
                                            {
                                                expParts[1] = EvaluateExpressionPart(expParts[1], bdl, out errors);
                                                allErrors.MergeErrors(errors);
                                                errors.ClearErrors();
                                            }

                                            
                                            lastFetch = EvaluateComplexExpression(lastFetch, expParts, out errors);
                                            expression = string.Empty;
                                              
                                        }

                                        if (expression != string.Empty && expression != " ") //Bug 7836
                                        {
                                            hasError = true;
                                            matchCnt--;
                                            allErrors.AddError("Attempt to use Recordset with * in a complex expression");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (designTimeBinding)
                                {
                                    // we need to keep unbound references in this mode
                                    if (expression.IndexOf(displayValue, StringComparison.Ordinal) >= 0)
                                    {
                                        Guid tmp = _dlServer.IDProvider.AllocateID();
                                        string replace = string.Concat("Dev2", tmp.ToString());
                                        expression = expression.Replace(displayValue, replace);
                                        _notFoundReplaceParts.Add(displayValue, replace);
                                        foundMatch = true;
                                        matchCnt++;
                                    }
                                }
                                else
                                {
                                    // it needs to be evaluated, blank it
                                    if (expression.Contains(displayValue))
                                    {
                                        foundMatch = true;
                                        matchCnt++;
                                        expression = expression.Replace(displayValue, string.Empty);
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

                if (!hasError)
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
                if (expression != string.Empty && expression != " ") //Bug 7836
                {
                    // we just need to return the expression as is in a IBinaryDataListEntry, but respect the defered read action
                    string fieldName = GlobalConstants.NullEntryNamespace+Guid.NewGuid().ToString();
                    IBinaryDataListEntry tmp = Dev2BinaryDataListFactory.CreateEntry(fieldName, string.Empty, bdl.UID);
                    bool deferedReadFlag;
                    IBinaryDataListItem itm = DataListConstants.baseItem.Clone();

                    if (deferedReads.TryGetValue(expression.GetHashCode(), out deferedReadFlag))
                    {
                        itm.IsDeferredRead = true;
                    }

                    itm.UpdateValue(expression);
                    itm.UpdateField(fieldName);

                    tmp.TryPutScalar(itm, out error);
                    lastFetch = tmp; // return the expression as the result now ;)

                }

                // super finally, remove the iteration evaluation since this is a wizard specific feature ;)
                if (isIterationDriven)
                {
                    UpsertSystemTag(bdl.UID, enSystemTag.EvaluateIteration, "false", out errors);
                }

                errors = allErrors;

                return lastFetch;
            }

            // else
            errors = new ErrorResultTO();
            string error2;
            var fieldName2 = GlobalConstants.NullEntryNamespace+Guid.NewGuid().ToString();
            IBinaryDataListEntry lastFetch21 = new BinaryDataListEntry(fieldName2, string.Empty, bdl.UID);

            lastFetch21.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem(expression, fieldName2), out error2);
            errors.AddError(error2);
            return lastFetch21;
        }


        private string EvaluateExpressionPart(string part, IBinaryDataList bdl, out ErrorResultTO errors)
        {
            string error = string.Empty;
            string result = part;
            // fully eval expression before we append it ;)
            IBinaryDataListEntry entry = InternalDataListEvaluate(part, bdl, false, out errors);
            errors.AddError(error);
            if (entry != null && !entry.IsRecordset)
            {
                // Append to recordset ;)
                IBinaryDataListItem itm = entry.FetchScalar();
                if (itm != null)
                {
                    result = itm.TheValue;
                }
                else
                {
                    errors.AddError("Attempted to evaluated a complex expression with recordset, but failed to fully evaluated it");
                }

            }

            return result;
        }

        /// <summary>
        /// Breaks the static expression into pre and post part.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="rsToken">The rs token.</param>
        /// <returns></returns>
        private string[] BreakStaticExpressionIntoPreAndPostPart(string expression, string rsToken)
        {
            string[] result = new string[2];

            int idx = expression.IndexOf(rsToken, StringComparison.Ordinal);
            int len = rsToken.Length;
            int end = expression.Length;
            
            if(idx > 0)
            {
                // we have prefix data
                string tmp = expression.Substring(0, idx);
                result[0] = tmp; 

                // check for post fix data
                
                if((idx + len) < end)
                {
                    // we have post fix data
                    tmp = expression.Substring((idx+len), (end-(idx+len)));
                    result[1] = tmp;
                }

            }else if(idx == 0)
            {
                // we have only postfix data
                result[0] = null;
                result[1] = expression.Substring(len, (end - len));
            }


            return result;
        } 

        /// <summary>
        /// Evaluates the complex expression.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="rawExpression">The raw expression.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private IBinaryDataListEntry EvaluateComplexExpression(IBinaryDataListEntry payload, string[] expParts, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();

            string error;
            if (payload.IsRecordset)
            {
                IIndexIterator idxItr = payload.FetchRecordsetIndexes();
                while (idxItr.HasMore())
                {
                    int next = idxItr.FetchNextIndex();
                    IList<IBinaryDataListItem> itms = payload.FetchRecordAt(next, out error);
                    errors.AddError(error);
                    if (itms != null)
                    {
                        foreach (IBinaryDataListItem itm in itms)
                        {
                            IBinaryDataListItem tmp = itm.Clone();
                            //tmp.UpdateValue(itm.TheValue + expression);
                            if(expParts[0] != null && expParts[1] != null)
                            {
                                // pre and post fix append
                                tmp.UpdateValue(expParts[0] + itm.TheValue + expParts[1]);    
                            }else if(expParts[0] == null && expParts[1] != null)
                            {
                                // postfix append
                                tmp.UpdateValue(itm.TheValue + expParts[1]);
                            }else if(expParts[0] != null && expParts[1] == null)
                            {
                                // prefix append only
                                tmp.UpdateValue(expParts[0] + itm.TheValue);
                            }
                                
                            payload.TryPutRecordItemAtIndex(tmp, next, out error);
                        }

                        errors.AddError(error);
                    }
                }
            }


            return payload;
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

        //PBI 8735 - Massimo.Guerrera - Debug items for the multiassign
        private List<KeyValuePair<string, IBinaryDataListEntry>> debugValues = new List<KeyValuePair<string, IBinaryDataListEntry>>();

        public List<KeyValuePair<string, IBinaryDataListEntry>> GetDebugItems()
        {
            return debugValues;
        }

        private Guid Upsert<T>(NetworkContext ctx, Guid curDLID, IDev2DataListUpsertPayloadBuilder<T> payload, out ErrorResultTO errors)
        {
            debugValues = new List<KeyValuePair<string,IBinaryDataListEntry>>();            
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
                    IBinaryDataListEntry entryUsed = null;
                    // iterate per frame fetching frame items
                    while (f.HasData())
                    {
                        DebugOutputTO debugOutputTO = new DebugOutputTO();
                        DataListPayloadFrameTO<T> frameItem = f.FetchNextFrameItem();

                        // find the part to use
                        part = tc.ParseTokenForMatch(frameItem.Expression, bdl.FetchIntellisenseParts());

                        // recusive eval ;)
                        if (part == null)
                        {
                            IBinaryDataListEntry tmpItem = InternalDataListEvaluate(frameItem.Expression, bdl, true, out errors);
                            allErrors.MergeErrors(errors);
                            errors.ClearErrors();

                            // now find the correct token based upon the eval
                            if (tmpItem != null)
                            {
                                part = tc.ParseTokenForMatch(tmpItem.FetchScalar().TheValue, bdl.FetchIntellisenseParts());
                            }
                        }

                        // now we have the part, build up the item to push into the entry....
                        if (part != null)
                        {

                            string field = part.Option.Field;
                            string rs = part.Option.Recordset;

                            string itemVal;
                            // Process evaluated values via some Generic magic....
                            IBinaryDataListEntry evaluatedValue = null;
                            if (typeof(T) == typeof(string))
                            {
                                itemVal = frameItem.Value.ToString();

                                evaluatedValue = InternalEvaluate(itemVal, bdl, false, out errors);
                                allErrors.MergeErrors(errors);

                            }
                            else if (typeof(T) == typeof(IBinaryDataListEntry))
                            {
                                evaluatedValue = (IBinaryDataListEntry)frameItem.Value;

                                if (!evaluatedValue.IsRecordset)
                                {
                                    IBinaryDataListItem itm = evaluatedValue.FetchScalar();
                                    if (!itm.IsDeferredRead)
                                    {
                                        var val = evaluatedValue.FetchScalar().TheValue;
                                        IIntellisenseResult res = tc.ParseTokenForMatch(val, bdl.FetchIntellisenseParts());
                                        if (res != null && res.Type == enIntellisenseResultType.Selectable)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            //if(payload.IsDebug)
                            //{
                            //    string debugError;
                            //    payload.DebugOutputs.Add(CreateDebugOuputItem(part, evaluatedValue.Clone(enTranslationDepth.Data, Guid.NewGuid(),out debugError), bdl));
    
                            //}                            

                            allErrors.MergeErrors(errors);

                            // check entry cache based upon type ;)
                            if (part.Option.IsScalar)
                            {
                                bdl.TryGetEntry(field, out entry, out error);
                                allErrors.AddError(error);

                                if(payload.IsDebug)
                                {
                                    if(entry != null)
                                    {
                                        debugOutputTO.TargetEntry = entry.Clone(enTranslationDepth.Data, Guid.NewGuid(), out error);
                                    }
                                }
                                
                                if (entry != null)
                                {
                                    if (evaluatedValue != null)
                                    {
                                        if (!evaluatedValue.IsRecordset)
                                        {
                                            // 01.02.2013 - Travis.Frisinger : Bug 8579 
                                            IBinaryDataListItem tmpI = evaluatedValue.FetchScalar().Clone();

                                            tmpI.UpdateField(field);
                                            entry.TryPutScalar(tmpI, out error);
                                            allErrors.AddError(error);

                                            //bdl
                                        }
                                        else
                                        {

                                            // process it as a recordset to scalar ie last value is placed ;)
                                            // 01.02.2013 - Travis.Frisinger : Bug 8579 
                                            IBinaryDataListItem tmpI = evaluatedValue.TryFetchLastIndexedRecordsetUpsertPayload(out error).Clone();
                                            tmpI.UpdateField(field);
                                            entry.TryPutScalar(tmpI, out error);
                                            allErrors.AddError(error);
                                        }
                                    } // else do nothing
                                }
                            }
                            else
                            {
                                bdl.TryGetEntry(part.Option.Recordset, out entry, out error);

                                if (payload.IsDebug)
                                {
                                    debugOutputTO.TargetEntry = entry.Clone(enTranslationDepth.Data, Guid.NewGuid(), out error);
                                }

                                allErrors.AddError(error);
                                if (entry != null)
                                {
                                    int idx = rsis.FetchRecordsetIndex(part, entry, payload.IsIterativePayload());
                                    enRecordsetIndexType idxType =
                                        DataListUtil.GetRecordsetIndexTypeRaw(part.Option.RecordsetIndex);

                                    if (idx > 0)
                                    {
                                        if (evaluatedValue != null)
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
                                                            // 01.02.2013 - Travis.Frisinger : Bug 8579 
                                                            IBinaryDataListItem tmpI = evaluatedValue.FetchScalar().Clone();
                                                            tmpI.UpdateField(field);
                                                            entry.TryPutRecordItemAtIndex(tmpI, next, out error);
                                                            allErrors.AddError(error);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // we need to move the iteration overwrite indexs ?
                                                        // 01.02.2013 - Travis.Frisinger : Bug 8579 
                                                        IBinaryDataListItem tmpI = evaluatedValue.FetchScalar().Clone();
                                                        tmpI.UpdateField(field);
                                                        tmpI.UpdateRecordset(rs);
                                                        tmpI.UpdateIndex(idx);

                                                        entry.TryPutRecordItemAtIndex(tmpI, idx, out error);

                                                        allErrors.AddError(error);
                                                    }
                                                }
                                                else
                                                {
                                                    // scalar to index
                                                    // 01.02.2013 - Travis.Frisinger : Bug 8579 

                                                    IBinaryDataListItem tmpI = evaluatedValue.FetchScalar().Clone();
                                                    tmpI.UpdateRecordset(rs);
                                                    tmpI.UpdateField(field);
                                                    tmpI.UpdateIndex(idx);
                                                    entry.TryPutRecordItemAtIndex(tmpI, idx, out error);
                                                    allErrors.AddError(error);
                                                }
                                            }
                                            else
                                            {
                                                //IList<int> starPopIdx = new List<int>();
                                                int starPopIdxPos = 0;

                                                // field to field move
                                                if (!string.IsNullOrEmpty(field))
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
                                                            IIndexIterator newIdxItr = Dev2BinaryDataListFactory.CreateLoopedIndexIterator(idxItr.MinIndex(), (starPopIdx.Count + gapAdd));
                                                            idxItr = newIdxItr; // swap for the repeat ;)
                                                        }
                                                    }


                                                    // now push the Value data into the recordset
                                                    //foreach (int k in iterateIdxs)
                                                    while (idxItr.HasMore())
                                                    {
                                                        int next = idxItr.FetchNextIndex();
                                                        IList<IBinaryDataListItem> itms = evaluatedValue.FetchRecordAt(next, out error);

                                                        allErrors.AddError(error);
                                                        // TODO : Handle * -> () correctly ;)

                                                        foreach (int index in populateIdxs)
                                                        {
                                                            allErrors.AddError(error);
                                                            if (itms != null && itms.Count == 1)
                                                            {
                                                                // 01.02.2013 - Travis.Frisinger : Bug 8579 
                                                                IBinaryDataListItem tmpI = itms[0].Clone();
                                                                tmpI.UpdateRecordset(rs);
                                                                tmpI.UpdateField(field);
                                                                tmpI.UpdateIndex(index);
                                                                entry.TryPutRecordItemAtIndex(tmpI, index, out error);
                                                                allErrors.AddError(error);
                                                            }
                                                            else if (itms != null && itms.Count > 1)
                                                            {
                                                                // all good move it
                                                                foreach (IBinaryDataListItem i in itms)
                                                                {
                                                                    // 01.02.2013 - Travis.Frisinger : Bug 8579 
                                                                    IBinaryDataListItem tmpI = i.Clone();

                                                                    tmpI.UpdateRecordset(rs);
                                                                    tmpI.UpdateField(field);
                                                                    tmpI.UpdateIndex(index);

                                                                    entry.TryPutRecordItemAtIndex(tmpI, index, out error);
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
                                                            IList<IBinaryDataListItem> itms = evaluatedValue.FetchRecordAt(next, out error);
                                                            allErrors.AddError(error);
                                                            // all good move it
                                                            foreach (IBinaryDataListItem i in itms)
                                                            {
                                                                
                                                                entry.TryPutRecordItemAtIndex(i, i.ItemCollectionIndex, out error);
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
                            if (payload.IsDebug)
                            {
                                debugOutputTO.FromEntry = evaluatedValue.Clone(enTranslationDepth.Data, Guid.NewGuid(),out error);
                            }
                            //entryUsed = evaluatedValue;
                        }
                        else
                        {

                            string token = DataListUtil.StripBracketsFromValue(frameItem.Expression);

                            // system expression ;)
                            if (token.IndexOf(GlobalConstants.SystemTagNamespace, StringComparison.Ordinal) >= 0 || DataListUtil.isSystemTag(token))
                            {

                                if (token.IndexOf(GlobalConstants.SystemTagNamespace, StringComparison.Ordinal) < 0)
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
                                        evalautedValue = Evaluate(ctx, curDLID, enActionType.User, frameItem.Value.ToString(), out errors);
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
                                //entryUsed = theEntry;
                            }
                            else
                            {

                                allErrors.AddError("Invalid Region " + frameItem.Expression);
                            }
                        }
                        //payload.DebugOutputs.Add(new DebugOutputTO(entry,entryUsed));
                        payload.DebugOutputs.Add(debugOutputTO);
                        debugValues.Add(new KeyValuePair<string, IBinaryDataListEntry>(frameItem.Expression,entryUsed));
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

        DebugOutputTO CreateDebugOuputItem(IIntellisenseResult part, IBinaryDataListEntry evaluatedValue,IBinaryDataList dataList)
        {
            IBinaryDataListEntry tmpEntry;
            string error;
            if(part.Option.IsScalar)
            {
                dataList.TryGetEntry(part.Option.Field, out tmpEntry, out error);                
            }
            else
            {
                dataList.TryGetEntry(part.Option.Recordset, out tmpEntry, out error);
            }

            return new DebugOutputTO(tmpEntry.Clone(enTranslationDepth.Data, Guid.NewGuid(), out error), evaluatedValue);            
        }

        #endregion

    }
}
