using System.Globalization;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Util;
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
        [FindMissing]
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
        [FindMissing]
        public string SearchCriteria { get; set; }

        /// <summary>
        /// Property for holding a string the user enters into the "Result" box
        /// </summary>
        [Outputs("Result")]
        [FindMissing]
        public new string Result { get; set; }

        /// <summary>
        /// Property for holding a string the user enters into the "Start Index" box
        /// </summary>
        [Inputs("StartIndex")]
        [FindMissing]
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
            SearchType = "<";
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
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();            
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            Guid executionID = dataObject.DataListID;

            try
            {

                // Fetch all fields to search....
                IList<string> toSearch = FieldsToSearch.Split(',');
                // now process each field for entire evaluated Where expression....
                IBinaryDataListEntry bdle = compiler.Evaluate(executionID, enActionType.User, SearchCriteria, false, out errors);
                if (dataObject.IsDebugMode())
                {
                    var itemToAdd = new DebugItem();
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Fields To Search" });
                    _debugInputs.Add(itemToAdd);
                }
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
                    int iterationIndex = 0;
                    foreach (string s in toSearch)
                    {
                        if (dataObject.IsDebug || dataObject.RemoteInvoke)
                        {
                            IBinaryDataListEntry tmpEntry = compiler.Evaluate(executionID, enActionType.User, s, false, out errors);
                            AddDebugInputItem(s, string.Empty, tmpEntry, executionID);
                        }
                        // each entry in the recordset
                        while (itr.HasMoreRecords())
                        {
                            IList<IBinaryDataListItem> cols = itr.FetchNextRowData();
                            foreach (IBinaryDataListItem c in cols)
                            {
                                IRecsetSearch searchTO = ConvertToSearchTO(c.TheValue, idx.ToString(CultureInfo.InvariantCulture));
                                IList<string> results = RecordsetInterrogator.FindRecords(toSearchList, searchTO, out errors);
                                allErrors.MergeErrors(errors);
                                string concatRes = string.Empty;
                                
                                foreach (string r in results)
                                {
                                    concatRes = string.Concat(concatRes, r, ",");
                                }

                                if (concatRes.EndsWith(","))
                                {
                                    concatRes = concatRes.Remove(concatRes.Length - 1);
                                }
                                    //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
                                List<string> regions = DataListCleaningUtils.SplitIntoRegions(Result);
                                //2013.06.07: Massimo Guerrera for BUG 9497 - To handle putting out to a scalar as a CSV
                                foreach (var region in regions)
                                {
                                    if (!DataListUtil.IsValueRecordset(region))
                                    {
                                        toUpsert.Add(region, concatRes);
                                        toUpsert.FlushIterationFrame();
                                        if (dataObject.IsDebug)
                                        {
                                            AddDebugOutputItem(region, concatRes, executionID, iterationIndex);
                                        }
                                    }
                                    else
                                    {
                                        iterationIndex = 0;

                                        foreach (string r in results)
                                        {
                                            toUpsert.Add(region, r);
                                            toUpsert.FlushIterationFrame();
                                            if (dataObject.IsDebug)
                                            {
                                                AddDebugOutputItem(region, r, executionID, iterationIndex);
                                            }
                                    
                                            iterationIndex++;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (dataObject.IsDebugMode())
                    {
                        AddDebugInputItem(SearchCriteria, "Where", bdle, executionID);
                    }

                    // now push the result to the server
                    compiler.Upsert(executionID, toUpsert, out errors);
                    allErrors.MergeErrors(errors);                   
                }
            }
            finally
            {

                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfFindRecordsActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }

                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }

        }
        #region Private Methods

        private void AddDebugInputItem(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            DebugItem itemToAdd = new DebugItem();
            
            if (labelText == "Where")
            {
                itemToAdd.Add(new DebugItemResult() { Type = DebugItemResultType.Label, Value = "Where" });
                itemToAdd.Add(new DebugItemResult() { Type = DebugItemResultType.Value, Value = SearchType });
                itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input));
                _debugInputs.Add(itemToAdd);
                return;
            }

            
            if (!string.IsNullOrWhiteSpace(labelText))
            {
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = labelText });
            }

            if (valueEntry != null)
            {
                itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input));
            }

            _debugInputs.Add(itemToAdd);
        }

        private void AddDebugOutputItem(string expression, string value, Guid dlId, int iterationIndex)
        {
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.AddRange(CreateDebugItemsFromString(expression, value, dlId, iterationIndex, enDev2ArgumentType.Output));
            _debugOutputs.Add(itemToAdd);
        }

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

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
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
