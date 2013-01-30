using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Converters.DateAndTime.Interfaces
{
    public interface IDateTimeFormatPartTO
    {
        string Value { get; set; }
        string Description { get; set; }
        bool Isliteral { get; set; }
    }
}
