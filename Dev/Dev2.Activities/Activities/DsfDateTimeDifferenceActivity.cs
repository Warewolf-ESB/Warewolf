using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Converters.DateAndTime;
using Dev2.Converters.DateAndTime.Interfaces;
using Dev2.Data.Factories;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Util;
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
        [FindMissing]
        public string Input1 { get; set; }

        /// <summary>
        /// The property that holds the input format string the user enters into the "Input2" box
        /// </summary>
        [Inputs("Input2")]
        [FindMissing]
        public string Input2 { get; set; }

        /// <summary>
        /// The property that holds the output format string the user enters into the "Input Format" box
        /// </summary>
        [Inputs("InputFormat")]
        [FindMissing]
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
        [FindMissing]
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

        // ReSharper disable RedundantOverridenMember
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }
        // ReSharper restore RedundantOverridenMember

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

            // Process if no errors
            try
            {

                IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                IDev2IteratorCollection colItr = Dev2ValueObjectFactory.CreateIteratorCollection();

                IBinaryDataListEntry Input1Entry = compiler.Evaluate(executionId, enActionType.User, string.IsNullOrEmpty(Input1) ? GlobalConstants.CalcExpressionNow : Input1, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator input1Itr = Dev2ValueObjectFactory.CreateEvaluateIterator(Input1Entry);
                colItr.AddIterator(input1Itr);

                IBinaryDataListEntry Input2Entry = compiler.Evaluate(executionId, enActionType.User, string.IsNullOrEmpty(Input2) ? GlobalConstants.CalcExpressionNow : Input2, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator input2Itr = Dev2ValueObjectFactory.CreateEvaluateIterator(Input2Entry);
                colItr.AddIterator(input2Itr);

                IBinaryDataListEntry InputFormatEntry = compiler.Evaluate(executionId, enActionType.User, InputFormat, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator ifItr = Dev2ValueObjectFactory.CreateEvaluateIterator(InputFormatEntry);
                colItr.AddIterator(ifItr);

                if(dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {
                    AddDebugInputItem(string.IsNullOrEmpty(Input1) ? GlobalConstants.CalcExpressionNow : Input1, "Start Date", Input1Entry, executionId);
                    AddDebugInputItem(string.IsNullOrEmpty(Input2) ? GlobalConstants.CalcExpressionNow : Input2, "End Date", Input2Entry, executionId);
                    AddDebugInputItem(InputFormat, "Input Format", InputFormatEntry, executionId);
                }

                int indexToUpsertTo = 1;

                while(colItr.HasMoreData())
                {
                    IDateTimeDiffTO transObj = ConvertToDateTimeDiffTO(colItr.FetchNextRow(input1Itr).TheValue,
                                                                       colItr.FetchNextRow(input2Itr).TheValue,
                                                                       colItr.FetchNextRow(ifItr).TheValue,
                                                                       OutputType);
                    //Create a DateTimeComparer using the DateTimeConverterFactory
                    IDateTimeComparer comparer = DateTimeConverterFactory.CreateComparer();
                    //Call the TryComparer method on the DateTimeComparer and pass it the IDateTimeDiffTO created from the ConvertToDateTimeDiffTO Method                
                    string result;
                    string error;
                    if(comparer.TryCompare(transObj, out result, out error))
                    {
                        string expression;
                        if(DataListUtil.IsValueRecordset(Result) &&
                            DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
                        {
                            expression = Result.Replace(GlobalConstants.StarExpression, indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            expression = Result;
                        }

                        //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
                        foreach(var region in DataListCleaningUtils.SplitIntoRegions(expression))
                        {
                            toUpsert.Add(region, result);

                            if(dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID))
                            {
                                AddDebugOutputItem(region, result, indexToUpsertTo - 1, executionId);
                            }
                        }
                    }
                    else
                    {
                        allErrors.AddError(error);
                    }
                    indexToUpsertTo++;
                }

                compiler.Upsert(executionId, toUpsert, out errors);
                allErrors.MergeErrors(errors);
            }
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {

                // Handle Errors
                if(allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfDateTimeDifferenceActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if(dataObject.IsDebug || dataObject.RemoteInvoke)
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        #region Private Methods

        private void AddDebugInputItem(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            DebugItem itemToAdd = new DebugItem();

            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = labelText });

            if(valueEntry != null)
            {
                itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input));
            }

            _debugInputs.Add(itemToAdd);

            if(labelText == "Input Format")
            {
                itemToAdd = new DebugItem();
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Output In" });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = OutputType });
                _debugInputs.Add(itemToAdd);
            }
        }

        private void AddDebugOutputItem(string expression, string value, int iterationCounter, Guid dlId)
        {
            DebugItem itemToAdd = new DebugItem();

            itemToAdd.AddRange(CreateDebugItemsFromString(expression, value, dlId, iterationCounter, enDev2ArgumentType.Output));
            _debugOutputs.Add(itemToAdd);
        }

        /// <summary>
        /// Used for converting the properties of this activity to a DateTimeTO object
        /// </summary>
        private IDateTimeDiffTO ConvertToDateTimeDiffTO(string input1, string input2, string evaledInputFormat, string outputType)
        {
            return DateTimeConverterFactory.CreateDateTimeDiffTO(input1, input2, evaledInputFormat, outputType);
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

        #region Get ForEach Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach(Tuple<string, string> t in updates)
            {

                if(t.Item1 == Input1)
                {
                    Input1 = t.Item2;
                }

                if(t.Item1 == Input2)
                {
                    Input2 = t.Item2;
                }

                if(t.Item1 == InputFormat)
                {
                    InputFormat = t.Item2;
                }

            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates.Count == 1)
            {
                Result = updates[0].Item2;
            }
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(Input1, Input2, InputFormat);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion

    }
}
