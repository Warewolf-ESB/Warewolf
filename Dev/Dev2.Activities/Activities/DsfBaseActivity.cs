using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics;
using Dev2.Util;
using Dev2.Validation;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Storage;

namespace Dev2.Activities
{
    public abstract class DsfBaseActivity : DsfActivityAbstract<string>
    {
        private List<string> _executionResult;

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList, int update)
        {
            return _debugInputs;
        }

        #endregion Get Debug Inputs/Outputs

        #region GetForEachInputs/Outputs

        [Outputs("Result")]
        [FindMissing]
        public new string Result { get; set; }

        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            ExecuteTool(dataObject, 0);
        }


        public override List<string> GetOutputs()
        {
            return new List<string> {Result};
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            _executionResult = new List<string>();
            allErrors.MergeErrors(errors);
            InitializeDebug(dataObject);
            // Process if no errors
            try
            {
                IsSingleValueRule.ApplyIsSingleValueRule(Result, allErrors);

                var colItr = new WarewolfListIterator();
                var iteratorPropertyDictionary = new Dictionary<string, IWarewolfIterator>();
                foreach (var propertyInfo in GetType().GetProperties().Where(info => info.IsDefined(typeof(Inputs))))
                {
                    var attributes = (Inputs[])propertyInfo.GetCustomAttributes(typeof(Inputs), false);
                    var variableValue = propertyInfo.GetValue(this) as string;
                    if (!string.IsNullOrEmpty(variableValue))
                    {
                        if (dataObject.IsDebugMode())
                        {
                            AddDebugInputItem(new DebugEvalResult(variableValue, attributes[0].UserVisibleName, dataObject.Environment, update));
                        }

                        var dtItr = CreateDataListEvaluateIterator(variableValue, dataObject.Environment, update);
                        colItr.AddVariableToIterateOn(dtItr);
                        iteratorPropertyDictionary.Add(propertyInfo.Name, dtItr);
                    }
                }
                if (colItr.FieldCount <= 0)
                {
                    var evaluatedValues = new Dictionary<string, string>();                    
                    _executionResult = PerformExecution(evaluatedValues);
                    AssignResult(dataObject, update);
                }
                else
                {
                    while (colItr.HasMoreData())
                    {
                        var evaluatedValues = new Dictionary<string, string>();
                        foreach (var dev2DataListEvaluateIterator in iteratorPropertyDictionary)
                        {
                            var binaryDataListItem = colItr.FetchNextValue(dev2DataListEvaluateIterator.Value);
                            evaluatedValues.Add(dev2DataListEvaluateIterator.Key, binaryDataListItem);
                        }
                        _executionResult = PerformExecution(evaluatedValues);
                        AssignResult(dataObject, update);
                    }
                }

                if (dataObject.IsDebugMode() && !allErrors.HasErrors() && !string.IsNullOrWhiteSpace(Result))
                if (dataObject.IsDebugMode() && !allErrors.HasErrors())
                {
                        if (!string.IsNullOrEmpty(Result))
                            AddDebugOutputItem(new DebugEvalResult(Result, "", dataObject.Environment, update));
                }
                allErrors.MergeErrors(errors);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(string.Format("{0} Exception", DisplayName), ex);
                allErrors.AddError(ex.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    DisplayAndWriteError(DisplayName, allErrors);
                    var errorList = allErrors.MakeDataListReady();
                    dataObject.Environment.AddError(errorList);
                    dataObject.Environment.Assign(Result, DisplayName.ToUpper().Contains("Dropbox".ToUpper()) ? GlobalConstants.DropBoxFailure : null, update);
                }
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        protected virtual void AssignResult(IDSFDataObject dataObject, int update)
        {
            if (!string.IsNullOrEmpty(Result))
            {
                if (DataListUtil.IsValueScalar(Result))
                {
                    dataObject.Environment.Assign(Result, _executionResult.Last(), update);
                }
                foreach(var res in _executionResult)
                {
                    dataObject.Environment.Assign(Result,res, update);
                }
                
            }
        }

        protected abstract List<string> PerformExecution(Dictionary<string, string> evaluatedValues);

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            foreach (var update in updates)
            {
                var propertyInfo = GetType().GetProperty(update.Item1);
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(this, update.Item2);
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == Result);
            if (itemUpdate != null)
            {
                Result = itemUpdate.Item2;
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

        #endregion GetForEachInputs/Outputs
    }
}