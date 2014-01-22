using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.StringTokenizer.Interfaces;
using Dev2.Data.Factories;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Interfaces;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfDataSplitActivity : DsfActivityAbstract<string>, ICollectionActivity
    {
        #region Fields

        string _sourceString;

        int _indexCounter = 1;
        private IList<DataSplitDTO> _resultsCollection;
        bool _reverseOrder;

        #endregion

        #region Properties

        public IList<DataSplitDTO> ResultsCollection
        {
            get
            {
                return _resultsCollection;
            }
            set
            {
                _resultsCollection = value;
                OnPropertyChanged("ResultsCollection");
            }
        }

        public bool ReverseOrder
        {
            get
            {
                return _reverseOrder;
            }
            set
            {
                _reverseOrder = value;
                OnPropertyChanged("ReverseOrder");
            }
        }

        public string SourceString
        {
            get
            {
                return _sourceString;
            }
            set
            {
                _sourceString = value;
                OnPropertyChanged("SourceString");
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

        public DsfDataSplitActivity()
            : base("Data Split")
        {
            ResultsCollection = new List<DataSplitDTO>();
        }

        #endregion

        #region Overridden NativeActivity Methods

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            _indexCounter = 1;
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            Guid dlID = dataObject.DataListID;
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors;

            try
            {
                CleanArguments(ResultsCollection);

                if (ResultsCollection.Count > 0)
                {
                    IBinaryDataListEntry expressionsEntry = compiler.Evaluate(dlID, enActionType.User, SourceString, false, out errors);

                    if (dataObject.IsDebugMode())
                    {
                        AddSourceStringDebugInputItem(SourceString, expressionsEntry, dlID);
                    }

                    allErrors.MergeErrors(errors);
                    IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);
                    IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                    string tmp;

                    bool singleInnerIteration = ArePureScalarTargets(ResultsCollection);
                    bool exit = false;
                    while (itr.HasMoreRecords())
                    {
                        IList<IBinaryDataListItem> cols = itr.FetchNextRowData();
                        foreach (IBinaryDataListItem c in cols)
                        {
                            // set up live flushing iterator details
                            toUpsert.HasLiveFlushing = true;
                            toUpsert.LiveFlushingLocation = dlID;

                            if (!string.IsNullOrEmpty(c.TheValue))
                            {
                                string val = c.TheValue;
                                IDev2Tokenizer tokenizer = CreateSplitPattern(ref val, ResultsCollection, compiler, dlID, dataObject.IsDebug);
                                int opCnt = 0;
                                int pos = 0;
                                int end = (ResultsCollection.Count - 1);
                                
                                // track used tokens so we can adjust flushing ;)
                                HashSet<string> usedTokens = new HashSet<string>();

                                while (tokenizer.HasMoreOps() && !exit)
                                {
                                     tmp = tokenizer.NextToken();

                                    var dataSplitDto = ResultsCollection[pos];
                                    var outputVariable = dataSplitDto.OutputVariable;
                                    if (!string.IsNullOrEmpty(outputVariable))
                                    {
                                        //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
                                        foreach(var region in DataListCleaningUtils.SplitIntoRegions(outputVariable))
                                        {
                                            // if it already exist, flush this round ;)
                                            if (!usedTokens.Add(region))
                                            {
                                                toUpsert.FlushIterationFrame();
                                            }
                                            
                                            toUpsert.Add(region, tmp);
                                        }
                                    }

                                    // Per pass
                                    if (pos == end)
                                    {
                                        pos = 0;
                                        opCnt++;                                     

                                        // clear token cache
                                        usedTokens.Clear();

                                        toUpsert.FlushIterationFrame();

                                        if (singleInnerIteration)
                                        {
                                            exit = true;
                                        }
                                    }
                                    else
                                    {
                                        pos++;
                                    }
                                }

                                // flush the final frame ;)

                                toUpsert.FlushIterationFrame();

                                if (dataObject.IsDebugMode())
                                {
                                    int innerCount = 1;
                                    foreach(DataSplitDTO dataSplitDto in ResultsCollection)
                                    {
                                        var outputVariable = ResultsCollection[innerCount-1].OutputVariable;
                                    if (outputVariable.Contains("()."))
                                    {
                                            outputVariable = outputVariable.Remove(outputVariable.IndexOf(".", StringComparison.Ordinal));
                                        outputVariable = outputVariable.Replace("()", "(*)")+"]]";
                                    }
                                        IBinaryDataListEntry binaryDataListEntry = compiler.Evaluate(dlID, enActionType.User, outputVariable, false, out errors);
                                        string expression = dataSplitDto.OutputVariable;
                                        if (expression.Contains("()."))
                                        {
                                            expression = expression.Replace("().", "(*).");
                                        }

                                        AddDebugOutputItemFromEntry(expression, binaryDataListEntry, innerCount, dlID);
                                        innerCount++;
                                    }
                                    
                                }

                                toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);

                            }
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
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
                // Handle Errors
                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfDataSplitActivity", allErrors);
                    compiler.UpsertSystemTag(dlID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
            }
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.MixedActivity;
        }

        #endregion

        #region Private Methods

        private bool ArePureScalarTargets(IList<DataSplitDTO> args)
        {

            foreach (var arg in args)
            {
                if(DataListUtil.IsValueRecordset(arg.OutputVariable))
                {
                    return false;
                }    
            }

            return true;
        }

        private void AddSourceStringDebugInputItem(string expression, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "String To Split" });

            itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input));

            _debugInputs.Add(itemToAdd);
        }

        private void AddDebugInputItem(string expression, string splitType, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            DebugItem itemToAdd = new DebugItem();

            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = _indexCounter.ToString(CultureInfo.InvariantCulture) });
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Split Using" });
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = splitType });

            if (valueEntry != null)
            {
                itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input));
            }

            _debugInputs.Add(itemToAdd);
        }


        private void AddDebugOutputItemFromEntry(string expression, IBinaryDataListEntry value, int indexCount, Guid dlId)
        {
            DebugItem itemToAdd = new DebugItem();

            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = indexCount.ToString(CultureInfo.InvariantCulture) });

            itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, value, dlId, enDev2ArgumentType.Output));
            _debugOutputs.Add(itemToAdd);
        }

        private void InsertToCollection(IList<string> listToAdd, ModelItem modelItem)
        {
            ModelItemCollection mic = modelItem.Properties["ResultsCollection"].Collection;

            if (mic != null)
            {
                List<DataSplitDTO> listOfValidRows = ResultsCollection.Where(c => !c.CanRemove()).ToList();
                if (listOfValidRows.Count > 0)
                {
                    int startIndex = ResultsCollection.Last(c => !c.CanRemove()).IndexNumber;
                    foreach (string s in listToAdd)
                    {
                        mic.Insert(startIndex, new DataSplitDTO(s, ResultsCollection[startIndex - 1].SplitType, ResultsCollection[startIndex - 1].At, startIndex + 1));
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
            ModelItemCollection mic = modelItem.Properties["ResultsCollection"].Collection;

            if (mic != null)
            {
                int startIndex = 0;
                string firstRowSplitType = ResultsCollection[0].SplitType;
                string firstRowAt = ResultsCollection[0].At;
                mic.Clear();
                foreach (string s in listToAdd)
                {
                    mic.Add(new DataSplitDTO(s, firstRowSplitType, firstRowAt, startIndex + 1));
                    startIndex++;
                }
                CleanUpCollection(mic, modelItem, startIndex);
            }
        }

        private void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
        {
            if (startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }
            mic.Add(new DataSplitDTO(string.Empty, "Chars", string.Empty, startIndex + 1));
            modelItem.Properties["DisplayName"].SetValue(CreateDisplayName(modelItem, startIndex + 1));
        }

        private string CreateDisplayName(ModelItem modelItem, int count)
        {
            string currentName = modelItem.Properties["DisplayName"].ComputedValue as string;
            if (currentName.Contains("(") && currentName.Contains(")"))
            {
                if (currentName.Contains(" ("))
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

        private IDev2Tokenizer CreateSplitPattern(ref string StringToSplit, IList<DataSplitDTO> Args, IDataListCompiler compiler, Guid DlID, bool isDebug = false)
        {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();
            ErrorResultTO errors = new ErrorResultTO();
            string error = string.Empty;
            dtb.ToTokenize = StringToSplit;

            for (int i = 0; i < Args.Count; i++)
            {
                IBinaryDataListEntry entry = null;
                switch (Args[i].SplitType)
                {
                    case "Index":
                        try
                        {
                            entry = compiler.Evaluate(DlID, enActionType.User, Args[i].At, true, out errors);
                            string index = DataListUtil.GetValueAtIndex(entry, 1, out error);
                            int indexNum = Convert.ToInt32(index);
                            if(indexNum > 0)
                            {
                                dtb.AddIndexOp(indexNum);
                            }
                        }
                        catch(Exception ex)
                        {
                            this.LogError(ex);
                        }
                        break;

                    case "End":
                        dtb.AddEoFOp();
                        break;

                    case "Space":
                        dtb.AddTokenOp(" ", Args[i].Include);
                        break;

                    case "Tab":
                        dtb.AddTokenOp("\t", Args[i].Include);
                        break;

                    case "New Line":
                        if (StringToSplit.Contains("\r\n"))
                        {
                            dtb.AddTokenOp("\r\n", Args[i].Include);
                        }
                        else if (StringToSplit.Contains("\n"))
                        {
                            dtb.AddTokenOp("\n", Args[i].Include);
                        }
                        else if (StringToSplit.Contains("\r"))
                        {
                            dtb.AddTokenOp("\r", Args[i].Include);
                        }
                        break;

                    case "Chars":
                        if (!string.IsNullOrEmpty(Args[i].At))
                        {
                            entry = compiler.Evaluate(DlID, enActionType.User, Args[i].At, true, out errors);
                            string val = DataListUtil.GetValueAtIndex(entry, 1, out error);
                            dtb.AddTokenOp(val, Args[i].Include);
                        }
                        break;
                }

                if (isDebug)
                {
                    AddDebugInputItem(Args[i].At, Args[i].SplitType, entry, DlID);
                }

                _indexCounter++;

            }

            return dtb.Generate();
        }

        private void CleanArguments(IList<DataSplitDTO> Args)
        {
            int count = 0;
            while (count < Args.Count)
            {
                if (Args[count].SplitType == "Index" && string.IsNullOrEmpty(Args[count].At) || Args[count].SplitType == "Chars" && string.IsNullOrEmpty(Args[count].At))
                {
                    Args.RemoveAt(count);
                }
                else
                {
                    count++;
                }
            }
        }

        #endregion Private Methods

        #region Overridden ActivityAbstact Methods

        public override IBinaryDataList GetWizardData()
        {
            string error = string.Empty;
            IBinaryDataList result = Dev2BinaryDataListFactory.CreateDataList();
            string recordsetName = "ResultsCollection";
            result.TryCreateScalarTemplate(string.Empty, "SourceString", string.Empty, true, out error);
            result.TryCreateScalarValue(SourceString, "SourceString", out error);
            result.TryCreateScalarTemplate(string.Empty, "ReverseOrder", string.Empty, true, out error);
            result.TryCreateScalarValue(ReverseOrder.ToString(), "ReverseOrder", out error);
            result.TryCreateRecordsetTemplate(recordsetName, string.Empty, new List<Dev2Column>() { DataListFactory.CreateDev2Column("SplitType", string.Empty), DataListFactory.CreateDev2Column("At", string.Empty), DataListFactory.CreateDev2Column("Include", string.Empty), DataListFactory.CreateDev2Column("Result", string.Empty) }, true, out error);
            foreach (DataSplitDTO item in ResultsCollection)
            {
                result.TryCreateRecordsetValue(item.SplitType, "SplitType", recordsetName, item.IndexNumber, out error);
                result.TryCreateRecordsetValue(item.At, "At", recordsetName, item.IndexNumber, out error);
                result.TryCreateRecordsetValue(item.Include.ToString(), "Include", recordsetName, item.IndexNumber, out error);
                result.TryCreateRecordsetValue(item.OutputVariable, "Result", recordsetName, item.IndexNumber, out error);
            }
            return result;
        }

        #endregion Overridden ActivityAbstact Methods

        #region Get Debug Inputs/Outputs

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        #endregion

        #region GetDebugOutputs

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion

        #endregion

        #region Get ForEach Inputs/Ouputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach (Tuple<string, string> t in updates)
                {
                    // locate all updates for this tuple
                    var items = ResultsCollection.Where(c => !string.IsNullOrEmpty(c.At) && c.At.Equals(t.Item1));

                    // issues updates
                    foreach (var a in items)
                    {
                        a.At = t.Item2;
                    }

                    if (SourceString == t.Item1)
                    {
                        SourceString = t.Item2;
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach (Tuple<string, string> t in updates)
                {
                    // locate all updates for this tuple
                    var items = ResultsCollection.Where(c => !string.IsNullOrEmpty(c.OutputVariable) && c.OutputVariable.Equals(t.Item1));

                    // issues updates
                    foreach (var a in items)
                    {
                        a.OutputVariable = t.Item2;
                    }
                }
            }
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var items = (new[] { SourceString }).Union(ResultsCollection.Where(c => !string.IsNullOrEmpty(c.At)).Select(c => c.At)).ToArray();
            return GetForEachItems(items);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            var items = ResultsCollection.Where(c => !string.IsNullOrEmpty(c.OutputVariable)).Select(c => c.OutputVariable).ToArray();
            return GetForEachItems(items);
        }

        #endregion

        #region Implementation of ICollectionActivity

        public int GetCollectionCount()
        {
            return ResultsCollection.Count(caseConvertTO => !caseConvertTO.CanRemove());
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
