/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.DateAndTime.TO;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Dev2.Common.DateAndTime
{
    public class DateTimeFormatter : IDateTimeFormatter
    {
        #region Class Members

        private static readonly Dictionary<string, Func<IDateTimeResultTO, DateTime, string>> DateTimeFormatParts =
            new Dictionary<string, Func<IDateTimeResultTO, DateTime, string>>();

        //27.09.2012: massimo.guerrera - Added for the new way of doing time modification
        private static readonly Dictionary<string, Func<DateTime, int, DateTime>> TimeModifiers =
            new Dictionary<string, Func<DateTime, int, DateTime>>();

        private static IList<string> _listOfModifierTypes = new List<string>();

        #endregion Class Members

        #region Constructors

        static DateTimeFormatter()
        {
            CreateDateTimeFormatParts();
            CreateTimeModifierTypes();
        }

        #endregion Constructors

        #region Properties

        public static IList<string> TimeModifierTypes
        {
            get { return _listOfModifierTypes; }
            private set { _listOfModifierTypes = value; }
        }

        #endregion Properties

        #region Methods

        public bool TryFormat(IDateTimeOperationTO dateTimeTO, out string result, out string error)
        {
            result = "";
            error = "";
            var dateTimeParser = DateTimeConverterFactory.CreateParser();
            var nothingDied = true;
            IDateTimeResultTO dateTimeResultTO;

            dateTimeTO.InputFormat = dateTimeTO.InputFormat?.Trim();
            var internallyParsedValue = dateTimeParser.TryParseDateTime(dateTimeTO.DateTime.Trim(), dateTimeTO.InputFormat, out dateTimeResultTO, out error);
            if (internallyParsedValue)
            {
                var tmpDateTime = dateTimeResultTO.ToDateTime();
                tmpDateTime = PerformDateTimeModification(dateTimeTO, tmpDateTime);

                if (nothingDied)
                {
                    result = PerformOutputFormatting(dateTimeTO, out error, dateTimeParser, out nothingDied, dateTimeResultTO, tmpDateTime);
                }
            }
            else
            {
                nothingDied = false;
            }
            return nothingDied;
        }

        private static DateTime PerformDateTimeModification(IDateTimeOperationTO dateTimeTO, DateTime tmpDateTime)
        {
            if (!string.IsNullOrWhiteSpace(dateTimeTO.TimeModifierType))
            {
                Func<DateTime, int, DateTime> funcToExecute;
                if (TimeModifiers.TryGetValue(dateTimeTO.TimeModifierType, out funcToExecute) &&
                    funcToExecute != null)
                {
                    tmpDateTime = funcToExecute(tmpDateTime, dateTimeTO.TimeModifierAmount);
                }
            }

            return tmpDateTime;
        }

        private static string PerformOutputFormatting(IDateTimeOperationTO dateTimeTO, out string error, IDateTimeParser dateTimeParser, out bool nothingDied, IDateTimeResultTO dateTimeResultTO, DateTime tmpDateTime)
        {
            var outputFormat = string.IsNullOrWhiteSpace(dateTimeTO.OutputFormat)
                                    ? dateTimeTO.InputFormat
                                    : dateTimeTO.OutputFormat;
            var result = "";
            if (string.IsNullOrWhiteSpace(outputFormat))
            {
                var finalPattern = GlobalConstants.Dev2DotNetDefaultDateTimeFormat;
                if (finalPattern.Contains("ss"))
                {
                    outputFormat = finalPattern.Insert(finalPattern.IndexOf("ss", StringComparison.Ordinal) + 2, ".fff");
                    outputFormat = dateTimeParser.TranslateDotNetToDev2Format(outputFormat, out error);
                }
            }

            List<IDateTimeFormatPartTO> formatParts;
            nothingDied = DateTimeParser.TryGetDateTimeFormatParts(outputFormat, out formatParts, out error);

            if (nothingDied)
            {
                var count = 0;
                var builder = new System.Text.StringBuilder();
                builder.Append(result);
                while (count < formatParts.Count && nothingDied)
                {
                    var formatPart = formatParts[count];

                    if (formatPart.Isliteral)
                    {
                        builder.Append(formatPart.Value);
                    }
                    else
                    {
                        Func<IDateTimeResultTO, DateTime, string> func;
                        if (DateTimeFormatParts.TryGetValue(formatPart.Value, out func))
                        {
                            builder.Append(func(dateTimeResultTO, tmpDateTime));
                        }
                        else
                        {
                            nothingDied = false;
                            error = string.Concat("Unrecognized format part '", formatPart.Value, "'.");
                        }
                    }

                    count++;
                }
                result = builder.ToString();
            }

            return result;
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        ///     Creates a list of all valid date time format parts
        /// </summary>
        private static void CreateDateTimeFormatParts()
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

        //2012.09.27: massimo.guerrera - Added for the new functionality for the time modification
        /// <summary>
        ///     Creates a list of all valid time modifier parts
        /// </summary>
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
            TimeModifiers.Add("Split Secs", AddSplits);
            TimeModifierTypes = new List<string>(TimeModifiers.Keys);
        }

        #region Format Methods

        private static string Format_yy(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("yy");
        }

        private static string Format_yyyy(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("yyyy");
        }

        private static string Format_mm(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("MM");
        }

        private static string Format_m(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("%M");
        }

        private static string Format_MM(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("MMMM");
        }

        private static string Format_M(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("MMM");
        }

        private static string Format_d(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("%d");
        }

        private static string Format_dd(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("dd");
        }

        private static string Format_DW(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("dddd");
        }

        private static string Format_dW(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("ddd");
        }

        private static string Format_dw(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return DateTimeParserHelper.GetDayOfWeekInt(dateTime.DayOfWeek).ToString(CultureInfo.InvariantCulture);
        }

        private static string Format_dy(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.DayOfYear.ToString(CultureInfo.InvariantCulture);
        }

        private static string Format_w(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            //27.09.2012: massimo.guerrera - Gets the week of the year according to the rule specified
            return
                CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday)
                    .ToString(CultureInfo.InvariantCulture);
        }

        private static string Format_ww(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            //27.09.2012: massimo.guerrera - Gets the week of the year according to the rule specified with a padding of 0 if needed.
            return
                CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday)
                    .ToString(CultureInfo.InvariantCulture)
                    .PadLeft(2, '0');
        }

        private static string Format_24h(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("HH");
        }

        private static string Format_12h(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("hh");
        }

        private static string Format_min(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("mm");
        }

        private static string Format_ss(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("ss");
        }

        private static string Format_sp(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            //2013.02.12: Ashley Lewis - Bug 8725, Task 8840 - The "FFF" format has a tenancy to shave trailing zeros off milliseconds
            return dateTime.Millisecond.ToString(CultureInfo.InvariantCulture);
        }

        private static string Format_am_pm(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("tt");
        }

        private static string Format_Z(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTimeResultTO.TimeZone.ShortName;
        }

        private static string Format_ZZ(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTimeResultTO.TimeZone.Name;
        }

        private static string Format_ZZZ(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTimeResultTO.TimeZone.LongName;
        }

        private static string Format_Era(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("gg");
        }

        #endregion Format Methods

        #region Time Modifier Methods

        private static DateTime AddYears(DateTime inputDateTime, int amountToAdd)
        {
            DateTime result = inputDateTime.AddYears(amountToAdd);
            return result;
        }

        private static DateTime AddMonths(DateTime inputDateTime, int amountToAdd)
        {
            DateTime result = inputDateTime.AddMonths(amountToAdd);
            return result;
        }

        private static DateTime AddDays(DateTime inputDateTime, int amountToAdd)
        {
            DateTime result = inputDateTime.AddDays(amountToAdd);
            return result;
        }

        private static DateTime AddWeeks(DateTime inputDateTime, int amountToAdd)
        {
            DateTime result = CultureInfo.InvariantCulture.Calendar.AddWeeks(inputDateTime, amountToAdd);
            return result;
        }

        private static DateTime AddHours(DateTime inputDateTime, int amountToAdd)
        {
            DateTime result = inputDateTime.AddHours(amountToAdd);
            return result;
        }

        private static DateTime AddMinutes(DateTime inputDateTime, int amountToAdd)
        {
            DateTime result = inputDateTime.AddMinutes(amountToAdd);
            return result;
        }

        private static DateTime AddSeconds(DateTime inputDateTime, int amountToAdd)
        {
            DateTime result = inputDateTime.AddSeconds(amountToAdd);
            return result;
        }

        private static DateTime AddSplits(DateTime inputDateTime, int amountToAdd)
        {
            DateTime result = inputDateTime.AddMilliseconds(amountToAdd);
            return result;
        }

        #endregion Time Modifier Methods

        #endregion Private Methods
    }
}