
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
using System.Globalization;
using Dev2.Common.ExtMethods;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.MathOperations;

namespace Dev2.Data.Operations
{
    public class Dev2NumberFormatter : IDev2NumberFormatter
    {
        #region Class Members

        // ReSharper disable InconsistentNaming
        const string _decimalSeperator = ".";
        private static readonly IFunctionEvaluator _functionEvaluator = MathOpsFactory.CreateFunctionEvaluator();
        // ReSharper restore InconsistentNaming

        #endregion Class Members

        #region Methods

        /// <summary>
        /// Formats a number.
        /// </summary>
        /// <param name="formatNumberTO">The information on how to format the number.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">formatNumberTO</exception>
        // ReSharper disable InconsistentNaming
        public string Format(FormatNumberTO formatNumberTO)
        // ReSharper restore InconsistentNaming
        {
            if(formatNumberTO == null)
            {
                throw new ArgumentNullException("formatNumberTO");
            }

            decimal tmp;
            if(!formatNumberTO.Number.IsNumeric(out tmp))
            {
                throw new InvalidOperationException("Unable to format '" + formatNumberTO.Number + "' because it isn't a number.");
            }

            if(formatNumberTO.RoundingDecimalPlaces < -14 || formatNumberTO.RoundingDecimalPlaces > 14)
            {
                throw new InvalidOperationException("Rounding decimal places must be between -14 and 14.");
            }

            if(formatNumberTO.AdjustDecimalPlaces && (formatNumberTO.DecimalPlacesToShow < -14 || formatNumberTO.DecimalPlacesToShow > 14))
            {
                throw new InvalidOperationException("Decimal places to show must be less between -14 than 14.");
            }

            string result = Round(formatNumberTO);
            result = AdjustDecimalPlaces(result, formatNumberTO.AdjustDecimalPlaces, formatNumberTO.DecimalPlacesToShow);
            return result;
        }

        #endregion Methods

        #region Private Methods

        // ReSharper disable InconsistentNaming
        private string BuildRoundingExpression(FormatNumberTO formatNumberTO)
        // ReSharper restore InconsistentNaming
        {
            string expression;

            enRoundingType roundingType = formatNumberTO.GetRoundingTypeEnum();
            if(roundingType == enRoundingType.Normal)
            {
                expression = "round({0}, {1})";
            }
            else if(roundingType == enRoundingType.Up)
            {
                expression = "roundup({0}, {1})";
            }
            else if(roundingType == enRoundingType.Down)
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

        // ReSharper disable InconsistentNaming
        private string Round(FormatNumberTO formatNumberTO)
        // ReSharper restore InconsistentNaming
        {
            string error;
            string result;
            _functionEvaluator.TryEvaluateFunction(BuildRoundingExpression(formatNumberTO), out result, out error);

            if(!string.IsNullOrWhiteSpace(error))
            {
                throw new InvalidOperationException(error);
            }

            return result;
        }

        private decimal Parse(string numberString)
        {
            decimal number;
            if(!numberString.IsNumeric(out number))
            {
                throw new InvalidOperationException("An error occurred while formatting a number, an ivalid value of '" + numberString + "' was returned from the rounding function.");
            }

            return number;
        }

        /// <summary>
        /// Mathamatically adjust the decimal places.
        /// </summary>
        /// <param name="numberString">The string containing the number to adjust.</param>
        /// <param name="adjustDecimalPlaces">if set to <c>true</c> decimal places are adjusted.</param>
        /// <param name="decimalPlacesToShow">The decimal places to show.</param>
        private string AdjustDecimalPlaces(string numberString, bool adjustDecimalPlaces, int decimalPlacesToShow)
        {
            decimal number = Parse(numberString);

            if(!adjustDecimalPlaces)
            {
                return FormatNumber(number, false, decimalPlacesToShow);
            }

            decimal modifier = (decimal)Math.Pow(10, Math.Abs(decimalPlacesToShow));
            decimal integral = Math.Truncate(number);
            decimal adjustedNumber;

            if(decimalPlacesToShow >= 0)
            {
                decimal decimals = number - integral;

                decimal newDecimals = Math.Truncate(decimals * modifier) / modifier;
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
        private string FormatNumber(decimal number, bool adjustDecimalPlaces, int decimalPlacesToShow)
        {
            string format = "0";

            if(adjustDecimalPlaces)
            {
                if(decimalPlacesToShow > 0)
                {
                    //
                    // Output a specific number of decimal places in thenumber
                    //
                    format += _decimalSeperator + format.PadRight(format.Length + decimalPlacesToShow - 1, '0');
                }
                else
                {
                    decimalPlacesToShow *= -1;
                    string multiplier = "1";
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
