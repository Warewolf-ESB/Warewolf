using Dev2;
using Dev2.Activities;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using System;
using System.Activities;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    /// <New>
    /// Activity for finding records accoring to a search criteria that the user specifies
    /// </New>
    public class DsfFindRecordsActivity : DsfActivityAbstract<string>, IRecsetSearch
    {
        #region Properties

        /// <summary>
        /// Property for holding a string the user enters into the "In Fields" box
        /// </summary>
        [Inputs("FieldsToSearch")]
        public string FieldsToSearch { get; set; }

        /// <summary>
        /// Property for holding a string the user selects in the "Where" drop down box
        /// </summary>
        [Inputs("SearchType")]
        public string SearchType { get; set; }

        /// <summary>
        /// Property for holding a string the user enters into the "Match" box
        /// </summary>
        [Inputs("SearchCriteria")]
        public string SearchCriteria { get; set; }

        /// <summary>
        /// Property for holding a string the user enters into the "Result" box
        /// </summary>
        [Outputs("Result")]
        public new string Result { get; set; }

        /// <summary>
        /// Property for holding a string the user enters into the "Start Index" box
        /// </summary>
        [Inputs("StartIndex")]
        public string StartIndex { get; set; }

        /// <summary>
        /// Property for holding a bool the user chooses with the "MatchCase" Checkbox
        /// </summary>
        [Inputs("MatchCase")]
        public bool MatchCase { get; set; }

        #endregion Properties

        #region Ctor

        public DsfFindRecordsActivity()
            : base("Find Record Index")
        {
            // Initialise all the properties here
            FieldsToSearch = string.Empty;
            SearchType = string.Empty;
            SearchCriteria = string.Empty;
            Result = string.Empty;
            StartIndex = string.Empty;

        }

        #endregion Ctor

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        /// <summary>
        /// Executes the logic of the activity and calls the backend code to do the work
        /// Also responsible for adding the results to the data list
        /// </summary>
        /// <param name="context"></param>
        protected override void OnExecute(NativeActivityContext context)
        {

            IDataListBinder binder = context.GetExtension<IDataListBinder>();
            IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            string error = string.Empty;
            Guid executionID = DataListExecutionID.Get(context);

            try
            {

                // Fetch all fields to search....
                IList<string> toSearch = FieldsToSearch.Split(',');
                // now process each field for entire evaluated Where expression....
                IBinaryDataListEntry bdle = compiler.Evaluate(executionID, enActionType.User, SearchCriteria, false, out errors);
                allErrors.MergeErrors(errors);

                if (bdle != null)
                {
                    IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(bdle);
                    IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                    int idx = 1;
                    if (!Int32.TryParse(StartIndex, out idx))
                    {
                        idx = 1;
                    }
                    IBinaryDataList toSearchList = compiler.FetchBinaryDataList(executionID, out errors);
                    allErrors.MergeErrors(errors);

                    IList<string> foundResults = new List<string>();

                    foreach (string s in toSearch)
                    {
                        // each entry in the recordset
                        while (itr.HasMoreRecords())
                        {
                            IList<IBinaryDataListItem> cols = itr.FetchNextRowData();
                            foreach (IBinaryDataListItem c in cols)
                            {
                                IRecsetSearch searchTO = ConvertToSearchTO(c.TheValue, idx.ToString());
                                IList<string> results = RecordsetInterrogator.FindRecords(toSearchList, searchTO, out errors);
                                allErrors.MergeErrors(errors);
                                foreach (string r in results)
                                {
                                    toUpsert.Add(Result, r);
                                    foundResults.Add(r);
                                    toUpsert.FlushIterationFrame();
                                }
                            }
                        }

                    }

                    // now push the result to the server
                    compiler.Upsert(executionID, toUpsert, out errors);
                    allErrors.MergeErrors(errors);

                    compiler.Shape(executionID, enDev2ArgumentType.Output, OutputMapping, out errors);
                    allErrors.MergeErrors(errors);
                }
            }
            finally
            {

                if (allErrors.HasErrors())
                {
                    string err = DisplayAndWriteError("DsfFindRecordsActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, err, out errors);
                }
            }

        }
        #region Private Methods
        /// <summary>
        /// Creates a new instance of the SearchTO object
        /// </summary>
        /// <returns></returns>
        private IRecsetSearch ConvertToSearchTO(string searchCriteria, string startIndex)
        {
            return DataListFactory.CreateSearchTO(FieldsToSearch, SearchType, searchCriteria, startIndex, Result, MatchCase);
        }
        #endregion Private Methods


        #region Get Debug Inputs/Outputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            var result = new List<IDebugItem>();

            IList<string> fieldsList = FieldsToSearch.Split(',');
            if (fieldsList.Count > 0)
            {
                var theValue = GetValue(dataList, fieldsList[0]);
                result.Add(new DebugItem("Lookin", "", ""));
                foreach (string field in fieldsList)
                {
                    if (DataListUtil.IsValueRecordset(field))
                    {
                        var fieldName = DataListUtil.ExtractFieldNameFromValue(field);
                        var recset = GetRecordSet(dataList, field);
                        var idxItr = recset.FetchRecordsetIndexes();
                        while (idxItr.HasMore())
                        {
                            string error;
                            var index = idxItr.FetchNextIndex();
                            var record = recset.FetchRecordAt(index, out error);
                            // ReSharper disable LoopCanBeConvertedToQuery
                            foreach (var recordField in record)
                            // ReSharper restore LoopCanBeConvertedToQuery
                            {
                                if (string.IsNullOrEmpty(fieldName) || recordField.FieldName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    result.Add(new DebugItem(index, recordField)
                                    {
                                        Group = field
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        theValue = GetValue(dataList, field);
                        result.Add(new DebugItem("", field, "= " + theValue));
                    }
                }
            }

            string searchValue = SearchCriteria;
            if (SearchCriteria.ContainsSafe("[["))
            {
                if (DataListUtil.IsValueRecordset(searchValue) && DataListUtil.GetRecordsetIndexType(searchValue) == enRecordsetIndexType.Star)
                {
                    result.Add(new DebugItem("Find Where " + SearchType, null, null));

                    var fieldName = DataListUtil.ExtractFieldNameFromValue(searchValue);
                    var recset = GetRecordSet(dataList, searchValue);
                    var idxItr = recset.FetchRecordsetIndexes();
                    while (idxItr.HasMore())
                    {
                        string error;
                        var index = idxItr.FetchNextIndex();
                        var record = recset.FetchRecordAt(index, out error);
                        // ReSharper disable LoopCanBeConvertedToQuery
                        foreach (var recordField in record)
                        // ReSharper restore LoopCanBeConvertedToQuery
                        {
                            if (string.IsNullOrEmpty(fieldName) || recordField.FieldName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                result.Add(new DebugItem(index, recordField)
                                {
                                    Group = searchValue
                                });
                            }
                        }
                    }
                }
                else
                {
                    searchValue = GetValue(dataList, SearchCriteria);
                    result.Add(new DebugItem("Find Where " + SearchType, SearchCriteria, "= " + searchValue));
                }
            }
            else
            {
                result.Add(new DebugItem("Find Where " + SearchType, null, searchValue));
            }

            return result;
        }

        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> results = new List<IDebugItem>();
            string theValue;
            if (DataListUtil.IsValueRecordset(Result) && DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
            {
                int indexNum = 1;
                var fieldName = DataListUtil.ExtractFieldNameFromValue(Result);
                var recset = GetRecordSet(dataList, Result);
                var idxItr = recset.FetchRecordsetIndexes();
                while (idxItr.HasMore())
                {
                    string error;
                    var index = idxItr.FetchNextIndex();
                    var record = recset.FetchRecordAt(index, out error);
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var recordField in record)
                    // ReSharper restore LoopCanBeConvertedToQuery
                    {
                        if (string.IsNullOrEmpty(fieldName) || recordField.FieldName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            results.Add(new DebugItem(indexNum.ToString(), DataListUtil.AddBracketsToValueIfNotExist(recordField.DisplayValue), " = " + recordField.TheValue)
                            {
                                Group = Result
                            });
                        }
                    }
                    indexNum++;
                }
            }
            else
            {
                theValue = GetValue(dataList, Result);
                results.Add(new DebugItem(string.Empty, Result, "= " + theValue));
            }


            return results;
        }

        #endregion Get Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates.Count == 1)
            {
                FieldsToSearch = updates[0].Item2;
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates.Count == 1)
            {
                Result = updates[0].Item2;
            }
        }


        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.Before, FieldsToSearch);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.After, Result);
        }

        #endregion
    }
}
