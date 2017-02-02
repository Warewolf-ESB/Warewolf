/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.Operations;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Util;
using Dev2.Validation;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    [ToolDescriptorInfo("Utility-FormatNumber", "Format Number", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Utility_Format Number")]
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

        protected override bool CanInduceIdle => true;

        #endregion Properties

        public override List<string> GetOutputs()
        {
            return new List<string> { Result };
        }

        #region Override Methods

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            var allErrors = new ErrorResultTO();

            InitializeDebug(dataObject);
            try
            {
                var expression = Expression ?? string.Empty;
                var roundingDecimalPlaces = RoundingDecimalPlaces ?? string.Empty;
                var decimalPlacesToShow = DecimalPlacesToShow ?? string.Empty;

                AddDebugInputItems(dataObject, update, expression, roundingDecimalPlaces, decimalPlacesToShow);
                var colItr = new WarewolfListIterator();
                var expressionIterator = CreateDataListEvaluateIterator(expression, dataObject.Environment, update);
                var roundingDecimalPlacesIterator = CreateDataListEvaluateIterator(roundingDecimalPlaces, dataObject.Environment, update);
                var decimalPlacesToShowIterator = CreateDataListEvaluateIterator(decimalPlacesToShow, dataObject.Environment, update);
                colItr.AddVariableToIterateOn(expressionIterator);
                colItr.AddVariableToIterateOn(roundingDecimalPlacesIterator);
                colItr.AddVariableToIterateOn(decimalPlacesToShowIterator);
                // Loop data ;)
                var rule = new IsSingleValueRule(() => Result);
                var single = rule.Check();
                var counter = 1;
                while (colItr.HasMoreData())
                {
                    int decimalPlacesToShowValue;
                    var tmpDecimalPlacesToShow = colItr.FetchNextValue(decimalPlacesToShowIterator);
                    var adjustDecimalPlaces = tmpDecimalPlacesToShow.IsRealNumber(out decimalPlacesToShowValue);
                    if (!string.IsNullOrEmpty(tmpDecimalPlacesToShow) && !adjustDecimalPlaces)
                    {
                        throw new Exception(ErrorResource.DecimalsNotValid);
                    }

                    var tmpDecimalPlaces = colItr.FetchNextValue(roundingDecimalPlacesIterator);
                    var roundingDecimalPlacesValue = 0;
                    if (!string.IsNullOrEmpty(tmpDecimalPlaces) && !tmpDecimalPlaces.IsRealNumber(out roundingDecimalPlacesValue))
                    {
                        throw new Exception(ErrorResource.RoundingNotValid);
                    }
                    string result;
                    var binaryDataListItem = colItr.FetchNextValue(expressionIterator);
                    var val = binaryDataListItem;
                    {
                        FormatNumberTO formatNumberTo = new FormatNumberTO(val, RoundingType, roundingDecimalPlacesValue, adjustDecimalPlaces, decimalPlacesToShowValue);
                        result = _numberFormatter.Format(formatNumberTo);
                    }


                    if (single != null)
                    {
                        allErrors.AddError(single.Message);
                    }
                    else
                    {
                        UpdateResultRegions(dataObject.Environment, result, update == 0 ? counter : update);
                        counter++;
                    }
                }
                if (dataObject.IsDebugMode())
                {
                    AddDebugOutputItem(new DebugEvalResult(Result, "", dataObject.Environment, update));
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("DSFNumberFormatActivity", e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                if (allErrors.HasErrors())
                {
                    if (dataObject.IsDebugMode())
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", Result, ""));
                    }
                    DisplayAndWriteError("DsfNumberFormatActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                }

                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        private void AddDebugInputItems(IDSFDataObject dataObject, int update, string expression, string roundingDecimalPlaces, string decimalPlacesToShow)
        {
            if (dataObject.IsDebugMode())
            {
                AddDebugInputItem(expression, "Number", dataObject.Environment, update);
                if (!String.IsNullOrEmpty(RoundingType))
                {
                    AddDebugInputItem(new DebugItemStaticDataParams(RoundingType, "Rounding"));
                }
                AddDebugInputItem(roundingDecimalPlaces, "Rounding Value", dataObject.Environment, update);
                AddDebugInputItem(decimalPlacesToShow, "Decimals to show", dataObject.Environment, update);
            }
        }

        void UpdateResultRegions(IExecutionEnvironment environment, string result, int update)
        {
            environment.Assign(Result, result, update);
        }

        #endregion Override Methods

        #region Private Methods

        private void AddDebugInputItem(string expression, string labelText, IExecutionEnvironment environment, int update)
        {
            DebugItem itemToAdd = new DebugItem();
            if (environment != null)
            {
                AddDebugItem(new DebugEvalResult(expression, labelText, environment, update), itemToAdd);
            }
            else
            {
                AddDebugItem(new DebugItemStaticDataParams("", labelText, expression), itemToAdd);
            }

            _debugInputs.Add(itemToAdd);
        }

        #endregion Private Methods

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList, int update)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion Get Debug Inputs/Outputs

        #region Update ForEach Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if (updates != null)
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

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == Result);
            if (itemUpdate != null)
            {
                Result = itemUpdate.Item2;
            }
        }

        #endregion Update ForEach Inputs/Outputs

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(Expression, RoundingType, RoundingDecimalPlaces, DecimalPlacesToShow);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion GetForEachInputs/Outputs
    }
}