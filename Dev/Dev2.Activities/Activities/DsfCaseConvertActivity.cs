/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Core.Convertors.Case;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Comparer;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Validation;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using Dev2.Activities.Factories.Case;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    [ToolDescriptorInfo("Data-CaseConversion", "Case Convert", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Data", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Data_Case_Convert")]
    public class DsfCaseConvertActivity : DsfActivityAbstract<string>, ICollectionActivity,IEquatable<DsfCaseConvertActivity>
    {
        public IList<ICaseConvertTO> ConvertCollection { get; set; }

        public DsfCaseConvertActivity()
            : base("Case Conversion")
        {
            ConvertCollection = new List<ICaseConvertTO>();
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            var allErrors = new ErrorResultTO();
            var errors = new ErrorResultTO();
            var env = dataObject.Environment;
            InitializeDebug(dataObject);
            try
            {
                CleanArgs();

                allErrors.MergeErrors(errors);

                var inputIndex = 1;
                var outputIndex = 1;

                foreach (ICaseConvertTO item in ConvertCollection.Where(a => !String.IsNullOrEmpty(a.StringToConvert)))
                {
                    IsSingleValueRule.ApplyIsSingleValueRule(item.ExpressionToConvert, allErrors);
                    if (dataObject.IsDebugMode())
                    {
                        var debugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("", inputIndex.ToString(CultureInfo.InvariantCulture)), debugItem);
                        AddDebugItem(new DebugEvalResult(item.StringToConvert, "Convert", env, update), debugItem);
                        AddDebugItem(new DebugItemStaticDataParams(item.ConvertType, "To"), debugItem);
                        _debugInputs.Add(debugItem);
                        inputIndex++;
                    }
                    if (!allErrors.HasErrors())
                    {
                        try
                        {
                            env.ApplyUpdate(item.StringToConvert, TryConvertFunc(item, env, update), update);
                        }
                        catch (Exception e)
                        {
                            allErrors.AddError(e.Message);
                        }

                        if (!allErrors.HasErrors() && dataObject.IsDebugMode())
                        {
                            var debugItem = new DebugItem();
                            AddDebugItem(new DebugItemStaticDataParams("", outputIndex.ToString(CultureInfo.InvariantCulture)), debugItem);
                            AddDebugItem(new DebugEvalResult(item.StringToConvert, "", env, update), debugItem);
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
                HandleErrors(dataObject, update, allErrors);
            }
        }

        void HandleErrors(IDSFDataObject dataObject, int update, ErrorResultTO allErrors)
        {
            var hasErrors = allErrors.HasErrors();
            if (hasErrors)
            {
                DisplayAndWriteError(nameof(DsfCaseConvertActivity), allErrors);
                var errorString = allErrors.MakeDisplayReady();
                dataObject.Environment.AddError(errorString);
            }
            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.Before, update);
                DispatchDebugState(dataObject, StateType.After, update);
            }
        }

        static Func<DataStorage.WarewolfAtom, DataStorage.WarewolfAtom> TryConvertFunc(ICaseConvertTO conversionType, IExecutionEnvironment env, int update)
        {
            var convertFunct = CaseConverter.GetFuncs();

            if (convertFunct.TryGetValue(conversionType.ConvertType, out Func<string, string> returnedFunc) && returnedFunc != null)
            {
                return a =>
                {
                    var upper = returnedFunc.Invoke(a.ToString());
                    var evalled = env.Eval(upper, update);

                    if (evalled.IsWarewolfAtomResult)
                    {
                        if (evalled is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult warewolfAtomResult)
                        {
                            return warewolfAtomResult.Item;
                        }
                        return DataStorage.WarewolfAtom.Nothing;
                    }

                    return DataStorage.WarewolfAtom.NewDataString(CommonFunctions.evalResultToString(evalled));
                };
            }
            throw new Exception(ErrorResource.ConvertOptionDoesNotExist);
        }


        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

        void BuildStringToConvert(int i, List<string> targetList, List<string> resultList)
        {
            ConvertCollection[i].StringToConvert = targetList[0];
            ConvertCollection[i].Result = resultList[0];
            var canidateResult = resultList[0];
            for (var q = 1; q < targetList.Count; q++)
            {
                var pos = ConvertCollection.Count + 1;

                // now process all new results ;)
                // we always keep the last value in-case we run out of indexes
                // as they do not have to balance ;)
                if (q < resultList.Count)
                {
                    canidateResult = resultList[q];
                }

                ConvertCollection.Add(new CaseConvertTO(targetList[q], ConvertCollection[i].ConvertType, canidateResult, pos));
            }
        }

        List<string> BreakIntoTokens(string value)
        {
            var parts = value.Split(',');
            var result = parts.Select(r => r.Trim()).ToList();
            return result;
        }

        void CleanArgs()
        {
            var workItems = new ICaseConvertTO[ConvertCollection.Count];
            ConvertCollection.CopyTo(workItems, 0);


            for (var i = 0; i < workItems.Length; i++)

            {
                var convertResult = workItems[i].Result;
                var convertTarget = workItems[i].StringToConvert;

                if (!string.IsNullOrEmpty(convertTarget) && !string.IsNullOrEmpty(convertResult))
                {
                    var targetList = BreakIntoTokens(convertTarget);
                    var resultList = BreakIntoTokens(convertResult);

                    // now add them back together
                    if (targetList.Count > 0 && resultList.Count > 0)
                    {
                        // build up the StringToConvert section ;)
                        // existing record
                        BuildStringToConvert(i, targetList, resultList);
                    }
                }
                else
                {
                    ConvertCollection.RemoveAt(i);
                }
            }
        }

        void InsertToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ConvertCollection"];
            if (modelProperty != null)
            {
                var mic = modelProperty.Collection;

                if (mic != null)
                {
                    var listOfValidRows = ConvertCollection.Where(c => !c.CanRemove()).ToList();
                    if (listOfValidRows.Count > 0)
                    {
                        var startIndex = ConvertCollection.IndexOf(listOfValidRows.Last()) + 1;
                        foreach (string s in listToAdd)
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

        void AddToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ConvertCollection"];
            if (modelProperty != null)
            {
                var mic = modelProperty.Collection;

                if (mic != null)
                {
                    var startIndex = 0;
                    var firstRowConvertType = ConvertCollection[0].ConvertType;
                    mic.Clear();
                    foreach (string s in listToAdd)
                    {
                        mic.Insert(startIndex, new CaseConvertTO(s, firstRowConvertType, s, startIndex + 1));
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
          

        void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
        {
            if (startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }
            mic.Add(new CaseConvertTO(string.Empty, "UPPER", string.Empty, startIndex + 1));
            var modelProperty = modelItem.Properties["DisplayName"];
            if (modelProperty != null)
            {
                modelProperty.SetValue(CreateDisplayName(modelItem, startIndex + 1));
            }
        }

        string CreateDisplayName(ModelItem modelItem, int count)
        {
            var modelProperty = modelItem.Properties["DisplayName"];
            if (modelProperty != null)
            {
                var currentName = modelProperty.ComputedValue as string;
                if (currentName != null && currentName.Contains("(") && currentName.Contains(")"))
                {
                    currentName = currentName.Remove(currentName.Contains(" (") ? currentName.IndexOf(" (", StringComparison.Ordinal) : currentName.IndexOf("(", StringComparison.Ordinal));
                }
                currentName = currentName + " (" + (count - 1) + ")";
                return currentName;
            }

            return string.Empty;
        }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment environment, int update)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment environment, int update)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            foreach (Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                var t1 = t;
                var items = ConvertCollection.Where(c => !string.IsNullOrEmpty(c.StringToConvert) && c.StringToConvert.Contains(t1.Item1));

                // issues updates
                foreach (var a in items)
                {
                    a.StringToConvert = a.StringToConvert.Replace(t.Item1, t.Item2);
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            foreach (Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                var t1 = t;
                var items = ConvertCollection.Where(c => !string.IsNullOrEmpty(c.Result) && c.Result.Contains(t1.Item1));

                // issues updates
                foreach (var a in items)
                {
                    a.Result = a.Result.Replace(t.Item1, t.Item2);
                }
            }
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var result = new List<DsfForEachItem>();

            foreach (var item in ConvertCollection)
            {
                if (!string.IsNullOrEmpty(item.StringToConvert) && item.StringToConvert.Contains("[["))
                {
                    result.Add(new DsfForEachItem { Name = item.StringToConvert, Value = item.Result });
                }
            }

            return result;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            var result = new List<DsfForEachItem>();
            foreach (var item in ConvertCollection)
            {
                if (!string.IsNullOrEmpty(item.StringToConvert) && item.StringToConvert.Contains("[["))
                {
                    result.Add(new DsfForEachItem { Name = item.Result, Value = item.StringToConvert });
                }
            }
            return result;
        }

        public int GetCollectionCount()
        {
            return ConvertCollection.Count(caseConvertTo => !caseConvertTo.CanRemove());
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

        public override List<string> GetOutputs()
        {
            return ConvertCollection.Select(to => to.Result).ToList();
        }

        public bool Equals(DsfCaseConvertActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var collectionEquals = CommonEqualityOps.CollectionEquals(ConvertCollection, other.ConvertCollection, new CaseConvertToComparer());
            
            return base.Equals(other) && collectionEquals;
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

            return Equals((DsfCaseConvertActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (ConvertCollection != null ? ConvertCollection.GetHashCode() : 0);
            }
        }
    }
}
