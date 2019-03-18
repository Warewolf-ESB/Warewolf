#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.DateAndTime;
using Dev2.Common.DateAndTime.TO;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Common.TimeZoneBuilder
{
    class TimeFormatPartBuilder : ITimeFormatPartBuilder
    {
        readonly Dictionary<string, ITimeZoneTO> _timeZones;
        readonly AssignManager _assignManager;
        static readonly IDatetimeParserHelper DatetimeParserHelper = new DateTimeParserHelper();
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


























