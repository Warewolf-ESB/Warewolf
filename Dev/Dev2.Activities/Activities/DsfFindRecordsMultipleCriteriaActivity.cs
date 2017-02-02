/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    /// <New>
    /// Activity for finding records accoring to a search criteria that the user specifies
    /// </New>
    [ToolDescriptorInfo("RecordSet-FindRecords", "Find Records", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Recordset", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Recordset_Find_Records")]
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


        public override List<string> GetOutputs()
        {
            return new List<string> { Result };
        }


        /// <summary>
        ///     Executes the logic of the activity and calls the backend code to do the work
        ///     Also responsible for adding the results to the data list
        /// </summary>
        /// <param name="context"></param>
        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {


            var env = dataObject.Environment;
            InitializeDebug(dataObject);
            var allErrors = new ErrorResultTO();
            try
            {
                IList<string> toSearch = FieldsToSearch.Split(',').Select(a => a.Trim()).ToList();
                var scalarValues = toSearch.Where(DataListUtil.IsValueScalar).ToList();
                if (scalarValues.Any())
                {
                    throw new Exception(ErrorResource.ScalarsNotAllowed + Environment.NewLine + string.Join(Environment.NewLine, scalarValues));
                }
                List<int> results = new List<int>();
                if (dataObject.IsDebugMode())
                {
                    AddDebugInputValues(dataObject, toSearch, ref allErrors, update);
                }

                bool hasEvaled = false;
                foreach (var searchvar in toSearch)
                {
                    Func<DataStorage.WarewolfAtom, bool> func = null;
                    foreach (FindRecordsTO to in ResultsCollection.Where(a => !String.IsNullOrEmpty(a.SearchType)))
                    {
                        if (to.From.Length > 0 && String.IsNullOrEmpty(to.To)
                           || to.To.Length > 0 && String.IsNullOrEmpty(to.From))
                        {
                            throw new Exception(ErrorResource.FROMAndTORequired);
                        }
                        ValidateRequiredFields(to, out errorsTo);
                        var right = env.EvalAsList(to.SearchCriteria, update);
                        IEnumerable<DataStorage.WarewolfAtom> from = new List<DataStorage.WarewolfAtom>();
                        IEnumerable<DataStorage.WarewolfAtom> tovalue = new List<DataStorage.WarewolfAtom>();

                        if (!String.IsNullOrEmpty(to.From))
                        {
                            @from = env.EvalAsList(to.From, update);
                        }
                        if (!String.IsNullOrEmpty(to.To))
                        {
                            tovalue = env.EvalAsList(to.To, update);
                        }
                        if (func == null)
                        {
                            func = CreateFuncFromOperator(to.SearchType, right, @from, tovalue);
                        }
                        else
                        {
                            func = RequireAllTrue ? CombineFuncAnd(func, to.SearchType, right, @from, tovalue) : CombineFuncOr(func, to.SearchType, right, @from, tovalue);
                        }
                    }
                    var output = env.EvalWhere(dataObject.Environment.ToStar(searchvar), func, update);

                    if (RequireAllFieldsToMatch && hasEvaled)
                    {
                        results = results.Intersect(output).ToList();
                    }
                    else
                    {
                        results = results.Union(output).ToList();
                    }
                    hasEvaled = true;
                }
                if (!results.Any())
                {
                    results.Add(-1);
                }
                var distinctResults = results.Distinct();
                if (DataListUtil.IsValueScalar(Result))
                {
                    var res = string.Join(",", distinctResults);
                    env.Assign(Result, res, update);
                }
                else
                {
                    foreach(var distinctResult in distinctResults)
                    {
                        env.Assign(Result, distinctResult.ToString(), update);
                    }
                }
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
                Dev2Logger.Error("DSFRecordsMultipleCriteria", exception);
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

        Func<DataStorage.WarewolfAtom, bool> CombineFuncAnd(Func<DataStorage.WarewolfAtom, bool> func, string searchType, IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> from, IEnumerable<DataStorage.WarewolfAtom> to)
        {
            var func2 = CreateFuncFromOperator(searchType, values, from, to);
            return a => func.Invoke(a) && func2.Invoke(a);
        }

        Func<DataStorage.WarewolfAtom, bool> CombineFuncOr(Func<DataStorage.WarewolfAtom, bool> func, string searchType, IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> from, IEnumerable<DataStorage.WarewolfAtom> to)
        {
            var func2 = CreateFuncFromOperator(searchType, values, from, to);
            return a => func.Invoke(a) || func2.Invoke(a);
        }


        Func<DataStorage.WarewolfAtom, bool> CreateFuncFromOperator(string searchType, IEnumerable<DataStorage.WarewolfAtom> values, IEnumerable<DataStorage.WarewolfAtom> from, IEnumerable<DataStorage.WarewolfAtom> to)
        {

            IFindRecsetOptions opt = FindRecsetOptions.FindMatch(searchType);
            return opt.GenerateFunc(values, from, to, RequireAllFieldsToMatch);
        }

        private void ValidateRequiredFields(FindRecordsTO searchTo, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            if (string.IsNullOrEmpty(searchTo.SearchType))
            {
                errors.AddError(string.Format(ErrorResource.IsRequired ,"Search Type"));
            }

            if (searchTo.SearchType.Equals("Is Between"))
            {
                if (string.IsNullOrEmpty(searchTo.From))
                {
                    errors.AddError(string.Format(ErrorResource.IsRequired, "FROM"));
                }

                if (string.IsNullOrEmpty(searchTo.To))
                {
                    errors.AddError(string.Format(ErrorResource.IsRequired,"TO"));
                }
            }
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.MixedActivity;
        }

        void AddDebugInputValues(IDSFDataObject dataObject, IEnumerable<string> toSearch, ref ErrorResultTO errorTos, int update)
        {
            if (dataObject.IsDebugMode())
            {
                try
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
                catch (Exception e)
                {
                    errorTos.AddError(e.Message);
                }
            }
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
                DebugItem debugItem = new DebugItem();
                if (!String.IsNullOrEmpty(findRecordsTo.SearchType))
                {
                    AddDebugItem(new DebugItemStaticDataParams("", indexCount.ToString(CultureInfo.InvariantCulture)), debugItem);
                    AddDebugItem(new DebugItemStaticDataParams(findRecordsTo.SearchType, ""), debugItem);

                    if (!string.IsNullOrEmpty(findRecordsTo.SearchCriteria))
                    {
                        AddDebugItem(new DebugEvalResult(findRecordsTo.SearchCriteria, "", environment, update), debugItem);
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
                FindRecordsTO findRecordsTo = ResultsCollection.Last(c => !c.CanRemove());
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
            if (currentName != null && currentName.Contains("(") && currentName.Contains(")"))
            {
                currentName = currentName.Remove(currentName.Contains(" (") ? currentName.IndexOf(" (", StringComparison.Ordinal) : currentName.IndexOf("(", StringComparison.Ordinal));
            }
            currentName = currentName + " (" + (count - 1) + ")";
            return currentName;
        }

        #endregion Private Methods

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList, int update)
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
    }
}
