
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
using Dev2.Common.Interfaces.Core.Convertors.Case;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Interfaces;
using Dev2.Validation;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DsfCaseConvertActivity : DsfActivityAbstract<string>, ICollectionActivity
    {
        #region Properties

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

        #region Overridden NativeActivity Methods

        // ReSharper disable RedundantOverridenMember
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }
        // ReSharper restore RedundantOverridenMember

        /// <summary>
        /// The execute method that is called when the activity is executed at run time and will hold all the logic of the activity
        /// </summary>       
        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();

            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            var env = dataObject.Environment;
            InitializeDebug(dataObject);
            try
            {
                CleanArgs();


                allErrors.MergeErrors(errors);

                int inputIndex = 1;
                int outputIndex = 1;

                foreach(ICaseConvertTO item in ConvertCollection.Where(a=>!String.IsNullOrEmpty(a.StringToConvert)))
                {

                    if (dataObject.IsDebugMode())
                    {
                        var debugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("", inputIndex.ToString(CultureInfo.InvariantCulture)), debugItem);
                        AddDebugItem(new DebugEvalResult(item.StringToConvert, "Convert", env), debugItem);
                        AddDebugItem(new DebugItemStaticDataParams(item.ConvertType, "To"), debugItem);
                        _debugInputs.Add(debugItem);
                        inputIndex++;
                    }

                    env.ApplyUpdate(item.StringToConvert, TryConvertFunc(item.ConvertType,env));
                    string errorMessage;
                    if (ExecutionEnvironment.IsValidVariableExpression(item.StringToConvert, out errorMessage))
                    {
                        IsSingleValueRule.ApplyIsSingleValueRule(item.ExpressionToConvert, allErrors);
                        if (dataObject.IsDebugMode())
                        {
                            var debugItem = new DebugItem();
                            AddDebugItem(new DebugItemStaticDataParams("", outputIndex.ToString(CultureInfo.InvariantCulture)), debugItem);
                            AddDebugItem(new DebugEvalResult(item.StringToConvert, "", env), debugItem);
                            _debugOutputs.Add(debugItem);
                            outputIndex++;
                        }
                    }

                   
                }

            }
            catch (Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfCaseConvertActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                }
                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }


        public Func<DataASTMutable.WarewolfAtom,DataASTMutable.WarewolfAtom> TryConvertFunc(string conversionType,IExecutionEnvironment env)
        {
            var convertFunct = CaseConverter.GetFuncs();
            Func<string, string> returnedFunc;

            if (convertFunct.TryGetValue(conversionType, out returnedFunc))
            {
                if (returnedFunc != null)
                {
                    return (a=>
                    {
                        var upper = returnedFunc.Invoke(a.ToString());
                        var evalled = env.Eval(upper);
                        if(evalled.IsWarewolfAtomResult)
                        {
                            var warewolfAtomResult = evalled as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                            if(warewolfAtomResult != null)
                            {
                                return warewolfAtomResult.Item;
                            }
                            return DataASTMutable.WarewolfAtom.Nothing;
                        }

                        return DataASTMutable.WarewolfAtom.NewDataString(  WarewolfDataEvaluationCommon.EvalResultToString(evalled));
                    });
                }
            }
            throw  new Exception("Convert option does not exist");
        }

        

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

        #endregion

        #region Private Methods

        private List<string> BreakIntoTokens(string value)
        {
            var parts = value.Split(',');
            var result = parts.Select(r => r.Trim()).ToList();
            return result;
        }

        private void CleanArgs()
        {
            ICaseConvertTO[] workItems = new ICaseConvertTO[ConvertCollection.Count];
            ConvertCollection.CopyTo(workItems, 0);

            // ReSharper disable ForCanBeConvertedToForeach
            for(var i = 0; i < workItems.Length; i++)
            // ReSharper restore ForCanBeConvertedToForeach
            {
                var convertResult = workItems[i].Result;
                var convertTarget = workItems[i].StringToConvert;

                if(!string.IsNullOrEmpty(convertTarget) && !string.IsNullOrEmpty(convertResult))
                {
                    var targetList = BreakIntoTokens(convertTarget);
                    var resultList = BreakIntoTokens(convertResult);

                    // now add them back together
                    if(targetList.Count > 0 && resultList.Count > 0)
                    {
                        // build up the StringToConvert section ;)
                        // existing record
                        ConvertCollection[i].StringToConvert = targetList[0];
                        ConvertCollection[i].Result = resultList[0];
                        var canidateResult = resultList[0];
                        for(var q = 1; q < targetList.Count; q++)
                        {
                            var pos = ConvertCollection.Count + 1;

                            // now process all new results ;)
                            // we always keep the last value in-case we run out of indexes 
                            // as they do not have to balance ;)
                            if(q < resultList.Count)
                            {
                                canidateResult = resultList[q];
                            }

                            ConvertCollection.Add(new CaseConvertTO(targetList[q], ConvertCollection[i].ConvertType, canidateResult, pos));
                        }
                    }
                }
                else
                {
                    ConvertCollection.RemoveAt(i);
                }
            }

        }

        private void InsertToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ConvertCollection"];
            if(modelProperty != null)
            {
                ModelItemCollection mic = modelProperty.Collection;

                if(mic != null)
                {
                    List<ICaseConvertTO> listOfValidRows = ConvertCollection.Where(c => !c.CanRemove()).ToList();
                    if(listOfValidRows.Count > 0)
                    {
                        int startIndex = ConvertCollection.IndexOf(listOfValidRows.Last()) + 1;
                        foreach(string s in listToAdd)
                        {
                            mic.Insert(startIndex, new CaseConvertTO(s, ConvertCollection[startIndex - 1].ConvertType, s, startIndex + 1));
                            startIndex++;
                        }
                        CleanUpCollection(mic, modelItem, startIndex);
                    }
                    else
                    {
                        AddToCollection(listToAdd, modelItem);
                    }
                }
            }
        }

        private void AddToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ConvertCollection"];
            if(modelProperty != null)
            {
                ModelItemCollection mic = modelProperty.Collection;

                if(mic != null)
                {
                    int startIndex = 0;
                    string firstRowConvertType = ConvertCollection[0].ConvertType;
                    mic.Clear();
                    foreach(string s in listToAdd)
                    {
                        mic.Add(new CaseConvertTO(s, firstRowConvertType, s, startIndex + 1));
                        startIndex++;
                    }
                    CleanUpCollection(mic, modelItem, startIndex);
                }
            }
        }

        private void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
        {
            if(startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }
            mic.Add(new CaseConvertTO(string.Empty, "UPPER", string.Empty, startIndex + 1));
            var modelProperty = modelItem.Properties["DisplayName"];
            if(modelProperty != null)
            {
                modelProperty.SetValue(CreateDisplayName(modelItem, startIndex + 1));
            }
        }

        private string CreateDisplayName(ModelItem modelItem, int count)
        {
            var modelProperty = modelItem.Properties["DisplayName"];
            if(modelProperty != null)
            {
                string currentName = modelProperty.ComputedValue as string;
                if(currentName != null && (currentName.Contains("(") && currentName.Contains(")")))
                {
                    currentName = currentName.Remove(currentName.Contains(" (") ? currentName.IndexOf(" (", StringComparison.Ordinal) : currentName.IndexOf("(", StringComparison.Ordinal));
                }
                currentName = currentName + " (" + (count - 1) + ")";
                return currentName;
            }

            return string.Empty;
        }

        #endregion Private Methods

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment environment)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment environment)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }


        #endregion

        #region Get ForEach Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach(Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                Tuple<string, string> t1 = t;
                var items = ConvertCollection.Where(c => !string.IsNullOrEmpty(c.StringToConvert) && c.StringToConvert.Contains(t1.Item1));

                // issues updates
                foreach(var a in items)
                {
                    a.StringToConvert = a.StringToConvert.Replace(t.Item1, t.Item2);
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {

            foreach(Tuple<string, string> t in updates)
            {

                // locate all updates for this tuple
                Tuple<string, string> t1 = t;
                var items = ConvertCollection.Where(c => !string.IsNullOrEmpty(c.Result) && c.Result.Contains(t1.Item1));

                // issues updates
                foreach(var a in items)
                {
                    a.Result = a.Result.Replace(t.Item1, t.Item2);
                }
            }
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var result = new List<DsfForEachItem>();

            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(var item in ConvertCollection)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                if(!string.IsNullOrEmpty(item.StringToConvert) && item.StringToConvert.Contains("[["))
                {
                    result.Add(new DsfForEachItem { Name = item.StringToConvert, Value = item.Result });
                }
            }

            return result;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            var result = new List<DsfForEachItem>();

            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(var item in ConvertCollection)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                if(!string.IsNullOrEmpty(item.StringToConvert) && item.StringToConvert.Contains("[["))
                {
                    result.Add(new DsfForEachItem { Name = item.Result, Value = item.StringToConvert });
                }
            }

            return result;
        }

        #endregion

        #region Implementation of ICollectionActivity

        public int GetCollectionCount()
        {
            return ConvertCollection.Count(caseConvertTo => !caseConvertTo.CanRemove());
        }

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
