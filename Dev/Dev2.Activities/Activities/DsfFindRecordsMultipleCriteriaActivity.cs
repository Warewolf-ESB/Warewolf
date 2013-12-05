using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Data.Factories;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    /// <New>
    /// Activity for finding records accoring to a search criteria that the user specifies
    /// </New>
    public class DsfFindRecordsMultipleCriteriaActivity : DsfActivityAbstract<string>,ICollectionActivity
    {
        IList<FindRecordsTO> _resultsCollection;

        #region Properties

        /// <summary>
        /// Property for holding a string the user enters into the "In Fields" box
        /// </summary>
        [Inputs("FieldsToSearch")]
        [FindMissing]
        public string FieldsToSearch { get; set; }

       
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

        public bool RequireAllTrue { get; set; }

        public bool RequireAllFieldsToMatch { get; set; }
        #endregion Properties

        #region Ctor

        public DsfFindRecordsMultipleCriteriaActivity()
            : base("Find Record Index")
        {
            // Initialise all the properties here
            _resultsCollection = new List<FindRecordsTO>();
            FieldsToSearch = string.Empty;
            Result = string.Empty;
            StartIndex = string.Empty;
            RequireAllTrue = true;
            RequireAllFieldsToMatch = false;
        }

        #endregion Ctor

        /// <summary>
        ///     Executes the logic of the activity and calls the backend code to do the work
        ///     Also responsible for adding the results to the data list
        /// </summary>
        /// <param name="context"></param>
        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            var compiler = DataListFactory.CreateDataListCompiler();
            var dataObject = context.GetExtension<IDSFDataObject>();
            var errorResultTO = new ErrorResultTO();
            var allErrors = new ErrorResultTO();
            var executionID = dataObject.DataListID;
            try
            {
                IList<string> toSearch = FieldsToSearch.Split(',');
                AddDebugInputValues(dataObject, toSearch, compiler, executionID, ref errorResultTO);
                allErrors.MergeErrors(errorResultTO);
                IEnumerable<string> results = new List<string>();
                var concatRes = string.Empty;
                var toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                var iterationIndex = 0;
                for(var i = 0; i < ResultsCollection.Count; i++)
                {
                    IBinaryDataListEntry binaryDataListEntry = compiler.Evaluate(executionID, enActionType.User, ResultsCollection[i].SearchCriteria, false, out errorResultTO);
                    IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(binaryDataListEntry);

                    int idx;
                    if(!Int32.TryParse(StartIndex, out idx))
                    {
                        idx = 1;
                    }
                    var toSearchList = compiler.FetchBinaryDataList(executionID, out errorResultTO);
                    allErrors.MergeErrors(errorResultTO);

                    var currentResults = results as IList<string> ?? results.ToList();

                    var searchType = ResultsCollection[i].SearchType;
                    var from = ResultsCollection[i].From;
                    var to = ResultsCollection[i].To;
                    if(string.IsNullOrEmpty(searchType))
                    {
                        continue;
                    }
                    while(itr.HasMoreRecords())
                    {
                        var cols = itr.FetchNextRowData();
                        foreach(var c in cols)
                        {
                            var searchTO = ConvertToSearchTO(c.TheValue, searchType, idx.ToString(CultureInfo.InvariantCulture),from,to);
                            var iterationResults = RecordsetInterrogator.FindRecords(toSearchList, searchTO, out errorResultTO);
                            allErrors.MergeErrors(errorResultTO);
                            if(RequireAllTrue)
                            {
                                results = i == 0 ? iterationResults : currentResults.Intersect(iterationResults);
                            }
                            else
                            {
                                results = currentResults.Union(iterationResults);
                            }
                        }
                    }
                }

                var regions = DataListCleaningUtils.SplitIntoRegions(Result);
                foreach(var region in regions)
                {
                    var allResults = results as IList<string> ?? results.ToList();
                    if(!DataListUtil.IsValueRecordset(region))
                    {
                        foreach(var r in allResults)
                        {
                            concatRes = string.Concat(concatRes, r, ",");
                        }

                        if(concatRes.EndsWith(","))
                        {
                            concatRes = concatRes.Remove(concatRes.Length - 1);
                        }
                        toUpsert.Add(region, concatRes);
                        toUpsert.FlushIterationFrame();
                        if(dataObject.IsDebug)
                        {
                            AddDebugOutputItem(region, concatRes, executionID, iterationIndex);
                        }
                    }
                    else
                    {
                        iterationIndex = 0;

                        foreach(var r in allResults)
                        {
                            toUpsert.Add(region, r);
                            toUpsert.FlushIterationFrame();
                            if(dataObject.IsDebug)
                            {
                                AddDebugOutputItem(region, r, executionID, iterationIndex);
                            }

                            iterationIndex++;
                        }
                    }
                    compiler.Upsert(executionID, toUpsert, out errorResultTO);
                    allErrors.MergeErrors(errorResultTO);
                }
            }
            finally
            {
                if(allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfFindRecordsMultipleCriteriaActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errorResultTO);
                }

                if(dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        void AddDebugInputValues(IDSFDataObject dataObject, IEnumerable<string> toSearch, IDataListCompiler compiler, Guid executionID, ref ErrorResultTO errorTos)
        {
            if(dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
            {
                var itemToAdd = new DebugItem();
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Fields To Search" });
                _debugInputs.Add(itemToAdd);
                foreach(var s in toSearch)
                {
                    var searchFields = s;
                    if(DataListUtil.IsValueRecordset(s))
                    {
                        searchFields = searchFields.Replace("()", "(*)");
                    }
                    IBinaryDataListEntry tmpEntry = compiler.Evaluate(executionID, enActionType.User, searchFields, false, out errorTos);
                    AddDebugInputItem(searchFields, string.Empty, tmpEntry, executionID);
                }
                AddRequireAllFieldsToMatchDebug();
                AddResultDebugInputs(ResultsCollection, executionID, compiler);
                AddRequireAllToBeTrueDebugItem();
            }
        }

        void AddRequireAllToBeTrueDebugItem()
        {
            var itemToAdd = new DebugItem();
            var debugItemResult = new DebugItemResult { Type = DebugItemResultType.Label, Value = "Require All Matches To Be True" };
            var requireAllToMatch = RequireAllTrue ? "YES" : "NO";
            var debugItemResult2 = new DebugItemResult { Type = DebugItemResultType.Variable, Value = requireAllToMatch };
            itemToAdd.Add(debugItemResult);
            itemToAdd.Add(debugItemResult2);
            _debugInputs.Add(itemToAdd);
        }

        void AddRequireAllFieldsToMatchDebug()
        {
            var itemToAdd = new DebugItem();
            var debugItemResult = new DebugItemResult { Type = DebugItemResultType.Label, Value = "Require All Fields To Match" };
            var requireAllFieldsToMatch = RequireAllFieldsToMatch ? "YES" : "NO";
            var debugItemResult2 = new DebugItemResult { Type = DebugItemResultType.Variable, Value = requireAllFieldsToMatch };
            itemToAdd.Add(debugItemResult);
            itemToAdd.Add(debugItemResult2);
            _debugInputs.Add(itemToAdd);
        }

        #region Private Methods

        void AddResultDebugInputs(IEnumerable<FindRecordsTO> resultsCollection, Guid executionID, IDataListCompiler compiler)
        {
            var itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Where" });
            _debugInputs.Add(itemToAdd);
            var indexCount = 1;
            foreach (var findRecordsTO in resultsCollection)
            {
                if (!String.IsNullOrEmpty(findRecordsTO.SearchType))
                {
                    itemToAdd = new DebugItem();
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = indexCount.ToString(CultureInfo.InvariantCulture) });
                    itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = findRecordsTO.SearchType });
                    if(!String.IsNullOrEmpty(findRecordsTO.SearchCriteria))
                    {
                        var expressionsEntry = compiler.Evaluate(executionID, enActionType.User, findRecordsTO.SearchCriteria, false, out errors);
                        itemToAdd.AddRange(CreateDebugItemsFromEntry(findRecordsTO.SearchCriteria, expressionsEntry, executionID, enDev2ArgumentType.Input));
                    }
                    _debugInputs.Add(itemToAdd);
                    indexCount++;
                }
            }

        }

        private void AddDebugInputItem(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            var itemToAdd = new DebugItem();
            
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
            var itemToAdd = new DebugItem();
            itemToAdd.AddRange(CreateDebugItemsFromString(expression, value, dlId, iterationIndex, enDev2ArgumentType.Output));
            _debugOutputs.Add(itemToAdd);
        }

        /// <summary>
        /// Creates a new instance of the SearchTO object
        /// </summary>
        /// <returns></returns>
        private IRecsetSearch ConvertToSearchTO(string searchCriteria,string searchType, string startIndex,string from,string to)
        {
            return DataListFactory.CreateSearchTO(FieldsToSearch, searchType, searchCriteria, startIndex, Result, MatchCase,RequireAllFieldsToMatch,from, to);
        }

        void InsertToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ResultsCollection"];
            if (modelProperty == null)
            {
                return;
            }
            var mic = modelProperty.Collection;

            if (mic == null)
            {
                return;
            }
            var listOfValidRows = ResultsCollection.Where(c => !c.CanRemove()).ToList();
            if (listOfValidRows.Count > 0)
            {
                var startIndex = ResultsCollection.Last(c => !c.CanRemove()).IndexNumber;
                foreach (var s in listToAdd)
                {
                    mic.Insert(startIndex, new FindRecordsTO(s, ResultsCollection[startIndex - 1].SearchType, startIndex + 1));
                    startIndex++;
                }
                CleanUpCollection(mic, modelItem, startIndex);
            }
            else
            {
                AddToCollection(listToAdd, modelItem);
            }
        }

        void AddToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ResultsCollection"];
            if (modelProperty == null)
            {
                return;
            }
            var mic = modelProperty.Collection;

            if (mic == null)
            {
                return;
            }
            var startIndex = 0;
            var searchType = ResultsCollection[0].SearchType;
            mic.Clear();
            foreach (var s in listToAdd)
            {
                mic.Add(new FindRecordsTO(s, searchType, startIndex + 1));
                startIndex++;
            }
            CleanUpCollection(mic, modelItem, startIndex);
        }

        void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
        {
            if (startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }
            mic.Add(new XPathDTO(string.Empty, "", startIndex + 1));
            var modelProperty = modelItem.Properties["DisplayName"];
            if (modelProperty != null)
            {
                modelProperty.SetValue(CreateDisplayName(modelItem, startIndex + 1));
            }
        }

        string CreateDisplayName(ModelItem modelItem, int count)
        {
            var modelProperty = modelItem.Properties["DisplayName"];
            if (modelProperty == null)
            {
                return "";
            }
            var currentName = modelProperty.ComputedValue as string;
            if (currentName != null && (currentName.Contains("(") && currentName.Contains(")")))
            {
                currentName = currentName.Remove(currentName.Contains(" (") ? currentName.IndexOf(" (", StringComparison.Ordinal) : currentName.IndexOf("(", StringComparison.Ordinal));
            }
            currentName = currentName + " (" + (count - 1) + ")";
            return currentName;
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

        #region Get ForEach Inputs/Ouputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach (Tuple<string, string> t in updates)
                {
                    // locate all updates for this tuple
                    Tuple<string, string> t1 = t;
                    var items = ResultsCollection.Where(c => !string.IsNullOrEmpty(c.SearchCriteria) && c.SearchCriteria.Equals(t1.Item1));

                    // issues updates
                    foreach (var a in items)
                    {
                        a.SearchCriteria = t.Item2;
                    }

                    if (FieldsToSearch == t.Item1)
                    {
                        FieldsToSearch = t.Item2;
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach (var t in updates)
                {
                    if(Result == t.Item1)
                    {
                        Result = t.Item2;
                    }
                }
            }
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var items = (new[] { FieldsToSearch }).Union(ResultsCollection.Where(c => !string.IsNullOrEmpty(c.SearchCriteria)).Select(c => c.SearchCriteria)).ToArray();
            return GetForEachItems(items);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            var items = Result;
            return GetForEachItems(items);
        }

        #endregion

        #region Implementation of ICollectionActivity

        public int GetCollectionCount()
        {
            return ResultsCollection.Count(findRecordsTO => !findRecordsTO.CanRemove());
        }

        public IList<FindRecordsTO> ResultsCollection
        {
            get
            {
                return _resultsCollection;
            }
            set
            {
                _resultsCollection = value;
            }
        }

        public void AddListToCollection(IList<string> listToAdd, bool overwrite, ModelItem modelItem)
        {
            if (!overwrite)
            {
                InsertToCollection(listToAdd, modelItem);
            }
            else
            {
                AddToCollection(listToAdd, modelItem);
            }
        }

        #endregion
    }
}