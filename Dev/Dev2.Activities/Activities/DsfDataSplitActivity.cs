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
using System.Text.RegularExpressions;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.StringTokenizer.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Validation;
using Warewolf.Core;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;
using Dev2.Comparer;
using System.IO;
using System.Text;
using Dev2.Common.Common;
using Dev2.Common.State;
using Dev2.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    [ToolDescriptorInfo("Data-DataSplit", "Data Split", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Data", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Data_Data_Split")]
    public class DsfDataSplitActivity : DsfActivityAbstract<string>, ICollectionActivity, IEquatable<DsfDataSplitActivity>
    {
        string _sourceString;
        int _indexCounter = 1;
        IList<DataSplitDTO> _resultsCollection;
        bool _reverseOrder;
        bool _skipBlankRows;

        public IList<DataSplitDTO> ResultsCollection
        {
            get => _resultsCollection;
            set
            {
                _resultsCollection = value;
                OnPropertyChanged("ResultsCollection");
            }
        }

        public bool ReverseOrder
        {
            get => _reverseOrder;
            set
            {
                _reverseOrder = value;
                OnPropertyChanged("ReverseOrder");
            }
        }

        public string SourceString
        {
            get => _sourceString;
            set
            {
                _sourceString = value;
                OnPropertyChanged("SourceString");
            }
        }

        public bool SkipBlankRows
        {
            get => _skipBlankRows;
            set
            {
                _skipBlankRows = value;
                OnPropertyChanged("SkipBlankRows");
            }
        }
        private string NewLineFormat { get; set; } = "\r\n";


        protected override bool CanInduceIdle => true;

        public DsfDataSplitActivity()
            : base("Data Split")
        {
            ResultsCollection = new List<DataSplitDTO>();
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[] {
                new StateVariable
                {
                    Name = "SourceString",
                    Value = SourceString,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "ReverseOrder",
                    Value = ReverseOrder.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "SkipBlankRows",
                    Value = SkipBlankRows.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name="ResultsCollection",
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(ResultsCollection),
                    Type = StateVariable.StateType.Output
                }
            };
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

#pragma warning disable S1541 // Methods and properties should not be too complex
        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            _indexCounter = 1;

            var allErrors = new ErrorResultTO();
            var env = dataObject.Environment;
            var iter = new WarewolfListIterator();

            InitializeDebug(dataObject);
            try
            {
                var sourceString = SourceString ?? "";
                if (dataObject.IsDebugMode())
                {
                    AddDebugInputItem(new DebugEvalResult(sourceString, "String to Split", env, update));
                    AddDebugInputItem(new DebugItemStaticDataParams(ReverseOrder ? "Backward" : "Forward", "Process Direction"));
                    AddDebugInputItem(new DebugItemStaticDataParams(SkipBlankRows ? "Yes" : "No", "Skip blank rows"));
                    AddDebug(ResultsCollection, dataObject.Environment, update);
                }
                var sourceStringValue = env.Eval(sourceString, update);
                var res = new WarewolfIterator(sourceStringValue);
                NewLineFormat = res.NewLineFormat;


                iter.AddVariableToIterateOn(res);
                IDictionary<string, int> positions = new Dictionary<string, int>();
                CleanArguments(ResultsCollection);
                ResultsCollection.ToList().ForEach(a =>
                {
                    if (!positions.ContainsKey(a.OutputVariable))
                    {
                        positions.Add(a.OutputVariable, update == 0 ? 1 : update);
                    }
                    IsSingleValueRule.ApplyIsSingleValueRule(a.OutputVariable, allErrors);
                });
                var singleInnerIteration = ArePureScalarTargets(ResultsCollection);
                var resultsEnumerator = ResultsCollection.GetEnumerator();
                var debugDictionary = new List<string>();
                while (res.HasMoreData())
                {
                    CommitItem(dataObject, update, allErrors, env, res, positions, singleInnerIteration, resultsEnumerator, debugDictionary);
                    if (singleInnerIteration)
                    {
                        break;
                    }
                }

                if (dataObject.IsDebugMode())
                {
                    var outputIndex = 1;
                    foreach (var varDebug in debugDictionary)
                    {
                        var debugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("", outputIndex.ToString(CultureInfo.InvariantCulture)), debugItem);
                        var dataSplitUsesStarForOutput = varDebug.Replace("().", "(*).");
                        AddDebugItem(new DebugEvalResult(dataSplitUsesStarForOutput, "", env, update), debugItem);
                        _debugOutputs.Add(debugItem);
                        outputIndex++;
                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("DSFDataSplit", e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }
            finally
            {
                HandleErrors(dataObject, update, allErrors);
            }
        }

        private void CommitItem(IDSFDataObject dataObject, int update, ErrorResultTO allErrors, IExecutionEnvironment env, WarewolfIterator res, IDictionary<string, int> positions, bool singleInnerIteration, IEnumerator<DataSplitDTO> resultsEnumerator, List<string> debugDictionary)
        {
            var item = new StringBuilder(res.GetNextValue());
            if (item.Length > 0)
            {
                var tokenizer = CreateSplitPattern(ref item, ResultsCollection, env, out ErrorResultTO errors, update);
                allErrors.MergeErrors(errors);

                if (!allErrors.HasErrors() && tokenizer != null)
                {
                    ProcessTokenizerItems(dataObject, update, env, positions, singleInnerIteration, resultsEnumerator, debugDictionary, tokenizer);
                }
            }
            env.CommitAssign();
        }

        void ProcessTokenizerItems(IDSFDataObject dataObject, int update, IExecutionEnvironment env, IDictionary<string, int> positions, bool singleInnerIteration, IEnumerator<DataSplitDTO> resultsEnumerator, List<string> debugDictionary, IDev2Tokenizer tokenizer)
        {
            var lastItemEndedInNewLine = false;
            while (tokenizer.HasMoreOps())
            {
                var currentval = resultsEnumerator.MoveNext();
                if (!currentval)
                {
                    if (singleInnerIteration)
                    {
                        break;
                    }
                    resultsEnumerator.Reset();
                    resultsEnumerator.MoveNext();
                }
                var tmp = tokenizer.NextToken();
                if (tmp.StartsWith(NewLineFormat) && !SkipBlankRows)
                {
                    resultsEnumerator.Reset();
                    while (resultsEnumerator.MoveNext())
                    {
                        AssignOutputVariable(update, env, resultsEnumerator, positions);
                    }
                    resultsEnumerator.Reset();
                    resultsEnumerator.MoveNext();
                }
                var outputVar = resultsEnumerator.Current.OutputVariable;
                if (!NewLine(tmp))
                {
                    lastItemEndedInNewLine = tmp.EndsWith(NewLineFormat);
                    AssignItem(update, env, positions, ref tmp, outputVar);
                    if (dataObject.IsDebugMode())
                    {
                        AddOutputToDebugOutput(update, env, resultsEnumerator, debugDictionary);
                    }
                }
            }

            if (lastItemEndedInNewLine)
            {
                var tovar = resultsEnumerator.Current.OutputVariable;
                var assignToVar = ExecutionEnvironment.ConvertToIndex(tovar, positions[tovar]);
                env.AssignWithFrame(new AssignValue(assignToVar, ""), update);
                positions[tovar] = positions[tovar] + 1;
            }
        }

        private void AssignItem(int update, IExecutionEnvironment env, IDictionary<string, int> positions, ref string tmp, string outputVar)
        {
            if (!String.IsNullOrEmpty(outputVar))
            {
                var assignVar = ExecutionEnvironment.ConvertToIndex(outputVar, positions[outputVar]);
                if (!SkipBlankRows)
                {
                    tmp = tmp.Replace(NewLineFormat, "");
                }
                env.AssignWithFrame(new AssignValue(assignVar, tmp), update);
                positions[outputVar] = positions[outputVar] + 1;
            }
        }

        void AddOutputToDebugOutput(int update, IExecutionEnvironment env, IEnumerator<DataSplitDTO> resultsEnumerator, List<string> debugDictionary)
        {
            var debugItem = new DebugItem();
            var outputVarTo = resultsEnumerator.Current.OutputVariable;
            AddDebugItem(new DebugEvalResult(outputVarTo, "", env, update), debugItem);
            if (!debugDictionary.Contains(outputVarTo))
            {
                debugDictionary.Add(outputVarTo);
            }
        }

        static void AssignOutputVariable(int update, IExecutionEnvironment env, IEnumerator<DataSplitDTO> resultsEnumerator, IDictionary<string, int> positions)
        {
            var tovar = resultsEnumerator.Current.OutputVariable;
            if (!String.IsNullOrEmpty(tovar))
            {
                var assignToVar = ExecutionEnvironment.ConvertToIndex(tovar, positions[tovar]);
                env.AssignWithFrame(new AssignValue(assignToVar, ""), update);
                positions[tovar] = positions[tovar] + 1;
            }
        }

        private static bool NewLine(string tmp) => tmp == "\r" || tmp == "\n" || tmp == Environment.NewLine;

        void HandleErrors(IDSFDataObject dataObject, int update, ErrorResultTO allErrors)
        {
            var hasErrors = allErrors.HasErrors();
            if (hasErrors)
            {
                DisplayAndWriteError("DsfDataSplitActivity", allErrors);
                var errorString = allErrors.MakeDisplayReady();
                dataObject.Environment.AddError(errorString);
            }

            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.Before, update);
                DispatchDebugState(dataObject, StateType.After, update);
            }
        }

        public override enFindMissingType GetFindMissingType() => enFindMissingType.MixedActivity;

        static bool ArePureScalarTargets(IEnumerable<DataSplitDTO> args) => args.All(arg => !DataListUtil.IsValueRecordset(arg.OutputVariable));

        void InsertToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ResultsCollection"];
            var mic = modelProperty?.Collection;

            if (mic != null)
            {
                var listOfValidRows = ResultsCollection.Where(c => !c.CanRemove()).ToList();
                if (listOfValidRows.Count > 0)
                {
                    ConcatenateCollections(listToAdd, modelItem, mic);
                }
                else
                {
                    AddToCollection(listToAdd, modelItem);
                }
            }
        }

        void ConcatenateCollections(IEnumerable<string> listToAdd, ModelItem modelItem, ModelItemCollection mic)
        {
            var dataSplitDto = ResultsCollection.Last(c => !c.CanRemove());
            var startIndex = ResultsCollection.IndexOf(dataSplitDto) + 1;
            foreach (string s in listToAdd)
            {
                mic.Insert(startIndex, new DataSplitDTO(s, ResultsCollection[startIndex - 1].SplitType, ResultsCollection[startIndex - 1].At, startIndex + 1));
                startIndex++;
            }
            CleanUpCollection(mic, modelItem, startIndex);
        }

        void AddToCollection(IEnumerable<string> listToAdd, ModelItem modelItem)
        {
            var modelProperty = modelItem.Properties["ResultsCollection"];
            var mic = modelProperty?.Collection;

            if (mic != null)
            {
                var startIndex = 0;
                var firstRowSplitType = ResultsCollection[0].SplitType;
                var firstRowAt = ResultsCollection[0].At;
                mic.Clear();
                foreach (string s in listToAdd)
                {
                    mic.Add(new DataSplitDTO(s, firstRowSplitType, firstRowAt, startIndex + 1));
                    startIndex++;
                }
                CleanUpCollection(mic, modelItem, startIndex);
            }
        }

        void CleanUpCollection(ModelItemCollection mic, ModelItem modelItem, int startIndex)
        {
            if (startIndex < mic.Count)
            {
                mic.RemoveAt(startIndex);
            }
            mic.Add(new DataSplitDTO(string.Empty, "Chars", string.Empty, startIndex + 1));
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

        IDev2Tokenizer CreateSplitPattern(ref StringBuilder stringToSplit, IEnumerable<DataSplitDTO> args, IExecutionEnvironment compiler, out ErrorResultTO errors, int update)
        {


            var dtb = new Dev2TokenizerBuilder { ToTokenize = stringToSplit, ReverseOrder = ReverseOrder };
            errors = new ErrorResultTO();

            foreach (DataSplitDTO t in args)
            {
                stringToSplit = AddTokenOp(stringToSplit, compiler, errors, update, dtb, t);
                _indexCounter++;
            }
            return stringToSplit.Length <= 0 || errors.HasErrors() ? null : dtb.Generate();
        }

        StringBuilder AddTokenOp(StringBuilder stringToSplit, IExecutionEnvironment compiler, ErrorResultTO errors, int update, Dev2TokenizerBuilder dtb, DataSplitDTO t)
        {
            var parsedAt = t.At ?? string.Empty;
            var entry = "";

            switch (t.SplitType)
            {
                case "Index":
                    AddIndexOperation(dtb, compiler, errors, update, parsedAt);
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
                    t.Include |= !SkipBlankRows;
                    AddLineBreakTokenOp(stringToSplit, dtb, t);
                    break;
                case "Chars":
                    AddCharacterTokenOp(ref stringToSplit, compiler, update, dtb, t, parsedAt, ref entry);
                    break;
                default:
                    Dev2Logger.Info("No Split type for the Data Split Property Name: " + t.SplitType, GlobalConstants.WarewolfInfo);
                    break;
            }

            return stringToSplit;
        }

        void AddCharacterTokenOp(ref StringBuilder stringToSplit, IExecutionEnvironment compiler, int update, Dev2TokenizerBuilder dtb, DataSplitDTO t, string parsedAt, ref string entry)
        {
            if (!string.IsNullOrEmpty(parsedAt))
            {
                entry = EvalLineBreakCharacter(ref stringToSplit, compiler, update, dtb, parsedAt);
                var escape = EvalEscapeCharacter(compiler, update, t);
                dtb.AddTokenOp(entry, t.Include, escape);
            }
        }

        static void AddLineBreakTokenOp(StringBuilder stringToSplit, Dev2TokenizerBuilder dtb, DataSplitDTO t)
        {
            if (stringToSplit.Contains("\r\n"))
            {
                dtb.AddTokenOp("\r\n", t.Include);
            }
            else if (stringToSplit.Contains("\n"))
            {
                dtb.AddTokenOp("\n", t.Include);
            }
            else
            {
                if (stringToSplit.Contains("\r"))
                {
                    dtb.AddTokenOp("\r", t.Include);
                }
            }
        }

        string EvalLineBreakCharacter(ref StringBuilder stringToSplit, IExecutionEnvironment compiler, int update, Dev2TokenizerBuilder dtb, string parsedAt)
        {
            var entry = compiler.EvalAsListOfStrings(parsedAt, update).FirstOrDefault();
            if (entry != null && (entry.Contains(@"\r\n") || entry.Contains(@"\n")))
            {
                var match = Regex.Match(stringToSplit.ToString(), @"[\r\n]+");
                if (match.Success && !SkipBlankRows)
                {
                    stringToSplit = new StringBuilder(Regex.Escape(stringToSplit.ToString()));
                    dtb.ToTokenize = stringToSplit;
                }
            }

            return entry;
        }

        static string EvalEscapeCharacter(IExecutionEnvironment compiler, int update, DataSplitDTO t)
        {
            var escape = t.EscapeChar;
            if (!String.IsNullOrEmpty(escape))
            {
                escape = compiler.EvalAsListOfStrings(t.EscapeChar, update).FirstOrDefault();
            }

            return escape;
        }

        static void AddIndexOperation(Dev2TokenizerBuilder dtb, IExecutionEnvironment compiler, ErrorResultTO errors, int update, string parsedAt)
        {
            try
            {
                var entry = compiler.EvalAsListOfStrings(parsedAt, update).FirstOrDefault();
                if (entry == null)
                {
                    throw new Exception("null iterator expression");
                }

                var index = entry;
                var indexNum = Convert.ToInt32(index);
                if (indexNum > 0)
                {
                    dtb.AddIndexOp(indexNum);
                }
            }
            catch (Exception ex)
            {
                errors.AddError(ex.Message);
            }
        }

        void AddDebug(IEnumerable<DataSplitDTO> resultCollection, IExecutionEnvironment env, int update)
        {
            foreach (DataSplitDTO t in resultCollection)
            {
                var debugItem = AddParamsToDebug(env, update, t);
                AddResultsToDebug(env, update, t, debugItem);
                _indexCounter++;
                _debugInputs.Add(debugItem);
            }
        }

        void AddResultsToDebug(IExecutionEnvironment env, int update, DataSplitDTO t, DebugItem debugItem)
        {
            switch (t.SplitType)
            {
                case "Index":
                    AddDebugItem(new DebugEvalResult(t.At, "Using", env, update), debugItem);
                    AddDebugItem(new DebugItemStaticDataParams(t.Include ? "Yes" : "No", "Include"), debugItem);
                    break;
                case "End":
                case "Space":
                case "Tab":
                case "New Line":
                    AddDebugItem(new DebugItemStaticDataParams(t.Include ? "Yes" : "No", "Include"), debugItem);
                    break;
                case "Chars":
                    AddDebugItem(new DebugEvalResult(t.At, "Using", env, update), debugItem);
                    AddDebugItem(new DebugItemStaticDataParams(t.Include ? "Yes" : "No", "Include"), debugItem);
                    AddDebugItem(new DebugItemStaticDataParams(t.EscapeChar, "Escape"), debugItem);
                    break;
                default:
                    return;
            }
        }

        DebugItem AddParamsToDebug(IExecutionEnvironment env, int update, DataSplitDTO t)
        {
            var debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", _indexCounter.ToString(CultureInfo.InvariantCulture)), debugItem);
            if (!string.IsNullOrEmpty(t.OutputVariable))
            {
                AddDebugItem(new DebugEvalResult(t.OutputVariable, "", env, update), debugItem);
            }
            AddDebugItem(new DebugItemStaticDataParams(t.SplitType, "With"), debugItem);
            return debugItem;
        }

        static void CleanArguments(IList<DataSplitDTO> args)
        {
            var count = 0;
            while (count < args.Count)
            {
                if (string.IsNullOrEmpty(args[count].OutputVariable))
                {
                    if (args[count].SplitType == "Index" && string.IsNullOrEmpty(args[count].At) ||
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
            if (updates == null)
            {
                return;
            }
            foreach (Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                var t1 = t;
                var items = ResultsCollection.Where(c => !string.IsNullOrEmpty(c.At) && c.At.Equals(t1.Item1));

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

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            if (updates == null)
            {
                return;
            }
            foreach (Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                var t1 = t;
                var items = ResultsCollection.Where(c => !string.IsNullOrEmpty(c.OutputVariable) && c.OutputVariable.Equals(t1.Item1));

                // issues updates
                foreach (var a in items)
                {
                    a.OutputVariable = t.Item2;
                }
            }
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var items = new[] { SourceString }.Union(ResultsCollection.Where(c => !string.IsNullOrEmpty(c.At)).Select(c => c.At)).ToArray();
            return GetForEachItems(items);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            var items = ResultsCollection.Where(c => !string.IsNullOrEmpty(c.OutputVariable)).Select(c => c.OutputVariable).ToArray();
            return GetForEachItems(items);
        }

        public int GetCollectionCount() => ResultsCollection.Count(caseConvertTo => !caseConvertTo.CanRemove());

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

        public override List<string> GetOutputs() => ResultsCollection.Select(dto => dto.OutputVariable).ToList();

        public bool Equals(DsfDataSplitActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var resultsCollectionsAreEqual = CommonEqualityOps.CollectionEquals(ResultsCollection.OrderBy(dto => dto.IndexNumber), other.ResultsCollection.OrderBy(dto => dto.IndexNumber), new DataSplitDTOComparer());
            return base.Equals(other)
                && string.Equals(SourceString, other.SourceString)
                && _indexCounter == other._indexCounter
                && resultsCollectionsAreEqual
                && ReverseOrder == other.ReverseOrder
                && SkipBlankRows == other.SkipBlankRows;
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

            return Equals((DsfDataSplitActivity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (SourceString != null ? SourceString.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _indexCounter;
                hashCode = (hashCode * 397) ^ (_resultsCollection != null ? _resultsCollection.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ReverseOrder.GetHashCode();
                hashCode = (hashCode * 397) ^ SkipBlankRows.GetHashCode();
                return hashCode;
            }
        }
    }
}