/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data;
using Dev2.Data.Operations;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using WarewolfParserInterop;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    [ToolDescriptorInfo("Data-DataMerge", "Data Merge", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Data", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Data_Data_Merge_Tags")]
    public class DsfDataMergeActivity : DsfActivityAbstract<string>, ICollectionActivity
    {
        #region Class Members

        private string _result;

        #endregion Class Members

        #region Properties

        private IList<DataMergeDTO> _mergeCollection;
        public IList<DataMergeDTO> MergeCollection
        {
            get
            {
                return _mergeCollection;
            }
            set
            {
                _mergeCollection = value;
                OnPropertyChanged("MergeCollection");
            }
        }

        public new string Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
                OnPropertyChanged("Result");
            }
        }

        protected override bool CanInduceIdle => true;

        #endregion

        #region Ctor

        public DsfDataMergeActivity()
            : base("Data Merge")
        {
            MergeCollection = new List<DataMergeDTO>();
        }

        #endregion

        #region Overridden NativeActivity Methods
        // ReSharper disable RedundantOverridenMember
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }
        // ReSharper restore RedundantOverridenMember

        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {


            IDev2MergeOperations mergeOperations = new Dev2MergeOperations();
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errorResultTo = new ErrorResultTO();

            InitializeDebug(dataObject);
            try
            {
                CleanArguments(MergeCollection);

                if(MergeCollection.Count <= 0)
                {
                    return;
                }
                IWarewolfListIterator warewolfListIterator = new WarewolfListIterator();
                allErrors.MergeErrors(errorResultTo);
                Dictionary<int, List<IWarewolfIterator>> listOfIterators = new Dictionary<int, List<IWarewolfIterator>>();

                #region Create a iterator for each row in the data grid in the designer so that the right iteration happen on the data

                int dictionaryKey = 0;
                foreach(DataMergeDTO row in MergeCollection)
                {
                    allErrors.MergeErrors(errorResultTo);

                    if(dataObject.IsDebugMode())
                    {
                        DebugItem debugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("", (MergeCollection.IndexOf(row) + 1).ToString(CultureInfo.InvariantCulture)), debugItem);
                        AddDebugItem(new DebugEvalResult(row.InputVariable, "", dataObject.Environment, update, true), debugItem);
                        AddDebugItem(new DebugItemStaticDataParams(row.MergeType, "With"), debugItem);
                        AddDebugItem(new DebugEvalResult(row.At, "Using", dataObject.Environment, update), debugItem);
                        AddDebugItem(new DebugEvalResult(row.Padding, "Pad", dataObject.Environment, update), debugItem);

                        //Old workflows don't have this set. 
                        if(row.Alignment == null)
                        {
                            row.Alignment = string.Empty;
                        }

                        AddDebugItem(DataListUtil.IsEvaluated(row.Alignment) ? new DebugItemStaticDataParams("", row.Alignment, "Align") : new DebugItemStaticDataParams(row.Alignment, "Align"), debugItem);

                        _debugInputs.Add(debugItem);
                    }
                    var listOfEvalResultsForInput = dataObject.Environment.EvalForDataMerge(row.InputVariable, update);
                    var innerIterator = new WarewolfListIterator();
                    var innerListOfIters = new List<WarewolfIterator>();

                    foreach(var listOfIterator in listOfEvalResultsForInput)
                    {
                        var inIterator = new WarewolfIterator(listOfIterator);
                        innerIterator.AddVariableToIterateOn(inIterator);
                        innerListOfIters.Add(inIterator);
                    }
                    var atomList = new List<DataStorage.WarewolfAtom>();
                    while(innerIterator.HasMoreData())
                    {
                        var stringToUse = "";
                        foreach(var warewolfIterator in innerListOfIters)
                        {
                            stringToUse += warewolfIterator.GetNextValue();
                        }
                        atomList.Add(DataStorage.WarewolfAtom.NewDataString(stringToUse));
                    }
                    var finalString = string.Join("", atomList);
                    var inputListResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomListresult(new WarewolfAtomList<DataStorage.WarewolfAtom>(DataStorage.WarewolfAtom.Nothing, atomList));
                    if(DataListUtil.IsFullyEvaluated(finalString))
                    {
                        inputListResult = dataObject.Environment.Eval(finalString, update);
                    }

                    var inputIterator = new WarewolfIterator(inputListResult);
                    var atIterator = new WarewolfIterator(dataObject.Environment.Eval(row.At, update));
                    var paddingIterator = new WarewolfIterator(dataObject.Environment.Eval(row.Padding, update));
                    warewolfListIterator.AddVariableToIterateOn(inputIterator);
                    warewolfListIterator.AddVariableToIterateOn(atIterator);
                    warewolfListIterator.AddVariableToIterateOn(paddingIterator);

                    listOfIterators.Add(dictionaryKey, new List<IWarewolfIterator> { inputIterator, atIterator, paddingIterator });
                    dictionaryKey++;
                }

                #endregion

                #region Iterate and Merge Data

                if(!allErrors.HasErrors())
                {
                    while(warewolfListIterator.HasMoreData())
                    {
                        int pos = 0;
                        foreach(var iterator in listOfIterators)
                        {
                            var val = warewolfListIterator.FetchNextValue(iterator.Value[0]);
                            var at = warewolfListIterator.FetchNextValue(iterator.Value[1]);
                            var pad = warewolfListIterator.FetchNextValue(iterator.Value[2]);

                            if(val != null)
                            {
                                if(at != null)
                                {
                                    if(pad != null)
                                    {
                                        if(MergeCollection[pos].MergeType == "Index")
                                        {
                                            if(string.IsNullOrEmpty(at))
                                            {
                                                allErrors.AddError(ErrorResource.BlankUSINGValue);
                                            }

                                            int atValue;
                                            if(!Int32.TryParse(at, out atValue) || atValue < 0)
                                            {
                                                allErrors.AddError(ErrorResource.USINGMustBeARealNumber);
                                            }
                                            if(pad.Length > 1)
                                            {
                                                allErrors.AddError(ErrorResource.PADDINGMustBeSingleCharecter);
                                            }
                                        }
                                        else
                                        {
                                            if(MergeCollection[pos].MergeType == "Chars" && string.IsNullOrEmpty(at))
                                            {
                                                allErrors.AddError(ErrorResource.BlankUSINGValue);
                                            }
                                        }
                                        mergeOperations.Merge(val, MergeCollection[pos].MergeType, at, pad, MergeCollection[pos].Alignment);
                                        pos++;
                                    }
                                }
                            }
                        }
                    }
                    if(!allErrors.HasErrors())
                    {
                        if(string.IsNullOrEmpty(Result))
                        {
                            AddDebugOutputItem(new DebugItemStaticDataParams("", ""));
                        }
                        else
                        {
                            dataObject.Environment.Assign(Result, mergeOperations.MergeData.ToString(), update);
                            allErrors.MergeErrors(errorResultTo);

                            if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                            {
                                AddDebugOutputItem(new DebugEvalResult(Result, "", dataObject.Environment, update));
                            }
                        }
                    }
                }

                #endregion Iterate and Merge Data
            }
            catch(Exception e)
            {
                Dev2Logger.Error("DSFDataMerge", e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                #region Handle Errors

                if(allErrors.HasErrors())
                {
                    if(dataObject.IsDebugMode())
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", Result, ""));
                    }
                    DisplayAndWriteError("DsfDataMergeActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                }

                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }

                #endregion
            }
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.MixedActivity;
        }

        #endregion

        #region Private Methods

        private void CleanArguments(IList<DataMergeDTO> args)
        {
            int count = 0;
            while(count < args.Count)
            {
                if(args[count].IsEmpty())
                {
                    args.RemoveAt(count);
                }
                else
                {
                    count++;
                }
            }
        }

        private void InsertToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["MergeCollection"];
            if(modelProperty != null)
            {
                ModelItemCollection mic = modelProperty.Collection;

                if(mic != null)
                {
                    List<DataMergeDTO> listOfValidRows = MergeCollection.Where(c => !c.CanRemove()).ToList();
                    if(listOfValidRows.Count > 0)
                    {
                        DataMergeDTO dataMergeDto = MergeCollection.Last(c => !c.CanRemove());
                        int startIndex = MergeCollection.IndexOf(dataMergeDto) + 1;
                        foreach(string s in listToAdd)
                        {
                            mic.Insert(startIndex, new DataMergeDTO(s, MergeCollection[startIndex - 1].MergeType, MergeCollection[startIndex - 1].At, startIndex + 1, MergeCollection[startIndex - 1].Padding, MergeCollection[startIndex - 1].Alignment));
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
            var modelProperty = modelItem.Properties["MergeCollection"];
            if(modelProperty != null)
            {
                ModelItemCollection mic = modelProperty.Collection;

                if(mic != null)
                {
                    int startIndex = 0;
                    string firstRowMergeType = MergeCollection[0].MergeType;
                    string firstRowPadding = MergeCollection[0].Padding;
                    string firstRowAlignment = MergeCollection[0].Alignment;
                    mic.Clear();
                    foreach(string s in listToAdd)
                    {
                        mic.Add(new DataMergeDTO(s, firstRowMergeType, string.Empty, startIndex + 1, firstRowPadding, firstRowAlignment));
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
            mic.Add(new DataMergeDTO(string.Empty, "None", string.Empty, startIndex + 1, " ", "Left To Right"));
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
                if(currentName != null && currentName.Contains("(") && currentName.Contains(")"))
                {
                    currentName = currentName.Remove(currentName.Contains(" (") ? currentName.IndexOf(" (", StringComparison.Ordinal) : currentName.IndexOf("(", StringComparison.Ordinal));
                }
                currentName = currentName + " (" + (count - 1) + ")";
                return currentName;
            }
            return null;
        }

        #endregion Private Methods

        #region Get Debug Inputs/Outputs





        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            return _debugInputs;
        }



        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }


        #endregion

        #region Get ForEach Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {
                    // locate all updates for this tuple
                    Tuple<string, string> t1 = t;
                    var items = MergeCollection.Where(c => !string.IsNullOrEmpty(c.InputVariable) && c.InputVariable.Equals(t1.Item1));

                    // issues updates
                    foreach(var a in items)
                    {
                        a.InputVariable = t.Item2;
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
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

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var items = MergeCollection.Where(c => !string.IsNullOrEmpty(c.InputVariable)).Select(c => c.InputVariable).Union(MergeCollection.Where(c => !string.IsNullOrEmpty(c.At)).Select(c => c.At)).ToArray();
            return GetForEachItems(items);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            var items = new string[1];
            if(!string.IsNullOrEmpty(Result))
            {
                items[0] = Result;
            }
            return GetForEachItems(items);
        }

        #endregion

        #region Implementation of ICollectionActivity

        public int GetCollectionCount()
        {
            return MergeCollection.Count(caseConvertTo => !caseConvertTo.CanRemove());
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

        public override List<string> GetOutputs()
        {
            return new List<string> { Result };
        }
    }
}
