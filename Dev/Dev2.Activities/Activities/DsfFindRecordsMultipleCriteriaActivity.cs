
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
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Util;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Interfaces;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    /// <New>
    /// Activity for finding records accoring to a search criteria that the user specifies
    /// </New>
    public class DsfFindRecordsMultipleCriteriaActivity : DsfActivityAbstract<string>, ICollectionActivity
    {
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
            ResultsCollection = new List<FindRecordsTO>();
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

            var dataObject = context.GetExtension<IDSFDataObject>();
            var env = dataObject.Environment;
            var executionId = dataObject.DataListID;
            InitializeDebug(dataObject);

            try
            {
                IList<string> toSearch = FieldsToSearch.Split(',');
                List<int> results = new List<int>();
                //if(dataObject.IsDebugMode())
                //{
                //    AddDebugInputValues(dataObject, toSearch, compiler, executionId, ref errorResultTo);
                //}
                foreach(var searchvar in toSearch)
                {
                    Func<DataASTMutable.WarewolfAtom, bool> func = null;
                    foreach(FindRecordsTO to in ResultsCollection.Where(a=> !String.IsNullOrEmpty(a.SearchType)))
                    {
                        var right = env.EvalAsList(to.SearchCriteria);
                        IEnumerable<DataASTMutable.WarewolfAtom> from = new List<DataASTMutable.WarewolfAtom>();
                        IEnumerable<DataASTMutable.WarewolfAtom> tovalue = new List<DataASTMutable.WarewolfAtom>(); 
                        if(!String.IsNullOrEmpty(to.From))
                         from = env.EvalAsList(to.From);
                        if (!String.IsNullOrEmpty(to.To))
                            tovalue = env.EvalAsList(to.To);
                        if (func == null)
                        {
                            func = CreateFuncFromOperator(to.SearchType, right, from,tovalue ,MatchCase);
                        }
                        else
                        {
                            if(RequireAllTrue)
                                func = CombineFuncAnd(func, to.SearchType, right, from, tovalue, MatchCase);
                            else
                            {
                                func = CombineFuncOr(func, to.SearchType, right, from, tovalue, MatchCase); 
                            }
                        }
                        
                       
   
                    }
                    var output = env.EnvalWhere(dataObject.Environment.ToStar(searchvar), func);
                    results.AddRange(output);
                   
                }
                var res =String.Join(",", results);
                env.Assign(Result, res);
                if(dataObject.IsDebugMode())
                {
                    AddDebugOutputItem(new DebugItemWarewolfAtomResult(String.Join(",",env.EvalAsListOfStrings(Result)),"Result"));
                }
                //IEnumerable<string> resultsDuringSearch = new List<string>();
                //var currenSearchResults = new List<string>();
                //IDev2IteratorCollection itrCollection = Dev2ValueObjectFactory.CreateIteratorCollection();

                //IBinaryDataListEntry binaryDataListEntrySearchCrit = compiler.Evaluate(executionId, enActionType.User, ResultsCollection[i].SearchCriteria, false, out errorResultTo);
                //IDev2DataListEvaluateIterator searchCritItr = Dev2ValueObjectFactory.CreateEvaluateIterator(binaryDataListEntrySearchCrit);
                //itrCollection.AddIterator(searchCritItr);
                //allErrors.MergeErrors(errorResultTo);

                //IBinaryDataListEntry binaryDataListEntryFrom = compiler.Evaluate(executionId, enActionType.User, ResultsCollection[i].From, false, out errorResultTo);
                //IDev2DataListEvaluateIterator fromItr = Dev2ValueObjectFactory.CreateEvaluateIterator(binaryDataListEntryFrom);
                //itrCollection.AddIterator(fromItr);
                //allErrors.MergeErrors(errorResultTo);

                //IBinaryDataListEntry binaryDataListEntryTo = compiler.Evaluate(executionId, enActionType.User, ResultsCollection[i].To, false, out errorResultTo);
                //IDev2DataListEvaluateIterator toItr = Dev2ValueObjectFactory.CreateEvaluateIterator(binaryDataListEntryTo);
                //itrCollection.AddIterator(toItr);
                //allErrors.MergeErrors(errorResultTo);

                //int idx;
                //if(!Int32.TryParse(StartIndex, out idx))
                //{
                //    idx = 1;
                //}
                //var toSearchList = compiler.FetchBinaryDataList(executionId, out errorResultTo);
                //allErrors.MergeErrors(errorResultTo);



                //var searchType = ResultsCollection[i].SearchType;
                //if(string.IsNullOrEmpty(searchType))
                //{
                //    continue;
                //}
                //// ReSharper disable PossibleMultipleEnumeration
                //while(itrCollection.HasMoreData())
                //{
                //    var currentResults = results as IList<string> ?? results.ToList();

                //    var splitOn = new[] { "," };
                //    var fieldsToSearch = FieldsToSearch.Split(splitOn, StringSplitOptions.RemoveEmptyEntries);

                //    SearchTO searchTo;

                //    if(fieldsToSearch.Length > 0)
                //    {
                //        bool isFirstFieldIteration = true;
                //        foreach(var field in fieldsToSearch)
                //        {

                //            IList<string> iterationResults = new List<string>();
                //            searchTo = DataListFactory.CreateSearchTO(field, searchType,
                //                                                      itrCollection.FetchNextRow(searchCritItr)
                //                                                                   .TheValue,
                //                                                      idx.ToString(CultureInfo.InvariantCulture),
                //                                                      Result, MatchCase,
                //                                                      false,
                //                                                      itrCollection.FetchNextRow(fromItr).TheValue,
                //                                                      itrCollection.FetchNextRow(toItr).TheValue);
                //            ValidateRequiredFields(searchTo, out errorResultTo);
                //            allErrors.MergeErrors(errorResultTo);

                //            // ReSharper disable ConvertClosureToMethodGroup
                //            (RecordsetInterrogator.FindRecords(new RecordSetSearchPayload[0], searchTo, out errorResultTo)).ToList().ForEach(it => iterationResults.Add(it));
                //            // ReSharper restore ConvertClosureToMethodGroup

                //            if(RequireAllFieldsToMatch)
                //            {
                //                resultsDuringSearch = isFirstFieldIteration ? iterationResults : currenSearchResults.Intersect(iterationResults);
                //                currenSearchResults = resultsDuringSearch.ToList();
                //            }
                //            else
                //            {
                //                resultsDuringSearch = currenSearchResults.Union(iterationResults);
                //                currenSearchResults = RequireAllTrue ? new List<string>() : resultsDuringSearch.ToList();
                //            }
                //            isFirstFieldIteration = false;

                //                }
                //            }
                //            else
                //            {
                //                searchTo = (SearchTO)ConvertToSearchTO(itrCollection.FetchNextRow(searchCritItr).TheValue,
                //                                                 searchType, idx.ToString(CultureInfo.InvariantCulture),
                //                                                 itrCollection.FetchNextRow(fromItr).TheValue,
                //                                                 itrCollection.FetchNextRow(toItr).TheValue);

                //                ValidateRequiredFields(searchTo, out errorResultTo);
                //                allErrors.MergeErrors(errorResultTo);
                //                resultsDuringSearch = RecordsetInterrogator.FindRecords(new RecordSetSearchPayload[0], searchTo, out errorResultTo);

                //            }

                //            allErrors.MergeErrors(errorResultTo);

                //            if(RequireAllTrue)
                //            {
                //                results = isFirstIteration ? resultsDuringSearch : currentResults.Intersect(resultsDuringSearch);
                //            }
                //            else
                //            {
                //                results = currentResults.Union(resultsDuringSearch);
                //            }
                //            isFirstIteration = false;
                //        }
                //    }

                //    DataListCleaningUtils.SplitIntoRegions(Result);
                //    var rule = new IsSingleValueRule(() => Result);
                //    var singleresError = rule.Check();
                //    if(singleresError != null)
                //        allErrors.AddError(singleresError.Message);
                //    else
                //    {

                //        string concatRes = String.Empty;
                //        var allResults = results as IList<string> ?? results.ToList();
                //        // ReSharper restore PossibleMultipleEnumeration
                //        if(allResults.Count == 0)
                //        {
                //            allResults.Add("-1");
                //        }

                //        if(!DataListUtil.IsValueRecordset(Result))
                //        {
                //            // ReSharper disable LoopCanBeConvertedToQuery
                //            foreach(var r in allResults)
                //            // ReSharper restore LoopCanBeConvertedToQuery
                //            {
                //                concatRes = string.Concat(concatRes, r, ",");
                //            }

                //            if(concatRes.EndsWith(","))
                //            {
                //                concatRes = concatRes.Remove(concatRes.Length - 1);
                //            }
                //            toUpsert.Add(Result, concatRes);
                //            toUpsert.FlushIterationFrame();
                //        }
                //        else
                //        {
                //            iterationIndex = 0;

                //            foreach(var r in allResults)
                //            {
                //                toUpsert.Add(Result, r);
                //                toUpsert.FlushIterationFrame();
                //                iterationIndex++;
                //            }
                //        }
                //        compiler.Upsert(executionId, toUpsert, out errorResultTo);
                //        allErrors.MergeErrors(errorResultTo);

                //        if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                //        {
                //            foreach(var debugTo in toUpsert.DebugOutputs)
                //            {
                //                AddDebugOutputItem(new DebugItemVariableParams(debugTo));
                //            }
                //        }

                //    }
                //}
            }
            catch (Exception exception)
            {
                Dev2Logger.Log.Error("DSFRecordsMultipleCriteria", exception);
                //allErrors.AddError(exception.Message);
            }
            finally
            {
                //var hasErrors = allErrors.HasErrors();
                //if(hasErrors)
                //{
                //    DisplayAndWriteError("DsfFindRecordsMultipleCriteriaActivity", allErrors);
                //    //compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errorResultTo);
                //    //compiler.Upsert(executionId, Result, (string)null, out errorResultTo);
                //}

                if(dataObject.IsDebugMode())
                {
                    //if(hasErrors)
                    //{
                    //    iterationIndex = 0;
                    //    var regions = DataListCleaningUtils.SplitIntoRegions(Result);
                    //    foreach(var region in regions)
                    //    {
                    //        AddDebugOutputItem(new DebugOutputParams(region, "", executionId, iterationIndex));
                    //        iterationIndex++;
                    //    }
                    //}

                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        Func<DataASTMutable.WarewolfAtom, bool> CombineFuncAnd(Func<DataASTMutable.WarewolfAtom, bool> func,string searchType,IEnumerable<DataASTMutable.WarewolfAtom> values,IEnumerable<DataASTMutable.WarewolfAtom> from,IEnumerable<DataASTMutable.WarewolfAtom> to, bool matchcase)
        {
            var func2 = CreateFuncFromOperator(searchType, values,from,to, matchcase);
            return new Func<DataASTMutable.WarewolfAtom, bool>(a=> func.Invoke(a) && func2.Invoke(a));
        }

        Func<DataASTMutable.WarewolfAtom, bool> CombineFuncOr(Func<DataASTMutable.WarewolfAtom, bool> func,string searchType,IEnumerable<DataASTMutable.WarewolfAtom> values,IEnumerable<DataASTMutable.WarewolfAtom> from,IEnumerable<DataASTMutable.WarewolfAtom> to, bool matchcase)
        {
            var func2 = CreateFuncFromOperator(searchType, values,from,to, matchcase);
            return new Func<DataASTMutable.WarewolfAtom, bool>(a => func.Invoke(a) || func2.Invoke(a));
            //return new Func<DataASTMutable.WarewolfAtom, bool>((a => { return func.Invoke(a) || func2.Invoke(a); }));
        }


        Func<DataASTMutable.WarewolfAtom,bool> CreateFuncFromOperator(string searchType,IEnumerable<DataASTMutable.WarewolfAtom> values,IEnumerable<DataASTMutable.WarewolfAtom> from,IEnumerable<DataASTMutable.WarewolfAtom> to, bool matchcase )
        {

            IFindRecsetOptions opt = FindRecsetOptions.FindMatch(searchType);
            return opt.GenerateFunc(values, from,to,RequireAllFieldsToMatch);
        }

        private void ValidateRequiredFields(SearchTO searchTo, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            if(string.IsNullOrEmpty(searchTo.FieldsToSearch))
            {
                errors.AddError("Fields to search is required");
            }

            if(string.IsNullOrEmpty(searchTo.SearchType))
            {
                errors.AddError("Search type is required");
            }

            if(searchTo.SearchType.Equals("Is Between"))
            {
                if(string.IsNullOrEmpty(searchTo.From))
                {
                    errors.AddError("From is required");
                }

                if(string.IsNullOrEmpty(searchTo.To))
                {
                    errors.AddError("To is required");
                }
            }
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.MixedActivity;
        }

        void AddDebugInputValues(IDSFDataObject dataObject, IEnumerable<string> toSearch, IDataListCompiler compiler, Guid executionId, ref ErrorResultTO errorTos)
        {
            if(dataObject.IsDebugMode())
            {
                var debugItem = new DebugItem();
                AddDebugItem(new DebugItemStaticDataParams("", "In Field(s)"), debugItem);
                foreach(var s in toSearch)
                {
                    var searchFields = s;
                    if(DataListUtil.IsValueRecordset(s))
                    {
                        searchFields = searchFields.Replace("()", "(*)");
                    }
                    IBinaryDataListEntry tmpEntry = compiler.Evaluate(executionId, enActionType.User, searchFields, false, out errorTos);
                    AddDebugItem(new DebugItemVariableParams(searchFields, "", tmpEntry, executionId), debugItem);
                }
                _debugInputs.Add(debugItem);
                AddResultDebugInputs(ResultsCollection, executionId, compiler);
                AddDebugInputItem(new DebugItemStaticDataParams(RequireAllFieldsToMatch ? "YES" : "NO", "Require All Fields To Match"));
                AddDebugInputItem(new DebugItemStaticDataParams(RequireAllTrue ? "YES" : "NO", "Require All Matches To Be True"));
            }
        }

        #region Overrides of DsfNativeActivity<string>

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env)
        {
            return _debugOutputs;
        }

        #endregion

        #region Private Methods

        void AddResultDebugInputs(IEnumerable<FindRecordsTO> resultsCollection, Guid executionId, IDataListCompiler compiler)
        {
            var indexCount = 1;
            foreach(var findRecordsTo in resultsCollection)
            {
                DebugItem debugItem = new DebugItem();
                if(!String.IsNullOrEmpty(findRecordsTo.SearchType))
                {
                    AddDebugItem(new DebugItemStaticDataParams("", indexCount.ToString(CultureInfo.InvariantCulture)), debugItem);
                    AddDebugItem(new DebugItemStaticDataParams(findRecordsTo.SearchType, ""), debugItem);

                    if(!string.IsNullOrEmpty(findRecordsTo.SearchCriteria))
                    {
                        var expressionsEntry = compiler.Evaluate(executionId, enActionType.User, findRecordsTo.SearchCriteria, false, out errorsTo);
                        AddDebugItem(new DebugItemVariableParams(findRecordsTo.SearchCriteria, "", expressionsEntry, executionId), debugItem);
                    }

                    if(findRecordsTo.SearchType == "Is Between" || findRecordsTo.SearchType == "Not Between")
                    {
                        var expressionsEntryFrom = compiler.Evaluate(executionId, enActionType.User, findRecordsTo.From, false, out errorsTo);
                        AddDebugItem(new DebugItemVariableParams(findRecordsTo.From, "", expressionsEntryFrom, executionId), debugItem);

                        var expressionsEntryTo = compiler.Evaluate(executionId, enActionType.User, findRecordsTo.To, false, out errorsTo);
                        AddDebugItem(new DebugItemVariableParams(findRecordsTo.To, " And", expressionsEntryTo, executionId), debugItem);
                    }

                    _debugInputs.Add(debugItem);
                    indexCount++;
                }
            }
        }

        /// <summary>
        /// Creates a new instance of the SearchTO object
        /// </summary>
        /// <returns></returns>
        // ReSharper disable InconsistentNaming
        private IRecsetSearch ConvertToSearchTO(string searchCriteria, string searchType, string startIndex, string from, string to)
        // ReSharper restore InconsistentNaming
        {
            return DataListFactory.CreateSearchTO(FieldsToSearch, searchType, searchCriteria, startIndex, Result, MatchCase, RequireAllFieldsToMatch, from, to);
        }

        void InsertToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ResultsCollection"];
            if(modelProperty == null)
            {
                return;
            }
            var mic = modelProperty.Collection;

            if(mic == null)
            {
                return;
            }
            var listOfValidRows = ResultsCollection.Where(c => !c.CanRemove()).ToList();
            if(listOfValidRows.Count > 0)
            {
                FindRecordsTO findRecordsTo = ResultsCollection.Last(c => !c.CanRemove());
                var startIndex = ResultsCollection.IndexOf(findRecordsTo) + 1;
                foreach(var s in listToAdd)
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
            if(modelProperty == null)
            {
                return;
            }
            var mic = modelProperty.Collection;

            if(mic == null)
            {
                return;
            }
            var startIndex = 0;
            var searchType = ResultsCollection[0].SearchType;
            mic.Clear();
            foreach(var s in listToAdd)
            {
                mic.Add(new FindRecordsTO(s, searchType, startIndex + 1));
                startIndex++;
            }
            CleanUpCollection(mic, modelItem, startIndex);
        }

        void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
        {
            if(startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }
            mic.Add(new XPathDTO(string.Empty, "", startIndex + 1));
            var modelProperty = modelItem.Properties["DisplayName"];
            if(modelProperty != null)
            {
                modelProperty.SetValue(CreateDisplayName(modelItem, startIndex + 1));
            }
        }

        string CreateDisplayName(ModelItem modelItem, int count)
        {
            var modelProperty = modelItem.Properties["DisplayName"];
            if(modelProperty == null)
            {
                return "";
            }
            var currentName = modelProperty.ComputedValue as string;
            if(currentName != null && (currentName.Contains("(") && currentName.Contains(")")))
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

        #region Get ForEach Inputs/Ouputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {
                    // locate all updates for this tuple
                    Tuple<string, string> t1 = t;
                    var items = ResultsCollection.Where(c => !string.IsNullOrEmpty(c.SearchCriteria) && c.SearchCriteria.Equals(t1.Item1));

                    // issues updates
                    foreach(var a in items)
                    {
                        a.SearchCriteria = t.Item2;
                    }

                    if(FieldsToSearch == t.Item1)
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
                foreach(var t in updates)
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
            return ResultsCollection.Count(findRecordsTo => !findRecordsTo.CanRemove());
        }

        public IList<FindRecordsTO> ResultsCollection { get; set; }

        public void AddListToCollection(IList<string> listToAdd, bool overwrite, ModelItem modelItem)
        {
            if(!overwrite)
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
