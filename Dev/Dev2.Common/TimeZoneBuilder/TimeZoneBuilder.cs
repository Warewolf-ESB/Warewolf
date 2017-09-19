using System;
using System.Collections.Generic;
using System.Globalization;
using Dev2.Common.DateAndTime.TO;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Common.TimeZoneBuilder
{
    internal class TimeZoneBuilder : ITimeZoneBuilder
    {

        #region Implementation of IDateTimeParserBuilder
        /// <summary>
        ///     Creates a list of all valid time zones
        /// </summary>
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
                        string min = minutes == 0 ? "00" : "30";
                        string hrs = string.Concat(hours / Math.Abs(hours) < 0 ? "-" : "+",
                            Math.Abs(hours).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'));
                        string uct = string.Concat(UctShort, hrs, ":", min);
                        TimeZones.Add(uct.ToLower(), new TimeZoneTO(UctShort, uct, UctLong));
                    }
                }
                else
                {
                    TimeZones.Add(UctShort + "-00:30", new TimeZoneTO(UctShort, UctShort + "-00:30", UctLong));
                    TimeZones.Add(UctShort + "+00:30", new TimeZoneTO(UctShort, UctShort + "+00:30", UctLong));
                }
            }

            //
            // Create GMT entries
            //
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
                        string min = minutes == 0 ? "00" : "30";
                        string hrs = string.Concat(hours / Math.Abs(hours) < 0 ? "-" : "+",
                            Math.Abs(hours).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'));
                        string gmt = string.Concat(GmtShort, hrs, ":", min);
                        TimeZones.Add(gmt.ToLower(), new TimeZoneTO(GmtShort, gmt, GmtLong));
                    }
                }
                else
                {
                    TimeZones.Add(GmtShort + "-00:30", new TimeZoneTO(GmtShort, GmtShort + "-00:30", GmtLong));
                    TimeZones.Add(GmtShort + "+00:30", new TimeZoneTO(GmtShort, GmtShort + "+00:30", GmtLong));
                }
            }

            //
            // Read in system time zones
            //
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

        #endregion

        #region Implementation of ITimeZoneBuilder

        public Dictionary<string, ITimeZoneTO> TimeZones { get; set; }

        #endregion
    }
}