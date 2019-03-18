#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
using Dev2.Common.State;
using Dev2.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    [ToolDescriptorInfo("Data-CaseConversion", "Case Convert", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Data", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Data_Case_Convert")]
    public class DsfCaseConvertActivity : DsfActivityAbstract<string>, ICollectionActivity, IEquatable<DsfCaseConvertActivity>
    {
        public IList<ICaseConvertTO> ConvertCollection { get; set; }

        public DsfCaseConvertActivity()
            : base("Case Conversion")
        {
            ConvertCollection = new List<ICaseConvertTO>();
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata) => base.CacheMetadata(metadata);

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
                TryExecute(dataObject, update, allErrors, errors, env);
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

        private void TryExecute(IDSFDataObject dataObject, int update, ErrorResultTO allErrors, ErrorResultTO errors, IExecutionEnvironment env)
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


        public override enFindMissingType GetFindMissingType() => enFindMissingType.DataGridActivity;

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
                    AddToConvertCollection(listToAdd, modelItem, mic);
                }
            }
        }

        private void AddToConvertCollection(IEnumerable<string> listToAdd, ModelItem modelItem, ModelItemCollection mic)
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

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
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

        public int GetCollectionCount() => ConvertCollection.Count(caseConvertTo => !caseConvertTo.CanRemove());

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

        public override List<string> GetOutputs() => ConvertCollection.Select(to => to.Result).ToList();

        public override IEnumerable<StateVariable> GetState()
        {
            return new[]
            {
                new StateVariable
                {
                    Name="Convert Collection",
                    Type= StateVariable.StateType.InputOutput,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(ConvertCollection)
                }
            };
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
