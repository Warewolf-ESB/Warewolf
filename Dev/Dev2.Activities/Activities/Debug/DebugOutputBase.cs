using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Audit;
using Dev2.Data.Binary_Objects;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;

namespace Dev2.Activities.Debug
{
    public abstract class DebugOutputBase
    {
        // I need to cache recordset data to build up later iterations ;)
        readonly IDictionary<string, string> _rsCachedValues = new Dictionary<string, string>();
        public abstract string LabelText { get; }

        public abstract List<IDebugItemResult> GetDebugItemResult();

        protected ErrorResultTO ErrorsTo;

        public List<IDebugItemResult> CreateDebugItemsFromEntry(string expression, IBinaryDataListEntry dlEntry, Guid dlId, enDev2ArgumentType argumentType, int indexToUse = -1)
        {
            return CreateDebugItemsFromEntry(expression, dlEntry, dlId, argumentType, "", indexToUse);
        }

        public List<IDebugItemResult> CreateDebugItemsFromEntry(string expression, IBinaryDataListEntry dlEntry, Guid dlId, enDev2ArgumentType argumentType, string labelText, int indexToUse = -1)
        {
            var results = new List<IDebugItemResult>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dataList = compiler.FetchBinaryDataList(dlId, out ErrorsTo);

            if(
                !(expression.Contains(GlobalConstants.CalculateTextConvertPrefix) &&
                  expression.Contains(GlobalConstants.CalculateTextConvertSuffix)))
            {
                if(!expression.ContainsSafe("[["))
                {
                    results.Add(new DebugItemResult
                        {
                            Label = labelText,
                            Type = DebugItemResultType.Value,
                            Value = expression
                        });
                    return results;
                }
            }
            else
            {
                expression =
                    expression.Replace(GlobalConstants.CalculateTextConvertPrefix, string.Empty)
                              .Replace(GlobalConstants.CalculateTextConvertSuffix, string.Empty);
            }

            if(dlEntry.ComplexExpressionAuditor == null)
            {
                int groupIndex = 0;
                enRecordsetIndexType rsType = DataListUtil.GetRecordsetIndexType(expression);

                if(dlEntry.IsRecordset &&
                    (DataListUtil.IsValueRecordset(expression) &&
                     (rsType == enRecordsetIndexType.Star ||
                      (rsType == enRecordsetIndexType.Numeric &&
                       DataListUtil.ExtractFieldNameFromValue(expression) == string.Empty) ||
                      (rsType == enRecordsetIndexType.Blank &&
                       DataListUtil.ExtractFieldNameFromValue(expression) == string.Empty))))
                {
                    // Added IsEmpty check for Bug 9263 ;)
                    if(!dlEntry.IsEmpty())
                    {
                        IList<IDebugItemResult> collection = CreateRecordsetDebugItems(expression, dlEntry, string.Empty,
                                                                                      -1, labelText);
                        if(collection.Count < 2 && collection.Count > 0)
                        {
                            collection[0].GroupName = "";
                        }
                        results.AddRange(collection);
                    }
                    else
                    {
                        results.Add(new DebugItemResult
                            {
                                Type = DebugItemResultType.Variable,
                                Label = labelText,
                                Variable = expression,
                                Operator = string.IsNullOrEmpty(expression) ? "" : "=",
                                Value = "",
                            });
                    }
                }
                else
                {
                    if(DataListUtil.IsValueRecordset(expression) &&
                        (DataListUtil.GetRecordsetIndexType(expression) == enRecordsetIndexType.Blank))
                    {
                        if(indexToUse == -1)
                        {
                            IBinaryDataListEntry tmpEntry;
                            string error;
                            dataList.TryGetEntry(DataListUtil.ExtractRecordsetNameFromValue(expression),
                                                 out tmpEntry, out error);
                            if(tmpEntry != null)
                            {
                                int index = tmpEntry.FetchAppendRecordsetIndex() - 1;
                                if(index > 0)
                                {
                                    groupIndex = index;
                                    expression = expression.Replace("().", string.Concat("(", index, ")."));
                                }
                            }
                        }
                        else
                        {
                            expression = expression.Replace("().", string.Concat("(", indexToUse, ")."));
                        }
                    }

                    if(dlEntry.IsRecordset)
                    {
                        var strIndx = DataListUtil.ExtractIndexRegionFromRecordset(expression);
                        int indx;
                        if(int.TryParse(strIndx, out indx))
                        {
                            if(indx > 0)
                            {
                                IBinaryDataListItem item = dlEntry.FetchScalar();
                                CreateScalarDebugItems(expression, item.TheValue, labelText, results, "", groupIndex);
                            }
                            else
                            {
                                CreateScalarDebugItems(expression, "", labelText, results, "", groupIndex);
                            }
                        }
                        else
                        {
                            IBinaryDataListItem itemx = dlEntry.FetchScalar();

                            if(!string.IsNullOrEmpty(strIndx))
                            {
                                IBinaryDataListEntry indexBinaryEntry;
                                IBinaryDataListItem indexItem = null;
                                string error;
                                if(DataListUtil.IsValueRecordset(strIndx))
                                {
                                    dataList.TryGetEntry(DataListUtil.ExtractRecordsetNameFromValue(strIndx),
                                                 out indexBinaryEntry, out error);
                                    var fieldName = DataListUtil.ExtractFieldNameFromValue(strIndx);
                                    int index;
                                    if(int.TryParse(DataListUtil.ExtractIndexRegionFromRecordset(strIndx), out index))
                                    {
                                        indexItem = indexBinaryEntry.TryFetchRecordsetColumnAtIndex(fieldName, index, out error);
                                    }
                                }
                                else
                                {
                                    dataList.TryGetEntry(strIndx, out indexBinaryEntry, out error);
                                    indexItem = indexBinaryEntry.FetchScalar();
                                }
                                if(indexItem != null)
                                {
                                    expression = expression.Replace(string.Format("({0})", strIndx), string.Format("({0})", indexItem.TheValue));
                                }
                            }

                            CreateScalarDebugItems(expression, itemx.TheValue, labelText, results, "", groupIndex);
                        }
                    }
                    else
                    {
                        IBinaryDataListItem item = dlEntry.FetchScalar();
                        CreateScalarDebugItems(expression, item.TheValue, labelText, results, "", groupIndex);
                    }
                }
            }
            else
            {
                // Complex expressions are handled differently ;)
                ComplexExpressionAuditor auditor = dlEntry.ComplexExpressionAuditor;

                int idx = 1;

                foreach(ComplexExpressionAuditItem item in auditor.FetchAuditItems())
                {
                    int grpIdx = idx;
                    string groupName = item.RawExpression;
                    string displayExpression = item.RawExpression;
                    if(displayExpression.Contains("()."))
                    {
                        displayExpression = displayExpression.Replace("().",
                                                                      string.Concat("(", auditor.GetMaxIndex(), ")."));
                    }
                    if(displayExpression.Contains("(*)."))
                    {
                        displayExpression = displayExpression.Replace("(*).", string.Concat("(", idx, ")."));
                    }

                    results.Add(new DebugItemResult
                        {
                            Type = DebugItemResultType.Variable,
                            Label = labelText,
                            Variable = displayExpression,
                            Operator = string.IsNullOrEmpty(displayExpression) ? "" : "=",
                            GroupName = groupName,
                            Value = item.BoundValue,
                            GroupIndex = grpIdx
                        });

                    idx++;
                }
            }

            return results;
        }

