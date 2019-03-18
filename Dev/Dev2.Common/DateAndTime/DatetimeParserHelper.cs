#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
            var nothingDied = true;

            int numericData;
            if (int.TryParse(data, NumberStyles.None, null, out numericData))
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
            var nothingDied = true;

            int numericData;
            if (int.TryParse(data, NumberStyles.None, null, out numericData) && numericData >= 1 && numericData <= 365)
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
            var nothingDied = true;

            int numericData;
            if (int.TryParse(data, NumberStyles.None, null, out numericData))
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
            var nothingDied = true;

            int numericData;
            if (int.TryParse(data, NumberStyles.None, null, out numericData))
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
            var nothingDied = true;

            int numericData;
            if (int.TryParse(data, NumberStyles.None, null, out numericData))
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
            var nothingDied = true;

            int numericData;
            if (int.TryParse(data, NumberStyles.None, null, out numericData))
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
            var nothingDied = true;

            int numericData;
            if (data.Length == 2 && int.TryParse(data, out numericData))
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
            var nothingDied = true;

            int numericData;
            if (int.TryParse(data, NumberStyles.None, null, out numericData))
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
            var nothingDied = true;

            int numericData;
            if (int.TryParse(data, NumberStyles.None, null, out numericData))
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
            var nothingDied = true;

            int numericData;
            if (int.TryParse(data, NumberStyles.None, null, out numericData))
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