#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Dev2.Common.DateAndTime
{
    public class DateTimeFormatter : DateTimeFormatterBase
    {
        protected static readonly Dictionary<string, Func<IDateTimeResultTO, DateTime, string>> DateTimeFormatParts =
            new Dictionary<string, Func<IDateTimeResultTO, DateTime, string>>();

        protected static readonly Dictionary<string, Func<DateTime, int, DateTime>> TimeModifiers =
            new Dictionary<string, Func<DateTime, int, DateTime>>();

        static IList<string> _listOfModifierTypes = new List<string>();

        static DateTimeFormatter()
        {
            CreateDateTimeFormatParts();
            CreateTimeModifierTypes();
        }

        public static IList<string> TimeModifierTypes
        {
            get { return _listOfModifierTypes; }
            private set { _listOfModifierTypes = value; }
        }

        public override bool TryFormat(DateTime dateTimeTO, out string result, out string error)
        {
            error = "";
            result = "";
            try
            {
                var shortPattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                var longPattern = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
                var finalPattern = shortPattern + " " + longPattern;
                var outputFormat = finalPattern;
                if (dateTimeTO.Millisecond > 0 && finalPattern.Contains("ss"))
                {
                    outputFormat = finalPattern.Insert(finalPattern.IndexOf("ss", StringComparison.Ordinal) + 2, ".fff");
                }
                result = dateTimeTO.ToString(outputFormat);
                return true;
            }
            catch
            {
                error = string.Concat("Unrecognized format '", dateTimeTO, "'.");
                return false;
            }
        }

        public override bool TryFormat(IDateTimeOperationTO dateTimeTO, out string result, out string error)
        {
            result = "";
            var dateTimeParser = DateTimeConverterFactory.CreateParser();
            var nothingDied = true;
            dateTimeTO.InputFormat = dateTimeTO.InputFormat?.Trim();

            if (dateTimeParser.TryParseDateTime(dateTimeTO.DateTime?.Trim(), dateTimeTO.InputFormat, out IDateTimeResultTO dateTimeResultTO, out error))
            {
                var tmpDateTime = dateTimeResultTO.ToDateTime();
                if (!string.IsNullOrWhiteSpace(dateTimeTO.TimeModifierType) && TimeModifiers.TryGetValue(dateTimeTO.TimeModifierType, out Func<DateTime, int, DateTime> funcToExecute) && funcToExecute != null)
                {
                    tmpDateTime = funcToExecute(tmpDateTime, dateTimeTO.TimeModifierAmount);
                }


                if (nothingDied)
                {
                    result = ApplyDateTimeFormatParts(dateTimeTO, result, out error, dateTimeParser, out nothingDied, dateTimeResultTO, tmpDateTime);
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        private static string ApplyDateTimeFormatParts(IDateTimeOperationTO dateTimeTO, string result, out string error, IDateTimeParser dateTimeParser, out bool nothingDied, IDateTimeResultTO dateTimeResultTO, DateTime tmpDateTime)
        {
            var outputFormat = string.IsNullOrWhiteSpace(dateTimeTO.OutputFormat)
                ? dateTimeTO.InputFormat
                : dateTimeTO.OutputFormat;
            if (string.IsNullOrWhiteSpace(outputFormat))
            {
                var shortPattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                var longPattern = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
                var finalPattern = shortPattern + " " + longPattern;
                if (finalPattern.Contains("ss"))
                {
                    outputFormat = finalPattern.Insert(finalPattern.IndexOf("ss", StringComparison.Ordinal) + 2, ".fff");
                    outputFormat = dateTimeParser.TranslateDotNetToDev2Format(outputFormat, out error);
                }
            }

            nothingDied = dateTimeParser.TryGetDateTimeFormatParts(outputFormat, out List<IDateTimeFormatPartTO> formatParts, out error);

            if (nothingDied)
            {
                AddParts(ref result, ref error, ref nothingDied, dateTimeResultTO, tmpDateTime, formatParts);
            }

            return result;
        }

        private static void AddParts(ref string result, ref string error, ref bool nothingDied, IDateTimeResultTO dateTimeResultTO, DateTime tmpDateTime, List<IDateTimeFormatPartTO> formatParts)
        {
            var count = 0;
            while (count < formatParts.Count && nothingDied)
            {
                var formatPart = formatParts[count];

                if (formatPart.Isliteral)
                {
                    result += formatPart.Value;
                }
                else
                {
                    if (DateTimeFormatParts.TryGetValue(formatPart.Value, out Func<IDateTimeResultTO, DateTime, string> func))
                    {
                        result += func(dateTimeResultTO, tmpDateTime);
                    }
                    else
                    {
                        nothingDied = false;
                        error = string.Concat("Unrecognized format part '", formatPart.Value, "'.");
                    }
                }

                count++;
            }
        }

        protected static void CreateDateTimeFormatParts()
        {
            DateTimeFormatParts.Add("yy", Format_yy);
            DateTimeFormatParts.Add("yyyy", Format_yyyy);
            DateTimeFormatParts.Add("mm", Format_mm);
            DateTimeFormatParts.Add("m", Format_m);
            DateTimeFormatParts.Add("M", Format_M);
            DateTimeFormatParts.Add("MM", Format_MM);
            DateTimeFormatParts.Add("d", Format_d);
            DateTimeFormatParts.Add("dd", Format_dd);
            DateTimeFormatParts.Add("DW", Format_DW);
            DateTimeFormatParts.Add("dW", Format_dW);
            DateTimeFormatParts.Add("dw", Format_dw);
            DateTimeFormatParts.Add("dy", Format_dy);
            DateTimeFormatParts.Add("w", Format_w);
            DateTimeFormatParts.Add("ww", Format_ww);
            DateTimeFormatParts.Add("24h", Format_24h);
            DateTimeFormatParts.Add("12h", Format_12h);
            DateTimeFormatParts.Add("min", Format_min);
            DateTimeFormatParts.Add("ss", Format_ss);
            DateTimeFormatParts.Add("sp", Format_sp);
            DateTimeFormatParts.Add("am/pm", Format_am_pm);
            DateTimeFormatParts.Add("Z", Format_Z);
            DateTimeFormatParts.Add("ZZ", Format_ZZ);
            DateTimeFormatParts.Add("ZZZ", Format_ZZZ);
            DateTimeFormatParts.Add("Era", Format_Era);
        }

        private static void CreateTimeModifierTypes()
        {
            TimeModifiers.Add("", null);
            TimeModifiers.Add("Years", AddYears);
            TimeModifiers.Add("Months", AddMonths);
            TimeModifiers.Add("Days", AddDays);
            TimeModifiers.Add("Weeks", AddWeeks);
            TimeModifiers.Add("Hours", AddHours);
            TimeModifiers.Add("Minutes", AddMinutes);
            TimeModifiers.Add("Seconds", AddSeconds);
            TimeModifiers.Add("Milliseconds", AddMilliseconds);
            TimeModifierTypes = new List<string>(TimeModifiers.Keys);
        }

        private static string Format_yy(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTime.ToString("yy");

        static string Format_yyyy(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTime.ToString("yyyy");

        static string Format_mm(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTime.ToString("MM");

        static string Format_m(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTime.ToString("%M");

        static string Format_MM(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTime.ToString("MMMM");

        static string Format_M(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTime.ToString("MMM");

        static string Format_d(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTime.ToString("%d");

        static string Format_dd(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTime.ToString("dd");

        static string Format_DW(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTime.ToString("dddd");

        static string Format_dW(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTime.ToString("ddd");

        static string Format_dw(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => DateTimeParserHelper.GetDayOfWeekInt(dateTime.DayOfWeek).ToString(CultureInfo.InvariantCulture);

        static string Format_dy(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTime.DayOfYear.ToString(CultureInfo.InvariantCulture);

        static string Format_w(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday)
            .ToString(CultureInfo.InvariantCulture);

        static string Format_ww(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday)
            .ToString(CultureInfo.InvariantCulture)
            .PadLeft(2, '0');

        static string Format_24h(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTime.ToString("HH");

        static string Format_12h(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTime.ToString("hh");

        static string Format_min(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTime.ToString("mm");

        static string Format_ss(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTime.ToString("ss");

        static string Format_sp(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTime.Millisecond.ToString(CultureInfo.InvariantCulture);

        static string Format_am_pm(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTime.ToString("tt");

        static string Format_Z(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTimeResultTO.TimeZone.ShortName;

        static string Format_ZZ(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTimeResultTO.TimeZone.Name;

        static string Format_ZZZ(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTimeResultTO.TimeZone.LongName;

        static string Format_Era(IDateTimeResultTO dateTimeResultTO, DateTime dateTime) => dateTime.ToString("gg");

        private static DateTime AddYears(DateTime inputDateTime, int amountToAdd)
        {
            var result = inputDateTime.AddYears(amountToAdd);
            return result;
        }

        static DateTime AddMonths(DateTime inputDateTime, int amountToAdd)
        {
            var result = inputDateTime.AddMonths(amountToAdd);
            return result;
        }

        static DateTime AddDays(DateTime inputDateTime, int amountToAdd)
        {
            var result = inputDateTime.AddDays(amountToAdd);
            return result;
        }

        static DateTime AddWeeks(DateTime inputDateTime, int amountToAdd)
        {
            var result = CultureInfo.InvariantCulture.Calendar.AddWeeks(inputDateTime, amountToAdd);
            return result;
        }

        static DateTime AddHours(DateTime inputDateTime, int amountToAdd)
        {
            var result = inputDateTime.AddHours(amountToAdd);
            return result;
        }

        static DateTime AddMinutes(DateTime inputDateTime, int amountToAdd)
        {
            var result = inputDateTime.AddMinutes(amountToAdd);
            return result;
        }

        static DateTime AddSeconds(DateTime inputDateTime, int amountToAdd)
        {
            var result = inputDateTime.AddSeconds(amountToAdd);
            return result;
        }

        private static DateTime AddMilliseconds(DateTime inputDateTime, int amountToAdd)
        {
            var result = inputDateTime.AddMilliseconds(amountToAdd);
            return result;
        }
    }
}