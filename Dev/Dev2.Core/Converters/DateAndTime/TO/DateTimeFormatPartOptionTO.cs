using System;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Converters.DateAndTime.TO
{
    internal class DateTimeFormatPartOptionTO : IDateTimeFormatPartOptionTO
    {
        #region Constructor

        public DateTimeFormatPartOptionTO(int length, Func<string, bool, bool> predicate, bool isNumeric, IConvertible actualValue, Action<IDateTimeResultTO, bool, IConvertible> assignAction,int resultLength = -1)
        {
            Length = length;
            Predicate = predicate;
            IsNumeric = isNumeric;
            ActualValue = actualValue;
            AssignAction = assignAction;
            if(resultLength == -1)
            {
                ResultLength = Length;    
            }
            else
            {
                ResultLength = resultLength;
        }

        }

        #endregion Constructor
        
        #region Properties

        public int Length { get; set; }
        public int ResultLength { get; set; }
        public Func<string, bool, bool> Predicate { get; set; }
        public IConvertible ActualValue { get; set; }
        public bool IsNumeric { get; set; }
        public Action<IDateTimeResultTO, bool, IConvertible> AssignAction { get; set; }

        #endregion Properties
    }
}
