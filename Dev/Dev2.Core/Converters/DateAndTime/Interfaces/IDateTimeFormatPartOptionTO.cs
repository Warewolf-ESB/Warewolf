using System;

namespace Dev2.Converters.DateAndTime.Interfaces
{
    internal interface IDateTimeFormatPartOptionTO
    {
        int Length { get; set; }
        int ResultLength { get; set; }
        Func<string, bool, bool> Predicate { get; set; }
        bool IsNumeric { get; set; }
        IConvertible ActualValue { get; set; }
        Action<IDateTimeResultTO, bool, IConvertible> AssignAction { get; set; }
    }
}
