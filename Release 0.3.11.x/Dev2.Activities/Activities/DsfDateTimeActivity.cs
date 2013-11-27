using System.Globalization;
using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Converters.DateAndTime;
using Dev2.Converters.DateAndTime.Interfaces;
using Dev2.Data.Factories;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Util;
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
        [FindMissing]
        public string DateTime { get; set; }

        /// <summary>
        /// The property that holds the input format string the user enters into the "Input Format" box
        /// </summary>
        [Inputs("InputFormat")]
        [FindMissing]
        public string InputFormat { get; set; }

        /// <summary>
        /// The property that holds the output format string the user enters into the "Output Format" box
        /// </summary>
        [Inputs("OutputFormat")]
        [FindMissing]
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
        [FindMissing]
        public string TimeModifierAmountDisplay { get; set; }

        /// <summary>
        /// The property that holds the time modifier amount int that is passed to the TO
        /// </summary>
        public int TimeModifierAmount { get; set; }

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
        public DsfDateTimeActivity()
            : base("Date and Time")
        {
            DateTime = string.Empty;
            InputFormat = string.Empty;
            OutputFormat = string.Empty;
            //2012.09.27: massimo.guerrera - Added for the new functionality for the time modification
            TimeModifierType = "";
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
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors;
            Guid executionId = DataListExecutionID.Get(context);
            string error;

            // Process if no errors
            try
            {

                IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                IDev2IteratorCollection colItr = Dev2ValueObjectFactory.CreateIteratorCollection();

                IDev2DataListEvaluateIterator dtItr = CreateDataListEvaluateIterator(DateTime, executionId, compiler, colItr, allErrors);
                colItr.AddIterator(dtItr);
                IDev2DataListEvaluateIterator ifItr = CreateDataListEvaluateIterator(InputFormat, executionId, compiler, colItr, allErrors);
                colItr.AddIterator(ifItr);
                IDev2DataListEvaluateIterator ofItr = CreateDataListEvaluateIterator(OutputFormat, executionId, compiler, colItr, allErrors);
                colItr.AddIterator(ofItr);
                IDev2DataListEvaluateIterator tmaItr = CreateDataListEvaluateIterator(TimeModifierAmountDisplay, executionId, compiler, colItr, allErrors);
                colItr.AddIterator(tmaItr);

                if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {
                    AddDebugInputItem(DateTime, "Start Date", dtItr.FetchEntry(), executionId);
                    AddDebugInputItem(InputFormat, "Input Format", ifItr.FetchEntry(), executionId);
                    AddDebugInputItem(TimeModifierAmountDisplay, "Add", tmaItr.FetchEntry(), executionId);
                    AddDebugInputItem(OutputFormat, "Output Format", ofItr.FetchEntry(), executionId);
                }

                // Loop data ;)

                int indexToUpsertTO = 1;
                string expression;

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
                        //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
                        foreach (var region in DataListCleaningUtils.SplitIntoRegions(expression))
                        {
                            toUpsert.Add(region, result);
                            if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                            {
                                AddDebugOutputItem(region, result, executionId, indexToUpsertTO - 1);
                            }
                        }
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
                    DisplayAndWriteError("DsfDateTimeActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if (dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
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

            if (valueEntry != null)
            {
                itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input));
            }

            if (labelText == "Add")
            {
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = TimeModifierType });
            }

            _debugInputs.Add(itemToAdd);
        }

        private void AddDebugOutputItem(string expression, string value, Guid dlId, int indexToUpsertTo)
        {
            DebugItem itemToAdd = new DebugItem();

            itemToAdd.AddRange(CreateDebugItemsFromString(expression, value, dlId, indexToUpsertTo, enDev2ArgumentType.Output));
            _debugOutputs.Add(itemToAdd);
        }

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

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(DateTime, InputFormat, TimeModifierAmountDisplay, OutputFormat);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion
    }
}