        public List<IDebugItemResult> CreateDebugItemForOutput(DebugTO debugTo, string labelText, List<string> regions)
        {
            var results = new List<IDebugItemResult>();
            if(debugTo.TargetEntry != null)
            {
                GetValue(debugTo.TargetEntry, debugTo, labelText, regions, results);
                debugTo.TargetEntry.DisposeCache();
            }

            return results;
        }

        static void GetValue(IBinaryDataListEntry entry, DebugTO debugTo, string labelText, List<string> regions, List<IDebugItemResult> results)
        {
            ComplexExpressionAuditor auditor = entry.ComplexExpressionAuditor;
            if(auditor != null)
            {
                int grpIdx = 0;
                IList<ComplexExpressionAuditItem> complexExpressionAuditItems = auditor.FetchAuditItems();

                foreach(ComplexExpressionAuditItem item in complexExpressionAuditItems)
                {
                    string groupName = null;
                    string displayExpression = item.Expression;
                    string rawExpression = item.RawExpression;
                    if(regions != null && regions.Count > 0)
                    {
                        //    
                    }

                    if(displayExpression.Contains("().") || displayExpression.Contains("(*)."))
                    {
                        grpIdx++;
                        groupName = displayExpression;
                        displayExpression = rawExpression;
                    }
                    else
                    {
                        if(regions != null && regions.Count > 0)
                        {
                            string indexRegionFromRecordset = DataListUtil.ExtractIndexRegionFromRecordset(displayExpression);
                            int indexForRecset;
                            int.TryParse(indexRegionFromRecordset, out indexForRecset);

                            if(indexForRecset > 0)
                            {
                                int indexOfOpenningBracket = displayExpression.IndexOf("(", StringComparison.Ordinal) + 1;
                                string group = displayExpression.Substring(0, indexOfOpenningBracket) + "*" + displayExpression.Substring(indexOfOpenningBracket + indexRegionFromRecordset.Length);

                                if(regions.Contains(@group))
                                {
                                    grpIdx++;
                                    groupName = @group;
                                }
                            }
                        }
                    }

                    int count = complexExpressionAuditItems.Count(i => i.Expression.Equals(item.Expression));
                    if(count < 2)
                    {
                        groupName = "";
                    }

                    var debugOperator = "";
                    var debugType = DebugItemResultType.Value;
                    if(DataListUtil.IsEvaluated(displayExpression))
                    {
                        debugOperator = "=";
                        debugType = DebugItemResultType.Variable;
                    }
                    else
                    {
                        displayExpression = null;
                    }
                    results.Add(new DebugItemResult
                    {
                        Type = debugType,
                        Label = labelText,
                        Variable = displayExpression,
                        Operator = debugOperator,
                        GroupName = groupName,
                        Value = item.BoundValue,
                        GroupIndex = grpIdx
                    });
                }
            }
            else
            {
                //Could not evaluate
                results.Add(new DebugItemResult
                {
                    Type = DebugItemResultType.Value,
                    Label = labelText,
                    Variable = debugTo.Expression,
                    Operator = "=",
                    GroupName = "",
                    Value = ""
                });
            }
        }

