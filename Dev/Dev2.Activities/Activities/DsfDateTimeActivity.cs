/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Reflection;
using System.Threading;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.DateAndTime;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
{

    [ToolDescriptorInfo("Utility-DateTime", "Date Time", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Utility_Date_Time")]
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
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }


        public override List<string> GetOutputs()
        {
            return new List<string> { Result };
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {


            ErrorResultTO allErrors = new ErrorResultTO();
            InitializeDebug(dataObject);
            // Process if no errors
            try
            {
                IsSingleValueRule.ApplyIsSingleValueRule(Result, allErrors);

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
                        AddDebugInputItem(new DebugEvalResult(DateTime, "Input", dataObject.Environment, update));
                    }
                    var cultureType = typeof(CultureInfo);
                    var fieldInfo = cultureType.GetField("s_userDefaultCulture",BindingFlags.NonPublic | BindingFlags.Static);
                    if (fieldInfo != null)
                    {
                        var val = fieldInfo.GetValue(CultureInfo.CurrentCulture);
                        var newCul = val as CultureInfo;
                        if (newCul != null)
                        {
                            Thread.CurrentThread.CurrentCulture = newCul;      
                        }
                    }
                    var dateTimePattern = string.Format("{0} {1}", Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern, Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongTimePattern);

                    if(string.IsNullOrEmpty(InputFormat))
                    {
                        var defaultDateTimeDebugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams("System Date Time Format", "Input Format"), defaultDateTimeDebugItem);
                        AddDebugItem(new DebugItemStaticDataParams(dateTimePattern, "="), defaultDateTimeDebugItem);
                        _debugInputs.Add(defaultDateTimeDebugItem);
                    }
                    else
                    {
                        AddDebugInputItem(new DebugEvalResult(InputFormat, "Input Format", dataObject.Environment, update));
                    }

                    var debugItem = new DebugItem();
                    AddDebugItem(new DebugItemStaticDataParams(TimeModifierType, "Add Time"), debugItem);
                    AddDebugItem(new DebugEvalResult(TimeModifierAmountDisplay, "", dataObject.Environment, update), debugItem);
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
                        AddDebugInputItem(new DebugEvalResult(OutputFormat, "Output Format", dataObject.Environment, update));
                    }
                }

                if(DataListUtil.HasNegativeIndex(InputFormat))
                {
                    allErrors.AddError(string.Format("Negative Recordset Index for Input Format: {0}", InputFormat));
                }
                if(DataListUtil.HasNegativeIndex(OutputFormat))
                {
                    allErrors.AddError(string.Format("Negative Recordset Index for Output Format: {0}", OutputFormat));
                }

                if(DataListUtil.HasNegativeIndex(TimeModifierAmountDisplay))
                {
                    allErrors.AddError(string.Format("Negative Recordset Index for Add Time: {0}", TimeModifierAmountDisplay));
                }
                if(!allErrors.HasErrors())
                {
                    var colItr = new WarewolfListIterator();

                    var dtItr = CreateDataListEvaluateIterator(string.IsNullOrEmpty(DateTime) ? GlobalConstants.CalcExpressionNow : DateTime, dataObject.Environment, update);
                    colItr.AddVariableToIterateOn(dtItr);
                    var ifItr = CreateDataListEvaluateIterator(InputFormat, dataObject.Environment, update);
                    colItr.AddVariableToIterateOn(ifItr);
                    var ofItr = CreateDataListEvaluateIterator(OutputFormat, dataObject.Environment, update);
                    colItr.AddVariableToIterateOn(ofItr);
                    var tmaItr = CreateDataListEvaluateIterator(TimeModifierAmountDisplay, dataObject.Environment, update);
                    colItr.AddVariableToIterateOn(tmaItr);

                    if(!allErrors.HasErrors())
                    {
                        while(colItr.HasMoreData())
                        {
                            IDateTimeOperationTO transObj = ConvertToDateTimeTo(colItr.FetchNextValue(dtItr),
                                colItr.FetchNextValue(ifItr),
                                colItr.FetchNextValue(ofItr),
                                TimeModifierType,
                                colItr.FetchNextValue(tmaItr)
                                );

                            IDateTimeFormatter format = DateTimeConverterFactory.CreateFormatter();
                            string result;
                            string error;
                            if(format.TryFormat(transObj, out result, out error))
                            {
                                string expression = Result;
                                dataObject.Environment.Assign(expression, result, update);
                            }
                            else
                            {
                                allErrors.AddError(error);
                            }
                        }
                        if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                        {
                            AddDebugOutputItem(new DebugEvalResult(Result, "", dataObject.Environment, update));
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Error("DSFDateTime", e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfDateTimeActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                }
                if(dataObject.IsDebugMode())
                {
                    if(hasErrors)
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", Result, ""));
                    }
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
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
                    throw new Exception(ErrorResource.TimeMustBeNumeric);
                }
            }
            return DateTimeConverterFactory.CreateDateTimeTO(evaledDateTime, evaledInputFormat, evaledOutputFormat, timeModifierType, tmpTimeAmount, Result);
        }

        #endregion Private Methods

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList, int update)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion Get Inputs/Outputs

        #region Get ForEach Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
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

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == Result);
            if(itemUpdate != null)
            {
                Result = itemUpdate.Item2;
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
