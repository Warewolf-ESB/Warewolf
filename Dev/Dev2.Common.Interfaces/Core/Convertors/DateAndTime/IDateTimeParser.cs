using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Core.Convertors.DateAndTime
{
    public interface IDateTimeParser
    {
        List<IDateTimeFormatPartTO> DateTimeFormatParts { get; }
        bool TryParseDateTime(string dateTime, string inputFormat, out IDateTimeResultTO parsedDateTime, out string error);
        bool TryParseTime(string time, string inputFormat, out IDateTimeResultTO parsedTime, out string error);
        string TranslateDotNetToDev2Format(string originalFormat, out string error);
    }
}
