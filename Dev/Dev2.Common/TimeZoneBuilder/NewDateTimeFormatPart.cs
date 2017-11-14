using System.Collections.Generic;
using Dev2.Common.DateAndTime;
using Dev2.Common.DateAndTime.TO;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using System.Globalization;

namespace Dev2.Common.TimeZoneBuilder
{
    internal class NewDateTimeFormatPart : DateTimeFormatPart
    {
        public NewDateTimeFormatPart(Dictionary<string, ITimeZoneTO> timeZones) : base(timeZones)
        {
        }

        protected override void AddYearParts()
        {
            DateTimeFormatsParts.Add("yy", new DateTimeFormatPartTO("yy", false, "Year in 2 digits: 08"));
            DateTimeFormatPartOptions.Add("yy", new List<IDateTimeFormatPartOptionTO>
            {
                new DateTimeFormatPartOptionTO(2, CompareTextValueToDateTimePart.IsTextNumeric, true, null, _assignManager.AssignYears)
            });

            DateTimeFormatsParts.Add("y", new DateTimeFormatPartTO("y", false, "Year in 1 digits: 8"));
            DateTimeFormatPartOptions.Add("y", new List<IDateTimeFormatPartOptionTO>
            {
                new DateTimeFormatPartOptionTO(2, CompareTextValueToDateTimePart.IsTextNumeric, true, null, _assignManager.AssignYears),
                 new DateTimeFormatPartOptionTO(1, CompareTextValueToDateTimePart.IsTextNumeric, true, null, _assignManager.AssignYears)
            });

            DateTimeFormatsParts.Add("yyyy", new DateTimeFormatPartTO("yyyy", false, "Year in 4 digits: 2008"));
            DateTimeFormatPartOptions.Add("yyyy",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, CompareTextValueToDateTimePart.IsTextNumeric, true, null, _assignManager.AssignYears)
                });

