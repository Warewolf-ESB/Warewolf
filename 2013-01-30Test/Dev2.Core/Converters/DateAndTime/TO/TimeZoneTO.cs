using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Converters.DateAndTime.Interfaces;

namespace Dev2.Converters.DateAndTime.TO
{
    public class TimeZoneTO : ITimeZoneTO
    {
        public TimeZoneTO(string shortName, string name, string longName)
        {
            ShortName = shortName;
            Name = name;
            LongName = longName;
        }

        public string ShortName { get; set; }
        public string Name { get; set; }
        public string LongName { get; set; }
    }
}
