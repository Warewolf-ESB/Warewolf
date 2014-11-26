
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
using Dev2.Data.Util;
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
// ReSharper restore CheckNamespace
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
            InitializeDebug(dataObject);
            // Process if no errors
            try
            {

                IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);

                toUpsert.IsDebug = dataObject.IsDebugMode();
                IDev2IteratorCollection colItr = Dev2ValueObjectFactory.CreateIteratorCollection();
                var datalist = compiler.ConvertFrom(dataObject.DataListID, DataListFormat.CreateFormat(GlobalConstants._Studio_XML), enTranslationDepth.Shape, out errors).ToString();
                if(!string.IsNullOrEmpty(datalist))
                {
                    ValidateInput(datalist, allErrors, Input1);
                    ValidateInput(datalist, allErrors, Input2);
                }
                IBinaryDataListEntry input1Entry = compiler.Evaluate(executionId, enActionType.User, string.IsNullOrEmpty(Input1) ? GlobalConstants.CalcExpressionNow : Input1, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator input1Itr = Dev2ValueObjectFactory.CreateEvaluateIterator(input1Entry);
                colItr.AddIterator(input1Itr);

                IBinaryDataListEntry input2Entry = compiler.Evaluate(executionId, enActionType.User, string.IsNullOrEmpty(Input2) ? GlobalConstants.CalcExpressionNow : Input2, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator input2Itr = Dev2ValueObjectFactory.CreateEvaluateIterator(input2Entry);
                colItr.AddIterator(input2Itr);

                IBinaryDataListEntry inputFormatEntry = compiler.Evaluate(executionId, enActionType.User, InputFormat ?? string.Empty, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator ifItr = Dev2ValueObjectFactory.CreateEvaluateIterator(inputFormatEntry);
                colItr.AddIterator(ifItr);

                if(dataObject.IsDebugMode())
                {
                    AddDebugInputItem(string.IsNullOrEmpty(Input1) ? GlobalConstants.CalcExpressionNow : Input1, "Input 1", input1Entry, executionId);
                    AddDebugInputItem(string.IsNullOrEmpty(Input2) ? GlobalConstants.CalcExpressionNow : Input2, "Input 2", input2Entry, executionId);
                    AddDebugInputItem(InputFormat, "Input Format", inputFormatEntry, executionId);
                    AddDebugInputItem(new DebugItemStaticDataParams(OutputType, "Output In"));
                }

                int indexToUpsertTo = 1;

                while(colItr.HasMoreData())
                {
                    IDateTimeDiffTO transObj = ConvertToDateTimeDiffTo(colItr.FetchNextRow(input1Itr).TheValue,
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
                        var rule = new IsSingleValueRule(() => Result);
                        var single = rule.Check();
                        if(single != null)
                        {
                            allErrors.AddError(single.Message);
                        }
                        else
                        {

                            toUpsert.Add(expression, result);

                        }
                    }
                    else
                    {
                        DoDebugOutput(dataObject, Result, result, executionId, indexToUpsertTo);
                        allErrors.AddError(error);
                    }
                    indexToUpsertTo++;
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
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error("DSFDateTime", e);
                allErrors.AddError(e.Message);
            }
            finally
            {

                // Handle Errors
                if(allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfDateTimeDifferenceActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                    compiler.Upsert(executionId, Result, (string)null, out errors);
                }
                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        void ValidateInput(string datalist, ErrorResultTO allErrors, string input)
        {

            var splitIntoRegions = DataListCleaningUtils.FindAllLanguagePieces(input);
            foreach(var region in splitIntoRegions)
            {
                string region1 = region;
                var isValidExpr = new IsValidExpressionRule(() => region1, datalist)
                {
                    LabelText = "Input1"
                };
                var errValid = isValidExpr.Check();
                if(errValid != null)
                {
                    var validationError = new ErrorResultTO();
                    validationError.AddError(errValid.Message);
                    allErrors.MergeErrors(validationError);
                }
            }

        }

        void DoDebugOutput(IDSFDataObject dataObject, string region, string result, Guid executionId, int indexToUpsertTo)
        {
            if(dataObject.IsDebugMode())
            {
                AddDebugOutputItem(new DebugOutputParams(region, result, executionId, indexToUpsertTo - 1));
            }
        }

        #region Private Methods

        private void AddDebugInputItem(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            AddDebugInputItem(new DebugItemVariableParams(expression, labelText, valueEntry, executionId));
        }

        /// <summary>
        /// Used for converting the properties of this activity to a DateTimeTO object
        /// </summary>
        /// <param name="input1">The input1.</param>
        /// <param name="input2">The input2.</param>
        /// <param name="evaledInputFormat">The evaled input format.</param>
        /// <param name="outputType">Type of the output.</param>
        /// <returns></returns>
        private static IDateTimeDiffTO ConvertToDateTimeDiffTo(string input1, string input2, string evaledInputFormat, string outputType)
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
            return GetForEachItems(Input1, Input2, InputFormat);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion

    }
}
