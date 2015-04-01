
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
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.StringTokenizer.Interfaces;
using Dev2.Data;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Interfaces;
using Dev2.Validation;
using Warewolf.Storage;
using WarewolfParserInterop;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfDataSplitActivity : DsfActivityAbstract<string>, ICollectionActivity
    {
        #region Fields

        string _sourceString;
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
            ErrorResultTO allErrors = new ErrorResultTO();
            var env = dataObject.Environment;
            WarewolfListIterator iter = new WarewolfListIterator();
           
            InitializeDebug(dataObject);
            try
            {
                var sourceString = SourceString ?? "";
                if (dataObject.IsDebugMode())
                {
                    AddDebugInputItem(new DebugEvalResult(sourceString, "String to Split",env));
                    AddDebugInputItem(new DebugItemStaticDataParams(ReverseOrder ? "Backward" : "Forward", "Process Direction"));
                    AddDebugInputItem(new DebugItemStaticDataParams(SkipBlankRows ? "Yes" : "No", "Skip blank rows"));
                    AddDebug(ResultsCollection, dataObject.Environment);
                }
                var res = new WarewolfIterator( env.Eval(sourceString));
                iter.AddVariableToIterateOn(res);
                IDictionary<string,int> positions = new Dictionary<string, int>();
                CleanArguments(ResultsCollection);
                ResultsCollection.ToList().ForEach(a =>
                {
                    if (!positions.ContainsKey(a.OutputVariable))
                        positions.Add(a.OutputVariable, 1);
                    IsSingleValueRule.ApplyIsSingleValueRule(a.OutputVariable, allErrors);
                });
                bool singleInnerIteration = ArePureScalarTargets(ResultsCollection);
                var resultsEnumerator = ResultsCollection.GetEnumerator();
                var debugDictionary = new Dictionary<string, List<DebugItem>>();
                while(res.HasMoreData())
                {
                    const int OpCnt = 0;
                    
                    var item = res.GetNextValue(); // item is the thing we split on
                    if (!string.IsNullOrEmpty(item))
                    {
                        string val = item;
                        var blankRows = new List<int>();
                        if (SkipBlankRows)
                        {
                            var strings = val.Split(new[] { Environment.NewLine, "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                            var newSourceString = string.Join(Environment.NewLine, strings);
                            val = newSourceString;
                        }
                        else
                        {

                            var strings = val.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                            for (int blankRow = 0; blankRow < strings.Length; blankRow++)
                            {
                                if (String.IsNullOrEmpty(strings[blankRow]))
                                {
                                    blankRows.Add(blankRow);
                                }
                            }
                        }

                        ErrorResultTO errors;
                        IDev2Tokenizer tokenizer = CreateSplitPattern(ref val, ResultsCollection, env, out errors);
                        allErrors.MergeErrors(errors);

                        if (!allErrors.HasErrors())
                        {
                            if (tokenizer != null)
                            {
                                int pos = 0;
                                int end = (ResultsCollection.Count - 1);
                                
                                // track used tokens so we can adjust flushing ;)
                                while (tokenizer.HasMoreOps())
                                {
                                    var currentval = resultsEnumerator.MoveNext(); 
                                    if(!currentval)
                                    {
                                        if(singleInnerIteration)
                                        {
                                            break;
                                        }
                                        resultsEnumerator.Reset();
                                        resultsEnumerator.MoveNext(); 
                                    }
                                    string tmp = tokenizer.NextToken();

                                    if(tmp.StartsWith(Environment.NewLine) && !SkipBlankRows)
                                    {

                                        resultsEnumerator.Reset();
                                        while(resultsEnumerator.MoveNext())
                                        {
                                            var tovar = resultsEnumerator.Current.OutputVariable;
                                            if (!String.IsNullOrEmpty(tovar))
                                            {
                                                var assignToVar = ExecutionEnvironment.ConvertToIndex(tovar, positions[tovar]);
                                                env.AssignWithFrame(new AssignValue(assignToVar, ""));
                                                positions[tovar] = positions[tovar] + 1;

                                            }
                                        }
                                        resultsEnumerator.Reset();
                                        resultsEnumerator.MoveNext(); 
                                    }
                                    if (blankRows.Contains(OpCnt) && blankRows.Count != 0)
                                    {
                                        tmp = tmp.Replace(Environment.NewLine, "");
                                        while (pos != end + 1)
                                        {
                                            pos++;
                                        }
                                    }
                                    var outputVar = resultsEnumerator.Current.OutputVariable;
                                    
                                    if (!String.IsNullOrEmpty(outputVar))
                                    {
                                        var assignVar = ExecutionEnvironment.ConvertToIndex(outputVar, positions[outputVar]);
                                        if (ExecutionEnvironment.IsRecordsetIdentifier(assignVar))
                                        {
                                            env.AssignWithFrame(new AssignValue(assignVar, tmp));
                                        }
                                        else if(  ExecutionEnvironment.IsScalar(assignVar) && positions[outputVar] ==1)
                                        {
                                            env.AssignWithFrame(new AssignValue(assignVar, tmp));
                                        }
                                        else
                                        {
                                            env.AssignWithFrame(new AssignValue(assignVar, tmp));
                                        }
                                        positions[outputVar] = positions[outputVar] + 1;
                                    }
                                    if (dataObject.IsDebugMode())
                                    {
                                        var debugItem = new DebugItem();
                                        var outputVarTo = resultsEnumerator.Current.OutputVariable;
                                        AddDebugItem(new DebugEvalResult(outputVarTo, "", env), debugItem);
                                        if (debugDictionary.ContainsKey(outputVarTo))
                                        {
                                            debugDictionary[outputVarTo].Add(debugItem);
                                        }
                                        else
                                        {
                                            debugDictionary.Add(outputVarTo,new List<DebugItem>{debugItem});
                                        }                                                                               
                                    }
                                    if (pos == end)
                                    {
                                        
                                    }
                                    else
                                    {
                                        pos++;
                                    }
                                }
                            }
                        }
                    }                    
                    env.CommitAssign();
                    if (singleInnerIteration)
                    {
                        break;
                    }
                }
                
                if (dataObject.IsDebugMode())
                {
                   
                    var outputIndex = 1;    
                    foreach(var varDebug in debugDictionary)
                    {
                        var debugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("", outputIndex.ToString(CultureInfo.InvariantCulture)), debugItem);
                        var dataSplitUsesStarForOutput = varDebug.Key.Replace("().", "(*).");
                        AddDebugItem(new DebugEvalResult(dataSplitUsesStarForOutput, "", env), debugItem);
                        _debugOutputs.Add(debugItem);
                        outputIndex++;    
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

        private IDev2Tokenizer CreateSplitPattern(ref string stringToSplit, IEnumerable<DataSplitDTO> args, IExecutionEnvironment compiler, out ErrorResultTO errors)
        {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder { ToTokenize = stringToSplit, ReverseOrder = ReverseOrder };
            errors = new ErrorResultTO();

            foreach(DataSplitDTO t in args)
            {
                t.At = t.At ?? "";
                string entry;
                
                switch(t.SplitType)
                {
                    case "Index":
                        try
                        {
                            entry = compiler.EvalAsListOfStrings(t.At).FirstOrDefault();
                            if(entry== null) throw new Exception("null iterator expression");
                            string index = entry;
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
                            entry = compiler.EvalAsListOfStrings(t.At).FirstOrDefault();

                           
                            string escape = t.EscapeChar;
                            if(!String.IsNullOrEmpty(escape))
                            {
                                escape = compiler.EvalAsListOfStrings(t.EscapeChar).FirstOrDefault();
                              
                            }

                            dtb.AddTokenOp(entry, t.Include, escape);
                        }
                        break;
                }
                _indexCounter++;
            }
            return string.IsNullOrEmpty(dtb.ToTokenize) || errors.HasErrors() ? null : dtb.Generate();
        }

        private void AddDebug(IEnumerable<DataSplitDTO> resultCollection, IExecutionEnvironment env)
        {
            foreach(DataSplitDTO t in resultCollection)
            {
                DebugItem debugItem = new DebugItem();
                AddDebugItem(new DebugItemStaticDataParams("",_indexCounter.ToString(CultureInfo.InvariantCulture)), debugItem);
                if (!string.IsNullOrEmpty(t.OutputVariable))
                {
                    AddDebugItem(new DebugEvalResult(t.OutputVariable, "", env), debugItem);
                }
                AddDebugItem(new DebugItemStaticDataParams(t.SplitType, "With"), debugItem);

                switch(t.SplitType)
                {
                    case "Index":
                        AddDebugItem(new DebugEvalResult(t.At, "Using", env), debugItem);
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
                        AddDebugItem(new DebugEvalResult(t.At, "Using", env), debugItem);
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

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        #endregion

        #region GetDebugOutputs

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList)
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
