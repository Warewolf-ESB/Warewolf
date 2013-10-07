using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Data.Factories;
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
using Dev2.Util;
using Dev2.Utilities;
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
            RoundingType = "None";
        }

        static DsfNumberFormatActivity()
        {
            _numberFormatter = new Dev2NumberFormatter(); // REVIEW : Please use a factory method to create

        }
        #endregion Constructors

        #region Properties

        [Inputs("Expression")]
        [FindMissing]
        public string Expression { get; set; }

        [Inputs("RoundingType")]        
        public string RoundingType { get; set; }

        [Inputs("RoundingDecimalPlaces")]
        [FindMissing]
        public string RoundingDecimalPlaces { get; set; }

        [Inputs("DecimalPlacesToShow")]
        [FindMissing]
        public string DecimalPlacesToShow { get; set; }

        [Outputs("Result")]
        [FindMissing]
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
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            var dataObject = context.GetExtension<IDSFDataObject>();            
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

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

                if (dataObject.IsDebugMode())
                {
                    AddDebugInputItem(expression, "Number To Format",expressionIterator.FetchEntry(),executionId);
                    AddDebugInputItem(roundingDecimalPlaces, "Rounding Decimal Places", roundingDecimalPlacesIterator.FetchEntry(), executionId);

                    AddDebugInputItem(decimalPlacesToShow, "Decimals To Show", decimalPlacesToShowIterator.FetchEntry(), executionId);
                }
                int iterationCounter = 0;
                // Loop data ;)
                while (colItr.HasMoreData())
                {
                    int roundingDecimalPlacesValue;
                    colItr.FetchNextRow(roundingDecimalPlacesIterator).TheValue.IsWholeNumber(out roundingDecimalPlacesValue);

                    int decimalPlacesToShowValue;
                    bool adjustDecimalPlaces = colItr.FetchNextRow(decimalPlacesToShowIterator).TheValue.IsWholeNumber(out decimalPlacesToShowValue);

                    var binaryDataListItem = colItr.FetchNextRow(expressionIterator);
                    var val = binaryDataListItem.TheValue;
                    FormatNumberTO formatNumberTo = new FormatNumberTO(val, RoundingType, roundingDecimalPlacesValue, adjustDecimalPlaces, decimalPlacesToShowValue);
                    string result = _numberFormatter.Format(formatNumberTo);

                    //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
                    foreach(var region in DataListCleaningUtils.SplitIntoRegions(Result))
                    {
                        toUpsert.Add(region, result);
                        toUpsert.FlushIterationFrame();
                    }
                }
                compiler.Upsert(executionId, toUpsert, out errors);
                foreach(var region in DataListCleaningUtils.SplitIntoRegions(Result))
                {
                    if(dataObject.IsDebugMode())
                    {
                        AddDebugOutputItem(region, string.Empty, executionId, iterationCounter);
                    }
                    iterationCounter++;
                }
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
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }

                #endregion

                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(context,StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        #endregion

        #region Private Methods

        private void AddDebugInputItem(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            DebugItem itemToAdd = new DebugItem();

            if (labelText == "Rounding Decimal Places")
            {
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Rounding Type" });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = RoundingType });                
            }

            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = labelText });

            if (valueEntry != null)
            {
                itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input));
            }

            _debugInputs.Add(itemToAdd);            
        }

        private void AddDebugOutputItem(string expression, string value, Guid dlId, int iterationCounter)
        {
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.AddRange(CreateDebugItemsFromString(expression, string.Empty, dlId, iterationCounter, enDev2ArgumentType.Output));
            _debugOutputs.Add(itemToAdd);
        }

        #endregion

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion

        #region Update ForEach Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
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
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates != null && updates.Count == 1)
            {
                Result = updates[0].Item2;
            }
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(Expression, RoundingType, RoundingDecimalPlaces, DecimalPlacesToShow);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion

    }
}
