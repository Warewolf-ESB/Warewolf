/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using System;
using System.Globalization;

namespace Dev2.Common.DateAndTime
{
    public class StandardDateTimeFormatter : DateTimeFormatter
    {
        protected override void CreateDateTimeFormatParts()
        {
            DateTimeFormatParts.Clear();
            DateTimeFormatParts.Add("y", (a, b) => b.ToString("y"));
            DateTimeFormatParts.Add("yy", (a, b) => b.ToString("yy"));
            DateTimeFormatParts.Add("yyy", (a, b) => b.ToString("yyy"));
            DateTimeFormatParts.Add("yyyy", (a, b) => b.ToString("yyyy"));
            DateTimeFormatParts.Add("M", (a, b) => b.ToString("M"));
            DateTimeFormatParts.Add("MM", (a, b) => b.ToString("MM"));
            DateTimeFormatParts.Add("MMM", (a, b) => b.ToString("MMM"));
            DateTimeFormatParts.Add("MMMM", (a, b) => b.ToString("MMMM"));
            DateTimeFormatParts.Add("d", (a, b) => b.ToString("d"));
            DateTimeFormatParts.Add("dd", (a, b) => b.ToString("dd"));
            DateTimeFormatParts.Add("ddd", (a, b) => b.ToString("ddd"));
            DateTimeFormatParts.Add("dddd", (a, b) => b.ToString("dddd"));
            DateTimeFormatParts.Add("h", (a, b) => b.ToString("h"));
            DateTimeFormatParts.Add("hh", (a, b) => b.ToString("hh"));
            DateTimeFormatParts.Add("H", (a, b) => b.ToString("H"));
            DateTimeFormatParts.Add("HH", (a, b) => b.ToString("HH"));
            DateTimeFormatParts.Add("m", (a, b) => b.ToString("m"));
            DateTimeFormatParts.Add("mm", (a, b) => b.ToString("mm"));
            DateTimeFormatParts.Add("s", (a, b) => b.ToString("s"));
            DateTimeFormatParts.Add("ss", (a, b) => b.ToString("ss"));
            DateTimeFormatParts.Add("F", (a, b) => b.ToString("F"));
            DateTimeFormatParts.Add("FF", (a, b) => b.ToString("FF"));
            DateTimeFormatParts.Add("FFF", (a, b) => b.ToString("FFF"));
            DateTimeFormatParts.Add("FFFF", (a, b) => b.ToString("FFFF"));
            DateTimeFormatParts.Add("FFFFF", (a, b) => b.ToString("FFFFF"));
            DateTimeFormatParts.Add("FFFFFF", (a, b) => b.ToString("FFFFFF"));
            DateTimeFormatParts.Add("FFFFFFF", (a, b) => b.ToString("FFFFFFF"));
            DateTimeFormatParts.Add("t", (a, b) => b.ToString("t"));
            DateTimeFormatParts.Add("tt", (a, b) => b.ToString("tt"));
            DateTimeFormatParts.Add("K", (a, b) => b.ToString("K"));
            DateTimeFormatParts.Add("gg", (a, b) => b.ToString("gg"));
        }
        public override bool TryFormat(IDateTimeOperationTO dateTimeTO, out string result, out string error)
        {
            {
                var internallyParsedValue = DateTime.TryParseExact(dateTimeTO.DateTime?.Trim(), dateTimeTO.InputFormat, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var dateResult);
                if (internallyParsedValue)
                {
                    var tmpDateTime = PerformDateTimeModification(dateTimeTO, dateResult);
                    result = tmpDateTime.ToString(dateTimeTO.OutputFormat, CultureInfo.InvariantCulture);
                    error = "";
                }
                else
                {
                    var secondResult = DateTime.Parse(dateTimeTO.DateTime?.Trim(), CultureInfo.InvariantCulture);
                    var tmpDateTime = PerformDateTimeModification(dateTimeTO, secondResult);
                    result = tmpDateTime.ToString(dateTimeTO.OutputFormat, CultureInfo.InvariantCulture);
                    error = "";

                }
                return true;
            }
        }

        DateTime PerformDateTimeModification(IDateTimeOperationTO dateTimeTO, DateTime tmpDateTime)
        {
            var dateTime = tmpDateTime;
            if (!string.IsNullOrWhiteSpace(dateTimeTO.TimeModifierType))
            {
                Func<DateTime, int, DateTime> funcToExecute;
                if (TimeModifiers.TryGetValue(dateTimeTO.TimeModifierType, out funcToExecute) &&
                    funcToExecute != null)
                {
                    dateTime = funcToExecute(dateTime, dateTimeTO.TimeModifierAmount);
                }
            }

            return dateTime;
        }
    }
}