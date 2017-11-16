using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.DateAndTime;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Validation;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities.DateAndTime
{
    [ToolDescriptorInfo("Utility-DateTimeDifference", "Date Time Diff", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Utility_Date_Time_Diff")]
    public class DsfNewDateTimeDifferenceActivity : DsfActivityAbstract<string>, IDateTimeDiffTO
    {
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

        /// <summary>
        /// The consructor for the activity 
        /// </summary>
        public DsfNewDateTimeDifferenceActivity()
            : base("Date and Time Difference")
        {
            Input1 = string.Empty;
            Input2 = string.Empty;
            InputFormat = string.Empty;
            OutputType = "Years";
            Result = string.Empty;
        }

        public override List<string> GetOutputs()
        {
            return new List<string> { Result };
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            var allErrors = new ErrorResultTO();
            var errors = new ErrorResultTO();
            allErrors.MergeErrors(errors);
            InitializeDebug(dataObject);
            try
            {
                AddDebugInputs(dataObject, update);
                var colItr = new WarewolfListIterator();
                var nowDate=DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat, CultureInfo.InvariantCulture);
                
                var input1Itr = new WarewolfIterator(dataObject.Environment.EvalStrict(string.IsNullOrEmpty(Input1) ? nowDate : Input1, update));
                colItr.AddVariableToIterateOn(input1Itr);

                var evalInp2 = dataObject.Environment.EvalStrict(string.IsNullOrEmpty(Input2) ? nowDate : Input2, update);

                var input2Itr = new WarewolfIterator(evalInp2);
                colItr.AddVariableToIterateOn(input2Itr);

                var ifItr = new WarewolfIterator(dataObject.Environment.Eval(InputFormat ?? string.Empty, update));
                colItr.AddVariableToIterateOn(ifItr);
                var indexToUpsertTo = 1;
                while (colItr.HasMoreData())
                {
                    var transObj = ConvertToDateTimeDiffTo(colItr.FetchNextValue(input1Itr),
                        colItr.FetchNextValue(input2Itr),
                        colItr.FetchNextValue(ifItr),
                        OutputType);
                    var comparer = DateTimeConverterFactory.CreateStandardComparer();
                    var expression = Result;

                    if (comparer.TryCompare(transObj, out string result, out string error))
                    {
                        expression = AssignResults(dataObject, update, allErrors, indexToUpsertTo, expression, result);
                    }
                    else
                    {
                        DoDebugOutput(dataObject, expression, update);
                        allErrors.AddError(error);
                    }
                    indexToUpsertTo++;
                }

                allErrors.MergeErrors(errors);
                if (dataObject.IsDebugMode() && !allErrors.HasErrors())
                {
                    AddDebugOutputItem(new DebugEvalResult(Result, null, dataObject.Environment, update));
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("DSFDateTime", e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }
            finally
            {
                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfDateTimeDifferenceActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                    dataObject.Environment.Assign(Result, null, update);
                }
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        string AssignResults(IDSFDataObject dataObject, int update, ErrorResultTO allErrors, int indexToUpsertTo, string expr, string result)
        {
            var expression = expr;
            if (DataListUtil.IsValueRecordset(Result) && DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
            {
                if (update == 0)
                {
                    expression = Result.Replace(GlobalConstants.StarExpression, indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
                }
            }
            else
            {
                expression = Result;
            }

            var rule = new IsSingleValueRule(() => Result);
            var single = rule.Check();
            if (single != null)
            {
                allErrors.AddError(single.Message);
            }
            else
            {
                dataObject.Environment.Assign(expression, result, update);
            }

            return expression;
        }

        void AddDebugInputs(IDSFDataObject dataObject, int update)
        {
            if (dataObject.IsDebugMode())
            {
                if (string.IsNullOrEmpty(Input1))
                {
                    AddDebugInputItem(new DebugItemStaticDataParams(DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), "now()", "Input 1", "="));
                }
                else
                {
                    AddDebugInputItem(Input1, "Input 1", dataObject.Environment, update);
                }

                if (string.IsNullOrEmpty(Input2))
                {
                    AddDebugInputItem(new DebugItemStaticDataParams(DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat), "now()", "Input 2", "="));
                }
                else
                {
                    AddDebugInputItem(Input2, "Input 2", dataObject.Environment, update);
                }

                AddDebugInputItem(InputFormat, "Input Format", dataObject.Environment, update);
                if (!String.IsNullOrEmpty(OutputType))
                {
                    AddDebugInputItem(new DebugItemStaticDataParams(OutputType, "Output In"));
                }
            }
        }

        void DoDebugOutput(IDSFDataObject dataObject, string region, int update)
        {
            if (dataObject.IsDebugMode())
            {
                AddDebugOutputItem(new DebugEvalResult(region, "", dataObject.Environment, update));
            }
        }

        void AddDebugInputItem(string expression, string labelText, IExecutionEnvironment environment, int update)
        {
            AddDebugInputItem(new DebugEvalResult(expression, labelText, environment, update));
        }

        /// <summary>
        /// Used for converting the properties of this activity to a DateTimeTO object
        /// </summary>
        /// <param name="input1">The input1.</param>
        /// <param name="input2">The input2.</param>
        /// <param name="evaledInputFormat">The evaled input format.</param>
        /// <param name="outputType">Type of the output.</param>
        /// <returns></returns>
        static IDateTimeDiffTO ConvertToDateTimeDiffTo(string input1, string input2, string evaledInputFormat, string outputType)
        {
            return DateTimeConverterFactory.CreateDateTimeDiffTO(input1, input2, evaledInputFormat, outputType);
        }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
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

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == Result);
            if (itemUpdate != null)
            {
                Result = itemUpdate.Item2;
            }
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(Input1, Input2, InputFormat);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }
    }
}
