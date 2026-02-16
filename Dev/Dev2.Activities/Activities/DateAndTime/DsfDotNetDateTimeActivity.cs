#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.DateAndTime;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Common.X6;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Validation;
using Dev2.WorkflowConverters;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities.DateAndTime
{
    [ToolDescriptorInfo("Utility-DateTime", "Date Time", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Utility_Date_Time")]
    public class DsfDotNetDateTimeActivity : DsfActivityAbstract<string>, IDateTimeOperationTO
    {
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

        /// <summary>
        /// The consructor for the activity 
        /// </summary>
        public DsfDotNetDateTimeActivity()
            : base("Date and Time")
        {
            DateTime = string.Empty;
            InputFormat = string.Empty;
            OutputFormat = string.Empty;
            TimeModifierType = "";
            TimeModifierAmountDisplay = string.Empty;
            TimeModifierAmount = 0;
            Result = string.Empty;
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        public override List<string> GetOutputs() => new List<string> { Result };

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            var allErrors = new ErrorResultTO();
            InitializeDebug(dataObject);
            try
            {
                TryExecute(dataObject, update, allErrors);
            }
            catch (Exception e)
            {
                Dev2Logger.Error("DSFDateTime", e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    if (!this.IsErrorHandled)
                    {
                        var errorString = allErrors.MakeDisplayReady();
                        dataObject.Environment.AddError(errorString);
                    }
                    DisplayAndWriteError(dataObject,DisplayName, allErrors);
                }
                if (dataObject.IsDebugMode())
                {
                    if (hasErrors)
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", Result, ""));
                    }
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }

                RunOnErrorSteps(dataObject, allErrors, update);
            }
        }

        void TryExecute(IDSFDataObject dataObject, int update, ErrorResultTO allErrors)
        {
            IsSingleValueRule.ApplyIsSingleValueRule(Result, allErrors);

            AddValidationErrors(allErrors);

            if (!allErrors.HasErrors())
            {
                UpdateEnvironmentAndDebugOutput(dataObject, update, allErrors);
            }
        }

        void UpdateEnvironmentAndDebugOutput(IDSFDataObject dataObject, int update, ErrorResultTO allErrors)
        {
            var colItr = new WarewolfListIterator();
            bool isDateTimeEmpty = false;
            if (string.IsNullOrEmpty(DateTime))
            {
                isDateTimeEmpty = true;
                DateTime = string.IsNullOrEmpty(DateTime) ? System.DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss") : DateTime;
            }          
            
            var dtItr = CreateDataListEvaluateIterator(DateTime, dataObject.Environment, update);
            colItr.AddVariableToIterateOn(dtItr);
            var ifItr = CreateDataListEvaluateIterator(string.IsNullOrEmpty(InputFormat) ? GlobalConstants.Dev2DotNetDefaultDateTimeFormat : InputFormat, dataObject.Environment, update);
            colItr.AddVariableToIterateOn(ifItr);
            var ofItr = CreateDataListEvaluateIterator(string.IsNullOrEmpty(OutputFormat) ? GlobalConstants.Dev2DotNetDefaultDateTimeFormat : OutputFormat, dataObject.Environment, update);
            colItr.AddVariableToIterateOn(ofItr);
            var tmaItr = CreateDataListEvaluateIterator(TimeModifierAmountDisplay, dataObject.Environment, update);
            colItr.AddVariableToIterateOn(tmaItr);

            if (!allErrors.HasErrors())
            {
                while (colItr.HasMoreData())
                {
                    var transObj = ConvertToDateTimeTo(colItr.FetchNextValue(dtItr),
                        colItr.FetchNextValue(ifItr),
                        colItr.FetchNextValue(ofItr),
                        TimeModifierType,
                        colItr.FetchNextValue(tmaItr)
                        );

                    var format = DateTimeConverterFactory.CreateStandardFormatter();
                    if (format.TryFormat(transObj, out string result, out string error))
                    {
                        AddDefaultDebugInfo(dataObject, result);
                        AddDebugInfo(dataObject, update);
                        var expression = Result;
                        dataObject.Environment.Assign(expression, result, update);
                    }
                    else
                    {
                        allErrors.AddError(error);
                    }
                }
                if (dataObject.IsDebugMode() && !allErrors.HasErrors())
                {

                    var resDebug = new DebugEvalResult(Result, "", dataObject.Environment, update);
                    AddDebugOutputItem(resDebug);
                }
            }
            if (isDateTimeEmpty)
            {
                DateTime = string.Empty;
            }
        }

        void AddValidationErrors(ErrorResultTO allErrors)
        {
            if (DataListUtil.HasNegativeIndex(InputFormat))
            {
                allErrors.AddError(string.Format("Negative Recordset Index for Input Format: {0}", InputFormat));
            }
            if (DataListUtil.HasNegativeIndex(OutputFormat))
            {
                allErrors.AddError(string.Format("Negative Recordset Index for Output Format: {0}", OutputFormat));
            }
            if (DataListUtil.HasNegativeIndex(TimeModifierAmountDisplay))
            {
                allErrors.AddError(string.Format("Negative Recordset Index for Add Time: {0}", TimeModifierAmountDisplay));
            }
        }

        void AddDebugInfo(IDSFDataObject dataObject, int update)
        {
            if (dataObject.IsDebugMode())
            {
                if (!string.IsNullOrEmpty(DateTime))
                {
                    AddDebugInputItem(new DebugEvalResult(DateTime, "Input", dataObject.Environment, update));
                }

                var dateTimePattern = string.Format("{0}", GlobalConstants.Dev2DotNetDefaultDateTimeFormat);

                if (string.IsNullOrEmpty(InputFormat))
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

                if (string.IsNullOrEmpty(OutputFormat))
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
        }

        void AddDefaultDebugInfo(IDSFDataObject dataObject, string result)
        {
            if (string.IsNullOrEmpty(DateTime) && dataObject.IsDebugMode())
            {
                var defaultDateTimeDebugItem = new DebugItem();
                AddDebugItem(new DebugItemStaticDataParams("System Date Time", "Input"), defaultDateTimeDebugItem);
                AddDebugItem(new DebugItemStaticDataParams(result, "="), defaultDateTimeDebugItem);
                _debugInputs.Add(defaultDateTimeDebugItem);
            }
        }

        IDateTimeOperationTO ConvertToDateTimeTo(string evaledDateTime, string evaledInputFormat, string evaledOutputFormat, string timeModifierType, string tTimeModifierAmount)
        {
            var tmpTimeAmount = 0;
            if (!string.IsNullOrWhiteSpace(tTimeModifierAmount) && !int.TryParse(tTimeModifierAmount, out tmpTimeAmount))
            {
                throw new Exception(ErrorResource.TimeMustBeNumeric);
            }

            return DateTimeConverterFactory.CreateDateTimeTO(evaledDateTime, evaledInputFormat, evaledOutputFormat, timeModifierType, tmpTimeAmount, Result);
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

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == Result);
            if (itemUpdate != null)
            {
                Result = itemUpdate.Item2;
            }
        }

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(DateTime, InputFormat, TimeModifierAmountDisplay, OutputFormat);

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(Result);

        public bool Equals(DsfDotNetDateTimeActivity other)
        {
            var eq = base.Equals(other);
            eq &= DisplayName.Equals(other.DisplayName);
            eq &= DateTime.Equals(other.DateTime);
            eq &= InputFormat.Equals(other.InputFormat);
            eq &= OutputFormat.Equals(other.OutputFormat);
            eq &= TimeModifierType.Equals(other.TimeModifierType);
            eq &= TimeModifierAmountDisplay.Equals(other.TimeModifierAmountDisplay);
            eq &= TimeModifierAmount.Equals(other.TimeModifierAmount);
            eq &= Result.Equals(other.Result);
            return eq;
        }

        public override bool Equals(object obj)
        {
            if (obj is DsfDotNetDateTimeActivity instance)
            {
                return Equals(instance);
            }
            return false;
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[]
            {
                new StateVariable
                {
                    Name="DateTime",
                    Type = StateVariable.StateType.Input,
                    Value = DateTime
                },
                new StateVariable
                {
                    Name="InputFormat",
                    Type = StateVariable.StateType.Input,
                    Value = InputFormat
                },
                new StateVariable
                {
                    Name="OutputFormat",
                    Type = StateVariable.StateType.Input,
                    Value = OutputFormat
                },
                new StateVariable
                {
                    Name="TimeModifierType",
                    Type = StateVariable.StateType.Input,
                    Value = TimeModifierType
                },
                new StateVariable
                {
                    Name="TimeModifierAmountDisplay",
                    Type = StateVariable.StateType.Input,
                    Value = TimeModifierAmountDisplay
                },
                new StateVariable
                {
                    Name="TimeModifierAmount",
                    Type = StateVariable.StateType.Input,
                    Value = TimeModifierAmount.ToString()
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = Result
                }
            };
        }

        /// <summary>
        /// Serializes the DotNet DateTime activity to X6 JSON format
        /// </summary>
        /// <param name="cell">The X6 cell to populate with DateTime data</param>
        public virtual void ToX6Json(Dev2.Common.X6.Cell cell)
        {
            if (cell.data == null) cell.data = new Dictionary<string, object>();

            // Call base implementation for common properties (OnError handling, etc.)
            // Note: Base class doesn't have ToX6Json, so we handle common properties here
            
            cell.shape = Constants.DSFDOTNETDATETIMEACTIVITY;
            // Set the activity type
            cell.data[Constants.TYPE] = Constants.DSFDOTNETDATETIMEACTIVITY.ToLower();
            cell.data[Constants.DISPLAYNAME] = DisplayName ?? Constants.DISPLAYNAME_DOTNETDATETIME;

            // Serialize DotNet DateTime specific properties
            cell.data[Constants.DOTNETDATETIME_DATETIME] = DateTime ?? string.Empty;
            cell.data[Constants.DOTNETDATETIME_INPUTFORMAT] = InputFormat ?? string.Empty;
            cell.data[Constants.DOTNETDATETIME_OUTPUTFORMAT] = OutputFormat ?? string.Empty;
            cell.data[Constants.DOTNETDATETIME_TIMEMODIFIERTYPE] = TimeModifierType ?? string.Empty;
            cell.data[Constants.DOTNETDATETIME_TIMEMODIFIERAMOUNTDISPLAY] = TimeModifierAmountDisplay ?? string.Empty;
            cell.data[Constants.DOTNETDATETIME_TIMEMODIFIERAMOUNT] = TimeModifierAmount;
            cell.data[Constants.DOTNETDATETIME_RESULT] = Result ?? string.Empty;
        }

        /// <summary>
        /// Deserializes the DotNet DateTime activity from X6 JSON format
        /// </summary>
        /// <param name="cell">The X6 cell containing DateTime data</param>
        public virtual void FromX6Json(Dev2.Common.X6.Cell cell)
        {
            if (cell == null || cell.data == null) return;

            // Note: Base class doesn't have FromX6Json, so we handle common properties here

            try
            {
                if (cell.data.TryGetString(Constants.DISPLAYNAME, out string displayName))
                    this.DisplayName = displayName;

                // Deserialize DotNet DateTime specific properties
                if (cell.data.TryGetString(Constants.DOTNETDATETIME_DATETIME, out string dateTime))
                    this.DateTime = dateTime;

                if (cell.data.TryGetString(Constants.DOTNETDATETIME_INPUTFORMAT, out string inputFormat))
                    this.InputFormat = inputFormat;

                if (cell.data.TryGetString(Constants.DOTNETDATETIME_OUTPUTFORMAT, out string outputFormat))
                    this.OutputFormat = outputFormat;

                if (cell.data.TryGetString(Constants.DOTNETDATETIME_TIMEMODIFIERTYPE, out string timeModifierType))
                    this.TimeModifierType = timeModifierType;

                if (cell.data.TryGetString(Constants.DOTNETDATETIME_TIMEMODIFIERAMOUNTDISPLAY, out string timeModifierAmountDisplay))
                    this.TimeModifierAmountDisplay = timeModifierAmountDisplay;

                if (cell.data.TryGetInt(Constants.DOTNETDATETIME_TIMEMODIFIERAMOUNT, out int timeModifierAmount))
                    this.TimeModifierAmount = timeModifierAmount;

                if (cell.data.TryGetString(Constants.DOTNETDATETIME_RESULT, out string result))
                    this.Result = result;

                // Defensive initialization
                DateTime ??= string.Empty;
                InputFormat ??= string.Empty;
                OutputFormat ??= string.Empty;
                TimeModifierType ??= string.Empty;
                TimeModifierAmountDisplay ??= string.Empty;
                Result ??= string.Empty;
            }
            catch (Exception ex)
            {
                // Log error but don't throw - graceful degradation
                Dev2Logger.Error($"Error deserializing DotNet DateTime data from X6 JSON: {ex.Message}", ex, GlobalConstants.WarewolfError);
            }
        }
    }
}
