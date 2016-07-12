using System;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Common.Interfaces
{
    internal interface IAssignManager
    {
        void AssignAmPm(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value);
        void AssignMilliseconds(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value);
        void AssignSeconds(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value);
        void AssignMinutes(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value);
        void Assign24Hours(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value);
        void AssignEra(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value);
        void AssignYears(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value);
        void AssignDaysOfWeek(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value);
        void AssignDaysOfYear(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value);
        void AssignWeeks(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value);
        void AssignDays(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value);
        void Assign12Hours(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value);
        void AssignMonths(IDateTimeResultTO dateTimeResultTo, bool assignAsTime, IConvertible value);
    }
}