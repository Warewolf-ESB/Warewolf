
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
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.StringTokenizer.Interfaces;
using Dev2.Data.Factories;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Interfaces;
using Dev2.Validation;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfDataSplitActivity : DsfActivityAbstract<string>, ICollectionActivity
    {
        #region Fields

        string _sourceString;
        string _datalistString = "";
        int _indexCounter = 1;
        private IList<DataSplitDTO> _resultsCollection;
        bool _reverseOrder;
        bool _skipBlankRows;

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

        public bool SkipBlankRows
        {
            get { return _skipBlankRows; }
            set
            {
                _skipBlankRows = value;
                OnPropertyChanged("SkipBlankRows");
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
            Guid dlId = dataObject.DataListID;
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors;
            _datalistString = compiler.ConvertFrom(dataObject.DataListID, DataListFormat.CreateFormat(GlobalConstants._Studio_XML), Dev2.DataList.Contract.enTranslationDepth.Shape, out errors).ToString();

            InitializeDebug(dataObject);
            try
            {
                var sourceString = SourceString ?? "";
                IBinaryDataListEntry expressionsEntry = compiler.Evaluate(dlId, enActionType.User, sourceString, false, out errors);

                if(dataObject.IsDebugMode())
                {
                    AddDebugInputItem(new DebugItemVariableParams(sourceString, "String to Split", expressionsEntry, dlId));
                    AddDebugInputItem(new DebugItemStaticDataParams(ReverseOrder ? "Backward" : "Forward", "Process Direction"));
                    AddDebugInputItem(new DebugItemStaticDataParams(SkipBlankRows ? "Yes" : "No", "Skip blank rows"));
                }
                CleanArguments(ResultsCollection);
                ResultsCollection.ToList().ForEach(a => IsSingleValueRule.ApplyIsSingleValueRule(a.OutputVariable, allErrors));
                if(ResultsCollection.Count > 0)
                {
                    if(dataObject.IsDebugMode())
                    {
                        AddDebug(ResultsCollection, compiler, dlId);
                    }

                    CheckIndex(sourceString);
                    allErrors.MergeErrors(errors);
                    IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);
                    IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                    bool singleInnerIteration = ArePureScalarTargets(ResultsCollection);
                    bool exit = false;
                    while(itr.HasMoreRecords())
                    {
                        IList<IBinaryDataListItem> cols = itr.FetchNextRowData();
                        foreach(IBinaryDataListItem c in cols)
                        {
                            // set up live flushing iterator details
                            toUpsert.HasLiveFlushing = true;
                            toUpsert.LiveFlushingLocation = dlId;

#pragma warning disable 219
                            int opCnt = 0;
#pragma warning restore 219
                            if(!string.IsNullOrEmpty(c.TheValue))
                            {
                                string val = c.TheValue;
                                var blankRows = new List<int>();
                                if(SkipBlankRows)
                                {
                                    var strings = val.Split(new[] { Environment.NewLine, "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                    var newSourceString = string.Join(Environment.NewLine, strings);
                                    val = newSourceString;
                                }
                                else
                                {

                                    var strings = val.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                                    for(int blankRow = 0; blankRow < strings.Length; blankRow++)
                                    {
                                        if(String.IsNullOrEmpty(strings[blankRow]))
                                        {
                                            blankRows.Add(blankRow);
                                        }
                                    }
                                }

                                IDev2Tokenizer tokenizer = CreateSplitPattern(ref val, ResultsCollection, compiler, dlId, out errors);
                                allErrors.MergeErrors(errors);

                                if(!allErrors.HasErrors())
                                {
                                    if(tokenizer != null)
                                    {
                                        int pos = 0;
                                        int end = (ResultsCollection.Count - 1);

                                        // track used tokens so we can adjust flushing ;)
                                        HashSet<string> usedTokens = new HashSet<string>();

                                        while(tokenizer.HasMoreOps() && !exit)
                                        {
                                            string tmp = tokenizer.NextToken();
                                            if(blankRows.Contains(opCnt) && blankRows.Count != 0)
                                            {
                                                tmp = tmp.Replace(Environment.NewLine, "");
                                                while(pos != end + 1)
                                                {
                                                    UpdateOutputVariableWithValue(pos, usedTokens, toUpsert, "");
                                                    pos++;
                                                }
                                                pos = CompletedRow(usedTokens, toUpsert, singleInnerIteration, ref opCnt, ref exit);
                                            }
                                            UpdateOutputVariableWithValue(pos, usedTokens, toUpsert, tmp);

                                            // Per pass
                                            if(pos == end)
                                            {
                                                //row has been processed
                                                pos = CompletedRow(usedTokens, toUpsert, singleInnerIteration, ref opCnt, ref exit);
                                            }
                                            else
                                            {
                                                pos++;
                                            }
                                        }

                                        // flush the final frame ;)

                                        toUpsert.FlushIterationFrame();
                                        toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                                    }
                                }
                            }
                        }
                    }
                    if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                    {
                        AddResultToDebug(compiler, dlId);
                    }
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error("DSFDataSplit", e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfDataSplitActivity", allErrors);
                    compiler.UpsertSystemTag(dlId, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }

                if(dataObject.IsDebugMode())
                {
                    if(hasErrors)
                    {
                        AddResultToDebug(compiler, dlId);
                    }
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        void AddResultToDebug(IDataListCompiler compiler, Guid dlId)
        {
            int innerCount = 1;
            foreach(DataSplitDTO dataSplitDto in ResultsCollection)
            {
                var outputVariable = ResultsCollection[innerCount - 1].OutputVariable;
                if(outputVariable.Contains("()."))
                {
                    outputVariable = outputVariable.Remove(outputVariable.IndexOf(".", StringComparison.Ordinal));
                    outputVariable = outputVariable.Replace("()", "(*)") + "]]";
                }
                ErrorResultTO errors;
                IBinaryDataListEntry binaryDataListEntry = compiler.Evaluate(dlId, enActionType.User, outputVariable, false, out errors);
                string expression = dataSplitDto.OutputVariable;
                if(expression.Contains("()."))
                {
                    expression = expression.Replace("().", "(*).");
                }

                AddDebugOutputItemFromEntry(expression, binaryDataListEntry, innerCount, dlId);
                innerCount++;
            }
        }


        static int CompletedRow(HashSet<string> usedTokens, IDev2DataListUpsertPayloadBuilder<string> toUpsert, bool singleInnerIteration, ref int opCnt, ref bool exit)
        {
            opCnt++;

            // clear token cache
            usedTokens.Clear();

            toUpsert.FlushIterationFrame();

            if(singleInnerIteration)
            {
                exit = true;
            }
            return 0;
        }


        void UpdateOutputVariableWithValue(int pos, HashSet<string> usedTokens, IDev2DataListUpsertPayloadBuilder<string> toUpsert, string tmp)
        {
            var dataSplitDto = ResultsCollection[pos];
            var outputVariable = dataSplitDto.OutputVariable;
            CheckIndex(outputVariable);

            CheckIndex(dataSplitDto.EscapeChar);
            if(!string.IsNullOrEmpty(outputVariable))
            {
                //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result

                // if it already exist, flush this round ;)
                if(!usedTokens.Add(outputVariable))
                {
                    toUpsert.FlushIterationFrame();
                }

                toUpsert.Add(outputVariable, tmp);

            }
        }

        static void CheckIndex(string toCheck)
        {
            if(!String.IsNullOrEmpty(toCheck) && DataListUtil.IsEvaluated(toCheck) && DataListUtil.IsValueRecordset(toCheck) && DataListUtil.GetRecordsetIndexType(toCheck) == enRecordsetIndexType.Numeric)
            {
                int index;
                if(!int.TryParse(DataListUtil.ExtractIndexRegionFromRecordset(toCheck), out index) || index < 0)
                {
                    throw new Exception(string.Format("Invalid numeric index for Recordset {0}", toCheck));
                }
            }
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.MixedActivity;
        }

        #endregion

        #region Private Methods

        private bool ArePureScalarTargets(IEnumerable<DataSplitDTO> args)
        {
            return args.All(arg => !DataListUtil.IsValueRecordset(arg.OutputVariable));
        }


        private void AddDebugOutputItemFromEntry(string expression, IBinaryDataListEntry value, int indexCount, Guid dlId)
        {
            DebugItem itemToAdd = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", indexCount.ToString(CultureInfo.InvariantCulture)), itemToAdd);
            AddDebugItem(new DebugItemVariableParams(expression, "", value, dlId), itemToAdd);
            _debugOutputs.Add(itemToAdd);
        }


        private void InsertToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ResultsCollection"];
            if(modelProperty != null)
            {
                ModelItemCollection mic = modelProperty.Collection;

                if(mic != null)
                {
                    List<DataSplitDTO> listOfValidRows = ResultsCollection.Where(c => !c.CanRemove()).ToList();
                    if(listOfValidRows.Count > 0)
                    {
                        DataSplitDTO dataSplitDto = ResultsCollection.Last(c => !c.CanRemove());
                        int startIndex = ResultsCollection.IndexOf(dataSplitDto) + 1;
                        foreach(string s in listToAdd)
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
        }

        private void AddToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ResultsCollection"];
            if(modelProperty != null)
            {
                ModelItemCollection mic = modelProperty.Collection;

                if(mic != null)
                {
                    int startIndex = 0;
                    string firstRowSplitType = ResultsCollection[0].SplitType;
                    string firstRowAt = ResultsCollection[0].At;
                    mic.Clear();
                    foreach(string s in listToAdd)
                    {
                        mic.Add(new DataSplitDTO(s, firstRowSplitType, firstRowAt, startIndex + 1));
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
            mic.Add(new DataSplitDTO(string.Empty, "Chars", string.Empty, startIndex + 1));
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

        private IDev2Tokenizer CreateSplitPattern(ref string stringToSplit, IEnumerable<DataSplitDTO> args, IDataListCompiler compiler, Guid dlId, out ErrorResultTO errors)
        {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder { ToTokenize = stringToSplit, ReverseOrder = ReverseOrder };
            errors = new ErrorResultTO();

            foreach(DataSplitDTO t in args)
            {
                var fieldName = t.OutputVariable;
                t.At = t.At ?? "";
                if(!string.IsNullOrEmpty(_datalistString))
                {
                    var isValidExpr = new IsValidExpressionRule(() => fieldName, _datalistString)
                    {
                        LabelText = fieldName
                    };

                    var errorInfo = isValidExpr.Check();
                    if(errorInfo != null)
                    {
                        errors.AddError(errorInfo.Message);
                        continue;
                    }
                }

                IBinaryDataListEntry entry;
                string error;
                switch(t.SplitType)
                {
                    case "Index":
                        try
                        {
                            entry = compiler.Evaluate(dlId, enActionType.User, t.At, false, out errors);
                            string index = DataListUtil.GetValueAtIndex(entry, 1, out error);
                            int indexNum = Convert.ToInt32(index);
                            if(indexNum > 0)
                            {
                                dtb.AddIndexOp(indexNum);
                            }
                        }
                        catch(Exception ex)
                        {
                            errors.AddError(ex.Message);
                        }
                        break;
                    case "End":
                        dtb.AddEoFOp();
                        break;
                    case "Space":
                        dtb.AddTokenOp(" ", t.Include);
                        break;
                    case "Tab":
                        dtb.AddTokenOp("\t", t.Include);
                        break;
                    case "New Line":
                        if(stringToSplit.Contains("\r\n"))
                        {
                            dtb.AddTokenOp("\r\n", t.Include);
                        }
                        else if(stringToSplit.Contains("\n"))
                        {
                            dtb.AddTokenOp("\n", t.Include);
                        }
                        else if(stringToSplit.Contains("\r"))
                        {
                            dtb.AddTokenOp("\r", t.Include);
                        }
                        break;
                    case "Chars":
                        if(!string.IsNullOrEmpty(t.At))
                        {
                            entry = compiler.Evaluate(dlId, enActionType.User, t.At, false, out errors);

                            string val = DataListUtil.GetValueAtIndex(entry, 1, out error);
                            string escape = t.EscapeChar;
                            if(!String.IsNullOrEmpty(escape))
                            {
                                entry = compiler.Evaluate(dlId, enActionType.User, t.EscapeChar, false, out errors);
                                escape = DataListUtil.GetValueAtIndex(entry, 1, out error);
                            }

                            dtb.AddTokenOp(val, t.Include, escape);
                        }
                        break;
                }
                _indexCounter++;
            }
            return string.IsNullOrEmpty(dtb.ToTokenize) || errors.HasErrors() ? null : dtb.Generate();
        }

        private void AddDebug(IEnumerable<DataSplitDTO> resultCollection, IDataListCompiler compiler, Guid dlId)
        {
            foreach(DataSplitDTO t in resultCollection)
            {
                IBinaryDataListEntry entry;
                ErrorResultTO errors;

                DebugItem debugItem = new DebugItem();
                AddDebugItem(new DebugItemStaticDataParams("", _indexCounter.ToString(CultureInfo.InvariantCulture)), debugItem);
                AddDebugItem(DebugUtil.EvaluateEmptyRecordsetBeforeAddingToDebugOutput(t.OutputVariable, "", dlId), debugItem);
                AddDebugItem(new DebugItemStaticDataParams(t.SplitType, "With"), debugItem);

                switch(t.SplitType)
                {
                    case "Index":
                        entry = compiler.Evaluate(dlId, enActionType.User, t.At, false, out errors);
                        AddDebugItem(new DebugItemVariableParams(t.At, "Using", entry, dlId), debugItem);
                        AddDebugItem(new DebugItemStaticDataParams(t.Include ? "Yes" : "No", "Include"), debugItem);
                        break;
                    case "End":
                        AddDebugItem(new DebugItemStaticDataParams(t.Include ? "Yes" : "No", "Include"), debugItem);
                        break;
                    case "Space":
                        AddDebugItem(new DebugItemStaticDataParams(t.Include ? "Yes" : "No", "Include"), debugItem);
                        break;
                    case "Tab":
                        AddDebugItem(new DebugItemStaticDataParams(t.Include ? "Yes" : "No", "Include"), debugItem);
                        break;
                    case "New Line":
                        AddDebugItem(new DebugItemStaticDataParams(t.Include ? "Yes" : "No", "Include"), debugItem);
                        break;
                    case "Chars":
                        entry = compiler.Evaluate(dlId, enActionType.User, t.At, false, out errors);
                        AddDebugItem(new DebugItemVariableParams(t.At, "Using", entry, dlId), debugItem);
                        AddDebugItem(new DebugItemStaticDataParams(t.Include ? "Yes" : "No", "Include"), debugItem);
                        AddDebugItem(new DebugItemStaticDataParams(t.EscapeChar, "Escape"), debugItem);
                        break;
                }
                _indexCounter++;
                _debugInputs.Add(debugItem);
            }
        }

        private void CleanArguments(IList<DataSplitDTO> args)
        {
            int count = 0;
            while(count < args.Count)
            {
                if(string.IsNullOrEmpty(args[count].OutputVariable))
                {
                    if(args[count].SplitType == "Index" && string.IsNullOrEmpty(args[count].At) ||
                       args[count].SplitType == "Chars" && string.IsNullOrEmpty(args[count].At))
                    {
                        args.RemoveAt(count);
                    }
                    else
                    {
                        count++;
                    }
                }
                else
                {
                    count++;
                }
            }
        }
        #endregion Private Methods

        #region Get Debug Inputs/Outputs

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        #endregion

        #region GetDebugOutputs

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
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
                foreach(Tuple<string, string> t in updates)
                {
                    // locate all updates for this tuple
                    Tuple<string, string> t1 = t;
                    var items = ResultsCollection.Where(c => !string.IsNullOrEmpty(c.At) && c.At.Equals(t1.Item1));

                    // issues updates
                    foreach(var a in items)
                    {
                        a.At = t.Item2;
                    }

                    if(SourceString == t.Item1)
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
                foreach(Tuple<string, string> t in updates)
                {
                    // locate all updates for this tuple
                    Tuple<string, string> t1 = t;
                    var items = ResultsCollection.Where(c => !string.IsNullOrEmpty(c.OutputVariable) && c.OutputVariable.Equals(t1.Item1));

                    // issues updates
                    foreach(var a in items)
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
            return ResultsCollection.Count(caseConvertTo => !caseConvertTo.CanRemove());
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
