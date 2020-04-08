#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.State;
using Dev2.Comparer;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.MathOperations;
using Dev2.Utilities;
using Warewolf.Data;
using Warewolf.Exceptions;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfMultiAssignActivity : DsfActivityAbstract<string>
    {
#pragma warning disable S3776,S1541,S134,CC0075,S1066,S1067
        public static readonly string CalculateTextConvertPrefix = GlobalConstants.CalculateTextConvertPrefix;
        public static readonly string CalculateTextConvertSuffix = GlobalConstants.CalculateTextConvertSuffix;
        public static readonly string CalculateTextConvertFormat = GlobalConstants.CalculateTextConvertFormat;

        IList<ActivityDTO> _fieldsCollection;

        public IList<ActivityDTO> FieldsCollection

        {
            get
            {
                return _fieldsCollection;
            }
            set
            {
                _fieldsCollection = value;
            }
        }

        public bool UpdateAllOccurrences { get; set; }
        public bool CreateBookmark { get; set; }
        public string ServiceHost { get; set; }

        protected override bool CanInduceIdle => true;

        public DsfMultiAssignActivity()
            : base("Assign")
        {
            _fieldsCollection = new List<ActivityDTO>();
        }

        public override List<string> GetOutputs() => FieldsCollection.Select(dto => dto.FieldName).ToList();

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _debugOutputs.Clear();
            _debugInputs.Clear();

            InitializeDebug(dataObject);
            var errors = new ErrorResultTO();
            var allErrors = new ErrorResultTO();

            try
            {
                if (!errors.HasErrors())
                {
                    var innerCount = 1;
                    foreach (ActivityDTO t in FieldsCollection)
                    {
                        allErrors = TryExecuteField(dataObject, update, innerCount++, t, allErrors);
                    }
                    dataObject.Environment.CommitAssign();
                    allErrors.MergeErrors(errors);
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    DisplayAndWriteError("DsfAssignActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                }
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        ErrorResultTO TryExecuteField(IDSFDataObject dataObject, int update, int innerCount, ActivityDTO t, ErrorResultTO allErrors)
        {
            try
            {
                if (!string.IsNullOrEmpty(t.FieldName))
                {
                    ExecuteField(dataObject, update, innerCount, t, allErrors);
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }
            return allErrors;
        }

        void ExecuteField(IDSFDataObject dataObject, int update, int innerCount, ActivityDTO t, ErrorResultTO allErrors)
        {
            var assignValue = new AssignValue(t.FieldName, t.FieldValue);
            var isCalcEvaluation = DataListUtil.IsCalcEvaluation(t.FieldValue, out string cleanExpression);
            if (isCalcEvaluation)
            {
                assignValue = new AssignValue(t.FieldName, cleanExpression);
            }
            DebugItem debugItem = null;
            if (dataObject.IsDebugMode())
            {
                debugItem = TryCreateDebugInput(dataObject.Environment, innerCount, assignValue, update);
            }

            try
            {
                if (isCalcEvaluation)
                {
                    DoCalculation(dataObject.Environment, t.FieldName, t.FieldValue, update);
                }
                else
                {
                    dataObject.Environment.AssignWithFrame(assignValue, update);
                }
            } catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }

            if (debugItem != null)
            {
                _debugInputs.Add(debugItem);
            }
            if (dataObject.IsDebugMode())
            {
                if (DataListUtil.IsValueRecordset(assignValue.Name) && DataListUtil.GetRecordsetIndexType(assignValue.Name) == enRecordsetIndexType.Blank)
                {
                    var length = dataObject.Environment.GetLength(DataListUtil.ExtractRecordsetNameFromValue(assignValue.Name));
                    assignValue = new AssignValue(DataListUtil.ReplaceRecordsetBlankWithIndex(assignValue.Name, length), assignValue.Value);
                }
                AddSingleDebugOutputItem(dataObject.Environment, innerCount, assignValue, update);
            }
        }

        void DoCalculation(IExecutionEnvironment environment, string fieldName, string cleanExpression, int update)
        {
            var functionEvaluator = new FunctionEvaluator();
            var warewolfEvalResult = environment.Eval(cleanExpression, update);

            if (warewolfEvalResult.IsWarewolfAtomResult && warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult atomResult)
            {
                var eval = PerformCalcForAtom(atomResult.Item, functionEvaluator);
                var doCalculation = new AssignValue(fieldName, eval);
                environment.AssignWithFrame(doCalculation, update);
            }

            if (warewolfEvalResult.IsWarewolfAtomListresult && warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult atomListResult)
            {
                var counter = 1;
                foreach (var item in atomListResult.Item)
                {
                    var eval = PerformCalcForAtom(item, functionEvaluator);
                    var doCalculation = new AssignValue(fieldName, eval);
                    environment.AssignWithFrame(doCalculation, update == 0 ? counter : update);
                    counter++;
                }
            }

        }

        static string PerformCalcForAtom(DataStorage.WarewolfAtom warewolfAtom, FunctionEvaluator functionEvaluator)
        {
            var calcExpression = ExecutionEnvironment.WarewolfAtomToString(warewolfAtom);
            DataListUtil.IsCalcEvaluation(calcExpression, out string exp);
            var res = functionEvaluator.TryEvaluateFunction(exp, out string eval, out string error);
            if (eval == exp.Replace("\"", "") && exp.Contains("\""))
            {
                try
                {
                    var b = functionEvaluator.TryEvaluateFunction(exp.Replace("\"", ""), out string eval2, out error);
                    if (b)
                    {
                        eval = eval2;
                    }
                }
                catch (Exception err)
                {
                    Dev2Logger.Warn(err, "Warewolf Warn");
                }
            }
            if (!res)
            {
                throw new Exception(ErrorResource.InvalidCalculate);
            }
            return eval;
        }

        DebugItem TryCreateDebugInput(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue, int update)
        {
            var debugItem = new DebugItem();
            const string VariableLabelText = "Variable";
            const string NewFieldLabelText = "New Value";

            try
            {
                CreateDebugInput(environment, innerCount, assignValue, update, debugItem, VariableLabelText, NewFieldLabelText);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("ParseError"))
                {
                    AddDebugItem(new DebugItemWarewolfAtomResult("", assignValue.Value, environment.EvalToExpression(assignValue.Name, update), "", VariableLabelText, NewFieldLabelText, "="), debugItem);
                    return debugItem;
                }
                if (!ExecutionEnvironment.IsValidVariableExpression(assignValue.Name, out string errorMessage, update))
                {
                    return null;
                }
                AddErrorDebugItem(environment, innerCount, assignValue, update, debugItem, VariableLabelText, NewFieldLabelText);
            }
            return debugItem;
        }

        private void AddErrorDebugItem(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue, int update, DebugItem debugItem, string VariableLabelText, string NewFieldLabelText)
        {
            AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
            if (DataListUtil.IsEvaluated(assignValue.Value))
            {
                var newValueResult = environment.Eval(assignValue.Value, update);

                if (newValueResult.IsWarewolfAtomResult)
                {
                    if (newValueResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult valueResult)
                    {
                        AddDebugItem(new DebugItemWarewolfAtomResult("", ExecutionEnvironment.WarewolfAtomToString(valueResult.Item), environment.EvalToExpression(assignValue.Name, update), assignValue.Value, VariableLabelText, NewFieldLabelText, "="), debugItem);
                    }
                }
                else
                {
                    if (newValueResult.IsWarewolfAtomListresult)
                    {
                        AddDebugItem(new DebugItemWarewolfAtomListResult(null, newValueResult, environment.EvalToExpression(assignValue.Value, update), assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
                    }
                }
            }
            else
            {
                AddDebugItem(new DebugItemWarewolfAtomResult("", assignValue.Value, environment.EvalToExpression(assignValue.Name, update), "", VariableLabelText, NewFieldLabelText, "="), debugItem);
            }
        }

        void CreateDebugInput(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue, int update, DebugItem debugItem, string VariableLabelText, string NewFieldLabelText)
        {
            if (!DataListUtil.IsEvaluated(assignValue.Value))
            {
                var evalResult = environment.Eval(assignValue.Name, update);
                AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
                if (evalResult.IsWarewolfAtomResult)
                {
                    if (evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult scalarResult)
                    {
                        AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item), assignValue.Value, environment.EvalToExpression(assignValue.Name, update), "", VariableLabelText, NewFieldLabelText, "="), debugItem);
                    }
                }
                else
                {
                    AddWarewolfAtomListDebugResult(environment, assignValue, update, debugItem, VariableLabelText, NewFieldLabelText, evalResult);
                }
            }
            else
            {
                if (DataListUtil.IsEvaluated(assignValue.Value))
                {
                    AddEvaluatedDebugItem(environment, innerCount, assignValue, update, debugItem, VariableLabelText, NewFieldLabelText);
                }
            }
        }

        void AddEvaluatedDebugItem(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue, int update, DebugItem debugItem, string VariableLabelText, string NewFieldLabelText)
        {
            var oldValueResult = environment.Eval(assignValue.Name, update);
            var newValueResult = environment.Eval(assignValue.Value, update);
            AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
            if (oldValueResult.IsWarewolfAtomResult && newValueResult.IsWarewolfAtomResult)
            {
                var valueResult = newValueResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                var scalarResult = oldValueResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                if (valueResult != null && scalarResult != null)
                {
                    AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item), ExecutionEnvironment.WarewolfAtomToString(valueResult.Item), assignValue.Name, environment.EvalToExpression(assignValue.Value, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
                }
            }
            else if (newValueResult.IsWarewolfAtomResult && oldValueResult.IsWarewolfAtomListresult)
            {
                AddDebugItem(new DebugItemWarewolfAtomListResult(null, newValueResult, environment.EvalToExpression(assignValue.Value, update), environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
            }
            else if (oldValueResult.IsWarewolfAtomResult && newValueResult.IsWarewolfAtomListresult)
            {
                AddDebugItem(new DebugItemWarewolfAtomListResult(null, newValueResult, environment.EvalToExpression(assignValue.Value, update), environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
            }
            else
            {
                if (oldValueResult.IsWarewolfAtomListresult && newValueResult.IsWarewolfAtomListresult)
                {
                    var recSetResult = oldValueResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                    AddDebugItem(new DebugItemWarewolfAtomListResult(recSetResult, newValueResult, environment.EvalToExpression(assignValue.Value, update), environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
                }
            }
        }

        void AddWarewolfAtomListDebugResult(IExecutionEnvironment environment, IAssignValue assignValue, int update, DebugItem debugItem, string VariableLabelText, string NewFieldLabelText, CommonFunctions.WarewolfEvalResult evalResult)
        {
            if (evalResult.IsWarewolfAtomListresult)
            {
                if (DataListUtil.GetRecordsetIndexType(assignValue.Name) == enRecordsetIndexType.Blank)
                {
                    AddDebugItem(new DebugItemWarewolfAtomListResult(null, assignValue.Value, "", environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
                }
                else
                {
                    if (evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recSetResult)
                    {
                        AddDebugItem(new DebugItemWarewolfAtomListResult(recSetResult, assignValue.Value, "", environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
                    }
                }
            }
        }

        void AddSingleDebugOutputItem(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue, int update)
        {
            const string VariableLabelText = "";
            const string NewFieldLabelText = "";
            var debugItem = new DebugItem();

            try
            {
                if (!DataListUtil.IsEvaluated(assignValue.Value))
                {
                    AddDebugItem(environment, innerCount, assignValue, update, VariableLabelText, NewFieldLabelText, debugItem);
                }
                else
                {
                    AddEvaluatedDebugItem(environment, innerCount, assignValue, update, VariableLabelText, NewFieldLabelText, debugItem);
                }
            }
            catch (NullValueInVariableException)
            {
                AddDebugItem(new DebugItemWarewolfAtomResult("", assignValue.Value, "", environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
            }
            _debugOutputs.Add(debugItem);
        }

        private void AddEvaluatedDebugItem(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue, int update, string VariableLabelText, string NewFieldLabelText, DebugItem debugItem)
        {
            if (DataListUtil.IsEvaluated(assignValue.Value))
            {
                var evalResult = environment.Eval(assignValue.Name, update);
                AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
                if (evalResult.IsWarewolfAtomResult && evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult scalarResult)
                {
                    AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item), "", environment.EvalToExpression(assignValue.Name, update), "", VariableLabelText, NewFieldLabelText, "="), debugItem);
                }

                var evalResult2 = environment.Eval(assignValue.Value, update);
                if (evalResult.IsWarewolfAtomListresult && evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recSetResult)
                {

                    AddDebugItem(DataListUtil.GetRecordsetIndexType(assignValue.Name) == enRecordsetIndexType.Blank ? new DebugItemWarewolfAtomListResult(recSetResult, evalResult2, "", assignValue.Name, VariableLabelText, NewFieldLabelText, "=") : new DebugItemWarewolfAtomListResult(recSetResult, environment.EvalToExpression(assignValue.Value, update), "", environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
                }
            }
        }

        void AddDebugItem(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue, int update, string VariableLabelText, string NewFieldLabelText, DebugItem debugItem)
        {
            var evalResult = environment.Eval(assignValue.Name, update);
            AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
            if (evalResult.IsWarewolfAtomResult)
            {
                if (evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult scalarResult)
                {
                    AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item), assignValue.Value, environment.EvalToExpression(assignValue.Name, update), "", VariableLabelText, NewFieldLabelText, "="), debugItem);
                }
            }
            else
            {
                if (evalResult.IsWarewolfAtomListresult && evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recSetResult)
                {
                    AddDebugItem(new DebugItemWarewolfAtomListResult(recSetResult, "", "", environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
                }

            }
        }

        public override enFindMissingType GetFindMissingType() => enFindMissingType.DataGridActivity;

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            foreach (Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                var t1 = t;
                var items = FieldsCollection.Where(c => !string.IsNullOrEmpty(c.FieldValue) && c.FieldValue.Contains(t1.Item1));

                // issues updates
                foreach (var a in items)
                {
                    a.FieldValue = a.FieldValue.Replace(t.Item1, t.Item2);
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            foreach (Tuple<string, string> t in updates)
            {

                // locate all updates for this tuple
                var t1 = t;
                var items = FieldsCollection.Where(c => !string.IsNullOrEmpty(c.FieldName) && c.FieldName.Contains(t1.Item1));

                // issues updates
                foreach (var a in items)
                {
                    a.FieldName = a.FieldName.Replace(t.Item1, t.Item2);
                }
            }
        }


        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update) => _debugInputs;

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update) => _debugOutputs;

        public override IList<DsfForEachItem> GetForEachInputs() => (from item in FieldsCollection
                                                                     where !string.IsNullOrEmpty(item.FieldValue) && item.FieldValue.Contains("[[")
                                                                     select new DsfForEachItem { Name = item.FieldName, Value = item.FieldValue }).ToList();

        public override IList<DsfForEachItem> GetForEachOutputs() => (from item in FieldsCollection
                                                                      where !string.IsNullOrEmpty(item.FieldName) && item.FieldName.Contains("[[")
                                                                      select new DsfForEachItem { Name = item.FieldValue, Value = item.FieldName }).ToList();

        public override IEnumerable<StateVariable> GetState()
        {
            return new[]
            {
                new StateVariable
                {
                    Name = "Fields Collection",
                    Type = StateVariable.StateType.InputOutput,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(FieldsCollection)
                }
            };
        }

        public bool Equals(DsfMultiAssignActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var sequenceEqual = FieldsCollection.SequenceEqual(other.FieldsCollection, new ActivityDtoComparer());
            return base.Equals(other) && sequenceEqual
                && UpdateAllOccurrences == other.UpdateAllOccurrences
                && CreateBookmark == other.CreateBookmark 
                && string.Equals(ServiceHost, other.ServiceHost)
                && string.Equals(DisplayName, other.DisplayName);
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

            return Equals((DsfMultiAssignActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (FieldsCollection != null ? FieldsCollection.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ UpdateAllOccurrences.GetHashCode();
                hashCode = (hashCode * 397) ^ CreateBookmark.GetHashCode();
                hashCode = (hashCode * 397) ^ (ServiceHost != null ? ServiceHost.GetHashCode() : 0);
                return hashCode;
            }
        }
#pragma warning restore S3776,S1541,S134,CC0075,S1066,S1067
    }

}
