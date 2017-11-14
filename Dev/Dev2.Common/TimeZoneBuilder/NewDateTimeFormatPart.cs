using System.Collections.Generic;
using Dev2.Common.DateAndTime;
using Dev2.Common.DateAndTime.TO;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

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

        protected override void AddDayParts()
        {
            //base.AddDayParts();
        }
        protected override void AddEraParts()
        {
            //base.AddEraParts();
        }
        protected override void AddHourParts()
        {
            //base.AddHourParts();
        }
        protected override void AddMinuteParts()
        {
            //base.AddMinuteParts();
        }
        protected override void AddMonthParts()
        {
            //base.AddMonthParts();
        }
        protected override void AddOffsetParts()
        {
            //base.AddOffsetParts();
        }
        protected override void AddSecondParts()
        {
            //base.AddSecondParts();
        }
        protected override void AddWeekParts()
        {
            //base.AddWeekParts();
        }
        
    }
}