
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
using Dev2.Data.Factories;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.Diagnostics;
using Dev2.Enums;
using Warewolf.Storage;
using WarewolfParserInterop;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
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

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Ctor

        public DsfMultiAssignActivity()
            : base("Assign")
        {
            _fieldsCollection = new List<ActivityDTO>();
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
            _debugOutputs.Clear();
            _debugInputs.Clear();

            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(false);
            toUpsert.IsDebug = (dataObject.IsDebugMode());
            toUpsert.ResourceID = dataObject.ResourceID;

            InitializeDebug(dataObject);

            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();

            try
            {
                if(!errors.HasErrors())
                {
                    int innerCount = 1;
                    foreach (ActivityDTO t in FieldsCollection)
                    {
                        
                        if (!string.IsNullOrEmpty(t.FieldName))
                        {
                            var assignValue = new AssignValue(t.FieldName, t.FieldValue);
                            if (dataObject.IsDebugMode())
                            {
                                AddSingleInputDebugItem(dataObject.Environment, innerCount, assignValue);
                            }
                            dataObject.Environment.AssignWithFrame(assignValue);
                            if (dataObject.IsDebugMode())
                            {
                                AddSingleDebugOutputItem(dataObject.Environment, innerCount, assignValue);
                            }
                        }
                        innerCount++;
                    }
                    dataObject.Environment.CommitAssign();
                    allErrors.MergeErrors(errors);
                }

            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error(e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfAssignActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if(dataObject.IsDebugMode())
                {                   
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        void AddSingleInputDebugItem(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue)
        {
            var debugItem = new DebugItem();
            const string VariableLabelText = "Variable";
            const string NewFieldLabelText = "New Value";
            AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
            try
            {
                if (!DataListUtil.IsEvaluated(assignValue.Value))
                {
                    var evalResult = environment.Eval(assignValue.Name);
                    if (evalResult.IsWarewolfAtomResult)
                    {
                        var scalarResult = evalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                        if (scalarResult != null)
                        {
                            AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item),assignValue.Value, assignValue.Name,"", VariableLabelText, NewFieldLabelText, "="), debugItem);
                        }
                    }
                    else if(evalResult.IsWarewolfAtomListresult)
                    {
                        var recSetResult = evalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;
                        if (recSetResult != null)
                        {
                            AddDebugItem(new DebugItemWarewolfAtomListResult(recSetResult, assignValue.Value, "", assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
                        }
                    }
                }
                else if (DataListUtil.IsEvaluated(assignValue.Value))
                {
                    var oldValueResult = environment.Eval(assignValue.Name);
                    var newValueResult = environment.Eval(assignValue.Value);

                    if (oldValueResult.IsWarewolfAtomResult && newValueResult.IsWarewolfAtomResult)
                    {
                        var valueResult = newValueResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                        var scalarResult = oldValueResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                        if (valueResult != null && scalarResult!=null)
                        {
                            AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item), ExecutionEnvironment.WarewolfAtomToString(valueResult.Item),assignValue.Name, assignValue.Value, VariableLabelText, NewFieldLabelText, "="), debugItem);
                        }
                    }
//                    if (oldValueResult.IsWarewolfAtomResult)
//                    {
//                        var scalarResult = oldValueResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
//                        if (scalarResult == null || scalarResult.Item.IsNothing)
//                        {
//                            AddDebugItem(new DebugItemWarewolfAtomResult(assignValue.Value, assignValue.Value, assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
//                        }
//                        else
//                        {
//                            var value = ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item);
//                            if (newValueResult.IsWarewolfAtomListresult)
//                            {
//                                AddDebugItem(new DebugItemWarewolfAtomListResult(null, newValueResult, assignValue.Value, assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
//                            }
//                            else
//                            {
//                                AddDebugItem(new DebugItemWarewolfAtomResult(value, assignValue.Value, assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
//                            }
//                        }
//                    }
//                    else if (oldValueResult.IsWarewolfAtomListresult)
//                    {
//                        var listResult = oldValueResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;
//                        AddDebugItem(new DebugItemWarewolfAtomListResult(listResult, newValueResult, assignValue.Value, assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
//                    }
                }                
            }
            catch(NullValueInVariableException)
            {
                AddDebugItem(new DebugItemWarewolfAtomResult("", assignValue.Value, assignValue.Name,"", VariableLabelText, NewFieldLabelText, "="), debugItem);
            }
            _debugInputs.Add(debugItem);
        }

        void AddSingleDebugOutputItem(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue)
        {
            const string VariableLabelText = "";
            const string NewFieldLabelText = "";
            var debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
            try
            {
                if (!DataListUtil.IsEvaluated(assignValue.Value))
                {
                    var evalResult = environment.Eval(assignValue.Name);
                    if (evalResult.IsWarewolfAtomResult)
                    {
                        var scalarResult = evalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                        if (scalarResult != null)
                        {
                            AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item),assignValue.Value, assignValue.Name, "" , VariableLabelText, NewFieldLabelText, "="), debugItem);
                        }
                    }
                    else if (evalResult.IsWarewolfAtomListresult)
                    {
                        var recSetResult = evalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;
                        if (recSetResult != null)
                        {
                            AddDebugItem(new DebugItemWarewolfAtomListResult(recSetResult, "", "", assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
                        }
                    }
                }
                else if (DataListUtil.IsEvaluated(assignValue.Value))
                {
                    var evalResult = environment.Eval(assignValue.Name);
                    if (evalResult.IsWarewolfAtomResult)
                    {
                        var scalarResult = evalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                        if (scalarResult != null)
                        {
                            AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item), "", assignValue.Name, "", VariableLabelText, NewFieldLabelText, "="), debugItem);
                        }
                    }
