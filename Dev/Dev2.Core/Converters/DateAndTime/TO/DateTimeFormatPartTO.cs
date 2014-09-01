using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Converters.DateAndTime.TO
{
    public class DateTimeFormatPartTO : IDateTimeFormatPartTO
    {
        #region Constructor

        public DateTimeFormatPartTO(string value, bool isLiteral, string description)
        {
            Value = value;
            Isliteral = isLiteral;
            Description = description;
        }

        #endregion Constructor

        #region Properties

        public string Value { get; set; }
        public bool Isliteral { get; set; }
        public string Description { get; set; }

        #endregion Properties
    }
}
