using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Converters;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Interfaces;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfCaseConvertActivity : DsfActivityAbstract<string>
    {

        #region Properties

        /// <summary>
        /// The property that holds all the convertions
        /// </summary>
        public IList<ICaseConvertTO> ConvertCollection { get; set; }

        #endregion Properties

        #region Ctor

        /// <summary>
        /// The consructor for the activity 
        /// </summary>
        public DsfCaseConvertActivity()
            : base("Case Conversion")
        {
            ConvertCollection = new List<ICaseConvertTO>();
        }

        #endregion Ctor

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        /// <summary>
        /// The execute method that is called when the activity is executed at run time and will hold all the logic of the activity
        /// </summary>       
        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();

            Guid dlID = dataObject.DataListID;
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = DataListExecutionID.Get(context);

            try
            {
                CleanArgs();
                //IList<string> expression = new List<string>();
                //IList<IBinaryDataListEntry> values = new List<IBinaryDataListEntry>();

                Dev2BaseConversionFactory fac = new Dev2BaseConversionFactory();

                ICaseConverter converter = CaseConverterFactory.CreateCaseConverter();
                string error = string.Empty;

                allErrors.MergeErrors(errors);

                foreach (ICaseConvertTO item in ConvertCollection)
                {
                    IBinaryDataListEntry tmp = compiler.Evaluate(executionId, enActionType.User, item.StringToConvert, false, out errors);

                    if (tmp != null)
                    {

                        IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(tmp);

                        string expression = string.Empty;
                        int indexToUpsertTo = 1;
                        while (itr.HasMoreRecords())
                        {

                            foreach (IBinaryDataListItem itm in itr.FetchNextRowData())
                            {

                                IBinaryDataListItem res = converter.TryConvert(item.ConvertType, itm);
                                //if (tmp.IsRecordset && DataListUtil.GetRecordsetIndexType(item.StringToConvert) == enRecordsetIndexType.Star)
                                if (DataListUtil.IsValueRecordset(item.Result) && DataListUtil.GetRecordsetIndexType(item.Result) == enRecordsetIndexType.Star)
                                {
                                    expression = item.Result.Replace(GlobalConstants.StarExpression, indexToUpsertTo.ToString());
                                }
                                else
                                {
                                    expression = item.Result;
                                }
                                toUpsert.Add(expression, res.TheValue);

                            }
                            indexToUpsertTo++;
                        }

                        // Upsert the entire payload
                        compiler.Upsert(executionId, toUpsert, out errors);
                        allErrors.MergeErrors(errors);

                        compiler.Shape(executionId, enDev2ArgumentType.Output, OutputMapping, out errors);
                        allErrors.MergeErrors(errors);

                    }
                }
            }
            finally
            {

                // Handle Errors
                if (allErrors.HasErrors())
                {
                    string err = DisplayAndWriteError("DsfCaseConvertActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, err, out errors);
                }
            }
        }

        #region Private Methods

        private void CleanArgs()
        {
            int count = 0;
            while (count < ConvertCollection.Count)
            {
                if (string.IsNullOrWhiteSpace(ConvertCollection[count].StringToConvert))
                {
                    ConvertCollection.RemoveAt(count);
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
            string error = string.Empty;
            IBinaryDataList result = Dev2BinaryDataListFactory.CreateDataList();
            string recordsetName = "ConvertCollection";
            result.TryCreateRecordsetTemplate(recordsetName, string.Empty, new List<Dev2Column>() { DataListFactory.CreateDev2Column("StringToConvert", string.Empty), DataListFactory.CreateDev2Column("ConvertType", string.Empty), DataListFactory.CreateDev2Column("Result", string.Empty) }, true, out error);
            foreach (CaseConvertTO item in ConvertCollection)
            {
                result.TryCreateRecordsetValue(item.StringToConvert, "StringToConvert", recordsetName, item.IndexNumber, out error);
                result.TryCreateRecordsetValue(item.ConvertType, "ConvertType", recordsetName, item.IndexNumber, out error);
                result.TryCreateRecordsetValue(item.Result, "Result", recordsetName, item.IndexNumber, out error);
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
            //foreach (CaseConvertTO caseConvertTo in ConvertCollection)
            //{
            //    if (!caseConvertTo.CanRemove())
            //    {
            //        string theValue;
            //        if (DataListUtil.IsValueRecordset(caseConvertTo.StringToConvert) && DataListUtil.GetRecordsetIndexType(caseConvertTo.StringToConvert) == enRecordsetIndexType.Star)
            //        {
            //            results.Add(new DebugItem(indexNum.ToString() + " Convert ", null, null));
            //            var fieldName = DataListUtil.ExtractFieldNameFromValue(caseConvertTo.StringToConvert);
            //            var recset = GetRecordSet(dataList, caseConvertTo.StringToConvert);
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
            //                            Group = caseConvertTo.StringToConvert
            //                        });
            //                    }
            //                }
            //            }
            //        }
            //        else
            //        {
            //            theValue = GetValue(dataList, caseConvertTo.StringToConvert);
            //            results.Add(new DebugItem(indexNum.ToString() + " Convert ", caseConvertTo.StringToConvert,
            //                                      "= " + theValue));
            //        }
            //        results.Add(new DebugItem(indexNum.ToString() + " To ", caseConvertTo.ConvertType, null));
            //        indexNum++;
            //    }
            //}

            return results;
        }

        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> results = new List<IDebugItem>();
            //BUG 8104 : Refactor DebugItem
            //foreach (CaseConvertTO caseConvertTo in ConvertCollection)
            //{
            //    string theValue;
            //    if (DataListUtil.IsValueRecordset(caseConvertTo.Result) && DataListUtil.GetRecordsetIndexType(caseConvertTo.Result) == enRecordsetIndexType.Star)
            //    {
            //        int indexNum = 1;
            //        var fieldName = DataListUtil.ExtractFieldNameFromValue(caseConvertTo.Result);
            //        var recset = GetRecordSet(dataList, caseConvertTo.Result);
            //        var idxItr = recset.FetchRecordsetIndexes();
            //        while (idxItr.HasMore())
            //        {
            //            string error;
            //            var index = idxItr.FetchNextIndex();
            //            var record = recset.FetchRecordAt(index, out error);
            //            // ReSharper disable LoopCanBeConvertedToQuery
            //            foreach (var recordField in record)
            //            // ReSharper restore LoopCanBeConvertedToQuery
            //            {
            //                if (string.IsNullOrEmpty(fieldName) || recordField.FieldName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
            //                {
            //                    results.Add(new DebugItem(indexNum.ToString(), DataListUtil.AddBracketsToValueIfNotExist(recordField.DisplayValue), " = " + recordField.TheValue)
            //                        {
            //                            Group = caseConvertTo.Result
            //                        });
            //                }
            //            }
            //            indexNum++;
            //        }
            //    }
            //    else
            //    {
            //        theValue = GetValue(dataList, caseConvertTo.Result);
            //        results.Add(new DebugItem(string.Empty, caseConvertTo.Result, "= " + theValue));
            //    }
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
                var items = ConvertCollection.Where(c => !string.IsNullOrEmpty(c.StringToConvert) && c.StringToConvert.Equals(t.Item1));

                // issues updates
                foreach (var a in items)
                {
                    a.StringToConvert = t.Item2;
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach (Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                var items = ConvertCollection.Where(c => !string.IsNullOrEmpty(c.Result) && c.Result.Equals(t.Item1));

                // issues updates
                foreach (var a in items)
                {
                    a.Result = t.Item2;
                }
            }
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.Before);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.After);
        }

        IList<DsfForEachItem> GetForEachItems(NativeActivityContext context, StateType stateType)
        {
            var items = ConvertCollection.Where(c => !string.IsNullOrEmpty(c.Result)).Select(c => c.Result).ToArray();
            return GetForEachItems(context, stateType, items);
        }
        #endregion
    }
}
