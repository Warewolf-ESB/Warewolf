using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

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
