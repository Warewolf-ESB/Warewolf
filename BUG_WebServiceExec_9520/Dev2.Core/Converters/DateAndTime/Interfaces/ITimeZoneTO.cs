using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Converters.DateAndTime.Interfaces
{
    public interface ITimeZoneTO
    {
        string ShortName { get; set; }
        string Name { get; set; }
        string LongName { get; set; }
    }
}
