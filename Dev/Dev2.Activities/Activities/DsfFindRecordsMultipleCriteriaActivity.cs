#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using Dev2.Comparer;
using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.State;
using Dev2.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities

{
    /// <New>
    /// Activity for finding records accoring to a search criteria that the user specifies
    /// </New>
    [ToolDescriptorInfo("RecordSet-FindRecords", "Find Records", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Recordset", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Recordset_Find_Records")]
    public class DsfFindRecordsMultipleCriteriaActivity : DsfActivityAbstract<string>, ICollectionActivity, IEquatable<DsfFindRecordsMultipleCriteriaActivity>
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

        public override IEnumerable<StateVariable> GetState()
        {
            return new[] {
                new StateVariable
                {
                    Name = "ResultsCollection",
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(ResultsCollection),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "FieldsToSearch",
                    Value = FieldsToSearch,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "RequireAllTrue",
                    Value = RequireAllTrue.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "RequireAllFieldsToMatch",
                    Value = RequireAllFieldsToMatch.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name="Result",
                    Value = Result,
                    Type = StateVariable.StateType.Output
                }
            };
        }

        public override List<string> GetOutputs() => new List<string> { Result };

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            var searchQuery = new SearchQuery(this, Result, RequireAllFieldsToMatch, RequireAllTrue);

            var allErrors = new ErrorResultTO();
            try
            {
                InitializeDebug(dataObject);
                var searchContext = new SearchContext(this);
                if (dataObject.IsDebugMode())
                {
                    TryAddDebugInputValues(dataObject, searchContext.ToSearch, ref allErrors, update);
                }

                searchQuery.Execute(searchContext, allErrors, dataObject, update);

                if (dataObject.IsDebugMode())
                {
                    if (DataListUtil.IsValueRecordset(Result))
                    {
                        var recVar = DataListUtil.ReplaceRecordsetBlankWithStar(Result);
                        AddDebugOutputItem(new DebugEvalResult(recVar, "", dataObject.Environment, update));
                    }
                    else
                    {
                        AddDebugOutputItem(new DebugEvalResult(Result, "", dataObject.Environment, update));
                    }
                }
            }
            catch (Exception exception)
            {
                Dev2Logger.Error("DSFRecordsMultipleCriteria", exception, GlobalConstants.WarewolfError);
                allErrors.AddError(exception.Message);
            }
            finally
            {
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    DisplayAndWriteError("DsfFindRecordsMultipleCriteriaActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                    dataObject.Environment.Assign(Result, "-1", update);
                    if (dataObject.IsDebugMode())
                    {
                        AddDebugOutputItem(new DebugEvalResult(Result, "", dataObject.Environment, update));
                    }
                }

                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        internal void SetErrorsTO(ErrorResultTO errorTo)
        {
            _errorsTo = errorTo;
        }

        public override enFindMissingType GetFindMissingType() => enFindMissingType.MixedActivity;

        void TryAddDebugInputValues(IDSFDataObject dataObject, IEnumerable<string> toSearch, ref ErrorResultTO errorTos, int update)
        {
            if (dataObject.IsDebugMode())
            {
                try
                {
                    AddDebugInputValues(dataObject, toSearch, update);
                }
                catch (Exception e)
                {
                    errorTos.AddError(e.Message);
                }
            }
        }

        private void AddDebugInputValues(IDSFDataObject dataObject, IEnumerable<string> toSearch, int update)
        {
            var debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "In Field(s)"), debugItem);
            foreach (var s in toSearch)
            {
                var searchFields = s;
                if (DataListUtil.IsValueRecordset(s))
                {
                    searchFields = searchFields.Replace("()", "(*)");
                }
                AddDebugItem(new DebugEvalResult(searchFields, "", dataObject.Environment, update), debugItem);
            }
            _debugInputs.Add(debugItem);
            AddResultDebugInputs(ResultsCollection, dataObject.Environment, update);
            AddDebugInputItem(new DebugItemStaticDataParams(RequireAllFieldsToMatch ? "YES" : "NO", "Require All Fields To Match"));
            AddDebugInputItem(new DebugItemStaticDataParams(RequireAllTrue ? "YES" : "NO", "Require All Matches To Be True"));
        }

        #region Overrides of DsfNativeActivity<string>

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            return _debugOutputs;
        }

        #endregion

        #region Private Methods

        void AddResultDebugInputs(IEnumerable<FindRecordsTO> resultsCollection, IExecutionEnvironment environment, int update)
        {
            var indexCount = 1;
            foreach (var findRecordsTo in resultsCollection)
            {
                var debugItem = new DebugItem();
                if (!String.IsNullOrEmpty(findRecordsTo.SearchType))
                {
                    AddDebugItem(new DebugItemStaticDataParams("", indexCount.ToString(CultureInfo.InvariantCulture)), debugItem);
                    AddDebugItem(new DebugItemStaticDataParams(findRecordsTo.SearchType, ""), debugItem);

                    if (!string.IsNullOrEmpty(findRecordsTo.SearchCriteria))
                    {
                        AddDebugItem(new DebugEvalResult(findRecordsTo.SearchCriteria, "", environment, update, ""), debugItem);
                    }

                    if (findRecordsTo.SearchType == "Is Between" || findRecordsTo.SearchType == "Not Between")
                    {
                        AddDebugItem(new DebugEvalResult(findRecordsTo.From, "", environment, update), debugItem);

                        AddDebugItem(new DebugEvalResult(findRecordsTo.To, " And", environment, update), debugItem);
                    }

                    _debugInputs.Add(debugItem);
                    indexCount++;
                }
            }
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
                var findRecordsTo = ResultsCollection.Last(c => !c.CanRemove());
                var startIndex = ResultsCollection.IndexOf(findRecordsTo) + 1;
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

        static void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
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

        static string CreateDisplayName(ModelItem modelItem, int count)
        {
            var modelProperty = modelItem.Properties["DisplayName"];
            if (modelProperty == null)
            {
                return "";
            }
            var currentName = modelProperty.ComputedValue as string;
            if (currentName != null && currentName.Contains("(") && currentName.Contains(")"))
            {
                currentName = currentName.Remove(currentName.Contains(" (") ? currentName.IndexOf(" (", StringComparison.Ordinal) : currentName.IndexOf("(", StringComparison.Ordinal));
            }
            currentName = currentName + " (" + (count - 1) + ")";
            return currentName;
        }

        #endregion Private Methods

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }


        #endregion Get Inputs/Outputs

        #region Get ForEach Inputs/Ouputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if (updates != null)
            {
                foreach (Tuple<string, string> t in updates)
                {
                    // locate all updates for this tuple
                    var t1 = t;
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

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            if (updates != null)
            {
                foreach (var t in updates)
                {
                    if (Result == t.Item1)
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
            var items = new[] { FieldsToSearch }.Union(ResultsCollection.Where(c => !string.IsNullOrEmpty(c.SearchCriteria)).Select(c => c.SearchCriteria)).ToArray();
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

        public bool Equals(DsfFindRecordsMultipleCriteriaActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var resultsCollectionsAreEqual = CommonEqualityOps.CollectionEquals(ResultsCollection, other.ResultsCollection, new FindRecordsTOComparer());
            return base.Equals(other)
                && string.Equals(FieldsToSearch, other.FieldsToSearch)
                && string.Equals(Result, other.Result)
                && string.Equals(StartIndex, other.StartIndex)
                && MatchCase == other.MatchCase
                && RequireAllTrue == other.RequireAllTrue
                && RequireAllFieldsToMatch == other.RequireAllFieldsToMatch
                && resultsCollectionsAreEqual;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((DsfFindRecordsMultipleCriteriaActivity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (FieldsToSearch != null ? FieldsToSearch.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (StartIndex != null ? StartIndex.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ MatchCase.GetHashCode();
                hashCode = (hashCode * 397) ^ RequireAllTrue.GetHashCode();
                hashCode = (hashCode * 397) ^ RequireAllFieldsToMatch.GetHashCode();
                hashCode = (hashCode * 397) ^ (ResultsCollection != null ? ResultsCollection.GetHashCode() : 0);
                return hashCode;
            }
        }
    }


    class SearchQuery
    {
        public readonly bool RequireAllFieldsToMatch;
        public readonly bool RequireAllTrue;
        protected DsfFindRecordsMultipleCriteriaActivity activity;
        protected readonly string Result;


        public SearchQuery(DsfFindRecordsMultipleCriteriaActivity activity, string Result, bool RequireAllFieldsToMatch, bool RequireAllTrue)
        {
            this.activity = activity;
            this.Result = Result;
            this.RequireAllFieldsToMatch = RequireAllFieldsToMatch;
            this.RequireAllTrue = RequireAllTrue;
        }


#pragma warning disable S1541 // Methods and properties should not be too complex
        public void Execute(SearchContext searchContext, ErrorResultTO errorResult, IDSFDataObject dataObject, int update)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            var env = dataObject.Environment;

            // Local Functions
            //-------------------------------------------------------------------------------------
            #region local-functions
            void ApplyResultsToEnvironment(IList<int> results)
            {
                var distinctResults = results.Distinct();
                if (DataListUtil.IsValueScalar(Result))
                {
                    var res = string.Join(",", distinctResults);
                    env.Assign(Result, res, update);
                }
                else
                {
                    foreach (var distinctResult in distinctResults)
                    {
                        env.Assign(Result, distinctResult.ToString(), update);
                    }
                }
            }
            IEnumerable<int> GetResultsForField(string searchvar)
            {
                Func<DataStorage.WarewolfAtom, bool> func = null;
                foreach (FindRecordsTO criteria in activity.ResultsCollection.Where(a => !String.IsNullOrEmpty(a.SearchType)))
                {
                    var right = env.EvalAsList(criteria.SearchCriteria, update);
                    IEnumerable<DataStorage.WarewolfAtom> from = new List<DataStorage.WarewolfAtom>();
                    IEnumerable<DataStorage.WarewolfAtom> tovalue = new List<DataStorage.WarewolfAtom>();

                    if (!String.IsNullOrEmpty(criteria.From))
                    {
                        @from = env.EvalAsList(criteria.From, update);
                    }
                    if (!String.IsNullOrEmpty(criteria.To))
                    {
                        tovalue = env.EvalAsList(criteria.To, update);
                    }
                    func = BuildQueryExpression(func, criteria, right, from, tovalue);
                }
                return env.EvalWhere(env.ToStar(searchvar), func, update);
            }
            List<int> GetResults()
            {
                var hasEvaled = false;
                var results = new List<int>();

                foreach (var searchvar in searchContext.ToSearch)
                {
                    var output = GetResultsForField(searchvar);

                    results = RequireAllFieldsToMatch && hasEvaled ? results.Intersect(output).ToList() : results.Union(output).ToList();
                    hasEvaled = true;
                }

                if (!results.Any())
                {
                    results.Add(-1);
                }

                return results;
            }

            Func<DataStorage.WarewolfAtom, bool> BuildQueryExpression(Func<DataStorage.WarewolfAtom, bool> func, FindRecordsTO criteria, IEnumerable<DataStorage.WarewolfAtom> right, IEnumerable<DataStorage.WarewolfAtom> from, IEnumerable<DataStorage.WarewolfAtom> tovalue)
            {
                return func == null ?
                    CreateFuncFromOperator(criteria.SearchType, right, @from, tovalue) :
                    RequireAllTrue ?
                        CombineFuncAnd(func, criteria.SearchType, right, @from, tovalue) :
                        CombineFuncOr(func, criteria.SearchType, right, @from, tovalue);
            }

            
            #endregion
            
            searchContext.Validate(errorResult);


            var queryResults = GetResults();
            ApplyResultsToEnvironment(queryResults);
        }


        Func<DataStorage.WarewolfAtom, bool> CombineFuncAnd(Func<DataStorage.WarewolfAtom, bool> func, string searchType, IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> from, IEnumerable<DataStorage.WarewolfAtom> to)
        {
            var func2 = CreateFuncFromOperator(searchType, values, from, to);

            return a =>
            {
                try
                {
                    return func.Invoke(a) && func2.Invoke(a);
                }
                catch (DataStorage.WarewolfInvalidComparisonException ex)
                {
                    return false;
                }
            };
        }

        Func<DataStorage.WarewolfAtom, bool> CombineFuncOr(Func<DataStorage.WarewolfAtom, bool> func, string searchType, IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> from, IEnumerable<DataStorage.WarewolfAtom> to)
        {
            var func2 = CreateFuncFromOperator(searchType, values, from, to);
            return a =>
            {
                bool CatchInvalidComparisons(Func<DataStorage.WarewolfAtom, bool> f)
                {
                    var ret = false;
                    try
                    {
                        ret = f.Invoke(a);
                        if (ret)
                        {
                            return ret;
                        }
                    }
                    catch (DataStorage.WarewolfInvalidComparisonException ex)
                    {
                        ret = false;
                    }
                    return ret;
                }
                return CatchInvalidComparisons(func) || CatchInvalidComparisons(func2);
            };
        }


        Func<DataStorage.WarewolfAtom, bool> CreateFuncFromOperator(string searchType, IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> from, IEnumerable<DataStorage.WarewolfAtom> to)
        {

            var opt = FindRecsetOptions.FindMatch(searchType);
            return (a) =>
            {
                try
                {
                    return opt.GenerateFunc(values, from, to, RequireAllFieldsToMatch).Invoke(a);
                }
                catch (DataStorage.WarewolfInvalidComparisonException ex)
                {
                    return false;
                }
            };
        }

    }

    class SearchContext
    {
        protected DsfFindRecordsMultipleCriteriaActivity activity;

        public readonly IList<string> ToSearch;

        public SearchContext(DsfFindRecordsMultipleCriteriaActivity activity)
        {
            this.activity = activity;
            ToSearch = activity.FieldsToSearch.Split(',')
                            .Select(a => a.Trim())
                            .ToList();
        }

        public void Validate(ErrorResultTO errorsTo)
        {
            ValidateSearchFields();
            ValidateMatchesAgainstFields(errorsTo);
        }

        void ValidateSearchFields()
        {
            var scalarValues = ToSearch.Where(DataListUtil.IsValueScalar).ToList();
            if (scalarValues.Any())
            {
                throw new Exception(ErrorResource.ScalarsNotAllowed + Environment.NewLine + string.Join(Environment.NewLine, scalarValues));
            }
        }

        void ValidateMatchesAgainstFields(ErrorResultTO errorsTo)
        {
            foreach (FindRecordsTO criteria in activity.ResultsCollection.Where(a => !String.IsNullOrEmpty(a.SearchType)))
            {
                if (criteria.From.Length > 0 && String.IsNullOrEmpty(criteria.To)
                   || criteria.To.Length > 0 && String.IsNullOrEmpty(criteria.From))
                {
                    throw new Exception(ErrorResource.FROMAndTORequired);
                }
                ValidateRequiredFields(criteria, errorsTo);
            }
        }

        static void ValidateRequiredFields(FindRecordsTO searchTo, ErrorResultTO errors)
        {
            if (string.IsNullOrEmpty(searchTo.SearchType))
            {
                errors.AddError(string.Format(ErrorResource.IsRequired, "Search Type"));
            }

            if (searchTo.SearchType.Equals("Is Between"))
            {
                if (string.IsNullOrEmpty(searchTo.From))
                {
                    errors.AddError(string.Format(ErrorResource.IsRequired, "FROM"));
                }

                if (string.IsNullOrEmpty(searchTo.To))
                {
                    errors.AddError(string.Format(ErrorResource.IsRequired, "TO"));
                }
            }
        }
    }
}
