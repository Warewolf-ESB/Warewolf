using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Converters.DateAndTime;
using Dev2.Converters.DateAndTime.Interfaces;
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

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfDateTimeActivity : DsfActivityAbstract<string>, IDateTimeOperationTO
    {

        #region Properties

        /// <summary>
        /// The property that holds the date time string the user enters into the "Input" box
        /// </summary>
        [Inputs("DateTime")]
        public string DateTime { get; set; }

        /// <summary>
        /// The property that holds the input format string the user enters into the "Input Format" box
        /// </summary>
        [Inputs("InputFormat")]
        public string InputFormat { get; set; }

        /// <summary>
        /// The property that holds the output format string the user enters into the "Output Format" box
        /// </summary>
        [Inputs("OutputFormat")]
        public string OutputFormat { get; set; }

        /// <summary>
        /// The property that holds the time modifier string the user selects in the "Add Time" combobox
        /// </summary>
        [Inputs("TimeModifierType")]
        public string TimeModifierType { get; set; }

        /// <summary>
        /// The property that holds the time modifier string the user enters into the "Amount" box
        /// </summary>
        [Inputs("TimeModifierAmountDisplay")]
        public string TimeModifierAmountDisplay { get; set; }

        /// <summary>
        /// The property that holds the time modifier amount int that is passed to the TO
        /// </summary>
        public int TimeModifierAmount { get; set; }

        /// <summary>
        /// The property that holds the result string the user enters into the "Result" box
        /// </summary>
        [Outputs("Result")]
        public new string Result { get; set; }

        #endregion Properties

        #region Ctor

        /// <summary>
        /// The consructor for the activity 
        /// </summary>
        public DsfDateTimeActivity()
            : base("Date and Time")
        {
            DateTime = string.Empty;
            InputFormat = string.Empty;
            OutputFormat = string.Empty;
            //2012.09.27: massimo.guerrera - Added for the new functionality for the time modification
            TimeModifierType = string.Empty;
            TimeModifierAmountDisplay = string.Empty;
            TimeModifierAmount = 0;
            Result = string.Empty;
        }

        #endregion Ctor

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        /// <summary>
        /// The execute method that is called when the activity is executed at run time and will hold all the logic of the activity
        /// </summary>       
        protected override void OnExecute(NativeActivityContext context)
        {

            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = DataListExecutionID.Get(context);
            string error = string.Empty;

            // Process if no errors
            try
            {

                IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                IDev2IteratorCollection colItr = Dev2ValueObjectFactory.CreateIteratorCollection();

                //Check for a star in the variables and execute for that many times
                //IList<string> expressionList = new List<string>() { DateTime, InputFormat, OutputFormat, TimeModifierAmountDisplay };
                //int numOfEx = compiler.GetMaxNumberOfExecutions(executionId, expressionList);

                //Evaluate the properties using the DataListCompiler.Evaluate method
                IDev2DataListEvaluateIterator dtItr = CreateDataListEvaluateIterator(DateTime, executionId, compiler, colItr, allErrors);
                colItr.AddIterator(dtItr);
                IDev2DataListEvaluateIterator ifItr = CreateDataListEvaluateIterator(InputFormat, executionId, compiler, colItr, allErrors);
                colItr.AddIterator(ifItr);
                IDev2DataListEvaluateIterator ofItr = CreateDataListEvaluateIterator(OutputFormat, executionId, compiler, colItr, allErrors);
                colItr.AddIterator(ofItr);
                IDev2DataListEvaluateIterator tmaItr = CreateDataListEvaluateIterator(TimeModifierAmountDisplay, executionId, compiler, colItr, allErrors);
                colItr.AddIterator(tmaItr);

                // Loop data ;)

                int indexToUpsertTO = 1;
                string expression = string.Empty;

                while (colItr.HasMoreData())
                {
                    IDateTimeOperationTO transObj = ConvertToDateTimeTO(colItr.FetchNextRow(dtItr).TheValue,
                                                                            colItr.FetchNextRow(ifItr).TheValue,
                                                                            colItr.FetchNextRow(ofItr).TheValue,
                                                                            TimeModifierType,
                                                                            colItr.FetchNextRow(tmaItr).TheValue
                                                                            );

                    //Create a DateTimeFomatter using the DateTimeConverterFactory.DONE
                    IDateTimeFormatter format = DateTimeConverterFactory.CreateFormatter();
                    string result;
                    if (format.TryFormat(transObj, out result, out error))
                    {
                        if (DataListUtil.IsValueRecordset(Result) &&
                            DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
                        {
                            expression = Result.Replace(GlobalConstants.StarExpression, indexToUpsertTO.ToString());
                        }
                        else
                        {
                            expression = Result;
                        }
                        toUpsert.Add(expression, result);
                        toUpsert.FlushIterationFrame();
                    }
                    else
                    {
                        allErrors.AddError(error);
                    }
                    indexToUpsertTO++;
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

                // Handle Errors
                if (allErrors.HasErrors())
                {
                    string err = DisplayAndWriteError("DsfDateTimeActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, err, out errors);
                }
            }
        }

        #region Private Methods

        /// <summary>
        /// Used for converting the properties of this activity to a DateTimeTO object
        /// </summary>
        private IDateTimeOperationTO ConvertToDateTimeTO(string evaledDateTime, string evaledInputFormat, string evaledOutputFormat, string TimeModifierType, string TimeModifierAmount)
        {
            //2012.09.27: massimo.guerrera - Added for the new functionality for the time modification
            //Create a DateTimeTO using the DateTimeConverterFactory and send through the properties of this activity.DONE
            int tmpTimeAmount = 0;
            if (!string.IsNullOrWhiteSpace(TimeModifierAmount))
            {
                if (!int.TryParse(TimeModifierAmount, out tmpTimeAmount))
                {
                    throw new Exception("Add Time amount must be numeric");
                }
            }
            return DateTimeConverterFactory.CreateDateTimeTO(evaledDateTime, evaledInputFormat, evaledOutputFormat, TimeModifierType, tmpTimeAmount, Result);
        }

        #endregion Private Methods

        #region Get Debug Inputs/Outputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> results = new List<IDebugItem>();

            if (!string.IsNullOrEmpty(DateTime))
            {
                foreach (IDebugItem debugItem in CreateDebugItems(DateTime, dataList))
                {
                    debugItem.Label = "Start Date " + debugItem.Label;
                    results.Add(debugItem);
                }
            }

            if (!string.IsNullOrEmpty(InputFormat))
            {
                foreach (IDebugItem debugItem in CreateDebugItems(InputFormat, dataList))
                {
                    debugItem.Label = "Input Format " + debugItem.Label;
                    results.Add(debugItem);
                }
            }

            if (!string.IsNullOrEmpty(TimeModifierAmountDisplay))
            {
                foreach (IDebugItem debugItem in CreateDebugItems(TimeModifierAmountDisplay, dataList))
                {
                    debugItem.Label = "Add Time " + debugItem.Label;
                    debugItem.Results[0].Value = debugItem.Results[0].Value + " " + TimeModifierType;
                    results.Add(debugItem);
                }
            }

            if (!string.IsNullOrEmpty(OutputFormat))
            {
                foreach (IDebugItem debugItem in CreateDebugItems(OutputFormat, dataList))
                {
                    debugItem.Label = "Output Format " + debugItem.Label;
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

        #endregion Get Inputs/Outputs

        #region Get ForEach Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach (Tuple<string, string> t in updates)
            {

                if (t.Item1 == DateTime)
                {
                    DateTime = t.Item2;
                }

                if (t.Item1 == InputFormat)
                {
                    InputFormat = t.Item2;
                }

                if (t.Item1 == TimeModifierAmountDisplay)
                {
                    TimeModifierAmountDisplay = t.Item2;
                }

                if (t.Item1 == OutputFormat)
                {
                    OutputFormat = t.Item2;
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
            return GetForEachItems(context, StateType.Before, DateTime, InputFormat, TimeModifierAmountDisplay, OutputFormat);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.After, Result);
        }

        #endregion
    }
}
