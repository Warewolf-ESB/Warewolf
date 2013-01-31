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
    public class DsfDateTimeDifferenceActivity : DsfActivityAbstract<string>, IDateTimeDiffTO
    {

        #region Properties

        /// <summary>
        /// The property that holds the date time string the user enters into the "Input1" box
        /// </summary>
        [Inputs("Input1")]
        public string Input1 { get; set; }

        /// <summary>
        /// The property that holds the input format string the user enters into the "Input2" box
        /// </summary>
        [Inputs("Input2")]
        public string Input2 { get; set; }

        /// <summary>
        /// The property that holds the output format string the user enters into the "Input Format" box
        /// </summary>
        [Inputs("InputFormat")]
        public string InputFormat { get; set; }

        /// <summary>
        /// The property that holds the time modifier string the user selects in the "Output In" combobox
        /// </summary>
        [Inputs("OutputType")]
        public string OutputType { get; set; }

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
        public DsfDateTimeDifferenceActivity()
            : base("Date and Time Difference")
        {
            Input1 = string.Empty;
            Input2 = string.Empty;
            InputFormat = string.Empty;
            OutputType = "Years";
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

            IList<OutputTO> outputs = new List<OutputTO>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();

            Guid dlID = dataObject.DataListID;
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = compiler.Shape(dlID, enDev2ArgumentType.Input, InputMapping, out errors);
            allErrors.MergeErrors(errors);
            string error = string.Empty;

            // Process if no errors
            try
            {

                IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                IDev2IteratorCollection colItr = Dev2ValueObjectFactory.CreateIteratorCollection();

                IBinaryDataListEntry Input1Entry = compiler.Evaluate(executionId, enActionType.User, Input1, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator input1Itr = Dev2ValueObjectFactory.CreateEvaluateIterator(Input1Entry);
                colItr.AddIterator(input1Itr);

                IBinaryDataListEntry Input2Entry = compiler.Evaluate(executionId, enActionType.User, Input2, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator input2Itr = Dev2ValueObjectFactory.CreateEvaluateIterator(Input2Entry);
                colItr.AddIterator(input2Itr);

                IBinaryDataListEntry InputFormatEntry = compiler.Evaluate(executionId, enActionType.User, InputFormat, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator ifItr = Dev2ValueObjectFactory.CreateEvaluateIterator(InputFormatEntry);
                colItr.AddIterator(ifItr);

                int indexToUpsertTo = 1;
                string expression = string.Empty;

                while (colItr.HasMoreData())
                {
                    IDateTimeDiffTO transObj = ConvertToDateTimeDiffTO(colItr.FetchNextRow(input1Itr).TheValue,
                                                                       colItr.FetchNextRow(input2Itr).TheValue,
                                                                       colItr.FetchNextRow(ifItr).TheValue,
                                                                       OutputType);
                    //Create a DateTimeComparer using the DateTimeConverterFactory
                    IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
                    //Call the TryComparer method on the DateTimeComparer and pass it the IDateTimeDiffTO created from the ConvertToDateTimeDiffTO Method                
                    string result;
                    if (comparer.TryCompare(transObj, out result, out error))
                    {
                        if (DataListUtil.IsValueRecordset(Result) &&
                            DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
                        {
                            expression = Result.Replace(GlobalConstants.StarExpression, indexToUpsertTo.ToString());
                        }
                        else
                        {
                            expression = Result;
                        }

                        toUpsert.Add(expression, result);
                    }
                    else
                    {
                        allErrors.AddError(error);
                    }
                    indexToUpsertTo++;
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
                    string err = DisplayAndWriteError("DsfDateTimeDifferenceActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, err, out errors);
                }
            }
        }

        #region Private Methods

        /// <summary>
        /// Used for converting the properties of this activity to a DateTimeTO object
        /// </summary>
        private IDateTimeDiffTO ConvertToDateTimeDiffTO(string input1, string input2, string evaledInputFormat, string outputType)
        {
            return DateTimeConverterFactory.CreateDateTimeDiffTO(input1, input2, evaledInputFormat, outputType);
        }

        #endregion Private Methods


        #region Get Debug Inputs/Outputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> results = new List<IDebugItem>();

            if (!string.IsNullOrEmpty(Input1))
            {
                foreach (IDebugItem debugItem in CreateDebugItems(Input1, dataList))
                {
                    // BUG 8104 : Refactor DebugItem
                    //debugItem.Label = debugItem.Label + " Start Date ";
                    results.Add(debugItem);
                }
            }

            if (!string.IsNullOrEmpty(Input2))
            {
                foreach (IDebugItem debugItem in CreateDebugItems(Input2, dataList))
                {
                    // BUG 8104 : Refactor DebugItem
                    //debugItem.Label = debugItem.Label + " End Date ";
                    results.Add(debugItem);
                }
            }

            if (!string.IsNullOrEmpty(InputFormat))
            {
                foreach (IDebugItem debugItem in CreateDebugItems(InputFormat, dataList))
                {
                    // BUG 8104 : Refactor DebugItem
                    //debugItem.Label = debugItem.Label + " Input Format ";
                    results.Add(debugItem);
                }
            }

            if (!string.IsNullOrEmpty(OutputType))
            {
                results.Add(new DebugItem(" Output Type ", OutputType, null));
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

                if (t.Item1 == Input1)
                {
                    Input1 = t.Item2;
                }

                if (t.Item1 == Input2)
                {
                    Input2 = t.Item2;
                }

                if (t.Item1 == InputFormat)
                {
                    InputFormat = t.Item2;
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
            return GetForEachItems(context, StateType.Before, Input1, Input2, InputFormat);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.After, Result);
        }

        #endregion

    }
}
