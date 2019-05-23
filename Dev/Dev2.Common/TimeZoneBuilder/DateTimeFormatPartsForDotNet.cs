#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Globalization;
using Dev2.Common.DateAndTime.TO;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;

namespace Dev2.Common.TimeZoneBuilder
{
    class DateTimeFormatPartsForDotNet: IDateTimeFormatPartsForDotNet
    {
        Dictionary<string, IDateTimeFormatPartTO> DateTimeFormatPartsForDotNetLu { get; }
        public Dictionary<string, List<IDateTimeFormatPartOptionTO>> DateTimeFormatPartOptionsForDotNet { get; }
        const char DateLiteralCharacter = '\'';
        const char TimeLiteralCharacter = ':';
        public DateTimeFormatPartsForDotNet()
        {
            DateTimeFormatPartsForDotNetLu = new Dictionary<string, IDateTimeFormatPartTO>();
            DateTimeFormatPartOptionsForDotNet = new Dictionary<string, List<IDateTimeFormatPartOptionTO>>();
        }
        #region Implementation of IDateTimeParserBuilder

        public void Build()
        {
            DateTimeFormatPartsForDotNetLu.Add("d", new DateTimeFormatPartTO("d", false, "Day in 1 or 2 digits: 8"));
            DateTimeFormatPartOptionsForDotNet.Add("d", null);
            DateTimeFormatPartsForDotNetLu.Add("dd", new DateTimeFormatPartTO("dd", false, "Day in 2 digits: 8"));
            DateTimeFormatPartOptionsForDotNet.Add("dd", null);
            DateTimeFormatPartsForDotNetLu.Add("ddd",
                new DateTimeFormatPartTO("ddd", false, "The abbreviated name of the day of the week: Mon"));
            DateTimeFormatPartOptionsForDotNet.Add("ddd", null);
            DateTimeFormatPartsForDotNetLu.Add("dddd",
                new DateTimeFormatPartTO("dddd", false, "The full name of the day of the week: Monday"));
            DateTimeFormatPartOptionsForDotNet.Add("dddd", null);

            DateTimeFormatPartsForDotNetLu.Add("f", new DateTimeFormatPartTO("f", false, "The tenths of a second: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("f", null);
            DateTimeFormatPartsForDotNetLu.Add("ff",
                new DateTimeFormatPartTO("ff", false, "The hundredths of a second in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("ff", null);
            DateTimeFormatPartsForDotNetLu.Add("fff",
                new DateTimeFormatPartTO("fff", false, "The milliseconds in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("fff", null);
            DateTimeFormatPartsForDotNetLu.Add("ffff",
                new DateTimeFormatPartTO("ffff", false, "The ten thousandths of a second in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("ffff", null);
            DateTimeFormatPartsForDotNetLu.Add("fffff",
                new DateTimeFormatPartTO("fffff", false,
                    "The hundred thousandths of a second in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("fffff", null);
            DateTimeFormatPartsForDotNetLu.Add("ffffff",
                new DateTimeFormatPartTO("ffffff", false, "The millionths of a second in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("ffffff", null);
            DateTimeFormatPartsForDotNetLu.Add("fffffff",
                new DateTimeFormatPartTO("ffffffff", false, "The ten millionths of a second in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("fffffff", null);

            DateTimeFormatPartsForDotNetLu.Add("F",
                new DateTimeFormatPartTO("f", false, "If non-zero, the tenths of a second: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("F", null);
            DateTimeFormatPartsForDotNetLu.Add("FF",
                new DateTimeFormatPartTO("FF", false,
                    "If non-zero, the hundredths of a second in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("FF", null);
            DateTimeFormatPartsForDotNetLu.Add("FFF",
                new DateTimeFormatPartTO("FFF", false, "If non-zero, the milliseconds in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("FFF", null);
            DateTimeFormatPartsForDotNetLu.Add("FFFF",
                new DateTimeFormatPartTO("FFFF", false,
                    "If non-zero, the ten thousandths of a second in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("FFFF", null);
            DateTimeFormatPartsForDotNetLu.Add("FFFFF",
                new DateTimeFormatPartTO("FFFFF", false,
                    "If non-zero, the hundred thousandths of a second in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("FFFFF", null);
            DateTimeFormatPartsForDotNetLu.Add("FFFFFF",
                new DateTimeFormatPartTO("FFFFFF", false,
                    "If non-zero, the millionths of a second in a date and time value: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("FFFFFF", null);

            DateTimeFormatPartsForDotNetLu.Add("g", new DateTimeFormatPartTO("g", false, "The period or era: A.D."));
            DateTimeFormatPartOptionsForDotNet.Add("g", null);
            DateTimeFormatPartsForDotNetLu.Add("gg", new DateTimeFormatPartTO("gg", false, "The period or era: A.D."));
            DateTimeFormatPartOptionsForDotNet.Add("gg", null);

            DateTimeFormatPartsForDotNetLu.Add("h",
                new DateTimeFormatPartTO("h", false, "The hour, using a 12-hour clock from 1 to 12: 1"));
            DateTimeFormatPartOptionsForDotNet.Add("h", null);
            DateTimeFormatPartsForDotNetLu.Add("hh",
                new DateTimeFormatPartTO("hh", false, "The hour, using a 12-hour clock from 1 to 12: 01"));
            DateTimeFormatPartOptionsForDotNet.Add("hh", null);
            DateTimeFormatPartsForDotNetLu.Add("H",
                new DateTimeFormatPartTO("H", false, "The hour, using a 24-hour clock from 1 to 24: 01"));
            DateTimeFormatPartOptionsForDotNet.Add("H", null);
            DateTimeFormatPartsForDotNetLu.Add("HH",
                new DateTimeFormatPartTO("HH", false, "The hour, using a 24-hour clock from 1 to 24: 13"));
            DateTimeFormatPartOptionsForDotNet.Add("HH", null);

            DateTimeFormatPartsForDotNetLu.Add("K", new DateTimeFormatPartTO("K", false, "Time zone information: +02:00"));
            DateTimeFormatPartOptionsForDotNet.Add("K", null);

            DateTimeFormatPartsForDotNetLu.Add("m", new DateTimeFormatPartTO("m", false, "Minute in 1 or 2 digits: 9"));
            DateTimeFormatPartOptionsForDotNet.Add("m", null);
            DateTimeFormatPartsForDotNetLu.Add("mm", new DateTimeFormatPartTO("mm", false, "Minute in two digits: 09"));
            DateTimeFormatPartOptionsForDotNet.Add("mm", null);

            DateTimeFormatPartsForDotNetLu.Add("M", new DateTimeFormatPartTO("M", false, "Month in 1 or 2 digits: 6"));
            DateTimeFormatPartOptionsForDotNet.Add("M", null);
            DateTimeFormatPartsForDotNetLu.Add("MM", new DateTimeFormatPartTO("MM", false, "Month in two digits: 06"));
            DateTimeFormatPartOptionsForDotNet.Add("MM", null);
            DateTimeFormatPartsForDotNetLu.Add("MMM",
                new DateTimeFormatPartTO("MMM", false, "Abbreviated name of the month: Jun"));
            DateTimeFormatPartOptionsForDotNet.Add("MMM", null);
            DateTimeFormatPartsForDotNetLu.Add("MMMM",
                new DateTimeFormatPartTO("MMM", false, "Full name of the month: June"));
            DateTimeFormatPartOptionsForDotNet.Add("MMMM", null);

            DateTimeFormatPartsForDotNetLu.Add("s", new DateTimeFormatPartTO("s", false, "Second in 1 or 2 digits: 9"));
            DateTimeFormatPartOptionsForDotNet.Add("s", null);
            DateTimeFormatPartsForDotNetLu.Add("ss", new DateTimeFormatPartTO("ss", false, "Second in two digits: 09"));
            DateTimeFormatPartOptionsForDotNet.Add("ss", null);

            DateTimeFormatPartsForDotNetLu.Add("t",
                new DateTimeFormatPartTO("t", false, "First character of the AM/PM designator: 9"));
            DateTimeFormatPartOptionsForDotNet.Add("t", null);
            DateTimeFormatPartsForDotNetLu.Add("tt", new DateTimeFormatPartTO("tt", false, "AM/PM designator: 09"));
            DateTimeFormatPartOptionsForDotNet.Add("tt", null);

            DateTimeFormatPartsForDotNetLu.Add("y", new DateTimeFormatPartTO("y", false, "Year in 1 or 2 digits: 13"));
            DateTimeFormatPartOptionsForDotNet.Add("y", null);
            DateTimeFormatPartsForDotNetLu.Add("yy", new DateTimeFormatPartTO("yy", false, "Year in two digits: 13"));
            DateTimeFormatPartOptionsForDotNet.Add("yy", null);
            DateTimeFormatPartsForDotNetLu.Add("yyy",
                new DateTimeFormatPartTO("yyy", false, "Year, with a minimum of three digits: 2013"));
            DateTimeFormatPartOptionsForDotNet.Add("yyy", null);
            DateTimeFormatPartsForDotNetLu.Add("yyyy",
                new DateTimeFormatPartTO("yyyy", false, "Year in four digits: 2013"));
            DateTimeFormatPartOptionsForDotNet.Add("yyyy", null);
            DateTimeFormatPartsForDotNetLu.Add("yyyyy",
                new DateTimeFormatPartTO("yyyyy", false, "Year in five digits: 02013"));
            DateTimeFormatPartOptionsForDotNet.Add("yyyyy", null);

            DateTimeFormatPartsForDotNetLu.Add("z",
                new DateTimeFormatPartTO("z", false, "Hours offset from UTC, with no leading zeros: +2"));
            DateTimeFormatPartOptionsForDotNet.Add("z", null);
            DateTimeFormatPartsForDotNetLu.Add("zz",
                new DateTimeFormatPartTO("zz", false, "Hours offset from UTC in two digits: +02"));
            DateTimeFormatPartOptionsForDotNet.Add("zz", null);
            DateTimeFormatPartsForDotNetLu.Add("zzz",
                new DateTimeFormatPartTO("zzz", false, "Hours and minutes offset from UTC: +02:00"));
            DateTimeFormatPartOptionsForDotNet.Add("zzz", null);

            DateTimeFormatPartsForDotNetLu.Add(TimeLiteralCharacter.ToString(CultureInfo.InvariantCulture),
                new DateTimeFormatPartTO(TimeLiteralCharacter.ToString(CultureInfo.InvariantCulture), false,
                    "The time separator: '" + TimeLiteralCharacter + "'"));
            DateTimeFormatPartOptionsForDotNet.Add(TimeLiteralCharacter.ToString(CultureInfo.InvariantCulture), null);
            DateTimeFormatPartsForDotNetLu.Add(DateLiteralCharacter.ToString(CultureInfo.InvariantCulture),
                new DateTimeFormatPartTO(DateLiteralCharacter.ToString(CultureInfo.InvariantCulture), false,
                    "The date separator: " + DateLiteralCharacter));
            DateTimeFormatPartOptionsForDotNet.Add(DateLiteralCharacter.ToString(CultureInfo.InvariantCulture), null);
        }

        #endregion
    }
}