            DateTimeFormatsParts.Add("yyy", new DateTimeFormatPartTO("yyy", false, "Year in 3 digits: 008"));
            DateTimeFormatPartOptions.Add("yyy",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(1, CompareTextValueToDateTimePart.IsTextNumeric, true, null, _assignManager.AssignYears)
                });
        }


        protected override void AddEraParts()
        {
            DateTimeFormatsParts.Add("gg", new DateTimeFormatPartTO("gg", false, "A.D."));
            DateTimeFormatPartOptions.Add("gg",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, (data, treatAsTim) => data.ToLower(CultureInfo.InvariantCulture).Equals("a.d."), false, "A.D.", _assignManager.AssignEra),
                });
        }
        protected override void AddHourParts()
        {
            DateTimeFormatsParts.Add("HH", new DateTimeFormatPartTO("HH", false, "Hours in 24 hour format: 15"));
            DateTimeFormatPartOptions.Add("HH",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumber24H, true, null, _assignManager.Assign24Hours)
                });
            DateTimeFormatsParts.Add("H", new DateTimeFormatPartTO("H", false, "Hours in 24 hour format: 15"));
            DateTimeFormatPartOptions.Add("H",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumber24H, true, null, _assignManager.Assign24Hours),
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumber24H, true, null, _assignManager.Assign24Hours),
                });
            DateTimeFormatsParts.Add("hh", new DateTimeFormatPartTO("hh", false, "Hours in 12 hour format: 15"));
            DateTimeFormatPartOptions.Add("hh",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumber12H, true, null, _assignManager.Assign12Hours)
                });
            DateTimeFormatsParts.Add("h", new DateTimeFormatPartTO("h", false, "Hours in 12 hour format: 15"));
            DateTimeFormatPartOptions.Add("h",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumber12H, true, null, _assignManager.Assign12Hours),
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumber12H, true, null, _assignManager.Assign12Hours),
                });
        }
        protected override void AddMinuteParts()
        {
            DateTimeFormatsParts.Add("m", new DateTimeFormatPartTO("m", false, "Minutes: 3"));
            DateTimeFormatPartOptions.Add("m",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMinutes, true, null, _assignManager.AssignMinutes),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberMinutes, true, null, _assignManager.AssignMinutes),
                });
            DateTimeFormatsParts.Add("mm", new DateTimeFormatPartTO("mm", false, "Minutes: 03"));
            DateTimeFormatPartOptions.Add("mm", new List<IDateTimeFormatPartOptionTO>
            {
                new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMinutes, true, null, _assignManager.AssignMinutes),
            });
        }
        protected override void AddDayParts()
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
            DateTimeFormatsParts.Add("ddd", new DateTimeFormatPartTO("ddd", false, "Month text abbreviated: Mar"));
            DateTimeFormatPartOptions.Add("ddd",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames[0].Length, CompareTextValueToDateTimePart.IsTextSunday, false, 1, _assignManager.AssignDays),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames[1].Length, CompareTextValueToDateTimePart.IsTextMonday, false, 2, _assignManager.AssignDays),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames[2].Length, CompareTextValueToDateTimePart.IsTextTuesday, false, 3,_assignManager.AssignDays),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames[3].Length, CompareTextValueToDateTimePart.IsTextWednesday, false, 4, _assignManager.AssignDays),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames[4].Length, CompareTextValueToDateTimePart.IsTextThursday, false, 5,  _assignManager.AssignDays),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames[5].Length, CompareTextValueToDateTimePart.IsTextFriday, false, 6, _assignManager.AssignDays),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames[6].Length, CompareTextValueToDateTimePart.IsTextSaturday, false, 7,_assignManager.AssignDays),

                });
            DateTimeFormatsParts.Add("dddd", new DateTimeFormatPartTO("dddd", false, "Day in full: Monday"));
            DateTimeFormatPartOptions.Add("dddd",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.DayNames[0].Length, CompareTextValueToDateTimePart.IsTextSunday, false, 1, _assignManager.AssignDays),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.DayNames[1].Length, CompareTextValueToDateTimePart.IsTextMonday, false, 2, _assignManager.AssignDays),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.DayNames[2].Length, CompareTextValueToDateTimePart.IsTextTuesday, false, 3, _assignManager.AssignDays),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.DayNames[3].Length, CompareTextValueToDateTimePart.IsTextWednesday, false, 4, _assignManager.AssignDays),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.DayNames[4].Length, CompareTextValueToDateTimePart.IsTextThursday, false, 5, _assignManager.AssignDays),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.DayNames[5].Length, CompareTextValueToDateTimePart.IsTextFriday, false, 6, _assignManager.AssignDays),
                    new DateTimeFormatPartOptionTO(CultureInfo.InvariantCulture.DateTimeFormat.DayNames[6].Length, CompareTextValueToDateTimePart.IsTextSaturday, false, 7, _assignManager.AssignDays),
                });
        }
        protected override void AddMonthParts()
        {
            DateTimeFormatsParts.Add("MM", new DateTimeFormatPartTO("MM", false, "Month in 2 digits: 03"));
            DateTimeFormatPartOptions.Add("MM",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMonth, true, null, _assignManager.AssignMonths)
                });

            DateTimeFormatsParts.Add("M", new DateTimeFormatPartTO("M", false, "Month in single digit: 3"));
            DateTimeFormatPartOptions.Add("M",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMonth, true, null, _assignManager.AssignMonths),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberMonth, true, null, _assignManager.AssignMonths),
                });

            DateTimeFormatsParts.Add("MMM", new DateTimeFormatPartTO("MMM", false, "Month text abbreviated: Mar"));
            DateTimeFormatPartOptions.Add("MMM",
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

            DateTimeFormatsParts.Add("MMMM", new DateTimeFormatPartTO("MMMM", false, "Month text in full: March"));
            DateTimeFormatPartOptions.Add("MMMM",
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
        protected override void AddOffsetParts()
        {
            DateTimeFormatsParts.Add("tt", new DateTimeFormatPartTO("tt", false, "am or pm"));
            DateTimeFormatPartOptions.Add("tt",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, CompareTextValueToDateTimePart.IsTextAmPm, false, null, _assignManager.AssignAmPm),
                    new DateTimeFormatPartOptionTO(3, CompareTextValueToDateTimePart.IsTextAmPm, false, null, _assignManager.AssignAmPm),
                    new DateTimeFormatPartOptionTO(2, CompareTextValueToDateTimePart.IsTextAmPm, false, null, _assignManager.AssignAmPm),
                });
            DateTimeFormatsParts.Add("K", new DateTimeFormatPartTO("K", false, "Time zone in short format: GMT (if available on the system)"));
            DateTimeFormatPartOptions.Add("K", new List<IDateTimeFormatPartOptionTO>
            {
             new DateTimeFormatPartOptionTO(6, CompareTextValueToDateTimePart.IsTextTimeZone, false, null, _assignManager.AssignTimeZone)
            });
        }
        protected override void AddSecondParts()
        {
            DateTimeFormatsParts.Add("ss", new DateTimeFormatPartTO("ss", false, "Seconds: 09"));
            DateTimeFormatPartOptions.Add("ss",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                });
            DateTimeFormatsParts.Add("s", new DateTimeFormatPartTO("s", false, "Seconds:  5"));
            DateTimeFormatPartOptions.Add("s",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds)
                });
            DateTimeFormatsParts.Add("F", new DateTimeFormatPartTO("F", false, "Tenths of a second"));
            DateTimeFormatPartOptions.Add("F",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds)
                });
            DateTimeFormatsParts.Add("FF", new DateTimeFormatPartTO("FF", false, "Tenths of a second"));
            DateTimeFormatPartOptions.Add("FF",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                });
            DateTimeFormatsParts.Add("FFF", new DateTimeFormatPartTO("FFF", false, "The milliseconds"));
            DateTimeFormatPartOptions.Add("FFF",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(3, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                });
            DateTimeFormatsParts.Add("FFFF", new DateTimeFormatPartTO("FFFF", false, "The milliseconds"));
            DateTimeFormatPartOptions.Add("FFFF",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(4, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(3, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                });
            DateTimeFormatsParts.Add("FFFFF", new DateTimeFormatPartTO("FFFFF", false, "Hundred thousandths of a second"));
            DateTimeFormatPartOptions.Add("FFFFF",
                new List<IDateTimeFormatPartOptionTO>
                {
                       new DateTimeFormatPartOptionTO(5, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(4, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(3, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                });
            DateTimeFormatsParts.Add("FFFFFF", new DateTimeFormatPartTO("FFFFFF", false, "Hundred thousandths of a second"));
            DateTimeFormatPartOptions.Add("FFFFFF",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(6, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(5, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(4, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(3, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                });
            DateTimeFormatsParts.Add("FFFFFFF", new DateTimeFormatPartTO("FFFFFFF", false, "Hundred thousandths of a second"));
            DateTimeFormatPartOptions.Add("FFFFFFF",
                new List<IDateTimeFormatPartOptionTO>
                {
                    new DateTimeFormatPartOptionTO(7, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(6, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(5, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(4, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(3, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                });
        }
        protected override void AddWeekParts()
        {
            //base.AddWeekParts();
        }

    }
}