        public List<IDebugItemResult> CreateDebugItemForInput(DebugTO debugTo, string labelText, string leftLabel, string rightLabel, List<string> regions)
        {
            var results = new List<IDebugItemResult>();
            if(debugTo.LeftEntry != null)
            {
                GetValue(debugTo.LeftEntry, debugTo, leftLabel, regions, results);
                debugTo.LeftEntry.DisposeCache();
            }

            if(debugTo.RightEntry != null)
            {
                GetValue(debugTo.RightEntry, debugTo, rightLabel, regions, results);
            }

            return results;
        }

        public List<IDebugItemResult> CreateDebugItemsFromString(string expression, string value, Guid dlId, int iterationNumber, enDev2ArgumentType argumentType)
        {
            return CreateDebugItemsFromString(expression, value, dlId, iterationNumber, "", argumentType);
        }

        public List<IDebugItemResult> CreateDebugItemsFromString(string expression, string value, Guid dlId, int iterationNumber, string labelText, enDev2ArgumentType argumentType)
        {
            ErrorResultTO errors;
            IList<IDebugItemResult> resultsToPush = new List<IDebugItemResult>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dataList = compiler.FetchBinaryDataList(dlId, out errors);
            if(DataListUtil.IsValueRecordset(expression))
            {
                enRecordsetIndexType recsetIndexType = DataListUtil.GetRecordsetIndexType(expression);
                string recsetName = DataListUtil.ExtractRecordsetNameFromValue(expression);
                IBinaryDataListEntry currentRecset;
                string error;
                dataList.TryGetEntry(recsetName, out currentRecset, out error);

                if(recsetIndexType == enRecordsetIndexType.Star)
                {
                    if(currentRecset != null)
                    {
                        resultsToPush = CreateRecordsetDebugItems(expression, currentRecset, value, iterationNumber, labelText);
                        if(resultsToPush.Count < 2)
                        {
                            resultsToPush[0].GroupName = "";
                        }
                    }
                }
                else if(recsetIndexType == enRecordsetIndexType.Blank)
                {
                    int recsetIndexToUse = 1;

                    if(currentRecset != null)
                    {
                        if(argumentType == enDev2ArgumentType.Input)
                        {
                            if(!currentRecset.IsEmpty())
                            {
                                recsetIndexToUse = currentRecset.FetchAppendRecordsetIndex() - 1;
                            }
                        }
                        else if(argumentType == enDev2ArgumentType.Output)
                        {
                            if(!currentRecset.IsEmpty())
                            {
                                recsetIndexToUse = currentRecset.FetchAppendRecordsetIndex() - 1;
                            }
                        }
                    }
                    recsetIndexToUse = recsetIndexToUse + iterationNumber;
                    expression = expression.Replace("().", string.Concat("(", recsetIndexToUse, ")."));
                    resultsToPush = string.IsNullOrEmpty(value) ? CreateDebugItemsFromEntry(expression, currentRecset, dlId, argumentType) : CreateScalarDebugItems(expression, value, labelText);
                }
                else
                {
                    resultsToPush = string.IsNullOrEmpty(value) ? CreateDebugItemsFromEntry(expression, currentRecset, dlId, argumentType, labelText) : CreateScalarDebugItems(expression, value, labelText);
                }
            }
            else
            {
                IBinaryDataListEntry binaryDataListEntry;
                string error;
                dataList.TryGetEntry(expression, out binaryDataListEntry, out error);
                resultsToPush = string.IsNullOrEmpty(value) ? CreateDebugItemsFromEntry(expression, binaryDataListEntry, dlId, argumentType, labelText) : CreateScalarDebugItems(expression, value, labelText);
            }

            return resultsToPush.ToList();
        }

