using Dev2;
using Dev2.Activities;
using Dev2.Data.Operations;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfDataMergeActivity : DsfActivityAbstract<string>
    {
        #region Class Members

        private string _result;

        #endregion Class Members

        #region Properties

        private IList<DataMergeDTO> _mergeCollection;
        public IList<DataMergeDTO> MergeCollection
        {
            get
            {
                return _mergeCollection;
            }
            set
            {
                _mergeCollection = value;
                OnPropertyChanged("MergeCollection");
            }
        }

        public new string Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
                OnPropertyChanged("Result");
            }
        }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Ctor

        public DsfDataMergeActivity()
            : base("Data Merge")
        {
            MergeCollection = new List<DataMergeDTO>();
        }

        #endregion

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        protected override void OnExecute(NativeActivityContext context)
        {

            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
            IDev2MergeOperations _mergeOperations = new Dev2MergeOperations();
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = DataListExecutionID.Get(context);

            try
            {

                CleanArguments(MergeCollection);

                if (MergeCollection.Count > 0)
                {
                    IDev2IteratorCollection iteratorCollection = Dev2ValueObjectFactory.CreateIteratorCollection();
                    IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                    allErrors.MergeErrors(errors);
                    List<IDev2DataListEvaluateIterator> listOfIterators = new List<IDev2DataListEvaluateIterator>();

                    #region Create a iterator for each row in the data grid in the designer so that the right iteration happen on the data

                    foreach (DataMergeDTO row in MergeCollection)
                    {
                        IBinaryDataListEntry expressionsEntry = compiler.Evaluate(executionId, enActionType.User, row.InputVariable, false, out errors);
                        allErrors.MergeErrors(errors);
                        IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);

                        iteratorCollection.AddIterator(itr);
                        listOfIterators.Add(itr);
                    }

                    #endregion

                    #region Iterate and Merge Data

                    while (iteratorCollection.HasMoreData())
                    {
                        int pos = 0;
                        foreach (IDev2DataListEvaluateIterator iterator in listOfIterators)
                        {
                            string value = iteratorCollection.FetchNextRow(iterator).TheValue;

                            _mergeOperations.Merge(value, MergeCollection[pos].MergeType, MergeCollection[pos].At,
                                                   MergeCollection[pos].Padding,
                                                   MergeCollection[pos].Alignment);
                            pos++;
                        }
                    }

                    #endregion Iterate and Merge Data

                    #region Add Result to DataList

                    toUpsert.Add(Result, _mergeOperations.MergedData);
                    toUpsert.FlushIterationFrame();
                    compiler.Upsert(executionId, toUpsert, out errors);
                    allErrors.MergeErrors(errors);
                    compiler.Shape(executionId, enDev2ArgumentType.Output, OutputMapping, out errors);
                    allErrors.MergeErrors(errors);

                    #endregion Add Result to DataList
                }
            }
            catch (Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                #region Handle Errors

                if (allErrors.HasErrors())
                {
                    string err = DisplayAndWriteError("DsfDataMergeActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, err, out errors);
                }

                #endregion
            }
        }

        #region Private Methods

        private void CleanArguments(IList<DataMergeDTO> args)
        {
            int count = 0;
            while (count < args.Count)
            {
                if (args[count].IsEmpty())
                {
                    args.RemoveAt(count);
                }
                else
                {
                    count++;
                }
            }
        }

        #endregion Private Methods

        #region Overridden ActivityAbstact Methods

        public override IBinaryDataList GetWizardData()
        {
            string error;
            IBinaryDataList result = Dev2BinaryDataListFactory.CreateDataList();
            string recordsetName = "MergeCollection";
            result.TryCreateScalarValue(Result, "Result", out error);
            result.TryCreateRecordsetTemplate(recordsetName, string.Empty, new List<Dev2Column> { DataListFactory.CreateDev2Column("MergeType", string.Empty), DataListFactory.CreateDev2Column("At", string.Empty), DataListFactory.CreateDev2Column("Result", string.Empty) }, true, out error);
            foreach (DataMergeDTO item in MergeCollection)
            {
                result.TryCreateRecordsetValue(item.MergeType, "MergeType", recordsetName, item.IndexNumber, out error);
                result.TryCreateRecordsetValue(item.At, "At", recordsetName, item.IndexNumber, out error);
            }
            return result;
        }

        #endregion Overridden ActivityAbstact Methods

        #region Get Debug Inputs/Outputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> results = new List<IDebugItem>();
            //BUG 8104 : Refactor DebugItem
            //int indexNum = 1;
            //foreach (DataMergeDTO dataMergeDto in MergeCollection)
            //{
            //    if (!string.IsNullOrEmpty(dataMergeDto.InputVariable))
            //    {
            //        string theValue;
            //        if (DataListUtil.IsValueRecordset(dataMergeDto.InputVariable) && DataListUtil.GetRecordsetIndexType(dataMergeDto.InputVariable) == enRecordsetIndexType.Star)
            //        {
            //            results.Add(new DebugItem(indexNum.ToString() + " Merge ", null, null));
            //            var fieldName = DataListUtil.ExtractFieldNameFromValue(dataMergeDto.InputVariable);
            //            var recset = GetRecordSet(dataList, dataMergeDto.InputVariable);
            //            var idxItr = recset.FetchRecordsetIndexes();
            //            while (idxItr.HasMore())
            //            {
            //                string error;
            //                var index = idxItr.FetchNextIndex();
            //                var record = recset.FetchRecordAt(index, out error);
            //                // ReSharper disable LoopCanBeConvertedToQuery
            //                foreach (var recordField in record)
            //                // ReSharper restore LoopCanBeConvertedToQuery
            //                {
            //                    if (string.IsNullOrEmpty(fieldName) ||
            //                        recordField.FieldName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
            //                    {
            //                        results.Add(new DebugItem(indexNum.ToString(), DataListUtil.AddBracketsToValueIfNotExist(recordField.DisplayValue), " = " + recordField.TheValue)
            //                        {
            //                            Group = dataMergeDto.InputVariable
            //                        });
            //                    }
            //                }
            //            }
            //        }
            //        else
            //        {
            //            theValue = GetValue(dataList, dataMergeDto.InputVariable);
            //            results.Add(new DebugItem(indexNum.ToString() + " Merge ", dataMergeDto.InputVariable,
            //                                      "= " + theValue));
            //        }
            //        string atValue = string.Empty;
            //        if (!string.IsNullOrEmpty(dataMergeDto.At))
            //        {
            //            atValue = " = " + dataMergeDto.At;
            //        }
            //        results.Add(new DebugItem(indexNum.ToString() + " With ", dataMergeDto.MergeType, atValue));
            //        indexNum++;
            //    }
            //}

            return results;
        }

        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> results = new List<IDebugItem>();
            //BUG 8104 : Refactor DebugItem
            //string theValue;
            //if (DataListUtil.IsValueRecordset(Result) && DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
            //{
            //    int indexNum = 1;
            //    var fieldName = DataListUtil.ExtractFieldNameFromValue(Result);
            //    var recset = GetRecordSet(dataList, Result);
            //    var idxItr = recset.FetchRecordsetIndexes();
            //    while (idxItr.HasMore())
            //    {
            //        string error;
            //        var index = idxItr.FetchNextIndex();
            //        var record = recset.FetchRecordAt(index, out error);
            //        // ReSharper disable LoopCanBeConvertedToQuery
            //        foreach (var recordField in record)
            //        // ReSharper restore LoopCanBeConvertedToQuery
            //        {
            //            if (string.IsNullOrEmpty(fieldName) || recordField.FieldName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
            //            {
            //                results.Add(new DebugItem(indexNum.ToString(), DataListUtil.AddBracketsToValueIfNotExist(recordField.DisplayValue), " = " + recordField.TheValue)
            //                {
            //                    Group = Result
            //                });
            //            }
            //        }
            //        indexNum++;
            //    }
            //}
            //else
            //{
            //    theValue = GetValue(dataList, Result);
            //    results.Add(new DebugItem(string.Empty, Result, "= " + theValue));
            //}


            return results;
        }


        #endregion

        #region Get ForEach Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach (Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                var items = MergeCollection.Where(c => !string.IsNullOrEmpty(c.InputVariable) && c.InputVariable.Equals(t.Item1));

                // issues updates
                foreach (var a in items)
                {
                    a.InputVariable = t.Item2;
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates.Count == 1)
            {
                Result = updates[0].Item2;
            }
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            var items = (MergeCollection.Where(c => !string.IsNullOrEmpty(c.InputVariable)).Select(c => c.InputVariable).Union(MergeCollection.Where(c => !string.IsNullOrEmpty(c.At)).Select(c => c.At))).ToArray();
            return GetForEachItems(context, StateType.Before, items);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            var items = new string[1];
            if (!string.IsNullOrEmpty(Result))
            {
                items[0] = Result;
            }
            return GetForEachItems(context, StateType.After, items);
        }

        #endregion

    }
}
