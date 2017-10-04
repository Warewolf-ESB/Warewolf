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
            return int.TryParse(data, NumberStyles.None, null, out int numericData);
        }

        /// <summary>
        ///     Determines if a given string represents am
        /// </summary>
        public static bool IsTextAmPm(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == "am" || lowerData == "pm" || lowerData == "a.m" || lowerData == "p.m" ||
                   lowerData == "a.m." || lowerData == "p.m.";
        }

        /// <summary>
        ///     Determines if a given string represents a time zone
        /// </summary>
        public static bool IsTextTimeZone(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return DateTimeParser.TimeZones.TryGetValue(lowerData, out ITimeZoneTO timeZoneTo);
        }

        /// <summary>
        ///     Determines if a given string represents the month of January
        /// </summary>
        public static bool IsTextJanuary(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[0].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[0].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of Febuary
        /// </summary>
        public static bool IsTextFebuary(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[1].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[1].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of March
        /// </summary>
        public static bool IsTextMarch(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[2].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[2].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of April
        /// </summary>
        public static bool IsTextApril(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[3].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[3].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of May
        /// </summary>
        public static bool IsTextMay(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[4].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[4].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of June
        /// </summary>
        public static bool IsTextJune(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[5].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[5].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of July
        /// </summary>
        public static bool IsTextJuly(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[6].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[6].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of August
        /// </summary>
        public static bool IsTextAugust(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[7].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[7].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of September
        /// </summary>
        public static bool IsTextSeptember(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[8].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[8].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of October
        /// </summary>
        public static bool IsTextOctober(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[9].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[9].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of November
        /// </summary>
        public static bool IsTextNovember(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[10].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[10].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the month of December
        /// </summary>
        public static bool IsTextDecember(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[11].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[11].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Monday
        /// </summary>
        public static bool IsTextMonday(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[1].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[1].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Tuesday
        /// </summary>
        public static bool IsTextTuesday(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[2].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[2].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Wednesday
        /// </summary>
        public static bool IsTextWednesday(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[3].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[3].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Thursday
        /// </summary>
        public static bool IsTextThursday(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[4].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[4].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Friday
        /// </summary>
        public static bool IsTextFriday(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[5].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[5].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Saturday
        /// </summary>
        public static bool IsTextSaturday(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[6].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[6].ToLower();
        }

        /// <summary>
        ///     Determines if a given string represents the day Sunday
        /// </summary>
        public static bool IsTextSunday(string data, bool treatAsTime)
        {
            string lowerData = data.ToLower();
            return lowerData == CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[0].ToLower() ||
                   lowerData == CultureInfo.CurrentCulture.DateTimeFormat.DayNames[0].ToLower();
        }
    }
}