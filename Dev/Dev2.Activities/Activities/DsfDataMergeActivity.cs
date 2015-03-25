
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
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Factories;
using Dev2.Data.Operations;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Interfaces;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
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

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

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
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IDev2MergeOperations mergeOperations = new Dev2MergeOperations();
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errorResultTo = new ErrorResultTO();
            Guid executionId = DataListExecutionID.Get(context);
            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
            toUpsert.IsDebug = dataObject.IsDebugMode();
            toUpsert.ResourceID = dataObject.ResourceID;

            InitializeDebug(dataObject);
            try
            {
                CleanArguments(MergeCollection);

                if(MergeCollection.Count <= 0)
                {
                    return;
                }
                IWarewolfListIterator warewolfListIterator = new WarewolfListIterator(dataObject.Environment);
                allErrors.MergeErrors(errorResultTo);
                Dictionary<int, List<string>> listOfIterators = new Dictionary<int, List<string>>();

                #region Create a iterator for each row in the data grid in the designer so that the right iteration happen on the data

                int dictionaryKey = 0;
                foreach(DataMergeDTO row in MergeCollection)
                {
                    
//                    var fieldName = row.InputVariable;
//                    var splitIntoRegions = DataListCleaningUtils.FindAllLanguagePieces(fieldName);
//                    var datalist = compiler.ConvertFrom(dataObject.DataListID, DataListFormat.CreateFormat(GlobalConstants._Studio_XML), enTranslationDepth.Shape, out errorResultTo).ToString();
//                    if(!string.IsNullOrEmpty(datalist))
//                    {
//                        foreach(var region in splitIntoRegions)
//                        {
//                            var r = DataListUtil.IsValueRecordset(region) ? DataListUtil.ReplaceRecordsetIndexWithBlank(region) : region;
//                            var isValidExpr = new IsValidExpressionRule(() => r, datalist)
//                            {
//                                LabelText = fieldName
//                            };
//
//                            var errorInfo = isValidExpr.Check();
//                            if(errorInfo != null)
//                            {
//                                row.InputVariable = "";
//                                errorResultTo.AddError(errorInfo.Message);
//                            }
//                            allErrors.MergeErrors(errorResultTo);
//                        }
//                    }

                    allErrors.MergeErrors(errorResultTo);

                    if(dataObject.IsDebugMode())
                    {
                        DebugItem debugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("", (MergeCollection.IndexOf(row) + 1).ToString(CultureInfo.InvariantCulture)), debugItem);
                        AddDebugItem(new DebugEvalResult(row.InputVariable, "",dataObject.Environment), debugItem);
                        AddDebugItem(new DebugItemStaticDataParams(row.MergeType, "With"), debugItem);
                        AddDebugItem(new DebugEvalResult(row.At, "Using", dataObject.Environment), debugItem);
                        AddDebugItem(new DebugEvalResult(row.Padding, "Pad", dataObject.Environment), debugItem);

                        //Old workflows don't have this set. 
                        if(row.Alignment == null)
                        {
                            row.Alignment = string.Empty;
                        }

                        AddDebugItem(DataListUtil.IsEvaluated(row.Alignment) ? new DebugItemStaticDataParams("", row.Alignment, "Align") : new DebugItemStaticDataParams(row.Alignment, "Align"), debugItem);

                        _debugInputs.Add(debugItem);
                    }
                    warewolfListIterator.AddVariableToIterateOn(row.InputVariable);
                    warewolfListIterator.AddVariableToIterateOn(row.At);
                    warewolfListIterator.AddVariableToIterateOn(row.Padding);

                    listOfIterators.Add(dictionaryKey, new List<string> { row.InputVariable, row.At, row.Padding });
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
                                                allErrors.AddError("The 'Using' value cannot be blank.");
                                            }

                                            int atValue;
                                            if(!Int32.TryParse(at, out atValue) || atValue < 0)
                                            {
                                                allErrors.AddError("The 'Using' value must be a real number.");
                                            }
                                            if(pad.Length > 1)
                                            {
                                                allErrors.AddError("'Padding' must be a single character");
                                            }
                                        }
                                        else
                                        {
                                            if(MergeCollection[pos].MergeType == "Chars" && string.IsNullOrEmpty(at))
                                            {
                                                allErrors.AddError("The 'Using' value cannot be blank.");
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
                            //var rule = new IsSingleValueRule(() => Result);
//                            var single = rule.Check();
//                            if(single != null)
//                            {
//                                allErrors.AddError(single.Message);
//                            }
//                            else
                            {
                                dataObject.Environment.Assign(Result, mergeOperations.MergeData.ToString());                               
                                allErrors.MergeErrors(errorResultTo);

                                if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                                {
                                    AddDebugOutputItem(new DebugEvalResult(Result,"",dataObject.Environment));                                    
                                }
                            }
                        }
                    }
                }

                #endregion Iterate and Merge Data

            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error("DSFDataMerge", e);
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
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errorResultTo);
                    compiler.Upsert(executionId, Result, (string)null, out errorResultTo);
                }

                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
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
                if(currentName != null && (currentName.Contains("(") && currentName.Contains(")")))
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


        #endregion

        #region Get ForEach Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
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

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var items = (MergeCollection.Where(c => !string.IsNullOrEmpty(c.InputVariable)).Select(c => c.InputVariable).Union(MergeCollection.Where(c => !string.IsNullOrEmpty(c.At)).Select(c => c.At))).ToArray();
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
    }
}
