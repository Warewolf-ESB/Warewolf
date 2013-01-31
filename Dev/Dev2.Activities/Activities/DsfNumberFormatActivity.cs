using Dev2;
using Dev2.Activities;
using Dev2.Common.ExtMethods;
using Dev2.Data.Operations;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using System;
using System.Activities;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{

    public class DsfNumberFormatActivity : DsfActivityAbstract<string>
    {
        #region Class Members

        // ReSharper disable InconsistentNaming
        private static readonly IDev2NumberFormatter _numberFormatter; //  REVIEW : Should this not be an instance variable....
        // ReSharper restore InconsistentNaming

        #endregion Class Members

        #region Constructors

        public DsfNumberFormatActivity()
            : base("Format Number")
        {

        }

        static DsfNumberFormatActivity()
        {
            _numberFormatter = new Dev2NumberFormatter(); // REVIEW : Please use a factory method to create
        }
        #endregion Constructors

        #region Properties

        [Inputs("Expression")]
        public string Expression { get; set; }

        [Inputs("RoundingType")]
        public string RoundingType { get; set; }

        [Inputs("RoundingDecimalPlaces")]
        public string RoundingDecimalPlaces { get; set; }

        [Inputs("DecimalPlacesToShow")]
        public string DecimalPlacesToShow { get; set; }

        [Outputs("Result")]
        public new string Result { get; set; }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        #endregion Properties

        #region Override Methods

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            var compiler = context.GetExtension<IDataListCompiler>();

            var allErrors = new ErrorResultTO();
            ErrorResultTO errors;

            Guid executionId = DataListExecutionID.Get(context);

            try
            {
                string expression = Expression ?? string.Empty;
                string roundingDecimalPlaces = RoundingDecimalPlaces ?? string.Empty;
                string decimalPlacesToShow = DecimalPlacesToShow ?? string.Empty;

                IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                IDev2IteratorCollection colItr = Dev2ValueObjectFactory.CreateIteratorCollection();

                IDev2DataListEvaluateIterator expressionIterator = CreateDataListEvaluateIterator(expression, executionId, compiler, colItr, allErrors);

                IDev2DataListEvaluateIterator roundingDecimalPlacesIterator = CreateDataListEvaluateIterator(roundingDecimalPlaces, executionId, compiler, colItr, allErrors);
                IDev2DataListEvaluateIterator decimalPlacesToShowIterator = CreateDataListEvaluateIterator(decimalPlacesToShow, executionId, compiler, colItr, allErrors);

                // Loop data ;)
                while (colItr.HasMoreData())
                {
                    int roundingDecimalPlacesValue;
                    colItr.FetchNextRow(roundingDecimalPlacesIterator).TheValue.IsWholeNumber(out roundingDecimalPlacesValue);

                    int decimalPlacesToShowValue;
                    bool adjustDecimalPlaces = colItr.FetchNextRow(decimalPlacesToShowIterator).TheValue.IsWholeNumber(out decimalPlacesToShowValue);

                    FormatNumberTO formatNumberTo = new FormatNumberTO(colItr.FetchNextRow(expressionIterator).TheValue, RoundingType, roundingDecimalPlacesValue, adjustDecimalPlaces, decimalPlacesToShowValue);
                    string result = _numberFormatter.Format(formatNumberTo);

                    toUpsert.Add(Result, result);
                    toUpsert.FlushIterationFrame();
                }

                compiler.Upsert(executionId, toUpsert, out errors);
                allErrors.MergeErrors(errors);

                compiler.Shape(executionId, enDev2ArgumentType.Output, OutputMapping, out errors);
                allErrors.MergeErrors(errors);

            }
            catch (Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                #region Handle Errors

                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfNumberFormatActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, allErrors.MakeDataListReady(), out errors);
                }

                #endregion
            }
        }

        #endregion

        #region Get Debug Inputs/Outputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> results = new List<IDebugItem>();

            if (!string.IsNullOrEmpty(Expression))
            {
                foreach (IDebugItem debugItem in CreateDebugItems(Expression, dataList))
                {
                    // BUG 8104 : Refactor DebugItem
                    //debugItem.Label = debugItem.Label + " Number To Format ";
                    results.Add(debugItem);
                }
            }

            if (!string.IsNullOrEmpty(RoundingType))
            {
                results.Add(new DebugItem(" Rounding Type ", RoundingType, null));
            }

            if (!string.IsNullOrEmpty(RoundingDecimalPlaces))
            {
                foreach (IDebugItem debugItem in CreateDebugItems(RoundingDecimalPlaces, dataList))
                {
                    // BUG 8104 : Refactor DebugItem
                    //debugItem.Label = debugItem.Label + " Rounding Decimal Places ";
                    results.Add(debugItem);
                }
            }

            if (!string.IsNullOrEmpty(DecimalPlacesToShow))
            {
                foreach (IDebugItem debugItem in CreateDebugItems(DecimalPlacesToShow, dataList))
                {
                    // BUG 8104 : Refactor DebugItem
                    //debugItem.Label = debugItem.Label + " Decimal Places To Show ";
                    results.Add(debugItem);
                }
            }

            return results;
        }

        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> results = new List<IDebugItem>();

            if (!string.IsNullOrEmpty(Result))
            {
                foreach (IDebugItem debugItem in CreateDebugItems(Result, dataList))
                {
                    results.Add(debugItem);
                }
            }
            return results;
        }

        #endregion

        #region Update ForEach Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach (Tuple<string, string> t in updates)
            {

                if (t.Item1 == Expression)
                {
                    Expression = t.Item2;
                }

                if (t.Item1 == RoundingType)
                {
                    RoundingType = t.Item2;
                }

                if (t.Item1 == RoundingDecimalPlaces)
                {
                    RoundingDecimalPlaces = t.Item2;
                }

                if (t.Item1 == DecimalPlacesToShow)
                {
                    DecimalPlacesToShow = t.Item2;
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates.Count == 1)
            {
                Result = updates[0].Item2;
            }
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.Before, Expression, RoundingType, RoundingDecimalPlaces, DecimalPlacesToShow);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.After, Result);
        }

        #endregion

    }
}
