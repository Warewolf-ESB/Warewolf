
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Converters.DateAndTime;
using Dev2.Data.Factories;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Util;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

// ReSharper disable CheckNamespace
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
            InitializeDebug(dataObject);
            // Process if no errors
            try
            {
                IsSingleValueRule.ApplyIsSingleValueRule(Result, allErrors);
                IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
                toUpsert.IsDebug = (dataObject.IsDebugMode());
                toUpsert.ResourceID = dataObject.ResourceID;
                IDev2IteratorCollection colItr = Dev2ValueObjectFactory.CreateIteratorCollection();

                IDev2DataListEvaluateIterator dtItr = CreateDataListEvaluateIterator(string.IsNullOrEmpty(DateTime) ? GlobalConstants.CalcExpressionNow : DateTime, executionId, compiler, colItr, allErrors);
                colItr.AddIterator(dtItr);
                IDev2DataListEvaluateIterator ifItr = CreateDataListEvaluateIterator(InputFormat, executionId, compiler, colItr, allErrors);
                colItr.AddIterator(ifItr);
                IDev2DataListEvaluateIterator ofItr = CreateDataListEvaluateIterator(OutputFormat, executionId, compiler, colItr, allErrors);
                colItr.AddIterator(ofItr);
                IDev2DataListEvaluateIterator tmaItr = CreateDataListEvaluateIterator(TimeModifierAmountDisplay, executionId, compiler, colItr, allErrors);
                colItr.AddIterator(tmaItr);

                if(dataObject.IsDebugMode())
                {
                    if(string.IsNullOrEmpty(DateTime))
                    {
                        var defaultDateTimeDebugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("System Date Time", "Input"), defaultDateTimeDebugItem);
                        AddDebugItem(new DebugItemStaticDataParams(System.DateTime.Now.ToString(CultureInfo.CurrentCulture), "="), defaultDateTimeDebugItem);
                        _debugInputs.Add(defaultDateTimeDebugItem);
                    }
                    else
                    {
                        AddDebugInputItem(new DebugItemVariableParams(DateTime, "Input", dtItr.FetchEntry(), executionId));
                    }

                    var dateTimePattern = string.Format("{0} {1}", CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern, CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern);

                    if(string.IsNullOrEmpty(InputFormat))
                    {
                        var defaultDateTimeDebugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("System Date Time Format", "Input Format"), defaultDateTimeDebugItem);
                        AddDebugItem(new DebugItemStaticDataParams(dateTimePattern, "="), defaultDateTimeDebugItem);
                        _debugInputs.Add(defaultDateTimeDebugItem);
                    }
                    else
                    {
                        AddDebugInputItem(new DebugItemVariableParams(InputFormat, "Input Format", ifItr.FetchEntry(), executionId));
                    }

                    var debugItem = new DebugItem();
                    AddDebugItem(new DebugItemStaticDataParams(TimeModifierType, "Add Time"), debugItem);
                    AddDebugItem(new DebugItemVariableParams(TimeModifierAmountDisplay, "", tmaItr.FetchEntry(), executionId, true), debugItem);
                    _debugInputs.Add(debugItem);

                    if(string.IsNullOrEmpty(OutputFormat))
                    {
                        var defaultDateTimeDebugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("System Date Time Format", "Output Format"), defaultDateTimeDebugItem);
                        AddDebugItem(new DebugItemStaticDataParams(dateTimePattern, "="), defaultDateTimeDebugItem);
                        _debugInputs.Add(defaultDateTimeDebugItem);
                    }
                    else
                    {
                        AddDebugInputItem(new DebugItemVariableParams(OutputFormat, "Output Format", ofItr.FetchEntry(), executionId));
                    }
                }

                if(!allErrors.HasErrors())
                {
                    while(colItr.HasMoreData())
                    {
                        IDateTimeOperationTO transObj = ConvertToDateTimeTo(colItr.FetchNextRow(dtItr).TheValue,
                                                                                colItr.FetchNextRow(ifItr).TheValue,
                                                                                colItr.FetchNextRow(ofItr).TheValue,
                                                                                TimeModifierType,
                                                                                colItr.FetchNextRow(tmaItr).TheValue
                                                                                );

                        //Create a DateTimeFomatter using the DateTimeConverterFactory.DONE
                        IDateTimeFormatter format = DateTimeConverterFactory.CreateFormatter();
                        string result;
                        string error;
                        if(format.TryFormat(transObj, out result, out error))
                        {
                            string expression = Result;
                            //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result

                            toUpsert.Add(expression, result);

                            toUpsert.FlushIterationFrame();
                        }
                        else
                        {
                            allErrors.AddError(error);
                        }
                    }
                    compiler.Upsert(executionId, toUpsert, out errors);
                    allErrors.MergeErrors(errors);
                    if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                    {
                        foreach(var debugOutputTo in toUpsert.DebugOutputs)
                        {
                            AddDebugOutputItem(new DebugItemVariableParams(debugOutputTo));
                        }
                    }
                    allErrors.MergeErrors(errors);
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error("DSFDateTime", e);
                allErrors.AddError(e.Message);
            }
            finally
            {

                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfDateTimeActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                    compiler.Upsert(executionId, Result, (string)null, out errors);
                }
                if(dataObject.IsDebugMode())
                {
                    if(hasErrors)
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", Result, ""));
                    }
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        #region Private Methods

        /// <summary>
        /// Used for converting the properties of this activity to a DateTimeTO object
        /// </summary>
        private IDateTimeOperationTO ConvertToDateTimeTo(string evaledDateTime, string evaledInputFormat, string evaledOutputFormat, string timeModifierType, string tTimeModifierAmount)
        {
            //2012.09.27: massimo.guerrera - Added for the new functionality for the time modification
            //Create a DateTimeTO using the DateTimeConverterFactory and send through the properties of this activity.DONE
            int tmpTimeAmount = 0;
            if(!string.IsNullOrWhiteSpace(tTimeModifierAmount))
            {
                if(!int.TryParse(tTimeModifierAmount, out tmpTimeAmount))
                {
                    throw new Exception("Add Time amount must be numeric");
                }
            }
            return DateTimeConverterFactory.CreateDateTimeTO(evaledDateTime, evaledInputFormat, evaledOutputFormat, timeModifierType, tmpTimeAmount, Result);
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

                if(t.Item1 == DateTime)
                {
                    DateTime = t.Item2;
                }

                if(t.Item1 == InputFormat)
                {
                    InputFormat = t.Item2;
                }

                if(t.Item1 == TimeModifierAmountDisplay)
                {
                    TimeModifierAmountDisplay = t.Item2;
                }

                if(t.Item1 == OutputFormat)
                {
                    OutputFormat = t.Item2;
                }
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
