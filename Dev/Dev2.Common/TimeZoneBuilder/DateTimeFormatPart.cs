using System.Collections.Generic;
using Dev2.Common.DateAndTime;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Common.TimeZoneBuilder
{
    internal  class DateTimeFormatPart: IDateTimeFormatPart
    {
        private readonly DateTimeParser _dateTimeParser;

        public DateTimeFormatPart(DateTimeParser dateTimeParser)
        {
            _dateTimeParser = dateTimeParser;
        }

        #region Implementation of IDateTimeParserBuilder
        Dictionary<string, List<IDateTimeFormatPartOptionTO>> TimeFormatPartOptions { get; set; }
        public void Build()
        {
            /*TimeFormatPartOptions = new Dictionary<string, List<IDateTimeFormatPartOptionTO>>
            {
                {
                    "yy", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, CompareTextValueToDateTimePart.IsTextNumeric, true, null, AssignYears)
                    }
                },
                {
                    "yyyy", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(4, CompareTextValueToDateTimePart.IsTextNumeric, true, null, AssignYears)
                    }
                },
                {
                    "mm", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, IsNumberMonth, true, null, AssignMonths)
                    }
                },
                {
                    "m", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, IsNumberMonth, true, null, AssignMonths),
                        new DateTimeFormatPartOptionTO(1, IsNumberMonth, true, null, AssignMonths),
                    }
                },
                {
                    "MM", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, IsNumberMonth, true, null, AssignMonths)
                    }
                },
                {
                    "M", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, IsNumberMonth, true, null, AssignMonths),
                        new DateTimeFormatPartOptionTO(1, IsNumberMonth, true, null, AssignMonths),
                    }
                },
                {
                    "d", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, IsNumberDay, true, null, AssignDays),
                        new DateTimeFormatPartOptionTO(1, IsNumberDay, true, null, AssignDays)
                    }
                },
                {
                    "dd", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, IsNumberDay, true, null, AssignDays)
                    }
                },
                {
                    "DW", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(1, IsNumberDayOfWeek, true, null, AssignDaysOfWeek),
                    }
                },
                {
                    "dW", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(1, IsNumberDayOfWeek, true, null, AssignDaysOfWeek),
                    }
                },
                {
                    "dw", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(1, IsNumberDayOfWeek, true, null, AssignDaysOfWeek),
                    }
                },
                {
                    "dy", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(3, IsNumberDayOfYear, true, null, AssignDaysOfYear),
                        new DateTimeFormatPartOptionTO(2, IsNumberDayOfYear, true, null, AssignDaysOfYear),
                        new DateTimeFormatPartOptionTO(1, IsNumberDayOfYear, true, null, AssignDaysOfYear)
                    }
                },
                {
                    "w", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, IsNumberWeekOfYear, true, null, AssignWeeks),
                        new DateTimeFormatPartOptionTO(1, IsNumberWeekOfYear, true, null, AssignWeeks),
                    }
                },
                {
                    "24h", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, IsNumber24H, true, null, Assign24Hours)
                    }
                },
                {
                    "12h", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, IsNumber12H, true, null, Assign12Hours),
                        new DateTimeFormatPartOptionTO(1, IsNumber12H, true, null, Assign12Hours),
                    }
                },
                {
                    "min", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, IsNumberMinutes, true, null, AssignMinutes),
                        new DateTimeFormatPartOptionTO(1, IsNumberMinutes, true, null, AssignMinutes),
                    }
                },
                {
                    "ss", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, IsNumberSeconds, true, null, AssignSeconds),
                        new DateTimeFormatPartOptionTO(1, IsNumberSeconds, true, null, AssignSeconds),
                    }
                },
                {
                    "sp", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(3, IsNumberMilliseconds, true, null, AssignMilliseconds),
                        new DateTimeFormatPartOptionTO(2, IsNumberMilliseconds, true, null, AssignMilliseconds),
                        new DateTimeFormatPartOptionTO(1, IsNumberMilliseconds, true, null, AssignMilliseconds),
                    }
                },
                {
                    "am/pm", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(4, CompareTextValueToDateTimePart.IsTextAmPm, false, null, AssignAmPm),
                        new DateTimeFormatPartOptionTO(3, CompareTextValueToDateTimePart.IsTextAmPm, false, null, AssignAmPm),
                        new DateTimeFormatPartOptionTO(2, CompareTextValueToDateTimePart.IsTextAmPm, false, null, AssignAmPm),
                    }
                },
                {
                    "Z", _dateTimeParser.TimeZones.Select(k =>
                    {
                        IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length, CompareTextValueToDateTimePart.IsTextTimeZone, false, null, AssignTimeZone);
                        return dateTimeFormatPartOptionTo;
                    }).OrderByDescending(k => k.Length).ToList()
                },
                {
                    "ZZ", _dateTimeParser.TimeZones.Select(k =>
                    {
                        IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length, CompareTextValueToDateTimePart.IsTextTimeZone, false, null, AssignTimeZone);
                        return dateTimeFormatPartOptionTo;
                    }).OrderByDescending(k => k.Length).ToList()
                },
                {
                    "ZZZ", _dateTimeParser.TimeZones.Select(k =>
                    {
                        IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length, CompareTextValueToDateTimePart.IsTextTimeZone, false, null, AssignTimeZone);
                        return dateTimeFormatPartOptionTo;
                    }).OrderByDescending(k => k.Length).ToList()
                },
                {
                    "era", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, (data, treatAsTim) => data.ToLower().Equals("ad"), false, "AD",
                            AssignEra)
                    }
                },
                {
                    "Era", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(3, (data, treatAsTim) => data.ToLower().Equals("a.d"), false, "A.D",
                            AssignEra)
                    }
                },
                {
                    "ERA", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(4, (data, treatAsTim) => data.ToLower().Equals("a.d."), false, "A.D.",
                            AssignEra)
                    }
                
            };*/
























        }

        #endregion
    }
}