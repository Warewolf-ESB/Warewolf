using System;

namespace Dev2.Common.Interfaces.Core.Convertors.DateAndTime
{
    public interface IDateTimeFormatPartOptionTO
    {
        int Length { get; set; }
        int ResultLength { get; set; }
        Func<string, bool, bool> Predicate { get; set; }
        bool IsNumeric { get; set; }
        IConvertible ActualValue { get; set; }
        Action<IDateTimeResultTO, bool, IConvertible> AssignAction { get; set; }
    }
}
