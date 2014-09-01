using System;

namespace Dev2.Common.Interfaces.Core.Convertors.DateAndTime
{
    public enum DateTimeAmPm
    {
        am,
        pm
    }

    public interface IDateTimeResultTO
    {
        int Years { get; set; }
        int Months { get; set; }
        int Days { get; set; }
        int DaysOfWeek { get; set; }
        int DaysOfYear { get; set; }
        int Weeks { get; set; }
        int Hours { get; set; }
        int Minutes { get; set; }
        int Seconds { get; set; }
        int Milliseconds { get; set; }
        bool Is24H { get; set; }
        DateTimeAmPm AmPm { get; set; }
        string Era { get; set; }
        ITimeZoneTO TimeZone { get; set; }

        void NormalizeTime();

        DateTime ToDateTime();
    }
}
