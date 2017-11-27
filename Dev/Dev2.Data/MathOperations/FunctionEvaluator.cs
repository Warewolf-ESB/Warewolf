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
using System.Globalization;
using Dev2.Common;
using Infragistics.Calculations.CalcManager;
using Infragistics.Calculations.Engine;
using Warewolf.Resource.Errors;
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.MathOperations
{
    public class FunctionEvaluator : IFunctionEvaluator
    {
        private readonly IDev2CalculationManager _manager;
        private readonly FunctionEvaluatorOption _functionEvaluatorOption;

        public FunctionEvaluator()
        {
            _manager = new Dev2CalculationManager();
            _functionEvaluatorOption = FunctionEvaluatorOption.Dev2DateTimeFormat;
        }

        public FunctionEvaluator(FunctionEvaluatorOption functionEvaluatorOption) : this()
        {
            _functionEvaluatorOption = functionEvaluatorOption;
        }

        public bool TryEvaluateFunction(string expression, out string evaluation, out string error)
        {
            var evaluationState = false;
            error = string.Empty;
            evaluation = string.Empty;
            if (!string.IsNullOrEmpty(expression))
            {
                try
                {
                    CalculationValue value = _manager.CalculateFormula(expression);
                    if (value.IsError)
                    {
                        error = value.ToErrorValue().Message;
                    }
                    else
                    {
                        evaluation = value.IsDateTime ? PerformEvaluation(value) : value.GetResolvedValue().ToString();
                        evaluationState = true;
                    }
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(ErrorResource.FunctionEvaluationError, ex, GlobalConstants.WarewolfError);
                    error = ex.Message;
                    evaluationState = false;
                }
            }
            else
            {
                error = ErrorResource.NothingToEvaluate;
            }

            return evaluationState;
        }

        private string PerformEvaluation(CalculationValue value)
        {
            string evaluation;
            var dateTime = value.ToDateTime();
            if (_functionEvaluatorOption == FunctionEvaluatorOption.DotNetDateTimeFormat)
            {
                evaluation = dateTime.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat);
            }
            else
            {
                var shortPattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                var longPattern = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
                var finalPattern = shortPattern + " " + longPattern;
                if (finalPattern.Contains("ss"))
                {
                    finalPattern = finalPattern.Insert(finalPattern.IndexOf("ss", StringComparison.Ordinal) + 2, ".fff");
                }
                evaluation = dateTime.ToString(finalPattern);
            }

            return evaluation;
        }
    }
}
