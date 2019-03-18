#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Collections.Generic;
using System.Globalization;
using Dev2.Common.DateAndTime.TO;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Common.TimeZoneBuilder
{
    class TimeZoneBuilder : ITimeZoneBuilder
    {
        #region Implementation of IDateTimeParserBuilder

        public void Build()
        {
            TimeZones = new Dictionary<string, ITimeZoneTO>();
            const string UctShort = "UCT";
            const string UctLong = "Coordinated Universal Time";
            TimeZones.Add(UctShort.ToLower(), new TimeZoneTO(UctShort, UctShort, UctLong));
            TimeZones.Add(UctLong.ToLower(), new TimeZoneTO(UctShort, UctShort, UctLong));

            for (int hours = -12; hours < 13; hours++)
            {
                if (hours != 0)
                {
                    for (int minutes = 0; minutes < 2; minutes++)
                    {
                        var min = minutes == 0 ? "00" : "30";
                        var hrs = string.Concat(hours / Math.Abs(hours) < 0 ? "-" : "+",
                            Math.Abs(hours).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'));
                        var uct = string.Concat(UctShort, hrs, ":", min);
                        TimeZones.Add(uct.ToLower(), new TimeZoneTO(UctShort, uct, UctLong));
                    }
                }
                else
                {
                    TimeZones.Add(UctShort + "-00:30", new TimeZoneTO(UctShort, UctShort + "-00:30", UctLong));
                    TimeZones.Add(UctShort + "+00:30", new TimeZoneTO(UctShort, UctShort + "+00:30", UctLong));
                }
            }

            CreateGMTEntries();            
            ReadSystemTimeZones();

        }

        private void ReadSystemTimeZones()
        {
            foreach (TimeZoneInfo timeZoneInfo in TimeZoneInfo.GetSystemTimeZones())
            {
                if (!TimeZones.TryGetValue(timeZoneInfo.DisplayName.ToLower(), out ITimeZoneTO timeZoneTo))
                {
                    TimeZones.Add(timeZoneInfo.DisplayName.ToLower(),
                        new TimeZoneTO(timeZoneInfo.StandardName, timeZoneInfo.StandardName, timeZoneInfo.DisplayName));
                }

                if (!TimeZones.TryGetValue(timeZoneInfo.DaylightName.ToLower(), out timeZoneTo))
                {
                    TimeZones.Add(timeZoneInfo.DaylightName.ToLower(),
                        new TimeZoneTO(timeZoneInfo.DaylightName, timeZoneInfo.DaylightName, timeZoneInfo.DisplayName));
                }

                if (!TimeZones.TryGetValue(timeZoneInfo.StandardName.ToLower(), out timeZoneTo))
                {
                    TimeZones.Add(timeZoneInfo.StandardName.ToLower(),
                        new TimeZoneTO(timeZoneInfo.StandardName, timeZoneInfo.StandardName, timeZoneInfo.DisplayName));
                }
            }
        }

        private void CreateGMTEntries()
        {
            const string GmtShort = "GMT";
            const string GmtLong = "Greenwich Mean Time";
            TimeZones.Add(GmtShort.ToLower(), new TimeZoneTO(GmtShort, GmtShort, GmtLong));
            TimeZones.Add(GmtLong.ToLower(), new TimeZoneTO(GmtShort, GmtShort, GmtLong));

            for (int hours = -12; hours < 13; hours++)
            {
                if (hours != 0)
                {
                    for (int minutes = 0; minutes < 2; minutes++)
                    {
                        var min = minutes == 0 ? "00" : "30";
                        var hrs = string.Concat(hours / Math.Abs(hours) < 0 ? "-" : "+",
                            Math.Abs(hours).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'));
                        var gmt = string.Concat(GmtShort, hrs, ":", min);
                        TimeZones.Add(gmt.ToLower(), new TimeZoneTO(GmtShort, gmt, GmtLong));
                    }
                }
                else
                {
                    TimeZones.Add(GmtShort + "-00:30", new TimeZoneTO(GmtShort, GmtShort + "-00:30", GmtLong));
                    TimeZones.Add(GmtShort + "+00:30", new TimeZoneTO(GmtShort, GmtShort + "+00:30", GmtLong));
                }
            }
        }

        #endregion

        #region Implementation of ITimeZoneBuilder

        public Dictionary<string, ITimeZoneTO> TimeZones { get; set; }

        #endregion
    }
}