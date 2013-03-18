using System.Globalization;
using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.MathOperations;
using System;
using System.Activities;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfCalculateActivity : DsfActivityAbstract<string>
    {
        #region Fields

        private IList<IDebugItem> _debugInputs = new List<IDebugItem>();
        private IList<IDebugItem> _debugOutputs = new List<IDebugItem>();

            #endregion

        #region Properties

        /// <summary>
        /// The property that holds the Expression string the user enters into the "fx" box
        /// </summary>
        [Inputs("Expression")]
        public string Expression { get; set; }

        /// <summary>
        /// The property that holds the Result string the user enters into the "Result" box
        /// </summary>
        [Outputs("Result")]
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

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        /// <summary>
        /// The execute method that is called when the activity is executed at run time and will hold all the logic of the activity
        /// </summary> 
        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<IDebugItem>();
            _debugOutputs = new List<IDebugItem>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            //IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = DataListExecutionID.Get(context);
            allErrors.MergeErrors(errors);

            // Process if no errors
            try
            {
                if(dataObject.IsDebug)
                {
                    AddDebugInputItem(executionId);    
                }                
                string result = string.Empty;
                IFunctionEvaluator functionEvaluator = MathOpsFactory.CreateFunctionEvaluator();
                IEvaluationFunction evaluationFunctionTO = MathOpsFactory.CreateEvaluationExpressionTO(Expression);

                result = functionEvaluator.EvaluateFunction(evaluationFunctionTO, executionId, out errors);
                allErrors.MergeErrors(errors);

                compiler.Upsert(executionId, Result, result, out errors);
                if (dataObject.IsDebug)
                {
                    AddDebugOutputItem(Result,result,executionId);
                }
                allErrors.MergeErrors(errors);
            }
            finally
            {

                // Handle Errors
                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfCalculateActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, allErrors.MakeDataListReady(), out errors);
                }
                if (dataObject.IsDebug)
                {
                    DispatchDebugState(context,StateType.Before);
                }
            }

        }

        #endregion Override Abstract Methods

        #region Private Methods

        private void AddDebugInputItem(Guid executionId)
        {   
            ErrorResultTO errors = new ErrorResultTO();
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Calculate" });
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = Expression });
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression });
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataListEntry entry = compiler.Evaluate(executionId, enActionType.CalculateSubstitution, Expression, false, out errors);
            IBinaryDataListItem item = entry.FetchScalar();
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = item.TheValue });            

            _debugInputs.Add(itemToAdd);
        }

        private void AddDebugOutputItem(string expression, string value, Guid dlId)
        {           
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.AddRange(CreateDebugItemsFromString(expression,value,dlId,0,enDev2ArgumentType.Output));
            _debugOutputs.Add(itemToAdd);                                              
        }

        #endregion Private Methods

        #region Get Debug Inputs/Outputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {            
            return _debugInputs;
        }

        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {            
            return _debugOutputs;
        }

        #endregion Get Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {

            if (updates.Count == 1)
            {
                Expression = updates[0].Item2;
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates.Count == 1)
            {
                Result = updates[0].Item2;
            }
        }


        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.Before, Expression);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.After, Result);
        }

        #endregion
    }
}

