#pragma warning disable
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
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities
{
    public abstract class DsfBaseActivity : DsfActivityAbstract<string>,IEquatable<DsfBaseActivity>
    {
        List<string> _executionResult;
        public IResponseManager ResponseManager { get; set; }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update) => _debugInputs;

        [Outputs("Result")]
        [FindMissing]
        public new string Result { get; set; }

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();

            ExecuteTool(dataObject, 0);
        }

        public override List<string> GetOutputs() => new List<string> { Result };

        protected IDSFDataObject DataObject { get; private set; }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            var allErrors = new ErrorResultTO();
            var errors = new ErrorResultTO();
            _executionResult = new List<string>();
            DataObject = dataObject;
            allErrors.MergeErrors(errors);
            InitializeDebug(dataObject);
            // Process if no errors
            try
            {
                TryExecute(dataObject, update, allErrors, errors);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(string.Format("{0} Exception", DisplayName), ex, GlobalConstants.WarewolfError);
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

#pragma warning disable S1541 // Methods and properties should not be too complex
        private void TryExecute(IDSFDataObject dataObject, int update, ErrorResultTO allErrors, ErrorResultTO errors)
#pragma warning restore S1541 // Methods and properties should not be too complex
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

            if (dataObject.IsDebugMode() && !allErrors.HasErrors() && !string.IsNullOrWhiteSpace(Result) && dataObject.IsDebugMode() && !allErrors.HasErrors() && !string.IsNullOrEmpty(Result))
            {
                AddDebugOutputItem(new DebugEvalResult(Result, "", dataObject.Environment, update));
            }


            allErrors.MergeErrors(errors);
        }

        protected virtual void AssignResult(IDSFDataObject dataObject, int update)
        {
            if (!string.IsNullOrEmpty(Result))
            {
                if (DataListUtil.IsValueScalar(Result))
                {
                    dataObject.Environment.Assign(Result, _executionResult.Last(), update);
                }
                foreach (var res in _executionResult)
                {
                    dataObject.Environment.Assign(Result, res, update);
                }
            }
        }

        protected abstract List<string> PerformExecution(Dictionary<string, string> evaluatedValues);

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            foreach (var update in updates)
            {
                var propertyInfo = GetType().GetProperty(update.Item1);
                propertyInfo?.SetValue(this, update.Item2);
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

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(Result);

        public bool Equals(DsfBaseActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) && string.Equals(Result, other.Result);
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

            return Equals((DsfBaseActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_executionResult != null ? _executionResult.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}