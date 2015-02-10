
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.Factories;
using Dev2.Data.Util;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Util;
using Dev2.Utilities;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    /// <New>
    /// Activity for finding records accoring to a search criteria that the user specifies
    /// </New>
    [ToolDescriptorInfo("RecordSet-FindRecords", "Find Records", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Bob", "1.0.0.0", "c:\\", "RecordSet", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]

    public class DsfFindRecordsActivity : DsfActivityAbstract<string>, IRecsetSearch
    {
        #region Fields

        private string _searchType;

        #endregion


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
        public string SearchType
        {
            get
            {
                return _searchType;
            }
            set
            {
                _searchType = FindRecordsDisplayUtil.ConvertForDisplay(value);
            }
        }

        public string From { get; set; }
        public string To { get; set; }

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

        public bool RequireAllFieldsToMatch { get; set; }

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
            RequireAllFieldsToMatch = false;
        }

        #endregion Ctor

        // ReSharper disable RedundantOverridenMember
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }
        // ReSharper restore RedundantOverridenMember

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
            ErrorResultTO errors;
            ErrorResultTO allErrors = new ErrorResultTO();
            Guid executionId = dataObject.DataListID;

            InitializeDebug(dataObject);
            try
            {
                IsSingleValueRule.ApplyIsSingleValueRule(Result, allErrors);
                // Fetch all fields to search....
                IList<string> toSearch = FieldsToSearch.Split(',');
                // now process each field for entire evaluated Where expression....
                IBinaryDataListEntry bdle = compiler.Evaluate(executionId, enActionType.User, SearchCriteria, false, out errors);
                if(dataObject.IsDebugMode())
                {
                    var itemToAdd = new DebugItem();
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Fields To Search" });
                    _debugInputs.Add(itemToAdd);
                }
                allErrors.MergeErrors(errors);

                if(bdle != null)
                {
                    IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(bdle);
                    IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                    int idx;
                    if(!Int32.TryParse(StartIndex, out idx))
                    {
                        idx = 1;
                    }
                    IBinaryDataList toSearchList = compiler.FetchBinaryDataList(executionId, out errors);
                    allErrors.MergeErrors(errors);
                    int iterationIndex = 0;
                    foreach(string s in toSearch)
                    {
                        if(dataObject.IsDebugMode())
                        {
                            IBinaryDataListEntry tmpEntry = compiler.Evaluate(executionId, enActionType.User, s, false, out errors);
                            AddDebugInputItem(s, string.Empty, tmpEntry, executionId);
                        }
                        // each entry in the recordset
                        while(itr.HasMoreRecords())
                        {
                            IList<IBinaryDataListItem> cols = itr.FetchNextRowData();
                            foreach(IBinaryDataListItem c in cols)
                            {
                                IRecsetSearch searchTo = ConvertToSearchTo(c.TheValue, idx.ToString(CultureInfo.InvariantCulture));
                                IList<string> results = RecordsetInterrogator.FindRecords(toSearchList, searchTo, out errors);
                                allErrors.MergeErrors(errors);
                                string concatRes = string.Empty;

                                // ReSharper disable LoopCanBeConvertedToQuery
                                foreach(string r in results)
                                // ReSharper restore LoopCanBeConvertedToQuery
                                {
                                    concatRes = string.Concat(concatRes, r, ",");
                                }

                                if(concatRes.EndsWith(","))
                                {
                                    concatRes = concatRes.Remove(concatRes.Length - 1);
                                }
                                //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
                                DataListCleaningUtils.SplitIntoRegions(Result);
                                //2013.06.07: Massimo Guerrera for BUG 9497 - To handle putting out to a scalar as a CSV

                                if(!DataListUtil.IsValueRecordset(Result))
                                {
                                    toUpsert.Add(Result, concatRes);
                                    toUpsert.FlushIterationFrame();
                                    if(dataObject.IsDebugMode())
                                    {
                                        AddDebugOutputItem(new DebugOutputParams(Result, concatRes, executionId, iterationIndex));
                                    }
                                }
                                else
                                {
                                    iterationIndex = 0;

                                    foreach(string r in results)
                                    {
                                        toUpsert.Add(Result, r);
                                        toUpsert.FlushIterationFrame();
                                        if(dataObject.IsDebugMode())
                                        {
                                            AddDebugOutputItem(new DebugOutputParams(Result, r, executionId, iterationIndex));
                                        }

                                        iterationIndex++;
                                    }

                                }
                            }
                        }
                    }

                    if(dataObject.IsDebugMode())
                    {
                        AddDebugInputItem(SearchCriteria, "Where", bdle, executionId);
                    }

                    // now push the result to the server
                    compiler.Upsert(executionId, toUpsert, out errors);
                    allErrors.MergeErrors(errors);
                }
            }
            finally
            {
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfFindRecordsActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                    compiler.Upsert(executionId, Result, (string)null, out errors);
                }

                if(dataObject.IsDebugMode())
                {
                    if(hasErrors)
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", Result, ""));
                    }
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }

        }
        #region Private Methods

        private void AddDebugInputItem(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            DebugItem itemToAdd = new DebugItem();

            if(labelText == "Where")
            {
                AddDebugItem(new DebugItemStaticDataParams(SearchType, "Where"), itemToAdd);
                AddDebugItem(new DebugItemVariableParams(expression, labelText, valueEntry, executionId), itemToAdd);
                _debugInputs.Add(itemToAdd);
                return;
            }

            if(!string.IsNullOrWhiteSpace(labelText))
            {
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = labelText });
            }

            if(valueEntry != null)
            {
                AddDebugItem(new DebugItemVariableParams(expression, labelText, valueEntry, executionId), itemToAdd);
            }

            _debugInputs.Add(itemToAdd);
        }

        /// <summary>
        /// Creates a new instance of the SearchTO object
        /// </summary>
        /// <returns></returns>
        private IRecsetSearch ConvertToSearchTo(string searchCriteria, string startIndex)
        {
            return DataListFactory.CreateSearchTO(FieldsToSearch, SearchType, searchCriteria, startIndex, Result, MatchCase);
        }

        #endregion Private Methods

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion Get Inputs/Outputs

        /// <summary>
        ///Not covered as this Activity has been deprecated and replaced with the <see cref="DsfFindRecordsMultipleCriteriaActivity"/>
        ///It is here purely for backward compatibility
        /// </summary>
        /// <param name="updates"></param>
        /// <param name="context"></param>
        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates.Count == 1)
            {
                FieldsToSearch = updates[0].Item2;
            }
        }
        /// <summary>
        ///Not covered as this Activity has been deprecated and replaced with the <see cref="DsfFindRecordsMultipleCriteriaActivity"/>
        ///It is here purely for backward compatibility
        /// </summary>
        /// <param name="updates"></param>
        /// <param name="context"></param>
        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                var itemUpdate = updates.FirstOrDefault(tuple => tuple.Item1 == Result);
                if(itemUpdate != null)
                {
                    Result = itemUpdate.Item2;
                }
            }
        }


        #region GetForEachInputs/Outputs
        /// <summary>
        ///Not covered as this Activity has been deprecated and replaced with the <see cref="DsfFindRecordsMultipleCriteriaActivity"/>
        ///It is here purely for backward compatibility
        /// </summary>
        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(FieldsToSearch);
        }
        /// <summary>
        ///Not covered as this Activity has been deprecated and replaced with the <see cref="DsfFindRecordsMultipleCriteriaActivity"/>
        ///It is here purely for backward compatibility
        /// </summary>
        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion
    }
}
