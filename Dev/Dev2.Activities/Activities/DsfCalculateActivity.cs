using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.MathOperations;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfCalculateActivity : DsfActivityAbstract<string>
    {

        #region Properties

        /// <summary>
        /// The property that holds the Expression string the user enters into the "fx" box
        /// </summary>
        [Inputs("Expression")]
        [FindMissing]
        public string Expression { get; set; }

        /// <summary>
        /// The property that holds the Result string the user enters into the "Result" box
        /// </summary>
        [Outputs("Result")]
        [FindMissing]
        public new string Result { get; set; }

        #endregion Properties

        #region Ctor

        public DsfCalculateActivity()
            : base("Calculate")
        {
            Expression = string.Empty;
            Result = string.Empty;
        }

        #endregion Ctor

        #region Override Abstract Methods

        /// <summary>
        /// The execute method that is called when the activity is executed at run time and will hold all the logic of the activity
        /// </summary> 
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
                if(dataObject.IsDebugMode())
                {
                    AddDebugInputItem(executionId);
                }
                IFunctionEvaluator functionEvaluator = MathOpsFactory.CreateFunctionEvaluator();

                string input = string.IsNullOrEmpty(Expression) ? Expression : Expression.Replace("\\r", string.Empty).Replace("\\n", string.Empty).Replace(Environment.NewLine, "");

                IEvaluationFunction evaluationFunctionTO = MathOpsFactory.CreateEvaluationExpressionTO(input);

                string result = functionEvaluator.EvaluateFunction(evaluationFunctionTO, executionId, out errors);
                allErrors.MergeErrors(errors);

                compiler.Upsert(executionId, Result, result, out errors);

                if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                {
                    AddDebugOutputItem(Result, executionId);
                }
                allErrors.MergeErrors(errors);
            }
            catch(Exception ex)
            {
                allErrors.AddError(ex.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfCalculateActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if(dataObject.IsDebugMode())
                {
                    if(hasErrors)
                    {
                        AddDebugOutputItem(Result, executionId);
                    }
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }

        }

        #endregion Override Abstract Methods

        #region Private Methods

        private void AddDebugInputItem(Guid executionId)
        {
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            var entry = compiler.Evaluate(executionId, enActionType.CalculateSubstitution, Expression, false, out errors);
            AddDebugInputItem(new DebugItemVariableParams(Expression, "fx =", entry, executionId));
        }

        private void AddDebugOutputItem(string expression, Guid executionId)
        {
            ErrorResultTO errors;
            var compiler = DataListFactory.CreateDataListCompiler();
            var entry = compiler.Evaluate(executionId, enActionType.User, expression, false, out errors);
            AddDebugOutputItem(new DebugItemVariableParams(expression, "", entry, executionId));
        }

        #endregion Private Methods

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion Get Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {

            if(updates != null && updates.Count == 1)
            {
                Expression = updates[0].Item2;
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                var itemUpdate = updates.FirstOrDefault(tuple => tuple.Item1 == Result);
                if(itemUpdate != null)
                {
                    Result = itemUpdate.Item2;
                }
            }
        }


        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(Expression);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion
    }
}

