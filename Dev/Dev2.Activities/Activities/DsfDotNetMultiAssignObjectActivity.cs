#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.MathOperations;
using Dev2.TO;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;
using Dev2.Common.State;
using Dev2.Utilities;
using Warewolf.Data;
using Warewolf.Exceptions;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    [ToolDescriptorInfo("AssignObject", "Assign Object", ToolType.Native, "1CBF58F0-199D-4439-B904-20259B7A87EE", "Dev2.Activities", "1.0.0.0", "Legacy", "Data", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Data_Assign_Object")]
    public class DsfDotNetMultiAssignObjectActivity : DsfActivityAbstract<string>
    {
        public static readonly string CalculateTextConvertPrefix = GlobalConstants.CalculateTextConvertPrefix;
        public static readonly string CalculateTextConvertSuffix = GlobalConstants.CalculateTextConvertSuffix;
        public static readonly string CalculateTextConvertFormat = GlobalConstants.CalculateTextConvertFormat;

        IList<AssignObjectDTO> _fieldsCollection;

        public IList<AssignObjectDTO> FieldsCollection
        
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

        public override List<string> GetOutputs() => FieldsCollection.Select(dto => dto.FieldName).ToList();

        public DsfDotNetMultiAssignObjectActivity()
            : base("Assign Object")
        {
            _fieldsCollection = new List<AssignObjectDTO>();
        }

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
                    foreach (AssignObjectDTO t in FieldsCollection)
                    {
                        innerCount = TryExecute(dataObject, update, allErrors, innerCount, t);
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
                HandleErrors(dataObject, update, allErrors);
            }
        }

        private int TryExecute(IDSFDataObject dataObject, int update, ErrorResultTO allErrors, int innerCount, AssignObjectDTO t)
        {
            try
            {
                innerCount = AssignField(dataObject, update, innerCount, t);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }

            return innerCount;
        }

        private int AssignField(IDSFDataObject dataObject, int update, int innerCount, AssignObjectDTO t)
        {
            if (!string.IsNullOrEmpty(t.FieldName))
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
                    debugItem = AddSingleInputDebugItem(dataObject.Environment, innerCount, assignValue, update);
                }
                if (isCalcEvaluation)
                {
                    DoCalculation(dataObject.Environment, t.FieldName, t.FieldValue, update);
                }
                else
                {
                    dataObject.Environment.AssignJson(assignValue, update);
                }
                if (debugItem != null)
                {
                    _debugInputs.Add(debugItem);
                }
                if (dataObject.IsDebugMode())
                {
                    if (DataListUtil.IsValueRecordset(assignValue.Name) && DataListUtil.GetRecordsetIndexType(assignValue.Name) == enRecordsetIndexType.Blank)
                    {
                        var length = dataObject.Environment.GetObjectLength(DataListUtil.ExtractRecordsetNameFromValue(assignValue.Name));
                        assignValue = new AssignValue(DataListUtil.ReplaceObjectBlankWithIndex(assignValue.Name, length), assignValue.Value);
                    }
                    AddSingleDebugOutputItem(dataObject.Environment, innerCount, assignValue, update);
                }
            }
            innerCount++;
            return innerCount;
        }

        void HandleErrors(IDSFDataObject dataObject, int update, ErrorResultTO allErrors)
        {
            var hasErrors = allErrors.HasErrors();
            if (hasErrors)
            {
                DisplayAndWriteError("DsfMultiAssignObjectActivity", allErrors);
                var errorString = allErrors.MakeDisplayReady();
                dataObject.Environment.AddError(errorString);
            }
            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.Before, update);
                DispatchDebugState(dataObject, StateType.After, update);
            }
        }

        void DoCalculation(IExecutionEnvironment environment, string fieldName, string cleanExpression, int update)
        {
            var functionEvaluator = new FunctionEvaluator(FunctionEvaluatorOption.DotNetDateTimeFormat);
            var warewolfEvalResult = environment.Eval(cleanExpression, update);

            if (warewolfEvalResult.IsWarewolfAtomResult && warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult atomResult)
            {
                var eval = PerformCalcForAtom(atomResult.Item, functionEvaluator);
                var doCalculation = new AssignValue(fieldName, eval);
                environment.AssignJson(doCalculation, update);
            }
            if (warewolfEvalResult.IsWarewolfAtomListresult && warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult atomListResult)
            {
                var counter = 1;
                foreach (var item in atomListResult.Item)
                {
                    var eval = PerformCalcForAtom(item, functionEvaluator);
                    var doCalculation = new AssignValue(fieldName, eval);
                    environment.AssignJson(doCalculation, update == 0 ? counter : update);
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

        DebugItem AddSingleInputDebugItem(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue, int update)
        {
            var debugItem = new DebugItem();
            const string VariableLabelText = "Variable";
            const string NewFieldLabelText = "New Value";

            try
            {
                if (!DataListUtil.IsEvaluated(assignValue.Value))
                {
                    AddNotEvaluatedDebugInputItem(environment, innerCount, assignValue, update, debugItem, VariableLabelText, NewFieldLabelText);
                }
                if (DataListUtil.IsEvaluated(assignValue.Value))
                {
                    AddEvaluatedDebugInputItem(environment, innerCount, assignValue, update, debugItem, VariableLabelText, NewFieldLabelText);
                }
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
                AddDefaultDebugItem(innerCount, debugItem);
                AddSingleInputDebugItemAfterException(environment, assignValue, update, debugItem, VariableLabelText, NewFieldLabelText);
            }
            return debugItem;
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        void AddEvaluatedDebugInputItem(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue, int update, DebugItem debugItem, string VariableLabelText, string NewFieldLabelText)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if (assignValue.Name.EndsWith("()]]"))
            {
                throw new Exception("Append data to array");
            }
            var oldValueResult = environment.Eval(assignValue.Name, update);
            var newValueResult = environment.Eval(assignValue.Value, update);
            AddDefaultDebugItem(innerCount, debugItem);
            if (oldValueResult.IsWarewolfAtomResult && newValueResult.IsWarewolfAtomResult)
            {
                var valueResult = newValueResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                var scalarResult = oldValueResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                if (valueResult != null && scalarResult != null)
                {
                    AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item), ExecutionEnvironment.WarewolfAtomToString(valueResult.Item), assignValue.Name, environment.EvalToExpression(assignValue.Value, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
                }
            }
            else if (newValueResult.IsWarewolfAtomResult && oldValueResult.IsWarewolfAtomListresult || oldValueResult.IsWarewolfAtomResult && newValueResult.IsWarewolfAtomListresult)
            {
                AddDebugItem(new DebugItemWarewolfAtomListResult(null, newValueResult, environment.EvalToExpression(assignValue.Value, update), assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
            }
            else
            {
                if (oldValueResult.IsWarewolfAtomListresult && newValueResult.IsWarewolfAtomListresult)
                {
                    var old = (CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult)oldValueResult;
                    if (!old.Item.Any())
                    {
                        AddDebugItem(new DebugItemWarewolfAtomListResult(null, newValueResult, environment.EvalToExpression(assignValue.Value, update), assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
                    }
                    else
                    {
                        var recSetResult = oldValueResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                        AddDebugItem(new DebugItemWarewolfAtomListResult(recSetResult, newValueResult, environment.EvalToExpression(assignValue.Value, update), assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
                    }
                }
            }
        }

        void AddNotEvaluatedDebugInputItem(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue, int update, DebugItem debugItem, string VariableLabelText, string NewFieldLabelText)
        {
            if (assignValue.Name.EndsWith("()]]"))
            {
                throw new Exception("Append data to array");
            }
            var evalResult = environment.Eval(assignValue.Name, update);
            AddDefaultDebugItem(innerCount, debugItem);
            if (evalResult.IsWarewolfAtomResult && evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult scalarResult)
            {
                AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item), assignValue.Value, environment.EvalToExpression(assignValue.Name, update), "", VariableLabelText, NewFieldLabelText, "="), debugItem);
            }
            if (evalResult.IsWarewolfAtomListresult)
            {
                AddDebugItem(new DebugItemWarewolfAtomListResult(null, assignValue.Value, "", environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
            }
        }

        void AddSingleInputDebugItemAfterException(IExecutionEnvironment environment, IAssignValue assignValue, int update, DebugItem debugItem, string VariableLabelText, string NewFieldLabelText)
        {
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

        void AddDefaultDebugItem(int innerCount, DebugItem debugItem)
        {
            AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
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
                    AddNotEvaluatedDebugOutputItem(environment, innerCount, assignValue, update, VariableLabelText, NewFieldLabelText, debugItem);
                }
                if (DataListUtil.IsEvaluated(assignValue.Value))
                {
                    AddEvaluatedDebugOutputItem(environment, innerCount, assignValue, update, VariableLabelText, NewFieldLabelText, debugItem);
                }
            }
            catch (NullValueInVariableException)
            {
                AddDebugItem(new DebugItemWarewolfAtomResult("", assignValue.Value, "", environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
            }
            _debugOutputs.Add(debugItem);
        }

        void AddEvaluatedDebugOutputItem(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue, int update, string VariableLabelText, string NewFieldLabelText, DebugItem debugItem)
        {
            var evalResult = environment.Eval(assignValue.Name, update);
            AddDefaultDebugItem(innerCount, debugItem);
            if (evalResult.IsWarewolfAtomResult && evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult scalarResult)
            {
                AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item), "", environment.EvalToExpression(assignValue.Name, update), "", VariableLabelText, NewFieldLabelText, "="), debugItem);
            }
            var evalResult2 = environment.Eval(assignValue.Value, update);
            if (evalResult.IsWarewolfAtomListresult && evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recSetResult)
            {
                AddDebugItem(DataListUtil.GetRecordsetIndexType(assignValue.Name) == enRecordsetIndexType.Blank ? new DebugItemWarewolfAtomListResult(recSetResult, evalResult2, "", assignValue.Name, VariableLabelText, NewFieldLabelText, "=") : new DebugItemWarewolfAtomListResult(recSetResult, environment.EvalToExpression(assignValue.Value, update), "", assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
            }
        }

        void AddNotEvaluatedDebugOutputItem(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue, int update, string VariableLabelText, string NewFieldLabelText, DebugItem debugItem)
        {
            var evalResult = environment.Eval(assignValue.Name, update);
            AddDefaultDebugItem(innerCount, debugItem);
            if (evalResult.IsWarewolfAtomResult && evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult scalarResult)
            {
                AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item), assignValue.Value, environment.EvalToExpression(assignValue.Name, update), "", VariableLabelText, NewFieldLabelText, "="), debugItem);
            }
            if (evalResult.IsWarewolfAtomListresult && evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recSetResult)
            {
                AddDebugItem(new DebugItemWarewolfAtomListResult(recSetResult, "", "", environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
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
                    Name="Fields Collection",
                    Type=StateVariable.StateType.InputOutput,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(FieldsCollection)
                }
            };
        }

        public bool Equals(DsfDotNetMultiAssignObjectActivity other)
        {
            if (FieldsCollection.Count != other.FieldsCollection.Count)
            {
                return false;
            }

            var eq = base.Equals(other);
            for (int i = 0; i < FieldsCollection.Count; i++)
            {
                eq &= FieldsCollection[i].Equals(other.FieldsCollection[i]);
            }
            eq &= UpdateAllOccurrences.Equals(other.UpdateAllOccurrences);
            eq &= CreateBookmark.Equals(other.CreateBookmark);
            if (!(ServiceHost is null)) {
                eq &= ServiceHost.Equals(other.ServiceHost);
            }

            return eq;
        }

        public override bool Equals(object obj)
        {
            if (obj is DsfDotNetMultiAssignObjectActivity instance)
            {
                return Equals(instance);
            }
            return false;
        }
    }
}