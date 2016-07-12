using System;
using System.Collections.Generic;
using System.Globalization;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Common.DateAndTime
{
    internal class AssignManager : IAssignManager
    {
        private readonly Dictionary<string, ITimeZoneTO> _timeZoneTos;

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
            string lowerValue = value.ToString(CultureInfo.InvariantCulture).ToLower();

            ITimeZoneTO timeZoneTo;
            if (_timeZoneTos.TryGetValue(lowerValue, out timeZoneTo))
            {
                dateTimeResultTo.TimeZone = timeZoneTo;
            }
        }
        public void AssignAmPm(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value)
        {
            string lowerValue = value.ToString(CultureInfo.InvariantCulture).ToLower();

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
            int years = Convert.ToInt32(value);

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