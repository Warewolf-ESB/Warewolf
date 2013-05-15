using Dev2.Converters.DateAndTime;
using Dev2.Converters.DateAndTime.Interfaces;
using Dev2.DataList.Contract;
using System;
using System.Collections.Generic;

namespace Dev2
{
    public static class StringExtension
    {
        public static bool IsDate(this string payload)
        {
            bool result = false;
            string errorMsg;
            IDateTimeResultTO to;
            List<string> acceptedDateFormats = new List<string>() { 
                "yyyymmdd",
                "mmddyyyy",
                "yyyyddmm",
                "ddmmyyyy", 
                "yyyy/mm/dd",
                "dd/mm/yyyy",
                "yyyy/dd/mm",
                "mm/dd/yyyy",
                "yyyy-mm-dd",
                "dd-mm-yyyy",
                "mm-dd-yyyy",
                "yyyy-dd-mm",                
                @"dd\mm\yyyy",
                @"yyyy\mm\dd",
                @"yyyy\dd\mm",
                @"mm\dd\yyyy",
                "dd mm yyyy",
                "mm dd yyyy",
                "yyyy mm dd",
                "yyyy dd mm",
                "yyyy mm dd",
                "dd.mm.yyyy",
                "mm.dd.yyyy",
                "yyyy.mm.dd",
                "yyyy.dd.mm"
                
            };
            DateTimeParser d = new DateTimeParser();
            int count = 0;
            while (result == false && count < acceptedDateFormats.Count)
            {
                result = d.TryParseDateTime(payload, acceptedDateFormats[count], out to, out errorMsg);
                count++;
            }
            return result;
        }

        public static bool IsXml(this string payload)
        {
            bool result = false;
            bool isFragment;

            try
            {
                if (DataListUtil.IsXml(payload, out isFragment))
                {
                    result = true;
                }
                else if (isFragment)
                {
                    result = true;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
    }
}
