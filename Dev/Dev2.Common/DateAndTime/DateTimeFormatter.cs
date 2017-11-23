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
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Dev2.Common.DateAndTime
{
    public class DateTimeFormatter : IDateTimeFormatter
    {
        static readonly Dictionary<string, Func<IDateTimeResultTO, DateTime, string>> DateTimeFormatParts =
            new Dictionary<string, Func<IDateTimeResultTO, DateTime, string>>();

        static readonly Dictionary<string, Func<DateTime, int, DateTime>> TimeModifiers =
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
        
        public bool TryFormat(IDateTimeOperationTO dateTimeTO, out string result, out string error)
        {            
            var outPutHasDev2Formating = DateTimeParserHelper.DateIsDev2DateFormat(dateTimeTO.OutputFormat);
            var inPutHasDev2Formating = DateTimeParserHelper.DateIsDev2DateFormat(dateTimeTO.InputFormat);
            var nothingDied = true;
            dateTimeTO.InputFormat = dateTimeTO.InputFormat ?? GlobalConstants.Dev2DotNetDefaultDateTimeFormat;
            dateTimeTO.OutputFormat = string.IsNullOrWhiteSpace(dateTimeTO.OutputFormat) ? dateTimeTO.InputFormat : dateTimeTO.OutputFormat;
            if (inPutHasDev2Formating || outPutHasDev2Formating)
            {
                result = "";
                var dateTimeParser = DateTimeConverterFactory.CreateParser();
                dateTimeTO.InputFormat = dateTimeTO.InputFormat?.Trim();
                if (dateTimeParser.TryParseDateTime(dateTimeTO.DateTime.Trim(), dateTimeTO.InputFormat, out IDateTimeResultTO dateTimeResultTO, out error))
                {
                    var tmpDateTime = dateTimeResultTO.ToDateTime();
                    tmpDateTime = PerformDateTimeModification(dateTimeTO, tmpDateTime);

                    if (nothingDied)
                    {
                        var outputFormat = BuildOutputFormat(dateTimeTO, ref error, dateTimeParser);
                        nothingDied = DateTimeParser.TryGetDateTimeFormatParts(outputFormat, out List<IDateTimeFormatPartTO> formatParts, out error);
                        BuildResult(ref result, ref error, ref nothingDied, dateTimeResultTO, tmpDateTime, formatParts);
                    }
                }
                else
                {
                    nothingDied = false;
                }

                return nothingDied;
            }
            try
            {
                return PerfomaStandardDateFormat(dateTimeTO, out result, out error);

            }
            catch (Exception ex)
            {
                result = "";
                error = ex.Message;
                nothingDied = false;
            }

            return nothingDied;
        }

        static string BuildOutputFormat(IDateTimeOperationTO dateTimeTO, ref string error, IDateTimeParser dateTimeParser)
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

            return outputFormat;
        }

        static void BuildResult(ref string result, ref string error,  ref bool nothingDied, IDateTimeResultTO dateTimeResultTO, DateTime tmpDateTime, List<IDateTimeFormatPartTO> formatParts)
        {
            
            if (nothingDied)
            {
                var stringbuilder = new StringBuilder();
                var count = 0;
                while (count < formatParts.Count && nothingDied)
                {
                    var formatPart = formatParts[count];
                    if (formatPart.Isliteral)
                    {
                        stringbuilder.Append(formatPart.Value);
                    }
                    else
                    {
                        Func<IDateTimeResultTO, DateTime, string> func;
                        if (DateTimeFormatParts.TryGetValue(formatPart.Value, out func))
                        {
                            stringbuilder.Append(func(dateTimeResultTO, tmpDateTime));
                        }
                        else
                        {
                            nothingDied = false;
                            error = string.Concat("Unrecognized format part '", formatPart.Value, "'.");
                        }
                    }

                    count++;
                }
                result = stringbuilder.ToString();
            }
        }

        static bool PerfomaStandardDateFormat(IDateTimeOperationTO dateTimeTO, out string result, out string error)
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

        static DateTime PerformDateTimeModification(IDateTimeOperationTO dateTimeTO, DateTime tmpDateTime)
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

        /// <summary>
        ///     Creates a list of all valid date time format parts
        /// </summary>
        static void CreateDateTimeFormatParts()
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

        static void CreateTimeModifierTypes()
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

        static string Format_yy(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("yy");
        }

        static string Format_yyyy(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("yyyy");
        }

        static string Format_mm(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("MM");
        }

        static string Format_m(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("%M");
        }

        static string Format_MM(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("MMMM");
        }

        static string Format_M(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("MMM");
        }

        static string Format_d(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("%d");
        }

        static string Format_dd(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("dd");
        }

        static string Format_DW(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("dddd");
        }

        static string Format_dW(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("ddd");
        }

        static string Format_dw(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return DateTimeParserHelper.GetDayOfWeekInt(dateTime.DayOfWeek).ToString(CultureInfo.InvariantCulture);
        }

        static string Format_dy(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.DayOfYear.ToString(CultureInfo.InvariantCulture);
        }

        static string Format_w(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            //27.09.2012: massimo.guerrera - Gets the week of the year according to the rule specified
            return
                CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday)
                    .ToString(CultureInfo.InvariantCulture);
        }

        static string Format_ww(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            //27.09.2012: massimo.guerrera - Gets the week of the year according to the rule specified with a padding of 0 if needed.
            return
                CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday)
                    .ToString(CultureInfo.InvariantCulture)
                    .PadLeft(2, '0');
        }

        static string Format_24h(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("HH");
        }

        static string Format_12h(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("hh");
        }

        static string Format_min(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("mm");
        }

        static string Format_ss(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("ss");
        }

        static string Format_sp(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.Millisecond.ToString(CultureInfo.InvariantCulture);
        }

        static string Format_am_pm(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("tt");
        }

        static string Format_Z(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTimeResultTO.TimeZone.ShortName;
        }

        static string Format_ZZ(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTimeResultTO.TimeZone.Name;
        }

        static string Format_ZZZ(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTimeResultTO.TimeZone.LongName;
        }

        static string Format_Era(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("gg");
        }

        static DateTime AddYears(DateTime inputDateTime, int amountToAdd)
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

        static DateTime AddSplits(DateTime inputDateTime, int amountToAdd)
        {
            var result = inputDateTime.AddMilliseconds(amountToAdd);
            return result;
        }
    }
}