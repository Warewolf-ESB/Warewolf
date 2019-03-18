#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