        IList<IDebugItemResult> CreateScalarDebugItems(string expression, string value, string label, IList<IDebugItemResult> results = null, string groupName = null, int groupIndex = 0)
        {
            if(results == null)
            {
                results = new List<IDebugItemResult>();
            }

            results.Add(new DebugItemResult
                {
                    Type = DebugItemResultType.Variable,
                    Variable = expression,
                    Operator = string.IsNullOrEmpty(expression) ? "" : "=",
                    Label = label,
                    GroupIndex = groupIndex,
                    GroupName = groupName,
                    Value = value
                });

            return results;
        }

        IList<IDebugItemResult> CreateRecordsetDebugItems(string expression, IBinaryDataListEntry dlEntry, string value, int iterCnt, string labelText)
        {
            var results = new List<IDebugItemResult>();
            if(dlEntry.ComplexExpressionAuditor == null)
            {
                string initExpression = expression;

                string fieldName = DataListUtil.ExtractFieldNameFromValue(expression);
                enRecordsetIndexType indexType = DataListUtil.GetRecordsetIndexType(expression);
                if(indexType == enRecordsetIndexType.Blank && string.IsNullOrEmpty(fieldName))
                {
                    indexType = enRecordsetIndexType.Star;
                }
                if(indexType == enRecordsetIndexType.Star || indexType == enRecordsetIndexType.Numeric)
                {
                    IIndexIterator idxItr = dlEntry.FetchRecordsetIndexes();
                    while(idxItr.HasMore())
                    {
                        GetValues(dlEntry, value, iterCnt, idxItr, indexType, results, initExpression, labelText, fieldName);
                    }
                }
            }
            else
            {
                // Complex expressions are handled differently ;)
                ComplexExpressionAuditor auditor = dlEntry.ComplexExpressionAuditor;
                enRecordsetIndexType indexType = DataListUtil.GetRecordsetIndexType(expression);

                foreach(ComplexExpressionAuditItem item in auditor.FetchAuditItems())
                {
                    int grpIdx = -1;

                    try
                    {
                        grpIdx = Int32.Parse(DataListUtil.ExtractIndexRegionFromRecordset(item.TokenBinding));
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch(Exception)
                    // ReSharper restore EmptyGeneralCatchClause
                    {
                        // Best effort ;)
                    }

                    if(indexType == enRecordsetIndexType.Star)
                    {
                        string displayExpression = item.Expression.Replace(item.Token, item.RawExpression);
                        results.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = displayExpression, GroupName = displayExpression, GroupIndex = grpIdx });
                        results.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression, GroupName = displayExpression, GroupIndex = grpIdx });
                        results.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = item.BoundValue, GroupName = displayExpression, GroupIndex = grpIdx });
                    }
                }
            }

            return results;
        }

        void GetValues(IBinaryDataListEntry dlEntry, string value, int iterCnt, IIndexIterator idxItr, enRecordsetIndexType indexType, IList<IDebugItemResult> results, string initExpression, string labelText, string fieldName = null)
        {
            string error;
            int index = idxItr.FetchNextIndex();
            if(string.IsNullOrEmpty(fieldName))
            {
                IList<IBinaryDataListItem> record = dlEntry.FetchRecordAt(index, out error);
                // ReSharper disable LoopCanBeConvertedToQuery
                foreach(IBinaryDataListItem recordField in record)
                // ReSharper restore LoopCanBeConvertedToQuery
                {
                    GetValue(dlEntry, value, iterCnt, fieldName, indexType, results, initExpression, recordField, index, false, labelText);
                }
            }
            else
            {
                IBinaryDataListItem recordField = dlEntry.TryFetchRecordsetColumnAtIndex(fieldName, index, out error);
                bool ignoreCompare = false;

                if(recordField == null)
                {
                    if(dlEntry.Columns.Count == 1)
                    {
                        recordField = dlEntry.TryFetchIndexedRecordsetUpsertPayload(index, out error);
                        ignoreCompare = true;
                    }
                }

                GetValue(dlEntry, value, iterCnt, fieldName, indexType, results, initExpression, recordField, index, ignoreCompare, labelText);
            }
        }

        void GetValue(IBinaryDataListEntry dlEntry, string value, int iterCnt, string fieldName, enRecordsetIndexType indexType, IList<IDebugItemResult> results, string initExpression, IBinaryDataListItem recordField, int index, bool ignoreCompare, string labelText)
        {
            if(!ignoreCompare)
            {
                OldGetValue(dlEntry, value, iterCnt, fieldName, indexType, results, initExpression, recordField, index, labelText);
            }
            else
            {
                NewGetValue(dlEntry, indexType, results, initExpression, recordField, index, labelText);
            }
        }

        /// <summary>
        ///     A new version of GetValue since Evaluate will now handle complex expressions it is now possible to create gnarly looking debug items
        ///     This method handles these ;)
        /// </summary>
        /// <param name="dlEntry">The dl entry.</param>
        /// <param name="indexType">Type of the index.</param>
        /// <param name="results">The results.</param>
        /// <param name="initExpression">The init expression.</param>
        /// <param name="recordField">The record field.</param>
        /// <param name="index">The index.</param>
        /// <param name="labelText"></param>
        void NewGetValue(IBinaryDataListEntry dlEntry, enRecordsetIndexType indexType, IList<IDebugItemResult> results, string initExpression, IBinaryDataListItem recordField, int index, string labelText)
        {
            string injectVal = string.Empty;
            ComplexExpressionAuditor auditorObj = dlEntry.ComplexExpressionAuditor;

            if(indexType == enRecordsetIndexType.Star && auditorObj != null)
            {
                string instanceData;
                IList<ComplexExpressionAuditItem> auditData = auditorObj.FetchAuditItems();
                if(index <= auditData.Count && index > 0)
                {
                    ComplexExpressionAuditItem useData = auditData[index - 1];
                    instanceData = useData.TokenBinding;
                    injectVal = useData.BoundValue;
                }
                else
                {
                    string recsetName = DataListUtil.CreateRecordsetDisplayValue(dlEntry.Namespace,
                                                                                 recordField.FieldName,
                                                                                 index.ToString(CultureInfo.InvariantCulture));
                    instanceData = DataListUtil.AddBracketsToValueIfNotExist(recsetName);
                }

                results.Add(new DebugItemResult
                    {
                        Label = labelText,
                        Type = DebugItemResultType.Variable,
                        Value = injectVal,
                        Operator = string.IsNullOrEmpty(instanceData) ? "" : "=",
                        Variable = instanceData,
                        GroupName = initExpression,
                        GroupIndex = index
                    });
            }
            else
            {
                injectVal = recordField.TheValue;

                string displayValue = recordField.DisplayValue;

                if(displayValue.IndexOf(GlobalConstants.NullEntryNamespace, StringComparison.Ordinal) >= 0)
                {
                    displayValue = DataListUtil.CreateRecordsetDisplayValue("Evaluated", GlobalConstants.EvaluationRsField, index.ToString(CultureInfo.InvariantCulture));
                }

                results.Add(new DebugItemResult
                    {
                        Type = DebugItemResultType.Variable,
                        Variable = DataListUtil.AddBracketsToValueIfNotExist(displayValue),
                        Operator = string.IsNullOrEmpty(displayValue) ? "" : "=",
                        GroupName = initExpression,
                        Value = injectVal,
                        GroupIndex = index
                    });
            }
        }

        void OldGetValue(IBinaryDataListEntry dlEntry, string value, int iterCnt, string fieldName, enRecordsetIndexType indexType, IList<IDebugItemResult> results, string initExpression, IBinaryDataListItem recordField, int index, string labelText)
        {
            if((string.IsNullOrEmpty(fieldName) || recordField.FieldName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase)))
            {
                string injectVal = recordField.TheValue;
                if(!string.IsNullOrEmpty(value) && recordField.ItemCollectionIndex == (iterCnt + 1))
                {
                    injectVal = value;
                    _rsCachedValues[recordField.DisplayValue] = injectVal;
                }
                else if(string.IsNullOrEmpty(injectVal) && recordField.ItemCollectionIndex != (iterCnt + 1))
                {
                    // is it in the cache? ;)
                    _rsCachedValues.TryGetValue(recordField.DisplayValue, out injectVal);
                    if(injectVal == null)
                    {
                        injectVal = string.Empty;
                    }
                }
                string recsetName;
                if(indexType == enRecordsetIndexType.Star)
                {
                    recsetName = DataListUtil.CreateRecordsetDisplayValue(dlEntry.Namespace,
                                                                          recordField.FieldName,
                                                                          index.ToString(CultureInfo.InvariantCulture));
                    recsetName = DataListUtil.AddBracketsToValueIfNotExist(recsetName);
                }
                else
                {
                    recsetName = DataListUtil.AddBracketsToValueIfNotExist(recordField.DisplayValue);
                }
                results.Add(new DebugItemResult
                    {
                        Label = labelText,
                        Type = DebugItemResultType.Variable,
                        Variable = recsetName,
                        Operator = string.IsNullOrEmpty(recsetName) ? "" : "=",
                        Value = injectVal,
                        GroupName = initExpression,
                        GroupIndex = index
                    });
            }
        }
    }
}