using System;
using System.Globalization;
using Dev2.Common.Interfaces;

namespace Dev2.Common.DateAndTime
{
    public class DateTimeParserHelper: IDatetimeParserHelper
    {
        #region Implementation of IDatetimeParserHelper

        public bool IsNumberWeekOfYear(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            if (int.TryParse(data, NumberStyles.None, null, out int numericData))
            {
                if (!treatAsTime)
                {
                    nothingDied = numericData >= 1 && numericData <= 52;
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        public bool IsNumberDayOfYear(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            if (int.TryParse(data, NumberStyles.None, null, out int numericData) && numericData >= 1 && numericData <= 365)
            {
                //nothing to do since nothignDied is already true
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        public bool IsNumberDayOfWeek(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            if (int.TryParse(data, NumberStyles.None, null, out int numericData))
            {
                if (!treatAsTime)
                {
                    nothingDied = numericData >= 1 && numericData <= 7;
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        internal static int GetDayOfWeekInt(DayOfWeek dayOfWeek)
        {
            int val;
            if (dayOfWeek == DayOfWeek.Sunday)
            {
                val = 7;
            }
            else
            {
                val = (int)dayOfWeek;
            }

            return val;
        }

        public bool IsNumberDay(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            if (int.TryParse(data, NumberStyles.None, null, out int numericData))
            {
                if (!treatAsTime)
                {
                    nothingDied = numericData >= 1 && numericData <= 31;
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        public bool IsNumberMonth(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            if (int.TryParse(data, NumberStyles.None, null, out int numericData))
            {
                if (!treatAsTime)
                {
                    nothingDied = numericData >= 1 && numericData <= 12;
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        public bool IsNumber12H(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            if (int.TryParse(data, NumberStyles.None, null, out int numericData))
            {
                if (!treatAsTime)
                {
                    nothingDied = numericData >= 1 && numericData <= 12;
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        public bool IsNumber24H(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            if (data.Length == 2 && int.TryParse(data, out int numericData))
            {
                if (!treatAsTime)
                {
                    nothingDied = numericData >= 0 && numericData <= 24;
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        public bool IsNumberMinutes(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            if (int.TryParse(data, NumberStyles.None, null, out int numericData))
            {
                if (!treatAsTime)
                {
                    nothingDied = numericData >= 0 && numericData <= 59;
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        public bool IsNumberMilliseconds(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            if (int.TryParse(data, NumberStyles.None, null, out int numericData))
            {
                if (!treatAsTime)
                {
                    nothingDied = numericData >= 0 && numericData <= 999;
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        public bool IsNumberSeconds(string data, bool treatAsTime)
        {
            bool nothingDied = true;

            if (int.TryParse(data, NumberStyles.None, null, out int numericData))
            {
                if (!treatAsTime)
                {
                    nothingDied = numericData >= 0 && numericData <= 59;
                }
            }
            else
            {
                nothingDied = false;
            }

            return nothingDied;
        }

        

        #endregion
    }
}