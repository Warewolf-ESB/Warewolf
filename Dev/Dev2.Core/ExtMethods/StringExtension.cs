/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Convertors.DateAndTime;
using Dev2.Converters.DateAndTime;
using Dev2.Data.Util;
using Newtonsoft.Json;

namespace Dev2
{
    public static class StringExtension
    {
        public static bool IsDate(this string payload)
        {
            bool result = false;

            if (string.IsNullOrEmpty(payload))
            {
                return false;
            }

            var acceptedDateFormats = new List<string>
            {
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
            var d = new DateTimeParser();
            int count = 0;
            while (result == false && count < acceptedDateFormats.Count)
            {
                string errorMsg;
                IDateTimeResultTO to;
                result = d.TryParseDateTime(payload, acceptedDateFormats[count], out to, out errorMsg);
                count++;
            }
            return result;
        }

        public static bool IsXml(this string payload)
        {
            bool result = false;
            bool isFragment;


            if (DataListUtil.IsXml(payload, out isFragment))
            {
                result = true;
            }
            else if (isFragment)
            {
                result = true;
            }

            return result;
        }

        public static bool IsJSON(this string payload)
        {
            try
            {
                JsonConvert.DeserializeObject(payload);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}