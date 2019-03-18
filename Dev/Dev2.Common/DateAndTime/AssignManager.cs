#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Collections.Generic;
using System.Globalization;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Common.DateAndTime
{
    class AssignManager : IAssignManager
    {
        readonly Dictionary<string, ITimeZoneTO> _timeZoneTos;

        public AssignManager(Dictionary<string, ITimeZoneTO> timeZoneTos)
        {
            _timeZoneTos = timeZoneTos;
        }

        public AssignManager()
        {
            
        }

        #region Implementation of IAssignManager

        public void AssignTimeZone(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            var lowerValue = value.ToString(CultureInfo.InvariantCulture).ToLower();

            ITimeZoneTO timeZoneTo;
            if (_timeZoneTos.TryGetValue(lowerValue, out timeZoneTo))
            {
                dateTimeResultTo.TimeZone = timeZoneTo;
            }
        }
        public void AssignAmPm(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            var lowerValue = value.ToString(CultureInfo.InvariantCulture).ToLower();

            if (lowerValue == "pm" || lowerValue == "p.m" || lowerValue == "p.m.")
            {
                dateTimeResultTo.AmPm = DateTimeAmPm.pm;
            }
            else
            {
                dateTimeResultTo.AmPm = DateTimeAmPm.am;
            }
        }

        public void AssignMilliseconds(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.Milliseconds = Convert.ToInt32(value);
        }

        public void AssignSeconds(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.Seconds = Convert.ToInt32(value);
        }

        public void AssignMinutes(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.Minutes = Convert.ToInt32(value);
        }

        public void Assign24Hours(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.Hours = Convert.ToInt32(value);
            dateTimeResultTo.Is24H = true;
        }

        public void AssignEra(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.Era = value.ToString(CultureInfo.InvariantCulture);
        }

        public void AssignYears(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            var years = Convert.ToInt32(value);

            if (!assignAsTime && years < 100)
            {
                years = CultureInfo.InvariantCulture.Calendar.ToFourDigitYear(years);
            }

            dateTimeResultTo.Years = Convert.ToInt32(years);
        }

        public void AssignDaysOfWeek(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.DaysOfWeek = Convert.ToInt32(value);
        }

        public void AssignDaysOfYear(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.DaysOfYear = Convert.ToInt32(value);
        }

        public void AssignWeeks(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.Weeks = Convert.ToInt32(value);
        }

        public void AssignDays(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.Days = Convert.ToInt32(value);
        }

        public void Assign12Hours(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.Hours = Convert.ToInt32(value);
        }

        public void AssignMonths(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            dateTimeResultTo.Months = Convert.ToInt32(value);
        }

        #endregion
    }
}