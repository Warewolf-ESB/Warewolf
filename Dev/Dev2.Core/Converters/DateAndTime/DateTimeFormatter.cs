/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Converters.DateAndTime
{
    public class DateTimeFormatter : IDateTimeFormatter
    {
        #region Class Members

        private static readonly Dictionary<string, Func<IDateTimeResultTO, DateTime, string>> _dateTimeFormatParts =
            new Dictionary<string, Func<IDateTimeResultTO, DateTime, string>>();

        //27.09.2012: massimo.guerrera - Added for the new way of doing time modification
        private static readonly Dictionary<string, Func<DateTime, int, DateTime>> _timeModifiers =
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

        /// <summary>
        ///     Converts a date from one format to another. If a valid time modifier is specified then the date is adjusted
        ///     accordingly before being returned.
        /// </summary>
        public bool TryFormat(IDateTimeOperationTO dateTimeTO, out string result, out string error)
        {
            result = "";
            IDateTimeParser dateTimeParser = DateTimeConverterFactory.CreateParser();

            bool nothingDied = true;
            IDateTimeResultTO dateTimeResultTO;

            //2013.05.06: Ashley Lewis - Bug 9300 - trim should allow null input format
            dateTimeTO.InputFormat = dateTimeTO.InputFormat != null ? dateTimeTO.InputFormat.Trim() : null;

            //2013.02.12: Ashley Lewis - Bug 8725, Task 8840 - Added trim to data
            if (dateTimeParser.TryParseDateTime(dateTimeTO.DateTime.Trim(), dateTimeTO.InputFormat, out dateTimeResultTO,
                out error))
            {
                //
                // Parse time, if present
                //
                DateTime tmpDateTime = dateTimeResultTO.ToDateTime();
                if (!string.IsNullOrWhiteSpace(dateTimeTO.TimeModifierType))
                {
                    //2012.09.27: massimo.guerrera - Added for the new functionality for the time modification
                    Func<DateTime, int, DateTime> funcToExecute;
                    if (_timeModifiers.TryGetValue(dateTimeTO.TimeModifierType, out funcToExecute) &&
                        funcToExecute != null)
                    {
                        tmpDateTime = funcToExecute(tmpDateTime, dateTimeTO.TimeModifierAmount);
                    }
                }

                //
                // If nothing has gone wrong yet
                //
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                if (nothingDied)
                    // ReSharper restore ConditionIsAlwaysTrueOrFalse
                {
                    //
                    // If there is no output format use the input format
                    //
                    string outputFormat = (string.IsNullOrWhiteSpace(dateTimeTO.OutputFormat))
                        ? dateTimeTO.InputFormat
                        : dateTimeTO.OutputFormat;
                    if (string.IsNullOrWhiteSpace(outputFormat))
                    {
                        //07.03.2013: Ashley Lewis - Bug 9167 null to default
                        outputFormat =
                            dateTimeParser.TranslateDotNetToDev2Format(GlobalConstants.Dev2DotNetDefaultDateTimeFormat,
                                out error);
                    }

                    //
                    // Format to output format
                    //
                    List<IDateTimeFormatPartTO> formatParts;

                    //
                    // Get output format parts
                    //
                    nothingDied = DateTimeParser.TryGetDateTimeFormatParts(outputFormat, out formatParts, out error);

                    if (nothingDied)
                    {
                        int count = 0;
                        while (count < formatParts.Count && nothingDied)
                        {
                            IDateTimeFormatPartTO formatPart = formatParts[count];

                            if (formatPart.Isliteral)
                            {
                                result += formatPart.Value;
                            }
                            else
                            {
                                Func<IDateTimeResultTO, DateTime, string> func;
                                if (_dateTimeFormatParts.TryGetValue(formatPart.Value, out func))
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
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        ///     Creates a list of all valid date time format parts
        /// </summary>
        private static void CreateDateTimeFormatParts()
        {
            _dateTimeFormatParts.Add("yy", Formatyy);
            _dateTimeFormatParts.Add("yyyy", Formatyyyy);
            _dateTimeFormatParts.Add("mm", Formatmm);
            _dateTimeFormatParts.Add("m", Formatm);
            _dateTimeFormatParts.Add("M", FormatM);
            _dateTimeFormatParts.Add("MM", FormatMm);
            _dateTimeFormatParts.Add("d", Formatd);
            _dateTimeFormatParts.Add("dd", Formatdd);
            _dateTimeFormatParts.Add("DW", FormatDw);
            _dateTimeFormatParts.Add("dW", FormatdW);
            _dateTimeFormatParts.Add("dw", Formatdw);
            _dateTimeFormatParts.Add("dy", Formatdy);
            _dateTimeFormatParts.Add("w", Formatw);
            _dateTimeFormatParts.Add("ww", Formatww);
            _dateTimeFormatParts.Add("24h", Format24H);
            _dateTimeFormatParts.Add("12h", Format12H);
            _dateTimeFormatParts.Add("min", Formatmin);
            _dateTimeFormatParts.Add("ss", Formatss);
            _dateTimeFormatParts.Add("sp", Formatsp);
            _dateTimeFormatParts.Add("am/pm", Formatampm);
            _dateTimeFormatParts.Add("Z", FormatZ);
            _dateTimeFormatParts.Add("ZZ", FormatZz);
            _dateTimeFormatParts.Add("ZZZ", FormatZzz);
            _dateTimeFormatParts.Add("Era", FormatEra);
        }

        //2012.09.27: massimo.guerrera - Added for the new functionality for the time modification
        /// <summary>
        ///     Creates a list of all valid time modifier parts
        /// </summary>
        private static void CreateTimeModifierTypes()
        {
            _timeModifiers.Add("", null);
            _timeModifiers.Add("Years", AddYears);
            _timeModifiers.Add("Months", AddMonths);
            _timeModifiers.Add("Days", AddDays);
            _timeModifiers.Add("Weeks", AddWeeks);
            _timeModifiers.Add("Hours", AddHours);
            _timeModifiers.Add("Minutes", AddMinutes);
            _timeModifiers.Add("Seconds", AddSeconds);
            _timeModifiers.Add("Split Secs", AddSplits);
            TimeModifierTypes = new List<string>(_timeModifiers.Keys);
        }

        #region Format Methods

        private static string Formatyy(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("yy");
        }

        private static string Formatyyyy(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("yyyy");
        }

        private static string Formatmm(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("MM");
        }

        private static string Formatm(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("%M");
        }

        private static string FormatMm(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("MMMM");
        }

        private static string FormatM(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("MMM");
        }

        private static string Formatd(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("%d");
        }

        private static string Formatdd(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("dd");
        }

        private static string FormatDw(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("dddd");
        }

        private static string FormatdW(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("ddd");
        }

        private static string Formatdw(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return DateTimeParser.GetDayOfWeekInt(dateTime.DayOfWeek).ToString(CultureInfo.InvariantCulture);
        }

        private static string Formatdy(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.DayOfYear.ToString(CultureInfo.InvariantCulture);
        }

        private static string Formatw(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            //27.09.2012: massimo.guerrera - Gets the week of the year according to the rule specified
            return
                CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday)
                    .ToString(CultureInfo.InvariantCulture);
        }

        private static string Formatww(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            //27.09.2012: massimo.guerrera - Gets the week of the year according to the rule specified with a padding of 0 if needed.
            return
                CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday)
                    .ToString(CultureInfo.InvariantCulture)
                    .PadLeft(2, '0');
        }

        private static string Format24H(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("HH");
        }

        private static string Format12H(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("hh");
        }

        private static string Formatmin(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("mm");
        }

        private static string Formatss(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("ss");
        }

        private static string Formatsp(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            //2013.02.12: Ashley Lewis - Bug 8725, Task 8840 - The "FFF" format has a tenancy to shave trailing zeros off milliseconds
            return dateTime.Millisecond.ToString(CultureInfo.InvariantCulture);
        }

        private static string Formatampm(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTime.ToString("tt");
        }

        private static string FormatZ(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTimeResultTO.TimeZone.ShortName;
        }

        private static string FormatZz(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTimeResultTO.TimeZone.Name;
        }

        private static string FormatZzz(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
        {
            return dateTimeResultTO.TimeZone.LongName;
        }

        private static string FormatEra(IDateTimeResultTO dateTimeResultTO, DateTime dateTime)
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