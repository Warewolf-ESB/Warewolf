using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Converters.DateAndTime.Interfaces;

namespace Dev2.Converters.DateAndTime.TO
{
    internal class DateTimeFormatPartOptionTO : IDateTimeFormatPartOptionTO
    {
        #region Constructor

        public DateTimeFormatPartOptionTO(int length, Func<string, bool, bool> predicate, bool isNumeric, IConvertible actualValue, Action<IDateTimeResultTO, bool, IConvertible> assignAction)
        {
            Length = length;
            Predicate = predicate;
            IsNumeric = isNumeric;
            ActualValue = actualValue;
            AssignAction = assignAction;
        }

        #endregion Constructor
        
        #region Properties

        public int Length { get; set; }
        public Func<string, bool, bool> Predicate { get; set; }
        public IConvertible ActualValue { get; set; }
        public bool IsNumeric { get; set; }
        public Action<IDateTimeResultTO, bool, IConvertible> AssignAction { get; set; }

        #endregion Properties
    }
}
