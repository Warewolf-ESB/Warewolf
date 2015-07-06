using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Util;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Storage;

namespace Dev2.Activities
{
    public abstract class DsfBaseActivity : DsfActivityAbstract<string>
    {
        public new abstract string DisplayName { get; set; }
        

        #region Get Debug Inputs/Outputs
        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList)
        {
          return _debugInputs;
        }

        #endregion Get Inputs/Outputs

        #region GetForEachInputs/Outputs

        [Outputs("Result")]
        [FindMissing]
        public new string Result { get; set; }

        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            ExecuteTool(dataObject);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject)
        {

            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            //Guid executionId = DataListExecutionID.Get(context);
            allErrors.MergeErrors(errors);
            InitializeDebug(dataObject);
            // Process if no errors
            try
            {
                IsSingleValueRule.ApplyIsSingleValueRule(Result, allErrors);

                var colItr = new WarewolfListIterator();
                var iteratorPropertyDictionary = new Dictionary<string, IWarewolfIterator>();
                foreach(var propertyInfo in GetType().GetProperties().Where(info => info.IsDefined(typeof(Inputs))))
                {
                    var attributes = (Inputs[])propertyInfo.GetCustomAttributes(typeof(Inputs), false);
                    var variableValue = propertyInfo.GetValue(this) as string;
                    if(dataObject.IsDebugMode())
                    {
                        AddDebugInputItem(new DebugEvalResult(variableValue, attributes[0].UserVisibleName, dataObject.Environment));
                    }
                    var dtItr = CreateDataListEvaluateIterator(variableValue, dataObject.Environment);
                    colItr.AddVariableToIterateOn(dtItr);
                    iteratorPropertyDictionary.Add(propertyInfo.Name, dtItr);
                }
                while(colItr.HasMoreData())
                {
                    var evaluatedValues = new Dictionary<string, string>();
                    foreach(var dev2DataListEvaluateIterator in iteratorPropertyDictionary)
                    {
                        var binaryDataListItem = colItr.FetchNextValue(dev2DataListEvaluateIterator.Value);
                        evaluatedValues.Add(dev2DataListEvaluateIterator.Key, binaryDataListItem);
                    }
                    var result = PerformExecution(evaluatedValues);
                    dataObject.Environment.Assign(Result, result);
                }

                allErrors.MergeErrors(errors);

                if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                {
                    if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                    {
                        AddDebugOutputItem(new DebugEvalResult(Result,"",dataObject.Environment));
                    }
                }
                allErrors.MergeErrors(errors);
            }
            catch(Exception ex)
            {
                Dev2Logger.Log.Error(string.Format("{0} Exception", DisplayName), ex);
                allErrors.AddError(ex.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError(DisplayName, allErrors);
                    var errorList = allErrors.MakeDataListReady();
                    dataObject.Environment.AddError(errorList);
                    dataObject.Environment.Assign(Result, null);
                }
                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before);
                    DispatchDebugState(dataObject, StateType.After);
                }
            }
        }

        protected abstract string PerformExecution(Dictionary<string, string> evaluatedValues);


        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            foreach(var update in updates)
            {
                var propertyInfo = GetType().GetProperty(update.Item1);
                if(propertyInfo != null)
                {
                    propertyInfo.SetValue(this, update.Item2);
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
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
        
        #endregion
    }
}