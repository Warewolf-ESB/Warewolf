using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Data.Factories;
using Dev2.Data.Operations;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Interfaces;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfDataMergeActivity : DsfActivityAbstract<string>, ICollectionActivity
    {
        #region Class Members

        private string _result;
        int _indexCounter = 1;

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
            _indexCounter = 1;
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IDev2MergeOperations mergeOperations = new Dev2MergeOperations();
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = DataListExecutionID.Get(context);

            try
            {
                CleanArguments(MergeCollection);

                if(MergeCollection.Count > 0)
                {
                    IDev2IteratorCollection iteratorCollection = Dev2ValueObjectFactory.CreateIteratorCollection();
                    IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                    allErrors.MergeErrors(errors);
                    Dictionary<int, List<IDev2DataListEvaluateIterator>> listOfIterators = new Dictionary<int, List<IDev2DataListEvaluateIterator>>();

                    #region Create a iterator for each row in the data grid in the designer so that the right iteration happen on the data

                    int dictionaryKey = 0;
                    foreach(DataMergeDTO row in MergeCollection)
                    {
                        IBinaryDataListEntry expressionsEntry = compiler.Evaluate(executionId, enActionType.User, row.InputVariable, false, out errors);
                        allErrors.MergeErrors(errors);

                        IBinaryDataListEntry atEntry = compiler.Evaluate(executionId, enActionType.User, row.At, false, out errors);
                        allErrors.MergeErrors(errors);

                        if(dataObject.IsDebugMode())
                        {
                            AddDebugInputItem(row.InputVariable, row.At, row.MergeType, expressionsEntry, atEntry, executionId);
                        }
                        IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);
                        IDev2DataListEvaluateIterator atItr = Dev2ValueObjectFactory.CreateEvaluateIterator(atEntry);

                        iteratorCollection.AddIterator(itr);
                        iteratorCollection.AddIterator(atItr);

                        listOfIterators.Add(dictionaryKey, new List<IDev2DataListEvaluateIterator> { itr, atItr });
                        dictionaryKey++;
                    }

                    #endregion

                    #region Iterate and Merge Data

                    while(iteratorCollection.HasMoreData())
                    {
                        int pos = 0;
                        foreach(var iterator in listOfIterators)
                        {

                            var val = iteratorCollection.FetchNextRow(iterator.Value[0]);
                            var at = iteratorCollection.FetchNextRow(iterator.Value[1]);

                            if(val != null)
                            {
                                if(at != null)
                                {
                                    if((MergeCollection[pos].MergeType == "Index" || MergeCollection[pos].MergeType == "Chars") && string.IsNullOrEmpty(at.TheValue))
                                    {
                                        allErrors.AddError("The At value cannot be blank.");
                                    }
                                    mergeOperations.Merge(val.TheValue, MergeCollection[pos].MergeType, at.TheValue, MergeCollection[pos].Padding, MergeCollection[pos].Alignment);
                                    pos++;
                                }
                            }
                        }
                    }

                    #endregion Iterate and Merge Data

                    #region Add Result to DataList

                    //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
                    foreach(var region in DataListCleaningUtils.SplitIntoRegions(Result))
                    {
                        toUpsert.Add(region, mergeOperations.MergeData.ToString());
                        if(dataObject.IsDebugMode())
                        {
                            AddDebugOutputItem(region, mergeOperations.MergeData.ToString(), executionId);
                        }

                        toUpsert.FlushIterationFrame();
                        compiler.Upsert(executionId, toUpsert, out errors);
                        allErrors.MergeErrors(errors);
                    }

                    #endregion Add Result to DataList
                }
            }
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                #region Handle Errors

                if(allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfDataMergeActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
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

        private void AddDebugInputItem(string expression, string atExpression, string mergeType, IBinaryDataListEntry inputValueEntry, IBinaryDataListEntry atValueEntry, Guid executionId)
        {
            DebugItem itemToAdd = new DebugItem();

            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = _indexCounter.ToString(CultureInfo.InvariantCulture) });
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Merge" });


            if(inputValueEntry != null)
            {
                itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, inputValueEntry, executionId, enDev2ArgumentType.Input));
            }

            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "With" });
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = mergeType });

            if(inputValueEntry != null)
            {
                itemToAdd.AddRange(CreateDebugItemsFromEntry(atExpression, atValueEntry, executionId, enDev2ArgumentType.Input));
            }

            _debugInputs.Add(itemToAdd);
        }

        private void AddDebugOutputItem(string expression, string value, Guid dlId)
        {
            DebugItem itemToAdd = new DebugItem();

            itemToAdd.AddRange(CreateDebugItemsFromString(expression, value, dlId, 0, enDev2ArgumentType.Output));
            _debugOutputs.Add(itemToAdd);
        }

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

        private void InsertToCollection(IList<string> listToAdd, ModelItem modelItem)
        {
            ModelItemCollection mic = modelItem.Properties["MergeCollection"].Collection;

            if(mic != null)
            {
                List<DataMergeDTO> listOfValidRows = MergeCollection.Where(c => !c.CanRemove()).ToList();
                if(listOfValidRows.Count > 0)
                {
                    int startIndex = MergeCollection.Last(c => !c.CanRemove()).IndexNumber;
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

        private void AddToCollection(IList<string> listToAdd, ModelItem modelItem)
        {
            ModelItemCollection mic = modelItem.Properties["MergeCollection"].Collection;

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

        private void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
        {
            if(startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }
            mic.Add(new DataMergeDTO(string.Empty, "None", string.Empty, startIndex + 1, " ", "Left To Right"));
            modelItem.Properties["DisplayName"].SetValue(CreateDisplayName(modelItem, startIndex + 1));
        }

        private string CreateDisplayName(ModelItem modelItem, int count)
        {
            string currentName = modelItem.Properties["DisplayName"].ComputedValue as string;
            if(currentName.Contains("(") && currentName.Contains(")"))
            {
                if(currentName.Contains(" ("))
                {
                    currentName = currentName.Remove(currentName.IndexOf(" ("));
                }
                else
                {
                    currentName = currentName.Remove(currentName.IndexOf("("));
                }
            }
            currentName = currentName + " (" + (count - 1) + ")";
            return currentName;
        }

        #endregion Private Methods

        #region Overridden ActivityAbstact Methods

        public override IBinaryDataList GetWizardData()
        {
            string error;
            IBinaryDataList result = Dev2BinaryDataListFactory.CreateDataList();
            const string recordsetName = "MergeCollection";
            result.TryCreateScalarValue(Result, "Result", out error);
            result.TryCreateRecordsetTemplate(recordsetName, string.Empty, new List<Dev2Column> { DataListFactory.CreateDev2Column("MergeType", string.Empty), DataListFactory.CreateDev2Column("At", string.Empty), DataListFactory.CreateDev2Column("Result", string.Empty) }, true, out error);
            foreach(DataMergeDTO item in MergeCollection)
            {
                result.TryCreateRecordsetValue(item.MergeType, "MergeType", recordsetName, item.IndexNumber, out error);
                result.TryCreateRecordsetValue(item.At, "At", recordsetName, item.IndexNumber, out error);
            }
            return result;
        }

        #endregion Overridden ActivityAbstact Methods

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
                    var items = MergeCollection.Where(c => !string.IsNullOrEmpty(c.InputVariable) && c.InputVariable.Equals(t.Item1));

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
            if(updates != null && updates.Count == 1)
            {
                Result = updates[0].Item2;
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
            return MergeCollection.Count(caseConvertTO => !caseConvertTO.CanRemove());
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
