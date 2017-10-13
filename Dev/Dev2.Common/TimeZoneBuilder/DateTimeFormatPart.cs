using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Common.DateAndTime;
using Dev2.Common.DateAndTime.TO;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Common.TimeZoneBuilder
{
    internal class DateTimeFormatPart : IDateTimeFormatPart
    {
        private readonly Dictionary<string, ITimeZoneTO> _timeZones;

        public Dictionary<string, IDateTimeFormatPartTO> DateTimeFormatsParts { get; set; }
        public Dictionary<string, List<IDateTimeFormatPartOptionTO>> DateTimeFormatPartOptions { get; set; }
        private static AssignManager _assignManager;
        private static readonly IDatetimeParserHelper DatetimeParserHelper = new DateTimeParserHelper();
        public DateTimeFormatPart(Dictionary<string, ITimeZoneTO> timeZones)
        {
            _timeZones = timeZones;
            DateTimeFormatsParts = new Dictionary<string, IDateTimeFormatPartTO>();
            DateTimeFormatPartOptions = new Dictionary<string, List<IDateTimeFormatPartOptionTO>>();
            _assignManager = new AssignManager(_timeZones);
        }
        #region Implementation of IDateTimeParserBuilder

        public void Build()
        {
            AddYearParts();
            AddMonthParts();
            AddDayParts();
            AddWeekParts();
            AddHourParts();
            AddMinuteParts();
            AddSecondParts();
            AddOffsetParts();
            AddEraParts();
        }

        #endregion

        #region Private Logic
        private void AddYearParts()
        {
            DateTimeFormatsParts.Add("yy", new DateTimeFormatPartTO("yy", false, "Year in 2 digits: 08"));
            DateTimeFormatPartOptions.Add("yy", new List<IDateTimeFormatPartOptionTO>
            {
                new DateTimeFormatPartOptionTO(2, CompareTextValueToDateTimePart.IsTextNumeric, true, null, _assignManager.AssignYears)
            });

            DateTimeFormatsParts.Add("yyyy", new DateTimeFormatPartTO("yyyy", false, "Year in 4 digits: 2008"));
            DateTimeFormatPartOptions.Add("yyyy",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, CompareTextValueToDateTimePart.IsTextNumeric, true, null, _assignManager.AssignYears)
                });
        }

        private void AddMonthParts()
        {
            DateTimeFormatsParts.Add("mm", new DateTimeFormatPartTO("mm", false, "Month in 2 digits: 03"));
            DateTimeFormatPartOptions.Add("mm",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMonth, true, null, _assignManager.AssignMonths)
                });

            DateTimeFormatsParts.Add("m", new DateTimeFormatPartTO("m", false, "Month in single digit: 3"));
            DateTimeFormatPartOptions.Add("m",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMonth, true, null, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberMonth, true, null, _assignManager.AssignMonths),
                });

            DateTimeFormatsParts.Add("M", new DateTimeFormatPartTO("M", false, "Month text abbreviated: Mar"));
            DateTimeFormatPartOptions.Add("M",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames[0].Length, CompareTextValueToDateTimePart.IsTextJanuary, false,
                        1, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames[1].Length, CompareTextValueToDateTimePart.IsTextFebuary, false,
                        2, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames[2].Length, CompareTextValueToDateTimePart.IsTextMarch, false, 3,
                        _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames[3].Length, CompareTextValueToDateTimePart.IsTextApril, false, 4,
                        _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames[4].Length, CompareTextValueToDateTimePart.IsTextMay, false, 5,
                        _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames[5].Length, CompareTextValueToDateTimePart.IsTextJune, false, 6,
                        _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames[6].Length, CompareTextValueToDateTimePart.IsTextJuly, false, 7,
                        _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames[7].Length, CompareTextValueToDateTimePart.IsTextAugust, false,
                        8, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames[8].Length, CompareTextValueToDateTimePart.IsTextSeptember,
                        false, 9, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames[9].Length, CompareTextValueToDateTimePart.IsTextOctober, false,
                        10, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames[10].Length, CompareTextValueToDateTimePart.IsTextNovember,
                        false, 11, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames[11].Length, CompareTextValueToDateTimePart.IsTextDecember,
                        false, 12, _assignManager.AssignMonths),
                });

            DateTimeFormatsParts.Add("MM", new DateTimeFormatPartTO("MM", false, "Month text in full: March"));
            DateTimeFormatPartOptions.Add("MM",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.MonthNames[0].Length, CompareTextValueToDateTimePart.IsTextJanuary, false, 1, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.MonthNames[1].Length, CompareTextValueToDateTimePart.IsTextFebuary, false, 2, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.MonthNames[2].Length, CompareTextValueToDateTimePart.IsTextMarch, false, 3, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.MonthNames[3].Length, CompareTextValueToDateTimePart.IsTextApril, false, 4, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.MonthNames[4].Length, CompareTextValueToDateTimePart.IsTextMay, false, 5, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.MonthNames[5].Length, CompareTextValueToDateTimePart.IsTextJune, false, 6, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.MonthNames[6].Length, CompareTextValueToDateTimePart.IsTextJuly, false, 7, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.MonthNames[7].Length, CompareTextValueToDateTimePart.IsTextAugust, false, 8, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.MonthNames[8].Length, CompareTextValueToDateTimePart.IsTextSeptember, false, 9, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.MonthNames[9].Length, CompareTextValueToDateTimePart.IsTextOctober, false, 10, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.MonthNames[10].Length, CompareTextValueToDateTimePart.IsTextNovember, false, 11, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.MonthNames[11].Length, CompareTextValueToDateTimePart.IsTextDecember, false, 12, _assignManager.AssignMonths),
                });
        }

        private void AddDayParts()
        {
            DateTimeFormatsParts.Add("d", new DateTimeFormatPartTO("d", false, "Day of month in single digit: 6"));
            DateTimeFormatPartOptions.Add("d",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberDay, true, null, _assignManager.AssignDays),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberDay, true, null, _assignManager.AssignDays)
                });

            DateTimeFormatsParts.Add("dd", new DateTimeFormatPartTO("dd", false, "Day of month in 2 digits: 06"));
            DateTimeFormatPartOptions.Add("dd",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberDay, true, null, _assignManager.AssignDays)
                });

            DateTimeFormatsParts.Add("DW", new DateTimeFormatPartTO("DW", false, "Day of Week in full: Thursday"));
            DateTimeFormatPartOptions.Add("DW",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.DayNames[0].Length, CompareTextValueToDateTimePart.IsTextSunday, false, 7, _assignManager.AssignDaysOfWeek),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.DayNames[1].Length, CompareTextValueToDateTimePart.IsTextMonday, false, 1, _assignManager.AssignDaysOfWeek),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.DayNames[2].Length, CompareTextValueToDateTimePart.IsTextTuesday, false, 2, _assignManager.AssignDaysOfWeek),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.DayNames[3].Length, CompareTextValueToDateTimePart.IsTextWednesday, false, 3, _assignManager.AssignDaysOfWeek),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.DayNames[4].Length, CompareTextValueToDateTimePart.IsTextThursday, false, 4, _assignManager.AssignDaysOfWeek),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.DayNames[5].Length, CompareTextValueToDateTimePart.IsTextFriday, false, 5, _assignManager.AssignDaysOfWeek),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.DayNames[6].Length, CompareTextValueToDateTimePart.IsTextSaturday, false, 6, _assignManager.AssignDaysOfWeek),
                });

            DateTimeFormatsParts.Add("dW", new DateTimeFormatPartTO("dW", false, "Day of Week text abbreviated: Thu"));
            DateTimeFormatPartOptions.Add("dW",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames[0].Length, CompareTextValueToDateTimePart.IsTextSunday, false, 7,
                        _assignManager.AssignDaysOfWeek, 6),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames[1].Length, CompareTextValueToDateTimePart.IsTextMonday, false, 1,
                        _assignManager.AssignDaysOfWeek, 6),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames[2].Length, CompareTextValueToDateTimePart.IsTextTuesday, false, 2,
                        _assignManager.AssignDaysOfWeek, 7),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames[3].Length, CompareTextValueToDateTimePart.IsTextWednesday, false,
                        3, _assignManager.AssignDaysOfWeek, 9),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames[4].Length, CompareTextValueToDateTimePart.IsTextThursday, false,
                        4, _assignManager.AssignDaysOfWeek, 8),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames[5].Length, CompareTextValueToDateTimePart.IsTextFriday, false, 5,
                        _assignManager.AssignDaysOfWeek, 6),
                    new DateTimeFormatPartOptionTO(
                        CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames[6].Length, CompareTextValueToDateTimePart.IsTextSaturday, false,
                        6, _assignManager.AssignDaysOfWeek, 7),
                });

            DateTimeFormatsParts.Add("dw", new DateTimeFormatPartTO("dw", false, "Day of Week number: 4"));
            DateTimeFormatPartOptions.Add("dw",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberDayOfWeek, true, null, _assignManager.AssignDaysOfWeek),
                });

            DateTimeFormatsParts.Add("dy", new DateTimeFormatPartTO("dy", false, "Day of year: 66"));
            DateTimeFormatPartOptions.Add("dy",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(3, DatetimeParserHelper.IsNumberDayOfYear, true, null, _assignManager.AssignDaysOfYear),
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberDayOfYear, true, null, _assignManager.AssignDaysOfYear),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberDayOfYear, true, null, _assignManager.AssignDaysOfYear)
                });
        }

        private void AddWeekParts()
        {
            DateTimeFormatsParts.Add("ww", new DateTimeFormatPartTO("ww", false, "Week of year: 09"));
            DateTimeFormatPartOptions.Add("ww",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberWeekOfYear, true, null, _assignManager.AssignWeeks),
                });

            DateTimeFormatsParts.Add("w", new DateTimeFormatPartTO("w", false, "Week of year: 9"));
            DateTimeFormatPartOptions.Add("w",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberWeekOfYear, true, null, _assignManager.AssignWeeks),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberWeekOfYear, true, null, _assignManager.AssignWeeks),
                });
        }

        private void AddHourParts()
        {
            DateTimeFormatsParts.Add("24h", new DateTimeFormatPartTO("24h", false, "Hours in 24 hour format: 15"));
            DateTimeFormatPartOptions.Add("24h",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumber24H, true, null, _assignManager.Assign24Hours)
                });

            DateTimeFormatsParts.Add("12h", new DateTimeFormatPartTO("12h", false, "Hours in 12 hour format: 3"));
            DateTimeFormatPartOptions.Add("12h",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumber12H, true, null, _assignManager.Assign12Hours),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumber12H, true, null, _assignManager.Assign12Hours),
                });
        }

        private void AddMinuteParts()
        {
            DateTimeFormatsParts.Add("min", new DateTimeFormatPartTO("min", false, "Minutes: 30"));
            DateTimeFormatPartOptions.Add("min",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMinutes, true, null, _assignManager.AssignMinutes),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberMinutes, true, null, _assignManager.AssignMinutes),
                });
        }
        private void AddSecondParts()
        {
            DateTimeFormatsParts.Add("ss", new DateTimeFormatPartTO("ss", false, "Seconds: 29"));
            DateTimeFormatPartOptions.Add("ss",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberSeconds, true, null,  _assignManager.AssignSeconds),
                });

            DateTimeFormatsParts.Add("sp", new DateTimeFormatPartTO("sp", false, "Split Seconds: 987"));
            DateTimeFormatPartOptions.Add("sp",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(3, DatetimeParserHelper.IsNumberMilliseconds, true, null, _assignManager. AssignMilliseconds),
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMilliseconds, true, null, _assignManager. AssignMilliseconds),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberMilliseconds, true, null, _assignManager. AssignMilliseconds),
                });
        }

        private void AddOffsetParts()
        {
            DateTimeFormatsParts.Add("am/pm", new DateTimeFormatPartTO("am/pm", false, "am or pm"));
            DateTimeFormatPartOptions.Add("am/pm",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, CompareTextValueToDateTimePart.IsTextAmPm, false, null, _assignManager.AssignAmPm),
                    new DateTimeFormatPartOptionTO(3, CompareTextValueToDateTimePart.IsTextAmPm, false, null, _assignManager.AssignAmPm),
                    new DateTimeFormatPartOptionTO(2, CompareTextValueToDateTimePart.IsTextAmPm, false, null, _assignManager.AssignAmPm),
                });

            DateTimeFormatsParts.Add("Z",
                new DateTimeFormatPartTO("Z", false, "Time zone in short format: GMT (if available on the system)"));
            DateTimeFormatPartOptions.Add("Z", _timeZones.Select(k =>
            {
                IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length, CompareTextValueToDateTimePart.IsTextTimeZone, false, null, _assignManager.AssignTimeZone);
                return dateTimeFormatPartOptionTo;
            }).OrderByDescending(k => k.Length).ToList());

            DateTimeFormatsParts.Add("ZZ", new DateTimeFormatPartTO("ZZ", false, "Time zone: Grenwich mean time"));
            DateTimeFormatPartOptions.Add("ZZ", _timeZones.Select(k =>
            {
                IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length, CompareTextValueToDateTimePart.IsTextTimeZone, false, null, _assignManager.AssignTimeZone);
                return dateTimeFormatPartOptionTo;
            }).OrderByDescending(k => k.Length).ToList());

            DateTimeFormatsParts.Add("ZZZ", new DateTimeFormatPartTO("ZZZ", false, "Time zone offset: GMT + 02:00"));
            DateTimeFormatPartOptions.Add("ZZZ", _timeZones.Select(k =>
            {
                IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length, CompareTextValueToDateTimePart.IsTextTimeZone, false, null, _assignManager.AssignTimeZone);
                return dateTimeFormatPartOptionTo;
            }).OrderByDescending(k => k.Length).ToList());
        }
        private void AddEraParts()
        {
            DateTimeFormatsParts.Add("Era", new DateTimeFormatPartTO("Era", false, "A.D."));

            DateTimeFormatPartOptions.Add("era",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, (data, treatAsTim) => data.ToLower().Equals("a.d."), false, "A.D.",
                        _assignManager.AssignEra),
                    new DateTimeFormatPartOptionTO(3, (data, treatAsTim) => data.ToLower().Equals("a.d"), false, "A.D",
                        _assignManager.AssignEra),
                    new DateTimeFormatPartOptionTO(2, (data, treatAsTim) => data.ToLower().Equals("ad"), false, "AD",
                        _assignManager.AssignEra)
                });

            DateTimeFormatPartOptions.Add("Era",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, (data, treatAsTim) => data.ToLower().Equals("a.d."), false, "A.D.",
                        _assignManager.AssignEra),
                    new DateTimeFormatPartOptionTO(3, (data, treatAsTim) => data.ToLower().Equals("a.d"), false, "A.D",
                        _assignManager.AssignEra),
                    new DateTimeFormatPartOptionTO(2, (data, treatAsTim) => data.ToLower().Equals("ad"), false, "AD",
                        _assignManager.AssignEra)
                });

            DateTimeFormatPartOptions.Add("ERA",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, (data, treatAsTim) => data.ToLower().Equals("a.d."), false, "A.D.",
                        _assignManager.AssignEra),
                    new DateTimeFormatPartOptionTO(3, (data, treatAsTim) => data.ToLower().Equals("a.d"), false, "A.D",
                        _assignManager.AssignEra),
                    new DateTimeFormatPartOptionTO(2, (data, treatAsTim) => data.ToLower().Equals("ad"), false, "AD",
                        _assignManager. AssignEra)
                });
        }
        #endregion

    }
}