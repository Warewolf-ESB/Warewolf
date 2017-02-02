/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.MathOperations;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using WarewolfParserInterop;
// ReSharper disable UnusedMember.Global

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    [ToolDescriptorInfo("Data-Assign", "Assign", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Data", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Data_Assign")]
    public class DsfMultiAssignActivity : DsfActivityAbstract<string>
    {
        #region Constants
        public const string CalculateTextConvertPrefix = GlobalConstants.CalculateTextConvertPrefix;
        public const string CalculateTextConvertSuffix = GlobalConstants.CalculateTextConvertSuffix;
        public const string CalculateTextConvertFormat = GlobalConstants.CalculateTextConvertFormat;
        #endregion

        #region Fields

        private IList<ActivityDTO> _fieldsCollection;

        #endregion

        #region Properties

        // ReSharper disable ConvertToAutoProperty
        public IList<ActivityDTO> FieldsCollection
        // ReSharper restore ConvertToAutoProperty
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

        #endregion

        #region Ctor

        public DsfMultiAssignActivity()
            : base("Assign")
        {
            _fieldsCollection = new List<ActivityDTO>();
        }

        #endregion

        public override List<string> GetOutputs()
        {
            return FieldsCollection.Select(dto => dto.FieldName).ToList();
        }

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

        #region Overrides of DsfNativeActivity<string>

        //public override IDev2Activity Execute(IDSFDataObject data)
        //{
        //    ExecuteTool(data);
        //    if(NextNodes != null && NextNodes.Count()>0)
        //    {
        //        NextNodes.First().Execute(data);
        //        return NextNodes.First();
        //    }
        //    return null;
        //}

        #endregion

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _debugOutputs.Clear();
            _debugInputs.Clear();

            InitializeDebug(dataObject);
            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();

            try
            {
                if(!errors.HasErrors())
                {
                    int innerCount = 1;
                    foreach(ActivityDTO t in FieldsCollection)
                    {
                        try
                        {
                            if(!string.IsNullOrEmpty(t.FieldName))
                            {
                                string cleanExpression;
                                var assignValue = new AssignValue(t.FieldName, t.FieldValue);
                                var isCalcEvaluation = DataListUtil.IsCalcEvaluation(t.FieldValue, out cleanExpression);
                                if(isCalcEvaluation)
                                {
                                    assignValue = new AssignValue(t.FieldName, cleanExpression);
                                }
                                DebugItem debugItem = null;
                                if(dataObject.IsDebugMode())
                                {
                                    debugItem = AddSingleInputDebugItem(dataObject.Environment, innerCount, assignValue, update);
                                }
                                if (isCalcEvaluation)
                                {
                                     DoCalculation(dataObject.Environment, t.FieldName, t.FieldValue, update);
                                }
                                else
                                {
                                    dataObject.Environment.AssignWithFrame(assignValue, update);
                                }
                                if(debugItem != null)
                                {
                                    _debugInputs.Add(debugItem);
                                }
                                if(dataObject.IsDebugMode())
                                {
                                    if(DataListUtil.IsValueRecordset(assignValue.Name) && DataListUtil.GetRecordsetIndexType(assignValue.Name) == enRecordsetIndexType.Blank)
                                    {
                                        var length = dataObject.Environment.GetLength(DataListUtil.ExtractRecordsetNameFromValue(assignValue.Name));
                                        assignValue = new AssignValue(DataListUtil.ReplaceRecordsetBlankWithIndex(assignValue.Name, length), assignValue.Value);
                                    }
                                    AddSingleDebugOutputItem(dataObject.Environment, innerCount, assignValue, update);
                                }
                            }
                            innerCount++;
                        }
                        catch(Exception e)
                        {
                            Dev2Logger.Error(e);
                            allErrors.AddError(e.Message);
                        }
                    }
                    dataObject.Environment.CommitAssign();
                    allErrors.MergeErrors(errors);
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Error(e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfAssignActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                }
                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        private void DoCalculation(IExecutionEnvironment environment, string fieldName, string cleanExpression, int update)
        {
            var functionEvaluator = new FunctionEvaluator();
            var warewolfEvalResult = environment.Eval(cleanExpression, update);
         
            if (warewolfEvalResult.IsWarewolfAtomResult)
            {
                var result = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                if(result != null)
                {
                    var eval = PerformCalcForAtom(result.Item, functionEvaluator);
                    var doCalculation = new AssignValue(fieldName, eval);
                    environment.AssignWithFrame(doCalculation, update);                    
                }
            }
            if (warewolfEvalResult.IsWarewolfAtomListresult)
            {
                var result = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                if (result != null)
                {
                    var counter = 1;
                    foreach(var item in result.Item)
                    {
                        var eval = PerformCalcForAtom(item, functionEvaluator);
                        var doCalculation = new AssignValue(fieldName, eval);
                        environment.AssignWithFrame(doCalculation, update == 0 ? counter : update);
                        counter++;
                    }
                }
            }
        }

        private static string PerformCalcForAtom(DataStorage.WarewolfAtom warewolfAtom, FunctionEvaluator functionEvaluator)
        {
            var calcExpression = ExecutionEnvironment.WarewolfAtomToString(warewolfAtom);
            string exp;
            DataListUtil.IsCalcEvaluation(calcExpression, out exp);
            string eval;
            string error;
            var res = functionEvaluator.TryEvaluateFunction(exp, out eval, out error);
            if(eval == exp.Replace("\"", "") && exp.Contains("\""))
            {
                try
                {
                    string eval2;
                    var b = functionEvaluator.TryEvaluateFunction(exp.Replace("\"", ""), out eval2, out error);
                    if(b)
                    {
                        eval = eval2;
                    }
                }
                catch(Exception err)
                {
                    Dev2Logger.Warn(err);
                }
            }
            if(!res)
            {
                throw new Exception(ErrorResource.InvalidCalculate);
            }
            return eval;
        }

        private DebugItem AddSingleInputDebugItem(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue, int update)
        {
            var debugItem = new DebugItem();
            const string VariableLabelText = "Variable";
            const string NewFieldLabelText = "New Value";
            
            try
            {
                if (!DataListUtil.IsEvaluated(assignValue.Value))
                {
                    var evalResult = environment.Eval(assignValue.Name, update);
                    AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
                    if (evalResult.IsWarewolfAtomResult)
                    {
                        var scalarResult = evalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                        if (scalarResult != null)
                        {
                            AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item),assignValue.Value, environment.EvalToExpression(assignValue.Name, update),"", VariableLabelText, NewFieldLabelText, "="), debugItem);
                        }
                    }
                    else if(evalResult.IsWarewolfAtomListresult)
                    {
                        if (DataListUtil.GetRecordsetIndexType(assignValue.Name) == enRecordsetIndexType.Blank)
                        {
                            AddDebugItem(new DebugItemWarewolfAtomListResult(null, assignValue.Value, "", environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
                        }
                        else
                        {
                            var recSetResult = evalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                            if (recSetResult != null)
                            {
                                AddDebugItem(new DebugItemWarewolfAtomListResult(recSetResult, assignValue.Value, "", environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
                            }
                        }
                    }
                }
                else if (DataListUtil.IsEvaluated(assignValue.Value))
                {
                    var oldValueResult = environment.Eval(assignValue.Name, update);
                    var newValueResult = environment.Eval(assignValue.Value, update);
                    AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
                    if (oldValueResult.IsWarewolfAtomResult && newValueResult.IsWarewolfAtomResult)
                    {
                        var valueResult = newValueResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                        var scalarResult = oldValueResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                        if (valueResult != null && scalarResult!=null)
                        {
                            AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item), ExecutionEnvironment.WarewolfAtomToString(valueResult.Item),assignValue.Name, environment.EvalToExpression(assignValue.Value, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
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
                    else if (oldValueResult.IsWarewolfAtomListresult && newValueResult.IsWarewolfAtomListresult)
                    {
                        var recSetResult = oldValueResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                        AddDebugItem(new DebugItemWarewolfAtomListResult(recSetResult, newValueResult, environment.EvalToExpression(assignValue.Value, update), environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
                    }
                }                
            }
            catch(Exception e)
            {
                if (e.Message.Contains("ParseError"))
                {
                    AddDebugItem(new DebugItemWarewolfAtomResult("", assignValue.Value,environment.EvalToExpression(assignValue.Name, update), "", VariableLabelText, NewFieldLabelText, "="), debugItem);
                    return debugItem;
                }
                string errorMessage;
                if (!ExecutionEnvironment.IsValidVariableExpression(assignValue.Name, out errorMessage, update))
                {
                    return null;
                }
                AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
                if (DataListUtil.IsEvaluated(assignValue.Value))
                {
                    var newValueResult = environment.Eval(assignValue.Value, update);

                    if (newValueResult.IsWarewolfAtomResult)
                    {
                        var valueResult = newValueResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                        if (valueResult != null)
                        {
                            AddDebugItem(new DebugItemWarewolfAtomResult("", ExecutionEnvironment.WarewolfAtomToString(valueResult.Item), environment.EvalToExpression(assignValue.Name, update), assignValue.Value, VariableLabelText, NewFieldLabelText, "="), debugItem);
                        }
                    }
                    else if (newValueResult.IsWarewolfAtomListresult)
                    {
                        AddDebugItem(new DebugItemWarewolfAtomListResult(null, newValueResult, environment.EvalToExpression(assignValue.Value, update), assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
                    }
                }
                else
                {
                    AddDebugItem(new DebugItemWarewolfAtomResult("", assignValue.Value, environment.EvalToExpression(assignValue.Name, update), "", VariableLabelText, NewFieldLabelText, "="), debugItem);
                }
            }
            return debugItem;
        }

        private void AddSingleDebugOutputItem(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue, int update)
        {
            const string VariableLabelText = "";
            const string NewFieldLabelText = "";
            var debugItem = new DebugItem();
            
            try
            {
                if (!DataListUtil.IsEvaluated(assignValue.Value))
                {
                    var evalResult = environment.Eval(assignValue.Name, update);
                    AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
                    if (evalResult.IsWarewolfAtomResult)
                    {
                        var scalarResult = evalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                        if (scalarResult != null)
                        {
                            AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item), assignValue.Value, environment.EvalToExpression(assignValue.Name, update), "", VariableLabelText, NewFieldLabelText, "="), debugItem);
                        }
                    }
                    else if (evalResult.IsWarewolfAtomListresult)
                    {
                        var recSetResult = evalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                        if (recSetResult != null)
                        {
                            AddDebugItem(new DebugItemWarewolfAtomListResult(recSetResult, "", "", environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
                        }
                    }
                }
                else if (DataListUtil.IsEvaluated(assignValue.Value))
                {
                    var evalResult = environment.Eval(assignValue.Name, update);
                    AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
                    if (evalResult.IsWarewolfAtomResult)
                    {
                        var scalarResult = evalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                        if (scalarResult != null)
                        {
                            AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item), "", environment.EvalToExpression(assignValue.Name, update), "", VariableLabelText, NewFieldLabelText, "="), debugItem);
                        }
                    }
                    var evalResult2 = environment.Eval(assignValue.Value, update);                                  
                    if (evalResult.IsWarewolfAtomListresult)
                    {
                        var recSetResult = evalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                        if (recSetResult != null)
                        {
                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                            if (DataListUtil.GetRecordsetIndexType(assignValue.Name) == enRecordsetIndexType.Blank)
                            {
                                AddDebugItem(new DebugItemWarewolfAtomListResult(recSetResult, evalResult2, "", assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
                            }
                            else
                            {

                                AddDebugItem(new DebugItemWarewolfAtomListResult(recSetResult, environment.EvalToExpression(assignValue.Value, update), "", environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);

                            }
                        }
                    }
                }
            }
            catch (NullValueInVariableException)
            {
                AddDebugItem(new DebugItemWarewolfAtomResult("", assignValue.Value, "", environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
            }
            _debugOutputs.Add(debugItem);
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            foreach(Tuple<string, string> t in updates)
            {
                // locate all updates for this tuple
                Tuple<string, string> t1 = t;
                var items = FieldsCollection.Where(c => !string.IsNullOrEmpty(c.FieldValue) && c.FieldValue.Contains(t1.Item1));

                // issues updates
                foreach(var a in items)
                {
                    a.FieldValue = a.FieldValue.Replace(t.Item1, t.Item2);
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            foreach(Tuple<string, string> t in updates)
            {

                // locate all updates for this tuple
                Tuple<string, string> t1 = t;
                var items = FieldsCollection.Where(c => !string.IsNullOrEmpty(c.FieldName) && c.FieldName.Contains(t1.Item1));

                // issues updates
                foreach(var a in items)
                {
                    a.FieldName = a.FieldName.Replace(t.Item1, t.Item2);
                }
            }
        }

        #endregion

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList, int update)
        {
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
        {
            return _debugOutputs;
        }
        
        #endregion Get Inputs/Outputs

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return (from item in FieldsCollection
                    where !string.IsNullOrEmpty(item.FieldValue) && item.FieldValue.Contains("[[")
                    select new DsfForEachItem { Name = item.FieldName, Value = item.FieldValue }).ToList();
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return (from item in FieldsCollection
                    where !string.IsNullOrEmpty(item.FieldName) && item.FieldName.Contains("[[")
                    select new DsfForEachItem { Name = item.FieldValue, Value = item.FieldName }).ToList();
        }

        #endregion

       
    }

}
