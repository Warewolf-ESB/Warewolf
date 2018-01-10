/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Globalization;
using Dev2.Common.ExtMethods;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.MathOperations;
using Dev2.MathOperations;
using Warewolf.Resource.Errors;

namespace Dev2.Data.Operations
{
    public class Dev2NumberFormatter : IDev2NumberFormatter
    {
        #region Class Members

        
        const string _decimalSeperator = ".";
        static readonly IFunctionEvaluator _functionEvaluator = MathOpsFactory.CreateFunctionEvaluator();


        #endregion Class Members

        #region Methods

        /// <summary>
        /// Formats a number.
        /// </summary>
        /// <param name="formatNumberTO">The information on how to format the number.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">formatNumberTO</exception>

        public string Format(IFormatNumberTO formatNumberTO)

        {
            if(formatNumberTO == null)
            {
                throw new ArgumentNullException("formatNumberTO");
            }

            if(formatNumberTO.RoundingDecimalPlaces < -14 || formatNumberTO.RoundingDecimalPlaces > 14)
            {
                throw new InvalidOperationException(string.Format(ErrorResource.RoundingDecimalPlaceBetween, "-14", "14"));
            }

            if(formatNumberTO.AdjustDecimalPlaces && (formatNumberTO.DecimalPlacesToShow < -14 || formatNumberTO.DecimalPlacesToShow > 14))
            {
                throw new InvalidOperationException(string.Format(ErrorResource.RoundingDecimalPlaceBetween, "-14", "14"));
            }

            var result = Round(formatNumberTO);
            result = AdjustDecimalPlaces(result, formatNumberTO.AdjustDecimalPlaces, formatNumberTO.DecimalPlacesToShow);
            return result;
        }

        #endregion Methods

        #region Private Methods


        string BuildRoundingExpression(IFormatNumberTO formatNumberTO)

        {
            string expression;

            var roundingType = formatNumberTO.GetRoundingTypeEnum();
            if (roundingType == enRoundingType.Normal)
            {
                expression = "round({0}, {1})";
            }
            else if (roundingType == enRoundingType.Up)
            {
                expression = "roundup({0}, {1})";
            }
            else if (roundingType == enRoundingType.Down)
            {
                expression = "rounddown({0}, {1})";
            }
            else
            {
                expression = "{0}";
            }

            expression = string.Format(expression, formatNumberTO.Number, formatNumberTO.RoundingDecimalPlaces);

            return expression;
        }


        string Round(IFormatNumberTO formatNumberTO)

        {
            _functionEvaluator.TryEvaluateFunction(BuildRoundingExpression(formatNumberTO), out string result, out string error);

            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new InvalidOperationException(error);
            }

            return result;
        }

        decimal Parse(string numberString)
        {
            if (!numberString.IsNumeric(out decimal number))
            {
                throw new InvalidOperationException(string.Format(ErrorResource.ErrorWhileFormattingANumber, numberString));
            }

            return number;
        }

        /// <summary>
        /// Mathamatically adjust the decimal places.
        /// </summary>
        /// <param name="numberString">The string containing the number to adjust.</param>
        /// <param name="adjustDecimalPlaces">if set to <c>true</c> decimal places are adjusted.</param>
        /// <param name="decimalPlacesToShow">The decimal places to show.</param>
        string AdjustDecimalPlaces(string numberString, bool adjustDecimalPlaces, int decimalPlacesToShow)
        {
            var number = Parse(numberString);

            if (!adjustDecimalPlaces)
            {
                return FormatNumber(number, false, decimalPlacesToShow);
            }

            var modifier = (decimal)Math.Pow(10, Math.Abs(decimalPlacesToShow));
            var integral = Math.Truncate(number);
            decimal adjustedNumber;

            if (decimalPlacesToShow >= 0)
            {
                var decimals = number - integral;

                var newDecimals = Math.Truncate(decimals * modifier) / modifier;
                adjustedNumber = integral + newDecimals;
            }
            else
            {
                adjustedNumber = number / modifier;
            }

            return FormatNumber(adjustedNumber, true, decimalPlacesToShow);
        }

        /// <summary>
        /// Formats a number into a string with the specified number of decimal places.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="adjustDecimalPlaces">if set to <c>true</c> decimal places as adjusted].</param>
        /// <param name="decimalPlacesToShow">The number decimal places to show.</param>
        string FormatNumber(decimal number, bool adjustDecimalPlaces, int decimalPlacesToShow)
        {
            var format = "0";

            if (adjustDecimalPlaces)
            {
                if (decimalPlacesToShow > 0)
                {
                    //
                    // Output a specific number of decimal places in thenumber
                    //
                    format += _decimalSeperator + format.PadRight(format.Length + decimalPlacesToShow - 1, '0');
                }
                else
                {
                    decimalPlacesToShow *= -1;
                    var multiplier = "1";
                    multiplier = multiplier.PadRight(decimalPlacesToShow + 1, char.Parse("0"));
                    var numbers = number.ToString(CultureInfo.InvariantCulture).Split('.');
                    return (Math.Truncate(decimal.Parse(numbers[0]) * int.Parse(multiplier)) / int.Parse(multiplier)).ToString(CultureInfo.InvariantCulture);
                }
            }
            else
            {
                //
                // Output the actual number of decimal places in the number
                //
                format += _decimalSeperator + "#############################";
            }

            return number.ToString(format);
        }

        #endregion Private Methods
    }
}