//                    var evalResult = environment.Eval(assignValue.Name);
//                    var evalResult2 = environment.Eval(assignValue.Value);
//                    if (evalResult2.IsWarewolfAtomResult)
//                    {
//                        var valueResult = evalResult2 as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
//                        if (valueResult != null)
//                        {
//                            AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(valueResult.Item),"", assignValue.Value, assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
//                        }
//                    }
//                    if (evalResult.IsWarewolfAtomResult)
//                    {
//                        var scalarResult = evalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
//                        if (scalarResult == null || scalarResult.Item.IsNothing)
//                        {
//                            AddDebugItem(new DebugItemWarewolfAtomResult(assignValue.Value, assignValue.Value,"", assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
//                        }
//                        else
//                        {
//                            var value = ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item);
//                            if (evalResult2.IsWarewolfAtomListresult)
//                            {
//                                AddDebugItem(new DebugItemWarewolfAtomListResult(null, evalResult2, assignValue.Value, assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
//                            }
//                            if (evalResult.IsWarewolfAtomResult)
//                            {
//                                AddDebugItem(new DebugItemWarewolfAtomResult(value, assignValue.Value, assignValue.Name,"", VariableLabelText, NewFieldLabelText, "="), debugItem);
//                            }
//                        }
//                    }
//
//                    else if (evalResult.IsWarewolfAtomListresult)
//                    {
//                        var listResult = evalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;
//                        AddDebugItem(new DebugItemWarewolfAtomListResult(listResult, evalResult2, assignValue.Value, assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
//                    }
                }
            }
            catch (NullValueInVariableException)
            {
                AddDebugItem(new DebugItemWarewolfAtomResult("", assignValue.Value,"", assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
            }
            _debugOutputs.Add(debugItem);
        }

        // ReSharper disable UnusedParameter.Local
        void AddDebugTos(IDev2DataListUpsertPayloadBuilder<string> toUpsert, Guid executionId)
        // ReSharper restore UnusedParameter.Local
        {
            int innerCount = 1;
            const string VariableLabelText = "Variable";
            const string NewFieldLabelText = "New Value";
            foreach(DebugTO debugOutputTo in toUpsert.DebugOutputs)
            {
                if(debugOutputTo != null &&
                  debugOutputTo.TargetEntry != null &&
                  debugOutputTo.TargetEntry.ComplexExpressionAuditor != null)
                {
                    var debugItem = new DebugItem();
                    AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
                    AddDebugItem(new DebugTOParams(debugOutputTo, true, VariableLabelText, NewFieldLabelText), debugItem);
                    innerCount++;
                    _debugInputs.Add(debugItem);
                }
            }

            innerCount = 1;

            foreach(DebugTO debugOutputTo in toUpsert.DebugOutputs)
            {
                if(debugOutputTo != null &&
                   debugOutputTo.TargetEntry != null &&
                   debugOutputTo.TargetEntry.ComplexExpressionAuditor != null)
                {
                    var debugItem = new DebugItem();
                    AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
                    AddDebugItem(new DebugItemVariableParams(debugOutputTo), debugItem);
                    _debugOutputs.Add(debugItem);
                    innerCount++;
                }
            }
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
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

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
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

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList)
        {
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList)
        {
            return _debugOutputs;
        }
         public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
         {
             return _debugOutputs;
         }

         public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
         {
             return _debugInputs;
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
