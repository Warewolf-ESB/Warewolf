using System;
using System.Globalization;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Converters.DateAndTime.TO
{
    public class DateTimeResultTO : IDateTimeResultTO
    {
        #region Constructors

        public DateTimeResultTO()
        {
            TimeZone = new TimeZoneTO(TimeZoneInfo.Local.StandardName, TimeZoneInfo.Local.StandardName, TimeZoneInfo.Local.DisplayName);
        }

        #endregion Constructors

        #region Properties

        public int Years { get; set; }
        public int Months { get; set; }
        public int Days { get; set; }
        public int DaysOfWeek { get; set; }
        public int DaysOfYear { get; set; }
        public int Weeks { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public int Milliseconds { get; set; }
        public bool Is24H { get; set; }
        public DateTimeAmPm AmPm { get; set; }
        public string Era { get; set; }
        public ITimeZoneTO TimeZone { get; set; }

        #endregion Properties

        #region Methods

        public void NormalizeTime()
        {
            if (Is24H && Hours >= 12)
            {
                AmPm = DateTimeAmPm.pm;
            }
            else if (Hours > 12)
            {
                Is24H = true;
                AmPm = DateTimeAmPm.pm;
            }
            else if (AmPm == DateTimeAmPm.pm)
            {
                Is24H = true;
                if (Hours != 12)
                {
                    Hours += 12;
                }
            }
            else if (AmPm == DateTimeAmPm.am)
            {
                Is24H = true;
                if (Hours >= 12)
                {
                    Hours -= 12;
                }
            }
        }

        public DateTime ToDateTime()
        {
            NormalizeTime();

            if (Years == 0)
            {
                Years = DateTime.MinValue.Year;
            }

            if (Months == 0)
            {
                if (DaysOfYear != 0)
                {
                    DateTime tmpDate = new DateTime(Years, 1, 1).AddDays(DaysOfYear - 1);
                    Months = tmpDate.Month;

                    if (Days == 0)
                    {
                        Days = tmpDate.Day;
                    }
                }
                else if (Weeks != 0)
                {
                    DateTime tmpDate = CultureInfo.CurrentCulture.Calendar.AddWeeks(new DateTime(Years, 1, 1), Weeks);
                    Months = tmpDate.Month;

                    if (Days == 0)
                    {
                        Days = tmpDate.Day;
                    }
                }
                else
                {
                    Months = DateTime.MinValue.Month;
                }
            }

            if (Days == 0)
            {
                Days = DateTime.MinValue.Day;

                if (DaysOfWeek != 0)
                {
                    DateTime tmpDate = new DateTime(Years, Months, Days);
                    Days = tmpDate.AddDays(DaysOfWeek - DateTimeParser.GetDayOfWeekInt(tmpDate.DayOfWeek)).Day;
                }
            }

            if (Hours == 0)
            {
                Hours = DateTime.MinValue.Hour;
            }

            if (Minutes == 0)
            {
                Minutes = DateTime.MinValue.Minute;
            }

            if (Seconds == 0)
            {
                Seconds = DateTime.MinValue.Second;
            }

            if (Milliseconds == 0)
            {
                Milliseconds = DateTime.MinValue.Millisecond;
            }

            return new DateTime(Years, Months, Days, Hours, Minutes, Seconds, Milliseconds);
        }

        #endregion Methods
    }
}
