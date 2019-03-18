#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System.Globalization;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;


namespace Dev2.Common.DateAndTime
{
    public static class CompareTextValueToDateTimePart
    {
        /// <summary>
        ///     Determines if a given string is a number
        /// </summary>
        public static bool IsTextNumeric(string data, bool treatAsTime)
        {
            int numericData;
            return int.TryParse(data, NumberStyles.None, null, out numericData);
        }

        /// <summary>
        ///     Determines if a given string represents am
        /// </summary>
        public static bool IsTextAmPm(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == "am" || lowerData == "pm" || lowerData == "a.m" || lowerData == "p.m" ||
                   lowerData == "a.m." || lowerData == "p.m.";
        }

        /// <summary>
        ///     Determines if a given string represents a time zone
        /// </summary>
        public static bool IsTextTimeZone(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            ITimeZoneTO timeZoneTo;
            return DateTimeParser.TimeZones.TryGetValue(lowerData, out timeZoneTo);
        }

        /// <summary>
        ///     Determines if a given string represents the month of January
        /// </summary>
        public static bool IsTextJanuary(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[0].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[0].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of Febuary
        /// </summary>
        public static bool IsTextFebuary(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[1].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[1].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of March
        /// </summary>
        public static bool IsTextMarch(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[2].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[2].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of April
        /// </summary>
        public static bool IsTextApril(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[3].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[3].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of May
        /// </summary>
        public static bool IsTextMay(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[4].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[4].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of June
        /// </summary>
        public static bool IsTextJune(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[5].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[5].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of July
        /// </summary>
        public static bool IsTextJuly(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[6].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[6].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of August
        /// </summary>
        public static bool IsTextAugust(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[7].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[7].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of September
        /// </summary>
        public static bool IsTextSeptember(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[8].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[8].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of October
        /// </summary>
        public static bool IsTextOctober(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[9].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[9].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of November
        /// </summary>
        public static bool IsTextNovember(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[10].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[10].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of December
        /// </summary>
        public static bool IsTextDecember(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[11].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[11].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Monday
        /// </summary>
        public static bool IsTextMonday(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[1].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[1].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Tuesday
        /// </summary>
        public static bool IsTextTuesday(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[2].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[2].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Wednesday
        /// </summary>
        public static bool IsTextWednesday(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[3].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[3].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Thursday
        /// </summary>
        public static bool IsTextThursday(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[4].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[4].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Friday
        /// </summary>
        public static bool IsTextFriday(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[5].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[5].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Saturday
        /// </summary>
        public static bool IsTextSaturday(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[6].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[6].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Sunday
        /// </summary>
        public static bool IsTextSunday(string data, bool treatAsTime)
        {
            var lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[0].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[0].ToLower();
        }
    }
}