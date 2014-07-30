using Dev2.Common;
using Dev2.Common.Enums;
using Dev2.Data.Audit;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Builders;
using Dev2.Data.Compilers;
using Dev2.Data.Enums;
using Dev2.Data.Factories;
using Dev2.Data.Interfaces;
using Dev2.Data.Parsers;
using Dev2.Data.Storage;
using Dev2.Data.SystemTemplates;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.TO;
using Dev2.DataList.Contract.Translators;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.MathOperations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

// ReSharper disable CheckNamespace
namespace Dev2.Server.Datalist
// ReSharper restore CheckNamespace
{

    /// <summary>
    /// Server DataList compiler
    /// </summary>
    internal class ServerDataListCompiler : IEnvironmentModelDataListCompiler
    {
        // ReSharper disable InconsistentNaming
        private readonly IDev2DataLanguageParser _parser = DataListFactory.CreateLanguageParser();
        // DataList Server
        private readonly IDataListServer _dlServer;

        internal ServerDataListCompiler(IDataListServer dlServ)
        {
            _dlServer = dlServ;
        }

        #region Private Method

        private static string CalcPrepValue(string eVal)
        {
            double tVal;

            if(!double.TryParse(eVal, out tVal))
            {
                return "\"" + eVal + "\"";
            }

            return eVal;
        }

        #endregion

        /// <summary>
        /// PLEASE NOTE THE META-DATA RETURNED FROM THIS METHOD IS TRANSIENT, THIS MEANS IT WILL ONLY BE EVENTUALLY CONSISTENT
        /// DO NOT RELY UPON IT FOR UI DRIVEN DETAILS, PLEASE WRITE YOUR OWN HELPER METHODS TO ACHIEVE YOUR GOAL
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="curDLID">The current dlid.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="toRoot">if set to <c>true</c> [automatic root].</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Cannot evaluate recordset index region [  + recsetIndexStr +  ]
        /// or
        /// Cannot evaluate recordset index region [  + recsetIndexStr +  ]
        /// </exception>
        public IBinaryDataListEntry Evaluate(NetworkContext ctx, Guid curDLID, enActionType typeOf, string expression, out ErrorResultTO errors, bool toRoot = false)
        {

            ErrorResultTO allErrors = new ErrorResultTO();
            string error;

            IBinaryDataList theDl = TryFetchDataList(curDLID, out error);
            IBinaryDataListEntry result = null;
            if(theDl != null)
            {
                if(typeOf == enActionType.User)
                {
                    result = InternalEvaluate(expression, theDl, out errors, toRoot);
                    allErrors.MergeErrors(errors);
                }
                else if(typeOf == enActionType.System)
                {
                    // prefix eval with Dev2System
                    string realExpression = BuildSystemTag(expression);
                    if(!theDl.TryGetEntry(realExpression, out result, out error))
                    {
                        allErrors.AddError(error);
                    }
                }
                else if(typeOf == enActionType.Internal)
                {
                    // NOTE : Delete operation is the only internal op                     
                    IBinaryDataListEntry tmpEntry;
                    string recsetName = DataListUtil.ExtractRecordsetNameFromValue(expression);
                    bool res = false;
                    if(theDl.TryGetEntry(recsetName, out tmpEntry, out error))
                    {
                        string recsetIndexStr = DataListUtil.ExtractIndexRegionFromRecordset(expression);

                        // evaluate it if need be ;)
                        if(DataListUtil.IsEvaluated(recsetIndexStr))
                        {
                            var evaluatedValue = Evaluate(ctx, curDLID, enActionType.User, recsetIndexStr, out errors);
                            allErrors.MergeErrors(errors);

                            if(evaluatedValue == null)
                            {
                                throw new Exception("Cannot evaluate recordset index region [ " + recsetIndexStr + " ]");
                            }

                            var scalar = evaluatedValue.FetchScalar();

                            if(scalar != null)
                            {
                                recsetIndexStr = scalar.TheValue;
                            }
                            else
                            {
                                throw new Exception("Cannot evaluate recordset index region [ " + recsetIndexStr + " ]");
                            }
                        }

                        res = tmpEntry.TryDeleteRows(recsetIndexStr, out error);
                    }
                    allErrors.AddError(error);
                    TryPushDataList(theDl, out error);
                    allErrors.AddError(error);

                    IBinaryDataListEntry newDlEntry = Dev2BinaryDataListFactory.CreateEntry(GlobalConstants.EvalautionScalar, string.Empty, theDl.UID);

                    allErrors.AddError(error);
                    if(res)
                    {
                        newDlEntry.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem("Success", GlobalConstants.EvalautionScalar), out error);
                        allErrors.AddError(error);

                    }
                    else
                    {
                        newDlEntry.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem("Failure", GlobalConstants.EvalautionScalar), out error);
                        allErrors.AddError(error);
                    }
                    result = newDlEntry;
                }
                else if(typeOf == enActionType.CalculateSubstitution)
                {
                    // Travis.Frisinger : 31.01.2013 - Added to properly levage the internal language   

                    // Break the expression up by , and sub in values?
                    IDev2DataLanguageParser parser = new Dev2DataLanguageParser();
                    IList<IIntellisenseResult> myParts = parser.ParseExpressionIntoParts(expression, theDl.FetchIntellisenseParts());
                    while(true)
                    {
                        expression = GetExpression(expression, myParts, theDl, out errors);
                        allErrors.MergeErrors(errors);
                        myParts = parser.ParseExpressionIntoParts(expression, theDl.FetchIntellisenseParts());
                        var rangeType = false;
                        foreach(var p in myParts)
                        {
                            // Ensure the expression exist and it is not a range operation
                            if(!(p.Type == enIntellisenseResultType.Selectable
                                     && expression.IndexOf((p.Option.DisplayValue + ":"), StringComparison.Ordinal) < 0
                                     && expression.IndexOf((":" + p.Option.DisplayValue), StringComparison.Ordinal) < 0))
                            {
                                rangeType = true;
                            }
                        }
                        if(myParts.Count == 0 || allErrors.HasErrors() || rangeType)
                        {
                            break;
                        }
                    }


                    IBinaryDataListEntry calcResult = Dev2BinaryDataListFactory.CreateEntry(GlobalConstants.EvalautionScalar, string.Empty, theDl.UID);
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

        string GetExpression(string expression, IEnumerable<IIntellisenseResult> myParts, IBinaryDataList theDl, out ErrorResultTO errors)
        {
            // Fetch each DL expression in the master expression and evalaute
            // Then build up the correct string to sub in ;)
            errors = new ErrorResultTO();
            foreach(IIntellisenseResult p in myParts)
            {
                // Ensure the expression exist and it is not a range operation
                if(p.Type == enIntellisenseResultType.Selectable
                   && expression.IndexOf(p.Option.DisplayValue, StringComparison.Ordinal) >= 0
                   && expression.IndexOf((p.Option.DisplayValue + ":"), StringComparison.Ordinal) < 0
                   && expression.IndexOf((":" + p.Option.DisplayValue), StringComparison.Ordinal) < 0)
                {
                    IBinaryDataListEntry bde = InternalEvaluate(p.Option.DisplayValue, theDl, out errors);
                    if(bde != null)
                    {
                        if(bde.IsRecordset)
                        {
                            // recordset op - build up the correct string to inject
                            IIndexIterator idxItr = bde.FetchRecordsetIndexes();
                            StringBuilder sb = new StringBuilder();

                            while(idxItr.HasMore())
                            {
                                var fetchIdx = idxItr.FetchNextIndex();
                                string error;
                                IList<IBinaryDataListItem> items = bde.FetchRecordAt(fetchIdx, out error);
                                errors.AddError(error);
                                foreach(IBinaryDataListItem itm in items)
                                {
                                    if(itm.TheValue != string.Empty)
                                    {
                                        // if numeric leave it, else append ""
                                        string eVal = itm.TheValue;
                                        eVal = CalcPrepValue(eVal);
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
                            if(DataListUtil.IsEvaluated(expression))
                            {
                                var dataListValue = toInject.TrimStart('\"').TrimEnd('\"');
                                expression = expression.Replace(toInject, dataListValue);
                            }
                        }
                        else
                        {
                            // scalar op
                            string eVal = bde.FetchScalar().TheValue;
                            eVal = CalcPrepValue(eVal);
                            expression = expression.Replace(p.Option.DisplayValue, eVal);
                            if(DataListUtil.IsEvaluated(expression))
                            {
                                var dataListValue = eVal.TrimStart('\"').TrimEnd('\"');
                                expression = expression.Replace(eVal, dataListValue);
                            }
                        }
                    }
                    errors.MergeErrors(errors);
                }
                else if(p.Type == enIntellisenseResultType.Error)
                {
                    errors.AddError(p.Message);
                }
            }
            return expression;
        }

        public Guid CloneDataList(Guid curDLID, out ErrorResultTO errors)
        {
            var allErrors = new ErrorResultTO();
            string error;

            Guid res = GlobalConstants.NullDataListID;

            IBinaryDataList tmpDl = TryFetchDataList(curDLID, out error);
            if(error != string.Empty)
            {
                allErrors.AddError(error);
            }
            else
            {
                // Ensure we have a non-null tmpDL

                IBinaryDataList result = tmpDl.Clone(enTranslationDepth.Data, out errors, false);
                if(result != null)
                {
                    allErrors.MergeErrors(errors);
                    TryPushDataList(result, out error);
                    allErrors.AddError(error);

                    res = result.UID;
                }

            }

            errors = allErrors;

            return res;
        }

        public IBinaryDataList FetchBinaryDataList(NetworkContext ctx, Guid curDLID, out ErrorResultTO errors)
        {

            string error;

            IBinaryDataList result = TryFetchDataList(curDLID, out error);
            errors = new ErrorResultTO();

            if(result == null)
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

            Guid result = Upsert(ctx, curDLID, payload, out errors);

            allErrors.MergeErrors(errors);
            errors = allErrors;

            return result;
        }

        public Guid Upsert(NetworkContext ctx, Guid curDLID, IList<string> expression, IList<string> values, out ErrorResultTO errors)
        {
            IDev2DataListUpsertPayloadBuilder<string> payload = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();
            ErrorResultTO allErrors = new ErrorResultTO();

            if(expression.Count == values.Count)
            {
                int pos = 0;
                foreach(string exp in expression)
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
            var allErrors = new ErrorResultTO();

            if(expressions.Count == values.Count)
            {
                int pos = 0;
                foreach(string exp in expressions)
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

        public Guid Upsert(NetworkContext ctx, Guid curDLID, IDev2DataListUpsertPayloadBuilder<List<string>> payload, out ErrorResultTO errors)
        {
            return Upsert<List<string>>(ctx, curDLID, payload, out errors);
        }

        public Guid Upsert(NetworkContext ctx, Guid curDLID, IDev2DataListUpsertPayloadBuilder<IBinaryDataListEntry> payload, out ErrorResultTO errors)
        {
            return Upsert<IBinaryDataListEntry>(ctx, curDLID, payload, out errors);
        }

        public Guid Shape(NetworkContext ctx, Guid curDLID, enDev2ArgumentType typeOf, IList<IDev2Definition> definitions, out ErrorResultTO errors)
        {

            // Use eval and upsert to processes definitions
            Guid result = Guid.Empty;
            errors = new ErrorResultTO();

            if(definitions != null)
            {
                if(typeOf == enDev2ArgumentType.Input || typeOf == enDev2ArgumentType.Output)
                {
                    result = InternalShape(ctx, curDLID, definitions, typeOf, out errors);
                }
            }

            return result;
        }

        public Guid Shape(NetworkContext ctx, Guid curDLID, enDev2ArgumentType typeOf, string definitions, out ErrorResultTO errors, Guid overrideID = default(Guid))
        {
            // Use eval and upsert to processes definitions
            Guid result = Guid.Empty;
            errors = new ErrorResultTO();

            ServerLogger.LogMessage("PRE-" + typeOf.ToString().ToUpper() + " SHAPE MEMORY USAGE [ " + BinaryDataListStorageLayer.GetUsedMemoryInMb() + " MBs ]");

            switch(typeOf)
            {
                case enDev2ArgumentType.Input:
                    result = PerformInputShaping(ctx, curDLID, typeOf, definitions, ref errors, overrideID);
                    break;
                case enDev2ArgumentType.Output_Append_Style:
                case enDev2ArgumentType.Output:
                case enDev2ArgumentType.DB_ForEach:
                    result = PerformOutputShaping(ctx, curDLID, typeOf, definitions, ref errors, result);
                    break;
            }

            ServerLogger.LogMessage("POST-" + typeOf.ToString().ToUpper() + " SHAPE MEMORY USAGE [ " + BinaryDataListStorageLayer.GetUsedMemoryInMb() + " MBs ]");

            return result;

        }

        /// <summary>
        /// Shapes for sub execution.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="parentDLID">The parent dlid.</param>
        /// <param name="childDLID">The child dlid.</param>
        /// <param name="inputDefs">The input defs.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public IList<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>> ShapeForSubExecution(NetworkContext ctx, Guid parentDLID, Guid childDLID, string inputDefs, string outputDefs, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();

            IDev2LanguageParser oParser = DataListFactory.CreateOutputParser();
            IDev2LanguageParser iParser = DataListFactory.CreateInputParser();
            IList<IDev2Definition> oDefs = oParser.Parse(outputDefs);
            IList<IDev2Definition> iDefs = iParser.Parse(inputDefs);

            // temporal data structures to help manage the core operations ;)
            HashSet<int> inputRemoveIdx = new HashSet<int>();
            HashSet<int> outputRemoveIdx = new HashSet<int>();

            ErrorResultTO invokeErrors;
            var childDl = FetchBinaryDataList(ctx, childDLID, out invokeErrors);
            var parentDl = FetchBinaryDataList(ctx, parentDLID, out invokeErrors);

            errors.MergeErrors(invokeErrors);

            // *** Process flow through mappings ;)

            // TODO : In here we need to create a mapping for the second recordset!


            // Foreach iDef break RawValue into RS and field
            // Then find a matching RS in oDefs
            int iPos = 0;
            int oPos;
            foreach(var def in iDefs)
            {
                if(def.IsRecordSet)
                {
                    var rsName = DataListUtil.ExtractRecordsetNameFromValue(def.RawValue);
                    var rsCol = DataListUtil.ExtractFieldNameFromValue(def.RawValue);

                    oPos = 0;
                    foreach(var def2 in oDefs)
                    {
                        if(def2.RecordSetName == rsName)
                        {
                            // If same cool beans ;)
                            if(def2.Name == rsCol)
                            {
                                IBinaryDataListEntry entry;
                                string error;
                                childDl.TryGetEntry(def.RecordSetName, out entry, out error);
                                errors.AddError(error);

                                // fetch parent entry now ;)
                                IBinaryDataListEntry parentEntry;
                                parentDl.TryGetEntry(rsName, out parentEntry, out error);
                                errors.AddError(error);

                                // adjust the column view so it propagates down to row ;)
                                entry.AdjustForIOMapping(parentDLID, rsCol, rsName, def.Name, out invokeErrors);
                                errors.MergeErrors(invokeErrors);
                                outputRemoveIdx.Add(oPos);
                                inputRemoveIdx.Add(iPos);
                                break;

                            }

                            var rsName2 = DataListUtil.ExtractRecordsetNameFromValue(def2.RawValue);
                            var rsCol2 = DataListUtil.ExtractFieldNameFromValue(def2.RawValue);

                            if(def.Name == rsCol2 && def.RecordSetName == rsName2)
                            {
                                IBinaryDataListEntry entry;
                                string error;
                                childDl.TryGetEntry(def.RecordSetName, out entry, out error);
                                errors.AddError(error);

                                entry.AdjustForIOMapping(parentDLID, rsCol, rsName, def.Name, out invokeErrors);
                                errors.MergeErrors(invokeErrors);
                                outputRemoveIdx.Add(oPos);
                                inputRemoveIdx.Add(iPos);
                                break;
                            }
                        }

                        oPos++;
                    }

                    //Now process outputRemoveIdx values ;)
                    PruneMappings(outputRemoveIdx, ref oDefs);
                    outputRemoveIdx.Clear();
                    iPos++;
                }
            }

            // prune input mappings ;)
            PruneMappings(inputRemoveIdx, ref iDefs);

            oPos = 0;
            // *** Process new Output mappings ;)
            foreach(var def in oDefs)
            {
                if(def.IsRecordSet)
                {
                    var rsName = DataListUtil.ExtractRecordsetNameFromValue(def.RawValue);
                    var rsCol = DataListUtil.ExtractFieldNameFromValue(def.RawValue);

                    // NOTE : This is a problem when the RecordsetName differs from the rsName?!

                    if(childDl != null)
                    {
                        string error;
                        IBinaryDataListEntry entry;

                        if(childDl.TryGetEntry(rsName, out entry, out error))
                        {
                            // way back some joker though it was a good idea to break the mapping syntax, this corrects for it ;)
                            // we we loose the aliasing for this level... oh well, best wait into all this bloody mutable xml mapping junk is gone before we properly address
                            errors.AddError(error);
                        }

                        if(entry != null && !string.IsNullOrEmpty(rsName) && !string.IsNullOrEmpty(rsCol))
                        {
                            entry.AdjustForIOMapping(parentDLID, rsCol, rsName, def.Name, out invokeErrors);
                            errors.MergeErrors(invokeErrors);
                            outputRemoveIdx.Add(oPos);
                        }
                    }
                }

                oPos++;
            }

            // prune output mappings ;)
            PruneMappings(outputRemoveIdx, ref oDefs);

            IList<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>> result = new List<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>>();

            if(iDefs.Count > 0)
            {
                // Do any remaining input shaping here ;)
                // Will return the childDLID
                InternalShape(ctx, parentDLID, iDefs, enDev2ArgumentType.Input, out invokeErrors, false, childDLID);
                errors.MergeErrors(invokeErrors);
            }

            if(oDefs.Count > 0)
            {
                // Return output mappings for later use ;)
                result.Add(new KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>(enDev2ArgumentType.Output, oDefs));
            }

            return result;

        }

        /// <summary>
        /// Prunes the mappings.
        /// </summary>
        /// <param name="removeIdx">Index of the remove.</param>
        /// <param name="defs">The defs.</param>
        void PruneMappings(IEnumerable<int> removeIdx, ref IList<IDev2Definition> defs)
        {
            // Now remove input mappings that have been processed ;)
            int removeOffSet = 0;
            foreach(var idx in removeIdx)
            {
                var toRemove = (idx - removeOffSet);
                defs.RemoveAt(toRemove);
                removeOffSet++;
            }
        }

        Guid PerformOutputShaping(NetworkContext ctx, Guid curDLID, enDev2ArgumentType typeOf, string definitions, ref ErrorResultTO errors, Guid result)
        {
            IDev2LanguageParser parser = DataListFactory.CreateOutputParser();
            IList<IDev2Definition> defs = parser.Parse(definitions);
            result = defs.Count > 0 ? InternalShape(ctx, curDLID, defs, typeOf, out errors) : UnionDataList(curDLID, ref errors, result);
            return result;
        }

        /// <summary>
        /// Performs the input shaping.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="curDLID">The current dlid.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="definitions">The definitions.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="result">The result.</param>
        /// <param name="isTransactionallyScoped">if set to <c>true</c> [is transactionally scoped].</param>
        /// <returns></returns>
        Guid PerformInputShaping(NetworkContext ctx, Guid curDLID, enDev2ArgumentType typeOf, string definitions, ref ErrorResultTO errors, Guid result, bool isTransactionallyScoped = false)
        {
            IDev2LanguageParser parser = DataListFactory.CreateInputParser();
            IList<IDev2Definition> defs = parser.Parse(definitions);
            result = defs.Count > 0 ? InternalShape(ctx, curDLID, defs, typeOf, out errors, isTransactionallyScoped, result) : CloneDataList(curDLID, ref errors, result, true);
            return result;
        }

        /// <summary>
        /// Clones the data list.
        /// </summary>
        /// <param name="curDLID">The current dlid.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="result">The result.</param>
        /// <param name="onlySystemTags">if set to <c>true</c> [only system tags].</param>
        /// <returns></returns>
        Guid CloneDataList(Guid curDLID, ref ErrorResultTO errors, Guid result, bool onlySystemTags = false)
        {
            string error;
            IBinaryDataList tmp = TryFetchDataList(curDLID, out error);
            if(error != string.Empty)
            {
                errors.AddError(error);
            }
            else
            {
                IBinaryDataList toPush = tmp.Clone(enTranslationDepth.Data, out errors, onlySystemTags);
                toPush.ParentUID = curDLID;
                TryPushDataList(toPush, out error);
                if(error != string.Empty)
                {
                    errors.AddError(error);
                }
                result = toPush.UID;
            }
            return result;
        }

        Guid UnionDataList(Guid curDLID, ref ErrorResultTO errors, Guid result)
        {
            string error;
            IBinaryDataList tmp = TryFetchDataList(curDLID, out error);
            if(error != string.Empty)
            {
                errors.AddError(error);
            }
            else
            {
                Guid pID = tmp.ParentUID;
                IBinaryDataList parentDl = TryFetchDataList(pID, out error);
                if(error != string.Empty)
                {
                    errors.AddError(error);
                }
                else
                {
                    tmp = parentDl.Merge(tmp, enDataListMergeTypes.Union, enTranslationDepth.Data_With_Blank_OverWrite, false, out errors);
                    TryPushDataList(tmp, out error);
                    if(error != string.Empty)
                    {
                        errors.AddError(error);
                    }
                    result = tmp.UID;
                }
            }
            return result;
        }

        public Guid Merge(NetworkContext ctx, Guid leftID, Guid rightID, enDataListMergeTypes mergeType, enTranslationDepth depth, bool createNewList, out ErrorResultTO errors)
        {

            string error;
            ErrorResultTO allErrors = new ErrorResultTO();
            Guid returnVal = Guid.Empty;

            IBinaryDataList left = TryFetchDataList(leftID, out error);
            if(left == null)
            {
                allErrors.AddError(error);
            }

            IBinaryDataList right = TryFetchDataList(rightID, out error);
            if(right == null)
            {
                allErrors.AddError(error);
            }

            // alright to merge
            if(right != null && left != null)
            {
                IBinaryDataList result = left.Merge(right, mergeType, depth, createNewList, out errors);
                if(errors.HasErrors())
                {
                    allErrors.MergeErrors(errors);
                }
                else
                {
                    // Push back into the server now ;)
                    if(!TryPushDataList(result, out error))
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

        public Guid ConditionalMerge(NetworkContext ctx, DataListMergeFrequency conditions,
            Guid destinationDatalistID, Guid sourceDatalistID, DataListMergeFrequency datalistMergeFrequency,
            enDataListMergeTypes datalistMergeType, enTranslationDepth datalistMergeDepth)
        {
            Guid mergeId = Guid.Empty;
            if(conditions.HasFlag(datalistMergeFrequency) && destinationDatalistID != Guid.Empty && sourceDatalistID != Guid.Empty)
            {
                ErrorResultTO errors;
                mergeId = Merge(ctx, destinationDatalistID, sourceDatalistID, datalistMergeType, datalistMergeDepth, false, out errors);

                if(errors != null && errors.HasErrors())
                {
                    ErrorResultTO tmpErrors;
                    mergeId = UpsertSystemTag(destinationDatalistID, enSystemTag.Dev2Error, errors.MakeDataListReady(), out tmpErrors);
                }
            }

            return mergeId;
        }

        public Guid TransferSystemTags(NetworkContext ctx, Guid parentDLID, Guid childDLID, bool parentToChild, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        public IList<KeyValuePair<string, IBinaryDataListEntry>> FetchChanges(NetworkContext ctx, Guid id, StateType direction)
        {
            return null;
        }

        public bool DeleteDataListByID(Guid curDLID, bool onlyIfNotPersisted)
        {
            bool result = _dlServer.DeleteDataList(curDLID, onlyIfNotPersisted);

            return result;
        }

        /// <summary>
        /// Populates the data list.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="input">The input.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="targetDLID">The target dlid.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid PopulateDataList(NetworkContext ctx, DataListFormat typeOf, object input, string outputDefs, Guid targetDLID, out ErrorResultTO errors)
        {

            Guid returnVal = Guid.Empty;
            ErrorResultTO allErrors = new ErrorResultTO();
            try
            {
                IDataListTranslator t = _dlServer.GetTranslator(typeOf);
                if(t != null)
                {
                    returnVal = t.Populate(input, targetDLID, outputDefs, out errors);
                    allErrors.MergeErrors(errors);
                }
                else
                {
                    allErrors.AddError("Invalid DataListFormt [ " + typeOf + " ] ");
                }
            }
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
            }

            // assign error var
            errors = allErrors;

            return returnVal;
        }

        public Guid ConvertAndOnlyMapInputs(NetworkContext ctx, DataListFormat typeOf, byte[] payload, string shape, out ErrorResultTO errors)
        {
            // _repo
            Guid returnVal = Guid.Empty;
            ErrorResultTO allErrors = new ErrorResultTO();
            try
            {
                IDataListTranslator t = _dlServer.GetTranslator(typeOf);
                if(t != null)
                {
                    IBinaryDataList result = t.ConvertAndOnlyMapInputs(payload, shape, out errors);
                    if(errors.HasErrors())
                    {
                        allErrors.MergeErrors(errors);
                    }

                    if(result != null)
                    {
                        // set the uid and place in cache
                        returnVal = result.UID;

                        string error;
                        if(!TryPushDataList(result, out error))
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
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
            }

            // assign error var
            errors = allErrors;

            return returnVal;
        }

        public Guid ConvertTo(NetworkContext ctx, DataListFormat typeOf, object payload, string shape, out ErrorResultTO errors)
        {
            // _repo
            Guid returnVal = Guid.Empty;
            ErrorResultTO allErrors = new ErrorResultTO();
            try
            {
                IDataListTranslator t = _dlServer.GetTranslator(typeOf);
                if(t != null)
                {
                    IBinaryDataList result = t.ConvertTo(payload, shape, out errors);
                    if(errors.HasErrors())
                    {
                        allErrors.MergeErrors(errors);
                    }

                    if(result != null)
                    {
                        // set the uid and place in cache
                        returnVal = result.UID;

                        string error;
                        if(!TryPushDataList(result, out error))
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
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
            }

            // assign error var
            errors = allErrors;

            return returnVal;
        }

        public Guid ConvertTo(NetworkContext ctx, DataListFormat typeOf, byte[] payload, string shape, out ErrorResultTO errors)
        {

            // _repo
            Guid returnVal = Guid.Empty;
            ErrorResultTO allErrors = new ErrorResultTO();
            try
            {
                IDataListTranslator t = _dlServer.GetTranslator(typeOf);
                if(t != null)
                {
                    IBinaryDataList result = t.ConvertTo(payload, shape, out errors);
                    if(errors.HasErrors())
                    {
                        allErrors.MergeErrors(errors);
                    }

                    if(result != null)
                    {
                        // set the uid and place in cache
                        returnVal = result.UID;

                        string error;
                        if(!TryPushDataList(result, out error))
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
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
            }

            // assign error var
            errors = allErrors;

            return returnVal;
        }

        public DataListTranslatedPayloadTO ConvertFrom(NetworkContext ctx, Guid curDLID, enTranslationDepth depth, DataListFormat typeOf, out ErrorResultTO errors)
        {
            DataListTranslatedPayloadTO returnVal = null;
            ErrorResultTO allErrors = new ErrorResultTO();
            string error;

            IBinaryDataList result = TryFetchDataList(curDLID, out error);

            if(result != null)
            {
                try
                {

                    IDataListTranslator t = _dlServer.GetTranslator(typeOf);
                    if(t != null)
                    {
                        returnVal = t.ConvertFrom(result, out errors);
                        if(errors.HasErrors())
                        {
                            allErrors.MergeErrors(errors);
                        }
                    }
                    else
                    {
                        allErrors.AddError("Invalid DataListFormt [ " + typeOf + " ] ");
                    }
                }
                catch(Exception e)
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
            string res = string.Empty;
            ErrorResultTO allErrors = new ErrorResultTO();
            string error;

            IBinaryDataList result = TryFetchDataList(curDLID, out error);

            if(result != null)
            {
                try
                {

                    IDataListTranslator t = _dlServer.GetTranslator(typeOf);
                    if(t != null)
                    {
                        res = t.ConvertAndFilter(result, filterShape, out errors);
                        allErrors.MergeErrors(errors);
                    }
                    else
                    {
                        allErrors.AddError("Invalid DataListFormt [ " + typeOf + " ] ");
                    }
                }
                catch(Exception e)
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
            string error;
            errors = new ErrorResultTO();
            IBinaryDataList bdl = TryFetchDataList(curDLID, out error);
            if(error != string.Empty)
            {
                errors.AddError(error);
            }
            if(bdl != null)
            {
                bdl.ParentUID = parentID;
                TryPushDataList(bdl, out error);
                if(error != string.Empty)
                {
                    errors.AddError(error);
                }
            }
        }

        public IList<DataListFormat> FetchTranslatorTypes()
        {
            return _dlServer.FetchTranslatorTypes();
        }

        public Guid UpsertSystemTag(Guid curDLID, enSystemTag tag, string val, out ErrorResultTO errors)
        {
            return UpsertSystemTag<string>(curDLID, tag, val, out errors);
        }

        public Guid UpsertSystemTag(Guid curDLID, enSystemTag tag, IBinaryDataListEntry val, out ErrorResultTO errors)
        {
            return UpsertSystemTag<IBinaryDataListEntry>(curDLID, tag, val, out errors);
        }

        public DataTable ConvertToDataTable(IBinaryDataList input, string recsetName, out ErrorResultTO errors, PopulateOptions populateOptions)
        {
            errors = new ErrorResultTO();
            try
            {
                IDataListTranslator t = _dlServer.GetTranslator(DataListFormat.CreateFormat(GlobalConstants._DATATABLE));
                return t.ConvertToDataTable(input, recsetName, out errors, populateOptions);
            }
            catch(Exception e)
            {
                errors.AddError(e.Message);
            }
            return null;
        }

        void ProcessRecordsetGroup(RecordsetGroup rsGroup, IBinaryDataList targeDataList, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();

            if(rsGroup == null)
            {
                return;
            }

            // properly extract endpoint key ;)
            Func<IDev2Definition, string> keySelector = def =>
            {
                var val = (DataListUtil.ExtractRecordsetNameFromValue(rsGroup.OutputExpressionExtractor(def)));

                if(string.IsNullOrEmpty(val))
                {
                    val = DataListUtil.RemoveLanguageBrackets(def.RawValue);
                }

                return val;
            };


            var targetRecordSetNames = rsGroup.Definitions.ToLookup(
                keySelector,
                d =>
                {
                    var inputExpr = rsGroup.InputExpressionExtractor(d);
                    var outputExpr = rsGroup.OutputExpressionExtractor(d);
                    bool isRs = true;
                    var field = DataListUtil.ExtractFieldNameFromValue(outputExpr);

                    var val = field;

                    // we need to check the recordset index ;)
                    var idx = DataListUtil.GetRecordsetIndexType(outputExpr);

                    //must be a scalar target ;)
                    if(string.IsNullOrEmpty(field))
                    {
                        val = DataListUtil.RemoveLanguageBrackets(outputExpr);
                        isRs = false;
                    }

                    return new KeyValuePair<string, TransientRecordsetProcessGroup>(inputExpr, new TransientRecordsetProcessGroup(val, idx, isRs));
                });

            foreach(var targetRecordSetName in targetRecordSetNames)
            {
                var outputRecordSetname = targetRecordSetName.Key;

                IBinaryDataListEntry targetEntry;
                string error;
                targeDataList.TryGetEntry(outputRecordSetname, out targetEntry, out error);

                var sourceIndices = rsGroup.SourceEntry.FetchRecordsetIndexes();
                while(sourceIndices.HasMore())
                {
                    var sourceIndex = sourceIndices.FetchNextIndex();
                    var targetIndex = sourceIndex;
                    var sourceItems = rsGroup.SourceEntry.FetchRowAt(sourceIndex, out error);
                    var targetItems = new List<IBinaryDataListItem>();

                    bool isRs = true;
                    enRecordsetIndexType idxType = enRecordsetIndexType.Error;
                    ErrorResultTO tmpErrors;

                    foreach(var targetDef in targetRecordSetName)
                    {
                        var inputFieldName = DataListUtil.ExtractFieldNameFromValue(targetDef.Key);
                        var outputFieldName = targetDef.Value.TargetValue;

                        isRs = targetDef.Value.IsTargetRecordSet;

                        // -- check for notation changes ;)

                        // did indexing types change?
                        if(targetDef.Value.IdxType != idxType && idxType != enRecordsetIndexType.Error)
                        {

                            FlushToEntry(isRs, targetEntry, targetItems, targetIndex, out tmpErrors);
                            errors.MergeErrors(tmpErrors);

                            // now clear things up for the next round ;)
                            targetItems.Clear();
                        }

                        // adjust target index if need be ;)
                        if(targetDef.Value.IdxType == enRecordsetIndexType.Blank)
                        {
                            targetIndex = targetEntry.FetchAppendRecordsetIndex(); // adjust to the append index ;)
                        }

                        IBinaryDataListItem item = sourceItems.FirstOrDefault(t => t.FieldName.ToLower() == inputFieldName.ToLower());

                        if(item != null)
                        {
                            IBinaryDataListItem binaryDataListItem = item.Clone();
                            binaryDataListItem.UpdateField(outputFieldName);
                            targetItems.Add(binaryDataListItem);
                        }

                        idxType = targetDef.Value.IdxType;
                    }//st

                    // flush the last bits or entire operation ;)
                    FlushToEntry(isRs, targetEntry, targetItems, targetIndex, out tmpErrors);
                    errors.MergeErrors(tmpErrors);
                }
            }
        }

        #region Private Methods


        /// <summary>
        /// Flushes the automatic entry.
        /// </summary>
        /// <param name="isRs">if set to <c>true</c> [is rs].</param>
        /// <param name="targetEntry">The target entry.</param>
        /// <param name="targetItems">The target items.</param>
        /// <param name="targetIndex">Index of the target.</param>
        /// <param name="errors">The errors.</param>
        private void FlushToEntry(bool isRs, IBinaryDataListEntry targetEntry, IList<IBinaryDataListItem> targetItems, int targetIndex, out ErrorResultTO errors)
        {
            string error;
            errors = new ErrorResultTO();

            if(isRs)
            {
                targetEntry.TryPutRecordRowAt(targetItems, targetIndex, out error);
                errors.AddError(error);
            }
            else
            {
                targetEntry.TryPutScalar(targetItems[0], out error);
                errors.AddError(error);
            }
        }

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
            string error;
            IBinaryDataList bdl = TryFetchDataList(curDLID, out error);

            Guid result = GlobalConstants.NullDataListID;

            if(error != string.Empty)
            {
                allErrors.AddError(error);
            }

            if(bdl != null)
            {
                string tt = DataListUtil.BuildSystemTagForDataList(tag, false);
                allErrors.MergeErrors(errors);
                IBinaryDataListItem itm = null;

                if(typeof(T) == typeof(string))
                {
                    itm = Dev2BinaryDataListFactory.CreateBinaryItem(val.ToString(), tt);

                }
                else if(typeof(T) == typeof(IBinaryDataListEntry))
                {
                    itm = ((IBinaryDataListEntry)val).FetchScalar();
                }

                result = GlobalConstants.NullDataListID;


                if(bdl.TryCreateScalarTemplate(string.Empty, tt, string.Empty, true, out error))
                {
                    IBinaryDataListEntry et;
                    bdl.TryGetEntry(tt, out et, out error);

                    if(et != null)
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
        /// Internals the shape input.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="definitions">The definitions.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="isTransactionallyScoped">if set to <c>true</c> [is transactionally scoped].</param>
        /// <param name="overRideShapeID">The over ride shape unique identifier.</param>
        /// <returns></returns>
        public Guid InternalShape(NetworkContext ctx, Guid curDLID, IList<IDev2Definition> definitions, enDev2ArgumentType typeOf, out ErrorResultTO errors, bool isTransactionallyScoped = false, Guid overRideShapeID = default(Guid))
        {
            Guid result = Guid.Empty;
            errors = new ErrorResultTO();
            var allErrors = new ErrorResultTO();
            string theShape = DataListUtil.GenerateDataListFromDefs(definitions);
            byte[] empty = new byte[0];

            // always an internal XML format ;)
            Guid shellId;
            if(typeOf == enDev2ArgumentType.Input)
            {
                shellId = overRideShapeID != Guid.Empty ? overRideShapeID : ConvertTo(ctx, DataListFormat.CreateFormat(GlobalConstants._XML), empty, theShape, out errors);
            }
            else
            {
                // output shape, we already have the ID, fetch parentID for merge up ;)
                shellId = curDLID;
            }

            if(errors.HasErrors())
            {
                allErrors.MergeErrors(errors);
            }
            else
            {
                // now we have a IBinaryDataList, we need to set the ParentID and populate with data
                IBinaryDataList childDl = FetchBinaryDataList(ctx, shellId, out errors);
                if(errors.HasErrors())
                {
                    allErrors.MergeErrors(errors);
                }
                else
                {
                    // set parentID on input shape
                    if(typeOf == enDev2ArgumentType.Input)
                    {
                        childDl.ParentUID = curDLID;
                        string error;
                        TryPushDataList(childDl, out error);
                        allErrors.AddError(error);
                    }

                    // now set the Func to execute depending upon direction ;)
                    Guid extractFromId = curDLID;
                    Guid pushToId = childDl.UID;

                    if(typeOf != enDev2ArgumentType.Input)
                    {
                        // swap extract from and pushTo around for output shaping
                        extractFromId = childDl.UID;
                        pushToId = childDl.ParentUID;
                    }

                    var inputExpressionExtractor = BuildInputExpressionExtractor(typeOf);
                    var outputExpressionExtractor = BuildOutputExpressionExtractor(typeOf);

                    ProcessRecordSets(ctx, definitions, inputExpressionExtractor, outputExpressionExtractor, typeOf, extractFromId, pushToId, errors);
                    allErrors.MergeErrors(errors);

                    ProcessScalars(ctx, definitions, inputExpressionExtractor, outputExpressionExtractor, typeOf, extractFromId, pushToId, errors);
                    allErrors.MergeErrors(errors);

                    result = pushToId;

                    // Merge System tags too ;)
                    // exctractFromID, pushToID
                    foreach(enSystemTag t in TranslationConstants.systemTags)
                    {
                        IBinaryDataListEntry sysVal = Evaluate(ctx, extractFromId, enActionType.System, t.ToString(), out errors);
                        allErrors.MergeErrors(errors);
                        // TRAVIS

                        if(sysVal == null)
                        {
                            string errorTmp;
                            sysVal = DataListConstants.baseEntry.Clone(enTranslationDepth.Shape, pushToId, out errorTmp);
                        }

                        UpsertSystemTag(pushToId, t, sysVal, out errors);
                        allErrors.MergeErrors(errors);

                    }
                }
            }

            // assign errors so they are returned ;)
            errors = allErrors;

            return result;
        }

        /// <summary>
        /// Processes the scalars.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="definitions">The definitions.</param>
        /// <param name="inputExpressionExtractor">The input expression extractor.</param>
        /// <param name="outputExpressionExtractor">The output expression extractor.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="extractFromId">The extract from unique identifier.</param>
        /// <param name="pushToId">The push automatic unique identifier.</param>
        /// <param name="allErrors">All errors.</param>
        void ProcessScalars(NetworkContext ctx, IEnumerable<IDev2Definition> definitions, Func<IDev2Definition, string> inputExpressionExtractor, Func<IDev2Definition, string> outputExpressionExtractor, enDev2ArgumentType typeOf, Guid extractFromId, Guid pushToId, ErrorResultTO allErrors)
        {
            // build framed upsert expression ;)
            var toUpsert = Dev2DataListBuilderFactory.CreateBinaryDataListUpsertBuilder(false);

            ErrorResultTO errors;
            foreach(IDev2Definition def in definitions.Where(definition => (!definition.IsRecordSet || !DataListUtil.IsValueRecordset(definition.RawValue)) || DefinitionHasNumericIndex(definition, inputExpressionExtractor, outputExpressionExtractor)))
            {
                string expression = inputExpressionExtractor(def);

                if(expression != string.Empty)
                {
                    // Evaluate from extractDL
                    IBinaryDataListEntry val = Evaluate(ctx, extractFromId, enActionType.User, expression, out errors);

                    allErrors.MergeErrors(errors);
                    if(val == null)
                    {
                        string errorTmp;
                        val = DataListConstants.baseEntry.Clone(enTranslationDepth.Shape, pushToId, out errorTmp);
                        allErrors.AddError(errorTmp);
                    }

                    // now upsert into the pushDL
                    string upsertExpression = outputExpressionExtractor(def);
                    toUpsert.Add(upsertExpression, val);
                }
                else if(expression == string.Empty && (typeOf == enDev2ArgumentType.Input) && def.IsRequired)
                {
                    allErrors.AddError("Required input [[" + def.Name + "]] cannot be populated");
                }
            }

            // finally process instruction set and move data
            if(toUpsert.HasData())
            {
                Upsert(ctx, pushToId, toUpsert, out errors);
                allErrors.MergeErrors(errors);
            }

        }

        /// <summary>
        /// Definitions the index of the has numeric.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="inputExpressionExtractor">The input expression extractor.</param>
        /// <param name="outputExpressionExtractor">The output expression extractor.</param>
        /// <returns></returns>
        bool DefinitionHasNumericIndex(IDev2Definition definition, Func<IDev2Definition, string> inputExpressionExtractor, Func<IDev2Definition, string> outputExpressionExtractor)
        {
            bool inputHasNumericIndex = DataListUtil.GetRecordsetIndexType(inputExpressionExtractor(definition)) == enRecordsetIndexType.Numeric;
            bool outputHasNumericIndex = DataListUtil.GetRecordsetIndexType(outputExpressionExtractor(definition)) == enRecordsetIndexType.Numeric;
            var hasNumericIndex = inputHasNumericIndex || outputHasNumericIndex;
            return hasNumericIndex;
        }

        /// <summary>
        /// Processes the record sets.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <param name="definitions">The definitions.</param>
        /// <param name="inputExpressionExtractor">The input expression extractor.</param>
        /// <param name="outputExpressionExtractor">The output expression extractor.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="extractFromId">The extract from unique identifier.</param>
        /// <param name="pushToId">The push automatic unique identifier.</param>
        /// <param name="allErrors">All errors.</param>
        void ProcessRecordSets(NetworkContext ctx, IEnumerable<IDev2Definition> definitions, Func<IDev2Definition, string> inputExpressionExtractor, Func<IDev2Definition, string> outputExpressionExtractor, enDev2ArgumentType typeOf, Guid extractFromId, Guid pushToId, ErrorResultTO allErrors)
        {
            ErrorResultTO errors;

            // fetch records where it is a recordset and mapped to something ;)
            var tmpRecs = definitions.Where(definition => definition.IsRecordSet && DataListUtil.IsValueRecordset(definition.RawValue)
                                                            && !string.IsNullOrEmpty(definition.RawValue)
                                                            && !DefinitionHasNumericIndex(definition, inputExpressionExtractor, outputExpressionExtractor));


            ILookup<string, IDev2Definition> recordSets;

            if(typeOf == enDev2ArgumentType.DB_ForEach)
            {
                // ReSharper disable ImplicitlyCapturedClosure
                recordSets = tmpRecs.ToLookup(definition =>
                    // ReSharper restore ImplicitlyCapturedClosure
                                              DataListUtil.AddBracketsToValueIfNotExist(
                                                  DataListUtil.MakeValueIntoHighLevelRecordset(
                                                      DataListUtil.ExtractRecordsetNameFromValue(
                                                          outputExpressionExtractor(definition)
                                                          ), true)));
            }
            else
            {
                // ReSharper disable ImplicitlyCapturedClosure
                recordSets = tmpRecs.ToLookup(definition =>
                    // ReSharper restore ImplicitlyCapturedClosure
                DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.MakeValueIntoHighLevelRecordset(DataListUtil.ExtractRecordsetNameFromValue(inputExpressionExtractor(definition)
                ), true)));
            }

            var recordsetUpserter = new Dev2DataListUpsertPayloadBuilder<RecordsetGroup>(false);

            foreach(var recordSet in recordSets)
            {
                var val = Evaluate(ctx, extractFromId, enActionType.User, recordSet.Key, out errors);

                if(val != null)
                {
                    var rsDefinitions = recordSet.ToList();

                    var recordSetGroup = new RecordsetGroup(val, rsDefinitions, inputExpressionExtractor, outputExpressionExtractor);

                    recordsetUpserter.Add(recordSetGroup);
                }
            }

            // only flush if there is work to do ;)
            if(recordsetUpserter.HasData())
            {
                Upsert(ctx, pushToId, recordsetUpserter, out errors);
                allErrors.MergeErrors(errors);
            }
        }

        public Func<IDev2Definition, string> BuildInputExpressionExtractor(enDev2ArgumentType typeOf)
        {
            switch(typeOf)
            {
                case enDev2ArgumentType.Input:
                    return def =>
                    {
                        string expression = string.Empty;
                        if(def.Value == string.Empty)
                        {
                            if(def.DefaultValue != string.Empty)
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
                case enDev2ArgumentType.Output:
                case enDev2ArgumentType.DB_ForEach:
                    return def =>
                            {
                                string expression;

                                if(def.RecordSetName != string.Empty)
                                {
                                    expression = def.RecordSetName + "(*)." + def.Name; // star because we are fetching all to place into the parentDataList
                                }
                                else
                                {
                                    expression = def.Name;
                                }

                                return "[[" + expression + "]]";
                            };
                case enDev2ArgumentType.Output_Append_Style:
                    return def =>
                            {
                                string expression;

                                if(def.RecordSetName != string.Empty)
                                {
                                    expression = def.RecordSetName + "()." + def.Name; // () because we are fetching last row to append
                                }
                                else
                                {
                                    expression = def.Name;
                                }

                                return "[[" + expression + "]]";
                            };
                default:
                    throw new ArgumentOutOfRangeException("typeOf");
            }

        }

        public Func<IDev2Definition, string> BuildOutputExpressionExtractor(enDev2ArgumentType typeOf)
        {
            switch(typeOf)
            {
                case enDev2ArgumentType.Input:
                    return def =>
                            {
                                string expression;

                                if(string.IsNullOrEmpty(def.RecordSetName))
                                {
                                    expression = def.Name;
                                }
                                else
                                {
                                    expression = def.RecordSetName + "(*)." + def.Name;
                                }

                                return DataListUtil.AddBracketsToValueIfNotExist(expression);
                            };
                case enDev2ArgumentType.Output:
                case enDev2ArgumentType.Output_Append_Style:
                case enDev2ArgumentType.DB_ForEach:
                    return def =>
                        {
                            string expression = string.Empty;

                            if(def.Value != string.Empty)
                            {
                                expression = def.RawValue;
                            }

                            return expression;
                        };
                default:
                    throw new ArgumentOutOfRangeException("typeOf");
            }

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
        /// Tries the fetch data list.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        /// TODO : Replace with proper server coms
        private IBinaryDataList TryFetchDataList(Guid id, out string error)
        {

            ErrorResultTO errors;
            error = string.Empty;

            IBinaryDataList result = _dlServer.ReadDatalist(id, out errors);

            if(result == null)
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
            ErrorResultTO errors;

            if(!_dlServer.WriteDataList(payload.UID, payload, out errors))
            {
                error = "Failed to write DataList";
                r = false; //Bug 8796, r was never being set to false >.<
            }

            return r;
        }



        /// <summary>
        /// Internals the calc evaluation.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="bdl">The BDL.</param>
        /// <param name="errors">The errors.</param>
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
        /// <param name="bdl">The BDL.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="toRoot">if set to <c>true</c> [automatic root].</param>
        /// <returns></returns>
        private IBinaryDataListEntry InternalEvaluate(string expression, IBinaryDataList bdl, out ErrorResultTO errors, bool toRoot = false)
        {
            IBinaryDataListEntry result;

            ErrorResultTO allErrors = new ErrorResultTO();
            string calcExp;

            if(DataListUtil.IsCalcEvaluation(expression, out calcExp))
            {
                expression = calcExp;
                string r = InternalCalcEvaluation(expression, bdl, out errors);
                allErrors.MergeErrors(errors);
                result = Dev2BinaryDataListFactory.CreateEntry(GlobalConstants.EvalautionScalar, string.Empty, bdl.UID);
                string error;
                result.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem(r, GlobalConstants.EvalautionScalar), out error);
                if(error != string.Empty)
                {
                    allErrors.AddError(error);
                }
            }
            else
            {
                //  Force debug mode for now
                EvaluateRuleSet ers = new EvaluateRuleSet { BinaryDataList = bdl, Expression = expression, EvaluateToRootOnly = toRoot, IsDebug = true };
                result = InternalDataListEvaluateV2(ers);
                allErrors.MergeErrors(ers.Errors);
            }

            errors = allErrors;

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
            if(payload == null)
            {
                return false;
            }

            var result = payload.IndexOf("[[", StringComparison.Ordinal) >= 0;

            return result;
        }

        private IBinaryDataListEntry InternalDataListEvaluateV2(EvaluateRuleSet rules)
        {
            if(IsEvaluated(rules.Expression))
            {
                IList<IIntellisenseResult> expressionParts = _parser.ParseExpressionIntoParts(rules.Expression, rules.FetchIntellisenseParts());

                // fetch all errors for processing
                var parseErrors = expressionParts.Where(c => c.Type == enIntellisenseResultType.Error);
                rules.ProcessErrorTokens(parseErrors);

                // fetch all valid parts for processing ;)
                var validParts = expressionParts.Where(c => c.Type == enIntellisenseResultType.Selectable && rules.Expression.IndexOf(c.Option.DisplayValue, StringComparison.Ordinal) >= 0);

                // now compile the expression in a phase 1 pass to replace dl token and leave static data ;)
                rules.CompileExpression(validParts);

                // Build new evaluate that only handles valid dl parts at a atomic level ;)
                var toBind = rules.FetchTokensForEvaluation();


                // bind each item ;)
                foreach(var binding in toBind)
                {
                    ErrorResultTO invokeErrors;
                    IBinaryDataListEntry res = BindVariable(rules.BinaryDataList, binding, out invokeErrors);
                    rules.Errors.MergeErrors(invokeErrors);
                    rules.AddBoundItem(binding, res);

                }

                var result = rules.BindCompiledExpression();
                if(result != null)
                {
                    // this block of logic got stuck at the bottom.... Quite key to recursive evaluation.
                    // must stay at this level since it triggers the root level evaluation behavior required for upsert ;)
                    // else you get the value, not the expression.
                    if(rules.EvaluateToRootOnly && DataListUtil.IsRootVariable(rules.CompiledExpression))
                    {
                        // Create a new entry for return ;)
                        string error;
                        var field = GlobalConstants.NullEntryNamespace + Guid.NewGuid();
                        result = Dev2BinaryDataListFactory.CreateEntry(field, string.Empty, rules.BinaryDataList.UID);
                        result.TryPutScalar(new BinaryDataListItem(rules.CompiledExpression, field), out error);
                        rules.Errors.AddError(error);

                        return result;
                    }

                    // Check if compiled expression has more parts ;)
                    if(IsEvaluated(rules.CompiledExpression))
                    {
                        EvaluateRuleSet ers = new EvaluateRuleSet(rules) { BinaryDataList = rules.BinaryDataList, Expression = rules.CompiledExpression, EvaluateToRootOnly = rules.EvaluateToRootOnly, IsDebug = rules.IsDebug };

                        result = InternalDataListEvaluateV2(ers);
                    }

                    return result;
                }

                if(rules.CompiledExpression == null)
                {
                    return null;
                }

                // not entirely sure how this block gets hit... 
                if(rules.EvaluateToRootOnly && DataListUtil.IsRootVariable(rules.CompiledExpression))
                {
                    // Create a new entry for return ;)
                    string error;
                    var field = GlobalConstants.NullEntryNamespace + Guid.NewGuid();
                    result = Dev2BinaryDataListFactory.CreateEntry(field, string.Empty, rules.BinaryDataList.UID);
                    result.TryPutScalar(new BinaryDataListItem(rules.CompiledExpression, field), out error);
                    rules.Errors.AddError(error);

                    return result;
                }


                // we need to drop in again for further evaluation ;)
                EvaluateRuleSet ers2 = new EvaluateRuleSet(rules) { BinaryDataList = rules.BinaryDataList, Expression = rules.CompiledExpression, EvaluateToRootOnly = rules.EvaluateToRootOnly, IsDebug = rules.IsDebug };

                result = InternalDataListEvaluateV2(ers2);
                return result;
            }
            else
            {

                string error;
                var fieldName = GlobalConstants.NullEntryNamespace + Guid.NewGuid();
                IBinaryDataListEntry result = new BinaryDataListEntry(fieldName, string.Empty, rules.BinaryDataList.UID);

                result.TryPutScalar(Dev2BinaryDataListFactory.CreateBinaryItem(rules.Expression, fieldName), out error);
                rules.Errors.AddError(error);
                return result;
            }
        }

        /// <summary>
        /// Binds the variable.
        /// </summary>
        /// <param name="bdl">The BDL.</param>
        /// <param name="token">The token.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private IBinaryDataListEntry BindVariable(IBinaryDataList bdl, IIntellisenseResult token, out ErrorResultTO errors)
        {
            string displayValue = token.Option.DisplayValue;
            string field = token.Option.Field;
            string rs = token.Option.Recordset;
            bool isScalar = token.Option.IsScalar;
            errors = new ErrorResultTO();

            // Evaluate from the DataList
            IBinaryDataListEntry val;
            IBinaryDataListEntry result = null;
            string error;

            if(isScalar)
            {
                if(bdl.TryGetEntry(field, out val, out error))
                {
                    result = val.Clone(enTranslationDepth.Data, bdl.UID, out error);
                }

                errors.AddError(error);

                return result;
            }

            var idx = token.Option.RecordsetIndex;

            string colsToKeep = !string.IsNullOrEmpty(field) ? field : GlobalConstants.AllColumns;

            if(bdl.TryGetEntry(rs, out val, out error))
            {
                enRecordsetIndexType idxType = DataListUtil.GetRecordsetIndexTypeRaw(idx);

                if(idxType == enRecordsetIndexType.Numeric || idxType == enRecordsetIndexType.Blank)
                {
                    int myIdx = idxType == enRecordsetIndexType.Numeric ? Int32.Parse(idx) : val.FetchLastRecordsetIndex();
                    var res = val.Clone(enTranslationDepth.Shape, bdl.UID, out error);
                    res.MakeRecordsetEvaluateReady(myIdx, colsToKeep, out error);
                    errors.AddError(error);

                    return res;
                }

                if(idxType == enRecordsetIndexType.Error)
                {
                    errors.AddError("Invalid Recordset Index For { " + displayValue + " }");
                    return null;
                }

                if(idxType == enRecordsetIndexType.Star)
                {
                    var res = val.Clone(enTranslationDepth.Shape, bdl.UID, out error);
                    res.MakeRecordsetEvaluateReady(GlobalConstants.AllIndexes, colsToKeep, out error);
                    errors.AddError(error);

                    return res;
                }
            }

            errors.AddError(error);

            return null;
        }

        //PBI 8735 - Massimo.Guerrera - Debug items for the multiassign
        private List<KeyValuePair<string, IBinaryDataListEntry>> _debugValues = new List<KeyValuePair<string, IBinaryDataListEntry>>();

        public List<KeyValuePair<string, IBinaryDataListEntry>> GetDebugItems()
        {
            return _debugValues;
        }

        /// <summary>
        /// Upserts the specified CTX.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctx">The CTX.</param>
        /// <param name="curDLID">The cur DLID.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private Guid Upsert<T>(NetworkContext ctx, Guid curDLID, IDev2DataListUpsertPayloadBuilder<T> payload, out ErrorResultTO errors)
        {
            _debugValues = new List<KeyValuePair<string, IBinaryDataListEntry>>();
            ErrorResultTO allErrors = new ErrorResultTO();
            Dev2RecordsetIndexScope rsis = new Dev2RecordsetIndexScope();
            Dev2TokenConverter tc = new Dev2TokenConverter();

            Guid result = GlobalConstants.NullDataListID;
            IBinaryDataList bdl = FetchBinaryDataList(ctx, curDLID, out errors);
            allErrors.MergeErrors(errors);
            errors.ClearErrors();

            int toRemoveFromGap = -1;
            int debugIdx = -1;

            if(bdl != null)
            {
                // Fetch will force a commit if any frames are hanging ;)
                string error;
                DebugTO debugTO = new DebugTO();
                foreach(IDataListPayloadIterationFrame<T> f in payload.FetchFrames())
                {
                    IBinaryDataListEntry entryUsed = null;
                    // iterate per frame fetching frame items

                    while(f.HasData())
                    {
                        if(typeof(T) == typeof(RecordsetGroup))
                        {
                            ProcessRecordsetGroup(f.FetchNextFrameItem().Value as RecordsetGroup, bdl, out errors);
                            allErrors.MergeErrors(errors);
                            continue;
                        }

                        if(!payload.IsIterativePayload())
                        {
                            debugTO = new DebugTO();
                        }
                        DataListPayloadFrameTO<T> frameItem = f.FetchNextFrameItem();

                        // find the part to use
                        IIntellisenseResult part = tc.ParseTokenForMatch(frameItem.Expression, bdl.FetchIntellisenseParts());

                        // recursive eval ;)
                        if(part == null)
                        {
                            EvaluateRuleSet ers = new EvaluateRuleSet { BinaryDataList = bdl, Expression = frameItem.Expression, EvaluateToRootOnly = true, IsDebug = payload.IsDebug };

                            IBinaryDataListEntry tmpItem = InternalDataListEvaluateV2(ers);
                            allErrors.MergeErrors(ers.Errors);
                            errors.ClearErrors();

                            // now find the correct token based upon the eval
                            if(tmpItem != null)
                            {
                                part = tc.ParseTokenForMatch(tmpItem.FetchScalar().TheValue, bdl.FetchIntellisenseParts());
                            }
                        }

                        // now we have the part, build up the item to push into the entry....
                        if(part != null)
                        {

                            string field = part.Option.Field;
                            string rs = part.Option.Recordset;

                            // Process evaluated values via some Generic magic....
                            IBinaryDataListEntry evaluatedValue = null;
                            if(typeof(T) == typeof(string))
                            {
                                string itemVal = frameItem.Value.ToString();
                                evaluatedValue = InternalEvaluate(itemVal, bdl, out errors);

                                if(errors.HasErrors())
                                {
                                    allErrors.MergeErrors(errors);
                                    evaluatedValue = InternalEvaluate(string.Empty, bdl, out errors);
                                }
                                else
                                {
                                    allErrors.MergeErrors(errors);
                                }
                            }
                            else if(typeof(T) == typeof(IBinaryDataListEntry))
                            {
                                evaluatedValue = (IBinaryDataListEntry)frameItem.Value;

                                if(!evaluatedValue.IsRecordset)
                                {
                                    IBinaryDataListItem itm = evaluatedValue.FetchScalar();
                                    if(!itm.IsDeferredRead)
                                    {
                                        var val = evaluatedValue.FetchScalar().TheValue;
                                        IIntellisenseResult res = tc.ParseTokenForMatch(val, bdl.FetchIntellisenseParts());
                                        if(res != null && res.Type == enIntellisenseResultType.Selectable)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            else if(typeof(T) == typeof(List<string>))
                            {
                                var value = frameItem.Value;
                                var list = value as List<string>;
                                if(part.Option.IsScalar)
                                {
                                    string csvValue = string.Join(",", list);
                                    evaluatedValue = InternalEvaluate(csvValue, bdl, out errors);
                                    allErrors.MergeErrors(errors);
                                }
                                else if(part.Option.HasRecordsetIndex && DataListUtil.GetRecordsetIndexType(part.Option.DisplayValue) == enRecordsetIndexType.Numeric)
                                {
                                    evaluatedValue = InternalEvaluate(list[list.Count - 1], bdl, out errors);
                                }
                                else
                                {
                                    string entryNamespace = GlobalConstants.NullEntryNamespace + Guid.NewGuid();
                                    IBinaryDataListEntry binaryDataListEntry = Dev2BinaryDataListFactory.CreateEntry(entryNamespace, string.Empty, new List<Dev2Column> { DataListFactory.CreateDev2Column(field, field) }, bdl.UID);
                                    for(int i = 0; i < list.Count; i++)
                                    {
                                        IBinaryDataListItem binaryDataListItem = Dev2BinaryDataListFactory.CreateBinaryItem(list[i], entryNamespace, field, i + 1);
                                        string errString;
                                        binaryDataListEntry.TryPutRecordItemAtIndex(binaryDataListItem, i + 1, out errString);
                                        if(!String.IsNullOrEmpty(errString))
                                        {
                                            errors.AddError(errString);
                                        }
                                    }

                                    evaluatedValue = binaryDataListEntry;
                                }
                            }

                            if(payload.IsDebug)
                            {
                                IBinaryDataListEntry leftSide;
                                IBinaryDataListEntry tmpEntry;
                                if(part.Option.IsScalar)
                                {
                                    bdl.TryGetEntry(field, out tmpEntry, out error);
                                    leftSide = tmpEntry.Clone(enTranslationDepth.Data, Guid.NewGuid(), out error);
                                }
                                else
                                {
                                    bdl.TryGetEntry(part.Option.Recordset, out tmpEntry, out error);
                                    leftSide = tmpEntry.Clone(enTranslationDepth.Data, Guid.NewGuid(), out error);
                                }
                                debugTO.LeftEntry = leftSide;
                                debugTO.LeftEntry.ComplexExpressionAuditor = new ComplexExpressionAuditor();
                                ErrorResultTO invokeError;
                                ProcessLeftSide(debugTO, frameItem, bdl, out invokeError);
                                allErrors.MergeErrors(invokeError);
                            }
                            allErrors.MergeErrors(errors);
                            if(payload.IsDebug)
                            {
                                if(evaluatedValue == null)
                                {
                                    allErrors.AddError("Problems Evaluating Expression [ " + frameItem.Value + " ]");
                                }
                                else
                                {
                                    debugTO.RightEntry = evaluatedValue;
                                    debugTO.RightEntry.ComplexExpressionAuditor = new ComplexExpressionAuditor();
                                    ErrorResultTO invokeError;
                                    ProcessRightSide(debugTO, frameItem, bdl, out invokeError);
                                    allErrors.MergeErrors(invokeError);
                                }
                            }
                            // check entry cache based upon type ;)
                            IBinaryDataListEntry entry;
                            if(part.Option.IsScalar)
                            {
                                bdl.TryGetEntry(field, out entry, out error);
                                allErrors.AddError(error);

                                if(payload.IsDebug && entry != null)
                                {
                                    debugTO.TargetEntry = entry.Clone(enTranslationDepth.Data, Guid.NewGuid(), out error);
                                }


                                if(evaluatedValue != null && entry != null)
                                {
                                    if(!evaluatedValue.IsRecordset)
                                    {
                                        // 01.02.2013 - Travis.Frisinger : Bug 8579 
                                        var scalar = evaluatedValue.FetchScalar();
                                        IBinaryDataListItem tmpI = scalar.Clone();

                                        tmpI.UpdateField(field);
                                        entry.TryPutScalar(tmpI, out error);
                                        allErrors.AddError(error);
                                        BuildComplexExpressionsForDebug(debugTO, part, tmpI, tmpI.DisplayValue);

                                    }
                                    else
                                    {
                                        // process it as a recordset to scalar ie last value is placed ;) unless needs to be placed in scalar as CSV :P
                                        if(payload.RecordSetDataAsCSVToScalar)
                                        {
                                            IBinaryDataListItem tmpI = evaluatedValue.TryFetchLastIndexedRecordsetUpsertPayload(out error).Clone();
                                            tmpI.UpdateField(field);
                                            string updatedValue = entry.FetchScalar().TheValue + "," + tmpI.TheValue;
                                            tmpI.UpdateValue(updatedValue.TrimStart(','));
                                            entry.TryPutScalar(tmpI, out error);
                                            allErrors.AddError(error);

                                            allErrors.AddError(error);
                                            BuildComplexExpressionsForDebug(debugTO, part, tmpI, tmpI.DisplayValue);
                                        }
                                        else
                                        {
                                            // 01.02.2013 - Travis.Frisinger : Bug 8579 
                                            IBinaryDataListItem tmpI = evaluatedValue.TryFetchLastIndexedRecordsetUpsertPayload(out error).Clone();
                                            tmpI.UpdateField(field);
                                            entry.TryPutScalar(tmpI, out error);
                                            allErrors.AddError(error);
                                            BuildComplexExpressionsForDebug(debugTO, part, tmpI, tmpI.DisplayValue);
                                        }
                                    }
                                } // else do nothing

                            }
                            else
                            {
                                bdl.TryGetEntry(part.Option.Recordset, out entry, out error);

                                if(payload.IsDebug && (!payload.IsIterativePayload() || debugTO.TargetEntry == null))
                                {
                                    debugTO.TargetEntry = Dev2BinaryDataListFactory.CreateEntry(entry.Namespace, entry.Description, entry.Columns, Guid.NewGuid());
                                    debugTO.TargetEntry.ComplexExpressionAuditor = entry.ComplexExpressionAuditor;
                                }

                                allErrors.AddError(error);
                                if(entry != null)
                                {
                                    int idx = rsis.FetchRecordsetIndex(part, entry, payload.IsIterativePayload());

                                    enRecordsetIndexType idxType = DataListUtil.GetRecordsetIndexTypeRaw(part.Option.RecordsetIndex);

                                    if(idx > 0)
                                    {
                                        debugIdx = idx;

                                        if(evaluatedValue != null)
                                        {
                                            if(!evaluatedValue.IsRecordset)
                                            {
                                                IBinaryDataListItem tmpI;
                                                // we have a scalar to recordset....
                                                switch(idxType)
                                                {
                                                    case enRecordsetIndexType.Star:
                                                        if(!payload.IsIterativePayload())
                                                        {
                                                            // scalar to star
                                                            IIndexIterator ii = entry.FetchRecordsetIndexes();
                                                            while(ii.HasMore())
                                                            {
                                                                int next = ii.FetchNextIndex();
                                                                // 01.02.2013 - Travis.Frisinger : Bug 8579 
                                                                var scalar = evaluatedValue.FetchScalar();
                                                                tmpI = scalar.Clone();
                                                                tmpI.UpdateField(field);
                                                                entry.TryPutRecordItemAtIndex(tmpI, next, out error);
                                                                allErrors.AddError(error);
                                                                BuildComplexExpressionsForDebug(debugTO, part, tmpI, DataListUtil.ReplaceStarWithFixedIndex(part.Option.DisplayValue, next));
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // we need to move the iteration overwrite indexs ?
                                                            // 01.02.2013 - Travis.Frisinger : Bug 8579 
                                                            var scalar = evaluatedValue.FetchScalar();
                                                            tmpI = scalar.Clone();
                                                            tmpI.UpdateField(field);
                                                            tmpI.UpdateRecordset(rs);
                                                            tmpI.UpdateIndex(idx);

                                                            entry.TryPutRecordItemAtIndex(tmpI, idx, out error);

                                                            allErrors.AddError(error);
                                                            BuildComplexExpressionsForDebug(debugTO, part, tmpI, tmpI.DisplayValue);
                                                        }
                                                        break;

                                                    case enRecordsetIndexType.Error:
                                                        //2013.05.29: Ashley Lewis for bug 9379 - throw an error on invalid recordset index
                                                        allErrors.AddError("Recordset index (" + part.Option.RecordsetIndex + ") contains invalid character(s)");
                                                        break;

                                                    default:
                                                        // scalar to index
                                                        // 01.02.2013 - Travis.Frisinger : Bug 8579 

                                                        var dataListItem = evaluatedValue.FetchScalar();
                                                        tmpI = dataListItem.Clone();
                                                        tmpI.UpdateRecordset(rs);
                                                        tmpI.UpdateField(field);
                                                        tmpI.UpdateIndex(idx);
                                                        entry.TryPutRecordItemAtIndex(tmpI, idx, out error);
                                                        allErrors.AddError(error);
                                                        BuildComplexExpressionsForDebug(debugTO, part, tmpI, tmpI.DisplayValue);
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                int starPopIdxPos = 0;

                                                // field to field move
                                                if(!string.IsNullOrEmpty(field))
                                                {

                                                    IIndexIterator idxItr = evaluatedValue.FetchRecordsetIndexes();
                                                    IIndexIterator starPopIdx = entry.FetchRecordsetIndexes();
                                                    IList<int> populateIdxs = new List<int> { idx };

                                                    if(idxType == enRecordsetIndexType.Star)
                                                    {

                                                        starPopIdx = entry.FetchRecordsetIndexes().Clone();

                                                        int gapAdd = 0;
                                                        if(starPopIdx.Count > 0)
                                                        {
                                                            toRemoveFromGap = idx;
                                                            starPopIdx.AddGap(idx);
                                                            gapAdd += 1;
                                                        }

                                                        if(idxItr.Count == 1)
                                                        {
                                                            //2013.07.29: Ashley Lewis for bug 9963 - don't add any gap if there is just one populated index
                                                            if(starPopIdx.Count == 1)
                                                            {
                                                                gapAdd = 0;
                                                            }
                                                            IIndexIterator newIdxItr = Dev2BinaryDataListFactory.CreateLoopedIndexIterator(idxItr.MinIndex(), (starPopIdx.Count + gapAdd));
                                                            idxItr = newIdxItr; // swap for the repeat ;)
                                                        }
                                                    }

                                                    // now push the Value data into the recordset
                                                    while(idxItr.HasMore())
                                                    {
                                                        int next = idxItr.FetchNextIndex();
                                                        IList<IBinaryDataListItem> itms = evaluatedValue.FetchRecordAt(next, out error);

                                                        allErrors.AddError(error);

                                                        // TODO : Handle * -> () correctly ;)
                                                        foreach(int index in populateIdxs)
                                                        {
                                                            allErrors.AddError(error);
                                                            if(itms != null && itms.Count == 1)
                                                            {
                                                                // 01.02.2013 - Travis.Frisinger : Bug 8579 
                                                                IBinaryDataListItem tmpI = itms[0].Clone();
                                                                tmpI.UpdateRecordset(rs);
                                                                tmpI.UpdateField(field);
                                                                tmpI.UpdateIndex(index);
                                                                entry.TryPutRecordItemAtIndex(tmpI, index, out error);
                                                                allErrors.AddError(error);
                                                                BuildComplexExpressionsForDebug(debugTO, part, tmpI, tmpI.DisplayValue);
                                                            }
                                                            else if(itms != null && itms.Count > 1)
                                                            {
                                                                // all good move it
                                                                foreach(IBinaryDataListItem i in itms)
                                                                {
                                                                    // 01.02.2013 - Travis.Frisinger : Bug 8579 
                                                                    IBinaryDataListItem tmpI = i.Clone();

                                                                    tmpI.UpdateRecordset(rs);
                                                                    tmpI.UpdateField(field);
                                                                    tmpI.UpdateIndex(index);

                                                                    entry.TryPutRecordItemAtIndex(tmpI, index, out error);
                                                                    allErrors.AddError(error);
                                                                    BuildComplexExpressionsForDebug(debugTO, part, tmpI, tmpI.DisplayValue);
                                                                }
                                                            }
                                                        }

                                                        // we need to roll the index to keep the ship moving...
                                                        if(entry.IsRecordset && idxType == enRecordsetIndexType.Blank && populateIdxs.Count > 0)
                                                        {
                                                            populateIdxs[0]++;
                                                        }
                                                        else if(idxType == enRecordsetIndexType.Star)
                                                        {
                                                            // handle * iteration ;)

                                                            if(starPopIdxPos < starPopIdx.Count)
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
                                                    if(entry.HasColumns(evaluatedValue.Columns))
                                                    {
                                                        IIndexIterator ii = evaluatedValue.FetchRecordsetIndexes();
                                                        while(ii.HasMore())
                                                        {
                                                            int next = ii.FetchNextIndex();
                                                            IList<IBinaryDataListItem> itms = evaluatedValue.FetchRecordAt(next, out error);
                                                            allErrors.AddError(error);
                                                            // all good move it
                                                            foreach(IBinaryDataListItem i in itms)
                                                            {
                                                                entry.TryPutRecordItemAtIndex(i, i.ItemCollectionIndex, out error);
                                                                BuildComplexExpressionsForDebug(debugTO, part, i, i.DisplayValue);
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
                                    if(toRemoveFromGap > 0)
                                    {
                                        entry.FetchRecordsetIndexes().RemoveGap(toRemoveFromGap);
                                        toRemoveFromGap = -1;
                                    }
                                }
                            }
                            if(payload.IsDebug)
                            {
                                debugTO.UsedRecordsetIndex = debugIdx;

                                // reset debug index ;)
                                debugIdx = -1;
                            }
                        }
                        else
                        {

                            string token = DataListUtil.StripBracketsFromValue(frameItem.Expression);

                            // system expression ;)
                            if(token.IndexOf(GlobalConstants.SystemTagNamespace, StringComparison.Ordinal) >= 0 || DataListUtil.IsSystemTag(token))
                            {

                                if(token.IndexOf(GlobalConstants.SystemTagNamespace, StringComparison.Ordinal) < 0)
                                {
                                    token = DataListUtil.BuildSystemTagForDataList(token, false);
                                }

                                IBinaryDataListEntry theEntry;
                                bdl.TryGetEntry(token, out theEntry, out error);
                                if(error != string.Empty)
                                {
                                    allErrors.AddError(error);
                                }
                                else
                                {
                                    IBinaryDataListEntry evalautedValue = null;
                                    if(typeof(T) == typeof(string))
                                    {
                                        evalautedValue = Evaluate(ctx, curDLID, enActionType.User, frameItem.Value.ToString(), out errors);
                                    }
                                    else if(typeof(T) == typeof(IBinaryDataListEntry))
                                    {
                                        evalautedValue = (IBinaryDataListEntry)frameItem.Value;
                                    }

                                    if(evalautedValue != null)
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

                        if(payload.IsDebug && !payload.IsIterativePayload())
                        {
                            var recIndexType = DataListUtil.GetRecordsetIndexType(frameItem.Expression);
                            switch(recIndexType)
                            {
                                case enRecordsetIndexType.Blank:
                                    debugTO.Expression = frameItem.Expression.Replace("()", "(" + debugTO.UsedRecordsetIndex + ")");
                                    break;
                                case enRecordsetIndexType.Star:
                                    debugTO.Expression = frameItem.Expression.Replace("(*)", "(" + debugTO.UsedRecordsetIndex + ")");
                                    break;
                                case enRecordsetIndexType.Numeric:
                                    debugTO.Expression = frameItem.Expression;
                                    break;
                            }

                            payload.DebugOutputs.Add(debugTO);
                            _debugValues.Add(new KeyValuePair<string, IBinaryDataListEntry>(frameItem.Expression, entryUsed));
                        }
                    }

                    // move index values
                    if(payload.IsIterativePayload())
                    {
                        rsis.MoveIndexesToNextPosition();
                    }
                }
                if(payload.IsIterativePayload())
                {
                    payload.DebugOutputs.Add(debugTO);
                }
                // Now flush all the entries to the bdl for this iteration ;)
                if(TryPushDataList(bdl, out error))
                {
                    result = bdl.UID;
                }
                allErrors.AddError(error);

            }

            errors = allErrors;

            return result;
        }

        static void BuildComplexExpressionsForDebug(DebugTO debugTO, IIntellisenseResult part, IBinaryDataListItem tmpI, string displayValue)
        {
            if(debugTO.TargetEntry != null)
            {
                if(debugTO.TargetEntry.ComplexExpressionAuditor == null)
                {
                    debugTO.TargetEntry.ComplexExpressionAuditor = new ComplexExpressionAuditor();
                }
                debugTO.TargetEntry.ComplexExpressionAuditor.AddAuditStep(part.Option.DisplayValue, "", "", 1, tmpI.TheValue, DataListUtil.AddBracketsToValueIfNotExist(displayValue));
            }
        }

        void ProcessLeftSide<T>(DebugTO debugTO, DataListPayloadFrameTO<T> frame, IBinaryDataList bdl, out ErrorResultTO invokeErrors)
        {
            var leftSide = frame.Expression;
            var leftEntry = debugTO.LeftEntry;
            invokeErrors = new ErrorResultTO();
            if(DataListUtil.IsValueRecordsetWithFields(leftSide))
            {
                var idxType = DataListUtil.GetRecordsetIndexType(leftSide);

                // we have an evaluated index ;)
                if(idxType == enRecordsetIndexType.Error)
                {
                    var idx = DataListUtil.ExtractIndexRegionFromRecordset(leftSide);
                    ErrorResultTO errors;
                    var entry = InternalEvaluate(idx, bdl, out errors);
                    invokeErrors.MergeErrors(errors);
                    var value = entry.FetchScalar();
                    if(value != null)
                    {
                        var idxValue = value.TheValue;
                        // replace for emit too ;)
                        if(leftSide != null)
                        {
                            leftSide = leftSide.Replace(idx, idxValue);
                        }

                        idxType = DataListUtil.GetRecordsetIndexTypeRaw(idxValue);
                    }
                }

                // handle silly things where the index does not make sense ;)
                if(idxType == enRecordsetIndexType.Error)
                {
                    invokeErrors.AddError("Error Parsing Recordset Index For Debug Item Generation");
                }

                if(idxType == enRecordsetIndexType.Star)
                {
                    ProcessStarEntry(leftEntry, leftSide);
                }
                else
                {
                    var leftValue = "";
                    if(idxType == enRecordsetIndexType.Numeric)
                    {
                        var stringIndexValue = DataListUtil.ExtractIndexRegionFromRecordset(leftSide);
                        var idx = int.Parse(stringIndexValue);
                        string error;
                        leftValue = leftEntry.TryFetchRecordsetColumnAtIndex(DataListUtil.ExtractFieldNameFromValue(leftSide), idx, out error).TheValue;
                    }
                    leftEntry.ComplexExpressionAuditor.AddAuditStep(leftSide, "", "", 1, leftValue, leftSide);
                }
            }
            else
            {
                leftEntry.ComplexExpressionAuditor.AddAuditStep(leftSide, "", "", 1, leftEntry.FetchScalar().TheValue, leftSide);
            }
        }

        void ProcessRightSide<T>(DebugTO debugTO, DataListPayloadFrameTO<T> frame, IBinaryDataList bdl, out ErrorResultTO invokeErrors)
        {
            var rightSide = "";
            var boundValue = debugTO.RightEntry.FetchScalar().TheValue;
            invokeErrors = new ErrorResultTO();
            var isCalcExpression = false;
            if(typeof(T) == typeof(string))
            {
                rightSide = frame.Value as string;

                string calculationExpression;
                if(DataListUtil.IsCalcEvaluation(rightSide, out calculationExpression))
                {
                    rightSide = string.Format("={0}", calculationExpression);
                    ErrorResultTO errors;
                    var binaryDataListEntry = InternalEvaluate(rightSide, bdl, out errors);
                    invokeErrors.MergeErrors(errors);
                    boundValue = binaryDataListEntry.FetchScalar().TheValue;
                    isCalcExpression = true;
                }
            }

            var rightEntry = debugTO.RightEntry;
            if(DataListUtil.IsValueRecordsetWithFields(rightSide))
            {

                var idxType = DataListUtil.GetRecordsetIndexType(rightSide);

                if(idxType == enRecordsetIndexType.Error && isCalcExpression)
                {
                    rightEntry.ComplexExpressionAuditor.AddAuditStep(rightSide, "", "", 1, boundValue, rightSide);
                    return;
                }

                // we have an evaluated index ;)
                if(idxType == enRecordsetIndexType.Error)
                {
                    var idx = DataListUtil.ExtractIndexRegionFromRecordset(rightSide);
                    ErrorResultTO errors;
                    var entry = InternalEvaluate(idx, bdl, out errors);
                    invokeErrors.MergeErrors(errors);
                    var value = entry.FetchScalar();
                    if(value != null)
                    {
                        var idxValue = value.TheValue;
                        // replace for emit too ;)
                        if(rightSide != null)
                        {
                            rightSide = rightSide.Replace(idx, idxValue);
                        }
                        idxType = DataListUtil.GetRecordsetIndexTypeRaw(idxValue);
                    }
                }

                // handle silly things where the index does not make sense ;)
                if(idxType == enRecordsetIndexType.Error)
                {
                    invokeErrors.AddError("Error Parsing Recordset Index For Debug Item Generation");
                }

                if(idxType == enRecordsetIndexType.Star)
                {
                    ProcessStarEntry(rightEntry, rightSide);
                }
                else
                {
                    rightEntry.ComplexExpressionAuditor.AddAuditStep(rightSide, "", "", 1, boundValue, rightSide);
                }
            }
            else
            {
                rightEntry.ComplexExpressionAuditor.AddAuditStep(rightSide, "", "", 1, boundValue, rightSide);
            }
        }

        static void ProcessStarEntry(IBinaryDataListEntry rightEntry, string rightSide)
        {
            var rightSideItr = rightEntry.FetchRecordsetIndexes();
            while(rightSideItr.HasMore())
            {
                var fetchNextIndex = rightSideItr.FetchNextIndex();
                string error;
                var binaryDataListItems = rightEntry.FetchRecordAt(fetchNextIndex, DataListUtil.ExtractFieldNameFromValue(rightSide), out error);
                if(binaryDataListItems != null && binaryDataListItems.Count > 0)
                {
                    var singleItem = binaryDataListItems[0];
                    var displayValue = DataListUtil.ReplaceStarWithFixedIndex(rightSide, fetchNextIndex);
                    rightEntry.ComplexExpressionAuditor.AddAuditStep(rightSide, "", "", 1, singleItem.TheValue, displayValue);
                }
            }
        }

        #endregion

        // ReSharper restore InconsistentNaming

    }
}
