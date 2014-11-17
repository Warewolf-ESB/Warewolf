using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Factories;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Util;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Dev2.Activities
{
    public abstract class DsfBaseActivity : DsfActivityAbstract<string>
    {
        public new abstract string DisplayName { get; set; }
        

        #region Get Debug Inputs/Outputs

        public List<DebugItem> GetDebugInputs()
        {
            return DebugItem.EmptyList;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion Get Inputs/Outputs

        #region GetForEachInputs/Outputs

        [Outputs("Result")]
        [FindMissing]
        public new string Result { get; set; }

        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = DataListExecutionID.Get(context);
            allErrors.MergeErrors(errors);
            InitializeDebug(dataObject);
            // Process if no errors
            try
            {
                IsSingleValueRule.ApplyIsSingleValueRule(Result, allErrors);

                if (dataObject.IsDebugMode())
                {
                    //AddDebugInputItem(executionId);
                }
                var itemToAdd = new DebugItem();
                IDev2IteratorCollection colItr = Dev2ValueObjectFactory.CreateIteratorCollection();
                IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                var iteratorPropertyDictionary = new Dictionary<string, IDev2DataListEvaluateIterator>();
                foreach (var propertyInfo in GetType().GetProperties().Where(info => info.IsDefined(typeof(Inputs))))
                {
                    var attributes = (Inputs[]) propertyInfo.GetCustomAttributes(typeof(Inputs), false);
                    var variableValue = propertyInfo.GetValue(this) as string;
                    var binaryDataListEntry = compiler.Evaluate(executionId, enActionType.User, variableValue, false, out errors);
                    AddDebugItem(new DebugItemVariableParams(variableValue, attributes[0].UserVisibleName, binaryDataListEntry, executionId),itemToAdd);
                    IDev2DataListEvaluateIterator dtItr = CreateDataListEvaluateIterator(variableValue, executionId, compiler, colItr, allErrors);
                    colItr.AddIterator(dtItr);
                    iteratorPropertyDictionary.Add(propertyInfo.Name,dtItr);

                }
                
                while (colItr.HasMoreData())
                {
                    var evaluatedValues = new Dictionary<string, string>();
                    foreach (var dev2DataListEvaluateIterator in iteratorPropertyDictionary)
                    {
                        var binaryDataListItem = colItr.FetchNextRow(dev2DataListEvaluateIterator.Value);
                        evaluatedValues.Add(dev2DataListEvaluateIterator.Key,binaryDataListItem.TheValue);
                    }
                    var result = PerformExecution(evaluatedValues);

                    toUpsert.Add(Result, result);
                }
  

                allErrors.MergeErrors(errors);
                compiler.Upsert(executionId, toUpsert, out errors);

                if (dataObject.IsDebugMode() && !allErrors.HasErrors())
                {
                    if (dataObject.IsDebugMode() && !allErrors.HasErrors())
                    {
                        AddDebugOutputItem(Result, executionId);
                    }
                }
                allErrors.MergeErrors(errors);
            }
            catch (Exception ex)
            {

                Dev2Logger.Log.Error("Calculate Exception", ex);
                allErrors.AddError(ex.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    DisplayAndWriteError("DsfCustomActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                    compiler.Upsert(executionId, Result, (string)null, out errors);
                }
                if (dataObject.IsDebugMode())
                {
                    if (hasErrors)
                    {
                       // AddDebugOutputItem(Result, executionId);
                    }
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        protected abstract string PerformExecution(Dictionary<string, string> evaluatedValues);


        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates != null)
            {
                var itemUpdate = updates.FirstOrDefault(tuple => tuple.Item1 == Result);
                if (itemUpdate != null)
                {
                    Result = itemUpdate.Item2;
                }
            }
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var result = new List<DsfForEachItem>();
            foreach (var propertyInfo in GetType().GetProperties().Where(info => info.IsDefined(typeof(Inputs))))
            {
                var variableValue = propertyInfo.GetValue(this) as string;
               result.AddRange(GetForEachItems(variableValue));

            }
            return result;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }
        private void AddDebugOutputItem(string expression, Guid executionId)
        {
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            var entry = compiler.Evaluate(executionId, enActionType.User, expression, false, out errors);
            AddDebugOutputItem(new DebugItemVariableParams(expression, "", entry, executionId));
        }
        #endregion
    }
}