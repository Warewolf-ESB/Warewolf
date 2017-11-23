using System.Collections.Generic;
using System.Linq;
using Dev2.Common.DateAndTime;
using Dev2.Common.DateAndTime.TO;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Common.TimeZoneBuilder
{
    internal class TimeFormatPartBuilder : ITimeFormatPartBuilder
    {
        private readonly Dictionary<string, ITimeZoneTO> _timeZones;
        private readonly AssignManager _assignManager;
        private static readonly IDatetimeParserHelper DatetimeParserHelper = new DateTimeParserHelper();
        public TimeFormatPartBuilder(Dictionary<string, ITimeZoneTO> timeZones)
        {
            _timeZones = timeZones;
            _assignManager = new AssignManager(_timeZones);
        }

       public Dictionary<string, List<IDateTimeFormatPartOptionTO>> TimeFormatPartOptions { get; set; }
        public void Build()
        {
            TimeFormatPartOptions = new Dictionary<string, List<IDateTimeFormatPartOptionTO>>
            {
                {
                    "yy", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, CompareTextValueToDateTimePart.IsTextNumeric, true, null, _assignManager.AssignYears)
                    }
                },
                {
                    "yyyy", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(4, CompareTextValueToDateTimePart.IsTextNumeric, true, null, _assignManager.AssignYears)
                    }
                },
                {
                    "mm", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMonth, true, null, _assignManager.AssignMonths)
                    }
                },
                {
                    "m", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMonth, true, null, _assignManager.AssignMonths),
                        new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberMonth, true, null, _assignManager.AssignMonths),
                    }
                },
                {
                    "MM", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMonth, true, null, _assignManager.AssignMonths)
                    }
                },
                {
                    "M", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMonth, true, null, _assignManager.AssignMonths),
                        new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberMonth, true, null, _assignManager.AssignMonths),
                    }
                },
                {
                    "d", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberDay, true, null, _assignManager.AssignDays),
                        new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberDay, true, null, _assignManager.AssignDays)
                    }
                },
                {
                    "dd", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberDay, true, null, _assignManager.AssignDays)
                    }
                },
                {
                    "DW", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberDayOfWeek, true, null, _assignManager.AssignDaysOfWeek),
                    }
                },
                {
                    "dW", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberDayOfWeek, true, null, _assignManager.AssignDaysOfWeek),
                    }
                },
                {
                    "dw", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberDayOfWeek, true, null, _assignManager.AssignDaysOfWeek),
                    }
                },
                {
                    "dy", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(3, DatetimeParserHelper.IsNumberDayOfYear, true, null, _assignManager.AssignDaysOfYear),
                        new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberDayOfYear, true, null, _assignManager.AssignDaysOfYear),
                        new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberDayOfYear, true, null, _assignManager.AssignDaysOfYear)
                    }
                },
                {
                    "w", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberWeekOfYear, true, null, _assignManager.AssignWeeks),
                        new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberWeekOfYear, true, null, _assignManager.AssignWeeks),
                    }
                },
                {
                    "24h", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumber24H, true, null, _assignManager.Assign24Hours)
                    }
                },
                {
                    "12h", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumber12H, true, null, _assignManager.Assign12Hours),
                        new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumber12H, true, null, _assignManager.Assign12Hours),
                    }
                },
                {
                    "min", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMinutes, true, null, _assignManager.AssignMinutes),
                        new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberMinutes, true, null, _assignManager.AssignMinutes),
                    }
                },
                {
                    "ss", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                        new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberSeconds, true, null, _assignManager.AssignSeconds),
                    }
                },
                {
                    "sp", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(3, DatetimeParserHelper.IsNumberMilliseconds, true, null, _assignManager.AssignMilliseconds),
                        new DateTimeFormatPartOptionTO(2, DatetimeParserHelper.IsNumberMilliseconds, true, null, _assignManager.AssignMilliseconds),
                        new DateTimeFormatPartOptionTO(1, DatetimeParserHelper.IsNumberMilliseconds, true, null, _assignManager.AssignMilliseconds),
                    }
                },
                {
                    "am/pm", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(4, CompareTextValueToDateTimePart.IsTextAmPm, false, null, _assignManager.AssignAmPm),
                        new DateTimeFormatPartOptionTO(3, CompareTextValueToDateTimePart.IsTextAmPm, false, null, _assignManager.AssignAmPm),
                        new DateTimeFormatPartOptionTO(2, CompareTextValueToDateTimePart.IsTextAmPm, false, null, _assignManager.AssignAmPm),
                    }
                },
                {
                    "Z", _timeZones.Select(k =>
                    {
                        IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length, CompareTextValueToDateTimePart.IsTextTimeZone, false, null, _assignManager.AssignTimeZone);
                        return dateTimeFormatPartOptionTo;
                    }).OrderByDescending(k => k.Length).ToList()
                },
                {
                    "ZZ", _timeZones.Select(k =>
                    {
                        IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length, CompareTextValueToDateTimePart.IsTextTimeZone, false, null, _assignManager.AssignTimeZone);
                        return dateTimeFormatPartOptionTo;
                    }).OrderByDescending(k => k.Length).ToList()
                },
                {
                    "ZZZ", _timeZones.Select(k =>
                    {
                        IDateTimeFormatPartOptionTO dateTimeFormatPartOptionTo = new DateTimeFormatPartOptionTO(k.Key.Length, CompareTextValueToDateTimePart.IsTextTimeZone, false, null, _assignManager.AssignTimeZone);
                        return dateTimeFormatPartOptionTo;
                    }).OrderByDescending(k => k.Length).ToList()
                },
                {
                    "era", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(2, (data, treatAsTim) => data.ToLower().Equals("ad"), false, "AD",
                            _assignManager.AssignEra)
                    }
                },
                {
                    "Era", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(3, (data, treatAsTim) => data.ToLower().Equals("a.d"), false, "A.D",
                            _assignManager.AssignEra)
                    }
                },
                {
                    "ERA", new List<IDateTimeFormatPartOptionTO>
                    {
                        new DateTimeFormatPartOptionTO(4, (data, treatAsTim) => data.ToLower().Equals("a.d."), false, "A.D.",
                            _assignManager.AssignEra)
                    }

            } };
        }
    }
}


























